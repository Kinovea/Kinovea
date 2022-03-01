
/*
Copyright © Joan Charmant 2008.
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
    [XmlType ("Angle")]
    public class DrawingAngle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, ITrackable, IMeasurable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.ToolTip_DrawingToolAngle2D; }
        }
        public override int ContentHash
        {
            get 
            {
                int hash = 0;

                // The hash of positions will be taken into account by trackability manager.
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
            get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();

                mnuSignedAngle.Text = ScreenManagerLang.mnuSignedAngle;
                mnuCounterClockwise.Text = ScreenManagerLang.mnuCounterClockwise;
                mnuSupplementaryAngle.Text = ScreenManagerLang.mnuSupplementaryAngle;
                contextMenu.AddRange(new ToolStripItem[] { mnuSignedAngle, mnuCounterClockwise, mnuSupplementaryAngle});
                
                return contextMenu; 
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        public AngleOptions AngleOptions
        {
            get { return new AngleOptions(signedAngle, counterClockwise, supplementaryAngle); }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private long trackingTimestamps = -1;
        private bool initializing = true;
        
        private AngleHelper angleHelper = new AngleHelper();
        private DrawingStyle style;
        private StyleHelper styleHelper = new StyleHelper();
        private InfosFading infosFading;
        private bool signedAngle = true;
        private bool counterClockwise = true;
        private bool supplementaryAngle = false;
        
        private ToolStripMenuItem mnuSignedAngle = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCounterClockwise = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSupplementaryAngle = new ToolStripMenuItem();

        private const int defaultBackgroundAlpha = 92;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingAngle(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            int length = 50;
            if (transformer != null)
                length = transformer.Untransform(50);

            points.Add("o", origin);
            points.Add("a", origin.Translate(length, 0));
            points.Add("b", origin.Translate(0, -length));

            styleHelper.Bicolor = new Bicolor(Color.Empty);
            styleHelper.Font = new Font("Arial", 12, FontStyle.Bold);
            if (preset == null)
                preset = ToolManager.GetStylePreset("Angle");

            style = preset.Clone();
            BindStyle();
            
            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            mnuSignedAngle.Click += mnuSignedAngle_Click;
            mnuSignedAngle.Checked = signedAngle;
            mnuCounterClockwise.Click += mnuCounterClockwise_Click;
            mnuCounterClockwise.Checked = counterClockwise;
            mnuSupplementaryAngle.Click += mnuSupplementaryAngle_Click;
            mnuSupplementaryAngle.Checked = supplementaryAngle;
        }
        public DrawingAngle(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacityFactor <= 0)
                return;
            
            ComputeValues(transformer);
            
            Point pointO = transformer.Transform(points["o"]);
            Point pointA = transformer.Transform(points["a"]);
            Point pointB = transformer.Transform(points["b"]);
            Rectangle boundingBox = transformer.Transform(angleHelper.SweepAngle.BoundingBox);

            if (boundingBox.Size == Size.Empty)
                return;

            using(Pen penEdges = styleHelper.GetBackgroundPen((int)(opacityFactor*255)))
            using(SolidBrush brushEdges = styleHelper.GetBackgroundBrush((int)(opacityFactor*255)))
            using(SolidBrush brushFill = styleHelper.GetBackgroundBrush((int)(opacityFactor*defaultBackgroundAlpha)))
            {
                penEdges.Width = 2;
                
                // Disk section
                canvas.FillPie(brushFill, boundingBox, angleHelper.SweepAngle.Start, angleHelper.SweepAngle.Sweep);
                canvas.DrawArc(penEdges, boundingBox, angleHelper.SweepAngle.Start, angleHelper.SweepAngle.Sweep);

                // Edges
                penEdges.DashStyle = DashStyle.Dash;
                canvas.DrawLine(penEdges, pointO, pointA);
                penEdges.DashStyle = DashStyle.Solid;
                canvas.DrawLine(penEdges, pointO, pointB);
    
                // Handlers
                canvas.DrawEllipse(penEdges, pointO.Box(3));
                canvas.FillEllipse(brushEdges, pointA.Box(3));
                canvas.FillEllipse(brushEdges, pointB.Box(3));

                angleHelper.DrawText(canvas, opacityFactor, brushFill, pointO, transformer, CalibrationHelper, styleHelper);
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacityFactor > 0)
            {
                if (HitTester.HitTest(points["o"], point, transformer))
                    result = 1;
                else if (HitTester.HitTest(points["a"], point, transformer))
                    result = 2;
                else if (HitTester.HitTest(points["b"], point, transformer))
                    result = 3;
                else if (IsPointInObject(point))
                    result = 0;
            }
            
            return result;
        }
        public override void MoveHandle(PointF point, int handle, Keys modifiers)
        {
            int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            switch (handle)
            {
                case 1:
                    points["o"] = point;
                    SignalTrackablePointMoved("o");
                    break;
                case 2:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["a"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["o"], point, constraintAngleSubdivisions);
                    else
                        points["a"] = point;
                    
                    SignalTrackablePointMoved("a");
                    break;
                case 3:
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        points["b"] = GeometryHelper.GetPointAtClosestRotationStepCardinal(points["o"], point, constraintAngleSubdivisions);
                    else
                        points["b"] = point;
                    
                    SignalTrackablePointMoved("b");
                    break;
                default:
                    break;
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            points["o"] = points["o"].Translate(dx, dy);
            points["a"] = points["a"].Translate(dx, dy);
            points["b"] = points["b"].Translate(dx, dy);
            SignalAllTrackablePointsMoved();
        }
        public override PointF GetCopyPoint()
        {
            return points["o"];
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
                    case "PointO":
                        points["o"] = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        break;
                    case "PointA":
                        points["a"] = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        break;
                    case "PointB":
                        points["b"] = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                        break;
                    case "Signed":
                        signedAngle = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "CCW":
                        counterClockwise = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
                    case "Supplementary":
                        supplementaryAngle = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
                        break;
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

            points["o"] = points["o"].Scale(scale.X, scale.Y);
            points["a"] = points["a"].Scale(scale.X, scale.Y);
            points["b"] = points["b"].Scale(scale.X, scale.Y);
            SignalAllTrackablePointsMoved();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("PointO", XmlHelper.WritePointF(points["o"]));
                w.WriteElementString("PointA", XmlHelper.WritePointF(points["a"]));
                w.WriteElementString("PointB", XmlHelper.WritePointF(points["b"]));
                w.WriteElementString("Signed", XmlHelper.WriteBoolean(signedAngle));
                w.WriteElementString("CCW", XmlHelper.WriteBoolean(counterClockwise));
                w.WriteElementString("Supplementary", XmlHelper.WriteBoolean(supplementaryAngle));
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
        public MeasuredDataAngle CollectMeasuredData()
        {
            angleHelper.Update(points["o"], points["a"], points["b"], signedAngle, counterClockwise, supplementaryAngle, CalibrationHelper);
            return MeasurementSerializationHelper.CollectAngle(name, angleHelper, CalibrationHelper);
        }
        #endregion
        
        #region IInitializable implementation
        public void InitializeMove(PointF point, Keys modifiers)
        {
            MoveHandle(point, 3, modifiers);
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
            get { return styleHelper.Bicolor.Background; }
        }
        public TrackingProfile CustomTrackingProfile
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
        
        #region Specific context menu
        
        private void mnuSignedAngle_Click(object sender, EventArgs e)
        {
            mnuSignedAngle.Checked = !mnuSignedAngle.Checked;

            signedAngle = mnuSignedAngle.Checked;
            SignalAllTrackablePointsMoved();
            InvalidateFromMenu(sender);
        }

        private void mnuCounterClockwise_Click(object sender, EventArgs e)
        {
            mnuCounterClockwise.Checked = !mnuCounterClockwise.Checked;

            counterClockwise = mnuCounterClockwise.Checked;
            SignalAllTrackablePointsMoved();
            InvalidateFromMenu(sender);
        }

        private void mnuSupplementaryAngle_Click(object sender, EventArgs e)
        {
            mnuSupplementaryAngle.Checked = !mnuSupplementaryAngle.Checked;

            supplementaryAngle = mnuSupplementaryAngle.Checked;
            SignalAllTrackablePointsMoved();
            InvalidateFromMenu(sender);
        }

        #endregion

        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("Angle"));
            style.Bind(styleHelper, "Bicolor", "line color");
            style.Bind(styleHelper, "Font", "font size");
        }
        private void ComputeValues(IImageToViewportTransformer transformer)
        {
            FixIfNull(transformer);
            angleHelper.UpdateTextDistance(styleHelper.Font.Size / 12.0f);
            angleHelper.Update(points["o"], points["a"], points["b"], signedAngle, counterClockwise, supplementaryAngle, CalibrationHelper);
        }
        private void FixIfNull(IImageToViewportTransformer transformer)
        {
            int length = transformer.Untransform(50);

            if (points["a"].NearlyCoincideWith(points["o"]))
                points["a"] = points["o"].Translate(0, -length);

            if (points["b"].NearlyCoincideWith(points["o"]))
                points["b"] = points["o"].Translate(length, 0);
        }
        private bool IsPointInObject(PointF point)
        {
            return angleHelper.SweepAngle.Hit(point);
        }
        #endregion
    } 
}