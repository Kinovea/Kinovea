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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Plane")]
	public class DrawingPlane : AbstractDrawing, IDecorable, IKvaSerializable, IScalable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
		public override InfosFading infosFading
		{
			get { return m_InfosFading; }
            set { m_InfosFading = value; }
		}
		public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
		public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading; }
		}
        public int Divisions
        {
            get { return m_iDivisions; }
            set { m_iDivisions = value; }
        }
        #endregion

        #region Members
        private Quadrilateral m_Corners = Quadrilateral.UnitRectangle;
        private Quadrilateral m_RefPlane = Quadrilateral.UnitRectangle;
        
        private int m_iDivisions;
        private bool m_bSupport3D;
        
        private InfosFading m_InfosFading;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private Pen m_PenEdges = Pens.White;
        
        private bool m_bInitialized = false;
        private bool m_bValidPlane = true;
        private float m_fShift = 0F;                     // used only for expand/retract, to stay relative to the original mapping.

        private const int m_iMinimumDivisions = 2;
        private const int m_iDefaultDivisions = 8;
        private const int m_iMaximumDivisions = 20;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(int _divisions, bool _support3D, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_iDivisions = _divisions == 0 ? m_iDefaultDivisions : _divisions;
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
			
            RedefineHomography();
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
                    float[] homography = GetHomographyMatrix(quad.ToArray());
                    
                    // Rows
                    for (int iRow = 0; iRow <= m_iDivisions; iRow++)
                    {
                        float v = (float)iRow / m_iDivisions;
                        PointF h1 = ProjectiveMapping(new PointF(0, v), homography);
                        PointF h2 = ProjectiveMapping(new PointF(1, v), homography);
                        _canvas.DrawLine(m_PenEdges, h1, h2);
                    }
                
                    // Columns
                    for (int iCol = 0; iCol <= m_iDivisions; iCol++)
                    {
                        float h = (float)iCol / m_iDivisions;
                        PointF h1 = ProjectiveMapping(new PointF(h, 0), homography);
                        PointF h2 = ProjectiveMapping(new PointF(h, 1), homography);
                        _canvas.DrawLine(m_PenEdges, h1, h2);
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
                m_iDivisions = m_iDivisions + ((_deltaX - _deltaY)/4);
                m_iDivisions = Math.Min(Math.Max(m_iDivisions, m_iMinimumDivisions), m_iMaximumDivisions);
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
                        float[] homography = GetHomographyMatrix(m_RefPlane.ToArray());
                        float fShift = m_fShift + ((float)(_deltaX - _deltaY) / 200);

                        PointF[] shiftedCorners = new PointF[4];
                        shiftedCorners[0] = ProjectiveMapping(new PointF(-fShift, -fShift), homography);
                        shiftedCorners[1] = ProjectiveMapping(new PointF(1 + fShift, -fShift), homography);
                        shiftedCorners[2] = ProjectiveMapping(new PointF(1 + fShift, 1 + fShift), homography);
                        shiftedCorners[3] = ProjectiveMapping(new PointF(-fShift, 1 + fShift), homography);
                        
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
				        m_iDivisions = _xmlReader.ReadElementContentAsInt();
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
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("PointUpperLeft", String.Format("{0};{1}", m_Corners.A.X, m_Corners.A.Y));
		    _xmlWriter.WriteElementString("PointUpperRight", String.Format("{0};{1}", m_Corners.B.X, m_Corners.B.Y));
		    _xmlWriter.WriteElementString("PointLowerRight", String.Format("{0};{1}", m_Corners.C.X, m_Corners.C.Y));
		    _xmlWriter.WriteElementString("PointLowerLeft", String.Format("{0};{1}", m_Corners.D.X, m_Corners.D.Y));
		    
            _xmlWriter.WriteElementString("Divisions", m_iDivisions.ToString());
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
            m_iDivisions = m_iDefaultDivisions;
            m_fShift = 0.0F;
            m_bValidPlane = true;
            m_bInitialized = false;
            m_Corners = Quadrilateral.UnitRectangle;
        }
        
        #region Private methods
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
        }   
        private void RedefineHomography()
        {
            m_RefPlane = m_Corners.Clone();
        }
        private float[] GetHomographyMatrix(Point[] _SourceCoords)
        {
            float[] homography = new float[18];

            float sx = (_SourceCoords[0].X - _SourceCoords[1].X) + (_SourceCoords[2].X - _SourceCoords[3].X);
            float sy = (_SourceCoords[0].Y - _SourceCoords[1].Y) + (_SourceCoords[2].Y - _SourceCoords[3].Y);
            float dx1 = _SourceCoords[1].X - _SourceCoords[2].X;
            float dx2 = _SourceCoords[3].X - _SourceCoords[2].X;
            float dy1 = _SourceCoords[1].Y - _SourceCoords[2].Y;
            float dy2 = _SourceCoords[3].Y - _SourceCoords[2].Y;

            float z = (dx1 * dy2) - (dy1 * dx2);
            float g = ((sx * dy2) - (sy * dx2)) / z;
            float h = ((sy * dx1) - (sx * dy1)) / z;

            // Transformation matrix. From the square to the quadrilateral.
            float a = homography[0] = _SourceCoords[1].X - _SourceCoords[0].X + g * _SourceCoords[1].X;
            float b = homography[1] = _SourceCoords[3].X - _SourceCoords[0].X + h * _SourceCoords[3].X;
            float c = homography[2] = _SourceCoords[0].X;
            float d = homography[3] = _SourceCoords[1].Y - _SourceCoords[0].Y + g * _SourceCoords[1].Y;
            float e = homography[4] = _SourceCoords[3].Y - _SourceCoords[0].Y + h * _SourceCoords[3].Y;
            float f = homography[5] = _SourceCoords[0].Y;
            homography[6] = g;
            homography[7] = h;
            homography[8] = 1;

            // Inverse Transformation Matrix. From the quadrilateral to our square.
            homography[9] = e - f * h;
            homography[10] = c * h - b;
            homography[11] = b * f - c * e;
            homography[12] = f * g - d;
            homography[13] = a - c * g;
            homography[14] = c * d - a * f;
            homography[15] = d * h - e * g;
            homography[16] = b * g - a * h;
            homography[17] = a * e - b * d;

            return homography;
        }
        private PointF ProjectiveMapping(PointF _SourcePoint, float[] _Homography) 
        {
            double x = (_Homography[0] * _SourcePoint.X + _Homography[1] * _SourcePoint.Y + _Homography[2]) / (_Homography[6] * _SourcePoint.X + _Homography[7] * _SourcePoint.Y + 1);
            double y = (_Homography[3] * _SourcePoint.X + _Homography[4] * _SourcePoint.Y + _Homography[5]) / (_Homography[6] * _SourcePoint.X + _Homography[7] * _SourcePoint.Y + 1);

            return new PointF((float)x, (float)y);
        }
        private PointF InverseProjectiveMapping(PointF _SourcePoint, float[] _Homography)
        {
            double x = (_Homography[9] * _SourcePoint.X + _Homography[10] * _SourcePoint.Y + _Homography[11]) / (_Homography[15] * _SourcePoint.X + _Homography[16] * _SourcePoint.Y + 1);
            double y = (_Homography[12] * _SourcePoint.X + _Homography[13] * _SourcePoint.Y + _Homography[14]) / (_Homography[15] * _SourcePoint.X + _Homography[16] * _SourcePoint.Y + 1);
            double z = (_Homography[18] * _SourcePoint.X + _Homography[16] * _SourcePoint.Y + _Homography[17]) / (_Homography[15] * _SourcePoint.X + _Homography[16] * _SourcePoint.Y + 1);
            
            return new PointF((float)x / (float)z, (float)y / (float)z);
        }
        #endregion

    }
}
