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
        #region Properties
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public override InfosFading  InfosFading
        {
        	// Fading is not modifiable from outside for chrono.
            get { return null; }
            set { }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.ConfigureColorSize; }
		}
        public override List<ToolStripItem> ContextMenu
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
            set 
            { 
                m_Label = value;
                UpdateLabelRectangle();
            }
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
        private string m_Timecode;
        private string m_Label;
        private bool m_bShowLabel;
        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private InfosFading m_InfosFading;
		private static readonly int m_iAllowedFramesOver = 12;  // Number of frames the chrono stays visible after the 'Hiding' point.
        private RoundedRectangle m_MainBackground = new RoundedRectangle();
        private RoundedRectangle m_lblBackground = new RoundedRectangle();
        
        private Metadata m_ParentMetadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        public DrawingChrono(Point p, long start, long _AverageTimeStampsPerFrame, DrawingStyle _preset)
        {
            // Core
            m_iVisibleTimestamp = start;
            m_iStartCountingTimestamp = long.MaxValue;
            m_iStopCountingTimestamp = long.MaxValue;
            m_iInvisibleTimestamp = long.MaxValue;
            m_bCountdown = false;
            m_MainBackground.Rectangle = new Rectangle(p, Size.Empty);

            m_Timecode = "error";
            
            m_StyleHelper.Bicolor = new Bicolor(Color.Black);
            m_StyleHelper.Font = new Font("Arial", 16, FontStyle.Bold);
            if(_preset != null)
            {
                m_Style = _preset.Clone();
                BindStyle();
            }
            
            m_Label = "";
            m_bShowLabel = true;
            
            // We use the InfosFading utility to fade the chrono away.
            // The refererence frame will be the frame at which fading start.
            // Must be updated on "Hide" menu.
            m_InfosFading = new InfosFading(m_iInvisibleTimestamp, _AverageTimeStampsPerFrame);
            m_InfosFading.FadingFrames = m_iAllowedFramesOver;
            m_InfosFading.UseDefault = false;
        }
        public DrawingChrono(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
            : this(Point.Empty, 0, 1, ToolManager.Chrono.StylePreset.Clone())
        {
            ReadXml(_xmlReader, _scale, _remapTimestampCallback);
        }
        #endregion

        #region AbstractDrawing Implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
        {
            if (_iCurrentTimestamp < m_iVisibleTimestamp)
                return;
            
            double fOpacityFactor = 1.0;
            if (_iCurrentTimestamp > m_iInvisibleTimestamp)
                fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);

            if (fOpacityFactor <= 0)
                return;

            m_Timecode = GetTimecode(_iCurrentTimestamp);

            // Update unscaled backround size according to timecode text. Needed for hit testing.
            Font f = m_StyleHelper.GetFont(1F);
            SizeF totalSize = _canvas.MeasureString(" " + m_Timecode + " ", f);
            SizeF textSize = _canvas.MeasureString(m_Timecode, f);
            f.Dispose();
            m_MainBackground.Rectangle = new Rectangle(m_MainBackground.Rectangle.Location, new Size((int)totalSize.Width, (int)totalSize.Height));
            
            using (SolidBrush brushBack = m_StyleHelper.GetBackgroundBrush((int)(fOpacityFactor * 128)))
            using (SolidBrush brushText = m_StyleHelper.GetForegroundBrush((int)(fOpacityFactor * 255)))
            using (Font fontText = m_StyleHelper.GetFont((float)_transformer.Scale))
            {
                Rectangle rect = _transformer.Transform(m_MainBackground.Rectangle);
                RoundedRectangle.Draw(_canvas, rect, brushBack, fontText.Height/4, false, false, null);

                int margin = (int)((totalSize.Width - textSize.Width) / 2);
                Point textLocation = new Point(rect.X + margin, rect.Y);
                _canvas.DrawString(m_Timecode, fontText, brushText, textLocation);

                if (m_bShowLabel && m_Label.Length > 0)
                {
                    using (Font fontLabel = m_StyleHelper.GetFont((float)_transformer.Scale * 0.5f))
                    {
                        SizeF lblTextSize = _canvas.MeasureString(m_Label, fontLabel);
                        Rectangle lblRect = new Rectangle(rect.Location.X, rect.Location.Y - (int)lblTextSize.Height, (int)lblTextSize.Width, (int)lblTextSize.Height);
                        RoundedRectangle.Draw(_canvas, lblRect, brushBack, fontLabel.Height/3, true, false, null);
                        _canvas.DrawString(m_Label, fontLabel, brushText, lblRect.Location);
                    }
                }
            }
        }
        public override int HitTest(Point point, long currentTimestamp, CoordinateSystem transformer)
        {
            // Convention: miss = -1, object = 0, handle = n.
            int result = -1;
            long maxHitTimeStamps = m_iInvisibleTimestamp;
            if (maxHitTimeStamps != long.MaxValue)
                maxHitTimeStamps += (m_iAllowedFramesOver * m_ParentMetadata.AverageTimeStampsPerFrame);

            if (currentTimestamp >= m_iVisibleTimestamp && currentTimestamp <= maxHitTimeStamps)
            {
                result = m_MainBackground.HitTest(point, true, transformer);
                if(result < 0) 
                    result = m_lblBackground.HitTest(point, false, transformer);
            }

            return result;
        }
        public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
        {
            // Invisible handler to change font size.
            int wantedHeight = point.Y - m_MainBackground.Rectangle.Location.Y;
            m_StyleHelper.ForceFontSize(wantedHeight, m_Timecode);
            UpdateLabelRectangle();
        }
        public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
        {
            m_MainBackground.Move(_deltaX, _deltaY);
            m_lblBackground.Move(_deltaX, _deltaY);
        }
        #endregion
        
        public override string ToString()
        {
            return ScreenManagerLang.ToolTip_DrawingToolChrono;
        }
        public override int GetHashCode()
        {
            int iHash = m_MainBackground.GetHashCode();
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
            _xmlWriter.WriteElementString("Position", String.Format("{0};{1}", m_MainBackground.Rectangle.Location.X, m_MainBackground.Rectangle.Location.Y));
            
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
             	userDuration = m_ParentMetadata.TimeStampsToTimecode(m_iStopCountingTimestamp - m_iStartCountingTimestamp, TimecodeFormat.Unknown, false);
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
		private void ReadXml(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
        {
            _xmlReader.ReadStartElement();
            
			while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "Position":
				        Point p = XmlHelper.ParsePoint(_xmlReader.ReadElementContentAsString());
                        Point location = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));
                        m_MainBackground.Rectangle = new Rectangle(location, Size.Empty);
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
        }
        private void ParseWorkingValues(XmlReader _xmlReader, TimeStampMapper _remapTimestampCallback)
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
        private void UpdateLabelRectangle()
        {
            using(Font f = m_StyleHelper.GetFont(0.5F))
            using(Button but = new Button())
            using(Graphics g = but.CreateGraphics())
            {
                SizeF size = g.MeasureString(m_Label, f);
                m_lblBackground.Rectangle = new Rectangle(m_MainBackground.X,
                                                          m_MainBackground.Y - (int)m_lblBackground.Rectangle.Height,
                                                          (int)size.Width + 11,
                                                          (int)size.Height);
                
            }
        }
        private string GetTimecode(long _iTimestamp)
        {
            long timestamps;

            // compute Text value depending on where we are.
            if (_iTimestamp > m_iStartCountingTimestamp)
            {
                if (_iTimestamp <= m_iStopCountingTimestamp)
                {
                	// After start and before stop.
                	if(m_bCountdown)
                		timestamps = m_iStopCountingTimestamp - _iTimestamp;
                	else
                		timestamps = _iTimestamp - m_iStartCountingTimestamp;                		
                }
                else
                {
                    // After stop. Keep max value.
                    timestamps = m_bCountdown ? 0 : m_iStopCountingTimestamp - m_iStartCountingTimestamp;
                }
            }
            else
            {
            	// Before start. Keep min value.
                timestamps = m_bCountdown ? m_iStopCountingTimestamp - m_iStartCountingTimestamp : 0;
            }

            return m_ParentMetadata.TimeStampsToTimecode(timestamps, TimecodeFormat.Unknown, false);
        }
        #endregion
    }

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
}
