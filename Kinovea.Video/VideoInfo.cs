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
 
    // TODO: hide those items that are implementation details.
    
    public struct VideoInfo
    {
        /// <summary>
        /// Full path of the video file.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Whether a separate KVA metadata file has been loaded for this video.
        /// </summary>
        public bool HasKva;

        /// <summary>
        /// Image size in the file.
        /// </summary>
        public Size OriginalSize;

        /// <summary>
        /// Image size after aspect ratio fix (either from pixel aspect ratio or by user configuration).
        /// Padded to 4 bytes along rotated width.
        /// </summary>
        public Size AspectRatioSize;

        /// <summary>
        /// Image size after aspect ratio fix and rotation.
        /// This is the unscaled image size, the images might be decoded at a smaller size still.
        /// </summary>
        public Size ReferenceSize;

        public double PixelAspectRatio;
        public Fraction SampleAspectRatio;
        public bool IsCodecMpeg2;

        /// <summary>
        /// Image rotation to use for decoding images.
        /// Either from video internal metadata or user configuration.
        /// </summary>
        public ImageRotation ImageRotation;

        // Timing info - some of this might be overriden by the user.
        public double AverageTimeStampsPerFrame;
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
                    ImageRotation = ImageRotation.Rotate0,
       
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
