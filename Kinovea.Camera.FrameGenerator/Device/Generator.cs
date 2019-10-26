using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Kinovea.Video;
using TurboJpegNet;
using Kinovea.Pipeline;
using System.Runtime.InteropServices;
using System.IO;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Creates images with baked in current timestamp.
    /// In the case of JPEG we just send the same frame over and over.
    /// Sends original frames. The caller is responsible for copying them before returning.
    /// </summary>
    public class Generator : IDisposable
    {
        private DeviceConfiguration configuration;
        private int stride;
        private Bitmap bmpTimestamp;    // Pre-allocated bitmap onto which we paint the timestamp.
        private Frame outputFrame;      // Pre-allocated output frame.
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

            if (configuration.ImageFormat == Video.ImageFormat.RGB24)
                CopyTimestamp();
            
            return outputFrame;
        }
        
        /// <summary>
        /// Pre-allocate the blank frame in the correct format.
        /// </summary>
        private void Initialize()
        {
            try
            {
                // We only support RGB24 and JPEG.
                if (configuration.ImageFormat != Video.ImageFormat.RGB24 && configuration.ImageFormat != Video.ImageFormat.JPEG)
                    throw new InvalidProgramException();

                int bufferSize = ImageFormatHelper.ComputeBufferSize(configuration.Width, configuration.Height, configuration.ImageFormat);
                outputFrame = new Frame(bufferSize);

                if (configuration.ImageFormat == Video.ImageFormat.RGB24)
                { 
                    stride = configuration.Width * 3;
                    InitializeTimestamp();
                    outputFrame.PayloadLength = bufferSize;
                }
                else
                {
                    InitializeJPEG();
                }

                allocated = true;
            }
            catch
            {
                log.ErrorFormat("Error while initializing camera simulator generator.");
            }
        }

        /// <summary>
        /// Creates a small bitmap onto which we'll paint the timestamp.
        /// </summary>
        private void InitializeTimestamp()
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

        private void InitializeJPEG()
        {
            // Take the initial framebuffer (blank RGB24) and encode it into a JPEG, then copy that JPEG back into the framebuffer.
            IntPtr jpegBuf = IntPtr.Zero;
            uint jpegSize = 0;
            IntPtr handle = tjnet.tjInitCompress();
            int pitch = configuration.Width * 3;
            TJPF format = TJPF.TJPF_BGR;
            int quality = 90;
            int result = tjnet.tjCompress2(handle, outputFrame.Buffer, configuration.Width, pitch, configuration.Height, format, ref jpegBuf, ref jpegSize, TJSAMP.TJSAMP_420, quality, TJFLAG.TJFLAG_FASTDCT);

            tjnet.tjDestroy(handle);

            Marshal.Copy(jpegBuf, outputFrame.Buffer, 0, (int)jpegSize);
            outputFrame.PayloadLength = (int)jpegSize;

            tjnet.tjFree(jpegBuf);
        }

        /// <summary>
        /// Paint the timestamp on the dedicated bitmap, then copy that bitmap into the output frame.
        /// </summary>
        private void CopyTimestamp()
        {
            string text = string.Format(@"{0:yyyy-MM-dd HH\:mm\:ss\.fff}", DateTime.Now);
            using (Graphics g = Graphics.FromImage(bmpTimestamp))
            {
                g.FillRectangle(backBrush, 0, 0, bmpTimestamp.Width, bmpTimestamp.Height);
                g.DrawString(text, font, foreBrush, Point.Empty);
            }

            BitmapHelper.CopyBitmapRectangle(bmpTimestamp, timestampLocation, outputFrame.Buffer, stride);
        }
    }
}
