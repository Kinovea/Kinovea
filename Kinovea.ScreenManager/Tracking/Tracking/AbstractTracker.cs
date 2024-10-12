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
    public abstract class AbstractTracker
    {
        public abstract TrackingParameters Parameters { get; }


        #region Abstract Methods
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
        public abstract bool TrackStep(List<AbstractTrackPoint> previousPoints, long time, Bitmap bmpImage, OpenCvSharp.Mat cvImage, out AbstractTrackPoint currentPoint);
        
        /// <summary>
        /// Creates a track point from auto-tracking.
        /// </summary>
        public abstract AbstractTrackPoint CreateTrackPoint(PointF p, long time, float similarity, Bitmap image, List<AbstractTrackPoint> previousPoints);

        /// <summary>
        /// Creates a Track point from a user-provided location.
        /// </summary>
        public abstract AbstractTrackPoint CreateTrackPointReference(PointF point, long time, Bitmap image);


        /// <summary>
        /// Creates a bare bone TrackPoint.
        /// This is used only in the case of importing from xml.
        /// Can't be used to track the next point. 
        /// Will have to be updated later with algo related info.
        /// </summary>
        public abstract AbstractTrackPoint CreateOrphanTrackPoint(PointF point, long time);

        /// <summary>
        /// Draw the tracker gizmo.
        /// </summary>
        public abstract void Draw(Graphics canvas, AbstractTrackPoint point, IImageToViewportTransformer transformer, Color color, double opacityFactor, bool isConfiguring);

        #endregion
    }
}
