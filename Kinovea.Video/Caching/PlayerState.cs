using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// A little immutable snapshot of the player state at a given time.
    /// This is used by the reader to detect changes in the timeline 
    /// position and inform its decoding policy.
    /// 
    /// During normal playback the state doesn't change. The playback epoch 
    /// is used to compute the timestamp the player would expect to display 
    /// at any given time.
    /// This can be used by the decoder to skip work if it falls behind.
    /// 
    /// The state should change anytime playback is paused/resumed or wraps around, 
    /// during manual browsing in the timeline or when navigation buttons are used.
    /// The generation is used to detect changes in the player state.
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// A monotonic counter incremented each time the player changes state. 
        /// (play, pause, wrap, move).
        /// </summary>
        public int Generation { get; }

        /// <summary>
        /// True if the player is currently playing, false if paused.
        /// </summary>
        //public bool IsPlaying { get; }

        public PlayerStateMode Mode { get; }

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
        /// The interval between presented frames during playback, in milliseconds.
        /// Never smaller than the monitor refresh interval.
        /// </summary>
        public double RefreshInterval { get; }

        /// <summary>
        /// The timestamp reference for non-playback mode. 
        /// For mode exact, this is the target timestamp.
        /// For mode step forward or step backward, this is the starting timestamp.
        /// For mode idle, this is the current timestamp.
        /// </summary>
        public long ReferenceTimestamp { get; }

        public PlayerState(
            int generation, 
            PlayerStateMode mode,
            long startPlaybackTimestamp, long startPlaybackEpoch, double playbackFrameInterval, double refreshInterval, 
            long frameByFrameTimestamp)
        {
            Generation = generation;
            Mode = mode;
            StartPlaybackTimestamp = startPlaybackTimestamp;
            StartPlaybackEpoch = startPlaybackEpoch;
            PlaybackFrameInterval = playbackFrameInterval;
            RefreshInterval = refreshInterval;
            ReferenceTimestamp = frameByFrameTimestamp;
        }

        public static PlayerState Empty => new PlayerState(0, PlayerStateMode.Timestamp, 0, 0, 0, 0, 0);
    }
}
