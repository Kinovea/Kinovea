namespace Kinovea.VideoService.Models.Requests
{
    public class MinioVideoUploadRequest
    {
        /// <summary>
        /// MinIO存储桶名称
        /// </summary>
        public required string BucketName { get; set; }

        /// <summary>
        /// MinIO对象名称（文件路径）
        /// </summary>
        public required string ObjectName { get; set; }

        /// <summary>
        /// 对象版本（可选）
        /// </summary>
        public string? ObjectVersion { get; set; }

        /// <summary>
        /// 元数据（JSON格式，可选）
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// 媒体类型（可选）
        /// </summary>
        public string? MediaType { get; set; }

        /// <summary>
        /// 视频类型，如 "mp4", "avi" 等（可选）
        /// </summary>
        public string? VideoType { get; set; }

        /// <summary>
        /// 上传的文件对象
        /// </summary>
        public required IFormFile File { get; set; }
    }
}
