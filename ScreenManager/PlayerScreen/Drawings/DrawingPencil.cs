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
    [XmlType ("Pencil")]
    public class DrawingPencil : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading infosFading
        {
            get { return m_InfosFading; }
            set { m_InfosFading = value; }
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
        
        // Core & decoration
        private List<Point> m_PointList;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private double m_fStretchFactor;
        private InfosFading m_InfosFading;
        private Point m_DirectZoomTopLeft;
        // Computed
        private List<Point> m_RescaledPointList;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPencil(int x1, int y1, int x2, int y2, long _iTimestamp, long _AverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_PointList = new List<Point>();
            m_PointList.Add(new Point(x1, y1));
            m_PointList.Add(new Point(x2, y2));

            m_InfosFading = new InfosFading(_iTimestamp, _AverageTimeStampsPerFrame);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            m_StyleHelper.Color = Color.Black;
            m_StyleHelper.LineSize = 1;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            // Computed
            m_RescaledPointList = new List<Point>();
            m_RescaledPointList.Add(RescalePoint(new Point(x1, y1), m_fStretchFactor));
            m_RescaledPointList.Add(RescalePoint(new Point(x2, y2), m_fStretchFactor));

            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public DrawingPencil(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(0, 0, 0, 0, 0, 0, ToolManager.Pencil.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            int iPenAlpha = (int)((double)255 * fOpacityFactor);

            if (iPenAlpha > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                Point[] points = new Point[m_RescaledPointList.Count];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new Point(m_RescaledPointList[i].X, m_RescaledPointList[i].Y);
                }
                
                Pen penLine = m_StyleHelper.GetPen(iPenAlpha, m_fStretchFactor);
                _canvas.DrawCurve(penLine, points, 0.5f);
                penLine.Dispose();
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            for(int i=0;i<m_PointList.Count;i++)
            {
                m_PointList[i] = new Point(m_PointList[i].X + _deltaX, m_PointList[i].Y + _deltaY);
            }

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object, 1+: on handle.
            
            int iHitResult = -1;

            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (IsPointInObject(_point))
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
					case "PointList":
				        ParsePointList(_xmlReader, _scale);
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
            
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        private void ParsePointList(XmlReader _xmlReader, PointF _scale)
        {
            m_PointList.Clear();
            m_RescaledPointList.Clear();
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                if(_xmlReader.Name == "Point")
				{
                    Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                    Point adapted = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    m_PointList.Add(adapted);
                    m_RescaledPointList.Add(adapted);
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
				    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteStartElement("PointList");
            _xmlWriter.WriteAttributeString("Count", m_PointList.Count.ToString());
            foreach (Point p in m_PointList)
            {
                _xmlWriter.WriteElementString("Point", String.Format("{0};{1}", p.X, p.Y));
            }
            _xmlWriter.WriteEndElement();
            
		    _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement(); 
		}
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return ScreenManagerLang.ToolTip_DrawingToolPencil;
        }
        public override int GetHashCode()
        {
            // combine all relevant fields with XOR to get the Hash.

            int iHashCode = 0;
            foreach (Point p in m_PointList)
            {
                iHashCode ^= p.GetHashCode();
            }

            iHashCode ^= m_StyleHelper.GetHashCode();

            return iHashCode;
        }

        #region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			AddPoint(point);
		}
        #endregion
        
        public void AddPoint(Point _coordinates)
        {
            m_PointList.Add(_coordinates);
            m_RescaledPointList.Add(RescalePoint(_coordinates, m_fStretchFactor));
        }

        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "pen size");
        }
        private Point RescalePoint(Point _point, double _fStretchFactor)
        {
            return new Point((int)((double)_point.X * _fStretchFactor), (int)((double)_point.Y * _fStretchFactor));
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            for(int i=0;i<m_PointList.Count;i++)
            {
                m_RescaledPointList[i] = new Point((int)((double)(m_PointList[i].X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_PointList[i].Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            }
        }
        private bool IsPointInObject(Point _point)
        {
            // _point is descaled.

            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            
            Point[] points = new Point[m_PointList.Count];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(m_PointList[i].X, m_PointList[i].Y);
            }
            areaPath.AddCurve(points, 0.5f);
            
            RectangleF bounds = areaPath.GetBounds();
            if(bounds.Width != 0 || bounds.Height != 0)
            {
                Pen areaPen = new Pen(Color.Black, m_StyleHelper.LineSize + 7);
                areaPen.StartCap = LineCap.Round;
                areaPen.EndCap = LineCap.Round;
                areaPen.LineJoin = LineJoin.Round;
                areaPath.Widen(areaPen);
    			areaPen.Dispose();
                Region areaRegion = new Region(areaPath);
                return areaRegion.IsVisible(_point);
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}