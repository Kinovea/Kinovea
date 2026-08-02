#pragma region License
/*
Copyright © Joan Charmant 2014.
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

#pragma once

extern "C" 
{
#ifndef __STDC_CONSTANT_MACROS
#define __STDC_CONSTANT_MACROS
#endif
#ifndef __STDC_LIMIT_MACROS
#define __STDC_LIMIT_MACROS
#endif
#include <avcodec.h>
#include <avformat.h>
#include <error.h>
#include <frame.h>
#include <imgutils.h>
#include <swscale.h> 
}

#include "SavingContext.h"

using namespace System;
using namespace System::Collections::Generic;				
using namespace System::ComponentModel;
using namespace System::Diagnostics;
using namespace System::Drawing;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Text;
using namespace System::Threading;
using namespace System::Windows::Forms;
using namespace Kinovea::Video;
using namespace Kinovea::Services;

namespace Kinovea { namespace Video { namespace FFMpeg
{
    public ref class MJPEGWriter
    {
    public:
        /// Constructor.
        MJPEGWriter();

        /// Destructor.
        ~MJPEGWriter();
        
        /// Finalizer.
        !MJPEGWriter();

        /// Create the saving context that stores global parameters we use throughout saving.
        /// Configure the muxer, stream and codec.
        /// Configure the scaling context for conversion from source to target pixel format.
        /// Write the file header.
        SaveResult OpenSavingContext(String^ _FilePath, VideoInfo _info, String^ _formatString, Kinovea::Services::ImageFormat _imageFormat, bool _uncompressed, double _fFramesInterval, double _fFileFramesInterval, ImageRotation rotation);
        
        /// Close the saving context and free any allocated resources.
        SaveResult CloseSavingContext(bool _bEncodingSuccess);
                       
        /// Encode and write one frame to the file.
        SaveResult SaveFrame(Kinovea::Services::ImageFormat format, array<System::Byte>^ buffer, Int64 length, bool topDown);

    private:
        static int GreatestCommonDenominator(int a, int b);

        double ComputeBitrate(Size outputSize, double frameInterval);
        
        /// Configure the codec with default parameters.
        bool SetupEncoder(SavingContext^ _SavingContext, Kinovea::Services::ImageFormat _imageFormat);

        /// Wrap the incoming buffer in a packet and writes it to the file.
        /// Used when the incoming buffer is already in the target format.
        bool WrapAndWrite(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length);
        
        /// Use the scaling context set up during `OpenSavingContext` to convert from 
        /// the source format to YUV, encode to JPEG and then write the packet out.
        bool EncodeAndWrite(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length, bool topDown);
        
        /// Write the encoded packet to the file.
        bool WritePacket(SavingContext^ savingCtx);

        void SanityCheck(AVFormatContext* s);
        
        // Logging utils.
        void LogFFMpegError(String^ context, int ffmpegError);
        void LogStats();

    // Members
    private :
        SavingContext^ m_SavingContext;
        Stopwatch^ m_swEncoding;
        Stopwatch^ m_swWrite;
        Int64 m_encodingDurationAccumulator;
        Int64 m_writeDurationAccumulator;
        static const double megabyte = 1024 * 1024;
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);
    };
}}}
