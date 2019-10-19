using System.Windows.Forms;
using System.Drawing;

namespace Kinovea.Services
{
    public class TextboxHotkey : TextBox
    {
        /// <summary>Gets or sets the KeyData</summary>
        public Keys KeyData
        {
            get { return keyData; }
        }

        private Keys keyData;
        private string category;
        private HotkeyCommand command;

        public void SetKeydata(string category, HotkeyCommand command)
        {
            this.category = category;
            this.command = command;
            keyData = command.KeyData;
            UpdateText();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!keyData.GetKeyCode().IsModifierKey())
            {
                this.keyData = keyData;
                UpdateText();
            }

            return true;
        }

        private void UpdateText()
        {
            this.Text = keyData.ToText();
            bool unique = HotkeySettingsManager.IsUnique(category, new HotkeyCommand(command.CommandCode, command.Name, keyData));
            this.ForeColor = unique ? Color.Black : Color.DarkRed;
        }

    }
}

