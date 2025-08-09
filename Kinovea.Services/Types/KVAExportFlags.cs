using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// Flags for what to export when saving a KVA file from capture.
    [Flags]
    public enum KVAExportFlags
    {
        /// Mandatory info like version and file paths,
        /// image level adjustments like rotation or mirror,
        /// timing information like capture frame rate and time origin.
        /// This should always be included.
        Basics = 0,

        /// Attached drawings and keyframes.
        Drawings = 1 << 2,

        /// Spatial calibration data, and the coordinate system drawing.
        Calibration = 1 << 3,

        /// Video specific info like detached drawings, trackability,
        /// video filters and camera motion.
        /// This should be excluded when exporting from capture.
        VideoSpecific = 1 << 4,

        /// Include everything.
        /// This should be what is used for everything except capture recording.
        Full = Basics | Drawings | Calibration | VideoSpecific,

        /// Default behavior for capture recording is to include everything except the video specific stuff.
        DefaultCaptureRecording = Basics | Drawings | Calibration
    }
}
