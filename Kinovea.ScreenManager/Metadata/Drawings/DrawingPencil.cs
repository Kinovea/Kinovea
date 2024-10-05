#region License
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
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
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
    [XmlType ("Pencil")]
    public class DrawingPencil : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolPencil; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = 0;
                foreach (PointF p in pointList)
                    hash ^= p.GetHashCode();
            
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
        private List<PointF> pointList = new List<PointF>();
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private InfosFading infosFading;
        private bool initializing = true;
        private bool debugMode = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingPencil(PointF origin, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            pointList.Add(origin);
            pointList.Add(origin.Translate(1, 0));
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            styleData.Color = Color.Black;
            styleData.LineSize = 1;
            styleData.PenShape = PenShape.Solid;
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Pencil");

            styleElements = preset.Clone();
            BindStyle();
        }
        public DrawingPencil(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0, ToolManager.GetDefaultStyleElements("Pencil"))
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if(opacityFactor <= 0)
                return;
            
            using(Pen penLine = styleData.GetPen(opacityFactor, transformer.Scale))
            {
                Point[] points = transformer.Transform(pointList).ToArray();

                if (debugMode)
                {
                    // Debugging frequency of points.
                    penLine.Width = 1.0f;
                    foreach (var p in points)
                        canvas.DrawEllipse(penLine, p.Box(4));

                    canvas.DrawLines(penLine, points);
                }
                else if (initializing)
                {
                    // During initialization show a thin line.
                    penLine.Width = 1.0f;
                    canvas.DrawLines(penLine, points);
                }
                else
                { 
                    // Normal mode.
                    penLine.EndCap = LineCap.Round;
                    penLine.StartCap = LineCap.Round;

                    if (styleData.PenShape == PenShape.Dash)
                        penLine.DashStyle = DashStyle.Dash;

                    // Sanity check that the point will be visible.
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddCurve(points, 0.5f);
                        RectangleF bounds = path.GetBounds();
                        if (bounds.IsEmpty)
                        {
                            canvas.DrawLine(penLine, points[0], points[0].Translate(1, 0));
                        }
                        else
                        {
                            canvas.DrawCurve(penLine, points, 0.5f);
                        }
                    }
                }
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            pointList = pointList.Select(p => p.Translate(dx, dy)).ToList();
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0 && IsPointInObject(point, transformer))
                result = 0;
                
            return result;
        }
        public override PointF GetCopyPoint()
        {
            return pointList[0];
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
            initializing = false;
        }
        private void ParsePointList(XmlReader xmlReader, PointF scale)
        {
            pointList.Clear();
            
            xmlReader.ReadStartElement();
            
            while(xmlReader.NodeType == XmlNodeType.Element)
            {
                if(xmlReader.Name == "Point")
                {
                    PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                    PointF adapted = p.Scale(scale.X, scale.Y);
                    pointList.Add(adapted);
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
                w.WriteAttributeString("Count", pointList.Count.ToString());
                foreach (PointF p in pointList)
                    w.WriteElementString("Point", XmlHelper.WritePointF(p));

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
        
        #region IInitializable implementation
        public void InitializeMove(PointF point, Keys modifiers)
        {
            AddPoint(point, modifiers);
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
        
        #region Lower level helpers
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Pencil"));
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "pen size");
            styleElements.Bind(styleData, "PenShape", "pen shape");
        }
        private void AddPoint(PointF point, Keys modifiers)
        {
            PointF newPoint = PointF.Empty;
            int pointsUsedToComputeDirection = Math.Min(10, pointList.Count);
            
            if((modifiers & Keys.Shift) == Keys.Shift)
            {
                // Checks whether the mouse motion is more horizontal or vertical, and only keep this component of the motion.
                float dx = Math.Abs(point.X - pointList[pointList.Count - pointsUsedToComputeDirection].X);
                float dy = Math.Abs(point.Y - pointList[pointList.Count - pointsUsedToComputeDirection].Y);
                if(dx > dy)
                    newPoint = new PointF(point.X, pointList[pointList.Count - 1].Y);
                else
                    newPoint = new PointF(pointList[pointList.Count - 1].X, point.Y);
            }
            else
            {
                newPoint = point;
            }
            
            pointList.Add(newPoint);
        }
        private bool IsPointInObject(PointF point, IImageToViewportTransformer transformer)
        {
            using(GraphicsPath path = new GraphicsPath())
            {
                path.AddCurve(pointList.ToArray(), 0.5f);
                return HitTester.HitPath(point, path, styleData.LineSize, false, transformer);
            }
        }
        #endregion
    }
}