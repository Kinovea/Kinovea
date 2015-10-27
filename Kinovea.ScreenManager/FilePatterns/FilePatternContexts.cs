using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Available context variables.
    /// </summary>
    public enum FilePatternContexts
    {
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
        
        CameraAlias,
        ConfiguredFramerate,
        ReceivedFramerate
    }
}
