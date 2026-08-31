using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// This class holds some information about the state of the cache after 
    /// the preparation step for the next job.
    /// This is used by the decoder to come up with an initialization plan.
    /// </summary>
    public class CachePreparationResult
    {
        /// <summary>
        /// Whether the target of the job is already in the cache.
        /// If true `Current` is now pointing to it.
        /// </summary>
        public bool TargetAcquired { get; }

        /// <summary>
        /// The timestamp of the matching frame if found.
        /// This is the resolved media-space timestamp,
        /// which may be different from the request timestamp.
        /// </summary>
        public long AcquiredTimestamp { get; }

        public CachePreparationResult(
            bool targetAcquired, 
            long acquiredTimestamp)
        {
            TargetAcquired = targetAcquired;
            AcquiredTimestamp = acquiredTimestamp;
        }

        public static CachePreparationResult Empty()
        {
            return new CachePreparationResult(false, -1);
        }
    }
}
