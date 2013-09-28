#region License
/*
Copyright © Joan Charmant 21/08/2012.
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
using System.Drawing;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represent a point that can be tracked in time. Hosts a timeline and current value.
    /// Tracking uses the closest known data point.
    /// If the point is not currently tracked, a separate value is kept outside the timeline.
    /// </summary>
    public class TrackablePoint
    {
        public Point CurrentValue
        {
            get { return currentValue; }
        }
        
        private bool isTracking;
        private Point currentValue;
        private TrackingContext context;
        private TrackerParameters trackerParameters;
        private Timeline<TrackFrame> trackTimeline = new Timeline<TrackFrame>();
        private Point nonTrackingValue;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TrackablePoint(TrackingContext context, TrackerParameters trackerParameters, Point value)
        {
            this.context = context;
            this.trackerParameters = trackerParameters;
            this.currentValue = value;
            this.nonTrackingValue = value;
        }
        
        /// <summary>
        /// Value adjusted by user.
        /// </summary>
        public void SetUserValue(Point value)
        {
            // For simplicity we consider this a change of target and invalidate all existing data.
            // A more clever technique would be to test for similarity with the second closest patch and invalidate more or less 
            // data depending on whether it's a change of target or just adjustment.
            
            // The context should have been set at Track() time when we landed on the video frame.
            currentValue = value;
            nonTrackingValue = value;
            
            if(!isTracking)
                return;

            if (trackerParameters.ResetOnMove)
                ClearTimeline();
            
            trackTimeline.Insert(context.Time, CreateTrackFrame(value, PositionningSource.Manual));
        }
        
        /// <summary>
        /// Track the point in the current image, or use the existing data if already known.
        /// </summary>
        /// <param name="context"></param>
        public void Track(TrackingContext context)
        {
            this.context = context;
           
            if(!isTracking)
                return;
            
            TrackFrame closestFrame = trackTimeline.ClosestFrom(context.Time);
            
            if(closestFrame == null)
                throw new InvalidOperationException("Tracking called before the trackable point was initialized.");
            
            if(closestFrame.Time == context.Time)
            {
                currentValue = closestFrame.Location;
                return;
            }
            
            TrackResult result = Tracker.Track(trackerParameters.SearchWindow, closestFrame, context.Image);

            if(result.Similarity >= trackerParameters.SimilarityThreshold)
            {
                currentValue = result.Location;
                
                if(result.Similarity > trackerParameters.TemplateUpdateThreshold)
                {
                    Bitmap template = closestFrame.Template.CloneDeep();
                    TrackFrame newFrame = new TrackFrame(context.Time, result.Location, template, PositionningSource.TemplateMatching);
                    trackTimeline.Insert(context.Time, newFrame);
                }
                else
                {
                    trackTimeline.Insert(context.Time, CreateTrackFrame(result.Location, PositionningSource.TemplateMatching));  
                }
                
            }
            else
            {
               currentValue = closestFrame.Location;
            }
        }
        
        public void Reset()
        {
            ClearTimeline();
        }
       
        public void SetTracking(bool isTracking)
        {
            if(this.isTracking == isTracking)
                return;
            
            this.isTracking = isTracking;
            
            if(!isTracking)
            {
                nonTrackingValue = currentValue;
            }
            else
            {
                //SetTemplateAndSearchWindowSizes();
                
                currentValue = nonTrackingValue;
                ClearTimeline();
                trackTimeline.Insert(context.Time, CreateTrackFrame(currentValue, PositionningSource.Manual)); 
            }
        }
        
        private TrackFrame CreateTrackFrame(Point location, PositionningSource positionningSource)
        {
            Rectangle region = location.Box(trackerParameters.BlockWindow);
            Bitmap template = context.Image.ExtractTemplate(region);
            return new TrackFrame(context.Time, location, template, positionningSource);
        }

        private void ClearTimeline()
        {
            trackTimeline.Clear((frame) => frame.Template.Dispose());
        }
        
    }
}
