#region License
/*
Copyright © Joan Charmant 2014.
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
        public override string ToolDisplayName
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
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.CopyPaste; }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                if (initializing)
                {
                    mnuAddThenFinish.Text = ScreenManagerLang.mnuPolyline_FinishHere;
                    mnuFinish.Text = ScreenManagerLang.mnuPolyline_FinishAtPrevious;
                    mnuCloseMenu.Text = ScreenManagerLang.mnuPolyline_CloseMenu;
                    contextMenu.Add(mnuAddThenFinish);
                    contextMenu.Add(mnuFinish);
                    contextMenu.Add(mnuCloseMenu);
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
        private long trackingTimestamps = -1;
        private bool initializing = true;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;

        // Context menu
        private ToolStripMenuItem mnuFinish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAddThenFinish = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseMenu = new ToolStripMenuItem();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPolyline(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            points["0"] = origin;
            points["1"] = origin;

            if (preset == null)
                preset = ToolManager.GetStylePreset("Polyline");
            
            style = preset.Clone();
            BindStyle();
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            mnuFinish.Click += mnuFinish_Click;
            mnuAddThenFinish.Click += mnuAddThenFinish_Click;
            mnuCloseMenu.Click += mnuCloseMenu_Click;

            mnuFinish.Image = Properties.Drawings.tick_small;
            mnuAddThenFinish.Image = Properties.Drawings.plus_small;
            mnuCloseMenu.Image = Properties.Drawings.cross_small;
        }
        public DrawingPolyline(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
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
                        Point[] path = pointList.TakeWhile((p, i) => i <= pointList.Length - 2).ToArray();
                        DrawPath(canvas, penEdges, path);
                    }
                }
                else
                {
                    Point[] path = pointList.ToArray();
                    DrawPath(canvas, penEdges, path);
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
        private void DrawPath(Graphics canvas, Pen penEdges, Point[] path)
        {
            penEdges.EndCap = LineCap.NoAnchor;
            penEdges.DashStyle = styleHelper.LineShape == LineShape.Dash ? DashStyle.Dash : DashStyle.Solid;
                        
            switch (styleHelper.LineShape)
            {
                case LineShape.Squiggle:
                    canvas.DrawSquigglyLines(penEdges, path);
                    break;
                case LineShape.Dash:
                case LineShape.Solid:
                    if (styleHelper.Curved)
                        canvas.DrawCurve(penEdges, path);
                    else
                        canvas.DrawLines(penEdges, path);
                    break;
            }
        }

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity <= 0)
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
        public override PointF GetCopyPoint()
        {
            return points["0"];
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

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "PointList":
                        ParsePointList(xmlReader, scale);
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
            
            // Only add the point if it's not the same as the last one.
            // This is mostly to fix a "click" instead of "drag" of first point.
            if (points.Count < 2)
                return null;

            string lastCommittedIndex = (points.Count - 2).ToString();
            PointF lastPoint = points[lastCommittedIndex];
            if (point == lastPoint)
                return null;

            // Commit point
            points[(points.Count - 1).ToString()] = point;

            // Create new dangling point
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
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            if (!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");

            points[name] = value;
            this.trackingTimestamps = trackingTimestamps;
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
        private void mnuFinish_Click(object sender, EventArgs e)
        {
            InitializeEndFromMenu(sender, true);
            InvalidateFromMenu(sender);
        }
        private void mnuAddThenFinish_Click(object sender, EventArgs e)
        {
            InitializeEndFromMenu(sender, false);
            InvalidateFromMenu(sender);
        }
        private void mnuCloseMenu_Click(object sender, EventArgs e)
        {
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("Polyline"));
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "line size");
            style.Bind(styleHelper, "LineShape", "line shape");
            style.Bind(styleHelper, "LineEnding", "arrows");
            style.Bind(styleHelper, "Curved", "curved");
        }
        private bool IsPointInObject(PointF point, DistortionHelper distorter, IImageToViewportTransformer transformer)
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
                    List<PointF> pp = new List<PointF>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        PointF p = points[i.ToString()];
                        if (pp.Count == 0 || p != pp[pp.Count-1])
                            pp.Add(p);
                    }
                    
                    if (styleHelper.Curved)
                        path.AddCurve(pp.ToArray());
                    else
                        path.AddLines(pp.ToArray());
                }

                return HitTester.HitTest(path, point, styleHelper.LineSize, false, transformer);
            }
        }

        #endregion
    }
}
