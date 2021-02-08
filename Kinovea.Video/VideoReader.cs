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
        
        public virtual IWorkingZoneFramesContainer WorkingZoneFrames {
            get { return null;}
        }
        public virtual VideoSection PreBufferingSegment {
            get { return VideoSection.Empty; }
        }
        // If the reader is subject to decoding drops (prebuffering), this property should be filled accordingly.
        public virtual int Drops {
            get {return 0; }
        }
        public VideoOptions Options { get; set; }
        
        public string FilePath {
            get { return Info.FilePath; }
        }
        public bool IsSingleFrame { 
            get { return Info.DurationTimeStamps == 1;}
        }
        public long EstimatedFrames {
            get {
                long duration = WorkingZone.End - WorkingZone.Start;
                return (duration / Info.AverageTimeStampsPerFrame) + 1;
            }
        }
        
        public virtual bool CanDrawUnscaled {
            get { return false;}
        }
        
        // Shorcuts for capabilities.
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
        #endregion

        #region Members
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Methods
        public abstract OpenVideoResult Open(string _filePath);
        public abstract void Close();
        public abstract VideoSummary ExtractSummary(string filePath, int thumbsToLoad, Size maxImageSize);
        
        /// <summary>
        /// Set the "Current" property to hold the next video frame.
        /// <para>For async readers, if the frame is not available right now, call it a drop.</para>
        /// <para>(Decoding should happen in a separate thread).</para>
        /// <para>decodeIfNecessary will be true for some scenarios like saving, next button, etc.</para>
        /// <para>In these cases return only after the frame has been pushed to .Current.</para>
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveNext(int _skip, bool _decodeIfNecessary);
        
        /// <summary>
        /// Set the "Current" property to hold an arbitrary video frame, based on timestamp.
        /// </summary>
        /// <returns>false if the end of file has been reached</returns>
        public abstract bool MoveTo(long _timestamp);
        
        public abstract void BeforeFrameEnumeration();
        public abstract void AfterFrameEnumeration();
        
        /// <summary>
        /// Called after load and before the first decode request.
        /// </summary>
        public abstract void PostLoad();
        
        /// <summary>
        /// Updates the internal working zone. Import whole zone to cache if possible.
        /// </summary>
        /// <param name="_workerFn">A function that will start a background thread for the actual import</param>
        public abstract void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler> _workerFn);
        
        #region Move playhead
        public bool MovePrev()
        {
            return MoveTo(Current.Timestamp - Info.AverageTimeStampsPerFrame);
        }
        public bool MoveFirst()
        {
            return MoveTo(WorkingZone.Start);
        }
        public bool MoveLast()
        {
            return MoveTo(WorkingZone.End);
        }
        public bool MoveBy(int _frames, bool _decodeIfNecessary)
        {
            if(_frames == 1)
            {
                return MoveNext(0, _decodeIfNecessary);
            }
            else
            {
                long currentTimestamp = Current == null ? 0 : Current.Timestamp;
                long target = currentTimestamp + (Info.AverageTimeStampsPerFrame * _frames);
                if(target < 0)
                    target = 0;
                return MoveTo(target);
            }
        }
        #endregion
        
        public virtual bool CanSwitchDecodingMode(VideoDecodingMode _mode)
        {
            switch(_mode)
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
        public virtual bool HasMoreFrames()
        {
            return Current != null && Current.Timestamp < WorkingZone.End;
        }
        
        public virtual void BeforePlayloop()
        {
            // Called right before starting the play loop.
            // Might be used to ensure the prebuffering thread is started.
            // Does nothing by default. Override to implement.
        }
        
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
        /// </summary>
        public virtual void DisableCustomDecodingSize()
        {
            // Does nothing by default. Override to implement.
        }
        
        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// </summary>
        public IEnumerable<VideoFrame> FrameEnumerator(long interval)
        {
            if(DecodingMode == VideoDecodingMode.PreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");
            
            bool hasMore = MoveFirst();
            yield return Current;
            
            while(hasMore)
            {
                if(interval == 0)
                    hasMore = MoveNext(0, true);
                else
                    hasMore = MoveTo(Current.Timestamp + interval);
                
                yield return Current;
            }
        }
        public IEnumerable<VideoFrame> FrameEnumerator()
        {
            return FrameEnumerator(0);
        }

        public virtual string ReadMetadata()
        {
            return "";
        }
        public virtual void ResetDrops()
        {
            // Called when the decoding drop counter should be reset (for example after forced slow down.)
            // (a reader specific action because not all reader are subject to dropping).
            // Does nothing by default. Override to implement.
        }
        #endregion
    }
}
