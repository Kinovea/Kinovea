using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Result returned by video export operations.
    /// </summary>
    public enum VideoExportResult
    {
        Success,
        UnknownError,
        Cancelled,
        InputError,
        FFMpegNotFound,
        FFMpegNotStarted,
        FFMpegError,
    }
}
