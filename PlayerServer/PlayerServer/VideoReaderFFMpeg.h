#pragma region License
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
#pragma endregion

//---------------------------------------------------------------------------------------------------------------
// Note on memory mapping.
//
// A decoded frame uses the following types:
// - uint8_t* : native pointer to a raw buffer containting the actual image data after decoding.
// - AVFrame* : native pointer to an FFMpeg wrapper around the buffer. 
//				Has pointers to various parts of the buffer for planes. ->data[0] is scan0, top left of image.
// - IntPtr	  : managed wrapper around the buffer *pointer*. Used to construct the Bitmap^ from the buffer pointer.
// - Bitmap^  : managed wrapper around the buffer.
// - VideoFrame^ : our wrapper around a Bitmap^ and the associated timestamp.
// AVPacket is a wrapper around the *encoded* data. Only used temporarily during the reading.
//
//  -- Memory management --
// As we avoid making deep copies of the image, we need to keep all the low level buffers alive,
// as long as their enclosing Bitmap might be used.
// The native buffer will *not* be automatically free'd when calling Bitmap->Dispose().
// This means we need to track the pointer and deallocate manually.
// To achieve that, we use the Tag property of the Bitmap to store an IntPtr wrapping the pointer to the buffer.
// When asked to release this specific Bitmap, we unwrap the IntPtr to the pointer, and call delete.
//
// Note: Calling av_free(AVFrame*) does not deallocate the data buffer either,
// so AVFrame variables can be local to the function, it won't kill the Bitmaps.
//---------------------------------------------------------------------------------------------------------------

#pragma once

extern "C" {
#define __STDC_CONSTANT_MACROS
#define __STDC_LIMIT_MACROS
#include <avformat.h>
#include <avcodec.h>
#include <avstring.h>
#include <swscale.h> 
}

#include <stdio.h>
#include "Enums.h"
#include "TimestampInfo.h"
#include "InfosVideo.h"			// <-- will be removed eventually.
#include "SavingContext.h"

using namespace System;
using namespace System::Collections::Generic;				
using namespace System::ComponentModel;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Text;
using namespace System::Threading;
using namespace System::Windows::Forms;

using namespace Kinovea::Base;
using namespace Kinovea::Video;

//#define INSTRUMENTATION // <-- Extra logging.

#define OUTPUT_MUXER_MKV 0
#define OUTPUT_MUXER_MP4 1
#define OUTPUT_MUXER_AVI 2

namespace Kinovea { namespace Video { namespace FFMpeg
{
	[SupportedExtensions(gcnew array<String^> {
			".3gp", ".asf", ".avi", ".dv", ".flv", ".f4v", ".m1v", ".m2p", ".m2t", ".m2ts", ".mts",
			".m2v", ".m4v", ".mkv", ".mod", ".mov", ".moov", ".mpg", ".mpeg", ".tod", ".mxf",
			".mp4", ".mpv", ".ogg", ".ogm", ".ogv", ".qt", ".rm", ".swf", ".vob", ".webm", ".wmv",
			".jpg", ".jpeg", ".png", ".bmp",
			"*"
	})]
	public ref class VideoReaderFFMpeg : VideoReader
	{
	// Properties (VideoReader implementation).
	public: 
		virtual property VideoReaderFlags Flags {
			VideoReaderFlags get() override { 
				return	VideoReaderFlags::SupportsAspectRatio & 
						VideoReaderFlags::SupportsDeinterlace; 
			}
        }
        virtual property bool Loaded {
            bool get() override { return m_bIsLoaded; }
        }
        virtual property VideoInfo Info {
			VideoInfo get() override { return m_VideoInfo; }
        }
        virtual property bool Caching {
            bool get() override { return m_bIsCaching; }
        }
		virtual property VideoSection WorkingZone {
            VideoSection get() override { return m_WorkingZone; }
            void set(VideoSection value) override { m_WorkingZone = value;}
        }

	// Public Methods (VideoReader implementation).
	public:
		virtual OpenVideoResult Open(String^ _filePath) override;
		virtual void Close() override;
		virtual bool MoveNext(bool _synchrounous) override;
		virtual bool MoveTo(int64_t _timestamp) override;
		virtual VideoSummary^ ExtractSummary(String^ _filePath, int _thumbs, int _width) override;
		virtual String^ ReadMetadata() override;

	// Construction / Destruction.
	public:
		VideoReaderFFMpeg();
		~VideoReaderFFMpeg();
	protected:
		!VideoReaderFFMpeg();

	// Members
	private:
		// General
		bool m_bIsLoaded;
		bool m_bIsCaching;
		VideoInfo m_VideoInfo;
		VideoSection m_WorkingZone;
		
		// FFMpeg specifics
		int m_iVideoStream;
		int m_iAudioStream;
		int m_iMetadataStream;
		AVFormatContext* m_pFormatCtx;
		AVCodecContext* m_pCodecCtx;
		TimestampInfo m_TimestampInfo;
		static const enum PixelFormat m_PixelFormatFFmpeg = PIX_FMT_BGRA;
		static const int DecodingQuality = SWS_FAST_BILINEAR;
		
		// Others
		static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);

	// Private methods
	private:
		ReadResult ReadFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode);
		void SetTimestampFromPacket(int64_t _dts, int64_t _pts, bool _bDecoded);
		bool RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace);
		static void DisposeFrame(VideoFrame^ _frame);
		static int GetFirstStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType);
		
		void DumpInfo();
		static void DumpStreamsInfos(AVFormatContext* _pFormatCtx);
		static void DumpFrameType(int _type);
		


	//----------------------------------------------------------------
	// Old methods and members. For compilation only - may be removed.
	//----------------------------------------------------------------
	private:
		PrimarySelection^ m_PrimarySelection;
		DefaultSettings^ m_DefaultSettings;
		List <DecompressedFrame ^>^	m_FrameList;
		BackgroundWorker^ m_bgWorker;
		InfosVideo^ m_InfosVideo;
		
		Bitmap^ m_BmpImage;
		AVFrame* m_pCurrentDecodedFrameBGR;		// decoded frame as an AVFrame.  <----- Remove. The AVFrame can be local to the decoding fn.
		uint8_t* m_Buffer;						// decoded frame data.

		
		void	ChangeAspectRatio(AspectRatio _aspectRatio);
		void	SetImageGeometry(void);

		int64_t GetFrameNumber(int64_t _iPosition);		
		ImportStrategy	PrepareSelection(int64_t% _iStartTimeStamp, int64_t% _iEndTimeStamp, bool _bForceReload);
		int		EstimateNumberOfFrames( int64_t _iStartTimeStamp, int64_t _iEndTimeStamp); 
		void	DeleteFrameList(void);
		void	ExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, bool _bForceReload);
		bool	CanExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, int _maxSeconds, int _maxMemory);
		
	};
}}}
