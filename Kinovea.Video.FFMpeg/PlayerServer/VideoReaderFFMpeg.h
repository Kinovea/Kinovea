#pragma region License
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
#ifndef __STDC_CONSTANT_MACROS
#define __STDC_CONSTANT_MACROS
#endif
#ifndef __STDC_LIMIT_MACROS
#define __STDC_LIMIT_MACROS
#endif
#include "libavcodec/avcodec.h"
#include "libavdevice/avdevice.h"
#include "libavfilter/avfilter.h"
#include "libavfilter/buffersink.h"
#include "libavfilter/buffersrc.h"
#include "libavformat/avformat.h"
#include "libavutil/avutil.h"
#include "libavutil/frame.h"
#include "libavutil/imgutils.h"
#include "libavutil/pixdesc.h"
#include "libavutil/display.h"
#include "libswresample/swresample.h"
#include "libswscale/swscale.h"
}

#include "ReadResult.h"
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
        ".3gp;.3gpp;.asf;.avi;.dv;.flv;.f4v;\
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
                return	mCapabilities; 
            }
        }
        virtual property VideoDecodingMode DecodingMode {
            VideoDecodingMode get() override { 
                return	mCachingMode; 
            }
        }
        virtual property bool Loaded {
            bool get() override { return mIsLoaded; }
        }
        virtual property VideoInfo Info {
            VideoInfo get() override { return mVideoInfo; }
        }
        virtual property IWorkingZoneFramesContainer^ WorkingZoneFrames {
            IWorkingZoneFramesContainer^ get() override { 
                if(mCachingMode == VideoDecodingMode::Caching)
                    return mCache;
                else 
                    return nullptr;
            }
        }
        virtual property VideoSection WorkingZone {
            // Return the internal working zone.
            VideoSection get() override { return mWorkingZone; }
        }

        virtual property VideoFrame^ Current {
            VideoFrame^ get() override { 
                return mFrameContainer != nullptr ? mFrameContainer->CurrentFrame : nullptr; 
            }
        }
        virtual property bool CanDrawUnscaled {
            bool get() override {
                return mCanDrawUnscaled;
            }
        }

    // Construction / Destruction.
    public:
        VideoReaderFFMpeg();
        ~VideoReaderFFMpeg();
    protected:
        !VideoReaderFFMpeg();


    public:
        
        /// Open the video and call Load().
        virtual OpenVideoResult Open(String^ _filePath) override;
        
        /// Unload the video and release unmanaged resources.
        virtual void Close() override;

        /// Open/Load the video, extract a few frames + basic info, close the video.
        virtual VideoSummary^ ExtractSummary(String^ _filePath, int _thumbs, Size _maxSize) override;
        
        /// Try to switch to a better caching strategy if possible.
        virtual void PostLoad() override;

        // Frame requests.
        virtual bool MoveNext(int _skip, bool _decodeIfNecessary) override;
        virtual bool MoveTo(int64_t from, int64_t target) override;
        
        // Decoding mode, play loop and frame enumeration.
        // TODO: these should be moved to C#.
        virtual void BeforePlayloop() override;
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

        static const enum AVPixelFormat sConvertPixelFormat = AV_PIX_FMT_BGRA;
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);

        // General
        VideoCapabilities mCapabilities;
        bool mIsLoaded;
        VideoInfo mVideoInfo;
        Dictionary<int64_t, TimedPoint^>^ mStabOffsets = gcnew Dictionary<int64_t, TimedPoint^>();
        
        // Summary extraction
        bool mIsForSummary;
        int64_t mSummaryPreviousSeek = AV_NOPTS_VALUE;

        // Decoding mode & working zone.
        int64_t mTimestampOffset = 0;
        VideoDecodingMode mCachingMode;
        VideoSection mWorkingZone;
        VideoSection mSectionToPrepend;
        VideoSection mSectionToAppend;
        
        // Output size after decode and scale filter.
        Size mDecodingSize;
        bool mCanDrawUnscaled;

        // Frame containers
        // mFrameContainer references one of the three below.
        // Only one is active at a time.
        IVideoFramesContainer^ mFrameContainer; 
        SingleFrame^ mSingleFrameContainer;
        PreBuffer2^ mPreBuffer;
        Cache^ mCache;
        
        // FFmpeg context.
        int mVideoStreamIndex;
        AVFormatContext* mFormatCtx;
        AVCodecContext* mVideoCodecCtx;
        

        // The frame-domain timestamp of the last stored frame (frame->best_effort_timestamp).
        // Stored as in put in the active frame container and available to the player.
        // This is not necessarily the frame that the player is currently showing.
        int64_t mCurrentTimestamp = AV_NOPTS_VALUE;
        
        // The frame-domain timestamp of the last decoded frame.
        int64_t mDecodedTimestamp = AV_NOPTS_VALUE;
        
        // The seek-domain timestamp of the last keyframe. (packet->pts or packet->dts).
        int64_t mCurrentGopTimestamp = AV_NOPTS_VALUE;

        // If the lag in seconds between the frame we are supposed to be presenting in the player 
        // and the last frame we decoded goes above this value, we declare decoding bankrupcy
        // and try to seek ahead to the next keyframe.
        static const double mSeekAheadLagThreshold = 1;

        // The active decoding policy (for pre-buffering). 
        // Determines if we should skip decoding or skip scaling/converting/storing 
        // some frames in case we fall behind the player demands.
        DecodingPolicy mDecodingPolicy = DecodingPolicy::Normal;

        

        // FFmpeg filter graph for scaling/converting the decoded frame to its final form.
        AVFilterGraph* mFilterGraph = nullptr;
        AVFilterContext* mFilterSource = nullptr;
        AVFilterContext* mFilterSink = nullptr;
        AVFrame* mFilteredFrame = nullptr;
        static bool mCopyFilteredFrame = true;

        // Active configuration of the filter graph.
        int mFilterSrcWidth = 0;
        int mFilterSrcHeight = 0;
        AVPixelFormat mFilterSrcFormat = AV_PIX_FMT_NONE;
        int mFilterDstWidth = 0;
        int mFilterDstHeight = 0;
        bool mFilterDeinterlace = false;

        // Others
        bool mWasPrebuffering;
        Thread^ mPreBufferingThread;
        ThreadCanceler^ mPreBufferingThreadCanceler;
        bool mVerbose = true;
        
        // Generic stopwatch for instrumentation/debugging purposes. 
        // Production logic should use its own stopwatch if needed.
        Stopwatch^ mStopwatch = gcnew Stopwatch();

        // Simple counter, only used for debugging and logging.
        int mDecodedFrames = 0;
    
    private:

        void DataInit();
        
        /// Load the video file and initialize the FFMpeg context.
        OpenVideoResult Load(String^ filePath, bool forSummary);
        
        /// Estimate the frame rate of the video stream.
        /// Updates mVideoInfo.FramesPerSeconds.
        void GuessFrameRate(AVFormatContext* formatCtx, AVCodecContext* videoCodecCtx, int streamIndex, bool verbose);

        /// Read one frame from the video stream and add it to the active frame container.
        /// Seeks backwards if needed.
        /// 
        /// If we are in the context of summary extraction seek to the nearest keyframe
        /// and decode only one frame, even if it's not the target.
        /// 
        /// Otherwise advance as many frames as needed to reach the target timestamp or frame.
        /// targetJumpFrame is relative to the current frame.
        ReadResult ReadFrame(int64_t targetTimestamp, int targetJumpFrame);
        
        /// Seek to a frame at or before the target. 
        /// Does not decode any frames.
        int SeekTo(int64_t targetTimestamp);

        /// Decode the next available frame from libav. 
        /// Demux and feed packets to libav until one frame is decoded or the end of the stream is reached.
        /// If a frame is already available doesn't demux anything.
        ReadResult DecodeOneFrame(AVFormatContext* formatCtx, int streamIndex, AVCodecContext* codecCtx, AVFrame* frame);

        /// Convert the libav AVFrame to a .NET Bitmap and store it to the container.
        /// 
        /// Calls rescale and convert to get a new AVFrame in the correct format.
        /// Sets up a bitmap and make it point to the AVFrame buffer.
        /// Applies stabilization offset.
        /// Applies image rotation.
        /// Creates a VideoFrame out of the bitmap and timestamp.
        /// Stores the VideoFrame in the active frame container.
        /// 
        /// Does not release the passed AVFrame.
        ReadResult ConvertAndStoreFrame(AVFrame* decodedFrame, bool forSummary);

        /// Apply the rotation to the .NET bitmap.
        void ApplyRotation(Bitmap^ bmp, ImageRotation rotation);
        
        /// Convert and scale the decoded frame to the final pixel format and size.
        /// dstFrame must already be allocated.
        /// Uses the old swscale pipeline.
        /// This variant does not support deinterlacing.
        bool RescaleAndConvert(
            AVFrame* srcFrame, AVFrame* dstFrame, 
            int dstWidth, int dstHeight, AVPixelFormat dstPixelFormat,
            bool forSummary);

        /// Convert and scale the decoded frame to the final pixel format and size.
        /// dstFrame must already be allocated.
        /// Uses the new filter graph pipeline.
        bool RescaleAndConvert2(AVFrame* srcFrame, AVFrame* dstFrame, int dstWidth, int dstHeight, AVPixelFormat dstPixelFormat, bool deinterlace);
        
        /// Create the filter graph.
        /// buffer -> [yadif] -> scale -> format -> buffersink.
        /// Should only be called when the parameters change.
        bool CreateVideoFilterGraph(
            int srcWidth, int srcHeight, AVPixelFormat srcPixelFormat,
            int dstWidth, int dstHeight,
            bool deinterlace, AVRational sar);

        void FreeVideoFilterGraph();

        /// Get the source format of decoded frames.
        /// This is just ctx->pix_fmt unless the user has specified a demosaicing option.
        AVPixelFormat GetSourceFormat(AVCodecContext* videoCodecCtx);

        /// <summary>
        /// Change the ffmpeg codec context based on the passed policy. 
        /// This determines whether we will decode everything or skip some frames.
        /// </summary>
        void UpdateDecodingPolicy(DecodingPolicy policy);
        

        /// Release the memory allocated by libav for the frame buffer.
        static void DisposeFrame(VideoFrame^ _frame);
        
        // Decoding size.
        void ResetDecodingSize();
        void UpdateReferenceSizes(ImageAspectRatio _ratio, bool verbose);
        Size FixSize(Size _size, bool sideways);

        /// Returns how many megabytes the working zone requires to be fully loaded in memory.
        /// This is for full size frames, not decoding size.
        double WorkingZoneMemoryRequirement(VideoSection _newZone);

        // DecodeScheduler
        // The following functions should eventually be refactored and moved to different classes.
        bool ReadManyToCache(BackgroundWorker^ _bgWorker, VideoSection _section, bool _prepend);
        void ChangeCachingMode(VideoDecodingMode wantedMode);
        void ChangeToBestAfterCaching();
        void ImportWorkingZoneToCache(System::Object^ sender,DoWorkEventArgs^ e);
        void StartPreBuffering();
        void StopPreBuffering();
        void PreBufferingWorker(Object^ _canceler);

        // Logging helpers.
        void LogFileInfo();
        static void LogPacketInfo(AVPacket* packet);
        static void LogFrameInfo(AVFrame* frame);
        static void LogFFMpegError(String^ context, int errorCode);
        static void LogStreamList(AVFormatContext* formatCtx);
        static String^ GetFrameTypeString(int type);
        static String^ GetFrameFormatString(AVPixelFormat format);
    };
}}}
