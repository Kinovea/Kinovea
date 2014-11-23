using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.IO;
using System;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// ConsumerMJPEGRecorder. Save samples to an MJPEG file (in MP4 container).
    /// The recorder is format agnostic, the format is simply passed along to the writer.
    /// The writer will decide if resampling and encoding are needed.
    /// </summary>
    public class ConsumerMJPEGRecorder : AbstractConsumer
    {
        private ImageDescriptor imageDescriptor;
        private MJPEGWriter writer;
        
        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            this.imageDescriptor = imageDescriptor;
        }

        public SaveResult Prepare(string filename, double interval)
        {
            if (imageDescriptor == null)
                throw new NotSupportedException("ImageDescriptor must be set before prepare.");

            if (writer != null)
                writer.Dispose();

            writer = new MJPEGWriter();
            
            VideoInfo info = new VideoInfo();
            info.OriginalSize = new Size(imageDescriptor.Width, imageDescriptor.Height);

            SaveResult result = writer.OpenSavingContext(filename, info, interval);

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
            writer.SaveFrame(imageDescriptor.Format, entry.Buffer, entry.PayloadLength);
        }
    }
}
