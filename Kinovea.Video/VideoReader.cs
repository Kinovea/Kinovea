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
        
        public virtual IWorkingZoneFramesContainer WorkingZoneFrames 
        {
            get { return null;}
        }

        /// <summary>
        /// Returns the start and end of the decoding buffer when in pre-buffering mode.
        /// </summary>
        public virtual VideoSection PreBufferingSegment 
        {
            get { return VideoSection.MakeEmpty(); }
        }

        /// <summary>
        /// Counter of dropped frames for asynchronous readers that did not have the requested frame ready.
        /// </summary>
        public virtual int Drops 
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets or sets the image-level options (aspect, rotation, demosaicing, deinterlace).
        /// </summary>
        public VideoOptions Options { get; set; }
        
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
        /// Whether the video frame in `Current` is at the requested decoding size or not.
        /// </summary>
        public virtual bool CanDrawUnscaled 
        {
            get { return false;}
        }

        # region Shorcuts for capabilities.
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
        public bool CanChangeWorkingZone {
            get { return (Flags & VideoCapabilities.CanChangeWorkingZone) != 0; }
        }
        public bool CanChangeDecodingSize {
            get { return (Flags & VideoCapabilities.CanChangeDecodingSize) != 0; }
        }
        public bool CanScaleIndefinitely
        {
            get { return (Flags & VideoCapabilities.CanScaleIndefinitely) != 0; }
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

        #region Open/Close
        public abstract OpenVideoResult Open(string filePath);
        
        public abstract void Close();

        /// <summary>
        /// Open the video file as fast as possible to extract basic information and thumbnails.
        /// </summary>
        public abstract VideoSummary ExtractSummary(string filePath, int thumbsToLoad, Size maxImageSize);

        /// <summary>
        /// Called after load and before the first decode request.
        /// </summary>
        public abstract void PostLoad();

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
        /// This is called in the context of the playback loop.
        /// For async readers, if the frame is not available right now, call it a drop.
        /// Decoding of that next frame should have happened in the decoding thread already.
        /// If `decodeIfNecessary` is true then force sync and only return after the frame has 
        /// been placed into `Current`. This is for scenarios like saving, next button, etc.
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveNext(int _skip, bool _decodeIfNecessary);
        
        /// <summary>
        /// Must set `Current` to the asked frame, by timestamp.
        /// This is called in the context of frame by frame navigation.
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveTo(long from, long target);
        #endregion
        
        #region Decoding mode, play loop and frame enumeration

        /// <summary>
        /// Called right before starting the play loop.
        /// Might be used to ensure the prebuffering thread is started.
        /// Does nothing by default. Override to implement.
        /// </summary>
        public virtual void BeforePlayloop()
        {
        }

        /// <summary>
        /// Called when the decoding drop counter should be reset, e.g: after forced slow down.
        /// This is a reader specific action because not all reader are subject to dropping.
        /// Does nothing by default. Override to implement.
        /// </summary>
        public virtual void ResetDrops()
        {
        }

        /// <summary>
        /// Updates the internal working zone. 
        /// Imports whole zone to cache if possible.
        /// </summary>
        /// <param name="_workerFn">A function that will start a background thread for the actual import</param>
        public abstract void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler> _workerFn);

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

        public bool HasMoreFrames()
        {
            return Current != null && Current.Timestamp < WorkingZone.End;
        }

        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// </summary>
        public IEnumerable<VideoFrame> EnumerateFrames(long interval)
        {
            if (DecodingMode == VideoDecodingMode.PreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");

            bool hasMore = MoveFirst();
            yield return Current;

            while (hasMore)
            {
                if (interval == 0)
                    hasMore = MoveNext(0, true);
                else
                    hasMore = MoveTo(Current.Timestamp, Current.Timestamp + interval);

                yield return Current;
            }
        }

        #endregion

        #region Move playhead shortcuts
        public bool MovePrev()
        {
            return MoveTo(Current.Timestamp, Current.Timestamp - Info.AverageTimeStampsPerFrame);
        }
        public bool MoveFirst()
        {
            return MoveTo(Current.Timestamp, WorkingZone.Start);
        }
        public bool MoveLast()
        {
            return MoveTo(Current.Timestamp, WorkingZone.End);
        }
        public bool MoveBy(int frames, bool decodeIfNecessary)
        {
            if(frames == 1)
            {
                return MoveNext(0, decodeIfNecessary);
            }
            else
            {
                long currentTimestamp = Current == null ? 0 : Current.Timestamp;
                long target = currentTimestamp + (Info.AverageTimeStampsPerFrame * frames);
                target = Math.Max(0, target);
                
                return MoveTo(currentTimestamp, target);
            }
        }
        #endregion
        
        #region Image adjustments (aspect, rotation, demosaicing, deinterlace, stabilization)
        /// <summary>
        /// Force a specific aspect ratio.
        /// </summary>
        /// <returns>returns true if the cache has been invalidated by the operation</returns>
        public virtual bool ChangeAspectRatio(ImageAspectRatio ratio)
        {
            // Does nothing by default. Override to implement.
            return false;
        }

        /// <summary>
        /// Force a specific image rotation.
        /// </summary>
        /// <returns>returns true if the cache has been invalidated by the operation</returns>
        public virtual bool ChangeImageRotation(ImageRotation rotation)
        {
            // Does nothing by default. Override to implement.
            return false;
        }

        /// <summary>
        /// Force a specific demosaicing pattern.
        /// </summary>
        /// <returns>returns true if the cache has been invalidated by the operation</returns>
        public virtual bool ChangeDemosaicing(Demosaicing demosaicing)
        {
            // Does nothing by default. Override to implement.
            return false;
        }

        /// <summary>
        /// Set deinterlace on or off.
        /// </summary>
        /// <returns>returns true if the cache has been invalidated by the operation</returns>
        public virtual bool ChangeDeinterlace(bool deint)
        {
            // Does nothing by default. Override to implement.
            return false;
        }

        /// <summary>
        /// Pass stabilization data to the reader.
        /// </summary>
        /// <returns>returns true if the cache has been invalidated by the operation</returns>
        public virtual bool SetStabilizationData(List<TimedPoint> points)
        {
            // Does nothing by default. Override to implement.
            return false;
        }
        #endregion

        #region Decoding size
        /// <summary>
        /// Ask the reader to provide its images at a specific size.
        /// Not necessarily honored by the reader.
        /// Returns true if the change was accepted or not required.
        /// </summary>
        public virtual bool ChangeDecodingSize(Size size)
        {
            // Does nothing by default. Override to implement.
            return false;
        }

        /// <summary>
        /// Ask the reader to reset the decoding size to the "aspect ratio" size.
        /// This is used when the player is doing operations that are not compatible with 
        /// decoding at smaller size and rendering unscaled. Object tracking for example 
        /// needs to get patches of the image at the original resolution for better precision.
        /// </summary>
        public virtual void DisableCustomDecodingSize()
        {
            // Does nothing by default. Override to implement.
        }
        #endregion
    }
}
