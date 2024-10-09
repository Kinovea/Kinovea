using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Type of action for drawing events.
    /// </summary>
    public enum DrawingAction
    {
        Unknown,
        Added,
        Deleted,
        Selected,
        StateChanged,
        StyleChanged,
        TrackingStatusChanged,
        TrackingParametersChanged,
        Moving,
        Moved,
        Resizing,
        Resized,
    }
}
