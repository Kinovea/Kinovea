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

        // Match features in consecutive frames and pre-filter out outliers. 
        MatchFeatures,

        // Find the homographies transforming image planes between consecutive frames.
        FindHomographies,

        // Register all images against a common coordinate system.
        BundleAdjustment,
    }
}
