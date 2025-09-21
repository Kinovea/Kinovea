#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{

    /// <summary>
    /// Drawings that have points that are "dynamic" and can change coordinates from outside the drawing, 
    /// without user intervention, based on the current frame.
    /// This change in coordinates can come from object tracking or camera tracking.
    /// 
    /// ContentHash: these drawings should not include the trackable points coordinates in the content hash.
    /// It is the responsibility of their DrawingTracker to keep track of the actual change
    /// in the reference value.
    /// Similarly for KVA export, they should ask the DrawingTracker to provide the reference value.
    /// Note that the reference value is in relation to a specific frame, not necessarily the keyframe.
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Id of the drawing. This is used by tracking managers to keep a timeline of 
        /// coordinates indexed by drawing id.
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Name of the drawing. This is used by the kinematics diagrams.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Color of the drawing. This is used by the kinematics diagrams.
        /// </summary>
        Color Color { get; }

        /// <summary>
        /// The reference timestamp of the drawing.
        /// This is the timestamp of the last time the user manually placed the drawing.
        /// It is not necessarily the timestamp of the parent keyframe.
        /// The user placed the object on top of world elements at this specific frame, 
        /// so the camera transform must be relative to this frame.
        /// </summary>
        long ReferenceTimestamp { get; set; }
        
        /// <summary>
        /// Tracking parameters.
        /// </summary>
        TrackingParameters CustomTrackingParameters { get; }
        
        /// <summary>
        /// Returns the list of trackable points.
        /// </summary>
        Dictionary<string, PointF> GetTrackablePoints();

        // Note:
        // Trackable drawings that support fading will also store a member named "trackingTimestamps".
        // This is the time distance between the current time and the time of the closest tracked value.
        // A value of -1 indicates the drawing has no tracking data.
        // This is used to handle opacity for tracked drawings.

        /// <summary>
        /// Called by the trackability manager during the Track() step at video time change.
        /// The value of the trackable point must be updated to reflect the tracked coordinate.
        /// trackingTimestamps indicates the distance between the current video time 
        /// and the closest entry in the timeline.
        /// </summary>
        void SetTrackablePointValue(string name, PointF value, long trackingTimestamps);
        
        /// <summary>
        /// Event raised by trackable drawings when the points are moved manually.
        /// </summary>
        event EventHandler<TrackablePointMovedEventArgs> TrackablePointMoved;
    }
}
