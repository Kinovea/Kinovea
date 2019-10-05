using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.IO;
using System;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using System.Drawing;
using Kinovea.Services;
using TurboJpegNet;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// ConsumerDelayer. 
    /// Push incoming samples to the delay buffer. Pulls from the delay buffer and saves to file.
    /// Convert all samples to RGB24 to be pushed into the delay buffer.
    /// </summary>
    public class ConsumerDelayer : AbstractConsumer
    {
        public string Filename
        {
            get { return filename; }
        }

        public bool Recording
        {
            get { return recording; }
        }

        private ImageDescriptor inputImageDescriptor;
        private int width;
        private int height;
        private int pitch;
        private Rectangle rect;
        private Bitmap bitmap;
        private byte[] decoded;
        private bool allocated;
        private Delayer delayer;
        private int age;
        private ImageDescriptor delayerImageDescriptor;
        private Frame delayedFrame;
        private MJPEGWriter writer;
        private bool recording;
        private string filename;
        private bool stopRecordAsked;
        private string shortId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConsumerDelayer(string shortId)
        {
            this.shortId = shortId;
        }

        /// <summary>
        /// Set the image descriptor for the incoming frames.
        /// </summary>
        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            // Allocate a long-lived bitmap that we will use to receive incoming samples.
            // And a long-lived frame we will use to collect delayed frames and send them to the writer.
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }

            if (decoded != null)
                decoded = null;

            if (delayedFrame != null)
            {
                delayedFrame = null;
            }

            GC.Collect();

            allocated = false;

            try
            {
                // Prepare the long-lived bitmap.
                this.inputImageDescriptor = imageDescriptor;
                width = imageDescriptor.Width;
                height = imageDescriptor.Height;
                rect = new Rectangle(0, 0, width, height);
                pitch = width * 3;

                decoded = new byte[pitch * height];
                bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

                // Prepare the long-lived delayed frame.
                SetDelayerImageDescriptor(imageDescriptor);
                delayedFrame = new Frame(delayerImageDescriptor.BufferSize);
                
                allocated = true;
            }
            catch (Exception e)
            {
                log.Error("The buffer could not be allocated.");
                log.Error(e);
            }
        }


        public void PrepareDelay(Delayer delayer)
        {
            this.delayer = delayer;
        }

        public SaveResult StartRecord(string filename, double interval, int age)
        {
            //-----------------------
            // Runs on the UI thread.
            //-----------------------

            if (delayerImageDescriptor == null)
                throw new NotSupportedException("ImageDescriptor must be set before prepare.");

            this.age = age;
            this.filename = filename;

            if (writer != null)
                writer.Dispose();

            writer = new MJPEGWriter();

            VideoInfo info = new VideoInfo();
            info.OriginalSize = new Size(delayerImageDescriptor.Width, delayerImageDescriptor.Height);

            bool uncompressed = PreferencesManager.CapturePreferences.SaveUncompressedVideo && delayerImageDescriptor.Format != Video.ImageFormat.JPEG;
            string formatString = FilenameHelper.GetFormatStringCapture(uncompressed);

            // If the capture happens at more than 100fps, set the video itself to be at 30fps.
            // This avoids erratic playback because the player can't cope with the framerate, drawback: prevents review in real time.
            double hrft = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold;
            double hrfo = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput;
            double fps = 1000.0 / interval;
            double fileInterval = interval;
            if (fps >= hrft)
            {
                fileInterval = 1000.0 / hrfo;
                log.DebugFormat("High speed recording detected, {0:0.###} fps. Forcing output framerate to {1:0.###} fps.", fps, hrfo);
            }

            log.DebugFormat("Frame budget for writer [{0}]: {1:0.000} ms.", shortId, interval);

            SaveResult result = writer.OpenSavingContext(filename, info, formatString, delayerImageDescriptor.Format, uncompressed, interval, fileInterval);

            recording = true;

            return result;
        }

        public void StopRecord()
        {
            //-----------------------
            // Runs on the UI thread.
            //-----------------------
            stopRecordAsked = true;
        }

        protected override void AfterDeactivate()
        {
            if (recording)
                DoStopRecord();

            base.AfterDeactivate();
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            if (!allocated)
                return;

            switch (inputImageDescriptor.Format)
            {
                case Video.ImageFormat.RGB24:
                    BitmapHelper.FillFromRGB24(bitmap, rect, inputImageDescriptor.TopDown, entry.Buffer);
                    break;
                case Video.ImageFormat.RGB32:
                    BitmapHelper.FillFromRGB32(bitmap, rect, inputImageDescriptor.TopDown, entry.Buffer);
                    break;
                case Video.ImageFormat.Y800:
                    BitmapHelper.FillFromY800(bitmap, rect, inputImageDescriptor.TopDown, entry.Buffer);
                    break;
                case Video.ImageFormat.JPEG:
                    FillBitmapJPEG(entry.Buffer, entry.PayloadLength);
                    break;
            }

            // We don't move back this call to the UI thread.
            // During recording we extract frames from the delayer on that very same thread, 
            // and for the display it's not critical that the images be broken. (less critical than switching context each frame).
            // As this mode is tailored for delay scenario, in all likelihood the display is not going to be reading the frame we are writing to.
            bool pushed = delayer.Push(bitmap);
            if (!pushed)
            {
                // Very critical error. Most likely cross thread access to the same frame.
                // Let's deactivate to avoid looping on the error.
                log.ErrorFormat("Critical error while trying to push frame to delayer.");
                DoStopRecord();
                Deactivate();
            }

            if (stopRecordAsked)
            {
                DoStopRecord();
            }
            else if (recording)
            {
                // Extract a bitmap from delayer at right delay and convert it into a frame for the writer.
                // Note that we do not go through the delay compositer. We only support "normal" delay here.
                // Compositers (e.g: quadrants with different ages) are only supported in display.
                Bitmap delayedBitmap = delayer.Get(age);
                BitmapHelper.CopyBitmapToBuffer(delayedBitmap, delayedFrame.Buffer);
                delayedFrame.PayloadLength = delayerImageDescriptor.BufferSize;

                writer.SaveFrame(delayerImageDescriptor.Format, delayedFrame.Buffer, delayedFrame.PayloadLength, delayerImageDescriptor.TopDown);
            }
        }

        private void SetDelayerImageDescriptor(ImageDescriptor imageDescriptor)
        {
            // Describes the format of frames we send to the writer.
            // The writer is then responsible for compressing or converting depending on "record raw" option.
            // We'll pull frames from the delay buffer which is always storing RGB24 top-down.
            Video.ImageFormat format = Video.ImageFormat.RGB24;
            bool topDown = true;
            int pfBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            delayerImageDescriptor = new ImageDescriptor(format, width, height, topDown, pfBufferSize);
        }

        private void DoStopRecord()
        {
            //---------------------------------------
            // Must be called on the consumer thread.
            //---------------------------------------
            stopRecordAsked = false;

            if (!recording)
                return;

            writer.CloseSavingContext(true);
            writer.Dispose();
            writer = null;

            recording = false;
        }

        private void FillBitmapJPEG(byte[] buffer, int payloadLength)
        {
            // Convert JPEG to RGB24 buffer then to bitmap.

            IntPtr handle = tjnet.tjInitDecompress();

            uint jpegSize = (uint)payloadLength;
            int width;
            int height;
            TJSAMP jpegSubsamp;
            tjnet.tjDecompressHeader2(handle, buffer, jpegSize, out width, out height, out jpegSubsamp);

            tjnet.tjDecompress2(handle, buffer, jpegSize, decoded, width, pitch, height, TJPF.TJPF_BGR, TJFLAG.TJFLAG_FASTDCT);

            tjnet.tjDestroy(handle);

            // Encapsulate into bitmap.
            // Fixme: do we need the copy here? What about getting an IntPtr from tjnet and setting it to scan0?
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Marshal.Copy(decoded, 0, bmpData.Scan0, bmpData.Stride * bitmap.Height);
            bitmap.UnlockBits(bmpData);
        }
    }
}
