using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This represents the different type of displayable information we have for one time section.
    /// </summary>
    
    [Flags]
    public enum ChronoColumns
    {
        None = 0,
        Name = 1,
        Duration = 2,
        Cumul = 4,
        Tag = 8,
    }
}
