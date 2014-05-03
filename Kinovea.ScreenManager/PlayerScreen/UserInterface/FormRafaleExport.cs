/*
Copyright © Joan Charmant 2008.
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

using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class formRafaleExport : Form
    {
        //----------------------------------------------------------
        // /!\ The interval slider is in thousandth of seconds. (ms)
        //----------------------------------------------------------
        #region Members
        private PlayerScreenUserInterface playerScreenUserInterface;      // parent
        private Metadata metadata;
        private string fullPath;
        private long selectionDuration;                                 // in timestamps.
        private double timestampsPerSecond;                             
        private double durationInSeconds;
        private int estimatedTotal;
        #endregion

        public formRafaleExport(PlayerScreenUserInterface playerScreenUserInterface, Metadata metadata, string fullPath, long selectionDuration, double timestampsPerSecond)
        {
            this.playerScreenUserInterface = playerScreenUserInterface;
            this.metadata = metadata;
            this.fullPath = fullPath;
            this.selectionDuration = selectionDuration;
            this.timestampsPerSecond = timestampsPerSecond;
            this.durationInSeconds = selectionDuration / timestampsPerSecond;
            this.estimatedTotal = 0;

            InitializeComponent();
            SetupUICulture();
            SetupData();
        }
        private void SetupUICulture()
        {
            // Window
            this.Text = "   " + ScreenManagerLang.dlgRafaleExport_Title;
            
            // Group Config
            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            chkBlend.Text = ScreenManagerLang.dlgRafaleExport_LabelBlend;
            chkKeyframesOnly.Text = ScreenManagerLang.dlgRafaleExport_LabelKeyframesOnly;
            chkKeyframesOnly.Enabled = metadata.Count > 0;
            
            // Group Infos
            grpboxInfos.Text = ScreenManagerLang.dlgRafaleExport_GroupInfos;
            lblInfosTotalFrames.Text = ScreenManagerLang.dlgRafaleExport_LabelTotalFrames;
            lblInfosFileSuffix.Text = ScreenManagerLang.dlgRafaleExport_LabelInfoSuffix;
            lblInfosTotalSeconds.Text = String.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalSeconds, durationInSeconds);

            // Buttons
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void SetupData()
        {
            // trkInterval values are in milliseconds.
            trkInterval.Minimum = 40;
            trkInterval.Maximum = 8000;
            trkInterval.Value = 1000;
            trkInterval.TickFrequency = 250;
        }
        private void trkInterval_ValueChanged(object sender, EventArgs e)
        {
            freqViewer.Interval = trkInterval.Value;
            UpdateLabels();
        }
        private void chkKeyframesOnly_CheckedChanged(object sender, EventArgs e)
        {
            trkInterval.Enabled = !chkKeyframesOnly.Checked;
            chkBlend.Checked = true;
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            // Frequency
            double interval = (double)trkInterval.Value / 1000;
            lblInfosFrequency.Text = ScreenManagerLang.dlgRafaleExport_LabelFrequencyRoot + " ";
            if (interval < 1)
            {
                int iHundredth = (int)(interval * 100);
                lblInfosFrequency.Text += String.Format(ScreenManagerLang.dlgRafaleExport_LabelFrequencyHundredth, iHundredth);
            }
            else
            {
                lblInfosFrequency.Text += String.Format(ScreenManagerLang.dlgRafaleExport_LabelFrequencySeconds, interval);
            }

            // Number of frames
            double totalFrames;
            if (chkKeyframesOnly.Checked)
                totalFrames = (double)metadata.Count;   
            else
                totalFrames = (durationInSeconds * (1 / interval)) + 0.5;

            estimatedTotal = (int)totalFrames;
            lblInfosTotalFrames.Text = String.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalFrames, totalFrames);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveSequenceTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = ScreenManagerLang.dlgSaveFilter;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(fullPath);

            Hide();

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                Show();
                return;
            }

            if (string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                Close();
                return;
            }

            long intervalTimeStamps = (long)(((double)trkInterval.Value / 1000) * timestampsPerSecond);

            // Launch the Progress bar dialog that will trigger the export, it will call the real function (in PlayerServerUI)
            formFramesExport ffe = new formFramesExport(playerScreenUserInterface, saveFileDialog.FileName, intervalTimeStamps, chkBlend.Checked, chkKeyframesOnly.Checked, estimatedTotal);
            ffe.ShowDialog();
            ffe.Dispose();

            Close();
        }
    }
}