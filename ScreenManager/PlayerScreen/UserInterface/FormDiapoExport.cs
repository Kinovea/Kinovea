#region License
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
#endregion

using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This dialog let the user configure a diaporama of the key images.
    /// A diaporama here is a movie where each key image is seen for a lenghty period of time.
    /// An other option is provided, for the creation of a more classic movie where each
    /// key image is paused for a lengthy period of time. (but non key images are included in the video)
    /// 
    /// The dialog is only used to configure the interval time and file name.
    /// </summary>
    public partial class formDiapoExport : Form
    {
        #region Properties
        public string Filename
        {
            get { return m_OutputFileName; }
        }   
        public double FrameInterval
        {
            get { return m_fFrameInterval; }
        }
        public bool PausedVideo
        {
            get { return radioSavePausedVideo.Checked; }
        }			
        #endregion
        
        #region Members
        private string m_OutputFileName;
        private double m_fFrameInterval;
        private bool m_bDiaporama;
        #endregion

        #region Construction and initialization
        public formDiapoExport(bool _diapo)
        {
            m_bDiaporama = _diapo;
            
            InitializeComponent();

            SetupUICulture();
            SetupData();
        }
        private void SetupUICulture()
        {
            this.Text = "   " + ScreenManagerLang.CommandSaveMovie_FriendlyName;
            
            groupSaveMethod.Text = ScreenManagerLang.dlgDiapoExport_GroupDiapoType;
            radioSaveSlideshow.Text = ScreenManagerLang.dlgDiapoExport_RadioSlideshow;            
            radioSavePausedVideo.Text = ScreenManagerLang.dlgDiapoExport_RadioPausedVideo;
            
            grpboxConfig.Text = ScreenManagerLang.Generic_Configuration;
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void SetupData()
        {
            // trkInterval values are in milliseconds.
            trkInterval.Minimum = 40;
            trkInterval.Maximum = 4000;
            trkInterval.Value = 2000;
            trkInterval.TickFrequency = 250;
            
            // default option
            if(m_bDiaporama)
            {
                radioSaveSlideshow.Checked = true;
            }
            else
            {
                radioSavePausedVideo.Checked = true;
            }
            
        }
        #endregion
        
        #region Choice handler
        private void trkInterval_ValueChanged(object sender, EventArgs e)
        {
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            // Frequency
            double fInterval = (double)trkInterval.Value / 1000;
            if (fInterval < 1)
            {
                int iHundredth = (int)(fInterval * 100);
                lblInfosFrequency.Text = String.Format(ScreenManagerLang.dlgDiapoExport_LabelFrequencyHundredth, iHundredth);
            }
            else
            {
                lblInfosFrequency.Text = String.Format(ScreenManagerLang.dlgDiapoExport_LabelFrequencySeconds, fInterval);
            }
        }
        #endregion

        #region OK / Cancel handler
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Hide/Close logic:
            // We start by hiding the current dialog.
            // If the user cancels on the file choosing dialog, we show back ourselves.
            
            Hide();
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveVideoTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
            saveFileDialog.FilterIndex = 1;
            
            DialogResult result = DialogResult.Cancel;
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    // Commit output props.
                    m_OutputFileName = filePath;
                    m_fFrameInterval = (double)trkInterval.Value;
                    
                    DialogResult = DialogResult.OK;
                    result = DialogResult.OK;
                }
            }
            
            if (result == DialogResult.OK)
            {
                Close();
            }
            else
            {
                Show();
            }
        }
        #endregion
    }
}