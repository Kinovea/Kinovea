using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum CaptureRecordingMode
    {
        /// <summary>
        /// In this mode the camera feed is hooked directly to the recorder.
        /// Encoding and storage is done on the fly and has to match camera framerate.
        /// In this mode the MJPEG sources will go straight to storage.
        /// Delay buffer is fed at display framerate and may not contain all the frames.
        /// </summary>
        Camera,

        /// <summary>
        /// In this mode the camera feed goes through the delay buffer before being pulled for recording.
        /// Encoding and storage is done on the fly and has to match camera framerate.
        /// </summary>
        Delay, 

        /// <summary>
        /// In this mode the camera feed goes through the delay buffer, but recording isn't done on the fly.
        /// At the recording stop event, the feed is frozen, and frames are taken from the delay buffer 
        /// and sent to storage all at once. This alleviates encoding perfs issues but only allow 
        /// for time-limited recording, based on the delay buffer capacity.
        /// </summary>
        Scheduled
    }
}
