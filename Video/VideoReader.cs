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
using System.Drawing;

using Kinovea.Services;

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
		public abstract OpenVideoResult Open(string _FilePath);
		public abstract void Close();
		
		/// <summary>
		/// Set the "Current" property to hold the next video frame.
		/// This function should be super fast. 
		/// For async readers, if the frame is not available right now, call it a drop.
		/// (Decoding should happen in a separate thread and fill a buffer).
		/// If we are on the last frame and Options.AutoRewind is false, just stay there.
		/// </summary>
		public abstract bool MoveNext();
		
		/// <summary>
		/// Set the "Current" property to hold an arbitrary video frame, based on timestamp.
		/// This function may take longer than MoveNext. 
		/// Don't return until you have found the frame and updated "Current" with it.
		/// </summary>
		public abstract bool MoveTo(long _timestamp);

		/// <summary>
		/// Request for caching.
		/// </summary>
		/// <param name="_start"></param>
		/// <param name="_end"></param>
		/// <param name="_maxSeconds"></param>
		/// <param name="_maxMemory"></param>
		/// <returns></returns>
		//public abstract bool Cache(long _start, long _end, int _maxSeconds, int _maxMemory);
        #endregion
		
		#region Concrete Properties
		public VideoFrameCache Cache { get; protected set; }
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
		public VideoOptions Options {
			get { return m_VideoOptions; }
			set { m_VideoOptions = value; }
		}
		public ImageAspectRatio ImageAspectRatio {
			get { return m_VideoOptions.ImageAspectRatio; }
			set { 
			    if((Flags & VideoReaderFlags.SupportsAspectRatio) != 0)
			        m_VideoOptions.ImageAspectRatio = value;
			}
		}
		public bool Deinterlace {
			get { return m_VideoOptions.Deinterlace; }
			set { 
			    if((Flags & VideoReaderFlags.SupportsDeinterlace) != 0)
			        m_VideoOptions.Deinterlace = value;
			}
		}
		public virtual string Metadata {
			get { return null; }
		}
		#endregion

		private VideoOptions m_VideoOptions;
		
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
		        return MoveNext();
            else
                return MoveTo(Current.Timestamp + (Info.AverageTimeStampsPerFrame * _frames));
		}
		#endregion
		
		
	}
}
