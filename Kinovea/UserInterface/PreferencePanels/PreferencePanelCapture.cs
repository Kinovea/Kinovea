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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;
using System.Collections.Generic;
using Microsoft.VisualBasic.Devices;

namespace Kinovea.Root
{
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
        public List<PreferenceTab> Tabs
        {
            get { return tabs; }
        }
        #endregion
        
        #region Members
        private string description;
        private Bitmap icon;
        private List<PreferenceTab> tabs = new List<PreferenceTab> { PreferenceTab.Capture_General, PreferenceTab.Capture_Memory, PreferenceTab.Capture_Recording, PreferenceTab.Capture_ImageNaming, PreferenceTab.Capture_VideoNaming};
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private Dictionary<CaptureVariable, TextBox> namingTextBoxes = new Dictionary<CaptureVariable, TextBox>();
        private double displaySynchronizationFramerate;
        private CaptureRecordingMode recordingMode;
        private bool saveUncompressedVideo;
        private int memoryBuffer;
        private FilenameHelper filenameHelper = new FilenameHelper();
        private FormPatterns formPatterns;
        private bool formPatternsVisible;
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelCapture()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabCapture;
            icon = Resources.pref_capture;
            
            ImportPreferences();
            InitPage();
        }

        public void OpenTab(PreferenceTab tab)
        {
            int index = tabs.IndexOf(tab);
            if (index < 0)
                return;

            tabSubPages.SelectedIndex = index;
        }

        private void ImportPreferences()
        {
            capturePathConfiguration = PreferencesManager.CapturePreferences.CapturePathConfiguration.Clone();
            displaySynchronizationFramerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
            saveUncompressedVideo = PreferencesManager.CapturePreferences.SaveUncompressedVideo;
            memoryBuffer = PreferencesManager.CapturePreferences.CaptureMemoryBuffer;
        }
        private void InitPage()
        {
            InitPageGeneral();
            InitPageMemory();
            InitPageRecording();
            InitPageImageNaming();
            InitPageVideoNaming();
            InitNamingTextBoxes();
        }

        private void InitPageGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;

            lblImageFormat.Text = RootLang.dlgPreferences_Capture_lblImageFormat;
            cmbImageFormat.Items.Add("JPG");
            cmbImageFormat.Items.Add("PNG");
            cmbImageFormat.Items.Add("BMP");
            int imageFormat = (int)capturePathConfiguration.ImageFormat;
            cmbImageFormat.SelectedIndex = ((int)imageFormat < cmbImageFormat.Items.Count) ? (int)imageFormat : 0;

            lblVideoFormat.Text = RootLang.dlgPreferences_Capture_lblVideoFormat;
            cmbVideoFormat.Items.Add("MP4");
            cmbVideoFormat.Items.Add("MKV");
            cmbVideoFormat.Items.Add("AVI");
            int videoFormat = (int)capturePathConfiguration.VideoFormat;
            cmbVideoFormat.SelectedIndex = ((int)videoFormat < cmbVideoFormat.Items.Count) ? (int)videoFormat : 0;

            lblUncompressedVideoFormat.Text = "Uncompressed video format:"; //RootLang.dlgPreferences_Capture_lblVideoFormat;
            cmbUncompressedVideoFormat.Items.Add("MKV");
            cmbUncompressedVideoFormat.Items.Add("AVI");
            int uncompressedVideoFormat = (int)capturePathConfiguration.UncompressedVideoFormat;
            cmbUncompressedVideoFormat.SelectedIndex = ((int)uncompressedVideoFormat < cmbUncompressedVideoFormat.Items.Count) ? (int)uncompressedVideoFormat : 0;

            lblFramerate.Text = RootLang.dlgPreferences_Capture_lblForcedFramerate;
            tbFramerate.Text = string.Format("{0:0.###}", displaySynchronizationFramerate);
        }

        private void InitPageMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            // Max allocation of memory based on bitness and physical memory.
            ComputerInfo ci = new ComputerInfo();
            ulong megabytes = 1024 * 1024;
            int maxMemory = (int)(ci.TotalPhysicalMemory / megabytes);
            int thresholdLargeMemory = 3072;
            int reserve = 2048;

            if (Software.Is32bit || maxMemory < thresholdLargeMemory)
                trkMemoryBuffer.Maximum = 1024;
            else
                trkMemoryBuffer.Maximum = maxMemory - reserve;

            memoryBuffer = Math.Min(memoryBuffer, trkMemoryBuffer.Maximum);
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();
        }

        private void InitPageRecording()
        {
            tabRecording.Text = RootLang.dlgPreferences_Capture_Recording;

            grpRecordingMode.Text = RootLang.dlgPreferences_Capture_RecordingMode;
            rbRecordingCamera.Text = RootLang.dlgPreferences_Capture_RecordingMode_Camera;
            rbRecordingDisplay.Text = RootLang.dlgPreferences_Capture_RecordingMode_Display;
            chkUncompressedVideo.Text = "Record without compression"; 

            rbRecordingCamera.Checked = recordingMode == CaptureRecordingMode.Camera;
            rbRecordingDisplay.Checked = recordingMode != CaptureRecordingMode.Camera;
            chkUncompressedVideo.Checked = saveUncompressedVideo;
        }

        private void InitPageImageNaming()
        {
            tabImageNaming.Text = RootLang.dlgPreferences_Capture_ImageNaming;

            grpLeftImage.Text = RootLang.dlgPreferences_Capture_Left;
            grpRightImage.Text = RootLang.dlgPreferences_Capture_Right;

            lblLeftImageRoot.Text = RootLang.dlgPreferences_Capture_Root;
            lblLeftImageSubdir.Text = RootLang.dlgPreferences_Capture_Subdir;
            lblLeftImageFile.Text = RootLang.dlgPreferences_Capture_File;
            lblRightImageRoot.Text = RootLang.dlgPreferences_Capture_Root;
            lblRightImageSubdir.Text = RootLang.dlgPreferences_Capture_Subdir;
            lblRightImageFile.Text = RootLang.dlgPreferences_Capture_File;
            
            tbLeftImageRoot.Text = capturePathConfiguration.LeftImageRoot;
            tbLeftImageSubdir.Text = capturePathConfiguration.LeftImageSubdir;
            tbLeftImageFile.Text = capturePathConfiguration.LeftImageFile;
            tbRightImageRoot.Text = capturePathConfiguration.RightImageRoot;
            tbRightImageSubdir.Text = capturePathConfiguration.RightImageSubdir;
            tbRightImageFile.Text = capturePathConfiguration.RightImageFile;
        }

        private void InitPageVideoNaming()
        {
            tabVideoNaming.Text = RootLang.dlgPreferences_Capture_VideoNaming;

            grpLeftVideo.Text = RootLang.dlgPreferences_Capture_Left;
            grpRightVideo.Text = RootLang.dlgPreferences_Capture_Right;

            lblLeftVideoRoot.Text = RootLang.dlgPreferences_Capture_Root;
            lblLeftVideoSubdir.Text = RootLang.dlgPreferences_Capture_Subdir;
            lblLeftVideoFile.Text = RootLang.dlgPreferences_Capture_File;
            lblRightVideoRoot.Text = RootLang.dlgPreferences_Capture_Root;
            lblRightVideoSubdir.Text = RootLang.dlgPreferences_Capture_Subdir;
            lblRightVideoFile.Text = RootLang.dlgPreferences_Capture_File;
            
            tbLeftVideoRoot.Text = capturePathConfiguration.LeftVideoRoot;
            tbLeftVideoSubdir.Text = capturePathConfiguration.LeftVideoSubdir;
            tbLeftVideoFile.Text = capturePathConfiguration.LeftVideoFile;
            tbRightVideoRoot.Text = capturePathConfiguration.RightVideoRoot;
            tbRightVideoSubdir.Text = capturePathConfiguration.RightVideoSubdir;
            tbRightVideoFile.Text = capturePathConfiguration.RightVideoFile;
        }

        private void InitNamingTextBoxes()
        {
            namingTextBoxes[CaptureVariable.LeftImageRoot] = tbLeftImageRoot;
            namingTextBoxes[CaptureVariable.LeftImageSubdir] = tbLeftImageSubdir;
            namingTextBoxes[CaptureVariable.LeftImageFile] = tbLeftImageFile;
            namingTextBoxes[CaptureVariable.RightImageRoot] = tbRightImageRoot;
            namingTextBoxes[CaptureVariable.RightImageSubdir] = tbRightImageSubdir;
            namingTextBoxes[CaptureVariable.RightImageFile] = tbRightImageFile;
            namingTextBoxes[CaptureVariable.LeftVideoRoot] = tbLeftVideoRoot;
            namingTextBoxes[CaptureVariable.LeftVideoSubdir] = tbLeftVideoSubdir;
            namingTextBoxes[CaptureVariable.LeftVideoFile] = tbLeftVideoFile;
            namingTextBoxes[CaptureVariable.RightVideoRoot] = tbRightVideoRoot;
            namingTextBoxes[CaptureVariable.RightVideoSubdir] = tbRightVideoSubdir;
            namingTextBoxes[CaptureVariable.RightVideoFile] = tbRightVideoFile;

            foreach (CaptureVariable v in namingTextBoxes.Keys)
            {
                namingTextBoxes[v].TextChanged += tbNamingVariable_TextChanged;
                namingTextBoxes[v].Tag = v;
            }

            btnLeftImageRoot.Tag = CaptureVariable.LeftImageRoot;
            btnRightImageRoot.Tag = CaptureVariable.RightImageRoot;
            btnLeftVideoRoot.Tag = CaptureVariable.LeftVideoRoot;
            btnRightVideoRoot.Tag = CaptureVariable.RightVideoRoot;
        }
        #endregion
        
        #region Handlers
        
        #region Tab general
        private void cmbImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            capturePathConfiguration.ImageFormat = (KinoveaImageFormat)cmbImageFormat.SelectedIndex;
        }
        private void cmbVideoFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            capturePathConfiguration.VideoFormat = (KinoveaVideoFormat)cmbVideoFormat.SelectedIndex;
        }
        private void cmbUncompressedVideoFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            capturePathConfiguration.UncompressedVideoFormat = (KinoveaUncompressedVideoFormat)cmbUncompressedVideoFormat.SelectedIndex;
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
        
        #region Tabs naming
        private void tbNamingVariable_TextChanged(object sender, EventArgs e)
        {
            Control tb = sender as Control;
            if (tb == null)
                return;

            CaptureVariable v = (CaptureVariable)tb.Tag;
            WriteVariable(v, tb.Text);
        }

        private void WriteVariable(CaptureVariable v, string text)
        {
            switch (v)
            {
                case CaptureVariable.LeftImageRoot:
                    capturePathConfiguration.LeftImageRoot = text;
                    break;
                case CaptureVariable.LeftImageSubdir:
                    capturePathConfiguration.LeftImageSubdir = text;
                    break;
                case CaptureVariable.LeftImageFile:
                    capturePathConfiguration.LeftImageFile = text;
                    break;
                case CaptureVariable.RightImageRoot:
                    capturePathConfiguration.RightImageRoot = text;
                    break;
                case CaptureVariable.RightImageSubdir:
                    capturePathConfiguration.RightImageSubdir = text;
                    break;
                case CaptureVariable.RightImageFile:
                    capturePathConfiguration.RightImageFile = text;
                    break;
                case CaptureVariable.LeftVideoRoot:
                    capturePathConfiguration.LeftVideoRoot = text;
                    break;
                case CaptureVariable.LeftVideoSubdir:
                    capturePathConfiguration.LeftVideoSubdir = text;
                    break;
                case CaptureVariable.LeftVideoFile:
                    capturePathConfiguration.LeftVideoFile = text;
                    break;
                case CaptureVariable.RightVideoRoot:
                    capturePathConfiguration.RightVideoRoot = text;
                    break;
                case CaptureVariable.RightVideoSubdir:
                    capturePathConfiguration.RightVideoSubdir = text;
                    break;
                case CaptureVariable.RightVideoFile:
                    capturePathConfiguration.RightVideoFile = text;
                    break;
            }
        }

        private void btnFolderSelection_Click(object sender, EventArgs e)
        {
            Control button = sender as Control;
            CaptureVariable captureVariable = (CaptureVariable)button.Tag;
            if (!namingTextBoxes.ContainsKey(captureVariable))
                return;

            TextBox tb = namingTextBoxes[captureVariable];

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "";
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;

            if (Directory.Exists(tb.Text))
                folderBrowserDialog.SelectedPath = tb.Text;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                tb.Text = folderBrowserDialog.SelectedPath;
        }
        #endregion
        
        #region Tab Memory
        private void trkMemoryBuffer_ValueChanged(object sender, EventArgs e)
        {
            memoryBuffer = trkMemoryBuffer.Value;
            UpdateMemoryLabel();
        }

        private void UpdateMemoryLabel()
        {
            lblMemoryBuffer.Text = String.Format(RootLang.dlgPreferences_Capture_lblMemoryBuffer, trkMemoryBuffer.Value);
        }
        #endregion

        #region Tab Recording
        private void radioRecordingMode_CheckedChanged(object sender, EventArgs e)
        {
            recordingMode = rbRecordingCamera.Checked ? CaptureRecordingMode.Camera : CaptureRecordingMode.Display;
            chkUncompressedVideo.Enabled = recordingMode == CaptureRecordingMode.Camera;
        }
        private void chkUncompressedVideo_CheckedChanged(object sender, EventArgs e)
        {
            saveUncompressedVideo = chkUncompressedVideo.Checked;
        }
        #endregion
        #endregion

        public void CommitChanges()
        {
            PreferencesManager.CapturePreferences.CapturePathConfiguration = capturePathConfiguration;
            PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate = displaySynchronizationFramerate;
            PreferencesManager.CapturePreferences.CaptureMemoryBuffer = memoryBuffer;
            PreferencesManager.CapturePreferences.RecordingMode = recordingMode;
            PreferencesManager.CapturePreferences.SaveUncompressedVideo = saveUncompressedVideo;
        }

        private void btnMacroReference_Click(object sender, EventArgs e)
        {
            if (formPatternsVisible)
                return;
            
            formPatterns = new FormPatterns();
            formPatterns.FormClosed += formPatterns_FormClosed;
            formPatternsVisible = true;
            formPatterns.Show(this);
        }

        private void formPatterns_FormClosed(object sender, FormClosedEventArgs e)
        {
            formPatterns.FormClosed -= formPatterns_FormClosed;
            formPatternsVisible = false;
        }
    }
}
