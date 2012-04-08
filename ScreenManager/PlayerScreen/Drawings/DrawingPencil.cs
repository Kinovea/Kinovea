#region License
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
using System.Linq;

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
        private List<Point> m_PointList = new List<Point>();
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPencil(Point _origin, Point _second, long _iTimestamp, long _AverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_PointList.Add(_origin);
            m_PointList.Add(_second);
            m_InfosFading = new InfosFading(_iTimestamp, _AverageTimeStampsPerFrame);
            
            m_StyleHelper.Color = Color.Black;
            m_StyleHelper.LineSize = 1;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
        }
        public DrawingPencil(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, Point.Empty, 0, 0, ToolManager.Pencil.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if(fOpacityFactor <= 0)
                return;
            
            using(Pen penLine = m_StyleHelper.GetPen(fOpacityFactor, _transformer.Scale))
            {
                Point[] points = m_PointList.Select(p => _transformer.Transform(p)).ToArray();
                _canvas.DrawCurve(penLine, points, 0.5f);
            }
        }
        public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
        {
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_PointList = m_PointList.Select(p => p.Translate(_deltaX, _deltaY)).ToList();
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0 && IsPointInObject(_point))
                iHitResult = 0;
                
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
        }
        private void ParsePointList(XmlReader _xmlReader, PointF _scale)
        {
            m_PointList.Clear();
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                if(_xmlReader.Name == "Point")
				{
                    Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                    Point adapted = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                    m_PointList.Add(adapted);
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
                _xmlWriter.WriteElementString("Point", String.Format("{0};{1}", p.X, p.Y));

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
            return ScreenManagerLang.ToolTip_DrawingToolPencil;
        }
        public override int GetHashCode()
        {
            int iHashCode = 0;
            foreach (Point p in m_PointList)
                iHashCode ^= p.GetHashCode();
            
            iHashCode ^= m_StyleHelper.GetHashCode();

            return iHashCode;
        }

        #region IInitializable implementation
        public void ContinueSetup(Point point, Keys modifiers)
		{
			AddPoint(point, modifiers);
		}
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "pen size");
        }
        private void AddPoint(Point _coordinates, Keys modifiers)
        {
            Point newPoint = Point.Empty;
            int pointsUsedToComputeDirection = Math.Min(10, m_PointList.Count);
            
            if((modifiers & Keys.Shift) == Keys.Shift)
            {
                // Checks whether the mouse motion is more horizontal or vertical, and only keep this component of the motion.
                int dx = Math.Abs(_coordinates.X - m_PointList[m_PointList.Count - pointsUsedToComputeDirection].X);
                int dy = Math.Abs(_coordinates.Y - m_PointList[m_PointList.Count - pointsUsedToComputeDirection].Y);
                if(dx > dy)
                    newPoint = new Point(_coordinates.X, m_PointList[m_PointList.Count - 1].Y);
                else
                    newPoint = new Point(m_PointList[m_PointList.Count - 1].X, _coordinates.Y);
            }
            else
            {
                newPoint = _coordinates;
            }
            
            m_PointList.Add(newPoint);
        }
        private bool IsPointInObject(Point _point)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddCurve(m_PointList.ToArray(), 0.5f);
            
                RectangleF bounds = areaPath.GetBounds();
                if(!bounds.IsEmpty)
                {
                    using(Pen areaPen = new Pen(Color.Black, m_StyleHelper.LineSize + 7))
                    {
                        areaPen.StartCap = LineCap.Round;
                        areaPen.EndCap = LineCap.Round;
                        areaPen.LineJoin = LineJoin.Round;
                        areaPath.Widen(areaPen);
                    }
                    using(Region areaRegion = new Region(areaPath))
                    {
                        hit = areaRegion.IsVisible(_point);
                    }
                }
                else
                {
                    hit = false;
                }
            }
            return hit;
        }
        #endregion
    }
}