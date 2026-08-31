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
        private string selectedCategory;
        private string selectedCommand;
        #endregion

        public PreferencePanelKeyboard()
        {
            InitializeComponent();
            this.BackColor = Color.White;

            description = RootLang.dlgPreferences_tabKeyboard;
            icon = Resources.ctrl_30;

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
        }
        private void InitPage()
        {
            lblCategories.Text = RootLang.dlgPreferences_Keyboard_lblCategories;
            lblCommands.Text = RootLang.dlgPreferences_Keyboard_lblCommands;
            btnApply.Text = RootLang.dlgPreferences_Keyboard_btnApply;
            btnClear.Text = RootLang.dlgPreferences_Keyboard_btnClear;
            btnDefault.Text = RootLang.dlgPreferences_Keyboard_btnDefault;

            lbCategories.Items.Clear();

            foreach (string category in HotkeySettingsManager.ActiveBindings.GetCategories())
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
            if (string.IsNullOrEmpty(category) || 
                !HotkeySettingsManager.ActiveBindings.HasCategory(category))
            {
                return;
            }

            selectedCategory = category;
            UpdateCommandView(selectedCategory);
        }

        /// <summary>
        /// Load all commands for this category.
        /// </summary>
        private void UpdateCommandView(string category)
        {
            lvCommands.Items.Clear();
            var commands = HotkeySettingsManager.ActiveBindings.GetCommandBindings(category);

            foreach (HotkeyCommand command in commands)
            {
                string name = command.Name;
                string key = command.KeyData == Keys.None ? "" : command.KeyData.ToText();
                ListViewItem item = new ListViewItem(new string[] { name, key });
                item.Tag = command;
                if (name == selectedCommand)
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

            selectedCommand = command.Name;
            
            lblHotkey.Text = string.Format(RootLang.dlgPreferences_Keyboard_lblHotkey, selectedCategory, command.Name);
            tbHotkey.SetKeydata(selectedCategory, command.Name, command.KeyData);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || string.IsNullOrEmpty(selectedCommand))
            { 
                return;
            }

            HotkeySettingsManager.ActiveBindings.Update(selectedCategory, selectedCommand, Keys.None);
            tbHotkey.SetKeydata(selectedCategory, selectedCommand, Keys.None);
            UpdateCommandView(selectedCategory);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || string.IsNullOrEmpty(selectedCommand))
            {
                return;
            }

            HotkeySettingsManager.ActiveBindings.Update(selectedCategory, selectedCommand, tbHotkey.KeyData);
            UpdateCommandView(selectedCategory);
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCategory) || string.IsNullOrEmpty(selectedCommand))
                return;

            HotkeySettingsManager.ResetToDefault(
                HotkeySettingsManager.ActiveBindings, 
                selectedCategory, 
                selectedCommand);

            HotkeyCommand updatedCommand = HotkeySettingsManager.ActiveBindings.FindByName(selectedCategory, selectedCommand);
            if (updatedCommand == null)
            {
                return;
            }

            tbHotkey.SetKeydata(selectedCategory, updatedCommand.Name, updatedCommand.KeyData);
            UpdateCommandView(selectedCategory);
        }

    }
}
