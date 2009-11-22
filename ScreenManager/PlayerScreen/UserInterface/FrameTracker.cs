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
	/// A control to let the user specify the current position in the video.
	/// The control is comprised of a cursor and a list of hairlines for keyframes
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
    public partial class FrameTracker : UserControl
    {
        #region Properties
        [Category("Behavior"), Browsable(true)]
        public long Minimum
        {
        	get{return m_iMinimum;}
            set
            {
                m_iMinimum = value;
                if (m_iPosition < m_iMinimum) m_iPosition = m_iMinimum;
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
                if (m_iPosition > m_iMaximum) m_iPosition = m_iMaximum;
                UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public long Position
        {
            get{return m_iPosition;}
            set
            {
            	m_iPosition  = value;
                if (m_iPosition < m_iMinimum) m_iPosition = m_iMinimum;
                if (m_iPosition > m_iMaximum) m_iPosition = m_iMaximum;
				UpdateAppearence();
            }
        }
        [Category("Behavior"), Browsable(true)]
        public bool ReportOnMouseMove
        {
            get { return m_bReportOnMouseMove;  }
            set { m_bReportOnMouseMove = value; }
        }
        #endregion
			
        #region Members
        private long m_iMinimum = 0;
        private long m_iPosition = 0;
        private long m_iMaximum = 0;
        private int m_iMaxWidth = 0;
        private bool m_bReportOnMouseMove = false;
        private long[] m_KeyframesTimestamps;
        private int[] m_KeyframesMarks;
        private long m_SyncPointTimestamp;
        private int m_SyncPointMark;
        private bool m_bEnabled = true;
       	private static readonly Pen m_PenKeyImageMark = new Pen(Color.FromArgb(192, Color.YellowGreen), 2); // YellowGreen
        private static readonly Pen m_PenSyncPointMark = new Pen(Color.FromArgb(192, Color.Firebrick), 2); // 
       	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events Delegates
        public delegate void PositionChangingHandler(object sender, long _iPosition);
        public delegate void PositionChangedHandler(object sender, long _iPosition);

        [Category("Action"), Browsable(true)]
        public event PositionChangingHandler PositionChanging;
        [Category("Action"), Browsable(true)]
        public event PositionChangedHandler PositionChanged;
        #endregion

        #region Constructor
        public FrameTracker()
        {
            InitializeComponent();
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - NavCursor.Width;
        }
		#endregion
		
		#region Public Methods
		public void UpdateKeyframesMarkers(long[] _markers)
		{
			if (m_KeyframesTimestamps == null)
            {
                m_KeyframesTimestamps = new long[_markers.Length];
            }
            else if (m_KeyframesTimestamps.Length != _markers.Length)
            {
                m_KeyframesTimestamps = new long[_markers.Length];
            }

            for (int i = 0; i < _markers.Length; i++)
            {
                m_KeyframesTimestamps[i] = _markers[i];
            }

            UpdateAppearence();
		}
		public void Remap(long _iMin, long _iMax)
        {
        	// This method is only a shortcut to updating min and max properties at once.
        	// This method update the appearence of the control only, it doesn't raise the events back.
        	m_iMinimum = _iMin;
        	m_iMaximum = _iMax;
        	
        	if (m_iPosition < m_iMinimum) m_iPosition = m_iMinimum;
        	if (m_iPosition > m_iMaximum) m_iPosition = m_iMaximum;
        	
        	UpdateAppearence();
        }
		public void UpdateSyncPointMarker(long _marker)
		{
			m_SyncPointTimestamp = _marker;
			UpdateAppearence();
		}
		public void EnableDisable(bool _bEnable)
		{
			m_bEnabled = _bEnable;
			NavCursor.Enabled = _bEnable;
		}
		#endregion
        
		#region Event Handlers - User Manipulation
        private void NavCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && m_bEnabled)
            {
                // Déplacer le curseur
                // GlobalMouseX correspond à la coordonnée dans le panelNavigation.
                int GlobalMouseX = e.X + NavCursor.Left;
                int OldCursorLeft = NavCursor.Left;

                // Empécher d'aller trop loin à droite et à gauche
                if ((GlobalMouseX < (this.Width - (NavCursor.Width / 2) - BumperRight.Width)) &&
                     (GlobalMouseX - (NavCursor.Width / 2) - BumperLeft.Width > 0))
                {
                    NavCursor.Left = GlobalMouseX - (NavCursor.Width / 2);

                    if (m_bReportOnMouseMove)
                    {
                    	long iPosition = m_iPosition;
                        m_iPosition = m_iMinimum + Rescale(NavCursor.Left - BumperLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
                        if (PositionChanging != null && m_iPosition != iPosition) 
                        { 
                        	PositionChanging(this, m_iPosition); 
                        }
                    }
                }
            }
        }
        private void FrameTracker_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && m_bEnabled)
            {
                Point MouseCoords = this.PointToClient(Cursor.Position);
                if ((MouseCoords.X > BumperLeft.Width + (NavCursor.Width / 2)) &&
                    (MouseCoords.X < this.Width - BumperRight.Width - (NavCursor.Width / 2)))
                {
                    NavCursor.Left = MouseCoords.X - (NavCursor.Width / 2);
                    UpdateValuesAndReport();
                }
            }
        }
        private void NavCursor_MouseUp(object sender, MouseEventArgs e)
        {
        	if(m_bEnabled)
        	{
            	UpdateValuesAndReport();
        	}
        }
        #endregion

        #region Event Handlers - Automatic
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
            m_iMaxWidth = this.Width - BumperLeft.Width - BumperRight.Width - NavCursor.Width;
            UpdateAppearence();
        }
        private void FrameTracker_Paint(object sender, PaintEventArgs e)
        {
            if (m_KeyframesMarks != null)
            {
                foreach (int mark in m_KeyframesMarks)
                {
                    if (mark > 0)
                    {
                        e.Graphics.DrawLine(m_PenKeyImageMark, mark, 2, mark, this.Height - 4);
                    }
                }
            }
            
            if(m_SyncPointMark > 0)
            {
            	e.Graphics.DrawLine(m_PenSyncPointMark, m_SyncPointMark, 2, m_SyncPointMark, this.Height - 4);
            }
        }
        #endregion
        
        #region Binding UI to Data
        private int Rescale(long _iOldValue, long _iOldMax, long _iNewMax)
        {
            // Rescale : Pixels -> Values
            if(_iOldMax > 0)
            {
            	return (int)(Math.Round((double)((double)_iOldValue * (double)_iNewMax) / (double)_iOldMax));
            }
            else
            {
            	return 0;
            }
        }
        private void UpdateValuesAndReport()
        {
        	long iPosition = m_iPosition;
            m_iPosition = m_iMinimum + Rescale(NavCursor.Left - BumperLeft.Width, m_iMaxWidth, m_iMaximum - m_iMinimum);
        	if (PositionChanged != null && m_iPosition != iPosition) 
        	{ 
        		PositionChanged(this, m_iPosition); 
        	}
        }
        private void UpdateAppearence()
        {
        	// Internal state of data has been modified programmatically.
        	// (for example, at initialization or reset.)
        	// This method update the appearence of the control only, it doesn't raise the events back.
    	
            if (m_iMaximum - m_iMinimum > 0)
            {
            	NavCursor.Left = BumperLeft.Width + Rescale(m_iPosition - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
            }

            UpdateMarkers();
        }
       	private void UpdateMarkers()
        {
       		// Key frames
            if (m_KeyframesTimestamps != null)
            {
                if (m_KeyframesMarks == null)
                {
                    m_KeyframesMarks = new int[m_KeyframesTimestamps.Length];
                }
                else if (m_KeyframesTimestamps.Length != m_KeyframesMarks.Length)
                {
                    // dispose ?
                    m_KeyframesMarks = new int[m_KeyframesTimestamps.Length];
                }

                // Assign new values.
                for (int i = 0; i < m_KeyframesTimestamps.Length; i++)
                {
                    m_KeyframesMarks[i] = (NavCursor.Width / 2) + BumperLeft.Width + Rescale(m_KeyframesTimestamps[i] - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
                }
            }
            
            // Sync point
            if(m_SyncPointTimestamp != 0)
            {
            	m_SyncPointMark = (NavCursor.Width / 2) + BumperLeft.Width + Rescale(m_SyncPointTimestamp - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);	
            }
            else
            {
            	m_SyncPointMark = 0;	
            }
        }
        #endregion
    }
}