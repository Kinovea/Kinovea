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
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Kinovea.Root
{
    /// <summary>
    /// PreferencePanelPlayer.
    /// </summary>
    public partial class PreferencePanelPlayer : UserControl, IPreferencePanel
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
            PreferenceTab.Player_General, 
            PreferenceTab.Player_Memory,
            PreferenceTab.Player_Image
        };

        // General
        private bool detectImageSequences;
        private string playbackKVA;

        // Memory
        private int memoryMegabytes;
        private bool showCacheInTimeline;

        // Player
        private bool enableHardwareDecoding;
        private bool enablePreviewScaling;
        private bool enableHardwareScaling;
        private bool enableFrameSkipping;
        private bool loopPlayback;
        private bool showFramerateInSpeedLabel;
        private bool interactiveFrameTracker;
        private bool syncLockSpeeds;
        private bool syncByMotion;

        // Jumping
        private float smallJumpSize;
        private TimelineJumpUnit smallJumpUnit;
        private float largeJumpSize;
        private TimelineJumpUnit largeJumpUnit;

        private List<HotkeyCommand> hotkeys;
        private string category = "PlayerScreen";
        private string selectedCommand;

        // Image
        private bool enablePixelFiltering;
        private ImageAspectRatio imageAspectRatio;
        private bool deinterlaceByDefault;
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelPlayer()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabPlayback;
            icon = Resources.circled_play_button_30;
            
            ImportPreferences();
            InitPages();
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
        }

        private void ImportPreferences()
        {
            // General
            detectImageSequences = PreferencesManager.PlayerPreferences.DetectImageSequences;
            playbackKVA = PreferencesManager.PlayerPreferences.PlaybackKVA;
            
            // Memory
            memoryMegabytes = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
            showCacheInTimeline = PreferencesManager.PlayerPreferences.ShowCacheInTimeline;
            
            // Player
            enableHardwareDecoding = PreferencesManager.PlayerPreferences.EnableHardwareDecoding;
            enablePreviewScaling = PreferencesManager.PlayerPreferences.EnablePreviewScaling;
            enableHardwareScaling = PreferencesManager.PlayerPreferences.EnableHardwareScaling;
            enableFrameSkipping = PreferencesManager.PlayerPreferences.EnableFrameSkipping;
            loopPlayback = PreferencesManager.PlayerPreferences.LoopPlayback;
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            showFramerateInSpeedLabel = PreferencesManager.PlayerPreferences.SpeedLabelFramerate;
            syncByMotion = PreferencesManager.PlayerPreferences.SyncByMotion;
            syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;

            // Time jump
            smallJumpSize = PreferencesManager.PlayerPreferences.TimelineJumpSmallSize;
            smallJumpUnit = PreferencesManager.PlayerPreferences.TimelineJumpSmallUnit;
            largeJumpSize = PreferencesManager.PlayerPreferences.TimelineJumpLargeSize;
            largeJumpUnit = PreferencesManager.PlayerPreferences.TimelineJumpLargeUnit;
            
            // Image
            enablePixelFiltering = PreferencesManager.PlayerPreferences.EnablePixelFiltering;
            imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;
            deinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
        }
        private void InitPages()
        {
            InitPageGeneral();
            InitPageMemory();
            InitPagePlayer();
            InitPageJumping();
            InitPageImage();
        }

        private void InitPageGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;
            chkDetectImageSequences.Text = RootLang.dlgPreferences_Player_ImportImageSequences;
            chkDetectImageSequences.Checked = detectImageSequences;
            
            lblPlaybackKVA.Text = RootLang.dlgPreferences_Player_DefaultKVA;
            tbPlaybackKVA.Text = playbackKVA;
        }

        private void InitPageMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            lblCacheMemory.Text = "Total playback cache memory (GB):";
            lblCacheMemoryDescription.Text = "Memory reserved for playback cache." +
            " Shared equally when two playback screens are open.";

            // Cap at 50% of physical memory.
            int total = MemoryHelper.TotalPhysicalMemory();
            int max = (int)(total * 0.5);
            memoryMegabytes = Math.Min(memoryMegabytes, max);

            // The value is shown in GB to the user, but stored in MB in the preferences.
            // Align max to nearest 0.5 GB.
            decimal maxValue = (decimal)(max / 1024.0);
            maxValue = Math.Round(maxValue * 2.0m) / 2.0m;

            nudCacheMemory.Minimum = 0;
            nudCacheMemory.Maximum = maxValue;
            nudCacheMemory.Value = (decimal)(memoryMegabytes / 1024.0);
            NudHelper.FixNudScroll(nudCacheMemory);

            cbCacheInTimeline.Text = "Show cache memory in the timeline";
            cbCacheInTimeline.Checked = showCacheInTimeline;
        }

        private void InitPagePlayer()
        {
            tabPlayer.Text = "Player";

            chkHardwareDecoding.Text = "Enable hardware decoding";
            chkHardwareScaling.Text = "Enable hardware scaling";
            chkEnableFrameSkipping.Text = "Allow skipping frames if there is not enough time";
            chkInteractiveTracker.Text = RootLang.dlgPreferences_Player_InteractiveFrameTracker;
            chkLockSpeeds.Text = RootLang.dlgPreferences_Player_SyncLockSpeeds;
            chkSyncByMotion.Text = "Use motion synchronization mode";
            chkShowFramerate.Text = "Show framerate in speed label";
            chkLoopPlayback.Text = "Loop playback";

            chkHardwareDecoding.Checked = enableHardwareDecoding;
            chkPreviewScaling.Checked = enablePreviewScaling;
            chkHardwareScaling.Checked = enableHardwareScaling;
            chkEnableFrameSkipping.Checked = enableFrameSkipping;
            chkInteractiveTracker.Checked = interactiveFrameTracker;
            chkLockSpeeds.Checked = syncLockSpeeds;
            chkSyncByMotion.Checked = syncByMotion;
            chkShowFramerate.Checked = showFramerateInSpeedLabel;
            chkLoopPlayback.Checked = loopPlayback;
        }

        private void InitPageJumping()
        {
            tabJumping.Text = "Jump size";
            grpJumping.Text = "Timeline jumping";

            lblSmallJump.Text = "Small jump size:";
            lblLargeJump.Text = "Large jump size:";
   
            nudSmallJump.Value = (decimal)smallJumpSize;
            nudLargeJump.Value = (decimal)largeJumpSize;
            NudHelper.FixNudScroll(nudSmallJump);
            NudHelper.FixNudScroll(nudLargeJump);

            cbSmallJump.Items.Add("Seconds");
            cbSmallJump.Items.Add("Milliseconds");
            cbSmallJump.Items.Add("Frames");
            cbSmallJump.Items.Add("Percent");
            int currentSmallJumpUnitIndex = (int)smallJumpUnit;
            cbSmallJump.SelectedIndex = currentSmallJumpUnitIndex < cbSmallJump.Items.Count ?
                currentSmallJumpUnitIndex : 0;

            cbLargeJump.Items.Add("Seconds");
            cbLargeJump.Items.Add("Milliseconds");
            cbLargeJump.Items.Add("Frames");
            cbLargeJump.Items.Add("Percent");
            int currentLargeJumpUnitIndex = (int)largeJumpUnit;
            cbLargeJump.SelectedIndex = currentLargeJumpUnitIndex < cbLargeJump.Items.Count ?
                currentLargeJumpUnitIndex : 0;

            UpdateCommandView();
        }
        
        private void InitPageImage()
        {
            tabImage.Text = RootLang.prefPanelPlayer_Image;

            chkEnablePixelFiltering.Text = RootLang.prefPanelPlayer_EnablePixelFiltering;
            chkEnablePixelFiltering.Checked = enablePixelFiltering;

            // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblAspectRatio.Text = RootLang.dlgPreferences_Player_lblImageFormat;
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatAuto);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce43);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce169);
            int currentAspectIndex = (int)imageAspectRatio;
            cmbImageFormats.SelectedIndex = currentAspectIndex < cmbImageFormats.Items.Count ? currentAspectIndex : 0;

            chkDeinterlace.Text = RootLang.dlgPreferences_Player_DeinterlaceByDefault;
            chkDeinterlace.Checked = deinterlaceByDefault;
        }
        #endregion

        #region Handlers
        #region General
        private void ChkDetectImageSequencesCheckedChanged(object sender, EventArgs e)
        {
            detectImageSequences = chkDetectImageSequences.Checked;
        }
        private void tbPlaybackKVA_TextChanged(object sender, EventArgs e)
        {
            playbackKVA = tbPlaybackKVA.Text;
        }
        private void btnPlaybackKVA_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            string initialDirectory = "";
            if (!string.IsNullOrEmpty(playbackKVA) && File.Exists(playbackKVA) && Path.IsPathRooted(playbackKVA))
                initialDirectory = Path.GetDirectoryName(playbackKVA);

            if (!string.IsNullOrEmpty(initialDirectory))
                dialog.InitialDirectory = initialDirectory;
            else
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            dialog.Title = ScreenManagerLang.dlgLoadAnalysis_Title;
            dialog.RestoreDirectory = true;
            dialog.Filter = FilesystemHelper.OpenKVAFilter(ScreenManagerLang.FileFilter_AllSupported);
            dialog.FilterIndex = 1;

            if (dialog.ShowDialog() == DialogResult.OK)
                tbPlaybackKVA.Text = dialog.FileName;
        }
        #endregion

        #region Memory
        private void nudCacheMemory_ValueChanged(object sender, EventArgs e)
        {
            memoryMegabytes = (int)(nudCacheMemory.Value * 1024.0m);
        }
        private void cbCacheInTimeline_CheckedChanged(object sender, EventArgs e)
        {
            showCacheInTimeline = cbCacheInTimeline.Checked;
        }
        #endregion

        #region Player
        private void chkHardwareDecoding_CheckedChanged(object sender, EventArgs e)
        {
            enableHardwareDecoding = chkHardwareDecoding.Checked;
        }

        private void chkPreviewScaling_CheckedChanged(object sender, EventArgs e)
        {
            enablePreviewScaling = chkPreviewScaling.Checked;
        }

        private void chkHardwareScaling_CheckedChanged(object sender, EventArgs e)
        {
            enableHardwareScaling = chkHardwareScaling.Checked;
        }

        private void ChkEnableFrameSkippingCheckedChanged(object sender, EventArgs e)
        {
            enableFrameSkipping = chkEnableFrameSkipping.Checked;
        }
        private void chkInteractiveTracker_CheckedChanged(object sender, EventArgs e)
        {
            interactiveFrameTracker = chkInteractiveTracker.Checked;
        }
        private void ChkLockSpeedsCheckedChanged(object sender, EventArgs e)
        {
            syncLockSpeeds = chkLockSpeeds.Checked;
        }
        private void chkSyncByMotion_CheckedChanged(object sender, EventArgs e)
        {
            syncByMotion = chkSyncByMotion.Checked;
        }

        private void cbShowFramerate_CheckedChanged(object sender, EventArgs e)
        {
            showFramerateInSpeedLabel = chkShowFramerate.Checked;
        }

        private void chkLoopPlayback_CheckedChanged(object sender, EventArgs e)
        {
            loopPlayback = chkLoopPlayback.Checked;
        }

        #endregion

        #region Jumping

        private void nudSmallJump_ValueChanged(object sender, EventArgs e)
        {
            smallJumpSize = (float)nudSmallJump.Value;
        }
        private void nudLargeJump_ValueChanged(object sender, EventArgs e)
        {
            largeJumpSize = (float)nudLargeJump.Value;
        }

        private void cbSmallJump_SelectedIndexChanged(object sender, EventArgs e)
        {
            smallJumpUnit = (TimelineJumpUnit)cbSmallJump.SelectedIndex;
        }

        private void cbLargeJump_SelectedIndexChanged(object sender, EventArgs e)
        {
            largeJumpUnit = (TimelineJumpUnit)cbLargeJump.SelectedIndex;
        }

        private void UpdateCommandView()
        {
            List<string> names = new List<string>() { 
                "SmallJumpForward", 
                "SmallJumpBackward", 
                "LargeJumpForward", 
                "LargeJumpBackward", };
            
            lvCommands.Items.Clear();
            foreach (string name in names)
            {
                var command = HotkeySettingsManager.ActiveBindings.FindByName(category, name);
                string key = command.KeyData == Keys.None ? "" : command.KeyData.ToText();
                ListViewItem item = new ListViewItem(new string[] { name, key });
                item.Tag = command;
                if (name == selectedCommand)
                    item.Selected = true;

                lvCommands.Items.Add(item);
            }

            int secondColumnWidth = lvCommands.ClientSize.Width - lvCommands.Columns[0].Width;
            lvCommands.Columns[1].Width = secondColumnWidth;

            if (lvCommands.Items.Count > 0 && (lvCommands.SelectedItems == null || lvCommands.SelectedItems.Count == 0))
                lvCommands.Items[0].Selected = true;

            if (lvCommands.SelectedItems.Count > 0)
                lvCommands.SelectedItems[0].EnsureVisible();

            lvCommands.Select();
            lvCommands.HideSelection = false;
        }
        private void lvCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvCommands.SelectedItems.Count != 1)
                return;

            HotkeyCommand command = lvCommands.SelectedItems[0].Tag as HotkeyCommand;
            if (command == null)
                return;

            selectedCommand = command.Name;
            tbHotkey.SetKeydata(category, command.Name, command.KeyData);
        }
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCommand))
            {
                return;
            }

            HotkeySettingsManager.ActiveBindings.Update(category, selectedCommand, Keys.None);
            tbHotkey.SetKeydata(category, selectedCommand, Keys.None);
            UpdateCommandView();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCommand))
            {
                return;
            }

            HotkeySettingsManager.ActiveBindings.Update(category, selectedCommand, tbHotkey.KeyData);
            UpdateCommandView();
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCommand))
                return;

            HotkeySettingsManager.ResetToDefault(
                HotkeySettingsManager.ActiveBindings, 
                category, 
                selectedCommand);

            HotkeyCommand updatedCommand = HotkeySettingsManager.ActiveBindings.FindByName(category, selectedCommand);
            if (updatedCommand == null)
            {
                return;
            }

            tbHotkey.SetKeydata(category, updatedCommand.Name, updatedCommand.KeyData);
            UpdateCommandView();
        }

        #endregion

        #region Image
        private void chkEnablePixelFiltering_CheckedChanged(object sender, EventArgs e)
        {
            enablePixelFiltering = chkEnablePixelFiltering.Checked;
        }
        private void cmbImageAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageAspectRatio = (ImageAspectRatio)cmbImageFormats.SelectedIndex;
        }
        private void chkDeinterlace_CheckedChanged(object sender, EventArgs e)
        {
            deinterlaceByDefault = chkDeinterlace.Checked;
        }
        #endregion

        #endregion

        public void CommitChanges()
        {
            // General
            PreferencesManager.PlayerPreferences.DetectImageSequences = detectImageSequences;
            PreferencesManager.PlayerPreferences.PlaybackKVA = playbackKVA;

            // Memory
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = memoryMegabytes;
            PreferencesManager.PlayerPreferences.ShowCacheInTimeline = showCacheInTimeline;

            // Player
            PreferencesManager.PlayerPreferences.EnableHardwareDecoding = enableHardwareDecoding;
            PreferencesManager.PlayerPreferences.EnablePreviewScaling = enablePreviewScaling;
            PreferencesManager.PlayerPreferences.EnableHardwareScaling = enableHardwareScaling;
            PreferencesManager.PlayerPreferences.EnableFrameSkipping = enableFrameSkipping;
            PreferencesManager.PlayerPreferences.InteractiveFrameTracker = interactiveFrameTracker;
            PreferencesManager.PlayerPreferences.SyncLockSpeed = syncLockSpeeds;
            PreferencesManager.PlayerPreferences.SyncByMotion = syncByMotion;
            PreferencesManager.PlayerPreferences.SpeedLabelFramerate = showFramerateInSpeedLabel;
            PreferencesManager.PlayerPreferences.LoopPlayback = loopPlayback;

            // Time jump
            PreferencesManager.PlayerPreferences.TimelineJumpSmallSize = smallJumpSize;
            PreferencesManager.PlayerPreferences.TimelineJumpSmallUnit = smallJumpUnit;
            PreferencesManager.PlayerPreferences.TimelineJumpLargeSize = largeJumpSize;
            PreferencesManager.PlayerPreferences.TimelineJumpLargeUnit = largeJumpUnit;

            // Image
            PreferencesManager.PlayerPreferences.EnablePixelFiltering = enablePixelFiltering;
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = deinterlaceByDefault;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
        }

    }
}
