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
    [XmlType ("CrossMark")]
    public class DrawingCross2D : AbstractDrawing, IKvaSerializable, IDecorable
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
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get 
			{ 
				// Rebuild the menu to get the localized text.
				List<ToolStripMenuItem> contextMenu = new List<ToolStripMenuItem>();
        		
				mnuShowCoordinates.Text = ScreenManagerLang.mnuShowCoordinates;
				mnuShowCoordinates.Checked = m_bShowCoordinates;
        		
        		contextMenu.Add(mnuShowCoordinates);
        		
				return contextMenu; 
			}
		}
        public bool ShowCoordinates
		{
			get { return m_bShowCoordinates; }
			set { m_bShowCoordinates = value; }
		}
        public Metadata ParentMetadata
        {
            // get => unused.
            set { m_ParentMetadata = value; }
        }
        
        // Next 2 props are accessed from Track creation.
        public Point CenterPoint 
		{
			get { return m_CenterPoint; }
		}
        public Color PenColor
        {
        	get { return m_StyleHelper.Color; }
        }
        #endregion

        #region Members
		// Position
        private Point m_CenterPoint;           
		private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
		private KeyframeLabel m_LabelCoordinates;
        private bool m_bShowCoordinates;
        private Metadata m_ParentMetadata;
        private static readonly int m_iDefaultBackgroundAlpha = 64;
        private static readonly int m_iDefaultRadius = 3;

        // Computed
        private Point RescaledCenterPoint;
        
        // Context menu
        private ToolStripMenuItem mnuShowCoordinates = new ToolStripMenuItem();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCross2D(int x, int y, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            // Position
            m_CenterPoint = new Point(x, y);
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            // Decoration & binding with editors
            m_StyleHelper.Color = Color.CornflowerBlue;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
                        
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
            m_LabelCoordinates = new KeyframeLabel(m_CenterPoint, Color.Black);
            
            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            
            // Context menu
            mnuShowCoordinates.Click += new EventHandler(mnuShowCoordinates_Click);
			mnuShowCoordinates.Image = Properties.Drawings.measure;
        }
        public DrawingCross2D(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(0,0,0,0, ToolManager.CrossMark.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
            m_ParentMetadata = _parent;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            int iAlpha = (int)((double)255 * fOpacityFactor);

            if (iAlpha > 0)
            {
                // Rescale the points.
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                // Cross
                Pen PenEdges = m_StyleHelper.GetPen(iAlpha);
                _canvas.DrawLine(PenEdges, RescaledCenterPoint.X - m_iDefaultRadius, RescaledCenterPoint.Y, RescaledCenterPoint.X + m_iDefaultRadius, RescaledCenterPoint.Y);
                _canvas.DrawLine(PenEdges, RescaledCenterPoint.X, RescaledCenterPoint.Y - m_iDefaultRadius, RescaledCenterPoint.X, RescaledCenterPoint.Y + m_iDefaultRadius);

                // Background
                SolidBrush tempBrush = m_StyleHelper.GetBrush((int)((double)m_iDefaultBackgroundAlpha * fOpacityFactor));
                
                _canvas.FillEllipse(tempBrush, RescaledCenterPoint.X - m_iDefaultRadius - 1, RescaledCenterPoint.Y - m_iDefaultRadius - 1, (m_iDefaultRadius * 2) + 2, (m_iDefaultRadius * 2) + 2);
                tempBrush.Dispose();
                PenEdges.Dispose();
                
                if(m_bShowCoordinates)
                {
                	m_LabelCoordinates.Text = m_ParentMetadata.CalibrationHelper.GetPointText(m_CenterPoint, true);
	                m_LabelCoordinates.Draw(_canvas, _fStretchFactor, _DirectZoomTopLeft, fOpacityFactor);
                }
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // This is only implemented for the coordinates mini label.
            if(handleNumber == 1)
            {
		        m_LabelCoordinates.MoveLabel(point);
            }
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // _delatX and _delatY are mouse delta already descaled.
            m_CenterPoint.X += _deltaX;
            m_CenterPoint.Y += _deltaY;
            
            m_LabelCoordinates.MoveTo(m_CenterPoint);

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // _point is mouse coordinates already descaled.
            // Hit Result: -1: miss, 0: on object.
            
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	if(m_bShowCoordinates && m_LabelCoordinates.HitTest(_point))
            	{
            		iHitResult = 1;
            	}
            	else if (IsPointInObject(_point))
                {
                    iHitResult = 0;
                }
            }
            
            return iHitResult;
        }
        #endregion
        
		#region Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "CenterPoint":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_CenterPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
				        break;
					case "CoordinatesVisible":
				        m_bShowCoordinates = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
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
            
			m_LabelCoordinates.MoveTo(CenterPoint);
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("CenterPoint", String.Format("{0};{1}", m_CenterPoint.X, m_CenterPoint.Y));
            _xmlWriter.WriteElementString("CoordinatesVisible", m_bShowCoordinates ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement(); 
            
            if(m_bShowCoordinates)
            {
            	// Spreadsheet support.
            	_xmlWriter.WriteStartElement("Coordinates");
            	
            	PointF coords = m_ParentMetadata.CalibrationHelper.GetPointInUserUnit(m_CenterPoint);
	            _xmlWriter.WriteAttributeString("UserX", String.Format("{0:0.00}", coords.X));
	            _xmlWriter.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.X));
	            _xmlWriter.WriteAttributeString("UserY", String.Format("{0:0.00}", coords.Y));
	            _xmlWriter.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.Y));
	            _xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.CalibrationHelper.GetLengthAbbreviation());
            	
            	_xmlWriter.WriteEndElement();
            }
		}
        #endregion
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return ScreenManagerLang.ToolTip_DrawingToolCross2D;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_CenterPoint.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }

        #region Context menu
        private void mnuShowCoordinates_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the coordinates for this cross marker.
			m_bShowCoordinates = !m_bShowCoordinates;
			
			// Use this setting as the default value for new lines.
			DrawingToolCross2D.ShowCoordinates = m_bShowCoordinates;
			
			CallInvalidateFromMenu(sender);
		}
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "back color");
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            RescaledCenterPoint = new Point((int)((double)(m_CenterPoint.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_CenterPoint.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private bool IsPointInObject(Point _point)
        {
            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            Pen areaPen = new Pen(Color.Black, 7);

            areaPath.AddLine(m_CenterPoint.X - m_iDefaultRadius, m_CenterPoint.Y, m_CenterPoint.X + m_iDefaultRadius, m_CenterPoint.Y);
            areaPath.Widen(areaPen);
            Region areaRegion = new Region(areaPath);
            areaPen.Dispose();
            
            return areaRegion.IsVisible(_point);
        }
        #endregion

    }
}
