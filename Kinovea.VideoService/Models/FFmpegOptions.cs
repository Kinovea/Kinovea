namespace Kinovea.VideoService.Models
{
    public class FFmpegOptions
    {
        public string FFmpegPath { get; set; } = string.Empty;
        public string TempPath { get; set; } = string.Empty;
        public bool EnableHardwareAcceleration { get; set; }
    }
}
