#region License
/*
Copyright © Joan Charmant 2014.
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
using System.Linq;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType("Polyline")]
    public class DrawingPolyline : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable, ITrackable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        #endregion
        
        #region Properties
        public override string DisplayName
        {
            get { return "Polyline"; }
        }
        public override int ContentHash
        {
            get
            {
                int iHash = 0;
                iHash ^= styleHelper.ContentHash;
                iHash ^= infosFading.ContentHash;
                iHash ^= curve.GetHashCode();
                return iHash;
            }
        }
        public DrawingStyle DrawingStyle
        {
            get { return style; }
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
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                if (initializing)
                {
                    mnuAddThenFinish.Text = "Finish polyline here";
                    mnuFinish.Text = "Finish polyline at previous point";
                    mnuCloseMenu.Text = "Close this menu";
                    contextMenu.Add(mnuAddThenFinish);
                    contextMenu.Add(mnuFinish);
                    contextMenu.Add(mnuCloseMenu);
                }
                else
                {
                    mnuCurve.Text = "Curve";
                    mnuCurve.Checked = curve;
                    contextMenu.Add(mnuCurve);
                }
                
                return contextMenu; 
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool tracking;
        private bool initializing = true;
        private bool curve = false;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;

        // Context menu
        private ToolStripMenuItem mnuCurve = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFinish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAddThenFinish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseMenu = new ToolStripMenuItem();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPolyline(Point start, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset, IImageToViewportTransformer transformer)
        {
            points["0"] = start;
            points["1"] = start;
            
            // Decoration
            styleHelper.Color = Color.DarkSlateGray;
            styleHelper.LineSize = 1;
            if (preset != null)
            {
                style = preset.Clone();
                BindStyle();
            }

            // Fading
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            mnuCurve.Click += mnuCurve_Click;
            mnuFinish.Click += mnuFinish_Click;
            mnuAddThenFinish.Click += mnuAddThenFinish_Click;
            mnuCloseMenu.Click += mnuCloseMenu_Click;

            mnuCurve.Image = Properties.Drawings.curve;
            mnuFinish.Image = Properties.Drawings.tick_small;
            mnuAddThenFinish.Image = Properties.Drawings.plus_small;
            mnuCloseMenu.Image = Properties.Drawings.cross_small;
        }
        public DrawingPolyline(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(Point.Empty, 0, 0, ToolManager.Polyline.StylePreset.Clone(), null)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);

            if (tracking)
                opacityFactor = 1.0;

            if (opacityFactor <= 0)
                return;

            if (points.Count < 2)
                return;

            Point[] pointList = transformer.Transform(points.Values).ToArray();

            using (Pen penEdges = styleHelper.GetPen((int)(opacityFactor * 255), transformer.Scale))
            {
                PointF danglingStart;
                PointF danglingEnd;
                
                if (initializing)
                {
                    danglingStart = pointList[pointList.Length - 2];
                    danglingEnd = pointList[pointList.Length - 1];

                    penEdges.DashStyle = DashStyle.Dot;
                    canvas.DrawLine(penEdges, danglingStart, danglingEnd);

                    if (pointList.Length > 2)
                    {
                        penEdges.EndCap = LineCap.NoAnchor;
                        penEdges.DashStyle = styleHelper.LineShape == LineShape.Dash ? DashStyle.Dash : DashStyle.Solid;

                        Point[] path = pointList.TakeWhile((p, i) => i <= pointList.Length - 2).ToArray();
                            
                        if (curve)
                            canvas.DrawCurve(penEdges, path);
                        else
                            canvas.DrawLines(penEdges, path);
                    }
                }
                else
                {
                    penEdges.EndCap = LineCap.NoAnchor;
                    penEdges.DashStyle = styleHelper.LineShape == LineShape.Dash ? DashStyle.Dash : DashStyle.Solid;

                    Point[] path = pointList.TakeWhile((p, i) => i <= pointList.Length - 1).ToArray();

                    if (curve)
                        canvas.DrawCurve(penEdges, path);
                    else
                        canvas.DrawLines(penEdges, path);
                }

                if (styleHelper.LineEnding == LineEnding.StartArrow || styleHelper.LineEnding == LineEnding.DoubleArrow)
                    ArrowHelper.Draw(canvas, penEdges, pointList[0], pointList[1]);

                if (styleHelper.LineEnding == LineEnding.EndArrow || styleHelper.LineEnding == LineEnding.DoubleArrow)
                    ArrowHelper.Draw(canvas, penEdges, pointList[pointList.Length - 1], pointList[pointList.Length - 2]);

                // Handlers
                if (selected)
                {
                    penEdges.Width = 2;
                    foreach (PointF p in pointList)
                        canvas.FillEllipse(penEdges.Brush, p.Box(3));
                }
                
            }

        }
        public override int HitTest(Point point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);

            if (!tracking && opacity <= 0)
                return -1;

            foreach (KeyValuePair<string, PointF> p in points)
            {
                if (HitTester.HitTest(p.Value, point, transformer))
                    result = int.Parse(p.Key) + 1;
            }

            if (result == -1 && IsPointInObject(point, distorter, transformer))
                result = 0;
            
            return result;
        }
        public override void MoveHandle(PointF point, int handle, Keys modifiers)
        {
            //int constraintAngleSubdivisions = 8; // (Constraint by 45° steps).
            
            string index = (handle - 1).ToString();
            if (points[index] != null)
            {
                points[index] = point;
                SignalTrackablePointMoved(index);
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            List<string> keys = points.Keys.ToList();
            foreach (string key in keys)
                points[key] = points[key].Translate(dx, dy);
            
            SignalAllTrackablePointsMoved();
        }
        #endregion

        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "PointList":
                        ParsePointList(xmlReader, scale);
                        break;
                    case "Curve":
                        curve = XmlHelper.ParseBoolean(xmlReader.ReadElementContentAsString());
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

            SignalAllTrackablePointsMoved();
        }
        private void ParsePointList(XmlReader xmlReader, PointF scale)
        {
            points.Clear();

            xmlReader.ReadStartElement();

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                if (xmlReader.Name == "Point")
                {
                    string key = "0";
                    if (xmlReader.MoveToAttribute("key"))
                        key = xmlReader.ReadContentAsString();

                    if (points.ContainsKey(key))
                        continue;

                    xmlReader.MoveToContent();
                    PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                    PointF adapted = p.Scale(scale.X, scale.Y);

                    points[key] = adapted;
                }
                else
                {
                    string unparsed = xmlReader.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            xmlReader.ReadEndElement();
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteStartElement("PointList");
                w.WriteAttributeString("Count", points.Count.ToString());
                foreach (var pair in points)
                {
                    w.WriteStartElement("Point");
                    w.WriteAttributeString("key", pair.Key);
                    w.WriteString(XmlHelper.WritePointF(pair.Value));
                    w.WriteEndElement();
                }

                w.WriteEndElement();

                w.WriteElementString("Curve", curve.ToString().ToLower());
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
            MoveHandle(point, points.Count, modifiers);
        }
        public string InitializeCommit(PointF point)
        {
            // Contrary to most other drawings, polyline is a multi-step initializable.
            // We add a point and we do not get out of initialization mode.
            points[(points.Count - 1).ToString()] = point;

            string key = points.Count.ToString();
            points.Add(key, point);

            return key;
        }
        public string InitializeEnd(bool cancelCurrentPoint)
        {
            initializing = false;

            string key = null;
            if (cancelCurrentPoint)
            {
                key = (points.Count - 1).ToString();
                points.Remove(key);
            }

            return key;
        }
        #endregion

        #region ITrackable implementation and support.
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
            if (!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");

            points[name] = value;
        }
        private void SignalAllTrackablePointsMoved()
        {
            if (TrackablePointMoved == null)
                return;

            foreach (KeyValuePair<string, PointF> p in points)
                TrackablePointMoved(this, new TrackablePointMovedEventArgs(p.Key, p.Value));
        }
        private void SignalTrackablePointMoved(string name)
        {
            if (TrackablePointMoved == null || !points.ContainsKey(name))
                return;

            TrackablePointMoved(this, new TrackablePointMovedEventArgs(name, points[name]));
        }
        #endregion

        #region Context menu
        private void mnuCurve_Click(object sender, EventArgs e)
        {
            curve = !curve;
            CallInvalidateFromMenu(sender);
        }
        private void mnuFinish_Click(object sender, EventArgs e)
        {
            // TODO: remove point from trackability.
            InitializeEnd(true);

            CallInvalidateFromMenu(sender);
        }
        private void mnuAddThenFinish_Click(object sender, EventArgs e)
        {
            InitializeEnd(false);
            CallInvalidateFromMenu(sender);
        }
        private void mnuCloseMenu_Click(object sender, EventArgs e)
        {
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
        private bool IsPointInObject(Point point, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            if (points.Count < 1)
                return false;

            using (GraphicsPath path = new GraphicsPath())
            {
                if (points.Count == 1)
                {
                    path.AddRectangle(points["0"].Box(5));
                }
                else
                {
                    // If any two points are conflated it throws an exception.
                    List<PointF> uniquePoints = new List<PointF>();
                    for(int i = 0; i < points.Count; i++)
                    {
                        PointF p = points[i.ToString()];
                        if (!uniquePoints.Contains(p))
                            uniquePoints.Add(p);
                    }

                    if (curve)
                        path.AddCurve(uniquePoints.ToArray());
                    else
                        path.AddLines(uniquePoints.ToArray());
                }

                return HitTester.HitTest(path, point, styleHelper.LineSize, false, transformer);
            }
        }

        #endregion
    }
}
