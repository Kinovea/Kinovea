using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Finds the best place to load the camera into, creating a new screen if necessary, and loads the camera into the chosen screen.
    /// </summary>
    public static class LoaderCamera
    {
        /// <summary>
        /// Load a camera in a specific or unspecified screen.
        /// This is called during auto-load or manual load.
        /// </summary>
        public static void LoadCameraInScreen(ScreenManagerKernel manager, CameraSummary summary, int targetScreen, ScreenDescriptorCapture screenDescriptor)
        {
            CameraTypeManager.CancelThumbnails();
            CameraTypeManager.StopDiscoveringCameras();

            if (targetScreen < 0)
                LoadUnspecified(manager, summary, screenDescriptor);
            else
                LoadInSpecificTarget(manager, targetScreen, summary, screenDescriptor);
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, CameraSummary summary, ScreenDescriptorCapture screenDescriptor)
        {
            if (manager.ScreenCount == 0)
            {
                manager.AddCaptureScreen();
                LoadInSpecificTarget(manager, 0, summary, screenDescriptor);
            }
            else if (manager.ScreenCount == 1)
            {
                LoadInSpecificTarget(manager, 0, summary, screenDescriptor);
            }
            else if (manager.ScreenCount == 2)
            {
                int target = manager.FindTargetScreen(typeof(CaptureScreen));
                if (target != -1)
                    LoadInSpecificTarget(manager, target, summary, screenDescriptor);
            }
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, int targetScreen, CameraSummary summary, ScreenDescriptorCapture screenDescriptor)
        {
            AbstractScreen screen = manager.GetScreenAt(targetScreen);

            if (screen is CaptureScreen)
            {
                CaptureScreen captureScreen = screen as CaptureScreen;
                captureScreen.ConfigureScreen(screenDescriptor);
                captureScreen.LoadCamera(summary);

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
                    LoadInSpecificTarget(manager, 1, summary, screenDescriptor);
                }
            }
        }

    }
}
