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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
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
        #region Delegates
        // To ask the UI to display the frame closest to selected pos.
        // used when moving the target in direct interactive mode.
        public ClosestFrameAction m_ShowClosestFrame;     
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
        		m_MainLabel.BackColor = value;
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
        public bool Invalid 
        {
            get { return m_Invalid;}
        }
		// Fading is not modifiable from outside.
        public override InfosFading  infosFading
        {
            get { throw new NotImplementedException("Track, The method or operation is not implemented."); }
            set { throw new NotImplementedException("Track, The method or operation is not implemented."); }
        }
        public override DrawingCapabilities Caps
		{
			get { return DrawingCapabilities.None; }
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
        private bool m_Invalid;                                 // Used for XML import.
        	
        // Tracker tool.
        private AbstractTracker m_Tracker;
        private bool m_bUntrackable;
        
        // Hardwired parameters.
        private const int m_iDefaultCrossRadius = 4;
        private const int m_iAllowedFramesOver = 12;  	// Number of frames over which the global fading spans (after end point).
        private const int m_iFocusFadingFrames = 30;	// Number of frames of the focus section. 
       
        // Internal data.
        private List<AbstractTrackPoint> m_Positions = new List<AbstractTrackPoint>();
        private List<KeyframeLabel> m_KeyframesLabels = new List<KeyframeLabel>();
		
        private long m_iBeginTimeStamp;     			// absolute.
        private long m_iEndTimeStamp = long.MaxValue; 	// absolute.
        private int m_iTotalDistance;       			// This is used to normalize timestamps to a par scale with distances.
        private int m_iCurrentPoint;

        // Decoration
        private StyleHelper m_StyleHelper = new StyleHelper();
        private DrawingStyle m_Style;
        private KeyframeLabel m_MainLabel = new KeyframeLabel();
        private string m_MainLabelText = "Label";
        private InfosFading m_InfosFading = new InfosFading(long.MaxValue, 1);
        private const int m_iBaseAlpha = 224;				// alpha of track in most cases.
        private const int m_iAfterCurrentAlpha = 64;		// alpha of track after the current point when in normal mode.
        private const int m_iEditModeAlpha = 128;			// alpha of track when in Edit mode.
        private const int m_iLabelFollowsTrackAlpha = 80;	// alpha of track when in LabelFollows view.
        
        // Memorization poul
        private TrackView m_MemoTrackView;
        private string m_MemoLabel;
        private Metadata m_ParentMetadata;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public Track(Point _origin, long _t, Bitmap _CurrentImage, Size _imageSize)
        {
            //-----------------------------------------------------------------------------------------
            // t is absolute time.
            // _bmp is the whole picture, if null it means we don't need it.
            // (Probably because we already have a few points that we are importing from xml.
            // In this case we'll only need the last frame to reconstruct the last point.)
			//-----------------------------------------------------------------------------------------
            
            // Create the first point
            if (_CurrentImage != null)
            {
            	m_Tracker = new TrackerBlock2(_CurrentImage.Width, _CurrentImage.Height);
           		AbstractTrackPoint atp = m_Tracker.CreateTrackPoint(true, _origin.X, _origin.Y, 1.0f, _t, _CurrentImage, m_Positions);
           		if(atp != null)
           			m_Positions.Add(atp);
           		else
           			m_bUntrackable = true;
            }
            else
            {
            	// Happens when loading Metadata from file or demuxing.
            	m_Tracker = new TrackerBlock2(_imageSize.Width, _imageSize.Height);
            	m_Positions.Add(m_Tracker.CreateOrphanTrackPoint(_origin.X, _origin.Y, _t));
            }

            if(!m_bUntrackable)
            {
	            m_iBeginTimeStamp = _t;
	            m_iEndTimeStamp = _t;
	            m_MainLabel.SetAttach(_origin, true);
	            
	            // We use the InfosFading utility to fade the track away.
	            // The refererence frame will be the last point (at which fading start).
	            // AverageTimeStampsPerFrame will be updated when we get the parent metadata.
	            m_InfosFading.FadingFrames = m_iAllowedFramesOver;
	            m_InfosFading.UseDefault = false;
	            m_InfosFading.Enabled = true;
            }
            
            // Decoration
            m_Style = new DrawingStyle();
            m_Style.Elements.Add("color", new StyleElementColor(Color.SeaGreen));
			m_Style.Elements.Add("line size", new StyleElementLineSize(3));
			m_Style.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));
            m_StyleHelper.Color = Color.Black;
            m_StyleHelper.LineSize = 3;
            m_StyleHelper.TrackShape = TrackShape.Dash;
            BindStyle();
            
            m_StyleHelper.ValueChanged += mainStyle_ValueChanged;
        }
        public Track(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback, Size _imageSize)
            : this(Point.Empty,0, null, _imageSize)
        {
            ReadXml(_xmlReader, _scale, _remapTimestampCallback);
        }
        #endregion

        #region AbstractDrawing implementation
        public override void Draw(Graphics _canvas, CoordinateSystem _transformer, bool _bSelected, long _iCurrentTimestamp)
		{
            if (_iCurrentTimestamp < m_iBeginTimeStamp)
                return;
                
            // 0. Compute the fading factor. 
            // Special case from other drawings:
            // ref frame is last point, and we only fade after it, not before.
            double fOpacityFactor = 1.0;
            if (m_TrackStatus == TrackStatus.Interactive && _iCurrentTimestamp > m_iEndTimeStamp)
            {
            	m_InfosFading.ReferenceTimestamp = m_iEndTimeStamp;
            	fOpacityFactor = m_InfosFading.GetOpacityFactor(_iCurrentTimestamp);
            }
            
            if(fOpacityFactor <= 0)
                return;

            m_iCurrentPoint = FindClosestPoint(_iCurrentTimestamp);
            
        	
        	// Draw various elements depending on combination of view and status.
        	// The exact alpha at which the traj will be drawn will be decided in GetTrackPen().
        	if(m_Positions.Count > 1)
        	{
                // Key Images titles.
            	if (m_TrackStatus == TrackStatus.Interactive && m_TrackView != TrackView.Label)
            		DrawKeyframesTitles(_canvas, fOpacityFactor, _transformer);	
            	
            	// Track.
            	int first = GetFirstVisiblePoint();
                int last = GetLastVisiblePoint();
            	if (m_TrackStatus == TrackStatus.Interactive && m_TrackView == TrackView.Complete)
            	{
            		DrawTrajectory(_canvas, first, m_iCurrentPoint, true, fOpacityFactor, _transformer);	
            		DrawTrajectory(_canvas, m_iCurrentPoint, last, false, fOpacityFactor, _transformer);
            	}
            	else
            	{
            		DrawTrajectory(_canvas, first, last, false, fOpacityFactor, _transformer);
            	}
        	}
        	
        	if(m_Positions.Count > 0)
        	{
            	// Track.
            	if( fOpacityFactor == 1.0 && m_TrackView != TrackView.Label)
            		DrawMarker(_canvas, fOpacityFactor, _transformer);
            	
            	// Search boxes. (only on edit)
                if ((m_TrackStatus == TrackStatus.Edit) && (fOpacityFactor == 1.0))
                    m_Tracker.Draw(_canvas, m_Positions[m_iCurrentPoint].Point, _transformer, m_StyleHelper.Color, fOpacityFactor);
                
                // Main label.
                if (m_TrackStatus == TrackStatus.Interactive && m_TrackView == TrackView.Label ||
                    m_TrackStatus == TrackStatus.Interactive && m_TrackExtraData != TrackExtraData.None)
                {
                	DrawMainLabel(_canvas, m_iCurrentPoint, fOpacityFactor, _transformer);
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
		public override void MoveHandle(Point point, int handleNumber, Keys modifiers)
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
                // If label attach mode, this will tell if we are on the label.
                if (m_TrackStatus == TrackStatus.Interactive)
                    iHitResult = IsOnKeyframesLabels(_point);
                    
                if (iHitResult == -1)
                {
                	Rectangle rectangleTarget;
                	if(m_TrackStatus == TrackStatus.Edit)
                		rectangleTarget = m_Tracker.GetEditRectangle(m_Positions[m_iCurrentPoint].Point);
                	else
                    	rectangleTarget = m_Positions[m_iCurrentPoint].Box(m_iDefaultCrossRadius + 3);
                    
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
                            for (int i = iStart; i < iEnd; i++)
                                points[i-iStart] = m_Positions[i].Point;

                            using(GraphicsPath areaPath = new GraphicsPath())
                            {
                                areaPath.AddCurve(points, 0.5f);
                                RectangleF bounds = areaPath.GetBounds();
                                if(!bounds.IsEmpty)
                                {
                                    using(Pen tempPen = new Pen(Color.Black, m_StyleHelper.LineSize + 7))
                                    {
                                        areaPath.Widen(tempPen);
                                    }
                                    using(Region areaRegion = new Region(areaPath))
                                    {
                                        iHitResult = areaRegion.IsVisible(_point) ? 0 : -1;
                                    }
                                }
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
        private void DrawTrajectory(Graphics _canvas, int _start, int _end, bool _before, double _fFadingFactor, CoordinateSystem _transformer)
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
                points[i] = _transformer.Transform(m_Positions[_start + i].Point);
                
            if (points.Length > 1)
            {
            	using(Pen trackPen = GetTrackPen(m_TrackStatus, _fFadingFactor, _before))
            	{
                    // Tension parameter is at 0.5f for bezier effect (smooth curve).
                    _canvas.DrawCurve(trackPen, points, 0.5f);
                	
                	if(m_StyleHelper.TrackShape.ShowSteps)
                	{
                	    using(Pen stepPen = new Pen(trackPen.Color, 2))
                	    {
                            int margin = (int)(trackPen.Width * 1.5);
        	            	foreach(Point p in points)
        	            	    _canvas.DrawEllipse(stepPen, p.Box(margin));
                	    }
                	}
            	}
            }
        }
        private void DrawMarker(Graphics _canvas,  double _fFadingFactor, CoordinateSystem _transformer)
        { 
        	int radius = m_iDefaultCrossRadius;
        	Point location = _transformer.Transform(m_Positions[m_iCurrentPoint].Point);
        	
        	if(m_TrackStatus == TrackStatus.Edit)
        	{
        		// Little cross.
        		using(Pen p = new Pen(Color.FromArgb((int)(_fFadingFactor * 255), m_StyleHelper.Color)))
        		{
        		  _canvas.DrawLine(p, location.X, location.Y - radius, location.X, location.Y + radius);
        		  _canvas.DrawLine(p, location.X - radius, location.Y, location.X + radius, location.Y);
        		}
        	}
        	else
        	{
	    		// Crash test dummy style target.
                int diameter = radius * 2;
	            _canvas.FillPie(Brushes.Black, location.X - radius , location.Y - radius , diameter, diameter, 0, 90);
	            _canvas.FillPie(Brushes.White, location.X - radius , location.Y - radius , diameter, diameter, 90, 90);
	            _canvas.FillPie(Brushes.Black, location.X - radius , location.Y - radius , diameter, diameter, 180, 90);
	            _canvas.FillPie(Brushes.White, location.X - radius , location.Y - radius , diameter, diameter, 270, 90);
    		    _canvas.DrawEllipse(Pens.White, location.Box(radius + 2));
        	}
        }
        private void DrawKeyframesTitles(Graphics _canvas, double _fFadingFactor, CoordinateSystem _transformer)
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
                	   m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T, kl.Timestamp, m_iFocusFadingFrames)
                	  )
                	{
                    	kl.Draw(_canvas, _transformer, _fFadingFactor);
                	}
                }
            }
        }
        private void DrawMainLabel(Graphics _canvas, int _iCurrentPoint, double _fFadingFactor, CoordinateSystem _transformer)
        {
            // Draw the main label and its connector to the current point.
            if (_fFadingFactor == 1.0f)
            {
                m_MainLabel.SetAttach(m_Positions[_iCurrentPoint].Point, true);
                
                if(m_TrackView == TrackView.Label)
                {
                    m_MainLabel.SetText(m_MainLabelText);
                }
                else
                {
                    m_MainLabel.SetText(GetExtraDataText(_iCurrentPoint));
                }
                
                m_MainLabel.Draw(_canvas, _transformer, _fFadingFactor);
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
        				iAlpha = (int)(_fFadingFactor * m_iBaseAlpha);
        			}
        			else
        			{
        				iAlpha = m_iAfterCurrentAlpha;
        			}
        		}
        		else if(m_TrackView == TrackView.Focus)
        		{
        			iAlpha = (int)(_fFadingFactor * m_iBaseAlpha);		
        		}
        		else if(m_TrackView == TrackView.Label)
        		{
        			iAlpha = (int)(_fFadingFactor * m_iLabelFollowsTrackAlpha);
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
        	// Cumulative distance between two points.
        	if(m_Positions.Count < 1)
        	    return "";
        	
        	if(_p1 < 0 || _p1 >= m_Positions.Count || _p2 < 0 || _p2 >= m_Positions.Count)
        	    return m_ParentMetadata.CalibrationHelper.GetLengthText(0);
        	
        	double fPixelDistance = 0;
    		for(int i = _p1; i < _p2; i++)
    			fPixelDistance += GeometryHelper.GetDistance(m_Positions[i].Point, m_Positions[i+1].Point);
    		
        	return m_ParentMetadata.CalibrationHelper.GetLengthText(fPixelDistance);
        }
        private string GetSpeedText(int _p1, int _p2)
        {
        	// return the instant speed at p2.
			// (that is the distance between p1 and p2 divided by the time to get from p1 to p2).
			// p2 needs to be after p1.
			
			if(m_Positions.Count < 1)
        	    return "";
        	
        	if(_p1 < 0 || _p1 >= m_Positions.Count-1 || _p2 < 0 || _p2 >= m_Positions.Count)
        	    return m_ParentMetadata.CalibrationHelper.GetSpeedText(m_Positions[0].Point, m_Positions[0].Point, 0);
        	
    		return m_ParentMetadata.CalibrationHelper.GetSpeedText(m_Positions[_p1].Point, m_Positions[_p2].Point, _p2 - _p1);
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
            }
            else
            {
                // Move Playhead to closest frame (x,y,t).
                // In this case, _X and _Y are absolute values.
                if (m_ShowClosestFrame != null && m_Positions.Count > 1)
                    m_ShowClosestFrame(new Point(_X, _Y), m_Positions, m_iTotalDistance, false);
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
            // Convention: -1 = miss, 2 = on main label, 3+ = on keyframe label.
            int iHitResult = -1;
            if (m_TrackView == TrackView.Label)
            {
                if (m_MainLabel.HitTest(_point))
                    iHitResult = 2;
            }
            else
            {
            	// Even when we aren't in TrackView.Label, the main label is visible
            	// if we are displaying the extra data (distance, speed).
            	if (m_TrackExtraData != TrackExtraData.None)
	            {
	                if (m_MainLabel.HitTest(_point))
	                    iHitResult = 2;
	            }	
            	
                for (int i = 0; i < m_KeyframesLabels.Count; i++)
                {
                    bool isVisible = m_InfosFading.IsVisible(m_Positions[m_iCurrentPoint].T, 
                                                             m_KeyframesLabels[i].Timestamp, 
                                                             m_iFocusFadingFrames);
                    if(m_TrackView == TrackView.Complete || isVisible)
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
        	if((m_TrackView != TrackView.Complete || m_TrackStatus == TrackStatus.Edit) && m_iCurrentPoint - m_iFocusFadingFrames > 0)
        		return m_iCurrentPoint - m_iFocusFadingFrames;
            else
                return 0;
        }
        private int GetLastVisiblePoint()
        {
        	if((m_TrackView != TrackView.Complete || m_TrackStatus == TrackStatus.Edit) && m_iCurrentPoint + m_iFocusFadingFrames < m_Positions.Count - 1)
        		return m_iCurrentPoint + m_iFocusFadingFrames;
            else
                return m_Positions.Count - 1;
        }
        #endregion
        
        #region Context Menu implementation
        public void ChopTrajectory(long _iCurrentTimestamp)
        {
        	// Delete end of track.
            m_iCurrentPoint = FindClosestPoint(_iCurrentTimestamp);
            if (m_iCurrentPoint < m_Positions.Count - 1)
                m_Positions.RemoveRange(m_iCurrentPoint + 1, m_Positions.Count - m_iCurrentPoint - 1);

            m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T;
            // Todo: we must now refill the last point with a patch image.
        }
        public List<AbstractTrackPoint> GetEndOfTrack(long _iTimestamp)
        {
        	// Called from CommandDeleteEndOfTrack,
        	// We need to keep the old values in case the command is undone.
          List<AbstractTrackPoint> endOfTrack = m_Positions.SkipWhile(p => p.T >= _iTimestamp).ToList();
        	return endOfTrack;
        }
        public void AppendPoints(long _iCurrentTimestamp, List<AbstractTrackPoint> _ChoppedPoints)
        {
            // Called when undoing CommandDeleteEndOfTrack,
            // revival of the discarded points.
            if (_ChoppedPoints.Count > 0)
            {
                // Some points may have been re added already and we don't want to mix the two lists.
                // Find the append insertion point, remove extra stuff, and append.
                int iMatchedPoint = m_Positions.Count - 1;
                
                while (m_Positions[iMatchedPoint].T >= _ChoppedPoints[0].T && iMatchedPoint > 0)
                    iMatchedPoint--;

                if (iMatchedPoint < m_Positions.Count - 1)
                    m_Positions.RemoveRange(iMatchedPoint + 1, m_Positions.Count - (iMatchedPoint+1));

                foreach (AbstractTrackPoint trkpos in _ChoppedPoints)
                    m_Positions.Add(trkpos);

                m_iEndTimeStamp = m_Positions[m_Positions.Count - 1].T;
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
        public void TrackCurrentPosition(VideoFrame _current)
        {
            // Match the previous point in current image.
            // New points to trajectories are always created from here, 
            // the user can only moves existing points.
            
            if (_current.Timestamp <= m_Positions.Last().T)
                return;
            
            AbstractTrackPoint p = null;
            bool bMatched = m_Tracker.Track(m_Positions, _current.Image, _current.Timestamp, out p);
                
            if(p==null)
            {
                StopTracking();
                return;
            }
        	
            m_Positions.Add(p);

			if (!bMatched)
                StopTracking();
			
        	// Adjust internal data.
        	m_iEndTimeStamp = m_Positions.Last().T;
            ComputeFlatDistance();
            IntegrateKeyframes();
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
            if (m_Positions.Count < 2 || m_iCurrentPoint < 0)
                return;
            
            AbstractTrackPoint current = m_Positions[m_iCurrentPoint];
        
        	current.ResetTrackData();
        	AbstractTrackPoint atp = m_Tracker.CreateTrackPoint(true, current.X, current.Y, 1.0f, current.T,  _CurrentImage, m_Positions);
        	
        	if(atp != null)
        		 m_Positions[m_iCurrentPoint] = atp;
        	
        	// Update the mini label (attach, position of label, and text).
        	for (int i = 0; i < m_KeyframesLabels.Count; i++)
        	{
        		if(m_KeyframesLabels[i].Timestamp == current.T)
        		{
        			m_KeyframesLabels[i].SetAttach(current.Point, true);
					if(m_TrackExtraData != TrackExtraData.None)
					    m_KeyframesLabels[i].SetText(GetExtraDataText(m_KeyframesLabels[i].AttachIndex));
                    
					break;
        		}
        	}
        }
		#endregion
        
        #region XML import/export
        public void WriteXml(XmlWriter _xmlWriter)
		{
		    _xmlWriter.WriteElementString("TimePosition", m_iBeginTimeStamp.ToString());
		    
		    TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackView));
            string xmlMode = enumConverter.ConvertToString(m_TrackView);
            _xmlWriter.WriteElementString("Mode", xmlMode);
            
            enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
            string xmlExtraData = enumConverter.ConvertToString(m_TrackExtraData);
            _xmlWriter.WriteElementString("ExtraData", xmlExtraData);
            
            TrackPointsToXml(_xmlWriter);
            
            _xmlWriter.WriteStartElement("DrawingStyle");
            m_Style.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();
            
            _xmlWriter.WriteStartElement("MainLabel");
            _xmlWriter.WriteAttributeString("Text", m_MainLabelText);
            m_MainLabel.WriteXml(_xmlWriter);
            _xmlWriter.WriteEndElement();

            if (m_KeyframesLabels.Count > 0)
            {
                _xmlWriter.WriteStartElement("KeyframeLabelList");
                _xmlWriter.WriteAttributeString("Count", m_KeyframesLabels.Count.ToString());

                foreach (KeyframeLabel kfl in m_KeyframesLabels)
                {
                    _xmlWriter.WriteStartElement("KeyframeLabel");
                    kfl.WriteXml(_xmlWriter);
                    _xmlWriter.WriteEndElement();    
                }

                _xmlWriter.WriteEndElement();
            }
		}
        private void TrackPointsToXml(XmlWriter _xmlWriter)
        {
            _xmlWriter.WriteStartElement("TrackPointList");
            _xmlWriter.WriteAttributeString("Count", m_Positions.Count.ToString());
            _xmlWriter.WriteAttributeString("UserUnitLength", m_ParentMetadata.CalibrationHelper.GetLengthAbbreviation());
            
            // The coordinate system defaults to the first point,
            // but can be specified by user.
            Point coordOrigin = m_Positions[0].Point;

            if(m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.X >= 0 || m_ParentMetadata.CalibrationHelper.CoordinatesOrigin.Y >= 0)
            	coordOrigin = m_ParentMetadata.CalibrationHelper.CoordinatesOrigin;
            
            if(m_Positions.Count > 0)
            {
            	foreach (AbstractTrackPoint tp in m_Positions)
            	{
            	    _xmlWriter.WriteStartElement("TrackPoint");
            	    
            	    // Data in user units.
                    // - The origin of the coordinates system is given as parameter.
                    // - X goes left (same than internal), Y goes up (opposite than internal).
                    // - Time is absolute.
                    double userX = m_ParentMetadata.CalibrationHelper.GetLengthInUserUnit((double)tp.X - (double)coordOrigin.X);
                    double userY = m_ParentMetadata.CalibrationHelper.GetLengthInUserUnit((double)coordOrigin.Y - (double)tp.Y);
                    string userT = m_ParentMetadata.TimeStampsToTimecode(tp.T, TimeCodeFormat.Unknown, false);
        			
                    _xmlWriter.WriteAttributeString("UserX", String.Format("{0:0.00}", userX));
                    _xmlWriter.WriteAttributeString("UserXInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", userX));
                    _xmlWriter.WriteAttributeString("UserY", String.Format("{0:0.00}", userY));
                    _xmlWriter.WriteAttributeString("UserYInvariant", String.Format(CultureInfo.InvariantCulture, "{0:0.00}", userY));
                    _xmlWriter.WriteAttributeString("UserTime", userT);
            
            		tp.WriteXml(_xmlWriter);
            		
            		_xmlWriter.WriteEndElement();
            	}	
            }
            _xmlWriter.WriteEndElement();
        }
        public void ReadXml(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
        {
            m_Invalid = true;
                
            if (_remapTimestampCallback == null)
            {
                string unparsed = _xmlReader.ReadOuterXml();
                log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                return;
            }
            
            _xmlReader.ReadStartElement();
			
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
				switch(_xmlReader.Name)
				{
					case "TimePosition":
				        m_iBeginTimeStamp = _remapTimestampCallback(_xmlReader.ReadElementContentAsLong(), false);
                        break;
					case "Mode":
                        {
                            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackView));
						    m_TrackView = (TrackView)enumConverter.ConvertFromString(_xmlReader.ReadElementContentAsString());
						    break;
                        }
				    case "ExtraData":
						{
    						TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(TrackExtraData));
    						m_TrackExtraData = (TrackExtraData)enumConverter.ConvertFromString(_xmlReader.ReadElementContentAsString());
    						break;
						}
					case "TrackPointList":
                        ParseTrackPointList(_xmlReader, _scale, _remapTimestampCallback);
						break;
				    case "DrawingStyle":
						m_Style = new DrawingStyle(_xmlReader);
						BindStyle();
						break;
                    case "MainLabel":
						{
						    m_MainLabelText = _xmlReader.GetAttribute("Text");
						    m_MainLabel = new KeyframeLabel(_xmlReader, _scale);
                            break;
						}
		            case "KeyframeLabelList":
						ParseKeyframeLabelList(_xmlReader, _scale);
						break;
					default:
						string unparsed = _xmlReader.ReadOuterXml();
						log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
						break;
				}
			}
			
			_xmlReader.ReadEndElement();
            
			if (m_Positions.Count > 0)
            {
                m_iEndTimeStamp = m_Positions.Last().T;
                m_MainLabel.SetAttach(m_Positions[0].Point, false);
                m_MainLabel.SetText(Label);
                
                if(m_Positions.Count > 1 || 
                   m_Positions[0].X != 0 || 
                   m_Positions[0].Y != 0 || 
                   m_Positions[0].T != 0)
                {
                    m_Invalid = false;
                }
            }
        }
        public void ParseTrackPointList(XmlReader _xmlReader, PointF _scale, TimeStampMapper _remapTimestampCallback)
        {
            m_Positions.Clear();
            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                if(_xmlReader.Name == "TrackPoint")
				{
                    AbstractTrackPoint tp = m_Tracker.CreateOrphanTrackPoint(0, 0, 0);
                    tp.ReadXml(_xmlReader);
                    
                    // time was stored in relative value, we still need to adjust it.
                    AbstractTrackPoint adapted = m_Tracker.CreateOrphanTrackPoint(	
                                                             	(int)(_scale.X * tp.X),
                                                            	(int)(_scale.Y * tp.Y),
                                                            	_remapTimestampCallback(tp.T, true));

                    m_Positions.Add(adapted);
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
				    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
        }
        public void ParseKeyframeLabelList(XmlReader _xmlReader, PointF _scale)
        {
            m_KeyframesLabels.Clear();

            _xmlReader.ReadStartElement();
            
            while(_xmlReader.NodeType == XmlNodeType.Element)
			{
                if(_xmlReader.Name == "KeyframeLabel")
				{
                    KeyframeLabel kfl = new KeyframeLabel(_xmlReader, _scale);
                    
                    if (m_Positions.Count > 0)
                    {
                        // Match with TrackPositions previously found.
                        int iMatchedTrackPosition = FindClosestPoint(kfl.Timestamp, m_Positions);
                        kfl.AttachIndex = iMatchedTrackPosition;
                        
                        kfl.SetAttach(m_Positions[iMatchedTrackPosition].Point, false);
                        m_KeyframesLabels.Add(kfl);
                    }
                }
                else
                {
                    string unparsed = _xmlReader.ReadOuterXml();
				    log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                }
            }
            
            _xmlReader.ReadEndElement();
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
                if (m_ParentMetadata[i].Position > m_iBeginTimeStamp && 
                    m_ParentMetadata[i].Position <= m_Positions.Last().T)
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
                        m_KeyframesLabels[iKnown].SetText(m_ParentMetadata[i].Title);
                    }
                    else
                    {
                        // Unknown Keyframe, Configure and add it to list.
                        KeyframeLabel kfl = new KeyframeLabel();
                        kfl.AttachIndex = FindClosestPoint(m_ParentMetadata[i].Position);
                        kfl.SetAttach(m_Positions[kfl.AttachIndex].Point, true);
                        kfl.Timestamp = m_Positions[kfl.AttachIndex].T;                        
                        kfl.SetText(m_ParentMetadata[i].Title);
                        
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
	                m_KeyframesLabels[iKfl].SetText(GetExtraDataText(m_KeyframesLabels[iKfl].AttachIndex));
            }
            
        }
        public override int GetHashCode()
        {
            // Combine all relevant fields with XOR to get the Hash.
            int iHash = 0;
            iHash ^= m_TrackView.GetHashCode();
            foreach (AbstractTrackPoint p in m_Positions)
                iHash ^= p.GetHashCode();

            iHash ^= m_iDefaultCrossRadius.GetHashCode();
            iHash ^= m_StyleHelper.GetHashCode();
            iHash ^= m_MainLabel.GetHashCode();

            foreach (KeyframeLabel kfl in m_KeyframesLabels)
                iHash ^= kfl.GetHashCode();

            return iHash;
        }
        public void MemorizeState()
        {
        	// Used by formConfigureTrajectory to be able to modify the trajectory in real time.
        	m_MemoTrackView = m_TrackView;
        	m_MemoLabel = m_MainLabelText;
        }
        public void RecallState()
        {
        	// Used when the user cancels his modifications on formConfigureTrajectory.
        	// m_StyleHelper has been reverted already as part of style elements framework.
        	// This in turn triggered mainStyle_ValueChanged() event handler so the m_MainLabel has been reverted already too.
        	m_TrackView = m_MemoTrackView;
        	m_MainLabelText = m_MemoLabel;
        }
        #endregion
		
		#region Miscellaneous private methods
        private int FindClosestPoint(long _iCurrentTimestamp)
        {
            return FindClosestPoint(_iCurrentTimestamp, m_Positions);
        }
        private int FindClosestPoint(long _iCurrentTimestamp, List<AbstractTrackPoint> _Positions)
        {
            // Find the closest registered timestamp
            // Parameter is given in absolute timestamp.
            long minErr = long.MaxValue;
            int iClosest = 0;

            for (int i = 0; i < _Positions.Count; i++)
            {
                long err = Math.Abs(_Positions[i].T - _iCurrentTimestamp);
                if (err < minErr)
                {
                    minErr = err;
                    iClosest = i;
                }
            }

            return iClosest;
        }
        private void mainStyle_ValueChanged(object sender, EventArgs e)
        {
        	m_MainLabel.BackColor = m_StyleHelper.Color;	
        }
        private void BindStyle()
        {
            m_Style.Bind(m_StyleHelper, "Color", "color");
            m_Style.Bind(m_StyleHelper, "LineSize", "line size");
            m_Style.Bind(m_StyleHelper, "TrackShape", "track shape");
        }
        #endregion
    }

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

}
