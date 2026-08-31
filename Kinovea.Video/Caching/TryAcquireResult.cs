using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public class TryAcquireResult
    {
        /// <summary>
        /// Whether the target was found in the cache.
        /// If true `Current` is now pointing to it.
        /// </summary>
        public bool TargetAcquired { get; }

        /// <summary>
        /// The actual timestamp of the matching frame if found.
        /// </summary>
        public long AcquiredTimestamp { get; }

        public TryAcquireResult(
            bool targetAcquired, 
            long acquiredTimestamp)
        {
            TargetAcquired = targetAcquired;
            AcquiredTimestamp = acquiredTimestamp;
        }

        public static TryAcquireResult MakeEmpty()
        {
            return new TryAcquireResult(false, -1);
        }
    }
}
