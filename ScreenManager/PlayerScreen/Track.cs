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
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using Kinovea.Services;
using AForge.Imaging;
using System.Drawing.Imaging;

namespace Kinovea.ScreenManager
{
    public delegate void ShowClosestFrame(Point _mouse, long _iBeginTimestamp, List<TrackPosition> _positions, int _iPixelTotalDistance, bool _b2DOnly);
    
    /// <summary>
    /// A class to encapsulate tracks and tracking.
    /// Contains the list of points and the list of keyframes markers.
    /// Handles the user actions, patch matching through AForge, display modes and xml import/export.
    /// 
    /// The trajectory can be in one of 3 views (complete traj, focused on a section, label).
    /// And in one of two status (edit or interactive).
    /// Edit: dragging the target moves the point's coordinates.
    /// Interactive: dragging the target moves to the next point (in time).
    /// </summary>
    public class Track
    {
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
        public long BeginTimeStamp
        {
            get { return m_iBeginTimeStamp; }
        }
        public long EndTimeStamp
        {
            get { return m_iEndTimeStamp; }
        }
        public Color MainColor
        {    
        	get { return m_LineStyle.Color; }
        	set 
        	{ 
        		m_LineStyle.Update(value);
        		m_MainLabel.TextDecoration.Update(m_LineStyle.Color);
        	}
        }
        public LineStyle TrajectoryStyle
        {
        	// Consider modifying this so we don't expose a reference.
        	// (encapsulation hole)
            get { return m_LineStyle; }
            set 
            { 
				// This is used to update the line shape and not its color.
				// Hence we don't update m_MainLabel here.
				m_LineStyle.Update(value, false, true, true);
            }
        }
        public string Label
        {
            get { return m_MainLabel.Text; }
            set 
            { 
                m_MainLabel.Text = value;
                m_MainLabel.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
            }
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
        #endregion

        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private static readonly int m_iDefaultCrossRadius = 4;
        private static readonly int m_iArrowWidth = 6;
        private static readonly int m_iArrowDistance = 10;
        private static readonly int m_iArrowLength = 25;
        private static readonly int m_iTemplateEdge = 20;				// The size of the patch of frame we look for in other frames.
        private static readonly int m_iSearchExpansionFactor = 5;		// The window we will look into.
        private static readonly float m_fSearchTreshold = 0.80f;		// If the best match is less than this, we don't consider it's a match.
        private static readonly int m_iAllowedFramesOver = 12;  		// Number of frames over which the global fading spans (after end point).
        private static readonly int m_iFocusFadingFrames = 30;		// Number of frames of the focus section. 
        private static readonly int m_iBaseAlpha = 224;				// alpha of track in most cases.
        private static readonly int m_iAfterCurrentAlpha = 64;		// alpha of track after the current point when in normal mode.
        private static readonly int m_iEditModeAlpha = 128;			// alpha of track when in Edit mode.
        private static readonly int m_iLabelFollowsTrackAlpha = 80;	// alpha of track when in LabelFollows view.
        
        
        private TrackView m_TrackView = TrackView.Complete;
        private TrackStatus m_TrackStatus = TrackStatus.Edit;
        
        private double m_fStretchFactor = 1.0;
        private Point m_DirectZoomTopLeft = new Point(0, 0);
        private List<TrackPosition> m_Positions = new List<TrackPosition>();
        private List<TrackPosition> m_RescaledPositions = new List<TrackPosition>();
        private List<KeyframeLabel> m_KeyframesLabels = new List<KeyframeLabel>();
        
        private ExhaustiveTemplateMatching m_TemplateMatcher = new ExhaustiveTemplateMatching(m_fSearchTreshold);
        private Size m_TemplateSize = new Size(m_iTemplateEdge, m_iTemplateEdge);
        private Size m_SearchSize = new Size(m_iTemplateEdge*m_iSearchExpansionFactor, m_iTemplateEdge*m_iSearchExpansionFactor);

        private long m_iBeginTimeStamp;     			// absolute.
        private long m_iEndTimeStamp = long.MaxValue; // absolute.
        private int m_iTotalDistance;       			// This is used to normalize timestamps to a par scale with distances.
        private int m_iCurrentPoint;

        // Decoration
        private LineStyle m_LineStyle = LineStyle.DefaultValue; 
        private KeyframeLabel m_MainLabel = new KeyframeLabel(true, Color.Black);
        private InfosFading m_InfosFading = new InfosFading(long.MaxValue, 1);
        
        // Memorization poul
        private TrackView m_MemoTrackView;
        private LineStyle m_MemoLineStyle;
        private string m_MemoLabel;
        private Metadata m_ParentMetadata;
        #endregion

        #region Constructor
        public Track(int _x, int _y, long _t, Bitmap _bmp)
        {
            //-------------------------------------------------------------
            // t is absolute time.
            // _bmp is the whole picture, if null it means we don't need it 
            // because we already have the points templates.
            //-------------------------------------------------------------
            
            // Create the first point
            if (_bmp != null)
            {
                // copy template zone from source image.
                Bitmap tpl = new Bitmap(m_TemplateSize.Width, m_TemplateSize.Height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(tpl);
                Rectangle rDst = new Rectangle(0, 0, m_TemplateSize.Width, m_TemplateSize.Height);
                Rectangle rSrc = new Rectangle(_x - (m_TemplateSize.Width / 2), _y - (m_TemplateSize.Height / 2), m_TemplateSize.Width, m_TemplateSize.Height);
                g.DrawImage(_bmp, rDst, rSrc, GraphicsUnit.Pixel);
                m_Positions.Add(new TrackPosition(_x, _y, 0, tpl));
            }
            else
            {
                m_Positions.Add(new TrackPosition(_x, _y, 0));
            }

            m_RescaledPositions.Add(RescalePosition(m_Positions[0], m_fStretchFactor, m_DirectZoomTopLeft));
            m_iBeginTimeStamp = _t;
            m_MainLabel.TrackPos = m_Positions[0];
            
            // We use the InfosFading utility to fade the track away.
            // The refererence frame will be the last point (at which fading start).
            // AverageTimeStampsPerFrame will be updated when we get the parent metadata.
            // Ref frame must be updated each time last point change.
            m_InfosFading.FadingFrames = m_iAllowedFramesOver;
            m_InfosFading.UseDefault = false;
            
            // Computed
            RescaleCoordinates(m_fStretchFactor, m_DirectZoomTopLeft);
            m_MainLabel.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        #endregion

        #region Public Interface
        public void Draw(Graphics _canvas, double _fStretchFactor, long _iCurrentTimestamp, Point _DirectZoomTopLeft)
        {
            if (_iCurrentTimestamp >= m_iBeginTimeStamp)
            {
                // 0. Compute the fading factor - (Special case from other drawings.)
                // ref frame is last point, and we only fade after it, not before.
                double fOpacityFactor = 1.0;
                if (m_TrackStatus == TrackStatus.Interactive &&  _iCurrentTimestamp > m_iEndTimeStamp)
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

                // Boundaries of visibility. 
                // First and last if complete traj, bounded otherwise.
                int iStart = 0;
                if(m_TrackView != TrackView.Complete && m_iCurrentPoint - m_iFocusFadingFrames > 0)
            	{
            		iStart = m_iCurrentPoint - m_iFocusFadingFrames;
            	}
                
            	int iEnd = m_RescaledPositions.Count - 1;
            	if(m_TrackView != TrackView.Complete && m_iCurrentPoint + m_iFocusFadingFrames < m_RescaledPositions.Count - 1)
            	{
            		iEnd = m_iCurrentPoint + m_iFocusFadingFrames;
            	}
        
            	// 3. Draw various elements depending on combination of view and status.
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
            	}
            	
            	if(m_RescaledPositions.Count > 0)
            	{
	            	// Target marker.
	            	if (fOpacityFactor == 1.0 && m_TrackView != TrackView.Label)
                    {
                    	DrawMarker(_canvas);
	            	}
                    
	            	// Search and template Windows (blue squares).
                    if ((m_TrackStatus == TrackStatus.Edit) && (fOpacityFactor == 1.0))
                    {
                        Color col = m_LineStyle.Color;
                        TrackPosition CurrentPos = m_RescaledPositions[m_iCurrentPoint];
                        
                        // Search window.
                        int iSrchLeft = CurrentPos.X - (int)(((double)m_SearchSize.Width * m_fStretchFactor) / 2);
                        int iSrchTop = CurrentPos.Y - (int)(((double)m_SearchSize.Height * m_fStretchFactor) / 2);
                        Rectangle SrchZone = new Rectangle(iSrchLeft, iSrchTop, (int)((double)m_SearchSize.Width * m_fStretchFactor), (int)((double)m_SearchSize.Height * m_fStretchFactor));
                        _canvas.DrawRectangle(new Pen(Color.FromArgb((int)(64.0f * fOpacityFactor), col)), SrchZone);

                        // Template window.
                        int iTplLeft = CurrentPos.X - (int)(((double)m_TemplateSize.Width * m_fStretchFactor) / 2);
                        int iTplTop = CurrentPos.Y - (int)(((double)m_TemplateSize.Height * m_fStretchFactor) / 2);
                        Rectangle TplZone = new Rectangle(iTplLeft, iTplTop, (int)((double)m_TemplateSize.Width * m_fStretchFactor), (int)((double)m_TemplateSize.Height * m_fStretchFactor));
                        _canvas.DrawRectangle(new Pen(Color.FromArgb((int)(128.0f * fOpacityFactor), col)), TplZone);
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
        public int HitTest(Point _point, long _iCurrentTimestamp)
        {
            //---------------------------------------------------------
            // Result: -1: miss, 0: on traj, 1: on Cursor, 2+: on Label
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
                    int widen = 3;
                    Rectangle WideTarget = new Rectangle(m_Positions[m_iCurrentPoint].X - m_iDefaultCrossRadius - widen, m_Positions[m_iCurrentPoint].Y - m_iDefaultCrossRadius - widen, (m_iDefaultCrossRadius+widen) * 2, (m_iDefaultCrossRadius+widen) * 2);
                    if (WideTarget.Contains(_point))
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
                            areaPath.Widen(new Pen(Color.Black, 12));

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

            return iHitResult;
        }
        private int IsOnKeyframesLabels(Point _point)
        {
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
                for (int i = 0; i < m_KeyframesLabels.Count; i++)
                {
                	if(m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T, m_KeyframesLabels[i].iTimestamp, m_iFocusFadingFrames))
                	{
                		if (m_KeyframesLabels[i].HitTest(_point))
	                    {
	                        iHitResult = i + 2;
	                        break;
	                    }
                	}
                }
            }

            return iHitResult;
        }
        public void StopTracking()
        {
            m_TrackStatus = TrackStatus.Interactive;
            m_iEndTimeStamp = m_Positions[m_Positions.Count -1].T + m_iBeginTimeStamp;
        }
		public void RestartTracking()
        {
            m_iEndTimeStamp = long.MaxValue;
            m_TrackStatus = TrackStatus.Edit;
        }
        public List<TrackPosition> GetEndOfTrack(long _iTimestamp)
        {
        	// Called from CommandDeleteEndOfTrack,
        	// when we need to keep the old values in case the command is undone.
        	List<TrackPosition> endOfTrack = new List<TrackPosition>();
        	foreach (TrackPosition trkpos in m_Positions)
            {
                if (trkpos.T >= _iTimestamp - m_iBeginTimeStamp)
                {
                    endOfTrack.Add(new TrackPosition(trkpos.X, trkpos.Y, trkpos.T));
                }
            }
        	return endOfTrack;
        }
        public void ChopTrajectory(long _iCurrentTimestamp)
        {
            m_iCurrentPoint = FindClosestPoint(_iCurrentTimestamp);

            if (m_iCurrentPoint < m_Positions.Count - 1)
            {
                m_Positions.RemoveRange(m_iCurrentPoint + 1, m_Positions.Count - m_iCurrentPoint - 1);
                m_RescaledPositions.RemoveRange(m_iCurrentPoint + 1, m_RescaledPositions.Count - m_iCurrentPoint - 1);
            }

            m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;
        }
        public void AppendPoints(long _iCurrentTimestamp, List<TrackPosition> _ChoppedPoints)
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
                foreach (TrackPosition trkpos in _ChoppedPoints)
                {
                    TrackPosition append = new TrackPosition(trkpos.X, trkpos.Y, trkpos.T, trkpos.Image);
                    m_Positions.Add(append);
                    m_RescaledPositions.Add(append);
                }

                m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;
            }
        }
        
        public void MoveCursor(int _X, int _Y)
        {
            if (m_TrackStatus == TrackStatus.Edit)
            {
                //----------------------------------------
                // Move cursor to new coords
                //----------------------------------------
                // In this case, _X and _Y are delta values.
                // Image will be reseted at mouse up. (=> UpdateCurrentPos)
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
                if (m_ShowClosestFrame != null)
                {
                    m_ShowClosestFrame(new Point(_X, _Y), m_iBeginTimeStamp, m_Positions, m_iTotalDistance, false);
                }
            }
        }
        public void MoveLabelTo(int _deltaX, int _deltaY, int _iLabelNumber)
        {
        	if (m_TrackStatus == TrackStatus.Edit || m_TrackView != TrackView.Label)
            {
                // Move the specified label by specified amount.
                int iLabel = _iLabelNumber - 2;

                // Don't use absolute coordinates:
                // We want the rectangle to be moved right under the mouse cursor.
                // Problem: the delta between the mouse and the top left corner will be rescaled aswell.
                m_KeyframesLabels[iLabel].Background = new Rectangle(m_KeyframesLabels[iLabel].Background.X + _deltaX, m_KeyframesLabels[iLabel].Background.Y + _deltaY, m_KeyframesLabels[iLabel].Background.Width, m_KeyframesLabels[iLabel].Background.Height);
                m_KeyframesLabels[iLabel].Rescale(m_fStretchFactor, m_DirectZoomTopLeft);
            }
            else if (m_TrackView == TrackView.Label)
            {
                m_MainLabel.Background = new Rectangle(m_MainLabel.Background.X + _deltaX, m_MainLabel.Background.Y + _deltaY, m_MainLabel.Background.Width, m_MainLabel.Background.Height);
                m_MainLabel.Rescale(m_fStretchFactor, m_DirectZoomTopLeft);
            }
        }
        public void IntegrateKeyframes()
        {
            //-----------------------------------------------------------------------------------
            // The Keyframes list changed (add/remove/comments)
            // Reconstruct the Keyframes Labels, but don't completely reset those we already have
            // (Keep custom coordinates)
            //-----------------------------------------------------------------------------------

            Button but = new Button(); // Hack to get a Graphics object.
            Graphics g = but.CreateGraphics();

            // Keep track of matched keyframes so we can remove the others.
            bool[] matched = new bool[m_KeyframesLabels.Count];

            for (int i = 0; i < m_ParentMetadata.Count; i++)
            {
                // strictly superior : we never show the origin key frame.
                if (m_ParentMetadata[i].Position > m_iBeginTimeStamp && m_ParentMetadata[i].Position <= (m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp))
                {
                    // The Keyframe is within the Trajectory interval.

                    // 1. Do we know it already ?
                    int iKnown = - 1;
                    for(int j=0;j<m_KeyframesLabels.Count;j++)
                    {
                        if (m_KeyframesLabels[j].TrackPos.T + m_iBeginTimeStamp == m_ParentMetadata[i].Position)
                        {
                            iKnown = j;
                            matched[j] = true;
                            break;
                        }
                    }
                    
                    if (iKnown >= 0)
                    {
                        // Known Keyframe, Read text again in case it changed
                        if (m_ParentMetadata[i].Title != m_KeyframesLabels[iKnown].Text)
                        {
                            m_KeyframesLabels[iKnown].Text = m_ParentMetadata[i].Title;
                            m_KeyframesLabels[iKnown].ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
						}
                    }
                    else
                    {
                        // Unknown Keyframe, Configure and add it to list.
                        KeyframeLabel kfl = new KeyframeLabel(false, Color.Black);
                        kfl.Text = m_ParentMetadata[i].Title;
                        kfl.TrackPos = m_Positions[FindClosestPoint(m_ParentMetadata[i].Position)];
                        kfl.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
                        m_KeyframesLabels.Add(kfl);
                    }
                }
            }
            g.Dispose();

            // Remove unused Keyframes.
            // We only look in the original list and remove in reverse so the index aren't messed up.
            for (int iLabel = matched.Length - 1; iLabel >= 0; iLabel--)
            {
                if (matched[iLabel] == false)
                {
                    // remove the Keyframe
                    m_KeyframesLabels.RemoveAt(iLabel);
                }
            }
        }
        public void TrackCurrentPosition(long _iCurrentTimestamp, Bitmap _bmpCurrent)
        {
            // This is where the matching occurs.
            // Match previous point in current image.

            // TEMP: obligatoirement supérieur au dernier ajouté...
            //if (_iCurrentTimestamp >= m_iBeginTimeStamp)
            if (_iCurrentTimestamp >= m_iBeginTimeStamp + m_Positions[m_Positions.Count - 1].T)
            {
                // Do we have it already ?
                bool bAlreadyTracked = false;
                for (int i = 0; i < m_Positions.Count; i++)
                {
                    if (m_Positions[i].T == _iCurrentTimestamp - m_iBeginTimeStamp)
                    {
                        bAlreadyTracked = true;
                    }
                }

                if (!bAlreadyTracked)
                {
                    // Initialize at same position as the previous point.
                    TrackPosition PreviousPos = m_Positions[m_Positions.Count - 1];
                    int iNewX = PreviousPos.X;
                    int iNewY = PreviousPos.Y;

                    // Try to match
                    bool bMatched = false;
                    if (PreviousPos.Image != null && _bmpCurrent != null)
                    {
                        Rectangle searchZone = new Rectangle(PreviousPos.X - (m_SearchSize.Width/2), PreviousPos.Y - (m_SearchSize.Height/2), m_SearchSize.Width, m_SearchSize.Height);
                        TemplateMatch[] matchings = m_TemplateMatcher.ProcessImage(_bmpCurrent, searchZone, PreviousPos.Image);

                        if (matchings.Length > 0)
                        {
                            // Get new center.
                            iNewX = matchings[0].Rectangle.Left + (matchings[0].Rectangle.Width / 2);
                            iNewY = matchings[0].Rectangle.Top + (matchings[0].Rectangle.Height / 2);
                            bMatched = true;
                            //log.Debug(String.Format("Tracking similarity result:{0}", matchings[0].Similarity));
                        }
                    }

                
                    // Copy matched zone. If no match found, copy same place.
                    // We still add the point even if we did not matched it because we will stop tracking at this point.
                    // If we don't add the point here it wouldn't be possible to move forward after an unmatch.
                    Bitmap bmpMatched = new Bitmap(m_TemplateSize.Width, m_TemplateSize.Height, PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(bmpMatched);
                    Rectangle rDst = new Rectangle(0, 0, m_TemplateSize.Width, m_TemplateSize.Height);
                    Rectangle rSrc = new Rectangle(iNewX - (m_TemplateSize.Width / 2), iNewY - (m_TemplateSize.Height / 2), m_TemplateSize.Width, m_TemplateSize.Height);
                    g.DrawImage(_bmpCurrent, rDst, rSrc, GraphicsUnit.Pixel);

                    TrackPosition TrackedPos = new TrackPosition(iNewX, iNewY, _iCurrentTimestamp - m_iBeginTimeStamp, bmpMatched);

                    // Add the position
                    m_Positions.Add(TrackedPos);
                    m_RescaledPositions.Add(RescalePosition(TrackedPos, m_fStretchFactor, m_DirectZoomTopLeft));

                    m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T + m_iBeginTimeStamp;

                    // Total distance (for rescaling time scale vs px scale.)
                    ComputeFlatDistance();
                    
                    // We may have reached a key frame...
                    IntegrateKeyframes();

                    if (!bMatched)
                    {
                        // Couldn't find the template => stop tracking.
                        StopTracking();
                    }
                }
            }
        }
        private void ComputeFlatDistance()
        {
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
        public void UpdateCurrentPos(Bitmap _bmp)
        {
            if (m_Positions.Count > 1 && m_iCurrentPoint >= 0)
            {
                TrackPosition CurrentPos = m_Positions[m_iCurrentPoint];

                if (CurrentPos.Image != null)
                {
                    CurrentPos.Image.Dispose();
                }

                CurrentPos.Image = new Bitmap(m_TemplateSize.Width, m_TemplateSize.Height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(CurrentPos.Image);
                Rectangle rDst = new Rectangle(0, 0, m_TemplateSize.Width, m_TemplateSize.Height);
                Rectangle rSrc = new Rectangle(CurrentPos.X - (m_TemplateSize.Width / 2), CurrentPos.Y - (m_TemplateSize.Height / 2), m_TemplateSize.Width, m_TemplateSize.Height);
                g.DrawImage(_bmp, rDst, rSrc, GraphicsUnit.Pixel);
            }
        }
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

            //_xmlWriter.WriteStartElement("ShowKeyframesTitles");
            //_xmlWriter.WriteString(m_bShowKeyframesTitles.ToString());
            //_xmlWriter.WriteEndElement();

            // Global Label
            _xmlWriter.WriteStartElement("Label");
            _xmlWriter.WriteStartElement("Text");
            _xmlWriter.WriteString(m_MainLabel.Text);
            _xmlWriter.WriteEndElement();
            _xmlWriter.WriteEndElement();

            // </Track>
            _xmlWriter.WriteEndElement();
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = 0;
            iHash ^= m_TrackView.GetHashCode();
            foreach (TrackPosition p in m_Positions)
            {
                iHash ^= p.GetHashCode();
            }

            iHash ^= m_iDefaultCrossRadius.GetHashCode();
            iHash ^= m_LineStyle.GetHashCode();
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
        	m_MemoLineStyle = m_LineStyle.Clone();
        	m_MemoTrackView = m_TrackView;
        	m_MemoLabel = m_MainLabel.Text;
        }
        public void RecallState()
        {
        	// Used when the user cancels his modifications on formConfigureTrajectory.
        	m_LineStyle = m_MemoLineStyle.Clone();
        	m_MainLabel.TextDecoration.Update(m_LineStyle.Color);
        	m_TrackView = m_MemoTrackView;
        	m_MainLabel.Text = m_MemoLabel;
            m_MainLabel.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
        }
        public static Track FromXml(XmlTextReader _xmlReader, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
        {
            Track trk = new Track(0,0,0, null);
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
                    trk.m_MainLabel.TrackPos = trk.m_Positions[0];
                    trk.m_MainLabel.RescaledTrackPos = trk.m_RescaledPositions[0];
                    trk.m_MainLabel.Text = trk.Label;
                    trk.m_MainLabel.ResetBackground(1.0, new Point(0, 0));
                }
                trk.RescaleCoordinates(trk.m_fStretchFactor, trk.m_DirectZoomTopLeft);
            }
            
            return trk;
        }
        #endregion

       	#region Private methods
        private TrackPosition RescalePosition(TrackPosition _position, double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            return new TrackPosition((int)((double)(_position.X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(_position.Y - _DirectZoomTopLeft.Y) * _fStretchFactor), _position.T);
        }
        private void RescaleCoordinates(double _fStretchFactor, Point _DirectZoomTopLeft)
        {
            // Trajectory Points
            for (int i = 0; i < m_Positions.Count; i++)
            {
                m_RescaledPositions[i] = new TrackPosition((int)((double)(m_Positions[i].X - _DirectZoomTopLeft.X) * _fStretchFactor), (int)((double)(m_Positions[i].Y - _DirectZoomTopLeft.Y) * _fStretchFactor), m_Positions[i].T);
            }

            // Labels Backgrounds
            foreach (KeyframeLabel kfl in m_KeyframesLabels)
            {
            	kfl.Rescale(_fStretchFactor, _DirectZoomTopLeft);
            }

            m_MainLabel.Rescale(_fStretchFactor, _DirectZoomTopLeft);
        }
        private int FindClosestPoint(long _iCurrentTimestamp)
        {
            return FindClosestPoint(_iCurrentTimestamp, m_Positions, m_iBeginTimeStamp);
        }
        private int FindClosestPoint(long _iCurrentTimestamp, List<TrackPosition> _Positions, long _iBeginTimestamp)
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
        
        #region Drawing Helpers
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
            	_canvas.DrawCurve(GetTrackPen(m_LineStyle, m_TrackStatus, _fFadingFactor, _before), points, 0.5f);	
            }
        }
        private Pen GetTrackPen(LineStyle _style, TrackStatus _status, double _fFadingFactor, bool _before)
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
        	
            return _style.GetInternalPen(iAlpha);
        }
        private void DrawMarker(Graphics _canvas)
        {
            // This draws the target marker (CrashTest Dummy style target)
            // If editmode, we draw the whole target, otherwise only the white sectors.
            
            //if (m_bEditMode)
            {
                _canvas.FillPie(Brushes.Black, (float)m_RescaledPositions[m_iCurrentPoint].X - m_iDefaultCrossRadius, (float)m_RescaledPositions[m_iCurrentPoint].Y - m_iDefaultCrossRadius, (float)m_iDefaultCrossRadius * 2, (float)m_iDefaultCrossRadius * 2, 0, 90);
            }
            
            _canvas.FillPie(Brushes.White, (float)m_RescaledPositions[m_iCurrentPoint].X - m_iDefaultCrossRadius, (float)m_RescaledPositions[m_iCurrentPoint].Y - m_iDefaultCrossRadius, (float)m_iDefaultCrossRadius * 2, (float)m_iDefaultCrossRadius * 2, 90, 90);
            
            //if (m_bEditMode)
            {
                 _canvas.FillPie(Brushes.Black, (float)m_RescaledPositions[m_iCurrentPoint].X - m_iDefaultCrossRadius, (float)m_RescaledPositions[m_iCurrentPoint].Y - m_iDefaultCrossRadius, (float)m_iDefaultCrossRadius * 2, (float)m_iDefaultCrossRadius * 2, 180, 90);
            }
            
            _canvas.FillPie(Brushes.White, (float)m_RescaledPositions[m_iCurrentPoint].X - m_iDefaultCrossRadius, (float)m_RescaledPositions[m_iCurrentPoint].Y - m_iDefaultCrossRadius, (float)m_iDefaultCrossRadius * 2, (float)m_iDefaultCrossRadius * 2, 270, 90);

            
            // Contour
            int ContourRadius = m_iDefaultCrossRadius + 2;            
            _canvas.DrawEllipse(Pens.White, m_RescaledPositions[m_iCurrentPoint].X - ContourRadius, m_RescaledPositions[m_iCurrentPoint].Y - ContourRadius, ContourRadius * 2, ContourRadius * 2);
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
					// Only show labels that are in focus section.
                	if(m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T, kl.iTimestamp, m_iFocusFadingFrames))
                	{
                    	// Shift/scale background, then draw.
                    	kl.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
                    	kl.Draw(_canvas, _fFadingFactor);
                	}
                }
            }
        }
        private void DrawMainLabel(Graphics _canvas, int _iCurrentPoint, double _fFadingFactor)
        {
            // This should only be called when in Label Follows mode.
            if (_fFadingFactor == 1.0f)
            {
                m_MainLabel.TrackPos = m_Positions[_iCurrentPoint];
                m_MainLabel.RescaledTrackPos = m_RescaledPositions[_iCurrentPoint];

                // Shift/scale background, then draw.
                m_MainLabel.ResetBackground(m_fStretchFactor, m_DirectZoomTopLeft);
                m_MainLabel.Draw(_canvas, _fFadingFactor);
            }
        }
        private void DrawArrow(Graphics _canvas, int _iCurrentPoint, double _fFadingFactor)
        {
            // This should only be called when in Arrow Follows mode.
            if (_fFadingFactor == 1.0f)
            {
                Pen p = new Pen(Color.FromArgb((int)(160.0f * _fFadingFactor), m_LineStyle.Color), (int)((double)m_iArrowWidth * m_fStretchFactor));
                p.StartCap = LineCap.ArrowAnchor;
                p.EndCap = LineCap.Round;
                _canvas.DrawLine(p, m_RescaledPositions[_iCurrentPoint].X, m_RescaledPositions[_iCurrentPoint].Y - (int)((double)m_iArrowDistance * m_fStretchFactor), m_RescaledPositions[_iCurrentPoint].X, m_RescaledPositions[_iCurrentPoint].Y - (int)((double)(m_iArrowDistance + m_iArrowLength) * m_fStretchFactor));
                p.Dispose();
            }
        }
        #endregion
        
        #region Xml routines
        private void ParseTrackLine(XmlTextReader _xmlReader, Track _track)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "LineStyle")
                    {
                    	_track.m_LineStyle = LineStyle.FromXml(_xmlReader);
                    }
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
                        TrackPosition tp = new TrackPosition(0, 0, 0);
                        tp.FromXml(_xmlReader);

                        TrackPosition adapted = new TrackPosition( (int)((float)tp.X * _scale.X),
                                                                   (int)((float)tp.Y * _scale.Y),
                                                                   _remapTimestampCallback(tp.T, true));

                        _track.m_Positions.Add(adapted);
                        _track.m_RescaledPositions.Add(adapted); // (not really scaled but must be added anyway so both array stay aligned.)
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
                            int MatchedTrackPosition = FindClosestPoint(kfl.iTimestamp, _track.m_Positions, _track.m_iBeginTimeStamp);
                            kfl.TrackPos = _track.m_Positions[MatchedTrackPosition];
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
        private KeyframeLabel ParseKeyframeLabel(XmlTextReader _xmlReader, PointF _scale, DelegateRemapTimestamp _remapTimestampCallback)
        {
            KeyframeLabel kfl = new KeyframeLabel(false, Color.Black);

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "KeyframeLabelSpacePosition")
                    {
                        Point p = XmlHelper.PointParse(_xmlReader.ReadString(), ';');

                        Point adapted = new Point((int)((float)p.X * _scale.X), (int)((float)p.Y * _scale.Y));

                        kfl.Background = new Rectangle(adapted,new Size(10, 10));
                        kfl.RescaledBackground = new Rectangle(adapted, new Size(10, 10));
                    }
                    else if (_xmlReader.Name == "KeyframeLabelTimePosition")
                    {
                        kfl.iTimestamp = long.Parse(_xmlReader.ReadString());
                    }
                    else
                    {
                        // forward compatibility : ignore new fields. 
                    }
                }
                else if (_xmlReader.Name == "KeyframeLabel")
                {
                    break;
                }
                else
                {
                    // Fermeture d'un tag interne.
                }
            }  

            return kfl;
        }
        private void ParseLabel(XmlTextReader _xmlReader, Track _track)
        {
            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    if (_xmlReader.Name == "Text")
                    {
                        _track.m_MainLabel.Text = _xmlReader.ReadString();
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
            _xmlWriter.WriteStartElement("TrackLine");
           
            m_LineStyle.ToXml(_xmlWriter);

            // </trackline>
            _xmlWriter.WriteEndElement();
        }
        private void TrackPointsToXml(XmlTextWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("TrackPositionList");
            _xmlWriter.WriteAttributeString("Count", m_Positions.Count.ToString());
            
            _xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.LineLengthHelper.GetAbbreviation());
            // todo: user unit time.
            
            if(m_Positions.Count > 0)
            {
            	foreach (TrackPosition tp in m_Positions)
            	{
            		tp.ToXml(_xmlWriter, m_ParentMetadata, m_Positions[0]);
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
        
        #endregion
         
    }
}
