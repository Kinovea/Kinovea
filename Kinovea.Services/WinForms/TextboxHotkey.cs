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

        private string category;
        private string name;
        private Keys keyData;
        
        public void SetKeydata(string category, string name, Keys keyData)
        {
            this.category = category;
            this.name = name;
            this.keyData = keyData;
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
            bool unique = HotkeySettingsManager.ActiveBindings.IsUnique(category, name, keyData);
            this.ForeColor = unique ? Color.Black : Color.DarkRed;
        }

    }
}

