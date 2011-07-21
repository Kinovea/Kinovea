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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class DrawingCircle : AbstractDrawing, IXMLSerializable, IDecorable, IInitializable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading infosFading
        {
            get{ return m_InfosFading;}
            set{ m_InfosFading = value;}
        }
        public override Capabilities Caps
		{
			get { return Capabilities.ConfigureColorSize | Capabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        #endregion

        #region Members
        // Core
        private Point m_Origin;
        private int m_iRadius;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style = new DrawingStyle();
        private InfosFading m_InfosFading;
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        private bool m_bSelected;

        // Computed
        private int m_iDiameter;
        private Point m_RescaledOrigin;
        private int m_iRescaledRadius;
        private int m_iRescaledDiameter;
        #endregion

        #region Constructor
        public DrawingCircle(int Ox, int Oy, int radius, long _iTimestamp, long _iAverageTimeStampsPerFrame)
        {
            // Core
            m_Origin = new Point(Ox, Oy);
            m_iRadius = radius;
            if(m_iRadius < 10) m_iRadius = 10;
            m_iDiameter = m_iRadius * 2;
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
			m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
			m_StyleHelper.Color = Color.Black;
			m_StyleHelper.LineSize = 1;
			m_Style.Elements.Add("color", new StyleElementColor(m_StyleHelper.Color));
			m_Style.Elements.Add("pen size", new StyleElementPenSize(m_StyleHelper.LineSize));
			m_Style.Bind(m_StyleHelper, "Color", "color");
			m_Style.Bind(m_StyleHelper, "LineSize", "pen size");
			
            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            int iPenAlpha = (int)((double)255 * fOpacityFactor);
			m_bSelected = _bSelected;
			
            if (iPenAlpha > 0)
            {
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                Pen penLine = m_StyleHelper.GetPen(iPenAlpha, m_fStretchFactor);
                _canvas.DrawEllipse(penLine, m_RescaledOrigin.X - m_iRescaledRadius, m_RescaledOrigin.Y - m_iRescaledRadius, m_iRescaledDiameter, m_iRescaledDiameter);
                
                if(_bSelected)
                {
                	// Draw a small arc in complementary color in the lower right part to show resizer.
                	penLine.Color = Color.FromArgb(iPenAlpha, 255 - m_StyleHelper.Color.R, 255 - m_StyleHelper.Color.G, 255 - m_StyleHelper.Color.B);
					_canvas.DrawArc(penLine, m_RescaledOrigin.X - m_iRescaledRadius, m_RescaledOrigin.Y - m_iRescaledRadius, m_iRescaledDiameter, m_iRescaledDiameter, 25, 40);
                }
                
                penLine.Dispose();
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // _point is mouse coordinates already descaled.
            // User is dragging the outline of the circle, figure out the new radius at this point.
            int shiftX = Math.Abs(point.X - m_Origin.X);
            int shiftY = Math.Abs(point.Y - m_Origin.Y);
            m_iRadius = (int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY));
            if(m_iRadius < 10) m_iRadius = 10;
            m_iDiameter = m_iRadius * 2;
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_Origin.X += _deltaX;
            m_Origin.Y += _deltaY;

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            //-----------------------------------------------------
            // This function is used by the PointerTool 
            // to know if we hit this particular drawing and where.
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object, 1+: on handle.
            //-----------------------------------------------------
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (m_bSelected && IsPointOnHandler(_point))
                {
                    iHitResult = 1;
                }
                else if (IsPointInObject(_point))
				{
                    iHitResult = 0;
                }
            }
            return iHitResult;
        }        
        #endregion
        
        #region IXMLSerializable implementation
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Drawing");
            _xmlWriter.WriteAttributeString("Type", "DrawingCircle");

            // Origin
            _xmlWriter.WriteStartElement("Origin");
            _xmlWriter.WriteString(m_Origin.X.ToString() + ";" + m_Origin.Y.ToString());
            _xmlWriter.WriteEndElement();

            // Radius
            _xmlWriter.WriteStartElement("Radius");
            _xmlWriter.WriteString(m_iRadius.ToString());
            _xmlWriter.WriteEndElement();

            //m_PenStyle.ToXml(_xmlWriter);
            m_InfosFading.ToXml(_xmlWriter, false);

            // </Drawing>
            _xmlWriter.WriteEndElement();
        }
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			MoveHandle(point, 2);
		}
        #endregion
        
        public override string ToString()
        {
            return ScreenManagerLang.ToolTip_DrawingToolCircle;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_Origin.GetHashCode();
            iHash ^= m_iRadius.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }
        public static AbstractDrawing FromXml(XmlTextReader _xmlReader, PointF _scale)
        {
        	// _scale.X and _scale.Y are used to map drawings that were set in one video,
        	// to a destination video with different dimensions.
        	// For the radius, we arbitrarily choose to scale on X.
            DrawingCircle dc = new DrawingCircle(0,0,0,0,0);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Origin")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');
                        dc.m_Origin = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    }
                    else if (_xmlReader.Name == "Radius")
                    {
                        int radius = int.Parse(_xmlReader.ReadString());
                        dc.m_iRadius = (int)((double)radius * _scale.X);
                    }
                    else if (_xmlReader.Name == "LineStyle")
                    {
                        //dc.m_PenStyle = LineStyle.FromXml(_xmlReader);   
                    }
                    else if (_xmlReader.Name == "InfosFading")
                    {
                        dc.m_InfosFading.FromXml(_xmlReader);
                    }
                    else
                    {
                        // forward compatibility: ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "Drawing")
                {
                    break;
                }
                else
                {
                    // closing internal tag.
                }
            }

            dc.m_iDiameter = dc.m_iRadius * 2;
            dc.RescaleCoordinates(dc.m_fStretchFactor, dc.m_DirectZoomTopLeft);
            return dc;
        }
        
        #region Lower level helpers
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	m_RescaledOrigin = new Point((int)((double)(m_Origin.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_Origin.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        	m_iRescaledRadius = (int)((double)m_iRadius * _fStretchFactor);
            m_iRescaledDiameter = m_iRescaledRadius * 2;
        }
        private bool IsPointInObject(Point _point)
        {
            // _point is already descaled.
            // Note: AddEllipse adds a filled ellipse. The points inside the ellipse are hits.

            bool bIsPointInObject = false;
			GraphicsPath areaPath = new GraphicsPath();
			int wideRadius = m_iRadius + 10;
			int wideDiameter = m_iDiameter + 20;
			areaPath.AddEllipse(m_Origin.X - wideRadius, m_Origin.Y - wideRadius, wideDiameter, wideDiameter);
			
			// Create region from the path
            Region areaRegion = new Region(areaPath);
            bIsPointInObject = new Region(areaPath).IsVisible(_point);

            return bIsPointInObject;
        }
        private bool IsPointOnHandler(Point _point)
        {
        	// _point is already descaled.
            bool bIsPointOnHandler = false;
			GraphicsPath areaPath = new GraphicsPath();			
			areaPath.AddArc(m_Origin.X - m_iRadius, m_Origin.Y - m_iRadius, m_iDiameter, m_iDiameter, 25, 40);
			
			Pen areaPen = new Pen(Color.Black, m_StyleHelper.LineSize + 10);
			areaPath.Widen(areaPen);
			areaPen.Dispose();
			
			// Create region from the path
            Region areaRegion = new Region(areaPath);
            bIsPointOnHandler = new Region(areaPath).IsVisible(_point);

            return bIsPointOnHandler;	
        }
        #endregion
    }

       
}