using Kinovea.Video;
using System.Drawing;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频文件操作接口
    /// </summary>
    public interface IVideoFileOperations
    {
        /// <summary>
        /// 打开视频文件
        /// </summary>
        /// <param name="filePath">视频文件路径</param>
        /// <returns>视频打开结果</returns>
        OpenVideoResult Open(string filePath);

        /// <summary>
        /// 关闭视频
        /// </summary>
        void CloseVideo(VideoReader reader);

        /// <summary>
        /// 提取视频摘要：获取视频的基本信息和缩略图。
        /// </summary>
        /// <param name="filePath">视频文件路径</param>
        /// <param name="thumbsToLoad">需要加载的缩略图数量</param>
        /// <param name="maxImageSize">缩略图的最大尺寸</param>
        /// <returns>视频摘要对象</returns>
        VideoSummary ExtractVideoSummary(string filePath, int thumbsToLoad, Size maxImageSize);
    }
}
