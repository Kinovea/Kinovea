using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public enum CacheAddResult
    {
        /// <summary>
        /// The frame has been added and is now owned by the cache.
        /// It will be disposed during eviction or clear.
        /// </summary>
        Added,

        /// <summary>
        /// The frame is already in the cache.
        /// The caller is responsible for disposing it if needed.
        /// </summary>
        Duplicate,

        /// <summary>
        /// The add was interrupted by a cancellation request.
        /// The caller is responsible for disposing the frame if needed.
        /// </summary>
        Interrupted,
    }
}
