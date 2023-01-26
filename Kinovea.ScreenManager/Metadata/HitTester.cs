using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public static class HitTester
    {

        /// <summary>
        /// Check if the passed point is near another point.
        /// </summary>
        /// <param name="p">The candidate point.</param>
        /// <param name="target">The location we want to know if we hit.</param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static bool HitPoint(PointF p, PointF target, IImageToViewportTransformer transformer)
        {
            int radius = 10;
            int boxRadius = Math.Max(1, transformer.Untransform(radius));
            return target.Box(boxRadius).Contains(p);
        }

        /// <summary>
        /// Check if a point is near a polyline or is inside a polygon.
        /// </summary>
        /// <param name="p">The candidate point.</param>
        /// <param name="path">The path we want to know if we hit.</param>
        /// <param name="lineSize">The width around the polyline considered a hit.</param>
        /// <param name="area">True if the path is an area rather than a polyline.</param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static bool HitPath(PointF p, GraphicsPath path, int lineSize, bool area, IImageToViewportTransformer transformer)
        {
            if (!area)
            {
                int expansion = 10;
                int enlarger = Math.Max(1, transformer.Untransform(expansion));

                try
                {
                    using (Pen pathPen = new Pen(Color.Black, lineSize + enlarger))
                    {
                        if (path.PathPoints.Length == 1)
                            path.AddEllipse(path.PathPoints[0].Box(5));
                        else
                            path.Widen(pathPen);
                    }
                }
                catch
                {
                    // Sometimes the path is invalid because it's too big or it is collapsed.
                    return false;
                }
            }
            
            using (Region r = new Region(path))
            {
                return r.IsVisible(p);
            }
        }

        /// <summary>
        /// Check if the passed point is on a segment. Supports distortion.
        /// </summary>
        /// <param name="p"> the candidate point.</param>
        /// <param name="a">Start point of the line in image space (non distorted).</param>
        /// <param name="b">End point of the line in image space (non distorted).</param>
        /// <returns></returns>
        public static bool HitLine(PointF p, PointF a, PointF b, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            if (a == b)
                return false;

            using (GraphicsPath path = new GraphicsPath())
            {
                if (distorter != null && distorter.Initialized)
                {
                    List<PointF> curve = distorter.DistortRectifiedLine(a, b);
                    path.AddCurve(curve.ToArray());
                }
                else
                {
                    path.AddLine(a, b);
                }

                return HitPath(p, path, 1, false, transformer);
            }
        }
    }
}
