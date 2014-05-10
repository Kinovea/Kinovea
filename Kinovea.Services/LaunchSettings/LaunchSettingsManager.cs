#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
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
        public static List<IScreenDescription> ScreenDescriptions
        {
            get { return screenDescriptions; }
        }

        public static bool ShowExplorer { get ; set; }

        static LaunchSettingsManager()
        {
            ShowExplorer = true;
        }
        
        private static List<IScreenDescription> screenDescriptions = new List<IScreenDescription>();
        
        public static void ClearScreenDescriptions()
        {
            screenDescriptions.Clear();
        }
        public static void AddScreenDescription(IScreenDescription screenDescription)
        {
            // Note: it is possible to add more than 2 screen descriptions.
            screenDescriptions.Add(screenDescription);
        }
    }
}
