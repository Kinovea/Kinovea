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

        /// <summary>
        /// True if there is a dense range starting at the target.
        /// </summary>
        public bool DenseForward { get; }

        /// <summary>
        /// End timestamp of the dense range.
        /// </summary>
        public long DenseEndTimestamp { get; }

        /// <summary>
        /// Timestamp of the first frame stored in the cache.
        /// </summary>
        public long CacheStartTimestamp { get; }

        /// <summary>
        /// Timestamp of the last frame stored in the cache.
        /// Useful to test if a pending frame is a continuation of the cache.
        /// </summary>
        public long CacheEndTimestamp { get; }

        /// <summary>
        /// True if the cache is full and cannot accept more frames.
        /// </summary>
        public bool Full { get; }

        public CachePreparationResult(
            bool targetAcquired, 
            long acquiredTimestamp, 
            bool denseForward, 
            long denseEndTimestamp,
            long cacheStartTimestamp,
            long cacheEndTimestamp,
            bool full)
        {
            TargetAcquired = targetAcquired;
            AcquiredTimestamp = acquiredTimestamp;
            DenseForward = denseForward;
            DenseEndTimestamp = denseEndTimestamp;
            CacheStartTimestamp = cacheStartTimestamp;
            CacheEndTimestamp = cacheEndTimestamp;
            Full = full;
        }

        public static CachePreparationResult Empty()
        {
            return new CachePreparationResult(false, -1, false, -1, -1, -1, false);
        }
    }
}
