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
using System.Drawing.Imaging;

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
		
		/// <summary>
		/// Request for caching the entire working zone.
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
		public ImageAspectRatio ImageAspectRatio {
			get { return Options.ImageAspectRatio; }
			set { 
			    if((Flags & VideoReaderFlags.SupportsAspectRatio) != 0)
			    {
			        Options = new VideoOptions(value, Options.Deinterlace);
			    }
			}
		}
		public bool Deinterlace {
			get { return Options.Deinterlace; }
			set { 
			    if((Flags & VideoReaderFlags.SupportsDeinterlace) != 0)
			        Options = new VideoOptions(Options.ImageAspectRatio, value);
			}
		}
		#endregion

		public const PixelFormat DecodingPixelFormat = PixelFormat.Format32bppPArgb;
		
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
		        return MoveNext(false);
            else
                return MoveTo(Current.Timestamp + (Info.AverageTimeStampsPerFrame * _frames));
		}
		#endregion
		
		
		
	}
}
