#region License
/*
Copyright � Joan Charmant 2008.
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
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using System.ComponentModel;

namespace Kinovea.ScreenManager
{
    [XmlType ("CrossMark")]
    public class DrawingCrossMark : AbstractDrawing, IKvaSerializable, IDecorable, ITrackable, IMeasurable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler<EventArgs<MeasureLabelType>> ShowMeasurableInfoChanged;
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolCross2D; }
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
            get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.DataAnalysis | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.Add(mnuOptions);
                contextMenu.Add(mnuMeasurement);
                mnuShowAsDot.Checked = showAsDot;
                return contextMenu;
            }
        }
        public PointF Location
        {
            get { return points["0"]; }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private long trackingTimestamps = -1; // timestamp of the last time we did a global track() step.
        private MiniLabel miniLabel;
        private bool measureInitialized;
        private MeasureLabelType measureLabelType = MeasureLabelType.None;
        
        // Decoration
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private InfosFading infosFading;
        private bool showAsDot = false;

        #region Menus
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowAsDot = new ToolStripMenuItem();
        #endregion

        private const int defaultBackgroundAlpha = 64;
        private const int defaultRadius = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCrossMark(PointF center, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            points["0"] = center;
            miniLabel = new MiniLabel(points["0"], Color.Black, transformer);

            SetupStyle(preset);
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            InitializeMenus();
        }
        public DrawingCrossMark(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        private void InitializeMenus()
        {
            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowAsDot.Image = Properties.Drawings.bullet_red;
            mnuShowAsDot.Click += mnuShowAsDot_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowAsDot,
            });

            // Measurement.
            mnuMeasurement.Image = Properties.Drawings.label;
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.AddRange(new ToolStripItem[] {
                CreateMeasureLabelTypeMenu(MeasureLabelType.None),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Name),
                new ToolStripSeparator(),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Position),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Distance),
            });
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if(opacityFactor <= 0)
                return;

            int iAlpha = (int)(opacityFactor * 255);
            Point c = transformer.Transform(points["0"]);

            using(Pen p = styleData.GetPen(iAlpha))
            using(SolidBrush b = styleData.GetBrush((int)(opacityFactor * defaultBackgroundAlpha)))
            {
                if (showAsDot)
                {
                    // 2x2 pixel block. Also tested with 1x1 but it's really hard to see.
                    canvas.DrawRectangle(p, c.Box(0.5f));
                }
                else
                {
                    // Cross with disc background.
                    canvas.DrawLine(p, c.X - defaultRadius, c.Y, c.X + defaultRadius, c.Y);
                    canvas.DrawLine(p, c.X, c.Y - defaultRadius, c.X, c.Y + defaultRadius);
                    canvas.FillEllipse(b, c.Box(defaultRadius + 1));
                }
            }

            if (measureLabelType != MeasureLabelType.None)
            {
                string text = GetMeasureLabelText(currentTimestamp);
                miniLabel.SetText(text, transformer);
                miniLabel.Draw(canvas, transformer, opacityFactor);
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if(handleNumber == 1)
                miniLabel.SetCenter(point);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            points["0"] = points["0"].Translate(dx, dy);
            SignalTrackablePointMoved();
            miniLabel.SetAttach(points["0"], true);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity > 0)
            {
                if(measureLabelType != MeasureLabelType.None && miniLabel.HitTest(point))
                    result = 1;
                else if (HitTester.HitPoint(point, points["0"], transformer))
                    result = 0;
            }

            return result;
        }
        public override PointF GetCopyPoint()
        {
            return points["0"];
        }
        #endregion

        #region Serialization
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
                    case "CenterPoint":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        points["0"] = p.Scale(scale.X, scale.Y);
                        break;
                    case "ReferenceTimestamp":
                        referenceTimestamp = XmlHelper.ParseTimestamp(xmlReader.ReadElementContentAsString());
                        break;
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
                    case "ShowAsDot":
                        showAsDot = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(xmlReader);
                        break;
                    case "Coordinates":
                        xmlReader.ReadOuterXml();
                        break;
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            xmlReader.ReadEndElement();
            measureInitialized = true;
            miniLabel.SetAttach(points["0"], false);
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
            SignalTrackablePointMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                PointF p = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "0");
                w.WriteElementString("CenterPoint", XmlHelper.WritePointF(p));
                w.WriteElementString("ReferenceTimestamp", XmlHelper.WriteTimestamp(referenceTimestamp));

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
                string xmlMeasureLabelType = enumConverter.ConvertToString(measureLabelType);
                w.WriteElementString("ExtraData", xmlMeasureLabelType);
                w.WriteElementString("ShowAsDot", XmlHelper.WriteBoolean(showAsDot));

                w.WriteStartElement("MeasureLabel");
                // Save the mini label relatively to the reference frame.
                miniLabel.WriteXml(w, p);
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
        public MeasuredDataPosition CollectMeasuredData()
        {
            return MeasurementSerializationHelper.CollectPosition(name, points["0"], CalibrationHelper);
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
        public void SetTracked(bool tracked)
        {
            //this.tracked = tracked;
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");

            points[name] = value;
            this.trackingTimestamps = trackingTimestamps;
            miniLabel.SetAttach(points["0"], true);
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;

            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
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
                MeasureLabelType.Position,
                MeasureLabelType.Distance
            };

            MeasureLabelType defaultMeasureLabelType = MeasureLabelType.Position;
            this.measureLabelType = supported.Contains(measureLabelType) ? measureLabelType : defaultMeasureLabelType;
        }
        #endregion

        #region Context menu
        private void mnuShowAsDot_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showAsDot = !mnuShowAsDot.Checked;
            InvalidateFromMenu(sender);
        }
        #endregion

        /// <summary>
        /// Force move the point from the outside.
        /// This is currently only used by the calibration validation window, 
        /// when the user sets the 3D location of the point and we recompute 
        /// the corresponding image location.
        /// </summary>
        public void MovePoint(PointF p)
        {
            points["0"] = p;
            SignalTrackablePointMoved();
            miniLabel.SetAttach(points["0"], true);
        }

        #region Lower level helpers
        private void SetupStyle(StyleElements preset)
        {
            styleData.Color = Color.CornflowerBlue;
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);

            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("CrossMark");

            styleElements = preset.Clone();

            styleData.ValueChanged += StyleHelper_ValueChanged;
            BindStyle();

        }
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("CrossMark"));
            styleElements.Bind(styleData, "Color", "back color");
            styleElements.Bind(styleData, "Font", "Font");
        }

        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
        }

        /// <summary>
        /// Capture the current state to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            Guid keyframeId = parentMetadata.FindAttachmentKeyframeId(this);
            var memento = new HistoryMementoModifyDrawing(parentMetadata, keyframeId, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }

        private void ReloadMenusCulture()
        {
            // Options
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowAsDot.Text = ScreenManagerLang.DrawingCrossMark_Dot;

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
        private string GetMeasureLabelOptionText(MeasureLabelType data)
        {
            switch (data)
            {
                case MeasureLabelType.None: return ScreenManagerLang.mnuMeasure_Label_None;
                case MeasureLabelType.Name: return ScreenManagerLang.mnuMeasure_Name;
                case MeasureLabelType.Position: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Position;
                case MeasureLabelType.Distance: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_DistanceToOrigin;
            }

            return "";
        }
        private string GetMeasureLabelText(long timestamp)
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

                case MeasureLabelType.Distance:
                    PointF o = CalibrationHelper.GetOrigin();
                    displayText = CalibrationHelper.GetLengthText(o, points["0"], true);
                    break;
                case MeasureLabelType.Position:
                    displayText = CalibrationHelper.GetPointText(points["0"], true, timestamp);
                    break;
                default:
                    break;
            }

            return displayText;
        }
        #endregion
    }
}
