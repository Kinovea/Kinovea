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
	    #region Properties
	    public abstract VideoReaderFlags Flags { get; }
	    public abstract bool Loaded { get; }
		public abstract VideoInfo Info { get; }
		public abstract bool Caching { get; }
		public abstract VideoSection WorkingZone { get; set; }
		#endregion
		
		#region Methods
		public abstract OpenVideoResult Open(string _filePath);
		public abstract void Close();
		
		/// <summary>
		/// Set the "Current" property to hold the next video frame.
		/// This function should be super fast.
		/// For async readers, if the frame is not available right now, call it a drop.
		/// (Decoding should happen in a separate thread and fill a buffer).
		/// _synchronous will be true only during saving operation. In this case, don't drop anything.
		/// </summary>
		public abstract bool MoveNext(bool _synchronous);
		
		/// <summary>
		/// Set the "Current" property to hold an arbitrary video frame, based on timestamp.
		/// Unlike MoveNext(), this function is always synchronous. 
		/// Don't return until you have found the frame and updated "Current" with it.
		/// </summary>
		public abstract bool MoveTo(long _timestamp);
		public abstract VideoSummary ExtractSummary(string _filePath, int _thumbs, int _width);
		public abstract string ReadMetadata();
		public abstract bool CanCacheWorkingZone(VideoSection _newZone, int _maxSeconds, int _maxMemory);
		public abstract void ReadMany(BackgroundWorker _bgWorker, VideoSection _section, bool _prepend);
		#endregion
		
		#region Concrete Properties
		public VideoFrameCache Cache { get; protected set; }
		public VideoOptions Options { get; set; }
		public VideoFrame Current {
		    get { 
		        if(Cache == null) return null;
		        else return Cache.Current;
		    }
		}
		public string FilePath {
			get { return Info.FilePath; }
		}
		public bool SingleFrame { 
		    get { return Info.DurationTimeStamps == 1;}
        }
        public Bitmap CurrentImage { 
		    get { 
		        if(Cache == null || Cache.Current == null) return null;
		        else return Cache.Current.Image;
		    }
        }
		#endregion

		public const PixelFormat DecodingPixelFormat = PixelFormat.Format32bppPArgb;
		
		#region Members
		private bool m_Prepend;
		private VideoSection m_SectionToCache;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
		
		#region Concrete Methods
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
		public bool MoveBy(int _frames)
		{
		    if(_frames == 1)
		    {
		        return MoveNext(false);
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
		/// Figures out what section of the video needs to be imported to cache.
		/// </summary>
		/// <returns>returns false if caching process is not necessary</returns>
		public virtual bool BeforeFullZoneCaching(VideoSection _newZone)
		{
            bool needed = true;
            m_Prepend = false;
            
            if((Flags & VideoReaderFlags.AlwaysCaching) != 0)
                return false;
            
            if(!Caching)
            {
                Cache.Clear();
                m_SectionToCache = _newZone;
            }
            else
            {
                if(_newZone < WorkingZone || _newZone == WorkingZone)
                {
                    // New zone is within the bounds of the old one.
                    // update the bounds directly, this will dispose outsiders.
                    m_SectionToCache = VideoSection.Empty;
                    WorkingZone = _newZone;
                    needed = false;
                }
                else if(_newZone.Start < WorkingZone.Start)
                {
                    m_SectionToCache = new VideoSection(_newZone.Start, WorkingZone.Start);
                    m_Prepend = true;
                }
                else
                {
                    m_SectionToCache = new VideoSection(WorkingZone.End, _newZone.End);
                }
            }
            return needed;
		}
		public virtual void CacheWorkingZone(object sender, DoWorkEventArgs e)
        {
            Thread.CurrentThread.Name = "Caching";
		    log.DebugFormat("Caching section {0}.", m_SectionToCache);
		    ReadMany((BackgroundWorker)sender, m_SectionToCache, m_Prepend);
        }
		public virtual void AfterFullZoneCaching(VideoSection _newZone)
		{
		    // Nothing by default. Override to implement.
		}
		public virtual void ExitFullZoneCaching(VideoSection _newZone)
		{
		    // Nothing by default. Override to implement.
		}
		#endregion
	}
	
	
}
