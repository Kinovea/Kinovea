#region License
/*
Copyright © Joan Charmant 2010.
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Xml;

using Kinovea.Services;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Renderer.Gdi;

namespace Kinovea.ScreenManager
{
    public class DrawingSVG : AbstractDrawing
    {
        #region Properties
        public override DrawingToolType ToolType
        {
        	get { return DrawingToolType.SVG; }
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
        }
        #endregion

        #region Members
		
        // SVG
        private GdiRenderer m_Renderer  = new GdiRenderer();
		private SvgWindow m_SvgWindow;
		private bool m_bLoaded;
		private Bitmap m_svgRendered;
		private Bitmap m_svgHitMap;
        
        // Position
        // The drawing scale is used to keep track of the user transform on the drawing, outside of the image transform context.
        // The unscale rendering window is used for hit testing.
        // Drawing original dimensions are used to compute the drawing scale.
        
        private float m_fDrawingScale = 1.0f;			// The current scale of the drawing if it were rendered on the original sized image.
        private Rectangle m_UnscaledRenderingWindow;	// The area of the original sized image that would be covered by the drawing in its current scale.
		private float m_fDrawingRenderingScale = 1.0f; // The scale of the drawing taking drawing transform AND image transform into account.
        private Rectangle m_RescaledRectangle;			// The area of the user sized image that will be covered by the drawing.
        
        private double m_fStretchFactor;				// The scaling of the image.
        private Point m_DirectZoomTopLeft;				// Shift of the image.
        
        private int m_iOriginalWidth;					// As in the SVG file.
        private int m_iOriginalHeight;
        private double m_fOriginalAspectRatio;
        
        private bool m_bFinishedResizing;
       
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
        public DrawingSVG(int x, int y, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
        	m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            //----------------------------
			// Init and import an SVG.
        	// Testing code. Should load a file according to a parameter.
			//----------------------------
			m_Renderer.BackColor = Color.Transparent;
			
			// Rendering window. The width and height will be updated later.
			m_SvgWindow = new SvgWindow(100, 100, m_Renderer);
			
			string folder = @"..\..\..\Tools\svg\";
         	string filename = folder + @"protractor256.svg";
	        //string filename = folder + @"shapes.svg";
         	m_SvgWindow.Src = filename;
	        m_bLoaded = true;
	        
	        m_iOriginalWidth = (int)m_SvgWindow.Document.RootElement.Width.BaseVal.Value;
	        m_iOriginalHeight  = (int)m_SvgWindow.Document.RootElement.Height.BaseVal.Value;
	        m_fOriginalAspectRatio = (double)m_iOriginalWidth / (double)m_iOriginalHeight;
			m_UnscaledRenderingWindow = new Rectangle(x - m_iOriginalWidth/2, y - m_iOriginalHeight/2, m_iOriginalWidth, m_iOriginalHeight);
			
			// Everything start unscaled.
			RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

	        // Render on first draw call.
	        m_bFinishedResizing = true;
	        
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_InfosFading.UseDefault = false;
            m_InfosFading.AlwaysVisible = true;            
            // This is kept in place but in the current implementation the drawing never fade. (motion guide).
            m_FadingColorMatrix.Matrix00 = 1.0f;
			m_FadingColorMatrix.Matrix11 = 1.0f;
			m_FadingColorMatrix.Matrix22 = 1.0f;
			m_FadingColorMatrix.Matrix33 = 1.0f;	// alpha value.
			m_FadingColorMatrix.Matrix44 = 1.0f;
			m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
			PreferencesManager pm = PreferencesManager.Instance();
			m_PenBoundingBox = new Pen(Color.FromArgb(255, pm.GridColor.R, pm.GridColor.G, pm.GridColor.B), 1);
		 	m_PenBoundingBox.DashStyle = DashStyle.Dash;
		 	m_BrushBoundingBox = new SolidBrush(m_PenBoundingBox.Color);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
        	double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
        	if (fOpacityFactor > 0 && m_bLoaded)
            {
            	if(m_fStretchFactor != _fStretchFactor || _DirectZoomTopLeft != m_DirectZoomTopLeft)
            	{
            		// Compute the new image coordinate system.
            		// We do not call the SVG rendering engine at this point, and 
            		// will use the .NET interpolation until the user is done resizing.
            		// Later on, ResizeFinished() should be called to trigger the full rendering.
            		m_fStretchFactor = _fStretchFactor;
            		m_DirectZoomTopLeft = _DirectZoomTopLeft;
            		RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            	}
            	
            	if(m_bFinishedResizing)
            	{
            		m_bFinishedResizing = false;
            		RenderAtNewScale();
            	}
                
            	if (m_svgRendered != null )
				{
            		m_FadingColorMatrix.Matrix33 = (float)fOpacityFactor;
            		m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            		
            		// Render drawing.
            		_canvas.DrawImage(m_svgRendered, m_RescaledRectangle, 0, 0, m_svgRendered.Width, m_svgRendered.Height, GraphicsUnit.Pixel, m_FadingImgAttr);
            		
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
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            // _point is new coordinates of the handle, already descaled.
            
            // Keep aspect ratio for now.
            
            // None of the computations below should involve the image stretch factor.
            // We just compute the drawing bounding box as if it drew on the unscaled image.
            
            // We do not call the SVG rendering engine at this point, and 
	        // will use the .NET interpolation until the user is done resizing.
	        // Later on, ResizeFinished() should be called to trigger the full rendering.
	            		
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
            
            // Update scaled coordinates accordingly (we must do this before the RenderAtNewScale happen).
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            
        }
        public override void MoveDrawing(int _deltaX, int _deltaY)
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
            
            log.Debug(String.Format("SVG Hit test input:{0}, unscaled rect:{1}. Result:{2}", 
                                    _point.ToString(), m_UnscaledRenderingWindow.ToString(), iHitResult.ToString()));
            
            return iHitResult;
        }
        public override void ToXmlString(XmlTextWriter _xmlWriter)
        {
        	// TODO: implement me.
        	
        	
        	/*
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingCross2D");

            // CenterPoint
            _xmlWriter.WriteStartElement("CenterPoint");
            _xmlWriter.WriteString(m_TopLeftPoint.X.ToString() + ";" + m_TopLeftPoint.Y.ToString());
            _xmlWriter.WriteEndElement();

            m_PenStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);

            // </Drawing>
            _xmlWriter.WriteEndElement();*/
        }
        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
        	
        	// TODO: implement me.
        	
        	
            DrawingSVG dsvg = new DrawingSVG(0,0,0,0);

            /*while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "CenterPoint")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        dc.m_CenterPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "LineStyle")
                    {
                        dc.m_PenStyle = LineStyle.FromXml(_xmlReader);   
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dc.m_InfosFading.FromXml(_xmlReader);
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "Drawing")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }

            dc.RescaleCoordinates(dc.m_fStretchFactor, dc.m_DirectZoomTopLeft);*/
            return dsvg;
        }
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return "SVG Drawing";
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_UnscaledRenderingWindow.GetHashCode();
            return iHash;
        }
        
        #region Not implemented
        public override void UpdateDecoration(Color _color)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));
        }
        public override void UpdateDecoration(LineStyle _style)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));
        }
        public override void UpdateDecoration(int _iFontSize)
        {
        	throw new Exception(String.Format("{0}, The method or operation is not implemented.", this.ToString()));
        }
        public override void MemorizeDecoration()
        {
        	// Not implemented.
        }
        public override void RecallDecoration()
        {
        	// Not implemented.
        }
        #endregion
        
        #endregion

        public void ResizeFinished()
        {
        	// While the user was resizing the drawing or the image, we didn't update / render the SVG image.
        	// Now that he is done, we can stop using the low quality interpolation and resort to SVG scalability.
        	
        	// However we do not know the final scale until we get back in Draw(),
        	// So we just switch a flag on and we'll call the rendering from there.
        	m_bFinishedResizing = true;
        }
        
        #region Lower level helpers
        private void RenderAtNewScale()
        {
        	// Depending on the complexity of the SVG, this can be a costly operation.
        	// We should only do that when mouse move is over,
        	// and use the interpolated version during the change.
        	
        	//m_SvgWindow.Document.RootElement.
        	
        	
        	// Compute the final drawing sizes,
        	// taking both the drawing transformation and the image scaling into account.
        	m_fDrawingScale = (float)m_UnscaledRenderingWindow.Width / (float)m_iOriginalWidth;
        	m_fDrawingRenderingScale = (float)(m_fStretchFactor * m_fDrawingScale);
        	
        	if(m_svgRendered == null || m_fDrawingRenderingScale != m_SvgWindow.Document.RootElement.CurrentScale)
        	{
	        	m_SvgWindow.Document.RootElement.CurrentScale = (float)m_fDrawingRenderingScale;
	        	m_SvgWindow.InnerWidth = m_RescaledRectangle.Width;
	        	m_SvgWindow.InnerHeight = m_RescaledRectangle.Height;
	        	
	            m_svgRendered = m_Renderer.Render(m_SvgWindow.Document as SvgDocument);
		        m_svgHitMap = m_Renderer.IdMapRaster;
		        
		        log.Debug(String.Format("Rendering SVG ({0};{1}), User transformation (scale) : {2:0.00}. Image transformation (scale) :{3:0.00}, Final transformation (scale) : {4:0.00}.", 
		                                m_iOriginalWidth, m_iOriginalHeight, m_fDrawingScale, m_fStretchFactor, m_fDrawingRenderingScale));
        	}
        }
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
        	// [x,y] is expressed in the original image coordinate system,
        	// But the drawing should have been rendered to its final scale by now.
        	// The final scale takes into account the user transformation and the image scaling.
        	
        	// We MUST stop the interpolation trick and render to the new dimensions before coming here.
        	
        	bool hit = false;
        	
        	// Rescale the mouse point.
           	int scaledX = (int)((x - m_DirectZoomTopLeft.X) * m_fStretchFactor);
            int scaledY = (int)((y - m_DirectZoomTopLeft.Y) * m_fStretchFactor);

        	// Unshift the mouse point. (because the drawing always draws itself on a [0, 0, width, height] image.
        	
        	int unshiftedX = scaledX - m_RescaledRectangle.X;
        	int unshiftedY = scaledY - m_RescaledRectangle.Y;
        	
        	if(unshiftedX >= 0 && unshiftedY >= 0 && unshiftedX < m_Renderer.IdMapRaster.Width && unshiftedY < m_Renderer.IdMapRaster.Height)
        	{
        		hit = m_Renderer.HitTest(unshiftedX, unshiftedY);
        	}
        	
        	return hit;
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	// Computes the rendering window in the user image coordinate system.
        	
        	// Stretch factor is the difference between the original image size and the current image size.
        	// It doesn't know anything about the SVG drawing internal scaling.

        	int iLeft = (int)((m_UnscaledRenderingWindow.X - _DirectZoomTopLeft.X) * _fStretchFactor);
        	int iTop = (int)((m_UnscaledRenderingWindow.Y - _DirectZoomTopLeft.Y) * _fStretchFactor);
            
        	int iHeight = (int)((double)m_UnscaledRenderingWindow.Height * _fStretchFactor);
        	int iWidth = (int)((double)m_UnscaledRenderingWindow.Width * _fStretchFactor);
        	
        	m_RescaledRectangle = new Rectangle(iLeft, iTop, iWidth, iHeight);
        }
        #endregion
    }
}

