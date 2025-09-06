#region License
/*
Copyright © Joan Charmant 2012.
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
using System.IO;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Allows the user to accept or discard the recovery of projects.
    /// All or nothing : either all projects are recovered or none.
    /// No second chance, once discarded, the recoverable projects are deleted for good.
    /// </summary>
    public partial class FormCrashRecovery : Form
    {
        private List<ScreenDescriptorPlayback> recoverables = new List<ScreenDescriptorPlayback>();
        
        public FormCrashRecovery(List<ScreenDescriptorPlayback> recoverables)
        {
            InitializeComponent();
            InitializeUI();

            this.recoverables = recoverables;
            foreach(ScreenDescriptorPlayback recoverable in recoverables)
            {
                ListViewItem item = lvRecoverables.Items.Add(Path.GetFileName(recoverable.FullPath));
                item.SubItems.Add(String.Format("{0:G}", recoverable.RecoveryLastSave));
            }
        }
        private void InitializeUI()
        {
            this.Text = ScreenManagerLang.FormCrashRecovery_Title;
            lblInfo.Text = ScreenManagerLang.FormCrashRecovery_Info;
            btnOk.Text = ScreenManagerLang.FormCrashRecovery_Recover;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
            lvRecoverables.Columns[0].Text = ScreenManagerLang.FormCrashRecovery_File;
            lvRecoverables.Columns[1].Text = ScreenManagerLang.FormCrashRecovery_Date;
        }
        
        private void BtnOKClick(object sender, EventArgs e)
        {
            // Store recover info in the launch manager, they will be picked up when the screen manager actually starts.
            LaunchSettingsManager.ClearScreenDescriptors();
            foreach(ScreenDescriptorPlayback sdp in recoverables)
                LaunchSettingsManager.AddScreenDescriptor(sdp);
            
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
