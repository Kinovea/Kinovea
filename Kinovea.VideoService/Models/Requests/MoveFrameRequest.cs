namespace Kinovea.VideoService.Models.Requests
{
    public class MoveFrameRequest
    {
        /// <summary>
        /// Gets or sets the number of frames to move.
        /// </summary>
        public int FrameCount { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to move forward (true) or backward (false).
        /// </summary>
        public bool Forward { get; set; }
        /// <summary>
        /// Gets or sets the video file path.
        /// </summary>
        public string VideoFilePath { get; set; }
    }
}
