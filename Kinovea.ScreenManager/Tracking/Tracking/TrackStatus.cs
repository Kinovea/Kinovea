using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The track status defines the behavior and look of the trajectory.
    /// </summary>
    public enum TrackStatus
    {
        /// <summary>
        /// The trajectory is currently tracking, between Start tracking 
        /// and End tracking actions. Advancing in the timeline will 
        /// track and append new points.
        /// We show the bounding boxes but without corner controls.
        /// Moving the search window adjusts the tracked position manually.
        /// </summary>
        Edit,

        /// <summary>
        /// The trajectory is closed for tracking.
        /// Dragging the points moves in time (can be disabled in options).
        /// </summary>
        Interactive,

        /// <summary>
        /// The trajectory parameters are being edited.
        /// We show the bounding boxes with their corner controls
        /// and the rest of the image should be dimmed.
        /// </summary>
        Configuration
    }
}
