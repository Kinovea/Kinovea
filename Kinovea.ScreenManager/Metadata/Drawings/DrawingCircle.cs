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
        public event EventHandler<EventArgs<MeasureLabelType>> ShowMeasurableInfoChanged;
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
                hash ^= filled.GetHashCode();
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
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuOptions,
                    mnuMeasurement,
                });

                mnuShowCenter.Checked = showCenter;
                mnuFilled.Checked = filled;
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
        private MeasureLabelType measureLabelType = MeasureLabelType.None;

        #region Menus
        private ToolStripMenuItem mnuMeasurement = new ToolStripMenuItem();
        private Dictionary<MeasureLabelType, ToolStripMenuItem> mnuMeasureLabelTypes = new Dictionary<MeasureLabelType, ToolStripMenuItem>();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowCenter = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFilled = new ToolStripMenuItem();
        #endregion

        // Decoration
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private InfosFading infosFading;
        private CalibrationHelper calibrationHelper;
        private Ellipse ellipseInImage;
        private PointF radiusLeftInImage;
        private PointF radiusRightInImage;
        private bool showCenter = false;
        private bool filled = false;
        
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
        public DrawingCircle(PointF center, long timestamp, double averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            this.center = center;
            miniLabel.SetAttach(center, true);

            if (transformer != null)
                this.radius = transformer.Untransform(25);

            this.radius = Math.Min(radius, 10);
            this.infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            SetupStyle(preset);
            InitializeMenus();
        }
        public DrawingCircle(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
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
                CreateMeasureLabelTypeMenu(MeasureLabelType.Center),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Radius),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Diameter),
                CreateMeasureLabelTypeMenu(MeasureLabelType.Circumference),
            });

            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowCenter.Image = Properties.Drawings.crossmark;
            mnuFilled.Image = Properties.Drawings.filled;
            mnuShowCenter.Click += mnuShowCenter_Click;
            mnuFilled.Click += mnuFilled_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowCenter,
                mnuFilled,
            });
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if(opacityFactor <= 0)
                return;
            
            int alpha = (int)(opacityFactor * 255);
            using(Pen p = styleData.GetPen(alpha, transformer.Scale))
            {
                if (styleData.PenShape == PenShape.Dash)
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
                    float angle = MathHelper.Degrees(ellipse.Rotation);
                    
                    canvas.TranslateTransform(ellipse.Center.X, ellipse.Center.Y);
                    canvas.RotateTransform(angle);

                    canvas.DrawEllipse(p, rect);
                    if (filled)
                    {
                        using (SolidBrush b = styleData.GetBrush(alpha))
                            canvas.FillEllipse(b, rect);
                    }
                    
                    canvas.RotateTransform(-angle);
                    canvas.TranslateTransform(-ellipse.Center.X, -ellipse.Center.Y);
                }
                else
                {
                    Rectangle boundingBox = transformer.Transform(center.Box(radius));
                    canvas.DrawEllipse(p, boundingBox);
                    if (filled)
                    {
                        using (SolidBrush b = styleData.GetBrush(alpha))
                            canvas.FillEllipse(b, boundingBox);
                    }
                }

                if (measureLabelType != MeasureLabelType.None)
                {
                    // Draw lines from the center to the periphery of the circle to show the radius or diameter.
                    if (measureLabelType == MeasureLabelType.Radius)
                    {
                        PointF radiusRight = transformer.Transform(radiusRightInImage);
                        canvas.DrawLine(p, circleCenter, radiusRight);
                    }
                    else if (measureLabelType == MeasureLabelType.Diameter)
                    {
                        PointF radiusLeft = transformer.Transform(radiusLeftInImage);
                        PointF radiusRight = transformer.Transform(radiusRightInImage);
                        canvas.DrawLine(p, radiusLeft, radiusRight);
                    }

                    string text = GetMeasureLabelText(currentTimestamp);
                    miniLabel.SetText(text, transformer);
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
                miniLabel.SetCenter(point);
                UpdateEllipseInImage();
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            center = center.Translate(dx, dy);
            if (CalibrationHelper == null)
                return;

            UpdateEllipseInImage();
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            // We do not need a special case to hit the radius or diameter line as they are inside the circle and 
            // don't have special handling compared to just moving the whole thing.
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;

            if (measureLabelType != MeasureLabelType.None && miniLabel.HitTest(point))
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
                            measureLabelType = XmlHelper.ParseEnum<MeasureLabelType>(xmlReader.ReadElementContentAsString(), MeasureLabelType.None);
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
                    case "Filled":
                        filled = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "DrawingStyle":
                        styleElements.ImportXML(xmlReader);
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
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Origin", XmlHelper.WritePointF(center));
                w.WriteElementString("Radius", radius.ToString());

                TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(MeasureLabelType));
                string xmlMeasureLabelType = enumConverter.ConvertToString(measureLabelType);
                w.WriteElementString("ExtraData", xmlMeasureLabelType);

                w.WriteStartElement("MeasureLabel");
                miniLabel.WriteXml(w);
                w.WriteEndElement();

                w.WriteElementString("ShowCenter", XmlHelper.WriteBoolean(showCenter));
                w.WriteElementString("Filled", XmlHelper.WriteBoolean(filled));
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
            CaptureMemento(SerializationFilter.Core);
            showCenter = !mnuShowCenter.Checked;
            InvalidateFromMenu(sender);
        }
        private void mnuFilled_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            filled = !mnuFilled.Checked;
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
                MeasureLabelType.Center,
                MeasureLabelType.Radius,
                MeasureLabelType.Diameter,
                MeasureLabelType.Circumference
            };

            MeasureLabelType defaultMeasureLabelType = MeasureLabelType.Center;
            this.measureLabelType = supported.Contains(measureLabelType) ? measureLabelType : defaultMeasureLabelType;

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
            if (measureLabelType == MeasureLabelType.Name || 
                measureLabelType == MeasureLabelType.Center ||
                measureLabelType == MeasureLabelType.Diameter)
                miniLabel.SetAttach(center, true);
            else if (measureLabelType == MeasureLabelType.Radius)
                miniLabel.SetAttach(GeometryHelper.GetMiddlePoint(center, radiusRightInImage), true);
            else if (measureLabelType == MeasureLabelType.Circumference)
                miniLabel.SetAttach(radiusRightInImage, true);
        }
        #endregion

        #region Lower level helpers
        private void SetupStyle(StyleElements preset)
        {
            styleData.Color = Color.Empty;
            styleData.LineSize = 1;
            styleData.PenShape = PenShape.Solid;
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);
            
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Circle");

            styleElements = preset.Clone();
            styleData.ValueChanged += StyleHelper_ValueChanged;
            BindStyle();
        }
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Circle"));
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "pen size");
            styleElements.Bind(styleData, "PenShape", "pen shape");
            styleElements.Bind(styleData, "Font", "Font");
        }
        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            miniLabel.BackColor = styleData.Color;
            miniLabel.FontSize = (int)styleData.Font.Size;
        }
        private bool IsPointInObject(PointF point, IImageToViewportTransformer transformer)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                GetHitPath(areaPath);
                hit = HitTester.HitPath(point, areaPath, 0, true, transformer);
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
                return HitTester.HitPath(point, areaPath, styleData.LineSize, false, transformer);
            }
        }

        private void GetHitPath(GraphicsPath areaPath)
        {
            if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                RectangleF rect = new RectangleF(-ellipseInImage.SemiMajorAxis, -ellipseInImage.SemiMinorAxis, ellipseInImage.SemiMajorAxis * 2, ellipseInImage.SemiMinorAxis * 2);
                float angle = MathHelper.Degrees(ellipseInImage.Rotation);

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
            // Measurement
            mnuMeasurement.Text = ScreenManagerLang.mnuMeasure_Label_Menu;
            foreach (var pair in mnuMeasureLabelTypes)
            {
                ToolStripMenuItem tsmi = pair.Value;
                MeasureLabelType measureLabelType = pair.Key;
                tsmi.Text = GetMeasureLabelOptionText(measureLabelType);
                tsmi.Checked = this.measureLabelType == measureLabelType;
            }

            // Options
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowCenter.Text = ScreenManagerLang.mnuShowCircleCenter;
            mnuFilled.Text = ScreenManagerLang.DrawingCircle_Filled;
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
            ResetAttachPoint();
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
                case MeasureLabelType.Center: return ScreenManagerLang.ExtraData_Center;
                case MeasureLabelType.Radius: return ScreenManagerLang.ExtraData_Radius;
                case MeasureLabelType.Diameter: return ScreenManagerLang.ExtraData_Diameter;
                case MeasureLabelType.Circumference: return ScreenManagerLang.ExtraData_Circumference;
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

                case MeasureLabelType.Center:
                    displayText = CalibrationHelper.GetPointText(center, true, timestamp);
                    break;
                case MeasureLabelType.Radius:
                    displayText = CalibrationHelper.GetLengthText(center, radiusRightInImage, true);
                    break;
                case MeasureLabelType.Diameter:
                    displayText = CalibrationHelper.GetLengthText(radiusLeftInImage, radiusRightInImage, true);
                    break;
                case MeasureLabelType.Circumference:
                    displayText = CalibrationHelper.GetCircumferenceText(center, radiusRightInImage, true);
                    break;
                default:
                    break;
            }

            return displayText;
        }
        #endregion
    }

       
}