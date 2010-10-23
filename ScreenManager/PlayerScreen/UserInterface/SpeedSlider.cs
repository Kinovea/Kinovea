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


using Kinovea.ScreenManager.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Imaging.Filters;

namespace Kinovea.ScreenManager
{
	/// A slider control.
	/// 
	/// When value is modified by user:
	/// - The internal value is modified.
	/// - Events are raised, which are listened to by parent control.
	/// - Parent control update its own internal data state by reading the properties.
	/// 
	/// When control appearence needs to be updated
	/// - This is when internal data of the parent control have been modified by other means.
	/// - (At initialization for example)
	/// - The public properties setters are provided, they doesn't raise the events back.
	/// 
	/// This control is pretty similar to FrameTracker. Maybe it would be possible to factorize the code.
	/// </summary>
    public partial class SpeedSlider : UserControl
    {
        #region Properties
        [Category("Behavior"), Browsable(true)]
        public int Minimum
        {
            get
            {
                return m_iMinimum;
            }
            set
            {
                m_iMinimum = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int Maximum
        {
            get
            {
                return m_iMaximum;
            }
            set
            {
                m_iMaximum = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int Value
        {
            get
            {
            	if (m_iValue < m_iMinimum) m_iValue = m_iMinimum;
                return m_iValue;
            }
            set
            {
                m_iValue = value;
                if (m_iValue < m_iMinimum) m_iValue = m_iMinimum;
                if (m_iValue > m_iMaximum) m_iValue = m_iMaximum;
                UpdateCursorPosition();
				Invalidate();				
            }
        }
        [Category("Misc"), Browsable(true)]
        public string ToolTip
        {
            set
            {
                toolTips.SetToolTip(this, value);
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int SmallChange
        {
            get
            {
                return m_iSmallChange;
            }
            set
            {
                m_iSmallChange = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int LargeChange
        {
            get
            {
                return m_iLargeChange;
            }
            set
            {
                m_iLargeChange = value;
            }
        }
        [Category("Behavior"), Browsable(true)]
        public int StickyValue
        {
            get
            {
                return m_iStickyValue;
            }
            set
            {
                m_iStickyValue = value;
                m_iStickyPixel = GetCoordFromValue(m_iStickyValue);
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool StickyMark
        {
            get
            {
                return m_bStickyMark;
            }
            set
            {
                m_bStickyMark = value;
            }
        }
        #endregion

        #region Members
        private bool m_bInvalidateAsked;	// To prevent reentry in MouseMove before the paint event has been honored.	
        private bool m_bEnabled = true;
        
        private int m_iMinimum = 1;
        private int m_iValue = 100;
        private int m_iMaximum = 200;
        private int m_iSmallChange = 1;
        private int m_iLargeChange = 5;
        private int m_iStickyValue = 100;
        
        private int m_iMaxWidth;			// Number of pixels in the control that can be used for values.
        private int m_iMinimumPixel;
        private int m_iMaximumPixel;
        private int m_iStickyPixel;
        private int m_iPixelPosition;		// Left of the cursor in pixels.
        private bool m_bStickyMark = true;
        
        private bool m_bDecreasing;
		private bool m_bIncreasing;
		
        private int m_iSpacers = 10;			// size of space between buttons and rail.
        private int m_iButtonWidth = Resources.SpeedTrkDecrease2.Width;
        private int m_iCursorWidth = Resources.SpeedTrkCursor7.Width;
        
        private Bitmap bmpDecrease = Resources.SpeedTrkDecrease2;
        private Bitmap bmpIncrease = Resources.SpeedTrkIncrease2;
        private Bitmap bmpBackground = Resources.SpeedTrkBack5;
        private Bitmap bmpCursor = Resources.SpeedTrkCursor7;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region EventDelegates
        public delegate void ValueChangedHandler(object sender, EventArgs e);
        
        [Category("Action"), Browsable(true)]
        public event ValueChangedHandler ValueChanged;
        #endregion

        #region Ctor
        public SpeedSlider()
        {
            InitializeComponent();
            
            // Activates double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            this.Cursor = Cursors.Hand;
            
            m_iMinimumPixel = m_iButtonWidth + m_iSpacers;
            m_iMaximumPixel = this.Width - (m_iSpacers + m_iButtonWidth);
            m_iMaxWidth = m_iMaximumPixel - m_iMinimumPixel;
            m_iStickyPixel = GetCoordFromValue(m_iStickyValue);
                        
            this.BackColor = Color.White;
        }
        #endregion

        #region Public Methods
        public void EnableDisable(bool _bEnable)
        {
        	m_bEnabled = _bEnable;
			Invalidate();
        }
        #endregion
        
        #region Event Handlers - User Manipulation
        private void SpeedSlider_MouseDown(object sender, MouseEventArgs e)
        {
        	// Register which button we hit. We'll handle the action in MouseUp.
        	
        	m_bDecreasing = false;
        	m_bIncreasing = false;
        	
        	if (m_bEnabled && e.Button == MouseButtons.Left)
            {
        		if(e.X >= 0 && e.X < m_iButtonWidth)
        		{
        			// on decrease button.
        			m_bDecreasing = true;
        		}
        		else if(e.X >= Width - m_iButtonWidth && e.X < Width)
        		{
        			// on increase button.
        			m_bIncreasing = true;
        		}
        	}
        }
        private void SpeedSlider_MouseMove(object sender, MouseEventArgs e)
        {
        	// Note: also raised on mouse down.
        	// User wants to jump to position.
        	if(m_bEnabled && !m_bInvalidateAsked)
        	{
	        	if (e.Button == MouseButtons.Left)
	            {
	        		Point mouseCoords = this.PointToClient(Cursor.Position);
	        		
	        		if ((mouseCoords.X > m_iMinimumPixel) && (mouseCoords.X < m_iMaximumPixel))
	                {
	        			m_iPixelPosition = mouseCoords.X - (m_iCursorWidth/2);
            			m_iValue = GetValueFromCoord(mouseCoords.X);
            			
			            // Stickiness
			            if (
			                (m_iValue >= (m_iStickyValue - 5)) && (m_iValue <= m_iStickyValue) ||
			                (m_iValue <= (m_iStickyValue + 5)) && (m_iValue >= m_iStickyValue)
			                )
			            {
			            	// Inside sticky zone, fall back to sticky value.
			            	m_iValue = m_iStickyValue;
			            	m_iPixelPosition = m_iStickyPixel - (m_iCursorWidth/2);
			            }
	        		
	        			Invalidate();
					    m_bInvalidateAsked = true;
					    
						if (ValueChanged != null) 
			            { 
			                ValueChanged(this, EventArgs.Empty);
			            }
	        		}
	        	}
        	}
        }
        private void SpeedSlider_MouseUp(object sender, MouseEventArgs e)
        {
        	// This is when the validation of the change occur.
        	if (m_bEnabled && e.Button == MouseButtons.Left)
            {
        		bool changed = false;
        		
        		if(m_bDecreasing)
        		{
	        		if (m_iMaximum - m_iMinimum > 0 && m_iValue > m_iMinimum + m_iLargeChange)
		            {
		                m_iValue -= m_iLargeChange;
		        		changed = true;
		            }
        		}
        		else if(m_bIncreasing)
        		{
        			if (m_iMaximum - m_iMinimum > 0 && m_iValue <= m_iMaximum - m_iLargeChange)
        			{
        				m_iValue += m_iLargeChange;
        				changed = true;
        			}
				}
        		
        		if(changed)
        		{
        			UpdateCursorPosition();
        			Invalidate();
    			    if (ValueChanged != null)
	                {
	                    ValueChanged(this, EventArgs.Empty);
	                }
        		}
        	}
        }
        #endregion
        
        #region Paint / Resize
        private void SpeedSlider_Paint(object sender, PaintEventArgs e)
        {
        	// When we land in this function, m_iPixelPosition should have been set already.
        	// It is the only member variable we'll use here.
        	
        	// Draw buttons
        	if(m_bEnabled)
        	{
        		e.Graphics.DrawImage(bmpDecrease, 0, 0);
        		e.Graphics.DrawImage(bmpIncrease, m_iMaximumPixel + m_iSpacers, 0);
        	}
        	
        	// Draw tiled background
        	for(int i=m_iMinimumPixel;i<m_iMaximumPixel; i+=bmpBackground.Width)
        	{
        		e.Graphics.DrawImage(bmpBackground, i, 0);
        	}
        	
        	// MiddleMarker
        	if(m_bStickyMark)
        	{
        		e.Graphics.DrawLine(Pens.Gray, m_iStickyPixel, 0, m_iStickyPixel, 3);
        		e.Graphics.DrawLine(Pens.Gray, m_iStickyPixel, 7, m_iStickyPixel, 10);
        	}
        	
      		// Draw th e cursor.
        	if(m_bEnabled)
        	{
        	    e.Graphics.DrawImage(bmpCursor, m_iPixelPosition, 0);
        	}
        	
        	m_bInvalidateAsked = false;
        }
        private void SpeedSlider_Resize(object sender, EventArgs e)
        {
        	// Resize of the control only : internal data doesn't change.
        	m_iMaximumPixel = this.Width - (m_iSpacers + m_iButtonWidth);
            m_iMaxWidth = m_iMaximumPixel - m_iMinimumPixel;
            m_iStickyPixel = GetCoordFromValue(m_iStickyValue);
            
            UpdateCursorPosition();
            Invalidate();
        }
        #endregion
        
        #region Binding UI to Data
        private void UpdateCursorPosition()
        {
        	// This method updates the appearence of the control only, it doesn't raise the events back.
        	// Should be called every time m_iPosition has been updated. 
            m_iPixelPosition = GetCoordFromValue(m_iValue) - (m_iCursorWidth/2);
        }
        private int GetCoordFromValue(int _value)
       	{
			int iret = m_iMinimumPixel + Rescale(_value - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
            return iret;
       	}
        private int GetValueFromCoord(int _pos)
       	{
       		int ret = m_iMinimum + Rescale(_pos - m_iMinimumPixel, m_iMaxWidth, m_iMaximum - m_iMinimum);
       		return ret;
       	}
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            if(_iOldMax > 0)
            {
            	return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
            }
            else
            {
            	return 0;
            }
        }
        #endregion
    }
}
