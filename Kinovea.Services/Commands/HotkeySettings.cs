using System;
using System.Xml.Serialization;

namespace Kinovea.Services
{
    /// <summary>
    /// Hotkey mappings of one control
    /// </summary>
    public class HotkeySettings
    {
        public string Name { get; set; }
        public HotkeyCommand[] Commands { get; set; }

        public HotkeySettings()
        {
        }

        public HotkeySettings(string name, params HotkeyCommand[] commands)
        {
            this.Name = name;
            this.Commands = commands;
        }
    }
}