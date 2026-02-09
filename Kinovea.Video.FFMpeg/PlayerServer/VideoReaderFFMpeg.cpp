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
    av_register_all();
    avfilter_register_all();
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
    if (m_bIsLoaded)
        Close();
}

void VideoReaderFFMpeg::DataInit()
{
    SwitchDecodingMode(VideoDecodingMode::NotInitialized);
    m_bIsLoaded = false;
    m_iVideoStream = -1;
    m_iAudioStream = -1;
    m_VideoInfo = VideoInfo::Empty;
    m_WorkingZone = VideoSection::MakeEmpty();
    m_TimestampInfo = TimestampInfo::Empty;
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
    if (!m_bIsLoaded)
        return;

    DataInit();

    if (m_pCodecCtx != nullptr)
        avcodec_close(m_pCodecCtx);

    if (m_pFormatCtx != nullptr)
    {
        AVFormatContext* pin = m_pFormatCtx;
        avformat_close_input(&pin);
        m_pFormatCtx = pin;
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
    if (loaded != OpenVideoResult::Success)
        return summary;

    SwitchDecodingMode(VideoDecodingMode::OnDemand);

    summary->IsImage = m_VideoInfo.DurationTimeStamps == 1;
    double durationSeconds = (m_VideoInfo.DurationTimeStamps - m_VideoInfo.AverageTimeStampsPerFrame) / m_VideoInfo.AverageTimeStampsPerSeconds;
    summary->DurationMilliseconds = (int64_t)Math::Round(durationSeconds * 1000.0);
    summary->ImageSize = m_VideoInfo.ReferenceSize;
    summary->Framerate = m_VideoInfo.FramesPerSeconds;

    //log->DebugFormat("ExtractSummary {0}. After load: {1} ms.", _filePath, m_Stopwatch->ElapsedMilliseconds);
    
    // Read some frames (directly decode at small size).
    float stretch = (float)m_VideoInfo.OriginalSize.Width / _maxSize.Width;
    m_DecodingSize = Size(_maxSize.Width, (int)(m_VideoInfo.OriginalSize.Height / stretch));

    int64_t step = (int64_t)Math::Ceiling(m_VideoInfo.DurationTimeStamps / (double)_thumbs);
    int64_t previousFrameTimestamp = -1;
    

    int index = 0;
    for (int64_t ts = 0; ts < m_VideoInfo.DurationTimeStamps; ts += step)
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
            m_TimestampInfo.CurrentTimestamp > previousFrameTimestamp)
        {
            Bitmap^ bmp = BitmapHelper::Copy(m_FramesContainer->CurrentFrame->Image);
            summary->Thumbs->Add(bmp);
            previousFrameTimestamp = m_TimestampInfo.CurrentTimestamp;
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

    if (m_bIsLoaded)
        Close();

    m_VideoInfo.FilePath = _filePath;
    if (Options == nullptr)
        Options = Options->Default;

    do
    {
        // Open file and get info on format (muxer).
        AVFormatContext* pFormatCtx = nullptr;

        // Libav expects the filename in the computer default codepage.
        String^ encFilePath = System::Text::Encoding::Default->GetString(System::Text::Encoding::UTF8->GetBytes(_filePath));
        char* pszFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(encFilePath).ToPointer());
        if (avformat_open_input(&pFormatCtx, pszFilePath, NULL, NULL) != 0)
        {
            result = OpenVideoResult::FileNotOpenned;
            log->ErrorFormat("The file {0} could not be openned. (Wrong path or not a video/image.)", _filePath);
            break;
        }
        Marshal::FreeHGlobal(safe_cast<IntPtr>(pszFilePath));

        // Info on streams.
        if (avformat_find_stream_info(pFormatCtx, nullptr) < 0)
        {
            result = OpenVideoResult::StreamInfoNotFound;
            log->Error("The streams Infos were not Found.");
            break;
        }

        // Video stream.
        if ((m_iVideoStream = GetStreamIndex(pFormatCtx, AVMEDIA_TYPE_VIDEO)) < 0)
        {
            result = OpenVideoResult::VideoStreamNotFound;
            log->Error("No Video stream found in the file. (File is audio only, or video stream is broken.)");
            break;
        }

        // Detect image rotation
        m_VideoInfo.ImageRotation = ImageRotation::Rotate0;
        AVDictionaryEntry* pRotationTag = av_dict_get(pFormatCtx->streams[m_iVideoStream]->metadata, "rotate", nullptr, 0);
        if (pRotationTag != nullptr)
        {
            String^ value = gcnew String(pRotationTag->value);
            if (value == "90")
                m_VideoInfo.ImageRotation = ImageRotation::Rotate90;
            else if (value == "180")
                m_VideoInfo.ImageRotation = ImageRotation::Rotate180;
            else if (value == "270")
                m_VideoInfo.ImageRotation = ImageRotation::Rotate270;
        }

        // Codec
        AVCodec* pCodec = nullptr;
        AVCodecContext* pCodecCtx = pFormatCtx->streams[m_iVideoStream]->codec;
        m_VideoInfo.IsCodecMpeg2 = (pCodecCtx->codec_id == CODEC_ID_MPEG2VIDEO);
        if ((pCodec = avcodec_find_decoder(pCodecCtx->codec_id)) == nullptr)
        {
            result = OpenVideoResult::CodecNotFound;
            log->Error("No suitable codec to decode the video. (Worse than an unsupported codec.)");
            break;
        }

        if (avcodec_open2(pCodecCtx, pCodec, nullptr) < 0)
        {
            result = OpenVideoResult::CodecNotOpened;
            log->Error("Codec could not be openned. (Codec known, but not supported yet.)");
            break;
        }

        // The fundamental unit of time in Kinovea is the timebase of the file.
        // The timebase unit is the span of time (in seconds) in which the timestamps are expressed.
        if (m_Verbose)
            log->DebugFormat("pFormatCtx->streams[m_iVideoStream]->time_base.den: {0}, .num: {1}", pFormatCtx->streams[m_iVideoStream]->time_base.den, pFormatCtx->streams[m_iVideoStream]->time_base.num);

        m_VideoInfo.AverageTimeStampsPerSeconds = (double)pFormatCtx->streams[m_iVideoStream]->time_base.den / (double)pFormatCtx->streams[m_iVideoStream]->time_base.num;
        double fAvgFrameRate = 0.0;
        if (pFormatCtx->streams[m_iVideoStream]->avg_frame_rate.den != 0)
            fAvgFrameRate = (double)pFormatCtx->streams[m_iVideoStream]->avg_frame_rate.num / (double)pFormatCtx->streams[m_iVideoStream]->avg_frame_rate.den;

        // This may be updated after the first actual decoding.
        long firstTimestamp = (int64_t)((double)((double)pFormatCtx->start_time / (double)AV_TIME_BASE) * m_VideoInfo.AverageTimeStampsPerSeconds);
        m_VideoInfo.FirstTimeStamp = Math::Max(firstTimestamp, 0);

        // In case of negative start time, we still want to expose 0-based timestamps to the outside.
        // We keep the offset around and add/remove it to low-level ffmpeg calls.
        if (firstTimestamp < 0)
        {
            m_timestampOffset = firstTimestamp - 1;
            if (!_forSummary)
                log->WarnFormat("Negative start time. Applying timestamp offset of {0}.", m_timestampOffset);
        }

        if (pFormatCtx->duration > 0)
            m_VideoInfo.DurationTimeStamps = (int64_t)((double)((double)pFormatCtx->duration / (double)AV_TIME_BASE) * m_VideoInfo.AverageTimeStampsPerSeconds);
        else
            m_VideoInfo.DurationTimeStamps = 0;

        if (m_VideoInfo.DurationTimeStamps <= 0)
        {
            result = OpenVideoResult::StreamInfoNotFound;
            log->Error("Duration info not found.");
            break;
        }

        // Average FPS. Based on the following sources:
        // - libav in stream info (already in fAvgFrameRate).
        // - libav in container or stream with duration in frames or microseconds (Rarely available but valid if so).
        // - stream->time_base	(Often KO, like 90000:1, expresses the timestamps unit)
        // - codec->time_base (Often OK, but not always).
        // - some ad-hoc special cases.
        int iTicksPerFrame = pCodecCtx->ticks_per_frame;
        m_VideoInfo.FramesPerSeconds = 0;
        bool verbose = !_forSummary;
        if (fAvgFrameRate != 0)
        {
            m_VideoInfo.FramesPerSeconds = fAvgFrameRate;
            if (verbose)
                log->Debug("Average Fps estimation method: libav.");
        }
        else
        {
            // 1.a. Durations
            if ((pFormatCtx->streams[m_iVideoStream]->nb_frames > 0) && (pFormatCtx->duration > 0))
            {
                m_VideoInfo.FramesPerSeconds = ((double)pFormatCtx->streams[m_iVideoStream]->nb_frames * (double)AV_TIME_BASE) / (double)pFormatCtx->duration;

                if (iTicksPerFrame > 1)
                    m_VideoInfo.FramesPerSeconds /= iTicksPerFrame;

                if (verbose)
                    log->Debug("Average Fps estimation method: Durations.");
            }
            else
            {
                // 1.b. stream->time_base, consider invalid if >= 1000.
                m_VideoInfo.FramesPerSeconds = (double)pFormatCtx->streams[m_iVideoStream]->time_base.den / (double)pFormatCtx->streams[m_iVideoStream]->time_base.num;

                if (m_VideoInfo.FramesPerSeconds < 1000)
                {
                    if (iTicksPerFrame > 1)
                        m_VideoInfo.FramesPerSeconds /= iTicksPerFrame;

                    if (verbose)
                        log->Debug("Average Fps estimation method: Stream timebase.");
                }
                else
                {
                    // 1.c. codec->time_base, consider invalid if >= 1000.
                    m_VideoInfo.FramesPerSeconds = (double)pCodecCtx->time_base.den / (double)pCodecCtx->time_base.num;

                    if (m_VideoInfo.FramesPerSeconds < 1000)
                    {
                        if (iTicksPerFrame > 1)
                            m_VideoInfo.FramesPerSeconds /= iTicksPerFrame;

                        if (verbose)
                            log->Debug("Average Fps estimation method: Codec timebase.");
                    }
                    else if (m_VideoInfo.FramesPerSeconds == 30000)
                    {
                        m_VideoInfo.FramesPerSeconds = 29.97;
                        if (verbose)
                            log->Debug("Average Fps estimation method: special case detection (30000:1 -> 30000:1001).");
                    }
                    else if (m_VideoInfo.FramesPerSeconds == 25000)
                    {
                        m_VideoInfo.FramesPerSeconds = 24.975;
                        if (verbose)
                            log->Debug("Average Fps estimation method: special case detection (25000:1 -> 25000:1001).");
                    }
                    else
                    {
                        // Detection failed. Force to 25fps.
                        m_VideoInfo.FramesPerSeconds = 25;
                        if (verbose)
                            log->Debug("Average Fps estimation method: Estimation failed. Fps will be forced to : " + m_VideoInfo.FramesPerSeconds);
                    }
                }
            }
        }

        if (verbose)
            log->Debug("Ticks per frame: " + iTicksPerFrame);

        m_VideoInfo.FrameIntervalMilliseconds = 1000.0 / m_VideoInfo.FramesPerSeconds;
        m_VideoInfo.AverageTimeStampsPerFrame = m_VideoInfo.AverageTimeStampsPerSeconds / m_VideoInfo.FramesPerSeconds;

        m_WorkingZone = VideoSection(
            m_VideoInfo.FirstTimeStamp,
            (int64_t)Math::Round(m_VideoInfo.FirstTimeStamp + m_VideoInfo.DurationTimeStamps - m_VideoInfo.AverageTimeStampsPerFrame));

        // Image size
        m_VideoInfo.OriginalSize = Size(pCodecCtx->width, pCodecCtx->height);

        if (pCodecCtx->sample_aspect_ratio.num != 0 && pCodecCtx->sample_aspect_ratio.num != pCodecCtx->sample_aspect_ratio.den)
        {
            // Anamorphic video, non square pixels.
            if (verbose)
                log->Debug("Display Aspect Ratio type: Anamorphic");

            if (pCodecCtx->codec_id == CODEC_ID_MPEG2VIDEO)
            {
                // If MPEG, sample_aspect_ratio is actually the DAR...
                // Reference for weird decision tree: mpeg12.c at mpeg_decode_postinit().
                double fDisplayAspectRatio = (double)pCodecCtx->sample_aspect_ratio.num / (double)pCodecCtx->sample_aspect_ratio.den;
                m_VideoInfo.PixelAspectRatio = ((double)pCodecCtx->height * fDisplayAspectRatio) / (double)pCodecCtx->width;

                if (m_VideoInfo.PixelAspectRatio < 1.0f)
                    m_VideoInfo.PixelAspectRatio = fDisplayAspectRatio;
            }
            else
            {
                m_VideoInfo.PixelAspectRatio = (double)pCodecCtx->sample_aspect_ratio.num / (double)pCodecCtx->sample_aspect_ratio.den;
            }

            m_VideoInfo.SampleAspectRatio = Fraction(pCodecCtx->sample_aspect_ratio.num, pCodecCtx->sample_aspect_ratio.den);
        }
        else
        {
            // Assume PAR=1:1.
            if (verbose)
                log->Debug("Display Aspect Ratio type: Square Pixels");
            m_VideoInfo.PixelAspectRatio = 1.0f;
        }

        Options->ImageRotation = m_VideoInfo.ImageRotation;
        UpdateReferenceSizes(Options->ImageAspectRatio, verbose);
        m_DecodingSize = m_VideoInfo.AspectRatioSize;

        m_pFormatCtx = pFormatCtx;
        m_pCodecCtx = pCodecCtx;

        m_bIsLoaded = true;

        // If not many frames compared to the dynamic cache size (single image or very short video), 
        // load everything right away, freeze the cache, and disable extra capabilities.
        // the Cache.WorkingZone boundaries may be updated with actual values from the file.
        double nbFrames = m_VideoInfo.DurationTimeStamps / m_VideoInfo.AverageTimeStampsPerFrame;
        int veryShortThresholdFrames = 0;
        m_bIsVeryShort = nbFrames <= veryShortThresholdFrames;

        if (_forSummary)
        {
            m_Capabilities = VideoCapabilities::CanDecodeOnDemand;
            SwitchDecodingMode(VideoDecodingMode::OnDemand);
        }
        else if (m_bIsVeryShort)
        {
            m_Capabilities =
                VideoCapabilities::CanCache |
                VideoCapabilities::CanChangeImageRotation |
                VideoCapabilities::CanStabilize;

            if (m_pCodecCtx->codec_id == AV_CODEC_ID_RAWVIDEO)
                m_Capabilities = m_Capabilities | VideoCapabilities::CanChangeDemosaicing;

            SwitchDecodingMode(VideoDecodingMode::Caching);
            ReadMany(nullptr, m_WorkingZone, false);
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

            if (m_pCodecCtx->codec_id == AV_CODEC_ID_RAWVIDEO)
                m_Capabilities = m_Capabilities | VideoCapabilities::CanChangeDemosaicing;

            SwitchDecodingMode(VideoDecodingMode::OnDemand);
        }

        result = OpenVideoResult::Success;
    } while (false);

    return result;
}

int VideoReaderFFMpeg::GetStreamIndex(AVFormatContext* _pFormatCtx, int _iCodecType)
{
    // Returns the best candidate stream for the specified type, -1 if not found.
    unsigned int iCurrentStreamIndex = -1;
    unsigned int iBestStreamIndex = -1;
    int64_t iBestFrames = -1;

    do
    {
        iCurrentStreamIndex++;
        if (_pFormatCtx->streams[iCurrentStreamIndex]->codec->codec_type != _iCodecType)
            continue;

        int64_t frames = _pFormatCtx->streams[iCurrentStreamIndex]->nb_frames;
        if (frames > iBestFrames)
        {
            iBestFrames = frames;
            iBestStreamIndex = iCurrentStreamIndex;
        }
    } while (iCurrentStreamIndex < _pFormatCtx->nb_streams - 1);

    return (int)iBestStreamIndex;
}

#pragma endregion

#pragma region Frame requests
bool VideoReaderFFMpeg::MoveNext(int _skip, bool _decodeIfNecessary)
{
    if (!m_bIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
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
    if (!m_bIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
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
                int64_t actualTarget = m_TimestampInfo.CurrentTimestamp;
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
    if (!m_bIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
        return;

    if (!CanChangeWorkingZone)
        throw gcnew CapabilityNotSupportedException();

    if (m_Verbose)
        log->DebugFormat("Update working zone request. {0} to {1}. Force reload:{2}", m_WorkingZone, _newZone, _forceReload);

    if (!_forceReload && m_WorkingZone == _newZone)
        return;

    if (!CanCache)
    {
        m_WorkingZone = _newZone;
        if (m_DecodingMode == VideoDecodingMode::OnDemand && CanPreBuffer)
            SwitchDecodingMode(VideoDecodingMode::PreBuffering);
        else if (m_DecodingMode == VideoDecodingMode::PreBuffering)
            m_PreBuffer->UpdateWorkingZone(m_WorkingZone);
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
            log->DebugFormat("Working zone update. Current:{0}, Asked:{1}", m_WorkingZone, _newZone);

        if (!WorkingZoneFitsInMemory(_newZone, _maxMemory))
        {
            if (m_Verbose)
                log->Debug("New working zone does not fit in memory.");

            m_WorkingZone = _newZone;
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
                if (_newZone.Start > m_WorkingZone.Start)
                {
                    // Only do it if the new start is at least one frame beyond the old one.
                    if (_newZone.Start - m_WorkingZone.Start > m_VideoInfo.AverageTimeStampsPerFrame)
                    {
                        m_Cache->ReduceWorkingZone(VideoSection(_newZone.Start, m_WorkingZone.End));
                        m_WorkingZone = m_Cache->WorkingZone;
                        log->DebugFormat("Reduced cache from the front: {0}.", m_WorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    _newZone = VideoSection(m_WorkingZone.Start, _newZone.End);
                }

                if (_newZone.End < m_WorkingZone.End)
                {
                    // Only do it if the new end is at least one frame before the old one.
                    if (m_WorkingZone.End - _newZone.End > m_VideoInfo.AverageTimeStampsPerFrame)
                    {
                        m_Cache->ReduceWorkingZone(VideoSection(m_WorkingZone.Start, _newZone.End));
                        m_WorkingZone = m_Cache->WorkingZone;
                        log->DebugFormat("Reduced cache from the back: {0}.", m_WorkingZone);
                    }

                    // Realign the request to avoid unnecessary loads due to timestamp mismatch.
                    _newZone = VideoSection(_newZone.Start, m_WorkingZone.End);
                }

                // Bail out if our job is done.
                if (_newZone.Start == m_WorkingZone.Start && _newZone.End == m_WorkingZone.End)
                    return;

                // Expand at the front if there is more than one frame to expand.
                if (m_WorkingZone.Start - _newZone.Start > m_VideoInfo.AverageTimeStampsPerFrame)
                {
                    m_SectionToPrepend = VideoSection(_newZone.Start, m_WorkingZone.Start);
                }

                // Expand at the back if there is more than one frame to expand.
                if (_newZone.End - m_WorkingZone.End > m_VideoInfo.AverageTimeStampsPerFrame)
                {
                    m_SectionToAppend = VideoSection(m_WorkingZone.End, _newZone.End);
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
        m_PreBuffer->UpdateWorkingZone(m_WorkingZone);
        SeekTo(m_WorkingZone.Start);
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
    if (CanPreBuffer && !m_WorkingZone.IsEmpty)
        SwitchDecodingMode(VideoDecodingMode::PreBuffering);
    else if (CanDecodeOnDemand)
        SwitchDecodingMode(VideoDecodingMode::OnDemand);
    else
        throw gcnew CapabilityNotSupportedException();
}

bool VideoReaderFFMpeg::WorkingZoneFitsInMemory(VideoSection _newZone, int _maxMemory)
{
    double durationSeconds = (double)(_newZone.End - _newZone.Start) / m_VideoInfo.AverageTimeStampsPerSeconds;

    // Loading is done at full aspect ratio size, not at the current decoding size based on the rendering container.
    // Otherwise we would have to potentially reload the cache each time there is a stretch/squeeze request.
    int64_t frameBytes = avpicture_get_size(m_PixelFormatFFmpeg, m_VideoInfo.ReferenceSize.Width, m_VideoInfo.ReferenceSize.Height);
    double frameMegaBytes = (double)frameBytes / 1048576;
    double durationMegaBytes = durationSeconds * m_VideoInfo.FramesPerSeconds * frameMegaBytes;

    return durationMegaBytes <= _maxMemory;
}

void VideoReaderFFMpeg::ImportWorkingZoneToCache(System::Object^ sender, DoWorkEventArgs^ e)
{
    BackgroundWorker^ worker = dynamic_cast<BackgroundWorker^>(sender);

    bool success = true;
    if (!m_SectionToPrepend.IsEmpty)
        success = ReadMany(worker, m_SectionToPrepend, true);

    if (success && !m_SectionToAppend.IsEmpty)
        success = ReadMany(worker, m_SectionToAppend, false);

    if (!success)
        SwitchToBestAfterCaching();
}

#pragma endregion

#pragma region Image adjustments (aspect, rotation, demosaicing, deinterlace, stabilization)

bool VideoReaderFFMpeg::ChangeAspectRatio(ImageAspectRatio _ratio)
{
    if (!CanChangeAspectRatio)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
        log->ErrorFormat("PreBuffering thread is started.");

    Options->ImageAspectRatio = _ratio;
    UpdateReferenceSizes(_ratio, true);

    // TODO: decoding size should be updated from the outside ?
    m_DecodingSize = m_VideoInfo.AspectRatioSize;

    m_FramesContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeImageRotation(ImageRotation rotation)
{
    if (!CanChangeImageRotation)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
        log->ErrorFormat("PreBuffering thread is started.");

    Options->ImageRotation = rotation;
    m_VideoInfo.ImageRotation = rotation;

    UpdateReferenceSizes(Options->ImageAspectRatio, true);
    m_DecodingSize = m_VideoInfo.AspectRatioSize;
    m_FramesContainer->Clear();
    return true;
}
bool VideoReaderFFMpeg::ChangeDemosaicing(Demosaicing demosaicing)
{
    if (!CanChangeDemosaicing)
        throw gcnew CapabilityNotSupportedException();

    // Decoding thread should be stopped at this point.
    if (m_PreBufferingThread != nullptr && m_PreBufferingThread->IsAlive)
        log->ErrorFormat("PreBuffering thread is started.");

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

    for (size_t i = 0; i < points->Count; i++)
    {
        if (stabOffsets->ContainsKey(points[i]->T))
            continue;

        TimedPoint^ p = gcnew TimedPoint(points[i]->X - points[0]->X, points[i]->Y - points[0]->Y, points[i]->T);
        stabOffsets->Add(points[i]->T, p);
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

    bool sideway = m_VideoInfo.ImageRotation == ImageRotation::Rotate90 || m_VideoInfo.ImageRotation == ImageRotation::Rotate270;
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
        log->DebugFormat("Changing decoding size from {0} to {1}", m_DecodingSize, targetSize);

    long currentTimestamp = m_PreBuffer->CurrentFrame != nullptr ? m_PreBuffer->CurrentFrame->Timestamp : -1;

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

    long currentTimestamp = m_PreBuffer->CurrentFrame != nullptr ? m_PreBuffer->CurrentFrame->Timestamp : -1;

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
    m_DecodingSize = m_VideoInfo.AspectRatioSize;
    m_CanDrawUnscaled = false;
}

void VideoReaderFFMpeg::UpdateReferenceSizes(Kinovea::Services::ImageAspectRatio _ratio, bool verbose)
{
    // Called during load or when aspect ratio or rotation changes.
    
    // Set the image geometry according to the pixel aspect ratio choosen.
    if (verbose)
        log->DebugFormat("Image aspect ratio: {0}", _ratio);

    // Constraint width and change height to match aspect ratio.
    m_VideoInfo.AspectRatioSize.Width = m_VideoInfo.OriginalSize.Width;

    switch (_ratio)
    {
    case Kinovea::Services::ImageAspectRatio::Force43:
        m_VideoInfo.AspectRatioSize.Height = (int)((m_VideoInfo.OriginalSize.Width * 3.0) / 4.0);
        break;
    case Kinovea::Services::ImageAspectRatio::Force169:
        m_VideoInfo.AspectRatioSize.Height = (int)((m_VideoInfo.OriginalSize.Width * 9.0) / 16.0);
        break;
    case Kinovea::Services::ImageAspectRatio::ForcedSquarePixels:
        m_VideoInfo.AspectRatioSize.Height = m_VideoInfo.OriginalSize.Height;
        break;
    case Kinovea::Services::ImageAspectRatio::Auto:
    default:
        m_VideoInfo.AspectRatioSize.Height = (int)((double)m_VideoInfo.OriginalSize.Height / m_VideoInfo.PixelAspectRatio);
        break;
    }

    bool sideway = m_VideoInfo.ImageRotation == ImageRotation::Rotate90 || m_VideoInfo.ImageRotation == ImageRotation::Rotate270;
    m_VideoInfo.AspectRatioSize = FixSize(m_VideoInfo.AspectRatioSize, sideway);
    m_VideoInfo.ReferenceSize = sideway ? Size(m_VideoInfo.AspectRatioSize.Height, m_VideoInfo.AspectRatioSize.Width) : m_VideoInfo.AspectRatioSize;

    if (verbose)
        log->DebugFormat("Image size: Original:{0}, AspectRatioSize:{1}, ReferenceSize:{2}.", m_VideoInfo.OriginalSize, m_VideoInfo.AspectRatioSize, m_VideoInfo.ReferenceSize);
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
           (m_Cache->WorkingZone.Start - _section.Start < m_VideoInfo.AverageTimeStampsPerFrame))
        {
            // Start target is less than one frame before the current start.
            _section = VideoSection(m_Cache->WorkingZone.Start, _section.End);
        }
        else if (!_prepend && 
            (_section.End - m_Cache->WorkingZone.End < m_VideoInfo.AverageTimeStampsPerFrame))
        {
            // End target is less than one frame beyond the current end.
            _section = VideoSection(_section.Start, m_Cache->WorkingZone.End);
        }

        log->DebugFormat("Aligned requested section to cache: {0}", _section);
    }

    double end = _section.End + (m_VideoInfo.AverageTimeStampsPerFrame * 0.5);
    double frames = (end - _section.Start) / m_VideoInfo.AverageTimeStampsPerFrame;
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
    while ((m_TimestampInfo.CurrentTimestamp < _section.End) &&
           (read < total) && 
           (res == ReadResult::Success))
    {
        if (_bgWorker != nullptr && _bgWorker->CancellationPending)
        {
            if (m_Verbose)
                log->DebugFormat("Cancellation at frame [{0}]", m_TimestampInfo.CurrentTimestamp);

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

    m_WorkingZone = m_Cache->WorkingZone;
    m_Cache->SetPrependBlock(false);

    // Sometimes a few frames at the end can't be read.
    if (m_TimestampInfo.CurrentTimestamp < _section.End && read < total)
    {
        log->ErrorFormat("Caching section: could only read {0} out of {1} frames.", read, total);
    
        if (read >= (total - 1) * 0.95)
        {
            m_WorkingZone = m_Cache->WorkingZone;
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

    m_LoopWatcher->LoopStart();

    // TODO: shouldn't need to lock. Make sure we don't synchronously ask for a frame while prebuffering.
    lock l(m_Locker);

    if (!m_bIsLoaded || m_DecodingMode == VideoDecodingMode::NotInitialized)
        return ReadResult::MovieNotLoaded;

    if (m_FramesContainer == nullptr)
        return ReadResult::FrameContainerNotSet;

    ReadResult result = ReadResult::Success;
    int	iFramesToDecode = _iFramesToDecode;
    int64_t iTargetTimeStamp = _iTimeStampToSeekTo;
    bool seeking = false;

    // Find the proper target and number of frames to decode.
    if (_iFramesToDecode < 0)
    {
        // Negative move. Compute seek target.
        iTargetTimeStamp = m_FramesContainer->CurrentFrame->Timestamp + (_iFramesToDecode * m_VideoInfo.AverageTimeStampsPerFrame);
        if (iTargetTimeStamp < 0)
            iTargetTimeStamp = 0;
    }

    if (iTargetTimeStamp >= 0)
    {
        seeking = true;
        iFramesToDecode = 1; // We'll use the target timestamp anyway.
        int iSeekRes = SeekTo(iTargetTimeStamp);
        if (iSeekRes < 0)
        {
            log->ErrorFormat("Error during seek. Error code:{0}. Seek target was:[{1}]", iSeekRes, iTargetTimeStamp);
            seeking = false;
        }
    }

    // Allocate 2 AVFrames, one for the raw decoded frame and one for deinterlaced/rescaled/converted frame.
    AVFrame* pDecodingAVFrame = av_frame_alloc();
    AVFrame* pFinalAVFrame = av_frame_alloc();

    // The buffer holding the actual frame data.
    int iSizeBuffer = avpicture_get_size(m_PixelFormatFFmpeg, m_DecodingSize.Width, m_DecodingSize.Height);
    uint8_t* pBuffer = iSizeBuffer > 0 ? new uint8_t[iSizeBuffer] : nullptr;

    if (pDecodingAVFrame == nullptr || pFinalAVFrame == nullptr || pBuffer == nullptr)
        return ReadResult::MemoryNotAllocated;

    // Assigns appropriate parts of buffer to image planes in the AVFrame.
    avpicture_fill((AVPicture*)pFinalAVFrame, pBuffer, m_PixelFormatFFmpeg, m_DecodingSize.Width, m_DecodingSize.Height);

    m_TimestampInfo.CurrentTimestamp = m_FramesContainer->CurrentFrame == nullptr ? -1 : m_FramesContainer->CurrentFrame->Timestamp;

    // Reading/Decoding loop
    bool done = false;
    bool bFirstPass = true;
    int iReadFrameResult;
    int gotPicturePtr = 0;
    int	iFramesDecoded = 0;
    do
    {
        // FFMpeg also has an internal buffer to cope with B-Frames entanglement.
        // The DTS/PTS announced is actually the one of the last frame that was put in the buffer by av_read_frame,
        // it is *not* the one of the frame that was extracted from the buffer by avcodec_decode_video.
        // To solve the DTS/PTS issue, we save the timestamps each time we find libav is buffering a frame.
        // And we use the previously saved timestamps.
        // Ref: http://lists.mplayerhq.hu/pipermail/libav-user/2008-August/001069.html

        // Read next packet
        AVPacket inputPacket;
        iReadFrameResult = av_read_frame(m_pFormatCtx, &inputPacket);
        if (iReadFrameResult < 0)
        {
            // Reading error. We don't know if the error happened on a video frame or audio one.
            done = true;
            delete[] pBuffer;
            result = ReadResult::FrameNotRead;
            break;
        }

        if (inputPacket.stream_index != m_iVideoStream)
        {
            av_free_packet(&inputPacket);
            continue;
        }

        // Decode video packet. This is needed even if we're not on the final frame yet.
        // I-Frame data is kept internally by ffmpeg which will need it to build the final frame.
        avcodec_decode_video2(m_pCodecCtx, pDecodingAVFrame, &gotPicturePtr, &inputPacket);
        if (gotPicturePtr == 0)
        {
            // Buffering frame. libav just read a I or P frame that will be presented later.
            // (But which was necessary to get now in order to decode a coming B frame.)
            av_free_packet(&inputPacket);
            continue;
        }

        long beTimestamp = pDecodingAVFrame->best_effort_timestamp;
        if (beTimestamp < m_timestampOffset)
        {
            m_timestampOffset = beTimestamp;
            if (m_Verbose)
                log->DebugFormat("Negative timestamp received. Applying new timestamp offset of {0}.", m_timestampOffset);
        }

        m_TimestampInfo.CurrentTimestamp = beTimestamp - m_timestampOffset;

        if (seeking && bFirstPass && !_approximate && iTargetTimeStamp >= 0 && m_TimestampInfo.CurrentTimestamp > iTargetTimeStamp)
        {
            // If the current ts is already after the target, we are dealing with this kind of files
            // where the seek doesn't work as advertised. We'll seek back again further,
            // and then decode until we get to it.

            // Do this only once.
            bFirstPass = false;

            // For some files, one additional second back is not enough. The seek is wrong by up to 4 seconds.
            // We also allow the target to go before 0.
            int iSecondsBack = 4;
            int64_t iForceSeekTimestamp = (int64_t)(iTargetTimeStamp - (m_VideoInfo.AverageTimeStampsPerSeconds * iSecondsBack));
            int64_t iMinTarget = System::Math::Min(iForceSeekTimestamp, (int64_t)0);

            // Do the seek.
            if (m_Verbose)
            {
                log->DebugFormat("[Seek] - First decoded frame [{0}] already after target [{1}]. Force seek {2} more seconds back to [{3}]",
                    m_TimestampInfo.CurrentTimestamp, iTargetTimeStamp, iSecondsBack, iForceSeekTimestamp);
            }

            avformat_seek_file(m_pFormatCtx, m_iVideoStream, iMinTarget + m_timestampOffset, iForceSeekTimestamp + m_timestampOffset, iForceSeekTimestamp + m_timestampOffset, AVSEEK_FLAG_BACKWARD);
            avcodec_flush_buffers(m_pFormatCtx->streams[m_iVideoStream]->codec);

            // Free the packet that was allocated by av_read_frame
            av_free_packet(&inputPacket);

            // Loop back to restart decoding frames until we get to the target.
            continue;
        }

        bFirstPass = false;
        iFramesDecoded++;

        //-------------------------------------------------------------------------------
        // If we're done, convert the image and store it into its final recipient.
        // - seek: if we reached the target timestamp.
        // - linear decoding: if we decoded the required number of frames.
        //-------------------------------------------------------------------------------
        if (seeking && m_TimestampInfo.CurrentTimestamp >= iTargetTimeStamp ||
            !seeking && iFramesDecoded >= iFramesToDecode ||
            _approximate)
        {
            done = true;

            if (m_Verbose && seeking /* && m_TimestampInfo.CurrentTimestamp != iTargetTimeStamp*/)
            {
                log->DebugFormat("Seeking to [{0}] completed. Final position:[{1}], decoded: {2} frames.", 
                    iTargetTimeStamp, m_TimestampInfo.CurrentTimestamp, iFramesDecoded);
            }

            // Deinterlace + rescale + convert pixel format.
            bool rescaled = RescaleAndConvert(
                pFinalAVFrame,
                pDecodingAVFrame,
                m_DecodingSize.Width,
                m_DecodingSize.Height,
                m_PixelFormatFFmpeg,
                Options->Deinterlace);

            if (!rescaled)
            {
                delete[] pBuffer;
                result = ReadResult::ImageNotConverted;
                break;
            }

            try
            {
                // Import ffmpeg buffer into a .NET bitmap.
                int imageStride = pFinalAVFrame->linesize[0];
                IntPtr scan0 = IntPtr((void*)pFinalAVFrame->data[0]);
                Bitmap^ bmp = nullptr;
                if (stabOffsets->ContainsKey(m_TimestampInfo.CurrentTimestamp))
                {
                    // Image stabilization. Paint the image with the offset applied.
                    // Prepare output bitmap.
                    bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, DecodingPixelFormat);

                    // Get the decoded frame in a bitmap and paint it over the output.
                    Bitmap^ bmp2 = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, scan0);
                    Graphics^ g = Graphics::FromImage(bmp);
                    float dx = stabOffsets[m_TimestampInfo.CurrentTimestamp]->X;
                    float dy = stabOffsets[m_TimestampInfo.CurrentTimestamp]->Y;
                    // TODO: handle scaling (decoding size).
                    g->DrawImageUnscaled(bmp2, -dx, -dy);
                    delete g;
                    delete bmp2;
                }
                else
                {
                    bmp = gcnew Bitmap(m_DecodingSize.Width, m_DecodingSize.Height, imageStride, DecodingPixelFormat, scan0);
                }

                // Rotation is handled after scaling and aspect ratio fix for simplicity.
                // In later versions of FFMpeg there are rotation routines built in, that might be simpler and faster.
                switch (m_VideoInfo.ImageRotation)
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

                // Store a pointer to the native buffer inside the Bitmap.
                // We'll be asked to free this resource later when the frame is not used anymore.
                // It is boxed inside an Object so we can extract it in a type-safe way.
                IntPtr^ boxedPtr = gcnew IntPtr((void*)pBuffer);
                bmp->Tag = boxedPtr;

                // Construct the VideoFrame and push it to the current container.
                VideoFrame^ vf = gcnew VideoFrame();
                vf->Image = bmp;
                vf->Timestamp = m_TimestampInfo.CurrentTimestamp;

                m_LoopWatcher->LoopEnd();

                // Finally, add the frame to the container.
                m_FramesContainer->Add(vf);
            }
            catch (Exception^ exp)
            {
                delete[] pBuffer;
                result = ReadResult::ImageNotConverted;
                log->Error("Error while converting AVFrame to Bitmap.");
                log->Error(exp);
            }
        }

        // Free the packet that was allocated by av_read_frame
        av_free_packet(&inputPacket);
    } while (!done);

    // Free the AVFrames. (This will not deallocate the data buffers).
    av_free(pFinalAVFrame);
    av_free(pDecodingAVFrame);

#ifdef INSTRUMENTATION	
    if (m_FramesContainer->Current != nullptr)
        log->DebugFormat("[{0}] - Memory: {1:0,0} bytes", m_PreBuffer->CurrentFrame->Timestamp, Process::GetCurrentProcess()->PrivateMemorySize64);
#endif

    if (!m_bFirstFrameRead)
    {
        m_bFirstFrameRead = true;
        m_VideoInfo.FirstTimeStamp = m_TimestampInfo.CurrentTimestamp;
        m_WorkingZone = VideoSection(m_VideoInfo.FirstTimeStamp, m_WorkingZone.End);
    }

    return result;
}

int VideoReaderFFMpeg::SeekTo(int64_t _target)
{
    // Perform an FFMpeg seek without decoding the frame.
    // AVSEEK_FLAG_BACKWARD -> goes to first I-Frame before target.
    // Then we'll need to decode frame by frame until the target is reached.
    long minTs = m_timestampOffset;
    long ts = _target + m_timestampOffset;
    long maxTs = (int64_t)(_target + m_timestampOffset + m_VideoInfo.AverageTimeStampsPerSeconds);

    int res = avformat_seek_file(
        m_pFormatCtx,
        m_iVideoStream,
        minTs,
        ts,
        maxTs,
        AVSEEK_FLAG_BACKWARD);

    avcodec_flush_buffers(m_pFormatCtx->streams[m_iVideoStream]->codec);
    m_TimestampInfo = TimestampInfo::Empty;
    return res;
}

bool VideoReaderFFMpeg::RescaleAndConvert(AVFrame* _pOutputFrame, AVFrame* _pInputFrame, int _OutputWidth, int _OutputHeight, int _OutputFmt, bool _bDeinterlace)
{
    //------------------------------------------------------------------------
    // Utility function called by ReadFrame().
    // Take the frame we just decoded and turn it to the right size/deint/fmt.
    // todo: sws_getContext could be done only once.
    //------------------------------------------------------------------------
    bool bSuccess = true;
    AVPixelFormat srcFormat = m_pCodecCtx->pix_fmt;
    if (CanChangeDemosaicing)
    {
        switch (Options->Demosaicing)
        {
        case Demosaicing::RGGB:
            srcFormat = AV_PIX_FMT_BAYER_RGGB8;
            break;
        case Demosaicing::BGGR:
            srcFormat = AV_PIX_FMT_BAYER_BGGR8;
            break;
        case Demosaicing::GRBG:
            srcFormat = AV_PIX_FMT_BAYER_GRBG8;
            break;
        case Demosaicing::GBRG:
            srcFormat = AV_PIX_FMT_BAYER_GBRG8;
            break;
        case Demosaicing::None:
        default:
            srcFormat = m_pCodecCtx->pix_fmt;
            break;
        }
    }

    SwsContext* pSWSCtx = sws_getContext(
        m_pCodecCtx->width, m_pCodecCtx->height, srcFormat,
        _OutputWidth, _OutputHeight, (AVPixelFormat)_OutputFmt,
        DecodingQuality,
        nullptr, nullptr, nullptr);

    uint8_t* pDeinterlaceBuffer = nullptr;
    uint8_t** ppOutputData = nullptr;
    int* piStride = nullptr;

    if (_bDeinterlace)
    {
        AVPicture* pDeinterlacingFrame;
        AVPicture	tmpPicture;

        // Deinterlacing happens before resizing.
        int iSizeDeinterlaced = avpicture_get_size(m_pCodecCtx->pix_fmt, m_pCodecCtx->width, m_pCodecCtx->height);

        pDeinterlaceBuffer = new uint8_t[iSizeDeinterlaced];
        pDeinterlacingFrame = &tmpPicture;
        avpicture_fill(pDeinterlacingFrame, pDeinterlaceBuffer, m_pCodecCtx->pix_fmt, m_pCodecCtx->width, m_pCodecCtx->height);

        int resDeint = avpicture_deinterlace(pDeinterlacingFrame, (AVPicture*)_pInputFrame, m_pCodecCtx->pix_fmt, m_pCodecCtx->width, m_pCodecCtx->height);

        if (resDeint < 0)
        {
            // Deinterlacing failed, use original image.
            log->Debug("Deinterlacing failed, use original image.");
            ppOutputData = _pInputFrame->data;
            piStride = _pInputFrame->linesize;
        }
        else
        {
            // Use deinterlaced image.
            ppOutputData = pDeinterlacingFrame->data;
            piStride = pDeinterlacingFrame->linesize;
        }
    }
    else
    {
        ppOutputData = _pInputFrame->data;
        piStride = _pInputFrame->linesize;
    }

    try
    {
        sws_scale(pSWSCtx, ppOutputData, piStride, 0, m_pCodecCtx->height, _pOutputFrame->data, _pOutputFrame->linesize);
    }
    catch (Exception^)
    {
        bSuccess = false;
        log->Error("RescaleAndConvert Error : sws_scale failed.");
    }

    // Clean Up.
    sws_freeContext(pSWSCtx);

    if (pDeinterlaceBuffer != nullptr)
        delete[] pDeinterlaceBuffer;

    return bSuccess;
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
            m_TimestampInfo.CurrentTimestamp, m_Stopwatch->ElapsedMilliseconds);*/


        if (canceler->CancellationPending)
        {
            log->DebugFormat("PreBuffering thread, cancellation detected. After ReadFrame().");
            break;
        }

        // Check if we hit the end of the zone.
        if (m_TimestampInfo.CurrentTimestamp > m_WorkingZone.End)
        {
            if (m_Verbose)
                log->DebugFormat("Average prebuffering loop time: {0:0.000}ms. (Budget: {1:0.000}ms).", m_LoopWatcher->Average, m_VideoInfo.FrameIntervalMilliseconds);
            
            m_LoopWatcher->Restart();
            ReadFrame(m_WorkingZone.Start, 1, false);
            continue;
        }

        if (res == ReadResult::FrameNotRead)
        {
            // We got a frame-not-read but we are not yet at the end of the zone.
            log->ErrorFormat("Frame not read in the middle of the working zone. Reached timestamp:[{0}], in {1}.", m_TimestampInfo.CurrentTimestamp, m_WorkingZone);
            
            if (m_WorkingZone.IsEmpty)
                break;

            // The most sensible thing to do is still to go back to the begining and start again, 
            // as if we just hit the end of the zone.
            m_LoopWatcher->Restart();
            ReadFrame(m_WorkingZone.Start, 1, false);
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
    log->Debug("[File] - Filename : " + Path::GetFileName(m_VideoInfo.FilePath));
    log->DebugFormat("[Container] - Name: {0} ({1})", gcnew String(m_pFormatCtx->iformat->name), gcnew String(m_pFormatCtx->iformat->long_name));
    DumpStreamsInfos(m_pFormatCtx);
    log->Debug("[Container] - Duration (s): " + (double)m_pFormatCtx->duration / 1000000);
    log->Debug("[Container] - Bit rate: " + m_pFormatCtx->bit_rate);
    if (m_pFormatCtx->streams[m_iVideoStream]->nb_frames > 0)
        log->DebugFormat("[Stream] - Duration (frames): {0}", m_pFormatCtx->streams[m_iVideoStream]->nb_frames);
    else
        log->Debug("[Stream] - Duration (frames): Unavailable.");
    log->DebugFormat("[Stream] - PTS wrap bits: {0}", m_pFormatCtx->streams[m_iVideoStream]->pts_wrap_bits);
    log->DebugFormat("[Stream] - TimeBase: {0}:{1}", m_pFormatCtx->streams[m_iVideoStream]->time_base.den, m_pFormatCtx->streams[m_iVideoStream]->time_base.num);
    log->DebugFormat("[Stream] - Average timestamps per seconds: {0}", m_VideoInfo.AverageTimeStampsPerSeconds);
    log->DebugFormat("[Container] - Start time (microseconds): {0}", m_pFormatCtx->start_time);
    log->DebugFormat("[Container] - Start timestamp: {0} ({1})", m_VideoInfo.FirstTimeStamp, m_timestampOffset);
    log->DebugFormat("[Codec] - Name: {0}, id:{1}", gcnew String(m_pCodecCtx->codec_name), (int)m_pCodecCtx->codec_id);
    log->DebugFormat("[Codec] - TimeBase: {0}:{1}", m_pCodecCtx->time_base.den, m_pCodecCtx->time_base.num);
    log->Debug("[Codec] - Bit rate: " + m_pCodecCtx->bit_rate);
    log->Debug("Duration in timestamps: " + m_VideoInfo.DurationTimeStamps);
    log->Debug("Duration in seconds (computed): " + (double)(double)m_VideoInfo.DurationTimeStamps / (double)m_VideoInfo.AverageTimeStampsPerSeconds);
    log->Debug("Average Fps: " + m_VideoInfo.FramesPerSeconds);
    log->Debug("Average Frame Interval (ms): " + m_VideoInfo.FrameIntervalMilliseconds);
    log->Debug("Average Timestamps per frame: " + m_VideoInfo.AverageTimeStampsPerFrame);
    log->DebugFormat("[Codec] - Has B Frames: {0}", m_pCodecCtx->has_b_frames);
    log->Debug("[Codec] - Width (pixels): " + m_pCodecCtx->width);
    log->Debug("[Codec] - Height (pixels): " + m_pCodecCtx->height);
    log->Debug("Pixel Aspect Ratio: " + m_VideoInfo.PixelAspectRatio);
    log->Debug("Image rotation: " + m_VideoInfo.ImageRotation.ToString());
    log->Debug("---------------------------------------------------");
}

void VideoReaderFFMpeg::DumpStreamsInfos(AVFormatContext* _pFormatCtx)
{
    log->Debug("[Container] - Number of streams: " + _pFormatCtx->nb_streams);

    for (int i = 0; i<(int)_pFormatCtx->nb_streams; i++)
    {
        String^ streamType;

        switch ((int)_pFormatCtx->streams[i]->codec->codec_type)
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

        log->DebugFormat("[Stream] #{0}, Type : {1}, {2}", i, streamType, _pFormatCtx->streams[i]->nb_frames);
    }
}

void VideoReaderFFMpeg::DumpFrameType(int _type)
{
    switch (_type)
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