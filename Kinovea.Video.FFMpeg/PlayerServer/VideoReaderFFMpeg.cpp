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
    mLocker = gcnew Object();
    mPreBufferingThreadCanceler = gcnew ThreadCanceler();

    VideoFrameDisposer^ disposer = gcnew VideoFrameDisposer(DisposeFrame);

    mSingleFrameContainer = gcnew SingleFrame(disposer);
    mPreBuffer = gcnew PreBuffer(disposer);
    mCache = gcnew Cache(disposer);
    
    mLoopWatcher = gcnew LoopWatcher();
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
    mWasPrebuffering = false;
    mCanDrawUnscaled = false;
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
    VideoSummary^ summary = gcnew VideoSummary(filePath);

    // Allocate 100 ms to this task. 
    // Always get at least one image but after that if we run out of time we cancel.
    int64_t timeout = 100;
    mStopwatchRead->Restart();

    OpenVideoResult loaded = Load(filePath, true);

    if (loaded != OpenVideoResult::Success)
    {
        return summary;
    }

    ChangeCachingMode(VideoDecodingMode::OnDemand);

    summary->IsImage = mVideoInfo.DurationTimeStamps == 1;
    double durationSeconds = mVideoInfo.DurationTimeStamps / mVideoInfo.AverageTimeStampsPerSeconds;
    summary->DurationMilliseconds = (int64_t)Math::Round(durationSeconds * 1000.0);
    summary->ImageSize = mVideoInfo.ReferenceSize;
    summary->Framerate = mVideoInfo.FramesPerSeconds;

    //log->DebugFormat("ExtractSummary {0}. After load: {1} ms.", filePath, mStopwatch->ElapsedMilliseconds);
    
    // Read some frames (directly decode at small size).
    float stretch = (float)mVideoInfo.OriginalSize.Width / maxSize.Width;
    mDecodingSize = Size(maxSize.Width, (int)(mVideoInfo.OriginalSize.Height / stretch));

    int64_t step = (int64_t)Math::Ceiling((double)mVideoInfo.DurationTimeStamps / count);
    int64_t previousFrameTimestamp = -1;
    
    int index = 0;
    for (int64_t ts = 0; ts < mVideoInfo.DurationTimeStamps; ts += step)
    {
        index++;
        ReadResult read = ReadFrame(ts == 0 ? -1 : ts, 1, true);
        
        //log->DebugFormat("After ReadFrame #{0} [{1}]: {2} ms.", index, mTimestampInfo.CurrentTimestamp, mStopwatch->ElapsedMilliseconds);

        if (read == ReadResult::Success &&
            mFrameContainer->CurrentFrame != nullptr &&
            mTimestampInfo.CurrentTimestamp > previousFrameTimestamp)
        {
            Bitmap^ bmp = BitmapHelper::CopyBgr32Rows(mFrameContainer->CurrentFrame->Image);
            summary->Thumbs->Add(bmp);
            previousFrameTimestamp = mTimestampInfo.CurrentTimestamp;
        }
        else
        {
            // Bail out on reading error.
            break;
        }

        if (mStopwatchRead->ElapsedMilliseconds > timeout)
        {
            log->WarnFormat("Thumbnail out of budget after {0} frames in {1} ms. {2}.", 
                index, mStopwatchRead->ElapsedMilliseconds, Path::GetFileName(filePath));
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

    AVFormatContext* formatCtx = nullptr;
    
    // FFmpeg expects filenames as UTF-8, .NET strings are UTF-16.
    // On Windows, FFmpeg converts UTF-8 file paths back to UTF-16 internally.
    int byteCount = Encoding::UTF8->GetByteCount(filePath);
    array<Byte>^ utf8Path = gcnew array<Byte>(byteCount + 1);
    Encoding::UTF8->GetBytes(filePath, 0, filePath->Length, utf8Path, 0);
    pin_ptr<Byte> pinnedPath = &utf8Path[0];
    const char* pszFilePath = reinterpret_cast<const char*>(pinnedPath);
    
    if (avformat_open_input(&formatCtx, pszFilePath, nullptr, nullptr) != 0)
    {
        log->ErrorFormat("The file {0} could not be openned. (Wrong path or not a video/image.)", filePath);
        return OpenVideoResult::FileNotOpenned;
    }

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


    // Enable multithreading.
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

    res = avcodec_open2(videoCodecCtx, videoCodec, nullptr);
    if (res < 0) 
    {
        log->ErrorFormat("Codec could not be openned. Error: {0}", res);
        return OpenVideoResult::CodecNotOpened;
    }
                
    //-----------------------------------------------------
    // Time info
    //-----------------------------------------------------
    // videoStream->nb_frames == 0 can happen.
    // videoStream->duration <= 0 can happen.
    bool verbose = !forSummary;
    mVideoInfo.AverageTimeStampsPerSeconds = (double)videoStream->time_base.den / (double)videoStream->time_base.num;

    // This may be updated after the first actual decoding.
    double startSeconds = (double)formatCtx->start_time / AV_TIME_BASE;
    long firstTimestamp = (long)Math::Round(startSeconds * mVideoInfo.AverageTimeStampsPerSeconds);
    mVideoInfo.FirstTimeStamp = Math::Max(firstTimestamp, 0);

    // Ignore negative start time.

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

    // Working zone representing the whole video.
    mWorkingZone = VideoSection(
        mVideoInfo.FirstTimeStamp,
        (int64_t)Math::Round(mVideoInfo.FirstTimeStamp + mVideoInfo.DurationTimeStamps - mVideoInfo.AverageTimeStampsPerFrame));

    //-----------------------------------------------------
    // Image size info
    //-----------------------------------------------------

    // Image rotation.
    mVideoInfo.ImageRotation = ImageRotation::Rotate0;
    const AVPacketSideData* displaymatrix = av_packet_side_data_get(videoStream->codecpar->coded_side_data, videoStream->codecpar->nb_coded_side_data, AV_PKT_DATA_DISPLAYMATRIX);
    if (displaymatrix)
    {
        // Get rotation as a double in [-180..+180].
        double rotation = Math::Round(av_display_rotation_get((const int32_t*)displaymatrix->data));
        // Map to 0..360 range.
        // Ignore rotations that aren't multiples of 90.
        rotation = ((int)-rotation + 360) % 360;
        if (rotation == 90)
            mVideoInfo.ImageRotation = ImageRotation::Rotate90;
        else if (rotation == 180)
            mVideoInfo.ImageRotation = ImageRotation::Rotate180;
        else if (rotation == 270)
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
        mCapabilities = VideoCapabilities::CanDecodeOnDemand;
        ChangeCachingMode(VideoDecodingMode::OnDemand);
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
            VideoCapabilities::CanChangeDecodingSize |
            VideoCapabilities::CanStabilize;

        if (mVideoCodecCtx->codec_id == AV_CODEC_ID_RAWVIDEO)
        {
            mCapabilities = mCapabilities | VideoCapabilities::CanChangeDemosaicing;
        }

        // Start with no caching, we'll switch later.
        ChangeCachingMode(VideoDecodingMode::OnDemand);
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
        mStopwatchRead->Restart();
        ReadResult res = ReadFrame(-1, _skip + 1, false);
        log->DebugFormat("MoveNext. Read frame in {0} ms.", mStopwatchRead->ElapsedMilliseconds);
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

            mStopwatchRead->Restart();

            ReadResult res = ReadFrame(target, 1, false);

            if (mVerbose)
            {
                log->DebugFormat("MoveTo. Read frame in {0} ms.", mStopwatchRead->ElapsedMilliseconds);
            }

            if (res == ReadResult::Success)
            {
                // The actual timestamp we land on might not be the one requested.
                int64_t actualTarget = mTimestampInfo.CurrentTimestamp;
                if (target != actualTarget)
                {
                    AddTimestampMapping(target, actualTarget);
                }

                moved = mPreBuffer->MoveTo(actualTarget);
                if (mVerbose)
                {
                    log->DebugFormat("MoveTo. Moved to {0}.", actualTarget);
                }
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
            mSectionToPrepend = VideoSection::MakeEmpty();
            mSectionToAppend = VideoSection::MakeEmpty();

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
                mSectionToPrepend = _newZone;
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
                    mSectionToPrepend = VideoSection(_newZone.Start, mWorkingZone.Start);
                }

                // Expand at the back if there is more than one frame to expand.
                if (_newZone.End - mWorkingZone.End > mVideoInfo.AverageTimeStampsPerFrame)
                {
                    mSectionToAppend = VideoSection(mWorkingZone.End, _newZone.End);
                }
            }

            if (!mSectionToPrepend.IsEmpty || !mSectionToAppend.IsEmpty)
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
        mWasPrebuffering = true;
        ChangeCachingMode(VideoDecodingMode::OnDemand);
    }
}

void VideoReaderFFMpeg::AfterFrameEnumeration()
{
    if (mWasPrebuffering)
        ChangeCachingMode(VideoDecodingMode::PreBuffering);
    mWasPrebuffering = false;
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

        mCanDrawUnscaled = false;
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
        ChangeToBestAfterCaching();
    }
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
    if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
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
    if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
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
    if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
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
    mStabOffsets->Clear();
    mFrameContainer->Clear();
    
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

#pragma region Decoding size

bool VideoReaderFFMpeg::ChangeDecodingSize(Size _size)
{
    // Should return true if we are going to use this size.

    if (!CanChangeDecodingSize)
        throw gcnew CapabilityNotSupportedException();

    bool sideway = mVideoInfo.ImageRotation == ImageRotation::Rotate90 || mVideoInfo.ImageRotation == ImageRotation::Rotate270;
    Size targetSize = FixSize(_size, sideway);
    if (targetSize == mDecodingSize)
    {
        // No change required. If we are not in pre-buffering, the decoding size is already the reference size.
        mCanDrawUnscaled = true;
        return true;
    }

    if (mCachingMode != VideoDecodingMode::PreBuffering)
    {
        log->Debug("Will not change decoding size because we are not prebuffering.");
        mCanDrawUnscaled = false;
        return false;
    }

    if (mVerbose)
        log->DebugFormat("Changing decoding size: {0}x{1} -> {2}x{3}", 
            mDecodingSize.Width, mDecodingSize.Height, targetSize.Width, targetSize.Height);

    int64_t currentTimestamp = mPreBuffer->CurrentFrame != nullptr ? mPreBuffer->CurrentFrame->Timestamp : -1;

    log->DebugFormat("ChangeDecodingSize, stopping pre-buffering.");
    StopPreBuffering();
    mPreBuffer->Clear();
    mDecodingSize = targetSize;

    mCanDrawUnscaled = true;

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
    mCanDrawUnscaled = false;

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
    mDecodingSize = mVideoInfo.AspectRatioSize;
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

    bool success = true;
    int read = 0;

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

    double end = section.End + (mVideoInfo.AverageTimeStampsPerFrame * 0.5);
    int totalFrames = (int)Math::Floor((end - section.Start) / mVideoInfo.AverageTimeStampsPerFrame);
    log->DebugFormat("Frames to cache: {0}", totalFrames);

    // Bail out if re-alignment revealed we don't need to cache anything new.
    if (totalFrames == 0)
    {
        return true;
    }

    // Read the first frame with seek.
    ReadResult res = ReadFrame(section.Start, 1, false);
    success = (res == ReadResult::Success);

    // Continue reading frames until we have the right number, we are past the target, or EOF.
    while ((mTimestampInfo.CurrentTimestamp < section.End) &&
           (read < totalFrames) && 
           (res == ReadResult::Success))
    {
        if (bgWorker->CancellationPending)
        {
            if (mVerbose)
            {
                log->DebugFormat("Cancellation at frame [{0}]", mTimestampInfo.CurrentTimestamp);
            }

            mCache->Clear();
            success = false;
            break;
        }

        // Read the next frame.
        res = ReadFrame(-1, 1, false);
        success = (res == ReadResult::Success);

        if (success)
        {
            read = read + 1;
            bgWorker->ReportProgress(read, totalFrames);
        }
    }

    if (!bgWorker->CancellationPending)
    {
        mWorkingZone = mCache->WorkingZone;
    }

    mCache->SetPrepending(false);
    return success;
}


ReadResult VideoReaderFFMpeg::ReadFrame(int64_t targetTimestamp, int targetFrameJump, bool approximate)
{
    mLoopWatcher->LoopStart();

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
    lock l(mLocker);

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

    // Log but don't fail if seeking landed beyond the target.
    // Might happen if the very first packet is not a keyframe, 
    // possibly from cut-off stream or corrupted file.
    if (seeking && !approximate && frame->best_effort_timestamp > targetTimestamp)
    {
        log->WarnFormat("Seek({0}) landed at {1}. Frame type: {2}",
            targetTimestamp,
            frame->best_effort_timestamp,
            GetFrameTypeString(frame->pict_type));
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
                    if (res == AVERROR_EOF)
                    {
                        result = ReadResult::EOFReached;
                        break;
                    }
                    else
                    {
                        continue;
                    }
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

    mTimestampInfo = TimestampInfo::Empty;
    return res;
}


ReadResult VideoReaderFFMpeg::ConvertAndStoreFrame(AVFrame* decodedFrame)
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
        convertedFrame->width = mDecodingSize.Width;
        convertedFrame->height = mDecodingSize.Height;
        int res = av_frame_get_buffer(convertedFrame, 1);
        if (res < 0)
        {
            LogFFMpegError("av_frame_get_buffer", res);
            av_frame_free(&convertedFrame);
            return ReadResult::MemoryNotAllocated;
        }
    }

    // Deinterlace, Scale and Convert the decoded AVFrame.
    bool converted = RescaleAndConvert(
        decodedFrame, 
        convertedFrame, 
        mDecodingSize.Width, 
        mDecodingSize.Height, 
        sConvertPixelFormat, 
        Options->Deinterlace);

    if (!converted)
    {
        av_frame_free(&convertedFrame);
        return ReadResult::ImageNotConverted;
    }

    // Wrap the data buffer in a Bitmap.
    int stride = convertedFrame->linesize[0];
    IntPtr data = IntPtr(convertedFrame->data[0]);
    Bitmap^ bmp = nullptr;

    if (mStabOffsets->ContainsKey(mTimestampInfo.CurrentTimestamp))
    {
        // Image stabilization. 

        // Wrap the native AVFrame in a bitmap.
        Bitmap^ bmp2 = gcnew Bitmap(
            mDecodingSize.Width,
            mDecodingSize.Height,
            stride,
            DecodingPixelFormat,
            data);

        bmp = gcnew Bitmap(mDecodingSize.Width, mDecodingSize.Height, DecodingPixelFormat);
        Graphics^ g = Graphics::FromImage(bmp);
        float dx = mStabOffsets[mTimestampInfo.CurrentTimestamp]->X;
        float dy = mStabOffsets[mTimestampInfo.CurrentTimestamp]->Y;
        
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
            mDecodingSize.Width, 
            mDecodingSize.Height, 
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
    // fast-bilinear is speed over quality, not needed for the use case.
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

void VideoReaderFFMpeg::StartPreBuffering()
{
    if (!CanPreBuffer)
        throw gcnew CapabilityNotSupportedException();

    if (mCachingMode == VideoDecodingMode::Caching)
        return;

    if (mPreBufferingThread != nullptr && mPreBufferingThread->IsAlive)
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
    mPreBufferingThreadCanceler->Reset();
    mPreBufferingThread = gcnew Thread(pts);
    mPreBufferingThread->Start(mPreBufferingThreadCanceler);
}

void VideoReaderFFMpeg::StopPreBuffering()
{
    if (mPreBufferingThread == nullptr || !mPreBufferingThread->IsAlive)
        return;

    if (mVerbose)
        log->Debug("Stopping prebuffering thread.");

    mPreBufferingThreadCanceler->Cancel();

    // The cancellation will only be effective when we next pass in the 
    // decoding loop and check the cancellation flag. This means that if the thread is in waiting state, 
    // (trying to push a frame to an already full buffer), the cancellation will not proceed.
    // UnblockAndMakeRoom will force a Pulse, dequeing a frame if necessary.
    // However, if we just make room for one frame and it's the UI thread that is doing the Add,
    // it will be blocked after the addition since the buffer will again be full. 
    // We must actually make sure the next Read operation won't block.
    mPreBuffer->UnblockAndMakeRoom();

    mPreBufferingThread->Join();
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


        // Read the next frame.
        // If the cache is full this will block.
        // When the frame is added to the cache it will run its eviction policy and free another frame.
        mStopwatchRead->Restart();
        ReadResult res = ReadFrame(-1, 1, false);
        log->DebugFormat("ReadFrame: [{0}], {1} ms.", mTimestampInfo.CurrentTimestamp, mStopwatchRead->ElapsedMilliseconds);

        if (canceler->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation detected. After ReadFrame().");
            break;
        }

        // Check if we hit the end of the zone.
        if (mTimestampInfo.CurrentTimestamp > mWorkingZone.End || res == ReadResult::EOFReached)
        {
            if (mVerbose)
                log->DebugFormat("Average prebuffering loop time: {0:0.000}ms. (Budget: {1:0.000}ms).", mLoopWatcher->Average, mVideoInfo.FrameIntervalMilliseconds);
            
            mLoopWatcher->Restart();
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
            mLoopWatcher->Restart();
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
    log->DebugFormat("[Format] - Duration (s): {0}", (double)mFormatCtx->duration / AV_TIME_BASE);
    log->DebugFormat("[Format] - Bit rate (bit/s): {0}", mFormatCtx->bit_rate);
    log->DebugFormat("[Format] - Start time (microseconds): {0}", mFormatCtx->start_time);
    log->DebugFormat("[Format] - Start timestamp: {0} ({1})", mVideoInfo.FirstTimeStamp, mTimestampOffset);
    LogStreamList(mFormatCtx);

    AVStream* stream = mFormatCtx->streams[mVideoStreamIndex];
    log->DebugFormat("[Stream] - Duration (frames): {0}", stream->nb_frames);
    log->DebugFormat("[Stream] - Duration (timestamps): {0}", stream->duration);
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

    // Calculated values
    log->Debug("Duration (timestamps): " + mVideoInfo.DurationTimeStamps);
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