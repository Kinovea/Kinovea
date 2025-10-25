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

        private bool detectImageSequences;
        private bool syncLockSpeeds;
        private bool syncByMotion;
        private int memoryBuffer;
        private string playbackKVA;
        private bool showCacheInTimeline;
        private bool interactiveFrameTracker;
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
        }

        private void ImportPreferences()
        {
            detectImageSequences = PreferencesManager.PlayerPreferences.DetectImageSequences;
            syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;
            syncByMotion = PreferencesManager.PlayerPreferences.SyncByMotion;
            playbackKVA = PreferencesManager.PlayerPreferences.PlaybackKVA;
            showCacheInTimeline = PreferencesManager.PlayerPreferences.ShowCacheInTimeline;
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            enablePixelFiltering = PreferencesManager.PlayerPreferences.EnablePixelFiltering;
            imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;
            deinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
            memoryBuffer = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
        }
        private void InitPage()
        {
            InitPageGeneral();
            InitPageMemory();
            InitPageImage();
        }

        private void InitPageGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;
            chkDetectImageSequences.Text = RootLang.dlgPreferences_Player_ImportImageSequences;
            chkLockSpeeds.Text = RootLang.dlgPreferences_Player_SyncLockSpeeds;
            chkSyncByMotion.Text = "Use motion synchronization mode";

            chkDetectImageSequences.Checked = detectImageSequences;
            chkLockSpeeds.Checked = syncLockSpeeds;
            chkSyncByMotion.Checked = syncByMotion;
            
            lblPlaybackKVA.Text = RootLang.dlgPreferences_Player_DefaultKVA;
            tbPlaybackKVA.Text = playbackKVA;

            
        }

        private void InitPageMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            int maxMemoryBuffer = MemoryHelper.MaxMemoryBuffer();
            trkMemoryBuffer.Maximum = maxMemoryBuffer;

            memoryBuffer = Math.Min(memoryBuffer, trkMemoryBuffer.Maximum);
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();

            cbCacheInTimeline.Text = "Show memory indicator in the timeline";
            cbCacheInTimeline.Checked = showCacheInTimeline;

            cbCacheInTimeline.Visible = false;
        }
        
        private void InitPageImage()
        {
            tabImage.Text = Kinovea.Root.Languages.RootLang.prefPanelPlayer_Image;

            chkInteractiveTracker.Text = RootLang.dlgPreferences_Player_InteractiveFrameTracker;
            chkInteractiveTracker.Checked = interactiveFrameTracker;

            chkEnablePixelFiltering.Text = Kinovea.Root.Languages.RootLang.prefPanelPlayer_EnablePixelFiltering;
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
        private void ChkLockSpeedsCheckedChanged(object sender, EventArgs e)
        {
            syncLockSpeeds = chkLockSpeeds.Checked;
        }
        private void chkSyncByMotion_CheckedChanged(object sender, EventArgs e)
        {
            syncByMotion = chkSyncByMotion.Checked;
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
        private void trkWorkingZoneMemory_ValueChanged(object sender, EventArgs e)
        {
            memoryBuffer = trkMemoryBuffer.Value;
            UpdateMemoryLabel();
        }
        private void UpdateMemoryLabel()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            string formatted = memoryBuffer.ToString("#,0", nfi);

            lblWorkingZoneMemory.Text = string.Format(RootLang.dlgPreferences_Player_lblMemory, formatted);
        }
        private void cbCacheInTimeline_CheckedChanged(object sender, EventArgs e)
        {
            showCacheInTimeline = cbCacheInTimeline.Checked;
        }
        #endregion

        #region Image
        private void chkInteractiveTracker_CheckedChanged(object sender, EventArgs e)
        {
            interactiveFrameTracker = chkInteractiveTracker.Checked;
        }
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
            PreferencesManager.PlayerPreferences.DetectImageSequences = detectImageSequences;
            PreferencesManager.PlayerPreferences.SyncLockSpeed = syncLockSpeeds;
            PreferencesManager.PlayerPreferences.SyncByMotion = syncByMotion;
            PreferencesManager.PlayerPreferences.PlaybackKVA = playbackKVA;
            PreferencesManager.PlayerPreferences.ShowCacheInTimeline = showCacheInTimeline;

            PreferencesManager.PlayerPreferences.WorkingZoneMemory = memoryBuffer;

            PreferencesManager.PlayerPreferences.InteractiveFrameTracker = interactiveFrameTracker;
            PreferencesManager.PlayerPreferences.EnablePixelFiltering = enablePixelFiltering;
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = deinterlaceByDefault;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
        }
    }
}
