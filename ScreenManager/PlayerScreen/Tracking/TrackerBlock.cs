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
using System.IO;
using AForge.Imaging;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// TrackerBlock uses simple Template Matching to perform tracking.
	/// It uses TrackPointBlock to describe a tracked point.
	/// 
	/// Implements the naive template matching algorithm:
	/// - Similarity is computed from pixel values at corresponding positions in the candidate block.
	/// - Forecasted location from previous motion also weights in the final selection.
	/// - Template is updated at each step. (may causes drift known as Template Update Paradox).
	/// 
	/// Options:
	/// - Similarity treshold can be used to prefilter good candidates.
	/// - UpdateStrategy, to update the template at each step, keep the original one, or mix them.
	/// - Search window and template size.
	/// - ForecastDistanceWeight, used to create a bias toward the current trend in motion.
	/// - Work on grayscale, the image and template are converted prior to attempt matching.
	///
	/// Notes:
	/// - for UpdateStrategy.Original, higher similarity treshold means the track will fail more quickly.
	/// - for UpdateStrategy.Current, lower similarity treshold means the track will be more subject to drift.
	/// - TODO: check the feature richness of the block to evaluate the similarity measure robustness.
	/// 
	/// Working:
	/// To find the point in image I:
	/// - use the template found in image I-1.
	/// - may use other reference template, also stored in point at image I-1.
	/// - save the template in point at image I.
	/// - no need to save the relative search window as points are saved in absolute coords. 
	/// </summary>
	public class TrackerBlock : AbstractTracker
	{
		#region Enum
		private enum UpdateStrategy
		{
			Original,				// Always use the original block selected by the user to track the current block.
			Current,				// Always use the last block tracked to track the current block.
			Mixed,					// Averages the original and current block to create an artificial block to track.
			Both					// Tracks both the original block and the current block and chooses the best.
		}
		#endregion
		
		#region Members
		
		// Options
		private static readonly float m_fSimilarityTreshold = 0.85f;			// Discard candidate block with lower similarity.
		private static readonly float m_fScoreTreshold = 0.85f;
		private static readonly float m_fOriginalSimilarityThreshold = 0.97f; 	// used in UpdateStrategy.Both.
		private static readonly double m_fForecastDistanceWeight = 0.25f;
		private static readonly bool m_bWorkOnGrayscale = true;
		
		// Update strategy.
		//private static readonly UpdateStrategy m_UpdateStrategy = UpdateStrategy.Current;
		private static readonly UpdateStrategy m_UpdateStrategy = UpdateStrategy.Both;
		//private static readonly UpdateStrategy m_UpdateStrategy = UpdateStrategy.Original;
		//private static readonly UpdateStrategy m_UpdateStrategy = UpdateStrategy.Mixed;
		
		private Size m_BlockSize = new Size(20, 20);						// Size of block to be matched.
		private Size m_SearchWindowSize = new Size(100, 100);				// Size of window of candidates.
		private double m_fMaxDistance = CalibrationHelper.PixelDistance(new Point(0,0), new Point(40, 40));
		
		// Monitoring, debugging.
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
			
			TrackPointBlock lastTrackPoint = (TrackPointBlock)_previousPoints[_previousPoints.Count -1];
			Point lastPoint = lastTrackPoint.ToPoint();
			
			// Compute the projected point.
			// (coordinate of the point that would keep following the same motion as in last step).
			Point forecast;
            if(_previousPoints.Count > 1)
            {
        		Point penultimate 	= _previousPoints[_previousPoints.Count - 2].ToPoint();
        		
        		int dx = lastPoint.X - penultimate.X;
        		int dy = lastPoint.Y - penultimate.Y;
        		
        		forecast = new Point(lastPoint.X + dx, lastPoint.Y + dy);
    		}
            else
            {
            	forecast = _previousPoints[0].ToPoint();
            }
            
			bool bMatched = false;
			_currentPoint = null;
			
			if (lastTrackPoint.Template != null && _CurrentImage != null)
            {
				// Center search zone around last point.
				Point searchCenter = lastPoint;
            	Rectangle searchZone = new Rectangle(	searchCenter.X - (m_SearchWindowSize.Width/2), 
				                                     	searchCenter.Y - (m_SearchWindowSize.Height/2), 
				                                     	m_SearchWindowSize.Width, 
				                                     	m_SearchWindowSize.Height);
	        	
				// Convert to grayscale prior to match, if necessary.
				Bitmap workingImage = m_bWorkOnGrayscale ? Grayscale.CommonAlgorithms.BT709.Apply(_CurrentImage) : _CurrentImage;
	        	
				double fBestScore = 0;
				Point bestCandidate = new Point(-1,-1);
				
				if(m_UpdateStrategy == UpdateStrategy.Both)
        		{
        			// Try to match the initial reference block in the image first. 
        			// If it gets a score over a given threshold, we give it the priority over the I-1 block.
        			// This is an attempt at correcting the drift issue.
        			
        			// Find the last reference block. (last block manually choosen by user.)
        			int iLastReferenceBlock = 0;
        			for(int b = _previousPoints.Count-1;b>=0;b--)
        			{
        				if(((TrackPointBlock)_previousPoints[b]).IsReferenceBlock)
        				{
        					iLastReferenceBlock = b;
        					break;
        				}
        			}
        			
        			Bitmap originalTemplate = ((TrackPointBlock)_previousPoints[iLastReferenceBlock]).Template;
        			Bitmap workingOriginalTemplate = m_bWorkOnGrayscale ? Grayscale.CommonAlgorithms.BT709.Apply(originalTemplate) : originalTemplate; 
        			
        			ExhaustiveTemplateMatching originalMatcher = new ExhaustiveTemplateMatching(m_fOriginalSimilarityThreshold);
					TemplateMatch[] matchingsOriginal = originalMatcher.ProcessImage(workingImage, workingOriginalTemplate, searchZone);
				
					if (matchingsOriginal.Length > 0)
		            {
						// We found a block with a very good similarity to the original block selected by the user.
						// It will take precedence over the I-1 block.
						TemplateMatch tm = matchingsOriginal[0];
	            		bestCandidate = new Point(tm.Rectangle.Left + (tm.Rectangle.Width / 2), tm.Rectangle.Top + (tm.Rectangle.Height / 2) );
                		fBestScore = tm.Similarity;
	            		
                		if(m_bMonitoring)
	                		log.Debug(String.Format("Original template found with good similarity ({0:0.000}), {1} candidates.", tm.Similarity, matchingsOriginal.Length));
		        	}
        		}
				
				if(bestCandidate.X == -1 || bestCandidate.Y == 1)
				{
					Bitmap workingTemplate = m_bWorkOnGrayscale ? Grayscale.CommonAlgorithms.BT709.Apply(lastTrackPoint.Template) : lastTrackPoint.Template; 
        			
					ExhaustiveTemplateMatching templateMatcher = new ExhaustiveTemplateMatching(m_fSimilarityTreshold);
					TemplateMatch[] matchings = templateMatcher.ProcessImage(workingImage, workingTemplate, searchZone);
					
					if (matchings.Length > 0)
		            {
	            		// Find the best candidate.
	            		// Score is weighted average of : similarity and closeness to forecast.
	            		int iBestCandidate = -1;
	            		double fWinnerDistance = 0;
	            		for(int i=0;i<matchings.Length;i++)
	            		{
	            			TemplateMatch tm = matchings[i];
	            			//if(_previousPoints.Count > 1)
	            			{
	                			Point candidatePoint = new Point(tm.Rectangle.Left + (tm.Rectangle.Width / 2), tm.Rectangle.Top + (tm.Rectangle.Height / 2) );
	                			double fDistanceToForecast = CalibrationHelper.PixelDistance(candidatePoint, forecast);
	                			double fScore = GetScore(tm.Similarity, fDistanceToForecast, m_fMaxDistance);
	                			
	                			if(fScore > fBestScore)
	                			{
	                				fBestScore = fScore;
	                				fWinnerDistance = fDistanceToForecast;
	                				iBestCandidate = i;
	                				bestCandidate = candidatePoint;
	                			}
	            			}
	            		}
	            		if(m_bMonitoring)
	            		{
	            			log.Debug(String.Format("Last template found with : Score:{0:0.000}, Similarity:{1:0.000} (index:{2:00}/{3:00}), Distance to forecast (px):{4:0.00}",
	        		                        		fBestScore,
	                		                        matchings[iBestCandidate].Similarity,
	        		                        		iBestCandidate, 
	        		                        		matchings.Length, 
	        		                        		fWinnerDistance));	
	            		}
		        	}
				}
				
            	// Result of the matching.	
        		if(bestCandidate.X != -1 && bestCandidate.Y != -1)
        		{
            		// Save template in the point.
            		_currentPoint = CreateTrackPoint(false, bestCandidate.X, bestCandidate.Y, _t, _CurrentImage, _previousPoints);
	                
            		// Finally, it is only considered a match if the score is over the threshold.	
            		if(fBestScore >= m_fScoreTreshold || _previousPoints.Count == 1)
            		{
            			bMatched = true;
            		}
        		}
        		else
        		{
        			// No match. Create the point at the center of the search window (whatever that might be).
	        		_currentPoint = CreateTrackPoint(false, searchCenter.X, searchCenter.Y, _t, _CurrentImage, _previousPoints);
	        		log.Debug("Track failed. No block over the similarity treshold in the search window.");	
        		}
        		
                #region Monitoring
                if(m_bMonitoring)
				{
                	// Save current template to file, to visually monitor the drift.
                	string tplDirectory = @"C:\Documents and Settings\Administrateur\Mes documents\Dev  Prog\Videa\Video Testing\Tracking\Template Update";
                	if(_previousPoints.Count == 1)
                	{
                		// Clean up folder.
                		string[] tplFiles = Directory.GetFiles(tplDirectory, "*.bmp");
                		foreach (string f in tplFiles)
					    {
					        File.Delete(f);
					    }
                	}
                	String iFileName = String.Format("{0}\\tpl-{1:000}.bmp", tplDirectory, _previousPoints.Count);
					((TrackPointBlock)_currentPoint).Template.Save(iFileName);
				}
                #endregion
            }
			else
			{
				// No image. (error case ?)
				// Create the point at the last point location.
				_currentPoint = CreateTrackPoint(false, lastTrackPoint.X, lastTrackPoint.Y, _t, _CurrentImage, _previousPoints);
				log.Debug("Track failed. No input image, or last point doesn't have any cached block image.");
			}
			
			return bMatched;
		}
		public override AbstractTrackPoint CreateTrackPoint(bool _bManual, int _x, int _y, long _t, Bitmap _CurrentImage, List<AbstractTrackPoint> _previousPoints)
		{
			// Creates a TrackPoint from the input image at the given coordinates.
			// Stores algorithm internal data in the point, to help next match.
			// _t is in relative timestamps from the first point.
			
			Bitmap tpl = new Bitmap(m_BlockSize.Width, m_BlockSize.Height, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(tpl);
			
			UpdateStrategy strategy = m_UpdateStrategy;
			
			if(_bManual)
			{
				// No points yet, or the user manually changed the point.
				// Use the block from the current image.
				strategy = UpdateStrategy.Current;
			}
			
			switch(strategy)
			{
				case UpdateStrategy.Original:
					// No update, we keep the original block.
					g.DrawImage(((TrackPointBlock)_previousPoints[0]).Template, 0, 0);
					break;
				case UpdateStrategy.Mixed:
				{
					// Update the template with a mix of the current block around match and the original block selected.
					
					// Paste the new block.
					Rectangle rDst = new Rectangle(0, 0, m_BlockSize.Width, m_BlockSize.Height);
					Rectangle rSrc = new Rectangle(_x - (m_BlockSize.Width / 2), _y - (m_BlockSize.Height / 2), m_BlockSize.Width, m_BlockSize.Height);
					g.DrawImage(_CurrentImage, rDst, rSrc, GraphicsUnit.Pixel);
					
					// Paste the original block at 50%.
					ColorMatrix mergeMatrix = new ColorMatrix();
					mergeMatrix.Matrix00 = 1.0f;
					mergeMatrix.Matrix11 = 1.0f;
					mergeMatrix.Matrix22 = 1.0f;
					mergeMatrix.Matrix33 = 0.5f;
					mergeMatrix.Matrix44 = 1.0f;
					
					ImageAttributes mergeImgAttr = new ImageAttributes();
					mergeImgAttr.SetColorMatrix(mergeMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
							
					//g.DrawImage(m_SyncMergeImage, rSyncDst, 0, 0, m_SyncMergeImage.Width, m_SyncMergeImage.Height, GraphicsUnit.Pixel, m_SyncMergeImgAttr);
					Bitmap originalTpl = ((TrackPointBlock)_previousPoints[0]).Template;
					g.DrawImage(originalTpl, rDst, 0, 0, m_BlockSize.Width, m_BlockSize.Height, GraphicsUnit.Pixel, mergeImgAttr);

					break;
				}
				case UpdateStrategy.Current:
				case UpdateStrategy.Both:
				default:
				{
					// Update the template with the block around the new position.
					Rectangle rDst = new Rectangle(0, 0, m_BlockSize.Width, m_BlockSize.Height);
					Rectangle rSrc = new Rectangle(_x - (m_BlockSize.Width / 2), _y - (m_BlockSize.Height / 2), m_BlockSize.Width, m_BlockSize.Height);
					g.DrawImage(_CurrentImage, rDst, rSrc, GraphicsUnit.Pixel);
					break;
				}
			}
			
			TrackPointBlock tpb = new TrackPointBlock(_x, _y, _t, tpl);
			tpb.IsReferenceBlock = _bManual;
			
			return tpb;
		}
		public override AbstractTrackPoint CreateOrphanTrackPoint(int _x, int _y, long _t)
        {
        	// This creates a bare bone TrackPoint.
        	// This is used only in the case of importing from xml.
        	// The TrackPoint can't be used as-is to track the next one because it's missing the algo internal data (block).
        	// We'll need to reconstruct it when we have the corresponding image.
        	return new TrackPointBlock(_x, _y, _t);
        }
		public override void Draw(Graphics _canvas, AbstractTrackPoint _currentPoint, Point _directZoomTopLeft, double _fStretchFactor, Color _color, double _fOpacityFactor)
		{
			// Draws a visual indication of the algorithm.
			// This should help the user understand how the algorithm is working.
			// The visual information may only make sense for dev purposes though.
			
			double fX = (((double)_currentPoint.X - (double)_directZoomTopLeft.X)  * _fStretchFactor);
			double fY = (((double)_currentPoint.Y  - (double)_directZoomTopLeft.Y) * _fStretchFactor);
			
			// Current Search window.
			int iSrchLeft = (int) (fX - (((double)m_SearchWindowSize.Width * _fStretchFactor) / 2));
			int iSrchTop = (int) (fY - (((double)m_SearchWindowSize.Height * _fStretchFactor) / 2));
            Rectangle SrchZone = new Rectangle(iSrchLeft, iSrchTop, (int)((double)m_SearchWindowSize.Width * _fStretchFactor), (int)((double)m_SearchWindowSize.Height * _fStretchFactor));
            //_canvas.DrawRectangle(new Pen(Color.FromArgb((int)(64.0f * _fOpacityFactor), _color)), SrchZone);
			_canvas.FillRectangle(new SolidBrush(Color.FromArgb((int)(48.0f * _fOpacityFactor), _color)), SrchZone);
            
            // Current Block.
            int iTplLeft = (int) (fX - (((double)m_BlockSize.Width * _fStretchFactor) / 2));
            int iTplTop = (int) (fY - (((double)m_BlockSize.Height * _fStretchFactor) / 2));
            Rectangle TplZone = new Rectangle(iTplLeft, iTplTop, (int)((double)m_BlockSize.Width * _fStretchFactor), (int)((double)m_BlockSize.Height * _fStretchFactor));
            _canvas.DrawRectangle(new Pen(Color.FromArgb((int)(128.0f * _fOpacityFactor), _color)), TplZone);	
		}
		#endregion
		
		#region Private Helpers
		private double GetScore(float _fSimilarity, double _fDist, double _fMaxDistance)
        {
        	// Normalize distance relative to similarity. (i.e: map [40..0] to [0..1])
        	double fNormalizedDistance = 0;
			double fForecastDistanceWeight = m_fForecastDistanceWeight;
        	if(_fDist > _fMaxDistance)
        	{
        		// This can happen if the user manually moved a point outside the search window.
        		// In this case we should just take similarity in account.
        		fForecastDistanceWeight = 0;
        	}
        	else
        	{
        		fNormalizedDistance = 1.0f - (_fDist/_fMaxDistance);	
        	}
        	
        	// Weighted average : Distance to forecast point and similarity to previous block.
        	double fScore = ((1 - fForecastDistanceWeight ) * _fSimilarity) + (fForecastDistanceWeight  * fNormalizedDistance);
        	
        	return fScore;
        }
		#endregion
	}
}
