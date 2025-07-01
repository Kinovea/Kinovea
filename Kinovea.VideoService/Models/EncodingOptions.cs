using System.Drawing;

namespace Kinovea.VideoService.Models
{
    /// <summary>
    /// 编码选项
    /// </summary>
    public class EncodingOptions
    {
        public string Codec { get; set; } = "libx264"; // 默认编码器
        public int Bitrate { get; set; } = 1000; // 比特率 (kbps)
        public int Framerate { get; set; } = 30; // 帧率
        public string Format { get; set; } = "mp4"; // 输出格式
        public Size Resolution { get; set; } = new Size(1920, 1080); // 分辨率
    }
}
