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
    /// ConsumerMJPEGRecorder. Save samples to an MJPEG file (in MP4 container).
    /// The recorder is format agnostic, the format is simply passed along to the writer.
    /// The writer will decide if resampling and encoding are needed.
    /// </summary>
    public class ConsumerMJPEGRecorder : AbstractConsumer
    {
        public string Filename
        {
            get { return filename; }
        }

        private ImageDescriptor imageDescriptor;
        private MJPEGWriter writer;
        private string filename;
        
        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            this.imageDescriptor = imageDescriptor;
        }

        public SaveResult Prepare(string filename, double interval)
        {
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

            SaveResult result = writer.OpenSavingContext(filename, info, formatString, uncompressed, interval, fileInterval);

            return result;
        }

        protected override void AfterDeactivate()
        {
            writer.CloseSavingContext(true);
            writer.Dispose();
            writer = null;

            base.AfterDeactivate();
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            writer.SaveFrame(imageDescriptor.Format, entry.Buffer, entry.PayloadLength, imageDescriptor.TopDown);
        }
    }
}
