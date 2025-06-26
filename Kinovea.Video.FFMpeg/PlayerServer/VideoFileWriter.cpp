/*
Copyright ?Joan Charmant 2008-2009.
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

#include "VideoFileWriter.h"

using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::Drawing::Drawing2D;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

using namespace Kinovea::Video;
using namespace Kinovea::Video::FFMpeg;

VideoFileWriter::VideoFileWriter()
{
    av_register_all();
}

VideoFileWriter::~VideoFileWriter()
{
}

VideoFileWriter::!VideoFileWriter()
{
}

SaveResult VideoFileWriter::Save(SavingSettings^ s, VideoInfo videoInfo, String^ formatString, IEnumerable<Bitmap^>^ images, BackgroundWorker^ worker)
{
    SaveResult result = SaveResult::Success;

    if(images == nullptr || worker == nullptr)
        return SaveResult::UnknownError;

    result = OpenSavingContext(s->File, videoInfo, formatString, s->OutputIntervalMilliseconds);

    if(result != SaveResult::Success)
    {
        log->Error("Saving context not opened.");
        return result;
    }

    int64_t i = 0;
    for each (Bitmap^ image in images)
    {
        if(worker->CancellationPending)
        {
            delete image;
            result = SaveResult::Cancelled;
            break;
        }
        
        result = SaveFrame(image);
        if(result != SaveResult::Success)
            log->Error("Frame not saved.");
        
        i++;
        worker->ReportProgress(i, s->TotalFrameCount);

        if(result != SaveResult::Success)
        {
            delete image;
            break;
        }
    }

    CloseSavingContext(true);

    if(result == SaveResult::Cancelled)
    {
        log->Debug("Saving cancelled by user, deleting temporary file.");
        if(File::Exists(s->File))
            File::Delete(s->File);
    }

    return result;
}

///<summary>
/// VideoFileWriter::OpenSavingContext
/// Open a saving context and configure it with default parameters.
///</summary>
SaveResult VideoFileWriter::OpenSavingContext(String^ _FilePath, VideoInfo _info, String^ _formatString, double _fFramesInterval)
{
    //---------------------------------------------------------------------------------------------------
    // Set the saving context.
    // Output file, encoding parameters, etc.
    // This will be used by the actual saving function.
    //---------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------
    // Sizes:
    // We'll get the frames as bitmaps, at the decoding size. (multiple of 4 and square pixels).
    // We'll scale the images before saving.
    // The final size will be the same as the original video.
    //---------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------------------------
    // [2009-08-01] - Stop using the input bitrate as a guideline for the output one.
    // The input bitrate may be the result of a codec more or less powerful than the one we will use here,
    // so it's obviously not a good indicator. 
    // (MPEG-4 AVC videos will always look bad when reencoded in MPEG-4 ASP at the same bitrate).
    //
    // Bottom line: until we have a two pass saving routine, we'll use 25 MB/s.
    //---------------------------------------------------------------------------------------------------

    // todo : group parameters list by simply passing the m_SavingContext.
    log->Debug("Opening the saving context.");

    SaveResult result = SaveResult::Success;

    if (m_SavingContext != nullptr) 
        delete m_SavingContext;
    
    m_SavingContext = gcnew SavingContext();
    m_Filename = _FilePath;
    m_SavingContext->pFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(_FilePath).ToPointer());
    
    if(!_info.ReferenceSize.IsEmpty)
        m_SavingContext->outputSize = _info.ReferenceSize;

    if(_info.PixelAspectRatio > 0)
        m_SavingContext->fPixelAspectRatio = _info.PixelAspectRatio;

    m_SavingContext->bInputWasMpeg2 = _info.IsCodecMpeg2;
    if(!_info.SampleAspectRatio.IsEmpty)
    {
        m_SavingContext->iSampleAspectRatioNumerator = (int)_info.SampleAspectRatio.Numerator;
        m_SavingContext->iSampleAspectRatioDenominator = (int)_info.SampleAspectRatio.Denominator;
    }

    if(_fFramesInterval > 0) 
        m_SavingContext->fFramesInterval = _fFramesInterval;
    
    m_SavingContext->iBitrate = ComputeBitrate(m_SavingContext->outputSize, m_SavingContext->fFramesInterval);
    
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

        // 2. Allocate muxer context.
        pin_ptr<AVFormatContext*> pinOutputFormatContext = &m_SavingContext->pOutputFormatContext;
        int averror = avformat_alloc_output_context2(pinOutputFormatContext, format, nullptr, nullptr);
        if (averror < 0)
        {
            result = SaveResult::MuxerParametersNotAllocated;
            LogError("Muxer parameters object not allocated", averror);
            break;
        }
        
        // 3. Configure muxer.
        if(!SetupMuxer(m_SavingContext))
        {
            result = SaveResult::MuxerParametersNotSet;
            log->Error("Muxer parameters not set");
            break;
        }

        // 4. Encoder selection
        if ((m_SavingContext->pOutputCodec = avcodec_find_encoder(CODEC_ID_MPEG4)) == nullptr)
        {
            result = SaveResult::EncoderNotFound;
            log->Error("Encoder not found");
            break;
        }
        
        // 5. Create video stream.
        m_SavingContext->pOutputVideoStream = avformat_new_stream(m_SavingContext->pOutputFormatContext, m_SavingContext->pOutputCodec);
        if (m_SavingContext->pOutputVideoStream == nullptr) 
        {
            result = SaveResult::VideoStreamNotCreated;
            log->Error("Video stream not created");
            break;
        }

        m_SavingContext->pOutputVideoStream->id = m_SavingContext->pOutputFormatContext->nb_streams - 1;

        // 6. Configure encoder.
        if(!SetupEncoder(m_SavingContext))
        {
            result = SaveResult::EncoderParametersNotSet;
            log->Error("Encoder parameters not set");
            break;
        }

        m_SavingContext->pOutputFormatContext->video_codec_id = m_SavingContext->pOutputCodec->id;

        // 7. Open the encoder.
        averror = avcodec_open2(m_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec, nullptr);
        if (averror < 0)
        {
            result = SaveResult::EncoderNotOpened;
            LogError("Encoder not opened", averror);
            break;
        }

        m_SavingContext->bEncoderOpened = true;
        
        // 8. Associate encoder to stream.
        m_SavingContext->pOutputVideoStream->codec = m_SavingContext->pOutputCodecContext;

        // 9. Open the file.
        averror = avio_open(&(m_SavingContext->pOutputFormatContext)->pb, m_SavingContext->pFilePath, AVIO_FLAG_WRITE);
        if (averror < 0) 
        {
            result = SaveResult::FileNotOpened;
            LogError("File not opened", averror);
            break;
        }

        SanityCheck(m_SavingContext->pOutputFormatContext);

        // 10. Write file header.
        averror = avformat_write_header(m_SavingContext->pOutputFormatContext, nullptr);
        if(averror < 0)
        {
            result = SaveResult::FileHeaderNotWritten;
            LogError("File header not written", averror);
            break;
        }

        // 11. Allocate memory for the current incoming frame holder. (will be reused for each frame). 
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
void VideoFileWriter::SanityCheck(AVFormatContext* s)
{
    // Taken/Adapted from the real sanity check from utils.c av_write_header.

    if (s->nb_streams != 1) 
    {
        log->Error("Sanity check failed: no streams.");
        return;
    }

    AVStream* st = s->streams[0];
    
    if (st->codec->codec_type != AVMEDIA_TYPE_VIDEO)
    {
        log->Error("Sanity check failed: not a video codec.");
        return;
    }
    
    if(st->codec->time_base.num <= 0 || st->codec->time_base.den <= 0)
        log->Error("VideoFileWriter sanity check failed: time base not set.");
        
    if(st->codec->width <= 0 || st->codec->height <= 0)
        log->Error("VideoFileWriter sanity check failed: dimensions not set.");
        
    if(av_cmp_q(st->sample_aspect_ratio, st->codec->sample_aspect_ratio))
    {
        log->Error("VideoFileWriter sanity check failed: Aspect ratio mismatch between encoder and muxer layer.");
        log->Debug(String::Format("stream SAR={0}:{1}, codec SAR:{2}:{3}", 
            st->sample_aspect_ratio.num, st->sample_aspect_ratio.den, st->codec->sample_aspect_ratio.num, st->codec->sample_aspect_ratio.den));
    }

    if(s->oformat->flags & AVFMT_GLOBALHEADER && !(st->codec->flags & CODEC_FLAG_GLOBAL_HEADER))
        log->Debug("VideoFileWriter sanity check warning: Codec does not use global headers but container format requires global headers");
}

///<summary>
/// VideoFileWriter::CloseSavingContext
/// Close the saving context and free any allocated resources.
///</summary>
SaveResult VideoFileWriter::CloseSavingContext(bool _bEncodingSuccess)
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
    
        // Free the InputFrame holder
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


///<summary>
/// VideoFileWriter::SaveFrame
/// Save a single bitmap in the file opened in a previous call to OpenSaving context.
///</summary>
SaveResult VideoFileWriter::SaveFrame(Bitmap^ _image)
{
    SaveResult result = SaveResult::Success;

    if(!EncodeAndWriteVideoFrame(m_SavingContext, _image))
    {
        log->Error("error while writing output frame");
        result = SaveResult::UnknownError;
    }

    return result;
}

double VideoFileWriter::ComputeBitrate(Size outputSize, double frameInterval)
{
    // Compute a bitrate equivalent to DV quality.
    // DV quality has a bitrate of 25 Mb/s for 720x576 px @ 30fps.
    // That translates to 2.01 bit per pixel.
    // Note that this parameter is not used anyway as we switched to constant quantization.

    double qualityFactor = 2.01;

    double pixelsPerFrame = outputSize.Width * outputSize.Height;
    double pixelsPerSecond = pixelsPerFrame * (1000.0 / frameInterval);
    double bitrate = pixelsPerSecond * qualityFactor;
    
    return bitrate;
}

///<summary>
/// VideoFileWriter::SetupMuxer
/// Configure the Muxer with default parameters.
///</summary>
bool VideoFileWriter::SetupMuxer(SavingContext^ _SavingContext)
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
/// VideoFileWriter::SetupEncoder
/// Configure the codec with default parameters.
///</summary>
bool VideoFileWriter::SetupEncoder(SavingContext^ _SavingContext)
{
    //----------------------------------------
    // Parameters for encoding.
    // some tweaked, some taken from Mencoder.
    // Not all clear...
    //----------------------------------------

    // TODO:
    // Implement from ref: http://www.mplayerhq.hu/DOCS/HTML/en/menc-feat-dvd-mpeg4.html

    log->Debug("Setting up the encoder.");

    _SavingContext->pOutputCodecContext = m_SavingContext->pOutputVideoStream->codec;
    avcodec_get_context_defaults3(_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec);

    // Codec.
    _SavingContext->pOutputCodecContext->codec_id = _SavingContext->pOutputCodec->id;
    _SavingContext->pOutputCodecContext->codec_type = AVMEDIA_TYPE_VIDEO;

    // Setting the four CC make avcodec_open2 fail.
    //_SavingContext->pOutputCodecContext->codec_tag = ('D'<<24) + ('I'<<16) + ('V'<<8) + 'X';

    // The average bitrate (unused for constant quantizer encoding.)
    _SavingContext->pOutputCodecContext->bit_rate = _SavingContext->iBitrate;

    // Number of bits the bitstream is allowed to diverge from the reference.
    // the reference can be CBR (for CBR pass1) or VBR (for pass2)
    // Source: Avidemux.
    _SavingContext->pOutputCodecContext->bit_rate_tolerance = 8000000;

    // Motion estimation algorithm used for video coding. 
    // src: MEncoder.
    _SavingContext->pOutputCodecContext->me_method = ME_EPZS;

    // Framerate - timebase.
    // Certains codecs (MPEG1/2) ne supportent qu'un certain nombre restreints de framerates.
    // src [kinovea]
    if(_SavingContext->fFramesInterval == 0)
        _SavingContext->fFramesInterval = 40;

    int iTimebase = (int)Math::Round(1000000.0f / _SavingContext->fFramesInterval);

    // Examples : 25000, 30000, 29970.
    // Treat some special cases and use rounding for the others.
    if(iTimebase == 29970)
    {
        log->Debug(String::Format("Pushing special timebase: 30000:1001"));
        _SavingContext->pOutputCodecContext->time_base.den	= 30000;
        _SavingContext->pOutputCodecContext->time_base.num	= 1001;
    }
    else if(iTimebase == 24975)
    {
        log->Debug(String::Format("Pushing special timebase: 25000:1001"));
        _SavingContext->pOutputCodecContext->time_base.den	= 25000;
        _SavingContext->pOutputCodecContext->time_base.num	= 1001;
    }
    else
    {
        double fps = 1000 / _SavingContext->fFramesInterval;
        _SavingContext->pOutputCodecContext->time_base.den = (int)Math::Round(1000 * fps);
        _SavingContext->pOutputCodecContext->time_base.num	= 1000;
    }
    
    // Picture width / height.
    // If we are transcoding from a video, this will be the same as the input size.
    // (not the decoding size).
    // src: [kinovea]
    _SavingContext->pOutputCodecContext->width				= _SavingContext->outputSize.Width;
    _SavingContext->pOutputCodecContext->height				= _SavingContext->outputSize.Height;
    

    //-------------------------------------------------------------------------------------------
    // Mode d'encodage (i, b, p frames)
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

    
    _SavingContext->pOutputCodecContext->strict_std_compliance = FF_COMPLIANCE_UNOFFICIAL;
    
    //-----------------------------------
    // h. Other settings. (From MEncoder) 
    //-----------------------------------
    //_SavingContext->pOutputCodecContext->i_luma_elim = 0;		// luma single coefficient elimination threshold
    //_SavingContext->pOutputCodecContext->i_chroma_elim = 0;		// chroma single coeff elimination threshold
    _SavingContext->pOutputCodecContext->lumi_masking = 0.0;
    _SavingContext->pOutputCodecContext->dark_masking = 0.0;
    // pre_me (prepass for motion estimation)
    // sample_rate
    // codecContext->channels = 2;
    // codecContext->mb_decision = 0;

    if (_SavingContext->pOutputFormatContext->oformat->flags & AVFMT_GLOBALHEADER)
        _SavingContext->pOutputCodecContext->flags |= CODEC_FLAG_GLOBAL_HEADER;

    return true;
}

///<summary>
/// VideoFileWriter::EncodeAndWriteVideoFrame
/// Save a single frame in the video file. Takes a Bitmap as input.
///</summary>
bool VideoFileWriter::EncodeAndWriteVideoFrame(SavingContext^ _SavingContext, Bitmap^ _InputBitmap)
{
    bool written = false;
    bool isBitmapLocked = false;
    
    AVFrame* pInputFrame = nullptr;
    AVFrame* pYUV420Frame = nullptr;
    uint8_t* pYUV420Buffer = nullptr;
    uint8_t* pOutputBuffer = nullptr;
    System::Drawing::Imaging::BitmapData^ bitmapData;

    AVPixelFormat pixelFormatInput = AV_PIX_FMT_BGRA;
    if(_InputBitmap->PixelFormat == Imaging::PixelFormat::Format32bppPArgb || _InputBitmap->PixelFormat == Imaging::PixelFormat::Format32bppArgb)
        pixelFormatInput = AV_PIX_FMT_BGRA;
    else if(_InputBitmap->PixelFormat == Imaging::PixelFormat::Format24bppRgb)
        pixelFormatInput = AV_PIX_FMT_BGR24;
    else if(_InputBitmap->PixelFormat == Imaging::PixelFormat::Format8bppIndexed)
        pixelFormatInput = PIX_FMT_BGR8; // AV_PIX_FMT_GRAY8 ?
    
    do
    {
        int inWidth = _InputBitmap->Width;
        int inHeight = _InputBitmap->Height;
        int outWidth = _SavingContext->outputSize.Width;
        int outHeight = _SavingContext->outputSize.Height;

        // Allocate the input frame that will wrap the bitmap content.
        if ((pInputFrame = av_frame_alloc()) == nullptr) 
        {
            log->Error("input frame not allocated");
            break;
        }
        
        // Associate the Bitmap data to the AVFrame
        Rectangle rect = Rectangle(0, 0, inWidth, inHeight);
        bitmapData = _InputBitmap->LockBits(rect, Imaging::ImageLockMode::ReadOnly, _InputBitmap->PixelFormat);
        isBitmapLocked = true;
        
        pin_ptr<uint8_t> pRGBBuffer = (uint8_t*)bitmapData->Scan0.ToPointer();
        avpicture_fill((AVPicture *)pInputFrame, pRGBBuffer, pixelFormatInput, inWidth, inHeight);
        
        // -> At that point, pInputFrame holds a non compressed bitmap in the input pixel format (ex: RGB24).
        // This bitmap is still at the decoding size.

        // Prepare the resized / color space converted frame.
        if ((pYUV420Frame = av_frame_alloc()) == nullptr) 
        {
            log->Error("output frame not allocated");
            break;
        }
         
        int yuvBufferSize = avpicture_get_size(AV_PIX_FMT_YUV420P, outWidth, outHeight);
        pYUV420Buffer = (uint8_t*)av_malloc(yuvBufferSize);

        if (pYUV420Buffer == nullptr) 
        {
            log->Error("YUV frame buffer not allocated");
            break;
        }

        avpicture_fill((AVPicture *)pYUV420Frame, pYUV420Buffer, AV_PIX_FMT_YUV420P, outWidth, outHeight);
        
        // Perform the color space conversion and resizing.
        SwsContext* scalingContext = sws_getContext(
            inWidth, inHeight, pixelFormatInput, 
            outWidth, outHeight, AV_PIX_FMT_YUV420P, SWS_BICUBIC,
            NULL, NULL, NULL);

        if (sws_scale(scalingContext, pInputFrame->data, pInputFrame->linesize, 0, inHeight, pYUV420Frame->data, pYUV420Frame->linesize) < 0) 
        {
            log->Error("scaling failed");
            sws_freeContext(scalingContext);
            break;
        }

        sws_freeContext(scalingContext);

        // Allocate encoded frame buffer.
        // Assumes compressed size is always smaller than uncompressed. (Not technically true).
        int encodedBufferSize = outWidth * outHeight * 4;
        pOutputBuffer = (uint8_t*)av_malloc(encodedBufferSize);
        
        if (pOutputBuffer == nullptr) 
        {
            log->Error("output video buffer not allocated");
            break;
        }
        
        // Actual encoding step.
        // AccessViolationException ? => memalign issue, requires recompiling libavc with the correct gcc.
        int encodedSize = avcodec_encode_video(_SavingContext->pOutputCodecContext, pOutputBuffer, encodedBufferSize, pYUV420Frame);
        
        // Write the video packet in the output.
        if (encodedSize > 0)
        {   
            if (!WriteFrame(encodedSize, _SavingContext, pOutputBuffer, true))
            {
                log->Error("problem while writing frame to file");
                break;
            }
            
            written = true;
        }
    }
    while(false);

    if (pOutputBuffer)
        av_free(pOutputBuffer);

    if (pInputFrame != nullptr)
        av_free(pInputFrame);

    if(isBitmapLocked)
        _InputBitmap->UnlockBits(bitmapData);

    if (pYUV420Frame != nullptr)
        av_free(pYUV420Frame);

    if (pYUV420Buffer != nullptr)
        av_free(pYUV420Buffer);

    // (Temporary) fix to OOM error that sometimes happen with very large image size.
    GC::Collect();

    return written;
}

///<summary>
/// VideoFileWriter::WriteBuffer
/// Commit a single frame in the video file.
///</summary>
bool VideoFileWriter::WriteFrame(int _iEncodedSize, SavingContext^ _SavingContext, uint8_t* _pOutputVideoBuffer, bool bForceKeyframe)
{
    if (_iEncodedSize > 0) 
    {
        AVPacket OutputPacket;
        av_init_packet(&OutputPacket);

        // Compute packet position.
        OutputPacket.pts = av_rescale_q(_SavingContext->pOutputCodecContext->coded_frame->pts, _SavingContext->pOutputCodecContext->time_base, _SavingContext->pOutputVideoStream->time_base);

        // Flag Keyframes as such.
        if(_SavingContext->pOutputCodecContext->coded_frame->key_frame || bForceKeyframe)
        {
            OutputPacket.flags |= AV_PKT_FLAG_KEY;
        }

        // Associate various buffers before the commit.
        OutputPacket.stream_index = _SavingContext->pOutputVideoStream->index;
        OutputPacket.data= _pOutputVideoBuffer;
        OutputPacket.size= _iEncodedSize;

        // Commit the packet to the file.
        int iWriteRes = av_write_frame(_SavingContext->pOutputFormatContext, &OutputPacket);
    } 
    else
    {
        log->Error("encoded size not positive");
    }

    return true;
}

void VideoFileWriter::LogError(String^ context, int error)
{
    char errbuf[256];
    av_strerror(error, errbuf, sizeof(errbuf));
    String^ message = Marshal::PtrToStringAnsi((IntPtr)errbuf);
    log->Error(String::Format("{0}, Error:{1}", context, message));
}

int VideoFileWriter::GreatestCommonDenominator(int a, int b)
{
     if (a == 0) return b;
     if (b == 0) return a;

     if (a > b)
        return GreatestCommonDenominator(a % b, b);
     else
        return GreatestCommonDenominator(a, b % a);
}
