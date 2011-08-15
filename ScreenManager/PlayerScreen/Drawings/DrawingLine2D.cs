#region License
/*
Copyright © Joan Charmant 2008-2011.
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
using System.ComponentModel;
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
    [XmlType ("Line")]
    public class DrawingLine2D : AbstractDrawing, IKvaSerializable, IDecorable, IInitializable
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
			get { return DrawingCapabilities.ConfigureColorSize | DrawingCapabilities.Fading; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get 
			{
        		// Rebuild the menu to get the localized text.
				List<ToolStripMenuItem> contextMenu = new List<ToolStripMenuItem>();
        		
				mnuShowMeasure.Text = ScreenManagerLang.mnuShowMeasure;
				mnuShowMeasure.Checked = m_bShowMeasure;
        		mnuSealMeasure.Text = ScreenManagerLang.mnuSealMeasure;
        		
        		contextMenu.Add(mnuShowMeasure);
        		contextMenu.Add(mnuSealMeasure);
        		
				return contextMenu; 
			}
		}
        public Metadata ParentMetadata
        {
            // get => unused.
            set { m_ParentMetadata = value; }
        }
        public bool ShowMeasure
        {
        	get { return m_bShowMeasure;}
        	set { m_bShowMeasure = value;}
        }
        #endregion

        #region Members
        // Core
        public Point m_StartPoint;            	// Public because also used for the Active Screen Bordering...
        public Point m_EndPoint;				// Idem.
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private KeyframeLabel m_LabelMeasure;
        private bool m_bShowMeasure;
        private Metadata m_ParentMetadata;
        private InfosFading m_InfosFading;
        // Context menu
        private ToolStripMenuItem mnuShowMeasure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSealMeasure = new ToolStripMenuItem();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingLine2D(Point _start, Point _end, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            m_StartPoint = _start;
            m_EndPoint = _end;
            m_LabelMeasure = new KeyframeLabel(GetMiddlePoint(), Color.Black);
            
            // Decoration
            m_StyleHelper.Color = Color.DarkSlateGray;
            m_StyleHelper.LineSize = 1;
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            // Fading
            m_InfosFading = new InfosFading(_iTimestamp, _iAverageTimeStampsPerFrame);
            
            // Context menu
            mnuShowMeasure.Click += new EventHandler(mnuShowMeasure_Click);
			mnuShowMeasure.Image = Properties.Drawings.measure;
			mnuSealMeasure.Click += new EventHandler(mnuSealMeasure_Click);
			mnuSealMeasure.Image = Properties.Drawings.linecalibrate;
        }
        public DrawingLine2D(XmlReader _xmlReader, PointF _scale, Metadata _parent)
            : this(Point.Empty, Point.Empty, 0, 0, ToolManager.Line.StylePreset.Clone())
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
            
            Point start = _transformer.Transform(m_StartPoint);
            Point end = _transformer.Transform(m_EndPoint);
            
            using(Pen penEdges = m_StyleHelper.GetPen((int)(fOpacityFactor * 255), _transformer.Scale))
            {
                _canvas.DrawLine(penEdges, start, end);
                
                // Handlers
                penEdges.Width = _bSelected ? 2 : 1;
                if(m_StyleHelper.LineEnding.StartCap != LineCap.ArrowAnchor)
                    _canvas.DrawEllipse(penEdges, start.Box(3));
                
                if(m_StyleHelper.LineEnding.EndCap != LineCap.ArrowAnchor)
                    _canvas.DrawEllipse(penEdges, end.Box(3));
            }

            if(m_bShowMeasure)
            {
            	// Text of the measure. (The helpers knows the unit)
            	m_LabelMeasure.SetText(m_ParentMetadata.CalibrationHelper.GetLengthText(m_StartPoint, m_EndPoint));
                m_LabelMeasure.Draw(_canvas, _transformer, fOpacityFactor);
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            switch(handleNumber)
            {
            	case 1:
            		m_StartPoint = point;
                    m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
            		break;
            	case 2:
            		m_EndPoint = point;
                    m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
            		break;
            	case 3:
            		// Move the center of the mini label to the mouse coord.
            		m_LabelMeasure.SetLabel(point);
            		break;
            }
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_StartPoint.X += _deltaX;
            m_StartPoint.Y += _deltaY;

            m_EndPoint.X += _deltaX;
            m_EndPoint.Y += _deltaY;

            m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            int iHitResult = -1;
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            if (fOpacityFactor > 0)
            {
            	if(m_bShowMeasure && m_LabelMeasure.HitTest(_point))
            		iHitResult = 3;
            	else if (m_StartPoint.Box(6).Contains(_point))
                    iHitResult = 1;
            	else if (m_EndPoint.Box(6).Contains(_point))
                    iHitResult = 2;
                else if (IsPointInObject(_point))
                    iHitResult = 0;
            }
            return iHitResult;
        }
        #endregion

		#region KVA Serialization
        private void ReadXml(XmlReader _xmlReader, PointF _scale)
        {
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Start":
				        {
				            Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
				            m_StartPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                            break;
				        }
					case "End":
				        {
    				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                            m_EndPoint = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                            break;
				        }
                    case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "InfosFading":
						m_InfosFading.ReadXml(_xmlReader);
						break;
				    case "MeasureVisible":
				        m_bShowMeasure = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
				        break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
			m_LabelMeasure.SetAttach(GetMiddlePoint(), true);
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
            _xmlWriter.WriteElementString("Start", String.Format("{0};{1}", m_StartPoint.X, m_StartPoint.Y));
            _xmlWriter.WriteElementString("End", String.Format("{0};{1}", m_EndPoint.X, m_EndPoint.Y));
            _xmlWriter.WriteElementString("MeasureVisible", m_bShowMeasure ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();  

            if(m_bShowMeasure)
            {
            	// Spreadsheet support.
            	_xmlWriter.WriteStartElement("Measure");
            	
            	double len = m_ParentMetadata.CalibrationHelper.GetLengthInUserUnit(m_StartPoint, m_EndPoint);
	            string value = String.Format("{0:0.00}", len);
	            string valueInvariant = String.Format(CultureInfo.InvariantCulture, "{0:0.00}", len);

            	_xmlWriter.WriteAttributeString("UserLength", value);
            	_xmlWriter.WriteAttributeString("UserLengthInvariant", valueInvariant);
            	_xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.CalibrationHelper.GetLengthAbbreviation());
            	
            	_xmlWriter.WriteEndElement();
            }
		}
        #endregion
        
        #region IInitializable implementation
        public void ContinueSetup(Point point)
		{
			MoveHandle(point, 2);
		}
        #endregion
        
        public override string ToString()
        {
            return ScreenManagerLang.ToolTip_DrawingToolLine2D;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_StartPoint.GetHashCode();
            iHash ^= m_EndPoint.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();

            return iHash;
        }
        
        #region Context menu
        private void mnuShowMeasure_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the measure for this line.
			m_bShowMeasure = !m_bShowMeasure;
			
			// Use this setting as the default value for new lines.
			DrawingToolLine2D.ShowMeasure = m_bShowMeasure;
			
			CallInvalidateFromMenu(sender);
		}
        private void mnuSealMeasure_Click(object sender, EventArgs e)
		{
			// display a dialog that let the user specify how many real-world-units long is this line.
			
			if(m_StartPoint.X != m_EndPoint.X || m_StartPoint.Y != m_EndPoint.Y)
			{
				if(!m_bShowMeasure)
					m_bShowMeasure = true;
				
				DrawingToolLine2D.ShowMeasure = true;
				
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DeactivateKeyboardHandler != null)
					dp.DeactivateKeyboardHandler();
	
				formConfigureMeasure fcm = new formConfigureMeasure(m_ParentMetadata, this);
				ScreenManagerKernel.LocateForm(fcm);
				fcm.ShowDialog();
				fcm.Dispose();
				
				// Update traj for distance and speed after calibration.
				m_ParentMetadata.UpdateTrajectoriesForKeyframes();
				
				CallInvalidateFromMenu(sender);
				
				if (dp.ActivateKeyboardHandler != null)
					dp.ActivateKeyboardHandler();
			}
		}
        #endregion
        
        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "line size");
            m_Style.Bind(m_StyleHelper, "LineEnding", "arrows");
        }
        private bool IsPointInObject(Point _point)
        {
            // Create path which contains wide line for easy mouse selection
            GraphicsPath areaPath = new GraphicsPath();
            
            if(m_StartPoint == m_EndPoint)
                areaPath.AddLine(m_StartPoint.X, m_StartPoint.Y, m_StartPoint.X + 2, m_StartPoint.Y + 2);
            else
            	areaPath.AddLine(m_StartPoint, m_EndPoint);
            
            Pen areaPen = new Pen(Color.Black, 7);
            areaPath.Widen(areaPen);
            areaPen.Dispose();
            Region areaRegion = new Region(areaPath);
            return areaRegion.IsVisible(_point);
        }
        private Point GetMiddlePoint()
        {
        	return new Point((m_StartPoint.X + m_EndPoint.X)/2, (m_StartPoint.Y + m_EndPoint.Y)/2);
        }
        #endregion
    }
}
