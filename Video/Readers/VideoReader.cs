#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
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

namespace Kinovea.Video
{
	/// <summary>
	/// A base class for all video decoders implementations.
	/// 
	/// Concrete implementations should add a SupportedExtensions attribute listing the extensions
	/// supported by this particular reader, as an array of string.
	/// Ex: [SupportedExtensions(new string[] {".avi", ".bmp"})]
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
		public abstract VideoSection WorkingZone { get; set; }
		public abstract VideoDecodingMode DecodingMode { get; }
		
		public virtual IWorkingZoneFramesContainer WorkingZoneFrames {
		    get { return null;}
		}
		public virtual int PreBufferingDrops {
		    get {return 0; }
            //get { return Cache == null ? 0 : Cache.Drops;}
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
		public bool CanChangeDeinterlacing {
		    get { return (Flags & VideoCapabilities.CanChangeDeinterlacing) != 0; }
		}
		public bool CanChangeWorkingZone {
		    get { return (Flags & VideoCapabilities.CanChangeWorkingZone) != 0; }
		}
		#endregion

		#region Members
		private bool m_bWasPreBuffering;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
		
		#region Methods
		public abstract OpenVideoResult Open(string _filePath);
		public abstract void Close();
		public abstract VideoSummary ExtractSummary(string _filePath, int _thumbs, int _width);
		
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
		public abstract bool MoveTo(long _timestamp, bool _decodeIfNecessary);
		
		public abstract void BeforeFrameEnumeration();
		public abstract void AfterFrameEnumerationStep();
		public abstract void CompletedFrameEnumeration();
		
		
		#region Move playhead
		public bool MovePrev()
		{
		    return MoveTo(Current.Timestamp - Info.AverageTimeStampsPerFrame, true);
		}
		public bool MoveFirst()
		{
		    return MoveTo(WorkingZone.Start, true);
		}
		public bool MoveLast()
		{
		    return MoveTo(WorkingZone.End, true);
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
		        log.Debug("MoveBy -> MoveTo");
		        return MoveTo(target, _decodeIfNecessary);
		    }
		}
		#endregion
		
		public virtual void BeforePlayloop()
		{
            //if(!IsCaching && !IsPreBuffering && CanPreBuffer)
            if(DecodingMode != VideoDecodingMode.Caching &&
               (CanPreBuffer && DecodingMode != VideoDecodingMode.PreBuffering))
            {
                // Just in case something wrong happened, make sure the decoding thread is alive.
                // Normally it should always be running (unless the whole zone is cached).
                log.Error("Forcing PreBuffering thread to restart.");
                StartPreBuffering();
            }
		}
		
		/// <summary>
		/// Force a specific aspect ratio.
		/// </summary>
		/// <returns>returns true if the cache has been invalidated by the operation</returns>
		public virtual bool ChangeAspectRatio(ImageAspectRatio _ratio)
		{
            // Does nothing by default. Override to implement.
            return false;
		}
		/// <summary>
		/// Set deinterlace on or off.
		/// </summary>
		/// <returns>returns true if the cache has been invalidated by the operation</returns>
		public virtual bool ChangeDeinterlace(bool _deint)
		{
		    // Does nothing by default. Override to implement.
            return false;
		}
		
        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// </summary>
        public virtual IEnumerable<VideoFrame> FrameEnumerator()
        {
            // TODO: how to provide this function without assuming container ?
            
            if(DecodingMode == VideoDecodingMode.PreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");
            
            bool hasMore = MoveFirst();
            yield return Current;
            
            while(hasMore)
            {
                hasMore = MoveNext(0, true);
                yield return Current;
                // next line should not be needed. 
                // we should be in single frame mode or in full cache mode.
                AfterFrameEnumerationStep();
            }
        }
		
		/// <summary>
		/// Updates the internal working zone. Import whole zone to cache if possible.
		/// </summary>
		/// <param name="_workerFn">A function that will start a background thread for the actual import</param>
		public abstract void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxSeconds, int _maxMemory, Action<DoWorkEventHandler> _workerFn);

		public virtual string ReadMetadata()
		{
		    return "";
		}
		public virtual void StartPreBuffering()
		{
		    // This should be used to initialize any variable or 
		    // enter any state necessary to sustain the play loop.
		    // Typically used to start a background thread for decoding.
		    
		    // Does nothing by default. Override to implement.
		}
		public virtual void StopPreBuffering()
		{
		    // Does nothing by default. Override to implement.
		}
		public virtual void SkipDrops()
		{
		    //TODO: do not assume prebuffering or specific container.
		    
		    /*if(Cache != null)
		        Cache.SkipDrops();*/
		}
		
		/// <summary>
		/// Must be called before every operation that would conflict with async decoding.
		/// For example, loading key images or saving image sequence, saving videos.
		/// These operations need the playhead to repeatedly move to non contiguous frames, which
		/// would induce unecessary decoding each time we move.
		/// the async decode thread should be stopped temporarily and restarted after the operation.
		/// </summary>
		public virtual void BeforeFrameOperation()
		{
		    // TODO: this will be replaced by a switch to NotBuffering mode.
            /*m_bWasPreBuffering = IsPreBuffering;
            if(m_bWasPreBuffering)
            {
                StopPreBuffering();
                Cache.Clear();
            }*/
		}
		/// <summary>
		/// Should be called after non-playback decoding operations.
		/// </summary>
		public virtual void AfterFrameOperation()
		{
		    // TODO: will change when frame containers are better split.
		    
            /*if(CanPreBuffer && !IsCaching)
            {
                // The operation may have corrupted the cache with non contiguous frames.
                Cache.Clear();
                
                if(m_bWasPreBuffering)
                    StartPreBuffering();
            }*/
		}
		#endregion
	}
}
