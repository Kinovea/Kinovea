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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Plane")]
	public class DrawingPlane : AbstractDrawing, IDecorable, IKvaSerializable, IScalable, IMeasurable
    {
	    #region Events
        public event EventHandler ShowMeasurableInfoChanged;
        #endregion
        
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
		public override InfosFading InfosFading
		{
			get { return m_InfosFading; }
            set { m_InfosFading = value; }
		}
		public override List<ToolStripItem> ContextMenu
		{
            get
            {
                List<ToolStripItem> contextMenu = new List<ToolStripItem>();
                
                mnuCalibrate.Text = ScreenManagerLang.mnuSealMeasure;
                contextMenu.Add(mnuCalibrate);

                return contextMenu;
		    }
		}
		public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading; }
		}
        public int Subdivisions
        {
            get { return subdivisions; }
            set { subdivisions = value; }
        }
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        public bool UsedForCalibration { get; set; }
        #endregion

        #region Members
        private Quadrilateral m_Corners = Quadrilateral.UnitRectangle;  // Coordinates of corners in Image system.
        private Quadrilateral m_RefPlane = Quadrilateral.UnitRectangle; // Corners in image system prior to expanding. (?)
        
        private int planeWidth;
        private int planeHeight;
        private Quadrilateral basePlane;                // Coordinates of corners in Plane system.
        
        private ProjectiveMapping projectiveMapping = new ProjectiveMapping();
        
        private int subdivisions;
        private bool m_bSupport3D;
        
        private InfosFading m_InfosFading;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private Pen m_PenEdges = Pens.White;
        
        private bool m_bInitialized = false;
        private bool m_bValidPlane = true;
        private float m_fShift = 0F;                     // used only for expand/retract, to stay relative to the original mapping.

        private ToolStripMenuItem mnuCalibrate = new ToolStripMenuItem();
        
        private const int m_iMinimumDivisions = 2;
        private const int m_iDefaultDivisions = 8;
        private const int m_iMaximumDivisions = 20;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(int _divisions, bool _support3D, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            subdivisions = _divisions == 0 ? m_iDefaultDivisions : _divisions;
            m_bSupport3D = _support3D;
            
            // Decoration
            m_StyleHelper.Color = Color.Empty;
            if(_preset != null)
            {
			    m_Style = _preset.Clone();
			    BindStyle();
            }
			
			m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
			m_InfosFading.UseDefault = false;
            m_InfosFading.AlwaysVisible = true;
            
            planeWidth = 200;
            planeHeight = 100;
            basePlane = new Quadrilateral(){
                A = new Point(0, 0),
                B = new Point(planeWidth, 0),
                C = new Point(planeWidth, planeHeight),
                D = new Point(0, planeHeight)
            };
			
            RedefineHomography();
            
            mnuCalibrate.Click += new EventHandler(mnuCalibrate_Click);
			mnuCalibrate.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingPlane(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(m_iDefaultDivisions, false, 0, 0, ToolManager.Grid.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion
        
        #region AbstractDrawing implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
        	double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
        	if(fOpacityFactor <= 0)
        	   return;
        	
            Quadrilateral quad = _transformer.Transform(m_Corners);
            
            using(m_PenEdges = m_StyleHelper.GetPen(fOpacityFactor, 1.0))
            using(SolidBrush br = m_StyleHelper.GetBrush(fOpacityFactor))
            {
                // Handlers
                foreach(Point p in quad)
                    _canvas.FillEllipse(br, p.Box(4));
                
                // Grid
                if (m_bValidPlane)
                {
                    InitProjectiveMapping(m_Corners);
                    
                    // Rows
                    //int start = - subdivisions;
                    //int end = subdivisions * 2;
                    //int total = subdivisions * 3;
                    
                    int start = 0;
                    int end = subdivisions;
                    int total = subdivisions;
                    
                    for (int i = start; i <= end; i++)
                    {
                        float v = i * ((float)planeHeight / total);
                        PointF h1 = PlaneToImageTransform(new PointF(0, v));
                        PointF h2 = PlaneToImageTransform(new PointF(planeWidth, v));
                        
                        _canvas.DrawLine(m_PenEdges, _transformer.Transform(h1), _transformer.Transform(h2));
                    }
                
                    // Columns
                    for (int i = start ; i <= end; i++)
                    {
                        float h = i * ((float)planeWidth / total);
                        PointF h1 = PlaneToImageTransform(new PointF(h, 0));
                        PointF h2 = PlaneToImageTransform(new PointF(h, planeHeight));
                        
                        _canvas.DrawLine(m_PenEdges, _transformer.Transform(h1), _transformer.Transform(h2));
                    }
                }
                else
                {
                    // Non convex quadrilateral: only draw the borders
                    _canvas.DrawLine(m_PenEdges, quad.A, quad.B);
                    _canvas.DrawLine(m_PenEdges, quad.B, quad.C);
                    _canvas.DrawLine(m_PenEdges, quad.C, quad.D);
                    _canvas.DrawLine(m_PenEdges, quad.D, quad.A);
                }
            }
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
		{
			int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if(fOpacityFactor > 0)
            {
                for(int i = 0; i < 4; i++)
                {
                    if(m_Corners[i].Box(6).Contains(_point))
                        iHitResult = i+1;
                }
                
	            if (iHitResult == -1 && m_Corners.Contains(_point))
	                iHitResult = 0;
            }
            
            return iHitResult;
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
			if ((_ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                // Just change the number of divisions.
                subdivisions = subdivisions + ((_deltaX - _deltaY)/4);
                subdivisions = Math.Min(Math.Max(subdivisions, m_iMinimumDivisions), m_iMaximumDivisions);
            }
            else if ((_ModifierKeys & Keys.Control) == Keys.Control)
            {
                // Expand the grid while staying on the same plane.
                int Offset = _deltaX;
                
                if (m_bSupport3D)
                {
                    if (m_bValidPlane)
                    {
                        // find new corners by growing the current homography.
                        InitProjectiveMapping(m_RefPlane);
                        float fShift = m_fShift + ((float)(_deltaX - _deltaY) / 200);

                        PointF[] shiftedCorners = new PointF[4];
                        shiftedCorners[0] = PlaneToImageTransform(new PointF(-fShift, -fShift));
                        shiftedCorners[1] = PlaneToImageTransform(new PointF(1 + fShift, -fShift));
                        shiftedCorners[2] = PlaneToImageTransform(new PointF(1 + fShift, 1 + fShift));
                        shiftedCorners[3] = PlaneToImageTransform(new PointF(-fShift, 1 + fShift));
                        
                        try
                        {
                            Quadrilateral expanded = new Quadrilateral() {
                                A = new Point((int)shiftedCorners[0].X, (int)shiftedCorners[0].Y),
                                B = new Point((int)shiftedCorners[1].X, (int)shiftedCorners[1].Y),
                                C = new Point((int)shiftedCorners[2].X, (int)shiftedCorners[2].Y),
                                D = new Point((int)shiftedCorners[3].X, (int)shiftedCorners[3].Y),
                            };
                            
                            m_fShift = fShift;
                            m_Corners = expanded.Clone();
                        }
                        catch(OverflowException)
                        {
                            log.Debug("Overflow during grid expansion");
                        }
                    }
                }
                else
                {
                    float fGrowFactor = 1 + ((float)Offset / 100); // for offset [-10;+10] => Growth [0.9;1.1]

                    int width = m_Corners.B.X - m_Corners.A.X;
                    int height = m_Corners.D.Y - m_Corners.A.Y;

                    float fNewWidth = fGrowFactor * width;
                    float fNewHeight = fGrowFactor * height;

                    int shiftx = (int)((fNewWidth - width) / 2);
                    int shifty = (int)((fNewHeight - height) / 2);
                    
                    m_Corners.Expand(shiftx, shifty);
                }
            }
            else
            {
                m_Corners.Translate(_deltaX, _deltaY);
                RedefineHomography();
                m_fShift = 0F;
            }
		}
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
		{
		    m_Corners[handleNumber - 1] = point;
		
			if (m_bSupport3D)
			{
			    m_bValidPlane = m_Corners.IsConvex;
			}
            else
            {
                if((modifiers & Keys.Shift) == Keys.Shift)
                    m_Corners.MakeSquare(handleNumber - 1);
                else
                    m_Corners.MakeRectangle(handleNumber - 1);
            }
            
            RedefineHomography();
            m_fShift = 0F;
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
                            m_Corners.A = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				            break;
				        }
				    case "PointUpperRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_Corners.B = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_Corners.C = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerLeft":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_Corners.D = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "Divisions":
				        subdivisions = _xmlReader.ReadElementContentAsInt();
                        break;
                    case "Perspective":
                        m_bSupport3D = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
                        break;
					case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "InfosFading":
						m_InfosFading.ReadXml(_xmlReader);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
			// Sanity check for rectangular constraint.
			if(!m_bSupport3D && !m_Corners.IsRectangle)
                m_bSupport3D = true;
                
			RedefineHomography();
			m_bInitialized = true;
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("PointUpperLeft", String.Format("{0};{1}", m_Corners.A.X, m_Corners.A.Y));
		    _xmlWriter.WriteElementString("PointUpperRight", String.Format("{0};{1}", m_Corners.B.X, m_Corners.B.Y));
		    _xmlWriter.WriteElementString("PointLowerRight", String.Format("{0};{1}", m_Corners.C.X, m_Corners.C.Y));
		    _xmlWriter.WriteElementString("PointLowerLeft", String.Format("{0};{1}", m_Corners.D.X, m_Corners.D.Y));
		    
            _xmlWriter.WriteElementString("Divisions", subdivisions.ToString());
            _xmlWriter.WriteElementString("Perspective", m_bSupport3D ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
        }
		
		#endregion
		
		#region IScalable implementation
		public void Scale(Size imageSize)
		{
		    // Initialize corners positions
            if (!m_bInitialized)
            {
                m_bInitialized = true;

                int horzTenth = (int)(((double)imageSize.Width) / 10);
                int vertTenth = (int)(((double)imageSize.Height) / 10);

                if (m_bSupport3D)
                {
                    // Initialize with a faked perspective.
                    m_Corners.A = new Point(3 * horzTenth, 4 * vertTenth);
                    m_Corners.B = new Point(7 * horzTenth, 4 * vertTenth);
                    m_Corners.C = new Point(9 * horzTenth, 8 * vertTenth);
                    m_Corners.D = new Point(1 * horzTenth, 8 * vertTenth);
                }
                else
                {
                    // initialize with a rectangle.
                    m_Corners.A = new Point(2 * horzTenth, 2 * vertTenth);
                    m_Corners.B = new Point(8 * horzTenth, 2 * vertTenth);
                    m_Corners.C = new Point(8 * horzTenth, 8 * vertTenth);
                    m_Corners.D = new Point(2 * horzTenth, 8 * vertTenth);
                }
            }
            
            RedefineHomography();
            m_fShift = 0.0F;
		}
		#endregion
		
        public void Reset()
        {
            // Used on metadata over load.
            subdivisions = m_iDefaultDivisions;
            m_fShift = 0.0F;
            m_bValidPlane = true;
            m_bInitialized = false;
            m_Corners = basePlane.Clone();
        }
        
        public void SetUsedForCalibration(bool used)
        {
            UsedForCalibration = used;
            RedefineHomography();
        }
        
        #region Private methods
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
        }   
        private void RedefineHomography()
        {
            m_RefPlane = m_Corners.Clone();
            
            // If used for the main calibration.
            if(UsedForCalibration && CalibrationHelper != null)
                CalibrationHelper.CalibrationByPlane_InitProjection(m_Corners);
        }
        
        private void InitProjectiveMapping(Quadrilateral quad)
        {
            projectiveMapping.Init(basePlane, quad);
        }
        
        private PointF PlaneToImageTransform(PointF p) 
        {
            return projectiveMapping.Forward(p);
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
            
            if(UsedForCalibration && CalibrationHelper != null)
                CalibrationHelper.CalibrationByPlane_InitProjection(m_Corners);
            
            CallInvalidateFromMenu(sender);
            
            if (dp.ActivateKeyboardHandler != null)
                dp.ActivateKeyboardHandler();
        }
        #endregion

    }
}
