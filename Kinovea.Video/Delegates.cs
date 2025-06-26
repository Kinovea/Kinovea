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

namespace Kinovea.Video
{
    /// <summary>
    /// 用于处理视频帧的委托
    /// </summary>
    /// <param name="frame"></param>
    public delegate void VideoFrameDisposer(VideoFrame frame);
    /// <summary>
    /// 用于从视频帧中检索图像的委托
    /// </summary>
    /// <param name="frame">视频帧</param>
    /// <param name="output">输出的位图</param>
    /// <returns></returns>
    public delegate bool ImageRetriever(VideoFrame frame, Bitmap output);
}
