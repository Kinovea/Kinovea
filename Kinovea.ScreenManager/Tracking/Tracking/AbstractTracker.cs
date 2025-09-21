#region License
/*
Copyright © Joan Charmant 2010.
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
    /// AbstractTracker is the generic class for perforing tracking.
    /// This class is not to be instanciated, use a concrete tracker instead,
    /// like TrackerSURF or TrackerBlock. 
    /// </summary>
    public abstract class AbstractTracker : IDisposable
    {
        public abstract TrackingParameters Parameters { get; }


        #region Abstract Methods

        /// <summary>
        /// Returns true if the tracker is ready to track.
        /// </summary>
        public abstract bool IsReady(TimedPoint lastTrackedPoint);

        /// <summary>
        /// Performs the tracking. 
        /// Finds the coordinate in current image of the point tracked, using data from previous matches. 
        /// </summary>
        /// <param name="previousPoints">The list of tracked points so far.</param>
        /// <param name="currentImage">Current image as a Bitmap.</param>
        /// <param name="cvImage">Current image as a cv Mat.</param>
        /// <param name="timestamp">The current timestamp to create the TrackPoint.</param>
        /// <param name="currentPoint">The resulting point that should be added to the list.</param>
        /// <returns>true if the tracking is reliable, false if the point couldn't be found.</returns>
        public abstract bool TrackStep(List<TimedPoint> previousPoints, long time, OpenCvSharp.Mat cvImage, out TimedPoint currentPoint);
        
        /// <summary>
        /// Creates a track point from auto-tracking.
        /// </summary>
        public abstract TimedPoint CreateTrackPoint(object trackingResult, long time, OpenCvSharp.Mat cvImage, List<TimedPoint> previousPoints);

        /// <summary>
        /// Creates a Track point from a user-provided location.
        /// </summary>
        public abstract void CreateReferenceTrackPoint(TimedPoint point, OpenCvSharp.Mat cvImage);

        /// <summary>
        /// Trim internal data related to points after the passed time.
        /// </summary>
        public abstract void Trim(long time);

        /// <summary>
        /// Clear any internal state data.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Pass the current image to the tracker.
        /// This may be used by trackers that need to show a modified version of the image
        /// for feedback. (ex: HSV filtering).
        /// </summary>
        public abstract void UpdateImage(long timestamp, OpenCvSharp.Mat cvImage, List<TimedPoint> previousPoints);

        /// <summary>
        /// Draw the tracker gizmo.
        /// </summary>
        public abstract void Draw(Graphics canvas, TimedPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor, bool isConfiguring);

        public abstract void Dispose();

        #endregion
    }
}
