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
        /// The enum value of the command. 
        /// </summary>
        public int CommandCode { get; }
        
        /// <summary>
        /// Name of the command, derived from the enum for now.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Shortcut key for the command.
        /// </summary>
        public Keys KeyData { get; set; }
        
        public HotkeyCommand(int commandCode, string name, Keys keyData)
        {
            CommandCode = commandCode;
            Name = name;
            KeyData = keyData;
        }
    }
}
