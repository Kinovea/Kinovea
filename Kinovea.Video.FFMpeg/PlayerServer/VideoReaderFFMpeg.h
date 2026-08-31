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
    public: 
        virtual property VideoCapabilities Flags 
        {
            VideoCapabilities get() override 
            {
                return	mCapabilities; 
            }
        }
        virtual property VideoDecodingMode DecodingMode 
        {
            VideoDecodingMode get() override 
            { 
                return	mCachingMode; 
            }
        }
        virtual property bool Loaded 
        {
            bool get() override 
            { 
                return mIsLoaded; 
            }
        }
        virtual property VideoInfo Info 
        {
            VideoInfo get() override { return mVideoInfo; }
        }
        virtual property IWorkingZoneFramesContainer^ WorkingZoneFrames 
        {
            IWorkingZoneFramesContainer^ get() override 
            { 
                if(mCachingMode == VideoDecodingMode::Caching)
                    return mCache;
                else 
                    return nullptr;
            }
        }
        virtual property VideoSection WorkingZone 
        {
            VideoSection get() override { return mWorkingZone; }
        }
        virtual property VideoFrame^ Current 
        {
            VideoFrame^ get() override 
            { 
                return mFrameContainer != nullptr ? mFrameContainer->CurrentFrame : nullptr; 
            }
        }
        virtual property VideoGeometry^ Geometry 
        {
            VideoGeometry^ get() override 
            {
                return mVideoGeometry;
            }
        }

    // Construction / Destruction.
    public:
        VideoReaderFFMpeg();
        ~VideoReaderFFMpeg();
    protected:
        !VideoReaderFFMpeg();

    public:
        
        //-------------------
        // Open/Close/Summary
        //-------------------
        
        /// Open the video and call Load().
        virtual OpenVideoResult Open(String^ _filePath) override;
        
        /// Unload the video and release unmanaged resources.
        virtual void Close() override;

        /// Open/Load the video, extract a few frames + basic info, close the video.
        virtual VideoSummary^ ExtractSummary(String^ _filePath, int _thumbs, Size _maxSize) override;
        
        //-------------------
        // Navigation and player state
        //-------------------
        virtual bool MoveNext(bool decodeIfNecessary) override;
        virtual bool MoveTo(int64_t target) override;
        virtual bool PlayerRequest(PlayerState^ newState) override;
        virtual void BeforePlayloop() override;

        //-------------------
        // Working zone and decoding mode
        //-------------------
        virtual void UpdateWorkingZone(VideoSection newZone, CacheLoadMode loadMode, int maxMemory, Action<DoWorkEventHandler^>^ workerFn) override;

        /// If we are not caching yet, try to switch to prebuffering.
        virtual void StartPrebufferingIfNotCaching() override;

        //-------------------
        // Frame enumeration
        //-------------------
        virtual void BeforeFrameEnumeration() override;
        virtual void AfterFrameEnumeration() override;

        //-------------------
        // Frame enumeration
        //-------------------
        virtual bool UpdateVideoGeometry(VideoGeometryRequest^ request) override;

    // Members
    private:

        static const enum AVPixelFormat sConvertPixelFormat = AV_PIX_FMT_BGRA;
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);

        // General
        VideoCapabilities mCapabilities;
        bool mIsLoaded;
        VideoInfo mVideoInfo;
        int64_t mTimestampOffset = 0;

        // Summary extraction
        bool mIsForSummary;
        int64_t mSummaryPreviousSeek = AV_NOPTS_VALUE;


        // Video Geometry configuration (rotation, deinterlacing, etc.)
        VideoGeometry^ mVideoGeometry;                  // Current published geometry.
        VideoGeometryRequest^ mVideoGeometryRequest;    // Last player side request.
        Dictionary<int64_t, TimedPoint^>^ mStabOffsets = gcnew Dictionary<int64_t, TimedPoint^>();
        
        // We deal with a bunch of sizes for different purposes.
        // 1. original size -> size of images out of the decoder. Stored in mVideoInfo.OriginalSize. 
        // 2. scaled size -> size of images out of the scaler, taking aspect ratio force into account.
        // 3. reference size -> rotated original size, used for coordinates. Stored in mVideoGeometry.ReferenceSize.
        // 4. output size -> rotated scaled size. What output images use. Stored in mVideoGeometry.OutputSize.
        // In practice we compute the output size from the reference size, 
        // and then we derive scaled size from the output based on rotation.
        // Only the reference size and output size are relevant to the player.
        Size mOriginalSize;     // ex: 1920 x 1080.
        Size mScaledSize;       // ex: 960 x 540.
        Size mReferenceSize;    // ex: 1080 x 1920.
        Size mOutputSize;       // ex: 540 x 960.


        // Working zone.
        VideoSection mWorkingZone;
        bool mIsFirstWZUpdate = true;

        // Caching mode and frame containers.
        // mFrameContainer points to one of the three below.
        // Only one is active at a time based on the caching mode.
        VideoDecodingMode mCachingMode;
        IVideoFramesContainer^ mFrameContainer; 
        SingleFrame^ mSingleFrameContainer;
        PreBuffer2^ mPreBuffer;
        Cache^ mCache;

        // Full caching mode.
        VideoSection mSectionToPrepend;
        VideoSection mSectionToAppend;
        
        
        //----------------------------------
        // FFmpeg context and decoder state.
        //----------------------------------
        AVFormatContext* mFormatCtx;
        int mVideoStreamIndex;
        AVCodecContext* mVideoCodecCtx;

        // The frame-domain timestamp of the frame we last added to the cache.
        // This is not necessarily the frame that the player is currently showing.
        int64_t mCachedTimestamp = AV_NOPTS_VALUE;

        // The frame-domain timestamp of the last decoded frame.
        int64_t mDecodedTimestamp = AV_NOPTS_VALUE;

        // The timestamp of the previous decoded decoded frame.
        int64_t mPreviousDecodedTimestamp = AV_NOPTS_VALUE;

        // The seek-domain timestamp of the last keyframe. (packet->pts or packet->dts).
        int64_t mCurrentGopTimestamp = AV_NOPTS_VALUE;

        //------------------------
        // Player state requests and decoding jobs
        //------------------------
        
        // The player state / job being worked on by the prebuffer thread.
        // This belongs solely to the prebuffer thread.
        PlayerState^ mWorkingPlayerState = PlayerState::Empty;

        // The job id matching the player state that has been 
        // received and prepared by the reader.
        // Written by the reader, read by the prebuffer thread.
        // All access must be protected by mLockNewJobReady.
        int mReadyJobId = -1;

        // Whether the request was fulfilled during the initial phase.
        // Written by the reader, read by the prebuffer thread.
        bool mRequestFulfilled = false;

        // Sync object to schedule the arrival and preparation of new player requests.
        Object^ mLockNewJobReady = gcnew Object();

        //----------------------------
        // Prebuffering
        //----------------------------

        Thread^ mPreBufferingThread;
        ThreadCanceler^ mPreBufferingThreadCanceler;

        // A frame that was waiting for space in the cache when the request 
        // for a new job arrived. 
        // We keep it around and may re-submit it when the new job starts.
        VideoFrame^ mPendingFrame;

        // When a request for a new job arrives this holds the result of the 
        // initial attempt to acquire the frame.
        // This is then used to figure out the relocation plan.
        TryAcquireResult^ mTryAcquireResult = nullptr;

        // When relocating the decoder if we get close to the target by this amount
        // we start storing frames. This way the target frame always has some frames
        // already available behind it for stepping back.
        int64_t mPreRollTimestamps = AV_NOPTS_VALUE;

        // The active frame-skipping policy (only for pre-buffering and during playback). 
        // Determines if we should skip scaling/converting/storing some frames or even 
        // skip decoding them altogether, in case we fall behind the player demands.
        FrameSkippingPolicy mFrameSkippingPolicy = FrameSkippingPolicy::Normal;

        // If the lag in seconds between the frame we are supposed to be presenting in the player 
        // and the last frame we decoded goes above this value, we declare decoding bankrupcy
        // and try to seek ahead to the next keyframe.
        static const double mSeekAheadLagThreshold = 1;


        bool mWasPrebufferingBeforeEnumeration;
        

        //------------------------
        // Scale/convert
        //------------------------
         
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

        //------------------------
        // Debugging
        //------------------------
        bool mVerbose = true;
        
        // Generic stopwatch for instrumentation/debugging purposes. 
        // Production logic should use its own stopwatch if needed.
        Stopwatch^ mStopwatch = gcnew Stopwatch();
        LoopWatcher^ mLoopWatcher = gcnew LoopWatcher();
        // Simple counter, only used for debugging and logging.
        int mDecodedFrames = 0;
    
    private:

        //-------------------
        // Open/Close/Summary
        //-------------------

        void DataInit();
        
        /// Load the video file and initialize the FFMpeg context.
        OpenVideoResult Load(String^ filePath, bool forSummary);
        
        /// Estimate the frame rate of the video stream.
        /// Updates mVideoInfo.FramesPerSeconds.
        void GuessFrameRate(AVFormatContext* formatCtx, AVCodecContext* videoCodecCtx, int streamIndex, bool verbose);


        //-------------------
        // Navigation / player demands
        //-------------------
        bool MoveOnDemand(int64_t target);
        bool MoveCaching(int64_t target);
        bool MovePrebuffer(int64_t target);

        //-------------------
        // Seeking/decoding
        //-------------------

        /// Read one frame at the start of the GOP containing the target.
        ReadResult ReadFrameThumbnail(int64_t targetTimestamp);

        /// Read the next frame.
        ReadResult VideoReaderFFMpeg::ReadFrameNext();

        /// Seek and decode until the target is reached.
        /// Store the reached frame to the container.
        ReadResult ReadFrameSeek(int64_t targetTimestamp, bool doSeek, bool preRoll);
        
        /// Seek to or before the target. Does not decode any frames.
        int SeekTo(int64_t targetTimestamp);

        /// Decode the next available frame from libav. 
        /// Demux and feed packets to libav until one frame is decoded or the end of the stream is reached.
        /// If a frame is already available doesn't demux anything.
        ReadResult DecodeOneFrame(AVFormatContext* formatCtx, int streamIndex, AVCodecContext* codecCtx, AVFrame* frame);

        /// Release the memory allocated by libav for the frame buffer.
        static void DisposeFrame(VideoFrame^ _frame);

        //-------------------
        // scale/conversion/store
        //-------------------

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
        ReadResult ConvertAndStoreFrame(AVFrame* decodedFrame, bool forSummary, bool force);

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

        //-------------------
        // Video geometry
        //-------------------
        void ResolveGeometry(VideoGeometryRequest^ request);
        bool SetStabilizationData(List<TimedPoint^>^ points);
        
        /// Computes the reference size and set an initial scaled size.
        void ComputeReferenceSize(ImageAspectRatio aspectRatio, ImageRotation rotation);
        Size PadSize(Size size, bool isSideway);
        

        //-------------------
        // Working zone and decoding mode
        //-------------------

        /// Returns how many megabytes the working zone requires to be fully loaded in memory.
        /// This is for full size frames, not decoding size.
        double WorkingZoneMemoryRequirement(VideoSection _newZone);

        void FirstUpdateWorkingZone(
            VideoSection newZone, 
            CacheLoadMode loadMode,
            bool fitsInMemory,
            Action<DoWorkEventHandler^>^ workerFn) override;

        /// Clears the old frame container and points it to the one of the new mode.
        /// This should be called after validating that the mode is available.
        /// This function stops the prebuffer thread if needed.
        /// It doesn't start the full cache loading nor the prebuffering thread.
        void ChangeCachingMode(VideoDecodingMode newMode);

        
        //--------------------
        // Static cache
        //--------------------

        void ImportWorkingZoneToCache(System::Object^ sender, DoWorkEventArgs^ e);
        
        bool ReadManyToCache(BackgroundWorker^ _bgWorker, VideoSection _section);
        

        //-------------------
        // Prebuffering
        //-------------------

        /// Start the prebuffering thread.
        void StartPreBufferingThread(int64_t startTimestamp);
        
        /// Stop the prebuffering thread.
        /// Does not clear the frame cache, does not change the caching mode.
        void StopPreBufferingThread();
        
        void PreBufferingWorker(Object^ _canceler);

        /// Check if a new job has been posted by the player.
        bool HasJobChanged();

        /// Pause the decoder thread until a new job is ready.
        PlayerState^ WaitForNewJobReady(ThreadCanceler^ canceller, int currentJobId);

        ReadResult ProcessJob(ThreadCanceler^ canceller);

        //------------------
        // Prebuffering > Job initialization.
        //------------------

        /// Prepare the decoder for the next job.
        void InitDecodingJob(PlayerState^ state);

        /// Get a relocation plan for the decoder, to seek, advance or stay in place,
        /// and what to do with the pending frame if any.
        /// This may also acquire the target if found to be available.
        DecodingJobPlan^ GetDecodingJobPlan(PlayerState^ state, TryAcquireResult^ tryAcquireResult);

        /// Perform the decoder relocation according to the plan.
        bool ExecuteDecodingJobPlan(PlayerState^ state, DecodingJobPlan^ plan);

        /// Dispose the pending frame and set it to null.
        void DisposePending();

        /// Resubmit the pending frame. 
        /// If the add succeeds and it matches the target, acquires it.
        /// Returns true if the target was acquired.
        bool ResubmitPending(int64_t target, bool forced);

        //------------------
        // Prebuffering > Frame skipping policy.
        //------------------

        /// Update the playback frame skipping policy based on how far behind 
        /// we are compared to the player.
        /// Only used during playback.
        void UpdateFrameSkippingPolicy();

        /// Change the ffmpeg codec context based on the passed policy. 
        /// This determines whether we will decode everything or skip certain frames.
        /// Only used during playback.
        void ApplyFrameSkippingPolicy(FrameSkippingPolicy policy);

        /// Whether the scale/convert/store step should be skipped 
        /// as part of the frame skipping policy.
        bool ShouldStoreFrame();

        //-------------------
        // Logging helpers
        //-------------------
        void LogFileInfo();
        static void LogPacketInfo(AVPacket* packet);
        static void LogFrameInfo(AVFrame* frame);
        static void LogFFMpegError(String^ context, int errorCode);
        static void LogStreamList(AVFormatContext* formatCtx);
        static void LogVideoGeometry(VideoGeometry^ geometry);
        static String^ GetFrameTypeString(int type);
        static String^ GetFrameFormatString(AVPixelFormat format);
    };
}}}
