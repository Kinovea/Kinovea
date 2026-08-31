using System;
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
                hotkeys = GetDefaultBindings();

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
                hotkeys = GetDefaultBindings();

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

        /// <summary>
        /// Reset a specific command to its default shortcut.
        /// </summary>
        public static void ResetToDefault(string category, HotkeyCommand command)
        {
            Dictionary<string, HotkeyCommand[]> defaultHotkeys = GetDefaultBindings();

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
        /// Return the default command bindings.
        /// </summary>
        private static Dictionary<string, HotkeyCommand[]> GetDefaultBindings()
        {
            Func<object, Keys, HotkeyCommand> make = (en, k) => new HotkeyCommand((int)en, en.ToString(), k);

            // Note: it is important for all commands to be listed here, even those that don't have a 
            // default keyboard shortcut binding. Listing here is what make them appear in the 
            // keyboard shortcut preferences and allow them to be bound by the user.
            // The list in the preferences also documents the existing commands.

            Dictionary<string, HotkeyCommand[]> result = new Dictionary<string, HotkeyCommand[]>
            {
                { "FileExplorer", new HotkeyCommand[]{
                    make(FileExplorerCommands.LaunchSelected, Keys.Enter),
                    make(FileExplorerCommands.RenameSelected, Keys.None),
                    make(FileExplorerCommands.DeleteSelected, Keys.Delete),
                    }
                },
                { "ThumbnailViewerContainer", new HotkeyCommand[]{
                    make(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add),
                    make(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract),
                    }
                },
                { "ThumbnailViewerFiles", new HotkeyCommand[]{
                    make(ThumbnailViewerFilesCommands.LaunchSelected, Keys.Enter),
                    make(ThumbnailViewerFilesCommands.RenameSelected, Keys.F2),
                    make(ThumbnailViewerFilesCommands.DeleteSelected, Keys.Delete),
                    make(ThumbnailViewerFilesCommands.Refresh, Keys.F5)
                    }
                },
                { "ThumbnailViewerCamera", new HotkeyCommand[]{
                    make(ThumbnailViewerCameraCommands.LaunchSelected, Keys.Enter),
                    make(ThumbnailViewerCameraCommands.RenameSelected, Keys.F2),
                    make(ThumbnailViewerCameraCommands.Refresh, Keys.F5)
                    }
                },
                { "DualPlayer", new HotkeyCommand[]{
                    make(DualPlayerCommands.TogglePlay, Keys.Space),
                    make(DualPlayerCommands.GotoPreviousImage, Keys.Left),
                    make(DualPlayerCommands.GotoNextImage, Keys.Right),
                    make(DualPlayerCommands.GotoFirstImage, Keys.Home),
                    make(DualPlayerCommands.GotoLastImage, Keys.End),
                    make(DualPlayerCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left),
                    make(DualPlayerCommands.GotoNextKeyframe, Keys.Control | Keys.Right),
                    make(DualPlayerCommands.GotoSyncPoint, Keys.F8),
                    make(DualPlayerCommands.ToggleSyncMerge, Keys.F9),
                    make(DualPlayerCommands.AddKeyframe, Keys.Insert)
                    }
                },
                { "PlayerScreen", new HotkeyCommand[]{

                    // General
                    make(PlayerScreenCommands.ResetViewport, Keys.Escape),
                    make(PlayerScreenCommands.Close, Keys.Control | Keys.F4),

                    // Playback control
                    make(PlayerScreenCommands.TogglePlay, Keys.Space),
                    make(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up),
                    make(PlayerScreenCommands.IncreaseSpeedRoundTo10, Keys.Shift | Keys.Up),
                    make(PlayerScreenCommands.IncreaseSpeedRoundTo25, Keys.Up),
                    make(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    make(PlayerScreenCommands.DecreaseSpeedRoundTo10, Keys.Shift | Keys.Down),
                    make(PlayerScreenCommands.DecreaseSpeedRoundTo25, Keys.Down),
                    
                    // Frame by frame navigation
                    make(PlayerScreenCommands.GotoPreviousImage, Keys.Left),
                    make(PlayerScreenCommands.GotoNextImage, Keys.Right),
                    make(PlayerScreenCommands.GotoFirstImage, Keys.Home),
                    make(PlayerScreenCommands.GotoLastImage, Keys.End),
                    make(PlayerScreenCommands.GotoPreviousImageForceLoop, Keys.Shift | Keys.Left),
                    make(PlayerScreenCommands.LargeJumpBackward, Keys.PageUp),
                    make(PlayerScreenCommands.LargeJumpForward, Keys.PageDown),
                    make(PlayerScreenCommands.SmallJumpBackward, Keys.Shift | Keys.PageUp),
                    make(PlayerScreenCommands.SmallJumpForward, Keys.Shift | Keys.PageDown),
                    make(PlayerScreenCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left),
                    make(PlayerScreenCommands.GotoNextKeyframe, Keys.Control | Keys.Right),
                    make(PlayerScreenCommands.GotoSyncPoint, Keys.F8),
                    
                    // Synchronization
                    make(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add),
                    make(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract),
                    make(PlayerScreenCommands.ToggleSyncMerge, Keys.F9),

                    // Zoom
                    make(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add),
                    make(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract),
                    make(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0),

                    // Keyframes
                    make(PlayerScreenCommands.AddKeyframe, Keys.Insert),
                    make(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete),
                    make(PlayerScreenCommands.Preset1, Keys.Control | Keys.NumPad1),
                    make(PlayerScreenCommands.Preset2, Keys.Control | Keys.NumPad2),
                    make(PlayerScreenCommands.Preset3, Keys.Control | Keys.NumPad3),
                    make(PlayerScreenCommands.Preset4, Keys.Control | Keys.NumPad4),
                    make(PlayerScreenCommands.Preset5, Keys.Control | Keys.NumPad5),
                    make(PlayerScreenCommands.Preset6, Keys.Control | Keys.NumPad6),
                    make(PlayerScreenCommands.Preset7, Keys.Control | Keys.NumPad7),
                    make(PlayerScreenCommands.Preset8, Keys.Control | Keys.NumPad8),
                    make(PlayerScreenCommands.Preset9, Keys.Control | Keys.NumPad9),
                    make(PlayerScreenCommands.Preset10, Keys.None),

                    // Annotations
                    make(PlayerScreenCommands.CutDrawing, Keys.Control | Keys.X),
                    make(PlayerScreenCommands.CopyDrawing, Keys.Control | Keys.C),
                    make(PlayerScreenCommands.PasteDrawing, Keys.Control | Keys.V),
                    make(PlayerScreenCommands.PasteInPlaceDrawing, Keys.None),
                    make(PlayerScreenCommands.DeleteDrawing, Keys.Delete),
                    make(PlayerScreenCommands.ValidateDrawing, Keys.Enter),
                    make(PlayerScreenCommands.CopyImage, Keys.Control | Keys.Shift | Keys.C),
                    make(PlayerScreenCommands.ToggleDrawingsVisibility, Keys.None),
                    make(PlayerScreenCommands.ChronometerStartStop, Keys.F5),
                    make(PlayerScreenCommands.ChronometerSplit, Keys.F6),
                    make(PlayerScreenCommands.CadenceBeat, Keys.F7),
                    make(PlayerScreenCommands.StartAllTracking, Keys.None),
                    }
                },
                { "DualCapture", new HotkeyCommand[]{
                    make(DualCaptureCommands.ToggleGrabbing, Keys.Space),
                    make(DualCaptureCommands.ToggleRecording, Keys.Control | Keys.Return),
                    make(DualCaptureCommands.TakeSnapshot, Keys.Shift | Keys.Return),
                    }
                },
                { "CaptureScreen", new HotkeyCommand[]{

                    // General
                    make(CaptureScreenCommands.ResetViewport, Keys.Escape),
                    make(CaptureScreenCommands.OpenConfiguration, Keys.F12), 
                    make(CaptureScreenCommands.Close, Keys.Control | Keys.F4),

                    // Grabbing & recording
                    make(CaptureScreenCommands.ToggleGrabbing, Keys.Space),
                    make(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return),
                    make(CaptureScreenCommands.TakeSnapshot, Keys.Shift | Keys.Return),
                    make(CaptureScreenCommands.ToggleArmCaptureTrigger, Keys.None),

                    // Zoom
        
                    // Frame by frame navigation
                    make(CaptureScreenCommands.GotoPreviousImage, Keys.Left),
                    make(CaptureScreenCommands.GotoNextImage, Keys.Right),
                    make(CaptureScreenCommands.GotoFirstImage, Keys.Home),
                    make(CaptureScreenCommands.GotoLastImage, Keys.End),
                    make(CaptureScreenCommands.BackwardRound10Percent, Keys.PageUp),
                    make(CaptureScreenCommands.ForwardRound10Percent, Keys.PageDown),
                    make(CaptureScreenCommands.BackwardRound1Percent, Keys.Shift | Keys.PageUp),
                    make(CaptureScreenCommands.ForwardRound1Percent, Keys.Shift | Keys.PageDown),

                    // Delay
                    make(CaptureScreenCommands.ToggleDelayedDisplay, Keys.Alt | Keys.Home),
                    make(CaptureScreenCommands.IncreaseDelayOneSecond, Keys.Up),
                    make(CaptureScreenCommands.DecreaseDelayOneSecond, Keys.Down), 
                    make(CaptureScreenCommands.IncreaseDelayOneFrame, Keys.Control | Keys.Up),
                    make(CaptureScreenCommands.DecreaseDelayOneFrame, Keys.Control | Keys.Down), 
                    make(CaptureScreenCommands.IncreaseDelayHalfSecond, Keys.Shift | Keys.Up),
                    make(CaptureScreenCommands.DecreaseDelayHalfSecond, Keys.Shift | Keys.Down), 
                    }
                }
            };

            return result;
        }
    }
}
