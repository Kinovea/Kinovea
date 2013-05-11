#region License
/*
Copyright © Joan Charmant 2008.
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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Plane")]
	public class DrawingPlane : AbstractDrawing, IDecorable, IKvaSerializable, IScalable, IMeasurable, ITrackable
    {
	    #region Events
	    public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
        public event EventHandler ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        public override string DisplayName
        {
		    get 
		    {  
                if(inPerspective)
                    return ToolManager.Plane.DisplayName;
                else
                    return ToolManager.Grid.DisplayName;
		    }
        }
        public override int ContentHash
        {
            get 
            { 
                int iHash = quadImage.A.GetHashCode();
                iHash ^= quadImage.B.GetHashCode();
                iHash ^= quadImage.C.GetHashCode();
                iHash ^= quadImage.D.GetHashCode();
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
			get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading | DrawingCapabilities.Track; }
		}
        public QuadrilateralF QuadImage
        {
            get { return quadImage;}
        }
        
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        public bool UsedForCalibration { get; set; }
        #endregion

        #region Members
        private QuadrilateralF quadImage = QuadrilateralF.UnitSquare;        // Quadrilateral defined by user.
        private QuadrilateralF quadPlane;                                       // Corresponding rectangle in plane system.
        private ProjectiveMapping projectiveMapping = new ProjectiveMapping();  // maps quadImage to quadPlane and back.
        private float planeWidth;                                               // width and height of rectangle in plane system.
        private float planeHeight;
        
        private bool inPerspective;
        private bool planeIsConvex = true;
        
        private Guid id = Guid.NewGuid();
        private bool tracking;
        
        private InfosFading infosFading;
        private StyleHelper styleHelper = new StyleHelper();
        private DrawingStyle style;
        private Pen penEdges = Pens.White;
        
        private bool initialized = false;
        
        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();
        
        private const int minimumSubdivisions = 2;
        private const int defaultSubdivisions = 8;
        private const int maximumSubdivisions = 20;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(bool inPerspective, long timestamp, long averageTimeStampsPerFrame, DrawingStyle preset)
        {
            this.inPerspective = inPerspective;
            
            // Decoration
            styleHelper.Color = Color.Empty;
            styleHelper.GridDivisions = 8;
            if(preset != null)
            {
			    style = preset.Clone();
			    BindStyle();
            }
			
			infosFading = new InfosFading(timestamp, averageTimeStampsPerFrame);
			infosFading.UseDefault = false;
            infosFading.AlwaysVisible = true;
            
            planeWidth = 100;
            planeHeight = 100;
            quadPlane = new QuadrilateralF(planeWidth, planeHeight);
            
            mnuCalibrate.Click += new EventHandler(mnuCalibrate_Click);
			mnuCalibrate.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingPlane(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(false, 0, 0, ToolManager.Grid.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion
        
        #region AbstractDrawing implementation
        public override void Draw(Graphics canvas, CoordinateSystem transformer, bool selected, long currentTimestamp)
		{
        	double opacityFactor = infosFading.GetOpacityFactor(currentTimestamp);
        	if(opacityFactor <= 0)
        	   return;
        	
            QuadrilateralF quad = transformer.Transform(quadImage);
            
            using(penEdges = styleHelper.GetPen(opacityFactor, 1.0))
            using(SolidBrush br = styleHelper.GetBrush(opacityFactor))
            {
                // Handlers
                foreach(PointF p in quad)
                    canvas.FillEllipse(br, p.Box(4));
                
                // Grid
                if (planeIsConvex)
                {
                    projectiveMapping.Update(quadPlane, quadImage);
                    
                    int start = 0;
                    int end = styleHelper.GridDivisions;
                    int total = styleHelper.GridDivisions;
                    
                    // Rows
                    for (int i = start; i <= end; i++)
                    {
                        float v = i * ((float)planeHeight / total);
                        PointF p1 = projectiveMapping.Forward(new PointF(0, v));
                        PointF p2 = projectiveMapping.Forward(new PointF(planeWidth, v));
                        
                        canvas.DrawLine(penEdges, transformer.Transform(p1), transformer.Transform(p2));
                    }
                
                    // Columns
                    for (int i = start ; i <= end; i++)
                    {
                        float h = i * (planeWidth / total);
                        PointF p1 = projectiveMapping.Forward(new PointF(h, 0));
                        PointF p2 = projectiveMapping.Forward(new PointF(h, planeHeight));
                        
                        canvas.DrawLine(penEdges, transformer.Transform(p1), transformer.Transform(p2));
                    }
                }
                else
                {
                    // Non convex quadrilateral: only draw the edges.
                    canvas.DrawLine(penEdges, quad.A, quad.B);
                    canvas.DrawLine(penEdges, quad.B, quad.C);
                    canvas.DrawLine(penEdges, quad.C, quad.D);
                    canvas.DrawLine(penEdges, quad.D, quad.A);
                }
            }
		}
		public override int HitTest(Point point, long currentTimestamp, CoordinateSystem transformer)
		{
            if(infosFading.GetOpacityFactor(currentTimestamp) <= 0)
                return -1;
            
            int boxSide = transformer.Untransform(6);
            
            for(int i = 0; i < 4; i++)
            {
                if(quadImage[i].Box(boxSide).Contains(point))
                    return i+1;
            }
            
            if (quadImage.Contains(point))
                return 0;
            
            return -1;
		}
		public override void MoveDrawing(int dx, int dy, Keys modifierKeys)
		{
			if ((modifierKeys & Keys.Alt) == Keys.Alt)
            {
                // Change the number of divisions.
                styleHelper.GridDivisions = styleHelper.GridDivisions + ((dx - dy)/4);
                styleHelper.GridDivisions = Math.Min(Math.Max(styleHelper.GridDivisions, minimumSubdivisions), maximumSubdivisions);
            }
			else
            {
                if(inPerspective)
                    TranslateInPlane(dx, dy);
                else
                    quadImage.Translate(dx, dy);
            }
            
            SignalAllTrackablePointsMoved();
		}
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
		{
		    int handle = handleNumber - 1;
		    quadImage[handle] = point;
		    
			if (inPerspective)
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
            
            SignalTrackablePointMoved(handle);
		}
		#endregion
	
		#region KVA Serialization
		private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
            Reset();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "PointUpperLeft":
				        {
				            Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            quadImage.A = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				            break;
				        }
				    case "PointUpperRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            quadImage.B = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            quadImage.C = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerLeft":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            quadImage.D = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "Perspective":
                        inPerspective = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
					case "DrawingStyle":
						style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "InfosFading":
						infosFading.ReadXml(_xmlReader);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
			// Sanity check for rectangular constraint.
			if(!inPerspective && !quadImage.IsRectangle)
                inPerspective = true;
                
			initialized = true;
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("PointUpperLeft", String.Format(CultureInfo.InvariantCulture, "{0};{1}", (int)quadImage.A.X, (int)quadImage.A.Y));
		    _xmlWriter.WriteElementString("PointUpperRight", String.Format(CultureInfo.InvariantCulture, "{0};{1}", (int)quadImage.B.X, (int)quadImage.B.Y));
		    _xmlWriter.WriteElementString("PointLowerRight", String.Format(CultureInfo.InvariantCulture, "{0};{1}", (int)quadImage.C.X, (int)quadImage.C.Y));
		    _xmlWriter.WriteElementString("PointLowerLeft", String.Format(CultureInfo.InvariantCulture, "{0};{1}", (int)quadImage.D.X, (int)quadImage.D.Y));
		    
            _xmlWriter.WriteElementString("Perspective", inPerspective ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            infosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
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

                if (inPerspective)
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
        public Guid ID
        {
            get { return id; }
        }
        public Dictionary<string, Point> GetTrackablePoints()
        {
            Dictionary<string, Point> points = new Dictionary<string, Point>();

            for(int i = 0; i<4; i++)
                points.Add(i.ToString(), new Point((int)quadImage[i].X, (int)quadImage[i].Y));
            
            return points;
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, Point value)
        {
            int p = int.Parse(name);
            quadImage[p] = new PointF(value.X, value.Y);

            projectiveMapping.Update(quadPlane, quadImage);
        }
        private void SignalAllTrackablePointsMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            for(int i = 0; i<4; i++)
            {
                Point p = new Point((int)quadImage[i].X, (int)quadImage[i].Y);
                TrackablePointMoved(this, new TrackablePointMovedEventArgs(i.ToString(), p));
            }
        }
        private void SignalTrackablePointMoved(int index)
        {
            if(TrackablePointMoved == null)
                return;
            
            Point p = new Point((int)quadImage[index].X, (int)quadImage[index].Y);
            TrackablePointMoved(this, new TrackablePointMovedEventArgs(index.ToString(), p));
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
        
        #region Private methods
        private void BindStyle()
        {
            style.Bind(styleHelper, "Color", "color");
            style.Bind(styleHelper, "GridDivisions", "divisions");
        }   
        
        private void TranslateInPlane(int deltaX, int deltaY)
        {
            // Translate the plane but can't keep the grid sticky with the pointer.
            // TODO: MoveDrawing() should receive the actual start and end points,
            // because here a delta doesn't have the same meaning depending on its location.
            PointF start = quadImage.A;
            PointF end = quadImage.A.Translate(deltaX, deltaY);
            TranslateInPlane(start, end);
        }
        
        private void TranslateInPlane(PointF start, PointF end)
        {
            // Translate grid on the plane, keeps the same part of the grid under the pointer.
            PointF old = projectiveMapping.Backward(start);
		    PointF now = projectiveMapping.Backward(end);
		    float dx = now.X - old.X;
		    float dy = now.Y - old.Y;
		    QuadrilateralF grid = quadPlane.Clone();
		    grid.Translate(dx, dy);
		    
		    PointF a = projectiveMapping.Forward(grid.A);
    		PointF b = projectiveMapping.Forward(grid.B);
    		PointF c = projectiveMapping.Forward(grid.C);
    		PointF d = projectiveMapping.Forward(grid.D);
    		quadImage = new QuadrilateralF(a, b, c, d);
        }
        
        private void mnuCalibrate_Click(object sender, EventArgs e)
        {
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.DeactivateKeyboardHandler != null)
                dp.DeactivateKeyboardHandler();
            
            FormCalibratePlane fcp = new FormCalibratePlane(CalibrationHelper, this);
            FormsHelper.Locate(fcp);
            fcp.ShowDialog();
            fcp.Dispose();
            
            CallInvalidateFromMenu(sender);
            
            if (dp.ActivateKeyboardHandler != null)
                dp.ActivateKeyboardHandler();
        }
        #endregion

    }
}
