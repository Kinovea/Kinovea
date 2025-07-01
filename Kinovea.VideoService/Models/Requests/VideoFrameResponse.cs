using System.Drawing;

namespace Kinovea.VideoService.Models.Requests
{
    public class VideoFrameResponse
    {
        /// <summary>
        /// 帧图像
        /// </summary>
        public required Bitmap Image { get; set; }

        /// <summary>
        /// 帧时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 帧索引（可选，若有）
        /// </summary>
        public int? FrameIndex { get; set; }

        /// <summary>
        /// 是否为关键帧（可选，若有）
        /// </summary>
        public bool? IsKeyFrame { get; set; }
    }
    public class VideoFrameInfo
    {
        public long Position { get; set; }
        public long Timestamp { get; set; }
    }

    public class VideoSessionRequest
    {
        public required string BucketName { get; set; }
        public required string ObjectName { get; set; }
    }
}
