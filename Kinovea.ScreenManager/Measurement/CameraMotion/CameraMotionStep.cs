﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public enum CameraMotionStep
    {
        All,
        FindFeatures,
        MatchFeatures,
        BundleAdjustment,
    }
}