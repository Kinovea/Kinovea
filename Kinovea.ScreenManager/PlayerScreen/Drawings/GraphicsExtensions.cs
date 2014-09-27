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

        public static void DrawSquigglyLine(this Graphics graphics, Pen pen, PointF pt1, PointF pt2)
        {
            float halfPeriod = 8;

            Vector v = new Vector(pt1, pt2);
            float norm = v.Norm();
            float ratio = halfPeriod / norm;
            Vector step = v * ratio;
            Vector stepUp = new Vector(step.Y / 2, -step.X / 2);
            Vector stepDown = new Vector(-step.Y / 2, step.X / 2);

            List<PointF> pp = new List<PointF>();
            pp.Add(pt1);

            PointF current = new PointF(pt1.X + step.X, pt1.Y + step.Y);
            pp.Add(current);

            float steps = norm / halfPeriod;
            for (int i = 0; i < steps - 3; i++)
            {
                Vector dir = i % 2 == 0 ? stepUp : stepDown;
                current = new PointF(current.X + step.X, current.Y + step.Y);

                PointF p = new PointF(current.X + dir.X, current.Y + dir.Y);
                pp.Add(p);
            }

            pp.Add(new PointF(current.X + (step.X / 2), current.Y + (step.Y / 2)));
            pp.Add(pt2);

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
    }
    
}
