#pragma region License
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
#pragma endregion

//-----------------------------------------------------------------------------
// VideoFileWriter - a class to write a set of frames to a file.
//
// call OpenSavingContext() -> SaveFrame()* -> CloseSavingContext().
// Typically SaveFrame is called as frames become available,
// either from capture device, or from reading module.
//-----------------------------------------------------------------------------

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

namespace Kinovea { namespace Video { namespace FFMpeg
{
    public ref class VideoFileWriter
    {
    // Construction/Destruction
    public:
        VideoFileWriter();
        ~VideoFileWriter();
    protected:
        !VideoFileWriter();

    // Public Methods
    public:
        SaveResult Save(SavingSettings _settings,  VideoInfo _info, String^ _formatString, IEnumerable<Bitmap^>^ _frames, BackgroundWorker^ _worker);
        SaveResult OpenSavingContext(String^ _FilePath, VideoInfo _info, String^ _formatString, double _fFramesInterval);
        SaveResult CloseSavingContext(bool _bEncodingSuccess);
        SaveResult SaveFrame(Bitmap^ _image);
    
    // Private Methods
    private:
        double ComputeBitrate(Size outputSize, double frameInterval);
        bool SetupMuxer(SavingContext^ _SavingContext);
        bool SetupEncoder(SavingContext^ _SavingContext);
        
        bool EncodeAndWriteVideoFrame(SavingContext^ _SavingContext, Bitmap^ _InputBitmap);
        bool WriteFrame(int _iEncodedSize, SavingContext^ _SavingContext, uint8_t* _pOutputVideoBuffer, bool _bForceKeyframe);
        void SanityCheck(AVFormatContext* s);
        void LogError(String^ context, int ffmpegError);
        static int GreatestCommonDenominator(int a, int b);
    
    // Members
    private :
        static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);
        SavingContext^ m_SavingContext;
    };
}}}
