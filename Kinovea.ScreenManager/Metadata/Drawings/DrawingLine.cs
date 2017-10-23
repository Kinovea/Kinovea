#region License
/*
Copyright � Joan Charmant 2008-2011.
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
using System.ComponentModel;
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
    [XmlType ("Line")]
    public class DrawingLine : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, ITrackable, IMeasurable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        public PointF A
        {
            get { return points["a"]; }
        }
        public PointF B
        {
            get { return points["b"]; }
        }
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolLine2D; }
        }
        public override int ContentHash
        {
            get 
            {
                int iHash = 0;
                iHash ^= styleHelper.ContentHash;
                iHash ^= ShowMeasurableInfo.GetHashCode();
                iHash ^= infosFading.ContentHash;
                return iHash;
            }
        }
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                // Rebuild the menu to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                mnuShowMeasure.Text = ScreenManagerLang.mnuShowMeasure;
                mnuShowMeasure.Checked = ShowMeasurableInfo;
                mnuSealMeasure.Text = ScreenManagerLang.mnuCalibrate;
                
                contextMenu.Add(mnuShowMeasure);
                contextMenu.Add(mnuSealMeasure);
                
                return contextMenu; 
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool tracking;
        private bool initializing = true;
        
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private KeyframeLabel labelMeasure;
        private InfosFading infosFading;
        
        // Context menu
        private ToolStripMenuItem mnuShowMeasure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSealMeasure = new ToolStripMenuItem();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingLine(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            points["a"] = origin;
            points["b"] = origin.Translate(10, 0);
            labelMeasure = new KeyframeLabel(GetMiddlePoint(), Color.Black, transformer);

            styleHelper.Color = Color.DarkSlateGray;
            styleHelper.LineSize = 1;

            if (preset == null)
                preset = ToolManager.GetStylePreset("Line");
            
            style = preset.Clone();
            BindStyle();
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            
            // Context menu
            mnuShowMeasure.Click += mnuShowMeasure_Click;
            mnuShowMeasure.Image = Properties.Drawings.measure;
            mnuSealMeasure.Click += mnuSealMeasure_Click;
            mnuSealMeasure.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingLine(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            
            if(tracking)
                opacityFactor = 1.0;
            
            if(opacityFactor <= 0)
                return;

            Point start = transformer.Transform(points["a"]);
            Point end = transformer.Transform(points["b"]);
            
            using(Pen penEdges = styleHelper.GetPen((int)(opacityFactor * 255), transformer.Scale))
            {
                if (distorter != null && distorter.Initialized)
                    DrawDistorted(canvas, distorter, transformer, penEdges, start, end);
                else
                    DrawStraight(canvas, transformer, penEdges, start, end);
            }

            if(ShowMeasurableInfo)
            {
                // Text of the measure. (The helpers knows the unit)
                PointF a = new PointF(points["a"].X, points["a"].Y);
                PointF b = new PointF(points["b"].X, points["b"].Y);
                labelMeasure.SetText(CalibrationHelper.GetLengthText(a, b, true, true));
                labelMeasure.Draw(canvas, transformer, opacityFactor);
            }
        }
        private void DrawDistorted(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, Pen penEdges, Point start, Point end)
        {
            List<PointF> curve = distorter.DistortLine(points["a"], points["b"]);
            List<Point> transformedCurve = transformer.Transform(curve);

            if (styleHelper.LineShape == LineShape.Squiggle)
            {
                canvas.DrawSquigglyLine(penEdges, start, end);
            }
            else if (styleHelper.LineShape == LineShape.Dash)
            {
                DashStyle oldDashStyle = penEdges.DashStyle;
                penEdges.DashStyle = DashStyle.Dash;
                canvas.DrawCurve(penEdges, transformedCurve.ToArray());
                penEdges.DashStyle = oldDashStyle;
            }
            else
            {
                canvas.DrawCurve(penEdges, transformedCurve.ToArray());
            }

            labelMeasure.SetAttach(curve[curve.Count / 2], true);
        }
        private void DrawStraight(Graphics canvas, IImageToViewportTransformer transformer, Pen penEdges, Point start, Point end)
        {
            if (styleHelper.LineShape == LineShape.Squiggle)
            {
                canvas.DrawSquigglyLine(penEdges, start, end);
            }
            else if (styleHelper.LineShape == LineShape.Dash)
            {
                penEdges.DashStyle = DashStyle.Dash;
                canvas.DrawLine(penEdges, start, end);
            }
            else
            {
                canvas.DrawLine(penEdges, start, end);
            }

            labelMeasure.SetAttach(GetMiddlePoint(), true);

            if (styleHelper.LineEnding == LineEnding.StartArrow || styleHelper.LineEnding == LineEnding.DoubleArrow)
                ArrowHelper.Draw(canvas, penEdges, start, end);

            if (styleHelper.LineEnding == LineEnding.EndArrow || styleHelper.LineEnding == LineEnding.DoubleArrow)
                ArrowHelper.Draw(canvas, penEdges, end, start);
        }

        

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            
            if (tracking || opacity > 0)
            {
                if(ShowMeasurableInfo && labelMeasure.HitTest(point, transformer))
                    result = 3;
                else if (HitTester.HitTest(points["a"], point, transformer))
                    result = 1;
                else if (HitTester.HitTest(points["b"], point, transformer))
                    result = 2;
                else if (IsPointInObject(point, distorter, transformer))
                    result = 0;
            }
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            int constraintAngleSubdivisions = 8; // (Constraint by 45� steps).
            switch(handleNumber)
            {
                case 1:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["a"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["b"], point, constraintAngleSubdivisions);
                    else
                        points["a"] = point;

                    SignalTrackablePointMoved("a");
                    break;
                case 2:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["b"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["a"], point, constraintAngleSubdivisions);
                    else
                        points["b"] = point;

                    SignalTrackablePointMoved("b");
                    break;
                case 3:
                    // Move the center of the mini label to the mouse coord.
                    labelMeasure.SetLabel(point);
                    break;
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            points["a"] = points["a"].Translate(dx, dy);
            points["b"] = points["b"].Translate(dx, dy);
            SignalAllTrackablePointsMoved();
        }
        #endregion

        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            if (xmlReader.MoveToAttribute("name"))
                name = xmlReader.ReadContentAsString();

            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                switch(xmlReader.Name)
                {
                    case "Start":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            points["a"] = p.Scale(scale.X, scale.Y);
                            break;
                        }
                    case "End":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            points["b"] = p.Scale(scale.X, scale.Y);
                            break;
                        }
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
                        BindStyle();
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(xmlReader);
                        break;
                    case "MeasureVisible":
                        ShowMeasurableInfo = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
            initializing = false;
            
            SignalAllTrackablePointsMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Start", XmlHelper.WritePointF(points["a"]));
                w.WriteElementString("End", XmlHelper.WritePointF(points["b"]));
                w.WriteElementString("MeasureVisible", ShowMeasurableInfo.ToString().ToLower());
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
            
            
            if(ShouldSerializeAll(filter))
            {
                // Spreadsheet support.
                w.WriteStartElement("Measure");
                
                PointF a = CalibrationHelper.GetPoint(new PointF(points["a"].X, points["a"].Y));
                PointF b = CalibrationHelper.GetPoint(new PointF(points["b"].X, points["b"].Y));

                float len = GeometryHelper.GetDistance(a, b);
                string value = String.Format("{0:0.00}", len);
                string valueInvariant = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", len);

                w.WriteAttributeString("UserLength", value);
                w.WriteAttributeString("UserLengthInvariant", valueInvariant);
                w.WriteAttributeString("UserUnitLength", CalibrationHelper.GetLengthAbbreviation());
                
                w.WriteEndElement();
            }
        }
        #endregion
        
        #region IInitializable implementation
        public void InitializeMove(PointF point, Keys modifiers)
        {
            MoveHandle(point, 2, modifiers);
        }
        public string InitializeCommit(PointF point)
        {
            initializing = false;
            return null;
        }
        public string InitializeEnd(bool cancelCurrentPoint)
        {
            return null;
        }
        #endregion
        
        #region ITrackable implementation and support.
        public Color Color
        {
            get { return styleHelper.Color; }
        }
        public TrackingProfile CustomTrackingProfile
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, PointF value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            foreach(KeyValuePair<string, PointF> p in points)
                TrackablePointMoved(this, new TrackablePointMovedEventArgs(p.Key, p.Value));
        }
        private void SignalTrackablePointMoved(string name)
        {
            if(TrackablePointMoved == null || !points.ContainsKey(name))
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs(name, points[name]));
        }
        #endregion
        
        #region Context menu
        private void mnuShowMeasure_Click(object sender, EventArgs e)
        {
            // Enable / disable the display of the measure for this line.
            ShowMeasurableInfo = !ShowMeasurableInfo;
            if(ShowMeasurableInfoChanged != null)
                ShowMeasurableInfoChanged(this, EventArgs.Empty);
            
            InvalidateFromMenu(sender);
        }
        
        private void mnuSealMeasure_Click(object sender, EventArgs e)
        {
            if(points["a"].NearlyCoincideWith(points["b"]))
                return;
            
            if(!ShowMeasurableInfo)
            {
                ShowMeasurableInfo = true;
                if(ShowMeasurableInfoChanged != null)
                    ShowMeasurableInfoChanged(this, EventArgs.Empty);
            }
            
            FormCalibrateLine fcm = new FormCalibrateLine(CalibrationHelper, this);
            FormsHelper.Locate(fcm);
            fcm.ShowDialog();
            fcm.Dispose();
            
            InvalidateFromMenu(sender);
        }
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
            style.Bind(styleHelper, "LineShape", "line shape");
            style.Bind(styleHelper, "LineEnding", "arrows");
        }
        private bool IsPointInObject(PointF point, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                if (points["a"].NearlyCoincideWith(points["b"]))
                {
                    areaPath.AddLine(points["a"].X, points["a"].Y, points["a"].X + 2, points["a"].Y + 2);
                }
                else
                {
                    if (distorter != null && distorter.Initialized)
                    {
                        List<PointF> curve = distorter.DistortLine(points["a"], points["b"]);
                        areaPath.AddCurve(curve.ToArray());
                    }
                    else
                    {
                        areaPath.AddLine(points["a"], points["b"]);
                    }
                }

                return HitTester.HitTest(areaPath, point, styleHelper.LineSize, false, transformer);
            }
        }
        private PointF GetMiddlePoint()
        {
            // Used only to attach the measure.
            return GeometryHelper.GetMiddlePoint(points["a"], points["b"]);
        }
        
        #endregion
    }
}
