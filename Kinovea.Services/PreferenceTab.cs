﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    /// <summary>
    /// List all the tabs from the preferences UI.
    /// This is used to open the preferences dialog on an arbitrary tab from other places of the UI.
    /// </summary>
    public enum PreferenceTab
    {
        General_General,

        Player_General,
        Player_Units,
        Player_Memory,

        Drawings_General,
        Drawings_Persistence,
        Drawings_Tracking,

        Capture_General,
        Capture_ImageNaming,
        Capture_VideoNaming,
        Capture_Memory,

        Keyboard_General
    }
}
