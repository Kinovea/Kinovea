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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A control to let the user specify the current position in the video.
	/// The control is comprised of a cursor and a list of markers.
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
        private bool m_bEnabled = true;
       	
        // Markers
        private Metadata m_Metadata;
        
        private List<int> m_KeyframesMarks = new List<int>();			// In control coordinates.
        private static readonly Pen m_PenKeyBorder = new Pen(Color.FromArgb(255, Color.YellowGreen), 1);
        private static readonly Pen m_PenKeyInside = new Pen(Color.FromArgb(96, Color.YellowGreen), 1);
        
        private List<Point> m_ChronosMarks = new List<Point>();			// Start and end of chronos in control coords.
        private static readonly Pen m_PenChronoBorder = new Pen(Color.FromArgb(255, Color.CornflowerBlue), 1); // skyblue
        private static readonly SolidBrush m_BrushChrono = new SolidBrush(Color.FromArgb(96, Color.CornflowerBlue));
        
        private List<Point> m_TracksMarks = new List<Point>();			// Start and end of tracks in control coords.
        private static readonly Pen m_PenTrackBorder = new Pen(Color.FromArgb(255, Color.Plum), 1); // Plum;SandyBrown
        private static readonly SolidBrush m_BrushTrack = new SolidBrush(Color.FromArgb(96, Color.Plum));
        
        private long m_SyncPointTimestamp;
        private int m_SyncPointMark;
        private static readonly Pen m_PenSyncBorder = new Pen(Color.FromArgb(255, Color.Firebrick), 1);
        private static readonly Pen m_PenSyncInside = new Pen(Color.FromArgb(96, Color.Firebrick), 1);
       	
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
		public void EnableDisable(bool _bEnable)
		{
			m_bEnabled = _bEnable;
			NavCursor.Enabled = _bEnable;
		}
		public void UpdateMarkers(Metadata _metadata)
		{
			// Keep a ref on the Metadata object so we can update the
			// markers position when only the size of the control changes.
			
			m_Metadata = _metadata;
			UpdateAppearence();
		}
		public void UpdateSyncPointMarker(long _marker)
		{
			m_SyncPointTimestamp = _marker;
			UpdateAppearence();
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
        	// Draw the various markers within the frame tracker gutter.
        	if (m_KeyframesMarks.Count > 0)
            {
                foreach (int mark in m_KeyframesMarks)
                {
                    if (mark > 0)
                    {
                    	DrawMark(e.Graphics, m_PenKeyBorder, m_PenKeyInside, mark);
                    }
                }
            }
        	
        	if(m_ChronosMarks.Count > 0)
        	{
        		foreach (Point mark in m_ChronosMarks)
                {
                	DrawMark(e.Graphics, m_PenChronoBorder, m_BrushChrono, mark);
                }
        	}
        	
        	if(m_TracksMarks.Count > 0)
        	{
        		foreach (Point mark in m_TracksMarks)
                {
                	DrawMark(e.Graphics, m_PenTrackBorder, m_BrushTrack, mark);
                }
        	}
            
            if(m_SyncPointMark > 0)
            {
            	DrawMark(e.Graphics, m_PenSyncBorder, m_PenSyncInside, m_SyncPointMark);
            }
        }
        private void DrawMark(Graphics _canvas, Pen _pBorder, Pen _pInside, int _iCoord)
        {
        	// Mark for a single point in time (key image).
        	
			// Simple line.
        	//_canvas.DrawLine(_p, _iCoord, 2, _iCoord, this.Height - 4);
        	//_canvas.DrawLine(_p, _iCoord+1, 2, _iCoord+1, this.Height - 4);
        	
        	// Small rectangles (Eclipse style)
        	int iLeft = _iCoord-1;
        	int iTop = 5;
        	int iWidth = 3;
        	int iHeight = 8;
        	
			_canvas.DrawRectangle(_pBorder, iLeft, iTop, iWidth, iHeight );
			_canvas.DrawRectangle(_pInside, iLeft + 1, iTop+1, iWidth-2, iHeight-2 );
        }
        private void DrawMark(Graphics _canvas, Pen _pBorder, SolidBrush _bInside, Point _iCoords)
        {
        	// Mark for a range in time (chrono or tracks).
        	int iLeft = _iCoords.X;
        	int iTop = 5;
        	int iWidth = _iCoords.Y;
        	int iHeight = 8;
        	
			_canvas.DrawRectangle(_pBorder, iLeft, iTop, iWidth, iHeight );
			_canvas.FillRectangle(_bInside, iLeft + 1, iTop+1, iWidth-1, iHeight-1 );
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

            UpdateMarkersPositions();
        }
       	private void UpdateMarkersPositions()
        {
       		if(m_Metadata != null)
       		{
       			// Key frames
	       		m_KeyframesMarks.Clear();
	       		foreach(Keyframe kf in m_Metadata.Keyframes)
	       		{
	       			m_KeyframesMarks.Add(GetCoordFromTimestamp(kf.Position));
	       		}
	       		
	       		// Chronos
	       		m_ChronosMarks.Clear();
	       		foreach(DrawingChrono dc in m_Metadata.Chronos)
	       		{
	       			if(dc.TimeStart != long.MaxValue && dc.TimeStop != long.MaxValue)
					{
	       				// todo: currently doesn't support the chrono without end.
	       			
		       			// We will store the range coords in a Point object, to get a couple of ints structure.
		       			// X will be the left coordinate, Y the width.
		       			int start = GetCoordFromTimestamp(dc.TimeStart);
		       			int stop = GetCoordFromTimestamp(dc.TimeStop);
		       			Point p = new Point(start, stop - start);
	
		       			m_ChronosMarks.Add(p);
	       			}
	       		}
	       		
	       		// Tracks
	       		m_TracksMarks.Clear();
	       		foreach(Track t in m_Metadata.Tracks)
	       		{
	       			// We will store the range coords in a Point object, to get a couple of ints structure.
	       			// X will be the left coordinate, Y the width.
	       			int start = GetCoordFromTimestamp(t.BeginTimeStamp);
	       			int stop = GetCoordFromTimestamp(t.EndTimeStamp);
	       			Point p = new Point(start, stop - start);

		       		m_TracksMarks.Add(p);
	       		}
	       		
       		}
       		
            // Sync point
            m_SyncPointMark = 0;
            if(m_SyncPointTimestamp != 0)
            {
            	m_SyncPointMark = GetCoordFromTimestamp(m_SyncPointTimestamp);
            }
        }
       	private int GetCoordFromTimestamp(long _ts)
       	{
       		return (NavCursor.Width / 2) + BumperLeft.Width + Rescale(_ts - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);	
       	}
        #endregion
    }
}