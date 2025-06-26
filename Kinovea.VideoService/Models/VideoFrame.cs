namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// 视频帧模型
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        /// 帧索引
        /// </summary>
        public long Index { get; set; }

        /// <summary>
        /// 图像格式
        /// </summary>
        public string Format { get; set; }
    }
}