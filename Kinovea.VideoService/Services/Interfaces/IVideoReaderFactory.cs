namespace Kinovea.VideoService.Services.Interfaces
{
    /// <summary>
    /// 视频读取器工厂接口
    /// </summary>
    public interface IVideoReaderFactory
    {
        /// <summary>
        /// 创建适合指定视频格式的读取器
        /// </summary>
        /// <param name="format">视频格式</param>
        IVideoReader CreateReader(string format);

        /// <summary>
        /// 获取支持的视频格式列表
        /// </summary>
        IEnumerable<string> GetSupportedFormats();
    }
}