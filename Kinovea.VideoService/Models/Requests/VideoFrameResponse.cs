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
        /// <summary>
        /// 帧位置（字节偏移）
        /// </summary>
        public long Position { get; set; }
        /// <summary>
        /// 帧时间戳
        /// </summary>
        public long Timestamp { get; set; }
    }

    public class VideoSessionRequest
    {
        /// <summary>
        /// 视频文件的存储桶名称
        /// </summary>
        public required string BucketName { get; set; }
        /// <summary>
        /// 视频文件的对象名称
        /// </summary>
        public required string ObjectName { get; set; }
    }
}
