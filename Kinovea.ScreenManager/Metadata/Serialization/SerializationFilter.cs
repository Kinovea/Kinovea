using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    [Flags]
    public enum SerializationFilter
    {
        None = 0,
        Core = 1, 
        Style = 2,
        Fading = 4,
        Spreadsheet = 8,
        
        KVA = Core + Style + Fading,
    }
}
