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
    m_SingleFrameContainer = gcnew SingleFrame(disposer);
    m_PreBuffer = gcnew PreBuffer(disposer);
    m_Cache = gcnew Cache(disposer);
    
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
    SwitchDecodingMode(VideoDecodingMode::NotInitialized);
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
        DumpInfo();

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
        //avcodec_free_context(&mVideoCodecCtx);
        //avcodec_close(mVideoCodecCtx);
    }


    if (mFormatCtx != nullptr)
    {
        AVFormatContext* pin = mFormatCtx;
        avformat_close_input(&pin);
        mFormatCtx = pin;
    }
}

VideoSummary^ VideoReaderFFMpeg::ExtractSummary(String^ _filePath, int _thumbs, Size _maxSize)
{
    // Open the file and extract some info + a few thumbnails.
    m_Verbose = false;
    VideoSummary^ summary = gcnew VideoSummary(_filePath);

    // Allocate 100 ms to this. Always get one image but after if we run out of time we stop.
    int64_t timeout = 100;
    m_Stopwatch->Restart();

    OpenVideoResult loaded = Load(_filePath, true);

    // Debug
    DumpInfo();

    if (loaded != OpenVideoResult::Success)
        return summary;

    SwitchDecodingMode(VideoDecodingMode::OnDemand);

    summary->IsImage = mVideoInfo.DurationTimeStamps == 1;
    double durationSeconds = (mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame) / mVideoInfo.AverageTimeStampsPerSeconds;
    summary->DurationMilliseconds = (int64_t)Math::Round(durationSeconds * 1000.0);
    summary->ImageSize = mVideoInfo.ReferenceSize;
    summary->Framerate = mVideoInfo.FramesPerSeconds;

    //log->DebugFormat("ExtractSummary {0}. After load: {1} ms.", _filePath, m_Stopwatch->ElapsedMilliseconds);
    
    // Read some frames (directly decode at small size).
    float stretch = (float)mVideoInfo.OriginalSize.Width / _maxSize.Width;
    m_DecodingSize = Size(_maxSize.Width, (int)(mVideoInfo.OriginalSize.Height / stretch));

    int64_t step = (int64_t)Math::Ceiling(mVideoInfo.DurationTimeStamps / (double)_thumbs);
    int64_t previousFrameTimestamp = -1;
    

    int index = 0;
    for (int64_t ts = 0; ts < mVideoInfo.DurationTimeStamps; ts += step)
    {
        index++;
        ReadResult read = ReadResult::FrameNotRead;
        if (ts == 0)
            read = ReadFrame(-1, 1, true);
        else
            read = ReadFrame(ts, 1, true);

        //log->DebugFormat("After ReadFrame [{0}]: {1} ms.", index, m_Stopwatch->ElapsedMilliseconds);

        if (read == ReadResult::Success &&
            m_FramesContainer->CurrentFrame != nullptr &&
            mTimestampInfo.CurrentTimestamp > previousFrameTimestamp)
        {
            Bitmap^ bmp = BitmapHelper::Copy(m_FramesContainer->CurrentFrame->Image);
            summary->Thumbs->Add(bmp);
            previousFrameTimestamp = mTimestampInfo.CurrentTimestamp;
        }
        else
        {
            break;
        }

        if (m_Stopwatch->ElapsedMilliseconds > timeout)
        {
            log->WarnFormat("Thumbnail out of budget after {0} frames in {1} ms. {2}.", 
                index, m_Stopwatch->ElapsedMilliseconds, Path::GetFileName(_filePath));
            break;
        }
    }

    Close();
    return summary;
}

void VideoReaderFFMpeg::PostLoad()
{
    if (CanPreBuffer && m_DecodingMode == VideoDecodingMode::OnDemand)
    {
        SwitchDecodingMode(VideoDecodingMode::PreBuffering);

        // FIXME: use a spin loop in the caller instead of sleeping.

        // Add a small temporisation so the prebuffering thread can decode the first frame.
        // The UI thread will very soon ask for the first frame of the working zone, 
        // if it's too quick we would cancel the thread at the same time it decodes the request frame.
        //Thread::CurrentThread->Sleep(40);
        Thread::CurrentThread->Sleep(100);
    }
}

OpenVideoResult VideoReaderFFMpeg::Load(String^ _filePath, bool _forSummary)
{
    OpenVideoResult result = OpenVideoResult::Success;

    if (mIsLoaded)
    {
        Close();
    }

    mVideoInfo.FilePath = _filePath;
    if (Options == nullptr)
    {
        Options = Options->Default;
    }

    // Libav expects the filename in the computer default codepage.
    // FIXME: this breaks especially on Korean Windows.
    String^ encFilePath = System::Text::Encoding::Default->GetString(System::Text::Encoding::UTF8->GetBytes(_filePath));
    char* pszFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(encFilePath).ToPointer());
        
    // Open format.
    // TODO: muxer options.
    AVFormatContext* formatCtx = nullptr;
    if (avformat_open_input(&formatCtx, pszFilePath, NULL, NULL) != 0)
    {
        log->ErrorFormat("The file {0} could not be openned. (Wrong path or not a video/image.)", _filePath);
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
        if (!_forSummary)
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

    bool verbose = !_forSummary;
        
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

    if (_forSummary)
    {
        m_Capabilities = VideoCapabilities::CanDecodeOnDemand;
        SwitchDecodingMode(VideoDecodingMode::OnDemand);
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
        SwitchDecodingMode(VideoDecodingMode::OnDemand);
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

#pragma endregion

#pragma region Frame requests
bool VideoReaderFFMpeg::MoveNext(int _skip, bool _decodeIfNecessary)
{
    if (!mIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
        return false;

    bool moved = false;

    if (m_DecodingMode == VideoDecodingMode::OnDemand)
    {
        ReadResult res = ReadFrame(-1, _skip + 1, false);
        moved = res == ReadResult::Success;
    }
    else if (m_DecodingMode == VideoDecodingMode::Caching)
    {
        moved = m_Cache->MoveBy(_skip + 1);
    }
    else if (m_DecodingMode == VideoDecodingMode::PreBuffering)
    {
        if (!_decodeIfNecessary || m_PreBuffer->HasNext(_skip))
        {
            m_PreBuffer->MoveBy(_skip + 1);
            moved = true;
        }
        else
        {
            // Stop thread, decode frame, move to it, restart thread.
            log->DebugFormat("MoveNext, stopping pre-buffering.");
            StopPreBuffering();
            ReadResult res = ReadFrame(-1, _skip + 1, false);
            if (res == ReadResult::Success)
                moved = m_PreBuffer->MoveBy(_skip + 1);
            StartPreBuffering();
        }
    }

    return moved && HasMoreFrames();
}
bool VideoReaderFFMpeg::MoveTo(int64_t from, int64_t target)
{
    if (!mIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
        return false;

    
    bool moved = false;
    target = MapTimestamp(target);
    //log->DebugFormat("VideoReaderFFMpeg::MoveTo: {0} -> {1}.", from, target);

    if (m_DecodingMode == VideoDecodingMode::OnDemand)
    {
        ReadResult res = ReadFrame(target, 1, false);
        moved = (res == ReadResult::Success);
    }
    else if (m_DecodingMode == VideoDecodingMode::Caching)
    {
        moved = m_Cache->MoveTo(target);
    }
    else if (m_DecodingMode == VideoDecodingMode::PreBuffering)
    {
        if (m_PreBuffer->Contains(target))
        {
            //if (m_Verbose)
            //    log->DebugFormat("MoveTo. From:{0} to target:{1}. In buffer:{2}.", from, target, m_PreBuffer->Segment);
            
            moved = m_PreBuffer->MoveTo(target);
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
            if (!m_PreBuffer->IsRolloverJump(target))
            {
                //if (m_Verbose)
                //    log->DebugFormat("MoveTo. From:{0} to target:{1}. Out of buffer:{2}. Clearing buffer.", from, target, m_PreBuffer->Segment);
                
                m_PreBuffer->Clear();
            }

            // This is done on the UI thread but the decoding thread has just been put to sleep.
            m_Stopwatch->Restart();
            ReadResult res = ReadFrame(target, 1, false);
            //ReadResult res = ReadFrame(target, 1, true);
            if (m_Verbose)
                log->DebugFormat("MoveTo. Read frame in {0} ms.", m_Stopwatch->ElapsedMilliseconds);
            
            if (res == ReadResult::Success)
            {
                // The actual timestamp we land on might not be the one requested.
                int64_t actualTarget = mTimestampInfo.CurrentTimestamp;
                if (target != actualTarget)
                    AddTimestampMapping(target, actualTarget);

                moved = m_PreBuffer->MoveTo(actualTarget);
                if (m_Verbose)
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
        SwitchDecodingMode(VideoDecodingMode::PreBuffering);
    }
}

void VideoReaderFFMpeg::ResetDrops()
{
    if (m_DecodingMode == VideoDecodingMode::PreBuffering)
        m_PreBuffer->ResetDrops();
}

void VideoReaderFFMpeg::UpdateWorkingZone(VideoSection _newZone, bool _forceReload, int _maxMemory, Action<DoWorkEventHandler^>^ _workerFn)
{
    if (!mIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
        return;

    if (!CanChangeWorkingZone)
        throw gcnew CapabilityNotSupportedException();

    if (m_Verbose)
        log->DebugFormat("Update working zone request. {0} to {1}. Force reload:{2}", mWorkingZone, _newZone, _forceReload);

    if (!_forceReload && mWorkingZone == _newZone)
        return;

    if (!CanCache)
    {
        mWorkingZone = _newZone;
        if (m_DecodingMode == VideoDecodingMode::OnDemand && CanPreBuffer)
            SwitchDecodingMode(VideoDecodingMode::PreBuffering);
        else if (m_DecodingMode == VideoDecodingMode::PreBuffering)
            m_PreBuffer->UpdateWorkingZone(mWorkingZone);
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

        if (m_Verbose)
            log->DebugFormat("Working zone update. Current:{0}, Asked:{1}", mWorkingZone, _newZone);

        if (!WorkingZoneFitsInMemory(_newZone, _maxMemory))
        {
            if (m_Verbose)
                log->Debug("New working zone does not fit in memory.");

            mWorkingZone = _newZone;
            SwitchToBestAfterCaching();
        }
        else
        {
            m_SectionToPrepend = VideoSection::MakeEmpty();
            m_SectionToAppend = VideoSection::MakeEmpty();

            if (m_DecodingMode != VideoDecodingMode::Caching || _forceReload)
            {
                if (m_Verbose)
                    log->Debug("Just entering the cached mode, import everything.");

                if (m_DecodingMode == VideoDecodingMode::Caching)
                {
                    // Force a reload of the cache.
                    if (m_FramesContainer != nullptr)
                        m_FramesContainer->Clear();
                }

                SwitchDecodingMode(VideoDecodingMode::Caching);
                m_SectionToPrepend = _newZone;
            }
            else
            {
                if (_newZone.Start > mWorkingZone.Start)
                {
                    // Only do it if the new start is at least one frame beyond the old one.
                    if (_newZone.Start - mWorkingZone.Start > mVideoInfo.AverageTimeStampsPerFrame)
                    {
                        m_Cache->ReduceWorkingZone(VideoSection(_newZone.Start, mWorkingZone.End));
                        mWorkingZone = m_Cache->WorkingZone;
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
                        m_Cache->ReduceWorkingZone(VideoSection(mWorkingZone.Start, _newZone.End));
                        mWorkingZone = m_Cache->WorkingZone;
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
                bool success = ReadMany((BackgroundWorker)s, sectionToCache, prepend));
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
    if (m_DecodingMode == VideoDecodingMode::PreBuffering)
    {
        m_WasPrebuffering = true;
        SwitchDecodingMode(VideoDecodingMode::OnDemand);
    }
}

void VideoReaderFFMpeg::AfterFrameEnumeration()
{
    if (m_WasPrebuffering)
        SwitchDecodingMode(VideoDecodingMode::PreBuffering);
    m_WasPrebuffering = false;
}

void VideoReaderFFMpeg::SwitchDecodingMode(VideoDecodingMode _mode)
{
    if (_mode == m_DecodingMode)
        return;

    if (!CanSwitchDecodingMode(_mode))
        throw gcnew CapabilityNotSupportedException();

    if (m_Verbose)
        log->DebugFormat("Switching decoding mode. {0} -> {1}", m_DecodingMode.ToString(), _mode.ToString());

    if (m_DecodingMode == VideoDecodingMode::PreBuffering)
    {
        log->DebugFormat("SwitchDecodingMode, stopping pre-buffering.");
        StopPreBuffering();
        ResetDecodingSize();

        m_CanDrawUnscaled = false;
    }

    if (m_FramesContainer != nullptr)
        m_FramesContainer->Clear();

    m_DecodingMode = _mode;
    switch (m_DecodingMode)
    {
    case VideoDecodingMode::OnDemand:
        m_FramesContainer = m_SingleFrameContainer;
        break;
    case VideoDecodingMode::PreBuffering:
        m_FramesContainer = m_PreBuffer;
        m_PreBuffer->UpdateWorkingZone(mWorkingZone);
        SeekTo(mWorkingZone.Start);
        StartPreBuffering();
        break;
    case VideoDecodingMode::Caching:

        m_FramesContainer = m_Cache;
        break;
    default:
        m_FramesContainer = nullptr;
    }
}

void VideoReaderFFMpeg::SwitchToBestAfterCaching()
{
    // If we cannot enter Caching mode, switch to the next best thing.
    if (CanPreBuffer && !mWorkingZone.IsEmpty)
        SwitchDecodingMode(VideoDecodingMode::PreBuffering);
    else if (CanDecodeOnDemand)
        SwitchDecodingMode(VideoDecodingMode::OnDemand);
    else
        throw gcnew CapabilityNotSupportedException();
}

bool VideoReaderFFMpeg::WorkingZoneFitsInMemory(VideoSection _newZone, int _maxMemory)
{
    return false;

    //double durationSeconds = (double)(_newZone.End - _newZone.Start) / mVideoInfo.AverageTimeStampsPerSeconds;

    //// Loading is done at full aspect ratio size, not at the current decoding size based on the rendering container.
    //// Otherwise we would have to potentially reload the cache each time there is a stretch/squeeze request.
    //int64_t frameBytes = avpicture_get_size(sFFMpegPixelFormat, mVideoInfo.ReferenceSize.Width, mVideoInfo.ReferenceSize.Height);
    //double frameMegaBytes = (double)frameBytes / 1048576;
    //double durationMegaBytes = durationSeconds * mVideoInfo.FramesPerSeconds * frameMegaBytes;

    //return durationMegaBytes <= _maxMemory;
}

void VideoReaderFFMpeg::ImportWorkingZoneToCache(System::Object^ sender, DoWorkEventArgs^ e)
{
    return;
    //BackgroundWorker^ worker = dynamic_cast<BackgroundWorker^>(sender);

    //bool success = true;
    //if (!m_SectionToPrepend.IsEmpty)
    //    success = ReadMany(worker, m_SectionToPrepend, true);

    //if (success && !m_SectionToAppend.IsEmpty)
    //    success = ReadMany(worker, m_SectionToAppend, false);

    //if (!success)
    //    SwitchToBestAfterCaching();
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
    
    m_FramesContainer->Clear();
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
    
    m_FramesContainer->Clear();
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

    m_FramesContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeDeinterlace(bool _deint)
{
    if (!CanChangeDeinterlacing)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    Options->Deinterlace = _deint;
    m_FramesContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::SetStabilizationData(List<Kinovea::Services::TimedPoint^>^ points)
{
    // Precompute the list of frame offsets with regards to the first point of the track.
    stabOffsets->Clear();
    m_FramesContainer->Clear();
    
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

    if (m_DecodingMode != VideoDecodingMode::PreBuffering)
    {
        log->Debug("Will not change decoding size because we are not prebuffering.");
        m_CanDrawUnscaled = false;
        return false;
    }

    if (m_Verbose)
        log->DebugFormat("Changing decoding size: {0}x{1} -> {2}x{3}", 
            m_DecodingSize.Width, m_DecodingSize.Height, targetSize.Width, targetSize.Height);

    int64_t currentTimestamp = m_PreBuffer->CurrentFrame != nullptr ? m_PreBuffer->CurrentFrame->Timestamp : -1;

    log->DebugFormat("ChangeDecodingSize, stopping pre-buffering.");
    StopPreBuffering();
    m_PreBuffer->Clear();
    m_DecodingSize = targetSize;

    m_CanDrawUnscaled = true;

    if (currentTimestamp >= 0)
    {
        ReadResult res = ReadFrame(currentTimestamp, 1, false);
        if (res == ReadResult::Success)
            m_PreBuffer->MoveTo(currentTimestamp);
    }

    StartPreBuffering();

    return true;
}

void VideoReaderFFMpeg::DisableCustomDecodingSize()
{
    m_CanDrawUnscaled = false;

    if (m_DecodingMode != VideoDecodingMode::PreBuffering)
        return;

    int64_t currentTimestamp = m_PreBuffer->CurrentFrame != nullptr ? m_PreBuffer->CurrentFrame->Timestamp : -1;

    log->DebugFormat("DisableCustomDecodingSize, stopping pre-buffering.");
    StopPreBuffering();
    m_PreBuffer->Clear();
    ResetDecodingSize();

    if (currentTimestamp >= 0)
    {
        ReadResult res = ReadFrame(currentTimestamp, 1, false);
        if (res == ReadResult::Success)
            m_PreBuffer->MoveTo(currentTimestamp);
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

bool VideoReaderFFMpeg::ReadMany(BackgroundWorker^ _bgWorker, VideoSection _section, bool _prepend)
{
    // Load the asked section to cache (doesn't move the playhead).
    // Called when filling the cache with the Working Zone.
    // Might also be called internally when loading a very short video or single image.

    if (!CanCache || m_DecodingMode != VideoDecodingMode::Caching)
        throw gcnew CapabilityNotSupportedException("Importing to cache is not supported for the video.");

    if (_bgWorker != nullptr)
        Thread::CurrentThread->Name = "CacheFilling";

    if (m_Verbose)
        log->DebugFormat("Requested section to cache: {0}. Prepend:{1}", _section, _prepend);

    m_Cache->SetPrependBlock(_prepend);

    bool success = true;
    int read = 0;

    // Note: the passed section only represents what we need to prepend or append, not the target section.
    // Realign the requested section on real timestamps.
    if (!m_Cache->WorkingZone.IsEmpty)
    {
        if (_prepend && 
           (m_Cache->WorkingZone.Start - _section.Start < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // Start target is less than one frame before the current start.
            _section = VideoSection(m_Cache->WorkingZone.Start, _section.End);
        }
        else if (!_prepend && 
            (_section.End - m_Cache->WorkingZone.End < mVideoInfo.AverageTimeStampsPerFrame))
        {
            // End target is less than one frame beyond the current end.
            _section = VideoSection(_section.Start, m_Cache->WorkingZone.End);
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
            if (m_Verbose)
                log->DebugFormat("Cancellation at frame [{0}]", mTimestampInfo.CurrentTimestamp);

            m_Cache->Clear();
            success = false;
            break;
        }

        // Read one frame.
        res = ReadFrame(-1, 1, false);
        success = (res == ReadResult::Success);

        if (_bgWorker != nullptr)
            _bgWorker->ReportProgress(read++, total);
    }

    mWorkingZone = m_Cache->WorkingZone;
    m_Cache->SetPrependBlock(false);

    // Sometimes a few frames at the end can't be read.
    if (mTimestampInfo.CurrentTimestamp < _section.End && read < total)
    {
        log->ErrorFormat("Caching section: could only read {0} out of {1} frames.", read, total);
    
        if (read >= (total - 1) * 0.95)
        {
            mWorkingZone = m_Cache->WorkingZone;
            success = true;
        }
    }

    return success;
}

ReadResult VideoReaderFFMpeg::ReadFrame(int64_t _iTimeStampToSeekTo, int _iFramesToDecode, bool _approximate)
{
    //------------------------------------------------------------------------------------
    // Reads a frame and adds it to the frame cache.
    // This function works either for MoveTo or MoveNext type of requests.
    // It decodes as many frames as needed to reach the target timestamp 
    // or the number of frames to decode. Seeks backwards if needed.
    //
    // The _approximate flag is used for thumbnails retrieval. 
    // In this case we don't really care to land exactly on the right frame,
    // so we return after the first decode post-seek.
    //------------------------------------------------------------------------------------

    //if (Thread::CurrentThread->Name != "PreBuffering")
    //    log->DebugFormat("ReadFrame: seek:{0}, decode:{1}.", _iTimeStampToSeekTo, _iFramesToDecode);

    return ReadResult::FrameNotRead;

    //m_LoopWatcher->LoopStart();

    //// TODO: shouldn't need to lock. Make sure we don't synchronously ask for a frame while prebuffering.
    //lock l(m_Locker);

    //if (!mIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
    //    return ReadResult::MovieNotLoaded;

    //if (m_FramesContainer == nullptr)
    //    return ReadResult::FrameContainerNotSet;

    //ReadResult result = ReadResult::Success;
    //int	iFramesToDecode = _iFramesToDecode;
    //int64_t iTargetTimeStamp = _iTimeStampToSeekTo;
    //bool seeking = false;

    //// Find the proper target and number of frames to decode.
    //if (_iFramesToDecode < 0)
    //{
    //    // Negative move. Compute seek target.
    //    iTargetTimeStamp = (int64_t)Math::Round(m_FramesContainer->CurrentFrame->Timestamp + (_iFramesToDecode * mVideoInfo.AverageTimeStampsPerFrame));
    //    if (iTargetTimeStamp < 0)
    //        iTargetTimeStamp = 0;
    //}

    //if (iTargetTimeStamp >= 0)
    //{
    //    seeking = true;
    //    iFramesToDecode = 1; // We'll use the target timestamp anyway.
    //    int iSeekRes = SeekTo(iTargetTimeStamp);
    //    if (iSeekRes < 0)
    //    {
    //        log->ErrorFormat("Error during seek. Error code:{0}. Seek target was:[{1}]", iSeekRes, iTargetTimeStamp);
    //        seeking = false;
    //    }
    //}

    //// Allocate 2 AVFrames, one for the raw decoded frame and one for deinterlaced/rescaled/converted frame.
    //AVFrame* pDecodingAVFrame = av_frame_alloc();
    //AVFrame* pFinalAVFrame = av_frame_alloc();

    //// The buffer holding the actual frame data.
    //int iSizeBuffer = avpicture_get_size(sFFMpegPixelFormat, m_DecodingSize.Width, m_DecodingSize.Height);
    //uint8_t* pBuffer = iSizeBuffer > 0 ? new uint8_t[iSizeBuffer] : nullptr;

    //if (pDecodingAVFrame == nullptr || pFinalAVFrame == nullptr || pBuffer == nullptr)
    //    return ReadResult::MemoryNotAllocated;

    //// Assigns appropriate parts of buffer to image planes in the AVFrame.
    //avpicture_fill((AVPicture*)pFinalAVFrame, pBuffer, sFFMpegPixelFormat, m_DecodingSize.Width, m_DecodingSize.Height);

    //mTimestampInfo.CurrentTimestamp = m_FramesContainer->CurrentFrame == nullptr ? -1 : m_FramesContainer->CurrentFrame->Timestamp;

    //// Reading/Decoding loop
    //bool done = false;
    //bool bFirstPass = true;
    //int iReadFrameResult;
    //int gotPicturePtr = 0;
    //int	iFramesDecoded = 0;
    //do
    //{
    //    // FFMpeg also has an internal buffer to cope with B-Frames entanglement.
    //    // The DTS/PTS announced is actually the one of the last frame that was put in the buffer by av_read_frame,
    //    // it is *not* the one of the frame that was extracted from the buffer by avcodec_decode_video.
    //    // To solve the DTS/PTS issue, we save the timestamps each time we find libav is buffering a frame.
    //    // And we use the previously saved timestamps.
    //    // Ref: http://lists.mplayerhq.hu/pipermail/libav-user/2008-August/001069.html

    //    // Read next packet
    //    AVPacket inputPacket;
    //    iReadFrameResult = av_read_frame(mFormatCtx, &inputPacket);
    //    if (iReadFrameResult < 0)
    //    {
    //        // Reading error. We don't know if the error happened on a video frame or audio one.
    //        done = true;
    //        delete[] pBuffer;
    //        result = ReadResult::FrameNotRead;
    //        break;
    //    }

    //    if (inputPacket.stream_index != mVideoStreamIndex)
    //    {
    //        av_free_packet(&inputPacket);
    //        continue;
    //    }

    //    // Decode video packet. This is needed even if we're not on the final frame yet.
    //    // I-Frame data is kept internally by ffmpeg which will need it to build the final frame.
    //    avcodec_decode_video2(mVideoCodecCtx, pDecodingAVFrame, &gotPicturePtr, &inputPacket);
    //    if (gotPicturePtr == 0)
    //    {
    //        // Buffering frame. libav just read a I or P frame that will be presented later.
    //        // (But which was necessary to get now in order to decode a coming B frame.)
    //        av_free_packet(&inputPacket);
    //        continue;
    //    }

    //    int64_t beTimestamp = pDecodingAVFrame->best_effort_timestamp;
    //    if (beTimestamp < m_timestampOffset)
    //    {
    //        m_timestampOffset = beTimestamp;
    //        if (m_Verbose)
    //            log->DebugFormat("Negative timestamp received. Applying new timestamp offset of {0}.", m_timestampOffset);
    //    }

    //    mTimestampInfo.CurrentTimestamp = beTimestamp - m_timestampOffset;

    //    if (seeking && bFirstPass && !_approximate && iTargetTimeStamp >= 0 && mTimestampInfo.CurrentTimestamp > iTargetTimeStamp)
    //    {
    //        // If the current ts is already after the target, we are dealing with this kind of files
    //        // where the seek doesn't work as advertised. We'll seek back again further,
    //        // and then decode until we get to it.

    //        // Do this only once.
    //        bFirstPass = false;

    //        // For some files, one additional second back is not enough. The seek is wrong by up to 4 seconds.
    //        // We also allow the target to go before 0.
    //        int iSecondsBack = 4;
    //        int64_t iForceSeekTimestamp = (int64_t)(iTargetTimeStamp - (mVideoInfo.AverageTimeStampsPerSeconds * iSecondsBack));
    //        int64_t iMinTarget = System::Math::Min(iForceSeekTimestamp, (int64_t)0);

    //        // Do the seek.
    //        if (m_Verbose)
    //        {
    //            log->DebugFormat("[Seek] - First decoded frame [{0}] already after target [{1}]. Force seek {2} more seconds back to [{3}]",
    //                mTimestampInfo.CurrentTimestamp, iTargetTimeStamp, iSecondsBack, iForceSeekTimestamp);
    //        }

    //        avformat_seek_file(mFormatCtx, mVideoStreamIndex, iMinTarget + m_timestampOffset, iForceSeekTimestamp + m_timestampOffset, iForceSeekTimestamp + m_timestampOffset, AVSEEK_FLAG_BACKWARD);
    //        avcodec_flush_buffers(mFormatCtx->streams[mVideoStreamIndex]->codec);

    //        // Free the packet that was allocated by av_read_frame
    //        av_free_packet(&inputPacket);

    //        // Loop back to restart decoding frames until we get to the target.
    //        continue;
    //    }

    //    bFirstPass = false;
    //    iFramesDecoded++;

    //    //-------------------------------------------------------------------------------
    //    // If we're done, convert the image and store it into its final recipient.
    //    // - seek: if we reached the target timestamp.
    //    // - linear decoding: if we decoded the required number of frames.
    //    //-------------------------------------------------------------------------------
    //    if (seeking && mTimestampInfo.CurrentTimestamp >= iTargetTimeStamp ||
    //        !seeking && iFramesDecoded >= iFramesToDecode ||
    //        _approximate)
    //    {
    //        done = true;

    //        if (m_Verbose && seeking /* && mTimestampInfo.CurrentTimestamp != iTargetTimeStamp*/)
    //        {
    //            log->DebugFormat("Seeking to [{0}] completed. Final position:[{1}], decoded: {2} frames.", 
    //                iTargetTimeStamp, mTimestampInfo.CurrentTimestamp, iFramesDecoded);
    //        }

    //        // Deinterlace + rescale + convert pixel format.
    //        bool rescaled = RescaleAndConvert(
    //            pFinalAVFrame,
    //            pDecodingAVFrame,
    //            m_DecodingSize.Width,
    //            m_DecodingSize.Height,
    //            sFFMpegPixelFormat,
    //            Options->Deinterlace);

    //        if (!rescaled)
    //        {
    //            delete[] pBuffer;
    //            result = ReadResult::ImageNotConverted;
    //            break;
    //        }

    //        try
    //        {
    //            // Import ffmpeg buffer into a .NET bitmap.
    //            int imageStride = pFinalAVFrame->linesize[0];
    //            IntPtr scan0 = IntPtr((void*)pFinalAVFrame->data[0]);
    //            Bitmap^ bmp = nullptr;
    //            if (stabOffsets->ContainsKey(mTimestampInfo.CurrentTimestamp))
    //            {
    //                // Image stabilization. Paint the image with the offset applied.
    //                // Prepare output bitmap.
    //                bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, DecodingPixelFormat);

    //                // Get the decoded frame in a bitmap and paint it over the output.
    //                Bitmap^ bmp2 = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, scan0);
    //                Graphics^ g = Graphics::FromImage(bmp);
    //                float dx = stabOffsets[mTimestampInfo.CurrentTimestamp]->X;
    //                float dy = stabOffsets[mTimestampInfo.CurrentTimestamp]->Y;
    //                // TODO: handle scaling (decoding size).
    //                g->DrawImageUnscaled(bmp2, (int)(-dx), (int)(-dy));
    //                delete g;
    //                delete bmp2;
    //            }
    //            else
    //            {
    //                bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, scan0);
    //            }

    //            // Rotation is handled after scaling and aspect ratio fix for simplicity.
    //            // In later versions of FFMpeg there are rotation routines built in, that might be simpler and faster.
    //            switch (mVideoInfo.ImageRotation)
    //            {
    //            case ImageRotation::Rotate90:
    //                bmp->RotateFlip(RotateFlipType::Rotate90FlipNone);
    //                break;
    //            case ImageRotation::Rotate180:
    //                bmp->RotateFlip(RotateFlipType::Rotate180FlipNone);
    //                break;
    //            case ImageRotation::Rotate270:
    //                bmp->RotateFlip(RotateFlipType::Rotate270FlipNone);
    //                break;
    //            default:
    //                break;
    //            }

    //            // Store a pointer to the native buffer inside the Bitmap.
    //            // We'll be asked to free this resource later when the frame is not used anymore.
    //            // It is boxed inside an Object so we can extract it in a type-safe way.
    //            IntPtr^ boxedPtr = gcnew IntPtr((void*)pBuffer);
    //            bmp->Tag = boxedPtr;

    //            // Construct the VideoFrame and push it to the current container.
    //            VideoFrame^ vf = gcnew VideoFrame();
    //            vf->Image = bmp;
    //            vf->Timestamp = mTimestampInfo.CurrentTimestamp;

    //            m_LoopWatcher->LoopEnd();

    //            // Finally, add the frame to the container.
    //            m_FramesContainer->Add(vf);
    //        }
    //        catch (Exception^ exp)
    //        {
    //            delete[] pBuffer;
    //            result = ReadResult::ImageNotConverted;
    //            log->Error("Error while converting AVFrame to Bitmap.");
    //            log->Error(exp);
    //        }
//        }
//
//        // Free the packet that was allocated by av_read_frame
//        av_free_packet(&inputPacket);
//    } while (!done);
//
//    // Free the AVFrames. (This will not deallocate the data buffers).
//    av_free(pFinalAVFrame);
//    av_free(pDecodingAVFrame);
//
//#ifdef INSTRUMENTATION	
//    if (m_FramesContainer->Current != nullptr)
//        log->DebugFormat("[{0}] - Memory: {1:0,0} bytes", m_PreBuffer->CurrentFrame->Timestamp, Process::GetCurrentProcess()->PrivateMemorySize64);
//#endif
//
//    if (!m_bFirstFrameRead)
//    {
//        m_bFirstFrameRead = true;
//        mVideoInfo.FirstTimeStamp = mTimestampInfo.CurrentTimestamp;
//        mWorkingZone = VideoSection(mVideoInfo.FirstTimeStamp, mWorkingZone.End);
//    }

    //return result;
}

int VideoReaderFFMpeg::SeekTo(int64_t _target)
{
    return -1;

    //// Perform an FFMpeg seek without decoding the frame.
    //// AVSEEK_FLAG_BACKWARD -> goes to first I-Frame before target.
    //// Then we'll need to decode frame by frame until the target is reached.
    //int64_t minTs = m_timestampOffset;
    //int64_t ts = _target + m_timestampOffset;
    //int64_t maxTs = (int64_t)(_target + m_timestampOffset + mVideoInfo.AverageTimeStampsPerSeconds);

    //int res = avformat_seek_file(
    //    mFormatCtx,
    //    mVideoStreamIndex,
    //    minTs,
    //    ts,
    //    maxTs,
    //    AVSEEK_FLAG_BACKWARD);

    //avcodec_flush_buffers(mFormatCtx->streams[mVideoStreamIndex]->codec);
    //mTimestampInfo = TimestampInfo::Empty;
    //return res;
}

bool VideoReaderFFMpeg::RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _decodingWidth, int _decodingHeight, int _outputFmt, bool _deinterlace)
{
    return false;

    ////------------------------------------------------------------------------
    //// Utility function called by ReadFrame().
    //// Take the frame we just decoded and turn it to the right size/deint/fmt.
    //// todo: sws_getContext could be done only once.
    ////------------------------------------------------------------------------
    //bool bSuccess = true;
    //AVPixelFormat srcFormat = mVideoCodecCtx->pix_fmt;
    //if (CanChangeDemosaicing)
    //{
    //    switch (Options->Demosaicing)
    //    {
    //    case Demosaicing::RGGB:
    //        srcFormat = AV_PIX_FMT_BAYER_RGGB8;
    //        break;
    //    case Demosaicing::BGGR:
    //        srcFormat = AV_PIX_FMT_BAYER_BGGR8;
    //        break;
    //    case Demosaicing::GRBG:
    //        srcFormat = AV_PIX_FMT_BAYER_GRBG8;
    //        break;
    //    case Demosaicing::GBRG:
    //        srcFormat = AV_PIX_FMT_BAYER_GBRG8;
    //        break;
    //    case Demosaicing::None:
    //    default:
    //        srcFormat = mVideoCodecCtx->pix_fmt;
    //        break;
    //    }
    //}

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

void VideoReaderFFMpeg::DisposeFrame(VideoFrame^ _frame)
{
    // Dispose the Bitmap and the native buffer.
    // The pointer to the native buffer was stored in the Tag property.
    IntPtr^ ptr = dynamic_cast<IntPtr^>(_frame->Image->Tag);
    delete _frame->Image;

    if (ptr != nullptr)
    {
        // Fixme: why is the delete [] taking more than 1ms ?
        uint8_t* pBuf = (uint8_t*)ptr->ToPointer();
        delete[] pBuf;
    }
}

#pragma endregion

#pragma region PreBuffering thread

void VideoReaderFFMpeg::StartPreBuffering()
{
    if (!CanPreBuffer)
        throw gcnew CapabilityNotSupportedException();

    if (m_DecodingMode == VideoDecodingMode::Caching)
        return;

    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
    {
        log->Error("Prebuffering thread already started");
        StopPreBuffering();
        m_PreBuffer->Clear();
        //debug - just to check when we could pass here.
        //throw gcnew CapabilityNotSupportedException();
    }

    if (m_Verbose)
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

    if (m_Verbose)
        log->Debug("Stopping prebuffering thread.");

    m_PreBufferingThreadCanceler->Cancel();

    // The cancellation will only be effective when we next pass in the 
    // decoding loop and check the cancellation flag. This means that if the thread is in waiting state, 
    // (trying to push a frame to an already full buffer), the cancellation will not proceed.
    // UnblockAndMakeRoom will force a Pulse, dequeing a frame if necessary.
    // However, if we just make room for one frame and it's the UI thread that is doing the Add,
    // it will be blocked after the addition since the buffer will again be full. 
    // We must actually make sure the next Read operation won't block.
    m_PreBuffer->UnblockAndMakeRoom();

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
            if (m_Verbose)
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

#pragma region Debug dumps
void VideoReaderFFMpeg::DumpInfo()
{
    log->Debug("---------------------------------------------------");
    log->Debug("[File] - Filename : " + Path::GetFileName(mVideoInfo.FilePath));
    
    // Format
    log->DebugFormat("[Format] - Format name: {0} ({1})", gcnew String(mFormatCtx->iformat->name), gcnew String(mFormatCtx->iformat->long_name));
    log->DebugFormat("[Format] - Duration (s): {0}", (double)mFormatCtx->duration / 1000000);
    log->DebugFormat("[Format] - Bit rate (bit/s): {0}", mFormatCtx->bit_rate);
    log->DebugFormat("[Format] - Start time (microseconds): {0}", mFormatCtx->start_time);
    log->DebugFormat("[Format] - Start timestamp: {0} ({1})", mVideoInfo.FirstTimeStamp, m_timestampOffset);
    DumpStreamsInfos(mFormatCtx);

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

void VideoReaderFFMpeg::DumpStreamsInfos(AVFormatContext* formatCtx)
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

void VideoReaderFFMpeg::DumpFrameType(int type)
{
    switch (type)
    {
    case AV_PICTURE_TYPE_I:
        log->Debug("(I) Frame +++++");
        break;
    case AV_PICTURE_TYPE_P:
        log->Debug("(P) Frame --");
        break;
    case AV_PICTURE_TYPE_B:
        log->Debug("(B) Frame .");
        break;
    case AV_PICTURE_TYPE_S:
        log->Debug("Frame : S(GMC)-VOP MPEG4");
        break;
    case AV_PICTURE_TYPE_SI:
        log->Debug("Switching Intra");
        break;
    case AV_PICTURE_TYPE_SP:
        log->Debug("Switching Predicted");
        break;
    case AV_PICTURE_TYPE_BI:
        log->Debug("FF_BI_TYPE");
        break;
    }
}
#pragma endregion