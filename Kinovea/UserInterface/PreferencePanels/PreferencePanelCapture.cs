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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;
using Kinovea.ScreenManager.Languages;

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
        private List<PreferenceTab> tabs = new List<PreferenceTab> { 
            PreferenceTab.Capture_General, 
            PreferenceTab.Capture_Memory, 
            PreferenceTab.Capture_Recording, 
            PreferenceTab.Capture_ImageNaming, 
            PreferenceTab.Capture_VideoNaming, 
            PreferenceTab.Capture_Automation
        };
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private Dictionary<CaptureVariable, TextBox> namingTextBoxes = new Dictionary<CaptureVariable, TextBox>();
        private double displaySynchronizationFramerate;
        private CaptureRecordingMode recordingMode;
        private bool saveUncompressedVideo;
        private int memoryBuffer;
        private FilenameHelper filenameHelper = new FilenameHelper();
        private FormPatterns formPatterns;
        private bool formPatternsVisible;
        private bool enableAudioTrigger;
        private float audioTriggerThreshold;
        private float audioQuietPeriod;
        private AudioTriggerAction triggerAction;
        private float recordingSeconds;
        private bool ignoreOverwriteWarning;
        private string audioInputDevice;
        private int audioTriggerHits = 0;
        private List<AudioInputDevice> audioInputDevices;
        private AudioInputLevelMonitor inputMonitor = new AudioInputLevelMonitor();
        private int thresholdFactor;
        private int decibelRange;
        private string postRecordCommand;
        private float replacementFramerateThreshold;
        private float replacementFramerate;
        private string captureKVA;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction & Initialization
        public PreferencePanelCapture()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabCapture;
            icon = Resources.pref_capture;

            // The audio amplitude is coming as [0..1] and is remapped to a logarithmic vumeter.
            // The value we show to the user has an arbitrary unit.
            // Since we use a decibel range of 60, it maps to a power ratio of 1000:1 between the highest and lowest amplitude,
            // thus quantizing this to 1000 steps seems as good as anything else.
            decibelRange = 60;
            thresholdFactor = (int)Math.Pow(10, decibelRange / 20);

            ImportPreferences();
            InitInputMonitor();
            InitPage();
        }

        public void OpenTab(PreferenceTab tab)
        {
            int index = tabs.IndexOf(tab);
            if (index < 0)
                return;

            tabSubPages.SelectedIndex = index;
        }

        public void Close()
        {
            inputMonitor.Stop();
            inputMonitor.Dispose();
        }

        private void ImportPreferences()
        {
            saveUncompressedVideo = PreferencesManager.CapturePreferences.SaveUncompressedVideo;
            displaySynchronizationFramerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            capturePathConfiguration = PreferencesManager.CapturePreferences.CapturePathConfiguration.Clone();
            captureKVA = PreferencesManager.CapturePreferences.CaptureKVA;
            memoryBuffer = PreferencesManager.CapturePreferences.CaptureMemoryBuffer;
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
            replacementFramerateThreshold = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold;
            replacementFramerate = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput;
            enableAudioTrigger = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputDevice = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
            audioTriggerThreshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;
            audioQuietPeriod = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioQuietPeriod;
            triggerAction = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerAction;
            recordingSeconds = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds;
            postRecordCommand = PreferencesManager.CapturePreferences.PostRecordCommand;
            ignoreOverwriteWarning = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.IgnoreOverwrite;
        }
        private void InitInputMonitor()
        {
            inputMonitor.Enabled = true;
            inputMonitor.Threshold = audioTriggerThreshold;
            inputMonitor.LevelChanged += InputMonitor_LevelChanged;
            inputMonitor.ThresholdPassed += InputMonitor_ThresholdPassed;
        }

        private void InitPage()
        {
            InitTabGeneral();
            InitTabMemory();
            InitTabRecording();
            InitTabImageNaming();
            InitTabVideoNaming();
            InitTabAutomation();

            InitNamingTextBoxes();
        }

        private void InitTabGeneral()
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

            lblUncompressedVideoFormat.Text = RootLang.dlgPreferences_Capture_lblUncompressedVideoFormat;
            cmbUncompressedVideoFormat.Items.Add("MKV");
            cmbUncompressedVideoFormat.Items.Add("AVI");
            int uncompressedVideoFormat = (int)capturePathConfiguration.UncompressedVideoFormat;
            cmbUncompressedVideoFormat.SelectedIndex = ((int)uncompressedVideoFormat < cmbUncompressedVideoFormat.Items.Count) ? (int)uncompressedVideoFormat : 0;

            lblFramerate.Text = RootLang.dlgPreferences_Capture_lblForcedFramerate;
            tbFramerate.Text = string.Format("{0:0.###}", displaySynchronizationFramerate);
            lblCaptureKVA.Text = RootLang.dlgPreferences_Player_DefaultKVA;
            tbCaptureKVA.Text = captureKVA;
        }

        private void InitTabMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            int maxMemoryBuffer = MemoryHelper.MaxMemoryBuffer();
            trkMemoryBuffer.Maximum = maxMemoryBuffer;

            memoryBuffer = Math.Min(memoryBuffer, trkMemoryBuffer.Maximum);
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();
        }

        private void InitTabRecording()
        {
            tabRecording.Text = RootLang.dlgPreferences_Capture_Recording;

            grpRecordingMode.Text = RootLang.dlgPreferences_Capture_RecordingMode;
            rbRecordingCamera.Text = RootLang.dlgPreferences_Capture_RecordingMode_Camera;
            rbRecordingDelayed.Text = RootLang.dlgPreferences_Capture_RecordingMode_Display;
            rbRecordingScheduled.Text = RootLang.dlgPreferences_Capture_RecordingMode_Scheduled; 
            chkUncompressedVideo.Text = RootLang.dlgPreferences_Capture_chkUncompressedVideo;

            rbRecordingCamera.Checked = recordingMode == CaptureRecordingMode.Camera;
            rbRecordingDelayed.Checked = recordingMode == CaptureRecordingMode.Delay;
            rbRecordingScheduled.Checked = recordingMode == CaptureRecordingMode.Scheduled;
            chkUncompressedVideo.Checked = saveUncompressedVideo;

            chkIgnoreOverwriteWarning.Text = RootLang.dlgPreferences_Capture_chkIgnoreOverwrite;
            chkIgnoreOverwriteWarning.Checked = ignoreOverwriteWarning;

            gbHighspeedCameras.Text = RootLang.dlgPreferences_Capture_gbHighspeedCameras;
            lblReplacementThreshold.Text = RootLang.dlgPreferences_Capture_lblReplacementThreshold;
            lblReplacementFramerate.Text = RootLang.dlgPreferences_Capture_lblReplacementValue;
            nudReplacementThreshold.Value = (decimal)replacementFramerateThreshold;
            nudReplacementFramerate.Value = (decimal)replacementFramerate;
            NudHelper.FixNudScroll(nudReplacementThreshold);
            NudHelper.FixNudScroll(nudReplacementFramerate);
            // Tooltip: Starting at this capture framerate, videos will be created with the replacement framerate in their metadata.
        }

        private void InitTabImageNaming()
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

        private void InitTabVideoNaming()
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

        private void InitTabAutomation()
        {
            tabAutomation.Text = RootLang.dlgPreferences_Capture_tabAutomation; 
            chkEnableAudioTrigger.Text = RootLang.dlgPreferences_Capture_chkEnableAudioTrigger;
            lblInputDevice.Text = RootLang.dlgPreferences_Capture_lblInputDevice;

            audioInputDevices = AudioInputLevelMonitor.GetDevices();
            if (audioInputDevices.Count > 0)
            {
                int preferredIndex = -1;
                for (int i = 0; i < audioInputDevices.Count; i++)
                {
                    cmbInputDevice.Items.Add(audioInputDevices[i]);

                    var wic = audioInputDevices[i].WaveInCapabilities;
                    log.DebugFormat("{0}: ProductGuid:{1}, ProductName:{2}", i, wic.ProductGuid, wic.ProductName);

                    if (wic.ProductName == audioInputDevice)
                        preferredIndex = i;
                }

                if (preferredIndex >= 0)
                    cmbInputDevice.SelectedIndex = preferredIndex;
                else
                    cmbInputDevice.SelectedIndex = 0;
            }

            lblAudioTriggerThreshold.Text = RootLang.dlgPreferences_Capture_lblTriggerThreshold;
            vumeter.Threshold = audioTriggerThreshold;
            vumeter.DecibelRange = decibelRange;
            nudAudioTriggerThreshold.Value = (decimal)vumeter.ThresholdLinear * decibelRange;
            nudAudioTriggerThreshold.Maximum = decibelRange;
            NudHelper.FixNudScroll(nudAudioTriggerThreshold);

            lblQuietPeriod.Text = RootLang.dlgPreferences_Capture_lblIdleTime;
            nudQuietPeriod.Value = (decimal)audioQuietPeriod;
            NudHelper.FixNudScroll(nudQuietPeriod);

            lblTriggerAction.Text = "Trigger action:";
            cmbTriggerAction.Items.Add(ScreenManagerLang.ToolTip_StartRecording);
            cmbTriggerAction.Items.Add(ScreenManagerLang.Generic_SaveImage);
            cmbTriggerAction.SelectedIndex = ((int)triggerAction < cmbTriggerAction.Items.Count) ? (int)triggerAction : 0;

            lblRecordingTime.Text = RootLang.dlgPreferences_Capture_lblStopRecordingByDuration;
            nudRecordingTime.Value = (decimal)recordingSeconds;
            NudHelper.FixNudScroll(nudRecordingTime);

            chkEnableAudioTrigger.Checked = enableAudioTrigger;
            EnableDisableAudioTrigger();

            lblPostRecordCommand.Text = RootLang.dlgPreferences_Capture_lblPostRecordingCommand; 
            tbPostRecordCommand.Text = postRecordCommand;
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

        private void tbCaptureKVA_TextChanged(object sender, EventArgs e)
        {
            captureKVA = tbCaptureKVA.Text;
        }

        private void btnCaptureKVA_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            string initialDirectory = "";
            if (!string.IsNullOrEmpty(captureKVA) && File.Exists(captureKVA) && Path.IsPathRooted(captureKVA))
                initialDirectory = Path.GetDirectoryName(captureKVA);

            if (!string.IsNullOrEmpty(initialDirectory))
                dialog.InitialDirectory = initialDirectory;
            else
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            dialog.Title = ScreenManagerLang.dlgLoadAnalysis_Title;
            dialog.RestoreDirectory = true;
            dialog.Filter = FilesystemHelper.OpenKVAFilter(ScreenManagerLang.FileFilter_AllSupported);
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == DialogResult.OK)
                tbCaptureKVA.Text = dialog.FileName;
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

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (Directory.Exists(tb.Text))
                dialog.InitialDirectory = tb.Text;
            else
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                tb.Text = dialog.FileName;
        }

        private void btnMacroReference_Click(object sender, EventArgs e)
        {
            if (formPatternsVisible)
                return;

            formPatterns = new FormPatterns(PatternSymbolsFile.Symbols);
            formPatterns.FormClosed += formPatterns_FormClosed;
            formPatternsVisible = true;
            formPatterns.Show(this);
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
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            string formatted = memoryBuffer.ToString("#,0", nfi);

            lblMemoryBuffer.Text = String.Format(RootLang.dlgPreferences_Capture_lblMemoryBuffer, formatted);
        }
        #endregion

        #region Tab Recording
        private void radioRecordingMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbRecordingCamera.Checked)
                recordingMode = CaptureRecordingMode.Camera;
            else if (rbRecordingDelayed.Checked)
                recordingMode = CaptureRecordingMode.Delay;
            else
                recordingMode = CaptureRecordingMode.Scheduled;
        }
        private void chkUncompressedVideo_CheckedChanged(object sender, EventArgs e)
        {
            saveUncompressedVideo = chkUncompressedVideo.Checked;
        }
        private void NudReplacementThreshold_ValueChanged(object sender, EventArgs e)
        {
            replacementFramerateThreshold = (float)nudReplacementThreshold.Value;
        }
        private void NudReplacementFramerate_ValueChanged(object sender, EventArgs e)
        {
            replacementFramerate = (float)nudReplacementFramerate.Value;
        }
        #endregion

        #region Tab Automation
        private void chkEnableAudioTrigger_CheckedChanged(object sender, EventArgs e)
        {
            enableAudioTrigger = chkEnableAudioTrigger.Checked;
            EnableDisableAudioTrigger();
            audioTriggerHits = 0;
            UpdateHits();
        }
        private void cmbInputDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            AudioInputDevice selected = cmbInputDevice.SelectedItem as AudioInputDevice;
            if (selected != null)
            {
                audioInputDevice = selected.WaveInCapabilities.ProductName;
                
                if (enableAudioTrigger)
                    inputMonitor.Start(audioInputDevice);
            }
            else
            {
                inputMonitor.Stop();
            }

            audioTriggerHits = 0;
            UpdateHits();
        }
        private void NudAudioTriggerThreshold_ValueChanged(object sender, EventArgs e)
        {
            vumeter.ThresholdLinear = (float)nudAudioTriggerThreshold.Value / decibelRange;
            audioTriggerThreshold = vumeter.Threshold;
            inputMonitor.Threshold = audioTriggerThreshold;
            audioTriggerHits = 0;
            UpdateHits();
        }
        private void Vumeter_ThresholdChanged(object sender, EventArgs e)
        {
            audioTriggerThreshold = vumeter.Threshold;
            inputMonitor.Threshold = audioTriggerThreshold;
            nudAudioTriggerThreshold.Text = string.Format("{0:0.0}", vumeter.ThresholdLinear * decibelRange);
            audioTriggerHits = 0;
            UpdateHits();
        }

        private void nudQuietPeriod_ValueChanged(object sender, EventArgs e)
        {
            audioQuietPeriod = (float)nudQuietPeriod.Value;
        }

        private void NudRecordingTime_ValueChanged(object sender, EventArgs e)
        {
            recordingSeconds = (float)nudRecordingTime.Value;
        }

        private void tbPostRecordCommand_TextChanged(object sender, EventArgs e)
        {
            Control tb = sender as Control;
            if (tb == null)
                return;

            // No validation whatsoever. The user is responsible for not messing this up.
            postRecordCommand = tb.Text;
        }

        private void btnPostRecordCommand_Click(object sender, EventArgs e)
        {
            if (formPatternsVisible)
                return;

            formPatterns = new FormPatterns(PatternSymbolsCommand.Symbols);
            formPatterns.FormClosed += formPatterns_FormClosed;
            formPatternsVisible = true;
            formPatterns.Show(this);
        }

        private void chkIgnoreOverwriteWarning_CheckedChanged(object sender, EventArgs e)
        {
            ignoreOverwriteWarning = chkIgnoreOverwriteWarning.Checked;
        }

        private void cmbTriggerAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            triggerAction = (AudioTriggerAction)cmbTriggerAction.SelectedIndex;
        }

        #endregion
        #endregion

        #region Audio monitor event handlers
        private void InputMonitor_ThresholdPassed(object sender, EventArgs e)
        {
            audioTriggerHits++;
            UpdateHits();
        }

        private void InputMonitor_LevelChanged(object sender, float e)
        {
            int level = (int)(e * 100);
            vumeter.Amplitude = e;
        }

        #endregion
        private void EnableDisableAudioTrigger()
        {
            bool enabled = enableAudioTrigger && audioInputDevices != null && audioInputDevices.Count > 0;

            lblInputDevice.Enabled = enabled;
            lblAudioTriggerHits.Enabled = enabled;
            cmbInputDevice.Enabled = enabled;
            lblAudioTriggerThreshold.Enabled = enabled;
            nudAudioTriggerThreshold.Enabled = enabled;
            vumeter.Enabled = enabled;
            lblQuietPeriod.Enabled = enabled;
            nudQuietPeriod.Enabled = enabled;

            if (!enabled)
                inputMonitor.Stop();
            else
                inputMonitor.Start(audioInputDevice);
        }

        private void UpdateHits()
        {
            lblAudioTriggerHits.Text = audioTriggerHits.ToString();
        }

        public void CommitChanges()
        {
            PreferencesManager.CapturePreferences.SaveUncompressedVideo = saveUncompressedVideo;
            PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate = displaySynchronizationFramerate;
            PreferencesManager.CapturePreferences.CapturePathConfiguration = capturePathConfiguration;
            PreferencesManager.CapturePreferences.CaptureMemoryBuffer = memoryBuffer;
            PreferencesManager.CapturePreferences.RecordingMode = recordingMode;
            PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold = replacementFramerateThreshold;
            PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput = replacementFramerate;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger = enableAudioTrigger;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice = audioInputDevice;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold = audioTriggerThreshold;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioQuietPeriod = audioQuietPeriod;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerAction = triggerAction;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds = recordingSeconds;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.IgnoreOverwrite = ignoreOverwriteWarning;
            PreferencesManager.CapturePreferences.PostRecordCommand = postRecordCommand;
            PreferencesManager.CapturePreferences.CaptureKVA = captureKVA;
        }

        private void formPatterns_FormClosed(object sender, FormClosedEventArgs e)
        {
            formPatterns.FormClosed -= formPatterns_FormClosed;
            formPatternsVisible = false;
        }
    }
}
