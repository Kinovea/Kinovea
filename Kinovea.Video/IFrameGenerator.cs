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
    /// VideoReaderGenerator helper objects.
    /// 视频读取器生成器辅助对象
    /// Must be able to generate a frame for any arbitrary timestamp asked.
    /// 必须能够为任何指定的时间戳生成一帧。
    /// The frame generator owns the memory of the generated images.
    /// 帧生成器拥有生成图像的内存。
    /// </summary>
    public interface IFrameGenerator
    {
        /// <summary>
        /// Size of images as stored in the file.
        /// 文件中所存储图像的大小。
        /// </summary>
        Size OriginalSize { get; }

        /// <summary>
        /// Size of images after aspect ratio fix and rotation.
        /// 调整比例和旋转后的图像尺寸。
        /// </summary>
        Size ReferenceSize { get; }

        /// <summary>
        /// Orientation of images.
        /// 图像的方向。
        /// </summary>
        ImageRotation ImageRotation { get; }

        /// <summary>
        /// Open the file.
        /// 打开文件
        /// </summary>
        OpenVideoResult Open(string filename);

        /// <summary>
        /// Close the file and clean resources.
        /// 关闭文件并清理资源。
        /// </summary>
        void Close();

        /// <summary>
        /// Change the orientation of the output image. 
        /// 改变输出图像的方向。
        /// </summary>
        void SetRotation(ImageRotation rotation);

        /// <summary>
        /// Produce the image at the passed timestamp.
        /// 根据传入的时间戳生成图像。
        /// </summary>
        Bitmap Generate(long timestamp);

        /// <summary>
        /// Produce the image at the passed timestamp and size.
        /// 根据传入的时间戳和尺寸生成图像。
        /// </summary>
        Bitmap Generate(long timestamp, Size maxSize);

        /// <summary>
        /// Dispose the last generated image.
        /// 处理最后生成的图像。
        /// </summary>
        void DisposePrevious(Bitmap previous);
    }
}
