using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Algorithm used to track a given point.
    /// </summary>
    public enum TrackingAlgorithm
    {
        /// <summary>
        /// Pattern matching with cross-correlation over the whole pattern window.
        /// Compute a correlation score at each possible location in the search window.
        /// </summary>
        Correlation,

        /// <summary>
        /// Finds a circle in the search window and use the center as the point.
        /// This is also used for tracking balls.
        /// </summary>
        RoundMarker,

        /// <summary>
        /// Finds the central corner of a "quadrant" marker and use it as the point.
        /// </summary>
        QuadrantMarker,
    }
}
