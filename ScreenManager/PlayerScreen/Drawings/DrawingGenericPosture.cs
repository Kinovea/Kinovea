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
    [XmlType("GenericPosture")]
    public class DrawingGenericPosture : AbstractDrawing, IKvaSerializable, IDecorable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
          get { return m_Style; }
        }
        public override InfosFading infosFading
        {
          get { return m_InfosFading; }
          set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
          get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading; }
        }
        public override List<ToolStripMenuItem> ContextMenu
        {
          get { return null; }
        }
        #endregion
        
        #region Members
        private GenericPosture m_GenericPosture;
        private List<AngleHelper> m_Angles = new List<AngleHelper>();
        //private Dictionary<AngleHelper, int[]> m_AngleMaps = new Dictionary<AngleHelper, int[]>();
        
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private InfosFading m_InfosFading;
        private const int m_iDefaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public DrawingGenericPosture(Point _origin, GenericPosture _posture, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            m_GenericPosture = _posture;
            for(int i=0;i<m_GenericPosture.Angles.Count;i++)
                m_Angles.Add(new AngleHelper(m_GenericPosture.Angles[i].Relative, 40));
            
            UpdateAngles();
            
            // Decoration and binding to mini editors.
            m_StyleHelper.Bicolor = new Bicolor(Color.Empty);
            m_StyleHelper.Font = new Font("Arial", 12, FontStyle.Bold);
            if (_stylePreset != null)
            {
                m_Style = _stylePreset.Clone();
                BindStyle();
            }
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
        }
        public DrawingGenericPosture(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, null, 0, 0, ToolManager.Angle.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
        
            if (fOpacityFactor <= 0)
                return;
        
            List<Point> points = _transformer.Transform(m_GenericPosture.Points);
            
            List<Rectangle> boxes = new List<Rectangle>();
            foreach(AngleHelper angle in m_Angles)
                boxes.Add(_transformer.Transform(angle.BoundingBox));
            
            using(Pen penEdge = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor * 255)))
            using(SolidBrush brushHandle = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*255)))
            using(SolidBrush brushFill = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*m_iDefaultBackgroundAlpha)))
            {
                foreach(GenericPostureSegment segment in m_GenericPosture.Segments)
                {
                    penEdge.Width = segment.Width;
                    penEdge.DashStyle = Convert(segment.Style);
                    _canvas.DrawLine(penEdge, points[segment.Start], points[segment.End]);
                }
                
                foreach(GenericPostureHandle handle in m_GenericPosture.Handles)
                {
                    if(handle.Type == HandleType.Point)
                        _canvas.FillEllipse(brushHandle, points[handle.RefPoint].Box(3));
                }
                
                penEdge.Width = 2;
                penEdge.DashStyle = DashStyle.Solid;
                for(int i = 0; i<m_Angles.Count; i++)
                {
                    _canvas.FillPie(brushFill, boxes[i], (float)m_Angles[i].Start, (float)m_Angles[i].Sweep);
                    _canvas.DrawArc(penEdge, boxes[i], (float)m_Angles[i].Start, (float)m_Angles[i].Sweep);
                    DrawText(_canvas, fOpacityFactor, _transformer, m_Angles[i], brushFill);
                }
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int iHitResult = -1;
            if (m_InfosFading.GetOpacityFactor(_iCurrentTimestamp) <= 0)
               return -1;

            for(int i = 0; i<m_GenericPosture.Handles.Count;i++)
            {
                if(m_GenericPosture.Handles[i].Type == HandleType.Point)
                {
                    // Test for point hit.
                    if(m_GenericPosture.Points[m_GenericPosture.Handles[i].RefPoint].Box(10).Contains(_point))
                    {
                        iHitResult = i+1;
                        break;
                    }
                }
                else if(m_GenericPosture.Handles[i].Type == HandleType.Segment)
                {
                    // Test for segment handle hit.
                }
            }
            
            if(iHitResult == -1 && IsPointInObject(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public override void MoveHandle(Point _point, int _iHandleNumber)
        {
            int index = _iHandleNumber - 1;
            GenericPostureConstraintEngine.MoveHandle(m_GenericPosture, index, _point);
            UpdateAngles();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            for(int i = 0;i<m_GenericPosture.Points.Count;i++)
                m_GenericPosture.Points[i] = m_GenericPosture.Points[i].Translate(_deltaX, _deltaY);
            
            UpdateAngles();
        }
        #endregion

        public override string ToString()
        {
            return m_GenericPosture.Name;
        }
        public override int GetHashCode()
        {
          int hash = 0;
          foreach(PointF p in m_GenericPosture.Points)
              hash ^= p.GetHashCode();
          return hash;
        }

        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
           /*_xmlReader.ReadStartElement();
        
          while (_xmlReader.NodeType == XmlNodeType.Element)
          {
            switch (_xmlReader.Name)
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
        private void UpdateAngles()
        {
            for(int i = 0; i<m_Angles.Count;i++)
            {
                PointF origin = m_GenericPosture.Points[m_GenericPosture.Angles[i].Origin];
                PointF leg1 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg1];
                PointF leg2 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg2];
                m_Angles[i].Update(origin.ToPoint(), leg1.ToPoint(), leg2.ToPoint());
            }
        }
        private DashStyle Convert(SegmentLineStyle style)
        {
            switch(style)
            {
            case SegmentLineStyle.Dash:     return DashStyle.Dash;
            case SegmentLineStyle.Solid:    return DashStyle.Solid;
            default: return DashStyle.Solid;
            }
        }
        private void BindStyle()
        {
          m_Style.Bind(m_StyleHelper, "Bicolor", "line color");
        }
        private void DrawText(Graphics _canvas, double _opacity, CoordinateSystem _transformer, AngleHelper angle, SolidBrush _brushFill)
        {
            //-------------------------------------------------
            // FIXME: function duplicated. Move to AngleHelper.
            // This version is already more generic.
            //-------------------------------------------------
            
            int angleValue = (int)Math.Floor(angle.Sweep);
            if(angleValue < 0)
                angleValue = -angleValue;
            string label = angleValue.ToString() + "°";
            
            SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_opacity * 255));
            Font tempFont = m_StyleHelper.GetFont((float)_transformer.Scale);
            SizeF labelSize = _canvas.MeasureString(label, tempFont);
                
            // Background
            float shiftx = (float)(_transformer.Scale * angle.TextPosition.X);
            float shifty = (float)(_transformer.Scale * angle.TextPosition.Y);
            Point origin = _transformer.Transform(angle.Origin);
            PointF textOrigin = new PointF(shiftx + origin.X - labelSize.Width / 2, shifty + origin.Y - labelSize.Height / 2);
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(_canvas, backRectangle, _brushFill, tempFont.Height/4, false);
    
            // Text
			_canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);
    			
    		tempFont.Dispose();
            fontBrush.Dispose();
        }
        private bool IsPointInObject(Point _point)
        {
            // Angles, hit zones, segments.
            
            bool hit = false;
            foreach(AngleHelper angle in m_Angles)
            {
                hit = angle.Hit(_point);
                if(hit)
                    break;
            }
            
            if(hit == true)
                return true;
            
            foreach(GenericPostureAbstractHitZone hitZone in m_GenericPosture.HitZones)
            {
                hit = IsPointInHitZone(hitZone, _point);
                if(hit)
                    break;
            }
                
            if(hit == true)
                return true;
            
            foreach(GenericPostureSegment segment in m_GenericPosture.Segments)
            {
                hit = IsPointOnSegment(segment, _point);
                if(hit)
                    break;
            }
            
            return hit;
        }
        private bool IsPointInHitZone(GenericPostureAbstractHitZone _hitZone, Point _point)
        {
            bool hit = false;
            
            switch(_hitZone.Type)
            {
                case HitZoneType.Polygon:
                {
                    GenericPostureHitZonePolygon hitPolygon = _hitZone as GenericPostureHitZonePolygon;
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        List<Point> points = new List<Point>();
                        foreach(int pointRef in hitPolygon.Points)
                            points.Add(m_GenericPosture.Points[pointRef].ToPoint());
    
                        gp.AddPolygon(points.ToArray());
                        using (Region region = new Region(gp))
                        {
                            hit = region.IsVisible(_point);
                        }
                    }
                    break;
                }
            }
            
            return hit;
        }
        private bool IsPointOnSegment(GenericPostureSegment _segment, Point _point)
        {
            bool hit = false;
            
            using(GraphicsPath segmentPath = new GraphicsPath())
            {
                segmentPath.AddLine(m_GenericPosture.Points[_segment.Start], m_GenericPosture.Points[_segment.End]);
                using(Pen p = new Pen(Color.Black, 7))
                {
                    segmentPath.Widen(p);
                }
                using(Region region = new Region(segmentPath))
                {
                     hit = region.IsVisible(_point);
                }
            }
            
            return hit;
        }
        #endregion
    }
}