using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Available context variables.
    /// This represent all the possible variables, the features using this may restrict the symbol map.
    /// </summary>
    public enum PatternContext
    {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
        Millisecond,

        Date,
        Time,
        DateTime,

        CameraAlias,
        ConfiguredFramerate,
        ReceivedFramerate,

        Escape, 

        CaptureDirectory,
        CaptureFilename,
        CaptureKVA,
    }
}
