
namespace Kinovea.Video
{
    /// <summary>
    /// The current decoding mode the video reader is in.
    /// </summary>
    public enum VideoDecodingMode
    {
        /// <summary>
        /// The video is just opening or has closed and the reader is not fully initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// Frames are decoded on the fly, synchronously, when the player requests them.
        /// Used at init, for frame enumeration (export or video modes), very small videos.
        /// </summary>
        OnDemand,

        /// <summary>
        /// Frames are decoded in a separate thread and pushed to a small buffer.
        /// Only supported by the FFMpeg reader.
        /// </summary>
        PreBuffering,

        /// <summary>
        /// All the frames of the working zone are loaded into a large buffer.
        /// Supported by FFMpeg and GIF readers.
        /// </summary>
        Caching,
    }
}
