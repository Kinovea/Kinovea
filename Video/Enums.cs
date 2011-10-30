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

namespace Kinovea.Video
{
    /// <summary>
    /// Flags indicating the capabilities of the specific file loaded by the reader.
    /// </summary>
    [Flags]
    public enum VideoCapabilities : int
	{
		None = 0,
		
		/// <summary>
		/// False if the entire video is extracted to cache during load and not touched afterwards.
		/// In this case the working zone is also locked.
		/// </summary>
		DynamicCache = 1,
		AspectRatio = 2,
		Deinterlacing = 4
	}
    
    public enum ImageAspectRatio
    {
    	Auto,
    	Force43,
    	Force169,
    	ForcedSquarePixels
    }
    
    public enum OpenVideoResult
    {
        Success,
        UnknownError,
        NotSupported,
        FileNotOpenned,
		StreamInfoNotFound,
		VideoStreamNotFound,
		CodecNotFound,
		CodecNotOpened,
		CodecNotSupported,
		Cancelled
    }
    
    public enum SaveResult
	{
		Success,
		MuxerNotFound,
		MuxerParametersNotAllocated,
		MuxerParametersNotSet,
		VideoStreamNotCreated,
		EncoderNotFound,
		EncoderParametersNotAllocated,
		EncoderParametersNotSet,
		EncoderNotOpened,
		FileNotOpened,
		FileHeaderNotWritten,
		InputFrameNotAllocated,
		MetadataStreamNotCreated,
		MetadataNotWritten,
		ReadingError,
		UnknownError,
		MovieNotLoaded,
		TranscodeNotFinished,
		Cancelled
	}
}
