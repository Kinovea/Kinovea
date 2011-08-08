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


using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.ScreenManager.Properties;

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
                UpdateMarkersPositions();
                UpdateCursorPosition();
                Invalidate();
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
                UpdateMarkersPositions();
                UpdateCursorPosition();
                Invalidate();
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
				UpdateCursorPosition();
				Invalidate();
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
        private bool m_bInvalidateAsked;	// To prevent reentry in MouseMove before the paint event has been honored.	
        private long m_iMinimum;			// In absolute timestamps.
        private long m_iPosition;			// In absolute timestamps.
        private long m_iMaximum;			// In absolute timestamps.
        
        private int m_iMaxWidth;			// Number of pixels in the control that can be used for position.
        private int m_iMinimumPixel;
        private int m_iMaximumPixel;
        private int m_iPixelPosition;		// Left of the cursor in pixels.
        
        private int m_iCursorWidth = Resources.liqcursor.Width;
        private int m_iSpacers = 10;
        
        private bool m_bReportOnMouseMove = false;
        private bool m_bEnabled = true;
        private Bitmap bmpNavCursor = Resources.liqcursor;
        private Bitmap bmpBumperLeft = Resources.liqbumperleft;
       	private Bitmap bmpBumperRight = Resources.liqbumperright;
       	private Bitmap bmpBackground = Resources.liqbackdock;
       	
        #region Markers handling
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
       	#endregion
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events Delegates
        [Category("Action"), Browsable(true)]
        public event EventHandler<PositionChangedEventArgs> PositionChanging;
        [Category("Action"), Browsable(true)]
        public event EventHandler<PositionChangedEventArgs> PositionChanged;
        #endregion

        #region Constructor
        public FrameTracker()
        {
            InitializeComponent();
            
            // Activates double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            
            this.Cursor = Cursors.Hand;
            
            m_iMinimumPixel = m_iSpacers + (m_iCursorWidth/2);
            m_iMaximumPixel = this.Width - m_iSpacers - (m_iCursorWidth/2);
            m_iMaxWidth = m_iMaximumPixel - m_iMinimumPixel;
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
        	
        	UpdateMarkersPositions();
        	UpdateCursorPosition();
        	Invalidate();
        }
		public void EnableDisable(bool _bEnable)
		{
			m_bEnabled = _bEnable;
			Invalidate();
		}
		public void UpdateMarkers(Metadata _metadata)
		{
			// Keep a ref on the Metadata object so we can update the
			// markers position when only the size of the control changes.
			
			m_Metadata = _metadata;
			UpdateMarkersPositions();
			Invalidate();
		}
		public void UpdateSyncPointMarker(long _marker)
		{
			m_SyncPointTimestamp = _marker;
			UpdateMarkersPositions();
			Invalidate();
		}
		#endregion
        
		#region Event Handlers - User Manipulation
        private void FrameTracker_MouseMove(object sender, MouseEventArgs e)
        {
        	// Note: also raised on mouse down.
        	// User wants to jump to position. Update the cursor and optionnaly the image.
        	if(m_bEnabled && !m_bInvalidateAsked)
        	{
	        	if (e.Button == MouseButtons.Left)
	            {
	        		Point mouseCoords = this.PointToClient(Cursor.Position);
	        		
	        		if ((mouseCoords.X > m_iMinimumPixel) && (mouseCoords.X < m_iMaximumPixel))
	                {
	        			m_iPixelPosition = mouseCoords.X - (m_iCursorWidth/2);
					    Invalidate();
					    m_bInvalidateAsked = true;
					    
					    if (m_bReportOnMouseMove && PositionChanging != null)
	                    {
	        				m_iPosition = GetTimestampFromCoord(m_iPixelPosition + (m_iCursorWidth/2));
	        				PositionChanging(this, new PositionChangedEventArgs(m_iPosition));
	                    }
	        			else
	        			{
	        				Invalidate();
	        			}
	        		}
	        	}
        	}
        }
        private void FrameTracker_MouseUp(object sender, MouseEventArgs e)
        {
        	// End of a mouse move, jump to position.
        	if(m_bEnabled)
        	{
	        	if (e.Button == MouseButtons.Left)
	            {
	                Point mouseCoords = this.PointToClient(Cursor.Position);
	                
	                if ((mouseCoords.X > m_iMinimumPixel) && (mouseCoords.X < m_iMaximumPixel))
	                {
	                    m_iPixelPosition = mouseCoords.X - (m_iCursorWidth/2);
			            Invalidate();
	                    if (PositionChanged != null)
			        	{ 
			            	m_iPosition = GetTimestampFromCoord(m_iPixelPosition + (m_iCursorWidth/2));
			            	PositionChanged(this, new PositionChangedEventArgs(m_iPosition));
			        	}
	                }
	            }
        	}
        }
        private void FrameTracker_Resize(object sender, EventArgs e)
        {
        	// Resize of the control only : internal data doesn't change.
        	m_iMaximumPixel = this.Width - m_iSpacers - (m_iCursorWidth/2);
            m_iMaxWidth = m_iMaximumPixel - m_iMinimumPixel;
            UpdateMarkersPositions();
            UpdateCursorPosition();
            Invalidate();
        }
        #endregion

        #region Painting
        private void FrameTracker_Paint(object sender, PaintEventArgs e)
        {
        	// When we land in this function, m_iPixelPosition should have been set already.
        	// It is the only member variable we'll use here.
        	
        	// Draw tiled background
        	for(int i=10;i<Width-20;i+=bmpBackground.Width)
        	{
        		e.Graphics.DrawImage(bmpBackground, i, 0);
        	}
        	
        	// Draw slider ends.
        	e.Graphics.DrawImage(bmpBumperLeft, 10, 0);
        	e.Graphics.DrawImage(bmpBumperRight, Width-20, 0);
        	
        	if(m_bEnabled)
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
	            
	            // Draw the cursor.
	            e.Graphics.DrawImage(bmpNavCursor, m_iPixelPosition, 0);
        	}
        	
        	m_bInvalidateAsked = false;
        }
        private void DrawMark(Graphics _canvas, Pen _pBorder, Pen _pInside, int _iCoord)
        {
        	// Mark for a single point in time (key image).
        	
			// Simple line.
        	//_canvas.DrawLine(_p, _iCoord, 2, _iCoord, this.Height - 4);
        	//_canvas.DrawLine(_p, _iCoord+1, 2, _iCoord+1, this.Height - 4);
        	
        	// Small rectangles.
        	int iLeft = _iCoord;
        	int iTop = 5;
        	int iWidth = 3;
        	int iHeight = 8;
        	
			_canvas.DrawRectangle(_pBorder, iLeft, iTop, iWidth, iHeight );
			_canvas.DrawRectangle(_pInside, iLeft + 1, iTop+1, iWidth-2, iHeight-2 );
        }
        private void DrawMark(Graphics _canvas, Pen _pBorder, SolidBrush _bInside, Point _iCoords)
        {
        	// Mark for a range in time (chrono or track).
        	int iLeft = _iCoords.X;
        	int iTop = 5;
        	int iWidth = _iCoords.Y;
        	int iHeight = 8;
        	
        	// Bound to bumpers.
        	if(iLeft < m_iMinimumPixel) iLeft = m_iMinimumPixel;
			if(iLeft + iWidth > m_iMaximumPixel) iWidth = m_iMaximumPixel - iLeft;
        	
			_canvas.DrawRectangle(_pBorder, iLeft, iTop, iWidth, iHeight );
			_canvas.FillRectangle(_bInside, iLeft + 1, iTop+1, iWidth-1, iHeight-1 );
        }
        #endregion
        
        #region Binding UI to Data
        private void UpdateCursorPosition()
        {
        	// This method updates the appearence of the control only, it doesn't raise the events back.
        	// Should be called every time m_iPosition has been updated. 
            m_iPixelPosition = GetCoordFromTimestamp(m_iPosition) - (m_iCursorWidth/2);
        }
       	private void UpdateMarkersPositions()
        {
       		// Translate timestamps into control coordinates and store the coordinates of the
       		// markers to draw them later.
       		// Should only be called when either the timestamps or the control size changed.
       		if(m_Metadata != null)
       		{
       			// Key frames
	       		m_KeyframesMarks.Clear();
	       		foreach(Keyframe kf in m_Metadata.Keyframes)
	       		{
	       			// Only display Key image that are in the selection.
	       			if(kf.Position >= m_iMinimum && kf.Position <= m_iMaximum)
	       			{
	       				m_KeyframesMarks.Add(GetCoordFromTimestamp(kf.Position));
	       			}
	       		}
	       		
	       		// ExtraDrawings
	       		// We will store the range coords in a Point object, to get a couple of ints structure.
		        // X will be the left coordinate, Y the width.
	       		m_ChronosMarks.Clear();
	       		m_TracksMarks.Clear();
	       		foreach(AbstractDrawing ad in m_Metadata.ExtraDrawings)
	       		{
					DrawingChrono dc = ad as DrawingChrono;
					Track trk = ad as Track;
					
					if(dc != null)
					{
						if(dc.TimeStart != long.MaxValue && dc.TimeStop != long.MaxValue)
						{
							// todo: currently doesn't support the chrono without end.
		       			 	// Only display chronometers that have at least something in the selection.
			       			if(dc.TimeStart <= m_iMaximum && dc.TimeStop >= m_iMinimum)
		       				{
			       				long startTs = Math.Max(dc.TimeStart, m_iMinimum);
		       					long stopTs = Math.Min(dc.TimeStop, m_iMaximum);
		       				
				       			int start = GetCoordFromTimestamp(startTs);
				       			int stop = GetCoordFromTimestamp(stopTs);
				       			
				       			Point p = new Point(start, stop - start);
			
				       			m_ChronosMarks.Add(p);
							}	
						}
	       			}
					else if(trk != null)
					{
		       			if(trk.BeginTimeStamp <= m_iMaximum && trk.EndTimeStamp >= m_iMinimum)
		       			{
		       				long startTs = Math.Max(trk.BeginTimeStamp, m_iMinimum);
		       				long stopTs = Math.Min(trk.EndTimeStamp, m_iMaximum);
		       				
			       			int start = GetCoordFromTimestamp(startTs);
			       			int stop = GetCoordFromTimestamp(stopTs);
			       			
			       			Point p = new Point(start, stop - start);
		
				       		m_TracksMarks.Add(p);
		       			}	
					}
	       		}
       		}
       		
            // Sync point
            m_SyncPointMark = 0;
            if(m_SyncPointTimestamp != 0 && m_SyncPointTimestamp >= m_iMinimum && m_SyncPointTimestamp <= m_iMaximum)
            {
            	m_SyncPointMark = GetCoordFromTimestamp(m_SyncPointTimestamp);
            }
        }
       	private int GetCoordFromTimestamp(long _ts)
       	{
			int iret = m_iMinimumPixel + Rescale(_ts - m_iMinimum, m_iMaximum - m_iMinimum, m_iMaxWidth);
            return iret;
       	}
       	private long GetTimestampFromCoord(int _pos)
       	{
       		long ret = m_iMinimum + Rescale(_pos - m_iMinimumPixel, m_iMaxWidth, m_iMaximum - m_iMinimum);
       		return ret;
       	}
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
        #endregion
    }
    
    public class PositionChangedEventArgs : EventArgs
    {
        public readonly long Position;
        public PositionChangedEventArgs(long _position)
        {
            Position = _position;
        }
    }
}