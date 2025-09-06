using System.IO;
using System;
using System.Drawing;
using System.Diagnostics;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using Kinovea.Services;

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

        public SaveResult StartRecord(string filename, double interval, ImageRotation rotation)
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
            string formatString = FilesystemHelper.GetFormatStringCapture(uncompressed);
            double fileInterval = CalibrationHelper.ComputeFileFrameInterval(interval);

            log.DebugFormat("Frame budget for writer [{0}]: {1:0.000} ms.", shortId, interval);

            SaveResult result = writer.OpenSavingContext(filename, info, formatString, imageDescriptor.Format, uncompressed, interval, fileInterval, rotation);

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
            if (writer == null)
                return;

            long then = stopwatch.ElapsedMilliseconds;

            writer.SaveFrame(imageDescriptor.Format, entry.Buffer, entry.PayloadLength, imageDescriptor.TopDown);

            Ellapsed = stopwatch.ElapsedMilliseconds - then;
        }
    }
}
