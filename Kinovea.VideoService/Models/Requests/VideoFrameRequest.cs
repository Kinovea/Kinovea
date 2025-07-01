namespace Kinovea.VideoService.Models.Requests
{
    public class VideoFrameRequest
    {
        public required string BucketName { get; set; }
        public required string ObjectName { get; set; }
        public long CurrentFrame { get; set; }
        public long TargetFrame { get; set; }
    }
}
