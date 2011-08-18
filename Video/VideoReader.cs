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
		public abstract bool Loaded { get; }
		public abstract VideoInfo Info { get; }
        
		public abstract bool Caching { get; }
		public abstract VideoSection Selection { get; }
		public abstract VideoFrame Current { get; }
		
		
		public abstract OpenVideoResult Open(string _FilePath);
		public abstract void Close();
		
		/// <summary>
		/// Set the "Current" property to hold the next video frame.
		/// This function should be super fast. 
		/// For buffered readers, if the frame is not available right now, call it a drop.
		/// Decoding should happen in a separate thread.
		/// If we are on the last frame and Options.AutoRewind is false, just stay there.
		/// </summary>
		public abstract bool MoveNext();
		
		/// <summary>
		/// Set the "Current" property to hold an arbitrary video frame, based on timestamp.
		/// </summary>
		public abstract bool MoveTo(long _timestamp);

		#region Concrete Properties
		public string FilePath {
			get { return Info.FilePath; }
		}
		public bool SingleFrame { 
		    get { return Info.DurationTimeStamps == 1;}
        }
        public Bitmap CurrentImage { 
		    get { return (Current != null) ? Current.Image : null; }
        }
		public VideoOptions Options {
			get { return m_VideoOptions; }
			set { m_VideoOptions = value; }
		}
		public ImageAspectRatio ImageAspectRatio {
			get { return Options.ImageAspectRatio; }
			set { m_VideoOptions.ImageAspectRatio = value; }
		}
		public bool Deinterlace {
			get { return Options.Deinterlace; }
			set { m_VideoOptions.Deinterlace = value; }
		}
		public virtual string Metadata {
			get { return null; }
		}
		#endregion

		private VideoOptions m_VideoOptions;
	}
}
