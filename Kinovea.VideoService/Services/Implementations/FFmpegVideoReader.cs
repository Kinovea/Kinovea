using Kinovea.Video;
using Kinovea.VideoService.Models;
using Kinovea.VideoService.Services.Interfaces;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// FFmpeg视频读取器实现
    /// </summary>
    public class FFmpegVideoReader : IVideoReader
    {
        private readonly ILogger<FFmpegVideoReader> _logger;
        private readonly string _filePath;
        private IMediaAnalysis _mediaInfo;
        private bool _isInitialized;

        public VideoCapabilities Capabilities => VideoCapabilities.CanDecodeOnDemand |
                                               VideoCapabilities.CanChangeFrameRate |
                                               VideoCapabilities.CanChangeDecodingSize;

        public FFmpegVideoReader(ILogger<FFmpegVideoReader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 打开视频文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<OpenVideoResult> OpenAsync(string filePath)
        {
            try
            {
                _mediaInfo = await FFProbe.AnalyseAsync(filePath);
                _isInitialized = true;
                return OpenVideoResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打开视频文件失败");
                return OpenVideoResult.UnknownError;
            }
        }

        public async Task<Models.VideoFrame> GetFrameAsync(TimeSpan position)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("视频读取器未初始化");

            try
            {
                var outputPath = Path.Combine(Path.GetTempPath(), $"frame_{Guid.NewGuid()}.jpg");
                await FFMpeg.SnapshotAsync(_filePath, outputPath, position);

                var frameData = await File.ReadAllBytesAsync(outputPath);
                File.Delete(outputPath);

                return new Kinovea.VideoService.Models.VideoFrame
                {
                    Data = frameData,
                    Timestamp = position,
                    Format = "image/jpeg"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频帧失败");
                throw;
            }
        }

        public async Task<VideoMetadata> GetMetadataAsync()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("视频读取器未初始化");

            return new VideoMetadata
            {
                Duration = _mediaInfo.Duration,
                FrameRate = _mediaInfo.VideoStreams.FirstOrDefault()?.FrameRate ?? 0,
                Width = _mediaInfo.VideoStreams.FirstOrDefault()?.Width ?? 0,
                Height = _mediaInfo.VideoStreams.FirstOrDefault()?.Height ?? 0,
                Codec = _mediaInfo.VideoStreams.FirstOrDefault()?.CodecName,
                Container = _mediaInfo.Format.FormatName
            };
        }

        public Task CloseAsync()
        {
            _isInitialized = false;
            _mediaInfo = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            CloseAsync().Wait();
        }
    }
}
