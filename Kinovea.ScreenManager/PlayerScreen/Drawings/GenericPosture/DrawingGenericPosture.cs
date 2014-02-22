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
using System.Globalization;
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
        public override string DisplayName
        {
            get {  return m_GenericPosture.Name; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = 0;
                foreach(PointF p in m_GenericPosture.Points)
                    hash ^= p.GetHashCode();
                
                hash ^= m_StyleHelper.ContentHash;
                hash ^= m_InfosFading.ContentHash;
                return hash;
            }
        } 
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
                // Rebuild the menu each time to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                if(m_GenericPosture.OptionGroups.Count > 0)
                {
                    menuOptions.Text = "Options"; // TODO: translate.
                    contextMenu.Add(menuOptions);
                }
                
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

                if(contextMenu.Count == 0)
                    return null;
                else 
                    return contextMenu; 
            }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion
        
        #region Members
        private bool tracking;
        private GenericPosture m_GenericPosture;
        private List<AngleHelper> m_Angles = new List<AngleHelper>();
        
        private ToolStripMenuItem menuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem menuFlipHorizontal = new ToolStripMenuItem();
        private ToolStripMenuItem menuFlipVertical = new ToolStripMenuItem();
        
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private InfosFading m_InfosFading;
        private const int m_iDefaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public DrawingGenericPosture(GenericPosture _posture, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            m_GenericPosture = _posture;
            if(m_GenericPosture != null)
                Init();
            
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
            : this(null, 0, 0, ToolManager.GenericPosture.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
            
            if(m_GenericPosture != null)
                Init();
            else 
                m_GenericPosture = new GenericPosture("", true, false);
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, IImageToViewportTransformer _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double opacity = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
        
            if(tracking)
                opacity = 1.0;
            
            if (opacity <= 0)
                return;
            
            List<Point> points = _transformer.Transform(m_GenericPosture.Points);
            
            int alpha = (int)(opacity * 255);
            alpha = Math.Max(0, Math.Min(255, alpha));

            int alphaBackground = (int)(opacity*m_iDefaultBackgroundAlpha);
            alphaBackground = Math.Max(0, Math.Min(255, alphaBackground));
            
            using(Pen penEdge = m_StyleHelper.GetBackgroundPen(alpha))
            using(SolidBrush brushHandle = m_StyleHelper.GetBackgroundBrush(alpha))
            using(SolidBrush brushFill = m_StyleHelper.GetBackgroundBrush(alphaBackground))
            {
                Color basePenEdgeColor = penEdge.Color;
                Color baseBrushHandleColor = brushHandle.Color;
                Color baseBrushFillColor = brushFill.Color;
                
                DrawComputedPoints(penEdge, basePenEdgeColor, brushHandle, baseBrushHandleColor, alpha, opacity, _canvas, _transformer);
                DrawSegments(penEdge, basePenEdgeColor, alpha, _canvas, _transformer, points);
                DrawEllipses(penEdge, basePenEdgeColor, alpha, _canvas, _transformer, points);
                DrawHandles(brushHandle, baseBrushHandleColor, alpha, _canvas, _transformer, points);
                DrawAngles(penEdge, basePenEdgeColor, brushFill, baseBrushFillColor, alpha, alphaBackground, opacity, _canvas, _transformer, points);
                DrawDistances(brushFill, baseBrushFillColor, alphaBackground, opacity, _canvas, _transformer, points);
                DrawPositions(brushFill, baseBrushFillColor, alphaBackground, opacity, _canvas, _transformer, points);
            }
        }
        public override int HitTest(Point point, long currentTimestamp, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            if (!tracking && m_InfosFading.GetOpacityFactor(currentTimestamp) <= 0)
               return -1;
            
            int boxSide = transformer.Untransform(10);

            for(int i = 0; i<m_GenericPosture.Handles.Count;i++)
            {
                if(result >= 0)
                    break;
                
                if(!HasActiveOption(m_GenericPosture.Handles[i].OptionGroup))
                    continue;
            
                int reference = m_GenericPosture.Handles[i].Reference;
                if(reference < 0)
                    continue;
                
                switch(m_GenericPosture.Handles[i].Type)
                {
                    case HandleType.Point:
                        if(reference < m_GenericPosture.Points.Count && m_GenericPosture.Points[reference].Box(boxSide).Contains(point))
                            result = i+1;
                        break;
                    case HandleType.Segment:
                        if(reference < m_GenericPosture.Segments.Count && IsPointOnSegment(m_GenericPosture.Segments[reference], point, transformer))
                        {
                            m_GenericPosture.Handles[i].GrabPoint = point;
                            result = i+1;
                        }
                        break;
                    case HandleType.Ellipse:
                        if (reference < m_GenericPosture.Ellipses.Count && IsPointOnEllipseArc(m_GenericPosture.Ellipses[reference], point, transformer))
                            result = i+1;
                        break;
                }
            }
            
            if(result == -1 && IsPointInObject(point, transformer))
                result = 0;

            return result;
        }
        public override void MoveHandle(PointF point, int handle, Keys modifiers)
        {
            int index = handle - 1;
            GenericPostureConstraintEngine.MoveHandle(m_GenericPosture, CalibrationHelper, index, point, modifiers);
            SignalTrackablePointMoved(index);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            for(int i = 0;i<m_GenericPosture.Points.Count;i++)
                m_GenericPosture.Points[i] = m_GenericPosture.Points[i].Translate(dx, dy);
            
            SignalAllTrackablePointsMoved();
        }
        #endregion

        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            // TODO: the ctor is initialized with the style preset of the Angle tool.
            // Create a real tool and reference it in the manager.
            
            // The id must be read before the point list.
            Guid toolId;

            if (_xmlReader.MoveToAttribute("id"))
                id = new Guid(_xmlReader.ReadContentAsString());

            _xmlReader.ReadStartElement();
            while (_xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (_xmlReader.Name)
                {
                    case "ToolId":
                        toolId = new Guid(_xmlReader.ReadElementContentAsString());
                        m_GenericPosture = GenericPostureManager.Instanciate(toolId, true);
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
            SignalAllTrackablePointsMoved();
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
                _xmlWriter.WriteElementString("Point", String.Format(CultureInfo.InvariantCulture, "{0};{1}", p.X, p.Y));
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
            if(m_GenericPosture.FromKVA)
                return;
            
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
                
        }
        #endregion
        
        #region ITrackable implementation and support.
        public TrackingProfile CustomTrackingProfile
        {
            get { return m_GenericPosture.CustomTrackingProfile; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return m_GenericPosture.GetTrackablePoints();
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, PointF value)
        {
            m_GenericPosture.SetTrackablePointValue(name, value, CalibrationHelper);
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
         
            m_GenericPosture.SignalAllTrackablePointsMoved(TrackablePointMoved);
        }
        private void SignalTrackablePointMoved(int handle)
        {
            if (TrackablePointMoved == null || !m_GenericPosture.Handles[handle].Trackable)
                return;

            m_GenericPosture.SignalTrackablePointMoved(handle, TrackablePointMoved);
        }
        #endregion
        
        private void menuFlipHorizontal_Click(object sender, EventArgs e)
        {
            m_GenericPosture.FlipHorizontal();
            SignalAllTrackablePointsMoved();
            CallInvalidateFromMenu(sender);
        }
        private void menuFlipVertical_Click(object sender, EventArgs e)
        {
            m_GenericPosture.FlipVertical();
            SignalAllTrackablePointsMoved();
            CallInvalidateFromMenu(sender);
        }
        
        #region Drawing helpers
        private void DrawComputedPoints(Pen penEdge, Color basePenEdgeColor, SolidBrush brushHandle, Color baseBrushHandleColor, int alpha, double opacity, Graphics canvas, IImageToViewportTransformer transformer)
        {
            penEdge.Width = 2;
            
            foreach(GenericPostureComputedPoint computedPoint in m_GenericPosture.ComputedPoints)
            {
                if(!HasActiveOption(computedPoint.OptionGroup))
                    continue;
                    
                PointF p = computedPoint.ComputeLocation(m_GenericPosture);
                PointF p2 = transformer.Transform(p);
                
                if (!string.IsNullOrEmpty(computedPoint.Symbol))
                {
                    brushHandle.Color = computedPoint.Color == Color.Transparent ? baseBrushHandleColor : Color.FromArgb(alpha, computedPoint.Color);
                    DrawSimpleText(p2, computedPoint.Symbol, canvas, opacity, transformer, brushHandle);
                }
                else
                {
                    penEdge.Color = computedPoint.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, computedPoint.Color);
                    canvas.DrawEllipse(penEdge, p2.Box(3));
                }
            }
            
            brushHandle.Color = baseBrushHandleColor;
            penEdge.Color = basePenEdgeColor;
            penEdge.Width = 1;
        }
        private void DrawSegments(Pen penEdge, Color basePenEdgeColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureSegment segment in m_GenericPosture.Segments)
            {
                if(!HasActiveOption(segment.OptionGroup))
                    continue;
                    
                penEdge.Width = segment.Width;
                penEdge.DashStyle = Convert(segment.Style);
                penEdge.Color = segment.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, segment.Color);

                if(segment.ArrowBegin)
                    penEdge.StartCap = LineCap.ArrowAnchor;
                if(segment.ArrowEnd)
                    penEdge.EndCap = LineCap.ArrowAnchor;

                PointF start = segment.Start >= 0 ? points[segment.Start] : GetComputedPoint(segment.Start, transformer);
                PointF end = segment.End >= 0 ? points[segment.End] : GetComputedPoint(segment.End, transformer);

                canvas.DrawLine(penEdge, start, end);
            }
            
            penEdge.Color = basePenEdgeColor;
            penEdge.StartCap = LineCap.NoAnchor;
            penEdge.EndCap = LineCap.NoAnchor;
        }
        private void DrawEllipses(Pen penEdge, Color basePenEdgeColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureEllipse ellipse in m_GenericPosture.Ellipses)
            {
                if(!HasActiveOption(ellipse.OptionGroup))
                    continue;
                    
                penEdge.Width = ellipse.Width;
                penEdge.DashStyle = Convert(ellipse.Style);
                penEdge.Color = ellipse.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, ellipse.Color);
                
                PointF center = ellipse.Center >= 0 ? points[ellipse.Center] : GetComputedPoint(ellipse.Center, transformer);
                
                int radius = transformer.Transform(ellipse.Radius);
                canvas.DrawEllipse(penEdge, center.Box(radius));
            }
            
            penEdge.Color = basePenEdgeColor;
        }
        private void DrawHandles(SolidBrush brushHandle, Color baseBrushHandleColor, int alpha, Graphics canvas, IImageToViewportTransformer _transformer, List<Point> points)
        {
            foreach(GenericPostureHandle handle in m_GenericPosture.Handles)
            {
                if(!HasActiveOption(handle.OptionGroup))
                    continue;
                    
                if(handle.Type == HandleType.Point && handle.Reference >= 0 && handle.Reference < points.Count)
                {
                    brushHandle.Color = handle.Color == Color.Transparent ? baseBrushHandleColor : Color.FromArgb(alpha, handle.Color);
                    canvas.FillEllipse(brushHandle, points[handle.Reference].Box(3));

                    /*Pen p = new Pen(handle.Color);
                    Point point = points[handle.Reference];
                    Rectangle block = point.Box(_transformer.Transform(m_GenericPosture.CustomTrackingProfile.BlockWindow));
                    Rectangle search = point.Box(_transformer.Transform(m_GenericPosture.CustomTrackingProfile.SearchWindow));
                    canvas.DrawRectangle(p, block);
                    canvas.DrawRectangle(p, search);*/
                }
            }
            
            brushHandle.Color = baseBrushHandleColor;
        }
        private void DrawAngles(Pen penEdge, Color basePenEdgeColor, SolidBrush brushFill, Color baseBrushFillColor, int alpha, int alphaBackground, double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            List<Rectangle> boxes = new List<Rectangle>();
            foreach(AngleHelper angle in m_Angles)
                boxes.Add(transformer.Transform(angle.BoundingBox));
            
            penEdge.Width = 2;
            penEdge.DashStyle = DashStyle.Solid;
            
            for(int i = 0; i<m_Angles.Count; i++)
            {
                if(!HasActiveOption(m_GenericPosture.Angles[i].OptionGroup))
                    continue;
                
                AngleHelper angle = m_Angles[i];

                UpdateAngles(transformer);
                    
                brushFill.Color = angle.Color == Color.Transparent ? baseBrushFillColor : Color.FromArgb(alphaBackground, angle.Color);
                
                canvas.FillPie(brushFill, boxes[i], (float)m_Angles[i].Angle.Start, (float)m_Angles[i].Angle.Sweep);
                
                try
                {
                    penEdge.Color = angle.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, angle.Color);
                    canvas.DrawArc(penEdge, boxes[i], (float)m_Angles[i].Angle.Start, (float)m_Angles[i].Angle.Sweep);
                }
                catch(Exception e)
                {
                    log.DebugFormat(e.ToString());
                }
                
                DrawAngleText(canvas, opacity, transformer, m_Angles[i], brushFill);
            }
            
            brushFill.Color = baseBrushFillColor;
            penEdge.Width = 1;
            penEdge.Color = basePenEdgeColor;
        }
        private void DrawDistances(SolidBrush brushFill, Color baseBrushFillColor, int alphaBackground, double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureDistance distance in m_GenericPosture.Distances)
            {
                if(!HasActiveOption(distance.OptionGroup))
                    continue;
                
                PointF untransformedA = distance.Point1 >= 0 ? m_GenericPosture.Points[distance.Point1] : GetUntransformedComputedPoint(distance.Point1);
                PointF untransformedB = distance.Point2 >= 0 ? m_GenericPosture.Points[distance.Point2] : GetUntransformedComputedPoint(distance.Point2);
                string label = CalibrationHelper.GetLengthText(untransformedA, untransformedB, true, true);
                
                if(!string.IsNullOrEmpty(distance.Symbol))
                    label = string.Format("{0} = {1}", distance.Symbol, label);
                
                PointF a = distance.Point1 >= 0 ? points[distance.Point1] : GetComputedPoint(distance.Point1, transformer);
                PointF b = distance.Point2 >= 0 ? points[distance.Point2] : GetComputedPoint(distance.Point2, transformer);

                brushFill.Color = distance.Color == Color.Transparent ? baseBrushFillColor : Color.FromArgb(alphaBackground, distance.Color);
                DrawDistanceText(a, b, label, canvas, opacity, transformer, brushFill);
            }
            
            brushFill.Color = baseBrushFillColor;
        }
        private void DrawPositions(SolidBrush brushFill, Color baseBrushFillColor, int alphaBackground, double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPosturePosition position in m_GenericPosture.Positions)
            {
                if(!HasActiveOption(position.OptionGroup))
                    continue;
                
                PointF untransformedP = position.Point >= 0 ? m_GenericPosture.Points[position.Point] : GetUntransformedComputedPoint(position.Point);
                string label = CalibrationHelper.GetPointText(untransformedP, true, true);
                
                if(!string.IsNullOrEmpty(position.Symbol))
                    label = string.Format("{0} = {1}", position.Symbol, label);
                
                PointF p = position.Point >= 0 ? points[position.Point] : GetComputedPoint(position.Point, transformer);
                
                brushFill.Color = position.Color == Color.Transparent ? baseBrushFillColor : Color.FromArgb(alphaBackground, position.Color);
                DrawPointText(p, label, canvas, opacity, transformer, brushFill);
            }
            
            brushFill.Color = baseBrushFillColor;
        }
        private void DrawAngleText(Graphics _canvas, double _opacity, IImageToViewportTransformer _transformer, AngleHelper angle, SolidBrush _brushFill)
        {
            //-------------------------------------------------
            // FIXME: function duplicated. Move to AngleHelper.
            // This version is already more generic.
            //-------------------------------------------------
            double value = CalibrationHelper.ConvertAngleFromDegrees(angle.CalibratedAngle.Sweep);
            if(value < 0)
                value = -value;
            
            string label = "";
            if(angle.Tenth)
                label = String.Format("{0:0.0} {1}", value, CalibrationHelper.GetAngleAbbreviation());
            else
                label = String.Format("{0} {1}", (int)Math.Round(value), CalibrationHelper.GetAngleAbbreviation());
            
            if(!string.IsNullOrEmpty(angle.Symbol))
                label = string.Format("{0} = {1}", angle.Symbol, label);
            
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
        private void DrawDistanceText(PointF a, PointF b, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer, SolidBrush brushFill)
        {
            PointF middle = GeometryHelper.GetMiddlePoint(a, b);
            PointF offset = new PointF(0, 15);
            
            DrawTextOnBackground(middle, offset, label, canvas, opacity, transformer, brushFill);
        }
        private void DrawPointText(PointF a, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer, SolidBrush brushFill)
        {
            PointF offset = new PointF(0, -20);
            DrawTextOnBackground(a, offset, label, canvas, opacity, transformer, brushFill);
        }
        private void DrawTextOnBackground(PointF location, PointF offset, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer, SolidBrush brushFill)
        {
            Font tempFont = m_StyleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            PointF textOrigin = new PointF(location.X - (labelSize.Width / 2) + offset.X, location.Y - (labelSize.Height / 2) + offset.Y);
            
            Bicolor bicolor = new Bicolor(brushFill.Color);
            SolidBrush fontBrush = new SolidBrush(Color.FromArgb((int)(opacity*255), bicolor.Foreground));

            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            RoundedRectangle.Draw(canvas, backRectangle, brushFill, tempFont.Height/4, false, false, null);

            // Text
            canvas.DrawString(label, tempFont, fontBrush, backRectangle.Location);
            
            fontBrush.Dispose();
            tempFont.Dispose();
        }
        private void DrawSimpleText(PointF location, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer, SolidBrush brush)
        {
            Font tempFont = m_StyleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            PointF textOrigin = new PointF(location.X - labelSize.Width / 2, location.Y - labelSize.Height / 2);
            canvas.DrawString(label, tempFont, brush, textOrigin);
            tempFont.Dispose();
        }
        #endregion
        
        #region Lower level helpers
        private void Init()
        {
            InitAngles();
            InitOptionMenus();
        }
        private void InitOptionMenus()
        {
            // Options
            if(m_GenericPosture == null || m_GenericPosture.OptionGroups == null || m_GenericPosture.OptionGroups.Count == 0)
                return;
            
            foreach(string option in m_GenericPosture.OptionGroups.Keys)
            {
                ToolStripMenuItem menu = new ToolStripMenuItem();
                menu.Text = option;
                menu.Checked = m_GenericPosture.OptionGroups[option];
                
                string closureOption = option;
                menu.Click += (s, e) => {
                    m_GenericPosture.OptionGroups[closureOption] = !m_GenericPosture.OptionGroups[closureOption];
                    menu.Checked = m_GenericPosture.OptionGroups[closureOption];
                    CallInvalidateFromMenu(s);
                };
                
                menuOptions.DropDownItems.Add(menu);
            }
            
            menuOptions.Image = Properties.Drawings.eye;
        }
        private void InitAngles()
        {
            for(int i=0;i<m_GenericPosture.Angles.Count;i++)
                m_Angles.Add(new AngleHelper(m_GenericPosture.Angles[i].Relative, 40, m_GenericPosture.Angles[i].Tenth, m_GenericPosture.Angles[i].Symbol));
        }
        private void UpdateAngles(IImageToViewportTransformer transformer)
        {
            for(int i = 0; i<m_Angles.Count;i++)
            {
                PointF origin = m_GenericPosture.Points[m_GenericPosture.Angles[i].Origin];
                PointF leg1 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg1];
                PointF leg2 = m_GenericPosture.Points[m_GenericPosture.Angles[i].Leg2];
                int radius = m_GenericPosture.Angles[i].Radius;
                Color color = m_GenericPosture.Angles[i].Color;
                m_Angles[i].Update(origin, leg1, leg2, radius, color, CalibrationHelper, transformer);
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
        
        private bool HasActiveOption(string option)
        {
            if(string.IsNullOrEmpty(option))
                return true;
            
            return m_GenericPosture.OptionGroups[option];
        }
        private PointF GetComputedPoint(int index, IImageToViewportTransformer transformer)
        {
            PointF result = PointF.Empty;
            
            int computedPointIndex = - index - 1;
            if(computedPointIndex < m_GenericPosture.ComputedPoints.Count)
                result = m_GenericPosture.ComputedPoints[computedPointIndex].LastPoint;
            
            return transformer.Transform(result);
        }
        private PointF GetUntransformedComputedPoint(int index)
        {
            PointF result = PointF.Empty;
            
            int computedPointIndex = - index - 1;
            if(computedPointIndex < m_GenericPosture.ComputedPoints.Count)
                result = m_GenericPosture.ComputedPoints[computedPointIndex].LastPoint;
            
            return result;
        }
        private bool IsPointInObject(Point _point, IImageToViewportTransformer transformer)
        {
            // Angles, hit zones, segments.
            
            bool hit = false;
            foreach(AngleHelper angle in m_Angles)
            {
                hit = angle.Hit(_point);
                if(hit)
                    break;
            }
            
            if(hit)
                return true;
            
            foreach(GenericPostureAbstractHitZone hitZone in m_GenericPosture.HitZones)
            {
                hit = IsPointInHitZone(hitZone, _point);
                if(hit)
                    break;
            }
                
            if(hit)
                return true;
            
            foreach(GenericPostureEllipse ellipse in m_GenericPosture.Ellipses)
            {
                hit = IsPointInsideEllipse(ellipse, _point);
                if(hit)
                    break;
            }
            
            if(hit)
                return true;
            
            foreach(GenericPostureSegment segment in m_GenericPosture.Segments)
            {
                hit = IsPointOnSegment(segment, _point, transformer);
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
        private bool IsPointOnSegment(GenericPostureSegment _segment, Point _point, IImageToViewportTransformer transformer)
        {
            bool hit = false;
            
            PointF start = _segment.Start >= 0 ? m_GenericPosture.Points[_segment.Start] : GetUntransformedComputedPoint(_segment.Start);
            PointF end = _segment.End >= 0 ? m_GenericPosture.Points[_segment.End] : GetUntransformedComputedPoint(_segment.End);
            
            if(start == end)
                return false;
            
            using(GraphicsPath segmentPath = new GraphicsPath())
            {
                segmentPath.AddLine(start, end);
                int expander = transformer.Untransform(7);
                using(Pen p = new Pen(Color.Black, expander))
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
                PointF center = _ellipse.Center >= 0 ? m_GenericPosture.Points[_ellipse.Center] : GetUntransformedComputedPoint(_ellipse.Center);
                path.AddEllipse(center.Box(_ellipse.Radius));
                using(Region region = new Region(path))
                {
                     hit = region.IsVisible(_point);
                }
            }
            
            return hit;
        }
        private bool IsPointOnEllipseArc(GenericPostureEllipse _ellipse, Point _point, IImageToViewportTransformer transformer)
        {
            bool hit = false;
            
            using(GraphicsPath path = new GraphicsPath())
            {
                PointF center = _ellipse.Center >= 0 ? m_GenericPosture.Points[_ellipse.Center] : GetUntransformedComputedPoint(_ellipse.Center);
                
                path.AddArc(center.Box(_ellipse.Radius), 0, 360);
                int expander = transformer.Untransform(7);
                using(Pen p = new Pen(Color.Black, expander))
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