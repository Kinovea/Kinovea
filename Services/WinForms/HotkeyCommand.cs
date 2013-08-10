using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class HotkeyCommand
    {
        public string Name { get; set; }
        public int CommandCode { get; set; }
        public Keys KeyData { get; set; }
        
        public HotkeyCommand()
        {
        }
        public HotkeyCommand(int commandCode, string name)
        {
            this.CommandCode = commandCode;
            this.Name = name;
        }

        public static HotkeyCommand[] FromEnum(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().Select(c => new HotkeyCommand((int)c, c.ToString())).ToArray();
        }
    }
}
