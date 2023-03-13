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
    /// 
    /// Limitation: this assumes the movement is linear in the interval. An improvement to this tool 
    /// would be to have two more points on frames I-1 and I+2 and use cubic interpolation.
    /// </summary>
    [XmlType ("TimeSegment")]
    public class DrawingTimeSegment : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return "Time segment"; }
        }
        public override int ContentHash
        {
            get 
            {
                int hash = 0;
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
            get { return null; }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private float fraction = 0.5f;
        private bool initializing = true;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private int lineSize = 1;
        private DrawingStyle style;
        private MiniLabel miniLabel = new MiniLabel();
        private InfosFading infosFading;

        // Context menu
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingTimeSegment(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            points["a"] = origin;
            points["b"] = origin.Translate(10, 0);
            
            styleHelper.Color = Color.DarkSlateGray;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("TimeSegment");
            
            style = preset.Clone();
            BindStyle();
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
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

            // Main segment.
            Point start = transformer.Transform(points["a"]);
            Point end = transformer.Transform(points["b"]);

            // Tick mark at A.
            Vector ab = new Vector(points["a"], points["b"]);
            Vector perp1 = new Vector(ab.Y, -ab.X) * 0.2f;
            Vector perp2 = perp1.Negate();
            Point p1A = transformer.Transform(new PointF(points["a"].X + perp1.X, points["a"].Y + perp1.Y));
            Point p2A = transformer.Transform(new PointF(points["a"].X + perp2.X, points["a"].Y + perp2.Y));

            // Distortion: this tool's target use-case is to be used at the center of the image, with very short lines,
            // thus there should not be any distortion.
            using (Pen penEdges = styleHelper.GetPen(opacityFactor, transformer.Scale))
            using (Brush brush = styleHelper.GetBrush(opacityFactor))
            {
                // Force line width to 1 at all zoom levels. This tool is all about pixel-perfect precision.
                penEdges.Width = 1;
                canvas.DrawLine(penEdges, start, end);
                canvas.DrawLine(penEdges, p1A, p2A);
            }
            
            // Attached mini label.
            string text = GetTimeText();
            miniLabel.SetText(text);
            miniLabel.SetAttach(GetTimePoint(), true);
            miniLabel.Draw(canvas, transformer, opacityFactor);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            PointF c = GetTimePoint();
            if (opacityFactor > 0)
            {
                // Give priority to the mini label and the middle point to guarantee
                // we can always move them out of the way of the end points if needed.
                if (miniLabel.HitTest(point, transformer))
                    result = 4;
                else if (HitTester.HitPoint(point, c, transformer))
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
            int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            switch(handleNumber)
            {
                case 1:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["a"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["b"], point, constraintAngleSubdivisions);
                    else
                        points["a"] = point;

                    break;
                case 2:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["b"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["a"], point, constraintAngleSubdivisions);
                    else
                        points["b"] = point;

                    break;
                case 3:
                    // Recompute the fraction based on where the point was slid to.
                    PointF c = GeometryHelper.GetClosestPoint(points["a"], points["b"], point, PointLinePosition.OnSegment, 0);
                    Vector ac = new Vector(points["a"], c);
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
                            break;
                        }
                    case "MeasureLabel":
                        {
                            miniLabel = new MiniLabel(xmlReader, scale);
                            break;
                        }
                    case "DrawingStyle":
                        {
                            style = new DrawingStyle(xmlReader);
                            BindStyle();
                            break;
                        }
                    case "InfosFading":
                        {
                            infosFading.ReadXml(xmlReader);
                            break;
                        }
                    case "Measure":
                        {
                            xmlReader.ReadOuterXml();
                            break;
                        }
                    default:
                        {
                            string unparsed = xmlReader.ReadOuterXml();
                            log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                            break;
                        }
                }
            }
            
            xmlReader.ReadEndElement();
            initializing = false;
            miniLabel.SetAttach(GetTimePoint(), false);
            miniLabel.BackColor = styleHelper.Color;
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Start", XmlHelper.WritePointF(points["a"]));
                w.WriteElementString("End", XmlHelper.WritePointF(points["b"]));
                w.WriteElementString("Fraction", XmlHelper.WriteFloat(fraction));

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

        private string GetTimeText()
        {
            if (parentMetadata == null)
                return "";

            // Get linearly interpolated time at the fraction of the segment.
            return parentMetadata.GetFractionTime(infosFading.ReferenceTimestamp, fraction);
        }
        
        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("TimeSegment"));
            style.Bind(styleHelper, "Color", "color");
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
                    areaPath.AddLine(points["a"], points["b"]);
                }

                return HitTester.HitPath(point, areaPath, lineSize, false, transformer);
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
        /// Returns the coordinate of the sliding point based on the fraction.
        /// </summary>
        private PointF GetTimePoint()
        {
            return GeometryHelper.Mix(points["a"], points["b"], fraction); 
        }
        
        #endregion
    }
}
