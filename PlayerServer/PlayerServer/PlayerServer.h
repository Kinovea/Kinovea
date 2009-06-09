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


//-----------------------------------------------------
// PlayerServer.h
//
//-----------------------------------------------------
#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Text;
using namespace System::Windows::Forms;
using namespace System::Threading;
using namespace System::ComponentModel;
using namespace System::Collections::Generic;				


//------------------------
#define FFMPEG_SUCCESS							0
#define FFMPEG_ERROR_FILE_NOT_OPENED			1
#define FFMPEG_ERROR_STREAM_INFO_NOT_FOUND		2
#define FFMPEG_ERROR_VIDEO_STREAM_NOT_FOUND		3
#define FFMPEG_ERROR_CODEC_NOT_FOUND			4
#define FFMPEG_ERROR_CODEC_NOT_OPENED			5
#define FFMPEG_ERROR_CODEC_NOT_SUPPORTED		6
#define FFMPEG_ERROR_TRANSCODE_NOT_FINISHED		7
#define FFMPEG_ERROR_LOAD_CANCELLED				8
#define FFMPEG_ERROR_FRAMECOUNT_ERROR			9
//------------------------
#define OUTPUT_MUXER_MKV 0
#define OUTPUT_MUXER_MP4 1
#define OUTPUT_MUXER_AVI 2


//---------------------------------------------------
// FFMpeg rev.10489, Checkout du SVN le 13 Sept 2007.
//---------------------------------------------------

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

#include "ImageFilter.h"
//------------------------


namespace VideaPlayerServer 
{

	// TODO: Utiliser des classes pour contenir InfosVideo et PrimarySelection.
	// NE PAS faire ref struct, les struct sont des types values en C# et VB.


	// Delegates
	/*public delegate Bitmap^ DelegateFlushDrawings(Int64 _iPosition);*/
	public delegate bool DelegateGetOutputBitmap(Graphics^ _canvas, int64_t _iTimestamp, bool _bFlushDrawings, bool _bKeyframesOnly);

//-------------------------------------------------------------------------------------------------	
	public ref struct InfosVideo
	{
		// Structure d'information sur l'ensemble de la video.
		public :
			
			// Read only:
			int64_t	iFileSize;
			int		iWidth;
			int		iHeight;
			double	fPixelAspectRatio;
			double  fFps;
			bool    bFpsIsReliable;					
			int		iFrameInterval;					// in Milliseconds.
			int64_t iDurationTimeStamps;			// Duration between first and last. Not between 0 and last.
			int64_t iFirstTimeStamp;				// The first frame timestamp. (not always 0) 
			double  fAverageTimeStampsPerSeconds;
			int64_t iAverageTimeStampsPerFrame;
			
			// Read / Write
			int		iDecodingWidth;
			int		iDecodingHeight;
			double	fDecodingStretchFactor;			// Used to set the output size of image.
			int		iDecodingFlag;					// Quality of scaling during format conversion.
			bool	bDeinterlaced;					// If frames should be deinterlaced.
	};
	public ref class InfosThumbnail
	{
		public:
			List <Bitmap^>^ Thumbnails;
			int64_t iDurationMilliseconds;
	};
//-------------------------------------------------------------------------------------------------
	public ref struct PrimarySelection
	{
		// Structure d'information sur la sélection primaire.

		public :
			
			int		iAnalysisMode;		// 0 : Selection, 1 : Analysis.
			bool	bFiltered;

			// Position
			int64_t iCurrentTimeStamp;	// Absolu									(Pour mode Selection)
			int		iCurrentFrame;		// Relatif, entre 0 et iDurationFrame - 1	(Pour mode Analyse)

			// Selection
			int		iDurationFrame;		// (Pour mode Analyse).
	};
//-------------------------------------------------------------------------------------------------
	public ref class DecompressedFrame
	{
		public:
			int64_t iTimeStamp;
			Bitmap^	BmpImage;
	};

	public enum class ImportStrategy
	{
		Reduced = -1,
		Complete = 0,
		InsertBefore = 1,
		InsertAfter = 2
	};

//-------------------------------------------------------------------------------------------------
	public ref class PlayerServer
	{
		public:
			
			// TODO : Finalizer (?)
			PlayerServer();
			~PlayerServer(void);


		public:
			int		LoadMovie(String^ _FilePath);
			void	UnloadMovie(void);

			String^	GetMetadata(void);

			int		GetNextFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode);
			void	ImportFramesToAnalysisList(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, bool _bForceReload);

			int64_t GetTimeStamp(int64_t _iPosition);
			int64_t GetFrameNumber(int64_t _iPosition);
			
			void	FilterImage(int _iFilter);

			int		SaveMovie( String^ _FilePath, int FrameInterval, int64_t _iSelStart, int64_t _iSelEnd, String^ _Metadata, bool _bFlushDrawings, bool _bKeyframesOnly, DelegateGetOutputBitmap^ _delegateGetOutputBitmap);
			bool	IsSelectionAnalyzable(int64_t _iStartTimeStamp, int64_t _iEndTimeStamp, int _maxSeconds, int _maxMemory);
			
			List <Bitmap^>^ ExtractForMosaic(int _iNumberOfFramesNeeded);

			//Bitmap^ GetThumbnail(String^ _FilePath);
			InfosThumbnail^ GetThumbnail(String^ _FilePath, int _iPicWidth);

		// -- Delegués --
			delegate bool UpdateProgressDelegate(int _iTotal, int _iValue, bool _bDone, int _iResult );
			

			/// <summary>
			/// Usually a form or a winform control that implements "Invoke/BeginInvode"
			/// </summary>
			System::Windows::Forms::ContainerControl^ m_sender;

			/// <summary>
			/// The delegate method (callback) on the sender to call
			/// </summary>
			System::Delegate^ m_senderDelegate;


		// -- Variables --

			System::IO::FileStream^								m_TraceLog;
			System::Diagnostics::TextWriterTraceListener^		m_TraceListener;

			// Entrée/Sortie du LoadMovie
				BackgroundWorker^				m_bgWorker;
				bool							m_bLoaderShown;

			// Affichage 
				Bitmap^							m_BmpImage;
				List <DecompressedFrame ^>^		m_FrameList;


			// Vidéo
				bool							m_bIsMovieLoaded;
				InfosVideo^						m_InfosVideo;
				PrimarySelection^				m_PrimarySelection;


			// Décodage
				
				// TODO : pinning pointers ??

				AVFormatContext*				m_pFormatCtx;
				AVCodecContext*					m_pCodecCtx;
				AVFrame*						m_pCurrentDecodedFrameBGR;		// Les données de la frame courante.
				uint8_t*						m_Buffer;
				int								m_iVideoStream;
				int								m_iMetadataStream;
			
			//Filtrage Image
				ImageFilter^					m_ImageFilter;
			
		private:
			
			// Initialisation
			void	ResetPrimarySelection(void);
			void	ResetInfosVideo(void);
			void	ResetImageFilter(void);
		
			// Loading
			int		GetFirstStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType);
			void	DumpStreamsInfos(AVFormatContext* _pFormatCtx);

			// Analysis mode
			ImportStrategy	PrepareSelection(int64_t% _iStartTimeStamp, int64_t% _iEndTimeStamp, bool _bForceReload);
			int		EstimateNumberOfFrames( int64_t _iStartTimeStamp, int64_t _iEndTimeStamp); 

			// Saving
			bool	SetupEncoder(AVCodecContext* _pOutputCodecContext,AVCodec* _pOutputCodec, int _iOutputWidth, int _iOutputHeight, int _iFrameInterval, int _iBitrate);
			bool	SetupEncoderForCopy(AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream);
			bool	SetupMuxer(AVFormatContext* _pOutputFormatContext, AVOutputFormat* _pOutputFormat, char* _pFilePath, int _iBitrate);
			int		GetInputBitrate(int _iOutputWidth, int _iOutputHeight);
			bool	NeedEncoding(int _iFramesInterval, bool _bFlushDrawings, bool _bKeyframesOnly);
			bool	EncodeAndWriteVideoFrame(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, int _iOutputWidth, int _iOutputHeight, SwsContext* _pScalingContext, AVFrame* _pInputFrame);
			bool	EncodeAndWriteVideoFrame(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, int _iOutputWidth, int _iOutputHeight, SwsContext* _pScalingContext, Bitmap^ _InputBitmap);
			bool	WriteFrame(int _iEncodedSize, AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, uint8_t* _pOutputVideoBuffer, bool _bForceKeyframe);
			bool	WriteMetadata(AVFormatContext* _pOutputFormatContext, AVCodecContext* _pOutputCodecContext, AVStream* _pOutputVideoStream, AVStream* _pOutputDataStream, String^ _Metadata);
			AVOutputFormat* GuessOutputFormat(String^ _FilePath, bool _bHasMetadata);


			// Other utilities
			void	MoveToTimestamp(int64_t _iPosition);
			void	RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace);
			int		GreatestCommonDenominator(int a, int b);

			// Unused / Testing only.
			void	BlitImage(int _iImageWidth, int _iImageHeight, int _iStride, System::Drawing::Imaging::PixelFormat _PixelFormat, System::IntPtr _ImageData );
			int		TranscodeToScratch(AVFormatContext* _pInputFormatContext, AVCodecContext* _pInputCodecContext, int _iVideoStreamIndex, int* _piTranscodedFrames);
			int		GetNumberOfFrames(AVFormatContext* _pInputFormatContext, AVCodecContext* _pInputCodecContext, int _iVideoStreamIndex, bool _bForceCount);
			void	SaveFrame(AVFrame* pFrame, int width, int height, int iFrame);

#ifdef TRACE
			float	TraceMemoryUsage(System::Diagnostics::PerformanceCounter^ _ramCounter, float _fLastRamValue, String^ _comment, float* _fRamBalance);
#endif


			static log4net::ILog^ log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);
			

	};
//-------------------------------------------------------------------------------------------------

}
