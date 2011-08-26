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

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;

namespace Kinovea { namespace Video
{
		// OBSOLETE. Use VideoInfo from Kinovea::Video namespace.
		public ref class InfosVideo
		{
			// Info structure for a video.
			// (Could be a value struct ?)
			public :
				
				int64_t	iFileSize;
				int		iWidth;
				int		iHeight;
				
				double	fPixelAspectRatio;
				int		iSampleAspectRatioNumerator;
				int		iSampleAspectRatioDenominator;
				bool	bIsCodecMpeg2;					// Used to adapt pixel ratio on output.
				AspectRatio eAspectRatio;				// Image format the user forces (auto, 4:3, 16:9).

				double  fFps;
				bool    bFpsIsReliable;					
				double	fFrameInterval;					// in Milliseconds.
				int64_t iDurationTimeStamps;			// Duration between first and last. Not between 0 and last.
				int64_t iFirstTimeStamp;				// The first frame timestamp. (not always 0) 
				double  fAverageTimeStampsPerSeconds;
				int64_t iAverageTimeStampsPerFrame;
				
				int		iDecodingWidth;
				int		iDecodingHeight;
				double	fDecodingStretchFactor;			// Used to set the output size of image.
				int		iDecodingFlag;					// Quality of scaling during format conversion.
				bool	bDeinterlaced;					// If frames should be deinterlaced, this is the setting as set by the user.
		};
		
		// Other helper classes.
		// TODO: merge into VideoFile or extract to a special .h

		// Should be a value struct !
		public ref class DefaultSettings
		{
			public:
				AspectRatio eAspectRatio;				// Image format the user forces (auto, 4:3, 16:9).
				bool		bDeinterlace;				// If frames should be deinterlaced.
		};
		
		// Replace with Section (?)
		public ref class PrimarySelection
		{
			// Structure d'information sur la sélection primaire.

			public :
				
				int		iAnalysisMode;		// 0 : Selection, 1 : Analysis.
				bool	bFiltered;

				// Position
				int64_t iCurrentTimeStamp;	// Absolu									(Pour mode Selection)
				int		iCurrentFrame;		// Relatif, entre 0 et iDurationFrame - 1	(Pour mode Analyse)
				int64_t iBufferedPTS;		// timestamp of a frame that was read but not decoded by libav.
				int64_t iLastDecodedPTS;	// timestamp of the last decoded frame.

				// Selection
				int		iDurationFrame;		// (Pour mode Analyse).
		};

		// Replace with VideoFrame.
		public ref class DecompressedFrame
		{
			public:
				int64_t iTimeStamp;
				Bitmap^	BmpImage;
				IntPtr	Hbmp;
		};
	}
}