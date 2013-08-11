using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class HotkeySettingsManager
    {
        public static Dictionary<string, HotkeyCommand[]> Hotkeys
        {
            get { return hotkeys; }
        }

        private static Dictionary<string, HotkeyCommand[]> hotkeys;

        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            if (hotkeys == null)
                hotkeys = CreateDefaultSettings();

            HotkeyCommand[] result = null;
            hotkeys.TryGetValue(name, out result);
            return result;
        }

        public static void Import(Dictionary<string, HotkeyCommand[]> imported)
        {
            hotkeys = imported ?? CreateDefaultSettings();
        }

        private static Dictionary<string, HotkeyCommand[]> CreateDefaultSettings()
        {
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString(), k);

            Dictionary<string, HotkeyCommand[]> result = new Dictionary<string, HotkeyCommand[]>
            {
                { "FileExplorer", new HotkeyCommand[]{
                    hk(FileExplorerCommands.Launch, Keys.Enter),
                    hk(ThumbnailViewerFilesCommands.Rename, Keys.F2),
                    hk(FileExplorerCommands.Delete, Keys.Delete)
                    }
                },
                { "ThumbnailViewerFiles", new HotkeyCommand[]{
                    hk(ThumbnailViewerFilesCommands.Launch, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.Rename, Keys.F2),
                    hk(ThumbnailViewerFilesCommands.Delete, Keys.Delete),
                    hk(ThumbnailViewerFilesCommands.Refresh, Keys.F5)
                    }
                },
                { "ThumbnailViewerCamera", new HotkeyCommand[]{
                    hk(ThumbnailViewerFilesCommands.Launch, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.Rename, Keys.F2)
                    }
                },
                { "ThumbnailViewerContainer", new HotkeyCommand[]{
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract), 
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add)
                    }
                },
                { "PlayerScreen", new HotkeyCommand[]{
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
                    hk(PlayerScreenCommands.GotoSyncPoint, Keys.F8), 
                    hk(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract), 
                    hk(PlayerScreenCommands.AddKeyframe, Keys.F6), 
                    hk(PlayerScreenCommands.AddKeyframe, Keys.Insert), 
                    hk(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete), 
                    hk(PlayerScreenCommands.DeleteDrawing, Keys.Delete), 
                    hk(PlayerScreenCommands.CopyImage, Keys.Control | Keys.Shift | Keys.C), 
                    hk(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRound10, Keys.Shift | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRound25, Keys.Up), 
                    hk(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRound10, Keys.Shift | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRound25, Keys.Down),
                    hk(PlayerScreenCommands.Close, Keys.Control | Keys.F4)
                    }
                },
                { "CaptureScreen", new HotkeyCommand[]{
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Space), 
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Return), 
                    hk(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return), 
                    hk(CaptureScreenCommands.ResetView, Keys.Escape), 
                    hk(CaptureScreenCommands.OpenConfiguration, Keys.F12), 
                    hk(CaptureScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(CaptureScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(CaptureScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(CaptureScreenCommands.IncreaseDelay, Keys.Control | Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelay, Keys.Control | Keys.Down), 
                    hk(CaptureScreenCommands.Close, Keys.Control | Keys.F4)
                    }
                }
            };

            return result;
        }
    }
}
