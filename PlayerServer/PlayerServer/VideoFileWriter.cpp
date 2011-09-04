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

SaveResult VideoFileWriter::Save(SavingSettings _settings, VideoReader^ _reader, BackgroundWorker^ _worker)
{
	//------------------------------------------------------------------------------------
	// Input parameters depending on type of save:
	// Classic save : 
	//		_fFramesInterval = used for all frames 
	//		_bFlushDrawings = true or false
	//		_bKeyframesOnly = false.
	// Diaporama : 
	//		_fFramesInterval = used for keyframes. Other frames aren't saved anyway.
	//		_bFlushDrawings = true.
	//		_bKeyframesOnly = true.
	// Paused Video : 
	//		_fFramesInterval = used for keyframes only, other frames at original interval. 
	//		_bFlushDrawings = true.
	//		_bKeyframesOnly = false.
	//	In this last case, _fFramesInterval must be a multiple of original interval.
	//------------------------------------------------------------------------------------


	log->Debug(String::Format("Saving selection [{0}]->[{1}] to: {2}", _settings.Section.Start, _settings.Section.End, Path::GetFileName(_settings.File)));

	SaveResult result = SaveResult::Success;
	
	do
	{
		// Refact: Check this in caller.
		/*if(!m_bIsLoaded) 
		{
			result = SaveResult::MovieNotLoaded;
			break;
		}*/
	
		if(_settings.ImageRetriever == nullptr)
		{
			result = SaveResult::UnknownError;
			break;
		}		
		
		// 1. Get required parameters for opening the saving context.
		bool bHasMetadata = !String::IsNullOrEmpty(_settings.Metadata);
		double fFramesInterval = 40;
		int iDuplicateFactor = 1;

		if(_settings.FrameInterval > 0) 
		{
			if(_settings.PausedVideo)
			{
				// bPausedVideo is a mode where the video runs at the same speed as the original
				// except for the key images which are paused for _fFramesInterval.
				// In this case, _fFramesInterval should be a multiple of the original frame interval.
						
				iDuplicateFactor = (int)(_settings.FrameInterval / _reader->Info.FrameIntervalMilliseconds);
				fFramesInterval = _reader->Info.FrameIntervalMilliseconds;
			}
			else
			{
				// In normal mode, the frame interval can not go down indefinitely.
				// We can't save at less than 8 fps, so we duplicate frames when necessary.
				
				iDuplicateFactor = (int)Math::Ceiling(_settings.FrameInterval / 125.0);
				fFramesInterval = _settings.FrameInterval  / iDuplicateFactor;	
				log->Debug(String::Format("fFramesInterval:{0}, iDuplicateFactor:{1}", fFramesInterval, iDuplicateFactor));
			}			
		}

		// 2. Open the saving context.
		result = OpenSavingContext(	_settings.File, _reader->Info, fFramesInterval, bHasMetadata);
		if(result != SaveResult::Success)
		{
			log->Error("Saving context not opened.");
			break;
		}

		// 3. Write metadata if needed.
		if(bHasMetadata)
		{
			if((result = SaveMetadata(_settings.Metadata)) != SaveResult::Success)
			{
				log->Error("Metadata not saved.");
				break;
			}
		}

		// 4. Loop through input frames and save them.
		// We use two different loop depending if frames are already available or not.
		if(_reader->Caching)
		{
			log->Debug("Analysis mode: looping through images already extracted in memory.");
			int i = 0;
			for each (VideoFrame^ vf in _reader->Cache)
			{
				Bitmap^ InputBitmap = AForge::Imaging::Image::Clone(vf->Image, vf->Image->PixelFormat);

				// following block is duplicated in both analysis mode loop and normal mode loop.

				// Commit drawings on image if needed.
				// The function returns the distance to the closest kf.
				int64_t iKeyImageDistance = _settings.ImageRetriever(Graphics::FromImage(InputBitmap), 
																	InputBitmap, 
																	vf->Timestamp,
																	_settings.FlushDrawings, 
																	_settings.KeyframesOnly);
				
				if(iKeyImageDistance == 0 || !_settings.KeyframesOnly)
				{
					if(_settings.PausedVideo && iKeyImageDistance != 0)
					{
						// Normal images in paused video mode are played at normal speed.
						// In paused video mode duplicate factor only applies to the key images.
						result = SaveFrame(InputBitmap);
						if(result != SaveResult::Success)
							log->Error("Frame not saved.");
					}
					else
					{
						for(int iDuplicate=0;iDuplicate<iDuplicateFactor;iDuplicate++)
						{
							result = SaveFrame(InputBitmap);
							if(result != SaveResult::Success)
								log->Error("Frame not saved.");
						}
					}
				}
				
				delete InputBitmap;

				// Report progress.
				if(_worker != nullptr)
				{
					if(_worker->CancellationPending)
						result = SaveResult::Cancelled;
					else
						_worker->ReportProgress(i+1, _reader->Cache->Count);
				}

				if(result != SaveResult::Success)
					break;
				
				i++;
			}
		}
		else
		{
			ReadResult res = ReadResult::Success;
			bool read = false;
			
			log->Debug("Normal mode: looping to read images from the file.");
			
			bool bFirstFrame = true;
			bool done = false;
			do
			{
				if(bFirstFrame)
				{
					// Reading the first frame individually as we need to ensure we are reading the right one.
					// (some codecs go too far on the initial seek.)
					//res = ReadFrame(_settings.Section.Start, 1);
					//log->Debug(String::Format("After first frame ts: {0}", m_PrimarySelection->iCurrentTimeStamp));
					read = _reader->MoveFirst();
					log->Debug(String::Format("After first frame ts: {0}", _reader->Cache->Current->Timestamp));
					bFirstFrame = false;
				}
				else
				{
					read = _reader->MoveNext(true);
				}

				//if(res == ReadResult::Success)
				if(read)
				{
					VideoFrame^ vf = _reader->Cache->Current;

					// Get a bitmap version.
					Bitmap^ InputBitmap = AForge::Imaging::Image::Clone(vf->Image, vf->Image->PixelFormat);

					// Commit drawings on image if needed.
					// The function returns the distance to the closest kf.
					int64_t iKeyImageDistance = _settings.ImageRetriever(Graphics::FromImage(InputBitmap), 
																		InputBitmap, 
																		vf->Timestamp, 
																		_settings.FlushDrawings, 
																		_settings.KeyframesOnly);
					if(!_settings.KeyframesOnly || iKeyImageDistance == 0)
					{
						if(_settings.PausedVideo && iKeyImageDistance != 0)
						{
							// Normal images in paused video mode are played at normal speed.
							// In paused video mode duplicate factor only applies to the key images.
							result = SaveFrame(InputBitmap);
							if(result != SaveResult::Success)
								log->Error("Frame not saved.");
						}
						else
						{
							for(int iDuplicate=0;iDuplicate<iDuplicateFactor;iDuplicate++)
							{
								result = SaveFrame(InputBitmap);
								if(result != SaveResult::Success)
								{
									log->Error("Frame not saved.");
									done = true;
								}
							}
						}
					}

					delete InputBitmap;

					// Report progress.
					if(_worker != nullptr)
						_worker->ReportProgress((int)(vf->Timestamp - _settings.Section.Start), (int)(_settings.Section.End - _settings.Section.Start));
					
					if(vf->Timestamp >= _settings.Section.End)
						done = true;
				}
				else
				{
					// This can be normal as when the file is over we get an FrameNotRead error.
					//if(res != ReadResult::FrameNotRead)
// instead : if(last read ->timestamp < _settings.Section.End).
					//	result = SaveResult::ReadingError;
					
					done = true;
				}

				// Check for cancellation
				if(_worker != nullptr && _worker->CancellationPending)
				{
					// Stop all operations and delete file.
					result = SaveResult::Cancelled;
					done = true;
				}

			}
			while(!done);
		}
		
		// Close the saving context.
		CloseSavingContext(true);
	
	}
	while(false);


	if(result == SaveResult::Cancelled)
	{
		// Delete the file.
		log->Debug("Saving cancelled by user, deleting temporary file.");
		if(File::Exists(_settings.File))
			File::Delete(_settings.File);
	}

	return result;
}





///<summary>
/// VideoFileWriter::OpenSavingContext
/// Open a saving context and configure it with default parameters.
///</summary>
SaveResult VideoFileWriter::OpenSavingContext(String^ _FilePath, VideoInfo _info, double _fFramesInterval, bool _bHasMetadata)
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

	if(m_SavingContext != nullptr) delete m_SavingContext;
	
	m_SavingContext = gcnew SavingContext();

	m_SavingContext->pFilePath = static_cast<char*>(Marshal::StringToHGlobalAnsi(_FilePath).ToPointer());
	
	// Apparently not all output size are ok, some crash sws_scale.
	// We will keep the input size and use the input pixel aspect ratio for maximum compatibility.
	// [2011-08-21] - Check if the issue with output size is related to odd number of rows.
	if(!_info.OriginalSize.IsEmpty)
		m_SavingContext->outputSize = _info.OriginalSize;
	
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
	
	do
	{
		// 1. Muxer selection.
		if ((m_SavingContext->pOutputFormat = VideoFileWriter::GuessOutputFormat(_FilePath, _bHasMetadata)) == nullptr) 
		{
			result = SaveResult::MuxerNotFound;
			log->Error("Muxer not found");
			break;
		}

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
		if ((m_SavingContext->pOutputVideoStream = av_new_stream(m_SavingContext->pOutputFormatContext, 0)) == nullptr) 
		{
			result = SaveResult::VideoStreamNotCreated;
			log->Error("Video stream not created");
			break;
		}

		// 5. Encoder selection
		if ((m_SavingContext->pOutputCodec = avcodec_find_encoder(CODEC_ID_MPEG4)) == nullptr)
		{
			result = SaveResult::EncoderNotFound;
			log->Error("Encoder not found");
			break;
		}

		// 6. Allocate encoder parameters object.
		if ((m_SavingContext->pOutputCodecContext = avcodec_alloc_context()) == nullptr) 
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

		// 8. Open encoder.
		if (avcodec_open(m_SavingContext->pOutputCodecContext, m_SavingContext->pOutputCodec) < 0)
		{
			result = SaveResult::EncoderNotOpened;
			log->Error("Encoder not opened");
			break;
		}

		m_SavingContext->bEncoderOpened = true;
		
		// 9. Associate encoder to stream.
		m_SavingContext->pOutputVideoStream->codec = m_SavingContext->pOutputCodecContext;

		
		if(_bHasMetadata)
		{
			log->Debug("Muxing metadata into a subtitle stream.");

			// Create metadata stream.
			if ((m_SavingContext->pOutputDataStream = av_new_stream(m_SavingContext->pOutputFormatContext, 1)) == nullptr) 
			{
				result = SaveResult::MetadataStreamNotCreated;
				log->Error("metadata stream not created");
				break;
			}

			// Get default configuration for subtitle streams.
			// (Will allocate pointed CodecCtx)
			avcodec_get_context_defaults2(m_SavingContext->pOutputDataStream->codec, AVMEDIA_TYPE_SUBTITLE);
			
			// Identify codec. Will show as "S_TEXT/UTF8" for Matroska.
			m_SavingContext->pOutputDataStream->codec->codec_id = CODEC_ID_TEXT;

			// ISO 639 code for subtitle language. ( -> en.wikipedia.org/wiki/List_of_ISO_639-3_codes)	 
			// => "Malaysian Sign Language" code is "XML" :-)
			av_metadata_set2(&m_SavingContext->pOutputDataStream->metadata, "language", "XML", 0);
		}

		int iFFMpegResult;

		// 10. Open the file.
		if ((iFFMpegResult = url_fopen(&(m_SavingContext->pOutputFormatContext)->pb, m_SavingContext->pFilePath, URL_WRONLY)) < 0) 
		{
			result = SaveResult::FileNotOpened;
			log->Error(String::Format("File not opened, AVERROR:{0}", iFFMpegResult));
			break;
		}

		// 11. Write file header.
		SanityCheck(m_SavingContext->pOutputFormatContext);
		if((iFFMpegResult = av_write_header(m_SavingContext->pOutputFormatContext)) < 0)
		{
			result = SaveResult::FileHeaderNotWritten;
			log->Error(String::Format("File header not written, AVERROR:{0}", iFFMpegResult));
			break;
		}

		// 12. Allocate memory for the current incoming frame holder. (will be reused for each frame). 
		if ((m_SavingContext->pInputFrame = avcodec_alloc_frame()) == nullptr) 
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

	if (s->nb_streams == 0) 
	{
		log->Error("VideoFileWriter sanity check failed: no streams.");
    }

	AVStream *st;
    for(unsigned int i=0;i < s->nb_streams;i++) 
	{
        st = s->streams[i];

        switch (st->codec->codec_type) 
		{
			case AVMEDIA_TYPE_VIDEO:
				if(st->codec->time_base.num <= 0 || st->codec->time_base.den <= 0)
				{ 
					log->Error("VideoFileWriter sanity check failed: time base not set.");
				}
				if(st->codec->width <= 0 || st->codec->height <= 0)
				{
					log->Error("VideoFileWriter sanity check failed: dimensions not set.");
				}
				if(av_cmp_q(st->sample_aspect_ratio, st->codec->sample_aspect_ratio))
				{
					log->Error("VideoFileWriter sanity check failed: Aspect ratio mismatch between encoder and muxer layer.");
					log->Debug(String::Format("stream SAR={0}:{1}, codec SAR:{2}:{3}", 
						st->sample_aspect_ratio.num, st->sample_aspect_ratio.den, st->codec->sample_aspect_ratio.num, st->codec->sample_aspect_ratio.den));
				}
				break;
		}

        if(s->oformat->flags & AVFMT_GLOBALHEADER && !(st->codec->flags & CODEC_FLAG_GLOBAL_HEADER))
		{
			log->Debug("VideoFileWriter sanity check warning: Codec does not use global headers but container format requires global headers");
		}
    }
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
	url_fclose(m_SavingContext->pOutputFormatContext->pb);

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

///<summary>
/// VideoFileWriter::SaveMetadata
/// Save an xml string in the file opened in a previous call to OpenSaving context.
///</summary>
SaveResult VideoFileWriter::SaveMetadata(String^ _Metadata)
{
	log->Debug("Saving metadata to file.");
	SaveResult result = SaveResult::Success;

	if(!WriteMetadata(m_SavingContext, _Metadata))
	{
		log->Error("metadata not written");
		result = SaveResult::MetadataNotWritten;
	}
	
	return result;
}

///<summary>
/// VideoFileWriter::GuessOutputFormat
/// Return the AVOutputFormat corresponding to a specific filename.
/// Forces to Matroska if Metadata must be saved.
///</summary>
AVOutputFormat* VideoFileWriter::GuessOutputFormat(String^ _FilePath, bool _bHasMetadata)
{
	//---------------------------------------------------------------
	// Hint:
	// To find a particular format name for ffmpeg,
	// search for the AVOutputFormat struct in source code for the format.
	// generally at the end of the file.
	//---------------------------------------------------------------

	AVOutputFormat*		pOutputFormat;

	String^ Filepath = gcnew String(_FilePath->ToLower());

	if(Filepath->EndsWith("mkv") || _bHasMetadata)
	{
		pOutputFormat = av_guess_format("matroska", nullptr, nullptr);
	}
	else if(Filepath->EndsWith("mp4")) 
	{
		pOutputFormat = av_guess_format("mp4", nullptr, nullptr);
	}
	else
	{
		pOutputFormat = av_guess_format("avi", nullptr, nullptr);
	}

	return pOutputFormat;
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
		
	_SavingContext->pOutputFormatContext->timestamp = 0;
		
	_SavingContext->pOutputFormatContext->bit_rate = _SavingContext->iBitrate;

		
	// Paramètres (par défaut ?) du muxeur
	AVFormatParameters	fpOutFile;
	memset(&fpOutFile, 0, sizeof(AVFormatParameters));
	if (av_set_parameters(_SavingContext->pOutputFormatContext, &fpOutFile) < 0)
	{
		log->Error("muxer parameters not set");
		return false;
	}

	// ?
	_SavingContext->pOutputFormatContext->preload   = (int)(0.5 * AV_TIME_BASE);
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

	// Codec.
	// Equivalent to : -vcodec mpeg4
	_SavingContext->pOutputCodecContext->codec_id = _SavingContext->pOutputCodec->id;
	_SavingContext->pOutputCodecContext->codec_type = AVMEDIA_TYPE_VIDEO;

	// By default the fourcc is 'FMP4' but Windows Media Player doesn't recognize it.
	// We'll force to 'XVID' fourcc. (similar as -vtag XVID) even if it wasn't the XviD codec that encoded the video :-(
	_SavingContext->pOutputCodecContext->codec_tag = ('D'<<24) + ('I'<<16) + ('V'<<8) + 'X';

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
		_SavingContext->pOutputCodecContext->time_base.den			= 30000;
		_SavingContext->pOutputCodecContext->time_base.num			= 1001;
	}
	else if(iTimebase == 24975)
	{
		log->Debug(String::Format("Pushing special timebase: 25000:1001"));
		_SavingContext->pOutputCodecContext->time_base.den			= 25000;
		_SavingContext->pOutputCodecContext->time_base.num			= 1001;
	}
	else
	{
		iTimebase = (int)Math::Round((double)iTimebase / 1000);
		log->Debug(String::Format("Pushing timebase: {0}:1", iTimebase));
		_SavingContext->pOutputCodecContext->time_base.den			= iTimebase;
		_SavingContext->pOutputCodecContext->time_base.num			= 1;
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
	_SavingContext->pOutputCodecContext->pix_fmt = PIX_FMT_YUV420P; 	


	// Frame rate emulation. If not zero, the lower layer (i.e. format handler) has to read frames at native frame rate.
	// src: ?
	// ->rate_emu


	// Quality/Technique of encoding.
	//_pOutputCodecContext->flags |= ;			// CODEC_FLAG_QSCALE : Constant Quantization = Best quality but innacceptably high file sizes.
	_SavingContext->pOutputCodecContext->qcompress = 0.5;		// amount of qscale change between easy & hard scenes (0.0-1.0) 
    _SavingContext->pOutputCodecContext->qblur = 0.5;			// amount of qscale smoothing over time (0.0-1.0)
	_SavingContext->pOutputCodecContext->qmin = 2;				// minimum quantizer (def:2)
	_SavingContext->pOutputCodecContext->qmax = 16;			// maximum quantizer (def:31)
	_SavingContext->pOutputCodecContext->max_qdiff = 3;		// maximum quantizer difference between frames (def:3)
	_SavingContext->pOutputCodecContext->mpeg_quant = 0;		// 0 -> h263 quant, 1 -> mpeg quant. (def:0)
	//_pOutputCodecContext->b_quant_factor (qscale factor between IP and B-frames)


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
	_SavingContext->pOutputCodecContext->luma_elim_threshold = 0;		// luma single coefficient elimination threshold
	_SavingContext->pOutputCodecContext->chroma_elim_threshold = 0;		// chroma single coeff elimination threshold
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
/// VideoFileWriter::WriteMetadata
/// Save the xml data in the video file.
///</summary>
bool VideoFileWriter::WriteMetadata(SavingContext^ _SavingContext, String^ _Metadata)
{
	// Create packet.
	AVPacket OutputPacket;
	av_init_packet(&OutputPacket);					
	
	// Packet position.
	OutputPacket.pts = av_rescale_q(_SavingContext->pOutputCodecContext->coded_frame->pts, _SavingContext->pOutputCodecContext->time_base, _SavingContext->pOutputVideoStream->time_base);

	// Associate packet to subtitle stream.
	OutputPacket.stream_index = _SavingContext->pOutputDataStream->index;

	char* pMetadata	= static_cast<char*>(Marshal::StringToHGlobalAnsi(_Metadata).ToPointer());

	OutputPacket.data = (uint8_t*)pMetadata;
	OutputPacket.size = _Metadata->Length;
	
	// Write to output file.
	int iWriteRes = av_write_frame(_SavingContext->pOutputFormatContext, &OutputPacket);
	
	Marshal::FreeHGlobal(safe_cast<IntPtr>(pMetadata));

	return (iWriteRes == 0);
}
///<summary>
/// VideoFileWriter::EncodeAndWriteVideoFrame
/// Save a single frame in the video file. Takes a Bitmap as input.
///</summary>
bool VideoFileWriter::EncodeAndWriteVideoFrame(SavingContext^ _SavingContext, Bitmap^ _InputBitmap)
{
	bool bWritten = false;
	bool bInputFrameAllocated = false;
	bool bOutputFrameAllocated = false;
	bool bBitmapLocked = false;
	
	AVFrame* pInputFrame;
	uint8_t* pInputFrameBuffer;
	AVFrame* pOutputFrame;
	uint8_t* pOutputFrameBuffer;
	System::Drawing::Imaging::BitmapData^ InputDataBitmap;
	enum PixelFormat pixelFormatFFmpeg;

	if(_InputBitmap->PixelFormat == Imaging::PixelFormat::Format32bppPArgb)
	{
		pixelFormatFFmpeg = PIX_FMT_BGRA;	
	}
	else
	{
		pixelFormatFFmpeg = PIX_FMT_BGR24;
	}

	do
	{
		// Allocate the input frame that we will fill up with the bitmap.
		if ((pInputFrame = avcodec_alloc_frame()) == nullptr) 
		{
			log->Error("input frame not allocated");
			break;
		}	
		
		// Allocate the buffer holding actual frame data.
		int iSizeInputFrameBuffer = avpicture_get_size(pixelFormatFFmpeg, _InputBitmap->Width, _InputBitmap->Height);
		pInputFrameBuffer = (uint8_t*)av_malloc(iSizeInputFrameBuffer);
		if (pInputFrameBuffer == nullptr) 
		{
			log->Error("input frame buffer not allocated");
			av_free(pInputFrame);
			break;
		}

		bInputFrameAllocated = true;
		
		// Setting up various pointers between the buffers.
		avpicture_fill((AVPicture *)pInputFrame, pInputFrameBuffer, pixelFormatFFmpeg, _InputBitmap->Width, _InputBitmap->Height);
		
		// Associate the Bitmap to the AVFrame
		Rectangle rect = Rectangle(0, 0, _InputBitmap->Width, _InputBitmap->Height);
		InputDataBitmap = _InputBitmap->LockBits(rect, Imaging::ImageLockMode::ReadOnly, _InputBitmap->PixelFormat);
		bBitmapLocked = true;
		// todo : pin_ptr ?
		uint8_t* data = (uint8_t*)InputDataBitmap->Scan0.ToPointer();
		pInputFrame->data[0] = data;
		pInputFrame->linesize[0] = InputDataBitmap->Stride;
			

		//------------------------------------------------------------------------------------------
		// -> At that point, pInputFrame holds a non compressed bitmap, at the .NET PIX_FMT (BGRA)
		// This bitmap is still at the decoding size.
		//------------------------------------------------------------------------------------------

		// f. L'objet frame receptacle de sortie.
		if ((pOutputFrame = avcodec_alloc_frame()) == nullptr) 
		{
			log->Error("output frame not allocated");
			break;
		}
		
		// g. Le poids d'une image selon le PIX_FMT de sortie et la taille donnée. 
		int iSizeOutputFrameBuffer = avpicture_get_size(_SavingContext->pOutputCodecContext->pix_fmt, _SavingContext->outputSize.Width, _SavingContext->outputSize.Height);
		
		// h. Allouer le buffer contenant les données réelles de la frame.
		pOutputFrameBuffer = (uint8_t*)av_malloc(iSizeOutputFrameBuffer);
		if (pOutputFrameBuffer == nullptr) 
		{
			log->Error("output frame buffer not allocated");
			av_free(pOutputFrame);
			break;
		}

		bOutputFrameAllocated = true;

		// i. Mise en place de pointeurs internes reliant certaines adresses à d'autres.
		avpicture_fill((AVPicture *)pOutputFrame, pOutputFrameBuffer, _SavingContext->pOutputCodecContext->pix_fmt, _SavingContext->outputSize.Width, _SavingContext->outputSize.Height);
		
		// j. Nouveau scaling context
		SwsContext* scalingContext = sws_getContext(_InputBitmap->Width, _InputBitmap->Height, pixelFormatFFmpeg, _SavingContext->outputSize.Width, _SavingContext->outputSize.Height, _SavingContext->pOutputCodecContext->pix_fmt, SWS_BICUBIC, NULL, NULL, NULL); 

		// k. Convertir l'image de son format de pixels d'origine vers le format de pixels de sortie.
		if (sws_scale(scalingContext, pInputFrame->data, pInputFrame->linesize, 0, _InputBitmap->Height, pOutputFrame->data, pOutputFrame->linesize) < 0) 
		{
			log->Error("scaling failed");
			sws_freeContext(scalingContext);
			break;
		}

		sws_freeContext(scalingContext);


		//------------------------------------------------------------------------------------------
		// -> Ici, pOutputFrame contient une bitmap non compressée, au nouveau PIX_FMT. (=> YUV420P)
		//------------------------------------------------------------------------------------------


		// f. allouer le buffer pour les données de la frame après compression. ( -> valeur tirée de ffmpeg.c)
		int iSizeOutputVideoBuffer = 4 *  _SavingContext->outputSize.Width *  _SavingContext->outputSize.Height;		
		uint8_t* pOutputVideoBuffer = (uint8_t*)av_malloc(iSizeOutputVideoBuffer);
		if (pOutputVideoBuffer == nullptr) 
		{
			log->Error("output video buffer not allocated");
			break;
		}
		
		// g. encodage vidéo.
		// AccessViolationException ? => problème de memalign. Recompiler libavc avec le bon gcc.
		int iEncodedSize = avcodec_encode_video(_SavingContext->pOutputCodecContext, pOutputVideoBuffer, iSizeOutputVideoBuffer, pOutputFrame);
		
		// Ecriture du packet vidéo dans le fichier (Force Keyframe)
		if(!WriteFrame(iEncodedSize, _SavingContext, pOutputVideoBuffer, true ))
		{
			log->Error("problem while writing frame to file");
		}
		
		av_free(pOutputVideoBuffer);

		bWritten = true;
	}
	while(false);

	// Cleanup
	if(bInputFrameAllocated)
	{
		av_free(pInputFrameBuffer);
		av_free(pInputFrame);
	}

	if(bBitmapLocked)
	{
		_InputBitmap->UnlockBits(InputDataBitmap);
	}

	if(bOutputFrameAllocated)
	{
		av_free(pOutputFrameBuffer);
		av_free(pOutputFrame);
	}

	// (Temporary) fix to OOM error that sometimes happen with very large image size.
	GC::Collect();

	return bWritten;
}

///<summary>
/// VideoFileWriter::WriteFrame
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




int VideoFileWriter::GreatestCommonDenominator(int a, int b)
{
     if (a == 0) return b;
     if (b == 0) return a;

     if (a > b)
        return GreatestCommonDenominator(a % b, b);
     else
        return GreatestCommonDenominator(a, b % a);
}
