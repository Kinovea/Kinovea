using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// A little immutable snapshot of the player state at a given time.
    /// This is used by the reader to inform its decoding strategy.
    /// 
    /// The playback epoch can be used to compute the timestamp the player would 
    /// expect to display at any given time during the decoding loop.
    /// 
    /// This can be used to skip work if it falls behind or change the retention policy.
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// A monotonic counter incremented each time the player changes state. 
        /// (play, pause, stop, seek).
        /// </summary>
        public int Generation { get; }

        /// <summary>
        /// The timestamp of the frame we were at when the playback started.
        /// </summary>
        public long StartPlaybackTimestamp { get; }

        /// <summary>
        /// Global computer time when playback started.
        /// Used to compute the current frame timestamp during playback.
        /// This is based on Stopwatch.GetTimestamp().
        /// </summary>
        public long StartPlaybackEpoch { get; }

        /// <summary>
        /// The interval between frames during playback, in milliseconds.
        /// Takes speed slider into account.
        /// </summary>
        public double PlaybackFrameInterval { get; }

        /// <summary>
        /// True if the player is currently playing, false if paused.
        /// </summary>
        public bool IsPlaying { get; }

        public PlayerState(int generation, long startPlaybackTimestamp, long startPlaybackEpoch, double playbackFrameInterval, bool isPlaying)
        {
            Generation = generation;
            StartPlaybackTimestamp = startPlaybackTimestamp;
            StartPlaybackEpoch = startPlaybackEpoch;
            PlaybackFrameInterval = playbackFrameInterval;
            IsPlaying = isPlaying;
        }
    }
}
