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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;

namespace Videa.ScreenManager
{
	/// <summary>
	/// This dialog let the user save a diaporama of the key images.
	/// This is a movie where each key image is seen for a lenghty period of time.
	/// 
	/// The dialog is only used to configure the interval time and file name.
	/// When done, we give back control to SaveDiaporama in PlayerScreenUserInterface.
	/// </summary>
    public partial class formDiapoExport : Form
    {
        #region Members
        private PlayerScreenUserInterface m_PlayerScreenUserInterface;      // parent
        private string m_FullPath;
        private ResourceManager m_ResourceManager;
        #endregion

        #region Construction and initialization
        public formDiapoExport(PlayerScreenUserInterface _psui, string _FullPath)
        {
            m_PlayerScreenUserInterface = _psui;
            m_FullPath = _FullPath;
            m_ResourceManager = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            InitializeComponent();

            SetupUICulture();
            SetupData();
        }
        private void SetupUICulture()
        {
            // Window
            this.Text = "   " + m_ResourceManager.GetString("dlgDiapoExport_Title", Thread.CurrentThread.CurrentUICulture);

            // Group Config
            grpboxConfig.Text = m_ResourceManager.GetString("Generic_Configuration", Thread.CurrentThread.CurrentUICulture);

            // Buttons
            btnOK.Text = m_ResourceManager.GetString("Generic_Save", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);
        }
        private void SetupData()
        {
            // trkInterval values are in milliseconds.
            trkInterval.Minimum = 40;
            trkInterval.Maximum = 8000;
            trkInterval.Value = 2000;
            trkInterval.TickFrequency = 250;
        }
        #endregion
        
        #region Choice handler
        private void trkInterval_ValueChanged(object sender, EventArgs e)
        {
            freqViewer.Interval = trkInterval.Value;
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            // Frequency
            double fInterval = (double)trkInterval.Value / 1000;
            if (fInterval < 1)
            {
                int iHundredth = (int)(fInterval * 100);
                lblInfosFrequency.Text = String.Format(m_ResourceManager.GetString("dlgDiapoExport_LabelFrequencyHundredth", Thread.CurrentThread.CurrentUICulture), iHundredth);
            }
            else
            {
                lblInfosFrequency.Text = String.Format(m_ResourceManager.GetString("dlgDiapoExport_LabelFrequencySeconds", Thread.CurrentThread.CurrentUICulture), fInterval);
            }
        }
        #endregion

        #region OK / Cancel handler
        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = m_ResourceManager.GetString("dlgSaveVideoTitle", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = m_ResourceManager.GetString("dlgSaveVideoFilterAlone", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.FilterIndex = 1;
            
            Hide();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    m_PlayerScreenUserInterface.SaveDiaporama(filePath, trkInterval.Value);
                }
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