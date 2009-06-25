/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public enum MagnifierMode
    {
        NotVisible,               
        Direct,      
        Indirect    
    }
        
    public class Magnifier
    {
        private enum Hit
        {
            None,
            SourceWindow,
            MagnifyWindow,
            TopLeftResizer,
            TopRightResizer,
            BottomLeftResizer,
            BottomRightResizer
        }

        #region Properties
        public double ZoomFactor
        {
            get { return m_fZoomFactor; }
            set 
            {
                m_fZoomFactor = value;
                m_iMagWidth = (int)((double)m_iSrcCustomWidth * m_fZoomFactor);
                m_iMagHeight = (int)((double)m_iSrcCustomHeight * m_fZoomFactor);
            }
        }
        public int MouseX = 0;
        public int MouseY = 0;
        public MagnifierMode Mode = MagnifierMode.NotVisible;
        public Point MagnifiedCenter
        {
            get { return new Point(m_ImgTopLeft.X + m_iImgWidth / 2, m_ImgTopLeft.Y + m_iImgHeight / 2); }
        }
        #endregion

        #region Members
        private double m_fStretchFactor = double.MaxValue;

        private Size m_iImageSize;

        // TODO : turn everything into Rectangles.
        
        // Precomputed values (computed only when image stretch factor changes)
        private int m_iSrcWidth = 0;
        private int m_iSrcHeight = 0;

        private int m_iMagLeft = 10;
        private int m_iMagTop = 10;
        private int m_iMagWidth = 0;
        private int m_iMagHeight = 0;

        private Point m_ImgTopLeft;     // Location of source zone in source image system.
        private int m_iImgWidth = 0;    // size of the source zone in the original image
        private int m_iImgHeight = 0;

        // Default coeffs
        private static double m_fDefaultWindowFactor  = 0.20; // Size of the source zone relative to image size.
        private static double m_fZoomFactor = 1.75;

        // Indirect mode values
        private int m_iSrcCustomLeft = 0;
        private int m_iSrcCustomTop = 0;
        private int m_iSrcCustomWidth = 0;
        private int m_iSrcCustomHeight = 0;
        private Point m_LastPoint;
        private Hit m_MovingObject = Hit.None;
        #endregion

        #region Constructor  
        public Magnifier()
        {
            m_LastPoint.X = 0;
            m_LastPoint.Y = 0;

            m_iImageSize = new Size(1, 1);
            m_ImgTopLeft = new Point(0, 0);
        }
        #endregion

        #region Public Interface
        public void Draw(Bitmap _bitmap, Graphics _canvas, double _fStretchFactor, ColorProfile _colorProfile)
        {
            m_iImageSize = new Size(_bitmap.Width, _bitmap.Height);

            if (_fStretchFactor != m_fStretchFactor)
            {
                m_LastPoint = new Point((int)((double)m_LastPoint.X * _fStretchFactor), (int)((double)m_LastPoint.Y * _fStretchFactor));

                if (m_fStretchFactor != double.MaxValue)
                {
                    // Scale to new stretch factor.

                    double fRescaleFactor = _fStretchFactor / m_fStretchFactor;

                    m_iSrcCustomLeft = (int)((double)m_iSrcCustomLeft * fRescaleFactor);
                    m_iSrcCustomTop = (int)((double)m_iSrcCustomTop * fRescaleFactor);
                    m_iSrcCustomWidth = (int)((double)m_iSrcCustomWidth * fRescaleFactor);
                    m_iSrcCustomHeight = (int)((double)m_iSrcCustomHeight * fRescaleFactor);

                    m_iMagLeft = (int)((double)m_iMagLeft * fRescaleFactor);
                    m_iMagTop = (int)((double)m_iMagTop * fRescaleFactor);
                    m_iMagWidth = (int)((double)m_iMagWidth * fRescaleFactor);
                    m_iMagHeight = (int)((double)m_iMagHeight * fRescaleFactor);
                }
                else
                {
                    // Initializations.
                    
                    m_iSrcWidth = (int)((double)_bitmap.Width * _fStretchFactor * m_fDefaultWindowFactor);
                    m_iSrcHeight = (int)((double)_bitmap.Height * _fStretchFactor * m_fDefaultWindowFactor);
                    
                    m_iMagLeft = 10;
                    m_iMagTop = 10;
                    
                    m_iMagWidth = (int)((double)m_iSrcWidth * m_fZoomFactor);
                    m_iMagHeight = (int)((double)m_iSrcHeight * m_fZoomFactor);
                    
                    m_iImgWidth = (int)((double)m_iSrcWidth / _fStretchFactor);
                    m_iImgHeight = (int)((double)m_iSrcHeight / _fStretchFactor);
                }

                m_fStretchFactor = _fStretchFactor;
            }

            if (Mode == MagnifierMode.Direct)
            {
                int iImgLeft = (int)((double)MouseX / _fStretchFactor) - (m_iImgWidth / 2);
                int iImgTop = (int)((double)MouseY / _fStretchFactor) - (m_iImgHeight / 2);
                m_ImgTopLeft = new Point(iImgLeft, iImgTop);

                _canvas.DrawRectangle(Pens.White, MouseX - m_iSrcWidth / 2, MouseY - m_iSrcHeight/2, m_iSrcWidth, m_iSrcHeight);
                
                // Image Window
                _canvas.DrawImage(_bitmap, new Rectangle(m_iMagLeft, m_iMagTop, m_iMagWidth, m_iMagHeight), new Rectangle(iImgLeft, iImgTop, m_iImgWidth, m_iImgHeight), GraphicsUnit.Pixel);
                _canvas.DrawRectangle(Pens.White, m_iMagLeft, m_iMagTop, m_iMagWidth, m_iMagHeight);
            }
            else if (Mode == MagnifierMode.Indirect)
            {
                int iImgLeft = (int)((double)m_iSrcCustomLeft / _fStretchFactor);
                int iImgTop = (int)((double)m_iSrcCustomTop / _fStretchFactor);
                m_ImgTopLeft = new Point(iImgLeft, iImgTop);

                _canvas.DrawRectangle(Pens.LightGray, m_iSrcCustomLeft, m_iSrcCustomTop, m_iSrcCustomWidth, m_iSrcCustomHeight);

               // Handlers
                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft - 2, m_iSrcCustomTop - 2, m_iSrcCustomLeft + 2, m_iSrcCustomTop - 2);
                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft - 2, m_iSrcCustomTop - 2, m_iSrcCustomLeft - 2, m_iSrcCustomTop + 2);

                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft - 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2, m_iSrcCustomLeft + 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2);
                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft - 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2, m_iSrcCustomLeft - 2, m_iSrcCustomTop + m_iSrcCustomHeight - 2);

                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop - 2, m_iSrcCustomLeft + m_iSrcCustomWidth - 2, m_iSrcCustomTop - 2);
                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop - 2, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop + 2);

                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2, m_iSrcCustomLeft + m_iSrcCustomWidth - 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2);
                _canvas.DrawLine(Pens.LightGray, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop + m_iSrcCustomHeight + 2, m_iSrcCustomLeft + m_iSrcCustomWidth + 2, m_iSrcCustomTop + m_iSrcCustomHeight - 2);
                
    
                // Image Window
                _canvas.DrawImage(_bitmap, new Rectangle(m_iMagLeft, m_iMagTop, m_iMagWidth, m_iMagHeight), new Rectangle(iImgLeft, iImgTop, m_iImgWidth, m_iImgHeight), GraphicsUnit.Pixel);
                _canvas.DrawRectangle(Pens.White, m_iMagLeft, m_iMagTop, m_iMagWidth, m_iMagHeight);
            }
        }
        public void OnMouseUp(MouseEventArgs e)
        {
            if (Mode == MagnifierMode.Direct)
            {
                Mode = MagnifierMode.Indirect;

                // Fix current values.
                m_iSrcCustomLeft = MouseX - m_iSrcWidth/2;
                m_iSrcCustomTop = MouseY - m_iSrcHeight/2;
                m_iSrcCustomWidth = m_iSrcWidth;
                m_iSrcCustomHeight = m_iSrcHeight;
            }
        }
        public bool OnMouseMove(MouseEventArgs e)
        {
            if (Mode == MagnifierMode.Indirect)
            {
                int deltaX = e.X - m_LastPoint.X;
                int deltaY = e.Y - m_LastPoint.Y;

                m_LastPoint.X = e.X;
                m_LastPoint.Y = e.Y;

                switch (m_MovingObject)
                {
                    case Hit.SourceWindow:
                        if ((m_iSrcCustomLeft + deltaX > 0) && (m_iSrcCustomLeft + m_iSrcCustomWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor))
                        {
                            m_iSrcCustomLeft += deltaX;
                        }
                        if ((m_iSrcCustomTop + deltaY > 0) && (m_iSrcCustomTop + m_iSrcCustomHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor))
                        {
                            m_iSrcCustomTop += deltaY;
                        }
                        break;
                    case Hit.MagnifyWindow:
                        if ((m_iMagLeft + deltaX > 0) && (m_iMagLeft + m_iMagWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor))
                        {
                            m_iMagLeft += deltaX;
                        }
                        if ((m_iMagTop + deltaY > 0) && (m_iMagTop + m_iMagHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor))
                        {
                            m_iMagTop += deltaY;
                        }
                        break;
                    case Hit.TopLeftResizer:
                        if ((m_iSrcCustomLeft + deltaX > 0) && 
                            (m_iSrcCustomTop + deltaY > 0)  &&
                            (m_iSrcCustomLeft + m_iSrcCustomWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor) &&
                            (m_iSrcCustomTop + m_iSrcCustomHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor)    )
                        {
                            m_iSrcCustomLeft += deltaX;
                            m_iSrcCustomTop += deltaY;
                            m_iSrcCustomWidth -= deltaX;
                            m_iSrcCustomHeight -= deltaY;

                            if (m_iSrcCustomWidth < 10) m_iSrcCustomWidth = 10;
                            if (m_iSrcCustomHeight < 10) m_iSrcCustomHeight = 10;

                            m_iMagWidth = (int)((double)m_iSrcCustomWidth * m_fZoomFactor);
                            m_iMagHeight = (int)((double)m_iSrcCustomHeight * m_fZoomFactor);
                            m_iImgWidth = (int)((double)m_iSrcCustomWidth / m_fStretchFactor);
                            m_iImgHeight = (int)((double)m_iSrcCustomHeight / m_fStretchFactor);
                        }
                        break;
                    case Hit.BottomLeftResizer:
                        if ((m_iSrcCustomLeft + deltaX > 0) &&
                            (m_iSrcCustomTop + deltaY > 0) &&
                            (m_iSrcCustomLeft + m_iSrcCustomWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor) &&
                            (m_iSrcCustomTop + m_iSrcCustomHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor))
                        {
                            m_iSrcCustomLeft += deltaX;
                            m_iSrcCustomWidth -= deltaX;
                            m_iSrcCustomHeight += deltaY;

                            if (m_iSrcCustomWidth < 10) m_iSrcCustomWidth = 10;
                            if (m_iSrcCustomHeight < 10) m_iSrcCustomHeight = 10;

                            m_iMagWidth = (int)((double)m_iSrcCustomWidth * m_fZoomFactor);
                            m_iMagHeight = (int)((double)m_iSrcCustomHeight * m_fZoomFactor);
                            m_iImgWidth = (int)((double)m_iSrcCustomWidth / m_fStretchFactor);
                            m_iImgHeight = (int)((double)m_iSrcCustomHeight / m_fStretchFactor);
                        }
                        break;
                    case Hit.TopRightResizer:
                        if ((m_iSrcCustomLeft + deltaX > 0) &&
                            (m_iSrcCustomTop + deltaY > 0) &&
                            (m_iSrcCustomLeft + m_iSrcCustomWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor) &&
                            (m_iSrcCustomTop + m_iSrcCustomHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor))
                        {
                            m_iSrcCustomTop += deltaY;
                            m_iSrcCustomWidth += deltaX;
                            m_iSrcCustomHeight -= deltaY;

                            if (m_iSrcCustomWidth < 10) m_iSrcCustomWidth = 10;
                            if (m_iSrcCustomHeight < 10) m_iSrcCustomHeight = 10;

                            m_iMagWidth = (int)((double)m_iSrcCustomWidth * m_fZoomFactor);
                            m_iMagHeight = (int)((double)m_iSrcCustomHeight * m_fZoomFactor);
                            m_iImgWidth = (int)((double)m_iSrcCustomWidth / m_fStretchFactor);
                            m_iImgHeight = (int)((double)m_iSrcCustomHeight / m_fStretchFactor);
                        }
                        break;
                    case Hit.BottomRightResizer:
                        if ((m_iSrcCustomLeft + deltaX > 0) &&
                            (m_iSrcCustomTop + deltaY > 0) &&
                            (m_iSrcCustomLeft + m_iSrcCustomWidth + deltaX + 1 < m_iImageSize.Width * m_fStretchFactor) &&
                            (m_iSrcCustomTop + m_iSrcCustomHeight + deltaY + 1 < m_iImageSize.Height * m_fStretchFactor))
                        {
                            m_iSrcCustomWidth += deltaX;
                            m_iSrcCustomHeight += deltaY;

                            if (m_iSrcCustomWidth < 10) m_iSrcCustomWidth = 10;
                            if (m_iSrcCustomHeight < 10) m_iSrcCustomHeight = 10;

                            m_iMagWidth = (int)((double)m_iSrcCustomWidth * m_fZoomFactor);
                            m_iMagHeight = (int)((double)m_iSrcCustomHeight * m_fZoomFactor);
                            m_iImgWidth = (int)((double)m_iSrcCustomWidth / m_fStretchFactor);
                            m_iImgHeight = (int)((double)m_iSrcCustomHeight / m_fStretchFactor);
                        }
                        break;
                    default:
                        break;
                }

                

                
            }

            return (m_MovingObject != Hit.None);
        }
        public bool OnMouseDown(MouseEventArgs e)
        {
            //----------------------------------------------------------------
            // Return true if we actually hit any of the magnifier elements
            // (mag window, resizers, etc...)
            // In case of the first switch to indirect mode, will return true.
            //----------------------------------------------------------------
            if (Mode == MagnifierMode.Indirect)
            {
                // initialize position.
                m_LastPoint.X = e.X;
                m_LastPoint.Y = e.Y;

                // initialize what we are moving.
                m_MovingObject = HitTest(new Point(e.X, e.Y));
            }

            return (m_MovingObject != Hit.None);
        }
        public bool IsOnObject(MouseEventArgs e)
        {
            return (HitTest(new Point(e.X, e.Y)) != Hit.None);
        }
        #endregion

        private Hit HitTest(Point _point)
        {
            // Hit Result:
            // -1: miss, 0: on source window, 1: on magnification window, 1+: on source resizer.

            Hit res = Hit.None;

            Rectangle srcRectangle = new Rectangle(m_iSrcCustomLeft, m_iSrcCustomTop, m_iSrcCustomWidth, m_iSrcCustomHeight);
            Rectangle magRectangle = new Rectangle(m_iMagLeft, m_iMagTop, m_iMagWidth, m_iMagHeight);

            // We widen the size of handlers rectangle for easier selection.
            int widen = 6;

            if (new Rectangle(m_iSrcCustomLeft - widen, m_iSrcCustomTop - widen, widen * 2, widen * 2).Contains(_point))
            {
                res = Hit.TopLeftResizer;
            }
            else if (new Rectangle(m_iSrcCustomLeft - widen, m_iSrcCustomTop + m_iSrcCustomHeight - widen, widen * 2, widen * 2).Contains(_point))
            {
                res = Hit.BottomLeftResizer;
            }
            else if (new Rectangle(m_iSrcCustomLeft + m_iSrcCustomWidth - widen, m_iSrcCustomTop - widen, widen * 2, widen * 2).Contains(_point))
            {
                res = Hit.TopRightResizer;
            }
            else if (new Rectangle(m_iSrcCustomLeft + m_iSrcCustomWidth - widen, m_iSrcCustomTop + m_iSrcCustomHeight - widen, widen * 2, widen * 2).Contains(_point))
            {
                res = Hit.BottomRightResizer;
            }
            else if (srcRectangle.Contains(_point))
            {
                res = Hit.SourceWindow;
            }
            else if (magRectangle.Contains(_point))
            {
                res = Hit.MagnifyWindow;
            }
            

            return res;
        }
        
    }
}
