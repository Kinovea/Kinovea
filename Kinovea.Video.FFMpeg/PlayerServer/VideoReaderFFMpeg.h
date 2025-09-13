#pragma region License
/*
Copyright � Joan Charmant 2008-2009.
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
// - VideoFrame^ :�our wrapper around a Bitmap^ and the associated timestamp.
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
#include <avcodec.h>
#include <avdevice.h>
#include <avfilter.h>
#include <avfiltergraph.h>
#include <buffersink.h>
#include <avformat.h>
#include <avutil.h>
#include <postprocess.h>
#include <swresample.h>
#include <swscale.h>
}

#include "ReadResult.h"
#include "TimestampInfo.h"
#include "SavingContext.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace System::Reflection;
using namespace System::Threading;
using namespace System::Diagnostics;
using namespace Kinovea::Video;
using namespace Kinovea::Services;

//#define INSTRUMENTATION // <-- Extra logging.

namespace Kinovea { namespace Video { namespace FFMpeg
{
    [SupportedExtensions(
        ".3gp;.asf;.avi;.dv;.flv;.f4v;\
        .m1v;.m2p;.m2t;.m2ts;.mts;.m2v;.m4v;.ts;.ts1;.ts2;.avr;\
        .mkv;.mod;.mov;.moov;.mpg;.mpeg;.tod;.mxf;\
        .mp4;.mpv;.ogg;.ogm;.ogv;.qt;.rm;.swf;.vob;.webm;.wmv;.y4m;\
        *"
    )]
    public ref class VideoReaderFFMpeg : VideoReader
    {
    // Properties (VideoReader subclassing).
    public: 
        virtual property VideoCapabilities Flags {
            VideoCapabilities get() override { 
                return	m_Capabilities; 
            }
        }
        virtual property VideoDecodingMode DecodingMode {
            VideoDecodingMode get() override { 
                return	m_DecodingMode; 
            }
        }
        virtual property bool Loaded {
            bool get() override { return m_bIsLoaded; }
        }
        virtual property VideoInfo Info {
            VideoInfo get() override { return m_VideoInfo; }
        }
        virtual property IWorkingZoneFramesContainer^ WorkingZoneFrames {
            IWorkingZoneFramesContainer^ get() override { 
                if(m_DecodingMode == VideoDecodingMode::Caching)
                    return m_Cache;
                else 
                    return nullptr;
            }
        }
        virtual property VideoSection WorkingZone {
            // Return the internal working zone.
            VideoSection get() override { return m_WorkingZone; }
        }
        virtual property VideoSection PreBufferingSegment {
            VideoSection get() override {
                if(m_DecodingMode == VideoDecodingMode::PreBuffering)
                    return m_PreBuffer->Segment;
                else 
                    return VideoSection::MakeEmpty(); 
            }
        }
        virtual property VideoFrame^ Current {
            VideoFrame^ get() override { 
                return m_FramesContainer != nullptr ? m_FramesContainer->CurrentFrame : nullptr; 
            }
        }
        virtual property int Drops {
            int get() override {
                return m_DecodingMode == VideoDecodingMode::PreBuffering ? m_PreBuffer->Drops : 0;
            }
        }
        virtual property bool CanDrawUnscaled {
            bool get() override {
                return m_CanDrawUnscaled;
            }
        }

    // Construction / Destruction.
    public:
        VideoReaderFFMpeg();
        ~VideoReaderFFMpeg();
    protected:
        !VideoReaderFFMpeg();


    public:
        
        // Open/Close
        virtual OpenVideoResult Open(String^ _filePath) override;
        virtual void Close() override;
        virtual VideoSummary^ ExtractSummary(String^ _filePath, int _thumbs, Size _maxSize) override;
        virtual void PostLoad() override;

        // Low level frame requests
        virtual bool MoveNext(int _skip, bool _decodeIfNecessary) override;
        virtual bool MoveTo(int64_t from, int64_t target) override;
        
        // Decoding mode, play loop and frame enumeration
        virtual void BeforePlayloop() override;
        virtual void ResetDrops() override;
        virtual void UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler^>^ _workerFn) override;
        virtual void BeforeFrameEnumeration() override;
        virtual void AfterFrameEnumeration() override;

        // Image adjustments
        virtual bool ChangeAspectRatio(ImageAspectRatio _ratio) override;
        virtual bool ChangeImageRotation(ImageRotation rotation) override;
        virtual bool ChangeDemosaicing(Demosaicing demosaicing) override;
        virtual bool ChangeDeinterlace(bool _deint) override;
        virtual bool SetStabilizationData(List<TimedPoint^>^ points) override;
        
        // Decoding size
        virtual bool ChangeDecodingSize(Size _size) override;
        virtual void DisableCustomDecodingSize() override;

    // Members
    private:
        // General
        VideoCapabilities m_Capabilities;
        bool m_bIsLoaded;
        bool m_bFirstFrameRead;
        VideoInfo m_VideoInfo;
        Dictionary<long, TimedPoint^>^ stabOffsets = gcnew Dictionary<long, TimedPoint^>();
        
        // Decoding mode & working zone.
        long m_timestampOffset = 0;
        VideoDecodingMode m_DecodingMode;
        bool m_bIsVeryShort;
        VideoSection m_WorkingZone;
        VideoSection m_SectionToPrepend;
        VideoSection m_SectionToAppend;
        
        // Decoding size.
        Size m_DecodingSize;
        bool m_CanDrawUnscaled;

        // Frame containers
        IVideoFramesContainer^ m_FramesContainer;
        SingleFrame^ m_SingleFrameContainer;
        PreBuffer^ m_PreBuffer;
        Cache^ m_Cache;
        
        // FFMpeg specifics
        int m_iVideoStream;
        int m_iAudioStream;
        AVFormatContext* m_pFormatCtx;
        AVCodecContext* m_pCodecCtx;
        TimestampInfo m_TimestampInfo;
        static const enum AVPixelFormat m_PixelFormatFFmpeg = AV_PIX_FMT_BGRA;
        static const int DecodingQuality = SWS_FAST_BILINEAR;

        // Others
        Object^ m_Locker;
        bool m_WasPrebuffering;
        LoopWatcher^ m_LoopWatcher;
        Thread^ m_PreBufferingThread;
        ThreadCanceler^ m_PreBufferingThreadCanceler;
        Stopwatch^ m_Stopwatch = gcnew Stopwatch();
        bool m_Verbose = true;
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);

    private:

        void DataInit();
        
        // Open/Close.
        OpenVideoResult Load(String^ _filePath, bool _forSummary);
        static int GetStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType);
        
        // Decoding size.
        void ResetDecodingSize();
        void UpdateReferenceSizes(ImageAspectRatio _ratio, bool verbose);
        Size FixSize(Size _size, bool sideways);

        // Low level frame reading.
        bool ReadMany(BackgroundWorker^ _bgWorker, VideoSection _section, bool _prepend);
        ReadResult ReadFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode, bool _approximate);
        int SeekTo(int64_t _target);
        bool RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace);
        static void DisposeFrame(VideoFrame^ _frame);
        
        // Decoding mode.
        void SwitchDecodingMode(VideoDecodingMode _mode);
        void SwitchToBestAfterCaching();
        bool WorkingZoneFitsInMemory(VideoSection _newZone, int _maxMemory);
        void ImportWorkingZoneToCache(System::Object^ sender,DoWorkEventArgs^ e);
        
        // Pre-buffering thread.
        void StartPreBuffering();
        void StopPreBuffering();
        void PreBufferingWorker(Object^ _canceler);

        // Degug dumps.
        void DumpInfo();
        static void DumpStreamsInfos(AVFormatContext* _pFormatCtx);
        static void DumpFrameType(int _type);
    };
}}}
