using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class ArrowHelper
    {
        private static float arrowBaseDistance = 20f;
        private static float arrowBaseLength = 20f;

        /// <summary>
        /// Draws an arrow at the "a" endpoint of segment [ab].
        /// </summary>
        public static void Draw(Graphics canvas, Pen penEdges, Point a, Point b)
        {
            Vector v = new Vector(a, b);
            float norm = v.Norm();

            float cornerRatio = (arrowBaseLength / norm) / 2;
            Vector cornerUp = new Vector(v.Y * cornerRatio, -v.X * cornerRatio);
            Vector cornerDown = new Vector(-v.Y * cornerRatio, v.X * cornerRatio);

            float distanceRatio = arrowBaseDistance / norm;
            Vector baseArrow = v * distanceRatio;

            PointF u = new PointF(a.X + baseArrow.X + cornerUp.X, a.Y + baseArrow.Y + cornerUp.Y);
            PointF d = new PointF(a.X + baseArrow.X + cornerDown.X, a.Y + baseArrow.Y + cornerDown.Y);
            canvas.DrawLine(penEdges, a, u);
            canvas.DrawLine(penEdges, a, d);
        }
    }
}
