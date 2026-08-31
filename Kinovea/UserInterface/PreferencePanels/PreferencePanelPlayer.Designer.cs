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
      this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
      this.lblWorkingZoneMemory = new System.Windows.Forms.Label();
      this.tabSubPages = new System.Windows.Forms.TabControl();
      this.tabGeneral = new System.Windows.Forms.TabPage();
      this.lblPlaybackKVA = new System.Windows.Forms.Label();
      this.tbPlaybackKVA = new System.Windows.Forms.TextBox();
      this.btnPlaybackKVA = new System.Windows.Forms.Button();
      this.chkDetectImageSequences = new System.Windows.Forms.CheckBox();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.cbCacheInTimeline = new System.Windows.Forms.CheckBox();
      this.tabImage = new System.Windows.Forms.TabPage();
      this.chkEnablePixelFiltering = new System.Windows.Forms.CheckBox();
      this.cmbImageFormats = new System.Windows.Forms.ComboBox();
      this.lblAspectRatio = new System.Windows.Forms.Label();
      this.chkDeinterlace = new System.Windows.Forms.CheckBox();
      this.tabJumping = new System.Windows.Forms.TabPage();
      this.rbJumpByTime = new System.Windows.Forms.RadioButton();
      this.rbSnapToSteps = new System.Windows.Forms.RadioButton();
      this.grpJumping = new System.Windows.Forms.GroupBox();
      this.nudSnapSmall = new System.Windows.Forms.NumericUpDown();
      this.lblSnapSmall = new System.Windows.Forms.Label();
      this.nudSnapLarge = new System.Windows.Forms.NumericUpDown();
      this.lblSnapLarge = new System.Windows.Forms.Label();
      this.nudJumpLarge = new System.Windows.Forms.NumericUpDown();
      this.lblJumpLarge = new System.Windows.Forms.Label();
      this.nudJumpSmall = new System.Windows.Forms.NumericUpDown();
      this.lblJumpSmall = new System.Windows.Forms.Label();
      this.tabPlayer = new System.Windows.Forms.TabPage();
      this.chkSyncByMotion = new System.Windows.Forms.CheckBox();
      this.chkLockSpeeds = new System.Windows.Forms.CheckBox();
      this.chkEnableFrameSkipping = new System.Windows.Forms.CheckBox();
      this.chkInteractiveTracker = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabMemory.SuspendLayout();
      this.tabImage.SuspendLayout();
      this.tabJumping.SuspendLayout();
      this.grpJumping.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudSnapSmall)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSnapLarge)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudJumpLarge)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudJumpSmall)).BeginInit();
      this.tabPlayer.SuspendLayout();
      this.SuspendLayout();
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
      this.tabSubPages.Controls.Add(this.tabPlayer);
      this.tabSubPages.Controls.Add(this.tabJumping);
      this.tabSubPages.Controls.Add(this.tabImage);
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
      this.tabGeneral.Controls.Add(this.chkDetectImageSequences);
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
      this.lblPlaybackKVA.Location = new System.Drawing.Point(20, 67);
      this.lblPlaybackKVA.Name = "lblPlaybackKVA";
      this.lblPlaybackKVA.Size = new System.Drawing.Size(121, 13);
      this.lblPlaybackKVA.TabIndex = 61;
      this.lblPlaybackKVA.Text = "Default annotations file :";
      // 
      // tbPlaybackKVA
      // 
      this.tbPlaybackKVA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbPlaybackKVA.Location = new System.Drawing.Point(263, 65);
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
      this.btnPlaybackKVA.Location = new System.Drawing.Point(444, 64);
      this.btnPlaybackKVA.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnPlaybackKVA.Name = "btnPlaybackKVA";
      this.btnPlaybackKVA.Size = new System.Drawing.Size(20, 20);
      this.btnPlaybackKVA.TabIndex = 63;
      this.btnPlaybackKVA.Tag = "";
      this.btnPlaybackKVA.UseVisualStyleBackColor = true;
      this.btnPlaybackKVA.Click += new System.EventHandler(this.btnPlaybackKVA_Click);
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
      // tabImage
      // 
      this.tabImage.Controls.Add(this.chkEnablePixelFiltering);
      this.tabImage.Controls.Add(this.cmbImageFormats);
      this.tabImage.Controls.Add(this.lblAspectRatio);
      this.tabImage.Controls.Add(this.chkDeinterlace);
      this.tabImage.Location = new System.Drawing.Point(4, 22);
      this.tabImage.Name = "tabImage";
      this.tabImage.Size = new System.Drawing.Size(482, 296);
      this.tabImage.TabIndex = 2;
      this.tabImage.Text = "Image";
      this.tabImage.UseVisualStyleBackColor = true;
      // 
      // chkEnablePixelFiltering
      // 
      this.chkEnablePixelFiltering.Location = new System.Drawing.Point(24, 58);
      this.chkEnablePixelFiltering.Name = "chkEnablePixelFiltering";
      this.chkEnablePixelFiltering.Size = new System.Drawing.Size(369, 20);
      this.chkEnablePixelFiltering.TabIndex = 35;
      this.chkEnablePixelFiltering.Text = "dlgPreferences_EnablePixelFiltering";
      this.chkEnablePixelFiltering.UseVisualStyleBackColor = true;
      this.chkEnablePixelFiltering.CheckedChanged += new System.EventHandler(this.chkEnablePixelFiltering_CheckedChanged);
      // 
      // cmbImageFormats
      // 
      this.cmbImageFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbImageFormats.Location = new System.Drawing.Point(264, 88);
      this.cmbImageFormats.Name = "cmbImageFormats";
      this.cmbImageFormats.Size = new System.Drawing.Size(201, 21);
      this.cmbImageFormats.TabIndex = 34;
      this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
      // 
      // lblAspectRatio
      // 
      this.lblAspectRatio.AutoSize = true;
      this.lblAspectRatio.Location = new System.Drawing.Point(21, 91);
      this.lblAspectRatio.Name = "lblAspectRatio";
      this.lblAspectRatio.Size = new System.Drawing.Size(102, 13);
      this.lblAspectRatio.TabIndex = 33;
      this.lblAspectRatio.Text = "Default aspect ratio:";
      // 
      // chkDeinterlace
      // 
      this.chkDeinterlace.Location = new System.Drawing.Point(24, 123);
      this.chkDeinterlace.Name = "chkDeinterlace";
      this.chkDeinterlace.Size = new System.Drawing.Size(369, 20);
      this.chkDeinterlace.TabIndex = 32;
      this.chkDeinterlace.Text = "dlgPreferences_DeinterlaceByDefault";
      this.chkDeinterlace.UseVisualStyleBackColor = true;
      this.chkDeinterlace.CheckedChanged += new System.EventHandler(this.chkDeinterlace_CheckedChanged);
      // 
      // tabJumping
      // 
      this.tabJumping.Controls.Add(this.grpJumping);
      this.tabJumping.Location = new System.Drawing.Point(4, 22);
      this.tabJumping.Name = "tabJumping";
      this.tabJumping.Size = new System.Drawing.Size(482, 296);
      this.tabJumping.TabIndex = 3;
      this.tabJumping.Text = "Jumping";
      this.tabJumping.UseVisualStyleBackColor = true;
      // 
      // rbJumpByTime
      // 
      this.rbJumpByTime.AutoSize = true;
      this.rbJumpByTime.Location = new System.Drawing.Point(21, 114);
      this.rbJumpByTime.Name = "rbJumpByTime";
      this.rbJumpByTime.Size = new System.Drawing.Size(86, 17);
      this.rbJumpByTime.TabIndex = 39;
      this.rbJumpByTime.TabStop = true;
      this.rbJumpByTime.Text = "Jump by time";
      this.rbJumpByTime.UseVisualStyleBackColor = true;
      this.rbJumpByTime.CheckedChanged += new System.EventHandler(this.rbSnapToSteps_CheckedChanged);
      // 
      // rbSnapToSteps
      // 
      this.rbSnapToSteps.AutoSize = true;
      this.rbSnapToSteps.Location = new System.Drawing.Point(21, 25);
      this.rbSnapToSteps.Name = "rbSnapToSteps";
      this.rbSnapToSteps.Size = new System.Drawing.Size(90, 17);
      this.rbSnapToSteps.TabIndex = 38;
      this.rbSnapToSteps.TabStop = true;
      this.rbSnapToSteps.Text = "Snap to steps";
      this.rbSnapToSteps.UseVisualStyleBackColor = true;
      this.rbSnapToSteps.CheckedChanged += new System.EventHandler(this.rbSnapToSteps_CheckedChanged);
      // 
      // grpJumping
      // 
      this.grpJumping.Controls.Add(this.nudJumpLarge);
      this.grpJumping.Controls.Add(this.lblJumpLarge);
      this.grpJumping.Controls.Add(this.nudJumpSmall);
      this.grpJumping.Controls.Add(this.lblJumpSmall);
      this.grpJumping.Controls.Add(this.nudSnapLarge);
      this.grpJumping.Controls.Add(this.lblSnapLarge);
      this.grpJumping.Controls.Add(this.nudSnapSmall);
      this.grpJumping.Controls.Add(this.lblSnapSmall);
      this.grpJumping.Controls.Add(this.rbJumpByTime);
      this.grpJumping.Controls.Add(this.rbSnapToSteps);
      this.grpJumping.Location = new System.Drawing.Point(3, 3);
      this.grpJumping.Name = "grpJumping";
      this.grpJumping.Size = new System.Drawing.Size(476, 290);
      this.grpJumping.TabIndex = 66;
      this.grpJumping.TabStop = false;
      this.grpJumping.Text = "Timeline jumping";
      // 
      // nudSnapSmall
      // 
      this.nudSnapSmall.Location = new System.Drawing.Point(355, 52);
      this.nudSnapSmall.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudSnapSmall.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudSnapSmall.Name = "nudSnapSmall";
      this.nudSnapSmall.Size = new System.Drawing.Size(55, 20);
      this.nudSnapSmall.TabIndex = 56;
      this.nudSnapSmall.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.nudSnapSmall.ValueChanged += new System.EventHandler(this.nudSnapSmall_ValueChanged);
      // 
      // lblSnapSmall
      // 
      this.lblSnapSmall.AutoSize = true;
      this.lblSnapSmall.Location = new System.Drawing.Point(71, 54);
      this.lblSnapSmall.Name = "lblSnapSmall";
      this.lblSnapSmall.Size = new System.Drawing.Size(167, 13);
      this.lblSnapSmall.TabIndex = 55;
      this.lblSnapSmall.Text = "Small jump (total number of steps):";
      // 
      // nudSnapLarge
      // 
      this.nudSnapLarge.Location = new System.Drawing.Point(355, 78);
      this.nudSnapLarge.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudSnapLarge.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudSnapLarge.Name = "nudSnapLarge";
      this.nudSnapLarge.Size = new System.Drawing.Size(55, 20);
      this.nudSnapLarge.TabIndex = 58;
      this.nudSnapLarge.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudSnapLarge.ValueChanged += new System.EventHandler(this.nudSnapLarge_ValueChanged);
      // 
      // lblSnapLarge
      // 
      this.lblSnapLarge.AutoSize = true;
      this.lblSnapLarge.Location = new System.Drawing.Point(71, 80);
      this.lblSnapLarge.Name = "lblSnapLarge";
      this.lblSnapLarge.Size = new System.Drawing.Size(169, 13);
      this.lblSnapLarge.TabIndex = 57;
      this.lblSnapLarge.Text = "Large jump (total number of steps):";
      // 
      // nudJumpLarge
      // 
      this.nudJumpLarge.DecimalPlaces = 3;
      this.nudJumpLarge.Location = new System.Drawing.Point(355, 167);
      this.nudJumpLarge.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.nudJumpLarge.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudJumpLarge.Name = "nudJumpLarge";
      this.nudJumpLarge.Size = new System.Drawing.Size(55, 20);
      this.nudJumpLarge.TabIndex = 62;
      this.nudJumpLarge.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudJumpLarge.ValueChanged += new System.EventHandler(this.nudJumpLarge_ValueChanged);
      // 
      // lblJumpLarge
      // 
      this.lblJumpLarge.AutoSize = true;
      this.lblJumpLarge.Location = new System.Drawing.Point(71, 169);
      this.lblJumpLarge.Name = "lblJumpLarge";
      this.lblJumpLarge.Size = new System.Drawing.Size(111, 13);
      this.lblJumpLarge.TabIndex = 61;
      this.lblJumpLarge.Text = "Large jump (seconds):";
      // 
      // nudJumpSmall
      // 
      this.nudJumpSmall.DecimalPlaces = 3;
      this.nudJumpSmall.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.nudJumpSmall.Location = new System.Drawing.Point(355, 141);
      this.nudJumpSmall.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudJumpSmall.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
      this.nudJumpSmall.Name = "nudJumpSmall";
      this.nudJumpSmall.Size = new System.Drawing.Size(55, 20);
      this.nudJumpSmall.TabIndex = 60;
      this.nudJumpSmall.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
      this.nudJumpSmall.ValueChanged += new System.EventHandler(this.nudJumpSmall_ValueChanged);
      // 
      // lblJumpSmall
      // 
      this.lblJumpSmall.AutoSize = true;
      this.lblJumpSmall.Location = new System.Drawing.Point(71, 143);
      this.lblJumpSmall.Name = "lblJumpSmall";
      this.lblJumpSmall.Size = new System.Drawing.Size(109, 13);
      this.lblJumpSmall.TabIndex = 59;
      this.lblJumpSmall.Text = "Small jump (seconds):";
      // 
      // tabPlayer
      // 
      this.tabPlayer.Controls.Add(this.chkInteractiveTracker);
      this.tabPlayer.Controls.Add(this.chkEnableFrameSkipping);
      this.tabPlayer.Controls.Add(this.chkSyncByMotion);
      this.tabPlayer.Controls.Add(this.chkLockSpeeds);
      this.tabPlayer.Location = new System.Drawing.Point(4, 22);
      this.tabPlayer.Name = "tabPlayer";
      this.tabPlayer.Size = new System.Drawing.Size(482, 296);
      this.tabPlayer.TabIndex = 4;
      this.tabPlayer.Text = "Player";
      this.tabPlayer.UseVisualStyleBackColor = true;
      // 
      // chkSyncByMotion
      // 
      this.chkSyncByMotion.Location = new System.Drawing.Point(20, 135);
      this.chkSyncByMotion.Name = "chkSyncByMotion";
      this.chkSyncByMotion.Size = new System.Drawing.Size(369, 20);
      this.chkSyncByMotion.TabIndex = 34;
      this.chkSyncByMotion.Text = "syncByMotion";
      this.chkSyncByMotion.UseVisualStyleBackColor = true;
      this.chkSyncByMotion.CheckedChanged += new System.EventHandler(this.chkSyncByMotion_CheckedChanged);
      // 
      // chkLockSpeeds
      // 
      this.chkLockSpeeds.Location = new System.Drawing.Point(20, 98);
      this.chkLockSpeeds.Name = "chkLockSpeeds";
      this.chkLockSpeeds.Size = new System.Drawing.Size(369, 20);
      this.chkLockSpeeds.TabIndex = 33;
      this.chkLockSpeeds.Text = "dlgPreferences_SyncLockSpeeds";
      this.chkLockSpeeds.UseVisualStyleBackColor = true;
      this.chkLockSpeeds.CheckedChanged += new System.EventHandler(this.ChkLockSpeedsCheckedChanged);
      // 
      // chkEnableFrameSkipping
      // 
      this.chkEnableFrameSkipping.Checked = true;
      this.chkEnableFrameSkipping.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkEnableFrameSkipping.Location = new System.Drawing.Point(20, 23);
      this.chkEnableFrameSkipping.Name = "chkEnableFrameSkipping";
      this.chkEnableFrameSkipping.Size = new System.Drawing.Size(369, 20);
      this.chkEnableFrameSkipping.TabIndex = 66;
      this.chkEnableFrameSkipping.Text = "Enable frame skipping";
      this.chkEnableFrameSkipping.UseVisualStyleBackColor = true;
      this.chkEnableFrameSkipping.CheckedChanged += new System.EventHandler(this.ChkEnableFrameSkippingCheckedChanged);
      // 
      // chkInteractiveTracker
      // 
      this.chkInteractiveTracker.Location = new System.Drawing.Point(20, 60);
      this.chkInteractiveTracker.Name = "chkInteractiveTracker";
      this.chkInteractiveTracker.Size = new System.Drawing.Size(369, 20);
      this.chkInteractiveTracker.TabIndex = 67;
      this.chkInteractiveTracker.Text = "dlgPreferences_InteractiveFrameTracker";
      this.chkInteractiveTracker.UseVisualStyleBackColor = true;
      this.chkInteractiveTracker.CheckedChanged += new System.EventHandler(this.chkInteractiveTracker_CheckedChanged);
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
      this.tabImage.ResumeLayout(false);
      this.tabImage.PerformLayout();
      this.tabJumping.ResumeLayout(false);
      this.grpJumping.ResumeLayout(false);
      this.grpJumping.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudSnapSmall)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSnapLarge)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudJumpLarge)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudJumpSmall)).EndInit();
      this.tabPlayer.ResumeLayout(false);
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabControl tabSubPages;
        private System.Windows.Forms.Label lblWorkingZoneMemory;
        private System.Windows.Forms.TrackBar trkMemoryBuffer;
        private System.Windows.Forms.CheckBox chkDetectImageSequences;
        private System.Windows.Forms.Label lblPlaybackKVA;
        private System.Windows.Forms.TextBox tbPlaybackKVA;
        private System.Windows.Forms.Button btnPlaybackKVA;
        private System.Windows.Forms.CheckBox cbCacheInTimeline;
        private System.Windows.Forms.TabPage tabImage;
        private System.Windows.Forms.CheckBox chkEnablePixelFiltering;
        private System.Windows.Forms.ComboBox cmbImageFormats;
        private System.Windows.Forms.Label lblAspectRatio;
        private System.Windows.Forms.CheckBox chkDeinterlace;
        private System.Windows.Forms.TabPage tabJumping;
        private System.Windows.Forms.RadioButton rbJumpByTime;
        private System.Windows.Forms.RadioButton rbSnapToSteps;
        private System.Windows.Forms.GroupBox grpJumping;
        private System.Windows.Forms.NumericUpDown nudJumpLarge;
        private System.Windows.Forms.Label lblJumpLarge;
        private System.Windows.Forms.NumericUpDown nudJumpSmall;
        private System.Windows.Forms.Label lblJumpSmall;
        private System.Windows.Forms.NumericUpDown nudSnapLarge;
        private System.Windows.Forms.Label lblSnapLarge;
        private System.Windows.Forms.NumericUpDown nudSnapSmall;
        private System.Windows.Forms.Label lblSnapSmall;
        private System.Windows.Forms.TabPage tabPlayer;
        private System.Windows.Forms.CheckBox chkInteractiveTracker;
        private System.Windows.Forms.CheckBox chkEnableFrameSkipping;
        private System.Windows.Forms.CheckBox chkSyncByMotion;
        private System.Windows.Forms.CheckBox chkLockSpeeds;
    }
}
