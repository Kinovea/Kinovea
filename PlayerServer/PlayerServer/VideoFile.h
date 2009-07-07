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

//----------------------------------------------------------------------------------------------------------------
// Troubleshooting:
//
// If MS linker crashes, like : 
// Project : error PRJ0002 : Error result -1073741819 returned from 'C:\Program Files\Microsoft Visual Studio 8\VC\bin\link.exe'.
// this is due to a bug in some MS code at embedding manifest stage...
// You need to go to registry:
// [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Command Processor]
// And rename the key "AutoRun" to something else, like "NoAutoRun". (Sometimes the other way around)
// ref : http://social.msdn.microsoft.com/forums/en-US/vcgeneral/thread/871103ca-6015-40ce-8a59-92e47ce68aeb/
//----------------------------------------------------------------------------------------------------------------

#pragma once

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

//------------------------
#define OUTPUT_MUXER_MKV 0
#define OUTPUT_MUXER_MP4 1
#define OUTPUT_MUXER_AVI 2

extern "C" 
{
#define __STDC_CONSTANT_MACROS
#define __STDC_LIMIT_MACROS
#include <avformat.h>
#include <avcodec.h>
#include <avstring.h>
#include <swscale.h> 
}

#define TRACE
#include <stdio.h>
#include "InfosVideo.h"
#include "SavingContext.h"

namespace Kinovea
{
namespace VideoFiles
{

#pragma region Namespace wide delegates
	public delegate bool DelegateGetOutputBitmap(Graphics^ _canvas, int64_t _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly);
#pragma endregion

#pragma region Namespace wide enums
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
		UnknownError,

		MovieNotLoaded,
		TranscodeNotFinished,
		Cancelled
	};
#pragma endregion

	public ref class VideoFile
	{

#pragma region Properties
	public:
		property bool Loaded
		{
			bool get(){ return m_bIsLoaded;}
		}
		property List <DecompressedFrame ^>^ FrameList
		{
			List <DecompressedFrame ^>^	get(){ return m_FrameList;}
		}
		property String^ FilePath
		{
			String^ get(){ return m_FilePath;}
		}
		property InfosVideo^ Infos
		{
			InfosVideo^ get(){ return m_InfosVideo;}
		}
		property PrimarySelection^ Selection
		{
			PrimarySelection^ get(){ return m_PrimarySelection;}
		}
		property Bitmap^ CurrentImage
		{
			Bitmap^ get(){return m_BmpImage;}
		}
		property BackgroundWorker^ BgWorker
		{
			BackgroundWorker^ get(){return m_bgWorker;}
			void set(BackgroundWorker^ _bgWorker){m_bgWorker = _bgWorker;}
		}
#pragma endregion

#pragma region Members
	private :
		static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);
		
		bool m_bIsLoaded;
		String^ m_FilePath;
		List <DecompressedFrame ^>^	m_FrameList;
		InfosVideo^ m_InfosVideo;
		PrimarySelection^ m_PrimarySelection;
		Bitmap^ m_BmpImage;
		BackgroundWorker^ m_bgWorker;
		
		// Decoding. TODO: Turn into a ReadingContext object !
		AVFormatContext*				m_pFormatCtx;
		AVCodecContext*					m_pCodecCtx;
		AVFrame*						m_pCurrentDecodedFrameBGR;		// Les données de la frame courante.
		uint8_t*						m_Buffer;
		int								m_iVideoStream;
		int								m_iMetadataStream;

		SavingContext^ m_SavingContext;

#pragma endregion

#pragma region Construction/Destruction
	public:
		VideoFile();
		~VideoFile();
	protected:
		!VideoFile();
#pragma endregion

#pragma region Public Methods
	public:

		LoadResult Load(String^ _FilePath);
		
		InfosThumbnail^ GetThumbnail(String^ _FilePath, int _iPicWidth);
		
		String^	GetMetadata();
		
		void	ExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, bool _bForceReload);
		
		bool	CanExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, int _maxSeconds, int _maxMemory);
		
		ReadResult ReadFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode);
		
		SaveResult Save( String^ _FilePath, int FrameInterval, int64_t _iSelStart, int64_t _iSelEnd, String^ _Metadata, bool _bFlushDrawings, bool _bKeyframesOnly, DelegateGetOutputBitmap^ _delegateGetOutputBitmap);

		void Unload();

		int64_t GetTimeStamp(int64_t _iPosition);
		int64_t GetFrameNumber(int64_t _iPosition);
		
		// New save methods, to be exported later in a new VideoFileWriter object.
		SaveResult OpenSavingContext(String^ _FilePath);
		SaveResult CloseSavingContext(bool _bEncodingSuccess);
		SaveResult SaveFrame(Bitmap^ _image);
		
#pragma endregion

#pragma region Private Methods
	private:

		// Loading
		int		GetFirstStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType);
		void	DumpStreamsInfos(AVFormatContext* _pFormatCtx);

		// Analysis mode
		ImportStrategy	PrepareSelection(int64_t% _iStartTimeStamp, int64_t% _iEndTimeStamp, bool _bForceReload);
		int		EstimateNumberOfFrames( int64_t _iStartTimeStamp, int64_t _iEndTimeStamp); 

		// Saving
		bool	SetupEncoder(AVCodecContext* _pOutputCodecContext,AVCodec* _pOutputCodec, Size _OutputSize, int _iFrameInterval, int _iBitrate);
		bool	SetupEncoderForCopy(AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream);
		bool	SetupMuxer(AVFormatContext* _pOutputFormatContext, AVOutputFormat* _pOutputFormat, char* _pFilePath, int _iBitrate);
		int		GetInputBitrate(int _iOutputWidth, int _iOutputHeight);
		bool	NeedEncoding(int _iFramesInterval, bool _bFlushDrawings, bool _bKeyframesOnly);
		bool	EncodeAndWriteVideoFrame(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, int _iOutputWidth, int _iOutputHeight, SwsContext* _pScalingContext, AVFrame* _pInputFrame);
		bool	EncodeAndWriteVideoFrame(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, int _iOutputWidth, int _iOutputHeight, SwsContext* _pScalingContext, Bitmap^ _InputBitmap);
		bool	WriteFrame(int _iEncodedSize, AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, uint8_t* _pOutputVideoBuffer, bool _bForceKeyframe);
		bool	WriteMetadata(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, AVStream* _pOutputDataStream, String^ _Metadata);
		static AVOutputFormat* GuessOutputFormat(String^ _FilePath, bool _bHasMetadata);


		// Other utilities
		void	MoveToTimestamp(int64_t _iPosition);
		void	RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace);
		int		GreatestCommonDenominator(int a, int b);
		void	ResetPrimarySelection(void);
		void	ResetInfosVideo(void);

#ifdef TRACE
		float	TraceMemoryUsage(PerformanceCounter^ _ramCounter, float _fLastRamValue, String^ _comment, float* _fRamBalance);
#endif
#pragma endregion
			
	};
}
}
