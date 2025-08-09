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
using System.Collections.Generic;
using System.Globalization;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

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
            PreferenceTab.Capture_Trigger,
            PreferenceTab.Capture_ImageNaming, 
            PreferenceTab.Capture_VideoNaming, 
            PreferenceTab.Capture_Automation
        };
        
        // General
        private bool saveUncompressedVideo;
        private double displaySynchronizationFramerate;
        private string captureKVA;

        // Memory
        private int memoryBuffer;

        // Recording
        private CaptureRecordingMode recordingMode;
        private float replacementFramerateThreshold;
        private float replacementFramerate;
        private KVAExportFlags exportFlags = KVAExportFlags.DefaultCaptureRecording;

        // Trigger
        private bool enableAudioTrigger;
        private float audioTriggerThreshold;
        private float triggerQuietPeriod;
        private string audioInputDevice;
        private int audioTriggerHits = 0;
        private List<AudioInputDevice> audioInputDevices;
        private AudioInputLevelMonitor audioMonitor = new AudioInputLevelMonitor();
        private int thresholdFactor;
        private int decibelRange;
        private bool enableUDPTrigger;
        private int udpPort = 8875;
        private int udpTriggerHits = 0;
        private UDPMonitor udpMonitor = new UDPMonitor();
        private CaptureTriggerAction triggerAction = CaptureTriggerAction.RecordVideo;
        private bool defaultTriggerArmed = false;

        // Naming and formats
        private CapturePathConfiguration capturePathConfiguration = new CapturePathConfiguration();
        private Dictionary<CaptureVariable, TextBox> namingTextBoxes = new Dictionary<CaptureVariable, TextBox>();
        private FilenameHelper filenameHelper = new FilenameHelper();
        private FormPatterns formPatterns;
        private bool formPatternsVisible;

        // Automation
        private float recordingSeconds;
        private bool ignoreOverwriteWarning;
        private string postRecordCommand;

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
            InitTriggerMonitors();
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
            audioMonitor.Stop();
            audioMonitor.Dispose();
            udpMonitor.Stop();
            udpMonitor.Dispose();
        }

        private void ImportPreferences()
        {
            // General
            saveUncompressedVideo = PreferencesManager.CapturePreferences.SaveUncompressedVideo;
            displaySynchronizationFramerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            captureKVA = PreferencesManager.CapturePreferences.CaptureKVA;
            
            // Memory
            memoryBuffer = PreferencesManager.CapturePreferences.CaptureMemoryBuffer;
            
            // Recording
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
            replacementFramerateThreshold = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold;
            replacementFramerate = PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput;
            exportFlags = PreferencesManager.CapturePreferences.ExportFlags;

            // Trigger
            enableAudioTrigger = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger;
            audioInputDevice = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice;
            audioTriggerThreshold = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold;
            enableUDPTrigger = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableUDPTrigger;
            udpPort = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort;
            triggerQuietPeriod = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerQuietPeriod;
            triggerAction = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerAction;
            defaultTriggerArmed = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.DefaultTriggerArmed;

            // Naming and formats
            capturePathConfiguration = PreferencesManager.CapturePreferences.CapturePathConfiguration.Clone();
            
            // Automation
            recordingSeconds = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds;
            postRecordCommand = PreferencesManager.CapturePreferences.PostRecordCommand;
            ignoreOverwriteWarning = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.IgnoreOverwrite;
        }
        private void InitTriggerMonitors()
        {
            audioMonitor.Enabled = true;
            audioMonitor.Threshold = audioTriggerThreshold;
            audioMonitor.LevelChanged += AudioMonitor_LevelChanged;
            audioMonitor.Triggered += AudioMonitor_Triggered;

            udpMonitor.Enabled = true;
            udpMonitor.Port = udpPort;
            udpMonitor.Triggered += UDPMonitor_Triggered;
        }

        private void InitPage()
        {
            InitTabGeneral();
            InitTabMemory();
            InitTabRecording();
            InitTabTrigger();
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

            chkExportCalibration.Checked = (exportFlags & KVAExportFlags.Calibration) != 0;
            chkExportDrawings.Checked = (exportFlags & KVAExportFlags.Drawings) != 0;
        }    

        private void InitTabTrigger()
        {
            tabTrigger.Text = "Trigger";

            // Audio trigger
            chkEnableAudioTrigger.Text = RootLang.dlgPreferences_Capture_chkEnableAudioTrigger;
            chkEnableAudioTrigger.Checked = enableAudioTrigger;

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

            EnableDisableAudioTrigger();

            // UDP trigger
            chkEnableUDPTrigger.Text = "Enable UDP trigger";
            chkEnableUDPTrigger.Checked = enableUDPTrigger;
            
            lblUDPPort.Text = "UDP port:";
            nudUDPPort.Value = Math.Max(Math.Min(nudUDPPort.Maximum, udpPort), nudUDPPort.Minimum);
            NudHelper.FixNudScroll(nudUDPPort);

            EnableDisableUDPTrigger();

            // Common
            lblQuietPeriod.Text = RootLang.dlgPreferences_Capture_lblIdleTime;
            nudQuietPeriod.Value = (decimal)triggerQuietPeriod;
            NudHelper.FixNudScroll(nudQuietPeriod);

            lblTriggerAction.Text = RootLang.dlgPreferences_Capture_TriggerAction;
            cmbTriggerAction.Items.Add(ScreenManagerLang.ToolTip_StartRecording);
            cmbTriggerAction.Items.Add(ScreenManagerLang.Generic_SaveImage);
            cmbTriggerAction.SelectedIndex = ((int)triggerAction < cmbTriggerAction.Items.Count) ? (int)triggerAction : 0;

            lblDefaultTriggerState.Text = "Default trigger state:";
            cmbDefaultTriggerState.Items.Add("Armed");
            cmbDefaultTriggerState.Items.Add("Disarmed");
            cmbDefaultTriggerState.SelectedIndex = defaultTriggerArmed ? 0 : 1;
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
            
            lblRecordingTime.Text = RootLang.dlgPreferences_Capture_lblStopRecordingByDuration;
            nudRecordingTime.Value = (decimal)recordingSeconds;
            NudHelper.FixNudScroll(nudRecordingTime);

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

            string initialDirectory = null;
            if (Directory.Exists(tb.Text))
                initialDirectory = tb.Text;
            else
                initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string path = FilesystemHelper.OpenFolderBrowserDialog(initialDirectory);
            if (!string.IsNullOrEmpty(path))
                tb.Text = path;
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
        private void chkExcludeDrawings_CheckedChanged(object sender, EventArgs e)
        {
            bool exportDrawings = chkExportDrawings.Checked;
            exportFlags = exportDrawings ? exportFlags | KVAExportFlags.Drawings : exportFlags & ~KVAExportFlags.Drawings;
        }
        private void chkExcludeCalibration_CheckedChanged(object sender, EventArgs e)
        {
            bool exportCalibration = chkExportCalibration.Checked;
            exportFlags = exportCalibration ? exportFlags | KVAExportFlags.Calibration : exportFlags & ~KVAExportFlags.Calibration;
        }
        #endregion

        #region Tab Trigger
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
                    audioMonitor.Start(audioInputDevice);
            }
            else
            {
                audioMonitor.Stop();
            }

            audioTriggerHits = 0;
            UpdateHits();
        }
        private void NudAudioTriggerThreshold_ValueChanged(object sender, EventArgs e)
        {
            vumeter.ThresholdLinear = (float)nudAudioTriggerThreshold.Value / decibelRange;
            audioTriggerThreshold = vumeter.Threshold;
            audioMonitor.Threshold = audioTriggerThreshold;
            audioTriggerHits = 0;
            UpdateHits();
        }
        private void Vumeter_ThresholdChanged(object sender, EventArgs e)
        {
            audioTriggerThreshold = vumeter.Threshold;
            audioMonitor.Threshold = audioTriggerThreshold;
            nudAudioTriggerThreshold.Text = string.Format("{0:0.0}", vumeter.ThresholdLinear * decibelRange);
            audioTriggerHits = 0;
            UpdateHits();
        }
        private void chkEnableUDPTrigger_CheckedChanged(object sender, EventArgs e)
        {
            enableUDPTrigger = chkEnableUDPTrigger.Checked;
            EnableDisableUDPTrigger();
            udpTriggerHits = 0;
            UpdateHits();
        }
        private void nudUDPPort_ValueChanged(object sender, EventArgs e)
        {
            udpPort = (int)nudUDPPort.Value;
            if (enableUDPTrigger)
                udpMonitor.Start(udpPort);

            udpTriggerHits = 0;
            UpdateHits();
        }

        private void nudQuietPeriod_ValueChanged(object sender, EventArgs e)
        {
            triggerQuietPeriod = (float)nudQuietPeriod.Value;
        }
        private void cmbTriggerAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            triggerAction = (CaptureTriggerAction)cmbTriggerAction.SelectedIndex;
        }

        private void cmbDefaultTriggerState_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cmbDefaultTriggerState.SelectedIndex;
            defaultTriggerArmed = index == 0;
        }
        #endregion

        #region Tab Automation
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
        #endregion
        #endregion

        #region Trigger monitors
        private void AudioMonitor_Triggered(object sender, EventArgs e)
        {
            audioTriggerHits++;
            UpdateHits();
        }

        private void AudioMonitor_LevelChanged(object sender, float e)
        {
            int level = (int)(e * 100);
            vumeter.Amplitude = e;
        }
        
        private void EnableDisableAudioTrigger()
        {
            bool enabled = enableAudioTrigger && audioInputDevices != null && audioInputDevices.Count > 0;

            lblInputDevice.Enabled = enabled;
            lblAudioTriggerHits.Enabled = enabled;
            cmbInputDevice.Enabled = enabled;
            lblAudioTriggerThreshold.Enabled = enabled;
            nudAudioTriggerThreshold.Enabled = enabled;
            vumeter.Enabled = enabled;

            if (!enabled)
                audioMonitor.Stop();
            else
                audioMonitor.Start(audioInputDevice);
        }

        private void UDPMonitor_Triggered(object sender, EventArgs e)
        {
            udpTriggerHits++;
            UpdateHits();
        }

        private void EnableDisableUDPTrigger()
        {
            bool enabled = enableUDPTrigger;

            nudUDPPort.Enabled = enabled;
            lblUDPPort.Enabled = enabled;
            lblUDPTriggerHits.Enabled = enabled;

            if (!enabled)
                udpMonitor.Stop();
            else
                udpMonitor.Start(udpPort);
        }

        private void UpdateHits()
        {
            lblAudioTriggerHits.Text = audioTriggerHits.ToString();
            lblUDPTriggerHits.Text = udpTriggerHits.ToString();
        }
        #endregion

        public void CommitChanges()
        {
            // General
            PreferencesManager.CapturePreferences.SaveUncompressedVideo = saveUncompressedVideo;
            PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate = displaySynchronizationFramerate;
            PreferencesManager.CapturePreferences.CaptureKVA = captureKVA;

            // Memory
            PreferencesManager.CapturePreferences.CaptureMemoryBuffer = memoryBuffer;

            // Recording
            PreferencesManager.CapturePreferences.RecordingMode = recordingMode;
            PreferencesManager.CapturePreferences.HighspeedRecordingFramerateThreshold = replacementFramerateThreshold;
            PreferencesManager.CapturePreferences.HighspeedRecordingFramerateOutput = replacementFramerate;
            PreferencesManager.CapturePreferences.ExportFlags = exportFlags;

            // Trigger
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger = enableAudioTrigger;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioInputDevice = audioInputDevice;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.AudioTriggerThreshold = audioTriggerThreshold;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableUDPTrigger = enableUDPTrigger;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.UDPPort = udpPort;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerQuietPeriod = triggerQuietPeriod;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerAction = triggerAction;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.DefaultTriggerArmed = defaultTriggerArmed;

            // Naming and formats
            PreferencesManager.CapturePreferences.CapturePathConfiguration = capturePathConfiguration;

            // Automation
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds = recordingSeconds;
            PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.IgnoreOverwrite = ignoreOverwriteWarning;
            PreferencesManager.CapturePreferences.PostRecordCommand = postRecordCommand;
        }

        private void formPatterns_FormClosed(object sender, FormClosedEventArgs e)
        {
            formPatterns.FormClosed -= formPatterns_FormClosed;
            formPatternsVisible = false;
        }
    }
}
