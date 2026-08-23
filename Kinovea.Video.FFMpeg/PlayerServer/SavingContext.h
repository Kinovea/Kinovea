/*
Copyright © Joan Charmant 2008-2009.
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

#pragma once

using namespace System::Drawing;

namespace Kinovea { namespace Video { namespace FFMpeg
{
	/// <summary>
	/// Encapsulate informations needed to save frames to file.
	/// </summary>
	public ref class SavingContext
	{

	public:

		// FFmpeg context
        AVFormatContext* pOutputFormatContext;	// Muxer parameters.
		const AVCodec* pOutputCodec;			// Encoder general infos. (codec_id, etc.)
		AVCodecContext* pOutputCodecContext;	// Encoder parameters.
        int streamIndex;
        
        // Image geometry
        Size outputSize;

        // Frame rate
        double frameInterval;
        long long frameDuration;                // Duration of a frame in stream time base.

        // Pixel format
        bool uncompressed;
        AVPixelFormat sourceFormat;		    // The pixel format of the incoming frames.
        AVPixelFormat targetFormat;		// The pixel format of the encoder.

        // Reusable frames/packets/scaling context.
        AVFrame* pSourceFrame;					// The current incoming frame.
        AVFrame* pConvertedFrame;				// Converted to the encoder format.
        AVPacket* pPacket;						// Encoded packet to be written to file.
        SwsContext* pScalingContext;            // The scaling context for the RGB -> YUV color conversion.

        int frameCounter;
    
		SavingContext::SavingContext()
		{
			outputSize = Size(720, 576);
            
			frameInterval = 40;			// Default speed : 25 fps.
            frameDuration = 1000;

            uncompressed = false;
            sourceFormat = AV_PIX_FMT_BGRA;
            targetFormat = AV_PIX_FMT_YUV420P;

            frameCounter = 0;
		}
	};
}}}
