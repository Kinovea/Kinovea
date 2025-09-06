using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using Kinovea.Pipeline;
using Kinovea.Services;
using Kinovea.Services.TurboJpeg;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Creates images with baked in current timestamp.
    /// In the case of JPEG we just send the same 8 frames over and over.
    /// Sends original frames. The caller is responsible for copying them before returning.
    /// </summary>
    public class Generator : IDisposable
    {
        private DeviceConfiguration configuration;
        private int stride;
        private List<Frame> frames = new List<Frame>();
        private Bitmap bmpTimestamp;        // Pre-allocated bitmap onto which we paint the timestamp.
        private int position;               // Absolute position.
        private int capacity = 8;
        private Point timestampLocation = new Point(10, 10);
        private SolidBrush backBrush = new SolidBrush(Color.DarkGray);
        private SolidBrush foreBrush = new SolidBrush(Color.White);
        private Font font;
        private bool allocated;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Construction & disposal
        public Generator(DeviceConfiguration configuration)
        {
            this.configuration = configuration;
            Initialize();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Generator()
        {
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bmpTimestamp != null)
                    bmpTimestamp.Dispose();

                frames.Clear();
            }
        }
        #endregion

        /// <summary>
        /// Returns a new frame.
        /// This is not a copy, the frame is owned by the generator. Callers must copy it before returning.
        /// </summary>
        public Frame GetFrame()
        {
            if (!allocated)
                return null;

            Frame entry = frames[position % capacity];

            if (configuration.ImageFormat == Kinovea.Services.ImageFormat.RGB24)
            {
                string text = string.Format(@"{0:HH\:mm\:ss\.fff} ({1})", DateTime.Now, position);
                CopyTimestamp(entry, text);
            }

            position++;

            return entry;
        }
        
        /// <summary>
        /// Pre-allocate the frames in the correct format.
        /// </summary>
        private void Initialize()
        {
            try
            {
                if (configuration.ImageFormat != Kinovea.Services.ImageFormat.RGB24 && configuration.ImageFormat != Kinovea.Services.ImageFormat.JPEG)
                    throw new InvalidProgramException();

                stride = configuration.Width * 3;
                InitializeTimestampBitmap();
                
                frames.Clear();
                frames = new List<Frame>(capacity);

                int bufferSize = ImageFormatHelper.ComputeBufferSize(configuration.Width, configuration.Height, configuration.ImageFormat);
                for (int i = 0; i < capacity; i++)
                {
                    frames.Add(new Frame(bufferSize));

                    if (configuration.ImageFormat == Kinovea.Services.ImageFormat.RGB24)
                        frames[i].PayloadLength = bufferSize;
                    else
                        InitializeJPEG(frames[i], i);
                }

                allocated = true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while initializing camera simulator generator. {0}", e.Message);
            }
        }

        /// <summary>
        /// Creates a small bitmap onto which we'll paint the timestamp or frame index.
        /// </summary>
        private void InitializeTimestampBitmap()
        {
            if (bmpTimestamp != null)
            {
                bmpTimestamp.Dispose();
                bmpTimestamp = null;
            }

            int height = configuration.Height / 20;
            int width = height * 10;
            font = new Font("Arial", height / 2, FontStyle.Regular);
            bmpTimestamp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
        }

        /// <summary>
        /// Prepare the JPEG at the passed slot index.
        /// </summary>
        private void InitializeJPEG(Frame entry, int i)
        {
            // Take the initial framebuffer (blank RGB24), paint the timestamp on it, encode it into a JPEG, then copy that JPEG back into the framebuffer.
            string text = string.Format("({0})", i);
            CopyTimestamp(entry, text);

            IntPtr jpegBuf = IntPtr.Zero;
            uint jpegSize = 0;
            IntPtr handle = tjnet.tjInitCompress();
            int pitch = configuration.Width * 3;
            TJPF format = TJPF.TJPF_BGR;
            int quality = 90;
            int result = tjnet.tjCompress2(handle, entry.Buffer, configuration.Width, pitch, configuration.Height, format, ref jpegBuf, ref jpegSize, TJSAMP.TJSAMP_420, quality, TJFLAG.TJFLAG_FASTDCT);

            tjnet.tjDestroy(handle);

            Marshal.Copy(jpegBuf, entry.Buffer, 0, (int)jpegSize);
            entry.PayloadLength = (int)jpegSize;

            tjnet.tjFree(jpegBuf);
        }

        /// <summary>
        /// Paint the timestamp on the dedicated bitmap, then copy that bitmap into the output frame.
        /// </summary>
        private void CopyTimestamp(Frame entry, string text)
        {
            using (Graphics g = Graphics.FromImage(bmpTimestamp))
            {
                g.FillRectangle(backBrush, 0, 0, bmpTimestamp.Width, bmpTimestamp.Height);
                g.DrawString(text, font, foreBrush, Point.Empty);
            }

            BitmapHelper.CopyBitmapToBufferRectangle(bmpTimestamp, timestampLocation, entry.Buffer, stride);
        }
    }
}
