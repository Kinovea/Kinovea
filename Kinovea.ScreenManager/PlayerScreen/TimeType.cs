using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Time coordinate system used for timecode generation.
    /// The passed time should always be absolute or a duration.
    /// </summary>
    public enum TimeType
    {
        /// <summary>
        /// The returned timecode will be relative to video start (0).
        /// Use this for the timecode of the start of the working zone itself.
        /// </summary>
        Absolute,

        /// <summary>
        /// The returned timecode will be relative to working zone start.
        /// This should not be used for anything, use UserOrigin instead.
        /// </summary>
        WorkingZone,

        /// <summary>
        /// The returned timecode will be relative to user-defined time origin or synchronization point.
        /// As long as the user doesn't define this manually it will be aligned with working zone start.
        /// </summary>
        UserOrigin,

        /// <summary>
        /// The passed time is a duration.
        /// </summary>
        Duration,
    }
}
