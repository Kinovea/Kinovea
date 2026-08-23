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
        /// Counter incremented each time the player changes state. 
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The kind of timeline action the player is currently doing.
        /// playback/jump/step forward/step backward.
        /// </summary>
        public PlayerStateMode Mode { get; }

        /// <summary>
        /// For mode playback, the timestamp at which playback started.
        /// For mode timestamp, this is the target of a jump.
        /// For mode step forward or step backward, this is the current frame timestamp.
        /// This may be in UI-space, mapped from timeline pixels or clock, 
        /// and not an actual media timestamp.
        /// </summary>
        public long ReferenceTimestamp { get; }

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


        public PlayerState(
            int generation, 
            PlayerStateMode mode,
            long referenceTimestamp,
            long startPlaybackEpoch = 0, 
            double playbackFrameInterval = 0, 
            double refreshInterval = 0) 
        {
            Id = generation;
            Mode = mode;
            ReferenceTimestamp = referenceTimestamp;
            StartPlaybackEpoch = startPlaybackEpoch;
            PlaybackFrameInterval = playbackFrameInterval;
            RefreshInterval = refreshInterval;
        }

        public static PlayerState Empty => new PlayerState(0, PlayerStateMode.Timestamp, 0);
    
        public override string ToString()
        {
            return string.Format("#{0} [Mode: {1}, Ref: ~{2}]", Id, Mode, ReferenceTimestamp);
        }

    }
}
