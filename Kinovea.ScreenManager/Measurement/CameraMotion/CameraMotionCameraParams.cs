using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Intrinsics and extrinsics parameters used during camera motion (rotation) estimation.
    /// </summary>
    public struct CameraMotionCameraParams
    {
        double focal;
        double aspect;
        double cx;
        double cy;
        Matrix3x3 R;
    }
}