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
        private Point m_TopLeftPoint; 
        private Rectangle m_RescaledRectangle;
		private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        private int m_iWidth;
        private int m_iHeight;
        
        // Decoration
        private InfosFading m_InfosFading;
        private ColorMatrix m_FadingColorMatrix = new ColorMatrix();
        private ImageAttributes m_FadingImgAttr = new ImageAttributes();
        
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
        	
			// The rendering will only be done on svgWindow.innerWidth, innerHeight.
			m_iWidth = 532;
			m_iHeight = 532;
			RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

			m_SvgWindow = new SvgWindow(m_RescaledRectangle.Width, m_RescaledRectangle.Height, m_Renderer);
			
			// Load the file and make the initial render.
			string folder = @"..\..\..\Tools\svg\";
         	string filename = folder + @"protractor.svg";
	        m_SvgWindow.Src = filename;
	        m_svgRendered = m_Renderer.Render(m_SvgWindow.Document as SvgDocument);
	        m_svgHitMap = m_Renderer.IdMapRaster;
	        m_bLoaded = true;
	        
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            m_FadingColorMatrix.Matrix00 = 1.0f;
			m_FadingColorMatrix.Matrix11 = 1.0f;
			m_FadingColorMatrix.Matrix22 = 1.0f;
			m_FadingColorMatrix.Matrix33 = 1.0f;	// alpha value.
			m_FadingColorMatrix.Matrix44 = 1.0f;
			m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
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
            		// We do not modify the internal DOM for the transformation.
            		// It would need a full render each time, and it doesn't seem to support panning anyway.
            		// Unfortunately, this means we don't leverage the "scalability" of SVG :-(
            		m_fStretchFactor = _fStretchFactor;
            		m_DirectZoomTopLeft = _DirectZoomTopLeft;
            		RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            	}
                
            	if (m_svgRendered != null )
				{
            		m_FadingColorMatrix.Matrix33 = (float)fOpacityFactor;
            		m_FadingImgAttr.SetColorMatrix(m_FadingColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            		
            		_canvas.DrawImage(m_svgRendered, m_RescaledRectangle, 0, 0, m_svgRendered.Width, m_svgRendered.Height, GraphicsUnit.Pixel, m_FadingImgAttr);
				}
            }
        }
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            // TODO.
        }
        public override void MoveDrawing(int _deltaX, int _deltaY)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_TopLeftPoint.X += _deltaX;
            m_TopLeftPoint.Y += _deltaY;

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object.
            
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	if(IsPointOnDrawing(_point.X, _point.Y))
            	{
                    iHitResult = 0;
                }
            }
            
            return iHitResult;
        }
        public override void ToXmlString(XmlTextWriter _xmlWriter)
        {
        	// Not implemented.
        	
        	
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
        	
        	// Not implemented.
        	
        	
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
            int iHash = m_TopLeftPoint.GetHashCode();
            return iHash;
        }
        
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

        #region Lower level helpers
        private bool IsPointOnDrawing(int x, int y)
        {
        	bool hit = false;
        	
        	int shiftedX = x - m_TopLeftPoint.X;
        	int shiftedY = y - m_TopLeftPoint.Y;
        	if(shiftedX >= 0 && shiftedY >= 0 && shiftedX < m_Renderer.IdMapRaster.Width && shiftedY < m_Renderer.IdMapRaster.Height)
        	{
        		hit = m_Renderer.HitTest(shiftedX, shiftedY);
        	}
        	
        	return hit;
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	int iLeft = (int)((m_TopLeftPoint.X - _DirectZoomTopLeft.X) * _fStretchFactor);
            int iTop = (int)((m_TopLeftPoint.Y - _DirectZoomTopLeft.Y) * _fStretchFactor);
            int iHeight = (int)((double)m_iHeight * _fStretchFactor);
        	int iWidth = (int)((double)m_iWidth * _fStretchFactor);
        	
        	m_RescaledRectangle = new Rectangle(iLeft, iTop, iWidth, iHeight);
        }
        #endregion
    }
}

