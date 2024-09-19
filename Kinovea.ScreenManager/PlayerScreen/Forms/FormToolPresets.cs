#region License
/*
Copyright © Joan Charmant 2011.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The dialog lets the user configure the whole list of tool presets.
    /// Modifications done on the current presets, reload from file to revert.
    /// Replaces FormColorProfile.
    /// </summary>
    public partial class FormToolPresets : Form
    {
        #region Members
        private bool manualClose;
        private List<AbstractStyleElement> styleElements = new List<AbstractStyleElement>();
        private int editorsLeft;
        private AbstractDrawingTool preselect;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        #region Constructor
        public FormToolPresets():this(null){}
        public FormToolPresets(AbstractDrawingTool preselect)
        {
            this.preselect = preselect;
            InitializeComponent();
            LocalizeForm();
        }
        #endregion

        private void FormToolPresets_Load(object sender, EventArgs e)
        {
            LoadPresets(true);
        }
        private void LocalizeForm()
        {
            // Window & Controls
            this.Text = "   " + ScreenManagerLang.dlgColorProfile_Title;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            btnApply.Text = ScreenManagerLang.Generic_Apply;

            // ToolTips
            toolTips.SetToolTip(btnLoadProfile, ScreenManagerLang.dlgColorProfile_ToolTip_LoadProfile);
            toolTips.SetToolTip(btnSaveProfile, ScreenManagerLang.dlgColorProfile_ToolTip_SaveProfile);
            toolTips.SetToolTip(btnDefault, ScreenManagerLang.dlgColorProfile_ToolTip_DefaultProfile);
        }
        private void LoadPresets(bool memorize)
        {
            lvPresets.Items.Clear();

            IEnumerable<AbstractDrawingTool> tools = ToolManager.Tools.Values.OrderBy(t => t.DisplayName);
            foreach(AbstractDrawingTool tool in tools)
            {
                if (tool.StyleElements == null || tool.StyleElements.Elements.Count == 0)
                    continue;
                
                iconList.Images.Add(tool.Name, tool.Icon);
                ListViewItem item = new ListViewItem(tool.DisplayName, tool.Name);
                item.Tag = tool;

                if(memorize)
                    tool.StyleElements.Memorize();

                if(tool == preselect)
                    item.Selected = true;
                
                lvPresets.Items.Add(item);
                int t = lvPresets.SelectedItems.Count;
            }

            if (lvPresets.Items.Count > 0 && lvPresets.SelectedItems.Count == 0)
                lvPresets.Items[0].Selected = true;
        }
        private void LoadPreset(AbstractDrawingTool preset)
        {
            // Load a single preset
            
            // Tool title and icon
            btnToolIcon.BackColor = Color.Transparent;
            btnToolIcon.Image = preset.Icon;
            lblToolName.Text = preset.DisplayName;
            
            // Clean up
            styleElements.Clear();
            grpConfig.Controls.Clear();
            Size editorSize = new Size(60,20);
            
            int minimalWidth = btnApply.Width + btnCancel.Width + 10;
            editorsLeft = minimalWidth - 20 - editorSize.Width;
            
            int mimimalHeight = grpConfig.Height;
            int lastEditorBottom = 10;
            
            foreach(KeyValuePair<string, AbstractStyleElement> pair in preset.StyleElements.Elements)
            {
                AbstractStyleElement styleElement = pair.Value;
                styleElements.Add(styleElement);
                
                Button btn = new Button();
                btn.Image = styleElement.Icon;
                btn.Size = new Size(20,20);
                btn.Location = new Point(10, lastEditorBottom + 15);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                
                Label lbl = new Label();
                lbl.Text = styleElement.DisplayName;
                lbl.AutoSize = true;
                lbl.Location = new Point(btn.Right + 10, lastEditorBottom + 20);
                
                SizeF labelSize = TextHelper.MeasureString(lbl.Text, lbl.Font);
                
                if(lbl.Left + labelSize.Width + 25 > editorsLeft)
                {
                    editorsLeft = (int)(lbl.Left + labelSize.Width + 25);
                }
                
                Control miniEditor = styleElement.GetEditor();
                miniEditor.Size = editorSize;
                miniEditor.Location = new Point(editorsLeft, btn.Top);
                
                lastEditorBottom = miniEditor.Bottom;
                
                grpConfig.Controls.Add(btn);
                grpConfig.Controls.Add(lbl);
                grpConfig.Controls.Add(miniEditor);
            }
            
            // Recheck all mini editors to realign them on the leftmost.
            foreach(Control c in grpConfig.Controls)
            {
                if(!(c is Label) && !(c is Button))
                {
                    if(c.Left < editorsLeft) 
                        c.Left = editorsLeft;
                }
            }
        }
        private void lvPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPresets.SelectedItems.Count != 1)
                return;

            AbstractDrawingTool preset = lvPresets.SelectedItems[0].Tag as AbstractDrawingTool;
            if (preset == null)
                return;
            
            LoadPreset(preset);
        }
        private void BtnDefaultClick(object sender, EventArgs e)
        {
            // Reset all tools to their default preset.
            foreach(AbstractDrawingTool tool in ToolManager.Tools.Values)
            {
                if(tool.StyleElements != null && tool.StyleElements.Elements.Count > 0)
                {
                    StyleElements memo = tool.StyleElements.Clone();
                    tool.ResetToDefaultStyle();
                    tool.StyleElements.Memorize(memo);
                }
            }
            
            LoadPresets(false);
        }
        private void BtnLoadProfileClick(object sender, EventArgs e)
        {
            // load file to working copy of the profile
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = ScreenManagerLang.dlgColorProfile_ToolTip_LoadProfile;
            openFileDialog.Filter = FilesystemHelper.OpenXMLFilter();
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Software.ColorProfileDirectory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    ToolManager.LoadPresets(filePath);
                    LoadPresets(false);
                }
            }
        }
        private void BtnSaveProfileClick(object sender, EventArgs e)
        {
            // Save current working copy to file
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgColorProfile_ToolTip_SaveProfile;
            saveFileDialog.Filter = FilesystemHelper.SaveXMLFilter();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.InitialDirectory = Software.ColorProfileDirectory;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    ToolManager.SavePresets(filePath);
                }
            }
        }
        
        #region Form closing
        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!manualClose)
            {
                Revert();
            }
        }
        private void Revert()
        {
            // Revert to memos
            foreach(AbstractDrawingTool tool in ToolManager.Tools.Values)
            {
                if(tool.StyleElements != null && tool.StyleElements.Elements.Count > 0)
                {
                    tool.StyleElements.Restore();
                }
            }
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Revert();
            manualClose = true;
        }
        private void BtnOK_Click(object sender, EventArgs e)
        {
            ToolManager.SavePresets();
            manualClose = true;	
        }
        #endregion
    }
}
