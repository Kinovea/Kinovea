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
    [XmlType("TimePolyline")]
    public class DrawingTimePolyline : AbstractDrawing, IKvaSerializable, IDecorable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return "Trajectory curve"; }
        }
        public override int ContentHash
        {
            get
            {
                int iHash = 0;
                iHash ^= styleData.ContentHash;
                iHash ^= infosFading.ContentHash;
                return iHash;
            }
        }
        public StyleElements StyleElements
        {
            get { return styleElements; }
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
                //List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                //return contextMenu; 
                return null;
            }
        }
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private Dictionary<string, long> times = new Dictionary<string, long>();
        private StyleData styleData = new StyleData();
        private StyleElements styleElements = new StyleElements();
        private InfosFading infosFading;
        // TODO: move to abstract drawing and setup after KVA import.
        private long creationTimestamp;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingTimePolyline(PointF origin, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null)
        {
            points["0"] = origin;
            points["1"] = origin.Translate(50, 50);

            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Polyline");
            
            styleElements = preset.Clone();
            BindStyle();
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            creationTimestamp = timestamp;
        }
        public DrawingTimePolyline(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            //double opacityFactor = infosFading.geto(trackingTimestamps, currentTimestamp);
            double opacityFactor = 1.0;
            if (opacityFactor <= 0)
                return;

            // TODO:
            // If only one point show it as a point.

            Point[] pointList = transformer.Transform(points.Values).ToArray();
            
            // TODO: 
            // Calculate correct location of points for this image based on CameraTransformer.
            

            using (Pen penEdges = styleData.GetPen((int)(opacityFactor * 255), transformer.Scale))
            {
                Point[] path = pointList.ToArray();
                DrawPath(canvas, penEdges, path);
                
                if (styleData.LineEnding == LineEnding.StartArrow || styleData.LineEnding == LineEnding.DoubleArrow)
                    ArrowHelper.Draw(canvas, penEdges, pointList[0], pointList[1]);

                if (styleData.LineEnding == LineEnding.EndArrow || styleData.LineEnding == LineEnding.DoubleArrow)
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
            penEdges.DashStyle = styleData.LineShape == LineShape.Dash ? DashStyle.Dash : DashStyle.Solid;
                        
            switch (styleData.LineShape)
            {
                case LineShape.Squiggle:
                    canvas.DrawSquigglyLines(penEdges, path);
                    break;
                case LineShape.Dash:
                case LineShape.Solid:
                    if (styleData.Curved)
                        canvas.DrawCurve(penEdges, path);
                    else
                        canvas.DrawLines(penEdges, path);
                    break;
            }
        }

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            int result = -1;
            //double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            double opacity = 1.0;
            if (opacity <= 0)
                return -1;

            foreach (KeyValuePair<string, PointF> p in points)
            {
                if (HitTester.HitPoint(p.Value, point, transformer))
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
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            List<string> keys = points.Keys.ToList();
            foreach (string key in keys)
                points[key] = points[key].Translate(dx, dy);
            
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

        #region Lower level helpers
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Polyline"));
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "line size");
            styleElements.Bind(styleData, "LineShape", "line shape");
            styleElements.Bind(styleData, "LineEnding", "arrows");
            styleElements.Bind(styleData, "Curved", "curved");
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
                    
                    if (styleData.Curved)
                        path.AddCurve(pp.ToArray());
                    else
                        path.AddLines(pp.ToArray());
                }

                return HitTester.HitPath(point, path, styleData.LineSize, false, transformer);
            }
        }

        #endregion
    }
}
