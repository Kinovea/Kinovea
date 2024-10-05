using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The tracking status defines the behavior and look of the trajectory.
    /// </summary>
    public enum TrackStatus
    {
        /// <summary>
        /// The trajectory is open for tracking.
        /// Advancing in the timeline will perform a tracking step
        /// which will either use already tracked point or perform the actual search.
        /// We show the bounding boxes but without corner controls.
        /// Moving the search window adjusts the tracked position manually.
        /// </summary>
        Edit,

        /// <summary>
        /// The trajectory is closed for tracking (read only).
        /// </summary>
        Interactive,
    }
}
