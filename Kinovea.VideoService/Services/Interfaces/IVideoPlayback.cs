using Kinovea.Video;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频播放控制接口
    /// </summary>
    public interface IVideoPlayback
    {
        /// <summary>
        /// 按顺序播放视频的每一帧，移动到指定帧
        /// </summary>
        /// <param name="reader">视频</param>
        /// <param name="skip">跳过的帧数</param>
        /// <param name="decodeIfNecessary">是否解码</param>
        /// <returns>是否成功移动</returns>
        bool MoveToNextFrame(VideoReader reader, int skip, bool decodeIfNecessary);

        /// <summary>
        /// 跳转到指定帧：直接定位到视频的指定帧。
        /// </summary>
        /// <param name="reader">视频读取器</param>
        /// <param name="from">起始帧</param>
        /// <param name="target">目标帧</param>
        /// <returns>是否成功移动</returns>
        bool MoveToSpecificFrame(VideoReader reader, long from, long target);
    }
}
