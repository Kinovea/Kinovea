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
            {
                LoadUnspecified(manager, summary, screenDescriptor);
                return;
            }

            // If the target is specified but icompatible, we don't load.
            // This is less surprising than loading in a different screen.
            AbstractScreen screen = manager.GetScreenAt(targetScreen);
            if (screen != null && screen is CaptureScreen)
            {
                LoadInSpecificTarget(manager, targetScreen, summary, screenDescriptor);
            }
        }

        private static void LoadUnspecified(ScreenManagerKernel manager, CameraSummary summary, ScreenDescriptorCapture screenDescriptor)
        {
            int index = manager.EnsureCaptureVisible();
            if (index < 0)
                return;
            
            LoadInSpecificTarget(manager, index, summary, screenDescriptor);
        }

        private static void LoadInSpecificTarget(ScreenManagerKernel manager, int targetScreen, CameraSummary summary, ScreenDescriptorCapture screenDescriptor)
        {
            CaptureScreen captureScreen = manager.GetScreenAt(targetScreen) as CaptureScreen;
            if (captureScreen == null)
                return;

            if (captureScreen.Full)
            {
                // We load a camera on top of another.
                // The incoming screen descriptor is blank (just camera name) while the one in the screen contains 
                // configuration, including post-recording command, that may not exist anywhere else.
                // Swap the screen descriptor for the one in the target screen.
                var cameraName = screenDescriptor.CameraName;
                screenDescriptor = (ScreenDescriptorCapture)captureScreen.GetScreenDescriptor();
                screenDescriptor.CameraName = cameraName;
            }
            else
            {
                // We load a camera on an empty screen, 
                // either for auto-launch or manually.
                // If we are auto-launching we keep the incoming descriptor created from the window.
                // If we are loading on empty in the middle of the session, the descriptor should
                // have been set up with the backup descriptor from the window, or as a last resort,
                // the default descriptor. So we also keep the incoming one.
                // See ScreenManager.DoLoadCameraInScreen().
            }

            captureScreen.ConfigureScreen(screenDescriptor);
            captureScreen.LoadCamera(summary);

            manager.OrganizeScreens();
            manager.OrganizeCommonControls();
            manager.OrganizeMenus();
        }
    }
}
