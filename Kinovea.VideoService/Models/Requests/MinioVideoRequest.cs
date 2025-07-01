namespace Kinovea.VideoService.Models.Requests
{
    /// <summary>
    /// MinIO视频请求模型
    /// </summary>
    public class MinioVideoRequest
    {
        /// <summary>
        /// MinIO存储桶名称
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// MinIO对象名称（文件路径）
        /// </summary>
        public string ObjectName { get; set; }
    }
}
