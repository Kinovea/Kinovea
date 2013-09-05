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
        private PlayerScreenUserInterface m_PlayerScreenUserInterface;      // parent
        private Metadata m_Metadata;
        private string m_FullPath;
        private long m_iSelectionDuration;                                 // in timestamps.
        private double m_fTimestampsPerSeconds;                             // ratio
        private double m_fDurationInSeconds;
        private int m_iEstimatedTotal;
        #endregion

        public formRafaleExport(PlayerScreenUserInterface _psui, Metadata _metadata, string _FullPath, long _iSelDuration, double _tsps)
        {
            m_PlayerScreenUserInterface = _psui;
            m_Metadata = _metadata;
            m_FullPath = _FullPath;
            m_iSelectionDuration = _iSelDuration;
            m_fTimestampsPerSeconds = _tsps;
            m_fDurationInSeconds = m_iSelectionDuration / m_fTimestampsPerSeconds;
            m_iEstimatedTotal = 0;

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
            if (m_Metadata.Count > 0)
            {
                chkKeyframesOnly.Enabled = true;
            }
            else
            {
                chkKeyframesOnly.Enabled = false;
            }
            
            // Group Infos
            grpboxInfos.Text = ScreenManagerLang.dlgRafaleExport_GroupInfos;
            lblInfosTotalFrames.Text = ScreenManagerLang.dlgRafaleExport_LabelTotalFrames;
            lblInfosFileSuffix.Text = ScreenManagerLang.dlgRafaleExport_LabelInfoSuffix;
            lblInfosTotalSeconds.Text = String.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalSeconds, m_fDurationInSeconds);

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
            if (chkKeyframesOnly.Checked)
            {
                trkInterval.Enabled = false;
                chkBlend.Checked = true;
            }
            else
            {
                trkInterval.Enabled = true;
                chkBlend.Checked = true;
            }
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            // Frequency
            double fInterval = (double)trkInterval.Value / 1000;
            lblInfosFrequency.Text = ScreenManagerLang.dlgRafaleExport_LabelFrequencyRoot + " ";
            if (fInterval < 1)
            {
                int iHundredth = (int)(fInterval * 100);
                lblInfosFrequency.Text += String.Format(ScreenManagerLang.dlgRafaleExport_LabelFrequencyHundredth, iHundredth);
            }
            else
            {
                lblInfosFrequency.Text += String.Format(ScreenManagerLang.dlgRafaleExport_LabelFrequencySeconds, fInterval);
            }

            // Number of frames
            double fTotalFrames;
            if (chkKeyframesOnly.Checked)
            {
                fTotalFrames = (double)m_Metadata.Count;   
            }
            else
            {
                fTotalFrames = (m_fDurationInSeconds * (1 / fInterval)) + 0.5;
            }
            m_iEstimatedTotal = (int)fTotalFrames;

            lblInfosTotalFrames.Text = String.Format(ScreenManagerLang.dlgRafaleExport_LabelTotalFrames, fTotalFrames);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveSequenceTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = ScreenManagerLang.dlgSaveFilter;
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(m_FullPath);
            
            Hide();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    long iIntervalTimeStamps = (long)(((double)trkInterval.Value / 1000) * m_fTimestampsPerSeconds);

                    // Launch the Progress bar dialog that will trigger the export.
                    // it will call the real function (in PlayerServerUI)
                    formFramesExport ffe = new formFramesExport(m_PlayerScreenUserInterface, filePath, iIntervalTimeStamps, chkBlend.Checked, chkKeyframesOnly.Checked, m_iEstimatedTotal);
                    ffe.ShowDialog();
                    ffe.Dispose();
                }
                Close();
            }
            else
            {
                Show();
            }
        }
    }
}