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
namespace Kinovea.Root
{
    partial class PreferencePanelPlayer
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
      this.chkDeinterlace = new System.Windows.Forms.CheckBox();
      this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
      this.lblWorkingZoneMemory = new System.Windows.Forms.Label();
      this.tabSubPages = new System.Windows.Forms.TabControl();
      this.tabGeneral = new System.Windows.Forms.TabPage();
      this.lblPlaybackKVA = new System.Windows.Forms.Label();
      this.tbPlaybackKVA = new System.Windows.Forms.TextBox();
      this.btnPlaybackKVA = new System.Windows.Forms.Button();
      this.chkSyncByMotion = new System.Windows.Forms.CheckBox();
      this.chkDetectImageSequences = new System.Windows.Forms.CheckBox();
      this.chkInteractiveTracker = new System.Windows.Forms.CheckBox();
      this.cmbImageFormats = new System.Windows.Forms.ComboBox();
      this.lblImageFormat = new System.Windows.Forms.Label();
      this.chkLockSpeeds = new System.Windows.Forms.CheckBox();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.cbCacheInTimeline = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabMemory.SuspendLayout();
      this.SuspendLayout();
      // 
      // chkDeinterlace
      // 
      this.chkDeinterlace.Location = new System.Drawing.Point(23, 193);
      this.chkDeinterlace.Name = "chkDeinterlace";
      this.chkDeinterlace.Size = new System.Drawing.Size(369, 20);
      this.chkDeinterlace.TabIndex = 23;
      this.chkDeinterlace.Text = "dlgPreferences_DeinterlaceByDefault";
      this.chkDeinterlace.UseVisualStyleBackColor = true;
      this.chkDeinterlace.CheckedChanged += new System.EventHandler(this.ChkDeinterlaceCheckedChanged);
      // 
      // trkMemoryBuffer
      // 
      this.trkMemoryBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.trkMemoryBuffer.BackColor = System.Drawing.Color.White;
      this.trkMemoryBuffer.Location = new System.Drawing.Point(15, 55);
      this.trkMemoryBuffer.Maximum = 1024;
      this.trkMemoryBuffer.Minimum = 16;
      this.trkMemoryBuffer.Name = "trkMemoryBuffer";
      this.trkMemoryBuffer.Size = new System.Drawing.Size(452, 45);
      this.trkMemoryBuffer.TabIndex = 35;
      this.trkMemoryBuffer.TickFrequency = 50;
      this.trkMemoryBuffer.Value = 512;
      this.trkMemoryBuffer.ValueChanged += new System.EventHandler(this.trkWorkingZoneMemory_ValueChanged);
      // 
      // lblWorkingZoneMemory
      // 
      this.lblWorkingZoneMemory.AutoSize = true;
      this.lblWorkingZoneMemory.Location = new System.Drawing.Point(15, 30);
      this.lblWorkingZoneMemory.Name = "lblWorkingZoneMemory";
      this.lblWorkingZoneMemory.Size = new System.Drawing.Size(208, 13);
      this.lblWorkingZoneMemory.TabIndex = 17;
      this.lblWorkingZoneMemory.Text = "Memory allocated for frame buffers: {0} MB";
      // 
      // tabSubPages
      // 
      this.tabSubPages.Controls.Add(this.tabGeneral);
      this.tabSubPages.Controls.Add(this.tabMemory);
      this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabSubPages.Location = new System.Drawing.Point(0, 0);
      this.tabSubPages.Name = "tabSubPages";
      this.tabSubPages.SelectedIndex = 0;
      this.tabSubPages.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.TabIndex = 27;
      // 
      // tabGeneral
      // 
      this.tabGeneral.Controls.Add(this.lblPlaybackKVA);
      this.tabGeneral.Controls.Add(this.tbPlaybackKVA);
      this.tabGeneral.Controls.Add(this.btnPlaybackKVA);
      this.tabGeneral.Controls.Add(this.chkSyncByMotion);
      this.tabGeneral.Controls.Add(this.chkDetectImageSequences);
      this.tabGeneral.Controls.Add(this.chkInteractiveTracker);
      this.tabGeneral.Controls.Add(this.cmbImageFormats);
      this.tabGeneral.Controls.Add(this.lblImageFormat);
      this.tabGeneral.Controls.Add(this.chkLockSpeeds);
      this.tabGeneral.Controls.Add(this.chkDeinterlace);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabGeneral.Size = new System.Drawing.Size(482, 296);
      this.tabGeneral.TabIndex = 0;
      this.tabGeneral.Text = "General";
      this.tabGeneral.UseVisualStyleBackColor = true;
      // 
      // lblPlaybackKVA
      // 
      this.lblPlaybackKVA.AutoSize = true;
      this.lblPlaybackKVA.Location = new System.Drawing.Point(20, 227);
      this.lblPlaybackKVA.Name = "lblPlaybackKVA";
      this.lblPlaybackKVA.Size = new System.Drawing.Size(121, 13);
      this.lblPlaybackKVA.TabIndex = 61;
      this.lblPlaybackKVA.Text = "Default annotations file :";
      // 
      // tbPlaybackKVA
      // 
      this.tbPlaybackKVA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPlaybackKVA.Location = new System.Drawing.Point(263, 225);
      this.tbPlaybackKVA.Name = "tbPlaybackKVA";
      this.tbPlaybackKVA.Size = new System.Drawing.Size(175, 20);
      this.tbPlaybackKVA.TabIndex = 62;
      this.tbPlaybackKVA.TextChanged += new System.EventHandler(this.tbPlaybackKVA_TextChanged);
      // 
      // btnPlaybackKVA
      // 
      this.btnPlaybackKVA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPlaybackKVA.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnPlaybackKVA.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnPlaybackKVA.FlatAppearance.BorderSize = 0;
      this.btnPlaybackKVA.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnPlaybackKVA.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnPlaybackKVA.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnPlaybackKVA.Location = new System.Drawing.Point(444, 224);
      this.btnPlaybackKVA.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnPlaybackKVA.Name = "btnPlaybackKVA";
      this.btnPlaybackKVA.Size = new System.Drawing.Size(20, 20);
      this.btnPlaybackKVA.TabIndex = 63;
      this.btnPlaybackKVA.Tag = "";
      this.btnPlaybackKVA.UseVisualStyleBackColor = true;
      this.btnPlaybackKVA.Click += new System.EventHandler(this.btnPlaybackKVA_Click);
      // 
      // chkSyncByMotion
      // 
      this.chkSyncByMotion.Location = new System.Drawing.Point(23, 80);
      this.chkSyncByMotion.Name = "chkSyncByMotion";
      this.chkSyncByMotion.Size = new System.Drawing.Size(369, 20);
      this.chkSyncByMotion.TabIndex = 32;
      this.chkSyncByMotion.Text = "syncByMotion";
      this.chkSyncByMotion.UseVisualStyleBackColor = true;
      this.chkSyncByMotion.CheckedChanged += new System.EventHandler(this.chkSyncByMotion_CheckedChanged);
      // 
      // chkDetectImageSequences
      // 
      this.chkDetectImageSequences.Location = new System.Drawing.Point(23, 27);
      this.chkDetectImageSequences.Name = "chkDetectImageSequences";
      this.chkDetectImageSequences.Size = new System.Drawing.Size(369, 20);
      this.chkDetectImageSequences.TabIndex = 31;
      this.chkDetectImageSequences.Text = "dlgPreferences_DetectImageSequences";
      this.chkDetectImageSequences.UseVisualStyleBackColor = true;
      this.chkDetectImageSequences.CheckedChanged += new System.EventHandler(this.ChkDetectImageSequencesCheckedChanged);
      // 
      // chkInteractiveTracker
      // 
      this.chkInteractiveTracker.Location = new System.Drawing.Point(23, 106);
      this.chkInteractiveTracker.Name = "chkInteractiveTracker";
      this.chkInteractiveTracker.Size = new System.Drawing.Size(369, 20);
      this.chkInteractiveTracker.TabIndex = 30;
      this.chkInteractiveTracker.Text = "dlgPreferences_InteractiveFrameTracker";
      this.chkInteractiveTracker.UseVisualStyleBackColor = true;
      this.chkInteractiveTracker.CheckedChanged += new System.EventHandler(this.ChkInteractiveTrackerCheckedChanged);
      // 
      // cmbImageFormats
      // 
      this.cmbImageFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbImageFormats.Location = new System.Drawing.Point(302, 158);
      this.cmbImageFormats.Name = "cmbImageFormats";
      this.cmbImageFormats.Size = new System.Drawing.Size(154, 21);
      this.cmbImageFormats.TabIndex = 29;
      this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
      // 
      // lblImageFormat
      // 
      this.lblImageFormat.AutoSize = true;
      this.lblImageFormat.Location = new System.Drawing.Point(20, 161);
      this.lblImageFormat.Name = "lblImageFormat";
      this.lblImageFormat.Size = new System.Drawing.Size(110, 13);
      this.lblImageFormat.TabIndex = 28;
      this.lblImageFormat.Text = "Default image format :";
      // 
      // chkLockSpeeds
      // 
      this.chkLockSpeeds.Location = new System.Drawing.Point(23, 53);
      this.chkLockSpeeds.Name = "chkLockSpeeds";
      this.chkLockSpeeds.Size = new System.Drawing.Size(369, 20);
      this.chkLockSpeeds.TabIndex = 24;
      this.chkLockSpeeds.Text = "dlgPreferences_SyncLockSpeeds";
      this.chkLockSpeeds.UseVisualStyleBackColor = true;
      this.chkLockSpeeds.CheckedChanged += new System.EventHandler(this.ChkLockSpeedsCheckedChanged);
      // 
      // tabMemory
      // 
      this.tabMemory.Controls.Add(this.cbCacheInTimeline);
      this.tabMemory.Controls.Add(this.trkMemoryBuffer);
      this.tabMemory.Controls.Add(this.lblWorkingZoneMemory);
      this.tabMemory.Location = new System.Drawing.Point(4, 22);
      this.tabMemory.Name = "tabMemory";
      this.tabMemory.Padding = new System.Windows.Forms.Padding(3);
      this.tabMemory.Size = new System.Drawing.Size(482, 296);
      this.tabMemory.TabIndex = 1;
      this.tabMemory.Text = "Memory";
      this.tabMemory.UseVisualStyleBackColor = true;
      // 
      // cbCacheInTimeline
      // 
      this.cbCacheInTimeline.Location = new System.Drawing.Point(18, 115);
      this.cbCacheInTimeline.Name = "cbCacheInTimeline";
      this.cbCacheInTimeline.Size = new System.Drawing.Size(369, 20);
      this.cbCacheInTimeline.TabIndex = 36;
      this.cbCacheInTimeline.Text = "Show memory indicator in the timeline";
      this.cbCacheInTimeline.UseVisualStyleBackColor = true;
      this.cbCacheInTimeline.CheckedChanged += new System.EventHandler(this.cbCacheInTimeline_CheckedChanged);
      // 
      // PreferencePanelPlayer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.tabSubPages);
      this.Name = "PreferencePanelPlayer";
      this.Size = new System.Drawing.Size(490, 322);
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).EndInit();
      this.tabSubPages.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.tabGeneral.PerformLayout();
      this.tabMemory.ResumeLayout(false);
      this.tabMemory.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cmbImageFormats;
        private System.Windows.Forms.CheckBox chkLockSpeeds;
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabControl tabSubPages;
        private System.Windows.Forms.Label lblWorkingZoneMemory;
        private System.Windows.Forms.TrackBar trkMemoryBuffer;
        private System.Windows.Forms.CheckBox chkDeinterlace;
        private System.Windows.Forms.CheckBox chkInteractiveTracker;
        private System.Windows.Forms.CheckBox chkDetectImageSequences;
        private System.Windows.Forms.CheckBox chkSyncByMotion;
        private System.Windows.Forms.Label lblPlaybackKVA;
        private System.Windows.Forms.TextBox tbPlaybackKVA;
        private System.Windows.Forms.Button btnPlaybackKVA;
        private System.Windows.Forms.CheckBox cbCacheInTimeline;
    }
}
