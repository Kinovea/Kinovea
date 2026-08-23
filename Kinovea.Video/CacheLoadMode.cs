using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Full cache loading/clearing policy when updating the working zone.
    /// </summary>
    public enum CacheLoadMode
    {
        /// <summary>
        /// If we are already caching do not invalidate the cache.
        /// Just load the extra frames or trim the excess.
        /// If we are not already caching load everything if possible.
        /// </summary>
        Keep,

        /// <summary>
        /// Load the cache even if we are already caching and even if 
        /// the working zone didn't change.
        /// This happens for example when we change image options like rotation.
        /// </summary>
        Reload,

        /// <summary>
        /// Do not load the cache even if we are not currently caching.
        /// This is used for replay watchers for example, to avoid disrupting
        /// the instant-replay feedback loop.
        /// </summary>
        DoNotLoad,
    }
}
