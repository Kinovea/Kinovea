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
	/// </summary>
	public abstract class VideoReader
	{
	    public const PixelFormat DecodingPixelFormat = PixelFormat.Format32bppPArgb;
	    
	    #region Properties
	    public abstract VideoCapabilities Flags { get; }
		public abstract VideoInfo Info { get; }
	    public abstract bool Loaded { get; }
		public abstract VideoSection WorkingZone { get; set; }
		public abstract bool IsPreBuffering { get; }
		
		public VideoFrameCache Cache { get; protected set; }
		public VideoOptions Options { get; set; }
		public bool IsCaching { get; protected set; }
		public VideoFrame Current {
		    get { return Cache == null ? null : Cache.Current;}
		}
		public int Drops {
		    get { return Cache == null ? 0 : Cache.Drops;}
		}
		public string FilePath {
			get { return Info.FilePath; }
		}
		public bool IsSingleFrame { 
		    get { return Info.DurationTimeStamps == 1;}
        }
        public Bitmap CurrentImage { 
		    get { 
		        if(Cache == null || Cache.Current == null) return null;
		        else return Cache.Current.Image;
		    }
        }
		public long EstimatedFrames {
		    get {
		        long duration = WorkingZone.End - WorkingZone.Start;
		        return (duration / Info.AverageTimeStampsPerFrame) + 1;
		    }
		}
		public bool CanPreBuffer {
		    get { return (Flags & VideoCapabilities.PreBuffer) != 0; }
		}
		public bool CanAspectRatio {
		    get { return (Flags & VideoCapabilities.AspectRatio) != 0; }
		}
		public bool CanDeinterlace {
		    get { return (Flags & VideoCapabilities.Deinterlacing) != 0; }
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
            if(!IsCaching && !IsPreBuffering && CanPreBuffer)
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
		/// Return true if the whole working zone would fit in the Cache.
		/// </summary>
		public virtual bool CanCacheWorkingZone(VideoSection _newZone, int _maxSeconds, int _maxMemory)
		{
		    return false;
		}
		
        /// <summary>
        /// Provide a lazy enumerator on each frame of the Working Zone.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VideoFrame> FrameEnumerator()
        {
            if(IsPreBuffering)
                throw new ThreadStateException("Frame enumerator called while prebuffering");
            
            bool hasMore = MoveFirst();
            yield return Current;
            
            while(hasMore)
            {
                hasMore = MoveNext(0, true);
                yield return Current;
                
                // Clean up continuously to avoid clogging the cache.
                if(CanPreBuffer && !IsCaching)
                    Cache.Clear();
            }
        }
		
		/// <summary>
		/// Updates the internal working zone. Import whole zone to cache if possible.
		/// </summary>
		/// <param name="_workerFn">A function that will start a background thread for the actual import</param>
		public virtual void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxSeconds, int _maxMemory, Action<DoWorkEventHandler> _workerFn)
        {
            if(!CanPreBuffer)
                return;
            
            if(_workerFn == null)
                throw new ArgumentNullException("workerFn");
            
            VideoSection oldZone = WorkingZone;
            WorkingZone = _newZone;
            
            if(!CanCacheWorkingZone(_newZone, _maxSeconds, _maxMemory))
            {
                IsCaching = false;
                Cache.Clear();
                return;
            }
            
            VideoSection sectionToCache = VideoSection.Empty;
            bool prepend = false;
            
            if(!IsCaching || _forceReload)
            {
                // Just entering the cached mode, import everything.
                Cache.Clear();
                sectionToCache = _newZone;
            }
            else if(oldZone.Contains(_newZone))
            {
                Cache.PurgeOutsiders();
            }
            else if(_newZone.Start < oldZone.Start && _newZone.End > oldZone.End)
            {
                // Special case of both prepend and append. Clear all and import all for simplicity.
                Cache.Clear();
                sectionToCache = _newZone;
            }
            else if(_newZone.Start < oldZone.Start)
            {
                // Prepending.
                sectionToCache = new VideoSection(_newZone.Start, oldZone.Start);
                prepend = true;
            }
            else
            {
                // Appending.
                sectionToCache = new VideoSection(oldZone.End, _newZone.End);
            }
            
            if(sectionToCache != VideoSection.Empty)
            {
                _workerFn((s,e) => IsCaching = ReadMany((BackgroundWorker)s, sectionToCache, prepend));
            }
		}
		/// <summary>
		/// Import several frames in sequence to cache.
		/// Used in the context of analysis mode (full working zone to cache)
		/// </summary>
		/// <param name="_bgWorker">Hosting background worker, for cancellation and progress</param>
		/// <param name="_section">The section to import</param>
		/// <param name="_prepend">true if the section is before what's currently in the cache, used to configure Cache.Add.</param>
		/// <returns>true if all went fine</returns>
		public virtual bool ReadMany(BackgroundWorker _bgWorker, VideoSection _section, bool _prepend)
		{
		    return false;
		}
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
		    if(Cache != null)
		        Cache.SkipDrops();
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
            m_bWasPreBuffering = IsPreBuffering;
            if(m_bWasPreBuffering)
            {
                StopPreBuffering();
                Cache.Clear();
            }
		}
		/// <summary>
		/// Should be called after non-playback decoding operations.
		/// </summary>
		public virtual void AfterFrameOperation()
		{
            if(CanPreBuffer && !IsCaching)
            {
                // The operation may have corrupted the cache with non contiguous frames.
                Cache.Clear();
                
                if(m_bWasPreBuffering)
                    StartPreBuffering();
            }
		}
		#endregion
	}
}
