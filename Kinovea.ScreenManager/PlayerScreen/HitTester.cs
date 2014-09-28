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

        public static bool HitTest(PointF target, Point mouse, IImageToViewportTransformer transformer)
        {
            int radius = 10;
            int boxRadius = Math.Max(1, transformer.Untransform(radius));
            return target.Box(boxRadius).Contains(mouse);
        }

        public static bool HitTest(GraphicsPath path, Point mouse, int lineSize, bool area, IImageToViewportTransformer transformer)
        {
            if (!area)
            {
                int expansion = 10;
                int enlarger = Math.Max(1, transformer.Untransform(expansion));

                using (Pen pathPen = new Pen(Color.Black, lineSize + enlarger))
                {
                    bool conflated = false;
                    if (path.PathPoints.Length > 1)
                    {
                        PointF first = path.PathPoints[0];
                        conflated = path.PathPoints.Skip(1).All(p => p == first);
                        if (conflated)
                            path.AddRectangle(first.Box(5));
                    }
            
                    path.Widen(pathPen);
                }
            }
            
            using (Region r = new Region(path))
            {
                return r.IsVisible(mouse);
            }
        }
    }
}
