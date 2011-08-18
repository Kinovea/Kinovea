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
        
        /*public long	FileSize;
        
        public Size OutputSize;
           
        public double DecodingStretchFactor;			// Used to set the output size of image.
        
        public Fraction SampleAspectRatio;      
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
