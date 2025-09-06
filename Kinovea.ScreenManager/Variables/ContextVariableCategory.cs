using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Types of variables supported.
    /// These variables can be used in various places like 
    /// capture folders, file names, post-recording command line arguments.
    /// Depending on the place some variables are not available.
    /// </summary>
    [Flags]
    public enum ContextVariableCategory
    {
        None = 0,

        /// <summary>
        /// Variables defined by the user from CSV files.
        /// Should be available everywhere.
        /// </summary>
        Custom = 1,

        /// <summary>
        /// Variables representing the current date.
        /// Should be available everywhere.
        /// </summary>
        Date = 2,

        /// <summary>
        /// Variables representing the current time.
        /// Should only be used for file names or commands.
        /// </summary>
        Time = 4,

        /// <summary>
        /// Variables with info about the camera.
        /// Should only be used for the capture file name.
        /// </summary>
        Camera = 8,

        /// <summary>
        /// Variables with info about the recorded file.
        /// Should only be used for the post-recording command.
        /// </summary>
        PostRecordingCommand = 16,
    }
}
