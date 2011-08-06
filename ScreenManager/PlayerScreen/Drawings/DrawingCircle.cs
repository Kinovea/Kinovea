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
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Circle")]
    public class DrawingCircle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
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
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        private bool m_bSelected;

        // Computed
        private int m_iDiameter;
        private Point m_RescaledOrigin;
        private int m_iRescaledRadius;
        private int m_iRescaledDiameter;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingCircle(int Ox, int Oy, int radius, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            // Core
            m_Origin = new Point(Ox, Oy);
            m_iRadius = radius;
            if(m_iRadius < 10) m_iRadius = 10;
            m_iDiameter = m_iRadius * 2;
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
			m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
			m_StyleHelper.Color = Color.Empty;
			m_StyleHelper.LineSize = 1;
			if(_preset != null)
			{
                m_Style = _preset.Clone();
                BindStyle();
			}
            
            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public DrawingCircle(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(0,0,0,0,0, ToolManager.Circle.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
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
        
        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Origin":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_Origin = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				        break;
					case "Radius":
				        int radius = _xmlReader.ReadElementContentAsInt();
                        m_iRadius = (int)((double)radius * _scale.X);
                        break;
					case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "InfosFading":
						m_InfosFading.ReadXml(_xmlReader);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
			m_iDiameter = m_iRadius * 2;
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            _xmlWriter.WriteElementString("Origin", String.Format("{0};{1}", m_Origin.X, m_Origin.Y));
            _xmlWriter.WriteElementString("Radius", m_iRadius.ToString());
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
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
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
			m_Style.Bind(m_StyleHelper, "LineSize", "pen size");
        }
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