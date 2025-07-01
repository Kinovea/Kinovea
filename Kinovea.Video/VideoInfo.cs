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
    //将那些属于实现细节的项目隐藏起来。
    /// <summary>
    /// Information about a video file.
    /// 有关视频文件的信息。
    /// </summary>

    public struct VideoInfo
    {
        /// <summary>
        /// Full path of the video file.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Whether a separate KVA metadata file has been loaded for this video.
        /// 是否已为该视频加载了单独的 KVA 元数据文件。
        /// </summary>
        public bool HasKva;

        /// <summary>
        /// Image size in the file.
        /// </summary>
        public Size OriginalSize;

        /// <summary>
        /// Image size after aspect ratio fix (either from pixel aspect ratio or by user configuration).
        /// 经过比例调整后的图像尺寸（无论是基于像素比例还是根据用户设置确定的）。
        /// Padded to 4 bytes along rotated width.
        /// 沿旋转后的宽度扩展至 4 个字节。
        /// </summary>
        public Size AspectRatioSize;

        /// <summary>
        /// Image size after aspect ratio fix and rotation.
        /// 经过比例调整和旋转后的图像尺寸。
        /// This is the unscaled image size, the images might be decoded at a smaller size still.
        /// 这是未缩放的图像尺寸，这些图像可能还能进一步被缩小处理以获得更小的尺寸。
        /// </summary>
        public Size ReferenceSize;
        public double PixelAspectRatio;
        public Fraction SampleAspectRatio;
        public bool IsCodecMpeg2;

        /// <summary>
        /// Image rotation to use for decoding images.
        /// 用于解码图像的图像旋转方式。
        /// Either from video internal metadata or user configuration.
        /// 要么来自视频内部的元数据，要么来自用户设置。
        /// </summary>
        public ImageRotation ImageRotation;

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
