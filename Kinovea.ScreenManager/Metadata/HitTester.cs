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

        public static bool HitTest(PointF target, PointF mouse, IImageToViewportTransformer transformer)
        {
            int radius = 10;
            int boxRadius = Math.Max(1, transformer.Untransform(radius));
            return target.Box(boxRadius).Contains(mouse);
        }

        public static bool HitTest(GraphicsPath path, PointF mouse, int lineSize, bool area, IImageToViewportTransformer transformer)
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
                    // Sometimes the path is invalid because it's too big or it collapsed.
                    return false;
                }
            }
            
            using (Region r = new Region(path))
            {
                return r.IsVisible(mouse);
            }
        }
    }
}
