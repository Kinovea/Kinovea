using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Time is used to denote a single time snapshot and will be converted to a corresponding instant in time.
    /// Duration is for stopwatch and other type of time.
    /// TotalDuration denotes the total number of time unit, so it's duration + 1 frame. Used for the duration of the video.
    /// </summary>
    public enum TimeType
    {
        Time,
        Duration,
        TotalDuration 
    }
}
