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

using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The drawing tracker is responsible for storing the timeline of values for each trackable point in the drawing.
    /// It knows about the drawing it tracks (via ITrackable) and updates it by pushing values directly to it.
    /// The drawing doesn't know about its tracker, it just raises events when the user changes the point manually.
    /// </summary>
    public class DrawingTracker
    {
        public bool IsTracking 
        { 
            get { return isTracking; }
        }
        
        private ITrackable drawing;
        private bool isTracking;
        private Dictionary<string, TrackablePoint> trackablePoints = new Dictionary<string, TrackablePoint>();
        
        public DrawingTracker(ITrackable drawing, TrackingContext context)
        {
            this.drawing = drawing;
            
            foreach(KeyValuePair<string, Point> pair in drawing.GetTrackablePoints())
                trackablePoints.Add(pair.Key, new TrackablePoint(context, pair.Value));
            
            drawing.TrackablePointMoved += drawing_TrackablePointMoved;
        }
  
        public void Track(TrackingContext context)
        {
            // This is where we would spawn new threads for each tracking.
            // TODO: Extract the bitmapdata once and pass it to all.
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                pair.Value.Track(context);
                
                if(isTracking)
                    drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue);
            }
        }
        
        public void ToggleTracking()
        {
            isTracking = !isTracking;
            
            foreach(KeyValuePair<string, TrackablePoint> pair in trackablePoints)
            {
                pair.Value.SetTracking(isTracking);
                if(!isTracking)
                    drawing.SetTrackablePointValue(pair.Key, pair.Value.CurrentValue);
            }
            
            drawing.SetTracking(isTracking);
        }
        
        public void Dispose()
        {
            foreach(TrackablePoint trackablePoint in trackablePoints.Values)
                trackablePoint.Dispose();
            
            drawing.TrackablePointMoved -= drawing_TrackablePointMoved;
        }
        
        private void drawing_TrackablePointMoved(object sender, TrackablePointMovedEventArgs e)
        {
            if(!trackablePoints.ContainsKey(e.PointName))
                throw new ArgumentException("This point is not bound.");
            
            trackablePoints[e.PointName].SetUserValue(e.Position);
        }
    }
}
