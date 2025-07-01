using Kinovea.Video;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频类型管理器服务接口
    /// </summary>
    public interface IVideoTypeManagerService
    {
        /// <summary>
        /// 加载所有视频读取器
        /// </summary>
        void LoadVideoReaders();

        /// <summary>
        /// 根据文件路径获取适合的视频读取器
        /// </summary>
        /// <param name="filePath">视频文件路径</param>
        /// <returns>视频读取器实例</returns>
        VideoReader GetVideoReader(string filePath);

        /// <summary>
        /// 检查是否支持特定的文件扩展名
        /// </summary>
        /// <param name="extension">文件扩展名（包含.）</param>
        /// <returns>是否支持</returns>
        bool IsFormatSupported(string extension);

        /// <summary>
        /// 获取图像序列读取器
        /// </summary>
        /// <returns>图像序列读取器实例</returns>
        VideoReader GetImageSequenceReader();
    }
}
