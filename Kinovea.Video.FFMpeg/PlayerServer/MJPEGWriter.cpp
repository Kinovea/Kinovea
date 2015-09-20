/*
Copyright © Joan Charmant 2008-2009.
joan.charmant@gmail.com 
 
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

using namespace Kinovea::Video;
using namespace Kinovea::Video::FFMpeg;

MJPEGWriter::MJPEGWriter()
{
    av_register_all();
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
SaveResult MJPEGWriter::OpenSavingContext(String^ _filePath, VideoInfo _info, String^ _formatString, double _fFramesInterval)
{
    //---------------------------------------------------------------------------------------------------
    // Set the saving context up.
    // Output file, encoding parameters, etc.
    // This will be used by the actual saving function.
    //---------------------------------------------------------------------------------------------------

    SaveResult result = SaveResult::Success;

    if (m_SavingContext != nullptr) 
        delete m_SavingContext;
    
    m_SavingContext = gcnew SavingContext();

    m_SavingContext->pFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(_filePath).ToPointer());
    
    // Apparently not all output size are ok, some crash sws_scale.
    // We will keep the input size and use the input pixel aspect ratio for maximum compatibility.
    // [2011-08-21] - Check if the issue with output size is related to odd number of rows.
    if(!_info.OriginalSize.IsEmpty)
        m_SavingContext->outputSize = _info.OriginalSize;
    
    if(_info.PixelAspectRatio > 0)
        m_SavingContext->fPixelAspectRatio = _info.PixelAspectRatio;

    if(_fFramesInterval > 0) 
        m_SavingContext->fFramesInterval = _fFramesInterval;
    
    m_SavingContext->iBitrate = (int)ComputeBitrate(m_SavingContext->outputSize, m_SavingContext->fFramesInterval);
    
    do
    {
        // 1. Muxer selection.
        char* pFormatString = static_cast<char*>(Marshal::StringToHGlobalAnsi(_formatString).ToPointer());
        AVOutputFormat* format = av_guess_format(pFormatString, nullptr, nullptr);
        if (format == nullptr) 
        {
            result = SaveResult::MuxerNotFound;
            Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));
            log->Error("Muxer not found");
            break;
        }

        Marshal::FreeHGlobal(safe_cast<IntPtr>(pFormatString));

        m_SavingContext->pOutputFormat = format;

        // 2. Allocate muxer parameters object.
        if ((m_SavingContext->pOutputFormatContext = avformat_alloc_context()) == nullptr) 
        {
            result = SaveResult::MuxerParametersNotAllocated;
            log->Error("Muxer parameters object not allocated");
            break;
        }
        
        // 3. Configure muxer.
        if(!SetupMuxer(m_SavingContext))
        {
            result = SaveResult::MuxerParametersNotSet;
            log->Error("Muxer parameters not set");
            break;
        }

        // 4. Create video stream.
        if ((m_SavingContext->pOutputVideoStream = avformat_new_stream(m_SavingContext->pOutputFormatContext, nullptr)) == nullptr) 
        {
            result = SaveResult::VideoStreamNotCreated;
            log->Error("Video stream not created");
            break;
        }

        // 5. Select encoder.
        if ((m_SavingContext->pOutputCodec = avcodec_find_encoder(AV_CODEC_ID_MJPEG)) == nullptr)
        {
            result = SaveResult::EncoderNotFound;
            log->Error("Encoder not found");
            break;
        }

        // 6. Allocate encoder parameters object.
        if ((m_SavingContext->pOutputCodecContext = avcodec_alloc_context3(nullptr)) == nullptr) 
        {
            result = SaveResult::EncoderParametersNotAllocated;
            log->Error("Encoder parameters object not allocated");
            break;
        }

        // 7. Configure encoder.
        if(!SetupEncoder(m_SavingContext))
        {
            result = SaveResult::EncoderParametersNotSet;
            log->Error("Encoder parameters not set");
            break;
        }

        m_SavingContext->pOutputFormatContext->video_codec_id = m_SavingContext->pOutputCodec->id;

        // 8. Open encoder.
        int openResult = avcodec_open2(m_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec, nullptr);
        if (openResult < 0)
        {
            result = SaveResult::EncoderNotOpened;
            log->Error("Encoder not opened");
            break;
        }

        m_SavingContext->bEncoderOpened = true;
        
        // 9. Associate encoder to stream.
        m_SavingContext->pOutputVideoStream->codec = m_SavingContext->pOutputCodecContext;

        // 10. Open the file.
        int iFFMpegResult;
        if ((iFFMpegResult = avio_open(&(m_SavingContext->pOutputFormatContext)->pb, m_SavingContext->pFilePath, AVIO_FLAG_WRITE)) < 0) 
        {
            result = SaveResult::FileNotOpened;
            log->Error(String::Format("File not opened, AVERROR:{0}", iFFMpegResult));
            break;
        }

        SanityCheck(m_SavingContext->pOutputFormatContext);

        // 11. Write file header.
        if((iFFMpegResult = avformat_write_header(m_SavingContext->pOutputFormatContext, nullptr)) < 0)
        {
            result = SaveResult::FileHeaderNotWritten;
            log->Error(String::Format("File header not written, AVERROR:{0}", iFFMpegResult));
            break;
        }

        // 12. Allocate memory for the current incoming frame holder. (will be reused for each frame). 
        if ((m_SavingContext->pInputFrame = av_frame_alloc()) == nullptr) 
        {
            result = SaveResult::InputFrameNotAllocated;
            log->Error("input frame not allocated");
            break;
        }
    }
    while(false);

    return result;
}

void MJPEGWriter::SanityCheck(AVFormatContext* s)
{
    // Taken/Adapted from the real sanity check from utils.c av_write_header.

    if (s->nb_streams != 1) 
    {
        log->Error("MJPEGWriter sanity check failed: no streams.");
        return;
    }

    AVStream *st = s->streams[0];
    
    if (st->codec->codec_type != AVMEDIA_TYPE_VIDEO)
    {
        log->Error("MJPEGWriter sanity check failed: not a video codec.");
        return;
    }

    if(st->codec->time_base.num <= 0 || st->codec->time_base.den <= 0)
        log->Error("MJPEGWriter sanity check failed: time base not set.");

    if(st->codec->width <= 0 || st->codec->height <= 0)
        log->Error("MJPEGWriter sanity check failed: dimensions not set.");

    if(av_cmp_q(st->sample_aspect_ratio, st->codec->sample_aspect_ratio))
    {
        log->Error("MJPEGWriter sanity check failed: Aspect ratio mismatch between encoder and muxer layer.");
        log->Debug(String::Format("stream SAR={0}:{1}, codec SAR:{2}:{3}", 
            st->sample_aspect_ratio.num, st->sample_aspect_ratio.den, st->codec->sample_aspect_ratio.num, st->codec->sample_aspect_ratio.den));
    }

    if(s->oformat->flags & AVFMT_GLOBALHEADER && !(st->codec->flags & CODEC_FLAG_GLOBAL_HEADER))
        log->Debug("MJPEGWriter sanity check warning: Codec does not use global headers but container format requires global headers");
    
}

///<summary>
/// MJPEGWriter::CloseSavingContext
/// Close the saving context and free any allocated resources.
///</summary>
SaveResult MJPEGWriter::CloseSavingContext(bool _bEncodingSuccess)
{
    log->Debug("Closing the saving context.");

    SaveResult result = SaveResult::Success;

    if(_bEncodingSuccess)
    {
        // Write file trailer.		
        av_write_trailer(m_SavingContext->pOutputFormatContext);
    }

    if(m_SavingContext->bEncoderOpened)
    {
        avcodec_close(m_SavingContext->pOutputVideoStream->codec);
        av_free(m_SavingContext->pInputFrame);
    }
        
    Marshal::FreeHGlobal(safe_cast<IntPtr>(m_SavingContext->pFilePath));
    
    // Stream release (equivalent to freeing pOutputCodec + pOutputVideoStream)
    for(int i = 0; i < (int)m_SavingContext->pOutputFormatContext->nb_streams; i++) 
    {
        av_freep(&(m_SavingContext->pOutputFormatContext)->streams[i]->codec);
        av_freep(&(m_SavingContext->pOutputFormatContext)->streams[i]);
    }

    // Close file.
    avio_close(m_SavingContext->pOutputFormatContext->pb);

    // Release muxer parameter object.
    av_free(m_SavingContext->pOutputFormatContext);

    // release pOutputFormat ?

    log->Debug("Saving video completed.");

    return result;
}

SaveResult MJPEGWriter::SaveFrame(ImageFormat format, array<System::Byte>^ buffer, Int64 length)
{
    SaveResult result = SaveResult::Success;
    bool saved = false;

    switch (format)
    {
    case ImageFormat::RGB24:
        saved = EncodeAndWriteVideoFrameRGB24(m_SavingContext, buffer, length);
        break;
    case ImageFormat::Y800:
        saved = EncodeAndWriteVideoFrameY800(m_SavingContext, buffer, length);
        break;
    case ImageFormat::JPEG:
        saved = EncodeAndWriteVideoFrameJPEG(m_SavingContext, buffer, length);
        break;

    }

    if(!saved)
    {
        log->Error("error while writing output frame");
        result = SaveResult::UnknownError;
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

///<summary>
/// MJPEGWriter::SetupMuxer
/// Configure the Muxer with default parameters.
///</summary>
bool MJPEGWriter::SetupMuxer(SavingContext^ _SavingContext)
{
    bool bResult = true;

    _SavingContext->pOutputFormatContext->oformat = _SavingContext->pOutputFormat;
    
    av_strlcpy(_SavingContext->pOutputFormatContext->filename, _SavingContext->pFilePath, sizeof(_SavingContext->pOutputFormatContext->filename));
        
    _SavingContext->pOutputFormatContext->bit_rate = _SavingContext->iBitrate;
    
    // ?
    //_SavingContext->pOutputFormatContext->preload   = (int)(0.5 * AV_TIME_BASE);
    _SavingContext->pOutputFormatContext->max_delay = (int)(0.7 * AV_TIME_BASE);

    return bResult;
}

///<summary>
/// MJPEGWriter::SetupEncoder
/// Configure the codec with default parameters.
///</summary>
bool MJPEGWriter::SetupEncoder(SavingContext^ _SavingContext)
{
    //----------------------------------------
    // Parameters for encoding.
    // some tweaked, some taken from Mencoder.
    // Not all clear...
    //----------------------------------------

    // TODO:
    // Implement from ref: http://www.mplayerhq.hu/DOCS/HTML/en/menc-feat-dvd-mpeg4.html


    log->Debug("Setting up the encoder.");

    // Codec.
    // Equivalent to : -vcodec mpeg4
    _SavingContext->pOutputCodecContext->codec_id = _SavingContext->pOutputCodec->id;
    _SavingContext->pOutputCodecContext->codec_type = AVMEDIA_TYPE_VIDEO;

    // FourCC
    _SavingContext->pOutputCodecContext->codec_tag = ('G'<<24) + ('P'<<16) + ('J'<<8) + 'M';

    // The average bitrate (unused for constant quantizer encoding.)
    // Source: statically fixed to 25Mb/s for now. 
    _SavingContext->pOutputCodecContext->bit_rate = _SavingContext->iBitrate;

    // Number of bits the bitstream is allowed to diverge from the reference.
    // the reference can be CBR (for CBR pass1) or VBR (for pass2)
    // Source: Avidemux.
    _SavingContext->pOutputCodecContext->bit_rate_tolerance = 8000000;

    // Motion estimation algorithm used for video coding. 
    // src: MEncoder.
    _SavingContext->pOutputCodecContext->me_method = ME_EPZS;

    // Framerate - timebase.
    if(_SavingContext->fFramesInterval == 0)
        _SavingContext->fFramesInterval = 40;

    double fps = 1000 / _SavingContext->fFramesInterval;
    _SavingContext->pOutputCodecContext->time_base.den			= (int)Math::Round(1000 * fps);
    _SavingContext->pOutputCodecContext->time_base.num			= 1000;
    
    // Picture width / height.
    // If we are transcoding from a video, this will be the same as the input size.
    // (not the decoding size).
    // src: [kinovea]
    _SavingContext->pOutputCodecContext->width				= _SavingContext->outputSize.Width;
    _SavingContext->pOutputCodecContext->height				= _SavingContext->outputSize.Height;
    

    //-------------------------------------------------------------------------------------------
    // Encoding mode (i, b, p frames)
    //
    // gop_size		: the number of pictures in a group of pictures, or 0 for intra_only. (default : 12)
    // max_b_frames	: maximum number of B-frames between non-B-frames (default : 0)
    //				  Note: The output will be delayed by max_b_frames+1 relative to the input.
    //
    // [kinovea]	: Intra only so we can always access prev frame right away in the Player.
    // [kinovea]	: Player doesn't support B-frames.
    //-------------------------------------------------------------------------------------------
    _SavingContext->pOutputCodecContext->gop_size				= 0;	
    _SavingContext->pOutputCodecContext->max_b_frames			= 0;								

    // Pixel format
    // src:ffmpeg.
    _SavingContext->pOutputCodecContext->pix_fmt = AV_PIX_FMT_YUV420P; 	


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
    
    _SavingContext->pOutputCodecContext->flags |= CODEC_FLAG_QSCALE;	// Constant Quantization. (this means the bitrate parameter won't be used).
    _SavingContext->pOutputCodecContext->qmin = 1;						// minimum quantizer (def:2)
    _SavingContext->pOutputCodecContext->qmax = 1;						// maximum quantizer (def:31) (When using QSCALE flag only qmin is used anyway.)
    
    // Sample Aspect Ratio.
    
    // Assume PAR=1:1 (square pixels).
    _SavingContext->pOutputCodecContext->sample_aspect_ratio.num = 1;
    _SavingContext->pOutputCodecContext->sample_aspect_ratio.den = 1;

    if(_SavingContext->fPixelAspectRatio != 1.0)
    {
        // -> Anamorphic video, non square pixels.
        // We also output an anamorphic video.

        if(_SavingContext->bInputWasMpeg2)
        {
            // If MPEG, sample_aspect_ratio is actually the DAR...
            // Reference for weird decision tree: mpeg12.c at mpeg_decode_postinit().
            double fDisplayAspectRatio	= (double)m_SavingContext->iSampleAspectRatioNumerator / (double)m_SavingContext->iSampleAspectRatioDenominator;
            double fPixelAspectRatio	= ((double)_SavingContext->outputSize.Height * fDisplayAspectRatio) / (double)_SavingContext->outputSize.Width;

            if(fPixelAspectRatio > 1.0f)
            {
                // In this case the input sample aspect ratio was actually the display aspect ratio.
                // We will recompute the aspect ratio.
                int gcd = GreatestCommonDenominator((int)((double)_SavingContext->outputSize.Width * fPixelAspectRatio), _SavingContext->outputSize.Width);
                _SavingContext->pOutputCodecContext->sample_aspect_ratio.num = (int)(((double)_SavingContext->outputSize.Width * fPixelAspectRatio)/gcd);
                _SavingContext->pOutputCodecContext->sample_aspect_ratio.den = _SavingContext->outputSize.Width / gcd;
            }
            else
            {
                _SavingContext->pOutputCodecContext->sample_aspect_ratio.num = m_SavingContext->iSampleAspectRatioNumerator;
                _SavingContext->pOutputCodecContext->sample_aspect_ratio.den = m_SavingContext->iSampleAspectRatioDenominator;
            }
        }
        else
        {
            _SavingContext->pOutputCodecContext->sample_aspect_ratio.num = m_SavingContext->iSampleAspectRatioNumerator;
            _SavingContext->pOutputCodecContext->sample_aspect_ratio.den = m_SavingContext->iSampleAspectRatioDenominator;
        }
    }

    // Ensure the container stream uses the same aspect ratio.
    _SavingContext->pOutputVideoStream->sample_aspect_ratio.num = _SavingContext->pOutputCodecContext->sample_aspect_ratio.num;
    _SavingContext->pOutputVideoStream->sample_aspect_ratio.den = _SavingContext->pOutputCodecContext->sample_aspect_ratio.den;

    
    //-----------------------------------
    // h. Other settings. (From MEncoder) 
    //-----------------------------------
    _SavingContext->pOutputCodecContext->strict_std_compliance= -1;		// strictly follow the standard (MPEG4, ...)
    //_SavingContext->pOutputCodecContext->i_luma_elim = 0;		// luma single coefficient elimination threshold
    //_SavingContext->pOutputCodecContext->i_chroma_elim = 0;		// chroma single coeff elimination threshold
    _SavingContext->pOutputCodecContext->lumi_masking = 0.0;;
    _SavingContext->pOutputCodecContext->dark_masking = 0.0;
    // codecContext->codec_tag							// 4CC : if not set then the default based on codec_id will be used.
    // pre_me (prepass for motion estimation)
    // sample_rate
    // codecContext->channels = 2;
    // codecContext->mb_decision = 0;
    
    return true;

}

///<summary>
/// Encode an RGB24 image into a JPEG and push it to the file.
///</summary>
bool MJPEGWriter::EncodeAndWriteVideoFrameRGB24(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length)
{
    bool written = false;
    
    AVFrame* pYUV420Frame = nullptr;
    uint8_t* pYUV420Buffer = nullptr;
    uint8_t* pJpegBuffer = nullptr;
    
    do
    {
        int outWidth = _SavingContext->outputSize.Width;
        int outHeight = _SavingContext->outputSize.Height;
        int inWidth = outWidth;
        int inHeight = outHeight;

        pin_ptr<uint8_t> pRGB24Buffer = &managedBuffer[0];
        avpicture_fill((AVPicture*)_SavingContext->pInputFrame, pRGB24Buffer, AV_PIX_FMT_BGR24, inWidth, inHeight);
        
        // Alter planes and stride to vertically flip image during conversion.
        _SavingContext->pInputFrame->data[0] += _SavingContext->pInputFrame->linesize[0] * (inHeight - 1);
        _SavingContext->pInputFrame->linesize[0] = - _SavingContext->pInputFrame->linesize[0];

        // Prepare the color space converted frame.
        if ((pYUV420Frame = av_frame_alloc()) == nullptr) 
        {
            log->Error("YUV420P frame not allocated");
            break;
        }

        int yuvBufferSize = avpicture_get_size(AV_PIX_FMT_YUV420P, outWidth, outHeight);
        pYUV420Buffer = (uint8_t*)av_malloc(yuvBufferSize);

        if (pYUV420Buffer == nullptr) 
        {
            log->Error("YUV frame buffer not allocated");
            break;
        }
        
        avpicture_fill((AVPicture*)pYUV420Frame, pYUV420Buffer, AV_PIX_FMT_YUV420P, outWidth, outHeight);
        
        // Perform the color space conversion.
        SwsContext* scalingContext = sws_getContext(
            inWidth, inHeight, AV_PIX_FMT_BGR24, 
            outWidth, outHeight, AV_PIX_FMT_YUV420P, SWS_FAST_BILINEAR,
            NULL, NULL, NULL);

        if (sws_scale(scalingContext, _SavingContext->pInputFrame->data, _SavingContext->pInputFrame->linesize, 0, inHeight, pYUV420Frame->data, pYUV420Frame->linesize) < 0) 
        {
            log->Error("scaling failed");
            sws_freeContext(scalingContext);
            break;
        }

        sws_freeContext(scalingContext);

        // Allocated JPEG frame buffer. 
        // Assumes uncompressed size is always smaller than compressed. (Not technically true).
        int jpegBufferSize = yuvBufferSize;
        pJpegBuffer = (uint8_t*)av_malloc(jpegBufferSize);

        if (pJpegBuffer == nullptr) 
        {
            log->Error("output video buffer not allocated");
            break;
        }

        // Actual encode.
        int jpegSize = avcodec_encode_video(_SavingContext->pOutputCodecContext, pJpegBuffer, jpegBufferSize, pYUV420Frame);

        if (jpegSize > 0)
            WriteFrame(jpegSize, _SavingContext, pJpegBuffer, true);

        written = true;
    }
    while(false);

    if (pJpegBuffer != nullptr)
        av_free(pJpegBuffer);

    if (pYUV420Frame != nullptr)
        av_free(pYUV420Frame);

    if (pYUV420Buffer != nullptr)
        av_free(pYUV420Buffer);

    //if (pRGB24Frame != nullptr)
        //av_free(pRGB24Frame);
    
    return written;
}

///<summary>
/// VideoFileWriter::EncodeAndWriteVideoFrameRGB24
///</summary>
bool MJPEGWriter::EncodeAndWriteVideoFrameY800(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length)
{
    bool written = false;
    bool yuvFrameAllocated = false;
    
    // init to nullptr.
    AVFrame* pInputFrame;
    AVFrame* pYuvFrame;
    uint8_t* pYuvBuffer;
    
    do
    {
        int outWidth = _SavingContext->outputSize.Width;
        int outHeight = _SavingContext->outputSize.Height;
        int inWidth = outWidth;
        int inHeight = outHeight;

        pin_ptr<uint8_t> pInputBuffer = &managedBuffer[0];

        // Put the input buffer inside an AVFrame.
        // This is needed only because we need to convert color space.
        if ((pInputFrame = av_frame_alloc()) == nullptr) 
        {
            log->Error("input frame not allocated");
            break;
        }

        avpicture_fill((AVPicture*)pInputFrame, pInputBuffer, AV_PIX_FMT_GRAY8, inWidth, inHeight);
        
        
        // Convert color space.
        // Unfortunately the MJPEG encoder doesn't know how to work directly with Y800/GRAY8 images.
        // Instead of directly pushing the buffer to the AVFrame we need to allocate a new buffer and convert it.

        if ((pYuvFrame = av_frame_alloc()) == nullptr) 
        {
            log->Error("YUV420P frame not allocated");
            break;
        }

        int yuvBufferSize = avpicture_get_size(AV_PIX_FMT_YUV420P, outWidth, outHeight);
        pYuvBuffer = (uint8_t*)av_malloc(yuvBufferSize);

        if (pYuvBuffer == nullptr) 
        {
            log->Error("Yuv frame buffer not allocated");
            av_free(pYuvFrame);
            break;
        }
        
        avpicture_fill((AVPicture*)pYuvFrame, pYuvBuffer, AV_PIX_FMT_YUV420P, outWidth, outHeight);

        yuvFrameAllocated = true;
        
        // Perform the color space conversion.
        SwsContext* scalingContext = sws_getContext(
            inWidth, inHeight, AV_PIX_FMT_GRAY8, 
            outWidth, outHeight, AV_PIX_FMT_YUV420P, 
            SWS_BICUBIC, NULL, NULL, NULL); 

        if (sws_scale(scalingContext, pInputFrame->data, pInputFrame->linesize, 0, inHeight, pYuvFrame->data, pYuvFrame->linesize) < 0) 
        {
            log->Error("scaling failed");
            sws_freeContext(scalingContext);
            break;
        }

        sws_freeContext(scalingContext);

        // Allocate output frame buffer. 
        // Assumes uncompressed size is always smaller than compressed. (Not technically true).
        int jpegBufferSize = yuvBufferSize;
        uint8_t* pJpegBuffer = (uint8_t*)av_malloc(jpegBufferSize);
        if (pJpegBuffer == nullptr) 
        {
            log->Error("output video buffer not allocated");
            break;
        }

        // Actual encode.
        int jpegSize = avcodec_encode_video(_SavingContext->pOutputCodecContext, pJpegBuffer, jpegBufferSize, pYuvFrame);

        if (jpegSize > 0)
            WriteFrame(jpegSize, _SavingContext, pJpegBuffer, true);
        
        av_free(pJpegBuffer);

        written = true;
    }
    while(false);

    if(yuvFrameAllocated)
    {
        av_free(pYuvFrame);
        av_free(pYuvBuffer);
    }

    // Test y800FrameAllocated and free it.
    
    return written;
}

///<summary>
/// VideoFileWriter::EncodeAndWriteVideoFrameJPEG
///</summary>
bool MJPEGWriter::EncodeAndWriteVideoFrameJPEG(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length)
{
    // As the buffer is already a JPEG sample, we bypass the encoding step entirely.
    bool bWritten = false;
    
    do
    {     
        pin_ptr<uint8_t> pOutputVideoBuffer = &managedBuffer[0];
        WriteFrame(length, _SavingContext, pOutputVideoBuffer, true);
        pOutputVideoBuffer = nullptr;
        bWritten = true;
    }
    while(false);

    return bWritten;
}

///<summary>
/// MJPEGWriter::WriteFrame
/// Commit a single frame in the video file.
///</summary>
bool MJPEGWriter::WriteFrame(int _iEncodedSize, SavingContext^ _SavingContext, uint8_t* _pOutputVideoBuffer, bool bForceKeyframe)
{
    AVPacket OutputPacket;
    av_init_packet(&OutputPacket);

    OutputPacket.stream_index = _SavingContext->pOutputVideoStream->index;
    OutputPacket.flags |= AV_PKT_FLAG_KEY;
    OutputPacket.data= _pOutputVideoBuffer;
    OutputPacket.size= _iEncodedSize;
    OutputPacket.pts = 0;

    // Commit the packet to the file.
    av_write_frame(_SavingContext->pOutputFormatContext, &OutputPacket);

    // Test save to individual file for debugging purposes.
    /*array<System::Byte>^ managedBuffer = gcnew array<System::Byte>(_iEncodedSize);
    Marshal::Copy(IntPtr((void*)_pOutputVideoBuffer), managedBuffer, 0, _iEncodedSize);
    
    FileStream^ fs = gcnew FileStream("frame-net" + m_iFrameIndex.ToString() + ".jpg", FileMode::Create, FileAccess::Write);
    fs->Write(managedBuffer, 0, _iEncodedSize);
    fs->Close();*/
    
    return true;
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
