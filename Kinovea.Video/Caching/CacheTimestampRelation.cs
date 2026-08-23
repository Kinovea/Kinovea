using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Relates a request timestamp with a cache.
    /// </summary>
    public enum CacheTimestampRelation
    {
        /// <summary>
        /// The cache is empty.
        /// </summary>
        Empty,

        /// <summary>
        /// The request is before the first frame.
        /// </summary>
        Behind,

        /// <summary>
        /// The request is on a frame in the cache.
        /// Fuzzy matching within tolerance.
        /// </summary>
        InBoundsMatch,

        /// <summary>
        /// The request is within bounds but not on a frame.
        /// </summary>
        InBoundsNoMatch,

        /// <summary>
        /// The request is nearby after the last frame.
        /// Nearby means it's worth it to advance the decoder to it
        /// compared to doing a seek.
        /// </summary>
        Ahead,

        /// <summary>
        /// The request is far after the last frame.
        /// A seek is likely necessary to reach it even if starting at the last frame.
        /// </summary>
        FarAhead,
    }
}
