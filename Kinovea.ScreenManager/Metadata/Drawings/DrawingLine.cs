#region License
/*
Copyright � Joan Charmant 2008-2011.
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
        public event EventHandler<EventArgs<MeasureLabelType>> ShowMeasurableInfoChanged;
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
                int hash = 0;
                hash ^= measureLabelType.GetHashCode();
                hash ^= miniLabel.GetHashCode();
                hash ^= styleData.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
            }
        }
        public StyleElements StyleElements
        {
            get { return styleElements;}
        }
        public override InfosFading InfosFading
        {
            get { return infosFading; }
            set { infosFading = value; }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                // Rebuild the menu to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuCalibrate,
                    mnuMeasurement,
                });

                return contextMenu; 
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        
        public CalibrationHelper CalibrationHelper { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private long trackingTimestamps = -1;
        private bool initializing = true;
        private bool measureInitialized;

        // Decoration
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private MiniLabel miniLabel = new MiniLabel();
        private MeasureLabelType measureLabelType = MeasureLabelType.None;
        private InfosFading infosFading;

        #region Menus
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();
        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();
        #endregion
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingLine(PointF origin, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            points["a"] = origin;
            points["b"] = origin.Translate(10, 0);
            miniLabel.SetAttach(GetMiddlePoint(), true);

            SetupStyle(preset);

            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            InitializeMenus();
        }
        
        public DrawingLine(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }

        private void InitializeMenus()
        {
            // Measurement.    
            mnuMeasurement.Image = Properties.Drawings.label;
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.AddRange(new ToolStripItem[] {
                CreateMeasureLabelTypeMenu(MeasureLabelType.None),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Name),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.TravelDistance),
            });

            // Calibrate
            mnuCalibrate.Image = Properties.Drawings.coordinates_graduations;
            mnuCalibrate.Click += mnuCalibrate_Click;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if(opacityFactor <= 0)
                return;

            PointF start = transformer.Transform(points["a"]);
            PointF end = transformer.Transform(points["b"]);

            using (Pen penEdges = styleData.GetPen(opacityFactor, transformer.Scale))
            using (Brush brush = styleData.GetBrush(opacityFactor))
            {
                if (distorter != null && distorter.Initialized && styleData.LineShape != LineShape.Squiggle)
                    DrawDistorted(canvas, distorter, transformer, penEdges, brush, start, end);
                else
                    DrawStraight(canvas, transformer, penEdges, brush, start, end);
            }

            if(measureLabelType != MeasureLabelType.None)
            {
                string text = GetMeasureLabelText();
                miniLabel.SetText(text, transformer);
                miniLabel.Draw(canvas, transformer, opacityFactor);
            }
        }
        private void DrawDistorted(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, Pen penEdges, Brush brush, PointF start, PointF end)
        {
            List<PointF> curve = distorter.DistortLine(points["a"], points["b"]);
            List<Point> transformedCurve = transformer.Transform(curve);

            PointF arrowOffsetStart = ArrowHelper.GetOffset(penEdges.Width, start, transformedCurve[1]);
            PointF arrowOffsetEnd = ArrowHelper.GetOffset(penEdges.Width, end, transformedCurve[transformedCurve.Count - 2]);
            float offsetLength = Math.Max(new Vector(arrowOffsetStart.X, arrowOffsetStart.Y).Norm(), new Vector(arrowOffsetEnd.X, arrowOffsetEnd.Y).Norm());
            float lineLength = GeometryHelper.GetDistance(start, end);
            bool canDrawArrow = lineLength > offsetLength;

            if (canDrawArrow)
            {
                if (styleData.LineEnding == LineEnding.StartArrow || styleData.LineEnding == LineEnding.DoubleArrow)
                {
                    start = new PointF(start.X + arrowOffsetStart.X, start.Y + arrowOffsetStart.Y).ToPoint();
                    transformedCurve[0] = start.ToPoint();
                }

                if (styleData.LineEnding == LineEnding.EndArrow || styleData.LineEnding == LineEnding.DoubleArrow)
                {
                    end = new PointF(end.X + arrowOffsetEnd.X, end.Y + arrowOffsetEnd.Y).ToPoint();
                    transformedCurve[transformedCurve.Count - 1] = end.ToPoint();
                }
            }

            if (styleData.LineShape == LineShape.Dash)
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

            miniLabel.SetAttach(curve[curve.Count / 2], true);

            if (canDrawArrow)
            {
                if (styleData.LineEnding == LineEnding.StartArrow || styleData.LineEnding == LineEnding.DoubleArrow)
                    ArrowHelper.Draw(canvas, penEdges, start, transformedCurve[1]);

                if (styleData.LineEnding == LineEnding.EndArrow || styleData.LineEnding == LineEnding.DoubleArrow)
                    ArrowHelper.Draw(canvas, penEdges, end, transformedCurve[transformedCurve.Count - 2]);
            }
        }
        private void DrawStraight(Graphics canvas, IImageToViewportTransformer transformer, Pen penEdges, Brush brush, PointF start, PointF end)
        {
            bool startArrow = styleData.LineEnding == LineEnding.StartArrow  || styleData.LineEnding == LineEnding.DoubleArrow;
            bool endArrow = styleData.LineEnding == LineEnding.EndArrow || styleData.LineEnding == LineEnding.DoubleArrow;
            bool canDrawArrow = ArrowHelper.UpdateStartEnd(penEdges.Width, ref start, ref end, startArrow, endArrow);
            
            if (styleData.LineShape == LineShape.Squiggle)
            {
                canvas.DrawSquigglyLine(penEdges, start, end);
            }
            else if (styleData.LineShape == LineShape.Dash)
            {
                penEdges.DashStyle = DashStyle.Dash;
                canvas.DrawLine(penEdges, start, end);
            }
            else
            {
                canvas.DrawLine(penEdges, start, end);
            }

            miniLabel.SetAttach(GetMiddlePoint(), true);

            if (canDrawArrow)
            {
                if (startArrow)
                    ArrowHelper.Draw(canvas, penEdges, start, end);

                if (endArrow)
                    ArrowHelper.Draw(canvas, penEdges, end, start);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            int result = -1;
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacityFactor > 0)
            {
                if(measureLabelType != MeasureLabelType.None && miniLabel.HitTest(point))
                    result = 3;
                else if (HitTester.HitPoint(point, points["a"], transformer))
                    result = 1;
                else if (HitTester.HitPoint(point, points["b"], transformer))
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
                    miniLabel.SetCenter(point);
                    break;
            }

            if (CalibrationHelper != null)
                CalibrationHelper.CalibrationByLine_Update(Id, points["a"], points["b"]);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            points["a"] = points["a"].Translate(dx, dy);
            points["b"] = points["b"].Translate(dx, dy);

            if (CalibrationHelper != null)
                CalibrationHelper.CalibrationByLine_Update(Id, points["a"], points["b"]);

            SignalAllTrackablePointsMoved();
        }
        public override PointF GetCopyPoint()
        {
            return GetMiddlePoint();
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
                    case "ReferenceTimestamp":
                        {
                            referenceTimestamp = XmlHelper.ParseTimestamp(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "ExtraData":
                        {
                            measureLabelType = XmlHelper.ParseEnum<MeasureLabelType>(xmlReader.ReadElementContentAsString(), MeasureLabelType.None);
                            break;
                        }
                    case "MeasureLabel":
                        {
                            miniLabel = new MiniLabel(xmlReader, scale);
                            break;
                        }
                    case "DrawingStyle":
                        styleElements.ImportXML(xmlReader);
                        BindStyle();
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(xmlReader);
                        break;
                    case "Measure":
                        xmlReader.ReadOuterXml();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
            initializing = false;
            measureInitialized = true;
            miniLabel.SetAttach(GetMiddlePoint(), false);
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
            SignalAllTrackablePointsMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                // In case of tracking or camera motion, the current time at the moment of the write call
                // is not necessarily the reference time of the drawing (last manual placement),
                // and the points coordinates are those based off the last track step (video time change).
                // Get the right values at the reference time.
                PointF a = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "a");
                PointF b = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "b");
                w.WriteElementString("Start", XmlHelper.WritePointF(a));
                w.WriteElementString("End", XmlHelper.WritePointF(b));
                w.WriteElementString("ReferenceTimestamp", XmlHelper.WriteTimestamp(referenceTimestamp));

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
                string xmlMeasureLabelType = enumConverter.ConvertToString(measureLabelType);
                w.WriteElementString("ExtraData", xmlMeasureLabelType);

                w.WriteStartElement("MeasureLabel");
                miniLabel.WriteXml(w, GeometryHelper.GetMiddlePoint(a, b));
                w.WriteEndElement();
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                styleElements.WriteXml(w);
                w.WriteEndElement();
            }

            if (ShouldSerializeFading(filter))
            {
                w.WriteStartElement("InfosFading");
                infosFading.WriteXml(w);
                w.WriteEndElement();
            }
        }
        public MeasuredDataDistance CollectMeasuredData()
        {
            return MeasurementSerializationHelper.CollectDistance(name, points["a"], points["b"], CalibrationHelper);
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
            get { return styleData.Color; }
        }
        public TrackingParameters CustomTrackingParameters
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            return points;
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            this.trackingTimestamps = trackingTimestamps;

            if (CalibrationHelper != null)
                CalibrationHelper.CalibrationByLine_Update(Id, points["a"], points["b"]);
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
        private void mnuCalibrate_Click(object sender, EventArgs e)
        {
            if(points["a"].NearlyCoincideWith(points["b"]))
                return;
            
            if (measureLabelType == MeasureLabelType.None)
            {
                measureLabelType = MeasureLabelType.TravelDistance;
                if (ShowMeasurableInfoChanged != null)
                    ShowMeasurableInfoChanged(this, new EventArgs<MeasureLabelType>(measureLabelType));
            }
            
            FormCalibrateLine fcm = new FormCalibrateLine(CalibrationHelper, this);
            FormsHelper.Locate(fcm);
            fcm.ShowDialog();
            fcm.Dispose();
            
            InvalidateFromMenu(sender);
        }
        #endregion

        #region IMeasurable implementation
        public void InitializeMeasurableData(MeasureLabelType measureLabelType)
        {
            // This is called when the drawing is added and a previous drawing had its measurement option switched on.
            // We try to retain a similar measurement option.
            if (measureInitialized)
                return;

            measureInitialized = true;

            List<MeasureLabelType> supported = new List<MeasureLabelType>() 
            {
                MeasureLabelType.None,
                MeasureLabelType.Name,
                MeasureLabelType.TravelDistance
            };

            MeasureLabelType defaultMeasureLabelType = MeasureLabelType.TravelDistance;
            this.measureLabelType = supported.Contains(measureLabelType) ? measureLabelType : defaultMeasureLabelType;
        }
        #endregion

        #region Lower level helpers
        private void SetupStyle(StyleElements preset)
        {
            // Init data
            styleData.Color = Color.DarkSlateGray;
            styleData.LineSize = 1;
            styleData.LineShape = LineShape.Solid;
            styleData.LineEnding = LineEnding.None;
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);

            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Line");

            styleElements = preset.Clone();

            // Listen to external changes of the style so we can update 
            // the mini label color.
            styleData.ValueChanged += StyleHelper_ValueChanged;
            BindStyle();
        }
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Line"));
            
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "line size");
            styleElements.Bind(styleData, "LineShape", "line shape");
            styleElements.Bind(styleData, "LineEnding", "arrows");
            styleElements.Bind(styleData, "Font", "Font");
        }
        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
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

                return HitTester.HitPath(point, areaPath, styleData.LineSize, false, transformer);
            }
        }
        private PointF GetMiddlePoint()
        {
            // Used only to attach the measure.
            return GeometryHelper.GetMiddlePoint(points["a"], points["b"]);
        }

        private void ReloadMenusCulture()
        {
            // Calibrate
            mnuCalibrate.Text = ScreenManagerLang.mnuCalibrate;

            // Measurement
            mnuMeasurement.Text = ScreenManagerLang.mnuMeasure_Label_Menu;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                ToolStripMenuItem tsmi = pair.Value;
                MeasureLabelType measureLabelType = pair.Key;
                tsmi.Text = GetMeasureLabelOptionText(measureLabelType);
                tsmi.Checked = this.measureLabelType == measureLabelType;
            }
        }
        
        private ToolStripMenuItem CreateMeasureLabelTypeMenu(MeasureLabelType measureLabelType)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Click += mnuMeasureLabelType_Click;
            mnuMeasureLabelTypes.Add(measureLabelType, mnu);
            return mnu;
        }

        private void mnuMeasureLabelType_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null)
                return;

            MeasureLabelType measureLabelType = MeasureLabelType.None;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                if (pair.Value == tsmi)
                {
                    measureLabelType = pair.Key;
                    break;
                }
            }

            this.measureLabelType = measureLabelType;
            InvalidateFromMenu(tsmi);

            if (ShowMeasurableInfoChanged != null)
                ShowMeasurableInfoChanged(this, new EventArgs<MeasureLabelType>(measureLabelType));
        }

        /// <summary>
        /// Returns the user-facing name of a measure label type.
        /// </summary>
        private string GetMeasureLabelOptionText(MeasureLabelType data)
        {
            switch (data)
            {
                case MeasureLabelType.None: return ScreenManagerLang.mnuMeasure_Label_None;
                case MeasureLabelType.Name: return ScreenManagerLang.mnuMeasure_Name;
                case MeasureLabelType.TravelDistance: return ScreenManagerLang.ExtraData_Length;
            }

            return "";
        }

        /// <summary>
        /// Get the final measure label to be rendered.
        /// </summary>
        private string GetMeasureLabelText()
        {
            string displayText = "";
            switch (measureLabelType)
            {
                case MeasureLabelType.None:
                    displayText = "";
                    break;
                case MeasureLabelType.Name:
                    displayText = name;
                    break;
                case MeasureLabelType.TravelDistance:
                    displayText = CalibrationHelper.GetLengthText(points["a"], points["b"], true);
                    break;
                default:
                    break;
            }

            return displayText;
        }

        #endregion
    }
}
