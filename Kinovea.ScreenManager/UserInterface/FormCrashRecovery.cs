#region License
/*
Copyright © Joan Charmant 2012.
joan.charmant@gmail.com 
 
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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Allows the user to accept or discard the recovery of projects.
    /// All or nothing : either all projects are recovered or none.
    /// No second chance, once discarded, the recoverable projects are deleted for good.
    /// </summary>
    public partial class FormCrashRecovery : Form
    {
        private List<ScreenDescriptionPlayback> recoverables = new List<ScreenDescriptionPlayback>();
        
        public FormCrashRecovery(List<ScreenDescriptionPlayback> recoverables)
        {
            InitializeComponent();
            InitializeUI();

            this.recoverables = recoverables;
            foreach(ScreenDescriptionPlayback recoverable in recoverables)
            {
                ListViewItem item = lvRecoverables.Items.Add(Path.GetFileName(recoverable.FullPath));
                item.SubItems.Add(String.Format("{0:G}", recoverable.RecoveryLastSave));
            }
        }
        private void InitializeUI()
        {
            this.Text = "Crash Recovery";
            lblInfo.Text = string.Format("Some projects were not saved properly the last time {0} was run. The following projects can be recovered:", Software.ApplicationName);
            btnOk.Text = "Recover";
            btnCancel.Text = "Cancel";
            lvRecoverables.Columns[0].Text = "File";
            lvRecoverables.Columns[1].Text = "Date";
        }
        
        private void BtnOKClick(object sender, EventArgs e)
        {
            // Store recover info in the launch manager, they will be picked up when the screen manager actually starts.
            LaunchSettingsManager.ClearScreenDescriptions();
            foreach(ScreenDescriptionPlayback sdp in recoverables)
                LaunchSettingsManager.AddScreenDescription(sdp);
            
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
