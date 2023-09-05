using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class HotkeyCommand
    {
        public int CommandCode { get; set; }
        public string Name { get; set; }
        public Keys KeyData { get; set; }
        
        public HotkeyCommand(int commandCode, string name, Keys keyData)
        {
            this.CommandCode = commandCode;
            this.Name = name;
            this.KeyData = keyData;
        }
    }
}
