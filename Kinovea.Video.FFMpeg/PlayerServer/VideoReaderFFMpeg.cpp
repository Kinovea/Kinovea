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
using namespace System::Threading;
using namespace msclr;

using namespace Kinovea::Services;
using namespace Kinovea::Video::FFMpeg;

#pragma region Construction/Destruction
VideoReaderFFMpeg::VideoReaderFFMpeg()
{
    m_Locker = gcnew Object();
    m_PreBufferingThreadCanceler = gcnew ThreadCanceler();

    VideoFrameDisposer^ disposer = gcnew VideoFrameDisposer(DisposeFrame);

    mSingleFrameContainer = gcnew SingleFrame(disposer);
    mPreBuffer = gcnew PreBuffer(disposer);
    mCache = gcnew Cache(disposer);
    
    m_LoopWatcher = gcnew LoopWatcher();
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
    mTimestampInfo = TimestampInfo::Empty;
    m_WasPrebuffering = false;
    m_CanDrawUnscaled = false;
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
        AVCodecContext* pin = mVideoCodecCtx;
        avcodec_free_context(&pin);
        mVideoCodecCtx = pin;
    }

    if (mFormatCtx != nullptr)
    {
        AVFormatContext* pin = mFormatCtx;
        avformat_close_input(&pin);
        mFormatCtx = pin;
    }
}


VideoSummary^ VideoReaderFFMpeg::ExtractSummary(String^ filePath, int count, Size maxSize)
{
    // Open the file and extract some info + a few thumbnails.
    mVerbose = false;
    VideoSummary^ summary = gcnew VideoSummary(filePath);

    // Allocate 100 ms to this task. 
    // Always get at least one image but after that if we run out of time we cancel.
    int64_t timeout = 100;
    m_Stopwatch->Restart();

    OpenVideoResult loaded = Load(filePath, true);

    if (loaded != OpenVideoResult::Success)
    {
        return summary;
    }

    ChangeCachingMode(VideoDecodingMode::OnDemand);

    summary->IsImage = mVideoInfo.DurationTimeStamps == 1;
    double durationSeconds = (mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame) / mVideoInfo.AverageTimeStampsPerSeconds;
    summary->DurationMilliseconds = (int64_t)Math::Round(durationSeconds * 1000.0);
    summary->ImageSize = mVideoInfo.ReferenceSize;
    summary->Framerate = mVideoInfo.FramesPerSeconds;

    //log->DebugFormat("ExtractSummary {0}. After load: {1} ms.", filePath, m_Stopwatch->ElapsedMilliseconds);
    
    // Read some frames (directly decode at small size).
    float stretch = (float)mVideoInfo.OriginalSize.Width / maxSize.Width;
    m_DecodingSize = Size(maxSize.Width, (int)(mVideoInfo.OriginalSize.Height / stretch));

    int64_t step = (int64_t)Math::Ceiling((double)mVideoInfo.DurationTimeStamps / count);
    int64_t previousFrameTimestamp = -1;
    
    int index = 0;
    for (int64_t ts = 0; ts < mVideoInfo.DurationTimeStamps; ts += step)
    {
        index++;
        ReadResult read = ReadFrame(ts == 0 ? -1 : ts, 1, true);
        
        //log->DebugFormat("After ReadFrame #{0} [{1}]: {2} ms.", index, mTimestampInfo.CurrentTimestamp, m_Stopwatch->ElapsedMilliseconds);

        if (read == ReadResult::Success &&
            mFrameContainer->CurrentFrame != nullptr &&
            mTimestampInfo.CurrentTimestamp > previousFrameTimestamp)
        {
            Bitmap^ bmp = BitmapHelper::Copy(mFrameContainer->CurrentFrame->Image);
            summary->Thumbs->Add(bmp);
            previousFrameTimestamp = mTimestampInfo.CurrentTimestamp;
        }
        else
        {
            // Bail out on reading error.
            break;
        }

        if (m_Stopwatch->ElapsedMilliseconds > timeout)
        {
            log->WarnFormat("Thumbnail out of budget after {0} frames in {1} ms. {2}.", 
                index, m_Stopwatch->ElapsedMilliseconds, Path::GetFileName(filePath));
            break;
        }
    }

    Close();
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
    if (Options == nullptr)
    {
        Options = Options->Default;
    }

    // Libav expects the filename in the computer default codepage.
    // FIXME: this breaks especially on Korean Windows.
    String^ encFilePath = System::Text::Encoding::Default->GetString(System::Text::Encoding::UTF8->GetBytes(filePath));
    char* pszFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(encFilePath).ToPointer());
        
    // Open format.
    // TODO: muxer options.
    AVFormatContext* formatCtx = nullptr;
    if (avformat_open_input(&formatCtx, pszFilePath, NULL, NULL) != 0)
    {
        log->ErrorFormat("The file {0} could not be openned. (Wrong path or not a video/image.)", filePath);
        return OpenVideoResult::FileNotOpenned;
    }

    Marshal::FreeHGlobal(safe_cast<IntPtr>(pszFilePath));

    // Get stream info.
    int res = avformat_find_stream_info(formatCtx, nullptr);
    if (res < 0)
    {
        log->ErrorFormat("Stream info not found. Error: {0}.", res);
        return OpenVideoResult::StreamInfoNotFound;
    }

    // Video stream.
    mVideoStreamIndex = av_find_best_stream(formatCtx, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
    if (mVideoStreamIndex < 0)
    {
        log->Error("No video stream found in the file.");
        return OpenVideoResult::VideoStreamNotFound;
    }

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
    
    res = avcodec_open2(videoCodecCtx, videoCodec, nullptr);
    if (res < 0) 
    {
        log->ErrorFormat("Codec could not be openned. Error: {0}", res);
        return OpenVideoResult::CodecNotOpened;
    }
                
    //-----------------------------------------------------
    // Time info
    //-----------------------------------------------------
    mVideoInfo.AverageTimeStampsPerSeconds = (double)videoStream->time_base.den / (double)videoStream->time_base.num;

    // This may be updated after the first actual decoding.
    long firstTimestamp = (long)((double)((double)formatCtx->start_time / (double)AV_TIME_BASE) * mVideoInfo.AverageTimeStampsPerSeconds);
    mVideoInfo.FirstTimeStamp = Math::Max(firstTimestamp, 0);

    // In case of negative start time, we still want to expose 0-based timestamps to the outside.
    // We keep the offset around and add/remove it to low-level ffmpeg calls.
    // TODO: there are options in the demuxer to automatically handle negative timestamps.
    if (firstTimestamp < 0)
    {
        m_timestampOffset = firstTimestamp - 1;
        if (!forSummary)
        {
            log->WarnFormat("Negative start time. Applying timestamp offset of {0}.", m_timestampOffset);
        }
    }

    if (formatCtx->duration > 0)
    {
        mVideoInfo.DurationTimeStamps = (int64_t)((double)((double)formatCtx->duration / (double)AV_TIME_BASE) * mVideoInfo.AverageTimeStampsPerSeconds);
    }
    else
    {
        mVideoInfo.DurationTimeStamps = 0;
    }

    if (mVideoInfo.DurationTimeStamps <= 0)
    {
        log->Error("Duration info not found.");
        return OpenVideoResult::StreamInfoNotFound;
    }

    bool verbose = !forSummary;
        
    mVideoInfo.FramesPerSeconds = 0;
    GuessFrameRate(formatCtx, videoCodecCtx, mVideoStreamIndex, verbose);

    mVideoInfo.FrameIntervalMilliseconds = 1000.0 / mVideoInfo.FramesPerSeconds;
    mVideoInfo.AverageTimeStampsPerFrame = mVideoInfo.AverageTimeStampsPerSeconds / mVideoInfo.FramesPerSeconds;

    mWorkingZone = VideoSection(
        mVideoInfo.FirstTimeStamp,
        (int64_t)Math::Round(mVideoInfo.FirstTimeStamp + mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame));

    //-----------------------------------------------------
    // Image size info
    //-----------------------------------------------------

    // Image rotation
    mVideoInfo.ImageRotation = ImageRotation::Rotate0;
    AVDictionaryEntry* rotationTag = av_dict_get(videoStream->metadata, "rotate", nullptr, 0);
    if (rotationTag != nullptr)
    {
        String^ value = gcnew String(rotationTag->value);
        if (value == "90")
            mVideoInfo.ImageRotation = ImageRotation::Rotate90;
        else if (value == "180")
            mVideoInfo.ImageRotation = ImageRotation::Rotate180;
        else if (value == "270")
            mVideoInfo.ImageRotation = ImageRotation::Rotate270;
    }

    // Remember if the codec is MPEG2. 
    // We use this to detect a specific behavior related to sample aspect ratio.
    mVideoInfo.IsCodecMpeg2 = (videoCodecId == AV_CODEC_ID_MPEG2VIDEO);

    mVideoInfo.OriginalSize = Size(videoCodecCtx->width, videoCodecCtx->height);

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

    this->Options->ImageRotation = mVideoInfo.ImageRotation;
    UpdateReferenceSizes(Options->ImageAspectRatio, verbose);

    //-----------------------------------------------------
        
    mFormatCtx = formatCtx;
    mVideoCodecCtx = videoCodecCtx;

    mIsLoaded = true;

    if (forSummary)
    {
        m_Capabilities = VideoCapabilities::CanDecodeOnDemand;
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
    else
    {
        m_Capabilities =
            VideoCapabilities::CanDecodeOnDemand |
            VideoCapabilities::CanPreBuffer |
            VideoCapabilities::CanCache |
            VideoCapabilities::CanChangeAspectRatio |
            VideoCapabilities::CanChangeImageRotation |
            VideoCapabilities::CanChangeDeinterlacing |
            VideoCapabilities::CanChangeWorkingZone |
            VideoCapabilities::CanChangeDecodingSize |
            VideoCapabilities::CanStabilize;

        if (mVideoCodecCtx->codec_id == AV_CODEC_ID_RAWVIDEO)
        {
            m_Capabilities = m_Capabilities | VideoCapabilities::CanChangeDemosaicing;
        }

        // Start with no caching, we'll switch later.
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }

    return OpenVideoResult::Success;
}


void VideoReaderFFMpeg::GuessFrameRate(AVFormatContext* formatCtx, AVCodecContext* videoCodecCtx, int streamIndex, bool verbose)
{
    // Average FPS. Based on the following sources:
    // - libav in stream info.
    // - libav in container or stream with duration in frames or microseconds (Rarely available but valid if so).
    // - stream->time_base	(Often KO, like 90000:1, expresses the timestamps unit)
    // - codec->time_base (Often OK, but not always).
    // - some ad-hoc special cases.

    double avgFrameRate = 0.0;
    if (formatCtx->streams[mVideoStreamIndex]->avg_frame_rate.den != 0)
    {
        // We found a valid average frame rate in the stream info, keep it.
        mVideoInfo.FramesPerSeconds = (double)formatCtx->streams[mVideoStreamIndex]->avg_frame_rate.num / (double)formatCtx->streams[mVideoStreamIndex]->avg_frame_rate.den;
        if (verbose)
        {
            log->Debug("Average Fps estimation method: libav > average frame rate in stream info.");
        }

        return;
    }

    //AV_CODEC_PROP_FIELDS
    //int ticksPerFrame = videoCodecCtx->ticks_per_frame;
    //if (verbose)
    //{
    //    log->Debug("Ticks per frame: " + ticksPerFrame);
    //}

    // Check stream frames and format duration.
    if ((formatCtx->streams[mVideoStreamIndex]->nb_frames > 0) && (formatCtx->duration > 0))
    {
        mVideoInfo.FramesPerSeconds = ((double)formatCtx->streams[mVideoStreamIndex]->nb_frames * (double)AV_TIME_BASE) / (double)formatCtx->duration;

        //if (ticksPerFrame > 1)
        //    mVideoInfo.FramesPerSeconds /= ticksPerFrame;

        if (verbose)
            log->Debug("Average Fps estimation method: Durations.");
    
        return;
    }
    

    // Stream->time_base, consider invalid if >= 1000.
    mVideoInfo.FramesPerSeconds = (double)formatCtx->streams[mVideoStreamIndex]->time_base.den / (double)formatCtx->streams[mVideoStreamIndex]->time_base.num;
    if (mVideoInfo.FramesPerSeconds < 1000)
    {
        /*if (ticksPerFrame > 1)
            mVideoInfo.FramesPerSeconds /= ticksPerFrame;*/

        if (verbose)
        {
            log->Debug("Average Fps estimation method: Stream timebase.");
        }

        return;
    }

    // Codec->time_base, consider invalid if >= 1000.
    mVideoInfo.FramesPerSeconds = (double)videoCodecCtx->time_base.den / (double)videoCodecCtx->time_base.num;

    if (mVideoInfo.FramesPerSeconds < 1000)
    {
        //if (ticksPerFrame > 1)
        //    mVideoInfo.FramesPerSeconds /= ticksPerFrame;

        if (verbose)
        {
            log->Debug("Average Fps estimation method: Codec timebase.");
        }

        return;
    }

    // Special case detection, seen in the wild.
    if (mVideoInfo.FramesPerSeconds == 30000)
    {
        mVideoInfo.FramesPerSeconds = 29.97;
        if (verbose)
        {
            log->Debug("Average Fps estimation method: special case detection (30000:1 -> 30000:1001).");
        }

        return;
    }

    if (mVideoInfo.FramesPerSeconds == 25000)
    {
        mVideoInfo.FramesPerSeconds = 24.975;
        if (verbose)
        {
            log->Debug("Average Fps estimation method: special case detection (25000:1 -> 25000:1001).");
        }

        return;
    }

    // Detection failed. Force to 25fps.
    mVideoInfo.FramesPerSeconds = 25;
    if (verbose)
    {
        log->Debug("Average Fps estimation method: Estimation failed. Fps will be forced to : " + mVideoInfo.FramesPerSeconds);
    }
}


void VideoReaderFFMpeg::PostLoad()
{
    if (CanPreBuffer && mCachingMode == VideoDecodingMode::OnDemand)
    {
        ChangeCachingMode(VideoDecodingMode::PreBuffering);

        // FIXME: use a spin loop in the caller instead of sleeping.

        // Add a small temporisation so the prebuffering thread can decode the first frame.
        // The UI thread will very soon ask for the first frame of the working zone, 
        // if it's too quick we would cancel the thread at the same time it decodes the request frame.
        //Thread::CurrentThread->Sleep(40);
        Thread::CurrentThread->Sleep(100);
    }
}
#pragma endregion

#pragma region Frame requests
bool VideoReaderFFMpeg::MoveNext(int _skip, bool _decodeIfNecessary)
{
    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return false;

    bool moved = false;

    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        ReadResult res = ReadFrame(-1, _skip + 1, false);
        moved = res == ReadResult::Success;
    }
    else if (mCachingMode == VideoDecodingMode::Caching)
    {
        moved = mCache->MoveBy(_skip + 1);
    }
    else if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        if (!_decodeIfNecessary || mPreBuffer->HasNext(_skip))
        {
            mPreBuffer->MoveBy(_skip + 1);
            moved = true;
        }
        else
        {
            // Stop thread, decode frame, move to it, restart thread.
            log->DebugFormat("MoveNext, stopping pre-buffering.");
            StopPreBuffering();
            ReadResult res = ReadFrame(-1, _skip + 1, false);
            if (res == ReadResult::Success)
                moved = mPreBuffer->MoveBy(_skip + 1);
            StartPreBuffering();
        }
    }

    return moved && HasMoreFrames();
}

bool VideoReaderFFMpeg::MoveTo(int64_t from, int64_t target)
{
    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return false;

    
    bool moved = false;
    target = MapTimestamp(target);
    //log->DebugFormat("VideoReaderFFMpeg::MoveTo: {0} -> {1}.", from, target);

    if (mCachingMode == VideoDecodingMode::OnDemand)
    {
        ReadResult res = ReadFrame(target, 1, false);
        moved = (res == ReadResult::Success);
    }
    else if (mCachingMode == VideoDecodingMode::Caching)
    {
        moved = mCache->MoveTo(target);
    }
    else if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        if (mPreBuffer->Contains(target))
        {
            //if (mVerbose)
            //    log->DebugFormat("MoveTo. From:{0} to target:{1}. In buffer:{2}.", from, target, mPreBuffer->Segment);
            
            moved = mPreBuffer->MoveTo(target);
        }
        else
        {
            // Stop thread, decode frame, move to it, restart thread.
            log->DebugFormat("MoveTo, stopping pre-buffering.");
            StopPreBuffering();

            // Adding the target frame will either keep the prebuffer frames contiguous or not.
            // If the frame is the next one or it's a rollover jump, fine. Otherwise we need to clear.
            // jump to next frame after current segment is currently not handled gracefully and will clear anyway.
            // (Avoids another locking just for a very rare case).
            if (!mPreBuffer->IsRolloverJump(target))
            {
                //if (mVerbose)
                //    log->DebugFormat("MoveTo. From:{0} to target:{1}. Out of buffer:{2}. Clearing buffer.", from, target, mPreBuffer->Segment);
                
                mPreBuffer->Clear();
            }

            // This is done on the UI thread but the decoding thread has just been put to sleep.
            m_Stopwatch->Restart();
            ReadResult res = ReadFrame(target, 1, false);
            //ReadResult res = ReadFrame(target, 1, true);
            if (mVerbose)
                log->DebugFormat("MoveTo. Read frame in {0} ms.", m_Stopwatch->ElapsedMilliseconds);
            
            if (res == ReadResult::Success)
            {
                // The actual timestamp we land on might not be the one requested.
                int64_t actualTarget = mTimestampInfo.CurrentTimestamp;
                if (target != actualTarget)
                    AddTimestampMapping(target, actualTarget);

                moved = mPreBuffer->MoveTo(actualTarget);
                if (mVerbose)
                    log->DebugFormat("MoveTo. Moved to {0}.", actualTarget);
            }

            StartPreBuffering();
        }
    }

    return moved && HasMoreFrames();
}
#pragma endregion

#pragma region Decoding mode, play loop and frame enumeration
void VideoReaderFFMpeg::BeforePlayloop()
{
    // Just in case something wrong happened, make sure the decoding thread is alive.
    if (DecodingMode != VideoDecodingMode::Caching &&
        (CanPreBuffer && DecodingMode != VideoDecodingMode::PreBuffering))
    {
        log->Error("Forcing PreBuffering thread to restart.");
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    }
}

void VideoReaderFFMpeg::ResetDrops()
{
    if (mCachingMode == VideoDecodingMode::PreBuffering)
        mPreBuffer->ResetDrops();
}

void VideoReaderFFMpeg::UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler^>^ _workerFn)
{
    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
        return;

    if (!CanChangeWorkingZone)
        throw gcnew CapabilityNotSupportedException();

    if (mVerbose)
        log->DebugFormat("Update working zone request. {0} to {1}. Force reload:{2}", mWorkingZone, _newZone, _forceReload);

    if (!_forceReload && mWorkingZone == _newZone)
        return;

    if (!CanCache)
    {
        mWorkingZone = _newZone;
        if (mCachingMode == VideoDecodingMode::OnDemand && CanPreBuffer)
            ChangeCachingMode(VideoDecodingMode::PreBuffering);
        else if (mCachingMode == VideoDecodingMode::PreBuffering)
            mPreBuffer->UpdateWorkingZone(mWorkingZone);
    }
    else
    {
        if (_workerFn == nullptr)
            throw gcnew ArgumentNullException("workerFn");

        // Try to (re)load the entire working zone in the cache.
        // We try not to load parts that are already loaded.

        // The new working zone requested may come from an interpolation between pixels and timestamps,
        // it is not guaranteed to land on exact frames. We must reupdate our internal value with
        // the actual boundaries, be it for reducing or expanding.

        if (mVerbose)
            log->DebugFormat("Working zone update. Current:{0}, Asked:{1}", mWorkingZone, _newZone);

        if (WorkingZoneMemoryRequirement(_newZone) > _maxMemory)
        {
            if (mVerbose)
                log->Debug("New working zone does not fit in memory.");

            mWorkingZone = _newZone;
            ChangeToBestAfterCaching();
        }
        else
        {
            m_SectionToPrepend = VideoSection::MakeEmpty();
            m_SectionToAppend = VideoSection::MakeEmpty();

            if (mCachingMode != VideoDecodingMode::Caching || _forceReload)
            {
                if (mVerbose)
                    log->Debug("Just entering the cached mode, import everything.");

                if (mCachingMode == VideoDecodingMode::Caching)
                {
                    // Force a reload of the cache.
                    if (mFrameContainer != nullptr)
                        mFrameContainer->Clear();
                }

                ChangeCachingMode(VideoDecodingMode::Caching);
                m_SectionToPrepend = _newZone;
            }
            else
            {
                if (_newZone.Start > mWorkingZone.Start)
                {
                    // Only do it if the new start is at least one frame beyond the old one.
                    if (_newZone.Start - mWorkingZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                    {
                        mCache->ReduceWorkingZone(VideoSection(_newZone.Start, mWorkingZone.End));
                        mWorkingZone = mCache->WorkingZone;
                        log->DebugFormat("Reduced cache from the front: {0}.", mWorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    _newZone = VideoSection(mWorkingZone.Start, _newZone.End);
                }

                if (_newZone.End < mWorkingZone.End)
                {
                    // Only do it if the new end is at least one frame before the old one.
                    if (mWorkingZone.End - _newZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                    {
                        mCache->ReduceWorkingZone(VideoSection(mWorkingZone.Start, _newZone.End));
                        mWorkingZone = mCache->WorkingZone;
                        log->DebugFormat("Reduced cache from the back: {0}.", mWorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    _newZone = VideoSection(_newZone.Start, mWorkingZone.End);
                }

                // Bail out if our job is done.
                if (_newZone.Start == mWorkingZone.Start && _newZone.End == mWorkingZone.End)
                    return;

                // Expand at the front if there is more than one frame to expand.
                if (mWorkingZone.Start - _newZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    m_SectionToPrepend = VideoSection(_newZone.Start, mWorkingZone.Start);
                }

                // Expand at the back if there is more than one frame to expand.
                if (_newZone.End - mWorkingZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    m_SectionToAppend = VideoSection(mWorkingZone.End, _newZone.End);
                }
            }

            if (!m_SectionToPrepend.IsEmpty || !m_SectionToAppend.IsEmpty)
            {
                // As C++/CLI doesn't support lambdas expressions, we have to resort to a separate method and global variables.
                DoWorkEventHandler^ workHandler = gcnew DoWorkEventHandler(this, &VideoReaderFFMpeg::ImportWorkingZoneToCache);
                _workerFn(workHandler);

                /*C# (including ImportWorkingZoneToCache)
                _workerFn((s,e) => {
                bool success = ReadManyToCache((BackgroundWorker)s, sectionToCache, prepend));
                if(!success)
                ExitCaching();
                }*/
            }
        }
    }
}

void VideoReaderFFMpeg::BeforeFrameEnumeration()
{
    // Frames are about to be enumerated (for example for saving).
    // This operation is not compatible with Prebuffering mode.
    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        m_WasPrebuffering = true;
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
}

void VideoReaderFFMpeg::AfterFrameEnumeration()
{
    if (m_WasPrebuffering)
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    m_WasPrebuffering = false;
}

void VideoReaderFFMpeg::ChangeCachingMode(VideoDecodingMode wantedMode)
{
    if (wantedMode == mCachingMode)
        return;

    if (!CanSwitchDecodingMode(wantedMode))
        throw gcnew CapabilityNotSupportedException();

    if (mVerbose)
        log->DebugFormat("Changing decoding mode: {0} -> {1}", mCachingMode.ToString(), wantedMode.ToString());

    if (mCachingMode == VideoDecodingMode::PreBuffering)
    {
        log->DebugFormat("ChangeCachingMode, stopping pre-buffering.");
        StopPreBuffering();
        ResetDecodingSize();

        m_CanDrawUnscaled = false;
    }

    if (mFrameContainer != nullptr)
        mFrameContainer->Clear();

    mCachingMode = wantedMode;
    switch (mCachingMode)
    {
    case VideoDecodingMode::OnDemand:
        mFrameContainer = mSingleFrameContainer;
        break;
    case VideoDecodingMode::PreBuffering:
        mFrameContainer = mPreBuffer;
        mPreBuffer->UpdateWorkingZone(mWorkingZone);
        SeekTo(mWorkingZone.Start);
        StartPreBuffering();
        break;
    case VideoDecodingMode::Caching:
        mFrameContainer = mCache;
        break;
    default:
        mFrameContainer = nullptr;
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
    int bufferSize = av_image_get_buffer_size(sConvertPixelFormat, mVideoInfo.ReferenceSize.Width, mVideoInfo.ReferenceSize.Height, 1);
    double frameMegaBytes = (double)bufferSize / 1048576;
    double durationMegaBytes = durationSeconds * mVideoInfo.FramesPerSeconds * frameMegaBytes;

    return durationMegaBytes;
}

void VideoReaderFFMpeg::ImportWorkingZoneToCache(System::Object^ sender, DoWorkEventArgs^ e)
{
    BackgroundWorker^ worker = dynamic_cast<BackgroundWorker^>(sender);

    bool success = true;
    if (!m_SectionToPrepend.IsEmpty)
        success = ReadManyToCache(worker, m_SectionToPrepend, true);

    if (success && !m_SectionToAppend.IsEmpty)
        success = ReadManyToCache(worker, m_SectionToAppend, false);

    if (!success)
        ChangeToBestAfterCaching();
}

#pragma endregion

#pragma region Image adjustments (aspect, rotation, demosaicing, deinterlace, stabilization)

bool VideoReaderFFMpeg::ChangeAspectRatio(ImageAspectRatio aspectRatio)
{
    if (!CanChangeAspectRatio)
        throw gcnew CapabilityNotSupportedException();

    if (aspectRatio == Options->ImageAspectRatio)
    {
        // Program error.
        log->ErrorFormat("Request to change aspect ratio but already using the correct aspect ratio.");
    }

    // Potentially changes the aspect ratio size and the reference image size.
    // This invalidates any cached frames.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
    {
        StopPreBuffering();
    }

    Options->ImageAspectRatio = aspectRatio;
    UpdateReferenceSizes(Options->ImageAspectRatio, true);
    
    mFrameContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeImageRotation(ImageRotation rotation)
{
    if (!CanChangeImageRotation)
        throw gcnew CapabilityNotSupportedException();

    if (rotation == Options->ImageRotation)
    {
        // Program error.
        log->ErrorFormat("Request to change image rotation but already using the correct rotation.");
    }

    // Potentially changes the aspect ratio size (padded to rotated width), 
    // and the reference image size. This invalidates any cached frames.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
    {
        StopPreBuffering();
    }

    Options->ImageRotation = rotation;
    mVideoInfo.ImageRotation = rotation;
    UpdateReferenceSizes(Options->ImageAspectRatio, true);
    
    mFrameContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeDemosaicing(Demosaicing demosaicing)
{
    if (!CanChangeDemosaicing)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
    {
        log->ErrorFormat("PreBuffering thread is started.");
    }

    Options->Demosaicing = demosaicing;

    mFrameContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeDeinterlace(bool _deint)
{
    if (!CanChangeDeinterlacing)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    Options->Deinterlace = _deint;
    mFrameContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::SetStabilizationData(List<Kinovea::Services::TimedPoint^>^ points)
{
    // Precompute the list of frame offsets with regards to the first point of the track.
    stabOffsets->Clear();
    mFrameContainer->Clear();
    
    if (points == nullptr)
        return true;

    for (int i = 0; i < points->Count; i++)
    {
        if (stabOffsets->ContainsKey((long)points[i]->T))
            continue;

        TimedPoint^ p = gcnew TimedPoint(points[i]->X - points[0]->X, points[i]->Y - points[0]->Y, points[i]->T);
        stabOffsets->Add((long)points[i]->T, p);
    }

    return true;
}

#pragma endregion

#pragma region Decoding size

bool VideoReaderFFMpeg::ChangeDecodingSize(Size _size)
{
    // Should return true if we are going to use this size.

    if (!CanChangeDecodingSize)
        throw gcnew CapabilityNotSupportedException();

    bool sideway = mVideoInfo.ImageRotation == ImageRotation::Rotate90 || mVideoInfo.ImageRotation == ImageRotation::Rotate270;
    Size targetSize = FixSize(_size, sideway);
    if (targetSize == m_DecodingSize)
    {
        // No change required. If we are not in pre-buffering, the decoding size is already the reference size.
        m_CanDrawUnscaled = true;
        return true;
    }

    if (mCachingMode != VideoDecodingMode::PreBuffering)
    {
        log->Debug("Will not change decoding size because we are not prebuffering.");
        m_CanDrawUnscaled = false;
        return false;
    }

    if (mVerbose)
        log->DebugFormat("Changing decoding size: {0}x{1} -> {2}x{3}", 
            m_DecodingSize.Width, m_DecodingSize.Height, targetSize.Width, targetSize.Height);

    int64_t currentTimestamp = mPreBuffer->CurrentFrame != nullptr ? mPreBuffer->CurrentFrame->Timestamp : -1;

    log->DebugFormat("ChangeDecodingSize, stopping pre-buffering.");
    StopPreBuffering();
    mPreBuffer->Clear();
    m_DecodingSize = targetSize;

    m_CanDrawUnscaled = true;

    if (currentTimestamp >= 0)
    {
        ReadResult res = ReadFrame(currentTimestamp, 1, false);
        if (res == ReadResult::Success)
            mPreBuffer->MoveTo(currentTimestamp);
    }

    StartPreBuffering();

    return true;
}

void VideoReaderFFMpeg::DisableCustomDecodingSize()
{
    m_CanDrawUnscaled = false;

    if (mCachingMode != VideoDecodingMode::PreBuffering)
        return;

    int64_t currentTimestamp = mPreBuffer->CurrentFrame != nullptr ? mPreBuffer->CurrentFrame->Timestamp : -1;

    log->DebugFormat("DisableCustomDecodingSize, stopping pre-buffering.");
    StopPreBuffering();
    mPreBuffer->Clear();
    ResetDecodingSize();

    if (currentTimestamp >= 0)
    {
        ReadResult res = ReadFrame(currentTimestamp, 1, false);
        if (res == ReadResult::Success)
            mPreBuffer->MoveTo(currentTimestamp);
    }

    StartPreBuffering();
}

void VideoReaderFFMpeg::ResetDecodingSize()
{
    // Reset the decoding size to the default.
    // "Aspect ratio size" is the video image size with 
    // custom aspect ratio and padded along rotated width.
    m_DecodingSize = mVideoInfo.AspectRatioSize;
}

void VideoReaderFFMpeg::UpdateReferenceSizes(Kinovea::Services::ImageAspectRatio _ratio, bool verbose)
{
    // Called during load or when aspect ratio or rotation changes.
    
    // Set the image geometry according to the pixel aspect ratio choosen.
    if (verbose)
        log->DebugFormat("Image aspect ratio: {0}", _ratio);

    // Constraint width and change height to match aspect ratio.
    mVideoInfo.AspectRatioSize.Width = mVideoInfo.OriginalSize.Width;

    switch (_ratio)
    {
    case Kinovea::Services::ImageAspectRatio::Force43:
        mVideoInfo.AspectRatioSize.Height = (int)((mVideoInfo.OriginalSize.Width * 3.0) / 4.0);
        break;
    case Kinovea::Services::ImageAspectRatio::Force169:
        mVideoInfo.AspectRatioSize.Height = (int)((mVideoInfo.OriginalSize.Width * 9.0) / 16.0);
        break;
    case Kinovea::Services::ImageAspectRatio::ForcedSquarePixels:
        mVideoInfo.AspectRatioSize.Height = mVideoInfo.OriginalSize.Height;
        break;
    case Kinovea::Services::ImageAspectRatio::Auto:
    default:
        mVideoInfo.AspectRatioSize.Height = (int)((double)mVideoInfo.OriginalSize.Height / mVideoInfo.PixelAspectRatio);
        break;
    }

    bool sideway = mVideoInfo.ImageRotation == ImageRotation::Rotate90 || mVideoInfo.ImageRotation == ImageRotation::Rotate270;
    mVideoInfo.AspectRatioSize = FixSize(mVideoInfo.AspectRatioSize, sideway);
    mVideoInfo.ReferenceSize = sideway ? Size(mVideoInfo.AspectRatioSize.Height, mVideoInfo.AspectRatioSize.Width) : mVideoInfo.AspectRatioSize;

    if (verbose)
        log->DebugFormat("Image size: Original:{0}, AspectRatioSize:{1}, ReferenceSize:{2}.", mVideoInfo.OriginalSize, mVideoInfo.AspectRatioSize, mVideoInfo.ReferenceSize);

    // After this the decoding size should be reset.
    // First to the default (aspect ratio size), and later to a custom size based on the viewport, if possible.
    // This second step will happen in psui > ResizeUpdate().
    ResetDecodingSize();
}

Size VideoReaderFFMpeg::FixSize(Size _size, bool sideways)
{
    // Fix unsupported width for conversion to .NET Bitmap. Must be a multiple of 4.
    // Subtlety: the padding must be in the dimension that will be the width after rotation.
    if (sideways)
        return Size(_size.Width, _size.Height + (_size.Height % 4));
    else
        return Size(_size.Width + (_size.Width % 4), _size.Height);
}

#pragma endregion

#pragma region Low level frame reading

bool VideoReaderFFMpeg::ReadManyToCache(BackgroundWorker^ _bgWorker, VideoSection _section, bool _prepend)
{
    // Load the asked section to cache (doesn't move the playhead).
    // Called when filling the cache with the Working Zone.
    // Might also be called internally when loading a very short video or single image.

    if (!CanCache || mCachingMode != VideoDecodingMode::Caching)
        throw gcnew CapabilityNotSupportedException("Importing to cache is not supported for the video.");

    if (_bgWorker != nullptr)
        Thread::CurrentThread->Name = "CacheFilling";

    if (mVerbose)
        log->DebugFormat("Requested section to cache: {0}. Prepend:{1}", _section, _prepend);

    mCache->SetPrependBlock(_prepend);

    bool success = true;
    int read = 0;

    // Note: the passed section only represents what we need to prepend or append, not the target section.
    // Realign the requested section on real timestamps.
    if (!mCache->WorkingZone.IsEmpty)
    {
        if (_prepend && 
           (mCache->WorkingZone.Start - _section.Start < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // Start target is less than one frame before the current start.
            _section = VideoSection(mCache->WorkingZone.Start, _section.End);
        }
        else if (!_prepend && 
            (_section.End - mCache->WorkingZone.End < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // End target is less than one frame beyond the current end.
            _section = VideoSection(_section.Start, mCache->WorkingZone.End);
        }

        log->DebugFormat("Aligned requested section to cache: {0}", _section);
    }

    double end = _section.End + (mVideoInfo.AverageTimeStampsPerFrame * 0.5);
    double frames = (end - _section.Start) / mVideoInfo.AverageTimeStampsPerFrame;
    int total = (int)Math::Floor(frames);

    log->DebugFormat("Frames to cache: {0}", total);

    // Bail out if re-alignment revealed we don't need to cache anything new.
    if (total == 0)
        return true;

    // If the video is very short this call can only happen when opening the video.
    // We avoid a useless seek in this case. Prevent problems with non seekable files like single images.
    ReadResult res;
    if (m_bIsVeryShort)
        res = ReadFrame(-1, 1, false);
    else
        res = ReadFrame(_section.Start, 1, false);

    success = (res == ReadResult::Success);




    // Continue reading frames until we have the right number or we are past the target.
    while ((mTimestampInfo.CurrentTimestamp < _section.End) &&
           (read < total) && 
           (res == ReadResult::Success))
    {
        if (_bgWorker != nullptr && _bgWorker->CancellationPending)
        {
            if (mVerbose)
                log->DebugFormat("Cancellation at frame [{0}]", mTimestampInfo.CurrentTimestamp);

            mCache->Clear();
            success = false;
            break;
        }

        // Read one frame.
        res = ReadFrame(-1, 1, false);
        success = (res == ReadResult::Success);

        if (_bgWorker != nullptr)
            _bgWorker->ReportProgress(read++, total);
    }

    mWorkingZone = mCache->WorkingZone;
    mCache->SetPrependBlock(false);

    // Sometimes a few frames at the end can't be read.
    if (mTimestampInfo.CurrentTimestamp < _section.End && read < total)
    {
        log->ErrorFormat("Caching section: could only read {0} out of {1} frames.", read, total);
    
        if (read >= (total - 1) * 0.95)
        {
            mWorkingZone = mCache->WorkingZone;
            success = true;
        }
    }

    return success;
}


ReadResult VideoReaderFFMpeg::ReadFrame(int64_t targetTimestamp, int targetFrameJump, bool approximate)
{
    m_LoopWatcher->LoopStart();

    if (!mIsLoaded || mCachingMode == VideoDecodingMode::NotInitialized)
    {
        return ReadResult::FileNotLoaded;
    }

    if (mFrameContainer == nullptr)
    {
        return ReadResult::FrameContainerNotSet;
    }

    ReadResult result = ReadResult::Success;

    // This is used for both seeking and relative jump.
    bool seeking = targetTimestamp >= 0;
    int	framesToDecode = targetFrameJump;
    int framesDecoded = 0;
    int res = 0;

    // TODO: shouldn't need to lock. Make sure we don't synchronously ask for a frame while prebuffering.
    lock l(m_Locker);

    // Convert negative jump to a seek target.
    if (targetFrameJump < 0)
    {
        targetTimestamp = (int64_t)Math::Round(mFrameContainer->CurrentFrame->Timestamp + (targetFrameJump * mVideoInfo.AverageTimeStampsPerFrame));
    
        // Never seek before start.
        targetTimestamp = std::max(targetTimestamp, 0LL);
        seeking = true;
    }

    // At this point there are 3 cases.
    // 1. Seek to specific timestamp.
    // 2. Jump forward to the next frame.
    // 3. Jump forward by n frames.

    // It's possible to get here with a target timestamp equal to the current timestamp.
    // For example when we change the output size.
    // Currently this means we'll seek back to the start of the GOP and decode many frames again.
    // TODO: keep the AVFrame around and just redo the convert.

    // Do an initial seek if a seek target is specified.
    // This should land us at the start of the GOP containing the target.
    // Note that even if the seek target is in the current GOP we go through the 
    // seeking call and reset the libav internal buffers, because we can't know it beforehand.
    if (seeking)
    {
        framesToDecode = 1;
        //log->DebugFormat("Seeking to {0}", targetTimestamp);
        res = SeekTo(targetTimestamp);
        if (res < 0)
        {
            LogFFMpegError("SeekTo", res);
            log->ErrorFormat("Error trying to seek to: [{1}]", targetTimestamp);

            // Switch to decoding the next frame.
            seeking = false;
        }
    }

    // Get the first frame after the seek.
    AVFrame* frame = av_frame_alloc();
    result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);
    if (result != ReadResult::Success)
    {
        av_frame_free(&frame);
        return result;
    }

    framesDecoded = 1;
    //LogFrameInfo(frame);

    // Check if seeking landed beyond the target.
    // TODO: seek back further.
    if (seeking && !approximate && frame->best_effort_timestamp > targetTimestamp)
    {
        log->ErrorFormat("Seek or decode landed after target.");
        av_frame_free(&frame);
        return ReadResult::SeekAfterTarget;
    }

    // At this point we have decoded one frame.
    // Depending on the call we may be done or need to keep decoding.

    mTimestampInfo.CurrentTimestamp = frame->best_effort_timestamp;

    if (approximate)
    {
        // Early exit for thumbnail extraction.
        result = ConvertAndStoreFrame(frame);
        av_frame_free(&frame);
        return result;
    }

    if (seeking)
    {
        // Check if the initial decode is already at the seek target.
        if (mTimestampInfo.CurrentTimestamp >= targetTimestamp)
        {
            log->DebugFormat("Found seek target, decoded {0} frames.", framesDecoded);
            result = ConvertAndStoreFrame(frame);
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

            //LogFrameInfo(frame);
            framesDecoded++;
            mTimestampInfo.CurrentTimestamp = frame->best_effort_timestamp;
        
            if (frame->best_effort_timestamp >= targetTimestamp)
            {
                log->DebugFormat("Found seek target, decoded {0} frames.", framesDecoded);
                result = ConvertAndStoreFrame(frame);
                av_frame_free(&frame);
                break;
            }

            // Keep decoding.
        }

        return result;
    }
    else
    {
        // Check initial decode is already the right number of frames.
        if (framesToDecode == 1)
        {
            // We are done.
            //log->DebugFormat("Found target, decoded {0} frames.", framesDecoded);
            result = ConvertAndStoreFrame(frame);
            av_frame_free(&frame);
            return result;
        }

        // Otherwise keep decoding frames until we get the right number, or EOF.
        while (true)
        {
            result = DecodeOneFrame(mFormatCtx, mVideoStreamIndex, mVideoCodecCtx, frame);

            if (result != ReadResult::Success)
            {
                av_frame_free(&frame);
                return result;
            }

            //LogFrameInfo(frame);
            framesDecoded++;
            mTimestampInfo.CurrentTimestamp = frame->best_effort_timestamp;

            if (framesDecoded >= framesToDecode)
            {
                // We are done.
                //log->DebugFormat("Found target, decoded {0} frames.", framesDecoded);
                result = ConvertAndStoreFrame(frame);
                av_frame_free(&frame);
                break;
            }

            // Keep decoding.
        }

        return result;
    }

    // We don't get here.
    return result;
}


ReadResult VideoReaderFFMpeg::DecodeOneFrame(AVFormatContext* formatCtx, int streamIndex, AVCodecContext* codecCtx, AVFrame* frame)
{
    if (frame == nullptr)
    {
        return ReadResult::InvalidProgram;
    }

    AVPacket* packet = av_packet_alloc();
    int res = 0;
    ReadResult result = ReadResult::UnknownError;

    while (true)
    {
        // Try to decode the frame immediately in case libav already has it.
        // This happens when the codec has B-frames, the next I-frame may already
        // be decoded since it's required to decode the inside of the GOP.
        res = avcodec_receive_frame(codecCtx, frame);

        if (res >= 0)
        {
            // Our job is done.
            result = ReadResult::Success;
            break;
        }
        else if (res == AVERROR(EAGAIN))
        {
            // The decoder needs more packets before it can produce a frame.
            // This is normal for codecs with B-frames.
            
            // Read packets until we get a video one and feed it to libav.
            while (true)
            {
                av_packet_unref(packet);
                res = av_read_frame(formatCtx, packet);

                // Don't fail on EOF here. It sends EOF when it reads the last packet,
                // we should treat it as normal, feed it to the decoder and loop back to decoding.
                if (res < 0 && res != AVERROR_EOF)
                {
                    // If not end of file this is unrecoverable.
                    // We don't even know if it's on the right stream.
                    LogFFMpegError("av_read_frame", res);
                    result = ReadResult::UnknownError;
                    break;
                }
                
                // Keep reading if it's not in the right stream.
                if (packet->stream_index != streamIndex)
                {
                    continue;
                }

                // Supply the raw packet data as input to the decoder.
                res = avcodec_send_packet(codecCtx, packet);
                if (res < 0)
                {
                    if (res == AVERROR(EAGAIN))
                    {
                        // libav buffer is full and requires a call to avcodec_receive_frame
                        // to consume its internal buffer.
                        // This should never happen here, as we were just told it needed more packets.
                        LogFFMpegError("avcodec_send_packet", res);
                        result = ReadResult::UnknownError;
                        break;
                    }
                    else if (res == AVERROR_EOF)
                    {
                        // The decoder has been flushed and will not accept any more packets.
                        LogFFMpegError("avcodec_send_packet", res);
                        result = ReadResult::EOFReached;
                        break;
                    }
                    else
                    {
                        LogFFMpegError("avcodec_send_packet", res);
                        result = ReadResult::UnknownError;
                        break;
                    }
                }

                // If we get here we have read and sent a packet to libav.
                break;
            }

            if (res < 0)
            {
                // An irrecoverable error occurred while reading or sending packets.
                // `result` variable should be set already.
                break;
            }
            
            // We are ready to try decoding a frame again.
            continue;
        }
        else if (res == AVERROR_EOF)
        {
            // The decoder has been fully flushed and will not return any more frames.
            LogFFMpegError("avcodec_receive_frame", res);
            result = ReadResult::EOFReached;
            break;
        }
        else
        {
            LogFFMpegError("avcodec_receive_frame", res);
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
    
    // OLD Code was allowing to go further than the target and before zero.
    //int64_t minTs = m_timestampOffset;
    //int64_t ts = _target + m_timestampOffset;
    //int64_t maxTs = (int64_t)(_target + m_timestampOffset + mVideoInfo.AverageTimeStampsPerSeconds);

    int64_t minTs = 0;
    int64_t ts = targetTimestamp;
    int64_t maxTs = targetTimestamp;

    int res = avformat_seek_file(
        mFormatCtx,
        mVideoStreamIndex,
        minTs,
        ts,
        maxTs,
        AVSEEK_FLAG_BACKWARD);

    // Reset the internal codec state. 
    avcodec_flush_buffers(mVideoCodecCtx);

    mTimestampInfo = TimestampInfo::Empty;
    return res;
}


ReadResult VideoReaderFFMpeg::ConvertAndStoreFrame(AVFrame* decodedFrame)
{
    // Convert the frame from the source format to our working format AV_PIX_FMT_BGRA and store it in a Bitmap.
    
    // Prepare a new AVFrame to hold the converted frame.
    AVFrame* convertedFrame = av_frame_alloc();
    convertedFrame->format = AV_PIX_FMT_BGRA;
    convertedFrame->width = m_DecodingSize.Width;
    convertedFrame->height = m_DecodingSize.Height;
    int res = av_frame_get_buffer(convertedFrame, 1);
    if (res < 0)
    {
        LogFFMpegError("av_frame_get_buffer", res);
        av_frame_free(&convertedFrame);
        return ReadResult::MemoryNotAllocated;
    }

    // Convert the decoded AVFrame to the correct format and size.
    bool converted = RescaleAndConvert(decodedFrame, convertedFrame, m_DecodingSize.Width, m_DecodingSize.Height, sConvertPixelFormat, Options->Deinterlace);
    if (!converted)
    {
        av_frame_free(&convertedFrame);
        return ReadResult::ImageNotConverted;
    }

    // Prepare the Bitmap
    int imageStride = convertedFrame->linesize[0];
    IntPtr^ scan0 = gcnew IntPtr((void*)convertedFrame->data[0]);
    Bitmap^ bmp = nullptr;

    if (stabOffsets->ContainsKey(mTimestampInfo.CurrentTimestamp))
    {
        // Image stabilization. Paint the image with the offset applied.
        // Prepare output bitmap.
        bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, DecodingPixelFormat);

        // Get the decoded frame in a bitmap and paint it over the output.
        Bitmap^ bmp2 = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, (IntPtr)scan0);
        Graphics^ g = Graphics::FromImage(bmp);
        float dx = stabOffsets[mTimestampInfo.CurrentTimestamp]->X;
        float dy = stabOffsets[mTimestampInfo.CurrentTimestamp]->Y;
        // TODO: handle scaling (decoding size).
        g->DrawImageUnscaled(bmp2, (int)(-dx), (int)(-dy));
        delete g;
        delete bmp2;
    }
    else
    {
        bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, (IntPtr)scan0);
    }

    // Store a pointer to the libav allocated buffer inside the Tag of the bitmap.
    // We will have to free this buffer later when the frame is not used anymore.
    bmp->Tag = scan0;

    // Note: rotation doesn't change the size of the buffer.
    ApplyRotation(bmp, mVideoInfo.ImageRotation);

    // Construct a VideoFrame.
    VideoFrame^ vf = gcnew VideoFrame();
    vf->Image = bmp;
    vf->Timestamp = mTimestampInfo.CurrentTimestamp;
    
    // Store it to the active container.
    // If we are in mode on-demand, this is synchronous and will replace the single stored frame.
    // If we are in mode prebuffer, we are in a background thread and this will potentially block if the 
    // cache is full. 
    mFrameContainer->Add(vf);
    //log->DebugFormat("Stored frame [{0}]", mTimestampInfo.CurrentTimestamp);

    return ReadResult::Success;
}


AVPixelFormat VideoReaderFFMpeg::GetSourceFormat(AVCodecContext* videoCodecCtx)
{
    if (!CanChangeDemosaicing)
    {
        return videoCodecCtx->pix_fmt;
    }

    switch (Options->Demosaicing)
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

bool VideoReaderFFMpeg::RescaleAndConvert(AVFrame* srcFrame, AVFrame* dstFrame, int dstWidth, int dstHeight, AVPixelFormat dstPixelFormat, bool deinterlace)
{
    // todo: sws_getContext could be done only once.
    bool result = true;
    AVPixelFormat srcFormat = GetSourceFormat(mVideoCodecCtx);

    int flags = SWS_BILINEAR;
    // SWS_FAST_BILINEAR
    // SWS_POINT

    // By this point the converted frame is already allocated.


    // Using old API.
    SwsContext* scalingCtx = sws_getContext(
        mVideoCodecCtx->width, mVideoCodecCtx->height, srcFormat,
        dstWidth, dstHeight, dstPixelFormat,
        flags, NULL, 
        NULL, NULL);


    //const uint8_t* const srcSlice[]
    //const int srcStride[]
    //int srcSliceY
    //int srcSliceH,
    //uint8_t* const dst[]
    //const int dstStride[]
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



    // TODO: use new API.
    /*SwsContext* scalingCtx = sws_alloc_context();
    if (!scalingCtx)
    {
        return false;
    }
    scalingCtx->flags = SWS_FAST_BILINEAR;*/
    //sws_scale_frame(scalingCtx, convertedFrame, decodedFrame);


    //av_freep(&src_data[0]);
    //av_freep(&dst_data[0]);
    //sws_freeContext(scalingCtx);


    //SwsContext* c = sws_getContext(
    //    mVideoCodecCtx->width, mVideoCodecCtx->height, srcFormat,
    //    _decodingWidth, _decodingHeight, (AVPixelFormat)_outputFmt,
    //    sDecodingQuality,
    //    nullptr, nullptr, nullptr);

    //uint8_t** srcSlice = nullptr;               // Array containing pointers to planes of source slice.
    //int* srcStride = nullptr;                   // Array containing strides for each plane of the source image. 
    //int srcSliceY = 0;                          // the position in the source image of the slice to process, 
    //                                            // that is the number(counted starting from zero) in the image 
    //                                            // of the first row of the slice
    //int srcSliceH = mVideoCodecCtx->height;        // the height of the source slice, that is the number of rows in the slice.
    //uint8_t** dst = _pOutputFrame->data;        // the array containing the pointers to the planes of the destination image.
    //int* dstStride = _pOutputFrame->linesize;   // the array containing the strides for each plane of the destination image.

    //uint8_t* pDeinterlaceBuffer = nullptr;
    //if (_deinterlace)
    //{
    //    AVPicture* pDeinterlacingFrame;
    //    AVPicture	tmpPicture;

    //    // Deinterlacing happens before resizing.
    //    int iSizeDeinterlaced = avpicture_get_size(mVideoCodecCtx->pix_fmt, mVideoCodecCtx->width, mVideoCodecCtx->height);

    //    pDeinterlaceBuffer = new uint8_t[iSizeDeinterlaced];
    //    pDeinterlacingFrame = &tmpPicture;
    //    avpicture_fill(pDeinterlacingFrame, pDeinterlaceBuffer, mVideoCodecCtx->pix_fmt, mVideoCodecCtx->width, mVideoCodecCtx->height);

    //    int resDeint = avpicture_deinterlace(pDeinterlacingFrame, (AVPicture*)_pInputFrame, mVideoCodecCtx->pix_fmt, mVideoCodecCtx->width, mVideoCodecCtx->height);

    //    if (resDeint < 0)
    //    {
    //        // Deinterlacing failed, use original image.
    //        log->Debug("Deinterlacing failed, use original image.");
    //        srcSlice = _pInputFrame->data;
    //        srcStride = _pInputFrame->linesize;
    //    }
    //    else
    //    {
    //        // Use deinterlaced image.
    //        srcSlice = pDeinterlacingFrame->data;
    //        srcStride = pDeinterlacingFrame->linesize;
    //    }
    //}
    //else
    //{
    //    srcSlice = _pInputFrame->data;
    //    srcStride = _pInputFrame->linesize;
    //}

    //try
    //{
    //    sws_scale(c, srcSlice, srcStride, srcSliceY, srcSliceH, dst, dstStride);
    //}
    //catch (Exception^)
    //{
    //    bSuccess = false;
    //    log->Error("RescaleAndConvert Error : sws_scale failed.");
    //}

    //// Clean Up.
    //sws_freeContext(c);

    //if (pDeinterlaceBuffer != nullptr)
    //    delete[] pDeinterlaceBuffer;

    //return bSuccess;
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
    //log->DebugFormat("Disposing frame [{0}].", videoFrame->Timestamp);

    // Dispose the Bitmap and the native buffer.
    // The pointer to the native buffer was stored in the Tag property.
    IntPtr^ ptr = dynamic_cast<IntPtr^>(videoFrame->Image->Tag);
    delete videoFrame->Image;

    if (ptr != nullptr)
    {
        void* pBuf = ptr->ToPointer();
        av_freep(&pBuf);
    }
}

#pragma endregion

#pragma region PreBuffering thread

void VideoReaderFFMpeg::StartPreBuffering()
{
    if (!CanPreBuffer)
        throw gcnew CapabilityNotSupportedException();

    if (mCachingMode == VideoDecodingMode::Caching)
        return;

    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
    {
        log->Error("Prebuffering thread already started");
        StopPreBuffering();
        mPreBuffer->Clear();
        //debug - just to check when we could pass here.
        //throw gcnew CapabilityNotSupportedException();
    }

    if (mVerbose)
        log->Debug("Starting prebuffering thread.");

    ParameterizedThreadStart^ pts = gcnew ParameterizedThreadStart(this, &VideoReaderFFMpeg::PreBufferingWorker);
    m_PreBufferingThreadCanceler->Reset();
    m_PreBufferingThread = gcnew Thread(pts);
    m_PreBufferingThread->Start(m_PreBufferingThreadCanceler);
}

void VideoReaderFFMpeg::StopPreBuffering()
{
    if (m_PreBufferingThread == nullptr || !m_PreBufferingThread->IsAlive)
        return;

    if (mVerbose)
        log->Debug("Stopping prebuffering thread.");

    m_PreBufferingThreadCanceler->Cancel();

    // The cancellation will only be effective when we next pass in the 
    // decoding loop and check the cancellation flag. This means that if the thread is in waiting state, 
    // (trying to push a frame to an already full buffer), the cancellation will not proceed.
    // UnblockAndMakeRoom will force a Pulse, dequeing a frame if necessary.
    // However, if we just make room for one frame and it's the UI thread that is doing the Add,
    // it will be blocked after the addition since the buffer will again be full. 
    // We must actually make sure the next Read operation won't block.
    mPreBuffer->UnblockAndMakeRoom();

    m_PreBufferingThread->Join();
}

void VideoReaderFFMpeg::PreBufferingWorker(Object^ _canceler)
{
    Thread::CurrentThread->Name = "PreBuffering";
    ThreadCanceler^ canceler = (ThreadCanceler^)_canceler;

    log->DebugFormat("PreBuffering thread started.");

    while (true)
    {
        if (canceler->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation detected. Before ReadFrame().");
            break;
        }

        m_Stopwatch->Restart();

        // Read the next frame.
        // If the cache is full this will block.
        // When the frame is added to the cache it will run its eviction policy and free another frame.
        ReadResult res = ReadFrame(-1, 1, false);

        /*log->DebugFormat("ReadFrame: [{0}], {1} ms.", 
            mTimestampInfo.CurrentTimestamp, m_Stopwatch->ElapsedMilliseconds);*/

        if (canceler->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation detected. After ReadFrame().");
            break;
        }

        // Check if we hit the end of the zone.
        if (mTimestampInfo.CurrentTimestamp > mWorkingZone.End)
        {
            if (mVerbose)
                log->DebugFormat("Average prebuffering loop time: {0:0.000}ms. (Budget: {1:0.000}ms).", m_LoopWatcher->Average, mVideoInfo.FrameIntervalMilliseconds);
            
            m_LoopWatcher->Restart();
            ReadFrame(mWorkingZone.Start, 1, false);
            continue;
        }

        if (res == ReadResult::FrameNotRead)
        {
            // We got a frame-not-read but we are not yet at the end of the zone.
            log->ErrorFormat("Frame not read in the middle of the working zone. Reached timestamp:[{0}], in {1}.", mTimestampInfo.CurrentTimestamp, mWorkingZone);
            
            if (mWorkingZone.IsEmpty)
                break;

            // The most sensible thing to do is still to go back to the begining and start again, 
            // as if we just hit the end of the zone.
            m_LoopWatcher->Restart();
            ReadFrame(mWorkingZone.Start, 1, false);
            continue;
        }
    }

    log->DebugFormat("Exiting PreBuffering thread.");
}

#pragma endregion

#pragma region Logging helpers
void VideoReaderFFMpeg::LogFileInfo()
{
    log->Debug("---------------------------------------------------");
    log->Debug("[File] - Filename : " + Path::GetFileName(mVideoInfo.FilePath));
    
    // Format
    log->DebugFormat("[Format] - Format name: {0} ({1})", gcnew String(mFormatCtx->iformat->name), gcnew String(mFormatCtx->iformat->long_name));
    log->DebugFormat("[Format] - Duration (s): {0}", (double)mFormatCtx->duration / 1000000);
    log->DebugFormat("[Format] - Bit rate (bit/s): {0}", mFormatCtx->bit_rate);
    log->DebugFormat("[Format] - Start time (microseconds): {0}", mFormatCtx->start_time);
    log->DebugFormat("[Format] - Start timestamp: {0} ({1})", mVideoInfo.FirstTimeStamp, m_timestampOffset);
    LogStreamList(mFormatCtx);

    AVStream* stream = mFormatCtx->streams[mVideoStreamIndex];
    log->DebugFormat("[Stream] - Duration (frames): {0}", stream->nb_frames);
    log->DebugFormat("[Stream] - PTS wrap bits: {0}", stream->pts_wrap_bits);
    log->DebugFormat("[Stream] - TimeBase: {0}/{1}", stream->time_base.num, stream->time_base.den);
    log->DebugFormat("[Stream] - Average timestamps per seconds: {0}", mVideoInfo.AverageTimeStampsPerSeconds);

    // Codec
    log->DebugFormat("[Codec] - Name: {0}, id:{1}", gcnew String(mVideoCodecCtx->codec->name), (int)mVideoCodecCtx->codec_id);
    log->DebugFormat("[Codec] - TimeBase: {0}/{1}", mVideoCodecCtx->time_base.num, mVideoCodecCtx->time_base.den);
    log->DebugFormat("[Codec] - Bit rate (bit/s): {0}", mVideoCodecCtx->bit_rate);
    log->DebugFormat("[Codec] - Has B Frames: {0}", mVideoCodecCtx->has_b_frames);
    log->DebugFormat("[Codec] - Width (pixels): {0}", mVideoCodecCtx->width);
    log->DebugFormat("[Codec] - Height (pixels): {0}", mVideoCodecCtx->height);

    // Calculated values
    log->Debug("Duration in timestamps: " + mVideoInfo.DurationTimeStamps);
    log->Debug("Duration in seconds (computed): " + (double)mVideoInfo.DurationTimeStamps / (double)mVideoInfo.AverageTimeStampsPerSeconds);
    log->Debug("Average Fps: " + mVideoInfo.FramesPerSeconds);
    log->Debug("Average Frame Interval (ms): " + mVideoInfo.FrameIntervalMilliseconds);
    log->Debug("Average Timestamps per frame: " + mVideoInfo.AverageTimeStampsPerFrame);
    log->Debug("Pixel Aspect Ratio: " + mVideoInfo.PixelAspectRatio);
    log->Debug("Image rotation: " + mVideoInfo.ImageRotation.ToString());
    log->Debug("---------------------------------------------------");
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
        GetFrameFormatString(frame->format),
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

String^ VideoReaderFFMpeg::GetFrameFormatString(int format)
{
    AVPixelFormat pixFmt = (AVPixelFormat)format;
    switch (pixFmt)
    {
    case AV_PIX_FMT_YUV420P:
        return "AV_PIX_FMT_YUV420P";
    case AV_PIX_FMT_RGB24:
        return "AV_PIX_FMT_RGB24";
    case AV_PIX_FMT_YUV411P:
        return "AV_PIX_FMT_YUV411P";
    default:
        return format.ToString();
    }
}
#pragma endregion