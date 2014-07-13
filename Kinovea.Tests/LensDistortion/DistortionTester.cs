using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Tests
{
    public class DistortionTester
    {
        public void Test()
        {

            // TODO:
            // Missing a way to build intrinsics from list of params.
            // Will be needed for import anyway.
        }

        /*private void TestLines()
        {
            // Draw some lines in distorted space.
            Random random = new Random();
            int total = 10;
            List<PointF[]> lines = new List<PointF[]>();
            for (int i = 0; i < total; i++)
            {
                PointF start = NextPointF(random, 0, imageSize.Width, 0, imageSize.Height);
                PointF end = NextPointF(random, 0, imageSize.Width, 0, imageSize.Height);

                PointF[] points = new PointF[2] { start, end };
                lines.Add(points);
            }

            Bitmap bmp = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRectangle(Brushes.White, 0, 0, imageSize.Width, imageSize.Height);

            Pen p = new Pen(Color.Black);

            foreach (PointF[] line in lines)
            {
                // Draw original line.
                g.DrawLine(Pens.Red, line[0], line[1]);

                // Draw distorted line.
                List<PointF> points = DistortLine(line[0], line[1]);

                g.DrawCurve(Pens.Green, points.ToArray());
            }

            bmp.Save(@"C:\Users\Joan\Videos\Kinovea\Video Testing\undistort\maps\lines.png");
        }

        private PointF NextPointF(Random random, int minX, int maxX, int minY, int maxY)
        {
            return new PointF((float)NextDouble(random, minX, maxX), (float)NextDouble(random, minY, maxY));
        }

        private double NextDouble(Random random, double min, double max)
        {
            return min + (random.NextDouble() * (max - min));
        }*/
    }
}
