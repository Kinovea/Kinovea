using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public enum CameraMotionStep
    {
        // All the steps below in order.
        All,

        // Find features in each frame.
        FindFeatures,

        // Match features in consecutive frames and 
        // find the homography transforming image planes
        // between consecutive frames.
        MatchFeatures,

        // Register all images against a common coordinate system.
        BundleAdjustment,
    }
}
