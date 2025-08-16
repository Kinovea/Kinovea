using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{

    /// <summary>
    /// Sync information for one particular player.
    /// </summary>
    public class PlayerSyncInfo
    {
        /// <summary>
        /// Local time of sync point.
        /// Timestamp of the time origin for this player.
        /// </summary>
        public long SyncTime { get; set; }

        /// <summary>
        /// Local last time.
        /// Timestamp of the last frame for this player.
        /// </summary>
        public long LastTime { get; set; }

        /// <summary>
        /// Local 0 in common time.
        /// Start of running zone for this player.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Scaling factor when using synchronization by motion instead of time.
        /// </summary>
        public double Scale { get; set; }
    }
}
