#region License
/*
Copyright © Joan Charmant 2012.
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
    [XmlType ("AlignmentAngle")]
    public class DrawingAlignmentAngle : AbstractDrawing, IKvaSerializable, IDecorable
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
			get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading; }
		}
		public override List<ToolStripMenuItem> ContextMenu
		{
		    get { return null;}
		}
        #endregion

        #region Members
        private Point m_PointA; // start
        private Point m_PointB; // angle origin.
        private Point m_PointC; // end
        private Point m_PointD; // angle end.
        private List<Point> m_Points = new List<Point>();
        
        private AngleHelper m_AngleHelper = new AngleHelper(true, 40);
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private InfosFading m_InfosFading;
        
        private const int m_iDefaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingAlignmentAngle(Point _origin, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            // Core
            m_PointA = _origin;
            InitDrawing();
            PushToList();
            ComputeValues();
            
            // Decoration and binding to mini editors.
            m_StyleHelper.Bicolor = new Bicolor(Color.Empty);
            m_StyleHelper.Font = new Font("Arial", 12, FontStyle.Bold);
            if(_stylePreset != null)
            {
                m_Style = _stylePreset.Clone();
                BindStyle();    
            }
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
        }
        public DrawingAlignmentAngle(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, 0, 0, ToolManager.Angle.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if (fOpacityFactor <= 0)
                return;
            
            Point pointA = _transformer.Transform(m_PointA);
            Point pointB = _transformer.Transform(m_PointB);
            Point pointC = _transformer.Transform(m_PointC);
            Point pointD = _transformer.Transform(m_PointD);
            Rectangle boundingBox = _transformer.Transform(m_AngleHelper.BoundingBox);
            
            using(Pen penEdges = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor*255)))
            using(Pen penDash = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor*255)))
            using(SolidBrush brushEdges = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*255)))
            using(SolidBrush brushFill = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*m_iDefaultBackgroundAlpha)))
            {
                //penEdges.Width = 2;
                penDash.Width = 2;
                penDash.DashStyle = DashStyle.Dash;
                
                // Edges
                _canvas.DrawLine(penEdges, pointA, pointB);
                _canvas.DrawLine(penDash, pointB, pointC);
                _canvas.DrawLine(penEdges, pointB, pointD);
                
                // Handlers
                _canvas.FillEllipse(brushEdges, pointA.Box(3));
                _canvas.FillEllipse(brushEdges, pointB.Box(3));
                _canvas.FillEllipse(brushEdges, pointC.Box(3));
                _canvas.FillEllipse(brushEdges, pointD.Box(3));
                
                // Disk section
                _canvas.FillPie(brushFill, boundingBox, (float)m_AngleHelper.Start, (float)m_AngleHelper.Sweep);
                _canvas.DrawArc(penEdges, boundingBox, (float)m_AngleHelper.Start, (float)m_AngleHelper.Sweep);
                
                DrawText(_canvas, fOpacityFactor, _transformer, pointB, brushFill);
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int iHitResult = -1;
            if (m_InfosFading.GetOpacityFactor(_iCurrentTimestamp) <= 0)
                return -1;

            for(int i = 0; i<m_Points.Count;i++)
            {
                if(m_Points[i].Box(10).Contains(_point))
                {
                    iHitResult = i+1;
                    break;
                }
            }
            
            if(iHitResult == -1 && IsPointInObject(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public override void MoveHandle(Point _point, int _iHandleNumber)
        {
            int index = _iHandleNumber - 1;
            
            if(index == 0)
            {
                m_PointA = _point;
                m_PointB = GeometryHelper.GetClosestPoint(m_PointA, m_PointC, m_PointB, true, 10);
            }
            else if(index == 1)
            {
                // Force B on the main line.
                m_PointB = GeometryHelper.GetClosestPoint(m_PointA, m_PointC, _point, true, 10);
                
                // Allow B to move freely, and force C to be aligned.
                /*m_PointB = _point;
                if(m_PointB == m_PointA)
                    m_PointB = new Point(m_PointA.X + 10, m_PointA.Y - 10);
                m_PointC = GeometryHelper.GetClosestPoint(m_PointA, m_PointB, m_PointC, false, 10);*/
            }
            else if(index == 2)
            {
                m_PointC = _point;
                if(m_PointC == m_PointA)
                    m_PointC = new Point(m_PointA.X + 20, m_PointA.Y - 20);
                m_PointB = GeometryHelper.GetClosestPoint(m_PointA, m_PointC, m_PointB, true, 10);
            }
            else if(index == 3)
            {
                m_PointD = _point;
            }
            
            if(m_PointD == m_PointB)
                m_PointD = new Point(m_PointB.X + 10, m_PointB.Y - 10);
            
            //PullFromList();
            PushToList();
            ComputeValues();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            for(int i = 0; i<m_Points.Count; i++)
                m_Points[i] = new Point(m_Points[i].X + _deltaX, m_Points[i].Y + _deltaY);

            PullFromList();
            ComputeValues();
        }
		#endregion
		
        public override string ToString()
        {
            return "Alignment Angle"; //ScreenManagerLang.ToolTip_DrawingToolAngle2D;
        }
        public override int GetHashCode()
        {
            /*int iHash = m_PointO.GetHashCode();
            iHash ^= m_PointA.GetHashCode();
            iHash ^= m_PointB.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;*/
            return 0;
        }        
            
        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            /*_xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "PointO":
				        m_PointO = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        break;
					case "PointA":
				        m_PointA = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        break;
					case "PointB":
				        m_PointB = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
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
            
            m_PointO = new Point((int)((float)m_PointO.X * _scale.X), (int)((float)m_PointO.Y * _scale.Y));
            m_PointA = new Point((int)((float)m_PointA.X * _scale.X), (int)((float)m_PointA.Y * _scale.Y));
            m_PointB = new Point((int)((float)m_PointB.X * _scale.X), (int)((float)m_PointB.Y * _scale.Y));

            ComputeValues();*/
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            /*_xmlWriter.WriteElementString("PointO", String.Format("{0};{1}", m_PointO.X, m_PointO.Y));
            _xmlWriter.WriteElementString("PointA", String.Format("{0};{1}", m_PointA.X, m_PointA.Y));
            _xmlWriter.WriteElementString("PointB", String.Format("{0};{1}", m_PointB.X, m_PointB.Y));
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            // Spreadsheet support.
        	_xmlWriter.WriteStartElement("Measure");        	
        	int angle = (int)Math.Floor(-m_fSweepAngle);        	
        	_xmlWriter.WriteAttributeString("UserAngle", angle.ToString());
        	_xmlWriter.WriteEndElement();*/
		}
        #endregion
        
        #region Lower level helpers
        private void InitDrawing()
        {
            m_PointB = new Point(m_PointA.X + 50, m_PointA.Y);
            m_PointC = new Point(m_PointA.X + 100, m_PointA.Y);
            m_PointD = new Point(m_PointA.X + 100, m_PointA.Y - 25);
        }
        private void PushToList()
        {
            m_Points.Clear();
            m_Points.AddRange(new Point[]{m_PointA, m_PointB, m_PointC, m_PointD });
        }
        private void PullFromList()
        {
            m_PointA = m_Points[0];
            m_PointB = m_Points[1];
            m_PointC = m_Points[2];
            m_PointD = m_Points[3];
        }
        private void ComputeValues()
        {
            m_AngleHelper.Update(m_PointB, m_PointC, m_PointD);
        }
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Bicolor", "line color");
        }
        private void DrawText(Graphics _canvas, double _opacity, CoordinateSystem _transformer, Point _origin, SolidBrush _brushFill)
        {
            int angle = (int)Math.Floor(m_AngleHelper.Sweep);
            if(angle < 0)
                angle = -angle;
            string label = angle.ToString() + "°";
            
            SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_opacity * 255));
            Font tempFont = m_StyleHelper.GetFont((float)_transformer.Scale);
            SizeF labelSize = _canvas.MeasureString(label, tempFont);
                
            // Background
            float shiftx = (float)(_transformer.Scale * m_AngleHelper.TextPosition.X);
            float shifty = (float)(_transformer.Scale * m_AngleHelper.TextPosition.Y);
            PointF textOrigin = new PointF(shiftx + _origin.X - labelSize.Width / 2, shifty + _origin.Y - labelSize.Height / 2);
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(_canvas, backRectangle, _brushFill, tempFont.Height/4, false);
    
            // Text
			_canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);
    			
    		tempFont.Dispose();
            fontBrush.Dispose();
        }
        private bool IsPointInObject(Point _point)
        {
            // directing line.
            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddLine(m_PointA, m_PointB);
            Pen areaPen = new Pen(Color.Black, 7);
            areaPath.Widen(areaPen);
            areaPen.Dispose();
            Region areaRegion = new Region(areaPath);
            
            if(areaRegion.IsVisible(_point))
                return true;
            else 
                return m_AngleHelper.Hit(_point);
        }
        #endregion
    }

       
}
