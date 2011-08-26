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

#pragma once

// Note: might be moved somewhere else, in Kinovea.Video assembly for example.
namespace Kinovea { namespace VideoFiles
{

	public enum class AspectRatio
	{
		AutoDetect,			// The program will detect square pixels or anamorphic and load it as such.
		Force43,			// The user wants 4:3, used by user when Auto doesn't work as expected.
		Force169,			// The user wants 16:9
		ForceSquarePixels	// The program forces square pixels to overcome a video-specific bug.
	};

	public enum class ImportStrategy
	{
		Complete,
		Reduction,
		InsertionBefore,
		InsertionAfter
	};
	public enum class LoadResult
	{
		Success,
		FileNotOpenned,
		StreamInfoNotFound,
		VideoStreamNotFound,
		CodecNotFound,
		CodecNotOpened,
		CodecNotSupported,
		Cancelled,
		FrameCountError
	};
	public enum class ReadResult
	{
		Success,
		MovieNotLoaded,
		MemoryNotAllocated,
		ImageNotConverted,
		FrameNotRead
	};
	public enum class SaveResult
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
	};



}
}
