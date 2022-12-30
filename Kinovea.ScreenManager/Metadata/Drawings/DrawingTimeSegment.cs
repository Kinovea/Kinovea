#region License
/*
Copyright © Joan Charmant 2022.
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
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Time segment. 
    /// A line that works in the time domain. 
    /// The line end points should be placed on a moving object visible in frame I and I+1.
    /// The sliding handle gives the fractional time within the frame interval.
    /// This is useful to get a more precise time for when the object crosses a line between two frames.
    /// Limitation: this assumes the movement is linear in the interval. A improvement to this tool 
    /// would be to have two more points on frames I-1 and I+2 and use cubic interpolation.
    /// </summary>
    [XmlType ("TimeSegment")]
    public class DrawingTimeSegment : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, IMeasurable
    {
        #region Events
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        /// <summary>
        /// Starting point of the time segment.
        /// </summary>
        public PointF A
        {
            get { return points["a"]; }
        }
        /// <summary>
        /// End point of the time segment.
        /// </summary>
        public PointF B
        {
            get { return points["b"]; }
        }

        /// <summary>
        /// Point somewhere on the time segment for which we want to know the time.
        /// </summary>
        public PointF C
        {
            get { return points["c"]; }
        }
        public override string ToolDisplayName
        {
            get { return "Time segment"; }
        }
        public override int ContentHash
        {
            get 
            {
                int hash = 0;
                hash ^= trackExtraData.GetHashCode();
                hash ^= miniLabel.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= infosFading.ContentHash;
                return hash;
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
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                // Rebuild the menu to get the localized text.
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReinitializeMenu();
                contextMenu.Add(mnuMeasurement);
                return contextMenu; 
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        public Metadata ParentMetadata
        {
            get { return parentMetadata; }    // unused.
            set { parentMetadata = value; }
        }

        public CalibrationHelper CalibrationHelper { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private float fraction = 0.5f;
        private bool initializing = true;
        private bool measureInitialized;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private MiniLabel miniLabel = new MiniLabel();
        private TrackExtraData trackExtraData = TrackExtraData.Time;
        private InfosFading infosFading;

        // Context menu
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private List<ToolStripMenuItem> mnuMeasurementOptions = new List<ToolStripMenuItem>();
        private Metadata parentMetadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingTimeSegment(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            points["a"] = origin;
            points["b"] = origin.Translate(10, 0);
            points["c"] = GetTimePoint();
            miniLabel.SetAttach(points["c"], true);
            
            styleHelper.Color = Color.DarkSlateGray;
            styleHelper.LineSize = 1;
            styleHelper.LineShape = LineShape.Solid;
            styleHelper.LineEnding = LineEnding.None;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("TimeSegment");
            
            style = preset.Clone();
            BindStyle();
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            // Context menu
            ReinitializeMenu();
        }
        
        public DrawingTimeSegment(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            Point start = transformer.Transform(points["a"]);
            Point end = transformer.Transform(points["b"]);
            Point mid = transformer.Transform(points["c"]);

            using (Pen penEdges = styleHelper.GetPen(opacityFactor, transformer.Scale))
            using (Brush brush = styleHelper.GetBrush(opacityFactor))
            {
                if (distorter != null && distorter.Initialized)
                    DrawDistorted(canvas, distorter, transformer, penEdges, brush, start, end, mid);
                else
                    DrawStraight(canvas, transformer, penEdges, brush, start, end, mid);
            }

            if(trackExtraData != TrackExtraData.None)
            {
                string text = GetExtraDataText();
                miniLabel.SetText(text);
                miniLabel.Draw(canvas, transformer, opacityFactor);
            }
        }
        private void DrawDistorted(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, Pen penEdges, Brush brush, Point start, Point end, Point mid)
        {
            List<PointF> curve = distorter.DistortLine(points["a"], points["b"]);
            List<Point> transformedCurve = transformer.Transform(curve);

            canvas.DrawCurve(penEdges, transformedCurve.ToArray());
        }
        private void DrawStraight(Graphics canvas, IImageToViewportTransformer transformer, Pen penEdges, Brush brush, Point start, Point end, Point mid)
        {
            canvas.DrawLine(penEdges, start, end);
            canvas.DrawEllipse(penEdges, mid.Box(4));
            miniLabel.SetAttach(points["c"], true);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor > 0)
            {
                // Give priority to the mini label and the middle point.
                if (trackExtraData != TrackExtraData.None && miniLabel.HitTest(point, transformer))
                    result = 4;
                else if (HitTester.HitTest(points["c"], point, transformer))
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
            int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            switch(handleNumber)
            {
                case 1:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["a"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["b"], point, constraintAngleSubdivisions);
                    else
                        points["a"] = point;

                    points["c"] = GetTimePoint();
                    break;
                case 2:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["b"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["a"], point, constraintAngleSubdivisions);
                    else
                        points["b"] = point;

                    points["c"] = GetTimePoint();
                    break;
                case 3:
                    if (points["a"].NearlyCoincideWith(points["b"]))
                        points["b"] = points["a"].Translate(10, 0);

                    points["c"] = GeometryHelper.GetClosestPoint(points["a"], points["b"], point, PointLinePosition.OnSegment, 0);
                    Vector ac = new Vector(points["a"], points["c"]);
                    Vector ab = new Vector(points["a"], points["b"]);
                    fraction = ac.Norm() / ab.Norm();
                    break;

                case 4:
                    // Move the center of the mini label to the mouse coord.
                    miniLabel.SetLabel(point);
                    break;
            }

            // Make sure the line is never shorter than 10 px long as this causes issues to expand it again.
            if (GeometryHelper.GetDistance(points["a"], points["b"]) < 10)
                points["b"] = points["a"].Translate(10, 0);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            points["a"] = points["a"].Translate(dx, dy);
            points["b"] = points["b"].Translate(dx, dy);
            points["c"] = GetTimePoint();
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
                    case "Fraction":
                        {
                            string strFraction = xmlReader.ReadElementContentAsString();
                            fraction = float.Parse(strFraction, CultureInfo.InvariantCulture);
                            points["c"] = GetTimePoint();
                            break;
                        }
                    case "ExtraData":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
                            trackExtraData = (TrackExtraData)enumConverter.ConvertFromString(xmlReader.ReadElementContentAsString());
                            break;
                        }
                    case "MeasureLabel":
                        {
                            miniLabel = new MiniLabel(xmlReader, scale);
                            break;
                        }
                    case "DrawingStyle":
                        style = new DrawingStyle(xmlReader);
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
            miniLabel.SetAttach(points["c"], false);
            miniLabel.BackColor = styleHelper.Color;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Start", XmlHelper.WritePointF(points["a"]));
                w.WriteElementString("End", XmlHelper.WritePointF(points["b"]));
                w.WriteElementString("Fraction", XmlHelper.WriteFloat(fraction));

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
                string xmlExtraData = enumConverter.ConvertToString(trackExtraData);
                w.WriteElementString("ExtraData", xmlExtraData);

                w.WriteStartElement("MeasureLabel");
                miniLabel.WriteXml(w);
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

        #region Context menu
        private void ReinitializeMenu()
        {
            InitializeMenuMeasurement();
        }
        private void InitializeMenuMeasurement()
        {
            mnuMeasurement.Image = Properties.Drawings.measure;
            mnuMeasurement.Text = ScreenManagerLang.mnuShowMeasure;
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.None));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Time));
        }
        private ToolStripMenuItem GetMeasurementMenu(TrackExtraData data)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Text = GetExtraDataOptionText(data);
            mnu.Checked = trackExtraData == data;

            mnu.Click += (s, e) =>
            {
                trackExtraData = data;
                InvalidateFromMenu(s);

                // Use this setting as the default value for new measurable objects.
                //if(ShowMeasurableInfoChanged != null)
                    //ShowMeasurableInfoChanged(this, new EventArgs<TrackExtraData>(trackExtraData));
            };

            return mnu;
        }
        private string GetExtraDataOptionText(TrackExtraData data)
        {
            switch (data)
            {
                case TrackExtraData.None: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None;
                case TrackExtraData.Time: return "Time"; //ScreenManagerLang.dlgConfigureDrawing_Name;
            }

            return "";
        }
        private string GetExtraDataText()
        {
            if (trackExtraData == TrackExtraData.None)
                return "";

            if (parentMetadata == null)
                return "";

            string displayText = "###";
            displayText = parentMetadata.GetFractionTime(infosFading.ReferenceTimestamp, fraction);
            return displayText;
        }
        #endregion

        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
            // This is called when the drawing is added and a previous drawing had its measurement option switched on.
            // We try to retain a similar measurement option.
            //if (measureInitialized)
            //    return;

            //measureInitialized = true;

            //// If the option is supported, we just use it, otherwise we use the length.
            //if (trackExtraData == TrackExtraData.None || 
            //    trackExtraData == TrackExtraData.Time)
            //    this.trackExtraData = trackExtraData;
            //else
            //    this.trackExtraData = TrackExtraData.Time;
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("TimeSegment"));
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
        }
        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleHelper.Color;
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
        
        /// <summary>
        /// Returns the middle point of the segment.
        /// </summary>
        private PointF GetMiddlePoint()
        {
            // Used only to attach the measure.
            return GeometryHelper.GetMiddlePoint(points["a"], points["b"]);
        }

        /// <summary>
        /// Returns the coordinate of the sliding point based on the current fraction.
        /// </summary>
        private PointF GetTimePoint()
        {
            Vector v = new Vector(points["a"], points["b"]);
            return points["a"] + (v * fraction);
        }
        
        #endregion
    }
}
