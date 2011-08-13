using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A helper class for drawings using a bounding box.
    /// The drawing should defer part of its hit testing, move handle and move drawing methods to it.
    /// By convention, the handles id of the bounding box will always be 1 to 4.
    /// When the drawing has other handles, they should start at id 5.
    /// </summary>
    public class BoundingBox
    {
        #region Properties
        public Rectangle Rectangle
        {
            get { return m_Rectangle; }
            set { m_Rectangle = value; }
        }
        #endregion

        #region Members
        private Rectangle m_Rectangle;
        #endregion

        public void Draw(Graphics _canvas, Rectangle _rect, Pen _pen, SolidBrush _brush, int _widen)
        {
            _canvas.DrawRectangle(_pen, _rect);
            _canvas.FillEllipse(_brush, _rect.Left - _widen, _rect.Top - _widen, _widen * 2, _widen * 2);
            _canvas.FillEllipse(_brush, _rect.Left - _widen, _rect.Bottom - _widen, _widen * 2, _widen * 2);
            _canvas.FillEllipse(_brush, _rect.Right - _widen, _rect.Top - _widen, _widen * 2, _widen * 2);
            _canvas.FillEllipse(_brush, _rect.Right - _widen, _rect.Bottom - _widen, _widen * 2, _widen * 2);
        }
        public int HitTest(Point _point)
        {
            int iHitResult = -1;

            Point topLeft = m_Rectangle.Location;
            Point topRight = new Point(m_Rectangle.Right, m_Rectangle.Top);
            Point botRight = new Point(m_Rectangle.Right, m_Rectangle.Bottom);
            Point botLeft = new Point(m_Rectangle.Left, m_Rectangle.Bottom);

            int widen = 6;
            if (topLeft.Box(widen).Contains(_point))
                iHitResult = 1;
            else if (topRight.Box(widen).Contains(_point))
                iHitResult = 2;
            else if (botRight.Box(widen).Contains(_point))
                iHitResult = 3;
            else if (botLeft.Box(widen).Contains(_point))
                iHitResult = 4;
            else if (m_Rectangle.Contains(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public void MoveHandle(Point point, int handleNumber, Size _originalSize)
        {
            // Force aspect ratio for now.
            switch (handleNumber)
            {
                case 1:
                    {
                        // Top left handler.
                        int dx = point.X - m_Rectangle.Left;
                        int newWidth = m_Rectangle.Width - dx;

                        if (newWidth > 50)
                        {
                            double qRatio = (double)newWidth / (double)_originalSize.Width;
                            int newHeight = (int)((double)_originalSize.Height * qRatio); 	// Only if square.

                            int newY = m_Rectangle.Top + m_Rectangle.Height - newHeight;

                            m_Rectangle = new Rectangle(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 2:
                    {

                        // Top right handler.
                        int dx = m_Rectangle.Right - point.X;
                        int newWidth = m_Rectangle.Width - dx;

                        if (newWidth > 50)
                        {
                            double qRatio = (double)newWidth / (double)_originalSize.Width;
                            int newHeight = (int)((double)_originalSize.Height * qRatio); 	// Only if square.

                            int newY = m_Rectangle.Top + m_Rectangle.Height - newHeight;
                            int newX = point.X - newWidth;

                            m_Rectangle = new Rectangle(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 3:
                    {
                        // Bottom right handler.
                        int dx = m_Rectangle.Right - point.X;
                        int newWidth = m_Rectangle.Width - dx;

                        if (newWidth > 50)
                        {
                            double qRatio = (double)newWidth / (double)_originalSize.Width;
                            int newHeight = (int)((double)_originalSize.Height * qRatio); 	// Only if square.

                            int newY = m_Rectangle.Y;
                            int newX = point.X - newWidth;

                            m_Rectangle = new Rectangle(newX, newY, newWidth, newHeight);
                        }
                        break;
                    }
                case 4:
                    {
                        // Bottom left handler.
                        int dx = point.X - m_Rectangle.Left;
                        int newWidth = m_Rectangle.Width - dx;

                        if (newWidth > 50)
                        {
                            double qRatio = (double)newWidth / (double)_originalSize.Width;
                            int newHeight = (int)((double)_originalSize.Height * qRatio); 	// Only if square.

                            int newY = m_Rectangle.Y;

                            m_Rectangle = new Rectangle(point.X, newY, newWidth, newHeight);
                        }
                        break;
                    }
                default:
                    break;
            }
        }
        public void Move(int _deltaX, int _deltaY)
        {
            m_Rectangle = new Rectangle(m_Rectangle.X + _deltaX, m_Rectangle.Y + _deltaY, m_Rectangle.Width, m_Rectangle.Height);
        }
    }
}
