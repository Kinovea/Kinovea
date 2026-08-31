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

        public event EventHandler<EventArgs<PlayerState>> FrameAcquired;
        protected void OnFrameAcquired(PlayerState state)
        {
            FrameAcquired?.Invoke(this, new EventArgs<PlayerState>(state));
        }

        public event EventHandler<EventArgs<PlayerState>> RequestFailed;
        protected void OnRequestFailed(PlayerState state)
        {
            RequestFailed?.Invoke(this, new EventArgs<PlayerState>(state));
        }

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

        /// <summary>
        /// Global computer time when playback started.
        /// Used to compute the current frame timestamp during playback.
        /// This is based on Stopwatch.GetTimestamp().
        /// </summary>
        public long StartPlaybackEpoch { get; set; }

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

        #region Members
        // Player state from the latest player request.
        // Used by the reader to schedule decoding.
        protected PlayerState mRequestedPlayerState = PlayerState.Empty;
        #endregion

        #region Open/Close/Summary
        /// <summary>
        /// Open the video.
        /// At the end of this call the reader must have initialized Info 
        /// and VideoGeometry properties.
        /// </summary>
        public abstract OpenVideoResult Open(string filePath);
        
        public abstract void Close();

        /// <summary>
        /// Open the video file as fast as possible to extract basic information and thumbnails.
        /// </summary>
        public abstract VideoSummary ExtractSummary(string filePath, int thumbsToLoad, Size maxImageSize);

        #endregion

        #region Navigation and player state

        /// <summary>
        /// The player requests a synchronous decode of the next frame.
        /// Used during frame enumeration for export, playback while tracking.
        /// TODO: instead of decode if necessary = false, call PlayerDemand(timestamp).
        /// </summary>
        public abstract bool MoveNext(bool _decodeIfNecessary);

        /// <summary>
        /// Request the frame closest to the target timestamp.
        /// This sould be lightweight and never block.
        /// </summary>
        public abstract bool MoveTo(long target);

        /// <summary>
        /// The player changed state and publishes it.
        /// This is for timeline navigation and start/pause playback.
        /// 
        /// Ultimately this will replace MoveNext and MoveTo for all 
        /// asynchronous scenarios.
        /// MoveTo can still be used for a lightweight acquire during 
        /// playback as state doesn't change during playback.
        /// </summary>
        public abstract bool PlayerRequest(PlayerState newState);
        
        /// <summary>
        /// During a playback loop, compute the expected frame timestamp 
        /// the player would like to see right now.
        /// The player does its own computation independently using similar code.
        /// </summary>
        public long GetPlaybackTimestamp(PlayerState state)
        {
            if (state == null || state.Mode != PlayerStateMode.Playback)
                return 0;

            long now = Stopwatch.GetTimestamp();
            double realElapsedSeconds = (double)(now - StartPlaybackEpoch) / Stopwatch.Frequency;
            double elapsedFrames = realElapsedSeconds * 1000.0 / state.PlaybackFrameInterval;
            double elapsedTimestamps = elapsedFrames * Info.AverageTimeStampsPerFrame;
            long expectedTimestamp = (long)Math.Round(state.ReferenceTimestamp + elapsedTimestamps);
            return expectedTimestamp;
        }

        /// <summary>
        /// Called right before starting the play loop.
        /// Might be used to ensure the prebuffering thread is started.
        /// Override to implement.
        /// 
        /// TO REMOVE: this should go through player demand / player state.
        /// 
        /// </summary>
        public virtual void BeforePlayloop()
        {
        }

        public TimestampRelation RelateTimestamps(long request, long reference)
        {
            double tolerance = Info.AverageTimeStampsPerFrame / 2.0;
            double farAheadThreshold = Info.AverageTimeStampsPerFrame * 50.0;

            if (request < 0 || reference < 0)
                return TimestampRelation.Unknown;

            long delta = request - reference;

            if (Math.Abs(delta) <= tolerance)
                return TimestampRelation.Match;

            if (delta < -farAheadThreshold)
                return TimestampRelation.FarBehind;

            if (delta < 0)
                return TimestampRelation.Behind;

            if (delta > farAheadThreshold)
                return TimestampRelation.FarAhead;

            return TimestampRelation.Ahead;
        }


        #endregion

        #region Working zone and decoding mode

        /// <summary>
        /// The player is asking the reader to update the working zone.
        /// This will try to switch to full caching mode.
        /// If it fits in memory it will use the provided background worker to load it.
        /// If it doesn't fit and the reader supports async decoding, it will switch to
        /// prebuffering mode and start the decoding thread.
        /// Otherwise it will stay in "on-demand" synchrounous mode.
        /// The internal working zone held by the reader should be updated. 
        /// The start frame is resolved to an actual media timestamp.
        /// In the case of full caching the end frame is also resolved.
        /// </summary>
        public virtual void WorkingZoneUpdateRequest(WorkingZoneRequest request, Action<DoWorkEventHandler> workerFunction)
        {
        }

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
        /// Concrete function checking if the reader can switch to a decoding mode.
        /// </summary>
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
        #endregion

        #region Frame enumeration
        public virtual void BeforeFrameEnumeration()
        {
        }

        public virtual void AfterFrameEnumeration()
        {
        }

        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// interval is the time in timestamps between each frame.
        /// </summary>
        public IEnumerable<VideoFrame> EnumerateFrames(double interval)
        {
            if (DecodingMode == VideoDecodingMode.PreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");

            bool hasMore = MoveTo(WorkingZone.Start);
            yield return Current;

            while (hasMore)
            {
                if (interval == 0)
                    hasMore = MoveNext(true);
                else
                    hasMore = MoveTo((long)Math.Round(Current.Timestamp + interval));

                yield return Current;
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
        #endregion

        #region Video geometry

        /// <summary>
        /// Requests the reader to recalculate the video geometry and
        /// invalidate any cache if necessary.
        /// The resulting geometry is published in the VideoGeometry property.
        /// Returns true if a cache has been invalidated.
        /// 
        /// This is public but should only be called from FrameServerPlayer.PublishVideoGeometryRequest().
        /// </summary>
        public abstract bool UpdateVideoGeometry(VideoGeometryRequest request);
        #endregion
    }
}
