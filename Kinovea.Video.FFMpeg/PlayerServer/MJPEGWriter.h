#pragma region License
/*
Copyright ?Joan Charmant 2014.
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
#define __STDC_CONSTANT_MACROS
#define __STDC_LIMIT_MACROS
#include <avformat.h>
#include <avcodec.h>
#include <avstring.h>
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
    // Construction/Destruction
    public:
        MJPEGWriter();
        ~MJPEGWriter();
    protected:
        !MJPEGWriter();

    // Public Methods
    public:
        SaveResult OpenSavingContext(String^ _FilePath, VideoInfo _info, String^ _formatString, Kinovea::Services::ImageFormat _imageFormat, bool _uncompressed, double _fFramesInterval, double _fFileFramesInterval, ImageRotation rotation);
        SaveResult CloseSavingContext(bool _bEncodingSuccess);
        SaveResult SaveFrame(Kinovea::Services::ImageFormat format, array<System::Byte>^ buffer, Int64 length, bool topDown);

    // Private Methods
    private:
        double ComputeBitrate(Size outputSize, double frameInterval);
        bool SetupMuxer(SavingContext^ _SavingContext);
        bool SetupEncoder(SavingContext^ _SavingContext, Kinovea::Services::ImageFormat _imageFormat);
        
        bool EncodeAndWriteVideoFrameRGB32(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length, bool topDown);
        bool EncodeAndWriteVideoFrameRGB24(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length, bool topDown);
        bool EncodeAndWriteVideoFrameY800(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length, bool topDown);
        bool EncodeAndWriteVideoFrameJPEG(SavingContext^ _SavingContext, array<System::Byte>^ managedBuffer, Int64 length);

        bool WriteBuffer(int _iEncodedSize, SavingContext^ _SavingContext, uint8_t* _pOutputVideoBuffer, bool _bForceKeyframe);
        void SanityCheck(AVFormatContext* s);
        void LogError(String^ context, int ffmpegError);
        void LogStats();
        static int GreatestCommonDenominator(int a, int b);

    // Members
    private :
        SavingContext^ m_SavingContext;
        Stopwatch^ m_swEncoding;
        Stopwatch^ m_swWrite;
        int m_frame;
        Int64 m_encodingDurationAccumulator;
        Int64 m_writeDurationAccumulator;
        static const double megabyte = 1024 * 1024;
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);
    };
}}}
