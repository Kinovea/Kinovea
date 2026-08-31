#region License
/*
Copyright © Joan Charmant 2011.
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
#endregion
using System;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.Video
{
    /// <summary>
    /// Information that is intrinsic to the file.
    /// </summary>
    public struct VideoInfo
    {
        /// <summary>
        /// Full path on the file system.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Raw image size without rotation or pixel aspect ratio.
        /// </summary>
        public Size OriginalSize;

        /// <summary>
        /// Image rotation flag stored in the file.
        /// </summary>
        public ImageRotation OriginalRotation;

        public double PixelAspectRatio;
        public Fraction SampleAspectRatio;
        public bool IsCodecMpeg2;

        public double AverageTimeStampsPerFrame;
        public double AverageTimeStampsPerSeconds;
        public double FramesPerSeconds;
        public double FrameIntervalMilliseconds;

        public long FirstTimeStamp;
        public long LastTimeStamp;
        public long DurationTimeStamps;

        public static VideoInfo MakeEmpty()
        {
            return new VideoInfo
            {
                FilePath = "",

                OriginalSize = Size.Empty,
                OriginalRotation = ImageRotation.Rotate0,
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
