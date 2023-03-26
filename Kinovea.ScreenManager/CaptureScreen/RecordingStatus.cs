using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This enum is used for visual feedback of the state of the screen.
    /// </summary>
    public enum RecordingStatus
    {
        /// <summary>
        /// Grabbing is running and audio trigger is disarmed.
        /// Recording can still be started manually.
        /// This is the default state.
        /// </summary>
        Disarmed,

        /// <summary>
        /// Audio trigger is armed.
        /// </summary>
        Armed,

        /// <summary>
        /// Audio trigger is in the quiet period.
        /// </summary>
        Quiet,

        /// <summary>
        /// The camera stream is paused.
        /// </summary>
        NotGrabbing,

        /// <summary>
        /// Recording is in progress.
        /// </summary>
        Recording,
    }
}
