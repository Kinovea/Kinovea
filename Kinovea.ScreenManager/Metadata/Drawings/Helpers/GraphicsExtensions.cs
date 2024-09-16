using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class GraphicsExtensions
    {
        public static float tension = 0.9f;
        public static float dodgeFactor = 1.5f;

        public static void DrawSquigglyLine(this Graphics graphics, Pen pen, PointF pt1, PointF pt2)
        {
            List<PointF> pp = new List<PointF>();
            pp.Add(pt1);

            AddSquigglePoints(pp, pt1, pt2, pen.Width);
            graphics.DrawCurve(pen, pp.ToArray(), tension);
        }

        public static void DrawSquigglyLine(this Graphics graphics, Pen pen, Point pt1, Point pt2)
        {
            DrawSquigglyLine(graphics, pen, (PointF)pt1, (PointF)pt2);
        }

        public static void DrawSquigglyLine(this Graphics graphics, Pen pen, int x1, int y1, int x2, int y2)
        {
            Point pt1 = new Point(x1, y1);
            Point pt2 = new Point(x2, y2);
            DrawSquigglyLine(graphics, pen, pt1, pt2);
        }

        public static void DrawSquigglyLine(this Graphics graphics, Pen pen, float x1, float y1, float x2, float y2)
        {
            PointF pt1 = new PointF(x1, y1);
            PointF pt2 = new PointF(x2, y2);
            DrawSquigglyLine(graphics, pen, pt1, pt2);
        }

        public static void DrawSquigglyLines(this Graphics graphics, Pen pen, Point[] points)
        {
            if (points.Length < 2)
                return;

            List<PointF> pp = new List<PointF>();
            pp.Add(points.First());

            for (int i = 0; i < points.Length - 1; i++)
            {
                Point a = points[i];
                Point b = points[i + 1];
                if (a != b)
                    AddSquigglePoints(pp, a, b, pen.Width);
            }

            pp.Add(points.Last());
            graphics.DrawCurve(pen, pp.ToArray(), tension);
        }

        private static void AddSquigglePoints(List<PointF> points, PointF a, PointF b, float refLength)
        {
            // The half period corresponds to how much we stray away from the segment.
            // Smaller dodgeFactor means tighter zigzags.
            refLength = Math.Max(refLength, 4);
            float halfPeriod = refLength * dodgeFactor;

            Vector v = new Vector(a, b);
            float norm = v.Norm();
            float ratio = halfPeriod / norm;
            Vector step = v * ratio;
            Vector stepUp = new Vector(step.Y / 2, -step.X / 2);
            Vector stepDown = new Vector(-step.Y / 2, step.X / 2);

            PointF current = new PointF(a.X + step.X, a.Y + step.Y);
            points.Add(current);

            float steps = norm / halfPeriod;
            for (int i = 0; i < steps - 3; i++)
            {
                Vector dir = i % 2 == 0 ? stepUp : stepDown;
                current = new PointF(current.X + step.X, current.Y + step.Y);

                PointF p = new PointF(current.X + dir.X, current.Y + dir.Y);
                points.Add(p);
            }

            points.Add(new PointF(current.X + (step.X / 2), current.Y + (step.Y / 2)));
            points.Add(b);
        }
    
    }
    
}
