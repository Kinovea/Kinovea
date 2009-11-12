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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
        [Category("Behavior"), Browsable(true)]
        public bool SelLocked
        {
            get
            {
                return m_bSelLocked;
            }
            set
            {
                m_bSelLocked = value;
            }
        }
        [Category("Behavior"), Browsable(false)]
        public long SelPos
        {
            get 
            {
                // Only for debugging purposes !
                return m_iSelPos;
            }
            set
            {
            	m_iSelPos = value;
                if (m_iSelPos < m_iSelStart) 
                {
                	m_iSelPos = m_iSelStart;
                }
                else if (m_iSelPos > m_iSelEnd) 
                {
                	m_iSelPos = m_iSelEnd;
                }
                
                UpdateAppearence();
            }
        }
        public long SelTarget
        {
            get { return m_iSelTarget;}
        }
        [Category("Misc"), Browsable(true)]
        public string ToolTip
        {
        	get {return toolTips.GetToolTip(this);}
            set
            {
                toolTips.SetToolTip(SelectedZone, value);
                toolTips.SetToolTip(this, value);
            }
        }
        #endregion

        #region Members
        private long m_iMinimum = 0;
        private long m_iMaximum = 100;
        private long m_iSelStart = 0;
        private long m_iSelEnd = 100;
        private bool m_bSelLocked = false;
        private long m_iSelPos = 0;
        private long m_iSelTarget = 0;
        
        private int m_iMaxWidth = 0;        // Max size of selection in pixels
        private bool m_bEnabled = true;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events Delegates
        public delegate void SelectionChangingHandler(object sender, EventArgs e);
        public delegate void SelectionChangedHandler(object sender, EventArgs e);
        public delegate void TargetAcquiredHandler(object sender, EventArgs e);

        [Category("Action"), Browsable(true)]
        public event SelectionChangingHandler SelectionChanging;
        [Category("Action"), Browsable(true)]
        public event SelectionChangedHandler SelectionChanged;
        [Category("Action"), Browsable(true)]
        public event TargetAcquiredHandler TargetAcquired;
        #endregion

        #region Contructor
        public SelectionTracker()
        {
            InitializeComponent();
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - HandlerLeft.Width - HandlerRight.Width; 
        }
        #endregion

        #region Public Methods
        public void UpdateInternalState(long _iMin, long _iMax, long _iStart, long _iEnd, long _iPos)
        {
        	// This method is only a shortcut to updating all properties at once.
        	// It should be called when the internal state of data has been modified
        	// by other means than the user manipulating the control.
        	// (for example, at initialization or reset.)
        	// This method update the appearence of the control only, it doesn't raise the events back.
        	m_iMinimum = _iMin;
        	m_iMaximum = _iMax;
        	m_iSelStart = _iStart;
        	m_iSelEnd = _iEnd;
        	m_iSelPos = _iPos;
        	
        	UpdateAppearence();
        }
        public void UpdatePositionValueOnly(long _iPos)
        {
        	// This method does't refresh the control.
        	// This is useful when dealing with manually modifiying the selection,
        	// when we don't want the manipulation to cause another refresh.
        	m_iSelPos = _iPos;
        }
        public void Reset()
        {
        	UpdateInternalState(m_iMinimum, m_iMaximum, m_iMinimum, m_iMaximum, m_iMinimum);
        }
        public void EnableDisable(bool _bEnable)
		{
			m_bEnabled = _bEnable;
			HandlerLeft.Enabled = _bEnable;
			HandlerRight.Enabled = _bEnable;
		}
        #endregion
        
        #region Event Handlers - Handlers
        private void HandlerLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (!m_bSelLocked) && m_bEnabled)
            {
                int GlobalMouseX = e.X + HandlerLeft.Left;
                int OldHandlerLeft = HandlerLeft.Left;

                // Prevent going too far left/right
                if (((GlobalMouseX + (HandlerLeft.Width / 2)) <= HandlerRight.Left) &&
                     (GlobalMouseX - (HandlerLeft.Width / 2)  >= BumperLeft.Width))
                {
                    HandlerLeft.Left = GlobalMouseX - (HandlerLeft.Width / 2);
                    StretchSelection();

                    UpdateValuesAndReport();
                }
            }
        }
        private void HandlerRight_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && !m_bSelLocked && m_bEnabled)
            {
                // Déplacer le handler
                int GlobalMouseX = e.X + HandlerRight.Left;
                int OldHandlerLeft = HandlerRight.Left;

                // Empécher d'aller trop loin à droite et à gauche
                if ((GlobalMouseX + (HandlerRight.Width / 2) <= (this.Width - BumperRight.Width)) &&
                    (GlobalMouseX - (HandlerRight.Width / 2)) >= (HandlerLeft.Left + HandlerLeft.Width))
                {
                    HandlerRight.Left = GlobalMouseX - (HandlerRight.Width / 2);
                    StretchSelection();

                    UpdateValuesAndReport();
                }
            }
        
        }
        private void HandlerLeft_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && !m_bSelLocked && m_bEnabled)
            {
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void HandlerRight_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && !m_bSelLocked && m_bEnabled)
            {
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        #endregion
        
        #region Event Handlers - Bumpers and background
        private void BumperLeft_MouseDoubleClick(object sender, MouseEventArgs e)
        {
			// Double click on bumper : make the handler to jump here.
            if ((e.Button == MouseButtons.Left) && !m_bSelLocked && m_bEnabled)
            {
                HandlerLeft.Left = BumperLeft.Left + BumperLeft.Width;
                StretchSelection();

                UpdateValuesAndReport();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void BumperRight_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && !m_bSelLocked && m_bEnabled)
            {
                // Ramener le handler de doite à la fin.
                HandlerRight.Left = BumperRight.Left - HandlerRight.Width;
                StretchSelection();

                UpdateValuesAndReport();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void EndOfTrackLeft_DoubleClick(object sender, EventArgs e)
        {
        	// End of track is same as bumper.
            if (!m_bSelLocked && m_bEnabled)
            {
                HandlerLeft.Left = BumperLeft.Left + BumperLeft.Width;
                StretchSelection();

                UpdateValuesAndReport();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void EndOfTrackRight_DoubleClick(object sender, EventArgs e)
        {
        	// End of track is same as bumper.
            if (!m_bSelLocked && m_bEnabled)
            {
                HandlerRight.Left = BumperRight.Left - HandlerRight.Width;
                StretchSelection();

                UpdateValuesAndReport();
                if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
            }
        }
        private void SelectionTracker_DoubleClick(object sender, EventArgs e)
        {
            // Double click in background : make the closest handler to jump here.
            if (!m_bSelLocked && m_bEnabled)
            {
                Point MouseCoords = this.PointToClient(Cursor.Position);

                if ((MouseCoords.X > BumperLeft.Width + (HandlerLeft.Width / 2)) &&
                    (MouseCoords.X < this.Width - BumperRight.Width - (HandlerRight.Width / 2)))
                {

                    if (MouseCoords.X < HandlerLeft.Left)
                    {
                        HandlerLeft.Left = MouseCoords.X - (HandlerLeft.Width / 2);
                    }
                    else
                    {
                        HandlerRight.Left = MouseCoords.X - (HandlerRight.Width / 2);
                    }

                    StretchSelection();
                    UpdateValuesAndReport();
                    if (SelectionChanged != null) { SelectionChanged(this, EventArgs.Empty); }
                }
            }
        }
        #endregion
        
        #region Event Handlers - Middle part
        private void SelectedZone_MouseClick(object sender, MouseEventArgs e)
        {
            // Target selection.
            if (e.Button == MouseButtons.Left && m_bEnabled)
            {
                m_iSelTarget = Rescale(SelectedZone.Left + e.X - BumperLeft.Width - HandlerLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
                
                m_iSelPos = m_iSelTarget + m_iMinimum;
                UpdateAppearence();
                
                if (TargetAcquired != null) { TargetAcquired(this, EventArgs.Empty); }
            }
        }
        #endregion
		
        #region Event Handlers - Automatic
        private void SelectionTracker_Resize(object sender, EventArgs e)
        {
        	m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - HandlerLeft.Width - HandlerRight.Width; 
        	UpdateAppearence();
        }
        private void SelectedZone_Paint(object sender, PaintEventArgs e)
        {
            // Drawing playhead as hairline.
            if ((m_iSelEnd - m_iSelStart > 0) && (m_iSelPos >= m_iSelStart) && (m_iSelPos <= m_iSelEnd))
            {
                int head = Rescale(m_iSelPos - m_iSelStart, m_iSelEnd - m_iSelStart, SelectedZone.Width);

                if (head == SelectedZone.Width) { head = SelectedZone.Width - 1; }

                e.Graphics.DrawLine(Pens.Black, head, 4, head, SelectedZone.Height - 6);
            }
        }
        #endregion
        
        #region Binding UI and Data
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            // Rescale : Pixels -> Values
            return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
        }
        private void UpdateValuesAndReport()
        {
        	// One or both handlers have been modified BY USER ACTION.
        	// Update the internal values and raise the 'changing' event.
        	
            if (m_iSelEnd - m_iSelStart >= 0)
            {
                m_iSelStart = Rescale(SelectedZone.Left - BumperLeft.Width - HandlerLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum) + m_iMinimum;
                m_iSelEnd = Rescale(SelectedZone.Left - BumperLeft.Width - HandlerLeft.Width + SelectedZone.Width, m_iMaxWidth, m_iMaximum - m_iMinimum) + m_iMinimum;
            
                // Forcer quand même à rester à l'intérieur des bornes fixées au chargement du contrôle.
                // Certaines vidéos ont des frames inaccessibles au premier abord qui se révèlent ensuite...
                // On les ignore car cela perturbe grandement le système de numération des frames.

                if (m_iSelStart < m_iMinimum)
                    m_iSelStart = m_iMinimum;

                if (m_iSelEnd > m_iMaximum)
                    m_iSelEnd = m_iMaximum;

                // Raise the event.
                if (SelectionChanging != null) { SelectionChanging(this, EventArgs.Empty); }
            }
        }
        private void UpdateAppearence()
        {
        	// Internal state of data has been modified programmatically.
        	// (for example, at initialization or reset.)
        	// This method update the appearence of the control only, it doesn't raise the events back.
    	
            if (m_iMaximum - m_iMinimum > 0)
            {
                HandlerLeft.Left = BumperLeft.Width + Rescale(m_iSelStart - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                HandlerRight.Left = BumperLeft.Width + HandlerLeft.Width + Rescale(m_iSelEnd - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                
                StretchSelection();
                SelectedZone.Invalidate();
            }
    	
        }
        private void StretchSelection()
        {
        	// Called after the Handlers have been set, to update the middle part.
        	
            SelectedZone.Left = HandlerLeft.Left + HandlerLeft.Width;
            SelectedZone.Width = HandlerRight.Left - SelectedZone.Left;
        }
        #endregion
    }
}
