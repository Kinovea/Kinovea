﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class PlayerSyncInfo
    {
        /// <summary>
        /// Local time of sync point.
        /// </summary>
        public long SyncTime { get; set; }

        /// <summary>
        /// Local last time.
        /// </summary>
        public long LastTime { get; set; }

        /// <summary>
        /// Local 0 in common time.
        /// (Start of running zone for this player).
        /// </summary>
        public long Offset { get; set; }
    }
}
