using Kinovea.Video.FFMpeg;

namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频元数据管理接口
    /// </summary>
    public interface IVideoMetadataManagement
    {
        /// <summary>
        /// 读取视频元数据：获取视频的相关元数据信息。
        /// </summary>
        /// <param name="reader">视频读取器</param>
        /// <returns>元数据信息字符串</returns>
        string ReadVideoMetadata(VideoReaderFFMpeg reader);
    }
}
