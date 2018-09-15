#region License
/*
Copyright © Joan Charmant 2010.
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
    [XmlType ("Circle")]
    public class DrawingCircle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, IMeasurable
    {
        #region Events
        public event EventHandler ShowMeasurableInfoChanged;
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
                hash ^= ShowMeasurableInfo.GetHashCode();
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
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                mnuShowCoordinates.Text = ScreenManagerLang.mnuShowCoordinates;
                mnuShowCoordinates.Checked = ShowMeasurableInfo;
                contextMenu.Add(mnuShowCoordinates);
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
        public bool ShowMeasurableInfo { get; set; }
        #endregion

        #region Members
        // Core
        private PointF center;
        private int radius;
        private bool initializing = true;
        private static readonly float crossSize = 15;
        private static readonly float crossRadius = crossSize / 2.0f;
        private MiniLabel miniLabel = new MiniLabel();
        private ToolStripMenuItem mnuShowCoordinates = new ToolStripMenuItem();
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        private CalibrationHelper calibrationHelper;
        private Ellipse ellipseInImage;


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
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("Circle");
            
            style = preset.Clone();
            BindStyle();

            mnuShowCoordinates.Click += new EventHandler(mnuShowCoordinates_Click);
            mnuShowCoordinates.Image = Properties.Drawings.measure;
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
                if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
                {
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

                    // Precision center.
                    p.Width = 1.0f;
                    Point c = ellipseCenter.ToPoint();
                    canvas.DrawLine(p, c.X - crossRadius, c.Y, c.X + crossRadius, c.Y);
                    canvas.DrawLine(p, c.X, c.Y - crossRadius, c.X, c.Y + crossRadius);
                }
                else
                {
                    Rectangle boundingBox = transformer.Transform(center.Box(radius));
                    canvas.DrawEllipse(p, boundingBox);

                    // Precision center.
                    p.Width = 1.0f;
                    Point c = boundingBox.Center();
                    canvas.DrawLine(p, c.X - crossRadius, c.Y, c.X + crossRadius, c.Y);
                    canvas.DrawLine(p, c.X, c.Y - crossRadius, c.X, c.Y + crossRadius);
                }

                if (ShowMeasurableInfo)
                {
                    miniLabel.SetText(CalibrationHelper.GetPointText(new PointF(center.X, center.Y), true, true, infosFading.ReferenceTimestamp));
                    miniLabel.Draw(canvas, transformer, opacityFactor);
                }
            }
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

                if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
                {
                    ellipseInImage = CalibrationHelper.GetEllipseFromCircle(center, radius);
                    miniLabel.SetAttach(ellipseInImage.Center, true);
                }
            }
            else if (handleNumber == 2)
            {
                miniLabel.SetLabel(point);
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            center = center.Translate(dx, dy);
            if (CalibrationHelper == null)
                return;

            if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                ellipseInImage = CalibrationHelper.GetEllipseFromCircle(center, radius);
                miniLabel.SetAttach(ellipseInImage.Center, true);
            }
            else
            {
                miniLabel.SetAttach(center, true);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;

            if (ShowMeasurableInfo && miniLabel.HitTest(point, transformer))
                return 2;

            if (IsPointOnHandler(point, transformer))
                return 1;

            if (IsPointInObject(point, transformer))
                return 0;

            return -1;
        }
        public override PointF GetPosition()
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
                        center = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        break;
                    case "Radius":
                        radius = (int)(xmlReader.ReadElementContentAsInt() * scale.X);
                        break;
                    case "CoordinatesVisible":
                        ShowMeasurableInfo = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
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
                    default:
                        string unparsed = xmlReader.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }
            
            xmlReader.ReadEndElement();
            initializing = false;
            miniLabel.SetAttach(center, false);
            miniLabel.BackColor = styleHelper.Color;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Origin", XmlHelper.WritePointF(center));
                w.WriteElementString("Radius", radius.ToString());
                w.WriteElementString("CoordinatesVisible", ShowMeasurableInfo.ToString().ToLower());

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
        private void mnuShowCoordinates_Click(object sender, EventArgs e)
        {
            // Enable / disable the display of the coordinates for this cross marker.
            ShowMeasurableInfo = !ShowMeasurableInfo;

            // Use this setting as the default value for new lines.
            if (ShowMeasurableInfoChanged != null)
                ShowMeasurableInfoChanged(this, EventArgs.Empty);

            InvalidateFromMenu(sender);
        }
        #endregion

        public void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            if (CalibrationHelper.CalibratorType == CalibratorType.Plane)
            {
                ellipseInImage = CalibrationHelper.GetEllipseFromCircle(center, radius);
                miniLabel.SetAttach(ellipseInImage.Center, true);
            }
        }

        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "pen size");
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