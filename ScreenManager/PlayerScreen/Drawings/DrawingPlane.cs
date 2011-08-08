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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    [XmlType ("Plane")]
	public class DrawingPlane : AbstractDrawing, IDecorable, IKvaSerializable
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
        private double m_fStretchFactor = 1.0;
        private Point m_DirectZoomTopLeft;
        
        private int m_iDivisions;
        private bool m_bSupport3D;
        
        private InfosFading m_InfosFading;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private Pen m_PenEdges = Pens.White;

        private Point[] m_SourceCorners;            // unscaled quadrilateral.
        private Point[] m_RescaledSourceCorners;    // rescaled quadrilateral.
        private Point[] m_HomoPlane;                // quadrilateral defining the reference plane. (in rescaled coordinates)

        private bool m_bInitialized = false;
        private bool m_bValidPlane = true;
        private float m_fShift = 0.0F;                     // used only for expand/retract, to stay relative to the original mapping.

        private static readonly int m_iMinimumSurface = 5000;
        private static readonly int m_iMinimumDivisions = 2;
        private static readonly int m_iDefaultDivisions = 8;
        private static readonly int m_iMaximumDivisions = 20;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingPlane(int _divisions, bool _support3D, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_fStretchFactor = 1.0f;
            m_DirectZoomTopLeft = new Point(0, 0);

            m_iDivisions = _divisions;
            if (m_iDivisions == 0) m_iDivisions = m_iDefaultDivisions;
            
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
			
            m_bSupport3D = _support3D;

            m_fShift = 0;

            m_HomoPlane = new Point[4];
            m_HomoPlane[0] = new Point(0, 0);
            m_HomoPlane[1] = new Point(1, 0);
            m_HomoPlane[2] = new Point(1, 1);
            m_HomoPlane[3] = new Point(0, 1);

            m_SourceCorners = new Point[4];
            m_SourceCorners[0] = new Point(0, 0);
            m_SourceCorners[1] = new Point(1, 0);
            m_SourceCorners[2] = new Point(1, 1);
            m_SourceCorners[3] = new Point(0, 1);

            m_RescaledSourceCorners = new Point[4];
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            RedefineHomography();
        }
        public DrawingPlane(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(m_iDefaultDivisions, false, 0, 0, ToolManager.Grid.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion
        
        
        public void Reset()
        {
            // Used on metadata over load.
            m_iDivisions = m_iDefaultDivisions;
            m_fShift = 0.0F;
            m_bValidPlane = true;
            m_bInitialized = false;

            m_SourceCorners[0] = new Point(0, 0);
            m_SourceCorners[1] = new Point(1, 0);
            m_SourceCorners[2] = new Point(1, 1);
            m_SourceCorners[3] = new Point(0, 1);

            RescaleCoordinates(1.0, new Point(0, 0));
            RedefineHomography();
        }

        #region AbstractDrawing implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
		{
        	double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
        	
			if (m_fStretchFactor != _fStretchFactor || _DirectZoomTopLeft.X != m_DirectZoomTopLeft.X || _DirectZoomTopLeft.Y != m_DirectZoomTopLeft.Y)
            {
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                RedefineHomography();
            }
            
			m_PenEdges = m_StyleHelper.GetPen((int)(fOpacityFactor * 255), 1.0);
            
            // Draw handlers as small filled circle.
            SolidBrush br = new SolidBrush(m_PenEdges.Color);
            for (int i = 0; i < m_RescaledSourceCorners.Length; i++)
            {
                _canvas.FillEllipse(br, GetRescaledHandleRectangle(i+1));
            }
            br.Dispose();
            
            if (m_bSupport3D)
            {
                if (m_bValidPlane)
                {
                    // Compute the homography that turns a [0,1] square into our quadrilateral.
                    // We use the RescaledCoordinates here, as they may have been expanded/contracted. 
                    // m_HomoPlane will keep the original associated coords.
                    // We need to keep the whole original homography because expand/contract is subject to rounding errors
                    float[] homography = GetHomographyMatrix(m_RescaledSourceCorners);
                    
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
                    // Invalid quadrilateral (not convex) Only draw the borders
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[0], m_RescaledSourceCorners[1]);
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[1], m_RescaledSourceCorners[2]);
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[2], m_RescaledSourceCorners[3]);
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[3], m_RescaledSourceCorners[0]);
                }
            }
            else
            {
                // For the 2d plane we don't use the homography at all.
                float fRowLength =  (float)(m_RescaledSourceCorners[0].Y - m_RescaledSourceCorners[3].Y) / m_iDivisions;
                float fColLength = (float)(m_RescaledSourceCorners[0].X - m_RescaledSourceCorners[1].X) / m_iDivisions;

                // Rows
                for (int iRow = 0; iRow <= m_iDivisions; iRow++)
                {
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[0].X, m_RescaledSourceCorners[3].Y + (iRow * fRowLength), m_RescaledSourceCorners[1].X, m_RescaledSourceCorners[3].Y + (iRow * fRowLength));
                }

                // Columns
                for (int iCol = 0; iCol <= m_iDivisions; iCol++)
                {
                    _canvas.DrawLine(m_PenEdges, m_RescaledSourceCorners[1].X + (iCol * fColLength), m_RescaledSourceCorners[0].Y, m_RescaledSourceCorners[1].X + (iCol * fColLength), m_RescaledSourceCorners[3].Y);    
                }
            }
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
		{
			//-----------------------------------------------------
            // This function is used by the PointerTool 
            // to know if we hit this particular drawing and where.
            //
            // Hit Result:
            // -1: miss, 0: on object, 1+: on handle.
            //
            // _point is mouse coordinates already descaled 
            // (in original image coords).
            //-----------------------------------------------------
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if(fOpacityFactor > 0)
            {
	            // On a corner ?
	            for (int i = 0; i < m_SourceCorners.Length; i++)
	            {
	                if (GetHandleRectangle(i+1).Contains(_point))
	                {
	                    iHitResult = i+1;
	                }
	            }
	
	            // On main grid ?
	            if (iHitResult == -1 && IsPointInObject(_point))
	            {
	                iHitResult = 0;
	            }
            }
            
            return iHitResult;
		}
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
			if ((_ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                // => Remesh.
                m_iDivisions = m_iDivisions + ((_deltaX - _deltaY)/4);
                if (m_iDivisions < m_iMinimumDivisions)
                {
                    m_iDivisions = m_iMinimumDivisions;
                }
                else if (m_iDivisions > m_iMaximumDivisions)
                {
                    m_iDivisions = m_iMaximumDivisions;
                }
            }
            else if ((_ModifierKeys & Keys.Control) == Keys.Control)
            {
                // Grow the grid (on the same plane).
                int Offset = (_deltaX - _deltaY) / 2;
                
                if (m_bSupport3D)
                {
                    if (m_bValidPlane)
                    {
                        // find new corners by growing the current homography.
                        float[] homography = GetHomographyMatrix(m_HomoPlane);
                        float fShift = m_fShift + ((float)(_deltaX - _deltaY) / 200);

                        PointF[] shiftedCorners = new PointF[4];
                        shiftedCorners[0] = ProjectiveMapping(new PointF(-fShift, -fShift), homography);
                        shiftedCorners[1] = ProjectiveMapping(new PointF(1 + fShift, -fShift), homography);
                        shiftedCorners[2] = ProjectiveMapping(new PointF(1 + fShift, 1 + fShift), homography);
                        shiftedCorners[3] = ProjectiveMapping(new PointF(-fShift, 1 + fShift), homography);

                        // Check for minimum surface.
                        int iOldArea = GetQuadrilateralArea(m_RescaledSourceCorners);
                        int iNewArea = GetQuadrilateralArea(shiftedCorners);

                        if ((iOldArea < m_iMinimumSurface && iNewArea > iOldArea) || (iNewArea > m_iMinimumSurface))
                        {
                            // Ok, use those new corners, but do not redefine the homography. 
                            // We'll keep it relative to the original until
                            // the user moves a corner or the whole grid at once.
                            m_fShift = fShift;
                            
                            m_RescaledSourceCorners[0] = new Point((int)shiftedCorners[0].X, (int)shiftedCorners[0].Y);
                            m_RescaledSourceCorners[1] = new Point((int)shiftedCorners[1].X, (int)shiftedCorners[1].Y);
                            m_RescaledSourceCorners[2] = new Point((int)shiftedCorners[2].X, (int)shiftedCorners[2].Y);
                            m_RescaledSourceCorners[3] = new Point((int)shiftedCorners[3].X, (int)shiftedCorners[3].Y);

                            UnscaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                        }
                    }
                }
                else
                {
                    float fGrowFactor = 1 + ((float)Offset / 100); // for offset [-10;+10] => Growth [0.9;1.1]

                    int width = m_RescaledSourceCorners[1].X - m_RescaledSourceCorners[0].X;
                    int height = m_RescaledSourceCorners[3].Y - m_RescaledSourceCorners[0].Y;

                    float fNewWidth; 
                    float fNewHeight;

                    fNewWidth = (float)width * fGrowFactor;
                    fNewHeight = (float)height * fGrowFactor;

                    int shiftx = ((int)fNewWidth - width) / 2;
                    int shifty = ((int)fNewHeight - height) / 2;                   
                    Size shift = new Size(shiftx, shifty);

                    m_RescaledSourceCorners[0] = new Point(m_RescaledSourceCorners[0].X - shift.Width, m_RescaledSourceCorners[0].Y - shift.Height);
                    m_RescaledSourceCorners[1] = new Point(m_RescaledSourceCorners[1].X + shift.Width, m_RescaledSourceCorners[1].Y - shift.Height);
                    m_RescaledSourceCorners[2] = new Point(m_RescaledSourceCorners[2].X + shift.Width, m_RescaledSourceCorners[2].Y + shift.Height);
                    m_RescaledSourceCorners[3] = new Point(m_RescaledSourceCorners[3].X - shift.Width, m_RescaledSourceCorners[3].Y + shift.Height);

                    UnscaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                }

                
            }
            else
            {
                // => Simple Move
                for (int i = 0; i < m_SourceCorners.Length; i++)
                {
                    m_SourceCorners[i] = new Point(m_SourceCorners[i].X + _deltaX, m_SourceCorners[i].Y + _deltaY);
                }
            
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                RedefineHomography();
                m_fShift = 0.0F;
            }
		}
		public override void MoveHandle(Point point, int handleNumber)
		{
			// _point is mouse coordinates already descaled.
            if (m_bSupport3D)
            {
                // If 3D, only move the selected corner.

                // Redefine the homography.
                m_SourceCorners[handleNumber - 1] = point;
                m_RescaledSourceCorners[handleNumber - 1] = RescalePoint(point, m_fStretchFactor, m_DirectZoomTopLeft);

                RedefineHomography();
                m_fShift = 0.0F;

                // Check if it is convex. angles must all be > 180 or < 180.
                double[] iAngles = new double[4];
                iAngles[0] = GetAngle(m_SourceCorners[0], m_SourceCorners[1], m_SourceCorners[2]);
                iAngles[1] = GetAngle(m_SourceCorners[1], m_SourceCorners[2], m_SourceCorners[3]);
                iAngles[2] = GetAngle(m_SourceCorners[2], m_SourceCorners[3], m_SourceCorners[0]);
                iAngles[3] = GetAngle(m_SourceCorners[3], m_SourceCorners[0], m_SourceCorners[1]);

                if ((iAngles[0] > 0 && iAngles[1] > 0 && iAngles[2] > 0 && iAngles[3] > 0) ||
                    (iAngles[0] < 0 && iAngles[1] < 0 && iAngles[2] < 0 && iAngles[3] < 0))
                {
                    m_bValidPlane = true;
                }
                else
                {
                    m_bValidPlane = false;
                }
            }
            else
            {
                // If 2D, move while keeping it rectangle.
                // TODO shift key keeps ratio.
                switch (handleNumber)
                {
                    case 1:
                        m_SourceCorners[0] = point;
                        m_SourceCorners[1] = new Point(m_SourceCorners[1].X, point.Y);
                        m_SourceCorners[3] = new Point(point.X, m_SourceCorners[3].Y);
                        break;
                    case 2:
                        m_SourceCorners[1] = point;
                        m_SourceCorners[0] = new Point(m_SourceCorners[0].X, point.Y);
                        m_SourceCorners[2] = new Point(point.X, m_SourceCorners[2].Y);
                        break;
                    case 3:
                        m_SourceCorners[2] = point;
                        m_SourceCorners[3] = new Point(m_SourceCorners[3].X, point.Y);
                        m_SourceCorners[1] = new Point(point.X, m_SourceCorners[1].Y);
                        break;
                    case 4:
                        m_SourceCorners[3] = point;
                        m_SourceCorners[2] = new Point(m_SourceCorners[2].X, point.Y);
                        m_SourceCorners[0] = new Point(point.X, m_SourceCorners[0].Y);
                        break;
                    default:
                        break;
                }
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                RedefineHomography();
            }
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
                            m_SourceCorners[0] = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				            break;
				        }
				    case "PointUpperRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_SourceCorners[1] = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerRight":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_SourceCorners[2] = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
    				        break;
				        }
				    case "PointLowerLeft":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_SourceCorners[3] = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
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
            
			// TODO: Sanity check for rectangular constraint if m_bSupport3D is false.
			
			RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            RedefineHomography();
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("PointUpperLeft", String.Format("{0};{1}", m_SourceCorners[0].X, m_SourceCorners[0].Y));
		    _xmlWriter.WriteElementString("PointUpperRight", String.Format("{0};{1}", m_SourceCorners[1].X, m_SourceCorners[1].Y));
		    _xmlWriter.WriteElementString("PointLowerRight", String.Format("{0};{1}", m_SourceCorners[2].X, m_SourceCorners[2].Y));
		    _xmlWriter.WriteElementString("PointLowerLeft", String.Format("{0};{1}", m_SourceCorners[3].X, m_SourceCorners[3].Y));
		    
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
		
		public void SetLocations(Size _ImageSize, double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            // Initialize corners positions
            
            m_fStretchFactor = _fStretchFactor;
            m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            RedefineHomography();

            if (!m_bInitialized)
            {
                m_bInitialized = true;

                int horzTenth = (int)(((double)_ImageSize.Width) / 10);
                int vertTenth = (int)(((double)_ImageSize.Height) / 10);

                if (m_bSupport3D)
                {
                    // initialize with a faked perspective.
                    m_SourceCorners[0] = new Point(3 * horzTenth, 4 * vertTenth);
                    m_SourceCorners[1] = new Point(7 * horzTenth, 4 * vertTenth);
                    m_SourceCorners[2] = new Point(9 * horzTenth, 8 * vertTenth);
                    m_SourceCorners[3] = new Point(1 * horzTenth, 8 * vertTenth);
                }
                else
                {
                    // initialize with a rectangle.
                    m_SourceCorners[0] = new Point(2 * horzTenth, 2 * vertTenth);
                    m_SourceCorners[1] = new Point(8 * horzTenth, 2 * vertTenth);
                    m_SourceCorners[2] = new Point(8 * horzTenth, 8 * vertTenth);
                    m_SourceCorners[3] = new Point(2 * horzTenth, 8 * vertTenth);
                }
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                RedefineHomography();
                m_fShift = 0.0F;
            }
        }

        private Rectangle GetHandleRectangle(int _iHandleId)
        {
            //----------------------------------------------------------------------------
            // This function is only used for Hit Testing.
            // The Rectangle here is bigger than the bounding box of the handlers circles.
            //----------------------------------------------------------------------------
            int widen = 6;
            int x = (int)((float)m_SourceCorners[_iHandleId - 1].X - (float)widen);
            int y = (int)((float)m_SourceCorners[_iHandleId - 1].Y - (float)widen);

            return new Rectangle(x, y, widen * 2, widen*2);
        }
        private Rectangle GetRescaledHandleRectangle(int _iHandleId)
        {
            // Only used for drawing handlers.
            int x = (int)((float)m_RescaledSourceCorners[_iHandleId - 1].X - (float)4);
            int y = (int)((float)m_RescaledSourceCorners[_iHandleId - 1].Y - (float)4);
 
            return new Rectangle(x, y, 8, 8);
        }
        private bool IsPointInObject(Point _point)
        {
            bool bIsPointInObject = false;
            if (m_bValidPlane)
            {
                GraphicsPath areaPath = new GraphicsPath();
                areaPath.AddLine(m_SourceCorners[0], m_SourceCorners[1]);
                areaPath.AddLine(m_SourceCorners[1], m_SourceCorners[2]);
                areaPath.AddLine(m_SourceCorners[2], m_SourceCorners[3]);
                areaPath.CloseAllFigures();

                // Create region from the path
                Region areaRegion = new Region(areaPath);

                // point is descaled.
                bIsPointInObject = areaRegion.IsVisible(_point);
            }
            
            return bIsPointInObject;
        }
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
        }
            
        
        #region Scaling
        private Point RescalePoint(Point _point, double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            return new Point((int)((double)(_point.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(_point.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            for (int i = 0; i < m_RescaledSourceCorners.Length; i++)
            {
                m_RescaledSourceCorners[i] = new Point((int)((double)(m_SourceCorners[i].X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_SourceCorners[i].Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            }
        }
        private void UnscaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            for (int i = 0; i < m_SourceCorners.Length; i++)
            {
                m_SourceCorners[i] = new Point((int)((double)(m_RescaledSourceCorners[i].X + _DirectZoomTopLeft.X) / _fStretchFactor), (int)((double)(m_RescaledSourceCorners[i].Y + _DirectZoomTopLeft.Y) / _fStretchFactor));
            }
        }
        private void RedefineHomography()
        {
            for (int i = 0; i < m_RescaledSourceCorners.Length; i++)
            {
                m_HomoPlane[i] = m_RescaledSourceCorners[i];
            }
        }
        #endregion

        #region Geometry Routines
        private double GetAngle(Point A, Point B, Point C)
        {
            // Compute the angle ABC.
            // using scalar and vector product between vectors BA and BC.

            double bax = ((double)A.X - (double)B.X);
            double bcx = ((double)C.X - (double)B.X);
            double scalX =  bax * bcx;

            double bay = ((double)A.Y - (double)B.Y);
            double bcy = ((double)C.Y - (double)B.Y);
            double scalY = bay * bcy;
            
            double scal = scalX + scalY;
            
            double normab = Math.Sqrt(bax * bax + bay * bay);
            double normbc = Math.Sqrt(bcx * bcx + bcy * bcy);
            double norm = normab * normbc;

            double angle = Math.Acos((double)(scal / norm));

            if ((bax * bcy - bay * bcx) < 0)
            {
                angle = -angle;
            }

            double deg = (angle * 180) / Math.PI;
            return deg;
        }
        private int GetQuadrilateralArea(Point[] _corners)
        {
            PointF[] floatCorners = new PointF[_corners.Length];
            
            for(int i=0;i<_corners.Length;i++)
            {
                floatCorners[i] = new PointF((float)_corners[i].X, (float)_corners[i].Y);
            }
            return GetQuadrilateralArea(floatCorners);
        }
        private int GetQuadrilateralArea(PointF[] _corners)
        {
            int area1 = GetTriangleArea(_corners[0], _corners[1], _corners[2]);
            int area2 = GetTriangleArea(_corners[0], _corners[2], _corners[3]);
            
            return area1 + area2;
        }
        private int GetTriangleArea(PointF A, PointF B, PointF C)
        {
            double bax = ((double)A.X - (double)B.X);
            double bcx = ((double)C.X - (double)B.X);
            double bay = ((double)A.Y - (double)B.Y);
            double bcy = ((double)C.Y - (double)B.Y);
            double acx = ((double)C.X - (double)A.X);
            double acy = ((double)C.Y - (double)A.Y);

            double normab = Math.Sqrt(bax * bax + bay * bay);
            double normbc = Math.Sqrt(bcx * bcx + bcy * bcy);
            double normac = Math.Sqrt(acx * acx + acy * acy);

            double semiperimeter = (normab + normbc + normac) / 2;

            double area = Math.Sqrt(semiperimeter * (semiperimeter - normab) * (semiperimeter - normbc) * (semiperimeter - normac));
            return (int)area;
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
