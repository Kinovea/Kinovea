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
    public enum PlayerAction
    {
        /// <summary>
        /// The player is starting playback. 
        /// The target frame should be set exactly.
        /// Future requests will go through the lightweight MoveRequest 
        /// until playback is stopped.
        /// </summary>
        Playback,

        /// <summary>
        /// The player requests an exact timestamp.
        /// </summary>
        Timestamp,

        /// <summary>
        /// The player requests the next media frame.
        /// </summary>
        StepForward,

        /// <summary>
        /// The player requests the previous media frame.
        /// </summary>
        StepBackward,
    }
}
