namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// MinIO存储服务接口
    /// </summary>
    public interface IMinioService
    {
        /// <summary>
        /// 从MinIO下载文件到本地临时目录
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>本地临时文件路径</returns>
        Task<string> DownloadFileToTempAsync(string bucketName, string objectName);

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>文件流</returns>
        Task<Stream> GetObjectAsync(string bucketName, string objectName);

        /// <summary>
        /// 检查对象是否存在
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>是否存在</returns>
        Task<bool> ObjectExistsAsync(string bucketName, string objectName);

        /// <summary>
        /// 清理临时文件
        /// </summary>
        /// <param name="filePath">临时文件路径</param>
        void CleanupTempFile(string filePath);


        /// <summary>
        /// 上传文件到MinIO
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream);
    }
}
