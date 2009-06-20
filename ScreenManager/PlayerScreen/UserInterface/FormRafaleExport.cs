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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Resources;
using System.Reflection;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class formRafaleExport : Form
    {
        //----------------------------------------------------------
        // /!\ The interval slider is in thousandth of seconds. (ms)
        //----------------------------------------------------------
        #region Members
        private PlayerScreenUserInterface m_PlayerScreenUserInterface;      // parent
        private string m_FullPath;
        private Int64 m_iSelectionDuration;                                 // in timestamps.
        private double m_fTimestampsPerSeconds;                             // ratio
        private double m_fDurationInSeconds;
        private double m_fFramesPerSeconds;                                 // only for infos, not used in calculations.
        private ResourceManager m_ResourceManager;
        private int m_iEstimatedTotal;
        #endregion

        public formRafaleExport(PlayerScreenUserInterface _psui, string _FullPath, Int64 _iSelDuration, double _tsps, double _fps)
        {
            m_PlayerScreenUserInterface = _psui;
            m_FullPath = _FullPath;
            m_iSelectionDuration = _iSelDuration;
            m_fTimestampsPerSeconds = _tsps;
            m_fDurationInSeconds = m_iSelectionDuration / m_fTimestampsPerSeconds;
            m_fFramesPerSeconds = _fps;
            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            m_iEstimatedTotal = 0;

            InitializeComponent();
            
            
            

            SetupUICulture();
            SetupData();
        }
        private void SetupUICulture()
        {
            // Window
            this.Text = "   " + m_ResourceManager.GetString("dlgRafaleExport_Title", Thread.CurrentThread.CurrentUICulture);
            
            // Group Config
            grpboxConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);
            //tooltip = m_ResourceManager.GetString("dlgRafaleExport_Tooltip_FrequencyViewer", Thread.CurrentThread.CurrentUICulture);
            //tooltip = m_ResourceManager.GetString("dlgRafaleExport_Tooltip_IntervalSlider", Thread.CurrentThread.CurrentUICulture);
            chkBlend.Text = m_ResourceManager.GetString("dlgRafaleExport_LabelBlend", Thread.CurrentThread.CurrentUICulture);
            chkKeyframesOnly.Text = m_ResourceManager.GetString("dlgRafaleExport_LabelKeyframesOnly", Thread.CurrentThread.CurrentUICulture);
            if (m_PlayerScreenUserInterface.Metadata.Count > 0)
            {
                chkKeyframesOnly.Enabled = true;
            }
            else
            {
                chkKeyframesOnly.Enabled = false;
            }
            
            // Group Infos
            grpboxInfos.Text = m_ResourceManager.GetString("dlgRafaleExport_GroupInfos", Thread.CurrentThread.CurrentUICulture);
            lblInfosTotalFrames.Text = m_ResourceManager.GetString("dlgRafaleExport_LabelTotalFrames", Thread.CurrentThread.CurrentUICulture);
            lblInfosFileSuffix.Text = m_ResourceManager.GetString("dlgRafaleExport_LabelInfoSuffix", Thread.CurrentThread.CurrentUICulture);
            lblInfosTotalSeconds.Text = String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelTotalSeconds", Thread.CurrentThread.CurrentUICulture), m_fDurationInSeconds);

            // Buttons
            btnOK.Text = m_ResourceManager.GetString("Generic_Save", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
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
            lblInfosFrequency.Text = m_ResourceManager.GetString("dlgRafaleExport_LabelFrequencyRoot", Thread.CurrentThread.CurrentUICulture) + " ";
            if (fInterval < 1)
            {
                int iHundredth = (int)(fInterval * 100);
                lblInfosFrequency.Text += String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelFrequencyHundredth", Thread.CurrentThread.CurrentUICulture), iHundredth);
            }
            else
            {
                lblInfosFrequency.Text += String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelFrequencySeconds", Thread.CurrentThread.CurrentUICulture), fInterval);
            }

            // Number of frames
            double fTotalFrames;
            if (chkKeyframesOnly.Checked)
            {
                fTotalFrames = (double)m_PlayerScreenUserInterface.Metadata.Count;   
            }
            else
            {
                fTotalFrames = (m_fDurationInSeconds * (1 / fInterval)) + 0.5;
            }
            m_iEstimatedTotal = (int)fTotalFrames;

            lblInfosTotalFrames.Text = String.Format(m_ResourceManager.GetString("dlgRafaleExport_LabelTotalFrames", Thread.CurrentThread.CurrentUICulture), fTotalFrames);
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = m_ResourceManager.GetString("dlgSaveSequenceTitle", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = m_ResourceManager.GetString("dlgSaveFilter", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(m_FullPath);
            
            Hide();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    Int64 iIntervalTimeStamps = (Int64)(((double)trkInterval.Value / 1000) * m_fTimestampsPerSeconds);

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