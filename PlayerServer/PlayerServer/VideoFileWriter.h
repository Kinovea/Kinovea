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
// call OpenSavingContext() -> SaveFrame() -> CloseSavingContext().
// Typically SaveFrame is called as frames become available,
// either from capture device, or from reading module.
//-----------------------------------------------------------------------------

#pragma once

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

//------------------------
#define OUTPUT_MUXER_MKV 0
#define OUTPUT_MUXER_MP4 1
#define OUTPUT_MUXER_AVI 2

extern "C" 
{
#define __STDC_CONSTANT_MACROS
#define __STDC_LIMIT_MACROS
#include <avformat.h>
#include <avcodec.h>
#include <avstring.h>
#include <swscale.h> 
}

#include <stdio.h>
#include "SavingContext.h"
#include "VideoFile.h"    // <- remove. SaveResult should be declared here.

namespace Kinovea
{
namespace VideoFiles
{
	public ref class VideoFileWriter
	{

#pragma region Members
	private :
		static log4net::ILog^ log = log4net::LogManager::GetLogger(MethodBase::GetCurrentMethod()->DeclaringType);
		
		SavingContext^ m_SavingContext;
#pragma endregion

#pragma region Construction/Destruction
	public:
		VideoFileWriter();
		~VideoFileWriter();
	protected:
		!VideoFileWriter();
#pragma endregion

#pragma region Public Methods
	public:
		
		SaveResult OpenSavingContext(String^ _FilePath, InfosVideo^ _infosVideo, int _iFramesInterval, bool _bHasMetadata);
		SaveResult CloseSavingContext(bool _bEncodingSuccess);
		SaveResult SaveFrame(Bitmap^ _image);
		SaveResult SaveMetadata(String^ _Metadata);
		
#pragma endregion

#pragma region Private Methods
	private:
		static AVOutputFormat* GuessOutputFormat(String^ _FilePath, bool _bHasMetadata);
		bool	SetupMuxer(SavingContext^ _SavingContext);
		bool	SetupEncoder(SavingContext^ _SavingContext);
		
		bool    WriteMetadata(SavingContext^ _SavingContext, String^ _Metadata);
		bool	EncodeAndWriteVideoFrame(SavingContext^ _SavingContext, Bitmap^ _InputBitmap);
		bool	WriteFrame(int _iEncodedSize, SavingContext^ _SavingContext, uint8_t* _pOutputVideoBuffer, bool _bForceKeyframe);
		void	SanityCheck(AVFormatContext* s);
		static int GreatestCommonDenominator(int a, int b);

#pragma endregion
	};
}
}