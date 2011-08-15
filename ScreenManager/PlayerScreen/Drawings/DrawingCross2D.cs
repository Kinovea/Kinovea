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
        public Point Center 
		{
			get { return m_Center; }
		}
        public Color PenColor
        {
        	get { return m_StyleHelper.Color; }
        }
        #endregion

        #region Members
		// Core
        private Point m_Center;           
		private KeyframeLabel m_LabelCoordinates;
		private bool m_bShowCoordinates;
		// Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
		
        // Context menu
        private ToolStripMenuItem mnuShowCoordinates = new ToolStripMenuItem();
        
        private Metadata m_ParentMetadata;
        private const int m_iDefaultBackgroundAlpha = 64;
        private const int m_iDefaultRadius = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCross2D(Point _center, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_Center = _center;
            m_LabelCoordinates = new KeyframeLabel(m_Center, Color.Black);
            
            // Decoration & binding with editors
            m_StyleHelper.Color = Color.CornflowerBlue;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
                        
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
            // Context menu
            mnuShowCoordinates.Click += new EventHandler(mnuShowCoordinates_Click);
			mnuShowCoordinates.Image = Properties.Drawings.measure;
        }
        public DrawingCross2D(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty,0,0, ToolManager.CrossMark.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale);
            m_ParentMetadata = _parent;
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if(fOpacityFactor <= 0)
                return;
            
            int iAlpha = (int)(fOpacityFactor * 255);
            Point c = _transformer.Transform(m_Center);

            using(Pen p = m_StyleHelper.GetPen(iAlpha))
            using(SolidBrush b = m_StyleHelper.GetBrush((int)(fOpacityFactor * m_iDefaultBackgroundAlpha)))
            {
                _canvas.DrawLine(p, c.X - m_iDefaultRadius, c.Y, c.X + m_iDefaultRadius, c.Y);
                _canvas.DrawLine(p, c.X, c.Y - m_iDefaultRadius, c.X, c.Y + m_iDefaultRadius);
                _canvas.FillEllipse(b, c.Box(m_iDefaultRadius + 1));
            }
            
            if(m_bShowCoordinates)
            {
                m_LabelCoordinates.SetText(m_ParentMetadata.CalibrationHelper.GetPointText(m_Center, true));
                m_LabelCoordinates.Draw(_canvas, _transformer, fOpacityFactor);
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            if(handleNumber == 1)
                m_LabelCoordinates.SetLabel(point);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_Center.X += _deltaX;
            m_Center.Y += _deltaY;
            m_LabelCoordinates.SetAttach(m_Center, true);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	if(m_bShowCoordinates && m_LabelCoordinates.HitTest(_point))
            		iHitResult = 1;
            	else if (m_Center.Box(m_iDefaultRadius + 10).Contains(_point))
                    iHitResult = 0;
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
                        m_Center = new Point((int)(_scale.X * p.X), (int)(_scale.Y * p.Y));
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
			m_LabelCoordinates.SetAttach(m_Center, true);
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("CenterPoint", String.Format("{0};{1}", m_Center.X, m_Center.Y));
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
            	
            	PointF coords = m_ParentMetadata.CalibrationHelper.GetPointInUserUnit(m_Center);
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
            return ScreenManagerLang.ToolTip_DrawingToolCross2D;
        }
        public override int GetHashCode()
        {
            int iHash = m_Center.GetHashCode();
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
        #endregion

    }
}
