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
    public static class NotificationCenter
    {
        public static EventHandler RecentFilesChanged;
        public static void RaiseRecentFilesChanged(object sender)
        {
            if(RecentFilesChanged != null)
                RecentFilesChanged(sender, EventArgs.Empty);
        }
        
        public static EventHandler<RefreshFileExplorerEventArgs> RefreshFileExplorer;
        public static void RaiseRefreshFileExplorer(object sender, bool refreshThumbnails)
        {
            if(RefreshFileExplorer != null)
                RefreshFileExplorer(sender, new RefreshFileExplorerEventArgs(refreshThumbnails));
        }
        
        public static EventHandler LaunchOpenDialog;
        public static void RaiseLaunchOpenDialog(object sender)
        {
            if(LaunchOpenDialog != null)
                LaunchOpenDialog(sender, EventArgs.Empty);
        }
        
        public static EventHandler<FileActionEventArgs> FileSelected;
        public static void RaiseFileSelected(object sender, string file)
        {
            if (FileSelected != null)
                FileSelected(sender, new FileActionEventArgs(file));
        }

        public static EventHandler<FileActionEventArgs> FileOpened;
        public static void RaiseFileOpened(object sender, string file)
        {
            if (FileOpened != null)
                FileOpened(sender, new FileActionEventArgs(file));
        }

        public static EventHandler<ExplorerTabEventArgs> ExplorerTabChanged;
        public static void RaiseExplorerTabChanged(object sender, ActiveFileBrowserTab tab)
        {
            if (ExplorerTabChanged != null)
                ExplorerTabChanged(sender, new ExplorerTabEventArgs(tab));
        }

        public static EventHandler StopPlayback;
        public static void RaiseStopPlayback(object sender)
        {
            if (StopPlayback != null)
                StopPlayback(sender, EventArgs.Empty);
        }

        public static EventHandler<CurrentDirectoryChangedEventArgs> CurrentDirectoryChanged;
        public static void RaiseCurrentDirectoryChanged(object sender, bool shortcuts, List<string> files, bool refresh)
        {
            if (CurrentDirectoryChanged != null)
                CurrentDirectoryChanged(sender, new CurrentDirectoryChangedEventArgs(shortcuts, files, refresh));
        }

        public static EventHandler<StatusUpdatedEventArgs> StatusUpdated;
        public static void RaiseStatusUpdated(object sender, string status)
        {
            if (StatusUpdated != null)
                StatusUpdated(sender, new StatusUpdatedEventArgs(status));
        }

        public static EventHandler FullScreenToggle;
        public static void RaiseFullScreenToggle(object sender)
        {
            if (FullScreenToggle != null)
                FullScreenToggle(sender, EventArgs.Empty);
        }

        public static EventHandler<PreferenceTabEventArgs> PreferenceTabAsked;
        public static void RaisePreferenceTabAsked(object sender, PreferenceTab tab)
        {
            if (PreferenceTabAsked != null)
                PreferenceTabAsked(sender, new PreferenceTabEventArgs(tab));
        }

        public static EventHandler PreferencesOpened;
        public static void RaisePreferencesOpened(object sender)
        {
            if (PreferencesOpened != null)
                PreferencesOpened(sender, EventArgs.Empty);
        }

        public static EventHandler<ExternalCommandEventArgs> ExternalCommand;
        public static void RaiseExternalCommand(object sender, string name)
        {
            if (ExternalCommand != null)
                ExternalCommand(sender, new ExternalCommandEventArgs(name));
        }
    }
}
