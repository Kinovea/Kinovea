#region License
/*
Copyright © Joan Charmant 2015.
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
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType("TestGrid")]
    public class DrawingTestGrid : AbstractDrawing, IDecorable, IScalable, IKvaSerializable
    {
        #region Properties
        public override string ToolDisplayName
        {
            get { return ScreenManagerLang.DrawingName_TestGrid; }
        }
        public override int ContentHash
        {
            get 
            {
                int hash = Visible.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= showHorizontalAxis.GetHashCode();
                hash ^= showVerticalAxis.GetHashCode();
                hash ^= showFraming.GetHashCode();
                hash ^= showThirds.GetHashCode();
                return hash; 
            }
        }
        public DrawingStyle DrawingStyle
        {
            get { return style; }
        }
        public override InfosFading InfosFading
        {
            get { return null; }
            set {  }
        }
        public override List<ToolStripItem> ContextMenu
        {
            get 
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuOptions,
                    new ToolStripSeparator(),
                    mnuHide,
                });

                mnuShowHorizontalAxis.Checked = showHorizontalAxis;
                mnuShowVerticalAxis.Checked = showVerticalAxis;
                mnuShowFraming.Checked = showFraming;
                mnuShowThirds.Checked = showThirds;

                return contextMenu;
            }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize; }
        }
        public bool Visible { get; set; }
        #endregion

        #region Members
        private SizeF imageSize;
        
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private Dictionary<string, GridLine> gridLines = new Dictionary<string, GridLine>();

        // Options
        private bool showHorizontalAxis = true;
        private bool showVerticalAxis = true;
        private bool showFraming = true;
        private bool showThirds = false;

        #region Menus
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowHorizontalAxis = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowVerticalAxis = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowFraming = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowThirds = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHide = new ToolStripMenuItem();
        #endregion
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingTestGrid(DrawingStyle preset = null)
        {
            CreateGridlines();

            // Decoration
            styleHelper.Color = Color.Red;
            if (preset != null)
            {
                style = preset.Clone();
                BindStyle();
            }

            InitializeMenus();
        }

        private void InitializeMenus()
        {
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowHorizontalAxis.Click += MnuShowHorizontalAxis_Click;
            mnuShowVerticalAxis.Click += MnuShowVerticalAxis_Click;
            mnuShowFraming.Click += MnuShowFraming_Click;
            mnuShowThirds.Click += MnuShowThirds_Click;
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowHorizontalAxis,
                mnuShowVerticalAxis,
                mnuShowFraming,
                mnuShowThirds,
            });

            mnuHide.Image = Properties.Drawings.hide;
            mnuHide.Click += MnuHide_Click;
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if (!Visible || imageSize == SizeF.Empty)
                return;

            DrawGrid(canvas, transformer);
        }
        private void DrawGrid(Graphics canvas, IImageToViewportTransformer transformer)
        {
            //Pen pen = new Pen(Color.Red, 1);
            Pen p = styleHelper.GetPen(255);

            if (showHorizontalAxis)
                DrawLine(canvas, transformer, p, gridLines["horizontal"]);
            
            if (showVerticalAxis)    
                DrawLine(canvas, transformer, p, gridLines["vertical"]);
            
            if (showFraming)
            {
                DrawLine(canvas, transformer, p, gridLines["frameLeft"]);
                DrawLine(canvas, transformer, p, gridLines["frameTop"]);
                DrawLine(canvas, transformer, p, gridLines["frameRight"]);
                DrawLine(canvas, transformer, p, gridLines["frameBottom"]);
            }
            
            if (showThirds)
            {
                DrawLine(canvas, transformer, p, gridLines["thirdsLeft"]);
                DrawLine(canvas, transformer, p, gridLines["thirdsTop"]);
                DrawLine(canvas, transformer, p, gridLines["thirdsRight"]);
                DrawLine(canvas, transformer, p, gridLines["thirdsBottom"]);
            }
            
            p.Dispose();
        }
        private void DrawLine(Graphics canvas, IImageToViewportTransformer transformer, Pen pen, GridLine gridLine)
        {
            canvas.DrawLine(pen, transformer.Transform(Map(gridLine.Start)), transformer.Transform(Map(gridLine.End)));
        }

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            if (!Visible)
                return -1;

            // The individual lines are not movable so we always only ever return -1 (no hit) or 0 (hit).
            // However we need to treat them separately because a given grid line might not be visible.
            
            void addGridLine(GraphicsPath path, GridLine line)
            {
                path.StartFigure();
                path.AddLine(Map(line.Start), Map(line.End));
                path.CloseFigure();
            }
            
            bool hit = false;
            List<GridLine> visibleGridLines = new List<GridLine>();
            using (GraphicsPath path = new GraphicsPath())
            {
                if (showHorizontalAxis)
                    addGridLine(path, gridLines["horizontal"]);

                if (showVerticalAxis)
                    addGridLine(path, gridLines["vertical"]);
                
                if (showFraming)
                {
                    addGridLine(path, gridLines["frameLeft"]);
                    addGridLine(path, gridLines["frameTop"]);
                    addGridLine(path, gridLines["frameRight"]);
                    addGridLine(path, gridLines["frameBottom"]);
                }

                if (showThirds)
                {
                    addGridLine(path, gridLines["thirdsLeft"]);
                    addGridLine(path, gridLines["thirdsTop"]);
                    addGridLine(path, gridLines["thirdsRight"]);
                    addGridLine(path, gridLines["thirdsBottom"]);
                }

                hit = HitTester.HitPath(point, path, 2, false, transformer);
            }
            
            return hit ? 0 : -1;
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
        }
        public override PointF GetCopyPoint()
        {
            return new PointF(imageSize.Width / 2, imageSize.Height / 2);
        }
        
        #endregion

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            this.imageSize = new SizeF(imageSize.Width, imageSize.Height);
        }
        #endregion

        #region Serialization
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Visible", XmlHelper.WriteBoolean(Visible));
                w.WriteElementString("ShowHorizontalAxis", XmlHelper.WriteBoolean(showHorizontalAxis));
                w.WriteElementString("ShowVerticalAxis", XmlHelper.WriteBoolean(showVerticalAxis));
                w.WriteElementString("ShowFraming", XmlHelper.WriteBoolean(showFraming));
                w.WriteElementString("ShowThirds", XmlHelper.WriteBoolean(showThirds));
            }

            if (ShouldSerializeStyle(filter))
            {
                w.WriteStartElement("DrawingStyle");
                style.WriteXml(w);
                w.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper)
        {
            // This method is just to conform to the IKvaSerializable interface and support undo/redo.
            ReadXml(xmlReader);
        }

        public void ReadXml(XmlReader r)
        {
            if (r.MoveToAttribute("id"))
                identifier = new Guid(r.ReadContentAsString());

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Visible":
                        Visible = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowHorizontalAxis":
                        showHorizontalAxis = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowVerticalAxis":
                        showVerticalAxis = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowFraming":
                        showFraming = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowThirds":
                        showThirds = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "DrawingStyle":
                        style = new DrawingStyle(r);
                        BindStyle();
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        break;
                }
            }

            r.ReadEndElement();
        }

        #endregion

        #region Context menu
        private void MnuHide_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            Visible = false;
            InvalidateFromMenu(sender);
        }
        private void MnuShowHorizontalAxis_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showHorizontalAxis = !mnuShowHorizontalAxis.Checked;
            if (IsInvisible())
                showHorizontalAxis = true;
            InvalidateFromMenu(sender);
        }

        private void MnuShowVerticalAxis_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showVerticalAxis = !mnuShowVerticalAxis.Checked;
            if (IsInvisible())
                showVerticalAxis = true;
            InvalidateFromMenu(sender);
        }

        private void MnuShowFraming_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showFraming = !mnuShowFraming.Checked;
            if (IsInvisible())
                showFraming = true;
            InvalidateFromMenu(sender);
        }

        private void MnuShowThirds_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showThirds = !mnuShowThirds.Checked;
            if (IsInvisible())
                showThirds = true;
            InvalidateFromMenu(sender);
        }
        #endregion


        #region Private methods
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("TestGrid"));
            style.Bind(styleHelper, "Color", "color");
        }
        

        private void CreateGridlines()
        {
            // Grid lines defined in normalized space [-1.0, +1.0].
            // +X left, +Y down.
            gridLines.Clear();
            
            // Main axes.
            gridLines.Add("horizontal", new GridLine(new PointF(-1, 0), new PointF(1, 0)));
            gridLines.Add("vertical", new GridLine(new PointF(0, -1), new PointF(0, 1)));
            
            // Safe framing.
            float margin = 0.8f;
            PointF a = new PointF(-margin, -margin);
            PointF b = new PointF(margin, -margin);
            PointF c = new PointF(margin, margin);
            PointF d = new PointF(-margin, margin);
            gridLines.Add("frameLeft", new GridLine(a, d));
            gridLines.Add("frameRight", new GridLine(b, c));
            gridLines.Add("frameTop", new GridLine(a, b));
            gridLines.Add("frameBottom", new GridLine(d, c));

            // Rule of thirds.
            gridLines.Add("thirdsLeft", new GridLine(new PointF(-1.0f / 3.0f, -1), new PointF(-1.0f / 3.0f, 1)));
            gridLines.Add("thirdsRight", new GridLine(new PointF(1.0f / 3.0f, -1), new PointF(1.0f / 3.0f, 1)));
            gridLines.Add("thirdsTop", new GridLine(new PointF(-1, -1.0f / 3.0f), new PointF(1, -1.0f / 3.0f)));
            gridLines.Add("thirdsBottom", new GridLine(new PointF(-1, 1.0f / 3.0f), new PointF(1, 1.0f / 3.0f)));
        }

        /// <summary>
        /// Go from normalized coordinates in [-1, +1] space to image coordinates.
        /// </summary>
        private PointF Map(PointF p)
        {
            return new PointF((p.X * 0.5f + 0.5f) * imageSize.Width, (p.Y * 0.5f + 0.5f) * imageSize.Height);
        }

        /// <summary>
        /// Capture the current state to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.SingletonDrawingsManager.Id, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }

        /// <summary>
        /// Return true if all the visible elements are disabled.
        /// We should always have at least one element visible to access the context menu.
        /// This is not the same as the "Hide" function. In the case of Hide we can bring back the 
        /// object via the top level menu, but it needs to draw *something*.
        /// </summary>
        private bool IsInvisible()
        {
            return !showHorizontalAxis && !showVerticalAxis && !showFraming && !showThirds;
        }

        private void ReloadMenusCulture()
        {
            mnuOptions.Text = "Options";
            mnuShowHorizontalAxis.Text = "Show horizontal axis";
            mnuShowVerticalAxis.Text = "Show vertical axis";
            mnuShowFraming.Text = "Show frame";
            mnuShowThirds.Text = "Show 3x3 grid";
            mnuHide.Text = ScreenManagerLang.mnuCoordinateSystemHide;
        }
        #endregion
    }
}
