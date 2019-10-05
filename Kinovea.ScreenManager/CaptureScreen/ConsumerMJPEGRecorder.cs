using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.IO;
using System;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// ConsumerMJPEGRecorder. 
    /// Saves samples to file.
    /// The recorder is format agnostic, the format is simply passed along to the writer.
    /// The writer will decide if resampling and encoding are needed.
    /// </summary>
    public class ConsumerMJPEGRecorder : AbstractConsumer
    {
        public string Filename
        {
            get { return filename; }
        }

        public bool Recording
        {
            get { return recording; }
        }

        private ImageDescriptor imageDescriptor;
        private MJPEGWriter writer;
        private bool recording;
        private string filename;
        
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

            // If the capture happens at more than 100fps, set the video itself to be at 30fps.
            // This avoids erratic playback because the player can't cope with the framerate, drawback: prevents review in real time.
            // FIXME: fix the player so that it can playback high speed video in real time.
            double fileInterval = interval;
            if (interval < 10)
                fileInterval = 1000.0/30;

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
            writer.SaveFrame(imageDescriptor.Format, entry.Buffer, entry.PayloadLength, imageDescriptor.TopDown);
        }
    }
}
