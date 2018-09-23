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
    [XmlType("DistortionGrid")]
    public class DrawingDistortionGrid : AbstractDrawing, IDecorable, IScalable, IKvaSerializable
    {
        #region Events
        public event EventHandler LensCalibrationAsked;
        #endregion

        #region Properties
        public override string ToolDisplayName
        {
            get { return ToolManager.Tools["DistortionGrid"].DisplayName; }
        }
        public override int ContentHash
        {
            get
            {
                int iHash = 0;

                foreach (PointF p in points)
                    iHash ^= p.GetHashCode();

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
            get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.CopyPaste; }
        }

        public CalibrationHelper CalibrationHelper { get; set; }
        public List<PointF> Points
        {
            get { return points; }
        }    
        #endregion

        #region Members
        private List<PointF> points = new List<PointF>();
        
        private InfosFading infosFading;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private Pen penEdges = Pens.White;

        private bool initialized = false;

        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();

        private int subdivisions = 4;
        private const int minimumSubdivisions = 2;
        private const int defaultSubdivisions = 4;
        private const int maximumSubdivisions = 20;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingDistortionGrid(PointF point, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset = null)
        {
            styleHelper.Color = Color.Empty;
            if (preset == null)
                preset = ToolManager.GetStylePreset("DistortionGrid");
            
            style = preset.Clone();
            BindStyle();
            
            infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
            infosFading.UseDefault = true;
            infosFading.AlwaysVisible = false;

            mnuCalibrate.Click += new EventHandler(mnuCalibrate_Click);
            mnuCalibrate.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingDistortionGrid(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata parent)
            : this(PointF.Empty, 0, 0)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, DistortionHelper distorter, IImageToViewportTransformer transformer, bool selected, long currentTimestamp)
        {
            double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
            if (opacityFactor <= 0)
                return;

            List<Point> screenPoints = transformer.Transform(points);

            using (penEdges = styleHelper.GetPen(opacityFactor, 1.0))
            using (SolidBrush br = styleHelper.GetBrush(opacityFactor))
            {
                int rows = subdivisions + 1;
                int cols = rows;

                // Rows
                for (int i = 0; i < rows; i++)
                {
                    int row = i;
                    List<Point> line = new List<Point>();
                    for (int j = 0; j < cols; j++)
                    {
                        int index = row * cols + j;
                        line.Add(screenPoints[index]);
                    }
                
                    canvas.DrawCurve(penEdges, line.ToArray());
                }

                // Columns
                for (int i = 0; i < cols; i++)
                {
                    int col = i;
                    List<Point> line = new List<Point>();
                    for (int j = 0; j < rows; j++)
                    {
                        int index = j * cols + col;
                        line.Add(screenPoints[index]);
                    }

                    canvas.DrawCurve(penEdges, line.ToArray());
                }

                // Handles
                foreach (PointF p in screenPoints)
                    canvas.FillEllipse(br, p.Box(4));
            }
        }
        public override int HitTest(PointF point, long currentTimestamp, DistortionHelper distorter, IImageToViewportTransformer transformer, bool zooming)
        {
            // Convention: miss = -1, object = 0, handle = n.

            if (infosFading.GetOpacityFactor(currentTimestamp) <= 0)
                return -1;

            for (int i = 0; i < points.Count; i++)
            {
                if (HitTester.HitTest(points[i], point, transformer))
                    return i + 1;
            }

            return -1;
        }
        public override void MoveDrawing(float dx, float dy, Keys modifierKeys, bool zooming)
        {
        }
        public override void MoveHandle(PointF point, int handleNumber, Keys modifiers)
        {
            int handle = handleNumber - 1;
            points[handle] = point;
        }
        public override PointF GetCopyPoint()
        {
            return points[0];
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

            while (xmlReader.NodeType == XmlNodeType.Element)
            {
                switch (xmlReader.Name)
                {
                    case "Points":
                        {
                            ParsePointList(xmlReader, scale);
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

            initialized = true;
        }
        private void ParsePointList(XmlReader r, PointF scale)
        {
            points = new List<PointF>();

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                if (r.Name == "Point")
                {
                    PointF p = XmlHelper.ParsePointF(r.ReadElementContentAsString());
                    PointF adapted = new PointF(p.X * scale.X, p.Y * scale.Y);
                    points.Add(adapted);
                }
                else
                {
                    string unparsed = r.ReadOuterXml();
                    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }

            r.ReadEndElement();

            // update subdivisions.
            int count = (int)Math.Floor(Math.Sqrt(points.Count));
            int total = count * count;

            int superfluous = points.Count - total;
            if (superfluous > 0)
                points.RemoveRange(total, superfluous);

            subdivisions = count - 1;
        }
        
        public void WriteXml(XmlWriter w, SerializationFilter filter)
        {
            if (ShouldSerializeCore(filter))
            {
                w.WriteStartElement("Points");
                foreach (PointF p in points)
                    w.WriteElementString("Point", String.Format(CultureInfo.InvariantCulture, "{0};{1}", p.X, p.Y));
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

        #region IScalable implementation
        public void Scale(Size imageSize)
        {
            if (initialized)
                return;

            initialized = true;

            int rows = subdivisions + 1;
            int cols = rows;

            int horzTenth = (int)(((double)imageSize.Width) / 10);
            int vertTenth = (int)(((double)imageSize.Height) / 10);

            int left = 2 * horzTenth;
            int width = 6 * horzTenth;
            int horzStep = width / cols;

            int top = 2 * vertTenth;
            int height = 6 * vertTenth;
            int vertStep = height / rows;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int x = left + j * horzStep;
                    int y = top + i * vertStep;
                    points.Add(new PointF(x, y));
                }
            }
        }
        #endregion

        #region Private methods
        private void BindStyle()
        {
            DrawingStyle.SanityCheck(style, ToolManager.GetStylePreset("DistortionGrid"));
            style.Bind(styleHelper, "Color", "color");
        }

        private void mnuCalibrate_Click(object sender, EventArgs e)
        {
            if (LensCalibrationAsked != null)
                LensCalibrationAsked(this, EventArgs.Empty);

            InvalidateFromMenu(sender);
        }
        #endregion

    }
}
