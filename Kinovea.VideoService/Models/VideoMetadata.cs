namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// 视频元数据
    /// </summary>
    public class VideoMetadata
    {
        /// <summary>
        /// 视频时长
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 帧率
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 视频宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 视频高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 视频编码格式
        /// </summary>
        public string Codec { get; set; }

        /// <summary>
        /// 视频容器格式
        /// </summary>
        public string Container { get; set; }
    }
}