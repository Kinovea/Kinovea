using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{

    /// <summary>
    /// Defines the startup mode of a window.
    /// These options are presented to the user as "startup" mode but they 
    /// actually define what and when we save the screen list in the window descriptor,
    /// at that's what will determine what will be available when the window is reopened.
    /// </summary>
    public enum WindowStartupMode
    {
        /// <summary>
        /// The window should start on the file explorer.
        /// This is equivalent to starting a browser on a blank page.
        /// We do not save any screen list in the descriptor.
        /// </summary>
        Explorer,

        /// <summary>
        /// The window should start with whatever screen list it had 
        /// when it was closed, and reload the content.
        /// We constantly save the screen list whenever it changes,
        /// and save it on close.
        /// </summary>
        Continue,

        /// <summary>
        /// The window should always start with a specific screen list.
        /// We only save the screen list once, from the "Window properties" dialog.
        /// </summary>
        ScreenList,
    }
}
