using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace Kinovea.Camera.FrameGenerator
{
    public class Generator
    {
        private DateTime then = DateTime.MaxValue;

        public Bitmap Generate(Size size)
        {
            DateTime now = DateTime.Now;
            
            Color backgroundColor = Color.AliceBlue;
            Color foregroundColor = Color.FromArgb(backgroundColor.A, 255 - backgroundColor.R, 255 - backgroundColor.G, 255 - backgroundColor.B);

            if (size.Width < 1 || size.Height < 1)
                size = new Size(640, 480);

            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (SolidBrush backBrush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(backBrush, g.ClipBounds);

                using (SolidBrush foreBrush = new SolidBrush(foregroundColor))
                using (Font font = new Font("Arial", 20, FontStyle.Regular))
                {
                    string dateText = string.Format(@"{0:yyyy-MM-dd HH\:mm\:ss-fff}", now);
                    g.DrawString(dateText, font, foreBrush, new PointF(25, 25));

                    if (then != DateTime.MaxValue)
                    {
                        TimeSpan span = now - then;
                        string spanText = string.Format(CultureInfo.InvariantCulture, "+{0}ms: {1:0.000}fps", span.TotalMilliseconds, 1000 / span.TotalMilliseconds);
                        g.DrawString(spanText, font, foreBrush, new PointF(25, 60));
                    }
                }
            }
            
            then = now;
            
            return bitmap;
        }
        
    }
}
