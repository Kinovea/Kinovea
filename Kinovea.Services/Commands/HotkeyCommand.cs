using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class HotkeyCommand
    {
        /// <summary>
        /// Name of the command, derived from the enum name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Shortcut key bound to the command.
        /// </summary>
        public Keys KeyData { get; set; }
        
        public HotkeyCommand(string name, Keys keyData)
        {
            Name = name;
            KeyData = keyData;
        }
    }
}
