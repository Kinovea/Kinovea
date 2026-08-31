using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public class KinoveaControl : UserControl
    {
        protected IEnumerable<HotkeyCommand> Hotkeys { get; set; }

        /// <summary>
        /// Overridden: Checks if a hotkey wants to handle the key before letting the message propagate
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.Hotkeys != null)
            {
                foreach (HotkeyCommand hotkey in this.Hotkeys)
                {
                    if (hotkey != null && hotkey.KeyData == keyData)
                        return ExecuteCommand(hotkey.Name);
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Override this method to handle form specific Hotkey commands
        /// </summary>
        protected virtual bool ExecuteCommand(string name)
        {
            return false;
        }
    }
}
