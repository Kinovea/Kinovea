using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class LiangBarsky
    {
        /// <summary>
        /// Implements the Liang-Barsky line clipping algorithm for segments, infinite lines or half lines.
        /// </summary>
        public static ClipResult ClipLine(RectangleF rect, PointF a, PointF b, double t0, double t1)
        {
            // ref paper: http://www.eecs.berkeley.edu/Pubs/TechRpts/1992/CSD-92-688.pdf
            
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            double[] p = {
                -dx,
                dx,
                -dy,
                dy
            };

            double[] q = { 
                a.X - rect.Left,
                rect.Right - a.X,
                a.Y - rect.Top, 
                rect.Bottom - a.Y
            };

            for (int i = 0; i < 4; i++)
            {
                if (p[i] == 0)
                {
                    // Parallel to edge.
                    if (q[i] < 0)
                        return ClipResult.Invisible;
                }
                else if (p[i] < 0)
                {
                    // Outside -> inside.
                    double t = q[i] / p[i];

                    if (t > t1)
                        return ClipResult.Invisible;

                    t0 = Math.Max(t, t0);
                }
                else if (p[i] > 0)
                {
                    // Inside -> outside.
                    double t = q[i] / p[i];

                    if (t < t0)
                        return ClipResult.Invisible;

                    t1 = Math.Min(t, t1);
                }
            }

            PointF entry = new PointF((float)(a.X + t0 * dx), (float)(a.Y + t0 * dy));
            PointF exit = new PointF((float)(a.X + t1 * dx), (float)(a.Y + t1 * dy));

            return new ClipResult(true, entry, exit);
        }

        /// <summary>
        /// Implements the Liang-Barsky line clipping algorithm for segments.
        /// </summary>
        public static ClipResult ClipSegment(RectangleF rect, PointF a, PointF b)
        {
            return ClipLine(rect, a, b, 0, 1);
        }

        /// <summary>
        /// Implements the Liang-Barsky line clipping algorithm for infinite lines.
        /// </summary>
        public static ClipResult ClipLine(RectangleF rect, PointF a, PointF b)
        {
            return ClipLine(rect, a, b, double.MinValue, double.MaxValue);
        }

    }
}
