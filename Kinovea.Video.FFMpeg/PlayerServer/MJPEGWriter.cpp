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
RecordingResult MJPEGWriter::OpenSavingContext(String^ _filePath, VideoInfo _info, String^ _formatString, Kinovea::Services::ImageFormat _imageFormat, bool _uncompressed, double _fFramesInterval, double _fFileFramesInterval, ImageRotation rotation)
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

    

    // Apparently not all output size are ok, some crash sws_scale.
    // We will keep the input size and use the input pixel aspect ratio for maximum compatibility.
    // [2011-08-21] - Check if the issue with output size is related to odd number of rows.
    if(!_info.OriginalSize.IsEmpty)
        m_SavingContext->outputSize = _info.OriginalSize;
    
    if(_info.PixelAspectRatio > 0)
        m_SavingContext->fPixelAspectRatio = _info.PixelAspectRatio;

    if(_fFileFramesInterval > 0) 
        m_SavingContext->fFramesInterval = _fFileFramesInterval;
    
    m_SavingContext->iBitrate = (int)ComputeBitrate(m_SavingContext->outputSize, m_SavingContext->fFramesInterval);
    
    m_SavingContext->uncompressed = _uncompressed;

    do
    {
        // Muxer selection.

        // StringToHGlobalAnsi is harmless here as we know the format string is ASCII.
        char* pFormatString = static_cast<char*>(Marshal::StringToHGlobalAnsi(_formatString).ToPointer());
        const AVOutputFormat* format = av_guess_format(pFormatString, nullptr, nullptr);
        if (format == nullptr) 
        {
            result = RecordingResult::MuxerNotFound;
            Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));
            log->Error("Muxer not found");
            break;
        }

        Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));
        m_SavingContext->pOutputFormat = format;

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
        AVCodecID codecId = _uncompressed ? AV_CODEC_ID_RAWVIDEO : AV_CODEC_ID_MJPEG;
        if ((m_SavingContext->pOutputCodec = avcodec_find_encoder(codecId)) == nullptr)
        {
            result = RecordingResult::EncoderNotFound;
            log->Error("Encoder not found");
            break;
        }

        // Create video stream.
        m_SavingContext->pOutputVideoStream = avformat_new_stream(m_SavingContext->pOutputFormatContext, m_SavingContext->pOutputCodec);
        if (m_SavingContext->pOutputVideoStream == nullptr) 
        {
            result = RecordingResult::VideoStreamNotCreated;
            log->Error("Video stream not created");
            break;
        }

        m_SavingContext->pOutputVideoStream->id = m_SavingContext->pOutputFormatContext->nb_streams - 1;

        switch (rotation)
        {
        case ImageRotation::Rotate90:
            av_dict_set(&m_SavingContext->pOutputVideoStream->metadata, "rotate", "90", 0);
            break;
        case ImageRotation::Rotate180:
            av_dict_set(&m_SavingContext->pOutputVideoStream->metadata, "rotate", "180", 0);
            break;
        case ImageRotation::Rotate270:
            av_dict_set(&m_SavingContext->pOutputVideoStream->metadata, "rotate", "270", 0);
            break;
        case ImageRotation::Rotate0:
        default:
            break;
        }
        
        // Configure encoder.
        if(!SetupEncoder(m_SavingContext, _imageFormat))
        {
            result = RecordingResult::EncoderParametersNotSet;
            log->Error("Encoder parameters not set");
            break;
        }

        m_SavingContext->pOutputFormatContext->video_codec_id = m_SavingContext->pOutputCodec->id;
        m_SavingContext->targetFormat = m_SavingContext->pOutputCodecContext->pix_fmt;

        // Open the encoder.
        averror = avcodec_open2(m_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec, nullptr);
        if (averror < 0)
        {
            result = RecordingResult::EncoderNotOpened;
            LogFFMpegError("Encoder not opened", averror);
            break;
        }

        m_SavingContext->bEncoderOpened = true;
        
        // Associate encoder to stream.
        averror = avcodec_parameters_from_context(m_SavingContext->pOutputVideoStream->codecpar, m_SavingContext->pOutputCodecContext);
        if (averror < 0) 
        {
            result = RecordingResult::EncoderParametersNotSet;
            LogFFMpegError("Failed to copy encoder parameters to stream", averror);
            break;
        }

        m_SavingContext->pOutputVideoStream->time_base = m_SavingContext->pOutputCodecContext->time_base;

        // Open the file.
        // Temporary pinned UTF-8 buffer.
        int byteCount = System::Text::Encoding::UTF8->GetByteCount(_filePath);
        array<Byte>^ utf8Path = gcnew array<Byte>(byteCount + 1);
        Encoding::UTF8->GetBytes(_filePath, 0, _filePath->Length, utf8Path, 0);
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
            (m_SavingContext->fFramesInterval * m_SavingContext->pOutputVideoStream->time_base.den) / 
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
        if (_imageFormat == Kinovea::Services::ImageFormat::RGB24)
            m_SavingContext->sourceFormat = AV_PIX_FMT_BGR24;
        else if (_imageFormat == Kinovea::Services::ImageFormat::Y800)
            m_SavingContext->sourceFormat = AV_PIX_FMT_GRAY8;

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

    if(pStream->time_base.num <= 0 || pStream->time_base.den <= 0)
        log->Error("MJPEGWriter sanity check failed: time base not set.");

    if(pStream->codecpar->width <= 0 || pStream->codecpar->height <= 0)
        log->Error("MJPEGWriter sanity check failed: dimensions not set.");

    if(av_cmp_q(pStream->sample_aspect_ratio, pStream->codecpar->sample_aspect_ratio))
    {
        log->Error("MJPEGWriter sanity check failed: Aspect ratio mismatch between encoder and muxer layer.");
        log->Debug(String::Format("pStream SAR={0}:{1}, codec SAR:{2}:{3}", 
            pStream->sample_aspect_ratio.num, pStream->sample_aspect_ratio.den, 
            pStream->codecpar->sample_aspect_ratio.num, pStream->codecpar->sample_aspect_ratio.den));
    }

    /*if(s->oformat->flags & AVFMT_GLOBALHEADER && !(pStream->flags & CODEC_FLAG_GLOBAL_HEADER))
        log->Debug("MJPEGWriter sanity check warning: Codec does not use global headers but container format requires global headers");*/
    
}


RecordingResult MJPEGWriter::CloseSavingContext(bool _bEncodingSuccess)
{
    log->Debug("Closing the saving context.");

    RecordingResult result = RecordingResult::Success;
    m_swEncoding->Stop();
    m_swWrite->Stop();

    if(_bEncodingSuccess)
    {
        // Write file trailer.		
        av_write_trailer(m_SavingContext->pOutputFormatContext);
    }

    if(m_SavingContext->bEncoderOpened)
    {
        //avcodec_close(m_SavingContext->pOutputVideoStream->codec);
        av_free(m_SavingContext->pSourceFrame);
    }
        
    // Stream release (equivalent to freeing pOutputCodec + pOutputVideoStream)
    for(int i = 0; i < (int)m_SavingContext->pOutputFormatContext->nb_streams; i++) 
    {
        //av_freep(&(m_SavingContext->pOutputFormatContext)->streams[i]->codecpar);
        av_freep(&(m_SavingContext->pOutputFormatContext)->streams[i]);
    }

    // Close the file.
    avio_close(m_SavingContext->pOutputFormatContext->pb);

    //avcodec_free_context(m_SavingContext->pOutputCodecContext);

    // Release scaling context
    sws_freeContext(m_SavingContext->pScalingContext);

    // Release muxer parameter object.
    //av_free(m_SavingContext->pOutputFormatContext);
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


double MJPEGWriter::ComputeBitrate(Size outputSize, double frameInterval)
{
    // Note that this parameter is not used anyway as we switched to constant quantization.
    
    // Compute a bitrate equivalent to DV quality.
    // DV quality has a bitrate of 25 Mb/s for 720x576 px @ 30fps.
    // That translates to 2.01 bit per pixel.

    double qualityFactor = 2.01;

    double pixelsPerFrame = outputSize.Width * outputSize.Height;
    double pixelsPerSecond = pixelsPerFrame * (1000.0 / frameInterval);
    double bitrate = pixelsPerSecond * qualityFactor;
    
    return bitrate;
}


bool MJPEGWriter::SetupEncoder(SavingContext^ savingCtx, Kinovea::Services::ImageFormat imgFormat)
{
    //----------------------------------------
    // Parameters for encoding.
    // some tweaked, some taken from Mencoder.
    // Not all clear...
    //----------------------------------------

    // TODO:
    // Implement from ref: http://www.mplayerhq.hu/DOCS/HTML/en/menc-feat-dvd-mpeg4.html

    log->Debug("Setting up the encoder.");

    savingCtx->pOutputCodecContext = avcodec_alloc_context3(savingCtx->pOutputCodec);

    // Picture width / height.
    savingCtx->pOutputCodecContext->width = savingCtx->outputSize.Width;
    savingCtx->pOutputCodecContext->height = savingCtx->outputSize.Height;

    // Framerate and timebase.
    if (savingCtx->fFramesInterval == 0)
    {
        savingCtx->fFramesInterval = 40;
    }

    // Set the timebase to a high resolution. 
    // For example for 60 fps this sets it to 1/60000 of a second.
    // That's the unit of time we are expressing the timestamps in.
    // May be adjusted aftewards by ffmpeg anyway.
    int resolution = (int)Math::Round(1000 * 1000 / savingCtx->fFramesInterval);
    savingCtx->pOutputCodecContext->time_base = av_make_q(1, resolution);

    // The average bitrate (unused for constant quantizer encoding.)
    savingCtx->pOutputCodecContext->bit_rate = savingCtx->iBitrate;
    
    // Number of bits the bitstream is allowed to diverge from the reference.
    // the reference can be CBR (for CBR pass1) or VBR (for pass2)
    // Source: Avidemux.
    savingCtx->pOutputCodecContext->bit_rate_tolerance = 8000000;
    
    // Motion estimation algorithm used for video coding. 
    // src: MEncoder.
    //savingCtx->pOutputCodecContext->me_method = ME_EPZS;
    //savingCtx->pOutputCodecContext->me_cmp = AV_
    
    //-------------------------------------------------------------------------------------------
    // Encoding mode (i, b, p frames)
    //
    // gop_size		: the number of pictures in a group of pictures, or 0 for intra_only. (default : 12)
    // max_b_frames	: maximum number of B-frames between non-B-frames (default : 0)
    //				  Note: The output will be delayed by max_b_frames+1 relative to the input.
    //
    // [kinovea]	: Intra only.
    //-------------------------------------------------------------------------------------------
    savingCtx->pOutputCodecContext->gop_size				= 0;	
    savingCtx->pOutputCodecContext->max_b_frames			= 0;								

    // Target pixel format.
    if (imgFormat == Kinovea::Services::ImageFormat::Y800 && savingCtx->uncompressed)
        savingCtx->pOutputCodecContext->pix_fmt = AV_PIX_FMT_GRAY8;
    else
        savingCtx->pOutputCodecContext->pix_fmt = AV_PIX_FMT_YUV420P; 	
    
    // Frame rate emulation. If not zero, the lower layer (i.e. format handler) has to read frames at native frame rate.
    // src: ?
    // ->rate_emu

    //-------------------------------------------------------------
    // Encoding quality.
    // 
    // Old parameters up to 0.8.21 :
    // CODEC_FLAG_QSCALE : not set.
    // qcompress = 0.5, qblur = 0.5, qmin = 2, qmax = 16, max_qdiff = 3, mpeg_quant = 0.
    //
    // These parameters adjust "qscale" which is the degree of quantization during image encoding.
    // The higher quantization, the more compression, the more artifacts, and the smaller filesize.
    // If QSCALE flag is not set, the encoder will use up to qmax for the actual qscale parameter.
    //
    // When encoding for entertainment there might be a tradeoff between size and quality, 
    // but in sport analysis we are heavy users of frame by frame on highly dynamic scenes, 
    // These highly dynamic scenes are exactly what the encoding algorithms "optimize" out, 
    // so if we use "entertainment" parameters we end up with artefacts exactly at the worst moment.
    // In order to retain full details in dynamic scenes we must use the minimum quantization possible, at the expense of file size.
    //-------------------------------------------------------------
    
    savingCtx->pOutputCodecContext->flags |= AV_CODEC_FLAG_QSCALE;	// Constant Quantization. (this means the bitrate parameter won't be used).
    savingCtx->pOutputCodecContext->qmin = 1;						// minimum quantizer (def:2)
    savingCtx->pOutputCodecContext->qmax = 1;						// maximum quantizer (def:31) (When using QSCALE flag only qmin is used anyway.)
    
    // Sample Aspect Ratio.
    
    // Assume PAR=1:1 (square pixels).
    savingCtx->pOutputCodecContext->sample_aspect_ratio.num = 1;
    savingCtx->pOutputCodecContext->sample_aspect_ratio.den = 1;

    if(savingCtx->fPixelAspectRatio != 1.0)
    {
        // -> Anamorphic video, non square pixels.
        // We also output an anamorphic video.

        if(savingCtx->bInputWasMpeg2)
        {
            // If MPEG, sample_aspect_ratio is actually the DAR...
            // Reference for weird decision tree: mpeg12.c at mpeg_decode_postinit().
            double fDisplayAspectRatio	= (double)m_SavingContext->iSampleAspectRatioNumerator / (double)m_SavingContext->iSampleAspectRatioDenominator;
            double fPixelAspectRatio	= ((double)savingCtx->outputSize.Height * fDisplayAspectRatio) / (double)savingCtx->outputSize.Width;

            if(fPixelAspectRatio > 1.0f)
            {
                // In this case the input sample aspect ratio was actually the display aspect ratio.
                // We will recompute the aspect ratio.
                int gcd = GreatestCommonDenominator((int)((double)savingCtx->outputSize.Width * fPixelAspectRatio), savingCtx->outputSize.Width);
                savingCtx->pOutputCodecContext->sample_aspect_ratio.num = (int)(((double)savingCtx->outputSize.Width * fPixelAspectRatio)/gcd);
                savingCtx->pOutputCodecContext->sample_aspect_ratio.den = savingCtx->outputSize.Width / gcd;
            }
            else
            {
                savingCtx->pOutputCodecContext->sample_aspect_ratio.num = m_SavingContext->iSampleAspectRatioNumerator;
                savingCtx->pOutputCodecContext->sample_aspect_ratio.den = m_SavingContext->iSampleAspectRatioDenominator;
            }
        }
        else
        {
            savingCtx->pOutputCodecContext->sample_aspect_ratio.num = m_SavingContext->iSampleAspectRatioNumerator;
            savingCtx->pOutputCodecContext->sample_aspect_ratio.den = m_SavingContext->iSampleAspectRatioDenominator;
        }
    }

    // Ensure the container stream uses the same aspect ratio.
    savingCtx->pOutputVideoStream->sample_aspect_ratio.num = savingCtx->pOutputCodecContext->sample_aspect_ratio.num;
    savingCtx->pOutputVideoStream->sample_aspect_ratio.den = savingCtx->pOutputCodecContext->sample_aspect_ratio.den;

    
    savingCtx->pOutputCodecContext->strict_std_compliance = FF_COMPLIANCE_UNOFFICIAL;
    
    //-----------------------------------
    // h. Other settings. (From MEncoder) 
    //-----------------------------------
    //savingCtx->pOutputCodecContext->i_luma_elim = 0;		// luma single coefficient elimination threshold
    //savingCtx->pOutputCodecContext->i_chroma_elim = 0;		// chroma single coeff elimination threshold
    savingCtx->pOutputCodecContext->lumi_masking = 0.0;
    savingCtx->pOutputCodecContext->dark_masking = 0.0;
    // pre_me (prepass for motion estimation)
    // sample_rate
    // codecContext->channels = 2;
    // codecContext->mb_decision = 0;
    
    if (savingCtx->pOutputFormatContext->oformat->flags & AVFMT_GLOBALHEADER)
        savingCtx->pOutputCodecContext->flags |= AV_CODEC_FLAG_GLOBAL_HEADER;

    return true;
}


bool MJPEGWriter::EncodeAndWrite(SavingContext^ savingCtx, array<System::Byte>^ managedBuffer, Int64 length, bool topDown)
{
    bool written = false;
    
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
    
    written = true;
    

    /*do
    {
        Int64 then = m_swEncoding->ElapsedMilliseconds;

        int width = savingCtx->outputSize.Width;
        int height = savingCtx->outputSize.Height;
        
        pin_ptr<uint8_t> pRGB24Buffer = &managedBuffer[0];
        avpicture_fill((AVPicture*)savingCtx->pInputFrame, pRGB24Buffer, AV_PIX_FMT_BGR24, width, height);
        
        // Alter planes and stride to vertically flip image during conversion.
        if (!topDown)
        {
          savingCtx->pInputFrame->data[0] += savingCtx->pInputFrame->linesize[0] * (height - 1);
          savingCtx->pInputFrame->linesize[0] = -savingCtx->pInputFrame->linesize[0];
        }

        // Prepare the color space converted frame.
        if ((pYUV420Frame = av_frame_alloc()) == nullptr) 
        {
            log->Error("YUV420P frame not allocated");
            break;
        }

        int yuvBufferSize = avpicture_get_size(AV_PIX_FMT_YUV420P, width, height);
        pYUV420Buffer = (uint8_t*)av_malloc(yuvBufferSize);
        if (pYUV420Buffer == nullptr) 
        {
            log->Error("YUV frame buffer not allocated");
            break;
        }
        
        avpicture_fill((AVPicture*)pYUV420Frame, pYUV420Buffer, AV_PIX_FMT_YUV420P, width, height);
        
        // Perform the color space conversion.
        if (sws_scale(savingCtx->pScalingContext, savingCtx->pInputFrame->data, savingCtx->pInputFrame->linesize, 0, height, pYUV420Frame->data, pYUV420Frame->linesize) < 0) 
        {
            log->Error("Color conversion failed");
            break;
        }

        int encodedSize = yuvBufferSize;
        if (!savingCtx->uncompressed)
        {
            // Allocated JPEG frame buffer. 
            // Assumes uncompressed size is always smaller than compressed. (Not technically true).
            int jpegBufferSize = yuvBufferSize;
            pJpegBuffer = (uint8_t*)av_malloc(jpegBufferSize);
            if (pJpegBuffer == nullptr) 
            {
                log->Error("output video buffer not allocated");
                break;
            }
        
            // Actual encoding step.
            encodedSize = avcodec_encode_video(savingCtx->pOutputCodecContext, pJpegBuffer, jpegBufferSize, pYUV420Frame);
        }

        m_encodingDurationAccumulator += (m_swEncoding->ElapsedMilliseconds - then);

        if (encodedSize <= 0)
            break;

        if (savingCtx->uncompressed)
            WritePacket(encodedSize, savingCtx, pYUV420Buffer, true);
        else
            WritePacket(encodedSize, savingCtx, pJpegBuffer, true);

        written = true;
    }
    while(false);

    if (pJpegBuffer != nullptr)
        av_free(pJpegBuffer);

    if (pYUV420Frame != nullptr)
        av_free(pYUV420Frame);

    if (pYUV420Buffer != nullptr)
        av_free(pYUV420Buffer);*/

    return written;
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
    savingCtx->pPacket->stream_index = savingCtx->pOutputVideoStream->index;
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
    if (m_SavingContext->frameCounter % 100 != 0)
        return;
    
    log->DebugFormat("Frame #{0}. Conversion/Encoding: ~{1:0.000} ms. Write: ~{2:0.000} ms.",
        m_SavingContext->frameCounter, 
        (float)m_encodingDurationAccumulator / 100, 
        (float)m_writeDurationAccumulator / 100);

    m_encodingDurationAccumulator = 0;
    m_writeDurationAccumulator = 0;
}


int MJPEGWriter::GreatestCommonDenominator(int a, int b)
{
     if (a == 0) return b;
     if (b == 0) return a;

     if (a > b)
        return GreatestCommonDenominator(a % b, b);
     else
        return GreatestCommonDenominator(a, b % a);
}
