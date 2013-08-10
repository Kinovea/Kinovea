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
                new HotkeySettings("ThumbnailViewerContainer",
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract), 
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add)
                )
            };

            return result;
        }
    }
}
