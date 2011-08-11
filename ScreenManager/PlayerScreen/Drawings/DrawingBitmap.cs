#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingBitmap : AbstractDrawing
    {
        #region Properties
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.Opacity; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        #endregion

        #region Members
		
        // Bitmap
        private Bitmap m_Bitmap;
        
        private float m_fInitialScale = 1.0f;			// The scale we apply upon loading to make sure the image fits the screen.
        private Rectangle m_UnscaledRenderingWindow;	// The area of the original sized image that would be covered by the drawing in its current scale.
		private Rectangle m_RescaledRectangle;			// The area of the user sized image that will be covered by the drawing.
        
        private double m_fStretchFactor;				// The scaling of the image.
        private Point m_DirectZoomTopLeft;				// Shift of the image.
        
        private int m_iOriginalWidth;
        private int m_iOriginalHeight;
        
        // Decoration
        private InfosFading m_InfosFading;
        private ColorMatrix m_FadingColorMatrix = new ColorMatrix();
        private ImageAttributes m_FadingImgAttr = new ImageAttributes();
        private Pen m_PenBoundingBox;
        private SolidBrush m_BrushBoundingBox;
        
        // Instru
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingBitmap(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame, string _filename)
        {
        	m_Bitmap = new Bitmap(_filename);

            if(m_Bitmap != null)
            {
            	Initialize(_iWidth, _iHeight, _iTimestamp, _iAverageTimeStampsPerFrame);
            }
        }
        public DrawingBitmap(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame, Bitmap _bmp)
        {
        	m_Bitmap = AForge.Imaging.Image.Clone(_bmp);

            if(m_Bitmap != null)
            {
            	Initialize(_iWidth, _iHeight, _iTimestamp, _iAverageTimeStampsPerFrame);
            }
        }
        private void Initialize(int _iWidth, int _iHeight, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
			m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
        	m_iOriginalWidth = m_Bitmap.Width;
	        m_iOriginalHeight  = m_Bitmap.Height;
	        
	        // Set the initial scale so that the drawing is some part of the image height, to make sure it fits well.
	        // For bitmap drawing, we only do this if no upsizing is involved.
	        m_fInitialScale = (float) (((float)_iHeight * 0.75) / m_iOriginalHeight);
	        if(m_fInitialScale < 1.0)
	        {
	        	m_iOriginalWidth = (int) ((float)m_iOriginalWidth * m_fInitialScale);
	        	m_iOriginalHeight = (int) ((float)m_iOriginalHeight * m_fInitialScale);
	        }
	        
	        m_UnscaledRenderingWindow = new Rectangle((_iWidth - m_iOriginalWidth)/2, (_iHeight - m_iOriginalHeight)/2, m_iOriginalWidth, m_iOriginalHeight);
			
			// Everything start unscaled.
			RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        
	        
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_InfosFading.UseDefault = false;
            m_InfosFading.AlwaysVisible = true;            
            
            // This is used to set the opacity factor.
            m_FadingColorMatrix.Matrix00 = 1.0f;
			m_FadingColorMatrix.Matrix11 = 1.0f;
			m_FadingColorMatrix.Matrix22 = 1.0f;
			m_FadingColorMatrix.Matrix33 = 1.0f;	// Change alpha value here for fading. (i.e: 0.5f).
			m_FadingColorMatrix.Matrix44 = 1.0f;
			m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			
			PreferencesManager pm = PreferencesManager.Instance();
			m_PenBoundingBox = new Pen(Color.White, 1);
		 	m_PenBoundingBox.DashStyle = DashStyle.Dash;
		 	m_BrushBoundingBox = new SolidBrush(m_PenBoundingBox.Color);        	
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
        	double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
        	if (fOpacityFactor > 0)
            {
            	if(m_fStretchFactor != _fStretchFactor || _DirectZoomTopLeft != m_DirectZoomTopLeft)
            	{
            		// Compute the new image coordinate system.
            		m_fStretchFactor = _fStretchFactor;
            		m_DirectZoomTopLeft = _DirectZoomTopLeft;
            		RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            	}
            	
            	if (m_Bitmap != null)
				{
            		// Prepare for opacity attribute.
            		m_FadingColorMatrix.Matrix33 = (float)fOpacityFactor;
            		m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            		
            		// Render drawing.
            		_canvas.DrawImage(m_Bitmap, m_RescaledRectangle, 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel, m_FadingImgAttr);
            		
            		// Render handling box.
            		if(_bSelected)
            		{         
            			_canvas.DrawRectangle(m_PenBoundingBox, m_RescaledRectangle); 
            			
            			_canvas.FillEllipse(m_BrushBoundingBox, m_RescaledRectangle.Left - 4, m_RescaledRectangle.Top - 4, 8, 8);
            			_canvas.FillEllipse(m_BrushBoundingBox, m_RescaledRectangle.Left - 4, m_RescaledRectangle.Bottom - 4, 8, 8);
            			_canvas.FillEllipse(m_BrushBoundingBox, m_RescaledRectangle.Right - 4, m_RescaledRectangle.Top - 4, 8, 8);
            			_canvas.FillEllipse(m_BrushBoundingBox, m_RescaledRectangle.Right - 4, m_RescaledRectangle.Bottom - 4, 8, 8);
            		}
				}
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates descaled.
            // Hit Result: -1: miss, 0: on object.
              
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	// On handles ?
            	for (int i = 0; i < 4; i++)
	            {
	                if (GetHandleRectangle(i+1).Contains(_point))
	                {
	                    iHitResult = i+1;
	                }
	            }
            	
            	// On main drawing ?
            	if(iHitResult == -1 && IsPointOnDrawing(_point.X, _point.Y))
            	{
                    iHitResult = 0;
                }
            }
            
            return iHitResult;
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // _point is new coordinates of the handle, already descaled.
            
            // Keep aspect ratio.
            
            // None of the computations below should involve the image stretch factor.
            // We just compute the drawing bounding box as if it drew on the unscaled image.
            // Code copy pasted from SVGDrawing (mutualize ?).
            
            switch (handleNumber)
            {
                case 1:
            	{
            		// Top left handler.
            	 	// Compute the new rendering window as if in original image coordinate system.
            		int dx = point.X - m_UnscaledRenderingWindow.Left;
            		int newWidth = m_UnscaledRenderingWindow.Width - dx;
	            	
            		if(newWidth > 50)
            		{
	            		double qRatio = (double)newWidth / (double)m_iOriginalWidth;
	            		int newHeight = (int)((double)m_iOriginalHeight * qRatio); 	// Only if square.
	            		
	            		int newY = m_UnscaledRenderingWindow.Top + m_UnscaledRenderingWindow.Height - newHeight;
	            		
	            		m_UnscaledRenderingWindow = new Rectangle(point.X, newY, newWidth, newHeight);
            		}
            		break;
            	}
                case 2:
            	{
        			
        			// Top right handler.
            	 	int dx = m_UnscaledRenderingWindow.Right - point.X;
            		int newWidth = m_UnscaledRenderingWindow.Width - dx;
	            	
            		if(newWidth > 50)
            		{
	            		double qRatio = (double)newWidth / (double)m_iOriginalWidth;
	            		int newHeight = (int)((double)m_iOriginalHeight * qRatio); 	// Only if square.
	            		
	            		int newY = m_UnscaledRenderingWindow.Top + m_UnscaledRenderingWindow.Height - newHeight;
	            		int newX = point.X - newWidth;
	            		
	            		m_UnscaledRenderingWindow = new Rectangle(newX, newY, newWidth, newHeight);
            		}
            		break;
            	}
                case 3:
            	{
            		// Bottom right handler.
            	 	int dx = m_UnscaledRenderingWindow.Right - point.X;
            		int newWidth = m_UnscaledRenderingWindow.Width - dx;
	            	
            		if(newWidth > 50)
            		{
	            		double qRatio = (double)newWidth / (double)m_iOriginalWidth;
	            		int newHeight = (int)((double)m_iOriginalHeight * qRatio); 	// Only if square.
	            		
	            		int newY = m_UnscaledRenderingWindow.Y;
	            		int newX = point.X - newWidth;
	            		
	            		m_UnscaledRenderingWindow = new Rectangle(newX, newY, newWidth, newHeight);
            		}
            		break;
            	}
                case 4:
            	{
            		// Bottom left handler.
            	 	int dx = point.X - m_UnscaledRenderingWindow.Left;
            		int newWidth = m_UnscaledRenderingWindow.Width - dx;
	            	
            		if(newWidth > 50)
            		{
	            		double qRatio = (double)newWidth / (double)m_iOriginalWidth;
	            		int newHeight = (int)((double)m_iOriginalHeight * qRatio); 	// Only if square.
	            		
	            		int newY = m_UnscaledRenderingWindow.Y;
	            		
	            		m_UnscaledRenderingWindow = new Rectangle(point.X, newY, newWidth, newHeight);
		        	}
            		break;
            	}
                default:
                    break;
            }
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            // Move the rendering window around, does not change the scale of the drawing.
            m_UnscaledRenderingWindow = new Rectangle(m_UnscaledRenderingWindow.X + _deltaX,
                                                	  m_UnscaledRenderingWindow.Y + _deltaY, 
                                                	  m_UnscaledRenderingWindow.Width, 
                                                	  m_UnscaledRenderingWindow.Height);
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return "Bitmap Drawing";
        }
        public override int GetHashCode()
        {
            // Should not trigger meta data changes.
            return 0;
        }

        #region Lower level helpers
        private Rectangle GetHandleRectangle(int _handle)
        {
            //----------------------------------------------------------------------------
            // This function is only used for Hit Testing.
            // The Rectangle here is bigger than the bounding box of the handlers circles.
            // handles are clockwise 1 to 4.
            //----------------------------------------------------------------------------
            int widen = 6;
            if (_handle == 1)
            {
                return new Rectangle(m_UnscaledRenderingWindow.Left - widen, m_UnscaledRenderingWindow.Top - widen, widen * 2, widen * 2);
            }
            else if(_handle == 2)
            {
                return new Rectangle(m_UnscaledRenderingWindow.Right - widen, m_UnscaledRenderingWindow.Top - widen, widen * 2, widen * 2);
            }
            else if(_handle == 3)
            {
            	return new Rectangle(m_UnscaledRenderingWindow.Right - widen, m_UnscaledRenderingWindow.Bottom - widen, widen * 2, widen * 2);
            }
            else
            {
            	return new Rectangle(m_UnscaledRenderingWindow.Left - widen, m_UnscaledRenderingWindow.Bottom - widen, widen * 2, widen * 2);
            }
        }
        private bool IsPointOnDrawing(int x, int y)
        {
            return m_UnscaledRenderingWindow.Contains(x, y);
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	// Computes the rendering window in the user image coordinate system.
        	
        	// Stretch factor is the difference between the original image size and the current image size.
        	// It doesn't know anything about the bitmap custom scaling done by user.

        	int iLeft = (int)((m_UnscaledRenderingWindow.X - _DirectZoomTopLeft.X) * _fStretchFactor);
        	int iTop = (int)((m_UnscaledRenderingWindow.Y - _DirectZoomTopLeft.Y) * _fStretchFactor);
            
        	int iHeight = (int)((double)m_UnscaledRenderingWindow.Height * _fStretchFactor);
        	int iWidth = (int)((double)m_UnscaledRenderingWindow.Width * _fStretchFactor);
        	
        	m_RescaledRectangle = new Rectangle(iLeft, iTop, iWidth, iHeight);
        }
        #endregion
    }
}