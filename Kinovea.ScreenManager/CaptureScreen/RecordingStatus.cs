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
        /// The camera is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The camera stream is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The camera is streaming and audio trigger is disarmed.
        /// Recording can still be started manually.
        /// This is the default state when connected.
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
        /// Recording is in progress.
        /// </summary>
        Recording,
    }
}
