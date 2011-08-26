#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.Drawing;

namespace Kinovea.Video
{
 
    // TODO: hide those items that are implementation details.
    
    public struct VideoInfo
    {
        // Structural info
        public string FilePath;
        public Size DecodingSize;
        
        // Timing info
        public long AverageTimeStampsPerFrame;
        public double AverageTimeStampsPerSeconds;
        public double FramesPerSeconds;
        public double FrameIntervalMilliseconds;
        
        public long FirstTimeStamp;
        public long LastTimeStamp;
        public long DurationTimeStamps;
        
        // Saving support.
        public Size OriginalSize;
        public double PixelAspectRatio;
        public Fraction SampleAspectRatio;
        public bool IsCodecMpeg2;
        
        public static VideoInfo Empty {
            get { 
                return new VideoInfo {
                    FilePath = "",
                    DecodingSize = Size.Empty,
                    AverageTimeStampsPerFrame = 0,
                    AverageTimeStampsPerSeconds = 0,
                    FramesPerSeconds = 0,
                    FrameIntervalMilliseconds = 0,
                    FirstTimeStamp = 0,
                    LastTimeStamp = 0,
                    DurationTimeStamps = 0,
                    OriginalSize = Size.Empty,
                    PixelAspectRatio = 1.0F,
                    SampleAspectRatio = new Fraction(),
                    IsCodecMpeg2 = false
                };
            }
        }
        
        /*
         * m_InfosVideo->iFileSize = 0;
	m_InfosVideo->iWidth = 320;
	m_InfosVideo->iHeight = 240;
	m_InfosVideo->fPixelAspectRatio = 1.0f;
	m_InfosVideo->fFps = 1.0f;
	m_InfosVideo->bFpsIsReliable = false;
	m_InfosVideo->fFrameInterval = 40;
	m_InfosVideo->iDurationTimeStamps = 1;
	m_InfosVideo->iFirstTimeStamp = 0;
	m_InfosVideo->fAverageTimeStampsPerSeconds = 1.0f;
	
	// Read / Write
	m_InfosVideo->iDecodingWidth = 320;
	m_InfosVideo->iDecodingHeight = 240;
	m_InfosVideo->fDecodingStretchFactor = 1.0f;
	m_InfosVideo->iDecodingFlag = SWS_FAST_BILINEAR;
	m_InfosVideo->bDeinterlaced = false;
	*/
        /*public long	FileSize;
        
        public Size OutputSize;
           
        public double DecodingStretchFactor;			// Used to set the output size of image.
        
              
        public double PixelAspectRatio;
        
                              // Should be a fraction ?
        public bool FpsIsReliable;					
        
        
        				// The first frame timestamp. (not always 0)
        
        
        
        */
        
        
        
        
        
        // Old fields:
        //public ImageAspectRatio ImageAspectRatio;
        //public int SampleAspectRatioNumerator;
        //public int SampleAspectRatioDenominator;
        //bool	bIsCodecMpeg2;					// Used to adapt pixel ratio on output.
        
        //int		iDecodingFlag;					// Quality of scaling during format conversion.
        //bool	bDeinterlaced;					// If frames should be deinterlaced, this is the setting as set by the user.
    }
}
