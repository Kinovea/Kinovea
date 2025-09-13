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
        /// The window or workspace name to load when this instance of Kinovea starts.
        /// Always passed from the command line. Should not be updated afterwards.
        /// </summary>
        public static string RequestedWindowName { get; set; }

        /// <summary>
        /// The window or workspace id to load when this instance of Kinovea starts.
        /// Always passed from the command line. Should not be updated afterwards.
        /// </summary>
        public static string RequestedWindowId { get; set; }

        /// <summary>
        /// Screen descriptor optionally created during the parsing of the command line. 
        /// This is for the -video argument or drag and dropping a video file directly on the program/shortcut.
        /// This is incompatible with starting on a specific window by name or id.
        /// This descriptor will be placed in the window descriptors.
        /// </summary>
        public static ScreenDescriptorPlayback CommandLineScreenDescriptor { get; set; }

        /// <summary>
        /// When starting on a single video with -video option, this describes the state of the navigation pane.
        /// This is equivalent to .ExplorerVisible in the window descriptor.
        /// Loading a single video is basically recreating a simplified window descriptor here, but 
        /// we don't care about the other window-specific properties like splitter distances.
        /// </summary>
        public static bool ExplorerVisible { get; set; } = true;
    }
}
