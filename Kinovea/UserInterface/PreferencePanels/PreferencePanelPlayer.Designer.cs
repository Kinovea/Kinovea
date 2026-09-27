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
      this.tabSubPages = new System.Windows.Forms.TabControl();
      this.tabGeneral = new System.Windows.Forms.TabPage();
      this.lblPlaybackKVA = new System.Windows.Forms.Label();
      this.tbPlaybackKVA = new System.Windows.Forms.TextBox();
      this.btnPlaybackKVA = new System.Windows.Forms.Button();
      this.chkDetectImageSequences = new System.Windows.Forms.CheckBox();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.lblCacheMemoryDescription = new System.Windows.Forms.Label();
      this.nudCacheMemory = new System.Windows.Forms.NumericUpDown();
      this.lblCacheMemory = new System.Windows.Forms.Label();
      this.cbCacheInTimeline = new System.Windows.Forms.CheckBox();
      this.tabPlayer = new System.Windows.Forms.TabPage();
      this.chkPreviewScaling = new System.Windows.Forms.CheckBox();
      this.chkHardwareScaling = new System.Windows.Forms.CheckBox();
      this.chkHardwareDecoding = new System.Windows.Forms.CheckBox();
      this.chkLoopPlayback = new System.Windows.Forms.CheckBox();
      this.chkShowFramerate = new System.Windows.Forms.CheckBox();
      this.chkInteractiveTracker = new System.Windows.Forms.CheckBox();
      this.chkEnableFrameSkipping = new System.Windows.Forms.CheckBox();
      this.chkSyncByMotion = new System.Windows.Forms.CheckBox();
      this.chkLockSpeeds = new System.Windows.Forms.CheckBox();
      this.tabJumping = new System.Windows.Forms.TabPage();
      this.grpShortcut = new System.Windows.Forms.GroupBox();
      this.btnDefault = new System.Windows.Forms.Button();
      this.lvCommands = new System.Windows.Forms.ListView();
      this.colCommand = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tbHotkey = new Kinovea.Services.TextboxHotkey();
      this.btnClear = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.grpJumping = new System.Windows.Forms.GroupBox();
      this.cbLargeJump = new System.Windows.Forms.ComboBox();
      this.cbSmallJump = new System.Windows.Forms.ComboBox();
      this.nudLargeJump = new System.Windows.Forms.NumericUpDown();
      this.lblLargeJump = new System.Windows.Forms.Label();
      this.nudSmallJump = new System.Windows.Forms.NumericUpDown();
      this.lblSmallJump = new System.Windows.Forms.Label();
      this.tabImage = new System.Windows.Forms.TabPage();
      this.chkEnablePixelFiltering = new System.Windows.Forms.CheckBox();
      this.cmbImageFormats = new System.Windows.Forms.ComboBox();
      this.lblAspectRatio = new System.Windows.Forms.Label();
      this.chkDeinterlace = new System.Windows.Forms.CheckBox();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabMemory.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCacheMemory)).BeginInit();
      this.tabPlayer.SuspendLayout();
      this.tabJumping.SuspendLayout();
      this.grpShortcut.SuspendLayout();
      this.grpJumping.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudLargeJump)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSmallJump)).BeginInit();
      this.tabImage.SuspendLayout();
      this.SuspendLayout();
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
      this.tabMemory.Controls.Add(this.lblCacheMemoryDescription);
      this.tabMemory.Controls.Add(this.nudCacheMemory);
      this.tabMemory.Controls.Add(this.lblCacheMemory);
      this.tabMemory.Controls.Add(this.cbCacheInTimeline);
      this.tabMemory.Location = new System.Drawing.Point(4, 22);
      this.tabMemory.Name = "tabMemory";
      this.tabMemory.Padding = new System.Windows.Forms.Padding(3);
      this.tabMemory.Size = new System.Drawing.Size(482, 296);
      this.tabMemory.TabIndex = 1;
      this.tabMemory.Text = "Memory";
      this.tabMemory.UseVisualStyleBackColor = true;
      // 
      // lblCacheMemoryDescription
      // 
      this.lblCacheMemoryDescription.Location = new System.Drawing.Point(15, 52);
      this.lblCacheMemoryDescription.Name = "lblCacheMemoryDescription";
      this.lblCacheMemoryDescription.Size = new System.Drawing.Size(452, 37);
      this.lblCacheMemoryDescription.TabIndex = 59;
      this.lblCacheMemoryDescription.Text = "Description";
      // 
      // nudCacheMemory
      // 
      this.nudCacheMemory.DecimalPlaces = 1;
      this.nudCacheMemory.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
      this.nudCacheMemory.Location = new System.Drawing.Point(293, 18);
      this.nudCacheMemory.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
      this.nudCacheMemory.Name = "nudCacheMemory";
      this.nudCacheMemory.Size = new System.Drawing.Size(45, 20);
      this.nudCacheMemory.TabIndex = 58;
      this.nudCacheMemory.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
      this.nudCacheMemory.ValueChanged += new System.EventHandler(this.nudCacheMemory_ValueChanged);
      // 
      // lblCacheMemory
      // 
      this.lblCacheMemory.AutoSize = true;
      this.lblCacheMemory.Location = new System.Drawing.Point(15, 22);
      this.lblCacheMemory.Name = "lblCacheMemory";
      this.lblCacheMemory.Size = new System.Drawing.Size(176, 13);
      this.lblCacheMemory.TabIndex = 57;
      this.lblCacheMemory.Text = "Total playback cache memory (GB):";
      // 
      // cbCacheInTimeline
      // 
      this.cbCacheInTimeline.Location = new System.Drawing.Point(18, 92);
      this.cbCacheInTimeline.Name = "cbCacheInTimeline";
      this.cbCacheInTimeline.Size = new System.Drawing.Size(369, 20);
      this.cbCacheInTimeline.TabIndex = 36;
      this.cbCacheInTimeline.Text = "Show memory indicator in the timeline";
      this.cbCacheInTimeline.UseVisualStyleBackColor = true;
      this.cbCacheInTimeline.CheckedChanged += new System.EventHandler(this.cbCacheInTimeline_CheckedChanged);
      // 
      // tabPlayer
      // 
      this.tabPlayer.Controls.Add(this.chkPreviewScaling);
      this.tabPlayer.Controls.Add(this.chkHardwareScaling);
      this.tabPlayer.Controls.Add(this.chkHardwareDecoding);
      this.tabPlayer.Controls.Add(this.chkLoopPlayback);
      this.tabPlayer.Controls.Add(this.chkShowFramerate);
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
      // chkPreviewScaling
      // 
      this.chkPreviewScaling.Checked = true;
      this.chkPreviewScaling.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkPreviewScaling.Location = new System.Drawing.Point(18, 46);
      this.chkPreviewScaling.Name = "chkPreviewScaling";
      this.chkPreviewScaling.Size = new System.Drawing.Size(369, 20);
      this.chkPreviewScaling.TabIndex = 72;
      this.chkPreviewScaling.Text = "Enable preview scaling";
      this.chkPreviewScaling.UseVisualStyleBackColor = true;
      this.chkPreviewScaling.CheckedChanged += new System.EventHandler(this.chkPreviewScaling_CheckedChanged);
      // 
      // chkHardwareScaling
      // 
      this.chkHardwareScaling.Checked = true;
      this.chkHardwareScaling.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkHardwareScaling.Location = new System.Drawing.Point(18, 263);
      this.chkHardwareScaling.Name = "chkHardwareScaling";
      this.chkHardwareScaling.Size = new System.Drawing.Size(369, 20);
      this.chkHardwareScaling.TabIndex = 71;
      this.chkHardwareScaling.Text = "Enable hardware scaling";
      this.chkHardwareScaling.UseVisualStyleBackColor = true;
      this.chkHardwareScaling.Visible = false;
      this.chkHardwareScaling.CheckedChanged += new System.EventHandler(this.chkHardwareScaling_CheckedChanged);
      // 
      // chkHardwareDecoding
      // 
      this.chkHardwareDecoding.Checked = true;
      this.chkHardwareDecoding.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkHardwareDecoding.Location = new System.Drawing.Point(18, 20);
      this.chkHardwareDecoding.Name = "chkHardwareDecoding";
      this.chkHardwareDecoding.Size = new System.Drawing.Size(369, 20);
      this.chkHardwareDecoding.TabIndex = 70;
      this.chkHardwareDecoding.Text = "Enable hardware decoding";
      this.chkHardwareDecoding.UseVisualStyleBackColor = true;
      this.chkHardwareDecoding.CheckedChanged += new System.EventHandler(this.chkHardwareDecoding_CheckedChanged);
      // 
      // chkLoopPlayback
      // 
      this.chkLoopPlayback.Checked = true;
      this.chkLoopPlayback.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkLoopPlayback.Location = new System.Drawing.Point(18, 150);
      this.chkLoopPlayback.Name = "chkLoopPlayback";
      this.chkLoopPlayback.Size = new System.Drawing.Size(369, 20);
      this.chkLoopPlayback.TabIndex = 69;
      this.chkLoopPlayback.Text = "Loop playback";
      this.chkLoopPlayback.UseVisualStyleBackColor = true;
      this.chkLoopPlayback.CheckedChanged += new System.EventHandler(this.chkLoopPlayback_CheckedChanged);
      // 
      // chkShowFramerate
      // 
      this.chkShowFramerate.Location = new System.Drawing.Point(18, 124);
      this.chkShowFramerate.Name = "chkShowFramerate";
      this.chkShowFramerate.Size = new System.Drawing.Size(369, 20);
      this.chkShowFramerate.TabIndex = 68;
      this.chkShowFramerate.Text = "Show frame rate in speed label";
      this.chkShowFramerate.UseVisualStyleBackColor = true;
      this.chkShowFramerate.CheckedChanged += new System.EventHandler(this.cbShowFramerate_CheckedChanged);
      // 
      // chkInteractiveTracker
      // 
      this.chkInteractiveTracker.Location = new System.Drawing.Point(18, 98);
      this.chkInteractiveTracker.Name = "chkInteractiveTracker";
      this.chkInteractiveTracker.Size = new System.Drawing.Size(369, 20);
      this.chkInteractiveTracker.TabIndex = 67;
      this.chkInteractiveTracker.Text = "dlgPreferences_InteractiveFrameTracker";
      this.chkInteractiveTracker.UseVisualStyleBackColor = true;
      this.chkInteractiveTracker.CheckedChanged += new System.EventHandler(this.chkInteractiveTracker_CheckedChanged);
      // 
      // chkEnableFrameSkipping
      // 
      this.chkEnableFrameSkipping.Checked = true;
      this.chkEnableFrameSkipping.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkEnableFrameSkipping.Location = new System.Drawing.Point(18, 72);
      this.chkEnableFrameSkipping.Name = "chkEnableFrameSkipping";
      this.chkEnableFrameSkipping.Size = new System.Drawing.Size(369, 20);
      this.chkEnableFrameSkipping.TabIndex = 66;
      this.chkEnableFrameSkipping.Text = "Enable frame skipping";
      this.chkEnableFrameSkipping.UseVisualStyleBackColor = true;
      this.chkEnableFrameSkipping.CheckedChanged += new System.EventHandler(this.ChkEnableFrameSkippingCheckedChanged);
      // 
      // chkSyncByMotion
      // 
      this.chkSyncByMotion.Location = new System.Drawing.Point(18, 202);
      this.chkSyncByMotion.Name = "chkSyncByMotion";
      this.chkSyncByMotion.Size = new System.Drawing.Size(369, 20);
      this.chkSyncByMotion.TabIndex = 34;
      this.chkSyncByMotion.Text = "syncByMotion";
      this.chkSyncByMotion.UseVisualStyleBackColor = true;
      this.chkSyncByMotion.CheckedChanged += new System.EventHandler(this.chkSyncByMotion_CheckedChanged);
      // 
      // chkLockSpeeds
      // 
      this.chkLockSpeeds.Location = new System.Drawing.Point(18, 176);
      this.chkLockSpeeds.Name = "chkLockSpeeds";
      this.chkLockSpeeds.Size = new System.Drawing.Size(369, 20);
      this.chkLockSpeeds.TabIndex = 33;
      this.chkLockSpeeds.Text = "dlgPreferences_SyncLockSpeeds";
      this.chkLockSpeeds.UseVisualStyleBackColor = true;
      this.chkLockSpeeds.CheckedChanged += new System.EventHandler(this.ChkLockSpeedsCheckedChanged);
      // 
      // tabJumping
      // 
      this.tabJumping.Controls.Add(this.grpShortcut);
      this.tabJumping.Controls.Add(this.grpJumping);
      this.tabJumping.Location = new System.Drawing.Point(4, 22);
      this.tabJumping.Name = "tabJumping";
      this.tabJumping.Size = new System.Drawing.Size(482, 296);
      this.tabJumping.TabIndex = 3;
      this.tabJumping.Text = "Jumping";
      this.tabJumping.UseVisualStyleBackColor = true;
      // 
      // grpShortcut
      // 
      this.grpShortcut.Controls.Add(this.btnDefault);
      this.grpShortcut.Controls.Add(this.lvCommands);
      this.grpShortcut.Controls.Add(this.tbHotkey);
      this.grpShortcut.Controls.Add(this.btnClear);
      this.grpShortcut.Controls.Add(this.btnApply);
      this.grpShortcut.Location = new System.Drawing.Point(3, 149);
      this.grpShortcut.Name = "grpShortcut";
      this.grpShortcut.Size = new System.Drawing.Size(476, 144);
      this.grpShortcut.TabIndex = 67;
      this.grpShortcut.TabStop = false;
      this.grpShortcut.Text = "Keyboard shortcuts";
      // 
      // btnDefault
      // 
      this.btnDefault.Location = new System.Drawing.Point(392, 115);
      this.btnDefault.Name = "btnDefault";
      this.btnDefault.Size = new System.Drawing.Size(75, 23);
      this.btnDefault.TabIndex = 71;
      this.btnDefault.Text = "Default";
      this.btnDefault.UseVisualStyleBackColor = true;
      this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
      // 
      // lvCommands
      // 
      this.lvCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCommand,
            this.colKey});
      this.lvCommands.FullRowSelect = true;
      this.lvCommands.GridLines = true;
      this.lvCommands.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lvCommands.HideSelection = false;
      this.lvCommands.Location = new System.Drawing.Point(6, 16);
      this.lvCommands.Name = "lvCommands";
      this.lvCommands.Size = new System.Drawing.Size(461, 75);
      this.lvCommands.TabIndex = 67;
      this.lvCommands.UseCompatibleStateImageBehavior = false;
      this.lvCommands.View = System.Windows.Forms.View.Details;
      this.lvCommands.SelectedIndexChanged += new System.EventHandler(this.lvCommands_SelectedIndexChanged);
      // 
      // colCommand
      // 
      this.colCommand.Text = "";
      this.colCommand.Width = 160;
      // 
      // colKey
      // 
      this.colKey.Text = "";
      this.colKey.Width = 129;
      // 
      // tbHotkey
      // 
      this.tbHotkey.Location = new System.Drawing.Point(6, 117);
      this.tbHotkey.Name = "tbHotkey";
      this.tbHotkey.Size = new System.Drawing.Size(218, 20);
      this.tbHotkey.TabIndex = 70;
      this.tbHotkey.Text = "None";
      // 
      // btnClear
      // 
      this.btnClear.Location = new System.Drawing.Point(311, 115);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(75, 23);
      this.btnClear.TabIndex = 69;
      this.btnClear.Text = "Clear";
      this.btnClear.UseVisualStyleBackColor = true;
      this.btnClear.Click += new System.EventHandler(this.btnRemove_Click);
      // 
      // btnApply
      // 
      this.btnApply.Location = new System.Drawing.Point(230, 115);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(75, 23);
      this.btnApply.TabIndex = 68;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // grpJumping
      // 
      this.grpJumping.Controls.Add(this.cbLargeJump);
      this.grpJumping.Controls.Add(this.cbSmallJump);
      this.grpJumping.Controls.Add(this.nudLargeJump);
      this.grpJumping.Controls.Add(this.lblLargeJump);
      this.grpJumping.Controls.Add(this.nudSmallJump);
      this.grpJumping.Controls.Add(this.lblSmallJump);
      this.grpJumping.Location = new System.Drawing.Point(3, 3);
      this.grpJumping.Name = "grpJumping";
      this.grpJumping.Size = new System.Drawing.Size(476, 140);
      this.grpJumping.TabIndex = 66;
      this.grpJumping.TabStop = false;
      this.grpJumping.Text = "Timeline jumping";
      // 
      // cbLargeJump
      // 
      this.cbLargeJump.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbLargeJump.Location = new System.Drawing.Point(240, 56);
      this.cbLargeJump.Name = "cbLargeJump";
      this.cbLargeJump.Size = new System.Drawing.Size(133, 21);
      this.cbLargeJump.TabIndex = 64;
      this.cbLargeJump.SelectedIndexChanged += new System.EventHandler(this.cbLargeJump_SelectedIndexChanged);
      // 
      // cbSmallJump
      // 
      this.cbSmallJump.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbSmallJump.Location = new System.Drawing.Point(240, 25);
      this.cbSmallJump.Name = "cbSmallJump";
      this.cbSmallJump.Size = new System.Drawing.Size(133, 21);
      this.cbSmallJump.TabIndex = 63;
      this.cbSmallJump.SelectedIndexChanged += new System.EventHandler(this.cbSmallJump_SelectedIndexChanged);
      // 
      // nudLargeJump
      // 
      this.nudLargeJump.DecimalPlaces = 1;
      this.nudLargeJump.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.nudLargeJump.Location = new System.Drawing.Point(145, 57);
      this.nudLargeJump.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudLargeJump.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.nudLargeJump.Name = "nudLargeJump";
      this.nudLargeJump.Size = new System.Drawing.Size(55, 20);
      this.nudLargeJump.TabIndex = 58;
      this.nudLargeJump.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.nudLargeJump.ValueChanged += new System.EventHandler(this.nudLargeJump_ValueChanged);
      // 
      // lblLargeJump
      // 
      this.lblLargeJump.AutoSize = true;
      this.lblLargeJump.Location = new System.Drawing.Point(39, 60);
      this.lblLargeJump.Name = "lblLargeJump";
      this.lblLargeJump.Size = new System.Drawing.Size(59, 13);
      this.lblLargeJump.TabIndex = 57;
      this.lblLargeJump.Text = "Long jump:";
      // 
      // nudSmallJump
      // 
      this.nudSmallJump.DecimalPlaces = 1;
      this.nudSmallJump.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.nudSmallJump.Location = new System.Drawing.Point(145, 26);
      this.nudSmallJump.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudSmallJump.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.nudSmallJump.Name = "nudSmallJump";
      this.nudSmallJump.Size = new System.Drawing.Size(55, 20);
      this.nudSmallJump.TabIndex = 56;
      this.nudSmallJump.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
      this.nudSmallJump.ValueChanged += new System.EventHandler(this.nudSmallJump_ValueChanged);
      // 
      // lblSmallJump
      // 
      this.lblSmallJump.AutoSize = true;
      this.lblSmallJump.Location = new System.Drawing.Point(38, 29);
      this.lblSmallJump.Name = "lblSmallJump";
      this.lblSmallJump.Size = new System.Drawing.Size(60, 13);
      this.lblSmallJump.TabIndex = 55;
      this.lblSmallJump.Text = "Short jump:";
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
      this.chkEnablePixelFiltering.Location = new System.Drawing.Point(19, 22);
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
      this.cmbImageFormats.Location = new System.Drawing.Point(259, 52);
      this.cmbImageFormats.Name = "cmbImageFormats";
      this.cmbImageFormats.Size = new System.Drawing.Size(201, 21);
      this.cmbImageFormats.TabIndex = 34;
      this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
      // 
      // lblAspectRatio
      // 
      this.lblAspectRatio.AutoSize = true;
      this.lblAspectRatio.Location = new System.Drawing.Point(16, 55);
      this.lblAspectRatio.Name = "lblAspectRatio";
      this.lblAspectRatio.Size = new System.Drawing.Size(102, 13);
      this.lblAspectRatio.TabIndex = 33;
      this.lblAspectRatio.Text = "Default aspect ratio:";
      // 
      // chkDeinterlace
      // 
      this.chkDeinterlace.Enabled = false;
      this.chkDeinterlace.Location = new System.Drawing.Point(19, 87);
      this.chkDeinterlace.Name = "chkDeinterlace";
      this.chkDeinterlace.Size = new System.Drawing.Size(369, 20);
      this.chkDeinterlace.TabIndex = 32;
      this.chkDeinterlace.Text = "dlgPreferences_DeinterlaceByDefault";
      this.chkDeinterlace.UseVisualStyleBackColor = true;
      this.chkDeinterlace.CheckedChanged += new System.EventHandler(this.chkDeinterlace_CheckedChanged);
      // 
      // PreferencePanelPlayer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.tabSubPages);
      this.Name = "PreferencePanelPlayer";
      this.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.tabGeneral.PerformLayout();
      this.tabMemory.ResumeLayout(false);
      this.tabMemory.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudCacheMemory)).EndInit();
      this.tabPlayer.ResumeLayout(false);
      this.tabJumping.ResumeLayout(false);
      this.grpShortcut.ResumeLayout(false);
      this.grpShortcut.PerformLayout();
      this.grpJumping.ResumeLayout(false);
      this.grpJumping.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudLargeJump)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudSmallJump)).EndInit();
      this.tabImage.ResumeLayout(false);
      this.tabImage.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabControl tabSubPages;
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
        private System.Windows.Forms.GroupBox grpJumping;
        private System.Windows.Forms.NumericUpDown nudLargeJump;
        private System.Windows.Forms.Label lblLargeJump;
        private System.Windows.Forms.NumericUpDown nudSmallJump;
        private System.Windows.Forms.Label lblSmallJump;
        private System.Windows.Forms.TabPage tabPlayer;
        private System.Windows.Forms.CheckBox chkInteractiveTracker;
        private System.Windows.Forms.CheckBox chkEnableFrameSkipping;
        private System.Windows.Forms.CheckBox chkSyncByMotion;
        private System.Windows.Forms.CheckBox chkLockSpeeds;
        private System.Windows.Forms.ListView lvCommands;
        private System.Windows.Forms.ColumnHeader colCommand;
        private System.Windows.Forms.ColumnHeader colKey;
        private System.Windows.Forms.Button btnDefault;
        private Services.TextboxHotkey tbHotkey;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.GroupBox grpShortcut;
        private System.Windows.Forms.ComboBox cbLargeJump;
        private System.Windows.Forms.ComboBox cbSmallJump;
        private System.Windows.Forms.CheckBox chkShowFramerate;
        private System.Windows.Forms.CheckBox chkLoopPlayback;
        private System.Windows.Forms.CheckBox chkHardwareDecoding;
        private System.Windows.Forms.CheckBox chkHardwareScaling;
        private System.Windows.Forms.CheckBox chkPreviewScaling;
        private System.Windows.Forms.Label lblCacheMemoryDescription;
        private System.Windows.Forms.NumericUpDown nudCacheMemory;
        private System.Windows.Forms.Label lblCacheMemory;
    }
}
