using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum CaptureRecordingMode
    {
        /// <summary>
        /// In this mode the camera feed is hooked directly to the recorder through the "Kinovea.Pipeline" framework.
        /// This is the most direct way and offers the best performance, minimum drops.
        /// </summary>
        Camera,

        /// <summary>
        /// In this mode the camera feed goes through the delay compositor before being fed back into the recorder.
        /// This mode allows to record what is displayed on screen including delay, composition and drawings.
        /// This mode runs on the UI thread and may experience frame drops.
        /// </summary>
        Display
    }
}
