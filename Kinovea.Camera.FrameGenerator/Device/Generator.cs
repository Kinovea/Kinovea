using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Kinovea.Video;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Creates images with baked in current timestamp.
    /// </summary>
    public class Generator
    {
        private DeviceConfiguration configuration;
        private int stride;
        private byte[] background;
        private byte[] frameBuffer;
        private Bitmap bmpTimestamp;
        private Point timestampLocation = new Point(10, 10);
        private SolidBrush backBrush = new SolidBrush(Color.DarkGray);
        private SolidBrush foreBrush = new SolidBrush(Color.White);
        private Font font;
        private bool allocated;

        public Generator(DeviceConfiguration configuration)
        {
            this.configuration = configuration;
            stride = configuration.Width * 3; // fixme, depends on image format.

            Initialize();
        }

        public byte[] Generate()
        {
            if (!allocated)
                return null;

            CopyBackground();
            CopyTimestamp();

            return frameBuffer;
        }

        private void Initialize()
        {
            int bufferSize = ImageFormatHelper.ComputeBufferSize(configuration.Width, configuration.Height, configuration.ImageFormat);

            try
            {
                background = new byte[bufferSize];
                frameBuffer = new byte[bufferSize];

                InitializeTimestamp();

                allocated = true;
            }
            catch
            {
            
            }
        }

        private void InitializeTimestamp()
        {
            int height = configuration.Height / 20;
            int width = height * 10;
            font = new Font("Arial", height / 2, FontStyle.Regular);
            bmpTimestamp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
        }

        private void CopyBackground()
        {

        }

        private void CopyTimestamp()
        {
            // Reset timestamp bitmap
            string text = string.Format(@"{0:yyyy-MM-dd HH\:mm\:ss\.fff}", DateTime.Now);

            using (Graphics g = Graphics.FromImage(bmpTimestamp))
            {
                g.FillRectangle(backBrush, 0, 0, bmpTimestamp.Width, bmpTimestamp.Height);
                g.DrawString(text, font, foreBrush, Point.Empty);
            }

            // Copy bitmap content over framebuffer.
            BitmapHelper.CopyBitmapRectangle(bmpTimestamp, timestampLocation, frameBuffer, stride);
        }
    }
}
