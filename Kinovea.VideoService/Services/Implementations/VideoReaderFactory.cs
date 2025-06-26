using Kinovea.VideoService.Services.Interfaces;

namespace Kinovea.VideoService.Services.Implementations
{
    /// <summary>
    /// 视频读取器工厂实现
    /// </summary>
    public class VideoReaderFactory : IVideoReaderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _readerTypes;

        public VideoReaderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _readerTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { ".mp4", typeof(FFmpegVideoReader) },
                { ".webm", typeof(FFmpegVideoReader) },
                { ".mov", typeof(FFmpegVideoReader) },
                // 可以添加更多格式支持
            };
        }

        public IVideoReader CreateReader(string format)
        {
            var extension = Path.GetExtension(format);
            if (!_readerTypes.TryGetValue(extension, out var readerType))
            {
                throw new NotSupportedException($"不支持的视频格式：{format}");
            }

            return (IVideoReader)ActivatorUtilities.CreateInstance(_serviceProvider, readerType);
        }

        public IEnumerable<string> GetSupportedFormats()
        {
            return _readerTypes.Keys;
        }
    }
}