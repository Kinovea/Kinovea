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
    [XmlType ("Angle")]
    public class DrawingAngle2D : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
    {
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading infosFading
        {
            get{ return m_InfosFading;}
            set{ m_InfosFading = value;}
        }
		public override Capabilities Caps
		{
			get { return Capabilities.ConfigureColor | Capabilities.Fading; }
		}
		public override List<ToolStripMenuItem> ContextMenu
		{
			get 
			{
				// Rebuild the menu to get the localized text.
				List<ToolStripMenuItem> contextMenu = new List<ToolStripMenuItem>();
        		
				m_mnuInvertAngle.Text = ScreenManagerLang.mnuInvertAngle;
        		contextMenu.Add(m_mnuInvertAngle);
        		
				return contextMenu; 
			}
		}
        #endregion

        #region Members
        // Core
        private Point m_PointO;
        private Point m_PointA;
        private Point m_PointB;
        private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;

        // Decoration
        private DrawingStyle m_Style;
        private StyleHelper m_StyleHelper = new StyleHelper();
        private LabelBackground m_LabelBackground = new LabelBackground();
        
        private static readonly int m_iDefaultBackgroundAlpha = 92;
        private static readonly double m_fLabelDistance = 40.0;
        
        // Fading
        private InfosFading m_InfosFading;
        
        // Computed
        private Point m_RescaledPointO;
        private Point m_RescaledPointA;
        private Point m_RescaledPointB;
        private Point m_BoundingPoint;
        private double m_fRadius;
        private float m_fStartAngle;
        private float m_fSweepAngle;	// This is the actual value of the angle.
        
        // Context menu
        private ToolStripMenuItem m_mnuInvertAngle = new ToolStripMenuItem();
		
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DrawingAngle2D(int Ox, int Oy, int Ax, int Ay, int Bx, int By, long _iTimestamp, 
                              long _iAverageTimeStampsPerFrame, DrawingStyle _stylePreset)
        {
            // Core
            m_PointO = new Point(Ox, Oy);
            m_PointA = new Point(Ax, Ay);
            m_PointB = new Point(Bx, By);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);

            // Decoration and binding to mini editors.
            m_StyleHelper.Bicolor = new Bicolor(Color.Empty);
            m_StyleHelper.Font = new Font("Arial", 12, FontStyle.Bold);
            
            if(_stylePreset != null)
            {
                m_Style = _stylePreset.Clone();
                BindStyle();    
            }
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);

            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            m_BoundingPoint = new Point(0, 0);
            
            // Context menu
            m_mnuInvertAngle.Click += new EventHandler(mnuInvertAngle_Click);
			m_mnuInvertAngle.Image = Properties.Drawings.angleinvert;
        }
        public DrawingAngle2D(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(0,0,0,0,0,0,0,0, null)
        {
            ReadXml(_xmlReader, _scale);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);

            if (fOpacityFactor > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
           
                //----------------------------------------------------------
                // Draw disk section
                // Unfortunately we need to compute everything at each draw
                // (draw may be triggered on image resize)
                //----------------------------------------------------------            
                ComputeFillRegion();

                Pen penEdges = m_StyleHelper.GetBackgroundPen((int)(fOpacityFactor*255));
                SolidBrush brushEdges = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*255));
                SolidBrush brushFill = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor*m_iDefaultBackgroundAlpha));
                
                _canvas.FillPie(brushFill, (float)m_BoundingPoint.X, (float)m_BoundingPoint.Y, (float)m_fRadius * 2, (float)m_fRadius * 2, m_fStartAngle, m_fSweepAngle);
                _canvas.DrawPie(penEdges, (float)m_BoundingPoint.X, (float)m_BoundingPoint.Y, (float)m_fRadius * 2, (float)m_fRadius * 2, m_fStartAngle, m_fSweepAngle);

                //-----------------------------
                // Draw the edges 
                //-----------------------------
                _canvas.DrawLine(penEdges, m_RescaledPointO.X, m_RescaledPointO.Y, m_RescaledPointA.X, m_RescaledPointA.Y);
                _canvas.DrawLine(penEdges, m_RescaledPointO.X, m_RescaledPointO.Y, m_RescaledPointB.X, m_RescaledPointB.Y);

                //-----------------------------
                // Draw handlers
                //-----------------------------
                _canvas.DrawEllipse(penEdges, GetRescaledHandleRectangle(1));
                _canvas.FillEllipse(brushEdges, GetRescaledHandleRectangle(2));
                _canvas.FillEllipse(brushEdges, GetRescaledHandleRectangle(3));
				
                brushFill.Dispose();
                penEdges.Dispose();
                brushEdges.Dispose();
                
                //----------------------------
                // Draw Measure
                //----------------------------

                // We try to be inside the pie, so we compute the bissectrice and do some trigo.
                // We start the text on the bissectrice, at a distance of iTextRadius.
                SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(fOpacityFactor * 255));
                
                int angle = (int)Math.Floor(-m_fSweepAngle);
				string label = angle.ToString() + "°";
                Font tempFont = m_StyleHelper.GetFont((float)m_fStretchFactor);
				SizeF labelSize = _canvas.MeasureString(label, tempFont);
                Point TextOrigin = GetTextPosition(labelSize);
                
                // Background
                m_LabelBackground.Location = TextOrigin;
            	int radius = (int)(tempFont.Size / 2);
            	m_LabelBackground.Draw(_canvas, fOpacityFactor, radius, (int)labelSize.Width, (int)labelSize.Height, m_StyleHelper.Bicolor.Background);
        
                // Text
				_canvas.DrawString(label, tempFont, fontBrush, m_LabelBackground.TextLocation);
                
                tempFont.Dispose();
                fontBrush.Dispose();
            }
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            //-----------------------------------------------------
            // This function is used by the PointerTool 
            // to know if we hit this particular drawing and where.
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object, 1+: on handle.
            //-----------------------------------------------------
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
                if (GetHandleRectangle(1).Contains(_point))
                {
                    iHitResult = 1;
                }
                else if (GetHandleRectangle(2).Contains(_point))
                {
                    iHitResult = 2;
                }
                else if (GetHandleRectangle(3).Contains(_point))
                {
                    iHitResult = 3;
                }
                else
                {
                    if (IsPointInObject(_point))
                    {
                        iHitResult = 0;
                    }
                }
            }
            return iHitResult;
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // Move the specified handle to the specified coordinates.
            // In Angle2D, handles are directly mapped to the endpoints of the lines.
            // _point is mouse coordinates already descaled.
            switch (handleNumber)
            {
                case 1:
                    m_PointO = point;
                    break;
                case 2:
                    m_PointA = point;
                    break;
                case 3:
                    m_PointB = point;
                    break;
                default:
                    break;
            }
            
            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_PointO.X += _deltaX;
            m_PointO.Y += _deltaY;

            m_PointA.X += _deltaX;
            m_PointA.Y += _deltaY;

            m_PointB.X += _deltaX;
            m_PointB.Y += _deltaY;

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
		#endregion
		
        public override string ToString()
        {
            return ScreenManagerLang.ToolTip_DrawingToolAngle2D;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_PointO.GetHashCode();
            iHash ^= m_PointA.GetHashCode();
            iHash ^= m_PointB.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }        
            
        #region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "PointO":
				        m_PointO = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        break;
					case "PointA":
				        m_PointA = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        break;
					case "PointB":
				        m_PointB = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
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
            
			// We only scale the position (PointO), not the size of the edges, 
            // because changing the size of the edges will change angle value.
            Point ShiftOA = new Point(m_PointA.X - m_PointO.X, m_PointA.Y - m_PointO.Y);
            Point ShiftOB = new Point(m_PointB.X - m_PointO.X, m_PointB.Y - m_PointO.Y);

            m_PointO = new Point((int)((float)m_PointO.X * _scale.X), (int)((float)m_PointO.Y * _scale.Y));
            m_PointA = new Point(m_PointO.X + ShiftOA.X, m_PointO.Y + ShiftOA.Y);
            m_PointB = new Point(m_PointO.X + ShiftOB.X, m_PointO.Y + ShiftOB.Y);

            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public void WriteXml(XmlWriter _xmlWriter)
		{
            _xmlWriter.WriteElementString("PointO", String.Format("{0};{1}", m_PointO.X, m_PointO.Y));
            _xmlWriter.WriteElementString("PointA", String.Format("{0};{1}", m_PointA.X, m_PointA.Y));
            _xmlWriter.WriteElementString("PointB", String.Format("{0};{1}", m_PointB.X, m_PointB.Y));
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            // Spreadsheet support.
        	_xmlWriter.WriteStartElement("Measure");        	
        	int angle = (int)Math.Floor(-m_fSweepAngle);        	
        	_xmlWriter.WriteAttributeString("UserAngle", angle.ToString());
        	_xmlWriter.WriteEndElement();
		}
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			MoveHandle(point, 2);
		}
        #endregion
        
        #region Specific context menu
        private void mnuInvertAngle_Click(object sender, EventArgs e)
		{
        	Point temp = m_PointA;
        	m_PointA = m_PointB;
        	m_PointB = temp;
        	RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        	
        	CallInvalidateFromMenu(sender);
		}
        public void InvertAngle()
        {
        	Point temp = m_PointA;
        	m_PointA = m_PointB;
        	m_PointB = temp;
        	RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Bicolor", "line color");
        }
        private void ComputeFillRegion()
        {

            // 2.1. Compute Radius (Smallest segment get to be the radius)
            double OALength = Math.Sqrt(((m_RescaledPointA.X - m_RescaledPointO.X) * (m_RescaledPointA.X - m_RescaledPointO.X)) + ((m_RescaledPointA.Y - m_RescaledPointO.Y) * (m_RescaledPointA.Y - m_RescaledPointO.Y)));
            double OBLength = Math.Sqrt(((m_RescaledPointB.X - m_RescaledPointO.X) * (m_RescaledPointB.X - m_RescaledPointO.X)) + ((m_RescaledPointB.Y - m_RescaledPointO.Y) * (m_RescaledPointB.Y - m_RescaledPointO.Y)));

            if(OALength == 0 || OBLength == 0)
            {
            	if(OALength == 0)
	            {
	            	m_PointA.X = m_PointO.X + 50;
	                m_PointA.Y = m_PointO.Y;
	            }
	            
            	if(OBLength == 0)
	            {
	                m_PointB.X = m_PointO.X;
	                m_PointB.Y = m_PointO.Y - 50;
            	}
	            	
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                OALength = Math.Sqrt(((m_RescaledPointA.X - m_RescaledPointO.X) * (m_RescaledPointA.X - m_RescaledPointO.X)) + ((m_RescaledPointA.Y - m_RescaledPointO.Y) * (m_RescaledPointA.Y - m_RescaledPointO.Y)));
                OBLength = Math.Sqrt(((m_RescaledPointB.X - m_RescaledPointO.X) * (m_RescaledPointB.X - m_RescaledPointO.X)) + ((m_RescaledPointB.Y - m_RescaledPointO.Y) * (m_RescaledPointB.Y - m_RescaledPointO.Y)));        	       
            }

            m_fRadius = Math.Min(OALength, OBLength);
            if(m_fRadius > 20) m_fRadius -= 10;

            // 2.2. Bounding box top/left
            m_BoundingPoint.X = m_RescaledPointO.X - (int)m_fRadius;
            m_BoundingPoint.Y = m_RescaledPointO.Y - (int)m_fRadius;

            // 2.3. Start and stop angles
            double fOARadians = Math.Atan((double)(m_RescaledPointA.Y - m_RescaledPointO.Y) / (double)(m_RescaledPointA.X - m_RescaledPointO.X));
            double fOBRadians = Math.Atan((double)(m_RescaledPointB.Y - m_RescaledPointO.Y) / (double)(m_RescaledPointB.X - m_RescaledPointO.X));

            double iOADegrees;
            if (m_PointA.X < m_PointO.X)
            {
                // angle obtu (entre 0° et OA)
                iOADegrees = (fOARadians * (180 / Math.PI)) - 180;
            }
            else
            {
                // angle aigu
                iOADegrees = fOARadians * (180 / Math.PI);
            }

            double iOBDegrees;
            if (m_PointB.X < m_PointO.X)
            {
                // Angle obtu
                iOBDegrees = (fOBRadians * (180 / Math.PI)) - 180;
            }
            else
            {
                // angle aigu
                iOBDegrees = fOBRadians * (180 / Math.PI);
            }

            // Always go direct orientation. The sweep always go from OA to OB, and is always negative.
            m_fStartAngle = (float)iOADegrees;
            if (iOADegrees > iOBDegrees)
            {
                m_fSweepAngle = -((float)iOADegrees - (float)iOBDegrees);
            }
            else
            {
                m_fSweepAngle = -((float)360.0 - ((float)iOBDegrees - (float)iOADegrees));
            }
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_RescaledPointO = new Point((int)((double)(m_PointO.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_PointO.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            m_RescaledPointA = new Point((int)((double)(m_PointA.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_PointA.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            m_RescaledPointB = new Point((int)((double)(m_PointB.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_PointB.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private Rectangle GetHandleRectangle(int _handle)
        {
            //----------------------------------------------------------------------------
            // This function is only used for Hit Testing.
            // The Rectangle here is bigger than the bounding box of the handlers circles.
            //----------------------------------------------------------------------------
            Rectangle handle;
            int widen = 10;

            switch (_handle)
            {
                case 1:
                    handle = new Rectangle(m_PointO.X - widen, m_PointO.Y - widen, widen * 2, widen*2);
                    break;
                case 2:
                    handle = new Rectangle(m_PointA.X - widen, m_PointA.Y - widen, widen * 2, widen * 2);
                    break;
                case 3:
                    handle = new Rectangle(m_PointB.X - widen, m_PointB.Y - widen, widen * 2, widen * 2);
                    break;
                default:
                    handle = new Rectangle(m_PointO.X - widen, m_PointO.Y - widen, widen * 2, widen * 2);
                    break;
            }

            return handle;
        }
        private Rectangle GetRescaledHandleRectangle(int _handle)
        {
        	// This function is only used for drawing.
            Rectangle handle;

            switch (_handle)
            {
                case 1:
                    handle = new Rectangle(m_RescaledPointO.X - 3, m_RescaledPointO.Y - 3, 6, 6);
                    break;
                case 2:
                    handle = new Rectangle(m_RescaledPointA.X - 3, m_RescaledPointA.Y - 3, 6, 6);
                    break;
                case 3:
                    handle = new Rectangle(m_RescaledPointB.X - 3, m_RescaledPointB.Y - 3, 6, 6);
                    break;
                default:
                    handle = new Rectangle(m_RescaledPointO.X - 3, m_RescaledPointO.Y - 3, 6, 6);
                    break;
            }

            return handle;
        }
        private bool IsPointInObject(Point _point)
        {
            // _point is already descaled.

            bool bIsPointInObject = false;
            if (m_fRadius > 0)
            {
                GraphicsPath areaPath = new GraphicsPath();
                
                double OALength = Math.Sqrt(((m_PointA.X - m_PointO.X) * (m_PointA.X - m_PointO.X)) + ((m_PointA.Y - m_PointO.Y) * (m_PointA.Y - m_PointO.Y)));
                double OBLength = Math.Sqrt(((m_PointB.X - m_PointO.X) * (m_PointB.X - m_PointO.X)) + ((m_PointB.Y - m_PointO.Y) * (m_PointB.Y - m_PointO.Y)));
                double iRadius = Math.Min(OALength, OBLength);
                Point unscaledBoundingPoint = new Point(m_PointO.X - (int)iRadius, m_PointO.Y - (int)iRadius);

                areaPath.AddPie(unscaledBoundingPoint.X, unscaledBoundingPoint.Y, (float)iRadius * 2, (float)iRadius * 2, m_fStartAngle, m_fSweepAngle);

                // Create region from the path
                Region areaRegion = new Region(areaPath);
                bIsPointInObject = new Region(areaPath).IsVisible(_point);
            }

            return bIsPointInObject;
        }
        private Point GetTextPosition(SizeF _labelSize)
        {
            // return a point at which the text should start.

            // Get bissect angle in degrees
            float iBissect = m_fStartAngle + (m_fSweepAngle / 2);
            if (iBissect < 0)
            {
                iBissect += 360;
            }

            double fRadiansBissect = (Math.PI / 180) * iBissect;
            double fSin = Math.Sin((double)fRadiansBissect);
            double fCos = Math.Cos((double)fRadiansBissect);
            
            double fOpposed = fSin * m_fLabelDistance;
            double fAdjacent = fCos * m_fLabelDistance;

            Point TextOrigin = new Point(m_RescaledPointO.X, m_RescaledPointO.Y);
            TextOrigin.X = TextOrigin.X + (int)fAdjacent - (int)(_labelSize.Width/2);
            TextOrigin.Y = TextOrigin.Y + (int)fOpposed - (int)(_labelSize.Height/2);
			
			return TextOrigin;
        }
        #endregion
    }

       
}