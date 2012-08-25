#region License
/*
Copyright © Joan Charmant 2012.
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
using System.Linq;

using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Manages the drawing trackers.
    /// </summary>
    public class TrackabilityManager
    {
        private Dictionary<Guid, DrawingTracker> trackers = new Dictionary<Guid, DrawingTracker>();

        public void Add(ITrackable drawing, VideoFrame videoFrame)
        {
            if(trackers.ContainsKey(drawing.ID))
               return;
            
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            trackers.Add(drawing.ID, new DrawingTracker(drawing, context));
        }
        
        public void Clear()
        {
            foreach(DrawingTracker tracker in trackers.Values)
                tracker.Dispose();
            
            trackers.Clear();
        }
        
        public void Remove(ITrackable drawing)
        {
            if(!trackers.ContainsKey(drawing.ID))
                throw new ArgumentException("This drawing was not registered for tracking.");
            
            trackers[drawing.ID].Dispose();
            trackers.Remove(drawing.ID);
        }

        public void Track(VideoFrame videoFrame)
        {
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            
            foreach(DrawingTracker tracker in trackers.Values)
            {
                tracker.Track(context);
            }
        }
        
        public bool IsTracking()
        {
            return trackers.Values.Any((tracker) => tracker.IsTracking);
        }
        
        public bool IsTracking(ITrackable drawing)
        {
             if(!trackers.ContainsKey(drawing.ID))
                throw new ArgumentException("This drawing was not registered for tracking.");
            
            return trackers[drawing.ID].IsTracking;
        }
        
        public void UpdateContext(ITrackable drawing, VideoFrame videoFrame)
        {
            if(!trackers.ContainsKey(drawing.ID))
                throw new ArgumentException("This drawing was not registered for tracking.");
            
            TrackingContext context = new TrackingContext(videoFrame.Timestamp, videoFrame.Image);
            
            trackers[drawing.ID].Track(context);
        }
        
        public void ToggleTracking(ITrackable drawing)
        {
            if(!trackers.ContainsKey(drawing.ID))
                throw new ArgumentException("This drawing was not registered for tracking.");
            
            trackers[drawing.ID].ToggleTracking();
        }
    }
}
