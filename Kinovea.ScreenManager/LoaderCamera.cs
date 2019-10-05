using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Camera;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds the best place to load the camera into, creating a new screen if necessary, and loads the camera into the chosen screen.
    /// </summary>
    public static class LoaderCamera
    {
        public static void LoadCameraInScreen(ScreenManagerKernel manager, CameraSummary summary, int targetScreen)
        {
            if (targetScreen < 0)
                LoadUnspecified(manager, summary);
            else
                LoadInSpecificTarget(manager, targetScreen, summary);
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, CameraSummary summary)
        {
            if (manager.ScreenCount == 0)
            {
                manager.AddCaptureScreen();
                LoadInSpecificTarget(manager, 0, summary);
            }
            else if (manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(manager, 0, summary);
            }
            else if (manager.ScreenCount == 2)
            {
                int emptyScreen = manager.FindEmptyScreen(typeof(CaptureScreen));

                if (emptyScreen != -1)
                    LoadInSpecificTarget(manager, emptyScreen, summary);
                else
                    LoadInSpecificTarget(manager, 1, summary);
            }
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, int targetScreen, CameraSummary summary)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                CaptureScreen captureScreen = screen as CaptureScreen;
                captureScreen.LoadCamera(summary);

                manager.UpdateCaptureBuffers();
                manager.OrganizeScreens();
                manager.OrganizeCommonControls();
                manager.OrganizeMenus();
            }
            else if (screen is PlayerScreen)
            {
                // Loading a camera onto a video should never close the video.
                // We only load the camera if there is room to create a new capture screen, otherwise we do nothing.
                if (manager.ScreenCount == 1)
                {
                    manager.AddCaptureScreen();
                    LoadInSpecificTarget(manager, 1, summary);
                }
            }
        }

    }
}
