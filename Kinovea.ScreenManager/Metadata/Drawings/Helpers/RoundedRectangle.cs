using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A helper class to draw a rounded rectangle for labels.
    /// The rectangle can have a drop shape (top left and bottom right corners are "pointy").
    /// It can also have a hidden handler in the bottom right corner.
    /// Change of size resulting from moving the hidden handler is the responsibility of the caller.
    /// </summary>
    public class RoundedRectangle
    {
        #region Properties
        public RectangleF Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }
        public PointF Center
        {
            get { return new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2); }
        }
        public float X
        {
            get { return rectangle.X; }
        }
        public float Y
        {
            get { return rectangle.Y; }
        }
        #endregion

        #region Members
        private RectangleF rectangle;
        #endregion

        /// <summary>
        /// Draw a rounded rectangle on the provided canvas. 
        /// This method is typically used after applying a transform to the original rectangle.
        /// </summary>
        public static void Draw(Graphics canvas, RectangleF rect, SolidBrush brush, int radius, bool dropShape, bool contour, Pen penContour)
        {
            if (dropShape)
                DrawDropShape(canvas, rect, brush, radius, contour, penContour);
            else
                DrawRoundedRectangle(canvas, rect, brush, radius, contour, penContour);
        }

        private static void DrawDropShape(Graphics canvas, RectangleF rect, SolidBrush brush, int radius, bool contour, Pen penContour)
        {
            int marginLeft = radius;
            int marginTop = radius;
            int marginRight = radius;
            int marginBottom = (int)(radius * 0.7f);
            float diameter = radius * 2;

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            // Corners drawn starting from top-left, clockwise.
            // The arc themselves are drawn using start/sweep angles in degrees, starts at X-axis and goes clockwise.
            gp.AddLine(rect.Left - marginLeft, rect.Top - marginTop, rect.Left - marginLeft + diameter, rect.Top - marginTop);
            gp.AddArc(rect.Right - diameter + marginRight, rect.Top - marginTop, diameter, diameter, 270, 90);
            gp.AddLine(rect.Right + marginRight, rect.Bottom + marginBottom, rect.Right - diameter + marginRight, rect.Bottom + marginBottom);
            gp.AddArc(rect.Left - marginLeft, rect.Bottom - diameter + marginBottom, diameter, diameter, 90, 90);
            gp.CloseFigure();
            gp.CloseFigure();

            canvas.FillPath(brush, gp);

            if (contour)
                canvas.DrawPath(penContour, gp);

            gp.Dispose();
        }

        private static void DrawRoundedRectangle(Graphics canvas, RectangleF rect, SolidBrush brush, int radius, bool contour, Pen penContour)
        {
            int marginLeft = radius;
            int marginTop = radius;
            int marginRight = radius;
            int marginBottom = (int)(radius * 0.7f);
            float diameter = radius * 2;
        
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            
            // Corners drawn starting from top-left, clockwise.
            // The arc themselves are drawn using start/sweep angles in degrees, starts at X-axis and goes clockwise.
            gp.AddArc(rect.Left - marginLeft, rect.Top - marginTop, diameter, diameter, 180, 90);
            gp.AddArc(rect.Right - diameter + marginRight, rect.Top - marginTop, diameter, diameter, 270, 90);
            gp.AddArc(rect.Right - diameter + marginRight, rect.Bottom - diameter + marginBottom, diameter, diameter, 0, 90);
            gp.AddArc(rect.Left - marginLeft, rect.Bottom - diameter + marginBottom, diameter, diameter, 90, 90);
            gp.CloseFigure();
            
            canvas.FillPath(brush, gp);
            
            if(contour)
                canvas.DrawPath(penContour, gp);
            
            gp.Dispose();
        }
        public int HitTest(PointF point, bool hiddenHandle, int hiddenHandleRadius, IImageToViewportTransformer transformer)
        {
            int result = -1;

            SizeF size = rectangle.Size;
            RectangleF hitArea = rectangle;

            if (hiddenHandle && hiddenHandleRadius > 0)
            {
                PointF handleCenter = new PointF(hitArea.Right, hitArea.Bottom);
                if (handleCenter.Box(hiddenHandleRadius).Contains(point))
                    result = 1;
            }

            if (result < 0 && hitArea.Contains(point))
                result = 0;

            return result;
        }
        public void Move(float dx, float dy)
        {
            rectangle = rectangle.Translate(dx, dy);
        }
        public void CenterOn(PointF point)
        {
            PointF location = new PointF(point.X - rectangle.Size.Width / 2, point.Y - rectangle.Size.Height / 2);
            rectangle = new RectangleF(location, rectangle.Size);
        }
    }
}
