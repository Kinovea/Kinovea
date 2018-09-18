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
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
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
            get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.DataAnalysis | DrawingCapabilities.CopyPaste; }
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
        public PointF Location
        {
            get { return points["0"]; }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool tracking;
        
        private MiniLabel miniLabel;
        private TrackExtraData trackExtraData = TrackExtraData.None;
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;

        // Context menu
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private List<ToolStripMenuItem> mnuMeasurementOptions = new List<ToolStripMenuItem>();
        
        private const int defaultBackgroundAlpha = 64;
        private const int defaultRadius = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCrossMark(PointF center, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            points["0"] = center;
            miniLabel = new MiniLabel(points["0"], Color.Black, transformer);
            
            // Decoration & binding with editors
            styleHelper.Color = Color.CornflowerBlue;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("CrossMark");

            style = preset.Clone();
            BindStyle();
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            ReinitializeMenu();
        }
        public DrawingCrossMark(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
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
            
            int iAlpha = (int)(opacityFactor * 255);
            Point c = transformer.Transform(points["0"]);

            using(Pen p = styleHelper.GetPen(iAlpha))
            using(SolidBrush b = styleHelper.GetBrush((int)(opacityFactor * defaultBackgroundAlpha)))
            {
                canvas.DrawLine(p, c.X - defaultRadius, c.Y, c.X + defaultRadius, c.Y);
                canvas.DrawLine(p, c.X, c.Y - defaultRadius, c.X, c.Y + defaultRadius);
                canvas.FillEllipse(b, c.Box(defaultRadius + 1));
            }
            
            if (trackExtraData != TrackExtraData.None)
            {
                string text = GetExtraDataText(infosFading.ReferenceTimestamp);
                miniLabel.SetText(text);
                miniLabel.Draw(canvas, transformer, opacityFactor);
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if(handleNumber == 1)
                miniLabel.SetLabel(point);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            points["0"] = points["0"].Translate(dx, dy);
            SignalTrackablePointMoved();
            miniLabel.SetAttach(points["0"], true);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (tracking || opacity > 0)
            {
                if(trackExtraData != TrackExtraData.None && miniLabel.HitTest(point, transformer))
                    result = 1;
                else if (HitTester.HitTest(points["0"], point, transformer))
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
            miniLabel.SetAttach(points["0"], false);
            miniLabel.BackColor = styleHelper.Color;
            SignalTrackablePointMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("CenterPoint", XmlHelper.WritePointF(points["0"]));

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
            
            if(ShouldSerializeAll(filter))
            {
                // Spreadsheet support.
                w.WriteStartElement("Coordinates");
                
                PointF p = new PointF(points["0"].X, points["0"].Y);
                PointF coords = CalibrationHelper.GetPoint(p);
                w.WriteAttributeString("UserX", String.Format("{0:0.00}", coords.X));
                w.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.X));
                w.WriteAttributeString("UserY", String.Format("{0:0.00}", coords.Y));
                w.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.Y));
                w.WriteAttributeString("UserUnitLength", CalibrationHelper.GetLengthAbbreviation());
                
                w.WriteEndElement();
            }
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
            miniLabel.SetAttach(points["0"], true);
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
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

            // TODO: unhook event handlers ?
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.None));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Name));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Position));
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

                if(ShowMeasurableInfoChanged != null)
                    ShowMeasurableInfoChanged(this, new EventArgs<TrackExtraData>(trackExtraData));
            };

            return mnu;
        }
        public string GetExtraDataOptionText(TrackExtraData data)
        {
            switch (data)
            {
                case TrackExtraData.None: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None;
                case TrackExtraData.Name: return ScreenManagerLang.dlgConfigureDrawing_Name;
                case TrackExtraData.Position: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_Position;
            }

            return "";
        }
        public string GetExtraDataText(long referencTimestamp)
        {
            if (trackExtraData == TrackExtraData.None)
                return "";

            string displayText = "###";
            switch (trackExtraData)
            {
                case TrackExtraData.Name:
                    displayText = name;
                    break;
                case TrackExtraData.Position:
                default:
                    displayText = CalibrationHelper.GetPointText(new PointF(points["0"].X, points["0"].Y), true, true, referencTimestamp);
                    break;
            }

            return displayText;
        }
        #endregion

        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
            if (trackExtraData == TrackExtraData.None || trackExtraData == TrackExtraData.Name)
                this.trackExtraData = trackExtraData;
            else
                this.trackExtraData = TrackExtraData.Position;
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "back color");
        }

        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleHelper.Color;
        }
        #endregion

    }
}
