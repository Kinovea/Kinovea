#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// A base class for all video decoders implementations.
    /// 
    /// Concrete implementations should add a SupportedExtensions attribute listing the extensions
    /// supported by this particular reader, as an array of string.
    /// Ex: [SupportedExtensions(new string[] {".avi", ".bmp"})]
    /// 
    /// Concrete implementation is responsible for the frames storage method. 
    /// (Some advanced storage classes are provided: Cache, Prebuffer).
    /// 
    /// Images should be decoded in the Format32bppPArgb pixel format.
    /// 
    /// Implementers: you may consider subclassing a more specific abstract class like VideoReaderAlwaysCaching,
    /// as they provide some boilerplate code for functions irrelevant to some video readers.
    /// </summary>
    public abstract class VideoReader
    {
        public const PixelFormat DecodingPixelFormat = PixelFormat.Format32bppPArgb;
        
        #region Properties
        public abstract VideoFrame Current { get; }
        public abstract VideoCapabilities Flags { get; }
        public abstract VideoInfo Info { get; }
        public abstract bool Loaded { get; }
        public abstract VideoSection WorkingZone { get;}
        public abstract VideoDecodingMode DecodingMode { get; }

        public abstract VideoGeometry Geometry { get; }

        public virtual IWorkingZoneFramesContainer WorkingZoneFrames 
        {
            get { return null;}
        }

        /// <summary>
        /// Full path to the video file.
        /// </summary>
        public string FilePath 
        {
            get { return Info.FilePath; }
        }

        /// <summary>
        /// Whether the video contains only one frame.
        /// </summary>
        public bool IsSingleFrame 
        { 
            get { return Info.DurationTimeStamps == 1;}
        }

        # region Shorcuts for capabilities.
        public bool CanChangeWorkingZone
        {
            get { return (Flags & VideoCapabilities.CanChangeWorkingZone) != 0; }
        }
        public bool CanDecodeOnDemand {
            get { return (Flags & VideoCapabilities.CanDecodeOnDemand) != 0; }
        }
        public bool CanPreBuffer {
            get { return (Flags & VideoCapabilities.CanPreBuffer) != 0; }
        }
        public bool CanCache {
            get { return (Flags & VideoCapabilities.CanCache) != 0; }
        }
        public bool CanChangeAspectRatio {
            get { return (Flags & VideoCapabilities.CanChangeAspectRatio) != 0; }
        }
        public bool CanChangeImageRotation
        {
            get { return (Flags & VideoCapabilities.CanChangeImageRotation) != 0; }
        }
        public bool CanChangeDemosaicing
        {
            get { return (Flags & VideoCapabilities.CanChangeDemosaicing) != 0; }
        }
        public bool CanChangeDeinterlacing {
            get { return (Flags & VideoCapabilities.CanChangeDeinterlacing) != 0; }
        }
        public bool CanStabilize
        {
            get { return (Flags & VideoCapabilities.CanStabilize) != 0; }
        }
        #endregion

        #endregion

        // Map of requested timestamps vs actual timestamps.
        // The request are based on average time stamp but the files often
        // have non regular timestamps intervals.
        private Dictionary<long, long> tsMap = new Dictionary<long, long>();

        // Latest snapshot of the player state.
        // Used by the decoding thread to predict what should be decoded next.
        protected PlayerState playerState = null;

        #region Open/Close

        /// <summary>
        /// Open the video.
        /// At the end of this call the reader must have initialized its Info 
        /// and VideoGeometry properties.
        /// </summary>
        public abstract OpenVideoResult Open(string filePath);
        
        public abstract void Close();

        /// <summary>
        /// Open the video file as fast as possible to extract basic information and thumbnails.
        /// </summary>
        public abstract VideoSummary ExtractSummary(string filePath, int thumbsToLoad, Size maxImageSize);

        #endregion

        #region Low level frame requests

        /// <summary>
        /// Map requested timestamp to actual timestamp, if we have seen it before.
        /// </summary>
        public long MapTimestamp(long requested)
        {
            if (tsMap.ContainsKey(requested))
                return tsMap[requested];
            
            return requested;
        }

        public void AddTimestampMapping(long requested, long actual)
        {
            if (!tsMap.ContainsKey(requested))
                tsMap.Add(requested, actual);
        }

        /// <summary>
        /// Must set `Current` to the next video frame.
        /// For async readers, if the frame is not available right now, call it a drop.
        /// Decoding of that next frame should have happened in the decoding thread already.
        /// If `decodeIfNecessary` is true then force sync and only return after the frame has 
        /// been placed into `Current`. This is for scenarios like saving, next button, etc.
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveNext(int _skip, bool _decodeIfNecessary);
        
        /// <summary>
        /// Informs the reader that the player is asking for the passed timestamp.
        /// For synchronous reading this should set `Current` to the exact requested frame.
        /// For asynchronous reading this should set `Current` the nearest known frame,
        /// and synchronize the player state (target and speed) with the decoding thread
        /// so it can take decisions on what to decode next.
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveTo(long from, long target);
        #endregion

        #region Decoding mode, play loop and frame enumeration

        /// <summary>
        /// Start prebuffering if supported and not already in full caching mode.
        /// This is normally called during first opening of the video or after 
        /// changing the presentation size.
        /// The first call to UpdateWorkingZone inhibits prebuffering as we don't 
        /// have a valid presentation size yet. Once the UI is loaded we can try again.
        /// </summary>
        public virtual void StartPrebufferingIfNotCaching()
        {
        }

        /// <summary>
        /// Called right before starting the play loop.
        /// Might be used to ensure the prebuffering thread is started.
        /// Override to implement.
        /// </summary>
        public virtual void BeforePlayloop()
        {
        }

        /// <summary>
        /// The player changed state and publishes the new state.
        /// This is used to inform which frames to decode next.
        /// </summary>
        public void PublishPlayerState(PlayerState newPlayerState)
        {
            Volatile.Write(ref playerState, newPlayerState);
        }

        /// <summary>
        /// Compute the expected frame timestamp the player would like to see right now.
        /// The player does its own computation independently using similar code.
        /// </summary>
        public long GetExpectedTimestamp(PlayerState state)
        {
            if (state == null || !state.IsPlaying)
                return 0;

            long now = Stopwatch.GetTimestamp();
            double realElapsedSeconds = (double)(now - playerState.StartPlaybackEpoch) / Stopwatch.Frequency;
            double elapsedFrames = realElapsedSeconds * 1000.0 / playerState.PlaybackFrameInterval;
            double elapsedTimestamps = elapsedFrames * Info.AverageTimeStampsPerFrame;
            long expectedTimestamp = (long)Math.Round(playerState.StartPlaybackTimestamp + elapsedTimestamps);
            return expectedTimestamp;
        }

        /// <summary>
        /// The player is asking the reader to update the working zone.
        /// This will update the bounds and try to switch to full caching mode.
        /// If it fits in memory it will use the provided background worker to load it.
        /// If it doesn't fit and the reader supports async decoding, it will switch to
        /// prebuffering mode and start the decoding thread.
        /// Otherwise it will stay in "on-demand" synchrounous mode.
        /// </summary>
        /// <param name="_workerFn">A function that will start a background thread for the actual import</param>
        public abstract void UpdateWorkingZone(
            VideoSection _newZone,
            CacheLoadMode loadMode,
            int _maxMemory,
            Action<DoWorkEventHandler> _workerFn);

        public abstract void BeforeFrameEnumeration();

        public abstract void AfterFrameEnumeration();

        public bool CanSwitchDecodingMode(VideoDecodingMode mode)
        {
            switch (mode)
            {
                case VideoDecodingMode.NotInitialized:
                    return true;
                case VideoDecodingMode.OnDemand:
                    return CanDecodeOnDemand;
                case VideoDecodingMode.PreBuffering:
                    return CanPreBuffer;
                case VideoDecodingMode.Caching:
                    return CanCache;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns true if the enumerator may still move to the next frame in the working zone.
        /// </summary>
        public bool HasMoreFrames()
        {
            if (Current == null)
                return false;

            double nextTimestamp = Current.Timestamp + Info.AverageTimeStampsPerFrame;
            bool result = nextTimestamp <= WorkingZone.End;
            return result;
        }

        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// interval is the time in timestamps between each frame.
        /// </summary>
        public IEnumerable<VideoFrame> EnumerateFrames(double interval)
        {
            if (DecodingMode == VideoDecodingMode.PreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");

            bool hasMore = MoveTo(Current.Timestamp, WorkingZone.Start);
            yield return Current;

            while (hasMore)
            {
                if (interval == 0)
                    hasMore = MoveNext(0, true);
                else
                    hasMore = MoveTo(Current.Timestamp, (long)Math.Round(Current.Timestamp + interval));

                yield return Current;
            }
        }

        #endregion

        #region Move playhead shortcuts
        public bool MoveBy(int frames, bool decodeIfNecessary)
        {
            if(frames == 1)
            {
                return MoveNext(0, decodeIfNecessary);
            }
            else
            {
                long currentTimestamp = Current == null ? 0 : Current.Timestamp;
                long target = currentTimestamp + (long)Math.Round(Info.AverageTimeStampsPerFrame * frames);
                target = Math.Max(0, target);
                
                return MoveTo(currentTimestamp, target);
            }
        }
        #endregion
        
        #region Video geometry

        /// <summary>
        /// Requests the reader to recalculate the video geometry and
        /// invalidate any cache if necessary.
        /// The resulting geometry is published in the VideoGeometry property.
        /// Returns true if a cache has been invalidated.
        /// </summary>
        public abstract bool UpdateVideoGeometry(VideoGeometryRequest request);
        #endregion
    }
}
