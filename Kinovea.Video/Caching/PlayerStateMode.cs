using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Type of presentation requests from the player.
    /// </summary>
    public enum PlayerStateMode
    {
        /// <summary>
        /// The player is in playback mode, the frame can be set to 
        /// the closest available in cache, best-effort basis.
        /// </summary>
        Playback,

        /// <summary>
        /// The player requests an exact timestamp.
        /// </summary>
        Timestamp,

        /// <summary>
        /// The player requests the very next frame.
        /// </summary>
        StepForward,

        /// <summary>
        /// The player requests the very previous frame.
        /// </summary>
        StepBackward,

        RefreshInPlace,
    }
}
