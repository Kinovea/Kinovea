#region License
/*
Copyright © Joan Charmant 2018.
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

namespace Kinovea.ScreenManager
{
    [XmlType ("Rectangle")]
    public class DrawingRectangle : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.DrawingName_Rectangle; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = quadImage.GetHashCode();
                hash ^= filled.GetHashCode();
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

                contextMenu.Add(mnuOptions);
                mnuFilled.Checked = filled;
                return contextMenu;
            }
        }
        public bool Initializing
        {
            get { return initializing; }
        }
        #endregion

        #region Members
        // Core
        private QuadrilateralF quadImage = QuadrilateralF.GetUnitSquare();
        private bool initializing = true;

        #region Menus
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuFilled = new ToolStripMenuItem();
        #endregion

        // Decoration
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private InfosFading infosFading;
        private bool filled = false;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public DrawingRectangle(PointF origin, long timestamp, long averageTimeStampsPerFrame, StyleElements preset = null, IImageToViewportTransformer transformer = null)
        {
            quadImage = new QuadrilateralF(origin, origin.Translate(50, 0), origin.Translate(50, 50), origin.Translate(0, 50));

            styleData.Color = Color.Empty;
            styleData.LineSize = 1;
            styleData.PenShape = PenShape.Solid;
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Rectangle");
            
            styleElements = preset.Clone();
            BindStyle();

            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);

            InitializeMenus();
        }
        public DrawingRectangle(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        private void InitializeMenus()
        {
            // Options
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuFilled.Image = Properties.Drawings.filled;
            mnuFilled.Click += mnuFilled_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
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

            QuadrilateralF quad = transformer.Transform(quadImage);

            int alpha = (int)(opacityFactor * 255);
            using(Pen p = styleData.GetPen(alpha, transformer.Scale))
                {
                    p.EndCap = LineCap.Square;
                    if (styleData.PenShape == PenShape.Dash)
                        p.DashStyle = DashStyle.Dash;

                canvas.DrawLine(p, quad.A, quad.B);
                canvas.DrawLine(p, quad.B, quad.C);
                canvas.DrawLine(p, quad.C, quad.D);
                canvas.DrawLine(p, quad.D, quad.A);
            }

            if (filled)
            {
                RectangleF box = new RectangleF(quad.A.X, quad.A.Y, quad.B.X - quad.A.X, quad.C.Y - quad.A.Y);
                using (SolidBrush b = styleData.GetBrush(alpha))
                    //canvas.FillRectangle(b, box);
                    canvas.FillPolygon(b, quad.ToArray());
            }
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (handleNumber < 5)
            {
                // Moving a corner.
                quadImage[handleNumber - 1] = point;
                quadImage.MakeRectangle(handleNumber - 1);
            }
            else
            {
                // Moving an edge.
                if (handleNumber == 5)
                {
                    quadImage.A = new PointF(quadImage.A.X, point.Y);
                    quadImage.B = new PointF(quadImage.B.X, point.Y);
                }
                else if (handleNumber == 6)
                {
                    quadImage.B = new PointF(point.X, quadImage.B.Y);
                    quadImage.C = new PointF(point.X, quadImage.C.Y);
                }
                else if (handleNumber == 7)
                {
                    quadImage.C = new PointF(quadImage.C.X, point.Y);
                    quadImage.D = new PointF(quadImage.D.X, point.Y);
                }
                else if (handleNumber == 8)
                {
                    quadImage.D = new PointF(point.X, quadImage.D.Y);
                    quadImage.A = new PointF(point.X, quadImage.A.Y);
                }
            }
        }
        public override void MoveDrawing(float dx, float dy, Keys modifiers)
        {
            quadImage.Translate(dx, dy);
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            double opacity = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacity <= 0)
                return -1;

            for (int i = 0; i < 4; i++)
            {
                if (HitTester.HitPoint(point, quadImage[i], transformer))
                    return i + 1;
            }

            for (int i = 0; i < 4; i++)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    PointF p1 = quadImage[i];
                    PointF p2 = quadImage[(i + 1) % 4];
                    if (!p1.NearlyCoincideWith(p2))
                    {
                        path.AddLine(p1, p2);
                        if (HitTester.HitPath(point, path, styleData.LineSize, false, transformer))
                            return i + 5;
                    }
                }
            }
            
            if (quadImage.Contains(point))
                return 0;
            
            return -1;
        }
        public override PointF GetCopyPoint()
        {
            return quadImage.GetBoundingBox().Center();
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
                    case "PointUpperLeft":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            quadImage.A = p.Scale(scale.X, scale.Y);
                            break;
                        }
                    case "PointUpperRight":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            quadImage.B = p.Scale(scale.X, scale.Y);
                            break;
                        }
                    case "PointLowerRight":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            quadImage.C = p.Scale(scale.X, scale.Y);
                            break;
                        }
                    case "PointLowerLeft":
                        {
                            PointF p = XmlHelper.ParsePointF(xmlReader.ReadElementContentAsString());
                            quadImage.D = p.Scale(scale.X, scale.Y);
                            break;
                        }
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
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("PointUpperLeft", XmlHelper.WritePointF(quadImage.A));
                w.WriteElementString("PointUpperRight", XmlHelper.WritePointF(quadImage.B));
                w.WriteElementString("PointLowerRight", XmlHelper.WritePointF(quadImage.C));
                w.WriteElementString("PointLowerLeft", XmlHelper.WritePointF(quadImage.D));
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

        #region Context menu
        private void mnuFilled_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            filled = !mnuFilled.Checked;
            InvalidateFromMenu(sender);
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
        
        #region Lower level helpers
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Rectangle"));
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "LineSize", "line size");
            styleElements.Bind(styleData, "PenShape", "pen shape");
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
            // Options
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuFilled.Text = ScreenManagerLang.DrawingCircle_Filled;
        }
        #endregion
    }
}