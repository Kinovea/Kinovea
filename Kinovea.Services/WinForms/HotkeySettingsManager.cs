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

        public static bool IsHandler(string category, Keys keys)
        {
            HotkeyCommand[] handledHotkeys = HotkeySettingsManager.LoadHotkeys(category);
            return handledHotkeys.Any(hk => hk != null && hk.KeyData == keys);
        }

        public static void Import(Dictionary<string, HotkeyCommand[]> imported)
        {
            if (hotkeys == null)
                hotkeys = CreateDefaultSettings();

            foreach (string category in imported.Keys)
                foreach (HotkeyCommand command in imported[category])
                    Update(category, command);
        }

        /// <summary>
        /// Returns false if there is a conflict on the hotkey for this category.
        /// </summary>
        public static bool IsUnique(string category, HotkeyCommand command)
        {
            if (!hotkeys.ContainsKey(category) || command.KeyData == Keys.None)
                return true;

            foreach (HotkeyCommand c in hotkeys[category])
            {
                if (c.CommandCode == command.CommandCode || c.KeyData != command.KeyData)
                    continue;

                return false;
            }

            return true;
        }

        public static void Update(string category, HotkeyCommand command)
        {
            // By convention only one hotkey is supported for each command.
            if (!hotkeys.ContainsKey(category))
                return;

            foreach (HotkeyCommand c in hotkeys[category])
            {
                // We test by the command name because the command code is subject to change
                // when we add commands between existing ones.
                if (c.Name == command.Name)
                {
                    c.KeyData = command.KeyData;
                    break;
                }
            }
        }

        public static void ResetToDefault(string category, HotkeyCommand command)
        {
            Dictionary<string, HotkeyCommand[]> defaultHotkeys = CreateDefaultSettings();

            if (!defaultHotkeys.ContainsKey(category))
                return;

            foreach (HotkeyCommand c in defaultHotkeys[category])
            {
                if (c.CommandCode == command.CommandCode)
                {
                    command.KeyData = c.KeyData;
                    break;
                }
            }
        }

        private static Dictionary<string, HotkeyCommand[]> CreateDefaultSettings()
        {
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString(), k);

            Dictionary<string, HotkeyCommand[]> result = new Dictionary<string, HotkeyCommand[]>
            {
                { "FileExplorer", new HotkeyCommand[]{
                    hk(FileExplorerCommands.LaunchSelected, Keys.Enter),
                    //hk(FileExplorerCommands.RenameSelected, Keys.F2),
                    hk(FileExplorerCommands.DeleteSelected, Keys.Delete)
                    }
                },
                { "ThumbnailViewerContainer", new HotkeyCommand[]{
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract), 
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add)
                    }
                },
                { "ThumbnailViewerFiles", new HotkeyCommand[]{
                    hk(ThumbnailViewerFilesCommands.LaunchSelected, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.RenameSelected, Keys.F2),
                    hk(ThumbnailViewerFilesCommands.DeleteSelected, Keys.Delete),
                    hk(ThumbnailViewerFilesCommands.Refresh, Keys.F5)
                    }
                },
                { "ThumbnailViewerCamera", new HotkeyCommand[]{
                    hk(ThumbnailViewerCameraCommands.LaunchSelected, Keys.Enter), 
                    hk(ThumbnailViewerCameraCommands.RenameSelected, Keys.F2),
                    hk(ThumbnailViewerCameraCommands.Refresh, Keys.F5)
                    }
                },
                { "DualPlayer", new HotkeyCommand[]{
                    hk(DualPlayerCommands.TogglePlay, Keys.Space),
                    hk(DualPlayerCommands.GotoPreviousImage, Keys.Left),
                    hk(DualPlayerCommands.GotoFirstImage, Keys.Home), 
                    hk(DualPlayerCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left), 
                    hk(DualPlayerCommands.GotoNextImage, Keys.Right), 
                    hk(DualPlayerCommands.GotoLastImage, Keys.End), 
                    hk(DualPlayerCommands.GotoNextKeyframe, Keys.Control | Keys.Right), 
                    hk(DualPlayerCommands.GotoSyncPoint, Keys.F8), 
                    hk(DualPlayerCommands.ToggleSyncMerge, Keys.F9), 
                    hk(DualPlayerCommands.AddKeyframe, Keys.Insert)
                    }
                },
                { "PlayerScreen", new HotkeyCommand[]{
                    hk(PlayerScreenCommands.TogglePlay, Keys.Space), 
                    hk(PlayerScreenCommands.ResetViewport, Keys.Escape), 
                    hk(PlayerScreenCommands.GotoPreviousImage, Keys.Left), 
                    hk(PlayerScreenCommands.GotoPreviousImageForceLoop, Keys.Shift | Keys.Left), 
                    hk(PlayerScreenCommands.GotoFirstImage, Keys.Home), 
                    hk(PlayerScreenCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left), 
                    hk(PlayerScreenCommands.BackwardRound10Percent, Keys.PageUp),
                    hk(PlayerScreenCommands.BackwardRound1Percent, Keys.Shift | Keys.PageUp),
                    hk(PlayerScreenCommands.GotoNextImage, Keys.Right), 
                    hk(PlayerScreenCommands.GotoLastImage, Keys.End), 
                    hk(PlayerScreenCommands.GotoNextKeyframe, Keys.Control | Keys.Right), 
                    hk(PlayerScreenCommands.ForwardRound10Percent, Keys.PageDown),
                    hk(PlayerScreenCommands.ForwardRound1Percent, Keys.Shift | Keys.PageDown),
                    hk(PlayerScreenCommands.GotoSyncPoint, Keys.F8), 
                    hk(PlayerScreenCommands.ToggleSyncMerge, Keys.F9), 
                    hk(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract), 
                    hk(PlayerScreenCommands.AddKeyframe, Keys.Insert), 
                    hk(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete), 
                    hk(PlayerScreenCommands.DeleteDrawing, Keys.Delete), 
                    hk(PlayerScreenCommands.CopyImage, Keys.Control | Keys.Shift | Keys.C), 
                    hk(PlayerScreenCommands.ValidateDrawing, Keys.Enter),
                    hk(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo10, Keys.Shift | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo25, Keys.Up), 
                    hk(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo10, Keys.Shift | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo25, Keys.Down),
                    hk(PlayerScreenCommands.Close, Keys.Control | Keys.F4)
                    }
                },
                { "DualCapture", new HotkeyCommand[]{
                    hk(DualCaptureCommands.ToggleGrabbing, Keys.Space),
                    hk(DualCaptureCommands.ToggleRecording, Keys.Control | Keys.Return),
                    hk(DualCaptureCommands.TakeSnapshot, Keys.Shift | Keys.Return)
                    }
                },
                { "CaptureScreen", new HotkeyCommand[]{
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Space), 
                    hk(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return), 
                    hk(CaptureScreenCommands.TakeSnapshot, Keys.Shift | Keys.Return), 
                    hk(CaptureScreenCommands.ResetViewport, Keys.Escape), 
                    hk(CaptureScreenCommands.OpenConfiguration, Keys.F12), 
                    //hk(CaptureScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    //hk(CaptureScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    //hk(CaptureScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
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
