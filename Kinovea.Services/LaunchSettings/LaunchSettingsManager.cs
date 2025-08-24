#region License
/*
Copyright © Joan Charmant 2012.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/
#endregion
using System;
using System.Collections.Generic;

namespace Kinovea.Services
{
    /// <summary>
    /// Holds the screen list and configuration to be used at launch.
    /// This is used in the context of command line and auto recovery.
    /// </summary>
    public static class LaunchSettingsManager
    {
        /// <summary>
        /// The window name to load when this instance of Kinovea starts.
        /// Always passed from the command line. Should not be updated afterwards.
        /// </summary>
        public static string RequestedWindowName { get; set; }

        /// <summary>
        /// The window id to load when this instance of Kinovea starts.
        /// This is typically only used by the "Reopen window" menu when
        /// clicking on an anonymous window.
        /// Always passed from the command line. Should not be updated afterwards.
        /// </summary>
        public static string RequestedWindowId { get; set; }

        /// <summary>
        /// The requested screen list to load in this instance.
        /// This is either explicitly described on the command line via the -video option, 
        /// or it is reconstructed from a saved window descriptor.
        /// This can also be used when recovering after a crash.
        /// It is also used as temporary storage to stash the part of the state of a screen we don't want 
        /// to lose after loading different content. (ex: speed slider in playback screen).
        /// </summary>
        public static List<IScreenDescription> ScreenDescriptions { get; } = new List<IScreenDescription>();

        /// <summary>
        /// When starting on a single video with -video option, this describes the state of the explorer.
        /// This is equivalent to .ExplorerVisible in the window descriptor.
        /// Loading a single video is basically recreating a simplified window descriptor here, but 
        /// we don't care about the other window-specific properties like splitter distances.
        /// </summary>
        public static bool ExplorerVisible { get; set; } = true;


        public static void ClearScreenDescriptions()
        {
            ScreenDescriptions.Clear();
        }
        public static void AddScreenDescription(IScreenDescription screenDescription)
        {
            ScreenDescriptions.Add(screenDescription);
        }
    }
}
