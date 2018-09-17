/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Base;
using Kinovea.ScreenManager.Properties;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A control to let the user specify the Working Zone.
    /// The control is comprised of bumpers at the ends, handlers around the selection,
    /// a middle section for the selection, and a hairline for the current position.
    /// 
    /// When control is modified by user:
    /// - The internal data is modified.
    /// - Events are raised, which are listened to by parent control.
    /// - Parent control update its own internal data state by reading the properties.
    /// 
    /// When control appearence needs to be updated
    /// - This is when internal data of the parent control have been modified by other means.
    /// - (At initialization for example)
    /// - The public properties setters are provided, they doesn't raise the events back. 
    /// </summary>
    public partial class SelectionTracker : UserControl
    {
        #region Properties
        [Category("Behavior"), Browsable(true)]
        public long Minimum
        {
            get{return m_iMinimum;}
            set
            {
                m_iMinimum = value;
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Maximum
        {
            get{return m_iMaximum;}
            set
            {
                m_iMaximum = value;
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long SelStart
        {
            get
            {
                return m_iSelStart;
            }
            set
            {
                m_iSelStart = value;
                if (m_iSelStart < m_iMinimum)
                {
                    m_iSelStart = m_iMinimum;
                }
                
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long SelEnd
        {
            get
            {
                return m_iSelEnd;
            }
            set
            {
                m_iSelEnd = value;
                if (m_iSelEnd < m_iSelStart) 
                {
                    m_iSelEnd = m_iSelStart;
                }
                else if (m_iSelEnd > m_iMaximum) 
                {
                    m_iSelEnd = m_iMaximum;
                }
                
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(false)]
        public long SelPos
        {
            get 
            {
                return m_iSelPos;
            }
            set
            {
                m_TimeWatcher.Restart();
                m_iSelPos = value;
                if (m_iSelPos < m_iSelStart) m_iSelPos = m_iSelStart;
                else if (m_iSelPos > m_iSelEnd) m_iSelPos = m_iSelEnd;
                
                m_TimeWatcher.LogTime("Selection tracker, Update appearance asked.");
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool SelLocked {
            get { return m_bSelLocked; }
            set { m_bSelLocked = value; }
        }
        [Category("Misc"), Browsable(true)]
        public string ToolTip
        {
            get {return toolTips.GetToolTip(this);}
            set
            {
                toolTips.SetToolTip(this, value);
            }
        }
        #endregion

        #region Members
        private bool m_bSelLocked;
        
        // Data
        private long m_iMinimum;		// All data are in absolute timestamps.
        private long m_iMaximum = 100;
        private long m_iSelStart;
        private long m_iSelEnd = 100;
        private long m_iSelPos;

        // Display
        private bool m_bEnabled = true;
        private int m_iMinimumPixel;
        private int m_iMaximumPixel;
        private int m_iMaxWidthPixel;      	// Inner size of selection in pixels.
        private int m_iStartPixel;			// First pixel of the selection zone.
        private int m_iEndPixel;			// Last pixel of the selection zone.
        private int m_iPositionPixel;		// Exact position of the playhead.
        
        // Graphics
        private static readonly Bitmap bmpBumperLeft = Resources.liqbumperleft;
        private static readonly Bitmap bmpBumperRight = Resources.liqbumperright;
        private static readonly Bitmap bmpBackground = Resources.liqbackdock;
        private static readonly Bitmap bmpHandlerLeft = Resources.liqhandlerleft2;
        private static readonly Bitmap bmpHandlerRight = Resources.liqhandlerright3;
        private static readonly Bitmap bmpMiddleBar = Resources.liqmiddlebar;
        private static readonly int m_iSpacerWidth = 10;
        private static readonly int m_iBumperWidth = bmpBumperLeft.Width;
        private static readonly int m_iHandlerWidth = bmpHandlerLeft.Width;
        
        // Interaction
        private bool m_bDraggingLeft;
        private bool m_bDraggingRight;
        private bool m_bDraggingTarget;
        
        private TimeWatcher m_TimeWatcher = new TimeWatcher();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events
        [Category("Action"), Browsable(true)]
        public event EventHandler SelectionChanging;
        [Category("Action"), Browsable(true)]
        public event EventHandler SelectionChanged;
        [Category("Action"), Browsable(true)]
        public event EventHandler TargetAcquired;
        #endregion

        #region Contructor
        public SelectionTracker()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.Cursor = Cursors.Hand;
            
            m_iMinimumPixel = m_iSpacerWidth + m_iBumperWidth;
            m_iMaximumPixel = Width - m_iSpacerWidth - m_iBumperWidth;
            m_iMaxWidthPixel = m_iMaximumPixel - m_iMinimumPixel;
        }
        #endregion

        #region Public Methods - Timestamps to pixels.
        public void UpdateInternalState(long _iMin, long _iMax, long _iStart, long _iEnd, long _iPos)
        {
            // This method is only a shortcut to updating all properties at once.
            // It should be called when the internal state of data has been modified
            // by other means than the user manipulating the control.
            // (for example, at initialization or reset.)
            // This method update the appearence of the control only, it doesn't raise the events back.
            // All input data are in absolute timestamps.
            m_iMinimum = _iMin;
            m_iMaximum = _iMax;
            m_iSelStart = _iStart;
            m_iSelEnd = _iEnd;
            m_iSelPos = _iPos;
            
            UpdateAppearence();
        }
        public void Reset()
        {
            m_iSelStart = m_iMinimum;
            m_iSelEnd = m_iMaximum;
            m_iSelPos = m_iMinimum;
            UpdateAppearence();
        }
        public void EnableDisable(bool _bEnable)
        {
            m_bEnabled = _bEnable;
            Invalidate();
        }
        #endregion
        
        #region Interaction Events - Pixels to timestamps.
        private void SelectionTracker_MouseDown(object sender, MouseEventArgs e)
        {
            if(m_bSelLocked || !m_bEnabled)
                return;
            
            m_bDraggingLeft = false;
            m_bDraggingRight = false;
            m_bDraggingTarget = false;

            if (e.Button == MouseButtons.Left)
            {
                if(e.X >= m_iStartPixel - m_iHandlerWidth && e.X < m_iStartPixel)
                {
                    // in handler left.
                    m_bDraggingLeft = true;
                }
                else if(e.X >= m_iEndPixel && e.X < m_iEndPixel + m_iHandlerWidth)
                {
                    // in handler right.
                    m_bDraggingRight = true;
                }
                else if(e.X >= m_iStartPixel && e.X < m_iEndPixel)
                {
                    // in selection.
                    m_bDraggingTarget = true;
                }
                else if(e.X < m_iMinimumPixel)
                {
                    // before minimum.
                }
                else if(e.X >= m_iMaximumPixel)
                {
                    // after maximum.
                }
                else
                {
                    // in background.
                }
            }
        }
        private void SelectionTracker_MouseMove(object sender, MouseEventArgs e)
        {
            if(m_bSelLocked || !m_bEnabled)
                return;
            
            if (e.Button == MouseButtons.Left &&
                (m_bDraggingLeft || m_bDraggingRight || m_bDraggingTarget))
            {
                if(m_bDraggingLeft)
                {
                    if(e.X >= m_iMinimumPixel - (m_iHandlerWidth / 2) && e.X < m_iEndPixel - (m_iHandlerWidth / 2))
                    {
                        m_iStartPixel = e.X + (m_iHandlerWidth / 2);
                        m_iPositionPixel = Math.Max(m_iPositionPixel, m_iStartPixel);
                    }
                }
                else if(m_bDraggingRight)
                {
                    if(e.X >= m_iStartPixel + (m_iHandlerWidth / 2) && e.X < m_iMaximumPixel + (m_iHandlerWidth / 2))
                    {
                        m_iEndPixel = e.X - (m_iHandlerWidth / 2);
                        m_iPositionPixel = Math.Min(m_iPositionPixel, m_iEndPixel);
                    }
                }
                else if(m_bDraggingTarget)
                {
                    if(e.X >= m_iStartPixel && e.X < m_iEndPixel)
                    {
                        m_iPositionPixel = e.X;
                    }
                }
                
                Invalidate();
                
                // Update values and report to container.
                m_iSelPos = GetTimestampFromCoord(m_iPositionPixel);
                m_iSelStart = GetTimestampFromCoord(m_iStartPixel);
                m_iSelEnd = GetTimestampFromCoord(m_iEndPixel);
                if (SelectionChanging != null) { SelectionChanging(this, EventArgs.Empty); }
            }
        }
        private void SelectionTracker_MouseUp(object sender, MouseEventArgs e)
        {
             if(m_bSelLocked || !m_bEnabled || e.Button != MouseButtons.Left)
                return;
             
            // This is when the validation of the change occur.
            if(m_bDraggingTarget)
            {
                // Handle the special case of simple click to change position.
                // (mouseMove is not triggered in this case.)
                if(e.X >= m_iStartPixel && e.X < m_iEndPixel)
                {
                    m_iPositionPixel = e.X;
                    Invalidate();
                }
                
                // Update values and report to container.
                m_iSelPos = GetTimestampFromCoord(m_iPositionPixel);
                if (TargetAcquired != null) { TargetAcquired(this, EventArgs.Empty); }
            }
            else if(m_bDraggingLeft || m_bDraggingRight)
            {
                // Update values and report to container.
                m_iSelStart = GetTimestampFromCoord(m_iStartPixel);
                m_iSelEnd = GetTimestampFromCoord(m_iEndPixel);
                if (SelectionChanged != null)
                    SelectionChanged(this, EventArgs.Empty);
            }
        }
        #endregion
        
        #region Paint / Resize
        private void SelectionTracker_Paint(object sender, PaintEventArgs e)
        {
            // Draw the control.
            // All the position variables must have been set already.
            
            m_TimeWatcher.LogTime("Selection tracker, actual start of paint.");
            
            Draw(e.Graphics);
            
            m_TimeWatcher.LogTime("Selection tracker, end of paint.");
            //m_TimeWatcher.DumpTimes();
        }
        private void Draw(Graphics _canvas)
        {
            // TODO: 
            // - draw the background stretched, not tiled.
            // - use draw unscaled where possible.
            
            // Draw background.
            // (we draw it first to be able to cover the extra tiling)
            for(int i=m_iMinimumPixel; i<m_iMaximumPixel; i+=bmpBackground.Width)
            {
                _canvas.DrawImage(bmpBackground, i, 0);
            }
            
            // Draw bumpers
            _canvas.DrawImage(bmpBumperLeft, m_iSpacerWidth, 0);
            _canvas.DrawImage(bmpBumperRight, m_iMaximumPixel, 0);
            
            // Draw content.
            if(m_bEnabled)
            {
                // Draw selection zone. 
                // (we draw it first to be able to cover the extra tiling)
                for(int i=m_iStartPixel;i<m_iEndPixel;i+=bmpMiddleBar.Width)
                {
                    _canvas.DrawImage(bmpMiddleBar, i, 0);
                }
                
                // Draw handlers
                _canvas.DrawImage(bmpHandlerLeft, m_iStartPixel - m_iHandlerWidth, 0);
                _canvas.DrawImage(bmpHandlerRight, m_iEndPixel, 0);
                
                // Draw hairline.
                _canvas.DrawLine(Pens.Black, m_iPositionPixel, 4, m_iPositionPixel, Height - 10);
            }
        }
        private void SelectionTracker_Resize(object sender, EventArgs e)
        {
            // Resize of the control only : data doesn't change.
            m_iMaximumPixel = Width - m_iSpacerWidth - m_iBumperWidth;
            m_iMaxWidthPixel = m_iMaximumPixel - m_iMinimumPixel;
            UpdateAppearence();
        }
        #endregion
        
        #region Binding UI and Data
        private void UpdateAppearence()
        {
            // Internal state of data has been modified programmatically.
            // (for example, initialization, reset, boundaries buttons, etc.)
            // This method updates the appearence of the control only, it doesn't raise the events back.
            if (m_iMaximum - m_iMinimum > 0)
            {
                m_iStartPixel = GetCoordFromTimestamp(m_iSelStart);
                m_iEndPixel = GetCoordFromTimestamp(m_iSelEnd);
                m_iPositionPixel = GetCoordFromTimestamp(m_iSelPos);
                Invalidate();
            }
        }
        private int GetCoordFromTimestamp(long _ts)
        {
            // Take any timestamp and convert it into a pixel coord.
            int iret = m_iMinimumPixel + Rescale(_ts - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidthPixel);
            return iret;
        }
        private long GetTimestampFromCoord(int _posPixel)
        {
            // Take any position in pixel and convert it into a timestamp.
            // At this point, the pixel position shouldn't be outside the boundaries values.
            long ret = m_iMinimum + Rescale(_posPixel - m_iMinimumPixel, m_iMaxWidthPixel, m_iMaximum - m_iMinimum);
            return ret;
        }
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
        }
        #endregion 
    }
}
