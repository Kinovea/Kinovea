using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Different levels of frame-skipping for the pre-buffer / async decoding.
    /// Only checked and applied during playback.
    /// </summary>
    public enum FrameSkippingPolicy
    {
        // Policy: decode and scale/convert/store all frames.
        // The decoding thread is decoding frames fast enough to keep up with the player demands.
        // Typically the cache will be full and the decoding thread will be waiting in Add().
        // Lag should be negative.
        Normal,

        // Policy: decode all frames but only do the scaling/conversion/store step
        // for one frame per refresh period.
        Behind,

        // Policy: decode only reference frames (I-frames).
        // The decoding thread is too slow to keep up with the player.
        FarBehind,

        // There is an extra level when we detect the lag is over an even higher threshold.
        // In that case we seek ahead to the next keyframe and restart decoding from there.
    }
}
