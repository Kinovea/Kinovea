
namespace Kinovea.Video
{
    /// <summary>
    /// The current caching mode the video reader is in.
    /// </summary>
    public enum VideoDecodingMode
    {
        /// <summary>
        /// The video is just opening or has closed and the reader is not fully initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// Synchronously read frames when the player requests them.
        /// Used at init, for frame enumeration (export or video modes).
        /// </summary>
        OnDemand,

        /// <summary>
        /// Asynchronous reading and decoding. 
        /// Frames are decoded on a separate thread and pushed to a cache.
        /// Only supported in the FFmpeg reader.
        /// </summary>
        PreBuffering,

        /// <summary>
        /// All the frames of the working zone are loaded into a large buffer.
        /// Supported by FFmpeg and GIF readers.
        /// </summary>
        Caching,
    }
}
