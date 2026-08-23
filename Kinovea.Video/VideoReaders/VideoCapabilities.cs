using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Flags indicating the capabilities of the specific file loaded by the reader.
    /// </summary>
    [Flags]
    public enum VideoCapabilities : int
    {
        None = 0,
        CanDecodeOnDemand = 1,
        CanPreBuffer = 2,
        CanCache = 4,
        CanChangeWorkingZone = 8,
        CanChangeAspectRatio = 16,
        CanChangeDeinterlacing = 32,
        CanChangeVideoDuration = 64,
        CanChangeFrameRate = 128,
        CanChangeDecodingSize = 256,
        CanScaleIndefinitely = 512,
        CanChangeImageRotation = 1024,
        CanChangeDemosaicing = 2048,
        CanStabilize = 4096,
    }
}
