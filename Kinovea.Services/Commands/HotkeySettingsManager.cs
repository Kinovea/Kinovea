﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class HotkeySettingsManager
    {
        private static ToolStripMenuItem dummy;

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

        /// <summary>
        /// Returns the command corresponding to the passed keyboard shortcut for a specific context.
        /// Returns null if this context doesn't handle the shortcut.
        /// </summary>
        public static HotkeyCommand FindCommand(string category, Keys keys)
        {
            HotkeyCommand[] handledHotkeys = HotkeySettingsManager.LoadHotkeys(category);
            return handledHotkeys.FirstOrDefault(hk => hk != null && hk.KeyData == keys);
        }

        public static void Import(Dictionary<string, HotkeyCommand[]> imported)
        {
            if (hotkeys == null)
                hotkeys = CreateDefaultSettings();

            foreach (string category in imported.Keys)
                foreach (HotkeyCommand command in imported[category])
                    Update(category, command);
        }

        public static Keys GetMenuShortcut(string category, int commandCode)
        {
            Keys keys = Keys.None;

            if (hotkeys.ContainsKey(category))
            {
                HotkeyCommand[] result = null;
                hotkeys.TryGetValue(category, out result);

                foreach (HotkeyCommand c in hotkeys[category])
                {
                    if (c.CommandCode == commandCode)
                    {
                        // Some keys like 'Enter' can't be used as menu shortcuts.
                        try
                        {
                            if (dummy == null)
                                dummy = new ToolStripMenuItem();

                            dummy.ShortcutKeys = c.KeyData;
                            keys = c.KeyData;
                        }
                        catch
                        {
                            // This shortcut key cannot be used as a menu shortcut.
                            keys = Keys.None;
                        }

                        break;
                    }
                }
            }

            return keys; 
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

        /// <summary>
        /// Set default keyboard shortcut bindings.
        /// </summary>
        private static Dictionary<string, HotkeyCommand[]> CreateDefaultSettings()
        {
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString(), k);

            // Note: it is important that all commands are listed here, even those that don't have a 
            // default keyboard shortcut binding. Listing here is what make them appear in the 
            // keyboard shortcut preferences and allow them to be bound by the user.
            // The list in the preferences also documents the existing commands.

            Dictionary<string, HotkeyCommand[]> result = new Dictionary<string, HotkeyCommand[]>
            {
                { "FileExplorer", new HotkeyCommand[]{
                    hk(FileExplorerCommands.LaunchSelected, Keys.Enter),
                    hk(FileExplorerCommands.RenameSelected, Keys.None),
                    hk(FileExplorerCommands.DeleteSelected, Keys.Delete),
                    }
                },
                { "ThumbnailViewerContainer", new HotkeyCommand[]{
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add),
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract),
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
                    hk(DualPlayerCommands.GotoNextImage, Keys.Right),
                    hk(DualPlayerCommands.GotoFirstImage, Keys.Home),
                    hk(DualPlayerCommands.GotoLastImage, Keys.End),
                    hk(DualPlayerCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left),
                    hk(DualPlayerCommands.GotoNextKeyframe, Keys.Control | Keys.Right),
                    hk(DualPlayerCommands.GotoSyncPoint, Keys.F8),
                    hk(DualPlayerCommands.ToggleSyncMerge, Keys.F9),
                    hk(DualPlayerCommands.AddKeyframe, Keys.Insert)
                    }
                },
                { "PlayerScreen", new HotkeyCommand[]{

                    // General
                    hk(PlayerScreenCommands.ResetViewport, Keys.Escape),
                    hk(PlayerScreenCommands.Close, Keys.Control | Keys.F4),

                    // Playback control
                    hk(PlayerScreenCommands.TogglePlay, Keys.Space),
                    hk(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up),
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo10, Keys.Shift | Keys.Up),
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo25, Keys.Up),
                    hk(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo10, Keys.Shift | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo25, Keys.Down),
                    
                    // Frame by frame navigation
                    hk(PlayerScreenCommands.GotoPreviousImage, Keys.Left),
                    hk(PlayerScreenCommands.GotoNextImage, Keys.Right),
                    hk(PlayerScreenCommands.GotoFirstImage, Keys.Home),
                    hk(PlayerScreenCommands.GotoLastImage, Keys.End),
                    hk(PlayerScreenCommands.GotoPreviousImageForceLoop, Keys.Shift | Keys.Left),
                    hk(PlayerScreenCommands.BackwardRound10Percent, Keys.PageUp),
                    hk(PlayerScreenCommands.ForwardRound10Percent, Keys.PageDown),
                    hk(PlayerScreenCommands.BackwardRound1Percent, Keys.Shift | Keys.PageUp),
                    hk(PlayerScreenCommands.ForwardRound1Percent, Keys.Shift | Keys.PageDown),
                    hk(PlayerScreenCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left),
                    hk(PlayerScreenCommands.GotoNextKeyframe, Keys.Control | Keys.Right),
                    hk(PlayerScreenCommands.GotoSyncPoint, Keys.F8),
                    
                    // Synchronization
                    hk(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add),
                    hk(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract),
                    hk(PlayerScreenCommands.ToggleSyncMerge, Keys.F9),

                    // Zoom
                    hk(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add),
                    hk(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract),
                    hk(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0),

                    // Keyframes
                    hk(PlayerScreenCommands.AddKeyframe, Keys.Insert),
                    hk(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete),
                    hk(PlayerScreenCommands.Preset1, Keys.Control | Keys.NumPad1),
                    hk(PlayerScreenCommands.Preset2, Keys.Control | Keys.NumPad2),
                    hk(PlayerScreenCommands.Preset3, Keys.Control | Keys.NumPad3),
                    hk(PlayerScreenCommands.Preset4, Keys.Control | Keys.NumPad4),
                    hk(PlayerScreenCommands.Preset5, Keys.Control | Keys.NumPad5),
                    hk(PlayerScreenCommands.Preset6, Keys.Control | Keys.NumPad6),
                    hk(PlayerScreenCommands.Preset7, Keys.Control | Keys.NumPad7),
                    hk(PlayerScreenCommands.Preset8, Keys.Control | Keys.NumPad8),
                    hk(PlayerScreenCommands.Preset9, Keys.Control | Keys.NumPad9),
                    hk(PlayerScreenCommands.Preset10, Keys.None),

                    // Annotations
                    hk(PlayerScreenCommands.CutDrawing, Keys.Control | Keys.X),
                    hk(PlayerScreenCommands.CopyDrawing, Keys.Control | Keys.C),
                    hk(PlayerScreenCommands.PasteDrawing, Keys.Control | Keys.V),
                    hk(PlayerScreenCommands.PasteInPlaceDrawing, Keys.None),
                    hk(PlayerScreenCommands.DeleteDrawing, Keys.Delete),
                    hk(PlayerScreenCommands.ValidateDrawing, Keys.Enter),
                    hk(PlayerScreenCommands.CopyImage, Keys.Control | Keys.Shift | Keys.C),
                    hk(PlayerScreenCommands.ToggleDrawingsVisibility, Keys.None),
                    hk(PlayerScreenCommands.ChronometerStartStop, Keys.F5),
                    hk(PlayerScreenCommands.ChronometerSplit, Keys.F6),
                    hk(PlayerScreenCommands.CadenceBeat, Keys.F7),
                    hk(PlayerScreenCommands.StartAllTracking, Keys.None),
                    }
                },
                { "DualCapture", new HotkeyCommand[]{
                    hk(DualCaptureCommands.ToggleGrabbing, Keys.Space),
                    hk(DualCaptureCommands.ToggleRecording, Keys.Control | Keys.Return),
                    hk(DualCaptureCommands.TakeSnapshot, Keys.Shift | Keys.Return),
                    }
                },
                { "CaptureScreen", new HotkeyCommand[]{

                    // General
                    hk(CaptureScreenCommands.ResetViewport, Keys.Escape),
                    hk(CaptureScreenCommands.OpenConfiguration, Keys.F12), 
                    hk(CaptureScreenCommands.Close, Keys.Control | Keys.F4),

                    // Grabbing & recording
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Space),
                    hk(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return),
                    hk(CaptureScreenCommands.TakeSnapshot, Keys.Shift | Keys.Return),
                    hk(CaptureScreenCommands.ToggleArmCaptureTrigger, Keys.None),

                    // Zoom
        
                    // Frame by frame navigation
                    hk(CaptureScreenCommands.GotoPreviousImage, Keys.Left),
                    hk(CaptureScreenCommands.GotoNextImage, Keys.Right),
                    hk(CaptureScreenCommands.GotoFirstImage, Keys.Home),
                    hk(CaptureScreenCommands.GotoLastImage, Keys.End),
                    hk(CaptureScreenCommands.BackwardRound10Percent, Keys.PageUp),
                    hk(CaptureScreenCommands.ForwardRound10Percent, Keys.PageDown),
                    hk(CaptureScreenCommands.BackwardRound1Percent, Keys.Shift | Keys.PageUp),
                    hk(CaptureScreenCommands.ForwardRound1Percent, Keys.Shift | Keys.PageDown),

                    // Delay
                    hk(CaptureScreenCommands.ToggleDelayedDisplay, Keys.Alt | Keys.Home),
                    hk(CaptureScreenCommands.IncreaseDelayOneSecond, Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelayOneSecond, Keys.Down), 
                    hk(CaptureScreenCommands.IncreaseDelayOneFrame, Keys.Control | Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelayOneFrame, Keys.Control | Keys.Down), 
                    hk(CaptureScreenCommands.IncreaseDelayHalfSecond, Keys.Shift | Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelayHalfSecond, Keys.Shift | Keys.Down), 
                    }
                }
            };

            return result;
        }
    }
}
