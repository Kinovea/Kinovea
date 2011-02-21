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

#include <stdio.h>
#include "InfosVideo.h"
#include "SavingContext.h"

namespace Kinovea
{
namespace VideoFiles
{

#pragma region Namespace wide delegates
	public delegate int64_t DelegateGetOutputBitmap(Graphics^ _canvas, Bitmap^ _sourceImage, int64_t _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly);
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
		ReadingError,
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
		DefaultSettings^ m_DefaultSettings;
		Bitmap^ m_BmpImage;
		BackgroundWorker^ m_bgWorker;
		
		// Decoding. TODO: Turn into a ReadingContext object !
		AVFormatContext*				m_pFormatCtx;
		
		int								m_iVideoStream;
		AVCodecContext*					m_pCodecCtx;
		AVFrame*						m_pCurrentDecodedFrameBGR;		// Last decoded video frame as an AVFrame.
		uint8_t*						m_Buffer;						// Last decoded video frame data.
		
		int								m_iAudioStream;
		AVCodecContext*					m_pAudioCodecCtx;
		
		int								m_iMetadataStream;

		uint8_t*						m_AudioBuffer;					// All the audio data of the primary selection.
		int								m_AudioBufferUsedSize;
		
		IntPtr							m_Hbmp;

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

		void SetDefaultSettings(int _AspectRatio, bool _bDeinterlaceByDefault);

		LoadResult Load(String^ _FilePath);
		
		InfosThumbnail^ GetThumbnail(String^ _FilePath, int _iPicWidth, int _iMaxThumbnails);
		
		String^	ReadMetadata();
		
		void	ExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, bool _bForceReload);
		
		bool	CanExtractToMemory(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, int _maxSeconds, int _maxMemory);
		
		ReadResult ReadFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode);

		void	ChangeAspectRatio(AspectRatio _aspectRatio);

		SaveResult Save( String^ _FilePath, double _fFramesInterval, int64_t _iSelStart, int64_t _iSelEnd, String^ _Metadata, bool _bFlushDrawings, bool _bKeyframesOnly, bool _bPausedVideo, DelegateGetOutputBitmap^ _delegateGetOutputBitmap);

		void Unload();

		int64_t GetTimeStamp(int64_t _iPosition);
		int64_t GetFrameNumber(int64_t _iPosition);

		void RenderToGraphics(Graphics^ _canvas, IntPtr _targetHDC);
		
#pragma endregion

#pragma region Private Methods
	private:

		// Loading
		int		GetFirstStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType);
		int		CountFrames(AVFormatContext*	_pFormatCtx, int _iVideoStream);
		int64_t GetLastTimestamp(AVFormatContext* _pFormatCtx, int _iVideoStream);
		void	SetTimestampFromPacket(int64_t _dts, int64_t _pts, bool _bDecoded);
		void	DumpStreamsInfos(AVFormatContext* _pFormatCtx);
		void	DumpFrameType(int _type);

		// Analysis mode
		ImportStrategy	PrepareSelection(int64_t% _iStartTimeStamp, int64_t% _iEndTimeStamp, bool _bForceReload);
		int		EstimateNumberOfFrames( int64_t _iStartTimeStamp, int64_t _iEndTimeStamp); 
		void	DeleteFrameList(void);

		// Other utilities
		bool	RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace);
		void	ResetPrimarySelection(void);
		void	ResetInfosVideo(void);
		void	SetImageGeometry(void);

#pragma endregion
			
	};
}
}
