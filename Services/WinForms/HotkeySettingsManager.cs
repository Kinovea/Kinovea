using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class HotkeySettingsManager
    {
        // Reload the hotkeys each time a control is instanciated, so we don't need to 
        // restart the application when mapping is changed.
        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            HotkeySettings settings = new HotkeySettings();
            HotkeySettings[] allSettings = LoadSettings();

            foreach (HotkeySettings s in allSettings)
            {
                if (s.Name == name)
                {
                    settings = s;
                    break;
                }
            }

            return settings != null ? settings.Commands : null;
        }

        private static HotkeySettings[] LoadSettings()
        {
            HotkeySettings[] defaultSettings = CreateDefaultSettings();
            return defaultSettings;
        }

        private static HotkeySettings[] CreateDefaultSettings()
        {
            // TODO: Check if we can set KeyData in the constructor of HotkeyCommand instead.
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString()) { KeyData = k };

            HotkeySettings[] result = new HotkeySettings[] { 
                new HotkeySettings("ThumbnailViewerFiles",
                    hk(ThumbnailViewerFilesCommands.Launch, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.Rename, Keys.F2),
                    hk(ThumbnailViewerFilesCommands.Delete, Keys.Delete)
                ),
                new HotkeySettings("ThumbnailViewerCamera",
                    hk(ThumbnailViewerFilesCommands.Launch, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.Rename, Keys.F2)
                ),
                new HotkeySettings("ThumbnailViewerContainer",
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract), 
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add)
                ),
                new HotkeySettings("PlayerScreen",
                    hk(PlayerScreenCommands.TogglePlay, Keys.Space), 
                    hk(PlayerScreenCommands.TogglePlay, Keys.Return), 
                    hk(PlayerScreenCommands.ResetView, Keys.Escape), 
                    hk(PlayerScreenCommands.GotoPreviousImage, Keys.Left), 
                    hk(PlayerScreenCommands.GotoPreviousImageForceLoop, Keys.Shift | Keys.Left), 
                    hk(PlayerScreenCommands.GotoFirstImage, Keys.Home), 
                    hk(PlayerScreenCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left), 
                    hk(PlayerScreenCommands.GotoNextImage, Keys.Right), 
                    hk(PlayerScreenCommands.GotoLastImage, Keys.End), 
                    hk(PlayerScreenCommands.GotoNextKeyframe, Keys.Control | Keys.Right), 
                    hk(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract), 
                    hk(PlayerScreenCommands.AddKeyframe, Keys.F6), 
                    hk(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete), 
                    hk(PlayerScreenCommands.DeleteDrawing, Keys.Delete), 
                    hk(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRound10, Keys.Shift | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRound25, Keys.Up), 
                    hk(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRound10, Keys.Shift | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRound25, Keys.Down),
                    hk(PlayerScreenCommands.Close, Keys.Control | Keys.F4)
                ),
                new HotkeySettings("CaptureScreen",
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Space), 
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Return), 
                    hk(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return), 
                    hk(CaptureScreenCommands.ResetView, Keys.Escape), 
                    hk(CaptureScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(CaptureScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(CaptureScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(CaptureScreenCommands.IncreaseDelay, Keys.Control | Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelay, Keys.Control | Keys.Down), 
                    hk(CaptureScreenCommands.Close, Keys.Control | Keys.F4) 
                )
            };

            return result;
        }
    }
}
