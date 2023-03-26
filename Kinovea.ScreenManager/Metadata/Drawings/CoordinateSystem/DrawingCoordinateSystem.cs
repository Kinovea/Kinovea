#region License
/*
Copyright © Joan Charmant 2012.
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
    [XmlType ("CoordinateSystem")]
    public class DrawingCoordinateSystem : AbstractDrawing, IScalable, ITrackable, IMeasurable, IDecorable, IKvaSerializable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler<EventArgs<MeasureLabelType>> ShowMeasurableInfoChanged = delegate {}; // not used.
        
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get {  return ScreenManagerLang.mnuCoordinateSystem; }
        }
        public override int ContentHash
        {
            get 
            { 
                int hash = Visible.GetHashCode();
                hash ^= styleHelper.ContentHash;
                hash ^= showGrid.GetHashCode();
                hash ^= showGraduations.GetHashCode();
                return hash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
        }
        public override InfosFading InfosFading
        {
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Track; }
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
                    mnuHide
                });
                
                mnuShowGrid.Checked = showGrid;
                mnuShowGraduations.Checked = showGraduations;
                
                return contextMenu; 
            }
        }
        public bool Visible { get; set; }
        public CalibrationHelper CalibrationHelper {get; set;}
        #endregion

        #region Members
        private Dictionary<string, PointF> points = new Dictionary<string, PointF>();
        private bool showGrid = true;
        private bool showGraduations = true;
        private Size imageSize;

        // Decoration
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;

        
        private bool trackingUpdate;


        private const int axesAlpha = 255;
        private const int gridAlpha = (int)(255*0.5f);
        private const int labelsAlpha = (int)(255*0.5f);
        private const int textMargin = 8;

        #region Context menu
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowGrid = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowGraduations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHide = new ToolStripMenuItem();
        #endregion
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCoordinateSystem(Point origin, DrawingStyle stylePreset)
        {
            points["0"] = origin;
            
            // Decoration & binding with editors
            styleHelper.Bicolor = new Bicolor(Color.Empty);
            styleHelper.Font = new Font("Arial", 8, FontStyle.Bold);
            if(stylePreset != null)
            {
                style = stylePreset.Clone();
                BindStyle();
            }

            InitializeMenus();
        }

        private void InitializeMenus()
        {
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowGrid.Image = Properties.Drawings.coordinates_grid;
            mnuShowGraduations.Image = Properties.Drawings.label;
            
            mnuShowGrid.Click += mnuShowGrid_Click;
            mnuShowGraduations.Click += mnuShowGraduations_Click;

            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowGrid,
                mnuShowGraduations,
            });

            mnuHide.Image = Properties.Drawings.hide;
            mnuHide.Click += mnuHide_Click;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            if(!Visible || CalibrationHelper == null)
                return;

            if (CalibrationHelper.CalibratorType == CalibratorType.Plane && !CalibrationHelper.CalibrationByPlane_IsValid())
                return;
            
            CoordinateSystemGrid grid = CalibrationHelper.GetCoordinateSystemGrid();
            if (grid == null)
                return;

            using (Pen penLine = styleHelper.GetBackgroundPen(255))
            {
                DrawGrid(canvas, distorter, transformer, grid);
            }
        }

        private void DrawGrid(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, CoordinateSystemGrid grid)
        {
            Pen pen = styleHelper.GetBackgroundPen(axesAlpha);
            
            // Axes
            pen.DashStyle = DashStyle.Solid;
            pen.Width = 1;
            if (grid.VerticalAxis != null)
                DrawGridLine(canvas, distorter, transformer, pen, grid.VerticalAxis.Start, grid.VerticalAxis.End);

            if (grid.HorizontalAxis != null)
                DrawGridLine(canvas, distorter, transformer, pen, grid.HorizontalAxis.Start, grid.HorizontalAxis.End);
            
            if (showGrid)
            {
                pen.Color = Color.FromArgb(gridAlpha, pen.Color);
                pen.DashStyle = DashStyle.Dash;
                    
                foreach (GridLine line in grid.GridLines)
                {
                    DrawGridLine(canvas, distorter, transformer, pen, line.Start, line.End);
                }
            }

            if (showGraduations)
            {
                SolidBrush brushFill = styleHelper.GetBackgroundBrush(labelsAlpha);
                SolidBrush fontBrush = styleHelper.GetForegroundBrush(255);
                Font font = styleHelper.GetFont(1.0F);

                foreach (TickMark tick in grid.TickMarks)
                    tick.Draw(canvas, distorter, transformer, brushFill, fontBrush, font, textMargin, false);
            
                font.Dispose();
                fontBrush.Dispose();
                brushFill.Dispose();
            }

            pen.Dispose();
        }

        private void DrawGridLine(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, Pen penLine, PointF a, PointF b)
        {
            if (distorter != null && distorter.Initialized)
            {
                List<PointF> curve = distorter.DistortRectifiedLine(a, b);
                List<Point> transformed = transformer.Transform(curve);
                canvas.DrawCurve(penLine, transformed.ToArray());
            }
            else
            {
                PointF p1 = transformer.Transform(a);
                PointF p2 = transformer.Transform(b);
                canvas.DrawLine(penLine, p1, p2);
            }
        }

        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // -1: miss
            // 0: not bound.
            // 1: origin point.
            // 2: horizontal axis.
            // 3: vertical axis.

            if(!Visible)
                return -1;
            
            int result = -1;
            
            if (HitTester.HitPoint(point, points["0"], transformer))
                return 1;
            
            CoordinateSystemGrid grid = CalibrationHelper.GetCoordinateSystemGrid();
            if (grid == null)
                return -1;

            if (grid.HorizontalAxis != null && HitTester.HitLine(point, grid.HorizontalAxis.Start, grid.HorizontalAxis.End, distorter, transformer))
                result = 2;
            else if (grid.VerticalAxis != null && HitTester.HitLine(point, grid.VerticalAxis.Start, grid.VerticalAxis.End, distorter, transformer))
                result = 3;
            
            return result;
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (handleNumber == 1)
                points["0"] = point;
            else if (handleNumber == 2)
                MoveHorizontalAxis(point);
            else if (handleNumber == 3)
                MoveVerticalAxis(point);

            CalibrationHelper.SetOrigin(points["0"]);
            SignalTrackablePointMoved();
        }
        public override void MoveDrawing(float dx, float dy, Keys _ModifierKeys, bool zooming)
        {
        }
        public override PointF GetCopyPoint()
        {
            return points["0"];
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
            /// Called by the trackability manager after a Track() call.
            /// The value of the trackable point should be updated inside the drawing so the 
            /// drawing reflects the new coordinate.
            if (!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;

            trackingUpdate = true;
            CalibrationHelper.SetOrigin(value);
            trackingUpdate = false;
        }
        private void SignalTrackablePointMoved()
        {
            // The trackable point has been moved through direct user interaction.
            // Alert the trackability manager of the new value.
            if(TrackablePointMoved == null)
                return;

            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
        }
        #endregion

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            if (imageSize == this.imageSize)
                return;
            
            this.imageSize = imageSize;
            points["0"] = new Point(imageSize.Width / 2, imageSize.Height / 2);
        }
        #endregion
        
        #region Custom menu handlers
        private void mnuShowGrid_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showGrid = !mnuShowGrid.Checked;
            InvalidateFromMenu(sender);
        }
        private void mnuShowGraduations_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showGraduations = !mnuShowGraduations.Checked;
            InvalidateFromMenu(sender);
        }
        private void mnuHide_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            Visible = false;
            InvalidateFromMenu(sender);
        }
        #endregion

        #region Serialization
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteElementString("Position", XmlHelper.WritePointF(points["0"]));
                w.WriteElementString("Visible", XmlHelper.WriteBoolean(Visible));
                w.WriteElementString("ShowGrid", XmlHelper.WriteBoolean(showGrid));
                w.WriteElementString("ShowGraduations", XmlHelper.WriteBoolean(showGraduations));
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
            
            while(r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Position":
                        PointF p = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                        points["0"] = p;
                        break;
                    case "Visible":
                        Visible = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowGrid":
                        showGrid = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "ShowGraduations":
                        showGraduations = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
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

            CalibrationHelper.SetOrigin(points["0"]);
            SignalTrackablePointMoved();

            r.ReadEndElement();
        }
        #endregion

        #region IMeasurable implementation
        public void UpdateOrigin()
        {
            // The coordinate system origin was updated from the outside.
            // Make sure the drawing reflects the new origin.
            if(CalibrationHelper != null)
            {
                points["0"] = CalibrationHelper.GetImagePoint(PointF.Empty);

                // Also ensure the trackability manager is up to date.
                // This is necessary to make sure it correctly imports the original value after KVA load.
                // But avoid calling it if the new origin is because we are reading the tracking timeline.
                if (!trackingUpdate)
                    SignalTrackablePointMoved();
            }
        }
        public void InitializeMeasurableData(MeasureLabelType measureLabelType)
        {
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            style.Bind(styleHelper, "Bicolor", "line color");
        }
        
        private void MoveHorizontalAxis(PointF p)
        {
            PointF point = CalibrationHelper.GetPoint(p);
            points["0"] = CalibrationHelper.GetImagePoint(new PointF(0, point.Y));
        }
        private void MoveVerticalAxis(PointF p)
        {
            PointF point = CalibrationHelper.GetPoint(p);
            points["0"] = CalibrationHelper.GetImagePoint(new PointF(point.X, 0));
        }

        /// <summary>
        /// Capture the current state to the undo/redo stack.
        /// </summary>
        private void CaptureMemento(SerializationFilter filter)
        {
            var memento = new HistoryMementoModifyDrawing(parentMetadata, parentMetadata.SingletonDrawingsManager.Id, this.Id, this.Name, filter);
            parentMetadata.HistoryStack.PushNewCommand(memento);
        }

        private void ReloadMenusCulture()
        {
            mnuOptions.Text = "Options";
            mnuShowGrid.Text = "Show grid";// ScreenManagerLang.mnuCoordinateSystemShowGrid;
            mnuShowGraduations.Text = "Show graduations"; //ScreenManagerLang.mnuCoordinateSystemShowTickMarks;
            mnuHide.Text = ScreenManagerLang.mnuCoordinateSystemHide;
        }
        #endregion
    }
}

