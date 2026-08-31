using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;

namespace Kinovea.Video
{
    public class WorkingZoneRequest
    {
        /// <summary>
        /// The new zone to load.
        /// </summary>
        public VideoSection WorkingZone { get; }

        /// <summary>
        /// Full cache loading/clearing policy.
        /// </summary>
        public CacheLoadMode CacheLoadMode { get; }

        /// <summary>
        /// Maximum memory allowed for full cache.
        /// </summary>
        public int MaxMemoryMB { get; }

        public WorkingZoneRequest(VideoSection workingZone, CacheLoadMode cacheLoadMode, int maxMemoryMB)
        {
            WorkingZone = workingZone;
            CacheLoadMode = cacheLoadMode;
            MaxMemoryMB = maxMemoryMB;
        }
    }
}
