using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds the best screen to load the video into, creating a new one if necessary, and loads the video into it.
    /// </summary>
    public static class LoaderVideo
    {
        public static void LoadVideoInScreen(ScreenManagerKernel manager, string path, int targetScreen)
        {
            if (targetScreen < 0)
                LoadUnspecified(manager, path, null);
            else
                LoadInSpecificTarget(manager, targetScreen, path, null);
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
                int emptyScreen = manager.FindEmptyScreen();

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
                bool confirmed = manager.BeforeReplacingPlayerContent(targetScreen);
                if (!confirmed)
                    return;

                LoadVideo(playerScreen, path, screenDescription);

                if (playerScreen.FrameServer.Loaded)
                {
                    NotificationCenter.RaiseFileOpened(null, path);
                    PreferencesManager.FileExplorerPreferences.AddRecentFile(path);
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
        private static void LoadVideo(PlayerScreen player, string path, ScreenDescriptionPlayback screenDescription)
        {
            NotificationCenter.RaiseStopPlayback(null);

            if (player.FrameServer.Loaded)
                player.view.ResetToEmptyState();

            if (screenDescription != null)
                player.view.SetLaunchDescription(screenDescription);

            OpenVideoResult res = player.FrameServer.Load(path);
            player.Id = Guid.NewGuid();

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
