using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// How we re-initialize the decoder for the next job.
    /// </summary>
    public enum DecoderInitAction
    {
        /// <summary>
        /// The decoder is already at the best spot to work on the new job.
        /// </summary>
        None,

        /// <summary>
        /// The decoder should advance a little to reach the target frame.
        /// </summary>
        Advance,

        /// <summary>
        /// We need a seek to move to a different spot.
        /// </summary>
        Seek
    }
}
