using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Kinovea.ScreenManager;

namespace Kinovea.Tests
{
    public class LineClippingTester
    {
        public void Test()
        {
            Random random = new Random();

            Rectangle window = new Rectangle(500, 500, 1000, 1000);
            Bitmap image = new Bitmap(2000, 2000, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(image);

            g.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);
            g.DrawRectangle(Pens.Black, window);

            // Random lines, just to see that nothing outside the window is visible.
            /*for (int i = 0; i < 20; i++)
            {
                PointF a = random.NextPointF(0, 2000, 0, 2000);
                PointF b = random.NextPointF(0, 2000, 0, 2000);
                ClipAndDraw(g, random.NextColor(255), window, a, b);
            }*/


            // Hand crafted lines, to check special cases.

            // Normal lines.
            ClipAndDraw(g, random.NextColor(255), window, new PointF(250, 1750), new PointF(1750, 250));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(250, 1500), new PointF(1750, 0));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(500, 250), new PointF(1750, 1250));

            // Line not visible.
            ClipAndDraw(g, random.NextColor(255), window, new PointF(250, 250), new PointF(500, 1750));

            // Lines parallel to edges.
            ClipAndDraw(g, random.NextColor(255), window, new PointF(250, 250), new PointF(250, 1750));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(600, 250), new PointF(600, 1750));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(500, 250), new PointF(500, 1750));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(250, 750), new PointF(1750, 750));

            // Line defined by segment with at least one end inside window.
            ClipAndDraw(g, random.NextColor(255), window, new PointF(750, 750), new PointF(1250, 1250));
            //ClipAndDraw(g, random.NextColor(255), window, new PointF(750, 750), new PointF(1750, 250));
            ClipAndDraw(g, random.NextColor(255), window, new PointF(1750, 250), new PointF(750, 750));


            image.Save(@"test.png");

        }

        private void ClipAndDraw(Graphics g, Color c, Rectangle window, PointF a, PointF b)
        {
            ClipResult result = LiangBarsky.ClipLine(window, a, b);

            if (result.Visible)
            {
                using (Pen p = new Pen(c))
                    g.DrawLine(p, result.A, result.B);
            }
        }
    }
}
