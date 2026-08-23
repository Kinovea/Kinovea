using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public enum SaveResult
    {
        Success,
        MuxerNotFound,
        MuxerParametersNotAllocated,
        MuxerParametersNotSet,
        VideoStreamNotCreated,
        EncoderNotFound,
        EncoderParametersNotAllocated,
        EncoderParametersNotSet,
        EncoderNotOpened,
        FileNotOpened,
        FileHeaderNotWritten,
        InputFrameNotAllocated,
        MetadataStreamNotCreated,
        MetadataNotWritten,

        /// <summary>
        /// Error with the input frames or enumerator.
        /// </summary>
        InputError,

        UnknownError,
        MovieNotLoaded,
        TranscodeNotFinished,
        Cancelled,

        // Specific to FFMpeg CLI.
        FFMpegNotFound,
        FFMpegNotStarted,
        FFMpegError,
    }
}
