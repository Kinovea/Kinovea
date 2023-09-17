using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Holds a match between two points in two different frames.
    /// This is used for drawing the matches on screen.
    /// </summary>
    public struct CameraMatch
    {
        /// <summary>
        /// Point in the first frame.
        /// </summary>
        public PointF P1;

        /// <summary>
        /// Point in the second frame.
        /// </summary>
        public PointF P2;

        /// <summary>
        /// Whether this match was used to compute the overall camera motion.
        /// </summary>
        public bool Inlier;

        public CameraMatch(PointF p1, PointF p2, bool inlier)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.Inlier = inlier;
        }
    }
}
