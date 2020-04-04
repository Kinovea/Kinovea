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
        public event EventHandler<EventArgs<TrackExtraData>> ShowMeasurableInfoChanged;
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
                int iHash = 0;
                iHash ^= quadImage.GetHashCode();
                iHash ^= styleHelper.ContentHash;
                iHash ^= infosFading.ContentHash;
                return iHash;
            }
        } 
        public DrawingStyle DrawingStyle
        {
            get { return style;}
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
                
                mnuCalibrate.Text = ScreenManagerLang.mnuCalibrate;
                contextMenu.Add(mnuCalibrate);

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
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool UsedForCalibration { get; set; }
        #endregion

        #region Members
        private QuadrilateralF quadImage = QuadrilateralF.GetUnitSquare();      // Quadrilateral defined by user in image space.
        private QuadrilateralF quadPlane;                                       // Corresponding rectangle in plane system.
        private ProjectiveMapping projectiveMapping = new ProjectiveMapping();  // maps quadImage to quadPlane and back.
        private float planeWidth;                                               // width and height of rectangle in plane system.
        private float planeHeight;
        private bool planeIsConvex = true;

        private long trackingTimestamps = -1;
        
        private InfosFading infosFading;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private Pen penEdges = Pens.White;
        
        private bool initialized = false;
        
        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();
        
        private static readonly int minimumSubdivisions = StyleElementGridDivisions.options[0];
        private static readonly int defaultSubdivisions = StyleElementGridDivisions.defaultValue;
        private static readonly int maximumSubdivisions = StyleElementGridDivisions.options[StyleElementGridDivisions.options.Count - 1];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(PointF origin, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            // Decoration
            styleHelper.Color = Color.Empty;
            styleHelper.GridDivisions = 8;
            styleHelper.Perspective = true;
            styleHelper.ValueChanged += StyleHelper_ValueChanged;
            if (preset == null)
                preset = ToolManager.GetStylePreset("Plane");
            
            style = preset.Clone();
            BindStyle();
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = false;
            infosFading.AlwaysVisible = true;
            
            planeWidth = 100;
            planeHeight = 100;
            quadPlane = new QuadrilateralF(planeWidth, planeHeight);
            
            mnuCalibrate.Click += new EventHandler(mnuCalibrate_Click);
            mnuCalibrate.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingPlane(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0, ToolManager.GetStylePreset("Grid"))
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion
        
        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacityFactor <= 0)
               return;

            QuadrilateralF quad = transformer.Transform(quadImage);

            bool drawEdgesOnly = !planeIsConvex || (!styleHelper.Perspective && !quadImage.IsAxisAlignedRectangle);
            
            using(penEdges = styleHelper.GetPen(opacityFactor, 1.0))
            using(SolidBrush br = styleHelper.GetBrush(opacityFactor))
            {
                foreach (PointF p in quad)
                    canvas.FillEllipse(br, p.Box(4));

                if (!drawEdgesOnly)
                {
                    if (distorter != null && distorter.Initialized)
                    {
                        QuadrilateralF undistortedQuadImage = distorter.Undistort(quadImage);
                        projectiveMapping.Update(quadPlane, undistortedQuadImage);
                    }
                    else
                    {
                        projectiveMapping.Update(quadPlane, quadImage);
                    }

                    DrawGrid(canvas, penEdges, projectiveMapping, distorter, transformer);
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
        private void DrawDiagonals(Graphics canvas, Pen pen, QuadrilateralF quadPlane, ProjectiveMapping projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            DrawDistortedLine(canvas, penEdges, quadPlane.A, quadPlane.B, projectiveMapping, distorter, transformer);
            DrawDistortedLine(canvas, penEdges, quadPlane.B, quadPlane.C, projectiveMapping, distorter, transformer);
            DrawDistortedLine(canvas, penEdges, quadPlane.C, quadPlane.D, projectiveMapping, distorter, transformer);
            DrawDistortedLine(canvas, penEdges, quadPlane.D, quadPlane.A, projectiveMapping, distorter, transformer);
            
            DrawDistortedLine(canvas, penEdges, quadPlane.A, quadPlane.C, projectiveMapping, distorter, transformer);
            DrawDistortedLine(canvas, penEdges, quadPlane.B, quadPlane.D, projectiveMapping, distorter, transformer);
        }
        private void DrawGrid(Graphics canvas, Pen pen, ProjectiveMapping projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            int start = 0;
            int end = styleHelper.GridDivisions;
            int total = styleHelper.GridDivisions;

            // Horizontals
            for (int i = start; i <= end; i++)
            {
                float v = i * ((float)planeHeight / total);
                DrawDistortedLine(canvas, pen, new PointF(0, v), new PointF(planeWidth, v), projectiveMapping, distorter, transformer);
            }

            // Verticals
            for (int i = start; i <= end; i++)
            {
                float h = i * (planeWidth / total);
                DrawDistortedLine(canvas, pen, new PointF(h, 0), new PointF(h, planeHeight), projectiveMapping, distorter, transformer);
            }
        }
        private void DrawDistortedLine(Graphics canvas, Pen pen, PointF a, PointF b, ProjectiveMapping projectiveMapping, DistortionHelper distorter, IImageToViewportTransformer transformer)
        {
            a = projectiveMapping.Forward(a);
            b = projectiveMapping.Forward(b);

            if (distorter != null && distorter.Initialized)
            {
                a = distorter.Distort(a);
                b = distorter.Distort(b);

                List<PointF> curve = distorter.DistortLine(a, b);
                List<Point> transformed = transformer.Transform(curve);
                canvas.DrawCurve(penEdges, transformed.ToArray());
            }
            else
            {
                canvas.DrawLine(pen, transformer.Transform(a), transformer.Transform(b));
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            double opacity = infosFading.GetOpacityTrackable(trackingTimestamps, currentTimestamp);
            if (opacity <= 0)
                return -1;
            
            for(int i = 0; i < 4; i++)
            {
                if(HitTester.HitTest(quadImage[i], point, transformer))
                    return i+1;
            }
            
            if (!zooming && !styleHelper.Perspective && quadImage.Contains(point))
                return 0;
            
            return -1;
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
            if (zooming)
                return;

            if (CalibrationHelper == null)
                return;

            if ((modifierKeys & Keys.Alt) == Keys.Alt)
            {
                // Change the number of divisions.
                styleHelper.GridDivisions = styleHelper.GridDivisions + (int)((dx - dy)/4);
                styleHelper.GridDivisions = Math.Min(Math.Max(styleHelper.GridDivisions, minimumSubdivisions), maximumSubdivisions);
            }
            else
            {
                if (!styleHelper.Perspective)
                {
                    quadImage.Translate(dx, dy);
                    CalibrationHelper.CalibrationByPlane_Update(Id, quadImage);
                }
            }
            
            SignalAllTrackablePointsMoved();
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            int handle = handleNumber - 1;
            quadImage[handle] = point;
            
            if (styleHelper.Perspective)
            {
                planeIsConvex = quadImage.IsConvex;
            }
            else
            {
                if((modifiers & Keys.Shift) == Keys.Shift)
                    quadImage.MakeSquare(handle);
                else
                    quadImage.MakeRectangle(handle);
            }
            
            SignalAllTrackablePointsMoved();
            CalibrationHelper.CalibrationByPlane_Update(Id, quadImage);
        }
        public override PointF GetCopyPoint()
        {
            return quadImage.A;
        }
        #endregion

        #region IKvaSerializable
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
                            quadImage.A = ReadPoint(xmlReader, scale); 
                            break;
                        }
                    case "PointUpperRight":
                        {
                            quadImage.B = ReadPoint(xmlReader, scale);
                            break;
                        }
                    case "PointLowerRight":
                        {
                            quadImage.C = ReadPoint(xmlReader, scale);
                            break;
                        }
                    case "PointLowerLeft":
                        {
                            quadImage.D = ReadPoint(xmlReader, scale);
                            break;
                        }
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
            
            if (!styleHelper.Perspective)
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
                w.WriteElementString("PointUpperLeft", XmlHelper.WritePointF(quadImage.A));
                w.WriteElementString("PointUpperRight", XmlHelper.WritePointF(quadImage.B));
                w.WriteElementString("PointLowerRight", XmlHelper.WritePointF(quadImage.C));
                w.WriteElementString("PointLowerLeft", XmlHelper.WritePointF(quadImage.D));
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
        
        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            // Initialize corners positions
            if (!initialized)
            {
                initialized = true;

                int horzTenth = (int)(((double)imageSize.Width) / 10);
                int vertTenth = (int)(((double)imageSize.Height) / 10);

                if (styleHelper.Perspective)
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
            get { return styleHelper.Color; }
        }
        public TrackingProfile CustomTrackingProfile
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
        
        public void Reset()
        {
            // Used on metadata over load.
            planeIsConvex = true;
            initialized = false;
            
            quadImage = quadPlane.Clone();
        }
        
        public void UpdateMapping(SizeF size)
        {
            planeWidth = size.Width;
            planeHeight = size.Height;
            quadPlane = new QuadrilateralF(planeWidth, planeHeight);
            
            projectiveMapping.Update(quadPlane, quadImage);
        }

        #region IMeasurable implementation
        public void InitializeMeasurableData(TrackExtraData trackExtraData)
        {
        }
        #endregion

        #region Private methods
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("Plane"));
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "GridDivisions", "divisions");
            style.Bind(styleHelper, "Perspective", "perspective");
        }
        private void StyleHelper_ValueChanged(object sender, EventArgs e)
        {
            // Handle the case where we convert from a perspective plane to a grid.
            // Note: we cannot force rectangle here because this would change the actual points, 
            // these points are not part of the "style" and so if we cancelled the style change we would not 
            // be able to go back to the original quad.
            planeIsConvex = styleHelper.Perspective ? quadImage.IsConvex : true;
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

    }
}
