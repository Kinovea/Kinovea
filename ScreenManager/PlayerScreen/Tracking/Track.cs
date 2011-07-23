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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public delegate void ShowClosestFrame(Point _mouse, long _iBeginTimestamp, List<AbstractTrackPoint> _positions, int _iPixelTotalDistance, bool _b2DOnly);
    
    /// <summary>
    /// A class to encapsulate track drawings.
    /// Contains the list of points and the list of keyframes markers.
    /// Handles the user actions, display modes and xml import/export.
    /// The tracking itself is delegated to a Tracker class.
    /// 
    /// The trajectory can be in one of 3 views (complete traj, focused on a section, label).
    /// And in one of two status (edit or interactive).
    /// In Edit state: dragging the target moves the point's coordinates.
    /// In Interactive state: dragging the target moves to the next point (in time).
    /// </summary>
    public class Track : AbstractDrawing, IDecorable
    {
    	#region Enums
        public enum TrackView
        {
            Complete,
            Focus,
            Label
        }
        public enum TrackStatus
        {
        	Edit,
        	Interactive
        }
        public enum TrackExtraData
        {
        	None,
        	TotalDistance,
        	Speed,
        	Acceleration
        }
		#endregion
        
        #region Delegates
        // To ask the UI to display the frame closest to selected pos.
        // used when moving the target in direct interactive mode.
        public ShowClosestFrame m_ShowClosestFrame;     
        #endregion

        #region Properties
        public TrackView View
        {
            get { return m_TrackView; }
            set { m_TrackView = value; }
        }
        public TrackStatus Status
        {
            get { return m_TrackStatus; }
            set { m_TrackStatus = value; }
        }
        public TrackExtraData ExtraData
        {
            get { return m_TrackExtraData; }
            set 
            { 
            	m_TrackExtraData = value; 
            	IntegrateKeyframes();
            }
        }
        public long BeginTimeStamp
        {
            get { return m_iBeginTimeStamp; }
        }
        public long EndTimeStamp
        {
            get { return m_iEndTimeStamp; }
        }
        public DrawingStyle DrawingStyle
        {
        	get { return m_Style;}
        }
        public Color MainColor
        {    
        	get { return m_StyleHelper.Color; }
        	set 
        	{ 
        		m_StyleHelper.Color = value;
        		m_MainLabel.TextDecoration.Update(value);
        	}
        }
        public string Label
        {
            get { return m_MainLabelText; }
            set { m_MainLabelText = value;}
        }
        public Metadata ParentMetadata
        {
            get { return m_ParentMetadata; }    // unused.
            set 
            { 
            	m_ParentMetadata = value; 
            	m_InfosFading.AverageTimeStampsPerFrame = m_ParentMetadata.AverageTimeStampsPerFrame;
            }
        }
        public bool Untrackable
		{
			get { return m_bUntrackable; }
		}
		// Fading is not modifiable from outside.
        public override InfosFading  infosFading
        {
            get { throw new Exception("Track, The method or operation is not implemented."); }
            set { throw new Exception("Track, The method or operation is not implemented."); }
        }
        public override Capabilities Caps
		{
			get { return Capabilities.None; }
		}
        public override List<ToolStripMenuItem> ContextMenu
		{
			get { return null; }
		}
        #endregion

        #region Members
        
        // Current state.
        private TrackView m_TrackView = TrackView.Complete;
        private TrackStatus m_TrackStatus = TrackStatus.Edit;
        private TrackExtraData m_TrackExtraData = TrackExtraData.None;
        private int m_iMovingHandler = -1;
        	
        // Tracker tool.
        private AbstractTracker m_Tracker;
        private bool m_bUntrackable;
        
        // Rendering coordinates
        private double m_fStretchFactor = 1.0;
        private Point m_DirectZoomTopLeft = new Point(0, 0);
        
        // Hardwired parameters.
        private static readonly int m_iDefaultCrossRadius = 4;
        private static readonly int m_iAllowedFramesOver = 12;  	// Number of frames over which the global fading spans (after end point).
        private static readonly int m_iFocusFadingFrames = 30;		// Number of frames of the focus section. 
       
        // Internal data.
        private List<AbstractTrackPoint> m_Positions = new List<AbstractTrackPoint>();
        private List<Point> m_RescaledPositions = new List<Point>();
        private List<KeyframeLabel> m_KeyframesLabels = new List<KeyframeLabel>();
		
        private long m_iBeginTimeStamp;     			// absolute.
        private long m_iEndTimeStamp = long.MaxValue; 	// absolute.
        private int m_iTotalDistance;       			// This is used to normalize timestamps to a par scale with distances.
        private int m_iCurrentPoint;

        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private KeyframeLabel m_MainLabel = new KeyframeLabel(Color.Black);
        private string m_MainLabelText = "Label";
        private InfosFading m_InfosFading = new InfosFading(long.MaxValue, 1);
        private static readonly int m_iBaseAlpha = 224;				// alpha of track in most cases.
        private static readonly int m_iAfterCurrentAlpha = 64;		// alpha of track after the current point when in normal mode.
        private static readonly int m_iEditModeAlpha = 128;			// alpha of track when in Edit mode.
        private static readonly int m_iLabelFollowsTrackAlpha = 80;	// alpha of track when in LabelFollows view.
        
        // Memorization poul
        private TrackView m_MemoTrackView;
        private string m_MemoLabel;
        private Metadata m_ParentMetadata;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Track(int _x, int _y, long _t, Bitmap _CurrentImage, Size _imageSize)
        {
            //-----------------------------------------------------------------------------------------
            // t is absolute time.
            // _bmp is the whole picture, if null it means we don't need it.
            // (Probably because we already have a few points already that we are importing from xml.
            // In this case we'll only need the last frame to reconstruct the last point.)
			//-----------------------------------------------------------------------------------------
            
            // Create the first point
            if (_CurrentImage != null)
            {
            	// Init tracker.
            	m_Tracker = new TrackerBlock2(_CurrentImage.Width, _CurrentImage.Height);
            	
           		AbstractTrackPoint atp = m_Tracker.CreateTrackPoint(true, _x, _y, 1.0f, 0, _CurrentImage, m_Positions);
           		if(atp != null)
           		{
           			m_Positions.Add(atp);
           		}
           		else
           		{
           			// TODO:
           			// Error message the user so he can choose another spot to track.
           			m_bUntrackable = true;
           		}
            }
            else
            {
            	// Happens when loading Metadata from file or demuxing.
            	m_Tracker = new TrackerBlock2(_imageSize.Width, _imageSize.Height);
            	m_Positions.Add(m_Tracker.CreateOrphanTrackPoint(_x, _y, 0));
            }

            if(!m_bUntrackable)
            {
            	m_RescaledPositions.Add(RescalePosition(m_Positions[0], m_fStretchFactor, m_DirectZoomTopLeft));
	            m_iBeginTimeStamp = _t;
	            m_iEndTimeStamp = m_iBeginTimeStamp;
	            m_MainLabel.MoveTo(m_Positions[0].ToPoint());
	            
	            // We use the InfosFading utility to fade the track away.
	            // The refererence frame will be the last point (at which fading start).
	            // AverageTimeStampsPerFrame will be updated when we get the parent metadata.
	            m_InfosFading.FadingFrames = m_iAllowedFramesOver;
	            m_InfosFading.UseDefault = false;
	            m_InfosFading.Enabled = true;
	            
	            // Computed
	            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            }
            
            // Decoration
            m_Style = new DrawingStyle();
            m_Style.Elements.Add("color", new StyleElementColor(Color.SeaGreen));
			m_Style.Elements.Add("line size", new StyleElementLineSize(3));
			m_Style.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));
            m_StyleHelper.Color = Color.Black;
            m_StyleHelper.LineSize = 3;
            m_StyleHelper.TrackShape = TrackShape.Dash;
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "line size");
            m_Style.Bind(m_StyleHelper, "TrackShape", "track shape");
        }
        #endregion

        #region AbstractDrawing implementation
		public override void Draw(Graphics _canvas, double _fStretchFactor, bool _bSelected, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
		{
            if (_iCurrentTimestamp >= m_iBeginTimeStamp)
            {
                // 0. Compute the fading factor. 
                // Special case from other drawings:
                // ref frame is last point, and we only fade after it, not before.
                double fOpacityFactor = 1.0;
                if (m_TrackStatus == TrackStatus.Interactive && _iCurrentTimestamp > m_iEndTimeStamp)
                {
                	m_InfosFading.ReferenceTimestamp = m_iEndTimeStamp;
                	fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
                }

                // 1. Find the closest position in the list.
                m_iCurrentPoint = FindClosestPoint(_iCurrentTimestamp);

                // 2. Rescale.
                if (_fStretchFactor != m_fStretchFactor || _DirectZoomTopLeft.X != m_DirectZoomTopLeft.X || _DirectZoomTopLeft.Y != m_DirectZoomTopLeft.Y)
                {
                    m_fStretchFactor = _fStretchFactor;
                    m_DirectZoomTopLeft = new Point(_DirectZoomTopLeft.X, _DirectZoomTopLeft.Y);
                    RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
                }

                // 3. Boundaries of visibility. 
                // First and last if complete traj, bounded otherwise.
                // Note: in edit mode, we also force the bounds otherwise the tracking perf is impaired by drawing time.
                int iStart = 0;
                if((m_TrackView != TrackView.Complete || m_TrackStatus == TrackStatus.Edit) && m_iCurrentPoint - m_iFocusFadingFrames > 0)
            	{
            		iStart = m_iCurrentPoint - m_iFocusFadingFrames;
            	}
                
            	int iEnd = m_RescaledPositions.Count - 1;
            	if((m_TrackView != TrackView.Complete || m_TrackStatus == TrackStatus.Edit) && m_iCurrentPoint + m_iFocusFadingFrames < m_RescaledPositions.Count - 1)
            	{
            		iEnd = m_iCurrentPoint + m_iFocusFadingFrames;
            	}
        
            	// 4. Draw various elements depending on combination of view and status.
            	// The exact alpha at which the traj will be drawn will be decided in GetTrackPen().
            	if(m_RescaledPositions.Count > 1)
            	{
	            	// Key Images titles.
	            	if ((m_TrackStatus == TrackStatus.Interactive) &&	
	            	    (m_TrackView != TrackView.Label) )
	            	{
	            		DrawKeyframesTitles(_canvas, fOpacityFactor);	
	            	}
	            	
	            	// Track.
	            	if ( (m_TrackStatus == TrackStatus.Interactive) && (m_TrackView == TrackView.Complete))
	            	{
	            		DrawTrajectory(_canvas, iStart, m_iCurrentPoint, true, fOpacityFactor);	
	            		DrawTrajectory(_canvas, m_iCurrentPoint, iEnd, false, fOpacityFactor);
	            	}
	            	else
	            	{
	            		DrawTrajectory(_canvas, iStart, iEnd, false, fOpacityFactor);	
	            	}
	            	
	            	// ExtraData (distance, speed) on main label.
	            	if(m_TrackStatus == TrackStatus.Interactive && 
	            	   m_TrackView != TrackView.Label && 
	            	   m_TrackExtraData != TrackExtraData.None)
	            	{
	            		DrawMainLabel(_canvas, m_iCurrentPoint, fOpacityFactor);
	            	}
            	}
            	
            	if(m_RescaledPositions.Count > 0)
            	{
	            	// Target marker.
	            	if( fOpacityFactor == 1.0 && m_TrackView != TrackView.Label)
                    {
	            		DrawMarker(_canvas, fOpacityFactor);
	            	}
	            	
	            	// Tracking algorithm visualization.
                    if ((m_TrackStatus == TrackStatus.Edit) && (fOpacityFactor == 1.0))
                    {
                        m_Tracker.Draw(_canvas, m_Positions[m_iCurrentPoint], _DirectZoomTopLeft, m_fStretchFactor, m_StyleHelper.Color, fOpacityFactor);
					}
                    
                    // Main label.
                    if ((m_TrackStatus == TrackStatus.Interactive) && 
                        (m_TrackView == TrackView.Label))
                    {
                    	DrawMainLabel(_canvas, m_iCurrentPoint, fOpacityFactor);
                    }
            	}
            }
        }
		public override void MoveDrawing(int _deltaX, int _deltaY, Keys _ModifierKeys)
		{
			if (m_TrackStatus == TrackStatus.Edit)
            {
				if(m_iMovingHandler == 1)
				{
	                // Update cursor label.
	                // Image will be reseted at mouse up. (=> UpdateTrackPoint)
	                m_Positions[m_iCurrentPoint].X += _deltaX;
	                m_Positions[m_iCurrentPoint].Y += _deltaY;
	                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
				}
            }
			else
            {
				if(m_iMovingHandler > 1)
				{
					// Update coords label.
					MoveLabelTo(_deltaX, _deltaY, m_iMovingHandler);
				}
            }
		}
		public override void MoveHandle(Point point, int handleNumber)
		{
			// We come here when moving the target or moving along the trajectory,
			// and in interactive mode (change current frame).
			if(m_TrackStatus == TrackStatus.Interactive && (handleNumber == 0 || handleNumber == 1))
			{
				MoveCursor(point.X, point.Y);
			}
		}
		public override int HitTest(Point _point, long _iCurrentTimestamp)
        {
            //---------------------------------------------------------
            // Result: 
            // -1 = miss, 0 = on traj, 1 = on Cursor, 2 = on main label, 3+ = on keyframe label.
            // _point is mouse coordinates already descaled.
            //---------------------------------------------------------
            int iHitResult = -1;

            if (_iCurrentTimestamp >= m_iBeginTimeStamp && _iCurrentTimestamp <= m_iEndTimeStamp)
            {
                // We give priority to labels in case a label is on the trajectory,
                // we need to be able to move it around.
                // If label attch mode, this will tell if we are on the label.
                if (m_TrackStatus == TrackStatus.Interactive)
                {
                    iHitResult = IsOnKeyframesLabels(_point);
                }

                if (iHitResult == -1)
                {
                	Rectangle rectangleTarget;
                	if(m_TrackStatus == TrackStatus.Edit)
                	{
                		rectangleTarget = m_Tracker.GetEditRectangle(m_Positions[m_iCurrentPoint].ToPoint());
                	}
                	else
                	{
                		int widen = 3;
                    	rectangleTarget = new Rectangle(m_Positions[m_iCurrentPoint].X - m_iDefaultCrossRadius - widen, m_Positions[m_iCurrentPoint].Y - m_iDefaultCrossRadius - widen, (m_iDefaultCrossRadius+widen) * 2, (m_iDefaultCrossRadius+widen) * 2);
                	}
                    
                    if (rectangleTarget.Contains(_point))
                    {
                        iHitResult = 1;
                    }
                    else
                    {
                        // TODO: investigate why this might crash sometimes.
                        try
                        {
                        	int iStart = GetFirstVisiblePoint();
                        	int iEnd = GetLastVisiblePoint();
			                
                        	// Create path which contains wide line for easy mouse selection
                            int iTotalVisiblePoints = iEnd - iStart;
                        	Point[] points = new Point[iTotalVisiblePoints];
                            for (int i = 0; i < iTotalVisiblePoints; i++)
                            {
                                points[i] = new Point(m_Positions[i+iStart].X, m_Positions[i+iStart].Y);
                            }

                            GraphicsPath areaPath = new GraphicsPath();
                            areaPath.AddLines(points);
                            Pen tempPen = new Pen(Color.Black, 12);
                            areaPath.Widen(tempPen);
                            tempPen.Dispose();
                            
                            // Create region from the path
                            Region areaRegion = new Region(areaPath);

                            if (areaRegion.IsVisible(_point))
                            {
                                iHitResult = 0;
                            }
                        }
                        catch (Exception exp)
                        {
                            iHitResult = -1;
                            log.Error("Error while hit testing track.");
                            log.Error("Exception thrown : " + exp.GetType().ToString() + " in " + exp.Source.ToString() + exp.TargetSite.Name.ToString());
        					log.Error("Message : " + exp.Message.ToString());
        					Exception inner = exp.InnerException;
			     			while( inner != null )
			     			{
			          			log.Error("Inner exception : " + inner.Message.ToString());
			          			inner = inner.InnerException;
			     			}
                        }
                    }
                }
            }
            
            if(iHitResult == 0 && m_TrackStatus == TrackStatus.Interactive)
            {
            	// Instantly jump to the frame.
            	MoveCursor(_point.X, _point.Y);
            }

            m_iMovingHandler = iHitResult;
            
            return iHitResult;
        }
       #endregion
        
        #region Drawing routines
        private void DrawTrajectory(Graphics _canvas, int _start, int _end, bool _before, double _fFadingFactor)
        {
        	// Points are drawn with various alpha values, possibly 0:
        	// In edit mode, all segments are drawn at 64 alpha.
        	// In normal mode, segments before the current point are drawn at 224, segments after at 64.
        	// In focus mode, (edit or normal) only a subset of segments are drawn from each part.
        	// It is not possible currently to make the curve vary smoothly in alpha.
        	// Either we make it vary in alpha for each segment but draw as connected lines.
        	// or draw as curve but at the same alpha for all.
        	// All segments are drawn at 224, even the after section.
        	
        	Point[] points = new Point[_end - _start + 1];
            for (int i = 0; i <= _end - _start; i++)
            {
                points[i] = new Point(m_RescaledPositions[_start + i].X, m_RescaledPositions[_start + i].Y);
            }
            
            if (points.Length > 1)
            {
            	// Tension parameter is at 0.5f for bezier effect (smooth curve).
            	Pen trackPen = GetTrackPen(m_TrackStatus, _fFadingFactor, _before);
            	_canvas.DrawCurve(trackPen, points, 0.5f);
            	
            	if(m_StyleHelper.TrackShape.ShowSteps)
            	{
            		Pen stepPen = new Pen(trackPen.Color, 2);
	            	int margin = (int)(trackPen.Width * 1.5);
	            	int diameter = margin *2;
	            	foreach(Point p in points)
	            	{
	            		_canvas.DrawEllipse(stepPen, p.X - margin, p.Y - margin, diameter, diameter);
	            	}
	            	stepPen.Dispose();
            	}
            	
            	trackPen.Dispose();
            }
        }
        private void DrawMarker(Graphics _canvas,  double _fFadingFactor)
        { 
        	int radius = m_iDefaultCrossRadius;
        	
        	if(m_TrackStatus == TrackStatus.Edit)
        	{
        		// Just a little cross.
        		Pen p = new Pen(Color.FromArgb((int)(255.0f * _fFadingFactor), m_StyleHelper.Color));
        		_canvas.DrawLine(p, m_RescaledPositions[m_iCurrentPoint].X, m_RescaledPositions[m_iCurrentPoint].Y - radius, 
        		                	m_RescaledPositions[m_iCurrentPoint].X, m_RescaledPositions[m_iCurrentPoint].Y + radius);

        		_canvas.DrawLine(p, m_RescaledPositions[m_iCurrentPoint].X - radius, m_RescaledPositions[m_iCurrentPoint].Y, 
        		                	m_RescaledPositions[m_iCurrentPoint].X + radius, m_RescaledPositions[m_iCurrentPoint].Y);
        	
        		p.Dispose();
        	}
        	else
        	{
	    		// Draws the target marker (CrashTest Dummy style target)
	    		int markerAlpha = 255;
	    		Brush brushBlack = new SolidBrush(Color.FromArgb(markerAlpha, 0,0,0));
	    		Brush brushWhite = new SolidBrush(Color.FromArgb(markerAlpha, 255, 255, 255));                              
	    		
	            _canvas.FillPie(brushBlack, (float)m_RescaledPositions[m_iCurrentPoint].X - radius , (float)m_RescaledPositions[m_iCurrentPoint].Y - radius , (float)radius  * 2, (float)radius  * 2, 0, 90);
	            _canvas.FillPie(brushWhite, (float)m_RescaledPositions[m_iCurrentPoint].X - radius , (float)m_RescaledPositions[m_iCurrentPoint].Y - radius , (float)radius  * 2, (float)radius  * 2, 90, 90);
	            _canvas.FillPie(brushBlack, (float)m_RescaledPositions[m_iCurrentPoint].X - radius , (float)m_RescaledPositions[m_iCurrentPoint].Y - radius , (float)radius  * 2, (float)radius  * 2, 180, 90);
	            _canvas.FillPie(brushWhite, (float)m_RescaledPositions[m_iCurrentPoint].X - radius , (float)m_RescaledPositions[m_iCurrentPoint].Y - radius , (float)radius  * 2, (float)radius  * 2, 270, 90);
        	
	            brushBlack.Dispose();
	            brushWhite.Dispose();
	            
	        	// Contour
	            int ContourRadius = radius  + 2;            
	            _canvas.DrawEllipse(Pens.White, m_RescaledPositions[m_iCurrentPoint].X - ContourRadius, m_RescaledPositions[m_iCurrentPoint].Y - ContourRadius, ContourRadius * 2, ContourRadius * 2);
        	}
            
        	
        }
        private void DrawKeyframesTitles(Graphics _canvas, double _fFadingFactor)
        {
            //------------------------------------------------------------
            // Draw the Keyframes labels
            // Each Label has its own coords and is movable.
            // Each label is connected to the TrackPosition point.
            // Rescaling for the current image size has already been done.
            //------------------------------------------------------------
            if (_fFadingFactor >= 0)
            {
                foreach (KeyframeLabel kl in m_KeyframesLabels)
                {
                	// In focus mode, only show labels that are in focus section.
                	if(m_TrackView == TrackView.Complete ||
                	   m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T + m_iBeginTimeStamp, kl.Timestamp, m_iFocusFadingFrames)
                	  )
                	{
                    	kl.Draw(_canvas, m_fStretchFactor, m_DirectZoomTopLeft, _fFadingFactor);
                	}
                }
            }
        }
        private void DrawMainLabel(Graphics _canvas, int _iCurrentPoint, double _fFadingFactor)
        {
            // Draw the main label and its connector to the current point.
            if (_fFadingFactor == 1.0f)
            {
                m_MainLabel.MoveTo(m_Positions[_iCurrentPoint].ToPoint());
                
                if(m_TrackView == TrackView.Label)
                {
                	m_MainLabel.Text = m_MainLabelText;
                }
                else
                {
                	m_MainLabel.Text = GetExtraDataText(_iCurrentPoint);
                }
                
                m_MainLabel.Draw(_canvas, m_fStretchFactor, m_DirectZoomTopLeft, _fFadingFactor);
            }
        }
        private Pen GetTrackPen(TrackStatus _status, double _fFadingFactor, bool _before)
        {
        	int iAlpha = 0;
        	
        	if(_status == TrackStatus.Edit)
        	{
        		iAlpha = m_iEditModeAlpha;
        	}
        	else 
            {
        		if(m_TrackView == TrackView.Complete)
        		{
        			if(_before)
        			{
        				iAlpha = (int)((double)m_iBaseAlpha * _fFadingFactor);
        			}
        			else
        			{
        				iAlpha = m_iAfterCurrentAlpha;
        			}
        		}
        		else if(m_TrackView == TrackView.Focus)
        		{
        			iAlpha = (int)((double)m_iBaseAlpha * _fFadingFactor);		
        		}
        		else if(m_TrackView == TrackView.Label)
        		{
        			iAlpha = (int)((double)m_iLabelFollowsTrackAlpha * _fFadingFactor);
        		}
            }
        	
            return m_StyleHelper.GetPen(iAlpha, 1.0);
        }
        #endregion

        #region Extra informations (Speed, distance)
		private string GetExtraDataText(int _index)
        {
        	string displayText = "";
            switch(m_TrackExtraData)
            {
            	case TrackExtraData.TotalDistance:
            		displayText = GetDistanceText(0, _index);
            		break;
            	case TrackExtraData.Speed:
            		displayText = GetSpeedText(_index - 1, _index);
            		break;
            	case TrackExtraData.Acceleration:
            		// Todo. GetAccelerationText();
            		break;
            	case TrackExtraData.None:
            		// keyframe title ?
            		break;
            }	
            return displayText;
        }
        private string GetDistanceText(int _p1, int _p2)
        {
        	// return the distance between two tracked points.
        	// Todo: currently it just return the distance between the points.
        	// We would like to get the distance between all points inside the range defined by the points.

        	string dist = "";
        	if(m_Positions.Count > 0)
        	{
        		if( _p1 >= 0 && _p1 < m_Positions.Count &&
        	   		_p2 >= 0 && _p2 < m_Positions.Count )
	        	{
        			double fPixelDistance = 0;
        			for(int i = _p1; i < _p2; i++)
        			{
        				fPixelDistance += CalibrationHelper.PixelDistance(m_Positions[i].ToPoint(), m_Positions[i+1].ToPoint());
        			}
        			
	        		dist = m_ParentMetadata.CalibrationHelper.GetLengthText(fPixelDistance);
	        	}
	        	else
	        	{
	        		// return 0.
	        		dist = m_ParentMetadata.CalibrationHelper.GetLengthText(0);	
	        	}
        	}

        	return dist;
        }
        private string GetSpeedText(int _p1, int _p2)
        {
        	// return the instant speed at p2.
			// (that is the distance between p1 and p2 divided by the time to get from p1 to p2).
			// p2 needs to be after p1.
			
        	string speed = "";
        	
        	if(m_Positions.Count > 0)
        	{
        		int iFrames = _p2 - _p1;
        	
        		if( _p1 >= 0 && _p1 < m_Positions.Count-1 &&
        	   		_p2 > _p1 && _p2 < m_Positions.Count )
	        	{
	        		speed = m_ParentMetadata.CalibrationHelper.GetSpeedText(m_Positions[_p1].ToPoint(), m_Positions[_p2].ToPoint(), iFrames);
	        	}
	        	else
	        	{
	        		// not computable, return 0.
	        		speed = m_ParentMetadata.CalibrationHelper.GetSpeedText(m_Positions[0].ToPoint(), m_Positions[0].ToPoint(), 0);	
	        	}
        	}

        	return speed;
        }
		#endregion
    
        #region User manipulation
        private void MoveCursor(int _X, int _Y)
        {
            if (m_TrackStatus == TrackStatus.Edit)
            {
                // Move cursor to new coords
                // In this case, _X and _Y are delta values.
                // Image will be reseted at mouse up. (=> UpdateTrackPoint)
                m_Positions[m_iCurrentPoint].X += _X;
                m_Positions[m_iCurrentPoint].Y += _Y;
                RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            }
            else
            {
                //----------------------------------------
                // Move Playhead to closest frame (x,y,t).
                //----------------------------------------
                // In this case, _X and _Y are absolute values.
                if (m_ShowClosestFrame != null && m_Positions.Count > 1)
                {
                    m_ShowClosestFrame(new Point(_X, _Y), m_iBeginTimeStamp, m_Positions, m_iTotalDistance, false);
                }
            }
        }
        private void MoveLabelTo(int _deltaX, int _deltaY, int _iLabelNumber)
        {
        	// _iLabelNumber coding: 2 = main label, 3+ = keyframes labels.
        	
        	if (m_TrackStatus == TrackStatus.Edit || m_TrackView != TrackView.Label)
            {
        		if(m_TrackExtraData != TrackExtraData.None && _iLabelNumber == 2)
        		{
        			// Move the main label.
        			m_MainLabel.MoveLabel(_deltaX, _deltaY);
        		}
        		else
        		{
        			// Move the specified label by specified amount.    
	                int iLabel = _iLabelNumber - 3;
	                m_KeyframesLabels[iLabel].MoveLabel(_deltaX, _deltaY);
        		}
            }
            else if (m_TrackView == TrackView.Label)
            {
            	m_MainLabel.MoveLabel(_deltaX, _deltaY);
            }
        }
        private int IsOnKeyframesLabels(Point _point)
        {
        	// Result: 
            // -1 = miss, 0 = on traj, 1 = on Cursor, 2 = on main label, 3+ = on keyframe label.
            
            int iHitResult = -1;

            if (m_TrackView == TrackView.Label)
            {
                if (m_MainLabel.HitTest(_point))
                {
                    iHitResult = 2;
                }
            }
            else
            {
            	// Even when we aren't in TrackView.Label, the main label is visible
            	// if we are displaying the extra data (distance, speed).
            	if (m_TrackExtraData != TrackExtraData.None)
	            {
	                if (m_MainLabel.HitTest(_point))
	                {
	                    iHitResult = 2;
	                }
	            }	
            	
                for (int i = 0; i < m_KeyframesLabels.Count; i++)
                {
                	if(m_TrackView == TrackView.Complete ||
                	   m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T + m_iBeginTimeStamp, m_KeyframesLabels[i].Timestamp, m_iFocusFadingFrames)
                	  )
                	{
                		if (m_KeyframesLabels[i].HitTest(_point))
	                    {
	                        iHitResult = i + 3;
	                        break;
	                    }
                	}
                }
            }

            return iHitResult;
        }
        private int GetFirstVisiblePoint()
        {
        	// Boundaries of visibility.
            // First and last if complete traj, bounded otherwise.
            
            int iStart = 0;
            
            if(m_TrackView != TrackView.Complete && m_iCurrentPoint - m_iFocusFadingFrames > 0)
        	{
        		iStart = m_iCurrentPoint - m_iFocusFadingFrames;
        	}
            
            return iStart;
        }
        private int GetLastVisiblePoint()
        {
        	// Boundaries of visibility.
            // First and last if complete traj, bounded otherwise.
            int iEnd = m_RescaledPositions.Count - 1;
        	if(m_TrackView != TrackView.Complete && m_iCurrentPoint + m_iFocusFadingFrames < m_RescaledPositions.Count - 1)
        	{
        		iEnd = m_iCurrentPoint + m_iFocusFadingFrames;
        	}
            
            return iEnd;
        }
        #endregion
        
        #region Context Menu implementation
        public void ChopTrajectory(long _iCurrentTimestamp)
        {
        	// Delete end of track.
            m_iCurrentPoint = FindClosestPoint(_iCurrentTimestamp);

            if (m_iCurrentPoint < m_Positions.Count - 1)
            {
                m_Positions.RemoveRange(m_iCurrentPoint + 1, m_Positions.Count - m_iCurrentPoint - 1);
                m_RescaledPositions.RemoveRange(m_iCurrentPoint + 1, m_RescaledPositions.Count - m_iCurrentPoint - 1);
            }

            m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;
            
            // Todo: we must now refill the last point with a patch image.
        }
        public List<AbstractTrackPoint> GetEndOfTrack(long _iTimestamp)
        {
        	// Called from CommandDeleteEndOfTrack,
        	// We need to keep the old values in case the command is undone.
        	List<AbstractTrackPoint> endOfTrack = new List<AbstractTrackPoint>();
        	foreach (AbstractTrackPoint trkpos in m_Positions)
            {
                if (trkpos.T >= _iTimestamp - m_iBeginTimeStamp)
                {
                    //endOfTrack.Add(new AbstractTrackPoint(trkpos.X, trkpos.Y, trkpos.T));
                    //endOfTrack.Add(trkpos.Clone());
                    endOfTrack.Add(trkpos);
                }
            }
        	return endOfTrack;
        }
        public void AppendPoints(long _iCurrentTimestamp, List<AbstractTrackPoint> _ChoppedPoints)
        {
            // Called when undoing CommandDeleteEndOfTrack,
            // revival of the discarded points.
            if (_ChoppedPoints.Count > 0)
            {
                // Find the append insertion point.
                // Some points may have been re added already and we don't want to mix the two lists.
                int iMatchedPoint = m_Positions.Count - 1;
                while (m_Positions[iMatchedPoint].T >= _ChoppedPoints[0].T && iMatchedPoint > 0)
                {
                    iMatchedPoint--;
                }

                // Remove uneeded points.
                if (iMatchedPoint < m_Positions.Count - 1)
                {
                    m_Positions.RemoveRange(iMatchedPoint + 1, m_Positions.Count - (iMatchedPoint+1));
                    m_RescaledPositions.RemoveRange(iMatchedPoint + 1, m_RescaledPositions.Count - (iMatchedPoint+1));
                }

                // Append
                foreach (AbstractTrackPoint trkpos in _ChoppedPoints)
                {
                    //AbstractTrackPoint append = new AbstractTrackPoint(trkpos.X, trkpos.Y, trkpos.T, trkpos.Image);
                    //m_Positions.Add(append);
                    m_Positions.Add(trkpos);
                    m_RescaledPositions.Add(trkpos.ToPoint());
                }

                m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;
            }
        }
        public void StopTracking()
        {
            m_TrackStatus = TrackStatus.Interactive;
        }
		public void RestartTracking()
        {
            m_TrackStatus = TrackStatus.Edit;
        }
        #endregion
        
        #region Tracking
        public void TrackCurrentPosition(long _iCurrentTimestamp, Bitmap _bmpCurrent)
        {
            // Matches previous point in current image.
            // New points to trajectories are always created from here, 
            // the user can only moves existing points.
            
            // A new point needs to be added if we are after the last existing one.
            // Note: the UI will force stop the tracking if the user jumps to more than
            // one frame ahead of the last registered point.
            if (_iCurrentTimestamp > m_iBeginTimeStamp + m_Positions[m_Positions.Count - 1].T)
            {
            	AbstractTrackPoint p;
            	bool bMatched = m_Tracker.Track(m_Positions, _bmpCurrent, _iCurrentTimestamp - m_iBeginTimeStamp, out p);
                
            	// We add it to the list even if matching failed (but we'll stop tracking then).
            	if(p != null)
            	{
					m_Positions.Add(p);
					m_RescaledPositions.Add(RescalePosition(p, m_fStretchFactor, m_DirectZoomTopLeft));
	
					if (!bMatched)
	                    StopTracking();
					
	            	// Adjust internal data.
					m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;
		            ComputeFlatDistance();
		            IntegrateKeyframes();
            	}
            	else
            	{
            		// Untrackable point. Error message the user.
            		StopTracking();
            	}
            }
        }
		private void ComputeFlatDistance()
        {
        	// This distance is used to normalize distance vs time in interactive manipulation.
			
			int iSmallestTop = int.MaxValue;
            int iSmallestLeft = int.MaxValue;
            int iHighestBottom = -1;
            int iHighestRight = -1;

            for (int i = 0; i < m_Positions.Count; i++)
            {
                if (m_Positions[i].X < iSmallestLeft)
                    iSmallestLeft = m_Positions[i].X;

                if (m_Positions[i].X > iHighestRight)
                    iHighestRight = m_Positions[i].X;

                if (m_Positions[i].Y < iSmallestTop)
                    iSmallestTop = m_Positions[i].Y;
                
                if (m_Positions[i].Y > iHighestBottom)
                    iHighestBottom = m_Positions[i].Y;
            }

            m_iTotalDistance = (int)Math.Sqrt(((iHighestRight - iSmallestLeft) * (iHighestRight - iSmallestLeft))
                                       + ((iHighestBottom - iSmallestTop) * (iHighestBottom - iSmallestTop)));
        }
        public void UpdateTrackPoint(Bitmap _CurrentImage)
        {
        	// The user moved a point that had been previously placed.
        	// We need to reconstruct tracking data stored in the point, for later tracking.
        	// The coordinate of the point have already been updated during the mouse move.
            if (m_Positions.Count > 1 && m_iCurrentPoint >= 0)
            {
            	m_Positions[m_iCurrentPoint].ResetTrackData();
            	AbstractTrackPoint atp = m_Tracker.CreateTrackPoint(true, m_Positions[m_iCurrentPoint].X, m_Positions[m_iCurrentPoint].Y, 1.0f, 
            	                           							m_Positions[m_iCurrentPoint].T,  _CurrentImage, m_Positions);
            	
            	if(atp != null)
            	{
            		m_Positions[m_iCurrentPoint] = atp;
            	}
            	else
            	{
					// TODO.
            		// Error message to the user, so he can choose another spot to track.
            	}
            	
            	// Update the mini label (attach, position of label, and text).
            	for (int i = 0; i < m_KeyframesLabels.Count; i++)
            	{
            		if(m_KeyframesLabels[i].Timestamp == m_Positions[m_iCurrentPoint].T + m_iBeginTimeStamp)
            		{
            			m_KeyframesLabels[i].MoveTo(m_Positions[m_iCurrentPoint].ToPoint());
						if(m_TrackExtraData != TrackExtraData.None)
            			{
            			 	m_KeyframesLabels[i].Text = GetExtraDataText(m_KeyframesLabels[i].AttachIndex);
            			}
            		}
            	}
            }
        }
		#endregion
        
        #region XML import/export
        public void ToXmlString(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("Track");

            _xmlWriter.WriteStartElement("TimePosition");
            _xmlWriter.WriteString(m_iBeginTimeStamp.ToString());
            _xmlWriter.WriteEndElement();

            _xmlWriter.WriteStartElement("Mode");
            _xmlWriter.WriteString(((int)m_TrackView).ToString());
            _xmlWriter.WriteEndElement();

            TrackPointsToXml(_xmlWriter);
            TrackLineToXml(_xmlWriter);
            KeyframesLabelsToXml(_xmlWriter);

            _xmlWriter.WriteStartElement("ExtraData");
            _xmlWriter.WriteString(((int)m_TrackExtraData).ToString());
            _xmlWriter.WriteEndElement();
            
            // Global Label
            _xmlWriter.WriteStartElement("Label");
            _xmlWriter.WriteStartElement("Text");
            _xmlWriter.WriteString(m_MainLabelText);
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();

            // </Track>
            _xmlWriter.WriteEndElement();
        }
        public static Track FromXml(XmlTextReader _xmlReader, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback, Size _imageSize)
        {
            Track trk = new Track(0,0,0, null, _imageSize);
            trk.m_TrackStatus = TrackStatus.Interactive;
            
            if (_remapTimestampCallback != null)
            {
                while (_xmlReader.Read())
                {
                    if (_xmlReader.IsStartElement())
                    {
                        if (_xmlReader.Name == "TimePosition")
                        {
                            trk.m_iBeginTimeStamp = _remapTimestampCallback(long.Parse(_xmlReader.ReadString()), false);
                        }
                        else if (_xmlReader.Name == "Mode")
                        {
                            trk.m_TrackView = (TrackView)int.Parse(_xmlReader.ReadString());
                        }
                        else if (_xmlReader.Name == "TrackPositionList")
                        {
                            trk.ParseTrackPositionList(_xmlReader, trk, _scale, _remapTimestampCallback);
                        }
                        else if (_xmlReader.Name == "TrackLine")
                        {
                            trk.ParseTrackLine(_xmlReader, trk);
                        }
                        else if (_xmlReader.Name == "MainLabel")
                        {
                            trk.ParseMainLabel(_xmlReader, trk);
                        }
                        else if (_xmlReader.Name == "KeyframeLabelList")
                        {
                            trk.ParseKeyframeLabelList(_xmlReader, trk, _scale, _remapTimestampCallback);
                        }
                        else if (_xmlReader.Name == "ExtraData")
                        {
                            trk.m_TrackExtraData = (TrackExtraData)int.Parse(_xmlReader.ReadString());
                        }
                        else if (_xmlReader.Name == "Label")
                        {
                            trk.ParseLabel(_xmlReader, trk);
                        }
                        else
                        {
                            // forward compatibility : ignore new fields.
                        }
                    }
                    else if (_xmlReader.Name == "Track")
                    {
                        break;
                    }
                    else
                    {
                        // Fermeture d'un tag interne.
                    }
                }

                if (trk.m_Positions.Count > 0)
                {
                    trk.m_iEndTimeStamp = trk.m_Positions[trk.m_Positions.Count - 1].T + trk.m_iBeginTimeStamp;
                    trk.m_MainLabel.AttachLocation = trk.m_Positions[0].ToPoint();
                    trk.m_MainLabel.Text = trk.Label;
                }
                trk.RescaleCoordinates(trk.m_fStretchFactor, trk.m_DirectZoomTopLeft);
            }
            
            return trk;
        }
        private void ParseTrackLine(XmlTextReader _xmlReader, Track _track)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    /*if (_xmlReader.Name == "LineStyle")
                    {
                    	_track.m_LineStyle = LineStyle.FromXml(_xmlReader);
                    }*/
                }
                else if (_xmlReader.Name == "TrackLine")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseTrackPositionList(XmlTextReader _xmlReader, Track _track, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
        {
            _track.m_Positions.Clear();
            _track.m_RescaledPositions.Clear();

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "TrackPosition")
                    {
                    	// We don't know the concrete tracker that was used to match the points.
                        AbstractTrackPoint tp = m_Tracker.CreateOrphanTrackPoint(0, 0, 0);
                        tp.FromXml(_xmlReader);
						
                        // time was stored in relative value, we still need to adjust it.
                        AbstractTrackPoint adapted = m_Tracker.CreateOrphanTrackPoint(	
                                                                 	(int)((float)tp.X * _scale.X),
                                                                	(int)((float)tp.Y * _scale.Y),
                                                                	_remapTimestampCallback(tp.T, true));

                        _track.m_Positions.Add(adapted);
                        _track.m_RescaledPositions.Add(adapted.ToPoint()); // (not really scaled but must be added anyway so both array stay parallel.)
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "TrackPositionList")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseMainLabel(XmlTextReader _xmlReader, Track _track)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "KeyframeLabel")
                    {
                        _track.m_MainLabel = KeyframeLabel.FromXml(_xmlReader, true, new PointF(1.0f, 1.0f));
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "MainLabel")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void ParseKeyframeLabelList(XmlTextReader _xmlReader, Track _track, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
        {
            _track.m_KeyframesLabels.Clear();

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "KeyframeLabel")
                    {
                        KeyframeLabel kfl = KeyframeLabel.FromXml(_xmlReader, false, _scale);

                        if (kfl != null && _track.m_Positions.Count > 0)
                        {
                            // Match with TrackPositions previously found.
                            int iMatchedTrackPosition = FindClosestPoint(kfl.Timestamp, _track.m_Positions, _track.m_iBeginTimeStamp);
                            kfl.AttachIndex = iMatchedTrackPosition;
                            
                            kfl.AttachLocation = _track.m_Positions[iMatchedTrackPosition].ToPoint();
                            m_KeyframesLabels.Add(kfl);
                        }
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "KeyframeLabelList")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }  
        }
        private void ParseLabel(XmlTextReader _xmlReader, Track _track)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Text")
                    {
                        _track.m_MainLabelText = _xmlReader.ReadString();
                    }
                }
                else if (_xmlReader.Name == "Label")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }
        }
        private void TrackLineToXml(XmlTextWriter _xmlWriter)
        {
            //_xmlWriter.WriteStartElement("TrackLine");
           
            //m_LineStyle.ToXml(_xmlWriter);

            // </trackline>
            //_xmlWriter.WriteEndElement();
        }
        private void TrackPointsToXml(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("TrackPositionList");
            _xmlWriter.WriteAttributeString("Count", m_Positions.Count.ToString());
            
            _xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.CalibrationHelper.GetLengthAbbreviation());
            // todo: user unit time.
            
            // The coordinate system defaults to the first point,
            // but can be specified by user.
            Point coordOrigin;
            if(m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.X >= 0 || m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.Y >= 0)
            {
            	coordOrigin = m_ParentMetadata.CalibrationHelper.CoordinatesOrigin;
            }
			else 
			{
				coordOrigin = new Point(m_Positions[0].X, m_Positions[0].Y);
			}
            
            if(m_Positions.Count > 0)
            {
            	foreach (AbstractTrackPoint tp in m_Positions)
            	{
            		tp.ToXml(_xmlWriter, m_ParentMetadata, coordOrigin);
            	}	
            }
            _xmlWriter.WriteEndElement();
        }
        private void KeyframesLabelsToXml(XmlTextWriter _xmlWriter)
        {
            // 1. Main label
            _xmlWriter.WriteStartElement("MainLabel");
            m_MainLabel.ToXml(_xmlWriter, m_iBeginTimeStamp);
            _xmlWriter.WriteEndElement();

            // 2. Keyframes labels.
            if (m_KeyframesLabels.Count > 0)
            {
                _xmlWriter.WriteStartElement("KeyframeLabelList");
                _xmlWriter.WriteAttributeString("Count", m_KeyframesLabels.Count.ToString());

                foreach (KeyframeLabel kfl in m_KeyframesLabels)
                {
                    kfl.ToXml(_xmlWriter, m_iBeginTimeStamp);
                }

                _xmlWriter.WriteEndElement();
            }
        }
        #endregion
        
        #region Miscellaneous public methods
		public void IntegrateKeyframes()
        {
            //-----------------------------------------------------------------------------------
            // The Keyframes list changed (add/remove/comments)
            // Reconstruct the Keyframes Labels, but don't completely reset those we already have
            // (Keep custom coordinates)
            //-----------------------------------------------------------------------------------

            // Keep track of matched keyframes so we can remove the others.
            bool[] matched = new bool[m_KeyframesLabels.Count];

            // Filter out key images that are not in the trajectory boundaries.
            for (int i = 0; i < m_ParentMetadata.Count; i++)
            {
                // Strictly superior because we don't show the keyframe that was created when the
                // user added the CrossMarker drawing to make the Track out of it.
                if (m_ParentMetadata[i].Position > m_iBeginTimeStamp && m_ParentMetadata[i].Position <= (m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp))
                {
                    // The Keyframe is within the Trajectory interval.
                    // Do we know it already ?
                    int iKnown = - 1;
                    for(int j=0;j<m_KeyframesLabels.Count;j++)
                    {
                    	if (m_KeyframesLabels[j].Timestamp == m_ParentMetadata[i].Position)
                        {
                            iKnown = j;
                            matched[j] = true;
                            break;
                        }
                    }
                    
                    if (iKnown >= 0)
                    {
                        // Known Keyframe, Read text again in case it changed
                        m_KeyframesLabels[iKnown].Text = m_ParentMetadata[i].Title;
                    }
                    else
                    {
                        // Unknown Keyframe, Configure and add it to list.
                        KeyframeLabel kfl = new KeyframeLabel(Color.Black);
                        kfl.AttachIndex = FindClosestPoint(m_ParentMetadata[i].Position);
                        kfl.MoveTo(m_Positions[kfl.AttachIndex].ToPoint());
                        kfl.Timestamp = m_Positions[kfl.AttachIndex].T + m_iBeginTimeStamp;                        
                        kfl.Text = m_ParentMetadata[i].Title;
                        
                        m_KeyframesLabels.Add(kfl);
                    }
                }
            }

            // Remove unused Keyframes.
            // We only look in the original list and remove in reverse order so the index aren't messed up.
            for (int iLabel = matched.Length - 1; iLabel >= 0; iLabel--)
            {
                if (matched[iLabel] == false)
                {
                    m_KeyframesLabels.RemoveAt(iLabel);
                }
            }
            
            // Reinject the labels in the list for extra data.
            if(m_TrackExtraData != TrackExtraData.None)
            {
	            for( int iKfl = 0; iKfl < m_KeyframesLabels.Count; iKfl++)
	            {
	            	m_KeyframesLabels[iKfl].Text = GetExtraDataText(m_KeyframesLabels[iKfl].AttachIndex);
	            }	
            }
            
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = 0;
            iHash ^= m_TrackView.GetHashCode();
            foreach (AbstractTrackPoint p in m_Positions)
            {
                iHash ^= p.GetHashCode();
            }

            iHash ^= m_iDefaultCrossRadius.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            iHash ^= m_MainLabel.GetHashCode();

            foreach (KeyframeLabel kfl in m_KeyframesLabels)
            {
                iHash ^= kfl.GetHashCode();
            }

            return iHash;
        }
        public void MemorizeState()
        {
        	// Used by formConfigureTrajectory to be able to modify the trajectory in real time.
        	m_MemoTrackView = m_TrackView;
        	m_MemoLabel = m_MainLabel.Text;
        }
        public void RecallState()
        {
        	// Used when the user cancels his modifications on formConfigureTrajectory.
        	m_MainLabel.TextDecoration.Update(m_StyleHelper.Color);
        	m_TrackView = m_MemoTrackView;
        	m_MainLabel.Text = m_MemoLabel;
        }
        #endregion
		
		#region Miscellaneous private methods
        private Point RescalePosition(AbstractTrackPoint _position, double _fStretchFactor, Point _DirectZoomTopLeft)
        {
        	// Extract the 2D coordinates and rescale.
			// Todo: use the CoordinateSystem helper class?
        	return new Point(	(int)((double)(_position.X - _DirectZoomTopLeft.X) * _fStretchFactor),
            					(int)((double)(_position.Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            // Trajectory Points
            for (int i = 0; i < m_Positions.Count; i++)
            {
               m_RescaledPositions[i] = new Point(	(int)((double)(m_Positions[i].X - _DirectZoomTopLeft.X) * _fStretchFactor),
                                                  	(int)((double)(m_Positions[i].Y - _DirectZoomTopLeft.Y) * _fStretchFactor));
            }
        }
        private int FindClosestPoint(long _iCurrentTimestamp)
        {
            return FindClosestPoint(_iCurrentTimestamp, m_Positions, m_iBeginTimeStamp);
        }
        private int FindClosestPoint(long _iCurrentTimestamp, List<AbstractTrackPoint> _Positions, long _iBeginTimestamp)
        {
            // Find the closest registered timestamp
            // Parameter is given in absolute timestamp.
            long minErr = long.MaxValue;
            int iClosest = 0;

            for (int i = 0; i < _Positions.Count; i++)
            {
                long err = Math.Abs((long)((_Positions[i].T + _iBeginTimestamp) - _iCurrentTimestamp));
                if (err < minErr)
                {
                    minErr = err;
                    iClosest = i;
                }
            }

            return iClosest;
        }
        #endregion
    }
}
