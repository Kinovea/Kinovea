#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;

namespace Kinovea.Root
{
    /// <summary>
    /// PreferencePanelCapture.
    /// </summary>
    public partial class PreferencePanelCapture : UserControl, IPreferencePanel
    {
        #region IPreferencePanel properties
        public string Description
        {
            get { return description;}
        }
        public Bitmap Icon
        {
            get { return icon;}
        }
        #endregion
        
        #region Members
        private string description;
        private Bitmap icon;
        private string imageDirectory;
        private string videoDirectory;
        private KinoveaImageFormat imageFormat;
        private KinoveaVideoFormat videoFormat;
        private bool useCameraSignalSynchronization;
        private double displaySynchronizationFramerate;
        private bool usePattern;
        private string pattern;
        private bool resetCounter;
        private long counter;
        private int memoryBuffer;
        private FilenameHelper filenameHelper = new FilenameHelper();
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelCapture()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_btnCapture;
            icon = Resources.pref_capture;
            
            // Use the tag property of labels to store the actual marker.
            lblYear.Tag = "%y";
            lblMonth.Tag = "%mo";
            lblDay.Tag = "%d";
            lblHour.Tag = "%h";
            lblMinute.Tag = "%mi";
            lblSecond.Tag = "%s";
            lblCounter.Tag = "%i";
            
            ImportPreferences();
            InitPage();
        }
        private void ImportPreferences()
        {
            imageDirectory = PreferencesManager.CapturePreferences.ImageDirectory;
            videoDirectory = PreferencesManager.CapturePreferences.VideoDirectory;
            imageFormat = PreferencesManager.CapturePreferences.ImageFormat;
            videoFormat = PreferencesManager.CapturePreferences.VideoFormat;
            useCameraSignalSynchronization = PreferencesManager.CapturePreferences.UseCameraSignalSynchronization;
            displaySynchronizationFramerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            usePattern = PreferencesManager.CapturePreferences.CaptureUsePattern;
            pattern = PreferencesManager.CapturePreferences.Pattern;
            counter = PreferencesManager.CapturePreferences.CaptureImageCounter; // Use the image counter for sample.
            memoryBuffer = PreferencesManager.CapturePreferences.CaptureMemoryBuffer;
        }
        private void InitPage()
        {
            // General tab
            tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
            lblImageDirectory.Text = RootLang.dlgPreferences_Capture_lblImageDirectory;
            lblVideoDirectory.Text = RootLang.dlgPreferences_Capture_lblVideoDirectory;
            tbImageDirectory.Text = imageDirectory;
            tbVideoDirectory.Text = videoDirectory;
            
            lblImageFormat.Text = RootLang.dlgPreferences_Capture_lblImageFormat;
            cmbImageFormat.Items.Add("JPG");
            cmbImageFormat.Items.Add("PNG");
            cmbImageFormat.Items.Add("BMP");
            cmbImageFormat.SelectedIndex = ((int)imageFormat < cmbImageFormat.Items.Count) ? (int)imageFormat : 0;

            lblVideoFormat.Text = RootLang.dlgPreferences_Capture_lblVideoFormat;
            cmbVideoFormat.Items.Add("MP4");
            cmbVideoFormat.Items.Add("MKV");
            cmbVideoFormat.Items.Add("AVI");
            cmbVideoFormat.SelectedIndex = ((int)videoFormat < cmbVideoFormat.Items.Count) ? (int)videoFormat : 0;

            rbCameraFrameSignal.Checked = useCameraSignalSynchronization;
            rbForcedFramerate.Checked = !useCameraSignalSynchronization;
            tbFramerate.Text = string.Format("{0:0.###}", displaySynchronizationFramerate);

            // Naming tab
            tabNaming.Text = RootLang.dlgPreferences_Capture_tabNaming;
            rbFreeText.Text = RootLang.dlgPreferences_Capture_rbFreeText;
            rbPattern.Text = RootLang.dlgPreferences_Capture_rbPattern;
            lblYear.Text = RootLang.dlgPreferences_Capture_lblYear;
            lblMonth.Text = RootLang.dlgPreferences_Capture_lblMonth;
            lblDay.Text = RootLang.dlgPreferences_Capture_lblDay;
            lblHour.Text = RootLang.dlgPreferences_Capture_lblHour;
            lblMinute.Text = RootLang.dlgPreferences_Capture_lblMinute;
            lblSecond.Text = RootLang.dlgPreferences_Capture_lblSecond;
            lblCounter.Text = RootLang.dlgPreferences_Capture_lblCounter;
            btnResetCounter.Text = RootLang.dlgPreferences_Capture_btnResetCounter;
            
            tbPattern.Text = pattern;
            UpdateSample();
            
            rbPattern.Checked = usePattern;
            rbFreeText.Checked = !usePattern;
            
            // Memory tab
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();
        }
        #endregion
        
        #region Handlers
        
        #region Tab general
        private void btnBrowseImageLocation_Click(object sender, EventArgs e)
        {
            SelectSavingDirectory(tbImageDirectory);
        }
        private void btnBrowseVideoLocation_Click(object sender, EventArgs e)
        {
            SelectSavingDirectory(tbVideoDirectory);
        }
        private void SelectSavingDirectory(TextBox tb)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = ""; // TODO.
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;

            if(Directory.Exists(tb.Text))
                folderBrowserDialog.SelectedPath = tb.Text;
            
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                tb.Text = folderBrowserDialog.SelectedPath;
        }
        private void tbImageDirectory_TextChanged(object sender, EventArgs e)
        {
            if(!filenameHelper.ValidateFilename(tbImageDirectory.Text, true))
                ScreenManagerKernel.AlertInvalidFileName();
            else
                imageDirectory = tbImageDirectory.Text;
        }
        private void tbVideoDirectory_TextChanged(object sender, EventArgs e)
        {
            if(!filenameHelper.ValidateFilename(tbVideoDirectory.Text, true))
                ScreenManagerKernel.AlertInvalidFileName();
            else
                videoDirectory = tbVideoDirectory.Text;
        }
        private void cmbImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageFormat = (KinoveaImageFormat)cmbImageFormat.SelectedIndex;
        }
        private void cmbVideoFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            videoFormat = (KinoveaVideoFormat)cmbVideoFormat.SelectedIndex;
        }
        private void radioDSS_CheckedChanged(object sender, EventArgs e)
        {
            useCameraSignalSynchronization = rbCameraFrameSignal.Checked;

            lblFramerate.Enabled = !useCameraSignalSynchronization;
            tbFramerate.Enabled = !useCameraSignalSynchronization;
        }
        private void tbFramerate_TextChanged(object sender, EventArgs e)
        {
            // Parse in current culture.
            double value;
            bool parsed = double.TryParse(tbFramerate.Text, out value);
            if (parsed)
                displaySynchronizationFramerate = value;
        }
        #endregion
        
        #region Tab naming
        private void tbPattern_TextChanged(object sender, EventArgs e)
        {
            if(filenameHelper.ValidateFilename(tbPattern.Text, true))
                UpdateSample();
            else
                ScreenManager.ScreenManagerKernel.AlertInvalidFileName();
        }
        private void btnMarker_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;
            
            int selStart = tbPattern.SelectionStart;
            tbPattern.Text = tbPattern.Text.Insert(selStart, btn.Text);
            tbPattern.SelectionStart = selStart + btn.Text.Length;
        }
        private void lblMarker_Click(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null)
                return;
            
            string macro = lbl.Tag as string;
            if (macro == null)
                return;
            
            int selStart = tbPattern.SelectionStart;
            tbPattern.Text = tbPattern.Text.Insert(selStart, macro);
            tbPattern.SelectionStart = selStart + macro.Length;
        }
        private void btnResetCounter_Click(object sender, EventArgs e)
        {
            resetCounter = true;
            counter = 1;
            UpdateSample();
        }
        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            usePattern = rbPattern.Checked;
            EnableDisablePattern(usePattern);
        }
        #endregion
        
        #region Tab Memory
        private void trkMemoryBuffer_ValueChanged(object sender, EventArgs e)
        {
            memoryBuffer = trkMemoryBuffer.Value;
            UpdateMemoryLabel();
        }
        #endregion
        
        #endregion
        
        #region Private methods
        private void UpdateSample()
        {
            string sample = filenameHelper.ConvertPattern(tbPattern.Text, counter);
            lblSample.Text = sample;
            pattern = tbPattern.Text;
        }
        private void EnableDisablePattern(bool _bEnable)
        {
            tbPattern.Enabled = _bEnable;
            lblSample.Enabled = _bEnable;
            btnYear.Enabled = _bEnable;
            btnMonth.Enabled = _bEnable;
            btnDay.Enabled = _bEnable;
            btnHour.Enabled = _bEnable;
            btnMinute.Enabled = _bEnable;
            btnSecond.Enabled = _bEnable;
            btnIncrement.Enabled = _bEnable;
            btnResetCounter.Enabled = _bEnable;
            lblYear.Enabled = _bEnable;
            lblMonth.Enabled = _bEnable;
            lblDay.Enabled = _bEnable;
            lblHour.Enabled = _bEnable;
            lblMinute.Enabled = _bEnable;
            lblSecond.Enabled = _bEnable;
            lblCounter.Enabled = _bEnable;
        }
        private void UpdateMemoryLabel()
        {
            lblMemoryBuffer.Text = String.Format(RootLang.dlgPreferences_Capture_lblMemoryBuffer, trkMemoryBuffer.Value);
        }
        #endregion
        
        public void CommitChanges()
        {
            PreferencesManager.CapturePreferences.ImageDirectory = imageDirectory;
            PreferencesManager.CapturePreferences.VideoDirectory = videoDirectory;
            PreferencesManager.CapturePreferences.ImageFormat = imageFormat;
            PreferencesManager.CapturePreferences.VideoFormat = videoFormat;

            PreferencesManager.CapturePreferences.UseCameraSignalSynchronization = useCameraSignalSynchronization;
            PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate = displaySynchronizationFramerate;

            PreferencesManager.CapturePreferences.CaptureUsePattern = usePattern;
            PreferencesManager.CapturePreferences.Pattern = pattern;
            if(resetCounter)
            {
                PreferencesManager.CapturePreferences.CaptureImageCounter = 1;
                PreferencesManager.CapturePreferences.CaptureVideoCounter = 1;
            }
            
            PreferencesManager.CapturePreferences.CaptureMemoryBuffer = memoryBuffer;
        }

        

        
    }
}
