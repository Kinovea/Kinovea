using Kinovea.VideoService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// MinIO存储服务实现
    /// </summary>
    public class MinioService : IMinioService, IDisposable
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioService> _logger;
        private readonly string _tempDirectory;

        public MinioService(IConfiguration configuration, ILogger<MinioService> logger)
        {
            _logger = logger;

            // 从配置中获取MinIO连接信息
            var endpoint = configuration["Minio:Endpoint"];
            var accessKey = configuration["Minio:AccessKey"];
            var secretKey = configuration["Minio:SecretKey"];
            var secure = bool.Parse(configuration["Minio:Secure"] ?? "false");
            var port = configuration.GetValue<int>("Minio:Port", 9000);

            // 初始化MinIO客户端
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint, port)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(secure)
                .Build();

            // 从配置中获取临时目录基路径（可配置）
            string baseTempPath = configuration["Minio:TempPath"]
                ?? Path.GetTempPath();

            // 添加应用专属子目录
            _tempDirectory = Path.Combine(
                baseTempPath,
                "Kinovea",
                "MinioTemp",
                Process.GetCurrentProcess().Id.ToString() // 添加进程ID避免冲突
            );

            try
            {
                // 确保目录存在并有权限
                if (Directory.Exists(_tempDirectory))
                {
                    // 清理可能存在的旧临时文件
                    CleanupOldTempFiles();
                }
                else
                {
                    Directory.CreateDirectory(_tempDirectory);
                }
                // 验证权限
                string testFile = Path.Combine(_tempDirectory, "test.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                _logger.LogInformation($"临时目录初始化成功: {_tempDirectory}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"临时目录初始化失败: {_tempDirectory}");
                throw new InvalidOperationException("无法初始化临时目录，请检查权限和路径配置", ex);
            }
        }


        /// <summary>
        /// 从MinIO下载文件到本地临时目录
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>本地临时文件路径</returns>
        public async Task<string> DownloadFileToTempAsync(string bucketName, string objectName)
        {
            string tempFilePath = null;
            try
            {
                // 生成唯一的临时文件名
                var fileExtension = Path.GetExtension(objectName);
                var tempFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{fileExtension}";
                tempFilePath = Path.Combine(_tempDirectory, tempFileName);

                // 下载文件
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFile(tempFilePath));

                _logger.LogDebug($"创建临时文件: {tempFilePath}");

                return tempFilePath;
            }
            catch (Exception ex)
            {
                // 如果下载失败，清理可能部分下载的文件
                if (tempFilePath != null && File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, $"清理失败的临时文件时出错: {tempFilePath}");
                    }
                }

                _logger.LogError(ex, $"从MinIO下载文件失败: {bucketName}/{objectName}");
                throw;
            }
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>文件流</returns>
        public async Task<Stream> GetObjectAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                await _minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream =>
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                    }));

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"从MinIO获取文件流失败: {bucketName}/{objectName}");
                throw;
            }
        }

        /// <summary>
        /// 检查对象是否存在
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="objectName">对象名称（文件路径）</param>
        /// <returns>是否存在</returns>
        public async Task<bool> ObjectExistsAsync(string bucketName, string objectName)
        {
            try
            {
                // 尝试获取对象的元数据，如果不存在会抛出异常
                await _minioClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName));

                return true;
            }
            catch (Minio.Exceptions.ObjectNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"检查MinIO对象是否存在时出错: {bucketName}/{objectName}");
                throw;
            }
        }

        /// <summary>
        /// 清理临时文件
        /// </summary>
        /// <param name="filePath">临时文件路径</param>
        public void CleanupTempFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"清理临时文件失败: {filePath}");
                // 不抛出异常，只记录日志
            }
        }

        /// <summary>
        /// 上传文件到MinIO
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> UploadFileAsync(string bucketName, string objectName, Stream fileStream)
        {
            // TODO: 实现上传文件到MinIO的逻辑
            // 生成文件名，实际情况为用户名+时间戳+guid+文件名+文件后缀
            // 暂时为了简化，使用固定的用户名
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{Path.GetExtension(objectName)}";
            try
            {
                // 上传到MinIO
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                                  .WithBucket(bucketName)
                                  .WithObject(objectName)
                                  .WithStreamData(fileStream)
                                  .WithObjectSize(fileStream.Length)
                                  .WithContentType("application/octet-stream"));
                
                _logger.LogInformation($"上传文件到MinIO成功: {bucketName}/{objectName}");

                return objectName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"上传文件到MinIO失败: {bucketName}/{objectName}");
                throw;
            }
        }


        public void Dispose()
        {
            try
            {
                // 清理临时目录
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                    _logger.LogInformation($"已清理临时目录: {_tempDirectory}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"清理临时目录失败: {_tempDirectory}");
            }
        }

        #region private methods

        /// <summary>
        /// 清理超过24小时的临时文件
        /// </summary>
        private void CleanupOldTempFiles()
        {
            try
            {
                // 清理超过24小时的临时文件
                var oldFiles = Directory.GetFiles(_tempDirectory)
                    .Select(f => new FileInfo(f))
                    .Where(f => (DateTime.Now - f.CreationTime).TotalHours > 24);

                foreach (var file in oldFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation($"已清理旧临时文件: {file.FullName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"清理旧临时文件失败: {file.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理旧临时文件时出错");
            }
        }


        #endregion
    }
}
