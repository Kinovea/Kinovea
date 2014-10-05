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
using Kinovea.Video;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Circle")]
    public class DrawingCircle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public override string DisplayName
        {
            get {  return ScreenManagerLang.ToolTip_DrawingToolCircle; }
        }
        public override int ContentHash
        {
            get 
            { 
                int iHash = center.GetHashCode();
                iHash ^= radius.GetHashCode();
                iHash ^= styleHelper.ContentHash;
                return iHash;
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
            get { return null; }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        #endregion

        #region Members
        // Core
        private PointF center;
        private int radius;
        private bool selected;
        private bool initializing = true;
        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private InfosFading infosFading;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingCircle(Point center, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null, IImageToViewportTransformer transformer = null)
        {
            this.center = center;

            if (transformer != null)
                this.radius = transformer.Untransform(25);

            this.radius = Math.Min(radius, 10);
            this.infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            styleHelper.Color = Color.Empty;
            styleHelper.LineSize = 1;

            if (preset == null)
                preset = ToolManager.GetStylePreset("Circle");
            
            style = preset.Clone();
            BindStyle();
        }
        public DrawingCircle(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(Point.Empty, 0, 0)
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
            this.selected = selected;
            
            using(Pen p = styleHelper.GetPen(alpha, transformer.Scale))
            {
                Rectangle boundingBox = transformer.Transform(center.Box(radius));
                canvas.DrawEllipse(p, boundingBox);
                
                if(selected)
                {
                    // Handler: arc in lower right quadrant.
                    p.Color = p.Color.Invert();
                    canvas.DrawArc(p, boundingBox, 25, 40);
                }
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            // User is dragging the outline of the circle, figure out the new radius at this point.
            float shiftX = Math.Abs(point.X - center.X);
            float shiftY = Math.Abs(point.Y - center.Y);
            radius = (int)Math.Sqrt((shiftX*shiftX) + (shiftY*shiftY));
            radius = Math.Max(radius, 10);
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers, bool zooming)
        {
            center = center.Translate(dx, dy);
        }
        public override int HitTest(Point point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity > 0)
            {
                if (selected && IsPointOnHandler(point, transformer))
                    result = 1;
                else if (IsPointInObject(point, transformer))
                    result = 0;
            }
            return result;
        }        
        #endregion
        
        #region KVA Serialization
        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            if (xmlReader.MoveToAttribute("id"))
                identifier = new Guid(xmlReader.ReadContentAsString());

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
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Origin", XmlHelper.WritePointF(center));
                w.WriteElementString("Radius", radius.ToString());
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
        
        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "LineSize", "pen size");
        }
        private bool IsPointInObject(Point point, IImageToViewportTransformer transformer)
        {
            bool hit = false;
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddEllipse(center.Box(radius + styleHelper.LineSize));
                hit = HitTester.HitTest(areaPath, point, 0, true, transformer);
            }
            return hit;
        }
        private bool IsPointOnHandler(Point point, IImageToViewportTransformer transformer)
        {
            if(radius < 0)
                return false;
            
            using(GraphicsPath areaPath = new GraphicsPath())
            {
                areaPath.AddArc(center.Box(radius), 25, 40);
                return HitTester.HitTest(areaPath, point, styleHelper.LineSize, false, transformer);
            }
        }
        #endregion
    }

       
}