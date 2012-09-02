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
    public class DrawingGenericPosture : AbstractDrawing, IKvaSerializable, IDecorable, ITrackable, IMeasurable, IScalable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler ShowMeasurableInfoChanged = delegate {}; // not used.
        #endregion
        
        #region Properties
        public DrawingStyle DrawingStyle
        {
          get { return m_Style; }
        }
        public override InfosFading InfosFading
        {
          get { return m_InfosFading; }
          set { m_InfosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get 
            {
                if(m_GenericPosture.Trackable)
                    return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.Track;
                else
                    return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading;
            }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                if(m_GenericPosture.Capabilities == GenericPostureCapabilities.None)
                    return null;
                
                // Rebuild the menu each time to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                if((m_GenericPosture.Capabilities & GenericPostureCapabilities.FlipHorizontal) == GenericPostureCapabilities.FlipHorizontal)
                {
                    menuFlipHorizontal.Text = ScreenManagerLang.mnuFlipHorizontally;
                    contextMenu.Add(menuFlipHorizontal);
                }

                if((m_GenericPosture.Capabilities & GenericPostureCapabilities.FlipVertical) == GenericPostureCapabilities.FlipVertical)
                {
                    menuFlipVertical.Text = ScreenManagerLang.mnuFlipVertically;
                    contextMenu.Add(menuFlipVertical);
                }

                return contextMenu; 
            }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion
        
        #region Members
        private Guid id = Guid.NewGuid();
    	private bool tracking;
    	private GenericPosture m_GenericPosture;
        private List<AngleHelper> m_Angles = new List<AngleHelper>();
        
        private ToolStripMenuItem menuFlipHorizontal = new ToolStripMenuItem();
        private ToolStripMenuItem menuFlipVertical = new ToolStripMenuItem();
        
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private InfosFading m_InfosFading;
        private const int m_iDefaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public DrawingGenericPosture(Point _origin, GenericPosture _posture, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            m_GenericPosture = _posture;
            if(m_GenericPosture != null)
                InitAngles();
            
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
            
            menuFlipHorizontal.Click += menuFlipHorizontal_Click;
            menuFlipHorizontal.Image = Properties.Drawings.fliphorizontal;
            menuFlipVertical.Click += menuFlipVertical_Click;
            menuFlipVertical.Image = Properties.Drawings.flipvertical;
        }
        public DrawingGenericPosture(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, null, 0, 0, ToolManager.GenericPosture.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
            
            if(m_GenericPosture != null)
                InitAngles();
            else 
                m_GenericPosture = new GenericPosture("", true);
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
        
            if(tracking)
                fOpacityFactor = 1.0;
            
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
                
                foreach(GenericPostureEllipse ellipse in m_GenericPosture.Ellipses)
                {
                    penEdge.Width = ellipse.Width;
                    penEdge.DashStyle = Convert(ellipse.Style);
                    Point center = points[ellipse.Center];
                    int radius = _transformer.Transform(ellipse.Radius);
                    _canvas.DrawEllipse(penEdge, center.Box(radius));
                }
                
                foreach(GenericPostureHandle handle in m_GenericPosture.Handles)
                {
                    if(handle.Type == HandleType.Point)
                        _canvas.FillEllipse(brushHandle, points[handle.Reference].Box(3));
                }
                
                // Angles
                penEdge.Width = 2;
                penEdge.DashStyle = DashStyle.Solid;
                for(int i = 0; i<m_Angles.Count; i++)
                {
                    _canvas.FillPie(brushFill, boxes[i], (float)m_Angles[i].Start, (float)m_Angles[i].Sweep);
                    
                    try
                    {
                        _canvas.DrawArc(penEdge, boxes[i], (float)m_Angles[i].Start, (float)m_Angles[i].Sweep);
                    }
                    catch(Exception e)
                    {
                        log.DebugFormat(e.ToString());
                    }
                    
                    DrawAngleText(_canvas, fOpacityFactor, _transformer, m_Angles[i], brushFill);
                }
                
                // Distances
                foreach(GenericPostureDistance distance in m_GenericPosture.Distances)
                {
                    PointF a = points[distance.Point1];
                    PointF b = points[distance.Point2];
                    DrawDistanceText(a, b, _canvas, fOpacityFactor, _transformer, brushFill);
                }
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int iHitResult = -1;
            if (!tracking && m_InfosFading.GetOpacityFactor(_iCurrentTimestamp) <= 0)
               return -1;

            for(int i = 0; i<m_GenericPosture.Handles.Count;i++)
            {
                if(iHitResult >= 0)
                    break;
                
                switch(m_GenericPosture.Handles[i].Type)
                {
                    case HandleType.Point:
                        if(m_GenericPosture.Points[m_GenericPosture.Handles[i].Reference].Box(10).Contains(_point))
                            iHitResult = i+1;
                        break;
                    case HandleType.Segment:
                        if(IsPointOnSegment(m_GenericPosture.Segments[m_GenericPosture.Handles[i].Reference], _point))
                        {
                            m_GenericPosture.Handles[i].GrabPoint = _point;
                            iHitResult = i+1;
                        }
                        break;
                    case HandleType.Ellipse:
                        if(IsPointOnEllipseArc(m_GenericPosture.Ellipses[m_GenericPosture.Handles[i].Reference], _point))
                            iHitResult = i+1;
                        break;
                }
            }
            
            if(iHitResult == -1 && IsPointInObject(_point))
                iHitResult = 0;

            return iHitResult;
        }
        public override void MoveHandle(Point point, int handle, Keys modifiers)
        {
            int index = handle - 1;
            GenericPostureConstraintEngine.MoveHandle(m_GenericPosture, index, point, modifiers);
            UpdateAngles();
            SignalAllTrackablePointsMoved();
        }
        public override void MoveDrawing(int deltaX, int deltaY, Keys modifiers)
        {
            for(int i = 0;i<m_GenericPosture.Points.Count;i++)
                m_GenericPosture.Points[i] = m_GenericPosture.Points[i].Translate(deltaX, deltaY);
            
            UpdateAngles();
            SignalAllTrackablePointsMoved();
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
            // TODO: the ctor is initialized with the style preset of the Angle tool.
            // Create a real tool and reference it in the manager.
            
            // The id must be read before the point list.
            Guid toolId;

            _xmlReader.ReadStartElement();
            while (_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (_xmlReader.Name)
                {
                    case "ToolId":
                        toolId = new Guid(_xmlReader.ReadElementContentAsString());
                        m_GenericPosture = GenericPostureManager.Instanciate(toolId);
                        break;
                    case "Positions":
                        if(m_GenericPosture != null)
                            ParsePointList(_xmlReader, _scale);
                        else
                            _xmlReader.ReadOuterXml();
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
            List<PointF> points = new List<PointF>();
            
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                if(_xmlReader.Name == "Point")
				{
                    PointF p = XmlHelper.ParsePointF(_xmlReader.ReadElementContentAsString());
                    PointF adapted = new PointF(p.X * _scale.X, p.Y * _scale.Y);
                    points.Add(adapted);
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
				    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
            
            if(points.Count == m_GenericPosture.Points.Count)
            {
                for(int i = 0; i<m_GenericPosture.Points.Count; i++)
                    m_GenericPosture.Points[i] = points[i];
            }
            else
            {
                log.ErrorFormat("Number of points do not match. Tool expects {0}, read:{1}", m_GenericPosture.Points.Count, points.Count);
            }
        }
        public void WriteXml(XmlWriter _xmlWriter)
        {
            if(m_GenericPosture.Id == Guid.Empty)
                return;
            
            _xmlWriter.WriteElementString("ToolId", m_GenericPosture.Id.ToString());
            
            _xmlWriter.WriteStartElement("Positions");
            foreach (PointF p in m_GenericPosture.Points)
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

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            // The coordinates are defined in a reference image of 800x600 (could be inside the posture file).
            // Scale the positions and angle radius according to the actual image size.
            Size referenceSize = new Size(800, 600);
            
            float ratioWidth = (float)imageSize.Width / referenceSize.Width;
            float ratioHeight = (float)imageSize.Height / referenceSize.Height;
            float ratio = Math.Min(ratioWidth, ratioHeight);
            
            for(int i = 0; i < m_GenericPosture.Points.Count; i++)
                m_GenericPosture.Points[i] = m_GenericPosture.Points[i].Scale(ratio, ratio);
            
            for(int i = 0; i < m_GenericPosture.Ellipses.Count; i++)
                m_GenericPosture.Ellipses[i].Radius = (int)(m_GenericPosture.Ellipses[i].Radius * ratio);
            
            for(int i = 0; i<m_GenericPosture.Angles.Count;i++)
                m_GenericPosture.Angles[i].Radius = (int)(m_GenericPosture.Angles[i].Radius * ratio);
                
            UpdateAngles();
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Guid ID
        {
            get { return id; }
        }
        public Dictionary<string, Point> GetTrackablePoints()
        {
            return m_GenericPosture.GetTrackablePoints();
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, Point value)
        {
            m_GenericPosture.SetTrackablePointValue(name, value);
            UpdateAngles();
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
         
            m_GenericPosture.SignalAllTrackablePointsMoved(TrackablePointMoved);
        }
        #endregion
        
        private void menuFlipHorizontal_Click(object sender, EventArgs e)
        {
            m_GenericPosture.FlipHorizontal();
            UpdateAngles();
            SignalAllTrackablePointsMoved();
            CallInvalidateFromMenu(sender);
        }
        private void menuFlipVertical_Click(object sender, EventArgs e)
        {
            m_GenericPosture.FlipVertical();
            UpdateAngles();
            SignalAllTrackablePointsMoved();
            CallInvalidateFromMenu(sender);
        }
        
        #region Lower level helpers
        private void InitAngles()
        {
            for(int i=0;i<m_GenericPosture.Angles.Count;i++)
                m_Angles.Add(new AngleHelper(m_GenericPosture.Angles[i].Relative, 40, m_GenericPosture.Angles[i].Tenth));

            UpdateAngles();
        }
        private void UpdateAngles()
        {
            for(int i = 0; i<m_Angles.Count;i++)
            {
                PointF origin = m_GenericPosture.Points[m_GenericPosture.Angles[i].Origin];
                PointF leg1 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg1];
                PointF leg2 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg2];
                int radius = m_GenericPosture.Angles[i].Radius;
                m_Angles[i].Update(origin, leg1, leg2, radius);
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
        private void DrawAngleText(Graphics _canvas, double _opacity, CoordinateSystem _transformer, AngleHelper angle, SolidBrush _brushFill)
        {
            //-------------------------------------------------
            // FIXME: function duplicated. Move to AngleHelper.
            // This version is already more generic.
            //-------------------------------------------------
            double value = angle.Sweep;
            if(value < 0)
                value = -value;
            
            string label = "";
            if(angle.Tenth)
                label = String.Format("{0:0.0}°", value);
            else
                label = String.Format("{0}°", (int)Math.Round(value));
            
            SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_opacity * 255));
            Font tempFont = m_StyleHelper.GetFont(Math.Max((float)_transformer.Scale, 1.0F));
            SizeF labelSize = _canvas.MeasureString(label, tempFont);
                
            // Background
            float shiftx = (float)(_transformer.Scale * angle.TextPosition.X);
            float shifty = (float)(_transformer.Scale * angle.TextPosition.Y);
            Point origin = _transformer.Transform(angle.Origin);
            PointF textOrigin = new PointF(shiftx + origin.X - labelSize.Width / 2, shifty + origin.Y - labelSize.Height / 2);
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(_canvas, backRectangle, _brushFill, tempFont.Height/4, false, false, null);
    
            // Text
			_canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);
    			
    		tempFont.Dispose();
            fontBrush.Dispose();
        }
        private void DrawDistanceText(PointF a, PointF b, Graphics canvas, double opacity, CoordinateSystem transformer, SolidBrush brushFill)
        {
            string label = CalibrationHelper.GetLengthText(a, b);
            
            SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(opacity * 255));
            Font tempFont = m_StyleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            
            PointF middle = GeometryHelper.GetMiddlePoint(a, b);
            PointF textOrigin = new PointF(middle.X - labelSize.Width / 2, middle.Y + 5);
            
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(canvas, backRectangle, brushFill, tempFont.Height/4, false, false, null);

            // Text
			canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);
            
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
            
            foreach(GenericPostureEllipse ellipse in m_GenericPosture.Ellipses)
            {
                hit = IsPointInsideEllipse(ellipse, _point);
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
                        List<PointF> points = new List<PointF>();
                        foreach(int pointRef in hitPolygon.Points)
                            points.Add(m_GenericPosture.Points[pointRef]);
    
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
            if(m_GenericPosture.Points[_segment.Start] == m_GenericPosture.Points[_segment.End])
                return false;
            
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
        private bool IsPointInsideEllipse(GenericPostureEllipse _ellipse, Point _point)
        {
            bool hit = false;
            
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(m_GenericPosture.Points[_ellipse.Center].Box(_ellipse.Radius));
                using(Region region = new Region(path))
                {
                     hit = region.IsVisible(_point);
                }
            }
            
            return hit;
        }
        private bool IsPointOnEllipseArc(GenericPostureEllipse _ellipse, Point _point)
        {
            bool hit = false;
            
        	using(GraphicsPath path = new GraphicsPath())
            {        	
        		path.AddArc(m_GenericPosture.Points[_ellipse.Center].Box(_ellipse.Radius), 0, 360);
        		using(Pen p = new Pen(Color.Black, 7))
                {
                    path.Widen(p);
                }
        		using(Region region = new Region(path))
                {
                     hit = region.IsVisible(_point);
                }
        	}
            return hit;
        }
        #endregion
    }
}