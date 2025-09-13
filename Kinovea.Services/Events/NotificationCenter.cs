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
    /// A central event hub to decouple modules.
    /// </summary>
    public static class NotificationCenter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Navigation pane <-> Thumbnail browser communication

        /// <summary>
        /// Event raised by either the naviagtion pane list views or the thumbnail viewer when a file is selected.
        /// To deselect everything set `file` to null.
        /// `sender` is used to check re-entry.
        /// </summary>
        public static EventHandler<EventArgs<string>> FileSelected;
        public static void RaiseFileSelected(object sender, string file)
        {
            FileSelected?.Invoke(sender, new EventArgs<string>(file));
        }

        /// <summary>
        /// Event raised when a folder change is initiated on the thumbnails viewer and requires 
        /// the navigation pane to load the file list and trigger an update, which will circle
        /// back to the file browser to load the list and update the thumbnails.
        /// This is for going "Up" or clicking on the address bar or entering a folder.
        /// Back and forth navigation is handled by FolderNavigationAsked.
        /// </summary>
        public static EventHandler<EventArgs<string>> FolderChangeAsked;
        public static void RaiseFolderChangeAsked(string path)
        {
            FolderChangeAsked?.Invoke(null, new EventArgs<string>(path));
        }

        /// <summary>
        /// Event raised when the file browser side requests a backward or forward navigation 
        /// action in the folder navigation history.
        /// </summary>
        public static EventHandler<EventArgs<FolderNavigationType>> FolderNavigationAsked;
        public static void RaiseFolderNavigationAsked(FolderNavigationType fnt)
        {
            FolderNavigationAsked?.Invoke(null, new EventArgs<FolderNavigationType>(fnt));
        }

        /// <summary>
        /// Event raised when either the navigation pane or the file browser change the content type.
        /// `sender` is used to check re-entry.
        /// </summary>
        public static EventHandler<EventArgs<BrowserContentType>> BrowserContentTypeChanged;
        public static void RaiseBrowserContentTypeChanged(object sender, BrowserContentType tab)
        {
            BrowserContentTypeChanged?.Invoke(sender, new EventArgs<BrowserContentType>(tab));
        }

        /// <summary>
        /// Event raised by the naviagtion pane when the directory was changed and the file list must 
        /// be synchronized to the thumbnails viewer.
        /// </summary>
        public static EventHandler<CurrentDirectoryChangedEventArgs> CurrentDirectoryChanged;
        public static void RaiseCurrentDirectoryChanged(string path, List<string> files, bool isShortcuts, bool doRefresh)
        {
            CurrentDirectoryChanged?.Invoke(null, new CurrentDirectoryChangedEventArgs(path, files, isShortcuts, doRefresh));
        }

        /// <summary>
        /// Event raised whenever a module makes a change to the file system.
        /// For example when creating a new recording, exporting a video, or deleting a file.
        /// </summary>
        public static EventHandler<EventArgs<bool>> RefreshFileList;
        public static void RaiseRefreshFileList(bool refreshThumbnails)
        {
            RefreshFileList?.Invoke(null, new EventArgs<bool>(refreshThumbnails));
        }
        #endregion

        #region Loading videos

        /// <summary>
        /// Event raised when we are about to load a video in a screen.
        /// The thumbnails viewer should stop handling folder changes coming from
        /// the navigation pane and especially avoid loading thumbnails while it is in the background.
        /// </summary>
        public static EventHandler BeforeLoadVideo;
        public static void RaiseBeforeLoadVideo(object sender = null)
        {
            BeforeLoadVideo?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised by the navigation pane or capture screen to load a video.
        /// </summary>
        public static EventHandler<VideoLoadAskedEventArgs> LoadVideoAsked;
        public static void RaiseLoadVideoAsked(string path, int target)
        {
            LoadVideoAsked?.Invoke(null, new VideoLoadAskedEventArgs(path, target));
        }

        /// <summary>
        /// A video file was just opened in a player.
        /// </summary>
        public static EventHandler<EventArgs<string>> FileOpened;
        public static void RaiseFileOpened(string file)
        {
            FileOpened?.Invoke(null, new EventArgs<string>(file));
        }

        /// <summary>
        /// Event raised when we need to stop the video playback.
        /// </summary>
        public static EventHandler StopPlaybackAsked;
        public static void RaiseStopPlayback(object sender = null)
        {
            StopPlaybackAsked?.Invoke(sender, EventArgs.Empty);
        }


        /// <summary>
        /// Event raised after a video is successfully loaded or the recent list is downsized or deleted.
        /// </summary>
        public static EventHandler RecentFilesChanged;
        public static void RaiseRecentFilesChanged()
        {
            RecentFilesChanged?.Invoke(null, EventArgs.Empty);
        }
        #endregion

        #region Sub modules -> Root communication

        /// <summary>
        /// Event raised by the screen manager toolbar to toggle the navigation pane.
        /// </summary>
        public static EventHandler ToggleNavigationPane;
        public static void RaiseToggleNavigationPane()
        {
            ToggleNavigationPane?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised by the thumbnail viewer to toggle full screen mode.
        /// </summary>
        public static EventHandler FullScreenToggle;
        public static void RaiseFullScreenToggle()
        {
            FullScreenToggle?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the status bar should be updated.
        /// </summary>
        public static EventHandler UpdateStatus;
        public static void RaiseUpdateStatus()
        {
            UpdateStatus?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when we need to open the preferences dialog and show a specific page.
        /// </summary>
        public static EventHandler<EventArgs<PreferenceTab>> PreferenceTabAsked;
        public static void RaisePreferenceTabAsked(PreferenceTab tab)
        {
            PreferenceTabAsked?.Invoke(null, new EventArgs<PreferenceTab>(tab));
        }

        /// <summary>
        /// This event is asking the root to refresh the whole UI and optionally 
        /// send a message to other instances.
        /// This should be called after changes in sub-modules that require
        /// to be reflected globally and in other windows.
        /// Ex: changing the context in the capture screen.
        /// </summary>
        public static EventHandler<EventArgs<bool>> TriggerPreferencesUpdated;
        public static void RaiseTriggerPreferencesUpdated(bool sendMessage)
        {
            log.DebugFormat("Raising: TriggerPreferencesUpdated, sendMessage: {0}", sendMessage);
            TriggerPreferencesUpdated?.Invoke(null, new EventArgs<bool>(sendMessage));
        }
        #endregion

        #region Preferences
        
        /// <summary>
        /// Event raised by the preferences dialog when it opens.
        /// Avoid handling playback and capture triggers during this time.
        /// </summary>
        public static EventHandler PreferencesOpened;
        public static void RaisePreferencesOpened()
        {
            PreferencesOpened?.Invoke(null, EventArgs.Empty);
        }

        #endregion

        #region Remote control and external events

        /// <summary>
        /// Event raised by a replay screen when a new video dropped.
        /// </summary>
        public static EventHandler WakeUpAsked;
        public static void RaiseWakeUpAsked()
        {
            WakeUpAsked?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the application window received an external command.
        /// </summary>
        public static EventHandler<EventArgs<string>> ReceivedExternalCommand;
        public static void RaiseReceivedExternalCommand(string name)
        {
            ReceivedExternalCommand?.Invoke(null, new EventArgs<string>(name));
        }
        #endregion
    }
}
