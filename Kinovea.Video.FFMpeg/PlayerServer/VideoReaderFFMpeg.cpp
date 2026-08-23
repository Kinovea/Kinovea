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
    mVideoInfo = VideoInfo::Empty;
    mWorkingZone = VideoSection::MakeEmpty();
    mCachedTimestamp = AV_NOPTS_VALUE;
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

    mPreBuffer->Tolerance = mVideoInfo.AverageTimeStampsPerFrame / 2.0;
    mPreBuffer->FarAheadThreshold = mVideoInfo.AverageTimeStampsPerFrame * 50.0;

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
    float scale = mOutputSize.Width / (float)mReferenceSize.Width;
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


void VideoReaderFFMpeg::StartPrebufferingIfNotCaching()
{
    // During loading we have called UpdateWorkingZone already, 
    // but only allowing it to either go full caching or stay in 
    // on-demand mode, because we didn't have a reliable presentation size.
    // 
    // Once we do we come back here, see if we can go from on-demand to prebuffering.
    // If we aren't in full caching mode by now it means either 
    // the wz doesn't fit in memory or this was disallowed, we don't 
    // change this here.

    // Special case: in the case where the initial updating the working zone 
    // triggered a full cache load, but the user cancelled it, 
    // we call this first when the progress bar dialog returns, 
    // and again in the PostLoad_Idle handler.
    // In that case it's normal that we are already prebuffering.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
        {
            log->WarnFormat("StartPrebufferingIfNotCaching called while already pre-buffering");
            return;
        }
        else
        {
            // Something went very wrong.
            mPreBuffer->Clear();
            log->ErrorFormat("Prebuffering thread stopped.");
            mCachingMode = VideoDecodingMode::OnDemand;
        }
    }

    if (mCachingMode == VideoDecodingMode::Caching)
    {
        return;
    }
    
    if (CanPreBuffer)
    {
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    }
}
#pragma endregion

#pragma region Frame requests / Player state updates
bool VideoReaderFFMpeg::MoveNext(bool decodeIfNecessary)
{
    //-----------------------------------------------------
    // This runs in the UI thread.
    //-----------------------------------------------------

    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return false;

    bool moved = false;

    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        mStopwatch->Restart();
        ReadResult res = ReadFrameNext();
        log->DebugFormat("Synchronous MoveNext(): {0} ms.", mStopwatch->ElapsedMilliseconds);
        moved = res == ReadResult::Success;
    }
    else if (mCachingMode == VideoDecodingMode::Caching)
    {
        moved = mCache->MoveBy(1);
    }
    else if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        // TODO:
        //mPreBuffer->AcquireClosest(_skip + 1, _decodeIfNecessary);
        mPreBuffer->AcquireClosest(0);
        moved = true;

        //if (!_decodeIfNecessary || mPreBuffer->HasNext(_skip))
        //{
        //    mPreBuffer->MoveBy(_skip + 1);
        //    moved = true;
        //}
        //else
        //{
        //    // Stop thread, decode frame, move to it, restart thread.
        //    log->DebugFormat("MoveNext, stopping pre-buffering.");
        //    StopPreBufferingThread();
        //    ReadResult res = ReadFrameSeek(-1, _skip + 1);
        //    if (res == ReadResult::Success)
        //        moved = mPreBuffer->MoveBy(_skip + 1);
        //    StartPreBufferingThread();
        //}
    }

    return moved && HasMoreFrames();
}

bool VideoReaderFFMpeg::MoveTo(int64_t target)
{
    //-----------------------------------------------------
    // This runs in the UI thread.
    //-----------------------------------------------------

    // The player is asking for a frame to present.

    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return false;

    bool moved = false;
    //target = MapTimestamp(target);

    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        return MoveOnDemand(target);
    }
    else if (mCachingMode == VideoDecodingMode::Caching)
    {
        return MoveCaching(target);
    }
    else if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        if (mPreBuffer->Count == 0)
        {
            // This should never happen. We synchronously decode the first frame
            // of the working zone before starting the thread.
            log->ErrorFormat("MoveTo([{0}]): empty prebuffer.", target);
            return false;
        }

        // The cache is possibly sparse, get whatever is closest.
        log->DebugFormat("Player requests presentation of [~{0}].", target);
        mPreBuffer->AcquireClosest(target);
        return true;
    }
}


bool VideoReaderFFMpeg::PlayerRequest(PlayerState^ newState)
{
    // For now we only support this for prebuffering mode
    // while the refactoring is in progress.

    if (mCachingMode != VideoDecodingMode::PreBuffering)
    {
        throw gcnew InvalidProgramException("PlayerDemand() called while not prebuffering.");
    }

    CachePreparationResult^ result = nullptr;

    // Convert the player request into a decode job.
    // If the decoder thread is not currently blocked in Add(), 
    // it will eventually see this and abandon its current job.
    // It will then wait until this new job is ready to be processed.
    Volatile::Write(requestedPlayerState, newState);
    log->DebugFormat("Published decode job {0}", requestedPlayerState);

    // Prepare the cache for the new state.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        // This will interrupt Add() if the decoder thread is waiting there.
        log->DebugFormat("Preparing prebuffer for decode job {0} Tolerance: [{1:0.000}]", 
            requestedPlayerState, mPreBuffer->Tolerance);

        mPreBuffer->Print();

        result = mPreBuffer->PrepareForNewJob(requestedPlayerState);

        Volatile::Write(mPreBufferPreparation, result);
    }
    else
    {
        // Nothing to do.
    }

    // The new job is now ready to be processed.
    lock l(mLockNewJobReady);
    {
        log->DebugFormat("Marking job #{0} as ready.", requestedPlayerState->Id);
        mReadyJobId = requestedPlayerState->Id;

        // Wake up the decoder thread if it was waiting in WaitForReadyJob().
        Monitor::PulseAll(mLockNewJobReady);
        l.release();
    }

    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        // Our work is done here.
        // The prebuffer should be working on the new job by now.
        return result->TargetAcquired;
    }

    // TODO: For other modes handle the job synchronously.

    return result->TargetAcquired;
}




bool VideoReaderFFMpeg::MoveOnDemand(int64_t target)
{
    if (!mSingleFrameContainer->IsEmpty && 
        mSingleFrameContainer->CurrentFrame->Timestamp == target)
    {
        return true;
    }
    else
    {
        // Synchronous read of the requested frame.
        // The ReadFrameSeek will call `store` on the single-frame frame container
        // which will set the `Current` property to the requested frame.
        ReadResult res = ReadFrameSeek(target);
        return (res == ReadResult::Success);
    }
}


bool VideoReaderFFMpeg::MoveCaching(int64_t target)
{
    if (mCache->Empty)
    {
        // In theory the UI shouldn't ask for a frame until the modal progress bar 
        // of the caching operation is closed so we should always have a frame.
        return false;
    }
    else
    {
        // Acquire the requested frame.
        // TODO: the input target is in UI space, we should ask with tolerance.
        return mCache->MoveTo(target);
    }
}
#pragma endregion

#pragma region Decoding mode, play loop and frame enumeration
void VideoReaderFFMpeg::BeforePlayloop()
{
    // Just in case something wrong happened, make sure the decoding thread is alive.
    // FIXME: this is just StartPrebufferingIfNotCaching().

    if (DecodingMode == VideoDecodingMode::Caching)
    {
        // All set.
        return;
    }

    if (CanPreBuffer && DecodingMode != VideoDecodingMode::PreBuffering)
    {
        log->Error("Forcing PreBuffering thread to restart.");
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    }
}

void VideoReaderFFMpeg::UpdateWorkingZone(
    VideoSection newZone, 
    CacheLoadMode loadMode,
    int maxMemory, 
    Action<DoWorkEventHandler^>^ workerFn)
{
    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
    {
        return;
    }

    if (!CanChangeWorkingZone)
    {
        throw gcnew CapabilityNotSupportedException();
    }

    log->DebugFormat("Update working zone request. {0} -> {1}. Cache load mode: {2}, First time: {3}", 
        mWorkingZone, newZone, loadMode, mIsFirstWZUpdate);
    
    // Important: the new zone is coming from pixel values and there is no guarantee that
    // actual frames exists in the video at these values.
    // We must update our internal values according to real timestamps as soon as possible.
    VideoSection oldZone = mWorkingZone;
    mWorkingZone = newZone;
    bool fitsInMemory = WorkingZoneMemoryRequirement(newZone) <= maxMemory;

    log->DebugFormat("New working zone fits: {0}.", fitsInMemory);

    if (mIsFirstWZUpdate)
    {
        FirstUpdateWorkingZone(newZone, loadMode, fitsInMemory, workerFn);
        mIsFirstWZUpdate = false;
        return;
    }

    if (fitsInMemory)
    {
        
        // FIXME:
        // We should read the first frame of the wz synchronously before returning.
        // This way the UI can present it immediately, like for prebuffering.
        // If we switch mode we are invalidating the old container, the UI
        // won't have anything to show until we have read a frame again, even if 
        // it's the same we already had.

        if (mCachingMode != VideoDecodingMode::Caching)
        {
            // Change mode (including clearing of the existing container) and load the cache.
            // Force reload is irrelevant here, we always need to reload.
            ChangeCachingMode(VideoDecodingMode::Caching);
            mSectionToPrepend = newZone;
            mSectionToAppend = VideoSection::MakeEmpty();
            DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
            workerFn(workHandler);
        }
        else
        {
            // We are already in Caching mode.
            mSectionToPrepend = VideoSection::MakeEmpty();
            mSectionToAppend = VideoSection::MakeEmpty();
            
            if (loadMode == CacheLoadMode::Reload)
            {
                mFrameContainer->Clear();
                mSectionToPrepend = newZone;
                DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
                workerFn(workHandler);
            }
            else
            {
                // Reset to the old zone and update it on the fly.
                mWorkingZone = oldZone;

                // Trim left.
                if (newZone.Start > mWorkingZone.Start)
                {
                    // Only do it if the new start is at least one frame beyond the old one.
                    if (newZone.Start - mWorkingZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                    {
                        mCache->ReduceWorkingZone(VideoSection(newZone.Start, mWorkingZone.End));
                        mWorkingZone = mCache->WorkingZone;
                        log->DebugFormat("Reduced cache from the front: {0}.", mWorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    newZone = VideoSection(mWorkingZone.Start, newZone.End);
                }

                // Trim right.
                if (newZone.End < mWorkingZone.End)
                {
                    // Only do it if the new end is at least one frame before the old one.
                    if (mWorkingZone.End - newZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                    {
                        mCache->ReduceWorkingZone(VideoSection(mWorkingZone.Start, newZone.End));
                        mWorkingZone = mCache->WorkingZone;
                        log->DebugFormat("Reduced cache from the back: {0}.", mWorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    newZone = VideoSection(newZone.Start, mWorkingZone.End);
                }

                // Bail out if this was purely a trimming job.
                if (mWorkingZone.Start == newZone.Start && mWorkingZone.End == newZone.End)
                {
                    return;
                }

                // Expand left.
                if (mWorkingZone.Start - newZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    mSectionToPrepend = VideoSection(newZone.Start, mWorkingZone.Start);
                }

                // Expand right.
                if (newZone.End - mWorkingZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    mSectionToAppend = VideoSection(mWorkingZone.End, newZone.End);
                }

                if (!mSectionToPrepend.IsEmpty || !mSectionToAppend.IsEmpty)
                {
                    DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
                    workerFn(workHandler);
                }
            }
        }
    }
    else
    {
        // The working zone doesn't fit in memory.
        // Change to the next best thing.
        if (mCachingMode == VideoDecodingMode::PreBuffering)
        {
            StopPreBufferingThread();
            mPreBuffer->Clear();
            StartPreBufferingThread(mWorkingZone.Start);
        }
        else
        {
            ChangeToBestAfterCaching();
        }
    }
}

void VideoReaderFFMpeg::FirstUpdateWorkingZone(
    VideoSection newZone, 
    CacheLoadMode loadMode,
    bool fitsInMemory, 
    Action<DoWorkEventHandler^>^ workerFn)
{
    if (mCachingMode != VideoDecodingMode::OnDemand)
    {
        throw gcnew InvalidProgramException();
    }

    // This is the very first call to update the working zone.
    // This may be coming from a KVA during loading of the video, 
    // or as an explicit call after the UI is ready.
    
    // We do not start the prebuffering thread here because the 
    // preferred decoding size may not be known reliably.
    // The UI will later make an explicit call to set the preferred size
    // and start the thread.

    if (loadMode == CacheLoadMode::DoNotLoad)
    {
        // This happens when the player wants to prevent the wait of loading 
        // the frames into the cache, for example for replay observers.
        // We stay in on-demand mode.
        return;
    }

    if (!fitsInMemory)
    {
        // Stay in on-demand mode.
        return;
    }

    // Change mode and load the cache.
    ChangeCachingMode(VideoDecodingMode::Caching);
    mSectionToPrepend = newZone;
    mSectionToAppend = VideoSection::MakeEmpty();
    DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
    workerFn(workHandler);
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

    if (newMode == mCachingMode)
    {
        return;
    }

    if (mVerbose)
    {
        log->DebugFormat("Changing caching mode: {0} -> {1}", mCachingMode, newMode);
    }

    // Clear the existing cache.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        StopPreBufferingThread();
        mFrameContainer->Clear();
    }
    else if (mFrameContainer != nullptr)
    {
        mFrameContainer->Clear();
    }

    mCachingMode = newMode;

    // Change container.
    switch (mCachingMode)
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

    // Recompute the geometry, to take into account a possible
    // change in allowPrescaling.
    ResolveGeometry(mVideoGeometryRequest);

    // For OnDemand, the frame will be read synchronously later.
    // For Caching, the caller should launch a background worker
    // to fill the cache.
    // For PreBuffering, we read the first frame and start the background thread.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        StartPreBufferingThread(mWorkingZone.Start);
    }
}

void VideoReaderFFMpeg::ChangeToBestAfterCaching()
{
    // If we cannot enter Caching mode, switch to the next best thing.
    if (CanPreBuffer && !mWorkingZone.IsEmpty)
    {
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    }
    else if (CanDecodeOnDemand)
    {
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
    else
    {
        throw gcnew CapabilityNotSupportedException();
    }
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
        mOutputSize = FitHelper::Fit(mReferenceSize, request->PresentationSize, false);
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
    bool isPrescaled = mOutputSize == request->PresentationSize;

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

    log->DebugFormat("Video geometry resolved: Original size: {0}x{1}, Scaled size:{2}x{3}.",
        mOriginalSize.Width, mOriginalSize.Height,
        mScaledSize.Width, mScaledSize.Height);

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
        success = ReadManyToCache(worker, mSectionToPrepend, true);
    }

    if (success && !mSectionToAppend.IsEmpty)
    {
        success = ReadManyToCache(worker, mSectionToAppend, false);
    }

    if (!success)
    {
        // Switch back to on-demand.
        // The UI is responsible for switching to prebuffering.
        // If this is running during the initial load we may not have set 
        // a reliable preferred size yet, but the UI side will be able to do 
        // it cancellation handling.
        // The first frame will be read again in the process of starting 
        // the prebuffer thread.
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
}

bool VideoReaderFFMpeg::ReadManyToCache(BackgroundWorker^ bgWorker, VideoSection section, bool isPrepend)
{
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
    
    if (mVerbose)
    {
        log->DebugFormat("Requested section to cache: {0}. Prepend:{1}", section, isPrepend);
    }

    mCache->SetPrepending(isPrepend);

    // Realign the requested section on real timestamps.
    if (!mCache->WorkingZone.IsEmpty)
    {
        if (isPrepend && (mCache->WorkingZone.Start - section.Start < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // Start target is less than one frame before the current start.
            section = VideoSection(mCache->WorkingZone.Start, section.End);
        }
        else if (!isPrepend && (section.End - mCache->WorkingZone.End < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // End target is less than one frame after the current end.
            section = VideoSection(section.Start, mCache->WorkingZone.End);
        }

        log->DebugFormat("Aligned requested section to cache: {0}", section);
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
    log->DebugFormat("Frames to cache: {0} (avg ts/f: {1}).", totalFrames, mVideoInfo.AverageTimeStampsPerFrame);

    Stopwatch^ stopwatchCaching = Stopwatch::StartNew();

    // Seek to first frame.
    int read = 0;
    bool success = true;
    ReadResult res = ReadFrameSeek(section.Start);
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

    log->DebugFormat("ReadManyToCache. Average: {0:0.000} ms.", mLoopWatcher->Average);

    // Update the working zone with real values.
    // The request may have been an approximation from pixel mapping.
    if (!bgWorker->CancellationPending)
    {
        mWorkingZone = mCache->WorkingZone;
    }

    mCache->SetPrepending(false);
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

    mDecodedTimestamp = frame->best_effort_timestamp;

    result = ConvertAndStoreFrame(frame, true);
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
    
    result = ConvertAndStoreFrame(frame, false);
    av_frame_free(&frame);
    return result;
}


ReadResult VideoReaderFFMpeg::ReadFrameSeek(int64_t targetTimestamp)
{
    mStopwatch->Restart();

    if (!mIsLoaded || 
        mCachingMode == VideoDecodingMode::NotInitialized || 
        mFrameContainer == nullptr ||
        mVideoGeometry == nullptr ||
        mVideoGeometry->OutputSize.IsEmpty)
    {
        return ReadResult::NotReady;
    }

    ReadResult result = ReadResult::UnknownError;

    // This is used for both seeking and relative jump.
    int	framesToDecode = 1;
    int framesDecoded = 0;
    int res = 0;

    // It's possible to get here with a target equal to that of the acquired frame.
    // For example when we change the output size (? this should invalidate everything).
    // We have to seek back to the start of the GOP and decode many frames again.

    // Initial seek.
    // This should land us at the start of the GOP containing the target.
    // Note that even if the seek target is in the current GOP we go through the 
    // seeking call and reset the libav internal buffers, because we can't know it beforehand.
    //log->DebugFormat("Seeking to {0}", targetTimestamp);
    res = SeekTo(targetTimestamp);
    if (res < 0)
    {
        LogFFMpegError("SeekTo", res);
        log->ErrorFormat("Error trying to seek to: [~{1}]", targetTimestamp);
        return ReadResult::UnknownError;
    }

    // Get the first frame after the seek.
    mLoopWatcher->LoopStart();
    AVFrame* frame = av_frame_alloc();
    result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
    mLoopWatcher->LoopEnd();

    //log->DebugFormat("ReadFrameSeek. Decoded frame at [{0}]. Frame type: {1}", frame->best_effort_timestamp, GetFrameTypeString(frame->pict_type));
    if (result != ReadResult::Success)
    {
        av_frame_free(&frame);
        return result;
    }

    if (HasJobChanged())
    {
        // FIXME: we should keep the decoded frame as pending here.
        // The next job might be able to just restart from there.
        log->DebugFormat("ReadFrameSeek. Job changed during decoding. Abandoning.");
        av_frame_free(&frame);
        return ReadResult::NewJob;
    }

    framesDecoded = 1;

    // If seeking landed beyond the target log it but don't fail. 
    // It might happen if the very first packet is not a keyframe, 
    // possibly from cut-off stream or corrupted file.
    if (frame->best_effort_timestamp > targetTimestamp)
    {
        log->WarnFormat("Seek to [~{0}] landed at [{1}]. Frame type: {2}",
            targetTimestamp,
            frame->best_effort_timestamp,
            GetFrameTypeString(frame->pict_type));
    }
    
    // At this point we have decoded one frame.
    // Depending on the call we may be done or need to keep decoding.
    mDecodedTimestamp = frame->best_effort_timestamp;

    //log->DebugFormat("Decoded frame [{0}]. {1} ms.", mDecodedTimestamp, mStopwatch->ElapsedMilliseconds);

     
    // Check if the initial decode is already at or after the seek target.
    if (mDecodedTimestamp >= targetTimestamp)
    {
        log->DebugFormat("Found seek target, decoded {0} frames.", framesDecoded);
        result = ConvertAndStoreFrame(frame, false);
        av_frame_free(&frame);
        return result;
    }

    // Otherwise keep decoding frames until we get to the target, or EOF.
    while (true)
    {
        result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
            
        if (result != ReadResult::Success)
        {
            av_frame_free(&frame);
            return result;
        }

        if (HasJobChanged())
        {
            av_frame_free(&frame);
            return ReadResult::NewJob;
        }

        //LogFrameInfo(frame);
        framesDecoded++;
        mDecodedTimestamp = frame->best_effort_timestamp;

        if (framesDecoded % 10 == 0)
        {
            log->DebugFormat("Seeking towards [~{0}]. Last decoded: [{1}]. Decoded {2} frames.", 
                targetTimestamp, mDecodedTimestamp, framesDecoded);
        }

        if (mDecodedTimestamp >= targetTimestamp)
        {
            log->DebugFormat("Found seek target. [~{0}] -> [{1}]. Decoded {2} frames.", 
                targetTimestamp, mDecodedTimestamp, framesDecoded);

            result = ConvertAndStoreFrame(frame, false);
            av_frame_free(&frame);
            break;
        }

        if (HasJobChanged())
        {
            av_frame_free(&frame);
            return ReadResult::NewJob;
        }

        // Keep decoding.
    }

    return result;
}

bool VideoReaderFFMpeg::ShouldStoreFrame()
{
    if (mCachingMode != VideoDecodingMode::PreBuffering)
        return true;

    if (mWorkingPlayerState->Mode != PlayerStateMode::Playback)
        return true;

    if (mDecodingPolicy != DecodingPolicy::Behind && mDecodingPolicy != DecodingPolicy::FarBehind)
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
    mDecodedTimestamp = AV_NOPTS_VALUE;
    mCurrentGopTimestamp = AV_NOPTS_VALUE;
    return res;
}


ReadResult VideoReaderFFMpeg::ConvertAndStoreFrame(AVFrame* decodedFrame, bool forSummary)
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

    // Construct a VideoFrame.
    VideoFrame^ vf = gcnew VideoFrame();
    vf->Image = bmp;
    vf->Timestamp = mDecodedTimestamp;
    
    // Store it to the active container.
    // If we are in mode on-demand, this is synchronous and will replace the single stored frame.
    // If we are in mode prebuffer, we are in a background thread and this will potentially block if the 
    // cache is full.
    CacheAddResult addResult = mFrameContainer->Add(vf);

    if (addResult == CacheAddResult::Added)
    {
        mCachedTimestamp = mDecodedTimestamp;
    }
    if (addResult == CacheAddResult::Duplicate)
    {
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
    videoFrame->Image = nullptr;

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
        mPreBuffer->Clear();
    }

    // Read the first frame outside the decoding thread so the UI may request it immediately.
    if (startTimestamp >= 0)
    {
        ReadResult res = ReadFrameSeek(startTimestamp);
        if (res == ReadResult::Success)
        {
            log->DebugFormat("Read first frame synchronously. [{0}].", mCachedTimestamp);
            mPreBuffer->AcquireClosest(mCachedTimestamp);
            mWorkingZone = VideoSection(mCachedTimestamp, mWorkingZone.End);
        }
    }

    log->Debug("Starting prebuffering thread.");

    ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &VideoReaderFFMpeg::PreBufferingWorker);
    mPreBufferingThreadCanceler->Reset();
    mPreBuffer->ResetInterruptAdd();
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
    ExecuteDecodingPolicy(DecodingPolicy::Normal);

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
        log->DebugFormat("Woken up by new job or cancellation - after work.");
    }

    log->DebugFormat("Exiting PreBuffering thread.");
}

ReadResult VideoReaderFFMpeg::ProcessJob(ThreadCanceler^ canceller)
{
    
    // The job in itself is essentially just a series of ReadFrameNext().
    // The decoder will have been moved to the right spot during BeginJob().


    log->DebugFormat("PreBuffering thread, processing job #{0} {1} -----------------------", 
        mWorkingPlayerState->Id, mWorkingPlayerState->Mode);

    mPreBuffer->Print();

    ReadResult res = ReadResult::Success;

    while (true)
    {
        if (canceller->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation.");
            break;
        }

        if (HasJobChanged())
        {
            // Abandon work.
            return ReadResult::NewJob;
        }

        if (mWorkingPlayerState->Mode == PlayerStateMode::Playback)
        {
            UpdateDecodePolicy();
        }

        // Read the next frame.
        // This will perform:
        // - decoding
        // - scaling/conversion/deint/rotation
        // - store in the prebuffer cache.
        // Each step can be skipped depending on the policy.
        // If the cache is full this will wait in PreBuffer.Add().
        res = ReadFrameNext();

        // Check if we were cancelled while waiting in Add().
        if (canceller->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation after ReadFrameSeek().");
            break;
        }

        if (HasJobChanged())
        {
            // Abandon work.
            return ReadResult::NewJob;
        }

        if (res == ReadResult::EOFReached || mDecodedTimestamp >= mWorkingZone.End)
        {
            // Return and wait for a new job.
            log->DebugFormat("EOF reached.");
            return ReadResult::EOFReached;
        }
    }

    return ReadResult::ThreadCancelled;
}

bool VideoReaderFFMpeg::HasJobChanged()
{
    PlayerState^ latestPlayerState = Volatile::Read(this->requestedPlayerState);
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

            PlayerState^ state = Volatile::Read(this->requestedPlayerState);

            if (state->Id > currentJobId && mReadyJobId >= state->Id)
            {
                log->DebugFormat("New job detected and ready: {0}", state);
                return state;
            }

            log->DebugFormat("Prebuffer thread, entering wait until a new job is ready. Current: #{0}.", mWorkingPlayerState->Id);

            Monitor::Wait(mLockNewJobReady);

            log->DebugFormat("Woken up by new job ready or cancellation.");
        }

        l.release();
    }

    return nullptr;
}

void VideoReaderFFMpeg::UpdateDecodePolicy()
{

    if (mWorkingPlayerState->Mode != PlayerStateMode::Playback)
        return;

    // Estimate how far behind we are compared to the player.
    // Note: this lag is in presentation space and independent from the speed slider.
    long expectedTimestamp = this->GetPlaybackTimestamp(mWorkingPlayerState);
    double lag = (expectedTimestamp - mCachedTimestamp) / mVideoInfo.AverageTimeStampsPerSeconds;

    String^ logLine = String::Format("UpdateDecodePolicy: estimated player timestamp: [{0}], latest stored: [{1}], Lag: {2:0.000}s, Cache: {3}/{4}. Frame: {5}.",
        expectedTimestamp, mCachedTimestamp, lag, mPreBuffer->Count, mPreBuffer->Capacity, mDecodedFrames);

    log->DebugFormat(logLine);
            
    // Check worst case scenario first.
    if (lag > mSeekAheadLagThreshold)
    {
        log->ErrorFormat("Lag of {0:0.000}s is over seek-ahead threshold. Decoded frames: {1}. Cache: {2}/{3}.", 
            lag, mDecodedFrames, mPreBuffer->Count, mPreBuffer->Capacity);
            
        // We are hopelessly behind, try to seek ahead.
        // Note: calling seek directly with min, target, max, doesn't always work.
        // First, some decoders just fallback to AVSEEK_FLAG_BACKWARD which moves us 
        // back in time and is worse than doing nothing.
        // Secondly, seek is done in a different "domain" than the timestamps we get from decoding frames.
        // The values of packet->pts don't always match the values of frame->best_effort_timestamp. 
        // 
        // As decoding progresses we keep track of the timestamps of key frame packets and use that 
        // to check if seek ahead is actually possible.
        //
        // Poll the index for the closest keyframe before the target timestamp.
        AVStream* stream = mFormatCtx->streams[mVideoStreamIndex];
        const AVIndexEntry* entry = avformat_index_get_entry_from_timestamp(stream, expectedTimestamp, AVSEEK_FLAG_BACKWARD);
        if (entry != nullptr)
        {
            //log->DebugFormat("Found index entry for timestamp [{0}]. Key at [{1}].", expectedTimestamp, entry->timestamp);
                
            // Only seek if the keyframe is ahead of the current GOP start. 
            // Otherwise we would just seeking back in time.
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
        }
    }
    else
    {
        // Figure out the decoding policy we should apply.
        // Different thresholds for entering and leaving the levels to avoid oscillations.

        // Behind mode: drop the scale/convert work for frames that aren't presented.
        // Only switch back when we are comfortably ahead.
        double thresholdEnterBehind = 0.000;
        double thresholdLeaveBehind = -0.200;

        // Far behind mode: drop decoding of B-frames at ffmpeg level.
        // This only makes a difference for files that have B-frames in the first place.
        double thresholdEnterFarBehind = 0.400;
        double thresholdLeaveFarBehind = 0.200;
                
        DecodingPolicy oldPolicy = mDecodingPolicy;
        switch (mDecodingPolicy)
        {
        case DecodingPolicy::Normal:
            if (lag > thresholdEnterFarBehind)
            {
                ExecuteDecodingPolicy(DecodingPolicy::FarBehind);
            }
            else if (lag > thresholdEnterBehind)
            {
                ExecuteDecodingPolicy(DecodingPolicy::Behind);
            }
            break;
        case DecodingPolicy::Behind:
            if (lag > thresholdEnterFarBehind)
            {
                ExecuteDecodingPolicy(DecodingPolicy::FarBehind);
            }
            else if (lag < thresholdLeaveBehind)
            {
                ExecuteDecodingPolicy(DecodingPolicy::Normal);
            }
            break;
        case DecodingPolicy::FarBehind:
            if (lag < thresholdLeaveFarBehind)
            {
                ExecuteDecodingPolicy(DecodingPolicy::Behind);
            }
            break;
        }
    }
}

void VideoReaderFFMpeg::ExecuteDecodingPolicy(DecodingPolicy policy)
{
    if (policy == mDecodingPolicy)
        return;

    log->DebugFormat("Updating decoding policy: {0} -> {1}.", mDecodingPolicy, policy);
    mDecodingPolicy = policy;

    // Note: we can change mVideoCodecCtx->skip_frame at any time 
    // no need to close and re-open the codec.

    switch (policy)
    {
    case DecodingPolicy::Normal:
    case DecodingPolicy::Behind:
        mVideoCodecCtx->skip_frame = AVDISCARD_DEFAULT;
        break;

    case DecodingPolicy::FarBehind:
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

    log->DebugFormat("InitDecodingJob. Initializing for job {0}", mWorkingPlayerState);
    log->DebugFormat("InitDecodingJob. Last decoded: [{0}]. Last stored: [{1}].", mDecodedTimestamp, mCachedTimestamp);

    // By this point we have abandonned all work from the old job,
    // the reader has finished preparing the cache for the new job.
    // We can now officially start working on the new job.

    // Make sure the cache accepts frames again.
    // This was interrupted while tearing the old job down.
    mPreBuffer->ResetInterruptAdd();

    // Reset decoding policy.
    ExecuteDecodingPolicy(DecodingPolicy::Normal);

    // Get the state of the cache from the preparation.
    CachePreparationResult^ cachePrepResult = Volatile::Read(this->mPreBufferPreparation);

    // Get and execute the plan that moves the decoder to the best spot to continue 
    // decoding or to fullfil the immediate request if it wasn't already acquired during prep.
    DecodingJobPlan^ plan = GetDecodingJobPlan(state, cachePrepResult);
    log->Debug(plan);
    bool acquiredDuringInit = ExecuteDecodingJobPlan(state, plan);
    if (!plan->TargetIsResolved && acquiredDuringInit)
    {
        log->DebugFormat("Target acquired during execution of the decoding job plan.");
        OnFrameAcquired(state);
    }

    // At this point the decoder can just start decoding frames forward until EOF or cancellation.
}

DecodingJobPlan^ VideoReaderFFMpeg::GetDecodingJobPlan(PlayerState^ state, CachePreparationResult^ cachePrepResult)
{
    //-----------------------------------------------------------------
    // The goal of this function is to find where we should move the decoder to:
    // 1. fullfil the immediate request if it wasn't acquired yet
    // 2. continue decoding from there.
    //
    // Just because the request was already acquired doesn't mean the decoder is at 
    // the right spot to continue.
    // 
    // Once the immediate request is fullfilled and the decoder relocated, the "job" will 
    // simply continue decoding frame by frame forward until EOF or cancellation.
    // 
    // By now the cache has already been reshaped to accomodate the new request.
    // 
    // Important: when in doubt we can always fall back to seeking to the target and 
    // restart decoding from there.
    // Everything else is optimization, so we identify a few known scenarios 
    // rather than try to handle all combinations of states.
    // If we can't prove an optimization is safe we should seek.
    //-----------------------------------------------------------------

    // Grab the results of the initial request handling from the cache side.
    bool isAcquired = cachePrepResult->TargetAcquired;
    int64_t acquiredTimestamp = cachePrepResult->AcquiredTimestamp;
    bool denseForward = cachePrepResult->DenseForward;
    int64_t denseEnd = cachePrepResult->DenseEndTimestamp;
    int64_t cacheStart = cachePrepResult->CacheStartTimestamp;
    int64_t cacheEnd = cachePrepResult->CacheEndTimestamp;
    bool cacheFull = cachePrepResult->Full;

    // Shared variables by all scenarios.
    int64_t requestedTimestamp = state->ReferenceTimestamp;
    int64_t targetTimestamp = isAcquired ? acquiredTimestamp : requestedTimestamp;

    // Failsafe scenario is to just seek and restart decoding,
    // unless we can prove we have a better thing to do.
    DecodingJobPlan^ plan = gcnew DecodingJobPlan();
    plan->RequestedTimestamp = requestedTimestamp;
    plan->TargetTimestamp = targetTimestamp;
    plan->TargetIsResolved = isAcquired;
    plan->DecoderInitAction = DecoderInitAction::Seek;
    plan->ResubmitPending = false;
    

    if (mDecodedTimestamp < 0)
    {
        // The decoder location is not known.
        // This may happen for example if we start a seek and get interrupted 
        // with a new job before decoding even the first frame.
        log->DebugFormat("Decoder is nowhere: seeking.");
        plan->DecoderInitAction = DecoderInitAction::Seek;
        return plan;
    }

    //---------------------------------------------
    // This can be further simplified.
    // Probably we only need to consider the furthest ahead frame between
    // pending, last decode and last stored.
    // First check if it's outside the cache.
    // Then compare with the player request/acquired target.
    //---------------------------------------------

    bool hasPending = mPendingFrame != nullptr;
    bool pendingIsNext = IsPendingNext(cacheEnd);

    
    if (hasPending && !pendingIsNext)
    {
        log->DebugFormat("Discarding pending frame [{0}].", mPendingFrame->Timestamp);
        DisposeFrame(mPendingFrame);
        mPendingFrame = nullptr;
        hasPending = false;
    }


    if (pendingIsNext)
    {
        if (isAcquired)
        {
           plan->DecoderInitAction = DecoderInitAction::None;
           plan->ResubmitPending = true;
           return plan;
        }
        else
        {
            // Check if target is next.
            TimestampRelation relTarget = RelateTimestamps(mPendingFrame->Timestamp, targetTimestamp);

            if (relTarget == TimestampRelation::Match)
            {
                plan->DecoderInitAction = DecoderInitAction::None;
                plan->ResubmitPending = true;
                return plan;
            }
        }
    }


    if (hasPending && !plan->ResubmitPending)
    {
        log->DebugFormat("Discarding pending frame [{0}].", mPendingFrame->Timestamp);
        DisposeFrame(mPendingFrame);
        mPendingFrame = nullptr;
        hasPending = false;
    }
 

    // Check if the target is completely outside the cache.

    CacheTimestampRelation relCache = mPreBuffer->RelateTimestamp(targetTimestamp);

    if (relCache == CacheTimestampRelation::Behind)
    {
        log->DebugFormat("Target before prebuffer cache: seeking.");
        plan->DecoderInitAction = DecoderInitAction::Seek;
        return plan;
    }
    else if (relCache == CacheTimestampRelation::Ahead)
    {
        // This should be Advance.
        log->DebugFormat("Target ahead of prebuffer: seeking.");
        plan->DecoderInitAction = DecoderInitAction::Seek;
        return plan;
    }
    else if (relCache == CacheTimestampRelation::FarAhead)
    {
        log->DebugFormat("Target far ahead of prebuffer: seeking.");
        plan->DecoderInitAction = DecoderInitAction::Seek;
        return plan;
    }


    // The target is within the bounds of the cache.
    // Whether the target was acquired or not, for now we use the same rules.
    // Compare where the decoder is with respect to the target.

    TimestampRelation relTarget = isAcquired ?
        RelateTimestamps(mDecodedTimestamp, acquiredTimestamp) :
        RelateTimestamps(mDecodedTimestamp, targetTimestamp);

    // TODO:
    // What we really want to know is whether the decoder is in a place that is 
    // densely bridged with the target (for dense jobs), or is loosely bridged 
    // with the target (for playback jobs).

    if (relTarget == TimestampRelation::Match)
    {
        log->DebugFormat("Decoder matches player.");
        plan->DecoderInitAction = DecoderInitAction::None;
        return plan;
    }
    else if (relTarget == TimestampRelation::Behind)
    {
        // We need to advance the decoder to at least the acquired frame.
        // This should be advance.
        log->DebugFormat("Decoder is behind player: seeking. TODO: advance.");
        plan->DecoderInitAction = DecoderInitAction::Seek;
        return plan;
    }
    else if (relTarget == TimestampRelation::Ahead)
    {
        // Decoder is ahead, good.
        // TODO: this should be more restricted in the future for dense jobs.
        log->DebugFormat("Decoder is ahead of player.");
        plan->DecoderInitAction = DecoderInitAction::None;
        return plan;
    }
    else if (relTarget == TimestampRelation::FarAhead)
    {
        // Decoder is far ahead, moving backwards?
        log->DebugFormat("Decoder is far ahead of player.");
        plan->DecoderInitAction = DecoderInitAction::None;
        return plan;
    }

    return plan;
}

bool VideoReaderFFMpeg::IsPendingNext(int64_t cacheEnd)
{
    // Test if the previousTimestamp of the pending frame matches the last frame of the cache.
    
    // Note: this doesn't check if the cache has gaps between the target and the last frame.

    if (mPendingFrame == nullptr)
        return false;

    // Must be the last decoded frame so the decoder can resume from there.
    if (mPendingFrame->Timestamp != mDecodedTimestamp)
        return false;

    // FIXME: we don't store the previousTimestamp yet.
    // The true test is that mPendingFrame->PreviousTimestamp == mCachedTimestamp == cacheEnd.
    if (mPendingFrame->Timestamp > cacheEnd)
    {
        return true;
    }

    return false;
}


bool VideoReaderFFMpeg::ExecuteDecodingJobPlan(PlayerState^ state, DecodingJobPlan^ plan)
{
    // Initialize the decoder according to the plan.

    // For now we don't implement advance option.
    if (plan->DecoderInitAction == DecoderInitAction::Seek)
    {
        log->DebugFormat("ExecuteDecodingJobPlan: seeking to [~{0}].", plan->TargetTimestamp);

        // Seek and decode until the target.
        // TODO: have another argument to tell if we should store the intermediate frames.
        ReadResult res = ReadFrameSeek(plan->TargetTimestamp);
        if (res == ReadResult::Success)
        {
            mPreBuffer->AcquireClosest(plan->TargetTimestamp);
            return true;
        }
    }
    else if (plan->DecoderInitAction == DecoderInitAction::Advance)
    {
        // Decode until we reach the target, without seek.
        // TODO.
        // Should store and acquire the frames as we go.
        // The goal is just to move a little bit ahead, without seek to avoid 
        // restarting from the GOP start.
        return false;
    }
    else if (plan->DecoderInitAction == DecoderInitAction::None)
    {
        // Do not move, the decoder is already in the right spot.
        
        if (!plan->ResubmitPending || mPendingFrame == nullptr)
        {
            if (mCachedTimestamp > 0)
            {
                // Target may have been decoded/stored while the job was in preparation.
                TimestampRelation relTarget = RelateTimestamps(mCachedTimestamp, plan->TargetTimestamp);

                if (relTarget == TimestampRelation::Match)
                {
                    mPreBuffer->AcquireClosest(plan->TargetTimestamp);
                    return true;
                }
            }

            // Target not acquired.
            return false;
        }

        // We have a pending frame that is interesting to resubmit.
        // Normally this only happens when the target was acquired or 
        // the pending frame was itself the target.
        // 
        // This may immediately block in add() again, but that's ok,
        // we'll wake up when the player moves forward or when a new job arrives.
        CacheAddResult result = mPreBuffer->Add(mPendingFrame);

        if (result == CacheAddResult::Added)
        {
            // Update last cached frame.
            mCachedTimestamp = mPendingFrame->Timestamp;
            mPendingFrame = nullptr;

            TimestampRelation relTarget = RelateTimestamps(mCachedTimestamp, plan->TargetTimestamp);
            if (relTarget == TimestampRelation::Match)
            {
                // The pending frame was the requested frame.
                // This can happen when we step forward fast enough.
                // The new request can come while we are decoding/storing it.
                mPreBuffer->AcquireClosest(plan->TargetTimestamp);
                return true;
            }
        }
        else if (result == CacheAddResult::Duplicate)
        {
            DisposeFrame(mPendingFrame);
            mPendingFrame = nullptr;
            return false;
        }
        else if (result == CacheAddResult::Interrupted)
        {
            // Interrupted again.
            // Then the frame stays in pending state.
            log->DebugFormat("ExecuteDecodingJobPlan: [{0}] is still pending.", mPendingFrame->Timestamp);
            return false;
        }

        return false;
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