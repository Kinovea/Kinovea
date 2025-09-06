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

        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, int targetScreen, ScreenDescriptorPlayback screenDescriptor)
        {
            CameraTypeManager.CancelThumbnails();
            CameraTypeManager.StopDiscoveringCameras();

            if (targetScreen < 0)
                LoadUnspecified(manager, path, screenDescriptor);
            else
                LoadInSpecificTarget(manager, targetScreen, path, screenDescriptor);
        }

        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            CameraTypeManager.CancelThumbnails();
            CameraTypeManager.StopDiscoveringCameras();

            LoadUnspecified(manager, path, screenDescriptor);
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            if (manager.ScreenCount == 0)
            {
                manager.AddPlayerScreen();
                LoadInSpecificTarget(manager, 0, path, screenDescriptor);
            }
            else if (manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(manager, 0, path, screenDescriptor);
            }
            else if (manager.ScreenCount == 2)
            {
                int target = manager.FindTargetScreen(typeof(PlayerScreen));
                if (target != -1)
                    LoadInSpecificTarget(manager, target, path, screenDescriptor);
            }
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, int targetScreen, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                // Loading a video onto a capture screen should not close the capture screen.
                // If there is room to add a second screen, we add a playback screen and load the video there, otherwise, we don't do anything.
                if (manager.ScreenCount == 1)
                {
                    manager.AddPlayerScreen();
                    LoadInSpecificTarget(manager, 1, path, screenDescriptor);
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

                bool confirmed = screen.BeforeUnloadingAnnotations();
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
                    NotificationCenter.RaiseFileOpened(null, videoPath);
                    PreferencesManager.FileExplorerPreferences.AddRecentFile(videoPath);
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
        public static void LoadVideo(PlayerScreen player, string path, ScreenDescriptorPlayback screenDescriptor)
        {
            log.DebugFormat("Loading video {0}.", Path.GetFileName(path));

            if (screenDescriptor == null)
                return;

            NotificationCenter.RaiseStopPlayback(null);

            if (player.FrameServer.Loaded)
            {
                player.DeactivateVideoFilter();
                player.view.ResetToEmptyState();
            }

            // The view maintains its own transient screen descriptor.
            player.view.ScreenDescriptor = (ScreenDescriptorPlayback)screenDescriptor.Clone();
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

            if (res != OpenVideoResult.Success && player.view.ScreenDescriptor != null && player.view.ScreenDescriptor.IsReplayWatcher)
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
