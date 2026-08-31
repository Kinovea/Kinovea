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

#include <msclr\lock.h>
#include "VideoReaderFFMpeg.h"

using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Drawing2D;
using namespace System::Drawing::Imaging;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;
using namespace System::Text;
using namespace System::Threading;
using namespace msclr;

using namespace Kinovea::Services;
using namespace Kinovea::Video::FFMpeg;

#pragma region Construction/Destruction
VideoReaderFFMpeg::VideoReaderFFMpeg()
{
    mPreBufferingThreadCanceler = gcnew ThreadCanceler();

    VideoFrameDisposer^ disposer = gcnew VideoFrameDisposer(DisposeFrame);

    mSingleFrameContainer = gcnew SingleFrame(disposer);
    mPreBuffer = gcnew PreBuffer2(disposer);
    mCache = gcnew Cache(disposer);
    
    DataInit();
}

VideoReaderFFMpeg::~VideoReaderFFMpeg()
{
    this->!VideoReaderFFMpeg();
}

VideoReaderFFMpeg::!VideoReaderFFMpeg()
{
    if (mIsLoaded)
        Close();
}

void VideoReaderFFMpeg::DataInit()
{
    ChangeCachingMode(VideoDecodingMode::NotInitialized);
    mIsLoaded = false;
    mVideoStreamIndex = -1;
    mVideoInfo = VideoInfo::MakeEmpty();
    mWorkingZone = VideoSection::MakeEmpty();

    mCachedTimestamp = AV_NOPTS_VALUE;
    mPreviousDecodedTimestamp = AV_NOPTS_VALUE;
    mDecodedTimestamp = AV_NOPTS_VALUE;
    mCurrentGopTimestamp = AV_NOPTS_VALUE;
    
    mWasPrebufferingBeforeEnumeration = false;
    
    mVideoGeometry = nullptr;
    mVideoGeometryRequest = nullptr;

    mOriginalSize = Size::Empty;
    mScaledSize = Size::Empty;
    mReferenceSize = Size::Empty;
    mOutputSize = Size::Empty;
}
#pragma endregion

#pragma region Open/Close
OpenVideoResult VideoReaderFFMpeg::Open(String^ filePath)
{
    OpenVideoResult result = Load(filePath, false);
    if (result == OpenVideoResult::Success)
        LogFileInfo();

    return result;
}

void VideoReaderFFMpeg::Close()
{
    // Unload the video and dispose unmanaged resources.
    if (!mIsLoaded)
        return;

    DataInit();

    if (mVideoCodecCtx != nullptr)
    {
        AVCodecContext* pVideoCodecCtx = mVideoCodecCtx;
        avcodec_free_context(&pVideoCodecCtx);
        mVideoCodecCtx = nullptr;
    }

    if (mFormatCtx != nullptr)
    {
        AVFormatContext* pFormatCtx = mFormatCtx;
        avformat_close_input(&pFormatCtx);
        mFormatCtx = nullptr;
    }
}


VideoSummary^ VideoReaderFFMpeg::ExtractSummary(String^ filePath, int count, Size maxSize)
{
    // Open the file and extract some info + a few thumbnails.
    mVerbose = false;
    mIsForSummary = true;
    VideoSummary^ summary = gcnew VideoSummary(filePath);

    // Allocate 100 ms to this task. 
    // Always get at least one image but after that if we run out of time we cancel.
    int64_t timeout = 100;
    Stopwatch^ stopwatchSummary = Stopwatch::StartNew();

    OpenVideoResult loaded = Load(filePath, true);
    if (loaded != OpenVideoResult::Success)
    {
        return summary;
    }

    ComputeReferenceSize(ImageAspectRatio::Auto, mVideoInfo.OriginalRotation);
    mScaledSize = FitHelper::Fit(mScaledSize, maxSize, true);
    // We don't publish the output size here so we don't have to update it.

    ChangeCachingMode(VideoDecodingMode::OnDemand);

    summary->IsImage = mVideoInfo.DurationTimeStamps == 1;
    double durationSeconds = mVideoInfo.DurationTimeStamps / mVideoInfo.AverageTimeStampsPerSeconds;
    summary->DurationMilliseconds = (int64_t)Math::Round(durationSeconds * 1000.0);
    summary->ImageSize = mVideoGeometry->ReferenceSize;
    summary->Framerate = mVideoInfo.FramesPerSeconds;

    String^ filename = Path::GetFileName(filePath);
    //log->DebugFormat("ExtractSummary \"{0}\"", filename);
    
    // Sample frames throughout the video.
    auto targets = gcnew List<int64_t>();
    int64_t start = mVideoInfo.FirstTimeStamp;
    int64_t end = (int64_t)Math::Round(start + mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame);
    if (count == 1)
    {
        targets->Add((int64_t)Math::Round((double)(start + end) / 2.0));
    }
    else
    {
        int64_t interval = (int64_t)Math::Round((double)(end - start) / count);
        for (int i = 0; i < count; i++)
        {
            int64_t ts = start + interval * i;
            targets->Add(ts);
        }
    }

    mSummaryPreviousSeek = AV_NOPTS_VALUE;
    for (int i = 0; i < targets->Count; i++)
    {
        int64_t ts = targets[i];
        
        ReadResult read = ReadFrameThumbnail(ts);

        if (read == ReadResult::Same)
        {
            // We detected that the seek would have landed on the same key frame as before.
            // This may happen for files with large GOP. 
            // We skipped the decode entirely.
            //log->DebugFormat("Same seek. [{0}]. {1} ms.", mSummaryPreviousSeek, stopwatchSummary->ElapsedMilliseconds);
            continue;
        }

        if (read != ReadResult::Success || mFrameContainer->CurrentFrame == nullptr)
        {
            // Bail out on any error.
            break;
        }

        Bitmap^ bmp = BitmapHelper::CopyBgr32Rows(mFrameContainer->CurrentFrame->Image);
        summary->Thumbs->Add(bmp);

        // Check if we are out of time budget.
        if (stopwatchSummary->ElapsedMilliseconds > timeout && i < targets->Count - 1)
        {
            log->WarnFormat("Summary extraction out of budget after {0} frames in {1} ms.", 
                i + 1, stopwatchSummary->ElapsedMilliseconds);
            break;
        }
    }

    log->DebugFormat("Summary extraction for \"{0}\": {1} ms.", filename, stopwatchSummary->ElapsedMilliseconds);

    Close();
    mIsForSummary = false;
    return summary;
}



OpenVideoResult VideoReaderFFMpeg::Load(String^ filePath, bool forSummary)
{
    OpenVideoResult result = OpenVideoResult::Success;

    if (mIsLoaded)
    {
        Close();
    }

    mVideoInfo.FilePath = filePath;
    
    AVFormatContext* formatCtx = avformat_alloc_context();

    // If we are opening the file just to extract thumbnails we take some shortcuts.
    // - Try to avoid calling avformat_find_stream_info and look for video stream manually.
    // - For the case where we can't do that, use limited probing settings.
    // - Disable multithreading which can increase buffering and latency, 
    // we only decode keyframes anyway so buffering shouldn't be useful.
    
    if (forSummary)
    {
        // Note: flag AVFMT_FLAG_NOBUFFER is too aggressive, some files fail to decode frames.
        formatCtx->probesize = 32 * 1024;
        formatCtx->max_analyze_duration = 25 * 1000;
        formatCtx->max_probe_packets = 10;
        formatCtx->flags |= AVFMT_FLAG_FAST_SEEK;
    }

    // FFmpeg expects filenames as UTF-8, .NET strings are UTF-16.
    // On Windows, FFmpeg will convert UTF-8 paths back to UTF-16 internally.
    int byteCount = Encoding::UTF8->GetByteCount(filePath);
    array<Byte>^ utf8Path = gcnew array<Byte>(byteCount + 1);
    Encoding::UTF8->GetBytes(filePath, 0, filePath->Length, utf8Path, 0);
    pin_ptr<Byte> pinnedPath = &utf8Path[0];
    const char* pszFilePath = reinterpret_cast<const char*>(pinnedPath);
    
    int res = avformat_open_input(&formatCtx, pszFilePath, nullptr, nullptr);
    if (res != 0)
    {
        log->ErrorFormat("The file {0} could not be openned. (Wrong path or not a video/image.)", filePath);
        return OpenVideoResult::FileNotOpenned;
    }

    mVideoStreamIndex = -1;
    if (forSummary)
    {
        // The call to avformat_find_stream_info can be quite expensive so 
        // for extracting thumbnails we just try to find the codec without probing.
        // This should work for most formats.
        // It fails for a few formats that have auto-descriptive frames like transport streams,
        // and for files with multiple video streams it may use the wrong stream.
        // This is rare and not catastrophic, we'll use the full workflow when opening the file.
        for (int i = 0; i < formatCtx->nb_streams; i++)
        {
            AVStream* stream = formatCtx->streams[i];
            AVCodecParameters* codecpar = stream->codecpar;
            if (codecpar->codec_type == AVMEDIA_TYPE_VIDEO)
            {
                // Verify we have the minimal info needed to decode frames.
                if (codecpar->codec_id != AV_CODEC_ID_NONE && codecpar->width > 0 && codecpar->height > 0)
                {
                    mVideoStreamIndex = i;
                }

                break;
            }
        }
    }

    if (!forSummary || (mVideoStreamIndex < 0))
    {
        // Get stream info by probing the first packets.
        res = avformat_find_stream_info(formatCtx, nullptr);
        if (res < 0)
        {
            log->ErrorFormat("Stream info not found. Error: {0}.", res);
            return OpenVideoResult::StreamInfoNotFound;
        }

        mVideoStreamIndex = av_find_best_stream(formatCtx, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
        if (mVideoStreamIndex < 0)
        {
            log->Error("No video stream found in the file.");
            return OpenVideoResult::VideoStreamNotFound;
        }
    }

    // Video stream.
    AVStream* videoStream = formatCtx->streams[mVideoStreamIndex];

    // Find, allocate and open the video codec context.
    AVCodecID videoCodecId = videoStream->codecpar->codec_id;
    const AVCodec* videoCodec = avcodec_find_decoder(videoCodecId);
    if (videoCodec == nullptr)
    {
        log->Error("Video decoder not found.");
        return OpenVideoResult::CodecNotFound;
    }

    AVCodecContext* videoCodecCtx = avcodec_alloc_context3(videoCodec);
    if (videoCodecCtx == nullptr)
    {
        log->Error("Video codec context allocation failed.");
        return OpenVideoResult::CodecNotOpened;
    }

    res = avcodec_parameters_to_context(videoCodecCtx, videoStream->codecpar);
    if (res < 0)
    {
        log->ErrorFormat("avcodec_parameters_to_context failed. Error: {0}", res);
        return OpenVideoResult::CodecNotOpened;
    }

    //-----------------------------------------------------
    // Collect image size and rotation
    //-----------------------------------------------------
    mOriginalSize = Size(videoCodecCtx->width, videoCodecCtx->height);
    mVideoInfo.OriginalSize = mOriginalSize;
    mVideoInfo.OriginalRotation = ImageRotation::Rotate0;
    const AVPacketSideData* displaymatrix = av_packet_side_data_get(videoStream->codecpar->coded_side_data, videoStream->codecpar->nb_coded_side_data, AV_PKT_DATA_DISPLAYMATRIX);
    if (displaymatrix)
    {
        // Get rotation as a double in [-180..+180].
        double rotation = Math::Round(av_display_rotation_get((const int32_t*)displaymatrix->data));
        // Map to 0..360 range.
        // Ignore rotations that aren't multiples of 90.
        rotation = ((int)-rotation + 360) % 360;
        if (rotation == 90)
            mVideoInfo.OriginalRotation = ImageRotation::Rotate90;
        else if (rotation == 180)
            mVideoInfo.OriginalRotation = ImageRotation::Rotate180;
        else if (rotation == 270)
            mVideoInfo.OriginalRotation = ImageRotation::Rotate270;
    }

    //-----------------------------------------------------
    // Configure codec
    //-----------------------------------------------------
    if (forSummary)
    {
        // Bypass deblocking/loop filter.
        videoCodecCtx->skip_loop_filter = AVDISCARD_ALL;
        //videoCodecCtx->skip_frame = AVDISCARD_NONKEY;
    }
    else
    {
        // Enable multithreading.
        // This increases the buffering in the decoder so only do this for actual playback
        // not for summary where we only do seeking.
        videoCodecCtx->thread_count = 0;
        if (videoCodec->capabilities & AV_CODEC_CAP_FRAME_THREADS)
        {
            videoCodecCtx->thread_type = FF_THREAD_FRAME;
        }
        else if (videoCodec->capabilities & AV_CODEC_CAP_SLICE_THREADS)
        {
            videoCodecCtx->thread_type = FF_THREAD_SLICE;
        }
        else
        {
            videoCodecCtx->thread_count = 1;
        }
    }
    
    res = avcodec_open2(videoCodecCtx, videoCodec, nullptr);
    if (res < 0) 
    {
        log->ErrorFormat("Codec could not be openned. Error: {0}", res);
        return OpenVideoResult::CodecNotOpened;
    }

    //-----------------------------------------------------
    // Collect timing info.
    //-----------------------------------------------------
    // videoStream->nb_frames == 0 can happen.
    // videoStream->duration <= 0 can happen.
    
    bool verbose = !forSummary;
    mVideoInfo.AverageTimeStampsPerSeconds = (double)videoStream->time_base.den / (double)videoStream->time_base.num;

    // This may be updated after the first actual decoding.
    // Ignore negative start time.
    double startSeconds = (double)formatCtx->start_time / AV_TIME_BASE;
    long firstTimestamp = (long)Math::Round(startSeconds * mVideoInfo.AverageTimeStampsPerSeconds);
    mVideoInfo.FirstTimeStamp = Math::Max(firstTimestamp, 0);

    mVideoInfo.DurationTimeStamps = 0;
    if (videoStream->duration > 0)
    {
        mVideoInfo.DurationTimeStamps = videoStream->duration;
    }
    else if (formatCtx->duration > 0)
    {
        double durationSeconds = (double)formatCtx->duration / (double)AV_TIME_BASE;
        mVideoInfo.DurationTimeStamps = (int64_t)Math::Round(durationSeconds * mVideoInfo.AverageTimeStampsPerSeconds);
    }

    if (mVideoInfo.DurationTimeStamps <= 0)
    {
        log->Error("Duration info not found.");
        return OpenVideoResult::StreamInfoNotFound;
    }
        
    mVideoInfo.FramesPerSeconds = 0;
    GuessFrameRate(formatCtx, videoCodecCtx, mVideoStreamIndex, verbose);

    mVideoInfo.FrameIntervalMilliseconds = 1000.0 / mVideoInfo.FramesPerSeconds;
    mVideoInfo.AverageTimeStampsPerFrame = mVideoInfo.AverageTimeStampsPerSeconds / mVideoInfo.FramesPerSeconds;

    // Initialize tolerance and thresholds.
    double tolerance = 0.5 * mVideoInfo.AverageTimeStampsPerFrame;
    mCache->Tolerance = tolerance;
    mPreBuffer->Tolerance = tolerance;
    mPreBuffer->FarAheadThreshold = 50.0 * mVideoInfo.AverageTimeStampsPerFrame;
    mPreRollTimestamps = (mPreBuffer->RetentionWindow + 2) * mVideoInfo.AverageTimeStampsPerFrame;


    // Initial working zone representing the whole video.
    // For the end timestamp we can calculate from either frame count or duration.
    // Compute both and use the max.
    int64_t lastTimestamp = mVideoInfo.FirstTimeStamp + mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame;
    if (videoStream->nb_frames > 0)
    {
        int64_t lastFrameTimestamp = mVideoInfo.FirstTimeStamp + (videoStream->nb_frames - 1) * mVideoInfo.AverageTimeStampsPerFrame;
        lastTimestamp = Math::Max(lastTimestamp, lastFrameTimestamp);
    }
    
    mWorkingZone = VideoSection(mVideoInfo.FirstTimeStamp, lastTimestamp);

    //-------------------------------
    // Remember if the codec is MPEG2. 
    // We use this to detect a specific behavior related to sample aspect ratio.
    mVideoInfo.IsCodecMpeg2 = (videoCodecId == AV_CODEC_ID_MPEG2VIDEO);

    if (videoCodecCtx->sample_aspect_ratio.num != 0 && videoCodecCtx->sample_aspect_ratio.num != videoCodecCtx->sample_aspect_ratio.den)
    {
        // Anamorphic video, non square pixels.
        if (mVideoInfo.IsCodecMpeg2)
        {
            // If MPEG, sample_aspect_ratio is actually the display aspect ratio.
            // Reference for weird decision tree: mpeg12.c at mpeg_decode_postinit().
            double displayAspect = (double)videoCodecCtx->sample_aspect_ratio.num / (double)videoCodecCtx->sample_aspect_ratio.den;
            mVideoInfo.PixelAspectRatio = ((double)videoCodecCtx->height * displayAspect) / (double)videoCodecCtx->width;

            if (mVideoInfo.PixelAspectRatio < 1.0f)
            {
                mVideoInfo.PixelAspectRatio = displayAspect;
            }
        }
        else
        {
            mVideoInfo.PixelAspectRatio = (double)videoCodecCtx->sample_aspect_ratio.num / (double)videoCodecCtx->sample_aspect_ratio.den;
        }

        mVideoInfo.SampleAspectRatio = Fraction(videoCodecCtx->sample_aspect_ratio.num, videoCodecCtx->sample_aspect_ratio.den);
    }
    else
    {
        // Assume PAR=1:1.
        mVideoInfo.PixelAspectRatio = 1.0f;
    }

        
    mFormatCtx = formatCtx;
    mVideoCodecCtx = videoCodecCtx;

    mIsLoaded = true;

    if (forSummary)
    {
        mCapabilities = VideoCapabilities::CanDecodeOnDemand;
    }
    else
    {
        mCapabilities =
            VideoCapabilities::CanDecodeOnDemand |
            VideoCapabilities::CanPreBuffer |
            VideoCapabilities::CanCache |
            VideoCapabilities::CanChangeAspectRatio |
            VideoCapabilities::CanChangeImageRotation |
            VideoCapabilities::CanChangeDeinterlacing |
            VideoCapabilities::CanChangeWorkingZone |
            VideoCapabilities::CanStabilize;

        if (mVideoCodecCtx->codec_id == AV_CODEC_ID_RAWVIDEO)
        {
            mCapabilities = mCapabilities | VideoCapabilities::CanChangeDemosaicing;
        }
    }

    // Start with no caching, we'll switch later if possible.
    ChangeCachingMode(VideoDecodingMode::OnDemand);

    // Initialize the video geometry.
    ComputeReferenceSize(ImageAspectRatio::Auto, mVideoInfo.OriginalRotation);
    mScaledSize = mOriginalSize;
    mOutputSize = mReferenceSize;
    bool isPrescaled = false;           // Set this to false because we don't have an actual presentation size yet.
    float scale = 1.0f;
    mVideoGeometry = gcnew VideoGeometry(
        mReferenceSize, 
        mOutputSize,
        isPrescaled,
        scale,
        ImageAspectRatio::Auto,
        mVideoInfo.OriginalRotation,
        Demosaicing::None,
        false,
        false,
        0);

    if (!forSummary)
    {
        LogVideoGeometry(mVideoGeometry);
    }

    return OpenVideoResult::Success;
}


void VideoReaderFFMpeg::GuessFrameRate(AVFormatContext* formatCtx, AVCodecContext* videoCodecCtx, int streamIndex, bool verbose)
{
    AVStream* stream = formatCtx->streams[mVideoStreamIndex];
    if (stream->avg_frame_rate.den != 0)
    {
        mVideoInfo.FramesPerSeconds = av_q2d(stream->avg_frame_rate);
        if (verbose) { log->Debug("Framerate estimation: stream average frame rate."); }
        return;
    }

    // Check stream frames and duration.
    if (stream->nb_frames > 0)
    {
        if (stream->duration > 0 && stream->time_base.num != 0 && stream->time_base.den != 0)
        {
            // Stream duration is expressed in the stream time base.
            mVideoInfo.FramesPerSeconds = ((double)stream->nb_frames * stream->time_base.den) / (stream->duration * stream->time_base.num);
            if (verbose) { log->Debug("Framerate estimation: stream frames / stream duration."); }
            return;
        }
        
        if (formatCtx->duration > 0)
        {
            // Container duration is expressed in the canonical time base AV_TIME_BASE = microseconds.
            mVideoInfo.FramesPerSeconds = ((double)stream->nb_frames * AV_TIME_BASE) / (formatCtx->duration);
            if (verbose) { log->Debug("Framerate estimation: stream frames / container duration."); }
            return;
        }
    }

    // Stream "real base framerate", least common multiple of all framerates in the stream.
    if (stream->r_frame_rate.num != 0 && stream->r_frame_rate.den != 0)
    {
        mVideoInfo.FramesPerSeconds = av_q2d(stream->r_frame_rate);
        if (verbose) { log->Debug("Framerate estimation: stream r_frame_rate."); }
        return;
    }

    // Detection failed. Force to 25fps.
    mVideoInfo.FramesPerSeconds = 25;
    if (verbose) { log->DebugFormat("Framerate estimation: fallback to {0}", mVideoInfo.FramesPerSeconds); }
}


void VideoReaderFFMpeg::RestartPrebuffering(int64_t startTimestamp)
{
    // (runs in UI thread).
    
    // In various places we have to turn off prebuffering and switch to on-demand.
    // After this the player should re-enable prebuffering by calling this method.
    if (mCachingMode == VideoDecodingMode::PreBuffering || 
        mCachingMode == VideoDecodingMode::Caching)
    {
        return;
    }

    if (CanPreBuffer)
    {
        ChangeCachingMode(VideoDecodingMode::PreBuffering);

        StartPreBufferingThread(startTimestamp);
        mPreBuffer->AcquireClosest(startTimestamp);
    }
}
#pragma endregion

#pragma region Frame requests / Player state updates


bool VideoReaderFFMpeg::PlayerRequest(PlayerState^ newState)
{
    //--------------------------------------------------------------
    // There is no queue or "pending" requests here, the caller is responsible
    // for scheduling and not submitting obsolete requests.
    // Any request received here is considered high priority and will interrupt any ongoing work.
    //
    // A decoding job lifecycle has two big phases:
    // 
    // 1. request fulfillement phase.
    //    The decoder is possibly relocated and the requested frame is acquired.
    // 
    // 2. sequential decoding phase.
    //    The decoder continuously decode frames until EOF/cancel/new job.
    // 
    // The second phase is only for the prebuffering mode.
    // 
    // If the decoder thread is not currently blocked in Add(), it will eventually see 
    // this new request and abandon its current job.
    // It will then wait until the new job is marked as ready to be processed.
    //--------------------------------------------------------------

    // Mark the request as the new official one.
    // This starts the lifecycle of this request on the reader side.
    Volatile::Write(mRequestedPlayerState, newState);
    log->DebugFormat("Received and comitted player request {0}", mRequestedPlayerState);

    bool acquired = false;

    if (mCachingMode == VideoDecodingMode::NotInitialized)
    {
        return false;
    }

    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        // Fulfill synchronously and return.
        int64_t target = mRequestedPlayerState->ReferenceTimestamp;
        mWorkingPlayerState = mRequestedPlayerState;

        if (mRequestedPlayerState->Action == PlayerAction::StepForward)
        {
            acquired = MoveRequestOnDemand(true, -1);
        }
        else if (mRequestedPlayerState->Action == PlayerAction::StepBackward &&
                 mSingleFrameContainer->CurrentFrame != nullptr && 
                 mSingleFrameContainer->CurrentFrame->PreviousTimestamp >= 0)
        {
            target = mSingleFrameContainer->CurrentFrame->PreviousTimestamp;
            acquired = MoveRequestOnDemand(false, target);
        }
        else
        {
            acquired = MoveRequestOnDemand(false, target);
        }
        
        mRequestFulfilled = true;
        return acquired;
    }

    if (mCachingMode == VideoDecodingMode::Caching)
    {
        // Fulfill synchronously and return.
        int64_t target = mRequestedPlayerState->ReferenceTimestamp;
        mWorkingPlayerState = mRequestedPlayerState;

        if (mRequestedPlayerState->Action == PlayerAction::StepForward)
        {
            acquired = MoveRequestCaching(true, -1);
        }
        else if (mRequestedPlayerState->Action == PlayerAction::StepBackward &&
                 mCache->CurrentFrame != nullptr &&
                 mCache->CurrentFrame->PreviousTimestamp >= 0)
        {
            target = mCache->CurrentFrame->PreviousTimestamp;
            acquired = MoveRequestCaching(false, target);
        }
        else
        {
            acquired = MoveRequestCaching(false, target);
        }

        mRequestFulfilled = true;
        return acquired;
    }

    //-------------------
    // Prebuffering mode.
    //-------------------

    // Whether the caller requires synchronous fulfillment or not, we always close 
    // the cache for business and try to acquire the target frame.

    mPreBuffer->InterruptAdd();

    TryAcquireResult^ result = mPreBuffer->TryAcquire(mRequestedPlayerState);
    Volatile::Write(mTryAcquireResult, result);
    
    acquired = result->TargetAcquired;
    bool fulfilled = false;

    // For async fulfillment our job is done.
    // We leave it to the decoder thread to work on the request in its own time.
    // When the job is marked ready it will come up with its own plan to relocate the decoder,
    // purge the cache and acquire the target frame.
    // Note: if `acquired` is false at this point, then we MUST raise either OnFrameAcquired() or 
    // OnRequestFailed() at the end of InitDecodingJob to finish the fulfillment part 
    // of the request lifecycle and signal the player that it can send new requests.

    if (mRequestedPlayerState->SynchronousFulfill)
    {
        // The relocation of the decoder must happen here before returning.
        log->DebugFormat("Cache contiguity: {0}.", mPreBuffer->IsContiguous());
        
        if (acquired)
        {
            // If already aquired we don't need to stop/restart the buffering thread,
            // but we will still need to handle a possible pending frame.
            lock l(mLockNewJobReady);
            {
                log->DebugFormat("Marking job #{0} as ready.", mRequestedPlayerState->Id);
                mReadyJobId = mRequestedPlayerState->Id;
                mRequestFulfilled = true;

                // Wake up the decoder thread.
                Monitor::PulseAll(mLockNewJobReady);
                l.release();
            }
        }
        else
        {
            // Relocate synchronously.
            // Note that we don't try to optimize for the "near ahead" case,
            // we always stop the thread and do a seek.
            // 
            // An example of this request subtype is when we are about to start playback:
            // we need to be sure the decoder is relocated before the actual playback starts.
            
            StopPreBufferingThread();
            mPreBuffer->Shutdown();
            DisposePending();

            // Make sure the relocation, which is performed on the UI thread before the decoding thread 
            // restarts, already knows about the new request.
            // When the decoding thread itself starts it initializes itself with job id -1 
            // so it will immediately find that "HasJobChanged".
            // We don't need to lock here since the decoding thread is dead.
            {
                mReadyJobId = mRequestedPlayerState->Id;
                mRequestFulfilled = true;
            }

            // Handling of next/prev request subtype.
            // We may not have the neighbor frame so we don't know the exact timestamps.
            // There are no use-cases for this right now.
            // Playback + tracking and enumeration during export both use MoveNext().
            // For now we estimate the target to be one frame away for simplicity.
            // ReadFrameSeek decodes until it is past that and then calls AcquireClosest().
            int64_t target = mRequestedPlayerState->ReferenceTimestamp;
            if (mRequestedPlayerState->Action == PlayerAction::StepForward)
            {
                target = target + mVideoInfo.AverageTimeStampsPerFrame;
            }
            else if (mRequestedPlayerState->Action == PlayerAction::StepBackward)
            {
                target = target - mVideoInfo.AverageTimeStampsPerFrame;
            }

            // Move to the target location and restart the thread, which will immediately
            // found that the new job is ready.
            StartPreBufferingThread(mRequestedPlayerState->ReferenceTimestamp);
            mPreBuffer->AcquireClosest(mRequestedPlayerState->ReferenceTimestamp);
        
            acquired = true;
        }
    }
    else
    {
        lock l(mLockNewJobReady);
        {
            // By now the decoding thread should be waiting in WaitForReadyJob().
            log->DebugFormat("Marking job #{0} as ready.", mRequestedPlayerState->Id);
            mReadyJobId = mRequestedPlayerState->Id;
            mRequestFulfilled = fulfilled;

            // Wake up the decoder thread.
            Monitor::PulseAll(mLockNewJobReady);
            l.release();
        }
    }
    
    return acquired;
}

bool VideoReaderFFMpeg::MoveRequest(bool next, int64_t target)
{
    //-----------------------------------------------------
    // This runs in the UI thread or in a video exporter background thread.
    //-----------------------------------------------------

    // This happens in two contexts:
    // 1. playback: the player is asking for a frame to present, now.
    //   In this case we may be in prebuffering mode, the request should be
    //   fulfilled immediately with whatever is the closest frame.
    // 2. Frame enumeration during export.
    //  In this case we are in on-demand or caching, the request should 
    //  be fulfilled exactly and synchronously.

    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return false;

    bool acquired = false;
    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        return MoveRequestOnDemand(next, target);
    }
    else if (mCachingMode == VideoDecodingMode::Caching)
    {
        return MoveRequestCaching(next, target);
    }
    else if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        return MoveRequestPrebuffer(next, target);
    }
}

bool VideoReaderFFMpeg::MoveRequestOnDemand(bool next, int64_t target)
{
    if (!mSingleFrameContainer->IsEmpty &&
        mSingleFrameContainer->CurrentFrame != nullptr && 
        mSingleFrameContainer->CurrentFrame->Timestamp == target)
    {
        return true;
    }
    else if (next)
    {
        // Synchronous read of the next frame.
        ReadResult res = ReadFrameNext();
        return (res == ReadResult::Success);
    }
    else
    {
        // Synchronous read of the requested frame.
        // ReadFrameSeek will call `store` on the single-frame frame container
        // which will set the `Current` property to the requested frame.
        ReadResult res = ReadFrameSeek(target, true, false, false);
        return (res == ReadResult::Success);
    }
}

bool VideoReaderFFMpeg::MoveRequestCaching(bool next, int64_t target)
{
    if (mCache->Empty)
    {
        // In theory the UI shouldn't ask for a frame until the modal progress bar 
        // of the caching operation is closed so we should always have a frame.
        return false;
    }
    else if (next)
    {
        return mCache->AcquireNext();
    }
    else
    {
        mCache->AcquireClosest(target);
        return true;
    }
}

bool VideoReaderFFMpeg::MoveRequestPrebuffer(bool next, int64_t target)
{
    if (mPreBuffer->Empty)
    {
        return false;
    }
    else if (next)
    {
        return mPreBuffer->AcquireNext();
    }
    else
    {
        mPreBuffer->AcquireClosest(target);
        return true;
    }
}

#pragma endregion

#pragma region Decoding mode, play loop and frame enumeration

void VideoReaderFFMpeg::WorkingZoneUpdateRequest(WorkingZoneRequest^ request, Action<DoWorkEventHandler^>^ workerFn)
{
    if (!CanChangeWorkingZone)
    {
        throw gcnew CapabilityNotSupportedException();
    }

    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
    {
        return;
    }

    if (request->WorkingZone.IsEmpty)
    {
        return;
    }

    log->DebugFormat("Update working zone request. {0} -> {1}. Cache load mode: {2}, First time: {3}", 
        mWorkingZone, request->WorkingZone, request->CacheLoadMode, mIsFirstWZUpdate);
    
    // Important: the new zone is coming from pixel values and there is no guarantee that
    // actual frames exists in the video at these values.
    // We must update our internal values according to real timestamps as soon as possible.
    VideoSection oldZone = mWorkingZone;
    double requiresMB = WorkingZoneMemoryRequirement(request->WorkingZone);
    bool fitsInMemory = requiresMB <= request->MaxMemoryMB;

    log->DebugFormat("New working zone: {0:0.000} GB. Allowed: {1:0.000} GB.", 
        requiresMB/1024.0, request->MaxMemoryMB/1024.0);

    bool allowPrebuffering = !mIsFirstWZUpdate;
    mIsFirstWZUpdate = false;

    if (fitsInMemory && request->CacheLoadMode != CacheLoadMode::DoNotLoad)
    {
        if (mCachingMode != VideoDecodingMode::Caching)
        {
            // Change mode (including clearing of the existing container) and load the cache.
            // Force reload is irrelevant here, we always need to reload.
            ChangeCachingMode(VideoDecodingMode::Caching);
            mSectionToPrepend = request->WorkingZone;
            mSectionToAppend = VideoSection::MakeEmpty();
            DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
            workerFn(workHandler);
        }
        else if (request->CacheLoadMode == CacheLoadMode::Reload)
        {
            // Force reload everything.
            mCache->Clear();
            mSectionToPrepend = request->WorkingZone;
            mSectionToAppend = VideoSection::MakeEmpty();
            DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
            workerFn(workHandler);
        }
        else
        {
            // Adjust existing cache to the new working zone.
            mSectionToPrepend = VideoSection::MakeEmpty();
            mSectionToAppend = VideoSection::MakeEmpty();
            
            VideoSection reqZone = request->WorkingZone;

            // Trim left.
            if (reqZone.Start > mWorkingZone.Start)
            {
                // Only do it if the new start is at least one frame beyond the old one.
                if (reqZone.Start - mWorkingZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    mCache->Trim(reqZone.Start, mWorkingZone.End);
                    mWorkingZone = mCache->WorkingZone;
                    log->DebugFormat("Trimmed working zone cache from the left: {0}.", mWorkingZone);
                }

                // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                reqZone = VideoSection(mWorkingZone.Start, reqZone.End);
            }

            // Trim right.
            if (reqZone.End < mWorkingZone.End)
            {
                // Only do it if the new end is at least one frame before the old one.
                if (mWorkingZone.End - reqZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    mCache->Trim(mWorkingZone.Start, reqZone.End);
                    mWorkingZone = mCache->WorkingZone;
                    log->DebugFormat("Trimmed working zone cache from the right: {0}.", mWorkingZone);
                }

                // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                reqZone = VideoSection(reqZone.Start, mWorkingZone.End);
            }

            // Bail out if this was purely a trimming job.
            if (mWorkingZone == reqZone)
            {
                return;
            }

            // Expand left.
            if (mWorkingZone.Start - reqZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
            {
                mSectionToPrepend = VideoSection(reqZone.Start, mWorkingZone.Start);
            }

            // Expand right.
            if (reqZone.End - mWorkingZone.End > mVideoInfo.AverageTimeStampsPerFrame)
            {
                mSectionToAppend = VideoSection(mWorkingZone.End, reqZone.End);
            }

            if (!mSectionToPrepend.IsEmpty || !mSectionToAppend.IsEmpty)
            {
                DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
                workerFn(workHandler);
            }
        }
    }
    else if (allowPrebuffering && CanPreBuffer)
    {
        if (mCachingMode == VideoDecodingMode::PreBuffering)
        {
            // Technically if the new zone is only trimmed/expanded by the right side
            // we don't really need to move the decoder to the start.
            // But then we have to handle corner cases like the current frame being 
            // outside the new zone, the decoder waiting in add(), or 
            // the decoder waiting for a new job after reaching EOF.
            // It's simpler to just restart from scratch.
            StopPreBufferingThread();
            mPreBuffer->Shutdown();
            DisposePending();
        }
        else
        {
            ChangeCachingMode(VideoDecodingMode::PreBuffering);
        }

        // At this point we are in prebuffering mode.
        // Relocate to the first frame and start the sequential decoding thread.
        int64_t target = request->WorkingZone.Start;
        StartPreBufferingThread(target);

        // Acquire the target and get the resolved value.
        // The end frame stays in "request" space.
        mPreBuffer->AcquireClosest(target);
        int64_t resolvedTarget = mPreBuffer->CurrentFrame->Timestamp;
        mWorkingZone = VideoSection(resolvedTarget, request->WorkingZone.End);
    }
    else
    {
        if (mCachingMode != VideoDecodingMode::OnDemand)
        {
            ChangeCachingMode(VideoDecodingMode::OnDemand);
        }

        mWorkingZone = request->WorkingZone;

        // TODO: move to the first frame if needed.
    }
}

void VideoReaderFFMpeg::BeforeFrameEnumeration()
{
    // Frames are about to be enumerated (for example for saving).
    // This operation is not compatible with Prebuffering mode.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        mWasPrebufferingBeforeEnumeration = true;
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
}

void VideoReaderFFMpeg::AfterFrameEnumeration()
{
    if (mWasPrebufferingBeforeEnumeration)
    {
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
        StartPreBufferingThread(mWorkingZone.Start);
    }

    mWasPrebufferingBeforeEnumeration = false;
}

void VideoReaderFFMpeg::ChangeCachingMode(VideoDecodingMode newMode)
{
    // At this point we must have validated that the target mode is doable,
    // in terms of global capabilities, player state and memory requirements.
    if (!CanSwitchDecodingMode(newMode))
    {
        throw gcnew CapabilityNotSupportedException();
    }

    VideoDecodingMode oldMode = mCachingMode;


    if (newMode == oldMode)
    {
        return;
    }

    log->DebugFormat("Changing caching mode: {0} -> {1}", oldMode, newMode);

    // Clear the existing cache.
    switch (oldMode)
    {
    case VideoDecodingMode::OnDemand:
        mSingleFrameContainer->Clear();
        break;
    case VideoDecodingMode::PreBuffering:
        StopPreBufferingThread();
        mPreBuffer->Shutdown();
        DisposePending();
        break;
    case VideoDecodingMode::Caching:
        mCache->Clear();
        break;
    }

    // Change container.
    switch (newMode)
    {
    case VideoDecodingMode::OnDemand:
        mFrameContainer = mSingleFrameContainer;
        break;
    case VideoDecodingMode::PreBuffering:
        mFrameContainer = mPreBuffer;
        break;
    case VideoDecodingMode::Caching:
        mFrameContainer = mCache;
        break;
    default:
        mFrameContainer = nullptr;
    }

    // We have officially switched.
    mCachingMode = newMode;

    // Recompute the geometry, to take into account a possible
    // change in allowPrescaling.
    ResolveGeometry(mVideoGeometryRequest);

    // For prebuffering, the thread is not started yet.
    // The caller is responsible for starting it on a specific target.
}

double VideoReaderFFMpeg::WorkingZoneMemoryRequirement(VideoSection _newZone)
{
    double durationSeconds = (double)(_newZone.End - _newZone.Start) / mVideoInfo.AverageTimeStampsPerSeconds;

    // Caching is done at full aspect ratio size, not at the current decoding size based on the rendering viewport.
    // Otherwise we would have to potentially reload the cache each time there is a stretch/squeeze request.
    int bufferSize = av_image_get_buffer_size(sConvertPixelFormat, mVideoGeometry->ReferenceSize.Width, mVideoGeometry->ReferenceSize.Height, 1);
    double frameMegaBytes = (double)bufferSize / 1048576;
    double durationMegaBytes = durationSeconds * mVideoInfo.FramesPerSeconds * frameMegaBytes;

    return durationMegaBytes;
}
#pragma endregion

#pragma region Video geometry

bool VideoReaderFFMpeg::UpdateVideoGeometry(VideoGeometryRequest^ request)
{
    Size oldOutputSize = mVideoGeometry == nullptr ? Size::Empty : mVideoGeometry->OutputSize;

    // Update published geometry based on the request.
    ResolveGeometry(request);

    // Check if we must invalidate the cache.
    bool invalidated = true;
    VideoGeometryRequest^ oldRequest = mVideoGeometryRequest;
    if (oldRequest != nullptr)
    {
        bool changedOptions =
            request->AspectRatio != oldRequest->AspectRatio ||
            request->Rotation != oldRequest->Rotation ||
            request->Demosaicing != oldRequest->Demosaicing ||
            request->Deinterlace != oldRequest->Deinterlace;
    
        // TODO: check if stabilization data has changed via a hash.

        invalidated = changedOptions || mVideoGeometry->OutputSize != oldOutputSize;
    }

    if (invalidated && oldRequest != nullptr)
    {
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }

    // The caching or prebuffering thread will be restarted by the caller
    // via an UpdateWorkingZone() call or StartPrebufferingIfNotCaching.

    mVideoGeometryRequest = request;

    return invalidated;
}

void VideoReaderFFMpeg::ResolveGeometry(VideoGeometryRequest^ request)
{
    if (request == nullptr)
    {
        return;
    }

    bool readerAllowsPrescaling = mCachingMode == VideoDecodingMode::PreBuffering;
    bool bothAllowPrescaling = readerAllowsPrescaling && request->AllowPreScaling;

    // Compute mReference size and an initial mScaledSize (aspect ratio but not rotated).
    ComputeReferenceSize(request->AspectRatio, request->Rotation);
    mOutputSize = mReferenceSize;
    
    if (bothAllowPrescaling)
    {
        // Allow upscaling.
        mOutputSize = FitHelper::Fit(mReferenceSize, request->PresentationSize, true);
    }

    mScaledSize = mOutputSize;
    if (request->Rotation == ImageRotation::Rotate90 || request->Rotation == ImageRotation::Rotate270)
    {
        mScaledSize = Size(mOutputSize.Height, mOutputSize.Width);
    }

    float decodingScale = mOutputSize.Width / (float)mReferenceSize.Width;
    
    SetStabilizationData(request->StabilizationData);

    // Prescaled means we output at the presentation size and the player 
    // can draw directly without rescaling.
    // Whether we decode at the full size or not is orthogonal.
    bool isPrescaled = 
        Math::Abs(mOutputSize.Width - request->PresentationSize.Width) <= 4 && 
        Math::Abs(mOutputSize.Height - request->PresentationSize.Height) <= 4;

    mVideoGeometry = gcnew VideoGeometry(
        mReferenceSize,
        mOutputSize,
        isPrescaled,
        decodingScale,
        request->AspectRatio,
        request->Rotation,
        request->Demosaicing,
        request->Deinterlace,
        true,
        0);

    log->DebugFormat("Video geometry resolved: Original: {0}x{1}, Scaled:{2}x{3}, Presentation:{4}x{5}, Prescaling: {6}.",
        mOriginalSize.Width, mOriginalSize.Height,
        mScaledSize.Width, mScaledSize.Height,
        request->PresentationSize.Width, request->PresentationSize.Height,
        isPrescaled);

    LogVideoGeometry(mVideoGeometry);
}

void VideoReaderFFMpeg::ComputeReferenceSize(ImageAspectRatio aspectRatio, ImageRotation rotation)
{
    Size aspectRatioSize = Size::Empty;
    aspectRatioSize.Width = mOriginalSize.Width;

    switch (aspectRatio)
    {
    case ImageAspectRatio::Force43:
        aspectRatioSize.Height = (int)((mOriginalSize.Width * 3.0) / 4.0);
        break;
    case ImageAspectRatio::Force169:
        aspectRatioSize.Height = (int)((mOriginalSize.Width * 9.0) / 16.0);
        break;
    case ImageAspectRatio::ForcedSquarePixels:
        aspectRatioSize.Height = mOriginalSize.Height;
        break;
    case ImageAspectRatio::Auto:
    default:
        aspectRatioSize.Height = (int)((double)mOriginalSize.Height / mVideoInfo.PixelAspectRatio);
        break;
    }

    bool isSideway = rotation == ImageRotation::Rotate90 || rotation == ImageRotation::Rotate270;

    aspectRatioSize = PadSize(aspectRatioSize, isSideway);

    // Init the scaled size to the full aspect ratio size, it will be changed later if allowed.
    mScaledSize = aspectRatioSize;
    mReferenceSize = aspectRatioSize;

    if (isSideway)
    {
        mReferenceSize = Size(mReferenceSize.Height, mReferenceSize.Width);
    }
}

Size VideoReaderFFMpeg::PadSize(Size _size, bool isSideway)
{
    // Fix unsupported width for conversion to .NET Bitmap. Must be a multiple of 4.
    // Subtlety: the padding must be in the dimension that will be the width after rotation.
    if (isSideway)
        return Size(_size.Width, _size.Height + (_size.Height % 4));
    else
        return Size(_size.Width + (_size.Width % 4), _size.Height);
}

bool VideoReaderFFMpeg::SetStabilizationData(List<Kinovea::Services::TimedPoint^>^ points)
{
    // Precompute the list of frame offsets with regards to the first point of the track.
    mStabOffsets->Clear();
    //mFrameContainer->Clear();

    if (points == nullptr)
        return true;

    for (int i = 0; i < points->Count; i++)
    {
        if (mStabOffsets->ContainsKey((long)points[i]->T))
            continue;

        TimedPoint^ p = gcnew TimedPoint(points[i]->X - points[0]->X, points[i]->Y - points[0]->Y, points[i]->T);
        mStabOffsets->Add((long)points[i]->T, p);
    }

    return true;
}

#pragma endregion

#pragma region Full caching mode

void VideoReaderFFMpeg::ImportWorkingZoneToCache(System::Object^ sender, DoWorkEventArgs^ e)
{
    BackgroundWorker^ worker = dynamic_cast<BackgroundWorker^>(sender);

    Thread::CurrentThread->Name = "CacheFilling";

    bool success = true;
    if (!mSectionToPrepend.IsEmpty)
    {
        success = ReadManyToCache(worker, mSectionToPrepend);
    }

    if (success && !mSectionToAppend.IsEmpty)
    {
        success = ReadManyToCache(worker, mSectionToAppend);
    }

    if (success)
    {
        // Make sure the official working zone reflects the resolved timestamps.
        mWorkingZone = mCache->WorkingZone;
    }
    else
    {
        // Switch back to on-demand.
        // The UI is responsible for switching to prebuffering.
        // If this is running during the initial load we may not have set 
        // a reliable preferred size yet, but the UI side will be able to do 
        // it during cancellation handling.
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
}

bool VideoReaderFFMpeg::ReadManyToCache(BackgroundWorker^ bgWorker, VideoSection section)
{
    //--------------------------
    // Runs on the cache thread.
    //--------------------------

    // Load the asked section to cache (doesn't move the playhead).
    // Called when filling the cache with the Working Zone.
    // This method is always called on a background thread.
    // Note: the passed section only represents what we need to prepend or append, not the full target section.

    if (bgWorker == nullptr)
    {
        throw gcnew InvalidProgramException("ReadManyToCache must be called on a background thread.");
    }
        
    if (!CanCache || mCachingMode != VideoDecodingMode::Caching)
    {
        throw gcnew CapabilityNotSupportedException("Importing to cache is not supported for the video.");
    }
    
    log->DebugFormat("Requested section to cache: [~{0} --> ~{1}].", section.Start, section.End);

    // Realign the requested section on real timestamps.
    if (!mCache->WorkingZone.IsEmpty)
    {
        bool realigned = false;
        double tolerance = 0.5 * mVideoInfo.AverageTimeStampsPerFrame;
        if (Math::Abs(section.Start - mCache->WorkingZone.Start) <= tolerance)
        {
            section = VideoSection(mCache->WorkingZone.Start, section.End);
            realigned = true;
        }

        if (Math::Abs(section.End - mCache->WorkingZone.End) <= tolerance)
        {
            section = VideoSection(section.Start, mCache->WorkingZone.End);
            realigned = true;
        }

        if (realigned)
        {
            log->DebugFormat("Realigned requested section to cache: [~{0} --> ~{1}]", section.Start, section.End);
        }
    }

    // Bail out if re-alignment revealed we don't need to cache anything new.
    if (section.End - section.Start < mVideoInfo.AverageTimeStampsPerFrame)
    {
        return true;
    }

    // The number of frames to cache is an estimate and is only used to report progress.
    // The actual reading only stops when we get the target end timestamp or EOF.
    double frameIntervals = (section.End - section.Start) / mVideoInfo.AverageTimeStampsPerFrame;
    int totalFrames = (int)Math::Round(frameIntervals + 1);
    log->DebugFormat("Frames to cache: ~{0} (avgtspf: {1:0.000}).", totalFrames, mVideoInfo.AverageTimeStampsPerFrame);

    Stopwatch^ stopwatchCaching = Stopwatch::StartNew();

    // Seek to first frame.
    int read = 0;
    bool success = true;
    ReadResult res = ReadFrameSeek(section.Start, true, false, false);
    success = (res == ReadResult::Success);
    read++;

    mLoopWatcher->Restart();
    
    while (true)
    {
        if (mCachedTimestamp >= section.End)
        {
            log->DebugFormat("Caching complete [{0}]. Read: {1} frames in {2} ms.", 
                mCachedTimestamp, read, stopwatchCaching->ElapsedMilliseconds);
            success = true;
            break;
        }

        if (res == ReadResult::EOFReached)
        {
            // Unexpected but not fatal.
            log->WarnFormat("EOF while caching [{0}]. Read: {1} frames in {2} ms.", 
                mCachedTimestamp, read, stopwatchCaching->ElapsedMilliseconds);
            success = true;
            break;
        }

        if (res != ReadResult::Success)
        {
            log->ErrorFormat("Error while caching [{0}]. Read: {1} frames.", mCachedTimestamp, read);
            success = false;
            break;
        }

        if (bgWorker->CancellationPending)
        {
            log->WarnFormat("Cancellation while caching [{0}]. Read: {1} frames.", mCachedTimestamp, read);
            mCache->Clear();
            success = false;
            break;
        }

        res = ReadFrameNext();
        read++;

        //mLoopWatcher->LoopEnd();

        bgWorker->ReportProgress(read, totalFrames);
    }

    log->DebugFormat("Cache filling, average per frame: {0:0.000} ms.", mLoopWatcher->Average);

    // Update the working zone with real values.
    // The request may have been an approximation from pixel mapping.
    if (!bgWorker->CancellationPending)
    {
        mWorkingZone = mCache->WorkingZone;
    }

    return success;
}

#pragma endregion

#pragma region Frame level reading/decoding/scaling/converting/storing


ReadResult VideoReaderFFMpeg::ReadFrameThumbnail(int64_t targetTimestamp)
{
    if (!mIsLoaded || 
        mCachingMode != VideoDecodingMode::OnDemand || 
        mFrameContainer == nullptr ||
        mVideoGeometry == nullptr)
    {
        return ReadResult::NotReady;
    }

    // Check where the seek is going to land.
    // If it's in the same GOP as the previous thumbnail we can skip it.
    // This only works on files with a frame index, keep the duplicate if there is no index.
    const AVIndexEntry* entry = avformat_index_get_entry_from_timestamp(mFormatCtx->streams[mVideoStreamIndex], targetTimestamp, AVSEEK_FLAG_BACKWARD);
    if (entry != nullptr && entry->timestamp != AV_NOPTS_VALUE)
    {
        if (entry->timestamp == mSummaryPreviousSeek)
        {
            log->DebugFormat("Skipping thumbnail request for [~{0}], same seek result: [{1}].", targetTimestamp, mSummaryPreviousSeek);
            return ReadResult::Same;
        }

        mSummaryPreviousSeek = entry->timestamp;
    }

    // Always seek even if the target is 0.
    // This improves perfs as some decoders have buffering causing slow down when just asking 
    // for frames until we get the first one, compared to forcing a seek to the nearest keyframe.

    int res = SeekTo(targetTimestamp);
    if (res < 0)
    {
        LogFFMpegError("SeekTo", res);
        log->ErrorFormat("Error trying to seek to: [{1}]", targetTimestamp);
    }

    // Get the first frame after the seek.
    AVFrame* frame = av_frame_alloc();
    ReadResult result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
    if (result != ReadResult::Success)
    {
        av_frame_free(&frame);
        return result;
    }

    mPreviousDecodedTimestamp = mDecodedTimestamp;
    mDecodedTimestamp = frame->best_effort_timestamp;

    result = ConvertAndStoreFrame(frame, true, false);
    av_frame_free(&frame);
    return result;
}


ReadResult VideoReaderFFMpeg::ReadFrameNext()
{
    if (!mIsLoaded ||
        mCachingMode == VideoDecodingMode::NotInitialized ||
        mFrameContainer == nullptr ||
        mVideoGeometry == nullptr ||
        mVideoGeometry->OutputSize.IsEmpty)
    {
        return ReadResult::NotReady;
    }

    ReadResult result = ReadResult::UnknownError;

    // Get the next frame available in the decoder.
    mLoopWatcher->LoopStart();
    //mStopwatch->Restart();
    AVFrame* frame = av_frame_alloc();
    result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
    mPreviousDecodedTimestamp = mDecodedTimestamp;
    mDecodedTimestamp = frame->best_effort_timestamp;
    mLoopWatcher->LoopEnd();

    // During dense jobs do not allow new jobs to interrupt the work
    // between decoding and storing.
    //if (HasJobChanged())
    //{
    //    // FIXME: we should keep the decoded frame as pending here.
    //    // The next job might be able to just restart from there.
    //    log->DebugFormat("ReadFrameNext. Job changed during decoding. Abandoning.");
    //    av_frame_free(&frame);
    //    return ReadResult::NewJob;
    //}

    if (result != ReadResult::Success)
    {
        av_frame_free(&frame);
        return result;
    }

    //log->DebugFormat("Decoded next frame [{0}]. {1} ms.", mDecodedTimestamp, mStopwatch->ElapsedMilliseconds);

    if (!ShouldStoreFrame())
    {
        av_frame_free(&frame);
        return ReadResult::Success;
    }
    
    result = ConvertAndStoreFrame(frame, false, false);
    av_frame_free(&frame);
    return result;
}


ReadResult VideoReaderFFMpeg::ReadFrameSeek(int64_t targetTimestamp, bool doSeek, bool preRoll, bool allowInterrupt)
{
    //-----------------------------------
    // This runs in either the main thread or the prebuffering thread.
    //-----------------------------------
    if (!mIsLoaded || 
        mCachingMode == VideoDecodingMode::NotInitialized || 
        mFrameContainer == nullptr ||
        mVideoGeometry == nullptr ||
        mVideoGeometry->OutputSize.IsEmpty)
    {
        return ReadResult::NotReady;
    }

    ReadResult result = ReadResult::UnknownError;

    
    int res = 0;

    // It's possible to get here with a target equal to that of the current decoder location.
    // For example when we change the output size (? this should invalidate everything).
    // We have to seek back to the start of the GOP and decode many frames again.

    Stopwatch^ stopwatchRelocate = Stopwatch::StartNew();

    if (doSeek)
    {
        // Initial seek.
        // This should land us at the start of the GOP containing the target.
        // Note that even if the seek target is in the current GOP we go through the 
        // seeking call and reset the libav internal buffers, because we can't know it beforehand.
        res = SeekTo(targetTimestamp);
        if (res < 0)
        {
            LogFFMpegError("SeekTo", res);
            log->ErrorFormat("Error trying to seek to: [~{1}]", targetTimestamp);
            return ReadResult::UnknownError;
        }
    }

    // Decode frames until we get to the target or EOF.
    AVFrame* frame = av_frame_alloc();
    bool inPreRollWindow = false;
    int framesDecoded = 0;
    while (true)
    {
        result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
            
        if (result != ReadResult::Success)
        {
            av_frame_free(&frame);
            return result;
        }
        else
        {
            framesDecoded++;
            mPreviousDecodedTimestamp = mDecodedTimestamp;
            mDecodedTimestamp = frame->best_effort_timestamp;
        }

        if (mPreBufferingThreadCanceler->CancellationPending)
        {
            av_frame_free(&frame);
            return ReadResult::ThreadCancelled;
        }

        if (allowInterrupt && HasJobChanged())
        {
            av_frame_free(&frame);
            return ReadResult::NewJob;
        }

        if (framesDecoded % 10 == 0)
        {
            log->DebugFormat("Advancing towards [~{0}]. Last decoded: [{1}]. Decoded {2} frames.", 
                targetTimestamp, mDecodedTimestamp, framesDecoded);
        }

        
        if (preRoll)
        {
            // Check if we are arriving near the target.
            if (!inPreRollWindow && targetTimestamp - mDecodedTimestamp <= mPreRollTimestamps)
            {
                inPreRollWindow = true;
            }

            if (inPreRollWindow)
            {
                //-------------------------------------------------------
                // Store the preroll frame.
                // This serves two purposes:
                // 
                // 1. When we are done relocating the user can immediately step backwards
                // without taking another seek hit.
                // 
                // 2. We get bracketing of the target for free. 
                // When the caller of this function calls cache.AcquireClosest(target) it will 
                // find the appropriate frame, which may be the penultimate one rather than the ultimate one.
                //-------------------------------------------------------
                result = ConvertAndStoreFrame(frame, false, true);
                log->DebugFormat("Stored preroll frame [{0}]. Decoded {1} frames.", mDecodedTimestamp, framesDecoded);
                
                if (mDecodedTimestamp >= targetTimestamp)
                {
                    av_frame_free(&frame);
                    break;
                }
            }
        }
        else
        {
            if (mDecodedTimestamp >= targetTimestamp)
            {
                result = ConvertAndStoreFrame(frame, false, false);
                av_frame_free(&frame);
                break;
            }
        }

        // Keep decoding.
    }

    int64_t elapsed = stopwatchRelocate->ElapsedMilliseconds;
    log->DebugFormat("Reached seek target. [~{0}] -> [{1}]. Decoded {2} frames in {3} ms (avg: {4:0.000} ms).",
        targetTimestamp, mDecodedTimestamp, framesDecoded, elapsed, (float)elapsed / framesDecoded);

    return result;
}

bool VideoReaderFFMpeg::ShouldStoreFrame()
{
    if (mCachingMode != VideoDecodingMode::PreBuffering)
        return true;

    if (mWorkingPlayerState->Action != PlayerAction::Playback)
        return true;

    if (!mAllowFrameSkipping)
        return true;

    if (mFrameSkippingPolicy != FrameSkippingPolicy::Behind && 
        mFrameSkippingPolicy != FrameSkippingPolicy::FarBehind)
        return true;

    // If we are behind the player we skip the scale/convert/store step as much as possible.
    // We keep presenting progress frames periodically.
    // Compute the next publish timestamp based on how many frames fit within the refresh interval.
    // 
    // Example:
    // Say we play a 120 fps video at 2.5x, on a 60Hz monitor.
    // The refresh rate will be capped at 60 fps, 16.67 ms per frame.
    // The playback frame rate will be 120*2.5 = 300 fps, 3.33 ms per frame.
    //
    // For every presented frame we have to decode 5 frames.
    // We compute the expected timestamp of the 5th frame and discard the first 4.
    // 
    // Note: the decoder thread never wraps around the end by itself within the same job,
    // so there shouldn't be any time where the decoded timestamp is less than the last published timestamp.

    int64_t lastPublished = mCachedTimestamp;
    double frames = mWorkingPlayerState->RefreshInterval / mWorkingPlayerState->PlaybackFrameInterval;
    double timestampsPerPublish = frames * mVideoInfo.AverageTimeStampsPerFrame;
    int64_t nextPublishTimestamp = (int64_t)Math::Round(lastPublished + timestampsPerPublish);
    bool shouldPublish = mDecodedTimestamp >= nextPublishTimestamp;

    if (!shouldPublish)
    {
        log->DebugFormat("Skipping store of [{0}]. xxxxxxxxxxxxx", mDecodedTimestamp);
    }

    //log->WarnFormat("Policy: {0}. Decoded frame [{1}]. Last published [{2}]. Next presentation [{3}]. Publish: {4}.", 
    //    mDecodingPolicy.ToString(), mDecodedTimestamp, lastPublished, nextPublishTimestamp, shouldPublish);

    return shouldPublish;
}


ReadResult VideoReaderFFMpeg::DecodeOneFrame(AVFormatContext* formatCtx, int streamIndex, AVCodecContext* codecCtx, AVFrame* frame)
{
    //-------------------------------------
    // Decode one frame from the video stream.
    //
    // Read packets from the video stream and feed them to libav until it can decode one frame.
    // The frame may already be decoded if the codec has B-frames and the next I-frame was already decoded.
    // Another way was tried where we feed packets to libav until its internal buffer is full, 
    // but this sometimes returns an error "Invalid data found when processing input.". To investigate.
    //-------------------------------------
    if (frame == nullptr)
    {
        return ReadResult::NotReady;
    }

    AVPacket* packet = av_packet_alloc();
    int decodeResult = 0;
    int demuxResult = 0;
    int feedDecoderResult = 0;
    ReadResult result = ReadResult::UnknownError;

    // This nested loop is somewhat reversed compared to the usual libav decoding example.
    // We exit the function to return a frame as soon as we have one.
    // We still feed the decoder with packets until it can produce that frame, so the 
    // next time we come here it's possible the decoder is ready to decode.
    // The first thing we do is try to get a frame from the decoder.

    while (true)
    {
        // Immediately try to decode a frame from the decoder's internal buffer.
        decodeResult = avcodec_receive_frame(codecCtx, frame);

        if (decodeResult >= 0)
        {
            // Our job is done.
            mDecodedFrames++;
            result = ReadResult::Success;
            break;
        }
        else if (decodeResult == AVERROR(EAGAIN))
        {
            // The decoder needs more packets before it can produce a frame.
            // This is normal for codecs with B-frames or buffering.
            // Note: enabling multithreading on the codec may increase 
            // buffering by one frame per thread.

            // Read packets until we get a video one and feed it to the decoder.
            while (true)
            {
                // Demux a packet.
                av_packet_unref(packet);
                demuxResult = av_read_frame(formatCtx, packet);

                if (demuxResult == AVERROR_EOF)
                {
                    // EOF here means the demuxer has no more packets (on any stream), 
                    // and we have nothing to feed to the decoder.
                    // The decoder may still have frames to output though.
                    // Flush the decoder by sending a null packet.
                    feedDecoderResult = avcodec_send_packet(codecCtx, nullptr);
                    if (feedDecoderResult < 0)
                    {
                        LogFFMpegError("avcodec_send_packet (flush)", feedDecoderResult);
                        result = ReadResult::UnknownError;
                        break;
                    }

                    // If we get here we should now be ready to try to decode a frame again.
                    break;
                }
                else if (demuxResult < 0)
                {
                    // If not EOF this is unrecoverable.
                    // We don't even know if it's on the right stream.
                    LogFFMpegError("av_read_frame", demuxResult);
                    result = ReadResult::UnknownError;
                    break;
                }
                
                // If it's not in the right stream just keep demuxing.
                if (packet->stream_index != streamIndex)
                {
                    continue;
                }

                // If we found a video packet and it's a keyframe, remember the timestamp. 
                // We use this to detect if we can seek ahead to a different GOP
                // in case we fall behind during async decoding.
                if (packet->flags & AV_PKT_FLAG_KEY)
                {
                    if (packet->dts != AV_NOPTS_VALUE)
                        mCurrentGopTimestamp = packet->dts;
                    else
                        mCurrentGopTimestamp = packet->pts;
                }

                // Supply the raw packet to the decoder.
                feedDecoderResult = avcodec_send_packet(codecCtx, packet);
                if (feedDecoderResult == AVERROR(EAGAIN))
                {
                    // The decoder is full and requires a call to avcodec_receive_frame
                    // to consume its internal buffer.
                    // This should never happen here, as we were just told it needed more packets.
                    LogFFMpegError("avcodec_send_packet", feedDecoderResult);
                    result = ReadResult::UnknownError;
                    break;
                }
                else if (feedDecoderResult == AVERROR_EOF)
                {
                    // The decoder has been flushed and will not accept any more packets.
                    LogFFMpegError("avcodec_send_packet", feedDecoderResult);
                    result = ReadResult::EOFReached;
                    break;
                }
                else if (feedDecoderResult < 0)
                {
                    // Decoder error.
                    LogFFMpegError("avcodec_send_packet", feedDecoderResult);
                    result = ReadResult::UnknownError;
                    break;
                }

                // If we get here we have demuxed one or more packets and 
                // fed at least one video packet to the decoder.
                break;
            }

            if (feedDecoderResult < 0)
            {
                // Unrecoverable error while feeding or flushing the decoder.
                // `result` variable should be set already.
                break;
            }
            else if (demuxResult == AVERROR_EOF)
            {
                // The decoder has been flushed.
                // We should now be ready to try to decode a frame again.
                continue;
            }
            else if (demuxResult < 0)
            {
                // Irrecoverable error occurred while demuxing.
                // `result` variable should be set already.
                break;
            }
            
            // Otherwise we are ready to try decoding a frame again.
            continue;
        }
        else if (decodeResult == AVERROR_EOF)
        {
            // The decoder has been fully flushed and will not return any more frames.
            LogFFMpegError("avcodec_receive_frame", decodeResult);
            result = ReadResult::EOFReached;
            break;
        }
        else
        {
            // Decoding error.
            LogFFMpegError("avcodec_receive_frame", decodeResult);
            result = ReadResult::UnknownError;
            break;
        }

        // We can't get here.
    }

    av_packet_unref(packet);
    return result;
}


int VideoReaderFFMpeg::SeekTo(int64_t targetTimestamp)
{
    // Seek to the first I-Frame before the target.
    // Does not decode any frame.
    int64_t minTs = 0;
    int64_t ts = targetTimestamp;
    int64_t maxTs = targetTimestamp;

    // Special case for jumping to the start of the file while non-zero start time.
    // Sometimes the first decode after the seek returns EAGAIN as if we were in the middle 
    // of a GOP. When this happens the first actual frame we get is beyond the seek and this
    // messes up everything.
    // To avoid this we make sure the seek goes to the absolute start of the file.
    if (targetTimestamp == mVideoInfo.FirstTimeStamp && mVideoInfo.FirstTimeStamp > 0)
    {
        minTs = 0;
        ts = 0;
        maxTs = 0;
    }
    
    int res = avformat_seek_file(
        mFormatCtx,
        mVideoStreamIndex,
        minTs,
        ts,
        maxTs,
        AVSEEK_FLAG_BACKWARD);
    
    // Reset the internal codec state. 
    avcodec_flush_buffers(mVideoCodecCtx);
    mCachedTimestamp = AV_NOPTS_VALUE;
    mPreviousDecodedTimestamp = AV_NOPTS_VALUE;
    mDecodedTimestamp = AV_NOPTS_VALUE;
    mCurrentGopTimestamp = AV_NOPTS_VALUE;
    return res;
}


ReadResult VideoReaderFFMpeg::ConvertAndStoreFrame(AVFrame* decodedFrame, bool forSummary, bool force)
{
    //-------------------------------------
    // Convert the decoded frame to the final frame, wrap it in a Bitmap, 
    // keep track of the native buffer through the .Tag of the Bitmap and
    // store the Bitmap in the active frame container.
    //--------------------------------------
    
    //--------------------------------------
    // The frame goes through an ffmpeg filter graph with optional deinterlace,
    // scaling and pixel format conversion.
    // 
    // The graph sink is either retrieved into a reusable staging AVFrame (mFilteredFrame),
    // which is then copied into the final converted AVFrame, or extracted directly
    // into the final converted AVFrame.
    // 
    // In theory the copy shouldn't be necessary. The problem is that this AVFrame has row padding 
    // and this seems to break a number of assumptions in other parts of the code.
    // 
    // We wrap the padded buffer into the Bitmap so anything handling these Bitmaps
    // now needs to be careful about stride being different than width*4.
    // 
    // The Bitmap Copy helpers have been updated to copy row by row but the features involving 
    // OpenCV aren't working correctly, most likely due to OpenCvSharp.Extensions.BitmapConverter.ToMat.
    // This seems to copy the padded buffer into a packed Mat.
    // It doesn't crash but the tracking doesn't work correctly.
    // One way to fix it is to make an extra packed copy before passing to OpenCV.
    // For simplicity we make the extra copy here.
    //-------------------------------------
    
    AVFrame* convertedFrame = av_frame_alloc();
    
    if (mCopyFilteredFrame)
    {
        // Preallocate buffers with packed alignment.
        convertedFrame->format = sConvertPixelFormat;
        convertedFrame->width = mScaledSize.Width;
        convertedFrame->height = mScaledSize.Height;
        int res = av_frame_get_buffer(convertedFrame, 1);
        if (res < 0)
        {
            LogFFMpegError("av_frame_get_buffer", res);
            av_frame_free(&convertedFrame);
            return ReadResult::UnknownError;
        }
    }

    bool converted = false;
    if (!mVideoGeometry->Deinterlacing && mCopyFilteredFrame)
    {
        // Scale and convert the decoded AVFrame to the correct format and size.
        // Uses swscale API.
        converted = RescaleAndConvert(
            decodedFrame,
            convertedFrame, 
            mScaledSize.Width,
            mScaledSize.Height,
            sConvertPixelFormat, 
            forSummary);
    }
    else
    {
        // Deinterlace, scale and convert the decoded AVFrame.
        // Uses filter graph API.
        converted = RescaleAndConvert2(
            decodedFrame, 
            convertedFrame, 
            mScaledSize.Width,
            mScaledSize.Height,
            sConvertPixelFormat, 
            mVideoGeometry->Deinterlacing);
    }

    if (!converted)
    {
        av_frame_free(&convertedFrame);
        return ReadResult::NotConverted;
    }

    // Wrap the data buffer in a Bitmap.
    int stride = convertedFrame->linesize[0];
    IntPtr data = IntPtr(convertedFrame->data[0]);
    Bitmap^ bmp = nullptr;

    if (mStabOffsets->ContainsKey(mDecodedTimestamp))
    {
        // Image stabilization. 

        // Wrap the native AVFrame in a bitmap.
        Bitmap^ bmp2 = gcnew Bitmap(
            mScaledSize.Width,
            mScaledSize.Height,
            stride,
            DecodingPixelFormat,
            data);

        bmp = gcnew Bitmap(mScaledSize.Width, mScaledSize.Height, DecodingPixelFormat);
        Graphics^ g = Graphics::FromImage(bmp);
        float dx = mStabOffsets[mDecodedTimestamp]->X;
        float dy = mStabOffsets[mDecodedTimestamp]->Y;
        
        // Paint the image with the offset applied into a new bitmap.
        // TODO: handle scaling (decoding size).
        g->DrawImageUnscaled(bmp2, (int)(-dx), (int)(-dy));
        delete g;
        
        // In this case we will save the pointer to the native buffer in the new Bitmap .Tag. 
        // We could destroy it right now since the bitmap is an independent copy but 
        // we keep a single flow for simplicity.
        // We put the copy bitmap in the frame container and when that is evicted 
        // the buffer will be released.
        delete bmp2;
    }
    else
    {
        // Normal case, just wrap the native AVFrame in the Bitmap.
        bmp = gcnew Bitmap(
            mScaledSize.Width,
            mScaledSize.Height,
            stride, 
            DecodingPixelFormat, 
            data);
    }

    // Find the AVBufferRef that owns data[0], and create our own reference 
    // that will stay alive after we free the AVFrame.
    // Store that in the .Tag of the bitmap for later release.
    AVBufferRef* planeBuffer = av_frame_get_plane_buffer(convertedFrame, 0);
    AVBufferRef* bitmapBuffer = av_buffer_ref(planeBuffer);
    bmp->Tag = IntPtr(bitmapBuffer);
    av_frame_free(&convertedFrame);

    // Note: rotation doesn't change the size of the buffer.
    ApplyRotation(bmp, mVideoGeometry->ImageRotation);

    // Construct the VideoFrame and store it to the active container.
    // If we are in mode on-demand, this is synchronous and will replace the single stored frame.
    // If we are in mode prebuffer, we are in a background thread and this will potentially block if the 
    // cache is full.
    VideoFrame^ vf = gcnew VideoFrame(bmp, mDecodedTimestamp, mPreviousDecodedTimestamp);
    
    CacheAddResult addResult = force ? mFrameContainer->ForceAdd(vf) : mFrameContainer->Add(vf);

    if (addResult == CacheAddResult::Added)
    {
        mCachedTimestamp = mDecodedTimestamp;
    }
    if (addResult == CacheAddResult::Duplicate)
    {
        mCachedTimestamp = mDecodedTimestamp;
        DisposeFrame(vf);
    }
    else if (addResult == CacheAddResult::Interrupted)
    {
        // Keep the frame as pending until the new job starts.
        // We might want to add it again to avoid gaps, 
        // or we might discard it if the new job is far away.
        log->DebugFormat("Cache add interrupted. Marking [{0}] as pending.", mDecodedTimestamp);
        mPendingFrame = vf;
    }
    
    //log->DebugFormat("Stored frame [{0}]. {1} ms.", mDecodedTimestamp, mStopwatch->ElapsedMilliseconds);

    return ReadResult::Success;
}


AVPixelFormat VideoReaderFFMpeg::GetSourceFormat(AVCodecContext* videoCodecCtx)
{
    if (!CanChangeDemosaicing)
    {
        return videoCodecCtx->pix_fmt;
    }

    switch (mVideoGeometry->Demosaicing)
    {
    case Demosaicing::RGGB:
        return AV_PIX_FMT_BAYER_RGGB8;
    case Demosaicing::BGGR:
        return AV_PIX_FMT_BAYER_BGGR8;
    case Demosaicing::GRBG:
        return AV_PIX_FMT_BAYER_GRBG8;
    case Demosaicing::GBRG:
        return AV_PIX_FMT_BAYER_GBRG8;
    case Demosaicing::None:
    default:
        return mVideoCodecCtx->pix_fmt;
    }
}


bool VideoReaderFFMpeg::RescaleAndConvert(AVFrame* srcFrame, AVFrame* dstFrame, int dstWidth, int dstHeight, AVPixelFormat dstPixelFormat, bool forSummary)
{
    // This variant doesn't support deinterlacing and uses the old sws_scale API.
    // It is faster than the new one.
    // By this point dstFrame is already allocated.

    bool result = true;
    AVPixelFormat srcFormat = GetSourceFormat(mVideoCodecCtx);

    int flags = SWS_BILINEAR;
    if (forSummary)
    {
        flags = SWS_POINT;
    }

    // TODO: keep the conext around and only recreate it when the values change.
    SwsContext* scalingCtx = sws_getContext(
        mVideoCodecCtx->width, mVideoCodecCtx->height, srcFormat,
        dstWidth, dstHeight, dstPixelFormat,
        flags, nullptr, nullptr, nullptr);

    const uint8_t* const* srcSlice = srcFrame->data;
    int* srcStride = srcFrame->linesize;
    int srcSliceY = 0;
    int srcSliceH = mVideoCodecCtx->height;
    uint8_t** dst = dstFrame->data;
    int* dstStride = dstFrame->linesize;

    try
    {
        sws_scale(scalingCtx, srcSlice, srcStride, srcSliceY, srcSliceH, dst, dstStride);
    }
    catch (Exception^)
    {
        result = false;
        log->Error("RescaleAndConvert Error : sws_scale failed.");
    }

    sws_freeContext(scalingCtx);

    return result;
}

bool VideoReaderFFMpeg::RescaleAndConvert2(AVFrame* srcFrame, AVFrame* dstFrame, int dstWidth, int dstHeight, AVPixelFormat dstPixelFormat, bool deinterlace)
{
    int srcWidth = srcFrame->width;
    int srcHeight = srcFrame->height;
    const AVPixelFormat srcPixelFormat = static_cast<AVPixelFormat>(srcFrame->format);
    
    // Recreate the graph if needed.
    if (!mFilterGraph ||
        mFilterSrcWidth != srcWidth || mFilterSrcHeight != srcHeight || mFilterSrcFormat != srcPixelFormat ||
        mFilterDstWidth != dstWidth || mFilterDstHeight != dstHeight ||
        mFilterDeinterlace != deinterlace)
    {
        AVRational sar = srcFrame->sample_aspect_ratio;
        bool created = CreateVideoFilterGraph(
            srcWidth, srcHeight, srcPixelFormat,
            dstWidth, dstHeight,
            deinterlace, sar);

        if (!created)
        {
            log->Error("RescaleAndConvert: CreateVideoFilterGraph failed.");
            return false;
        }
    }

    // Feed the decoded frame to libavfilter.
    int ret = av_buffersrc_add_frame_flags(mFilterSource, srcFrame, AV_BUFFERSRC_FLAG_KEEP_REF);
    if (ret < 0)
    {
        LogFFMpegError("av_buffersrc_add_frame_flags", ret);
        return false;
    }

    //-------------------------------------
    // Retrieve the processed frame.
    // Here we decide if we can use the AVFrame from the sink filter directly as 
    // the result or if we make an extra copy into a preallocated destination.
    // This is used to control the buffer alignment of the final frame.
    // 
    // For the copy scenario we first extract the frame into a staging AVFrame.
    //-------------------------------------

    if (mCopyFilteredFrame)
    {
        av_frame_unref(mFilteredFrame);
        ret = av_buffersink_get_frame(mFilterSink, mFilteredFrame);
    }
    else
    {
        ret = av_buffersink_get_frame(mFilterSink, dstFrame);
    }

    if (ret == AVERROR(EAGAIN))
    {
        // This can happen when YADIF needs another input frame before it can produce this output frame.
        log->Error("av_buffersink_get_frame returned EAGAIN.");
        return false;
    }
    else if (ret < 0)
    {
        LogFFMpegError("av_buffersink_get_frame", ret);
        return false;
    }
    
    // Copy the filtered frame into the destination if needed.
    // This will get rid of any padding and conform it to 
    // the alignment of the passed destination AVFrame.
    if (mCopyFilteredFrame)
    {
        ret = av_frame_make_writable(dstFrame);
        if (ret < 0)
        {
            LogFFMpegError("av_frame_make_writable", ret);
            av_frame_unref(mFilteredFrame);
            return false;
        }

        ret = av_frame_copy(dstFrame, mFilteredFrame);
        if (ret < 0)
        {
            LogFFMpegError("av_frame_copy", ret);
            av_frame_unref(mFilteredFrame);
            return false;
        }

        // Propagate PTS, color metadata, aspect ratio, side data, etc.
        ret = av_frame_copy_props(dstFrame, mFilteredFrame);
        av_frame_unref(mFilteredFrame);
    }
    
    return ret >= 0;
}


void VideoReaderFFMpeg::FreeVideoFilterGraph()
{
    // Note: the member variables are on managed-heap and can be moved by the GC.
    // The CLR is allowed to move the whole VideoReaderFFMpeg object, 
    // and the memory of say, mFilteredFrame can change, so we can't directly pass their address.
    // &mFilteredFrame has type interior_ptr<AVFrame*>, not AVFrame**. 
    // We can either use a pin_ptr or a temporary variable.

    if (mCopyFilteredFrame)
    {
        AVFrame* pFilteredFrame = mFilteredFrame;
        av_frame_free(&pFilteredFrame);
        mFilteredFrame = nullptr;
    }

    AVFilterGraph* pFilterGraph = mFilterGraph;
    avfilter_graph_free(&pFilterGraph);
    mFilterGraph = nullptr;

    mFilterSource = nullptr;
    mFilterSink = nullptr;

    mFilterSrcWidth = 0;
    mFilterSrcHeight = 0;
    mFilterSrcFormat = AV_PIX_FMT_NONE;

    mFilterDstWidth = 0;
    mFilterDstHeight = 0;
}


bool VideoReaderFFMpeg::CreateVideoFilterGraph(
    int srcWidth, int srcHeight, AVPixelFormat srcPixelFormat,
    int dstWidth, int dstHeight,
    bool deinterlace, AVRational sar)
{
    FreeVideoFilterGraph();

    //----------------------------------------------------
    // Build the following filter graph:
    // buffer -> [yadif] -> scale -> format -> buffersink.
    //----------------------------------------------------

    const AVFilter* bufferFilter = avfilter_get_by_name("buffer");
    const AVFilter* yadifFilter = deinterlace ? avfilter_get_by_name("yadif") : nullptr;
    const AVFilter* scaleFilter = avfilter_get_by_name("scale");
    const AVFilter* formatFilter = avfilter_get_by_name("format");
    const AVFilter* bufferSinkFilter = avfilter_get_by_name("buffersink");

    if (!bufferFilter || (deinterlace && !yadifFilter) || !scaleFilter || !formatFilter || !bufferSinkFilter)
    {
        log->Error("Failed to get one or more filters.");
        return false;
    }

    mFilterGraph = avfilter_graph_alloc();
    if (!mFilterGraph)
    {
        log->Error("Failed to allocate filter graph.");
        return false;
    }

    //------------------------------
    // Buffer
    //------------------------------
    AVRational timebase = mFormatCtx->streams[mVideoStreamIndex]->time_base;
    if (sar.num <= 0 || sar.den <= 0)
    {
        sar = av_make_q(1, 1);
    }

    char args[512];
    snprintf(args, sizeof(args), 
        "video_size=%dx%d:pix_fmt=%d:time_base=%d/%d:pixel_aspect=%d/%d",
        srcWidth, srcHeight, srcPixelFormat,
        timebase.num, timebase.den,
        sar.num, sar.den);

    AVFilterContext* filterSourceCtx = nullptr;
    int ret = avfilter_graph_create_filter(&filterSourceCtx, bufferFilter, "source", args, nullptr, mFilterGraph);
    if (ret < 0)
    {
        LogFFMpegError("Failed to create buffer filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    mFilterSource = filterSourceCtx;
    AVFilterContext* previousCtx = mFilterSource;

    //------------------------------
    // Yadif (deinterlacing)
    //------------------------------
    if (deinterlace)
    {
        // Configuration
        // One output frame per input frame rather than one per field, 
        // automatically determine field parity, 
        // deinterlace every frame.
        AVFilterContext* yadifCtx = nullptr;
        ret = avfilter_graph_create_filter(&yadifCtx, yadifFilter, "yadif", "mode=send_frame:parity=auto:deint=all", nullptr, mFilterGraph);
        if (ret < 0)
        {
            LogFFMpegError("Failed to create yadif filter.", ret);
            FreeVideoFilterGraph();
            return false;
        }

        ret = avfilter_link(previousCtx, 0, yadifCtx, 0);
        if (ret < 0)
        {
            LogFFMpegError("Failed to link yadif filter.", ret);
            FreeVideoFilterGraph();
            return false;
        }
            
        previousCtx = yadifCtx;
    }

    //------------------------------
    // Scaling
    //------------------------------
    // fast-bilinear is speed over quality, not needed for main images.

    AVFilterContext* scaleCtx = nullptr;
    snprintf(args, sizeof(args), "w=%d:h=%d:flags=bilinear", dstWidth, dstHeight);
    ret = avfilter_graph_create_filter(&scaleCtx, scaleFilter, "scale", args, nullptr, mFilterGraph);
    if (ret < 0)
    {
        LogFFMpegError("Failed to create scale filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    ret = avfilter_link(previousCtx, 0, scaleCtx, 0);
    if (ret < 0)
    {
        LogFFMpegError("Failed to link scale filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    previousCtx = scaleCtx;

    //------------------------------
    // Pixel format conversion
    // bgra = AV_PIX_FMT_BGRA.
    //------------------------------
    AVFilterContext* formatCtx = nullptr;
    ret = avfilter_graph_create_filter(&formatCtx, formatFilter, "format", "pix_fmts=bgra", nullptr, mFilterGraph);
    if (ret < 0)
    {
        LogFFMpegError("Failed to create format filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    ret = avfilter_link(previousCtx, 0, formatCtx, 0);
    if (ret < 0)
    {
        LogFFMpegError("Failed to link format filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    previousCtx = formatCtx;

    //------------------------------
    // Buffersink
    //------------------------------
    AVFilterContext* filterSinkCtx = nullptr;
    ret = avfilter_graph_create_filter(&filterSinkCtx, bufferSinkFilter, "sink", nullptr, nullptr, mFilterGraph);
    if (ret < 0)
    {
        LogFFMpegError("Failed to create buffersink filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    mFilterSink = filterSinkCtx;

    ret = avfilter_link(previousCtx, 0, mFilterSink, 0);
    if (ret < 0)
    {
        LogFFMpegError("Failed to link buffersink filter.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    ret = avfilter_graph_config(mFilterGraph, nullptr);
    if (ret < 0)
    {
        LogFFMpegError("Failed to configure filter graph.", ret);
        FreeVideoFilterGraph();
        return false;
    }

    if (mCopyFilteredFrame)
    {
        mFilteredFrame = av_frame_alloc();
        if (!mFilteredFrame)
        {
            log->Error("Failed to allocate filtered frame.");
            FreeVideoFilterGraph();
            return false;
        }
    }
    
    // Recall the parameters so we can check if the filter graph needs to be rebuilt.
    mFilterSrcWidth = srcWidth;
    mFilterSrcHeight = srcHeight;
    mFilterSrcFormat = srcPixelFormat;
    mFilterDstWidth = dstWidth;
    mFilterDstHeight = dstHeight;
    mFilterDeinterlace = deinterlace;
    return true;
}


void VideoReaderFFMpeg::ApplyRotation(Bitmap^ bmp, ImageRotation rotation)
{
    switch (rotation)
    {
    case ImageRotation::Rotate90:
        bmp->RotateFlip(RotateFlipType::Rotate90FlipNone);
        break;
    case ImageRotation::Rotate180:
        bmp->RotateFlip(RotateFlipType::Rotate180FlipNone);
        break;
    case ImageRotation::Rotate270:
        bmp->RotateFlip(RotateFlipType::Rotate270FlipNone);
        break;
    default:
        break;
    }
}


void VideoReaderFFMpeg::DisposeFrame(VideoFrame^ videoFrame)
{
    if (videoFrame->Image == nullptr)
    {
        // This should never happen.
        log->WarnFormat("DisposeFrame called with null image.");
        return;
    }

    //log->DebugFormat("Disposing frame [{0}].", videoFrame->Timestamp);
    // Dispose the Bitmap and the native buffer.
    IntPtr bufferPtr = IntPtr::Zero;
    if (videoFrame->Image->Tag != nullptr)
    {
        bufferPtr = safe_cast<IntPtr>(videoFrame->Image->Tag);
        videoFrame->Image->Tag = nullptr;
    }

    delete videoFrame->Image;

    if (bufferPtr != IntPtr::Zero)
    {
        AVBufferRef* bufferRef = static_cast<AVBufferRef*>(bufferPtr.ToPointer());
        av_buffer_unref(&bufferRef);
    }
}

#pragma endregion

#pragma region PreBuffering thread




void VideoReaderFFMpeg::StartPreBufferingThread(int64_t startTimestamp)
{
    //--------------------------
    // Runs on the UI thread.
    //--------------------------
    if (!CanPreBuffer)
    {
        throw gcnew InvalidProgramException();
    }

    if (mCachingMode != VideoDecodingMode::PreBuffering || mFrameContainer != mPreBuffer)
    {
        throw gcnew InvalidProgramException();
    }

    if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
    {
        // This shouldn't happen.
        log->Error("Prebuffering thread already started");
        StopPreBufferingThread();
        mPreBuffer->Shutdown();
    }

    // Make sure we allow adding the first frame.
    mPreBufferingThreadCanceler->Reset();
    mPreBuffer->ResetInterruptAdd();

    // Read the first frame outside the decoding thread so the UI may request it immediately.
    if (startTimestamp >= 0)
    {
        log->DebugFormat("Buffering thread. Relocating to [~{0}] before starting the thread.", startTimestamp);
        ReadResult res = ReadFrameSeek(startTimestamp, true, true, false);
        if (res == ReadResult::Success)
        {
            // Note that the frame actually closest to the target timestamp may be the 
            // one before the last one. Since we use a preroll window it sould be there.
            // If the file has only I-frames there is a possibility to land after though.
            log->DebugFormat("Read first frame synchronously until [{0}].", mCachedTimestamp);
            mPreBuffer->AcquireClosest(startTimestamp);
            mWorkingZone = VideoSection(startTimestamp, mWorkingZone.End);
        }
    }

    log->Debug("Starting prebuffering thread.");
    ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &VideoReaderFFMpeg::PreBufferingWorker);
    mPreBufferingThread = gcnew Thread(pts);
    mPreBufferingThread->Start(mPreBufferingThreadCanceler);
}

void VideoReaderFFMpeg::StopPreBufferingThread()
{
    if (mPreBufferingThread == nullptr || !mPreBufferingThread->IsAlive)
        return;

    log->Debug("Stopping prebuffering thread.");

    mPreBufferingThreadCanceler->Cancel();

    // Signal the cache that we are cancelling.
    // This will unblock the thread if it's waiting in Add().
    mPreBuffer->InterruptAdd();

    // Wake up the thread if it's waiting for a job.
    lock l(mLockNewJobReady);
    {
        Monitor::PulseAll(mLockNewJobReady);
        l.release();
    }

    mPreBufferingThread->Join();
    mPreBufferingThreadCanceler->Reset();
}

void VideoReaderFFMpeg::PreBufferingWorker(Object^ objCanceller)
{
    Thread::CurrentThread->Name = "PreBuffering";
    ThreadCanceler^ canceller = (ThreadCanceler^)objCanceller;

    log->DebugFormat("PreBuffering thread started.");

    //-------------------------------
    // Main decoding loop.
    // This loop is live as long as we are in Prebuffering mode.
    // The decoding thread waits on synchronization objects in two cases.
    // 
    // 1. before storing a frame, if the cache is full. 
    // In this case it has useful work but can't currently store another frame.
    // It will be woken up when the player moves forward and a frame from the retention
    // becomes stale.
    // 
    // 2. at the end of the working zone or if the player published a new state.
    // In this case it has no useful work to do, it waits for the reader to wake it.
    //-------------------------------

    mDecodedFrames = 0;
    ApplyFrameSkippingPolicy(FrameSkippingPolicy::Normal);

    mWorkingPlayerState = WaitForNewJobReady(canceller, -1);

    while (true)
    {
        if (canceller->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation.");
            break;
        }

        if (mWorkingPlayerState == nullptr)
        {
            break;
        }

        InitDecodingJob(mWorkingPlayerState);

        if (HasJobChanged())
        {
            mWorkingPlayerState = WaitForNewJobReady(canceller, mWorkingPlayerState->Id);
            continue;
        }

        ReadResult result = ProcessJob(canceller);
        
        if (result == ReadResult::ThreadCancelled)
        {
            break;
        }

        // If we leave the ProcessJob loop it means we are done.
        // Either EOF was reached or we detected a new job request.
        // -> wait for next job to arrive and/or be ready.

        mWorkingPlayerState = WaitForNewJobReady(canceller, mWorkingPlayerState->Id);
    }

    log->DebugFormat("Exiting PreBuffering thread.");

    if (mPendingFrame != nullptr)
    {
        log->DebugFormat("Disposing pending frame [{0}] on thread exit.", mPendingFrame->Timestamp);
        DisposeFrame(mPendingFrame);
        mPendingFrame = nullptr;
    }
}

ReadResult VideoReaderFFMpeg::ProcessJob(ThreadCanceler^ canceller)
{
    // The job in itself is essentially just a series of ReadFrameNext().
    // The decoder has already been relocated.

    log->DebugFormat("ProcessJob. Starting sequential decoding for job #{0} ({1}) -----------------------", 
        mWorkingPlayerState->Id, mWorkingPlayerState->Action);

    mPreBuffer->Print();

    ReadResult res = ReadResult::Success;

    while (true)
    {
        if (canceller->CancellationPending)
        {
            log->DebugFormat("ProcessJob cancelled.");
            return ReadResult::ThreadCancelled;
        }

        if (HasJobChanged())
        {
            // Abandon work.
            return ReadResult::NewJob;
        }

        // Read the next frame.
        // This will perform:
        // - decoding
        // - scaling/conversion/deint/rotation
        // - store in the prebuffer cache.
        // 
        // During playback each step can be skipped depending on the lag.
        // If the cache is full this will wait in PreBuffer.Add().

        if (mWorkingPlayerState->Action == PlayerAction::Playback)
        {
            UpdateFrameSkippingPolicy();
        }

        res = ReadFrameNext();

        if (canceller->CancellationPending)
        {
            log->DebugFormat("ProcessJob cancelled.");
            return ReadResult::ThreadCancelled;
        }

        if (HasJobChanged())
        {
            // Abandon this job, wait for a new one.
            return ReadResult::NewJob;
        }

        if (res == ReadResult::EOFReached || mDecodedTimestamp >= mWorkingZone.End)
        {
            // No more work, wait for a new job.
            log->DebugFormat("EOF reached.");
            return ReadResult::EOFReached;
        }
    }

    return ReadResult::ThreadCancelled;
}

bool VideoReaderFFMpeg::HasJobChanged()
{
    PlayerState^ latestPlayerState = Volatile::Read(this->mRequestedPlayerState);

    if (mWorkingPlayerState == nullptr)
    {
        log->ErrorFormat("mWorkingPlayerState == nullptr");
        return true;
    }

    return latestPlayerState->Id > mWorkingPlayerState->Id;
}

PlayerState^ VideoReaderFFMpeg::WaitForNewJobReady(ThreadCanceler^ canceller, int currentJobId)
{
    lock l(mLockNewJobReady);
    {
        while (true)
        {
            if (canceller->CancellationPending)
            {
                log->DebugFormat("Cancellation while waiting for a new job.");
                break;
            }

            PlayerState^ state = Volatile::Read(this->mRequestedPlayerState);

            if (state->Id > currentJobId && mReadyJobId >= state->Id)
            {
                log->DebugFormat("New job detected and ready: {0}", state);
                return state;
            }

            //log->DebugFormat("Prebuffer thread, entering wait until a new job is ready. Current: #{0}.", mWorkingPlayerState->Id);

            Monitor::Wait(mLockNewJobReady);
        }

        l.release();
    }

    return nullptr;
}


void VideoReaderFFMpeg::UpdateAllowFrameSkipping(bool allow)
{
    if (mAllowFrameSkipping == allow)
        return;
    
    mAllowFrameSkipping = allow;
    log->DebugFormat("UpdateAllowFrameSkipping: {0}", mAllowFrameSkipping);
}

void VideoReaderFFMpeg::UpdateFrameSkippingPolicy()
{
    if (!mAllowFrameSkipping)
        return;

    if (mWorkingPlayerState->Action != PlayerAction::Playback)
        return;

    // Estimate how far behind we are compared to the player.
    // Note: this lag is in presentation space and independent from the speed slider.
    long expectedTimestamp = this->GetPlaybackTimestamp(mWorkingPlayerState);
    double lag = (expectedTimestamp - mCachedTimestamp) / mVideoInfo.AverageTimeStampsPerSeconds;

    String^ logLine = String::Format("UpdateFrameSkippingPolicy. Lag: {0:0.000} s, Cache: {1}/{2}. Frame: {3}.",
        lag, mPreBuffer->Count, mPreBuffer->Capacity, mDecodedFrames);

    log->DebugFormat(logLine);
            
    // Check worst case scenario first.
    if (lag > mSeekAheadLagThreshold)
    {
        log->ErrorFormat("Lag of {0:0.000} s is over seek-ahead threshold. Cache: {1}/{2}. Frame: {3}", 
            lag, mPreBuffer->Count, mPreBuffer->Capacity, mDecodedFrames);
            
        // We are hopelessly behind, try to seek ahead.
        // Note: calling seek directly with min, target, max, doesn't always work.
        // 
        // First, some decoders just fallback to AVSEEK_FLAG_BACKWARD which moves us 
        // back in time and is worse than doing nothing.
        // 
        // Secondly, seek is done in a different "domain" than the timestamps we get from decoding frames.
        // The values of packet->pts don't always match the values of frame->best_effort_timestamp. 
        // 
        // Solution: during decoding we keep track of the timestamps of the start of the GOP and only do the 
        // seek if we are going to a new GOP.
        // Here we poll the index for the keyframe of the GOP containing the target.
        // Only works on files with an index.
        AVStream* stream = mFormatCtx->streams[mVideoStreamIndex];
        const AVIndexEntry* entry = avformat_index_get_entry_from_timestamp(stream, expectedTimestamp, AVSEEK_FLAG_BACKWARD);
        if (entry != nullptr)
        {
            //log->DebugFormat("Found index entry for timestamp [{0}]. Key at [{1}].", expectedTimestamp, entry->timestamp);
                
            // Only seek if the keyframe is ahead of the current GOP start. 
            // Otherwise we would just be seeking backwards.
            int64_t seekTimestamp = entry->timestamp;
            if (seekTimestamp > mCurrentGopTimestamp)
            {
                log->WarnFormat("Decoding thread synchronization. Seeking ahead to [{0}]", seekTimestamp);
                int res = avformat_seek_file(
                    mFormatCtx,
                    mVideoStreamIndex,
                    seekTimestamp,
                    seekTimestamp,
                    seekTimestamp,
                    0);
                    
                if (res >= 0)
                {
                    avcodec_flush_buffers(mVideoCodecCtx);
                    mCachedTimestamp = AV_NOPTS_VALUE;
                    mCurrentGopTimestamp = AV_NOPTS_VALUE;
                }
            }

            // TODO: Should we further catch up by advance here if we are still behind?
        }
    }
    else
    {
        // Figure out the frame skipping policy we should apply.
        // Different thresholds for entering and leaving the levels to avoid oscillations.

        // Behind mode: drop the scale/convert work for frames that aren't presented.
        // Only switch back when we are comfortably ahead.
        double thresholdEnterBehind = 0.000;
        double thresholdLeaveBehind = -0.200;

        // Far behind mode: drop decoding of B-frames at ffmpeg level.
        // This only makes a difference for files that have B-frames in the first place.
        double thresholdEnterFarBehind = 0.400;
        double thresholdLeaveFarBehind = 0.200;
                
        FrameSkippingPolicy oldPolicy = mFrameSkippingPolicy;
        switch (mFrameSkippingPolicy)
        {
        case FrameSkippingPolicy::Normal:
            if (lag > thresholdEnterFarBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::FarBehind);
            }
            else if (lag > thresholdEnterBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::Behind);
            }
            break;
        case FrameSkippingPolicy::Behind:
            if (lag > thresholdEnterFarBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::FarBehind);
            }
            else if (lag < thresholdLeaveBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::Normal);
            }
            break;
        case FrameSkippingPolicy::FarBehind:
            if (lag < thresholdLeaveBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::Normal);
            }
            else if (lag < thresholdLeaveFarBehind)
            {
                ApplyFrameSkippingPolicy(FrameSkippingPolicy::Behind);
            }
            break;
        }
    }
}

void VideoReaderFFMpeg::ApplyFrameSkippingPolicy(FrameSkippingPolicy policy)
{
    if (policy == mFrameSkippingPolicy)
        return;

    log->DebugFormat("Updating decoding policy: {0} -> {1}.", mFrameSkippingPolicy, policy);
    mFrameSkippingPolicy = policy;

    // Note: we can change mVideoCodecCtx->skip_frame at any time 
    // no need to close and re-open the codec.

    switch (policy)
    {
    case FrameSkippingPolicy::Normal:
    case FrameSkippingPolicy::Behind:
        mVideoCodecCtx->skip_frame = AVDISCARD_DEFAULT;
        break;

    case FrameSkippingPolicy::FarBehind:
        mVideoCodecCtx->skip_frame = AVDISCARD_NONREF;

        if (mVideoCodecCtx->has_b_frames == 0)
        {
            log->Warn("File has no B-frames, skipping non-ref frames will have no effect.");
        }

        break;
    }
}

void VideoReaderFFMpeg::InitDecodingJob(PlayerState^ state)
{
    //-------------------------------
    // Runs in the prebuffer thread.
    //-------------------------------

    log->DebugFormat("InitDecodingJob. Job: #{0}. Last decoded: [{1}]. Last stored: [{2}].", 
        mWorkingPlayerState->Id, mDecodedTimestamp, mCachedTimestamp);

    // By this point we have abandonned all work from the old job,
    // the reader has finished preparing the cache for the new job.
    // We can now officially start working on the new job.

    // Make sure the cache accepts frames again.
    mPreBuffer->ResetInterruptAdd();

    // Reset playback decoding policy.
    ApplyFrameSkippingPolicy(FrameSkippingPolicy::Normal);

    if (state->SynchronousFulfill)
    {
        // In this case the decoder was already relocated, the request already fulfilled, 
        // and the player acquired the target.
        // The only thing left is the possible pending frame.
        if (mPendingFrame != nullptr)
        {
            ResubmitPending(state->ReferenceTimestamp, false);
        }

        return;
    }

    TryAcquireResult^ tryAcquireResult = Volatile::Read(this->mTryAcquireResult);

    // Get a relocation plan and purge the cache.
    // In some cases this will also acquire the target.
    DecodingJobPlan^ plan = GetDecodingJobPlan(state, tryAcquireResult);
    
    log->Debug(plan);

    log->DebugFormat("Executing plan for decoding job #{0}.", state->Id);

    // By the end of this we MUST have signalled the player that the request is 
    // either fulfilled or failed, so it can restart scheduling new jobs.

    ExecuteDecodingJobPlan(state, plan);

    if (plan->TargetAcquired || plan->RequestFulfilledInPlanning)
    {
        return;
    }

    // Request failed.
    OnRequestFulfilled(state, false, -1);
}

DecodingJobPlan^ VideoReaderFFMpeg::GetDecodingJobPlan(PlayerState^ state, TryAcquireResult^ tryAcquireResult)
{
    //-----------------------------------------------------------------
    // The goal of this function is to find where we should move the decoder to:
    // 1. fulfill the immediate request if it wasn't acquired yet
    // 2. continue decoding from there.
    //
    // Just because the request was already acquired doesn't mean the decoder is at 
    // the right spot to continue.
    // 
    // Once the immediate request is fulfilled and the decoder relocated, 
    // the actual "job" will simply continue decoding frames sequentially 
    // until EOF or cancellation.
    // 
    // Buffer: by this point the cache has already been purged of any frames 
    // older than the retention window compared to the closest match to the target.
    // This function will further purge if we need to seek.
    // 
    // Important: when in doubt we can always fall back to seeking to the target and 
    // restart decoding from there.
    // Everything else is optimization, so we identify known scenarios 
    // rather than try to handle all combinations of states.
    // If we can't prove an optimization is safe we should seek.
    //-----------------------------------------------------------------

    // Grab the results of the initial request handling from the cache side.
    bool isAcquired = tryAcquireResult->TargetAcquired;
    int64_t acquiredTimestamp = tryAcquireResult->AcquiredTimestamp;
    
    // Some useful variables for all scenarios.
    bool hasPending = mPendingFrame != nullptr;
    int64_t targetTimestamp = isAcquired ? acquiredTimestamp : state->ReferenceTimestamp;

    // Failsafe scenario is to just seek and restart decoding,
    // unless we can prove we have a better thing to do.
    DecodingJobPlan^ plan = gcnew DecodingJobPlan();
    plan->TargetTimestamp = targetTimestamp;
    plan->TargetAcquired = isAcquired;
    plan->RequestFulfilledInPlanning = false;
    plan->DecoderRelocation = DecoderRelocation::Seek;
    plan->ResubmitPending = false;
    
    log->DebugFormat("Creating relocation plan: Target: [~{0}]. Acquired: {1}.", targetTimestamp, isAcquired);
    mPreBuffer->Print();

    // In the case we couldn't find next/prev, approximate the requested target timestamp.
    // In these cases the reference timestamp was set to the current frame.
    if (!isAcquired)
    {
        if (state->Action == PlayerAction::StepForward)
        {
            targetTimestamp = state->ReferenceTimestamp + mVideoInfo.AverageTimeStampsPerFrame;
            plan->TargetTimestamp = targetTimestamp;
        }
        else if (state->Action == PlayerAction::StepBackward)
        {
            targetTimestamp = state->ReferenceTimestamp - mVideoInfo.AverageTimeStampsPerFrame;
            plan->TargetTimestamp = targetTimestamp;
        }
    }


    if (mDecodedTimestamp < 0)
    {
        // The decoder location is not known.
        // This may happen for example if we start a seek and get interrupted 
        // with a new job before decoding even the first frame.

        // There is one valid case: we reached EOF and we have nothing more to decode.
        // TODO: keep an EOF flag, this will be more accurate.
        if (isAcquired)
        {
            plan->DecoderRelocation = DecoderRelocation::None;
            return plan;
        }

        log->DebugFormat("DecodingJobPlan: Decoder is nowhere: seeking.");
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }

    //---------------------------------------------
    // This can be further simplified.
    // Probably we only need to consider the furthest ahead frame between
    // pending, last decode and last stored.
    //---------------------------------------------

    if (hasPending && mPendingFrame->Timestamp != mDecodedTimestamp)
    {
        // This should never happen, so pretend it didn't happen.
        log->DebugFormat("DecodingJobPlan: Discarding pending frame [{0}].", mPendingFrame->Timestamp);
        DisposeFrame(mPendingFrame);
        mPendingFrame = nullptr;
        hasPending = false;
    }

    // The "happy surprise" cases: the target was decoded while the job was in preparation.
    if (!isAcquired)
    {
        if (hasPending)
        {
            // Case 1: the target was decoded but was blocked in add().
            TimestampRelation relPendingTarget = RelateTimestamps(mPendingFrame->Timestamp, targetTimestamp);
            if (relPendingTarget == TimestampRelation::Match)
            {
                log->DebugFormat("DecodingJobPlan: Pending frame matches target.");
            
                // Force add.
                int64_t resolvedTarget = mPendingFrame->Timestamp;
                bool addedMatch = ResubmitPending(targetTimestamp, true);
                if (addedMatch)
                {
                    OnRequestFulfilled(state, true, resolvedTarget);
                    plan->RequestFulfilledInPlanning = true;
                }
                
                plan->ResubmitPending = false;
                plan->DecoderRelocation = DecoderRelocation::None;
                return plan;
            }

            // The pending frame doesn't match the target.
            // Keep it around for now, we'll see in the other scenarios if we need
            // to resubmit it or not.
        }
        else if (mCachedTimestamp > 0)
        {
            // Case 2: the target was decoded and added to the cache.
            TimestampRelation relTarget = RelateTimestamps(mCachedTimestamp, plan->TargetTimestamp);
            if (relTarget == TimestampRelation::Match)
            {
                log->DebugFormat("DecodingJobPlan: Last cached frame matches target.");

                OnRequestFulfilled(state, true, mCachedTimestamp);
                plan->RequestFulfilledInPlanning = true;
                plan->DecoderRelocation = DecoderRelocation::None;
                return plan;
            }
        }
    }

    // Sanity check.
    CacheTimestampRelation relCache = mPreBuffer->RelateTimestamp(mDecodedTimestamp);
    if (relCache == CacheTimestampRelation::FarAhead)
    {
        log->ErrorFormat("DecodingJobPlan: Rogue decoder far ahead of the cache: seeking.");

        // This may happen when moving wildly on the timeline because the cache
        // always tries to keep the "current" pointer alive when purging.
        DisposePending();
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }

    // Next decisions: if the target is completely outside the cache.
    relCache = mPreBuffer->RelateTimestamp(targetTimestamp);
    
    if (relCache == CacheTimestampRelation::Empty)
    {
        log->DebugFormat("DecodingJobPlan: Cache is empty: seeking.");
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }
    else if (relCache == CacheTimestampRelation::Behind)
    {
        log->DebugFormat("DecodingJobPlan: Target is before prebuffer cache: seeking.");
        
        // Cache: for now just purge.
        // If we are just doing a -1 from start there is maybe an argument to keep
        // the frames ahead but it's tough since we lose the decoder location anyway.
        // If the player moves forward again the frames would be available,
        // and decoded as duplicates at the same time.
        DisposePending();
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }
    else if (relCache == CacheTimestampRelation::Ahead)
    {
        log->DebugFormat("DecodingJobPlan: Target ahead of prebuffer: advancing.");
        
        // The cache has already been purged and only retains a retention 
        // window worth of frames at the end.
        plan->ResubmitPending = true;
        plan->DecoderRelocation = DecoderRelocation::Advance;
        return plan;
    }
    else if (relCache == CacheTimestampRelation::FarAhead)
    {
        log->DebugFormat("DecodingJobPlan: Target far ahead of prebuffer: seeking.");
        
        DisposePending();
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }


    // By now we know the target is within the bounds of the cache.
    // Whether the target was acquired or not, for now we use the same rules.
    // Compare where the decoder is with respect to the target.
    // TODO: watch out for sparse -> dense job transition.
    TimestampRelation relTarget = RelateTimestamps(mDecodedTimestamp, targetTimestamp);

    if (relTarget == TimestampRelation::Match)
    {
        log->DebugFormat("DecodingJobPlan: Decoder matches player.");

        // We are in a good place, but no wiggle room.

        if (!plan->TargetAcquired && !plan->RequestFulfilledInPlanning)
        {
            log->WarnFormat("DecodingJobPlan: Decoder matches player but request not fulfilled.");
        }

        plan->ResubmitPending = true;
        plan->DecoderRelocation = DecoderRelocation::None;
        return plan;
    }
    else if (relTarget == TimestampRelation::Behind)
    {
        log->DebugFormat("DecodingJobPlan: Decoder is behind player: advancing.");
        plan->ResubmitPending = true;
        plan->DecoderRelocation = DecoderRelocation::Advance;
        return plan;
    }
    else if (relTarget == TimestampRelation::Ahead)
    {
        log->DebugFormat("DecodingJobPlan: Decoder is ahead of player.");

        // Decoder is ahead, good.
        // This is the best-case scenario. 
        // For example player was at [0], asking for [1], and we have decoded everything 
        // between [0] and [31], maybe even with [32] pending.
        
        if (!isAcquired)
        {
            // We're good but we haven't found the exact target in the cache on first try. 
            // This can happen if the cache is sparse and the request is for a frame that wasn't stored.
            // Tell the player to acquire whatever is closest anyway.
            OnRequestFulfilled(state, true, plan->TargetTimestamp);
            plan->RequestFulfilledInPlanning = true;

            log->DebugFormat("DecodingJobPlan: Request within cache bounds but no nearby frames. Target:[~{0}].", 
                plan->TargetTimestamp);

            // This is a problem with sparse jobs.
            // If we haven't added the frame, then we won't find it now either.
            // It will just pick a neighboring frame.
            // Since the decoder is already ahead we'll never decode it.
        }
        
        plan->ResubmitPending = true;
        plan->DecoderRelocation = DecoderRelocation::None;
        return plan;
    }
    else if (relTarget == TimestampRelation::FarAhead)
    {
        log->WarnFormat("DecodingJobPlan: Decoder is far ahead of player: seeking.");
        
        // Decoder is far ahead but still within the cache.
        // One way to get this is to move the timeline quickly back and forth between far away locations,
        // so that spurious frames may linger at either end due to being pinned as current.
        DisposePending();
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }
    else if (relTarget == TimestampRelation::FarBehind)
    {
        log->WarnFormat("DecodingJobPlan: Decoder is far behind player: seeking.");
        
        // Decoder far behind but still within the cache.
        // Same as above, may happen when moving widldy back and forth between remote locations.
        DisposePending();
        mPreBuffer->Purge();
        plan->DecoderRelocation = DecoderRelocation::Seek;
        return plan;
    }

    return plan;
}


void VideoReaderFFMpeg::ExecuteDecodingJobPlan(PlayerState^ state, DecodingJobPlan^ plan)
{
    //-------------------------------
    // Runs on the prebuffer thread.
    //-------------------------------
    
    // Relocate the decoder according to the plan.
    // 
    // The first thing we need to handle is the pending frame if any.
    // Then we move to the target.
    // While moving towards the target we might want to add the last 
    // few frames to pre-fill the back part of the cache.
    // 
    // Just because the target was acquired during preparation doesn't mean 
    // we don't have to move the decoder.
    // We might be behind the target and need to advance.
    

    // Double check in case the target was decoded/stored during preparation.
    // Case 1: we managed to add it to the cache.
    // Case 2: it was the pending frame.
    // TODO: move this back with the other case in GetDecodingJobPlan().
    

    if (plan->DecoderRelocation == DecoderRelocation::Seek)
    {
        log->DebugFormat("ExecuteDecodingJobPlan: seeking to [~{0}].", plan->TargetTimestamp);

        DisposePending();

        // Seek and decode until the target.
        ReadResult res = ReadFrameSeek(plan->TargetTimestamp, true, true, true);
        if (res == ReadResult::Success)
        {
            // Note: use the original request target for bracketing.
            OnRequestFulfilled(state, true, plan->TargetTimestamp);
            plan->RequestFulfilledInPlanning = true;
            return;
        }
    }
    else if (plan->DecoderRelocation == DecoderRelocation::Advance)
    {
        // Decode from wherever we are, until we reach the target.
        
        log->DebugFormat("ExecuteDecodingJobPlan: advancing to [~{0}].", plan->TargetTimestamp);

        if (plan->ResubmitPending && mPendingFrame != nullptr)
        {
            // We MUST advance so we use force=true to make sure this add 
            // doesn't block and we can continue with the ReadFrameSeek below.
            bool addedMatch = ResubmitPending(plan->TargetTimestamp, true);
            if (addedMatch)
            {
                // This should never happen but we'll take it anyway.
                OnRequestFulfilled(state, true, plan->TargetTimestamp);
                plan->RequestFulfilledInPlanning = true;
                return;
            }
        }
        else
        {
            DisposePending();
        }

        ReadResult res = ReadFrameSeek(plan->TargetTimestamp, false, true, true);
        if (res == ReadResult::Success)
        {
            // Report the original target to get bracketing.
            OnRequestFulfilled(state, true, plan->TargetTimestamp);
            plan->RequestFulfilledInPlanning = true;
            return;
        }

        return;
    }
    else if (plan->DecoderRelocation == DecoderRelocation::None)
    {
        // The decoder is already in the right spot.
        bool addedMatch = false;
        if (plan->ResubmitPending && mPendingFrame != nullptr)
        {
            // Note that here we submit with force=false.
            // We don't want to disloge the first frame of the cache.
            // Example scenario: flicking between frame 0 and 1, with cache filled to 32.
            addedMatch = ResubmitPending(plan->TargetTimestamp, false);
            if (addedMatch)
            {
                OnRequestFulfilled(state, true, plan->TargetTimestamp);
                plan->RequestFulfilledInPlanning = true;
                return;
            }
        }
        else
        {
            DisposePending();
        }
    }
}


void VideoReaderFFMpeg::DisposePending()
{
    if (mPendingFrame != nullptr)
    {
        log->DebugFormat("Disposing pending frame [{0}].", mPendingFrame->Timestamp);
        DisposeFrame(mPendingFrame);
        mPendingFrame = nullptr;
    }
}


bool VideoReaderFFMpeg::ResubmitPending(int64_t target, bool force)
{
    //-------------------------------------------------
    // We can get here in a few cases.
    // 
    // 1. Pending frame is the requested target.
    //    Should be sumbitted with force.
    // 
    // 2. Target was aquired from cache during job preparation.
    //    Don't force the add. This may immediately block again, but that's ok.
    //    We'll wake up when the player moves forward or when a new job arrives.
    // 
    // 3. Target is ahead of the pending frame.
    //    Force add and continue decoding.
    //-------------------------------------------------

    CacheAddResult result = force ? mPreBuffer->ForceAdd(mPendingFrame) : mPreBuffer->Add(mPendingFrame);

    if (result == CacheAddResult::Added)
    {
        // Update last cached frame.
        mCachedTimestamp = mPendingFrame->Timestamp;
        mPendingFrame = nullptr;

        TimestampRelation relTarget = RelateTimestamps(mCachedTimestamp, target);
        if (relTarget == TimestampRelation::Match)
        {
            // The pending frame was the requested frame.
            // This can happen when we step forward fast enough.
            // The new request can come while we are decoding/storing it.
            // The caller should signal the UI that the request is now fulfilled.
            return true;
        }
    }
    else if (result == CacheAddResult::Duplicate)
    {
        DisposePending();
    }
    else if (result == CacheAddResult::Interrupted)
    {
        // Interrupted again.
        // The frame stays in pending state.
        log->DebugFormat("ResubmitPending: [{0}] is still pending.", mPendingFrame->Timestamp);
    }

    return false;
}


#pragma endregion

#pragma region Logging helpers
void VideoReaderFFMpeg::LogFileInfo()
{
    log->Debug("---------------------------------------------------");
    log->Debug("[File] - Filename : " + Path::GetFileName(mVideoInfo.FilePath));
    
    // Format
    log->DebugFormat("[Format] - Format name: {0} ({1})", gcnew String(mFormatCtx->iformat->name), gcnew String(mFormatCtx->iformat->long_name));
    log->DebugFormat("[Format] - Duration (s): {0}", (double)mFormatCtx->duration / AV_TIME_BASE);
    log->DebugFormat("[Format] - Bit rate (bit/s): {0}", mFormatCtx->bit_rate);
    log->DebugFormat("[Format] - Start time (microseconds): {0}", mFormatCtx->start_time);
    log->DebugFormat("[Format] - Start timestamp: {0} ({1})", mVideoInfo.FirstTimeStamp, mTimestampOffset);
    LogStreamList(mFormatCtx);

    AVStream* stream = mFormatCtx->streams[mVideoStreamIndex];
    log->DebugFormat("[Stream] - Duration (frames): {0}", stream->nb_frames);
    log->DebugFormat("[Stream] - Duration (timestamps): {0}", stream->duration == AV_NOPTS_VALUE ? "N/A" : stream->duration.ToString());
    log->DebugFormat("[Stream] - Average framerate: {0}", av_q2d(stream->avg_frame_rate));
    log->DebugFormat("[Stream] - TimeBase: {0}/{1}", stream->time_base.num, stream->time_base.den);
    log->DebugFormat("[Stream] - PTS wrap bits: {0}", stream->pts_wrap_bits);
    log->DebugFormat("[Stream] - Average timestamps per seconds: {0}", mVideoInfo.AverageTimeStampsPerSeconds);

    // Codec
    log->DebugFormat("[Codec] - Name: {0}, id:{1}", gcnew String(mVideoCodecCtx->codec->name), (int)mVideoCodecCtx->codec_id);
    log->DebugFormat("[Codec] - TimeBase: {0}/{1}", mVideoCodecCtx->time_base.num, mVideoCodecCtx->time_base.den);
    log->DebugFormat("[Codec] - Bit rate (bit/s): {0}", mVideoCodecCtx->bit_rate);
    log->DebugFormat("[Codec] - Has B Frames: {0}", mVideoCodecCtx->has_b_frames);
    log->DebugFormat("[Codec] - Width (pixels): {0}", mVideoCodecCtx->width);
    log->DebugFormat("[Codec] - Height (pixels): {0}", mVideoCodecCtx->height);
    log->DebugFormat("[Codec] - Image rotation: {0}", mVideoInfo.OriginalRotation.ToString());

    // Calculated values
    log->DebugFormat("Duration (timestamps): {0}", mVideoInfo.DurationTimeStamps);
    log->DebugFormat("Average Fps: {0}", mVideoInfo.FramesPerSeconds);
    log->DebugFormat("Average Frame Interval (ms): {0}", mVideoInfo.FrameIntervalMilliseconds);
    log->DebugFormat("Average Timestamps per frame: {0}", mVideoInfo.AverageTimeStampsPerFrame);
    log->DebugFormat("Pixel Aspect Ratio: {0:0.000}", mVideoInfo.PixelAspectRatio);
    log->DebugFormat("---------------------------------------------------");
}

void VideoReaderFFMpeg::LogPacketInfo(AVPacket* packet)
{
    log->DebugFormat("Packet info. Stream index:{0}, DTS:{1}, PTS:{2}, Flags:{3}, Duration:{4}, Byte pos:{5}.", 
        packet->stream_index, 
        packet->dts,                // Time at which the packet is decompressed.
        packet->pts,                // the time at which the decompressed packet will be presented to the user.
                                    // Can be AV_NOPTS_VALUE if it is not stored in the file.
                                    // pts MUST be larger or equal to dts as presentation cannot happen before decompression.
        packet->flags,              // combination of AV_PKT_FLAG values
        packet->duration,           // Duration of this packet. equals (next_pts - this_pts).
        packet->pos);               // Byte position in stream.
}

void VideoReaderFFMpeg::LogFrameInfo(AVFrame* frame)
{
    log->DebugFormat("Frame info. Type:{0}, Format:{1}, PTS:{2}, Packet DTS:{3}, BETS:{4}, Dur:{5}, Flags:{6}, Size:{7}x{8} px.",
        GetFrameTypeString(frame->pict_type),
        GetFrameFormatString((AVPixelFormat)frame->format),
        frame->pts,
        frame->pkt_dts,
        frame->best_effort_timestamp,
        frame->duration,
        frame->flags,
        frame->width,
        frame->height);

    int metadataCount = av_dict_count(frame->metadata);
    if (metadataCount > 0)
    {
        log->DebugFormat("\tMetadata");
        const AVDictionaryEntry* e = nullptr;
        while ((e = av_dict_iterate(frame->metadata, e))) 
        {
            log->DebugFormat("\t\t{0} = {1}", gcnew String(e->key), gcnew String(e->value));
        }
    }
}

void VideoReaderFFMpeg::LogFFMpegError(String^ context, int errorCode)
{
    char errbuf[AV_ERROR_MAX_STRING_SIZE] = { 0 };
    av_strerror(errorCode, errbuf, AV_ERROR_MAX_STRING_SIZE);
    log->ErrorFormat("{0}. Error:{1}: {2}", context, errorCode, gcnew String(errbuf));
}

void VideoReaderFFMpeg::LogStreamList(AVFormatContext* formatCtx)
{
    log->Debug("[Format] - Number of streams: " + formatCtx->nb_streams);

    for (int i = 0; i<(int)formatCtx->nb_streams; i++)
    {
        String^ streamType;

        switch ((int)formatCtx->streams[i]->codecpar->codec_type)
        {
        case AVMEDIA_TYPE_VIDEO:
            streamType = "AVMEDIA_TYPE_VIDEO";
            break;
        case AVMEDIA_TYPE_AUDIO:
            streamType = "AVMEDIA_TYPE_AUDIO";
            break;
        case AVMEDIA_TYPE_DATA:
            streamType = "AVMEDIA_TYPE_DATA";
            break;
        case AVMEDIA_TYPE_SUBTITLE:
            streamType = "AVMEDIA_TYPE_SUBTITLE";
            break;
        case AVMEDIA_TYPE_UNKNOWN:
        default:
            streamType = "AVMEDIA_TYPE_UNKNOWN";
            break;
        }

        log->DebugFormat("\tStream #{0}: {1}, {2} frames.", i, streamType, formatCtx->streams[i]->nb_frames);
    }
}

void VideoReaderFFMpeg::LogVideoGeometry(VideoGeometry^ geometry)
{
    log->DebugFormat("Video geometry resolved: Reference size: {0}x{1}, Output size: {2}x{3}, Decoding scale: {4:0.000}.",
        geometry->ReferenceSize.Width, geometry->ReferenceSize.Height,
        geometry->OutputSize.Width, geometry->OutputSize.Height,
        geometry->Scale);
}


String^ VideoReaderFFMpeg::GetFrameTypeString(int type)
{
    switch (type)
    {
    case AV_PICTURE_TYPE_I:
        return "I-Frame";
    case AV_PICTURE_TYPE_P:
        return "P-Frame";
    case AV_PICTURE_TYPE_B:
        return "B-Frame";
    case AV_PICTURE_TYPE_S:
        return "S(GMC)-VOP MPEG4";
    case AV_PICTURE_TYPE_SI:
        return "Switching Intra";
    case AV_PICTURE_TYPE_SP:
        return "Switching Predicted";
    case AV_PICTURE_TYPE_BI:
        return "FF_BI_TYPE";
    default:
        return "Unknown";
    }
}

String^ VideoReaderFFMpeg::GetFrameFormatString(AVPixelFormat format)
{
    switch (format)
    {
    case AV_PIX_FMT_YUV420P:
        return "AV_PIX_FMT_YUV420P";
    case AV_PIX_FMT_RGB24:
        return "AV_PIX_FMT_RGB24";
    case AV_PIX_FMT_YUV411P:
        return "AV_PIX_FMT_YUV411P";
    default:
        return ((int)format).ToString();
    }
}
#pragma endregion