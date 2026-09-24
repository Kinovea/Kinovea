using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using System.IO;
using Kinovea.Camera;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds the best screen to load the video into, creating a new one if necessary, and loads the video into it.
    /// </summary>
    public static class LoaderVideo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, ScreenDescriptorPlayback screenDescriptor, int targetScreen = -1)
        {
            CameraTypeManager.CancelThumbnails();
            CameraTypeManager.StopDiscoveringCameras();

            if (targetScreen < 0)
            {
                LoadUnspecified(manager, path, screenDescriptor);
                return;
            }

            // If the target is specified but icompatible, we don't load.
            // This is less surprising than loading in a different screen.
            AbstractScreen screen = manager.GetScreenAt(targetScreen);
            if (screen != null && screen is PlayerScreen)
            {
                LoadInSpecificTarget(manager, path, screenDescriptor, targetScreen);
            }
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            int index = manager.EnsurePlayerVisible();
            if (index < 0)
                return;

            LoadInSpecificTarget(manager, path, screenDescriptor, index);
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, string path, ScreenDescriptorPlayback screenDescriptor, int targetScreen)
        {
            PlayerScreen playerScreen = manager.GetScreenAt(targetScreen) as PlayerScreen;
            if (playerScreen == null)
                return;
   

            if (playerScreen.IsWaitingForIdle)
            {
                // The player screen will yield its thread after having loaded the first frame and come back later.
                // We must not launch a new video while it's waiting.
                return;
            }

            bool confirmed = playerScreen.BeforeUnloadingAnnotations();
            if (!confirmed)
                return;

            LoadVideo(playerScreen, path, screenDescriptor);

            if (screenDescriptor != null && screenDescriptor.IsReplayWatcher)
            {
                PreferencesManager.FileExplorerPreferences.LastReplayFolder = path;
            }

            if (playerScreen.FrameServer.Loaded)
            {
                //string videoPath = playerScreen.FrameServer.Metadata.VideoPath;
                string videoPath = playerScreen.FrameServer.VideoReader.FilePath;
                NotificationCenter.RaiseFileOpened(videoPath);
                PreferencesManager.FileExplorerPreferences.AddRecentFile(videoPath);
            }

            manager.OrganizeScreens();
            manager.OrganizeCommonControls();
            manager.OrganizeMenus();
            NotificationCenter.RaiseUpdateStatus();
        }
   
        /// <summary>
        /// Actually loads the video into the chosen screen.
        /// </summary>
        public static void LoadVideo(PlayerScreen player, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            log.DebugFormat("Loading video {0}.", Path.GetFileName(path));

            if (screenDescriptor == null)
                return;

            NotificationCenter.RaiseBeforeLoadVideo();
            NotificationCenter.RaiseStopPlayback(null);

            if (player.FrameServer.Loaded)
            {
                player.DeactivateVideoFilter();
                player.view.ResetToEmptyState();
            }

            // The view we are loading into may already have a screen descriptor that we should honor.
            // This has been handled in ScreenManager.DoLoadVideoInScreen().
            // The passed screen descriptor is already merged with the existing state.
            player.view.ScreenDescriptor = screenDescriptor;
            player.Id = screenDescriptor.Id;
            
            if (string.IsNullOrEmpty(path))
            {
                // This can happen when we load an empty screen from launch settings / workspace.
                player.view.EnableDisableActions(false);
                return;
            }

            OpenVideoResult res = player.FrameServer.Load(path);

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
                        break;
                    }
                default:
                    {
                        DisplayErrorAndDisable(player, ScreenManagerLang.LoadMovie_UnkownError);
                        break;
                    }
            }

            if (res != OpenVideoResult.Success && 
                player.view.ScreenDescriptor != null && 
                player.view.ScreenDescriptor.IsReplayWatcher)
            {
                // Even if we can't load the latest video, or there's no video at all, we should still start watching this folder.
                player.view.EnableDisableActions(false);
                player.StartReplayWatcher(null);
            }
        }

        private static void AfterLoadSuccess(PlayerScreen player)
        {
            // Try to load first frame and other initializations.
            int postLoadResult = player.view.PostLoadProcess();
            player.AfterLoad();

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
