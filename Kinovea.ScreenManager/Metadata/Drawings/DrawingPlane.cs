#region License
/*
Copyright � Joan Charmant 2008.
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
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A flat or perspective grid.
    /// </summary>
    [XmlType ("Plane")]
    public class DrawingPlane : AbstractDrawing, IDecorable, IKvaSerializable, IScalable, IMeasurable, ITrackable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler<EventArgs<MeasureLabelType>> ShowMeasurableInfoChanged;
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get
            {
                return ToolManager.Tools["Plane"].DisplayName;
            }
        }
        public override int ContentHash
        {
            get
            {
                int hash = 0;
                hash ^= styleData.ContentHash;
                hash ^= infosFading.ContentHash;
                hash ^= showGrid.GetHashCode();
                hash ^= showXLine.GetHashCode();
                hash ^= showYLine.GetHashCode();
                hash ^= xLineCoord.GetHashCode();
                hash ^= yLineCoord.GetHashCode();
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
        public override List<ToolStripItem> ContextMenu
        {
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                ReloadMenusCulture();

                contextMenu.AddRange(new ToolStripItem[] {
                    mnuOptions,
                    new ToolStripSeparator(),
                    mnuCalibrate,
                });

                mnuShowGrid.Checked = showGrid;
                mnuShowXLine.Checked = showXLine;
                mnuShowYLine.Checked = showYLine;

                return contextMenu;
            }
        }
        public override DrawingCapabilities Caps
        {
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track | DrawingCapabilities.CopyPaste; }
        }
        public QuadrilateralF QuadImage
        {
            get { return quadImage;}
        }
        public CalibrationHelper CalibrationHelper
        {
            get
            {
                return calibrationHelper;
            }
            set
            {
                calibrationHelper = value;
                calibrationHelper.CalibrationChanged += CalibrationHelper_CalibrationChanged;
            }
        }
        #endregion

        #region Members
        private QuadrilateralF quadImage = QuadrilateralF.GetUnitSquare();      // Quadrilateral defined by user in image space.
        private QuadrilateralF quadPlane;                                       // Corresponding rectangle in plane system.
        private ProjectiveMapper projectiveMapping = new ProjectiveMapper();  // maps quadImage to quadPlane and back.
        private float planeWidth;                                               // width and height of rectangle in plane system.
        private float planeHeight;
        private bool planeIsConvex = true;
        
        // Options
        private bool showGrid = true;
        private bool showXLine = false;
        private bool showYLine = false;
        
        private float xLineCoord = 0.5f;        // Normalized coordinate of the sliding line along the X axis.
        private float yLineCoord = 0.5f;        // Normalized coordinate of the sliding line along the Y axis.
        
        private const float tickMarkLengthFactor = 1.0f/40.0f;    // Size of the tickmarks when not showing the grid, normalized to the size of the grid.

        private const int defaultBackgroundAlpha = 92;
        private const int textMargin = 20;
        private List<TickMark> tickMarks = new List<TickMark>();

        private long trackingTimestamps = -1;

        private InfosFading infosFading;
        private StyleElements styleElements = new StyleElements();
        private StyleData styleData = new StyleData();
        private Pen penEdges = Pens.White;
        private CalibrationHelper calibrationHelper;

        private bool initialized = false;

        #region Context menu
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowGrid = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowXLine = new ToolStripMenuItem();
        private ToolStripMenuItem mnuShowYLine = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(PointF origin, long timestamp, double averageTimeStampsPerFrame, StyleElements preset = null)
        {
            // Decoration
            styleData.Color = Color.Empty;
            styleData.GridCols = 10;
            styleData.GridRows = 10;
            styleData.Perspective = true;
            styleData.Font = new Font("Arial", 8, FontStyle.Bold);
            styleData.ValueChanged += StyleData_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetDefaultStyleElements("Plane");

            styleElements = preset.Clone();
            BindStyle();

            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.AlwaysVisible = true;

            planeWidth = 100;
            planeHeight = 100;
            quadPlane = new QuadrilateralF(planeWidth, planeHeight);

            InitializeMenus();
        }
        public DrawingPlane(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0, ToolManager.GetDefaultStyleElements("Grid"))
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }

        private void InitializeMenus()
        {
            mnuOptions.Image = Properties.Resources.equalizer;
            mnuShowGrid.Image = Properties.Drawings.coordinates_grid;
            mnuShowXLine.Image = Properties.Drawings.yline;
            mnuShowYLine.Image = Properties.Drawings.xline;
            mnuShowGrid.Click += mnuShowGrid_Click;
            mnuShowXLine.Click += mnuShowXLine_Click;
            mnuShowYLine.Click += mnuShowYLine_Click;

            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] {
                mnuShowGrid,
                mnuShowXLine,
                mnuShowYLine,
            });

            mnuCalibrate.Click += mnuCalibrate_Click;
            mnuCalibrate.Image = Properties.Drawings.coordinates_graduations;
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, CameraTransformer cameraTransformer, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            float opacityFactor = (float)infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacityFactor <= 0)
               return;

            QuadrilateralF quad = transformer.Transform(quadImage);

            bool drawEdgesOnly = !planeIsConvex || (!styleData.Perspective && !quadImage.IsAxisAlignedRectangle);

            const int defaultRadius = 4;

            using (penEdges = styleData.GetPen(opacityFactor, 1.0))
            using(SolidBrush br = styleData.GetBrush(opacityFactor))
            {
                // Handles
                foreach (PointF p in quad)
                {
                    canvas.DrawLine(penEdges, p.X - defaultRadius, p.Y, p.X + defaultRadius, p.Y);
                    canvas.DrawLine(penEdges, p.X, p.Y - defaultRadius, p.X, p.Y + defaultRadius);
                }

                // Show origin if we are the driver for the coordinate system.
                if (calibrationHelper.CalibrationDrawingId == this.Id)
                    canvas.DrawEllipse(penEdges, quad.D.Box(5));

                if (!drawEdgesOnly)
                {
                    // FIXME: why do we recompute the projective mapping every draw call?

                    if (distorter != null && distorter.Initialized)
                    {
                        QuadrilateralF undistortedQuadImage = distorter.Undistort(quadImage);
                        projectiveMapping.Update(quadPlane, undistortedQuadImage);
                    }
                    else
                    {
                        projectiveMapping.Update(quadPlane, quadImage);
                    }

                    DrawGrid(canvas, penEdges, opacityFactor, projectiveMapping, distorter, transformer);
                    DrawSlidingLines(canvas, penEdges, opacityFactor, projectiveMapping, distorter, transformer);
                }
                else
                {
                    // Non convex quadrilateral or non rectangle 2d grid: only draw the edges.
                    canvas.DrawLine(penEdges, quad.A, quad.B);
                    canvas.DrawLine(penEdges, quad.B, quad.C);
                    canvas.DrawLine(penEdges, quad.C, quad.D);
                    canvas.DrawLine(penEdges, quad.D, quad.A);
                }
            }
        }

        private void DrawGrid(Graphics canvas, Pen pen, float opacity, ProjectiveMapper projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            if (showGrid)
            {
                // Draw vertical and horizontal lines crossing the full grid.
                int start = 0;
                int cols = styleData.GridCols;
                int rows = styleData.GridRows;

                // Horizontals
                float step = planeHeight / rows;
                for (int i = start; i <= rows; i++)
                {
                    float v = step * i;
                    DrawDistortedLine(canvas, pen, new PointF(0, v), new PointF(planeWidth, v), projectiveMapping, distorter, transformer);
                }

                // Verticals
                step = planeWidth / cols;
                for (int i = start; i <= cols; i++)
                {
                    float h = step * i;
                    DrawDistortedLine(canvas, pen, new PointF(h, 0), new PointF(h, planeHeight), projectiveMapping, distorter, transformer);
                }
            }
            else
            {
                // Draw the edges and secondary ticks only.
            
                // Edges.
                DrawDistortedLine(canvas, pen, new PointF(0, 0), new PointF(planeWidth, 0), projectiveMapping, distorter, transformer);
                DrawDistortedLine(canvas, pen, new PointF(0, planeHeight), new PointF(planeWidth, planeHeight), projectiveMapping, distorter, transformer);
                DrawDistortedLine(canvas, pen, new PointF(0, 0), new PointF(0, planeHeight), projectiveMapping, distorter, transformer);
                DrawDistortedLine(canvas, pen, new PointF(planeWidth, 0), new PointF(planeWidth, planeHeight), projectiveMapping, distorter, transformer);

                // Secondary ticks.
                int start = 1;
                int cols = styleData.GridCols - 1;
                int rows = styleData.GridRows - 1;

                // Horizontals
                float step = planeHeight / styleData.GridRows;
                float halfLength = planeWidth * tickMarkLengthFactor;
                for (int i = start; i <= rows; i++)
                {
                    float v = step * i;
                    DrawDistortedLine(canvas, pen, new PointF(-halfLength, v), new PointF(halfLength, v), projectiveMapping, distorter, transformer);
                    DrawDistortedLine(canvas, pen, new PointF(planeWidth - halfLength, v), new PointF(planeWidth + halfLength, v), projectiveMapping, distorter, transformer);
                }

                // Verticals
                step = planeWidth / styleData.GridCols;
                halfLength = planeHeight * tickMarkLengthFactor;
                for (int i = start; i <= cols; i++)
                {
                    float h = step * i;
                    DrawDistortedLine(canvas, pen, new PointF(h, -halfLength), new PointF(h, halfLength), projectiveMapping, distorter, transformer);
                    DrawDistortedLine(canvas, pen, new PointF(h, planeHeight - halfLength), new PointF(h, planeHeight + halfLength), projectiveMapping, distorter, transformer);
                }
            }
        }

        /// <summary>
        /// Draw the sliding line and tickmarks for the distance grid.
        /// </summary>
        private void DrawSlidingLines(Graphics canvas, Pen pen, float opacity, ProjectiveMapper projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            // This should only be drawn if we are actually the one used for calibration.
            if (calibrationHelper.CalibrationDrawingId != this.Id)
                return;

            if (showXLine)
            {
                // Vertical sliding line.
                float x = xLineCoord * planeWidth;
                DrawDistortedLine(canvas, pen, new PointF(x, 0), new PointF(x, planeHeight), projectiveMapping, distorter, transformer);
            }

            if (showYLine)
            {
                // Horizontal sliding line.
                float y = yLineCoord * planeHeight;
                DrawDistortedLine(canvas, pen, new PointF(0, y), new PointF(planeWidth, y), projectiveMapping, distorter, transformer);
            }

            if (!showXLine && !showYLine)
                return;

            // Graduation tick marks.
            SolidBrush brushFill = styleData.GetBrush(defaultBackgroundAlpha);
            Brush brushFont = pen.Color.GetBrightness() > 0.6 ? Brushes.Black : Brushes.White;

            Font font = styleData.GetFont(1.0F);
            foreach (TickMark tick in tickMarks)
                tick.Draw(canvas, distorter, transformer, brushFill, brushFont as SolidBrush, font, textMargin, true);
            
            font.Dispose();
            brushFill.Dispose();
        }

        /// <summary>
        /// Takes a line segment by its endpoints in world space and draw it.
        /// </summary>
        private void DrawDistortedLine(Graphics canvas, Pen pen, PointF a, PointF b, ProjectiveMapper projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            a = projectiveMapping.Forward(a);
            b = projectiveMapping.Forward(b);

            if (distorter != null && distorter.Initialized)
            {
                a = distorter.Distort(a);
                b = distorter.Distort(b);

                List<PointF> curve = distorter.DistortLine(a, b);
                List<Point> transformed = transformer.Transform(curve);
                canvas.DrawCurve(pen, transformed.ToArray());
            }
            else
            {
                canvas.DrawLine(pen, transformer.Transform(a), transformer.Transform(b));
            }
        }
        
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity <= 0)
                return -1;

            // Mapping:
            // 0: hit anywhere inside the flat grid. Disabled for perspective grids.
            // 1-4: hit a corner.
            // 5: X line.
            // 6: Y line.

            if (showXLine)
            {
                // Reverse the mapping to find the location of the line in image space.
                float h = planeWidth * xLineCoord;
                PointF a = projectiveMapping.Forward(new PointF(h, 0));
                PointF b = projectiveMapping.Forward(new PointF(h, planeHeight));
                if (HitTester.HitLine(point, a, b, distorter, transformer))
                    return 5;
            }

            if (showYLine)
            {
                // Reverse the mapping to find the location of the line in image space.
                float v = planeHeight * yLineCoord;
                PointF a = projectiveMapping.Forward(new PointF(0, v));
                PointF b = projectiveMapping.Forward(new PointF(planeWidth, v));
                if (HitTester.HitLine(point, a, b, distorter, transformer))
                    return 6;
            }

            for (int i = 0; i < 4; i++)
            {
                if (HitTester.HitPoint(point, quadImage[i], transformer))
                    return i+1;
            }

            return -1;
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys)
        {
            // Move drawing is actually impossible on grids.
            log.ErrorFormat("Move drawing on grid not supported.");
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            if (handleNumber == 5)
            {
                // Dragging the X line.
                // We get a point in image space, convert to a point in calibrated space and grab the x coord.
                PointF p = projectiveMapping.Backward(point);
                xLineCoord = p.X / planeWidth;
                xLineCoord = ClampSlidingLine(xLineCoord); 
                UpdateTickMarks();
            }
            else if (handleNumber == 6)
            {
                // Dragging the Y line.
                // We get a point in image space, convert to a point in calibrated space and grab the y coord.
                PointF p = projectiveMapping.Backward(point);
                yLineCoord = p.Y / planeHeight;
                yLineCoord = ClampSlidingLine(yLineCoord);
                UpdateTickMarks();
            }
            else
            {
                int corner = handleNumber - 1;
                quadImage[corner] = point;

                if (styleData.Perspective)
                {
                    planeIsConvex = quadImage.IsConvex;
                }
                else
                {
                    if((modifiers & Keys.Shift) == Keys.Shift)
                        quadImage.MakeSquare(corner);
                    else
                        quadImage.MakeRectangle(corner);
                }

                AfterQuadOp();
            }
        }
        public override PointF GetCopyPoint()
        {
            return quadImage.A;
        }
        #endregion

        #region IKvaSerializable
        public void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            if (r.MoveToAttribute("id"))
                identifier = new Guid(r.ReadContentAsString());

            if (r.MoveToAttribute("name"))
                name = r.ReadContentAsString();

            r.ReadStartElement();

            while(r.NodeType == XmlNodeType.Element)
            {
                switch(r.Name)
                {
                    case "PointUpperLeft":
                        {
                            quadImage.A = ReadPoint(r, scale);
                            break;
                        }
                    case "PointUpperRight":
                        {
                            quadImage.B = ReadPoint(r, scale);
                            break;
                        }
                    case "PointLowerRight":
                        {
                            quadImage.C = ReadPoint(r, scale);
                            break;
                        }
                    case "PointLowerLeft":
                        {
                            quadImage.D = ReadPoint(r, scale);
                            break;
                        }
                    case "ReferenceTimestamp":
                        {
                            referenceTimestamp = XmlHelper.ParseTimestamp(r.ReadElementContentAsString());
                            break;
                        }
                    case "ShowGrid":
                        {
                            showGrid = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                            break;
                        }
                    case "ShowXLine":
                        {
                            showXLine = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                            break;
                        }
                    case "ShowYLine":
                        {
                            showYLine = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                            break;
                        }
                    case "XLineCoord":
                        {
                            bool read = float.TryParse(r.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out xLineCoord);
                            if (!read)
                                xLineCoord = 0.5f;
                            break;
                        }
                    case "YLineCoord":
                        {
                            bool read = float.TryParse(r.ReadElementContentAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out yLineCoord);
                            if (!read)
                                yLineCoord = 0.5f;
                            break;
                        }
                    case "DrawingStyle":
                        StyleElements styleXML = new StyleElements(r);

                        // Special case to convert grid divisions into cols x rows.
                        if (styleXML.Elements.ContainsKey("divisions"))
                        {
                            styleXML.Elements.Add("cols", new StyleElementInt((int)styleXML.Elements["divisions"].Value));
                            styleXML.Elements.Add("rows", new StyleElementInt((int)styleXML.Elements["divisions"].Value));
                            styleXML.Elements.Remove("divisions");
                        }

                        styleElements.ImportValues(styleXML);
                        BindStyle();
                        break;
                    case "InfosFading":
                        infosFading.ReadXml(r);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();

            if (!styleData.Perspective)
                planeIsConvex = quadImage.IsConvex;

            initialized = true;

            SignalAllTrackablePointsMoved();
        }
        private PointF ReadPoint(XmlReader reader, PointF scale)
        {
            PointF p = XmlHelper.ParsePointF(reader.ReadElementContentAsString());
            return p.Scale(scale.X, scale.Y);
        }
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                PointF a = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "0");
                PointF b = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "1");
                PointF c = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "2");
                PointF d = parentMetadata.TrackabilityManager.GetReferenceValue(Id, "3");
                w.WriteElementString("PointUpperLeft", XmlHelper.WritePointF(a));
                w.WriteElementString("PointUpperRight", XmlHelper.WritePointF(b));
                w.WriteElementString("PointLowerRight", XmlHelper.WritePointF(c));
                w.WriteElementString("PointLowerLeft", XmlHelper.WritePointF(d));
                w.WriteElementString("ReferenceTimestamp", XmlHelper.WriteTimestamp(referenceTimestamp));

                w.WriteElementString("ShowGrid", XmlHelper.WriteBoolean(showGrid));
                w.WriteElementString("ShowXLine", XmlHelper.WriteBoolean(showXLine));
                w.WriteElementString("ShowYLine", XmlHelper.WriteBoolean(showYLine));

                w.WriteElementString("XLineCoord", XmlHelper.WriteFloat(xLineCoord));
                w.WriteElementString("YLineCoord", XmlHelper.WriteFloat(yLineCoord));
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

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            // Initialize corners positions
            if (!initialized)
            {
                initialized = true;

                int horzTenth = (int)(((double)imageSize.Width) / 10);
                int vertTenth = (int)(((double)imageSize.Height) / 10);

                if (styleData.Perspective)
                {
                    // Initialize with a faked perspective.
                    quadImage.A = new Point(3 * horzTenth, 4 * vertTenth);
                    quadImage.B = new Point(7 * horzTenth, 4 * vertTenth);
                    quadImage.C = new Point(9 * horzTenth, 8 * vertTenth);
                    quadImage.D = new Point(1 * horzTenth, 8 * vertTenth);
                }
                else
                {
                    // initialize with a rectangle.
                    quadImage.A = new Point(2 * horzTenth, 2 * vertTenth);
                    quadImage.B = new Point(8 * horzTenth, 2 * vertTenth);
                    quadImage.C = new Point(8 * horzTenth, 8 * vertTenth);
                    quadImage.D = new Point(2 * horzTenth, 8 * vertTenth);
                }
            }
        }
        #endregion

        #region ITrackable implementation and support.
        public Color Color
        {
            get { return styleData.Color; }
        }
        public TrackingParameters CustomTrackingParameters
        {
            get { return null; }
        }
        public Dictionary<string, PointF> GetTrackablePoints()
        {
            Dictionary<string, PointF> points = new Dictionary<string, PointF>();

            for(int i = 0; i < 4; i++)
                points.Add(i.ToString(), quadImage[i]);

            return points;
        }
        public void SetTrackablePointValue(string name, PointF value, long trackingTimestamps)
        {
            int p = int.Parse(name);
            quadImage[p] = new PointF(value.X, value.Y);
            this.trackingTimestamps = trackingTimestamps;

            // FIXME: this should use rectified image space.
            // CalibrationByPlane_Update will take care of the distortion, why do we 
            // need to keep a separate projectiveMapper here?
            projectiveMapping.Update(quadPlane, quadImage);
            CalibrationHelper.CalibrationByPlane_Update(Id, quadImage);
            planeIsConvex = quadImage.IsConvex;
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;

            for(int i = 0; i<4; i++)
                TrackablePointMoved(this, new TrackablePointMovedEventArgs(i.ToString(), quadImage[i]));
        }
        private void SignalTrackablePointMoved(int index)
        {
            if(TrackablePointMoved == null)
                return;

            TrackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), quadImage[index]));
        }
        #endregion

        #region Context menu

        private void mnuShowGrid_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showGrid = !mnuShowGrid.Checked;
            InvalidateFromMenu(sender);
        }

        private void mnuShowXLine_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showXLine = !mnuShowXLine.Checked;
            UpdateTickMarks();
            InvalidateFromMenu(sender);
        }

        private void mnuShowYLine_Click(object sender, EventArgs e)
        {
            CaptureMemento(SerializationFilter.Core);
            showYLine = !mnuShowYLine.Checked;
            UpdateTickMarks();
            InvalidateFromMenu(sender);
        }

        private void mnuCalibrate_Click(object sender, EventArgs e)
        {
            FormCalibratePlane fcp = new FormCalibratePlane(CalibrationHelper, this);
            FormsHelper.Locate(fcp);
            fcp.ShowDialog();
            fcp.Dispose();

            InvalidateFromMenu(sender);
        }
        #endregion

        public void Reset()
        {
            // Used on metadata over load.
            planeIsConvex = true;
            initialized = false;

            quadImage = quadPlane.Clone();
        }

        public void FlipHorizontal()
        {
            quadImage.FlipHorizontal();
            AfterQuadOp();
        }

        public void FlipVertical()
        {
            quadImage.FlipVertical();
            AfterQuadOp();
        }

        public void Rotate90()
        {
            quadImage.Rotate90();
            AfterQuadOp();
        }

        
        public void UpdateMapping(SizeF size)
        {
            planeWidth = size.Width;
            planeHeight = size.Height;
            quadPlane = new QuadrilateralF(planeWidth, planeHeight);

            // FIXME: should use rectified image-space coordinates.
            projectiveMapping.Update(quadPlane, quadImage);
            UpdateTickMarks();
        }

        #region IMeasurable implementation
        public void InitializeMeasurableData(MeasureLabelType measureLabelType)
        {
        }
        #endregion

        #region Private methods
        private void BindStyle()
        {
            StyleElements.SanityCheck(styleElements, ToolManager.GetDefaultStyleElements("Plane"));
            styleElements.Bind(styleData, "Color", "color");
            styleElements.Bind(styleData, "GridCols", "cols");
            styleElements.Bind(styleData, "GridRows", "rows");
            styleElements.Bind(styleData, "Toggles/Perspective", "perspective");
        }
        private void StyleData_ValueChanged(object sender, EventArgs e)
        {
            // Handle the case where we convert from a perspective plane to a grid.
            // Note: we cannot force rectangle here because this would change the actual points,
            // these points are not part of the "style" and so if we cancelled the style change we would not
            // be able to go back to the original quad.
            planeIsConvex = styleData.Perspective ? quadImage.IsConvex : true;
        }

        private float ClampSlidingLine(float a)
        {
            // Limit the sliding line to a certain margin from the side.
            return Math.Min(1.0f, Math.Max(0.0f, a));
        }

        /// <summary>
        /// Pre-build the graduation tick marks locations and values. 
        /// </summary>
        private void UpdateTickMarks()
        {
            // Update the placement of the six tickmarks.
            tickMarks.Clear();

            // Hide the start/end markers when the sliding line is close to them to avoid overlap.
            if (showXLine)
            {
                if (xLineCoord > 0.1)
                    AddTickMarks(0, true);

                if (xLineCoord < 0.9)
                    AddTickMarks(planeWidth, true);

                AddTickMarks(xLineCoord * planeWidth, true);
            }

            if (showYLine)
            {
                if (yLineCoord > 0.1)
                    AddTickMarks(0, false);

                if (yLineCoord < 0.9)
                  AddTickMarks(planeHeight, false);

                AddTickMarks(yLineCoord * planeHeight, false);
            }

        }

        /// <summary>
        /// Takes a coordinate in Grid space along an axis, and create two tick marks 
        /// on both sides of the grid with the corresponding values.
        /// </summary>
        private void AddTickMarks(float coord, bool isXAxis)
        {
            var calibrator = CalibrationHelper.CalibrationByPlane_GetCalibrator();

            if (isXAxis)
            {
                // Coords in grid space.
                PointF pTop = new PointF(coord, 0);
                PointF pBottom = new PointF(coord, planeHeight);

                float value = calibrator.GridToWorldWithOffset(pTop).X;

                // Grid to Image.
                pTop = projectiveMapping.Forward(pTop);
                pBottom = projectiveMapping.Forward(pBottom);

                tickMarks.Add(new TickMark(value, pTop, TextAlignment.Top));
                tickMarks.Add(new TickMark(value, pBottom, TextAlignment.Bottom));
            }
            else
            {
                PointF pLeft = new PointF(0, coord);
                PointF pRight = new PointF(planeWidth, coord);
                
                float value = calibrator.GridToWorldWithOffset(pLeft).Y;

                // Grid to Image.
                pLeft = projectiveMapping.Forward(pLeft);
                pRight = projectiveMapping.Forward(pRight);

                tickMarks.Add(new TickMark(value, pLeft, TextAlignment.Left));
                tickMarks.Add(new TickMark(value, pRight, TextAlignment.Right));
            }
        }

        private void AfterQuadOp()
        {
            SignalAllTrackablePointsMoved();
            CalibrationHelper.CalibrationByPlane_Update(Id, quadImage);
        }

        public void CalibrationHelper_CalibrationChanged(object sender, EventArgs e)
        {
            // Update the projective mapping if we are the calibration drawing.
            SizeF size = new SizeF(100, 100);
            if (calibrationHelper.CalibrationDrawingId == this.Id)
                size = calibrationHelper.CalibrationByPlane_GetRectangleSize();
                
            UpdateMapping(size);
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
            mnuOptions.Text = ScreenManagerLang.Generic_Options;
            mnuShowGrid.Text = ScreenManagerLang.mnuOptions_CoordinateSystem_ShowGrid;
            mnuShowXLine.Text = ScreenManagerLang.DrawingPlane_ShowVerticalLine;
            mnuShowYLine.Text = ScreenManagerLang.DrawingPlane_ShowHorizontalLine;
            mnuCalibrate.Text = ScreenManagerLang.mnuCalibrate;
        }
        #endregion
    }
}
