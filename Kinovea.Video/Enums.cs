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

namespace Kinovea.Video
{
    /// <summary>
    /// Flags indicating the capabilities of the specific file loaded by the reader.
    /// 表示由读取器加载的特定文件功能的标志。
    /// </summary>
    [Flags]
    public enum VideoCapabilities : int
    {
        /// <summary>
        /// 没有特殊功能。
        /// </summary>
        None = 0,
        /// <summary>
        /// 视频可以按需解码。
        /// </summary>
        CanDecodeOnDemand = 1,
        /// <summary>
        /// 视频可以预缓冲。
        /// </summary>
        CanPreBuffer = 2,
        /// <summary>
        /// 视频可以缓存。
        /// </summary>
        CanCache = 4,
        /// <summary>
        /// 视频可以改变工作区域。
        /// </summary>
        CanChangeWorkingZone = 8,
        /// <summary>
        /// 视频可以改变宽高比。
        /// </summary>
        CanChangeAspectRatio = 16,
        /// <summary>
        /// 视频可以改变去交错模式。
        /// </summary>
        CanChangeDeinterlacing = 32,
        /// <summary>
        /// 视频可以改变视频持续时间。
        /// </summary>
        CanChangeVideoDuration = 64,
        /// <summary>
        /// 视频可以改变帧率。
        /// </summary>
        CanChangeFrameRate = 128,
        /// <summary>
        /// 视频可以改变解码大小。
        /// </summary>
        CanChangeDecodingSize = 256,
        /// <summary>
        /// 视频可以无限缩放。
        /// </summary>
        CanScaleIndefinitely = 512,
        /// <summary>
        /// 视频可以改变图像旋转。
        /// </summary>
        CanChangeImageRotation = 1024,
        /// <summary>
        /// 视频可以改变去马赛克模式。
        /// </summary>
        CanChangeDemosaicing = 2048,
        /// <summary>
        /// 视频可以稳定。
        /// </summary>
        CanStabilize = 4096,
    }

    /// <summary>
    /// 视频读取器当前所采用的解码模式。
    /// </summary>
    public enum VideoDecodingMode
    {
        /// <summary>
        /// 视频读取器未初始化。
        /// 该视频要么尚未开始播放，要么已经结束，而且播放器尚未完全初始化。
        /// </summary>
        NotInitialized, // The video is just opening or has closed and the reader is not fully initialized.
        /// <summary>
        /// 视频读取器正在解码。
        /// 每当玩家需要时，每一帧都会即时进行解码。
        /// </summary>
        OnDemand,       // each frame is decoded on the fly when the player needs it.
        /// <summary>
        /// 视频读取器正在预缓冲。
        /// 帧会在一个单独的线程中进行解码，并被推送到一个小型缓冲区中。
        /// </summary>
        PreBuffering,   // frames are decoded in a separate thread and pushed to a small buffer.
        /// <summary>
        /// 视频读取器正在缓存。
        /// 工作区域的所有框架都已被加载到一个大型缓冲区中。
        /// </summary>
        Caching         // All the frames of the working zone have been loaded to a large buffer.
    }

    /// <summary>
    /// 视频打开结果。
    /// </summary>
    public enum OpenVideoResult
    {
        /// <summary>
        /// 视频已成功打开。
        /// </summary>
        Success,
        /// <summary>
        /// 视频打开失败。
        /// </summary>
        UnknownError,
        /// <summary>
        /// 视频格式不受支持。
        /// </summary>
        NotSupported,
        /// <summary>
        /// 视频文件未打开。
        /// </summary>
        FileNotOpenned,
        /// <summary>
        /// 流信息未找到
        /// </summary>
        StreamInfoNotFound,
        /// <summary>
        /// 视频流未找到。
        /// </summary>
        VideoStreamNotFound,
        /// <summary>
        /// 音频流未找到。
        /// </summary>
        CodecNotFound,
        /// <summary>
        /// 编解码器参数未分配。
        /// </summary>
        CodecNotOpened,
        /// <summary>
        /// 编解码器参数未设置。
        /// </summary>
        CodecNotSupported,
        /// <summary>
        /// 视频流未创建。
        /// </summary>
        Cancelled,
        /// <summary>
        /// 视频流为空。
        /// </summary>
        EmptyWatcher,
    }
    /// <summary>
    /// 视频保存结果。
    /// </summary>
    public enum SaveResult
    {
        /// <summary>
        /// 视频已成功保存。
        /// </summary>
        Success,
        /// <summary>
        /// 视频保存失败。
        /// </summary>
        MuxerNotFound,
        /// <summary>
        /// 视频复用器参数未分配。
        /// </summary>
        MuxerParametersNotAllocated,
        /// <summary>
        /// 视频复用器参数未设置。
        /// </summary>
        MuxerParametersNotSet,
        /// <summary>
        /// 视频复用器未打开。
        /// </summary>
        VideoStreamNotCreated,
        /// <summary>
        /// 视频编码器未找到。
        /// </summary>
        EncoderNotFound,
        /// <summary>
        /// 视频编码器参数未分配。
        /// </summary>
        EncoderParametersNotAllocated,
        /// <summary>
        /// 编码器参数未设置
        /// </summary>
        EncoderParametersNotSet,
        /// <summary>
        /// 编码器未打开。
        /// </summary>
        EncoderNotOpened,
        /// <summary>
        /// 文件未打开。
        /// </summary>
        FileNotOpened,
        /// <summary>
        /// 文件头未写入。
        /// </summary>
        FileHeaderNotWritten,
        /// <summary>
        /// 输入帧未分配。
        /// </summary>
        InputFrameNotAllocated,
        /// <summary>
        /// 元数据流未创建。
        /// </summary>
        MetadataStreamNotCreated,
        /// <summary>
        /// 元数据未写入。
        /// </summary>
        MetadataNotWritten,
        /// <summary>
        /// 视频流未创建。
        /// </summary>
        ReadingError,
        /// <summary>
        /// 视频保存过程中发生未知错误。
        /// </summary>
        UnknownError,
        /// <summary>
        /// 视频保存已取消。
        /// </summary>
        MovieNotLoaded,
        /// <summary>
        /// 视频转码未完成。
        /// </summary>
        TranscodeNotFinished,
        /// <summary>
        /// 视频保存已取消。
        /// </summary>
        Cancelled
    }
}
