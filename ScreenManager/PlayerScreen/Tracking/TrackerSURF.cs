#region License
/*
Copyright © Joan Charmant 2010.
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
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenSURF;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// TrackerSURF uses Surf feature matching to perform tracking.
	/// See http://en.wikipedia.org/wiki/SURF
	/// It uses TrackPointSURF to describe a tracked point.
	/// 
	/// We do not track the user point. Rather we track the SURF feature that is
	/// the closest to the user point, and we keep a way to retrieve the actual user point 
	/// coordinate from the tracked SURF feature (even if rotation occured).
	/// 
	/// If at a given point we find that there is a new feature that is even more closer to 
	/// the computed user point, we may use it instead… (?).
	/// If we can't find the feature, should we retry tracking with the next closer one or stop tracking ?
	/// 
	/// NOT FUNCTIONNAL.
	/// As of 2010/02/07, this tracker is not yet functionnal. 
	/// - Finding features in the current frame is too dependant on the initial parameters.
	/// - The features tracked at image I are not the same as the ones tracked at image I+1.
	/// - feature matching seems to work and the general feature update mechanism for the 
	/// tracker is implemented.
	/// If we could get stable SURF features, it would work. 
	/// (works for very simple tracking cases with tuned parameters.)
	/// 
	/// Working:
	/// To find the point in image I:
	/// - finds a list of feature in image I, around point found in image I-1.
	/// - track the match of I-1 in the list of features found in image I.
	/// - match is saved in coordinates relative to the search window it was found in.
	/// </summary>
	public class TrackerSURF : AbstractTracker
	{
		#region Members		
       	private Size m_SearchWindowSize = new Size(100, 100);	// Size of window for feature finding.
		
       	private int m_iOctaves = 1;								// octave * intervals = number of layers in the scale-space.
        private int m_iIntervals = 3;							// interval: number of layer in an octave (min=3). octave: doubling the size (min=1).	
        private int m_iInitSample = 1;							// initial sampling factor (?). Lower = scales closer to one another ?
       	private float m_fThreshold = 0.00001f;				// Filter out bad features. (can be 0).
        private int m_iInterpolationSteps = 20;					// Increase stability ?
		private bool m_bUpright = false;
		
		// Monitoring, debug.
		private static readonly bool m_bMonitoring = true;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       	#endregion
		
		#region AbstractTracker Implementation
		public override bool Track(List<AbstractTrackPoint> _previousPoints, Bitmap _CurrentImage, long _t, out AbstractTrackPoint _currentPoint)
		{
			//---------------------------------------------------------------------
			// The input informations we have at hand are:
			// - The current bitmap we have to find the point into.
			// - The coordinates of all the previous points tracked.
			// - Previous tracking infos, stored in the TrackPoints tracked so far.
			//---------------------------------------------------------------------
			
			bool bMatched = false;
			
			TrackPointSURF lastTrackPoint = (TrackPointSURF)_previousPoints[_previousPoints.Count -1];
			
			// Create a point centered on last match.
			// This will find and register all the SURF features located in the search zone.
			
			// Test with grayscale image.
			Bitmap grayCurrentImage =  Grayscale.CommonAlgorithms.BT709.Apply(_CurrentImage);
					
			_currentPoint = CreateTrackPoint(false, lastTrackPoint.X, lastTrackPoint.Y, 1.0f, _t, grayCurrentImage, _previousPoints);
			
			if(_currentPoint == null)
			{
				// Untrackable area.
			}
			else
			{
				if(((TrackPointSURF)_currentPoint).FoundFeatures.Count > 0)
				{
					// Feature matching.
					// Look for the nearest neighbour to the previous match, in the list of newly found features.
					Match m = null;
					COpenSURF.MatchPoint(lastTrackPoint.MatchedFeature, ((TrackPointSURF)_currentPoint).FoundFeatures, out m);
					
					// Also look for a match of the first feature, to compensate for occlusion and drift).
					Match m2 = null;
					TrackPointSURF firstTrackPoint = (TrackPointSURF)_previousPoints[0];
					COpenSURF.MatchPoint(firstTrackPoint .MatchedFeature, ((TrackPointSURF)_currentPoint).FoundFeatures, out m2);
					
					// Take the best match out of the two.
					Match matchedFeature = (m.Distance2 < m2.Distance2) ? m : m2;
					
					// TODO:
					// check if distance (match similarity) is over a given threshold.
					
					// 3. Store the new matched feature with associated data.
					((TrackPointSURF)_currentPoint).MatchedFeature = matchedFeature.Ipt2;

					_currentPoint.X = ((TrackPointSURF)_currentPoint).SearchWindow.X + (int)matchedFeature.Ipt2.x;
		    		_currentPoint.Y = ((TrackPointSURF)_currentPoint).SearchWindow.Y + (int)matchedFeature.Ipt2.y;
					
		    		log.Debug(String.Format("Tracking result: [{0};{1}]", _currentPoint.X, _currentPoint.Y));
		    		bMatched = true;
				}
				
				if(m_bMonitoring)
				{
					log.Debug(_currentPoint.ToString());
				}
				
				// Problems:
				// The user did not choose a feature, so we have extra work to do to keep the correspondance between
				// the feature saved in the track point and the actual coordinates the user is looking for.
				// Currently we just discard the user's point entirely and try to track the closest feature.
			}
			return bMatched;
		}
		public override AbstractTrackPoint CreateTrackPoint(bool _manual, int _x, int _y, double _fSimilarity, long _t, Bitmap _CurrentImage, List<AbstractTrackPoint> _previousPoints)
		{
			// Creates a TrackPoint from the input image at the given coordinates.
    		// Find features in the search window.
    		
    		// Scale-space image of the search window.
    		int searchLeft = _x - (m_SearchWindowSize.Width/2);
    		int searchTop = _y - (m_SearchWindowSize.Height/2);
			Rectangle searchZone = new Rectangle(searchLeft, searchTop, m_SearchWindowSize.Width, m_SearchWindowSize.Height);
			Bitmap searchImage = new Bitmap(m_SearchWindowSize.Width, m_SearchWindowSize.Height, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(searchImage);
			Rectangle rDst = new Rectangle(0, 0, m_SearchWindowSize.Width, m_SearchWindowSize.Height);
			g.DrawImage(_CurrentImage, rDst, searchZone, GraphicsUnit.Pixel);
			//g.Dispose();
			
			IplImage pIplImage = IplImage.LoadImage(searchImage);
            pIplImage = pIplImage.BuildIntegral(null);

            List<Ipoint> ipts = new List<Ipoint>();
            CFastHessian pCFastHessian = new CFastHessian(pIplImage, ref ipts, m_iOctaves, m_iIntervals, m_iInitSample, m_fThreshold, m_iInterpolationSteps);
            
            // Fill the scale-space image with actual data and finds the local extrema.
            pCFastHessian.getIpoints();
            
            // Fill the descriptor field, orientation and laplacian of the feature.
            Surf pSurf = new Surf(pIplImage, ipts);
            pSurf.getDescriptors(m_bUpright);
            
            // Save algorithm-related data in the point.
            TrackPointSURF tps = new TrackPointSURF(_x, _y, _t);
            tps.FoundFeatures = ipts;
            
            tps.SearchWindow = new Point(searchLeft, searchTop);
            
            if(_previousPoints.Count == 0 || _manual)
            {
            	// Find the closest point from the user's point.
            	int iClosest = -1;
		        double fBestDistance = double.MaxValue;
	            if(ipts.Count > 0)
	            {
		            Point userPoint = new Point(_x, _y);
		            for(int i=0; i<ipts.Count;i++)
		            {
		            	double fDistance = CalibrationHelper.PixelDistance(new PointF((float)_x, (float)_y), new PointF(ipts[i].x + searchLeft, ipts[i].y + searchTop));
		            	if(fDistance < fBestDistance)
		            	{
		            		fBestDistance = fDistance;
		            		iClosest = i;
		            	}
		            }
		        	
		            tps.MatchedFeature = tps.FoundFeatures[iClosest];
	            	tps.X = (int)(tps.MatchedFeature.x + searchLeft);
	            	tps.Y = (int)(tps.MatchedFeature.y + searchTop);
		        
	            	log.Debug(String.Format("Initializing of the tracking. Closest feature to user's point : {0:0.00}", fBestDistance));
	            	log.Debug(String.Format("Tracking result (init): [{0};{1}], user selection was: [{2};{3}]", tps.X, tps.Y, _x, _y));
	            }
	            else
	            {
	            	// Ouch! The point selected by the user is in a no-feature zone.
	            	tps = null;
	            	log.Debug(String.Format("Tracking impossible from this point. Selected point is in No-feature zone."));
	            }
            }
            
			return tps;
		}
		public override AbstractTrackPoint CreateOrphanTrackPoint(int _x, int _y, long _t)
        {
        	// This creates a bare bone TrackPoint.
        	// This is used only in the case of importing from xml.
        	// The TrackPoint can't be used as is to track the next one because it's missing the algo internal data.
        	// We'll need to reconstruct it when we have the corresponding image.
        	return new TrackPointSURF(_x, _y, _t);
        }
		public override void Draw(Graphics _canvas, AbstractTrackPoint _currentPoint, Point _directZoomTopLeft, double _fStretchFactor, Color _color, double _fOpacityFactor)
		{
			// Current Search window.
            int iSrchLeft = _currentPoint.X - (int)(((double)m_SearchWindowSize.Width * _fStretchFactor) / 2);
            int iSrchTop = _currentPoint.Y - (int)(((double)m_SearchWindowSize.Height * _fStretchFactor) / 2);
            
            Rectangle SrchZone = new Rectangle(iSrchLeft, iSrchTop, (int)((double)m_SearchWindowSize.Width * _fStretchFactor), (int)((double)m_SearchWindowSize.Height * _fStretchFactor));
            _canvas.DrawRectangle(new Pen(Color.FromArgb((int)(64.0f * _fOpacityFactor), _color)), SrchZone);
			
            // Features coordinates are relative to the previous search window.
            foreach (Ipoint p in ((TrackPointSURF)_currentPoint).FoundFeatures)
            {
            	// Use inverse color for failed points.
            	Color invert = Color.FromArgb(64, 255 - _color.R, 255 - _color.G, 255 - _color.B);
            	DrawFeature(_canvas, invert, 1, p, ((TrackPointSURF)_currentPoint).SearchWindow);
            }
            
            DrawFeature(_canvas, _color, 2, ((TrackPointSURF)_currentPoint).MatchedFeature, ((TrackPointSURF)_currentPoint).SearchWindow);
		}
		
		#endregion
		
		#region Private helpers
		private void DrawFeature(Graphics _canvas, Color _color, int _width, Ipoint _point, Point _topLeft)
		{
			int xd = (int)(_point.x + _topLeft.X);
			int yd = (int)(_point.y + _topLeft.Y);
            float scale = _point.scale;
            float orientation = _point.orientation;
            float radius = ((9.0f / 1.2f) * scale) / 3.0f;
            
            Pen penPoint = new Pen(_color, _width);
            _canvas.DrawEllipse(penPoint, xd - radius, yd - radius, 2 * radius, 2 * radius);

            double dx = radius * Math.Cos(orientation);
            double dy = radius * Math.Sin(orientation);
            _canvas.DrawLine(penPoint, new Point(xd, yd), new Point((int)(xd+dx),(int)(yd+dy)));
	
            penPoint.Dispose();
		}
		#endregion
	}
}
