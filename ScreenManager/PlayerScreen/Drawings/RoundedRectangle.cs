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
        public Rectangle Rectangle
        {
            get { return m_Rectangle; }
            set { m_Rectangle = value; }
        }
        public Point Center
        {
            get { return new Point(m_Rectangle.X + m_Rectangle.Width / 2, m_Rectangle.Y + m_Rectangle.Height / 2); }
        }
        public int X
        {
            get { return m_Rectangle.X; }
        }
        public int Y
        {
            get { return m_Rectangle.Y; }
        }
        #endregion

        #region Members
        private Rectangle m_Rectangle;
        #endregion

        /// <summary>
        /// Draw a rounded rectangle on the provided canvas. 
        /// This method is typically used after applying a transform to the original rectangle.
        /// </summary>
        /// <param name="_canvas">The graphics object on which to draw</param>
        /// <param name="_rect">The rectangle specifications</param>
        /// <param name="_brush">Brush to draw with</param>
        /// <param name="_radius">Radius of the rounded corners</param>
        public static void Draw(Graphics _canvas, RectangleF _rect, SolidBrush _brush, int _radius, bool _dropShape)
        {
            int radius = _radius;
            int diameter = _radius * 2;

            GraphicsPath gp = new GraphicsPath();
            gp.StartFigure();

            if (_dropShape)
            {
                gp.AddLine(_rect.X, _rect.Y, _rect.X + _rect.Width - diameter, _rect.Y);
            }
            else
            {
                gp.AddArc(_rect.X, _rect.Y, diameter, diameter, 180, 90);
                gp.AddLine(_rect.X + radius, _rect.Y, _rect.X + _rect.Width - diameter, _rect.Y);
            }
            
            gp.AddArc(_rect.X + _rect.Width - diameter, _rect.Y, diameter, diameter, 270, 90);
            gp.AddLine(_rect.X + _rect.Width, _rect.Y + radius, _rect.X + _rect.Width, _rect.Y + _rect.Height);

            if (_dropShape)
            {
                gp.AddLine(_rect.X + _rect.Width, _rect.Y + _rect.Height, _rect.X + radius, _rect.Y + _rect.Height);
            }
            else
            {
                gp.AddArc(_rect.X + _rect.Width - diameter, _rect.Y + _rect.Height - diameter, diameter, diameter, 0, 90);
                gp.AddLine(_rect.X + _rect.Width - radius, _rect.Y + _rect.Height, _rect.X + radius, _rect.Y + _rect.Height);
            }
            
            gp.AddArc(_rect.X, _rect.Y + _rect.Height - diameter, diameter, diameter, 90, 90);
            gp.AddLine(_rect.X, _rect.Y + _rect.Height - radius, _rect.X, _rect.Y);

            gp.CloseFigure();
            _canvas.FillPath(_brush, gp);
        }
        public int HitTest(Point _point, bool _hiddenHandle)
        {
            int iHitResult = -1;
            if (_hiddenHandle)
            {
                Point botRight = new Point(m_Rectangle.Right, m_Rectangle.Bottom);
                if (botRight.Box(10).Contains(_point))
                    iHitResult = 1;
            }

            if (iHitResult < 0 && m_Rectangle.Contains(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public void Move(int _deltaX, int _deltaY)
        {
            m_Rectangle = new Rectangle(m_Rectangle.X + _deltaX, m_Rectangle.Y + _deltaY, m_Rectangle.Width, m_Rectangle.Height);
        }
        public void CenterOn(Point _point)
        {
            Point location = new Point(_point.X - m_Rectangle.Size.Width / 2, _point.Y - m_Rectangle.Size.Height / 2);
            m_Rectangle = new Rectangle(location, m_Rectangle.Size);
        }
    }
}
