using Kinovea.Video;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频处理选项接口
    /// </summary>
    public interface IVideoProcessingOptions
    {
        /// <summary>
        /// 设置视频处理选项：如设置图像宽高比、旋转、去马赛克、去隔行等。
        /// </summary>
        /// <param name="reader">视频读取器</param>
        /// <param name="options">视频选项</param>
        void SetVideoOptions(VideoReader reader, VideoOptions options);
    }
}
