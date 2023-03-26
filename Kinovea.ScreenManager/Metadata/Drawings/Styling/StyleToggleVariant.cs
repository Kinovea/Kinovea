using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Toggle style options.
    /// Many toggles are drawing-specific to support implementing several drawings with a single class.
    /// </summary>
    public enum StyleToggleVariant
    {
        Unknown,
        
        /// <summary>
        /// Polyine: tells if the polyline should be drawn as a curve.
        /// </summary>
        Curved,

        /// <summary>
        /// Plane: tells if this is the 2D rectangular grid or the perspective quadrilateral one.
        /// </summary>
        Perspective,

        /// <summary>
        /// Plane: tells whether this grid is used as a distance grid.
        /// </summary>
        DistanceGrid,

        /// <summary>
        /// Chrono: specialization for single time point clock.
        /// </summary>
        Clock,
    }
}
