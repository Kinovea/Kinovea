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
            PreferenceTab.Player_Memory 
        };
        private bool deinterlaceByDefault;
        private bool detectImageSequences;
        private bool interactiveFrameTracker;
        private ImageAspectRatio imageAspectRatio;
        private bool syncLockSpeeds;
        private bool syncByMotion;
        private int memoryBuffer;
        private string playbackKVA;
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelPlayer()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_tabPlayback;
            icon = Resources.video;
            
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
            deinterlaceByDefault = PreferencesManager.PlayerPreferences.DeinterlaceByDefault;
            detectImageSequences = PreferencesManager.PlayerPreferences.DetectImageSequences;
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            imageAspectRatio = PreferencesManager.PlayerPreferences.AspectRatio;
            syncLockSpeeds = PreferencesManager.PlayerPreferences.SyncLockSpeed;
            syncByMotion = PreferencesManager.PlayerPreferences.SyncByMotion;
            memoryBuffer = PreferencesManager.PlayerPreferences.WorkingZoneMemory;
            playbackKVA = PreferencesManager.PlayerPreferences.PlaybackKVA;
        }
        private void InitPage()
        {
            InitPageGeneral();
            InitPageMemory();
        }

        private void InitPageGeneral()
        {
            tabGeneral.Text = RootLang.dlgPreferences_tabGeneral;
            chkDetectImageSequences.Text = RootLang.dlgPreferences_Player_ImportImageSequences;
            chkLockSpeeds.Text = RootLang.dlgPreferences_Player_SyncLockSpeeds;
            chkSyncByMotion.Text = "Use motion synchronization mode";
            chkInteractiveTracker.Text = RootLang.dlgPreferences_Player_InteractiveFrameTracker;

            // Combo Image Aspect Ratios (MUST be filled in the order of the enum)
            lblImageFormat.Text = RootLang.dlgPreferences_Player_lblImageFormat;
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatAuto);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce43);
            cmbImageFormats.Items.Add(ScreenManagerLang.mnuFormatForce169);
            
            chkDeinterlace.Text = RootLang.dlgPreferences_Player_DeinterlaceByDefault;

            chkDetectImageSequences.Checked = detectImageSequences;
            chkLockSpeeds.Checked = syncLockSpeeds;
            chkSyncByMotion.Checked = syncByMotion;
            chkInteractiveTracker.Checked = interactiveFrameTracker;
            chkDeinterlace.Checked = deinterlaceByDefault;
            lblPlaybackKVA.Text = RootLang.dlgPreferences_Player_DefaultKVA;
            tbPlaybackKVA.Text = playbackKVA;

            // Select current image format.
            int selected = (int)imageAspectRatio;
            cmbImageFormats.SelectedIndex = selected < cmbImageFormats.Items.Count ? selected : 0;
        }

        private void InitPageMemory()
        {
            tabMemory.Text = RootLang.dlgPreferences_Capture_tabMemory;

            int maxMemoryBuffer = MemoryHelper.MaxMemoryBuffer();
            trkMemoryBuffer.Maximum = maxMemoryBuffer;

            memoryBuffer = Math.Min(memoryBuffer, trkMemoryBuffer.Maximum);
            trkMemoryBuffer.Value = memoryBuffer;
            UpdateMemoryLabel();
        }
        #endregion

        #region Handlers
        #region General
        private void ChkDeinterlaceCheckedChanged(object sender, EventArgs e)
        {
            deinterlaceByDefault = chkDeinterlace.Checked;
        }
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
        private void ChkInteractiveTrackerCheckedChanged(object sender, EventArgs e)
        {
            interactiveFrameTracker = chkInteractiveTracker.Checked;
        }
        private void cmbImageAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            imageAspectRatio = (ImageAspectRatio)cmbImageFormats.SelectedIndex;
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
        #endregion
        #endregion

        public void CommitChanges()
        {
            PreferencesManager.PlayerPreferences.DeinterlaceByDefault = deinterlaceByDefault;
            PreferencesManager.PlayerPreferences.DetectImageSequences = detectImageSequences;
            PreferencesManager.PlayerPreferences.SyncLockSpeed = syncLockSpeeds;
            PreferencesManager.PlayerPreferences.SyncByMotion = syncByMotion;
            PreferencesManager.PlayerPreferences.InteractiveFrameTracker = interactiveFrameTracker;
            PreferencesManager.PlayerPreferences.AspectRatio = imageAspectRatio;
            PreferencesManager.PlayerPreferences.PlaybackKVA = playbackKVA;
            PreferencesManager.PlayerPreferences.WorkingZoneMemory = memoryBuffer;
        }
    }
}
