using Kinovea.Video;
using Kinovea.VideoService.Models;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频读取器接口
    /// </summary>
    public interface IVideoReader : IDisposable
    {
        /// <summary>
        /// 视频处理能力
        /// </summary>
        VideoCapabilities Capabilities { get; }

        /// <summary>
        /// 打开视频文件
        /// </summary>
        Task<OpenVideoResult> OpenAsync(string filePath);

        /// <summary>
        /// 获取指定时间点的视频帧
        /// </summary>
        Task<Kinovea.VideoService.Models.VideoFrame> GetFrameAsync(TimeSpan position);

        /// <summary>
        /// 获取视频元数据
        /// </summary>
        Task<VideoMetadata> GetMetadataAsync();

        /// <summary>
        /// 关闭视频
        /// </summary>
        Task CloseAsync();
    }
}