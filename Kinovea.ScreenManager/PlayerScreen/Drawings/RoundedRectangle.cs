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
            get { return m_Rectangle; }
            set { m_Rectangle = value; }
        }
        public PointF Center
        {
            get { return new PointF(m_Rectangle.X + m_Rectangle.Width / 2, m_Rectangle.Y + m_Rectangle.Height / 2); }
        }
        public float X
        {
            get { return m_Rectangle.X; }
        }
        public float Y
        {
            get { return m_Rectangle.Y; }
        }
        #endregion

        #region Members
        //private Rectangle m_Rectangle;
        private RectangleF m_Rectangle;
        #endregion

        /// <summary>
        /// Draw a rounded rectangle on the provided canvas. 
        /// This method is typically used after applying a transform to the original rectangle.
        /// </summary>
        /// <param name="_canvas">The graphics object on which to draw</param>
        /// <param name="_rect">The rectangle specifications</param>
        /// <param name="_brush">Brush to draw with</param>
        /// <param name="_radius">Radius of the rounded corners</param>
        public static void Draw(Graphics _canvas, RectangleF _rect, SolidBrush _brush, int _radius, bool _dropShape, bool _contour, Pen _penContour)
        {
            float diameter = 2F * _radius;
            RectangleF arc = new RectangleF(_rect.Location, new SizeF(diameter, diameter));
        
            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();
            
            if(_dropShape)
                gp.AddLine(arc.Left, arc.Top, arc.Right, arc.Top);
            else
                gp.AddArc(arc, 180, 90);
            
            arc.X = _rect.Right - diameter;
            gp.AddArc(arc, 270, 90);

            arc.Y = _rect.Bottom - diameter;
             if(_dropShape)
                gp.AddLine(arc.Right, arc.Top, arc.Right, arc.Bottom);
            else
                gp.AddArc(arc, 0, 90);
            
            arc.X = _rect.Left;
            gp.AddArc(arc, 90, 90);
            
            gp.CloseFigure();
            
            _canvas.FillPath(_brush, gp);
            
            if(_contour)
                _canvas.DrawPath(_penContour, gp);
            
            gp.Dispose();
        }
        public int HitTest(Point point, bool hiddenHandle, IImageToViewportTransformer transformer)
        {
            int result = -1;
            if (hiddenHandle)
            {
                int boxSide = transformer.Untransform(10);
                PointF bottomRight = new PointF(m_Rectangle.Right, m_Rectangle.Bottom);
                if (bottomRight.Box(boxSide).Contains(point))
                    result = 1;
            }

            if (result < 0 && m_Rectangle.Contains(point))
                result = 0;

            return result;
        }
        public void Move(float dx, float dy)
        {
            m_Rectangle = m_Rectangle.Translate(dx, dy);
        }
        public void CenterOn(PointF point)
        {
            PointF location = new PointF(point.X - m_Rectangle.Size.Width / 2, point.Y - m_Rectangle.Size.Height / 2);
            m_Rectangle = new RectangleF(location, m_Rectangle.Size);
        }
    }
}
