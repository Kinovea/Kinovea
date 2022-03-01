#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
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
using System.Linq;
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
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged = delegate {}; // not used.
        #endregion
        
        #region Properties
        public override string ToolDisplayName
        {
            get {  return genericPosture.Name; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = 0;
                foreach(PointF p in genericPosture.PointList)
                    hash ^= p.GetHashCode();
                
                hash ^= styleHelper.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
          get { return style; }
        }
        public override InfosFading InfosFading
        {
          get { return infosFading; }
          set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get 
            {
                DrawingCapabilities basicCaps = DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.CopyPaste;
                if (genericPosture.Trackable)
                    basicCaps |= DrawingCapabilities.Track;

                return basicCaps;
            }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                // Rebuild the menu each time to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                if(genericPosture.HasNonHiddenOptions)
                {
                    menuOptions.Text = ScreenManagerLang.Generic_Options;
                    contextMenu.Add(menuOptions);
                }
                
                if((genericPosture.Capabilities & GenericPostureCapabilities.FlipHorizontal) == GenericPostureCapabilities.FlipHorizontal)
                {
                    menuFlipHorizontal.Text = ScreenManagerLang.mnuFlipHorizontally;
                    contextMenu.Add(menuFlipHorizontal);
                }

                if((genericPosture.Capabilities & GenericPostureCapabilities.FlipVertical) == GenericPostureCapabilities.FlipVertical)
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
        public List<GenericPostureAngle> GenericPostureAngles 
        {
            get { return genericPosture.Angles; }
        }
        public List<AngleHelper> AngleHelpers 
        {
            get { return angles; }
        }
        public List<GenericPostureHandle> GenericPostureHandles
        {
            get { return genericPosture.Handles; }
        }
        public Guid ToolId
        {
            get { return toolId; }
        }
        public GenericPosture GenericPosture
        {
            get { return genericPosture; }
        }
        #endregion

        #region Members
        private Guid toolId;
        private long trackingTimestamps = -1;
        private PointF origin;
        private GenericPosture genericPosture;
        private List<AngleHelper> angles = new List<AngleHelper>();
        private ToolStripMenuItem menuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem menuFlipHorizontal = new ToolStripMenuItem();
        private ToolStripMenuItem menuFlipVertical = new ToolStripMenuItem();

        private Font debugFont = new Font("Arial", 8, FontStyle.Bold);
        private PointF debugOffset = new PointF(10, 10);
        SolidBrush debugBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0));

        private DrawingStyle style;
        private StyleHelper styleHelper = new StyleHelper();
        private InfosFading infosFading;
        private const int defaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public DrawingGenericPosture(Guid toolId, PointF origin, GenericPosture posture, long timestamp, long averageTimeStampsPerFrame, DrawingStyle stylePreset)
        {
            this.toolId = toolId;
            this.origin = origin;
            this.genericPosture = posture;
            if(genericPosture != null)
                InitOptionMenus();
            
            // Decoration and binding to mini editors.
            styleHelper.Bicolor = new Bicolor(Color.Empty);
            styleHelper.Font = new Font("Arial", 12, FontStyle.Bold);

            if (stylePreset == null)
            {
                stylePreset = new DrawingStyle();
                stylePreset.Elements.Add("line color", new StyleElementColor(Color.DarkOliveGreen));
            }
            
            style = stylePreset.Clone();
            BindStyle();
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            
            menuFlipHorizontal.Click += menuFlipHorizontal_Click;
            menuFlipHorizontal.Image = Properties.Drawings.fliphorizontal;
            menuFlipVertical.Click += menuFlipVertical_Click;
            menuFlipVertical.Image = Properties.Drawings.flipvertical;
        }
        
        public DrawingGenericPosture(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(Guid.Empty, PointF.Empty, null, 0, 0, null)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        
        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity <= 0)
                return;
            
            List<Point> points = transformer.Transform(genericPosture.PointList);
            
            int alpha = (int)(opacity * 255);
            alpha = Math.Max(0, Math.Min(255, alpha));

            int alphaBackground = (int)(opacity * defaultBackgroundAlpha);
            alphaBackground = Math.Max(0, Math.Min(255, alphaBackground));
            
            using(Pen penEdge = styleHelper.GetBackgroundPen(alpha))
            using(SolidBrush brushHandle = styleHelper.GetBackgroundBrush(alpha))
            using(SolidBrush brushFill = styleHelper.GetBackgroundBrush(alphaBackground))
            {
                Color basePenEdgeColor = penEdge.Color;
                Color baseBrushHandleColor = brushHandle.Color;
                Color baseBrushFillColor = brushFill.Color;
                
                DrawSegments(penEdge, basePenEdgeColor, alpha, canvas, transformer, points);
                DrawCircles(penEdge, basePenEdgeColor, alpha, canvas, transformer, points);
                DrawAngles(penEdge, basePenEdgeColor, brushFill, baseBrushFillColor, alpha, alphaBackground, opacity, canvas, transformer, points);
                DrawPolylines(penEdge, basePenEdgeColor, alpha, canvas, transformer, points);
                DrawPoints(brushHandle, baseBrushHandleColor, alpha, canvas, transformer, points);

                DrawComputedPoints(penEdge, basePenEdgeColor, brushHandle, baseBrushHandleColor, alpha, opacity, canvas, transformer);
                DrawDistances(brushFill, baseBrushFillColor, alphaBackground, opacity, canvas, transformer, points);
                DrawPositions(brushFill, baseBrushFillColor, alphaBackground, opacity, canvas, transformer, points);

                if (PreferencesManager.PlayerPreferences.EnableCustomToolsDebugMode)
                    DrawDebug(opacity, canvas, transformer, points);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity <= 0)
               return -1;
            
            for(int i = 0; i<genericPosture.Handles.Count;i++)
            {
                if(result >= 0)
                    break;
                
                if(!IsActive(genericPosture.Handles[i].OptionGroup))
                    continue;
            
                int reference = genericPosture.Handles[i].Reference;
                if(reference < 0)
                    continue;
                
                switch(genericPosture.Handles[i].Type)
                {
                    case HandleType.Point:
                        if(reference < genericPosture.PointList.Count && HitTester.HitTest(genericPosture.PointList[reference], point, transformer))
                            result = i+1;
                        break;
                    case HandleType.Segment:
                        if(reference < genericPosture.Segments.Count && IsPointOnSegment(genericPosture.Segments[reference], point, transformer))
                        {
                            genericPosture.Handles[i].GrabPoint = point;
                            result = i+1;
                        }
                        break;
                    case HandleType.Ellipse:
                    case HandleType.Circle:
                        if (reference < genericPosture.Circles.Count && IsPointOnArc(genericPosture.Circles[reference], point, transformer))
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
            GenericPostureConstraintEngine.MoveHandle(genericPosture, CalibrationHelper, index, point, modifiers);
            SignalTrackablePointMoved(index);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            for(int i = 0;i<genericPosture.PointList.Count;i++)
                genericPosture.PointList[i] = genericPosture.PointList[i].Translate(dx, dy);
            
            SignalAllTrackablePointsMoved();
        }
        public override PointF GetCopyPoint()
        {
            return genericPosture.PointList[0];
        }
        #endregion

        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            // The tool id must be read before the point list.
            
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            if (xmlReader.MoveToAttribute("name"))
                name = xmlReader.ReadContentAsString();

            xmlReader.ReadStartElement();
            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "ToolId":
                        toolId = new Guid(xmlReader.ReadElementContentAsString());
                        genericPosture = GenericPostureManager.Instanciate(toolId, true);
                        break;
                    case "Positions":
                        if(genericPosture != null)
                            ParsePointList(xmlReader, scale);
                        else
                            xmlReader.ReadOuterXml();
                        break;
                   case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(xmlReader);
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
            SignalAllTrackablePointsMoved();

            if (genericPosture != null)
                InitOptionMenus();
            else
                genericPosture = new GenericPosture("", true, false);
        }
        private void ParsePointList(XmlReader xmlReader, PointF scale)
        {
            List<PointF> points = new List<PointF>();
            
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                if(xmlReader.Name == "Point")
                {
                    PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                    PointF adapted = new PointF(p.X * scale.X, p.Y * scale.Y);
                    points.Add(adapted);
                }
                else
                {
                    string unparsed = xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            xmlReader.ReadEndElement();
            
            if(points.Count == genericPosture.PointList.Count)
            {
                for(int i = 0; i<genericPosture.PointList.Count; i++)
                    genericPosture.PointList[i] = points[i];
            }
            else
            {
                log.ErrorFormat("Number of points do not match. Tool expects {0}, read:{1}", genericPosture.PointList.Count, points.Count);
            }
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if(genericPosture.Id == Guid.Empty)
                return;
            
            w.WriteElementString("ToolId", genericPosture.Id.ToString());

            if (ShouldSerializeCore(filter))
            {
                w.WriteStartElement("Positions");
                foreach (PointF p in genericPosture.PointList)
                    w.WriteElementString("Point", String.Format(CultureInfo.InvariantCulture, "{0};{1}", p.X, p.Y));
                w.WriteEndElement();
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }

            if (ShouldSerializeFading(filter))
            {
                w.WriteStartElement("InfosFading");
                infosFading.WriteXml(w);
                w.WriteEndElement();
            }
        }
        
        public List<MeasuredDataPosition> CollectMeasuredDataPositions()
        {
            List<MeasuredDataPosition> mdps = new List<MeasuredDataPosition>();

            GenericPosture gp = genericPosture;
            foreach (GenericPosturePosition gpp in gp.Positions)
            {
                if (gpp.Point < 0)
                    continue;

                MeasuredDataPosition mdp = new MeasuredDataPosition();
                string exportedName = name;
                if (!string.IsNullOrEmpty(gpp.Name))
                    exportedName = exportedName + " - " + gpp.Name;

                mdps.Add(MeasurementSerializationHelper.CollectPosition(exportedName, gp.PointList[gpp.Point], CalibrationHelper));
            }

            foreach (GenericPostureComputedPoint gpcp in gp.ComputedPoints)
            {
                string exportedName = name;
                if (!string.IsNullOrEmpty(gpcp.Name))
                    exportedName = exportedName + " - " + gpcp.Name;

                PointF p = gpcp.ComputeLocation(gp);
                mdps.Add(MeasurementSerializationHelper.CollectPosition(exportedName, p, CalibrationHelper));
            }

            return mdps;
        }

        public List<MeasuredDataDistance> CollectMeasuredDataDistances()
        {
            List<MeasuredDataDistance> mdds = new List<MeasuredDataDistance>();

            GenericPosture gp = genericPosture;
            foreach (GenericPostureDistance gpd in gp.Distances)
            {
                MeasuredDataDistance mdp = new MeasuredDataDistance();
                string exportedName = name;
                if (!string.IsNullOrEmpty(gpd.Name))
                    exportedName = exportedName + " - " + gpd.Name;

                PointF p1 = gp.PointList[gpd.Point1];
                PointF p2 = gp.PointList[gpd.Point2];
                mdds.Add(MeasurementSerializationHelper.CollectDistance(exportedName, p1, p2, CalibrationHelper));
            }

            return mdds;
        }
        public List<MeasuredDataAngle> CollectMeasuredDataAngles()
        {
            UpdateAngles();
            List<MeasuredDataAngle> mdas = new List<MeasuredDataAngle>();
            for (int i = 0; i < GenericPostureAngles.Count; i++)
            {
                GenericPostureAngle gpa = GenericPostureAngles[i];

                MeasuredDataAngle mda = new MeasuredDataAngle();
                string exportedName = name;
                if (!string.IsNullOrEmpty(gpa.Name))
                    exportedName = exportedName + " - " + gpa.Name;

                AngleHelper angleHelper = AngleHelpers[i];
                mdas.Add(MeasurementSerializationHelper.CollectAngle(exportedName, angleHelper, CalibrationHelper));
            }

            return mdas;
        }

        /// <summary>
        /// Returns the actual name associated with a point. 
        /// For simplicity the key used for trackable points is always derived from the point index.
        /// For spreadsheet export however we want to get the actual name of the point if it is declared in the file.
        /// </summary>
        public string GetTrackablePointName(string key)
        {
            int pointIndex = int.Parse(key);
            if (pointIndex >= genericPosture.Points.Count)
                return key;

            GenericPosturePoint point = genericPosture.Points[pointIndex];
            return string.IsNullOrEmpty(point.Name) ? key : point.Name;
        }
        #endregion

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            // The coordinates are defined in a reference image of 800x600 (could be inside the posture file).
            // We scale the positions and angle radius according to the actual image size.
            // We also translate the whole drawing so that the first point lies at the cursor.
            Size referenceSize = new Size(800, 600);

            float ratioWidth = (float)imageSize.Width / referenceSize.Width;
            float ratioHeight = (float)imageSize.Height / referenceSize.Height;
            float ratio = Math.Min(ratioWidth, ratioHeight);
            float dx = 0;
            float dy = 0;

            // Angles and circle radii need to be scaled every time.
            // For loading from KVA, this will restore the initial scaling from when the tool was first added.
            for (int i = 0; i < genericPosture.Angles.Count; i++)
            {
                genericPosture.Angles[i].Radius = (int)(genericPosture.Angles[i].Radius * ratio);
                genericPosture.Angles[i].TextDistance = (int)(genericPosture.Angles[i].TextDistance * ratio);
            }

            InitAngles();
            
            for (int i = 0; i < genericPosture.Circles.Count; i++)
                genericPosture.Circles[i].Radius = (int)(genericPosture.Circles[i].Radius * ratio);

            if (genericPosture.FromKVA)
                return;

            // Setup the initial point list.
            if (genericPosture.PointList.Count > 0)
            {
                PointF scaled = genericPosture.PointList[0].Scale(ratio, ratio);
                dx = origin.X - scaled.X;
                dy = origin.Y - scaled.Y;
            }

            for (int i = 0; i < genericPosture.PointList.Count; i++)
                genericPosture.PointList[i] = genericPosture.PointList[i].Scale(ratio, ratio).Translate(dx, dy);
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Color Color
        {
            get { return styleHelper.Bicolor.Background; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return genericPosture.CustomTrackingProfile; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return genericPosture.GetTrackablePoints();
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            genericPosture.SetTrackablePointValue(name, value, CalibrationHelper, TrackablePointMoved);
            this.trackingTimestamps = trackingTimestamps;
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
         
            genericPosture.SignalAllTrackablePointsMoved(TrackablePointMoved);
        }
        private void SignalTrackablePointMoved(int handle)
        {
            if (TrackablePointMoved == null || !genericPosture.Handles[handle].Trackable)
                return;

            genericPosture.SignalTrackablePointMoved(handle, TrackablePointMoved);
        }
        #endregion
        
        private void menuFlipHorizontal_Click(object sender, EventArgs e)
        {
            genericPosture.FlipHorizontal();

            foreach (var angle in genericPosture.Angles)
                angle.CCW = !angle.CCW;

            SignalAllTrackablePointsMoved();
            InvalidateFromMenu(sender);
        }
        private void menuFlipVertical_Click(object sender, EventArgs e)
        {
            genericPosture.FlipVertical();

            foreach (var angle in genericPosture.Angles)
                angle.CCW = !angle.CCW;

            SignalAllTrackablePointsMoved();
            InvalidateFromMenu(sender);
        }
        
        #region Drawing helpers
        private void DrawComputedPoints(Pen penEdge, Color basePenEdgeColor, SolidBrush brushHandle, Color baseBrushHandleColor, int alpha, double opacity, Graphics canvas, IImageToViewportTransformer transformer)
        {
            penEdge.Width = 2;
            
            foreach(GenericPostureComputedPoint computedPoint in genericPosture.ComputedPoints)
            {
                if(!IsActive(computedPoint.OptionGroup))
                    continue;
                    
                PointF p = computedPoint.ComputeLocation(genericPosture);
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
                
                if ((PreferencesManager.PlayerPreferences.EnableCustomToolsDebugMode)  && !string.IsNullOrEmpty(computedPoint.Name))
                    DrawDebugText(p2, debugOffset, computedPoint.Name, canvas, opacity, transformer);
            }
            
            brushHandle.Color = baseBrushHandleColor;
            penEdge.Color = basePenEdgeColor;
            penEdge.Width = 1;
        }
        private void DrawSegments(Pen penEdge, Color basePenEdgeColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureSegment segment in genericPosture.Segments)
            {
                if(!IsActive(segment.OptionGroup))
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

                if (segment.ArrowBegin)
                    ArrowHelper.Draw(canvas, penEdge, start.ToPoint(), end.ToPoint());

                if (segment.ArrowEnd)
                    ArrowHelper.Draw(canvas, penEdge, end.ToPoint(), start.ToPoint());
            }
            
            penEdge.Color = basePenEdgeColor;
            penEdge.StartCap = LineCap.NoAnchor;
            penEdge.EndCap = LineCap.NoAnchor;
        }
        private void DrawCircles(Pen penEdge, Color basePenEdgeColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureCircle circle in genericPosture.Circles)
            {
                if(!IsActive(circle.OptionGroup))
                    continue;
                    
                penEdge.Width = circle.Width;
                penEdge.DashStyle = Convert(circle.Style);
                penEdge.Color = circle.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, circle.Color);
                
                PointF center = circle.Center >= 0 ? points[circle.Center] : GetComputedPoint(circle.Center, transformer);
                
                int radius = transformer.Transform(circle.Radius);
                canvas.DrawEllipse(penEdge, center.Box(radius));
            }
            
            penEdge.Color = basePenEdgeColor;
        }
        private void DrawPoints(SolidBrush brushHandle, Color baseBrushHandleColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            // Points are by default invisible unless there is a handle on them.
            foreach(GenericPostureHandle handle in genericPosture.Handles)
            {
                if(!IsActive(handle.OptionGroup))
                    continue;
                    
                if(handle.Type == HandleType.Point && handle.Reference >= 0 && handle.Reference < points.Count && handle.Reference < genericPosture.Points.Count)
                {
                    GenericPosturePoint gpp = genericPosture.Points[handle.Reference];
                    if (gpp == null)
                        continue;

                    brushHandle.Color = baseBrushHandleColor;
                    if (gpp.Color != Color.Transparent)
                        brushHandle.Color = Color.FromArgb(alpha, gpp.Color);
                    else if (handle.Color != Color.Transparent)
                        brushHandle.Color = Color.FromArgb(alpha, handle.Color);

                    canvas.FillEllipse(brushHandle, points[handle.Reference].Box(3));
                }
            }
            
            brushHandle.Color = baseBrushHandleColor;
        }
        private void DrawAngles(Pen penEdge, Color basePenEdgeColor, SolidBrush brushFill, Color baseBrushFillColor, int alpha, int alphaBackground, double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            UpdateAngles();
            
            penEdge.Width = 2;
            penEdge.DashStyle = DashStyle.Solid;
            
            for(int i = 0; i<angles.Count; i++)
            {
                if(!IsActive(genericPosture.Angles[i].OptionGroup))
                    continue;
                
                AngleHelper angleHelper = angles[i];
                Rectangle box = transformer.Transform(angleHelper.SweepAngle.BoundingBox);
                Color color = genericPosture.Angles[i].Color;

                try
                {
                    penEdge.Color = color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, color);
                    brushFill.Color = color == Color.Transparent ? baseBrushFillColor : Color.FromArgb(alphaBackground, color);
                    
                    canvas.FillPie(brushFill, box, angleHelper.SweepAngle.Start, angleHelper.SweepAngle.Sweep);
                    canvas.DrawArc(penEdge, box, angleHelper.SweepAngle.Start, angleHelper.SweepAngle.Sweep);
                }
                catch(Exception e)
                {
                    log.DebugFormat(e.ToString());
                }

                Point origin = transformer.Transform(angleHelper.SweepAngle.Origin);

                if (!PreferencesManager.PlayerPreferences.EnableCustomToolsDebugMode)
                {
                    angleHelper.DrawText(canvas, opacity, brushFill, origin, transformer, CalibrationHelper, styleHelper);
                }
                else
                {
                    GenericPostureAngle gpa = genericPosture.Angles[i];
                    
                    float value = CalibrationHelper.ConvertAngle(angleHelper.CalibratedAngle);
                    string debugLabel = string.Format("A{0} [P{1}, P{2}, P{3}]\n", i, gpa.Leg1, gpa.Origin, gpa.Leg2);
                    if (!string.IsNullOrEmpty(gpa.Name))
                        debugLabel += string.Format("Name:{0}\n", gpa.Name);

                    debugLabel += string.Format("Signed:{0}\n", gpa.Signed);
                    debugLabel += string.Format("CCW:{0}\n", gpa.CCW);
                    debugLabel += string.Format("Supplementary:{0}\n", gpa.Supplementary);
                    debugLabel += string.Format("Value: {0:0.0} {1}", value, CalibrationHelper.GetAngleAbbreviation());
                    
                    SizeF debugLabelSize = canvas.MeasureString(debugLabel, debugFont);
                    int debugLabelDistance = (int)debugOffset.X * 8;
                    PointF debugLabelPositionRelative = angleHelper.GetTextPosition(debugLabelDistance, debugLabelSize);
                    debugLabelPositionRelative = debugLabelPositionRelative.Scale((float)transformer.Scale);
                    PointF debugLabelPosition = new PointF(origin.X + debugLabelPositionRelative.X, origin.Y + debugLabelPositionRelative.Y);
                    
                    RectangleF backRectangle = new RectangleF(debugLabelPosition, debugLabelSize);
                    int roundingRadius = (int)(debugFont.Height * 0.25f);
                    RoundedRectangle.Draw(canvas, backRectangle, debugBrush, roundingRadius, false, false, null);
                    canvas.DrawString(debugLabel, debugFont, Brushes.White, backRectangle.Location);
                }
            }
            
            brushFill.Color = baseBrushFillColor;
            penEdge.Width = 1;
            penEdge.Color = basePenEdgeColor;
        }

        private void DrawPolylines(Pen penEdge, Color basePenEdgeColor, int alpha, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach (GenericPosturePolyline polyline in genericPosture.Polylines)
            {
                if (!IsActive(polyline.OptionGroup))
                    continue;

                penEdge.Width = polyline.Width;
                penEdge.DashStyle = Convert(polyline.Style);
                penEdge.Color = polyline.Color == Color.Transparent ? basePenEdgeColor : Color.FromArgb(alpha, polyline.Color);

                PointF[] path = polyline.Points.Select(i => i >= 0 ? points[i] : GetComputedPoint(i, transformer)).ToArray();
                canvas.DrawCurve(penEdge, path);
            }

            penEdge.Color = basePenEdgeColor;
            penEdge.StartCap = LineCap.NoAnchor;
            penEdge.EndCap = LineCap.NoAnchor;
        }
        private void DrawDistances(SolidBrush brushFill, Color baseBrushFillColor, int alphaBackground, double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            foreach(GenericPostureDistance distance in genericPosture.Distances)
            {
                if(!IsActive(distance.OptionGroup))
                    continue;
                
                PointF untransformedA = distance.Point1 >= 0 ? genericPosture.PointList[distance.Point1] : GetUntransformedComputedPoint(distance.Point1);
                PointF untransformedB = distance.Point2 >= 0 ? genericPosture.PointList[distance.Point2] : GetUntransformedComputedPoint(distance.Point2);
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
            foreach(GenericPosturePosition position in genericPosture.Positions)
            {
                if(!IsActive(position.OptionGroup))
                    continue;
                
                PointF untransformedP = position.Point >= 0 ? genericPosture.PointList[position.Point] : GetUntransformedComputedPoint(position.Point);
                string label = CalibrationHelper.GetPointText(untransformedP, true, true, infosFading.ReferenceTimestamp);
                
                if(!string.IsNullOrEmpty(position.Symbol))
                    label = string.Format("{0} = {1}", position.Symbol, label);
                
                PointF p = position.Point >= 0 ? points[position.Point] : GetComputedPoint(position.Point, transformer);
                
                brushFill.Color = position.Color == Color.Transparent ? baseBrushFillColor : Color.FromArgb(alphaBackground, position.Color);
                DrawPointText(p, label, canvas, opacity, transformer, brushFill);
            }
            
            brushFill.Color = baseBrushFillColor;
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
            Font tempFont = styleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
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
        private void DrawDebugText(PointF location, PointF offset, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer)
        {
            SizeF labelSize = canvas.MeasureString(label, debugFont);
            PointF textOrigin = new PointF(location.X - (labelSize.Width / 2) + offset.X, location.Y - (labelSize.Height / 2) + offset.Y);
            
            RectangleF backRectangle = new RectangleF(textOrigin, labelSize);
            int roundingRadius = (int)(debugFont.Height * 0.25f);
            RoundedRectangle.Draw(canvas, backRectangle, debugBrush, roundingRadius, false, false, null);
            
            canvas.DrawString(label, debugFont, Brushes.White, backRectangle.Location);
        }

        private void DrawSimpleText(PointF location, string label, Graphics canvas, double opacity, IImageToViewportTransformer transformer, SolidBrush brush)
        {
            Font tempFont = styleHelper.GetFont(Math.Max((float)transformer.Scale, 1.0F));
            SizeF labelSize = canvas.MeasureString(label, tempFont);
            PointF textOrigin = new PointF(location.X - labelSize.Width / 2, location.Y - labelSize.Height / 2);
            canvas.DrawString(label, tempFont, brush, textOrigin);
            tempFont.Dispose();
        }
        private void DrawDebug(double opacity, Graphics canvas, IImageToViewportTransformer transformer, List<Point> points)
        {
            // Note: some of the labels are drawn during the normal method with an extra test there.
            PointF offset = new PointF(10, 10);

            // Points id.
            for (int i = 0; i < genericPosture.PointList.Count; i++)
            {
                string label = string.Format("P{0}", i);
                PointF p = points[i];
                DrawDebugText(p, offset, label, canvas, opacity, transformer);
            }

            // Segments id and name.
            for (int i = 0; i < genericPosture.Segments.Count; i++)
            {
                GenericPostureSegment segment = genericPosture.Segments[i];
                if (segment.Start < 0 || segment.End < 0)
                    continue;

                PointF start = points[segment.Start];
                PointF end = points[segment.End];
                PointF middle = GeometryHelper.GetMiddlePoint(start, end);
                
                string label = "";
                if (!string.IsNullOrEmpty(segment.Name))
                    label = string.Format("S{0} [P{1}, P{2}]: {3}", i, segment.Start, segment.End, segment.Name);
                else
                    label = string.Format("S{0} [P{1}, P{2}]", i, segment.Start, segment.End);

                DrawDebugText(middle, offset, label, canvas, opacity, transformer);
            }
            
        }
        #endregion

        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
        }
        #endregion

        #region Lower level helpers
        private void InitOptionMenus()
        {
            // Options
            if(genericPosture == null || genericPosture.Options == null || genericPosture.Options.Count == 0)
                return;
            
            foreach(GenericPostureOption option in genericPosture.Options.Values)
            {
                if (option.Hidden)
                    continue;

                ToolStripMenuItem menu = new ToolStripMenuItem();
                menu.Text = option.Label;
                menu.Checked = option.Value;

                string closureOption = option.Key;
                menu.Click += (s, e) => {
                    genericPosture.Options[closureOption].Value = !genericPosture.Options[closureOption].Value;
                    menu.Checked = genericPosture.Options[closureOption].Value;
                    InvalidateFromMenu(s);
                };
                
                menuOptions.DropDownItems.Add(menu);
            }
            
            menuOptions.Image = Properties.Drawings.eye;
        }
        private void InitAngles()
        {
            angles.Clear();
            for (int i = 0; i < genericPosture.Angles.Count; i++)
                angles.Add(new AngleHelper(genericPosture.Angles[i].TextDistance, genericPosture.Angles[i].Radius, genericPosture.Angles[i].Tenth, genericPosture.Angles[i].Symbol));
        }
        private void UpdateAngles()
        {
            for(int i = 0; i<angles.Count;i++)
            {
                PointF origin = genericPosture.PointList[genericPosture.Angles[i].Origin];
                PointF leg1 = genericPosture.PointList[genericPosture.Angles[i].Leg1];
                PointF leg2 = genericPosture.PointList[genericPosture.Angles[i].Leg2];

                bool signed = genericPosture.Angles[i].Signed;
                bool ccw = genericPosture.Angles[i].CCW;
                bool supplementary = genericPosture.Angles[i].Supplementary;
                
                angles[i].Update(origin, leg1, leg2, signed, ccw, supplementary, CalibrationHelper);
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
            style.Bind(styleHelper, "Bicolor", "line color");
        }
        
        private bool IsActive(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            string[] keys = value.Split(new char[] { '|' });

            // We only implement the "AND" logic at the moment:
            // in case of multiple options on the object, they all need to be active for the object to be active.
            bool active = true;
            foreach (string key in keys)
            {
                if (!genericPosture.Options.ContainsKey(key))
                    continue;

                if (!genericPosture.Options[key].Value)
                {
                    active = false;
                    break;
                }
            }

            return active;
        }
        private PointF GetComputedPoint(int index, IImageToViewportTransformer transformer)
        {
            PointF result = PointF.Empty;
            
            int computedPointIndex = - index - 1;
            if(computedPointIndex < genericPosture.ComputedPoints.Count)
                result = genericPosture.ComputedPoints[computedPointIndex].LastPoint;
            
            return transformer.Transform(result);
        }
        private PointF GetUntransformedComputedPoint(int index)
        {
            PointF result = PointF.Empty;
            
            int computedPointIndex = - index - 1;
            if(computedPointIndex < genericPosture.ComputedPoints.Count)
                result = genericPosture.ComputedPoints[computedPointIndex].LastPoint;
            
            return result;
        }
        private bool IsPointInObject(PointF point, IImageToViewportTransformer transformer)
        {
            // Angles, hit zones, segments.
            
            bool hit = false;
            foreach(AngleHelper angle in angles)
            {
                hit = angle.SweepAngle.Hit(point);
                if(hit)
                    break;
            }
            
            if (hit)
                return true;
            
            foreach(GenericPostureAbstractHitZone hitZone in genericPosture.HitZones)
            {
                hit = IsPointInHitZone(hitZone, point);
                if(hit)
                    break;
            }
                
            if (hit)
                return true;
            
            foreach(GenericPostureCircle circle in genericPosture.Circles)
            {
                hit = IsPointInsideCircle(circle, point, transformer);
                if(hit)
                    break;
            }
            
            if (hit)
                return true;
            
            foreach(GenericPostureSegment segment in genericPosture.Segments)
            {
                hit = IsPointOnSegment(segment, point, transformer);
                if(hit)
                    break;
            }

            if (hit)
                return true;

            foreach (GenericPosturePolyline polyline in genericPosture.Polylines)
            {
                hit = IsPointOnPolyline(polyline, point, transformer);
                if (hit)
                    break;
            }
            
            return hit;
        }
        private bool IsPointInHitZone(GenericPostureAbstractHitZone hitZone, PointF point)
        {
            bool hit = false;
            
            switch(hitZone.Type)
            {
                case HitZoneType.Polygon:
                {
                    GenericPostureHitZonePolygon hitPolygon = hitZone as GenericPostureHitZonePolygon;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        List<PointF> points = new List<PointF>();
                        foreach(int pointRef in hitPolygon.Points)
                            points.Add(genericPosture.PointList[pointRef]);
    
                        path.AddPolygon(points.ToArray());
                        using (Region region = new Region(path))
                        {
                            hit = region.IsVisible(point);
                        }
                    }
                    break;
                }
            }
            
            return hit;
        }
        private bool IsPointOnSegment(GenericPostureSegment segment, PointF point, IImageToViewportTransformer transformer)
        {
            PointF start = segment.Start >= 0 ? genericPosture.PointList[segment.Start] : GetUntransformedComputedPoint(segment.Start);
            PointF end = segment.End >= 0 ? genericPosture.PointList[segment.End] : GetUntransformedComputedPoint(segment.End);
            
            if(start == end)
                return false;
            
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddLine(start, end);
                return HitTester.HitTest(path, point, segment.Width, false, transformer);
            }
        }
        private bool IsPointOnPolyline(GenericPosturePolyline polyline, PointF point, IImageToViewportTransformer transformer)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                PointF[] points = polyline.Points.Select(i => genericPosture.PointList[i]).ToArray();
                path.AddCurve(points);
                return HitTester.HitTest(path, point, polyline.Width, false, transformer);
            }
        }
        private bool IsPointInsideCircle(GenericPostureCircle circle, PointF point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                PointF center = circle.Center >= 0 ? genericPosture.PointList[circle.Center] : GetUntransformedComputedPoint(circle.Center);
                path.AddEllipse(center.Box(circle.Radius));
                return HitTester.HitTest(path, point, 0, true, transformer);
            }
        }
        private bool IsPointOnArc(GenericPostureCircle circle, PointF point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                PointF center = circle.Center >= 0 ? genericPosture.PointList[circle.Center] : GetUntransformedComputedPoint(circle.Center);
                path.AddArc(center.Box(circle.Radius), 0, 360);
                return HitTester.HitTest(path, point, circle.Width, false, transformer);
            }
        }
        #endregion
    }
}