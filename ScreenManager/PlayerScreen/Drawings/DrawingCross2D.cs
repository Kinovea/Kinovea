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
    public class DrawingCross2D : AbstractDrawing, IKvaSerializable, IDecorable, ITrackable, IMeasurable
    {
        #region Events
        public event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved; 
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
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColor | DrawingCapabilities.Fading | DrawingCapabilities.Track; }
		}
        public override List<ToolStripItem> ContextMenu
		{
			get 
			{ 
				// Rebuild the menu to get the localized text.
				List<ToolStripItem> contextMenu = new List<ToolStripItem>();
        		
				mnuShowCoordinates.Text = ScreenManagerLang.mnuShowCoordinates;
				mnuShowCoordinates.Checked = ShowMeasurableInfo;
        		
        		contextMenu.Add(mnuShowCoordinates);
        		
				return contextMenu; 
			}
		}
        
        public CalibrationHelper CalibrationHelper { get; set; }
        public bool ShowMeasurableInfo { get; set; }
        #endregion

        #region Members
		private Guid id = Guid.NewGuid();
    	private Dictionary<string, Point> points = new Dictionary<string, Point>();
    	private bool tracking;
    	
		private KeyframeLabel m_LabelCoordinates;
		// Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
		
        // Context menu
        private ToolStripMenuItem mnuShowCoordinates = new ToolStripMenuItem();
        
        private const int m_iDefaultBackgroundAlpha = 64;
        private const int m_iDefaultRadius = 3;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingCross2D(Point _center, long _iTimestamp, long _iAverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            points["0"] = _center;
            m_LabelCoordinates = new KeyframeLabel(points["0"], Color.Black);
            
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
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            double fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            
            if(tracking)
                fOpacityFactor = 1.0;
            
            if(fOpacityFactor <= 0)
                return;
            
            int iAlpha = (int)(fOpacityFactor * 255);
            Point c = _transformer.Transform(points["0"]);

            using(Pen p = m_StyleHelper.GetPen(iAlpha))
            using(SolidBrush b = m_StyleHelper.GetBrush((int)(fOpacityFactor * m_iDefaultBackgroundAlpha)))
            {
                _canvas.DrawLine(p, c.X - m_iDefaultRadius, c.Y, c.X + m_iDefaultRadius, c.Y);
                _canvas.DrawLine(p, c.X, c.Y - m_iDefaultRadius, c.X, c.Y + m_iDefaultRadius);
                _canvas.FillEllipse(b, c.Box(m_iDefaultRadius + 1));
            }
            
            if(ShowMeasurableInfo)
            {
                m_LabelCoordinates.SetText(CalibrationHelper.GetPointText(new PointF(points["0"].X, points["0"].Y), true, true));
                m_LabelCoordinates.Draw(_canvas, _transformer, fOpacityFactor);
            }
        }
        public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
        {
            if(handleNumber == 1)
                m_LabelCoordinates.SetLabel(point);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            points["0"] = new Point(points["0"].X + _deltaX, points["0"].Y + _deltaY);
            SignalTrackablePointMoved();
            m_LabelCoordinates.SetAttach(points["0"], true);
        }
        public override int HitTest(Point point, long currentTimestamp, CoordinateSystem transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            double opacity = m_InfosFading.GetOpacityFactor(currentTimestamp);
            if (tracking || opacity > 0)
            {
                int boxSide = transformer.Untransform(m_iDefaultRadius + 10);
            	
                if(ShowMeasurableInfo && m_LabelCoordinates.HitTest(point, transformer))
            		result = 1;
            	else if (points["0"].Box(boxSide).Contains(point))
                    result = 0;
            }
            
            return result;
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
                        points["0"] = new Point((int)(_scale.X * p.X), (int)(_scale.Y * p.Y));
				        break;
					case "CoordinatesVisible":
				        ShowMeasurableInfo = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
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
			m_LabelCoordinates.SetAttach(points["0"], true);
        }
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("CenterPoint", String.Format("{0};{1}", points["0"].X, points["0"].Y));
            _xmlWriter.WriteElementString("CoordinatesVisible", ShowMeasurableInfo ? "true" : "false");
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("InfosFading");
            m_InfosFading.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement(); 
            
            if(ShowMeasurableInfo)
            {
            	// Spreadsheet support.
            	_xmlWriter.WriteStartElement("Coordinates");
            	
            	PointF p = new PointF(points["0"].X, points["0"].Y);
            	PointF coords = CalibrationHelper.GetPoint(p);
	            _xmlWriter.WriteAttributeString("UserX", String.Format("{0:0.00}", coords.X));
	            _xmlWriter.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.X));
	            _xmlWriter.WriteAttributeString("UserY", String.Format("{0:0.00}", coords.Y));
	            _xmlWriter.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", coords.Y));
	            _xmlWriter.WriteAttributeString("UserUnitLength", CalibrationHelper.GetLengthAbbreviation());
            	
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
            int iHash = points["0"].GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            return iHash;
        }
        
        #region ITrackable implementation and support.
        public Guid ID
        {
            get { return id; }
        }
        public Dictionary<string, Point> GetTrackablePoints()
        {
            return points;
        }
        public void SetTracking(bool tracking)
        {
            this.tracking = tracking;
        }
        public void SetTrackablePointValue(string name, Point value)
        {
            if(!points.ContainsKey(name))
                throw new ArgumentException("This point is not bound.");
            
            points[name] = value;
            m_LabelCoordinates.SetAttach(points["0"], true);
        }
        private void SignalTrackablePointMoved()
        {
            if(TrackablePointMoved == null)
                return;
            
            TrackablePointMoved(this, new TrackablePointMovedEventArgs("0", points["0"]));
        }
        #endregion

        #region Context menu
        private void mnuShowCoordinates_Click(object sender, EventArgs e)
		{
			// Enable / disable the display of the coordinates for this cross marker.
			ShowMeasurableInfo = !ShowMeasurableInfo;
			
			// Use this setting as the default value for new lines.
			if(ShowMeasurableInfoChanged != null)
			    ShowMeasurableInfoChanged(this, EventArgs.Empty);
			
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
