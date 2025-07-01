using Kinovea.Video;

namespace Kinovea.VideoService.Models
{
    public class VideoSession
    {
        public required VideoReader Reader { get; set; }
        public string? TempFilePath { get; set; }
        public DateTime LastAccessed { get; set; }
    }
}
