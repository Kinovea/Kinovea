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
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        #endregion

        #region Members
        // Core
        private Point m_Center;
        private int m_iRadius;
        private bool m_bSelected;
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingCircle(Point _center, int radius, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_Center = _center;
            m_iRadius = Math.Min(radius, 10);
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
			m_StyleHelper.Color = Color.Empty;
			m_StyleHelper.LineSize = 1;
			if(_preset != null)
			{
                m_Style = _preset.Clone();
                BindStyle();
			}
        }
        public DrawingCircle(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty,0,0,0, ToolManager.Circle.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if(fOpacityFactor <= 0)
                return;
            
            int alpha = (int)(fOpacityFactor * 255);
            m_bSelected = _bSelected;
			
			using(Pen p = m_StyleHelper.GetPen(alpha, _transformer.Scale))
            {
                Rectangle boundingBox = _transformer.Transform(m_Center.Box(m_iRadius));
                _canvas.DrawEllipse(p, boundingBox);
                
                if(_bSelected)
                {
                	// Handler: arc in lower right quadrant.
                	p.Color = p.Color.Invert();
    				_canvas.DrawArc(p, boundingBox, 25, 40);
                }
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // User is dragging the outline of the circle, figure out the new radius at this point.
            int shiftX = Math.Abs(point.X - m_Center.X);
            int shiftY = Math.Abs(point.Y - m_Center.Y);
            m_iRadius = (int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY));
            if(m_iRadius < 10) 
                m_iRadius = 10;
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_Center.X += _deltaX;
            m_Center.Y += _deltaY;
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (m_bSelected && IsPointOnHandler(_point))
                    iHitResult = 1;
                else if (IsPointInObject(_point))
                    iHitResult = 0;
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
                        m_Center = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
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
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            _xmlWriter.WriteElementString("Origin", String.Format("{0};{1}", m_Center.X, m_Center.Y));
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
			MoveHandle(point, 1);
		}
        #endregion
        
        public override string ToString()
        {
            return ScreenManagerLang.ToolTip_DrawingToolCircle;
        }
        public override int GetHashCode()
        {
            int iHash = m_Center.GetHashCode();
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
        private bool IsPointInObject(Point _point)
        {
            bool bIsPointInObject = false;
			GraphicsPath areaPath = new GraphicsPath();
			areaPath.AddEllipse(m_Center.Box(m_iRadius + 10));
            bIsPointInObject = new Region(areaPath).IsVisible(_point);
            return bIsPointInObject;
        }
        private bool IsPointOnHandler(Point _point)
        {
        	bool bIsPointOnHandler = false;
            if(m_iRadius > 0)
            {
                GraphicsPath areaPath = new GraphicsPath();			
                areaPath.AddArc(m_Center.Box(m_iRadius + 5), 25, 40);
    			
    			Pen areaPen = new Pen(Color.Black, m_StyleHelper.LineSize + 10);
    			areaPath.Widen(areaPen);
    			areaPen.Dispose();
                bIsPointOnHandler = new Region(areaPath).IsVisible(_point);
            }
			
            return bIsPointOnHandler;	
        }
        #endregion
    }

       
}