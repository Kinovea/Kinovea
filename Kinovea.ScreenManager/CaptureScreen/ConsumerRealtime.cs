using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.IO;
using System;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using System.Drawing;
using Kinovea.Services;
using System.Diagnostics;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// ConsumerRealtime. 
    /// Saves frames to file as soon as they are coming from the camera.
    /// The recorder is format agnostic, the format is simply passed along to the writer.
    /// The writer will decide if pixel format conversion and/or encoding are needed.
    /// </summary>
    public class ConsumerRealtime : AbstractConsumer
    {
        public string Filename
        {
            get { return filename; }
        }

        public bool Recording
        {
            get { return recording; }
        }

        public long Ellapsed { get; private set; }

        private ImageDescriptor imageDescriptor;
        private MJPEGWriter writer;
        private bool recording;
        private string filename;
        private string shortId;
        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConsumerRealtime(string shortId)
        {
            this.shortId = shortId;
            stopwatch.Start();
        }
        
        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            this.imageDescriptor = imageDescriptor;
        }

        public SaveResult StartRecord(string filename, double interval)
        {
            //-----------------------
            // Runs on the UI thread.
            //-----------------------

            if (imageDescriptor == null)
                throw new NotSupportedException("ImageDescriptor must be set before prepare.");

            this.filename = filename;

            if (writer != null)
                writer.Dispose();

            writer = new MJPEGWriter();
            
            VideoInfo info = new VideoInfo();
            info.OriginalSize = new Size(imageDescriptor.Width, imageDescriptor.Height);

            bool uncompressed = PreferencesManager.CapturePreferences.SaveUncompressedVideo && imageDescriptor.Format != ImageFormat.JPEG;
            string formatString = FilenameHelper.GetFormatStringCapture(uncompressed);

            // If the capture happens too fast or too slow for a regular player, set the video metadata to a more sensible framerate.
            // This avoids erratic playback because the player can't cope with the framerate, drawback: prevents review in real time.
            double hrft = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold;
            double srft = PreferencesManager.CapturePreferences.SlowspeedRecordingFramerateThreshold;
            double fps = 1000.0 / interval;
            double fileInterval = interval;
            if (fps >= hrft)
            {
                double hrfo = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput;
                fileInterval = 1000.0 / hrfo;
                log.DebugFormat("High speed recording detected, {0:0.###} fps. Forcing output framerate to {1:0.###} fps.", fps, hrfo);
            }
            else if (fps <= srft)
            {
                double srfo = PreferencesManager.CapturePreferences.SlowspeedRecordingFramerateOutput;
                fileInterval = 1000.0 / srfo;
                log.DebugFormat("Slow speed recording detected, {0:0.###} fps. Forcing output framerate to {1:0.###} fps.", fps, srfo);
            }


            log.DebugFormat("Frame budget for writer [{0}]: {1:0.000} ms.", shortId, interval);

            SaveResult result = writer.OpenSavingContext(filename, info, formatString, imageDescriptor.Format, uncompressed, interval, fileInterval);

            recording = true;

            return result;
        }

        protected override void AfterDeactivate()
        {
            if (recording)
            {
                writer.CloseSavingContext(true);
                writer.Dispose();
                writer = null;

                recording = false;
            }

            base.AfterDeactivate();
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            long then = stopwatch.ElapsedMilliseconds;

            writer.SaveFrame(imageDescriptor.Format, entry.Buffer, entry.PayloadLength, imageDescriptor.TopDown);

            Ellapsed = stopwatch.ElapsedMilliseconds - then;
        }
    }
}
