namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// 视频元数据
    /// </summary>
    public class VideoMetadata
    {
        /// <summary>
        /// 视频编码格式
        /// </summary>
        public string Codec { get; set; }

        /// <summary>
        /// 视频帧率
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 视频时长(毫秒)
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// 视频宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 视频高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 视频总帧数
        /// </summary>
        public long FrameCount { get; set; }

        /// <summary>
        /// 比特率(bps)
        /// </summary>
        public int BitRate { get; set; }
    }
}