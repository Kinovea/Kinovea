using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Result returned by recording operations in the capture screen.
    /// </summary>
    public enum RecordingResult
    {
        Success,
        UnknownError,
        MuxerNotFound,
        MuxerParametersNotAllocated,
        EncoderNotFound,
        VideoStreamNotCreated,
        EncoderParametersNotSet,
        EncoderNotOpened,
        FileNotOpened,
        FileHeaderNotWritten,
        InputFrameNotAllocated
    }
}
