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
    [XmlType ("Posture")]
    public class DrawingPosture : AbstractDrawing, IKvaSerializable, IDecorable
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
		public override List<ToolStripMenuItem> ContextMenu {
		    get { return null; }
		}
        #endregion

        #region Members
        
        private Point m_leftLineTop;
        private Point m_leftLineBottom;
        private Point m_middleLineTop;
        private Point m_middleLineBottom;
        private Point m_rightLineTop;
        private Point m_rightLineBottom;
        private Point m_headCenter;
        private Point m_shoulderLineStart;
        private Point m_shoulderLineEnd;
        private int m_headRadius;
        private List<Point> m_Points = new List<Point>();
        
        private Point m_shoulderGrab;
        // Decoration
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private InfosFading m_InfosFading;
        #endregion

        #region Constructor
        public DrawingPosture(Point o, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            m_headCenter = o;
            InitDrawing();
            PushToList();

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
        public DrawingPosture(XmlReader _xmlReader, PointF _scale, Metadata _parent)
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
            
            Point leftLineTop = _transformer.Transform(m_leftLineTop);
            Point leftLineBottom = _transformer.Transform(m_leftLineBottom);
            Point middleLineTop = _transformer.Transform(m_middleLineTop);
            Point middleLineBottom = _transformer.Transform(m_middleLineBottom);
            Point rightLineTop = _transformer.Transform(m_rightLineTop);
            Point rightLineBottom = _transformer.Transform(m_rightLineBottom);
            Point headCenter = _transformer.Transform(m_headCenter);
            Point shoulderLineStart = _transformer.Transform(m_shoulderLineStart);
            Point shoulderLineEnd = _transformer.Transform(m_shoulderLineEnd);
            int radius = _transformer.Transform(m_headRadius);
            
            using(Pen penEdges = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor*255)))
            using(Pen penDash = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor*255)))
            using(SolidBrush brushEdges = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*128)))
            {
                penEdges.Width = 2;
                penDash.Width = 2;
                penDash.DashStyle = DashStyle.Dash;
                
                _canvas.DrawLine(penEdges, leftLineTop, leftLineBottom);
                _canvas.DrawLine(penDash, middleLineTop, middleLineBottom);
                _canvas.DrawLine(penEdges, rightLineTop, rightLineBottom);
                
                _canvas.DrawEllipse(penEdges, headCenter.Box(radius));
                _canvas.FillEllipse(brushEdges, headCenter.Box(5));
                
                _canvas.DrawLine(penEdges, shoulderLineStart, shoulderLineEnd);
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
            
            if(iHitResult == -1 && IsPointOnHeadCircle(_point))
                iHitResult = 10;
            
            if(iHitResult == -1 && IsPointOnLeftLine(_point))
                iHitResult = 11;
            
            if(iHitResult == -1 && IsPointOnRightLine(_point))
                iHitResult = 12;
            
            if(iHitResult == -1 && IsPointOnShoulderLine(_point))
            {
                m_shoulderGrab = _point;
                iHitResult = 13;
            }
            
            if(iHitResult == -1 && IsPointInObject(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public override void MoveHandle(Point _point, int _iHandleNumber)
        {
            int index = _iHandleNumber - 1;
            if(index >= 0 && index <= 2)
            {
                // Top row. Move together.
                for(int i = 0; i < 3; i++)
                    m_Points[i] = new Point(m_Points[i].X, Math.Min(_point.Y, m_Points[3].Y - 10));
            }
            else if(index > 2 && index <= 5)
            {
                // Bottom row. Move together.
                for(int i = 3; i < 6; i++)
                    m_Points[i] = new Point(m_Points[i].X, Math.Max(_point.Y, m_Points[0].Y + 10));
            }
            else if(index == 6 || index == 7)
            {
                // Shoulder line end points. Move freely.
                m_Points[index] = new Point(_point.X, _point.Y);
            }
            else if(index == 8)
            {
                // Head center dot. Move along Y.
                m_Points[index] = new Point(m_Points[index].X, _point.Y);
            }
            else if(index == 9)
            {
                // Head circle. Constrain to side lines.
                int shiftX = Math.Abs(_point.X - m_headCenter.X);
                int shiftY = Math.Abs(_point.Y - m_headCenter.Y);
                int radius = (int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY));
                m_headRadius = Math.Min(Math.Max(radius, 10), m_Points[1].X - m_Points[0].X);
            }
            else if(index == 10)
            {
                // Left side wall. Force symmetry on right side.
                int left = Math.Min(_point.X, (m_headCenter.X - 5));
                int right = m_headCenter.X + (m_headCenter.X - left);
                m_Points[0] = new Point(left, m_Points[0].Y);
                m_Points[3] = new Point(left, m_Points[3].Y);
                m_Points[2] = new Point(right, m_Points[2].Y);
                m_Points[5] = new Point(right, m_Points[5].Y);
            }
            else if(index == 11)
            {
                // Right side wall. Force symmetry on left side.
                int right = Math.Max(_point.X, (m_headCenter.X + 5));
                int left = m_headCenter.X - (right - m_headCenter.X);
                m_Points[0] = new Point(left, m_Points[0].Y);
                m_Points[3] = new Point(left, m_Points[3].Y);
                m_Points[2] = new Point(right, m_Points[2].Y);
                m_Points[5] = new Point(right, m_Points[5].Y);
            }
            else if(index == 12)
            {
                // Shoulder line grab. Compute delta back and move freely.
                int deltaX = _point.X - m_shoulderGrab.X;
                int deltaY = _point.Y - m_shoulderGrab.Y;
                m_Points[6] = new Point(m_Points[6].X + deltaX, m_Points[6].Y + deltaY);
                m_Points[7] = new Point(m_Points[7].X + deltaX, m_Points[7].Y + deltaY);
                m_shoulderGrab = m_shoulderGrab.Translate(deltaX, deltaY);
            }
            
            // Constrain head center to the top and bottom.
            m_Points[8] = new Point(m_Points[8].X, Math.Min(Math.Max(m_Points[8].Y, m_Points[0].Y), m_Points[3].Y));
            
            PullFromList();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            for(int i = 0; i<m_Points.Count; i++)
                m_Points[i] = new Point(m_Points[i].X + _deltaX, m_Points[i].Y + _deltaY);

            PullFromList();
        }
		#endregion
		
        public override string ToString()
        {
            return "Posture"; //ScreenManagerLang.ToolTip_DrawingToolAngle2D;
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
            _xmlReader.ReadStartElement();
            
			/*while(_xmlReader.NodeType == XmlNodeType.Element)
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
            m_headRadius = 25;
            
            int top = m_headCenter.Y - 40;
            int bottom = m_headCenter.Y + 200;
            int left = m_headCenter.X - 75;
            int right = m_headCenter.X + 75;
            int middle = m_headCenter.X;
            int shoulders = m_headCenter.Y + 40;
            
            m_middleLineTop = new Point(middle, top);
            m_middleLineBottom = new Point(middle, bottom);
            m_leftLineTop = new Point(left, top);
            m_rightLineTop = new Point(right, top);
            m_leftLineBottom = new Point(left, bottom);
            m_rightLineBottom = new Point(right, bottom);

            m_shoulderLineStart = new Point(left + 20, shoulders);
            m_shoulderLineEnd = new Point(right - 20, shoulders);
        }
        private void PushToList()
        {
            m_Points.Clear();
            m_Points.AddRange(new Point[]{ 
                                  m_leftLineTop, m_middleLineTop, m_rightLineTop,
                                  m_leftLineBottom, m_middleLineBottom, m_rightLineBottom,
                                  m_shoulderLineStart, m_shoulderLineEnd,
                                  m_headCenter});
        }
        private void PullFromList()
        {
            m_leftLineTop = m_Points[0];
            m_middleLineTop = m_Points[1];
            m_rightLineTop = m_Points[2];
            
            m_leftLineBottom = m_Points[3];
            m_middleLineBottom = m_Points[4];
            m_rightLineBottom = m_Points[5];
            
            m_shoulderLineStart = m_Points[6];
            m_shoulderLineEnd = m_Points[7];
            m_headCenter = m_Points[8];
        }
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Bicolor", "line color");
        }
        private bool IsPointInObject(Point _point)
        {
            Rectangle boundingBox = new Rectangle(m_leftLineTop, new Size(m_rightLineTop.X - m_leftLineTop.X, m_leftLineBottom.Y - m_leftLineTop.Y));
            return boundingBox.Contains(_point);
        }
        private bool IsPointOnHeadCircle(Point _point)
        {
        	GraphicsPath areaPath = new GraphicsPath();			
			areaPath.AddArc(m_headCenter.Box(m_headRadius), 0, 360);
			Pen areaPen = new Pen(Color.Black, 10);
			areaPath.Widen(areaPen);
			areaPen.Dispose();
			
			bool bIsPointOnHandler = false;
            bIsPointOnHandler = new Region(areaPath).IsVisible(_point);
            return bIsPointOnHandler;
        }
        private bool IsPointOnLeftLine(Point _point)
        {
            Rectangle leftLine = new Rectangle(m_leftLineTop.X - 5, m_leftLineTop.Y - 5, 10, m_leftLineBottom.Y - m_leftLineTop.Y + 10);
            return leftLine.Contains(_point);
        }
        private bool IsPointOnRightLine(Point _point)
        {
            Rectangle rightLine = new Rectangle(m_rightLineTop.X - 5, m_rightLineTop.Y - 5, 10, m_rightLineBottom.Y - m_rightLineTop.Y + 10);
            return rightLine.Contains(_point);
        }
        private bool IsPointOnShoulderLine(Point _point)
        {
            GraphicsPath areaPath = new GraphicsPath();
            
            if(m_shoulderLineStart == m_shoulderLineEnd)
                areaPath.AddLine(m_shoulderLineStart.X, m_shoulderLineStart.Y, m_shoulderLineStart.X + 2, m_shoulderLineStart.Y + 2);
            else
            	areaPath.AddLine(m_shoulderLineStart, m_shoulderLineEnd);
            
            Pen areaPen = new Pen(Color.Black, 7);
            areaPath.Widen(areaPen);
            areaPen.Dispose();
            Region areaRegion = new Region(areaPath);
            return areaRegion.IsVisible(_point);
        }
        #endregion
    }

       
}
