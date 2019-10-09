using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using System.IO;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds the best screen to load the video into, creating a new one if necessary, and loads the video into it.
    /// </summary>
    public static class LoaderVideo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, int targetScreen, ScreenDescriptionPlayback screenDescription = null)
        {
            if (targetScreen < 0)
                LoadUnspecified(manager, path, screenDescription);
            else
                LoadInSpecificTarget(manager, targetScreen, path, screenDescription);
        }

        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, ScreenDescriptionPlayback screenDescription)
        {
            LoadUnspecified(manager, path, screenDescription);
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, string path, ScreenDescriptionPlayback screenDescription)
        {
            if (manager.ScreenCount == 0)
            {
                manager.AddPlayerScreen();
                LoadInSpecificTarget(manager, 0, path, screenDescription);
            }
            else if (manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(manager, 0, path, screenDescription);
            }
            else if (manager.ScreenCount == 2)
            {
                int emptyScreen = manager.FindEmptyScreen(typeof(PlayerScreen));

                if (emptyScreen != -1)
                    LoadInSpecificTarget(manager, emptyScreen, path, screenDescription);
                else
                    LoadInSpecificTarget(manager, 1, path, screenDescription);
            }
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, int targetScreen, string path, ScreenDescriptionPlayback screenDescription)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                // Loading a video onto a capture screen should not close the capture screen.
                // If there is room to add a second screen, we add a playback screen and load the video there, otherwise, we don't do anything.
                if (manager.ScreenCount == 1)
                {
                    manager.AddPlayerScreen();
                    manager.UpdateCaptureBuffers();
                    LoadInSpecificTarget(manager, 1, path, screenDescription);
                }
            }
            else if (screen is PlayerScreen)
            {
                PlayerScreen playerScreen = screen as PlayerScreen;

                if (playerScreen.IsWaitingForIdle)
                {
                    // The player screen will yield its thread after having loaded the first frame and come back later.
                    // We must not launch a new video while it's waiting.
                    log.ErrorFormat("Player screen is currently busy loading the previous video. Aborting load.");
                    return;
                }

                bool confirmed = manager.BeforeReplacingPlayerContent(targetScreen);
                if (!confirmed)
                    return;

                LoadVideo(playerScreen, path, screenDescription);

                if (playerScreen.FrameServer.Loaded)
                {
                    NotificationCenter.RaiseFileOpened(null, path);

                    if (screenDescription != null && screenDescription.IsReplayWatcher)
                    {
                        // At this point we have lost the actual file that was loaded. The path here still contaiins the special '*' to indicate the watched folder.
                        // The actual file is the latest file in the folder this was computed right before loading.
                        string actualPath = FilesystemHelper.GetMostRecentFile(Path.GetDirectoryName(path));
                        PreferencesManager.FileExplorerPreferences.AddRecentFile(actualPath);
                        PreferencesManager.FileExplorerPreferences.LastReplayFolder = path;
                    }
                    else
                    {
                        PreferencesManager.FileExplorerPreferences.AddRecentFile(path);
                    }

                    PreferencesManager.Save();
                }

                manager.OrganizeScreens();
                manager.OrganizeCommonControls();
                manager.OrganizeMenus();
                manager.UpdateStatusBar();
            }
        }
   
        /// <summary>
        /// Actually loads the video into the chosen screen.
        /// </summary>
        public static void LoadVideo(PlayerScreen player, string path, ScreenDescriptionPlayback screenDescription)
        {
            // In the case of replay watcher this will only be called the first time, to setup the screen.
            // Subsequent video loads will be done directly by the replay watcher.

            log.DebugFormat("Loading video {0}.", path);
            
            NotificationCenter.RaiseStopPlayback(null);

            if (player.FrameServer.Loaded)
                player.view.ResetToEmptyState();

            player.view.LaunchDescription = screenDescription;

            OpenVideoResult res = player.FrameServer.Load(path);
            player.Id = Guid.NewGuid();

            if (screenDescription != null && screenDescription.Id != Guid.Empty)
                player.Id = screenDescription.Id;

            switch (res)
            {
                case OpenVideoResult.Success:
                    {
                        AfterLoadSuccess(player);
                        break;
                    }
                case OpenVideoResult.FileNotOpenned:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_FileNotOpened);
                        break;
                    }
                case OpenVideoResult.StreamInfoNotFound:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_StreamInfoNotFound);
                        break;
                    }
                case OpenVideoResult.VideoStreamNotFound:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_VideoStreamNotFound);
                        break;
                    }
                case OpenVideoResult.CodecNotFound:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_CodecNotFound);
                        break;
                    }
                case OpenVideoResult.CodecNotOpened:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_CodecNotOpened);
                        break;
                    }
                case OpenVideoResult.CodecNotSupported:
                case OpenVideoResult.NotSupported:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_CodecNotSupported);
                        break;
                    }
                case OpenVideoResult.Cancelled:
                    {
                        break;
                    }
                case OpenVideoResult.EmptyWatcher:
                    {
                        player.view.EnableDisableActions(false);
                        player.StartReplayWatcher(player.view.LaunchDescription, null);
                        PreferencesManager.FileExplorerPreferences.LastReplayFolder = path;
                        PreferencesManager.Save();
                        break;
                    }
                default:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_UnkownError);
                        break;
                    }
            }

        }

        private static void AfterLoadSuccess(PlayerScreen player)
        {
            // Try to load first frame and other initializations.
            int postLoadResult = player.view.PostLoadProcess();
            player.AfterLoad();

            // Note: player.StartReplayWatcher will update the launch descriptor with the current value of the speed slider.
            // This is to support carrying over user defined speed when swapping with the latest video.
            // In the case of the initial load, we need to wait until here to call this function so the view has had time
            // to update the slider with the value set in the descriptor (when using a special default replay speed).
            // Otherwise we would always pick the default value from the view.
            if (player.view.LaunchDescription != null && player.view.LaunchDescription.IsReplayWatcher)
            {
                player.StartReplayWatcher(player.view.LaunchDescription, player.FilePath);
            }
            else
            {
                player.StopReplayWatcher();
            }

            switch (postLoadResult)
            {
                case 0:
                    // Loading succeeded. We already switched to analysis mode if possible.
                    player.view.EnableDisableActions(true);
                    break;
                case -1:
                    {
                        // Loading the first frame failed.
                        player.view.ResetToEmptyState();
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_InconsistantMovieError);
                        break;
                    }
                case -2:
                    {
                        // Loading first frame showed that the file is not supported after all.
                        player.view.ResetToEmptyState();
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_InconsistantMovieError);
                        break;
                    }
                default:
                    break;
            }
        }

        private static void DisplayErrorAndDisable(PlayerScreen player, string error)
        {
            player.view.EnableDisableActions(false);

            MessageBox.Show(
                error,
                ScreenManagerLang.LoadMovie_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }
    }
}
