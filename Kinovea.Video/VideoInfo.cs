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
        public bool HasKva;

        // Image size - Not included: decoding size.
        public Size OriginalSize;           // The image size in the file.
        public Size AspectRatioSize;        // The unscaled size after aspect ratio fix, either from pixel aspect ratio or forced by user. Padded to 4 bytes along rotated width.
        public Size ReferenceSize;          // The unscaled size after aspect ratio fix and rotation.
        public double PixelAspectRatio;
        public Fraction SampleAspectRatio;
        public bool IsCodecMpeg2;

        // Timing info - some of this might be overriden by the user.
        public long AverageTimeStampsPerFrame;
        public double AverageTimeStampsPerSeconds;
        public double FramesPerSeconds;
        public double FrameIntervalMilliseconds;
        
        public long FirstTimeStamp;
        public long LastTimeStamp;
        public long DurationTimeStamps;
        
        public static VideoInfo Empty {
            get { 
                return new VideoInfo {
                    FilePath = "",
                    HasKva = false,

                    OriginalSize = Size.Empty,
                    AspectRatioSize = Size.Empty,
                    ReferenceSize = Size.Empty,
                    PixelAspectRatio = 1.0F,
                    SampleAspectRatio = new Fraction(),
                    IsCodecMpeg2 = false,

                    AverageTimeStampsPerFrame = 0,
                    AverageTimeStampsPerSeconds = 0,
                    FramesPerSeconds = 0,
                    FrameIntervalMilliseconds = 0,
                    FirstTimeStamp = 0,
                    LastTimeStamp = 0,
                    DurationTimeStamps = 0
                };
            }
        }
    }
}
