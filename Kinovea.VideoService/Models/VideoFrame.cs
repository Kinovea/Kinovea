using System.Drawing;

namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// 视频帧模型
    /// </summary>
    public class VideoFrame
    {
        /// <summary>
        /// 帧图像
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// 时间戳(毫秒)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 帧索引
        /// </summary>
        public int FrameIndex { get; set; }

        /// <summary>
        /// 是否为关键帧
        /// </summary>
        public bool IsKeyFrame { get; set; }
    }
}