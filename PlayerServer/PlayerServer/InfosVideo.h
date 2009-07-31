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

namespace Kinovea
{
	namespace VideoFiles
	{
		public ref class InfosVideo
		{
			// Info structure for a video.
			// (Could be a value struct ?)
			public :
				
				int64_t	iFileSize;
				int		iWidth;
				int		iHeight;
				double	fPixelAspectRatio;
				double  fFps;
				bool    bFpsIsReliable;					
				int		iFrameInterval;					// in Milliseconds.
				int64_t iDurationTimeStamps;			// Duration between first and last. Not between 0 and last.
				int64_t iFirstTimeStamp;				// The first frame timestamp. (not always 0) 
				double  fAverageTimeStampsPerSeconds;
				int64_t iAverageTimeStampsPerFrame;
				
				int		iDecodingWidth;
				int		iDecodingHeight;
				double	fDecodingStretchFactor;			// Used to set the output size of image.
				int		iDecodingFlag;					// Quality of scaling during format conversion.
				bool	bDeinterlaced;					// If frames should be deinterlaced.
		};
		
		// Other helper classes.
		// TODO: merge into VideoFile or extract to a special .h
	
		public ref class InfosThumbnail
		{
			public:
				List <Bitmap^>^ Thumbnails;
				int64_t iDurationMilliseconds;
		};
		public ref class PrimarySelection
		{
			// Structure d'information sur la sélection primaire.

			public :
				
				int		iAnalysisMode;		// 0 : Selection, 1 : Analysis.
				bool	bFiltered;

				// Position
				int64_t iCurrentTimeStamp;	// Absolu									(Pour mode Selection)
				int		iCurrentFrame;		// Relatif, entre 0 et iDurationFrame - 1	(Pour mode Analyse)

				// Selection
				int		iDurationFrame;		// (Pour mode Analyse).
		};
		public ref class DecompressedFrame
		{
			public:
				int64_t iTimeStamp;
				Bitmap^	BmpImage;
		};
	}
}