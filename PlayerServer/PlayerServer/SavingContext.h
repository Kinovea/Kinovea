/*
Copyright © Joan Charmant 2008-2009.
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

namespace Kinovea
{
	namespace VideoFiles
	{
		/// <summary>
		/// Encapsulate informations needed to save frames to file.
		/// This is a mixed type (contains unmannaged members).
		/// </summary>
		public ref class SavingContext
		{

		public:

			// FFMpeg context
			AVOutputFormat*	pOutputFormat;			// Muxer general infos. (mime, extensions, supported codecs, etc.)
			AVFormatContext* pOutputFormatContext;	// Muxer parameters.
			AVCodec* pOutputCodec;					// Encoder general infos. (codec_id, etc.)
			AVCodecContext* pOutputCodecContext;	// Encoder parameters.
			AVStream* pOutputVideoStream;			// Ouput stream for frames.
			AVStream* pOutputDataStream;			// Output stream for meta data.
			AVFrame* pInputFrame;					// The current incoming frame.
			
			// User parameters
			char* pFilePath;
			int iFramesInterval;
			Size outputSize;

			// Control
			bool bEncoderOpened;
		};
	}
}