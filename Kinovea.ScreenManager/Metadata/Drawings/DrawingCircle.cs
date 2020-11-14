#region License
/*
Copyright © Joan Charmant 2010.
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
    [XmlType ("Circle")]
    public class DrawingCircle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, IMeasurable
    {
        #region Events
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolCircle; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = center.GetHashCode();
                hash ^= radius.GetHashCode();
                hash ^= showCenter.GetHashCode();
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
            get{ return infosFading;}
            set{ infosFading = value;}
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
                contextMenu.Add(mnuShowCenter);
                return contextMenu;
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        public CalibrationHelper CalibrationHelper
        {
            get
            {
                return calibrationHelper;
            }
            set
            {
                calibrationHelper = value;
                calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            }
        }
        #endregion

        #region Members
        // Core
        private PointF center;
        private int radius;
        private bool initializing = true;
        private bool measureInitialized = false;
        private static readonly float crossSize = 15;
        private static readonly float crossRadius = crossSize / 2.0f;
        private MiniLabel miniLabel = new MiniLabel();
        private TrackExtraData trackExtraData = TrackExtraData.None;
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private List<ToolStripMenuItem> mnuMeasurementOptions = new List<ToolStripMenuItem>();
        private ToolStripMenuItem mnuShowCenter = new ToolStripMenuItem();

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        private CalibrationHelper calibrationHelper;
        private Ellipse ellipseInImage;
        private PointF radiusLeftInImage;
        private PointF radiusRightInImage;
        private bool showCenter = true;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        //------------------------------------------------------
        // Note:
        // When using the planar calibration, the projection of the circle in world space and back in image space
        // creates an ellipse whose center is not exactly on the center of the original circle.
        // This is why there are extra checks to move the minilabel attachment point everytime the drawing moves, 
        // changes size, or when the calibration changes.
        //------------------------------------------------------

        #region Constructor
        public DrawingCircle(PointF center, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            this.center = center;
            miniLabel.SetAttach(center, true);

            if (transformer != null)
                this.radius = transformer.Untransform(25);

            this.radius = Math.Min(radius, 10);
            this.infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            styleHelper.Color = Color.Empty;
            styleHelper.LineSize = 1;
            styleHelper.PenShape = PenShape.Solid;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("Circle");
            
            style = preset.Clone();
            BindStyle();

            mnuShowCenter.Image = Properties.Drawings.crossmark;
            mnuShowCenter.Checked = showCenter;
            mnuShowCenter.Click += mnuShowCenter_Click;
            ReinitializeMenu();
        }
        public DrawingCircle(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if(opacityFactor <= 0)
                return;
            
            int alpha = (int)(opacityFactor * 255);
            using(Pen p = styleHelper.GetPen(alpha, transformer.Scale))
            {
                if (styleHelper.PenShape == PenShape.Dash)
                    p.DashStyle = DashStyle.Dash;

                // The center of the original circle is still the correct center even in perspective.
                PointF circleCenter = transformer.Transform(center);
                if (showCenter)
                    DrawCenter(canvas, p, circleCenter.ToPoint());

                if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
                {
                    // Draw the circle in perspective.
                    PointF ellipseCenter = transformer.Transform(ellipseInImage.Center);
                    float semiMinorAxis = transformer.Transform((int)ellipseInImage.SemiMinorAxis);
                    float semiMajorAxis = transformer.Transform((int)ellipseInImage.SemiMajorAxis);
                    Ellipse ellipse = new Ellipse(ellipseCenter, semiMajorAxis, semiMinorAxis, ellipseInImage.Rotation);
                    RectangleF rect = new RectangleF(-ellipse.SemiMajorAxis, -ellipse.SemiMinorAxis, ellipse.SemiMajorAxis * 2, ellipse.SemiMinorAxis * 2);
                    float angle = (float)(ellipse.Rotation * MathHelper.RadiansToDegrees);
                    
                    canvas.TranslateTransform(ellipse.Center.X, ellipse.Center.Y);
                    canvas.RotateTransform(angle);

                    canvas.DrawEllipse(p, rect);
                    
                    canvas.RotateTransform(-angle);
                    canvas.TranslateTransform(-ellipse.Center.X, -ellipse.Center.Y);
                }
                else
                {
                    Rectangle boundingBox = transformer.Transform(center.Box(radius));
                    canvas.DrawEllipse(p, boundingBox);
                }

                if (trackExtraData != TrackExtraData.None)
                {
                    // Draw lines from the center to the periphery of the circle to show the radius or diameter.
                    if (trackExtraData == TrackExtraData.Radius)
                    {
                        PointF radiusRight = transformer.Transform(radiusRightInImage);
                        canvas.DrawLine(p, circleCenter, radiusRight);
                    }
                    else if (trackExtraData == TrackExtraData.Diameter)
                    {
                        PointF radiusLeft = transformer.Transform(radiusLeftInImage);
                        PointF radiusRight = transformer.Transform(radiusRightInImage);
                        canvas.DrawLine(p, radiusLeft, radiusRight);
                    }

                    string text = GetExtraDataText(currentTimestamp);
                    miniLabel.SetText(text);
                    miniLabel.Draw(canvas, transformer, opacityFactor);
                }
            }
        }
        private void DrawCenter(Graphics canvas, Pen pen, Point center)
        {
            // Precision center.
            float memoPenWidth = pen.Width;
            DashStyle memoPenDash = pen.DashStyle;

            pen.Width = 1.0f;
            pen.DashStyle = DashStyle.Solid;
            Point c = center;
            canvas.DrawLine(pen, center.X - crossRadius, center.Y, center.X + crossRadius, center.Y);
            canvas.DrawLine(pen, center.X, center.Y - crossRadius, center.X, center.Y + crossRadius);

            pen.Width = memoPenWidth;
            pen.DashStyle = memoPenDash;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (handleNumber == 1)
            {
                // User is dragging the outline of the circle, figure out the new radius at this point.
                float shiftX = Math.Abs(point.X - center.X);
                float shiftY = Math.Abs(point.Y - center.Y);
                radius = (int)Math.Sqrt((shiftX * shiftX) + (shiftY * shiftY));
                radius = Math.Max(radius, 10);

                UpdateEllipseInImage();
            }
            else if (handleNumber == 2)
            {
                miniLabel.SetLabel(point);
                UpdateEllipseInImage();
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            center = center.Translate(dx, dy);
            if (CalibrationHelper == null)
                return;

            UpdateEllipseInImage();
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            // We do not need a special case to hit the radius or diameter line as they are inside the circle and 
            // don't have special handling compared to just moving the whole thing.
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;

            if (trackExtraData != TrackExtraData.None && miniLabel.HitTest(point, transformer))
                return 2;

            if (IsPointOnHandler(point, transformer))
                return 1;

            if (IsPointInObject(point, transformer))
                return 0;

            return -1;
        }
        public override PointF GetCopyPoint()
        {
            return center;
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
                    case "Origin":
                        PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        center = p.Scale(scale.X, scale.Y);
                        break;
                    case "Radius":
                        radius = (int)(xmlReader.ReadElementContentAsInt() * Math.Min(scale.X, scale.Y));
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
                    case "ShowCenter":
                        showCenter = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
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
            initializing = false;
            measureInitialized = true;
            miniLabel.SetAttach(center, false);
            miniLabel.BackColor = styleHelper.Color;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Origin", XmlHelper.WritePointF(center));
                w.WriteElementString("Radius", radius.ToString());

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
                string xmlExtraData = enumConverter.ConvertToString(trackExtraData);
                w.WriteElementString("ExtraData", xmlExtraData);

                w.WriteStartElement("MeasureLabel");
                miniLabel.WriteXml(w);
                w.WriteEndElement();

                w.WriteElementString("ShowCenter", XmlHelper.WriteBoolean(showCenter));
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
            MoveHandle(point, 1, modifiers);
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
        private void mnuShowCenter_Click(object sender, EventArgs e)
        {
            mnuShowCenter.Checked = !mnuShowCenter.Checked;
            showCenter = mnuShowCenter.Checked;
            InvalidateFromMenu(sender);
        }
        private void ReinitializeMenu()
        {
            InitializeMenuMeasurement();
            mnuShowCenter.Text = ScreenManagerLang.mnuShowCircleCenter;
            mnuShowCenter.Checked = showCenter;
        }
        private void InitializeMenuMeasurement()
        {
            //mnuMeasurement.MergeIndex = 4;
            mnuMeasurement.Image = Properties.Drawings.measure;
            mnuMeasurement.Text = ScreenManagerLang.mnuShowMeasure;

            // TODO: unhook event handlers ?
            mnuMeasurement.DropDownItems.Clear();
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.None));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Name));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Center));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Radius));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Diameter));
            mnuMeasurement.DropDownItems.Add(GetMeasurementMenu(TrackExtraData.Circumference));
        }
        private ToolStripMenuItem GetMeasurementMenu(TrackExtraData data)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Text = GetExtraDataOptionText(data);
            mnu.Checked = trackExtraData == data;

            mnu.Click += (s, e) =>
            {
                trackExtraData = data;
                ResetAttachPoint();
                InvalidateFromMenu(s);

                if(ShowMeasurableInfoChanged != null)
                    ShowMeasurableInfoChanged(this, new EventArgs<TrackExtraData>(trackExtraData));
            };

            return mnu;
        }
        private string GetExtraDataOptionText(TrackExtraData data)
        {
            switch (data)
            {
                case TrackExtraData.None: return ScreenManagerLang.dlgConfigureTrajectory_ExtraData_None;
                case TrackExtraData.Name: return ScreenManagerLang.dlgConfigureDrawing_Name;
                case TrackExtraData.Center: return ScreenManagerLang.ExtraData_Center;
                case TrackExtraData.Radius: return ScreenManagerLang.ExtraData_Radius;
                case TrackExtraData.Diameter: return ScreenManagerLang.ExtraData_Diameter;
                case TrackExtraData.Circumference: return ScreenManagerLang.ExtraData_Circumference;
            }

            return "";
        }
        private string GetExtraDataText(long currentTimestamp)
        {
            if (trackExtraData == TrackExtraData.None)
                return "";

            string displayText = "###";
            switch (trackExtraData)
            {
                case TrackExtraData.Center:
                    displayText = CalibrationHelper.GetPointText(center, true, true, currentTimestamp);
                    break;
                case TrackExtraData.Radius:
                    displayText = CalibrationHelper.GetLengthText(center, radiusRightInImage, true, true);
                    break;
                case TrackExtraData.Diameter:
                    displayText = CalibrationHelper.GetLengthText(radiusLeftInImage, radiusRightInImage, true, true);
                    break;
                case TrackExtraData.Circumference:
                    displayText = CalibrationHelper.GetCircumferenceText(center, radiusRightInImage, true, true);
                    break;
                case TrackExtraData.Name:
                default:
                    displayText = name;
                    break;
            }

            return displayText;
        }
        #endregion
        
        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
            // This is called when the drawing is added and a previous drawing had its measurement option switched on.
            // We try to retain a similar measurement option.
            if (measureInitialized)
                return;

            measureInitialized = true;
            
            // If the option is supported, we just use it, otherwise we use the center.
            if (trackExtraData == TrackExtraData.None || 
                trackExtraData == TrackExtraData.Name || 
                trackExtraData == TrackExtraData.Center || 
                trackExtraData == TrackExtraData.Radius || 
                trackExtraData == TrackExtraData.Diameter || 
                trackExtraData == TrackExtraData.Circumference)
                this.trackExtraData = trackExtraData;
            else
                this.trackExtraData = TrackExtraData.Center;

            ResetAttachPoint();
        }
        public void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            UpdateEllipseInImage();
        }
        private void UpdateEllipseInImage()
        {
            // Takes the circle in image space and figure out the corresponding ellipse in image space.
            // Also get the left and right points along the horizontal diameter, also in image space, 
            // these are used to show measurements and attach the minilabel.
            // This should be called any time the center or circumference change.
            if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                ellipseInImage = CalibrationHelper.GetEllipseFromCircle(center, radius, out radiusLeftInImage, out radiusRightInImage);
            }
            else
            {
                radiusLeftInImage = new PointF(center.X - radius, center.Y);
                radiusRightInImage = new PointF(center.X + radius, center.Y);
            }

            ResetAttachPoint();
        }
        private void ResetAttachPoint()
        {
            if (trackExtraData == TrackExtraData.Name || 
                trackExtraData == TrackExtraData.Center ||
                trackExtraData == TrackExtraData.Diameter)
                miniLabel.SetAttach(center, true);
            else if (trackExtraData == TrackExtraData.Radius)
                miniLabel.SetAttach(GeometryHelper.GetMiddlePoint(center, radiusRightInImage), true);
            else if (trackExtraData == TrackExtraData.Circumference)
                miniLabel.SetAttach(radiusRightInImage, true);
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("Circle"));
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "pen size");
            style.Bind(styleHelper, "PenShape", "pen shape");
        }
        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleHelper.Color;
        }
        private bool IsPointInObject(PointF point, IImageToViewportTransformer transformer)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                GetHitPath(areaPath);
                hit = HitTester.HitTest(areaPath, point, 0, true, transformer);
            }
            return hit;
        }
        private bool IsPointOnHandler(PointF point, IImageToViewportTransformer transformer)
        {
            if(radius < 0)
                return false;
            
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                GetHitPath(areaPath);
                return HitTester.HitTest(areaPath, point, styleHelper.LineSize, false, transformer);
            }
        }

        private void GetHitPath(GraphicsPath areaPath)
        {
            if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                RectangleF rect = new RectangleF(-ellipseInImage.SemiMajorAxis, -ellipseInImage.SemiMinorAxis, ellipseInImage.SemiMajorAxis * 2, ellipseInImage.SemiMinorAxis * 2);
                float angle = (float)(ellipseInImage.Rotation * MathHelper.RadiansToDegrees);

                areaPath.AddEllipse(rect);

                Matrix transform = new Matrix();
                transform.Translate(ellipseInImage.Center.X, ellipseInImage.Center.Y);
                transform.Rotate(angle);
                areaPath.Transform(transform);
            }
            else
            {
                areaPath.AddEllipse(center.Box(radius));
            }
        }
        #endregion
    }

       
}