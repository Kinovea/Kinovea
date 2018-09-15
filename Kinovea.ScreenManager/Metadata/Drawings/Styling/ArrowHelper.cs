using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kinovea.ScreenManager
{
    public static class ArrowHelper
    {
        /// <summary>
        /// Draws an arrow at the "a" endpoint of segment [ab].
        /// </summary>
        public static void Draw(Graphics canvas, Pen penEdges, Point a, Point b)
        {
            Vector v = new Vector(a, b);
            float norm = v.Norm();
            
            // The arrow is drawn at the start point of the segment, so the vector [ab] can be used to get relative segments.
            // All dimensions are relative to the segment width.
            // Left and Right are to be understood as when the arrow is pointing downwards.

            float refLength = penEdges.Width;
            refLength = Math.Max(refLength, 4);
            
            // 1. Point along the segment, inside the segment.
            float triangleBaseRatio = refLength / norm;
            Vector triangleBaseRelative = v * triangleBaseRatio;
            PointF triangleBase = new PointF(a.X + triangleBaseRelative.X, a.Y + triangleBaseRelative.Y);

            // 2. Point along the segment, outside the segment.
            float triangleTopRatio = (refLength * 3) / norm;
            Vector triangleTopRelative = v * triangleTopRatio;
            PointF triangleTop = new PointF(a.X - triangleTopRelative.X, a.Y - triangleTopRelative.Y);

            // 3. Points perpendicular to the segment.
            // When the constant goes smaller, the base of the triangle is smaller.
            float triangleSideRatio = (refLength * 1.5f) / norm;
            Vector triangleLeftRelative = new Vector(v.Y * triangleSideRatio, -v.X * triangleSideRatio);
            PointF triangleLeft = new PointF(triangleBase.X + triangleLeftRelative.X, triangleBase.Y + triangleLeftRelative.Y);
            Vector triangleRightRelative = new Vector(-v.Y * triangleSideRatio, v.X * triangleSideRatio);
            PointF triangleRight = new PointF(triangleBase.X + triangleRightRelative.X, triangleBase.Y + triangleRightRelative.Y);
            
            FillTriangle(canvas, penEdges.Color, triangleTop, triangleLeft, triangleRight);
        }
        
        private static void DrawTriangle(Graphics canvas, Color color, PointF a, PointF b, PointF c)
        {
            using (Pen pen = new Pen(color))
            {
                canvas.DrawLine(pen, a, b);
                canvas.DrawLine(pen, b, c);
                canvas.DrawLine(pen, c, a);
            }
        }

        private static void FillTriangle(Graphics canvas, Color color, PointF a, PointF b, PointF c)
        {
            using (GraphicsPath path = new GraphicsPath())
            using (SolidBrush brush = new SolidBrush(color))
            {
                path.AddLine(a, b);
                path.AddLine(b, c);
                path.CloseFigure();
                canvas.FillPath(brush, path);
            }
        }
    }
}
