#region License
/*
Copyright © Joan Charmant 2008-2009.
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
    [XmlType ("Chrono")]
    public class DrawingChrono : AbstractDrawing, IDecorable, IKvaSerializable
    {
        #region Enums
        /// <summary>
        /// Enum used in CommandModifyChrono to know what value we are touching.
        /// </summary>
        public enum ChronoModificationType
        {
            TimeStart,
            TimeStop,
            TimeHide,
            Countdown
        }
        #endregion

        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading  infosFading
        {
        	// Fading is not modifiable from outside for chrono.
            get { throw new Exception("DrawingChrono, The method or operation is not implemented."); }
            set { throw new Exception("DrawingChrono, The method or operation is not implemented."); }
        }
        public override Capabilities Caps
		{
			get { return Capabilities.ConfigureColorSize; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        public Metadata ParentMetadata
        {
            get { return m_ParentMetadata; }    // unused.
            set { m_ParentMetadata = value; }
        }
        public long TimeStart
        {
            get { return m_iStartCountingTimestamp; }
        }
        public long TimeStop
        {
            get { return m_iStopCountingTimestamp; }
        }
        public long TimeVisible
        {
            get { return m_iVisibleTimestamp; }
        }
        public long TimeInvisible
        {
            get { return m_iInvisibleTimestamp; }
        }
        public bool CountDown
        {
        	get { return m_bCountdown;}
        	set 
        	{
        		// We should only toggle to countdown if we do have a stop value.
        		m_bCountdown = value;
        	}
        }
        public bool HasTimeStop
        {
        	// This is used to know if we can toggle to countdown or not.
        	get{ return (m_iStopCountingTimestamp != long.MaxValue);}
        }
        
        // The following properties are used from the formConfigureChrono.
        public string Label
        {
            get { return m_Label; }
            set { m_Label = value; }
        }
        public bool ShowLabel
        {
            get { return m_bShowLabel; }
            set { m_bShowLabel = value; }
        }
        #endregion

        #region Members
        
        // Core
        private long m_iStartCountingTimestamp;         	// chrono starts counting.
        private long m_iStopCountingTimestamp;          	// chrono stops counting. 
        private long m_iVisibleTimestamp;               	// chrono becomes visible.
        private long m_iInvisibleTimestamp;             	// chrono stops being visible.
        private bool m_bCountdown;							// chrono works backwards. (Must have a stop)
		
        // Data
        private string m_Text;							  	// Actual text displayed.
        private string m_Label;
        private bool m_bShowLabel;
        
        // Position
        private Point m_TopLeft;                         	// position (in image coords).
		private double m_fStretchFactor;
        private Point m_DirectZoomTopLeft;
        
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
		private static readonly int m_iAllowedFramesOver = 12;  // Number of frames the chrono stays visible after the 'Hiding' point.

        private Metadata m_ParentMetadata;

        // Computed
		private LabelBackground m_LabelBackground = new LabelBackground();
        private SizeF m_BackgroundSize;				  	// Size of the background rectangle (scaled).
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingChrono(int x, int y, long start, long _AverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            // Core
            m_TopLeft = new Point(x, y);
            m_BackgroundSize = new SizeF(100, 20);
            m_iVisibleTimestamp = start;
            m_iStartCountingTimestamp = long.MaxValue;
            m_iStopCountingTimestamp = long.MaxValue;
            m_iInvisibleTimestamp = long.MaxValue;
            m_bCountdown = false;

            m_Text = "error";
            
            m_StyleHelper.Bicolor = new Bicolor(Color.Black);
            m_StyleHelper.Font = new Font("Arial", 16, FontStyle.Bold);
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            m_Label = "";
            m_bShowLabel = true;
            
            m_fStretchFactor = 1.0;
            m_DirectZoomTopLeft = new Point(0, 0);
            
            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            m_InfosFading = new InfosFading(m_iInvisibleTimestamp, _AverageTimeStampsPerFrame);
            m_InfosFading.FadingFrames = m_iAllowedFramesOver;
            m_InfosFading.UseDefault = false;

            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public DrawingChrono(XmlReader _xmlReader, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
            : this(0, 0, 0, 1, ToolManager.Chrono.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale, _remapTimestampCallback);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            if (_iCurrentTimestamp >= m_iVisibleTimestamp)
            {
                m_fStretchFactor = _fStretchFactor;
                m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);

                // Compute the fading factor. (Special case from other drawings.)
                // ref frame is m_iInvisibleTimestamp, and we only fade after it, not before.
                double fOpacityFactor = 1.0;
                if (_iCurrentTimestamp > m_iInvisibleTimestamp)
                {
                	fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
                }

                // Update the text before we draw the background because it is used to compute size. 
                m_Text = GetTextValue(_iCurrentTimestamp);

                DrawBackground(_canvas, fOpacityFactor);
                DrawText(_canvas, fOpacityFactor);
                if (m_bShowLabel && m_Label.Length > 0)
                {
                    DrawLabel(_canvas, fOpacityFactor);
                }
            }
        }
        public override void MoveHandle(Point point, int handleNumber)
        {
            // Invisible handler to change font size.
            // Compare wanted mouse position with current bottom right.
            int wantedHeight = point.Y - m_TopLeft.Y;
            m_StyleHelper.ForceFontSize(wantedHeight, m_Text);
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            // Note: _delatX and _delatY are mouse delta already descaled.            
            m_TopLeft.X += _deltaX;
            m_TopLeft.Y += _deltaY;

            // Update scaled coordinates accordingly.
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            // Note: Coordinates are already descaled.
            // Hit Result: -1: miss, 0: on object, 1 on handle.

            int iHitResult = -1;
            
            long iMaxHitTimeStamps = m_iInvisibleTimestamp;
            if (iMaxHitTimeStamps != long.MaxValue)
            {
                iMaxHitTimeStamps += (m_iAllowedFramesOver * m_ParentMetadata.AverageTimeStampsPerFrame); 
            }

            if (_iCurrentTimestamp >= m_iVisibleTimestamp && _iCurrentTimestamp <= iMaxHitTimeStamps)
            {
            	if(GetHandleRectangle().Contains(_point))
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
        
        public override string ToString()
        {
            // Return the name of the tool used to draw this drawing.
            return ScreenManagerLang.ToolTip_DrawingToolChrono;
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = m_TopLeft.GetHashCode();
            iHash ^= m_iStartCountingTimestamp.GetHashCode();
            iHash ^= m_iStopCountingTimestamp.GetHashCode();
            iHash ^= m_iVisibleTimestamp.GetHashCode();
            iHash ^= m_iInvisibleTimestamp.GetHashCode();
            iHash ^= m_bCountdown.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            iHash ^= m_Label.GetHashCode();
            iHash ^= m_bShowLabel.GetHashCode();

            return iHash;
        }
		
		#region KVA Serialization
		public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("Position", String.Format("{0};{1}", m_TopLeft.X, m_TopLeft.Y));
            
		    _xmlWriter.WriteStartElement("Values");
		    _xmlWriter.WriteElementString("Visible", (m_iVisibleTimestamp == long.MaxValue) ? "-1" : m_iVisibleTimestamp.ToString());
            _xmlWriter.WriteElementString("StartCounting", (m_iStartCountingTimestamp == long.MaxValue) ? "-1" : m_iStartCountingTimestamp.ToString());
		    _xmlWriter.WriteElementString("StopCounting", (m_iStopCountingTimestamp == long.MaxValue) ? "-1" : m_iStopCountingTimestamp.ToString());
            _xmlWriter.WriteElementString("Invisible", (m_iInvisibleTimestamp == long.MaxValue) ? "-1" : m_iInvisibleTimestamp.ToString());
            _xmlWriter.WriteElementString("Countdown", m_bCountdown ? "true" : "false");
            
            // Spreadsheet support
            string userDuration = "0";
            if (m_iStartCountingTimestamp != long.MaxValue && m_iStopCountingTimestamp != long.MaxValue)
            {
             	userDuration = m_ParentMetadata.m_TimeStampsToTimecodeCallback(m_iStopCountingTimestamp - m_iStartCountingTimestamp, TimeCodeFormat.Unknown, false);
            }
            _xmlWriter.WriteElementString("UserDuration", userDuration);
            
            // </values>
            _xmlWriter.WriteEndElement();
            
            // Label
            _xmlWriter.WriteStartElement("Label");
            _xmlWriter.WriteElementString("Text", m_Label);
            _xmlWriter.WriteElementString("Show", m_bShowLabel ? "true" : "false");
            _xmlWriter.WriteEndElement();
            
		    _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
		}
		private void ReadXml(XmlReader _xmlReader, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Position":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        m_TopLeft = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                        break;
					case "Values":
						ParseWorkingValues(_xmlReader, _remapTimestampCallback);
						break;
					case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
				    case "Label":
						ParseLabel(_xmlReader);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        private void ParseWorkingValues(XmlReader _xmlReader, DelegateRemapTimestamp _remapTimestampCallback)
        {
            if(_remapTimestampCallback == null)
            {
                _xmlReader.ReadOuterXml();
                return;                
            }
            
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Visible":
				        m_iVisibleTimestamp = _remapTimestampCallback(_xmlReader.ReadElementContentAsLong(), false);
                        break;
					case "StartCounting":
                        long start = _xmlReader.ReadElementContentAsLong(); 
                        m_iStartCountingTimestamp = (start == -1) ? long.MaxValue : _remapTimestampCallback(start, false);
						break;
					case "StopCounting":
						long stop = _xmlReader.ReadElementContentAsLong();
						m_iStopCountingTimestamp = (stop == -1) ? long.MaxValue : _remapTimestampCallback(stop, false);
						break;
				    case "Invisible":
						long hide = _xmlReader.ReadElementContentAsLong();
                        m_iInvisibleTimestamp = (hide == -1) ? long.MaxValue : _remapTimestampCallback(hide, false);                        
						break;
					case "Countdown":
						m_bCountdown = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
            // Sanity check values.
            if (m_iVisibleTimestamp < 0) m_iVisibleTimestamp = 0;
            if (m_iStartCountingTimestamp < 0) m_iStartCountingTimestamp = 0;
            if (m_iStopCountingTimestamp < 0) m_iStopCountingTimestamp = 0;
            if (m_iInvisibleTimestamp < 0) m_iInvisibleTimestamp = 0;

            if (m_iVisibleTimestamp > m_iStartCountingTimestamp)
            {
                m_iVisibleTimestamp = m_iStartCountingTimestamp;
            }

            if (m_iStopCountingTimestamp < m_iStartCountingTimestamp)
            {
                m_iStopCountingTimestamp = long.MaxValue;
            }

            if (m_iInvisibleTimestamp < m_iStopCountingTimestamp)
            {
                m_iInvisibleTimestamp = long.MaxValue;
            }
        }
        private void ParseLabel(XmlReader _xmlReader)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Text":
				        m_Label = _xmlReader.ReadElementContentAsString();
                        break;
					case "Show":
                        m_bShowLabel = XmlHelper.ParseBoolean(_xmlReader.ReadElementContentAsString());
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
        }
		#endregion

        #region PopMenu commands Implementation that change internal values.
        public void Start(long _iCurrentTimestamp)
        {
            m_iStartCountingTimestamp = _iCurrentTimestamp;

            // Reset if crossed.
            if (m_iStartCountingTimestamp >= m_iStopCountingTimestamp)
            {
                m_iStopCountingTimestamp = long.MaxValue; 
            }
        }
        public void Stop(long _iCurrentTimestamp)
        {
            m_iStopCountingTimestamp = _iCurrentTimestamp;

            // ? if crossed.
            if (m_iStopCountingTimestamp <= m_iStartCountingTimestamp)
            {
                m_iStartCountingTimestamp = m_iStopCountingTimestamp;
            }

            if (m_iStopCountingTimestamp > m_iInvisibleTimestamp)
            {
                m_iInvisibleTimestamp = m_iStopCountingTimestamp;
            }
        }
        public void Hide(long _iCurrentTimestamp)
        {
            m_iInvisibleTimestamp = _iCurrentTimestamp;

            // Update fading conf.
            m_InfosFading.ReferenceTimestamp = m_iInvisibleTimestamp;
            
            // Avoid counting when fading.
            if (m_iInvisibleTimestamp < m_iStopCountingTimestamp)
            {
                m_iStopCountingTimestamp = m_iInvisibleTimestamp;
                if (m_iStopCountingTimestamp < m_iStartCountingTimestamp)
                {
                    m_iStartCountingTimestamp = m_iStopCountingTimestamp;
                }
            }
        }
        #endregion

        #region Lower level helpers
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Bicolor", "color");
            m_Style.Bind(m_StyleHelper, "Font", "font size");    
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            m_LabelBackground.Location = new Point((int)((double)(m_TopLeft.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_TopLeft.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private void ShiftCoordinates(int _iShiftHorz, int _iShiftVert, double _fStretchFactor)
        {
            m_LabelBackground.Location = new Point((int)((double)m_TopLeft.X * _fStretchFactor) + _iShiftHorz, (int)((double)m_TopLeft.Y * _fStretchFactor) + _iShiftVert);
        }
        private void DrawBackground(Graphics _canvas, double _fOpacityFactor)
        {
            // Draw background rounded rectangle.
            // The radius for rounding is based on font size.
            Font f = m_StyleHelper.GetFont((float)m_fStretchFactor);
            m_BackgroundSize = _canvas.MeasureString(m_Text + " ", f);
            int radius = (int)(f.Size / 2);
            f.Dispose();
            
            m_LabelBackground.Draw(_canvas, _fOpacityFactor, radius, (int)m_BackgroundSize.Width, (int)m_BackgroundSize.Height, m_StyleHelper.Bicolor.Background);
        }
        private void DrawLabel(Graphics _canvas, double _fOpacityFactor)
        {
            // Label background and size is relative to the main chrono.
            Font f = m_StyleHelper.GetFont((float)m_fStretchFactor);
            int radius = (int)(f.Size / 4);
            f.Dispose();
            Font fontText = m_StyleHelper.GetFont(0.5f);
            SizeF labelSize = _canvas.MeasureString(m_Label + " ", fontText);
            
			// the label background starts at the end of the rounded angle of the main background.
            Rectangle RescaledBackground = new Rectangle(m_LabelBackground.Location.X + radius, m_LabelBackground.Location.Y - (int)labelSize.Height - 1, (int)labelSize.Width + 11, (int)labelSize.Height);

            LabelBackground labelBG = new LabelBackground(RescaledBackground.Location, true, 11, 0);
            labelBG.Draw(_canvas, _fOpacityFactor, radius, (int)labelSize.Width, (int)labelSize.Height, m_StyleHelper.GetBackgroundColor(128));
            
            // Label text
            SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_fOpacityFactor * 255));
            _canvas.DrawString(m_Label, fontText, fontBrush, new Point(RescaledBackground.X+4, RescaledBackground.Y+1));
            fontBrush.Dispose();
            fontText.Dispose();
        }
        private void DrawText(Graphics _canvas, double _fOpacityFactor)
        {
        	Font fontText = m_StyleHelper.GetFont((float)m_fStretchFactor);
        	SolidBrush fontBrush = m_StyleHelper.GetForegroundBrush((int)(_fOpacityFactor * 255));
        	_canvas.DrawString(m_Text, fontText, fontBrush, m_LabelBackground.TextLocation);
        	fontBrush.Dispose();
        	fontText.Dispose();
        }
        private bool IsPointInObject(Point _point)
        {
            // Point coordinates are descaled.
            // We need to descale the hit area size for coherence.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            GraphicsPath areaPath = new GraphicsPath();
            areaPath.AddRectangle(new Rectangle(m_TopLeft.X, m_TopLeft.Y, descaledSize.Width, descaledSize.Height));

            // Create region from the path
            Region areaRegion = new Region(areaPath);

            bool hit = areaRegion.IsVisible(_point);
            return hit;
        }
        private string GetTextValue(long _iTimestamp)
        {
            long timestamps;

            // compute Text value depending on where we are.
            if (_iTimestamp > m_iStartCountingTimestamp)
            {
                if (_iTimestamp <= m_iStopCountingTimestamp)
                {
                	// After start and before stop.
                	if(m_bCountdown)
                	{
                		timestamps = m_iStopCountingTimestamp - _iTimestamp;
                	}
                	else
                	{
                		timestamps = _iTimestamp - m_iStartCountingTimestamp;                		
                	}
                }
                else
                {
                    // After stop. Keep max value.
                    if(m_bCountdown)
                    {
                    	timestamps = 0;
                    }
                    else
                    {
                    	timestamps = m_iStopCountingTimestamp - m_iStartCountingTimestamp;
                    }
                }
            }
            else
            {
            	// Before start. Keep min value.
            	if(m_bCountdown)
            	{
            		timestamps = m_iStopCountingTimestamp - m_iStartCountingTimestamp;
            	}
				else
				{
					timestamps = 0;
				}
            }

            return m_ParentMetadata.m_TimeStampsToTimecodeCallback(timestamps, TimeCodeFormat.Unknown, false);
        }
    	private Rectangle GetHandleRectangle()
        {
            // This function is only used for Hit Testing.
            Size descaledSize = new Size((int)((m_BackgroundSize.Width + m_LabelBackground.MarginWidth) / m_fStretchFactor), (int)((m_BackgroundSize.Height + m_LabelBackground.MarginHeight) / m_fStretchFactor));

            return new Rectangle(m_TopLeft.X + descaledSize.Width - 10, m_TopLeft.Y + descaledSize.Height - 10, 20, 20);
        }
        #endregion
    }
}
