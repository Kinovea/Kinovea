using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Root.Properties;
using Kinovea.Services;
using Kinovea.Root.Languages;

namespace Kinovea.Root
{
    public partial class PreferencePanelKeyboard : UserControl, IPreferencePanel
    {
        #region IPreferencePanel properties
        public string Description
        {
            get { return description; }
        }
        public Bitmap Icon
        {
            get { return icon; }
        }
        public List<PreferenceTab> Tabs
        {
            get { return tabs; }
        }
        #endregion

        #region Members
        private string description;
        private Bitmap icon;
        private List<PreferenceTab> tabs = new List<PreferenceTab> { PreferenceTab.Keyboard_General };
        private Dictionary<string, HotkeyCommand[]> hotkeys;
        private string selectedCategory;
        private HotkeyCommand selectedCommand;
        #endregion

        public PreferencePanelKeyboard()
        {
            InitializeComponent();
            this.BackColor = Color.White;

            description = RootLang.dlgPreferences_tabKeyboard;
            icon = Resources.keyboard;

            ImportPreferences();
            InitPage();
        }

        public void OpenTab(PreferenceTab tab)
        {
        }

        public void Close()
        {
        }

        private void ImportPreferences()
        {
            hotkeys = HotkeySettingsManager.Hotkeys;
        }
        private void InitPage()
        {
            lblCategories.Text = RootLang.dlgPreferences_Keyboard_lblCategories;
            lblCommands.Text = RootLang.dlgPreferences_Keyboard_lblCommands;
            btnApply.Text = RootLang.dlgPreferences_Keyboard_btnApply;
            btnClear.Text = RootLang.dlgPreferences_Keyboard_btnClear;
            btnDefault.Text = RootLang.dlgPreferences_Keyboard_btnDefault;

            lbCategories.Items.Clear();

            foreach (string category in hotkeys.Keys)
                lbCategories.Items.Add(category);

            if (lbCategories.Items.Count > 0)
                lbCategories.SelectedIndex = 0;
        }

        public void CommitChanges()
        {
            // TODO: refactor to be able to cancel.
            // the whole hotkeys should be an object, not a static, 
            // so we can work on a clone of it and discard it if user cancels.
        }

        private void lbCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            string category = lbCategories.SelectedItem as string;
            if (string.IsNullOrEmpty(category) || !hotkeys.ContainsKey(category))
                return;

            selectedCategory = category;
            UpdateCommandView(selectedCategory);
        }

        private void UpdateCommandView(string category)
        {
            lvCommands.Items.Clear();
            HotkeyCommand[] commands = hotkeys[category];

            foreach (HotkeyCommand command in commands)
            {
                string name = command.Name;
                string key = command.KeyData.ToText();
                ListViewItem item = new ListViewItem(new string[] { name, key });
                item.Tag = command;
                if (command == selectedCommand)
                    item.Selected = true;
                lvCommands.Items.Add(item);
            }

            int secondColumnWidth = lvCommands.ClientSize.Width - lvCommands.Columns[0].Width;
            lvCommands.Columns[1].Width = secondColumnWidth;
            
            if (lvCommands.Items.Count > 0 && (lvCommands.SelectedItems == null || lvCommands.SelectedItems.Count == 0))
                lvCommands.Items[0].Selected = true;

            if (lvCommands.SelectedItems.Count > 0)
                lvCommands.SelectedItems[0].EnsureVisible();

            lvCommands.Select();
            lvCommands.HideSelection = false;
        }

        private void lvCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvCommands.SelectedItems.Count != 1)
                return;

            HotkeyCommand command = lvCommands.SelectedItems[0].Tag as HotkeyCommand;
            if (command == null)
                return;

            selectedCommand = command;

            lblHotkey.Text = string.Format(RootLang.dlgPreferences_Keyboard_lblHotkey, selectedCategory, selectedCommand.Name);
            tbHotkey.SetKeydata(selectedCategory, selectedCommand);

            // save
            // revert/cancel.
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || selectedCommand == null || selectedCommand.KeyData == Keys.None)
                return;

            selectedCommand.KeyData = Keys.None;
            HotkeySettingsManager.Update(selectedCategory, selectedCommand);
            tbHotkey.SetKeydata(selectedCategory, selectedCommand);
            UpdateCommandView(selectedCategory);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || selectedCommand == null || selectedCommand.KeyData == tbHotkey.KeyData)
                return;

            selectedCommand.KeyData = tbHotkey.KeyData;
            HotkeySettingsManager.Update(selectedCategory, selectedCommand);
            UpdateCommandView(selectedCategory);
        }

        private HotkeyCommand GetSelectedCommand()
        {
            string category = lbCategories.SelectedItem as string;

            if (category == null || lvCommands.SelectedItems.Count != 1)
                return null;

            HotkeyCommand command = lvCommands.SelectedItems[0].Tag as HotkeyCommand;
            return command;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || selectedCommand == null)
                return;

            Keys old = selectedCommand.KeyData;
            HotkeySettingsManager.ResetToDefault(selectedCategory, selectedCommand);

            if (old == selectedCommand.KeyData)
                return;

            tbHotkey.SetKeydata(selectedCategory, selectedCommand);
            UpdateCommandView(selectedCategory);
        }

    }
}
