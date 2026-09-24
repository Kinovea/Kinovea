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

#include "MJPEGWriter.h"

using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Drawing2D;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Text;

using namespace Kinovea::Services;
using namespace Kinovea::Video;
using namespace Kinovea::Video::FFMpeg;

MJPEGWriter::MJPEGWriter()
{
    m_swEncoding = gcnew Stopwatch();
    m_swWrite = gcnew Stopwatch();
}
MJPEGWriter::~MJPEGWriter()
{
}
MJPEGWriter::!MJPEGWriter()
{
}

///<summary>
/// MJPEGWriter::OpenSavingContext
/// Open a saving context and configure it with default parameters.
///</summary>
RecordingResult MJPEGWriter::OpenSavingContext(RecordingSettings^ settings)
{
    //---------------------------------------------------------------------------------------------------
    // Set up the saving context.
    // Output file, encoding parameters, etc.
    // This will be used by the actual saving function.
    //---------------------------------------------------------------------------------------------------

    RecordingResult result = RecordingResult::Success;
    m_swEncoding->Start();
    m_swWrite->Start();

    if (m_SavingContext != nullptr) 
        delete m_SavingContext;
    
    m_SavingContext = gcnew SavingContext();
    m_SavingContext->outputSize = settings->ImageSize;
    
    if (settings->FileFrameInterval > 0)
    {
        m_SavingContext->frameInterval = settings->FileFrameInterval;
    }
    
    m_SavingContext->uncompressed = settings->Uncompressed;

    do
    {
        // Muxer selection.

        // StringToHGlobalAnsi is harmless here as we know the format string is ASCII.
        String^ formatString = FilesystemHelper::GetFormatStringCapture(settings->Uncompressed);
        char* pFormatString = static_cast<char*>(Marshal::StringToHGlobalAnsi(formatString).ToPointer());
        const AVOutputFormat* format = av_guess_format(pFormatString, nullptr, nullptr);
        if (format == nullptr) 
        {
            result = RecordingResult::MuxerNotFound;
            Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));
            log->Error("Muxer not found");
            break;
        }

        Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));

        // Allocate muxer context.
        pin_ptr<AVFormatContext*> pinOutputFormatContext = &m_SavingContext->pOutputFormatContext;
        int averror = avformat_alloc_output_context2(pinOutputFormatContext, format, nullptr, nullptr);
        if (averror < 0)
        {
            result = RecordingResult::MuxerParametersNotAllocated;
            LogFFMpegError("Failed to allocate output context", averror);
            break;
        }

        m_SavingContext->pOutputFormatContext->oformat = format;


        //-----------------------
        // Video stream / encoder
        //-----------------------

        // Find specific encoder.
        AVCodecID codecId = settings->Uncompressed ? AV_CODEC_ID_RAWVIDEO : AV_CODEC_ID_MJPEG;
        if ((m_SavingContext->pOutputCodec = avcodec_find_encoder(codecId)) == nullptr)
        {
            result = RecordingResult::EncoderNotFound;
            log->Error("Encoder not found");
            break;
        }

        // Create video stream.
        AVStream* pOutputVideoStream;
        pOutputVideoStream = avformat_new_stream(m_SavingContext->pOutputFormatContext, m_SavingContext->pOutputCodec);
        if (pOutputVideoStream == nullptr)
        {
            result = RecordingResult::VideoStreamNotCreated;
            log->Error("Video stream not created");
            break;
        }

        pOutputVideoStream->id = m_SavingContext->pOutputFormatContext->nb_streams - 1;
        m_SavingContext->streamIndex = pOutputVideoStream->id;

        // Configure encoder.
        SetupEncoder(m_SavingContext, settings->ImageFormat, settings->Quality);

        m_SavingContext->pOutputFormatContext->video_codec_id = m_SavingContext->pOutputCodec->id;
        m_SavingContext->targetFormat = m_SavingContext->pOutputCodecContext->pix_fmt;
        pOutputVideoStream->sample_aspect_ratio = m_SavingContext->pOutputCodecContext->sample_aspect_ratio;

        // Open the encoder.
        averror = avcodec_open2(m_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec, nullptr);
        if (averror < 0)
        {
            result = RecordingResult::EncoderNotOpened;
            LogFFMpegError("Encoder not opened", averror);
            break;
        }

        log->DebugFormat(
            "MJPEG threading: requested count={0}, requested type={1}, active type={2}.",
            m_SavingContext->pOutputCodecContext->thread_count,
            m_SavingContext->pOutputCodecContext->thread_type,
            m_SavingContext->pOutputCodecContext->active_thread_type);

        // Associate encoder to stream.
        averror = avcodec_parameters_from_context(pOutputVideoStream->codecpar, m_SavingContext->pOutputCodecContext);
        if (averror < 0) 
        {
            result = RecordingResult::EncoderParametersNotSet;
            LogFFMpegError("Failed to copy encoder parameters to stream", averror);
            break;
        }

        pOutputVideoStream->time_base = m_SavingContext->pOutputCodecContext->time_base;

        // Write rotation data.
        // This must be done AFTER the call to avcodec_parameters_from_context, otherwise coded_side_data is overwritten.
        // But it must be done BEFORE avformat_write_header.
        switch (settings->Rotation)
        {
        case ImageRotation::Rotate90:
            SetRotation(pOutputVideoStream, 90);
            break;
        case ImageRotation::Rotate180:
            SetRotation(pOutputVideoStream, 180);
            break;
        case ImageRotation::Rotate270:
            SetRotation(pOutputVideoStream, 270);
            break;
        case ImageRotation::Rotate0:
        default:
            break;
        }

        // Open the file.
        // Temporary pinned UTF-8 buffer.
        int byteCount = System::Text::Encoding::UTF8->GetByteCount(settings->FilePath);
        array<Byte>^ utf8Path = gcnew array<Byte>(byteCount + 1);
        Encoding::UTF8->GetBytes(settings->FilePath, 0, settings->FilePath->Length, utf8Path, 0);
        pin_ptr<Byte> pinnedPath = &utf8Path[0];
        const char* pszFilePath = reinterpret_cast<const char*>(pinnedPath);
        averror = avio_open(&(m_SavingContext->pOutputFormatContext)->pb, pszFilePath, AVIO_FLAG_WRITE);
        if (averror < 0) 
        {
            result = RecordingResult::FileNotOpened;
            LogFFMpegError("File not opened", averror);
            break;
        }

        SanityCheck(m_SavingContext->pOutputFormatContext);

        // Write file header.
        averror = avformat_write_header(m_SavingContext->pOutputFormatContext, nullptr);
        if (averror < 0)
        {
            result = RecordingResult::FileHeaderNotWritten;
            LogFFMpegError("File header not written", averror);
            break;
        }

        // In theory packet duration should always be 1000 but the call to write_header may change 
        // the requested time base to something else, so we compute it from the actual one.
        // This does (frame interval in seconds / time base).
        m_SavingContext->frameDuration = (long long)Math::Round(
            (m_SavingContext->frameInterval * pOutputVideoStream->time_base.den) /
            (1000.0 * m_SavingContext->pOutputCodecContext->time_base.num));

        //------------------------------------------------------
        // Prepare for the conversion/encoding loop.
        //------------------------------------------------------ 
        
        m_SavingContext->pSourceFrame = av_frame_alloc();
        m_SavingContext->pConvertedFrame = av_frame_alloc();
        m_SavingContext->pPacket = av_packet_alloc();
        if (!m_SavingContext->pSourceFrame || !m_SavingContext->pConvertedFrame || !m_SavingContext->pPacket)
        {
            result = RecordingResult::InputFrameNotAllocated;
            log->Error("Frames not allocated");
            break;
        }

        m_SavingContext->sourceFormat = AV_PIX_FMT_BGRA;
        if (settings->ImageFormat == ImageFormat::RGB24)
        {
            m_SavingContext->sourceFormat = AV_PIX_FMT_BGR24;
        }
        else if (settings->ImageFormat == ImageFormat::Y800)
        {
            m_SavingContext->sourceFormat = AV_PIX_FMT_GRAY8;
        }

        // Source frame initially has no storage of its own, 
        // its data pointers will be attached to the byte array later.
        m_SavingContext->pSourceFrame->format = m_SavingContext->sourceFormat;
        m_SavingContext->pSourceFrame->width = m_SavingContext->outputSize.Width;
        m_SavingContext->pSourceFrame->height = m_SavingContext->outputSize.Height;

        m_SavingContext->pConvertedFrame->format = m_SavingContext->targetFormat;
        m_SavingContext->pConvertedFrame->width = m_SavingContext->outputSize.Width;
        m_SavingContext->pConvertedFrame->height = m_SavingContext->outputSize.Height;
        
        // Allocate buffers for the converted frame.
        // Passing 0 lets FFmpeg choose an appropriate alignment.
        av_frame_get_buffer(m_SavingContext->pConvertedFrame, 0);

        // Prepare the pixel format conversion context.
        // Using nearest neighbor instead of bilinear gains about 1.5ms on a 1600x1200 frame.
        int flags = SWS_POINT;
        SwsContext* scalingContext = sws_getContext(
            m_SavingContext->outputSize.Width, m_SavingContext->outputSize.Height, m_SavingContext->sourceFormat,
            m_SavingContext->outputSize.Width, m_SavingContext->outputSize.Height, m_SavingContext->targetFormat, 
            flags, NULL, NULL, NULL);

        m_SavingContext->pScalingContext = scalingContext;
    }
    while(false);

    return result;
}

void MJPEGWriter::SanityCheck(AVFormatContext* s)
{
    // Adapted from the real sanity check from utils.c av_write_header.

    if (s->nb_streams != 1) 
    {
        log->Error("Sanity check failed: no streams.");
        return;
    }

    AVStream* pStream = s->streams[0];
    if (pStream->codecpar->codec_type != AVMEDIA_TYPE_VIDEO)
    {
        log->Error("Sanity check failed: not a video codec.");
        return;
    }

    if (pStream->time_base.num <= 0 || pStream->time_base.den <= 0)
    {
        log->Error("MJPEGWriter sanity check failed: time base not set.");
    }

    if (pStream->codecpar->width <= 0 || pStream->codecpar->height <= 0)
    {
        log->Error("MJPEGWriter sanity check failed: dimensions not set.");
    }

    if(av_cmp_q(pStream->sample_aspect_ratio, pStream->codecpar->sample_aspect_ratio))
    {
        log->Error("MJPEGWriter sanity check failed: Aspect ratio mismatch between encoder and muxer layer.");
        log->Debug(String::Format("pStream SAR={0}:{1}, codec SAR:{2}:{3}", 
            pStream->sample_aspect_ratio.num, pStream->sample_aspect_ratio.den, 
            pStream->codecpar->sample_aspect_ratio.num, pStream->codecpar->sample_aspect_ratio.den));
    }
}


RecordingResult MJPEGWriter::CloseSavingContext(bool success)
{
    log->Debug("Closing the saving context.");

    RecordingResult result = RecordingResult::Success;
    m_swEncoding->Stop();
    m_swWrite->Stop();

    // Write trailer and close the file.
    if(success)
    {
        av_write_trailer(m_SavingContext->pOutputFormatContext);
    }

    avio_close(m_SavingContext->pOutputFormatContext->pb);
    
    // Release reusable objects.
    av_free(m_SavingContext->pSourceFrame);
    av_free(m_SavingContext->pConvertedFrame);
    sws_freeContext(m_SavingContext->pScalingContext);

    // Release streams, incl. codec.
    for(int i = 0; i < (int)m_SavingContext->pOutputFormatContext->nb_streams; i++) 
    {
        av_freep(&(m_SavingContext->pOutputFormatContext)->streams[i]);
    }

    // Release muxer.
    avformat_free_context(m_SavingContext->pOutputFormatContext);

    log->Debug("Saving video completed.");

    return result;
}


RecordingResult MJPEGWriter::SaveFrame(Kinovea::Services::ImageFormat format, array<System::Byte>^ buffer, Int64 length, bool topDown)
{
    RecordingResult result = RecordingResult::Success;
    bool saved = false;

    bool doNotEncode = 
        (format == Kinovea::Services::ImageFormat::JPEG) || 
        (format == Kinovea::Services::ImageFormat::Y800 && m_SavingContext->uncompressed);

    if (doNotEncode)
    {
        saved = WrapAndWrite(m_SavingContext, buffer, length);
    }
    else
    {
        saved = EncodeAndWrite(m_SavingContext, buffer, length, topDown);
    }

    if(!saved)
    {
        log->Error("error while writing output frame");
        result = RecordingResult::UnknownError;
    }

    return result;
}

void MJPEGWriter::SetRotation(AVStream* stream, double degrees)
{
    AVCodecParameters* par = stream->codecpar;

    AVPacketSideData* sideData = av_packet_side_data_new(
        &par->coded_side_data,
        &par->nb_coded_side_data,
        AV_PKT_DATA_DISPLAYMATRIX,
        sizeof(int32_t) * 9,
        0);

    if (sideData == nullptr)
        return;

    int32_t* matrix = reinterpret_cast<int32_t*>(sideData->data);

    av_display_rotation_set(matrix, degrees);

    return;
}

void MJPEGWriter::SetupEncoder(SavingContext^ savingCtx, ImageFormat imgFormat, int quality)
{
    AVCodecContext* ctx = avcodec_alloc_context3(savingCtx->pOutputCodec);
    
    // Image geometry
    ctx->width = savingCtx->outputSize.Width;
    ctx->height = savingCtx->outputSize.Height;
    ctx->sample_aspect_ratio = av_make_q(1, 1);

    // Sanity check frame interval.
    if (savingCtx->frameInterval == 0)
    {
        savingCtx->frameInterval = 40;
    }

    // High resolution time base.
    // For example for 60 fps this sets it to 1/60000 of a second.
    // That's the unit of time we are expressing the timestamps in.
    // May be adjusted aftewards by ffmpeg anyway.
    int resolution = (int)Math::Round(1000 * 1000 / savingCtx->frameInterval);
    ctx->time_base = av_make_q(1, resolution);

    // Global header handling.
    if (savingCtx->pOutputFormatContext->oformat->flags & AVFMT_GLOBALHEADER)
    {
        ctx->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;
    }

    if (savingCtx->uncompressed)
    {
        ctx->pix_fmt = imgFormat == ImageFormat::Y800 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_YUV420P;
        savingCtx->pOutputCodecContext = ctx;
        return;
    }

    // MJPEG encoding.
    // supports 4:2:0, 4:2:2 and 4:4:4 planar YUV.
    ctx->pix_fmt = AV_PIX_FMT_YUV420P;
    ctx->color_range = AVCOL_RANGE_JPEG;
    
    // Constant quality.
    ctx->flags |= AV_CODEC_FLAG_QSCALE;
    ctx->qmin = quality;
    ctx->qmax = quality;

    // Optimized vs default Huffman tables.
    // optimal Huffman: smaller files, extra encoding work.
    // default Huffman: faster / simpler encoding, somewhat larger files.
    av_opt_set(ctx->priv_data, "huffman", "default", 0);

    // Intra-only
    ctx->gop_size = 0;
    ctx->max_b_frames = 0;

    // Threading
    ctx->thread_type = FF_THREAD_SLICE;
    ctx->thread_count = 0; // auto

    savingCtx->pOutputCodecContext = ctx;
}


bool MJPEGWriter::EncodeAndWrite(SavingContext^ savingCtx, array<System::Byte>^ managedBuffer, Int64 length, bool topDown)
{
    long long then = m_swEncoding->ElapsedMilliseconds;

    // Wrap the existing contiguous byte array.
    // Does not copy the bytes and does not transfer ownership.
    // align=1 means the source rows are tightly packed.
    pin_ptr<uint8_t> pSourceBuffer = &managedBuffer[0];
    int required_size = av_image_fill_arrays(
        savingCtx->pSourceFrame->data,
        savingCtx->pSourceFrame->linesize,
        pSourceBuffer,
        savingCtx->sourceFormat,
        savingCtx->outputSize.Width,
        savingCtx->outputSize.Height,
        1);

    // Alter planes and stride to vertically flip image during conversion if needed.
    if (!topDown)
    {
        savingCtx->pSourceFrame->data[0] += savingCtx->pSourceFrame->linesize[0] * (savingCtx->outputSize.Height - 1);
        savingCtx->pSourceFrame->linesize[0] = -savingCtx->pSourceFrame->linesize[0];
    }

    // The encoder may retain the previous buffer, make sure we have writable storage before sws_scale().
    av_frame_make_writable(savingCtx->pConvertedFrame);

    // Convert pixel format.
    int outputRows = sws_scale(
        savingCtx->pScalingContext,
        savingCtx->pSourceFrame->data,
        savingCtx->pSourceFrame->linesize,
        0,
        savingCtx->outputSize.Height,
        savingCtx->pConvertedFrame->data,
        savingCtx->pConvertedFrame->linesize);

    if (outputRows != savingCtx->pConvertedFrame->height)
    {
        log->Error("Pixel format conversion failed");
        return false;
    }

    // Encoding.
    // Use the same process for uncompressed and compressed, 
    // the encoder will just pass through the uncompressed frame if the codec is rawvideo.
    int ret = avcodec_send_frame(savingCtx->pOutputCodecContext, savingCtx->pConvertedFrame);
    if (ret < 0) 
    {
        log->Error("Error sending frame to encoder");
        return false;
    }

    ret = avcodec_receive_packet(savingCtx->pOutputCodecContext, savingCtx->pPacket);
    if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF)
    {
        log->Error("Encoder not ready to receive packet");
        return false;
    }
    else if (ret < 0)
    {
        log->Error("Error receiving packet from encoder");
        return false;
    }

    m_encodingDurationAccumulator += (m_swEncoding->ElapsedMilliseconds - then);

    WritePacket(savingCtx);
    
    return true;
}


bool MJPEGWriter::WrapAndWrite(SavingContext^ savingCtx, array<System::Byte>^ managedBuffer, Int64 length)
{
    // As the buffer is already in the target format we bypass the encoding step entirely.
    // Create a new AVPacket from scratch and put the JPEG buffer in it.
    bool bWritten = false;
    
    do
    {     
        int ret = av_new_packet(savingCtx->pPacket, (int)length);
        if (ret < 0) 
        {
            LogFFMpegError("Error allocating packet", ret);
            break;
        }

        // Put the incoming buffer into the packet.
        pin_ptr<uint8_t> pOutputVideoBuffer = &managedBuffer[0];
        savingCtx->pPacket->data = pOutputVideoBuffer;

        WritePacket(savingCtx);
        pOutputVideoBuffer = nullptr;
        bWritten = true;
    }
    while(false);

    return bWritten;
}


bool MJPEGWriter::WritePacket(SavingContext^ savingCtx)
{
    long long then = m_swWrite->ElapsedMilliseconds;

    // Shared packet parameters.
    savingCtx->pPacket->stream_index = savingCtx->streamIndex;
    savingCtx->pPacket->flags |= AV_PKT_FLAG_KEY;

    // Duration of this packet in stream time base units.
    // Normally this will be 1000 timestamps.
    savingCtx->pPacket->duration = m_SavingContext->frameDuration;
    
    // Presentation timestamp in stream time base units.
    savingCtx->pPacket->pts = savingCtx->frameCounter * savingCtx->pPacket->duration;
    
    // Decoding timestamp in stream time base units.
    savingCtx->pPacket->dts = savingCtx->pPacket->pts;
    
    // Takes ownership of the packet reference and leaves packet blank, including on error.
    int ret = av_interleaved_write_frame(savingCtx->pOutputFormatContext, savingCtx->pPacket);
    if (ret < 0) 
    {
        LogFFMpegError("Error writing video frame", ret);
        return false;
    }
    
    savingCtx->frameCounter++;
    m_writeDurationAccumulator += (m_swWrite->ElapsedMilliseconds - then);

    LogStats();
    return true;
}


void MJPEGWriter::LogFFMpegError(String^ context, int errorCode)
{
    char errbuf[AV_ERROR_MAX_STRING_SIZE] = { 0 };
    av_strerror(errorCode, errbuf, AV_ERROR_MAX_STRING_SIZE);
    log->ErrorFormat("{0}. Error:{1}: {2}", context, errorCode, gcnew String(errbuf));
}


void MJPEGWriter::LogStats()
{
    int periodFrames = 50;
    if (m_SavingContext->frameCounter % periodFrames != 0)
        return;
    
    log->DebugFormat("Frame #{0}. Conversion/Encoding: ~{1:0.000} ms. Write: ~{2:0.000} ms.",
        m_SavingContext->frameCounter, 
        (float)m_encodingDurationAccumulator / periodFrames,
        (float)m_writeDurationAccumulator / periodFrames);

    m_encodingDurationAccumulator = 0;
    m_writeDurationAccumulator = 0;
}

