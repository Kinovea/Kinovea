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
	partial class PreferencePanelCapture
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
      this.lblCaptureKVA = new System.Windows.Forms.Label();
      this.tbCaptureKVA = new System.Windows.Forms.TextBox();
      this.btnCaptureKVA = new System.Windows.Forms.Button();
      this.chkUncompressedVideo = new System.Windows.Forms.CheckBox();
      this.cmbUncompressedVideoFormat = new System.Windows.Forms.ComboBox();
      this.lblUncompressedVideoFormat = new System.Windows.Forms.Label();
      this.lblFramerate = new System.Windows.Forms.Label();
      this.tbFramerate = new System.Windows.Forms.TextBox();
      this.cmbVideoFormat = new System.Windows.Forms.ComboBox();
      this.lblVideoFormat = new System.Windows.Forms.Label();
      this.cmbImageFormat = new System.Windows.Forms.ComboBox();
      this.lblImageFormat = new System.Windows.Forms.Label();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.lblMemoryBuffer = new System.Windows.Forms.Label();
      this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
      this.tabRecording = new System.Windows.Forms.TabPage();
      this.gbHighspeedCameras = new System.Windows.Forms.GroupBox();
      this.nudReplacementFramerate = new System.Windows.Forms.NumericUpDown();
      this.lblReplacementFramerate = new System.Windows.Forms.Label();
      this.nudReplacementThreshold = new System.Windows.Forms.NumericUpDown();
      this.lblReplacementThreshold = new System.Windows.Forms.Label();
      this.grpRecordingMode = new System.Windows.Forms.GroupBox();
      this.rbRecordingDelayed = new System.Windows.Forms.RadioButton();
      this.rbRecordingScheduled = new System.Windows.Forms.RadioButton();
      this.rbRecordingCamera = new System.Windows.Forms.RadioButton();
      this.tabTrigger = new System.Windows.Forms.TabPage();
      this.cmbDefaultTriggerState = new System.Windows.Forms.ComboBox();
      this.lblDefaultTriggerState = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.nudUDPPort = new System.Windows.Forms.NumericUpDown();
      this.chkEnableUDPTrigger = new System.Windows.Forms.CheckBox();
      this.lblUDPPort = new System.Windows.Forms.Label();
      this.lblUDPTriggerHits = new System.Windows.Forms.Label();
      this.cmbTriggerAction = new System.Windows.Forms.ComboBox();
      this.gbAudioTrigger = new System.Windows.Forms.GroupBox();
      this.nudAudioTriggerThreshold = new System.Windows.Forms.NumericUpDown();
      this.chkEnableAudioTrigger = new System.Windows.Forms.CheckBox();
      this.lblAudioTriggerThreshold = new System.Windows.Forms.Label();
      this.lblInputDevice = new System.Windows.Forms.Label();
      this.vumeter = new Kinovea.Services.VolumeMeterThreshold();
      this.cmbInputDevice = new System.Windows.Forms.ComboBox();
      this.lblAudioTriggerHits = new System.Windows.Forms.Label();
      this.lblTriggerAction = new System.Windows.Forms.Label();
      this.lblQuietPeriod = new System.Windows.Forms.Label();
      this.nudQuietPeriod = new System.Windows.Forms.NumericUpDown();
      this.tabImageNaming = new System.Windows.Forms.TabPage();
      this.grpRightImage = new System.Windows.Forms.GroupBox();
      this.btnRightImageFile = new System.Windows.Forms.Button();
      this.btnRightImageSubdir = new System.Windows.Forms.Button();
      this.btnRightImageRoot = new System.Windows.Forms.Button();
      this.lblRightImageFile = new System.Windows.Forms.Label();
      this.tbRightImageFile = new System.Windows.Forms.TextBox();
      this.lblRightImageSubdir = new System.Windows.Forms.Label();
      this.tbRightImageSubdir = new System.Windows.Forms.TextBox();
      this.lblRightImageRoot = new System.Windows.Forms.Label();
      this.tbRightImageRoot = new System.Windows.Forms.TextBox();
      this.grpLeftImage = new System.Windows.Forms.GroupBox();
      this.btnLeftImageFile = new System.Windows.Forms.Button();
      this.btnLeftImageSubdir = new System.Windows.Forms.Button();
      this.lblLeftImageFile = new System.Windows.Forms.Label();
      this.tbLeftImageFile = new System.Windows.Forms.TextBox();
      this.lblLeftImageSubdir = new System.Windows.Forms.Label();
      this.tbLeftImageSubdir = new System.Windows.Forms.TextBox();
      this.lblLeftImageRoot = new System.Windows.Forms.Label();
      this.tbLeftImageRoot = new System.Windows.Forms.TextBox();
      this.btnLeftImageRoot = new System.Windows.Forms.Button();
      this.tabVideoNaming = new System.Windows.Forms.TabPage();
      this.grpRightVideo = new System.Windows.Forms.GroupBox();
      this.btnRightVideoFile = new System.Windows.Forms.Button();
      this.btnRightVideoSubdir = new System.Windows.Forms.Button();
      this.btnRightVideoRoot = new System.Windows.Forms.Button();
      this.lblRightVideoFile = new System.Windows.Forms.Label();
      this.tbRightVideoFile = new System.Windows.Forms.TextBox();
      this.lblRightVideoSubdir = new System.Windows.Forms.Label();
      this.tbRightVideoSubdir = new System.Windows.Forms.TextBox();
      this.lblRightVideoRoot = new System.Windows.Forms.Label();
      this.tbRightVideoRoot = new System.Windows.Forms.TextBox();
      this.grpLeftVideo = new System.Windows.Forms.GroupBox();
      this.btnLeftVideoRoot = new System.Windows.Forms.Button();
      this.btnLeftVideoFile = new System.Windows.Forms.Button();
      this.btnLeftVideoSubdir = new System.Windows.Forms.Button();
      this.lblLeftVideoFile = new System.Windows.Forms.Label();
      this.tbLeftVideoFile = new System.Windows.Forms.TextBox();
      this.lblLeftVideoSubdir = new System.Windows.Forms.Label();
      this.tbLeftVideoSubdir = new System.Windows.Forms.TextBox();
      this.lblLeftVideoRoot = new System.Windows.Forms.Label();
      this.tbLeftVideoRoot = new System.Windows.Forms.TextBox();
      this.tabAutomation = new System.Windows.Forms.TabPage();
      this.label1 = new System.Windows.Forms.Label();
      this.nudRecordingTime = new System.Windows.Forms.NumericUpDown();
      this.chkIgnoreOverwriteWarning = new System.Windows.Forms.CheckBox();
      this.btnPostRecordCommand = new System.Windows.Forms.Button();
      this.lblPostRecordCommand = new System.Windows.Forms.Label();
      this.tbPostRecordCommand = new System.Windows.Forms.TextBox();
      this.lblRecordingTime = new System.Windows.Forms.Label();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabMemory.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
      this.tabRecording.SuspendLayout();
      this.gbHighspeedCameras.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudReplacementFramerate)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudReplacementThreshold)).BeginInit();
      this.grpRecordingMode.SuspendLayout();
      this.tabTrigger.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudUDPPort)).BeginInit();
      this.gbAudioTrigger.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudAudioTriggerThreshold)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudQuietPeriod)).BeginInit();
      this.tabImageNaming.SuspendLayout();
      this.grpRightImage.SuspendLayout();
      this.grpLeftImage.SuspendLayout();
      this.tabVideoNaming.SuspendLayout();
      this.grpRightVideo.SuspendLayout();
      this.grpLeftVideo.SuspendLayout();
      this.tabAutomation.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudRecordingTime)).BeginInit();
      this.SuspendLayout();
      // 
      // tabSubPages
      // 
      this.tabSubPages.Controls.Add(this.tabGeneral);
      this.tabSubPages.Controls.Add(this.tabMemory);
      this.tabSubPages.Controls.Add(this.tabRecording);
      this.tabSubPages.Controls.Add(this.tabTrigger);
      this.tabSubPages.Controls.Add(this.tabImageNaming);
      this.tabSubPages.Controls.Add(this.tabVideoNaming);
      this.tabSubPages.Controls.Add(this.tabAutomation);
      this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabSubPages.Location = new System.Drawing.Point(0, 0);
      this.tabSubPages.Name = "tabSubPages";
      this.tabSubPages.SelectedIndex = 0;
      this.tabSubPages.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.TabIndex = 0;
      // 
      // tabGeneral
      // 
      this.tabGeneral.Controls.Add(this.lblCaptureKVA);
      this.tabGeneral.Controls.Add(this.tbCaptureKVA);
      this.tabGeneral.Controls.Add(this.btnCaptureKVA);
      this.tabGeneral.Controls.Add(this.chkUncompressedVideo);
      this.tabGeneral.Controls.Add(this.cmbUncompressedVideoFormat);
      this.tabGeneral.Controls.Add(this.lblUncompressedVideoFormat);
      this.tabGeneral.Controls.Add(this.lblFramerate);
      this.tabGeneral.Controls.Add(this.tbFramerate);
      this.tabGeneral.Controls.Add(this.cmbVideoFormat);
      this.tabGeneral.Controls.Add(this.lblVideoFormat);
      this.tabGeneral.Controls.Add(this.cmbImageFormat);
      this.tabGeneral.Controls.Add(this.lblImageFormat);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabGeneral.Size = new System.Drawing.Size(482, 296);
      this.tabGeneral.TabIndex = 0;
      this.tabGeneral.Text = "General";
      this.tabGeneral.UseVisualStyleBackColor = true;
      // 
      // lblCaptureKVA
      // 
      this.lblCaptureKVA.AutoSize = true;
      this.lblCaptureKVA.Location = new System.Drawing.Point(19, 211);
      this.lblCaptureKVA.Name = "lblCaptureKVA";
      this.lblCaptureKVA.Size = new System.Drawing.Size(121, 13);
      this.lblCaptureKVA.TabIndex = 58;
      this.lblCaptureKVA.Text = "Default annotations file :";
      // 
      // tbCaptureKVA
      // 
      this.tbCaptureKVA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbCaptureKVA.Location = new System.Drawing.Point(262, 209);
      this.tbCaptureKVA.Name = "tbCaptureKVA";
      this.tbCaptureKVA.Size = new System.Drawing.Size(175, 20);
      this.tbCaptureKVA.TabIndex = 59;
      this.tbCaptureKVA.TextChanged += new System.EventHandler(this.tbCaptureKVA_TextChanged);
      // 
      // btnCaptureKVA
      // 
      this.btnCaptureKVA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCaptureKVA.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnCaptureKVA.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnCaptureKVA.FlatAppearance.BorderSize = 0;
      this.btnCaptureKVA.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnCaptureKVA.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCaptureKVA.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnCaptureKVA.Location = new System.Drawing.Point(443, 208);
      this.btnCaptureKVA.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnCaptureKVA.Name = "btnCaptureKVA";
      this.btnCaptureKVA.Size = new System.Drawing.Size(20, 20);
      this.btnCaptureKVA.TabIndex = 60;
      this.btnCaptureKVA.Tag = "";
      this.btnCaptureKVA.UseVisualStyleBackColor = true;
      this.btnCaptureKVA.Click += new System.EventHandler(this.btnCaptureKVA_Click);
      // 
      // chkUncompressedVideo
      // 
      this.chkUncompressedVideo.AutoSize = true;
      this.chkUncompressedVideo.Location = new System.Drawing.Point(22, 26);
      this.chkUncompressedVideo.Name = "chkUncompressedVideo";
      this.chkUncompressedVideo.Size = new System.Drawing.Size(152, 17);
      this.chkUncompressedVideo.TabIndex = 47;
      this.chkUncompressedVideo.Text = "Save uncompressed video";
      this.chkUncompressedVideo.UseVisualStyleBackColor = true;
      this.chkUncompressedVideo.CheckedChanged += new System.EventHandler(this.chkUncompressedVideo_CheckedChanged);
      // 
      // cmbUncompressedVideoFormat
      // 
      this.cmbUncompressedVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbUncompressedVideoFormat.FormattingEnabled = true;
      this.cmbUncompressedVideoFormat.Location = new System.Drawing.Point(262, 167);
      this.cmbUncompressedVideoFormat.Name = "cmbUncompressedVideoFormat";
      this.cmbUncompressedVideoFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbUncompressedVideoFormat.TabIndex = 43;
      this.cmbUncompressedVideoFormat.SelectedIndexChanged += new System.EventHandler(this.cmbUncompressedVideoFormat_SelectedIndexChanged);
      // 
      // lblUncompressedVideoFormat
      // 
      this.lblUncompressedVideoFormat.AutoSize = true;
      this.lblUncompressedVideoFormat.Location = new System.Drawing.Point(19, 167);
      this.lblUncompressedVideoFormat.Name = "lblUncompressedVideoFormat";
      this.lblUncompressedVideoFormat.Size = new System.Drawing.Size(145, 13);
      this.lblUncompressedVideoFormat.TabIndex = 42;
      this.lblUncompressedVideoFormat.Text = "Uncompressed video format :";
      // 
      // lblFramerate
      // 
      this.lblFramerate.AutoSize = true;
      this.lblFramerate.Location = new System.Drawing.Point(19, 62);
      this.lblFramerate.Name = "lblFramerate";
      this.lblFramerate.Size = new System.Drawing.Size(117, 13);
      this.lblFramerate.TabIndex = 41;
      this.lblFramerate.Text = "Display framerate (fps) :";
      // 
      // tbFramerate
      // 
      this.tbFramerate.Location = new System.Drawing.Point(262, 62);
      this.tbFramerate.Name = "tbFramerate";
      this.tbFramerate.Size = new System.Drawing.Size(30, 20);
      this.tbFramerate.TabIndex = 40;
      this.tbFramerate.TextChanged += new System.EventHandler(this.tbFramerate_TextChanged);
      // 
      // cmbVideoFormat
      // 
      this.cmbVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbVideoFormat.FormattingEnabled = true;
      this.cmbVideoFormat.Location = new System.Drawing.Point(262, 137);
      this.cmbVideoFormat.Name = "cmbVideoFormat";
      this.cmbVideoFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbVideoFormat.TabIndex = 41;
      this.cmbVideoFormat.SelectedIndexChanged += new System.EventHandler(this.cmbVideoFormat_SelectedIndexChanged);
      // 
      // lblVideoFormat
      // 
      this.lblVideoFormat.AutoSize = true;
      this.lblVideoFormat.Location = new System.Drawing.Point(19, 137);
      this.lblVideoFormat.Name = "lblVideoFormat";
      this.lblVideoFormat.Size = new System.Drawing.Size(72, 13);
      this.lblVideoFormat.TabIndex = 40;
      this.lblVideoFormat.Text = "Video format :";
      // 
      // cmbImageFormat
      // 
      this.cmbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbImageFormat.FormattingEnabled = true;
      this.cmbImageFormat.Location = new System.Drawing.Point(262, 107);
      this.cmbImageFormat.Name = "cmbImageFormat";
      this.cmbImageFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbImageFormat.TabIndex = 5;
      this.cmbImageFormat.SelectedIndexChanged += new System.EventHandler(this.cmbImageFormat_SelectedIndexChanged);
      // 
      // lblImageFormat
      // 
      this.lblImageFormat.AutoSize = true;
      this.lblImageFormat.Location = new System.Drawing.Point(19, 107);
      this.lblImageFormat.Name = "lblImageFormat";
      this.lblImageFormat.Size = new System.Drawing.Size(74, 13);
      this.lblImageFormat.TabIndex = 2;
      this.lblImageFormat.Text = "Image format :";
      // 
      // tabMemory
      // 
      this.tabMemory.Controls.Add(this.lblMemoryBuffer);
      this.tabMemory.Controls.Add(this.trkMemoryBuffer);
      this.tabMemory.Location = new System.Drawing.Point(4, 22);
      this.tabMemory.Name = "tabMemory";
      this.tabMemory.Size = new System.Drawing.Size(482, 296);
      this.tabMemory.TabIndex = 2;
      this.tabMemory.Text = "Memory";
      this.tabMemory.UseVisualStyleBackColor = true;
      // 
      // lblMemoryBuffer
      // 
      this.lblMemoryBuffer.AutoSize = true;
      this.lblMemoryBuffer.Location = new System.Drawing.Point(15, 30);
      this.lblMemoryBuffer.Name = "lblMemoryBuffer";
      this.lblMemoryBuffer.Size = new System.Drawing.Size(221, 13);
      this.lblMemoryBuffer.TabIndex = 36;
      this.lblMemoryBuffer.Text = "Memory allocated for capture buffers : {0} MB";
      // 
      // trkMemoryBuffer
      // 
      this.trkMemoryBuffer.BackColor = System.Drawing.Color.White;
      this.trkMemoryBuffer.Location = new System.Drawing.Point(15, 55);
      this.trkMemoryBuffer.Maximum = 1024;
      this.trkMemoryBuffer.Minimum = 16;
      this.trkMemoryBuffer.Name = "trkMemoryBuffer";
      this.trkMemoryBuffer.Size = new System.Drawing.Size(452, 45);
      this.trkMemoryBuffer.TabIndex = 38;
      this.trkMemoryBuffer.TickFrequency = 50;
      this.trkMemoryBuffer.Value = 16;
      this.trkMemoryBuffer.ValueChanged += new System.EventHandler(this.trkMemoryBuffer_ValueChanged);
      // 
      // tabRecording
      // 
      this.tabRecording.Controls.Add(this.gbHighspeedCameras);
      this.tabRecording.Controls.Add(this.grpRecordingMode);
      this.tabRecording.Location = new System.Drawing.Point(4, 22);
      this.tabRecording.Name = "tabRecording";
      this.tabRecording.Padding = new System.Windows.Forms.Padding(3);
      this.tabRecording.Size = new System.Drawing.Size(482, 296);
      this.tabRecording.TabIndex = 4;
      this.tabRecording.Text = "Recording";
      this.tabRecording.UseVisualStyleBackColor = true;
      // 
      // gbHighspeedCameras
      // 
      this.gbHighspeedCameras.Controls.Add(this.nudReplacementFramerate);
      this.gbHighspeedCameras.Controls.Add(this.lblReplacementFramerate);
      this.gbHighspeedCameras.Controls.Add(this.nudReplacementThreshold);
      this.gbHighspeedCameras.Controls.Add(this.lblReplacementThreshold);
      this.gbHighspeedCameras.Location = new System.Drawing.Point(6, 113);
      this.gbHighspeedCameras.Name = "gbHighspeedCameras";
      this.gbHighspeedCameras.Size = new System.Drawing.Size(470, 94);
      this.gbHighspeedCameras.TabIndex = 41;
      this.gbHighspeedCameras.TabStop = false;
      this.gbHighspeedCameras.Text = "High speed cameras";
      // 
      // nudReplacementFramerate
      // 
      this.nudReplacementFramerate.Location = new System.Drawing.Point(281, 57);
      this.nudReplacementFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.nudReplacementFramerate.Name = "nudReplacementFramerate";
      this.nudReplacementFramerate.Size = new System.Drawing.Size(45, 20);
      this.nudReplacementFramerate.TabIndex = 56;
      this.nudReplacementFramerate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
      this.nudReplacementFramerate.ValueChanged += new System.EventHandler(this.NudReplacementFramerate_ValueChanged);
      // 
      // lblReplacementFramerate
      // 
      this.lblReplacementFramerate.AutoSize = true;
      this.lblReplacementFramerate.Location = new System.Drawing.Point(17, 59);
      this.lblReplacementFramerate.Name = "lblReplacementFramerate";
      this.lblReplacementFramerate.Size = new System.Drawing.Size(120, 13);
      this.lblReplacementFramerate.TabIndex = 55;
      this.lblReplacementFramerate.Text = "Replacement framerate:";
      // 
      // nudReplacementThreshold
      // 
      this.nudReplacementThreshold.Location = new System.Drawing.Point(281, 31);
      this.nudReplacementThreshold.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudReplacementThreshold.Minimum = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.nudReplacementThreshold.Name = "nudReplacementThreshold";
      this.nudReplacementThreshold.Size = new System.Drawing.Size(45, 20);
      this.nudReplacementThreshold.TabIndex = 54;
      this.nudReplacementThreshold.Value = new decimal(new int[] {
            150,
            0,
            0,
            0});
      this.nudReplacementThreshold.ValueChanged += new System.EventHandler(this.NudReplacementThreshold_ValueChanged);
      // 
      // lblReplacementThreshold
      // 
      this.lblReplacementThreshold.AutoSize = true;
      this.lblReplacementThreshold.Location = new System.Drawing.Point(17, 33);
      this.lblReplacementThreshold.Name = "lblReplacementThreshold";
      this.lblReplacementThreshold.Size = new System.Drawing.Size(164, 13);
      this.lblReplacementThreshold.TabIndex = 53;
      this.lblReplacementThreshold.Text = "Framerate replacement threshold:";
      // 
      // grpRecordingMode
      // 
      this.grpRecordingMode.Controls.Add(this.rbRecordingDelayed);
      this.grpRecordingMode.Controls.Add(this.rbRecordingScheduled);
      this.grpRecordingMode.Controls.Add(this.rbRecordingCamera);
      this.grpRecordingMode.Location = new System.Drawing.Point(6, 6);
      this.grpRecordingMode.Name = "grpRecordingMode";
      this.grpRecordingMode.Size = new System.Drawing.Size(470, 101);
      this.grpRecordingMode.TabIndex = 40;
      this.grpRecordingMode.TabStop = false;
      this.grpRecordingMode.Text = "Recording mode";
      // 
      // rbRecordingDelayed
      // 
      this.rbRecordingDelayed.AutoSize = true;
      this.rbRecordingDelayed.Location = new System.Drawing.Point(21, 48);
      this.rbRecordingDelayed.Name = "rbRecordingDelayed";
      this.rbRecordingDelayed.Size = new System.Drawing.Size(228, 17);
      this.rbRecordingDelayed.TabIndex = 39;
      this.rbRecordingDelayed.TabStop = true;
      this.rbRecordingDelayed.Text = "Delayed: records delayed frames on the fly.";
      this.rbRecordingDelayed.UseVisualStyleBackColor = true;
      this.rbRecordingDelayed.CheckedChanged += new System.EventHandler(this.radioRecordingMode_CheckedChanged);
      // 
      // rbRecordingScheduled
      // 
      this.rbRecordingScheduled.AutoSize = true;
      this.rbRecordingScheduled.Location = new System.Drawing.Point(21, 71);
      this.rbRecordingScheduled.Name = "rbRecordingScheduled";
      this.rbRecordingScheduled.Size = new System.Drawing.Size(221, 17);
      this.rbRecordingScheduled.TabIndex = 40;
      this.rbRecordingScheduled.TabStop = true;
      this.rbRecordingScheduled.Text = "Buffered: records delayed frames on stop.";
      this.rbRecordingScheduled.UseVisualStyleBackColor = true;
      this.rbRecordingScheduled.CheckedChanged += new System.EventHandler(this.radioRecordingMode_CheckedChanged);
      // 
      // rbRecordingCamera
      // 
      this.rbRecordingCamera.AutoSize = true;
      this.rbRecordingCamera.Location = new System.Drawing.Point(21, 25);
      this.rbRecordingCamera.Name = "rbRecordingCamera";
      this.rbRecordingCamera.Size = new System.Drawing.Size(227, 17);
      this.rbRecordingCamera.TabIndex = 38;
      this.rbRecordingCamera.TabStop = true;
      this.rbRecordingCamera.Text = "Camera: records real time frames on the fly.";
      this.rbRecordingCamera.UseVisualStyleBackColor = true;
      this.rbRecordingCamera.CheckedChanged += new System.EventHandler(this.radioRecordingMode_CheckedChanged);
      // 
      // tabTrigger
      // 
      this.tabTrigger.Controls.Add(this.cmbDefaultTriggerState);
      this.tabTrigger.Controls.Add(this.lblDefaultTriggerState);
      this.tabTrigger.Controls.Add(this.groupBox1);
      this.tabTrigger.Controls.Add(this.cmbTriggerAction);
      this.tabTrigger.Controls.Add(this.gbAudioTrigger);
      this.tabTrigger.Controls.Add(this.lblTriggerAction);
      this.tabTrigger.Controls.Add(this.lblQuietPeriod);
      this.tabTrigger.Controls.Add(this.nudQuietPeriod);
      this.tabTrigger.Location = new System.Drawing.Point(4, 22);
      this.tabTrigger.Name = "tabTrigger";
      this.tabTrigger.Padding = new System.Windows.Forms.Padding(3);
      this.tabTrigger.Size = new System.Drawing.Size(482, 296);
      this.tabTrigger.TabIndex = 6;
      this.tabTrigger.Text = "Trigger";
      this.tabTrigger.UseVisualStyleBackColor = true;
      // 
      // cmbDefaultTriggerState
      // 
      this.cmbDefaultTriggerState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbDefaultTriggerState.FormattingEnabled = true;
      this.cmbDefaultTriggerState.Location = new System.Drawing.Point(205, 202);
      this.cmbDefaultTriggerState.Name = "cmbDefaultTriggerState";
      this.cmbDefaultTriggerState.Size = new System.Drawing.Size(254, 21);
      this.cmbDefaultTriggerState.TabIndex = 62;
      this.cmbDefaultTriggerState.SelectedIndexChanged += new System.EventHandler(this.cmbDefaultTriggerState_SelectedIndexChanged);
      // 
      // lblDefaultTriggerState
      // 
      this.lblDefaultTriggerState.AutoSize = true;
      this.lblDefaultTriggerState.Location = new System.Drawing.Point(17, 205);
      this.lblDefaultTriggerState.Name = "lblDefaultTriggerState";
      this.lblDefaultTriggerState.Size = new System.Drawing.Size(102, 13);
      this.lblDefaultTriggerState.TabIndex = 61;
      this.lblDefaultTriggerState.Text = "Default trigger state:";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.nudUDPPort);
      this.groupBox1.Controls.Add(this.chkEnableUDPTrigger);
      this.groupBox1.Controls.Add(this.lblUDPPort);
      this.groupBox1.Controls.Add(this.lblUDPTriggerHits);
      this.groupBox1.Location = new System.Drawing.Point(10, 118);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(466, 75);
      this.groupBox1.TabIndex = 57;
      this.groupBox1.TabStop = false;
      // 
      // nudUDPPort
      // 
      this.nudUDPPort.Location = new System.Drawing.Point(195, 41);
      this.nudUDPPort.Maximum = new decimal(new int[] {
            49151,
            0,
            0,
            0});
      this.nudUDPPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
      this.nudUDPPort.Name = "nudUDPPort";
      this.nudUDPPort.Size = new System.Drawing.Size(50, 20);
      this.nudUDPPort.TabIndex = 52;
      this.nudUDPPort.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
      this.nudUDPPort.ValueChanged += new System.EventHandler(this.nudUDPPort_ValueChanged);
      // 
      // chkEnableUDPTrigger
      // 
      this.chkEnableUDPTrigger.AutoSize = true;
      this.chkEnableUDPTrigger.Location = new System.Drawing.Point(10, 16);
      this.chkEnableUDPTrigger.Name = "chkEnableUDPTrigger";
      this.chkEnableUDPTrigger.Size = new System.Drawing.Size(117, 17);
      this.chkEnableUDPTrigger.TabIndex = 44;
      this.chkEnableUDPTrigger.Text = "Enable UDP trigger";
      this.chkEnableUDPTrigger.UseVisualStyleBackColor = true;
      this.chkEnableUDPTrigger.CheckedChanged += new System.EventHandler(this.chkEnableUDPTrigger_CheckedChanged);
      // 
      // lblUDPPort
      // 
      this.lblUDPPort.AutoSize = true;
      this.lblUDPPort.Location = new System.Drawing.Point(37, 43);
      this.lblUDPPort.Name = "lblUDPPort";
      this.lblUDPPort.Size = new System.Drawing.Size(55, 13);
      this.lblUDPPort.TabIndex = 47;
      this.lblUDPPort.Text = "UDP Port:";
      // 
      // lblUDPTriggerHits
      // 
      this.lblUDPTriggerHits.AutoSize = true;
      this.lblUDPTriggerHits.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblUDPTriggerHits.Location = new System.Drawing.Point(433, 41);
      this.lblUDPTriggerHits.Name = "lblUDPTriggerHits";
      this.lblUDPTriggerHits.Size = new System.Drawing.Size(14, 16);
      this.lblUDPTriggerHits.TabIndex = 49;
      this.lblUDPTriggerHits.Text = "0";
      // 
      // cmbTriggerAction
      // 
      this.cmbTriggerAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTriggerAction.FormattingEnabled = true;
      this.cmbTriggerAction.Location = new System.Drawing.Point(205, 235);
      this.cmbTriggerAction.Name = "cmbTriggerAction";
      this.cmbTriggerAction.Size = new System.Drawing.Size(254, 21);
      this.cmbTriggerAction.TabIndex = 60;
      this.cmbTriggerAction.SelectedIndexChanged += new System.EventHandler(this.cmbTriggerAction_SelectedIndexChanged);
      // 
      // gbAudioTrigger
      // 
      this.gbAudioTrigger.Controls.Add(this.nudAudioTriggerThreshold);
      this.gbAudioTrigger.Controls.Add(this.chkEnableAudioTrigger);
      this.gbAudioTrigger.Controls.Add(this.lblAudioTriggerThreshold);
      this.gbAudioTrigger.Controls.Add(this.lblInputDevice);
      this.gbAudioTrigger.Controls.Add(this.vumeter);
      this.gbAudioTrigger.Controls.Add(this.cmbInputDevice);
      this.gbAudioTrigger.Controls.Add(this.lblAudioTriggerHits);
      this.gbAudioTrigger.Location = new System.Drawing.Point(10, 6);
      this.gbAudioTrigger.Name = "gbAudioTrigger";
      this.gbAudioTrigger.Size = new System.Drawing.Size(466, 106);
      this.gbAudioTrigger.TabIndex = 56;
      this.gbAudioTrigger.TabStop = false;
      // 
      // nudAudioTriggerThreshold
      // 
      this.nudAudioTriggerThreshold.DecimalPlaces = 1;
      this.nudAudioTriggerThreshold.Location = new System.Drawing.Point(195, 70);
      this.nudAudioTriggerThreshold.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.nudAudioTriggerThreshold.Name = "nudAudioTriggerThreshold";
      this.nudAudioTriggerThreshold.Size = new System.Drawing.Size(42, 20);
      this.nudAudioTriggerThreshold.TabIndex = 52;
      this.nudAudioTriggerThreshold.Value = new decimal(new int[] {
            59,
            0,
            0,
            0});
      this.nudAudioTriggerThreshold.ValueChanged += new System.EventHandler(this.NudAudioTriggerThreshold_ValueChanged);
      // 
      // chkEnableAudioTrigger
      // 
      this.chkEnableAudioTrigger.AutoSize = true;
      this.chkEnableAudioTrigger.Location = new System.Drawing.Point(10, 16);
      this.chkEnableAudioTrigger.Name = "chkEnableAudioTrigger";
      this.chkEnableAudioTrigger.Size = new System.Drawing.Size(120, 17);
      this.chkEnableAudioTrigger.TabIndex = 44;
      this.chkEnableAudioTrigger.Text = "Enable audio trigger";
      this.chkEnableAudioTrigger.UseVisualStyleBackColor = true;
      this.chkEnableAudioTrigger.CheckedChanged += new System.EventHandler(this.chkEnableAudioTrigger_CheckedChanged);
      // 
      // lblAudioTriggerThreshold
      // 
      this.lblAudioTriggerThreshold.AutoSize = true;
      this.lblAudioTriggerThreshold.Location = new System.Drawing.Point(37, 74);
      this.lblAudioTriggerThreshold.Name = "lblAudioTriggerThreshold";
      this.lblAudioTriggerThreshold.Size = new System.Drawing.Size(115, 13);
      this.lblAudioTriggerThreshold.TabIndex = 46;
      this.lblAudioTriggerThreshold.Text = "Audio trigger threshold:";
      // 
      // lblInputDevice
      // 
      this.lblInputDevice.AutoSize = true;
      this.lblInputDevice.Location = new System.Drawing.Point(37, 43);
      this.lblInputDevice.Name = "lblInputDevice";
      this.lblInputDevice.Size = new System.Drawing.Size(114, 13);
      this.lblInputDevice.TabIndex = 47;
      this.lblInputDevice.Text = "Preferred input device:";
      // 
      // vumeter
      // 
      this.vumeter.Amplitude = 0F;
      this.vumeter.BackColor = System.Drawing.Color.White;
      this.vumeter.DecibelRange = 60F;
      this.vumeter.Location = new System.Drawing.Point(249, 70);
      this.vumeter.Name = "vumeter";
      this.vumeter.Size = new System.Drawing.Size(175, 21);
      this.vumeter.TabIndex = 51;
      this.vumeter.Text = "volumeMeterThreshold1";
      this.vumeter.Threshold = 0.001F;
      this.vumeter.ThresholdLinear = 0F;
      this.vumeter.ThresholdChanged += new System.EventHandler(this.Vumeter_ThresholdChanged);
      // 
      // cmbInputDevice
      // 
      this.cmbInputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbInputDevice.FormattingEnabled = true;
      this.cmbInputDevice.Location = new System.Drawing.Point(195, 40);
      this.cmbInputDevice.Name = "cmbInputDevice";
      this.cmbInputDevice.Size = new System.Drawing.Size(254, 21);
      this.cmbInputDevice.TabIndex = 48;
      this.cmbInputDevice.SelectedIndexChanged += new System.EventHandler(this.cmbInputDevice_SelectedIndexChanged);
      // 
      // lblAudioTriggerHits
      // 
      this.lblAudioTriggerHits.AutoSize = true;
      this.lblAudioTriggerHits.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblAudioTriggerHits.Location = new System.Drawing.Point(433, 72);
      this.lblAudioTriggerHits.Name = "lblAudioTriggerHits";
      this.lblAudioTriggerHits.Size = new System.Drawing.Size(14, 16);
      this.lblAudioTriggerHits.TabIndex = 49;
      this.lblAudioTriggerHits.Text = "0";
      // 
      // lblTriggerAction
      // 
      this.lblTriggerAction.AutoSize = true;
      this.lblTriggerAction.Location = new System.Drawing.Point(17, 238);
      this.lblTriggerAction.Name = "lblTriggerAction";
      this.lblTriggerAction.Size = new System.Drawing.Size(75, 13);
      this.lblTriggerAction.TabIndex = 58;
      this.lblTriggerAction.Text = "Trigger action:";
      // 
      // lblQuietPeriod
      // 
      this.lblQuietPeriod.AutoSize = true;
      this.lblQuietPeriod.Location = new System.Drawing.Point(18, 268);
      this.lblQuietPeriod.Name = "lblQuietPeriod";
      this.lblQuietPeriod.Size = new System.Drawing.Size(84, 13);
      this.lblQuietPeriod.TabIndex = 58;
      this.lblQuietPeriod.Text = "Quiet period (s) :";
      // 
      // nudQuietPeriod
      // 
      this.nudQuietPeriod.DecimalPlaces = 1;
      this.nudQuietPeriod.Location = new System.Drawing.Point(205, 266);
      this.nudQuietPeriod.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudQuietPeriod.Name = "nudQuietPeriod";
      this.nudQuietPeriod.Size = new System.Drawing.Size(42, 20);
      this.nudQuietPeriod.TabIndex = 59;
      this.nudQuietPeriod.ValueChanged += new System.EventHandler(this.nudQuietPeriod_ValueChanged);
      // 
      // tabImageNaming
      // 
      this.tabImageNaming.Controls.Add(this.grpRightImage);
      this.tabImageNaming.Controls.Add(this.grpLeftImage);
      this.tabImageNaming.Location = new System.Drawing.Point(4, 22);
      this.tabImageNaming.Name = "tabImageNaming";
      this.tabImageNaming.Padding = new System.Windows.Forms.Padding(3);
      this.tabImageNaming.Size = new System.Drawing.Size(482, 296);
      this.tabImageNaming.TabIndex = 1;
      this.tabImageNaming.Text = "Image naming";
      this.tabImageNaming.UseVisualStyleBackColor = true;
      // 
      // grpRightImage
      // 
      this.grpRightImage.Controls.Add(this.btnRightImageFile);
      this.grpRightImage.Controls.Add(this.btnRightImageSubdir);
      this.grpRightImage.Controls.Add(this.btnRightImageRoot);
      this.grpRightImage.Controls.Add(this.lblRightImageFile);
      this.grpRightImage.Controls.Add(this.tbRightImageFile);
      this.grpRightImage.Controls.Add(this.lblRightImageSubdir);
      this.grpRightImage.Controls.Add(this.tbRightImageSubdir);
      this.grpRightImage.Controls.Add(this.lblRightImageRoot);
      this.grpRightImage.Controls.Add(this.tbRightImageRoot);
      this.grpRightImage.Location = new System.Drawing.Point(6, 106);
      this.grpRightImage.Name = "grpRightImage";
      this.grpRightImage.Size = new System.Drawing.Size(470, 94);
      this.grpRightImage.TabIndex = 47;
      this.grpRightImage.TabStop = false;
      this.grpRightImage.Text = "Right";
      // 
      // btnRightImageFile
      // 
      this.btnRightImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightImageFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightImageFile.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightImageFile.FlatAppearance.BorderSize = 0;
      this.btnRightImageFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightImageFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightImageFile.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnRightImageFile.Location = new System.Drawing.Point(440, 64);
      this.btnRightImageFile.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightImageFile.Name = "btnRightImageFile";
      this.btnRightImageFile.Size = new System.Drawing.Size(20, 20);
      this.btnRightImageFile.TabIndex = 51;
      this.btnRightImageFile.Tag = "";
      this.btnRightImageFile.UseVisualStyleBackColor = true;
      this.btnRightImageFile.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnRightImageSubdir
      // 
      this.btnRightImageSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightImageSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightImageSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightImageSubdir.FlatAppearance.BorderSize = 0;
      this.btnRightImageSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightImageSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightImageSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnRightImageSubdir.Location = new System.Drawing.Point(440, 38);
      this.btnRightImageSubdir.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightImageSubdir.Name = "btnRightImageSubdir";
      this.btnRightImageSubdir.Size = new System.Drawing.Size(20, 20);
      this.btnRightImageSubdir.TabIndex = 50;
      this.btnRightImageSubdir.Tag = "";
      this.btnRightImageSubdir.UseVisualStyleBackColor = true;
      this.btnRightImageSubdir.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnRightImageRoot
      // 
      this.btnRightImageRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightImageRoot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightImageRoot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightImageRoot.FlatAppearance.BorderSize = 0;
      this.btnRightImageRoot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightImageRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightImageRoot.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnRightImageRoot.Location = new System.Drawing.Point(440, 12);
      this.btnRightImageRoot.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightImageRoot.Name = "btnRightImageRoot";
      this.btnRightImageRoot.Size = new System.Drawing.Size(20, 20);
      this.btnRightImageRoot.TabIndex = 49;
      this.btnRightImageRoot.Tag = "";
      this.btnRightImageRoot.UseVisualStyleBackColor = true;
      this.btnRightImageRoot.Click += new System.EventHandler(this.btnFolderSelection_Click);
      // 
      // lblRightImageFile
      // 
      this.lblRightImageFile.AutoSize = true;
      this.lblRightImageFile.Location = new System.Drawing.Point(6, 68);
      this.lblRightImageFile.Name = "lblRightImageFile";
      this.lblRightImageFile.Size = new System.Drawing.Size(29, 13);
      this.lblRightImageFile.TabIndex = 45;
      this.lblRightImageFile.Text = "File :";
      // 
      // tbRightImageFile
      // 
      this.tbRightImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightImageFile.Location = new System.Drawing.Point(145, 65);
      this.tbRightImageFile.Name = "tbRightImageFile";
      this.tbRightImageFile.Size = new System.Drawing.Size(289, 20);
      this.tbRightImageFile.TabIndex = 46;
      // 
      // lblRightImageSubdir
      // 
      this.lblRightImageSubdir.AutoSize = true;
      this.lblRightImageSubdir.Location = new System.Drawing.Point(6, 42);
      this.lblRightImageSubdir.Name = "lblRightImageSubdir";
      this.lblRightImageSubdir.Size = new System.Drawing.Size(75, 13);
      this.lblRightImageSubdir.TabIndex = 43;
      this.lblRightImageSubdir.Text = "Sub directory :";
      // 
      // tbRightImageSubdir
      // 
      this.tbRightImageSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightImageSubdir.Location = new System.Drawing.Point(145, 39);
      this.tbRightImageSubdir.Name = "tbRightImageSubdir";
      this.tbRightImageSubdir.Size = new System.Drawing.Size(289, 20);
      this.tbRightImageSubdir.TabIndex = 44;
      // 
      // lblRightImageRoot
      // 
      this.lblRightImageRoot.AutoSize = true;
      this.lblRightImageRoot.Location = new System.Drawing.Point(6, 16);
      this.lblRightImageRoot.Name = "lblRightImageRoot";
      this.lblRightImageRoot.Size = new System.Drawing.Size(36, 13);
      this.lblRightImageRoot.TabIndex = 38;
      this.lblRightImageRoot.Text = "Root :";
      // 
      // tbRightImageRoot
      // 
      this.tbRightImageRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightImageRoot.Location = new System.Drawing.Point(145, 13);
      this.tbRightImageRoot.Name = "tbRightImageRoot";
      this.tbRightImageRoot.Size = new System.Drawing.Size(289, 20);
      this.tbRightImageRoot.TabIndex = 40;
      // 
      // grpLeftImage
      // 
      this.grpLeftImage.Controls.Add(this.btnLeftImageFile);
      this.grpLeftImage.Controls.Add(this.btnLeftImageSubdir);
      this.grpLeftImage.Controls.Add(this.lblLeftImageFile);
      this.grpLeftImage.Controls.Add(this.tbLeftImageFile);
      this.grpLeftImage.Controls.Add(this.lblLeftImageSubdir);
      this.grpLeftImage.Controls.Add(this.tbLeftImageSubdir);
      this.grpLeftImage.Controls.Add(this.lblLeftImageRoot);
      this.grpLeftImage.Controls.Add(this.tbLeftImageRoot);
      this.grpLeftImage.Controls.Add(this.btnLeftImageRoot);
      this.grpLeftImage.Location = new System.Drawing.Point(6, 6);
      this.grpLeftImage.Name = "grpLeftImage";
      this.grpLeftImage.Size = new System.Drawing.Size(470, 94);
      this.grpLeftImage.TabIndex = 44;
      this.grpLeftImage.TabStop = false;
      this.grpLeftImage.Text = "Left";
      // 
      // btnLeftImageFile
      // 
      this.btnLeftImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftImageFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftImageFile.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftImageFile.FlatAppearance.BorderSize = 0;
      this.btnLeftImageFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftImageFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftImageFile.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnLeftImageFile.Location = new System.Drawing.Point(440, 64);
      this.btnLeftImageFile.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftImageFile.Name = "btnLeftImageFile";
      this.btnLeftImageFile.Size = new System.Drawing.Size(20, 20);
      this.btnLeftImageFile.TabIndex = 48;
      this.btnLeftImageFile.Tag = "";
      this.btnLeftImageFile.UseVisualStyleBackColor = true;
      this.btnLeftImageFile.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnLeftImageSubdir
      // 
      this.btnLeftImageSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftImageSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftImageSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftImageSubdir.FlatAppearance.BorderSize = 0;
      this.btnLeftImageSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftImageSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftImageSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnLeftImageSubdir.Location = new System.Drawing.Point(440, 38);
      this.btnLeftImageSubdir.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftImageSubdir.Name = "btnLeftImageSubdir";
      this.btnLeftImageSubdir.Size = new System.Drawing.Size(20, 20);
      this.btnLeftImageSubdir.TabIndex = 47;
      this.btnLeftImageSubdir.Tag = "";
      this.btnLeftImageSubdir.UseVisualStyleBackColor = true;
      this.btnLeftImageSubdir.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // lblLeftImageFile
      // 
      this.lblLeftImageFile.AutoSize = true;
      this.lblLeftImageFile.Location = new System.Drawing.Point(6, 68);
      this.lblLeftImageFile.Name = "lblLeftImageFile";
      this.lblLeftImageFile.Size = new System.Drawing.Size(29, 13);
      this.lblLeftImageFile.TabIndex = 45;
      this.lblLeftImageFile.Text = "File :";
      // 
      // tbLeftImageFile
      // 
      this.tbLeftImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftImageFile.Location = new System.Drawing.Point(145, 65);
      this.tbLeftImageFile.Name = "tbLeftImageFile";
      this.tbLeftImageFile.Size = new System.Drawing.Size(289, 20);
      this.tbLeftImageFile.TabIndex = 46;
      // 
      // lblLeftImageSubdir
      // 
      this.lblLeftImageSubdir.AutoSize = true;
      this.lblLeftImageSubdir.Location = new System.Drawing.Point(6, 42);
      this.lblLeftImageSubdir.Name = "lblLeftImageSubdir";
      this.lblLeftImageSubdir.Size = new System.Drawing.Size(75, 13);
      this.lblLeftImageSubdir.TabIndex = 43;
      this.lblLeftImageSubdir.Text = "Sub directory :";
      // 
      // tbLeftImageSubdir
      // 
      this.tbLeftImageSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftImageSubdir.Location = new System.Drawing.Point(145, 39);
      this.tbLeftImageSubdir.Name = "tbLeftImageSubdir";
      this.tbLeftImageSubdir.Size = new System.Drawing.Size(289, 20);
      this.tbLeftImageSubdir.TabIndex = 44;
      // 
      // lblLeftImageRoot
      // 
      this.lblLeftImageRoot.AutoSize = true;
      this.lblLeftImageRoot.Location = new System.Drawing.Point(6, 16);
      this.lblLeftImageRoot.Name = "lblLeftImageRoot";
      this.lblLeftImageRoot.Size = new System.Drawing.Size(36, 13);
      this.lblLeftImageRoot.TabIndex = 38;
      this.lblLeftImageRoot.Text = "Root :";
      // 
      // tbLeftImageRoot
      // 
      this.tbLeftImageRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftImageRoot.Location = new System.Drawing.Point(145, 13);
      this.tbLeftImageRoot.Name = "tbLeftImageRoot";
      this.tbLeftImageRoot.Size = new System.Drawing.Size(289, 20);
      this.tbLeftImageRoot.TabIndex = 40;
      // 
      // btnLeftImageRoot
      // 
      this.btnLeftImageRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftImageRoot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftImageRoot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftImageRoot.FlatAppearance.BorderSize = 0;
      this.btnLeftImageRoot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftImageRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftImageRoot.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnLeftImageRoot.Location = new System.Drawing.Point(440, 12);
      this.btnLeftImageRoot.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftImageRoot.Name = "btnLeftImageRoot";
      this.btnLeftImageRoot.Size = new System.Drawing.Size(20, 20);
      this.btnLeftImageRoot.TabIndex = 42;
      this.btnLeftImageRoot.Tag = "";
      this.btnLeftImageRoot.UseVisualStyleBackColor = true;
      this.btnLeftImageRoot.Click += new System.EventHandler(this.btnFolderSelection_Click);
      // 
      // tabVideoNaming
      // 
      this.tabVideoNaming.Controls.Add(this.grpRightVideo);
      this.tabVideoNaming.Controls.Add(this.grpLeftVideo);
      this.tabVideoNaming.Location = new System.Drawing.Point(4, 22);
      this.tabVideoNaming.Name = "tabVideoNaming";
      this.tabVideoNaming.Padding = new System.Windows.Forms.Padding(3);
      this.tabVideoNaming.Size = new System.Drawing.Size(482, 296);
      this.tabVideoNaming.TabIndex = 3;
      this.tabVideoNaming.Text = "Video naming";
      this.tabVideoNaming.UseVisualStyleBackColor = true;
      // 
      // grpRightVideo
      // 
      this.grpRightVideo.Controls.Add(this.btnRightVideoFile);
      this.grpRightVideo.Controls.Add(this.btnRightVideoSubdir);
      this.grpRightVideo.Controls.Add(this.btnRightVideoRoot);
      this.grpRightVideo.Controls.Add(this.lblRightVideoFile);
      this.grpRightVideo.Controls.Add(this.tbRightVideoFile);
      this.grpRightVideo.Controls.Add(this.lblRightVideoSubdir);
      this.grpRightVideo.Controls.Add(this.tbRightVideoSubdir);
      this.grpRightVideo.Controls.Add(this.lblRightVideoRoot);
      this.grpRightVideo.Controls.Add(this.tbRightVideoRoot);
      this.grpRightVideo.Location = new System.Drawing.Point(6, 106);
      this.grpRightVideo.Name = "grpRightVideo";
      this.grpRightVideo.Size = new System.Drawing.Size(470, 94);
      this.grpRightVideo.TabIndex = 49;
      this.grpRightVideo.TabStop = false;
      this.grpRightVideo.Text = "Right";
      // 
      // btnRightVideoFile
      // 
      this.btnRightVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightVideoFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightVideoFile.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightVideoFile.FlatAppearance.BorderSize = 0;
      this.btnRightVideoFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightVideoFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightVideoFile.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnRightVideoFile.Location = new System.Drawing.Point(440, 64);
      this.btnRightVideoFile.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightVideoFile.Name = "btnRightVideoFile";
      this.btnRightVideoFile.Size = new System.Drawing.Size(20, 20);
      this.btnRightVideoFile.TabIndex = 51;
      this.btnRightVideoFile.Tag = "";
      this.btnRightVideoFile.UseVisualStyleBackColor = true;
      this.btnRightVideoFile.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnRightVideoSubdir
      // 
      this.btnRightVideoSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightVideoSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightVideoSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightVideoSubdir.FlatAppearance.BorderSize = 0;
      this.btnRightVideoSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightVideoSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightVideoSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnRightVideoSubdir.Location = new System.Drawing.Point(440, 38);
      this.btnRightVideoSubdir.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightVideoSubdir.Name = "btnRightVideoSubdir";
      this.btnRightVideoSubdir.Size = new System.Drawing.Size(20, 20);
      this.btnRightVideoSubdir.TabIndex = 50;
      this.btnRightVideoSubdir.Tag = "";
      this.btnRightVideoSubdir.UseVisualStyleBackColor = true;
      this.btnRightVideoSubdir.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnRightVideoRoot
      // 
      this.btnRightVideoRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRightVideoRoot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRightVideoRoot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRightVideoRoot.FlatAppearance.BorderSize = 0;
      this.btnRightVideoRoot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnRightVideoRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRightVideoRoot.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnRightVideoRoot.Location = new System.Drawing.Point(440, 12);
      this.btnRightVideoRoot.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightVideoRoot.Name = "btnRightVideoRoot";
      this.btnRightVideoRoot.Size = new System.Drawing.Size(20, 20);
      this.btnRightVideoRoot.TabIndex = 49;
      this.btnRightVideoRoot.Tag = "";
      this.btnRightVideoRoot.UseVisualStyleBackColor = true;
      this.btnRightVideoRoot.Click += new System.EventHandler(this.btnFolderSelection_Click);
      // 
      // lblRightVideoFile
      // 
      this.lblRightVideoFile.AutoSize = true;
      this.lblRightVideoFile.Location = new System.Drawing.Point(6, 68);
      this.lblRightVideoFile.Name = "lblRightVideoFile";
      this.lblRightVideoFile.Size = new System.Drawing.Size(29, 13);
      this.lblRightVideoFile.TabIndex = 45;
      this.lblRightVideoFile.Text = "File :";
      // 
      // tbRightVideoFile
      // 
      this.tbRightVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightVideoFile.Location = new System.Drawing.Point(145, 65);
      this.tbRightVideoFile.Name = "tbRightVideoFile";
      this.tbRightVideoFile.Size = new System.Drawing.Size(289, 20);
      this.tbRightVideoFile.TabIndex = 46;
      // 
      // lblRightVideoSubdir
      // 
      this.lblRightVideoSubdir.AutoSize = true;
      this.lblRightVideoSubdir.Location = new System.Drawing.Point(6, 42);
      this.lblRightVideoSubdir.Name = "lblRightVideoSubdir";
      this.lblRightVideoSubdir.Size = new System.Drawing.Size(75, 13);
      this.lblRightVideoSubdir.TabIndex = 43;
      this.lblRightVideoSubdir.Text = "Sub directory :";
      // 
      // tbRightVideoSubdir
      // 
      this.tbRightVideoSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightVideoSubdir.Location = new System.Drawing.Point(145, 39);
      this.tbRightVideoSubdir.Name = "tbRightVideoSubdir";
      this.tbRightVideoSubdir.Size = new System.Drawing.Size(289, 20);
      this.tbRightVideoSubdir.TabIndex = 44;
      // 
      // lblRightVideoRoot
      // 
      this.lblRightVideoRoot.AutoSize = true;
      this.lblRightVideoRoot.Location = new System.Drawing.Point(6, 16);
      this.lblRightVideoRoot.Name = "lblRightVideoRoot";
      this.lblRightVideoRoot.Size = new System.Drawing.Size(36, 13);
      this.lblRightVideoRoot.TabIndex = 38;
      this.lblRightVideoRoot.Text = "Root :";
      // 
      // tbRightVideoRoot
      // 
      this.tbRightVideoRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbRightVideoRoot.Location = new System.Drawing.Point(145, 13);
      this.tbRightVideoRoot.Name = "tbRightVideoRoot";
      this.tbRightVideoRoot.Size = new System.Drawing.Size(289, 20);
      this.tbRightVideoRoot.TabIndex = 40;
      // 
      // grpLeftVideo
      // 
      this.grpLeftVideo.Controls.Add(this.btnLeftVideoRoot);
      this.grpLeftVideo.Controls.Add(this.btnLeftVideoFile);
      this.grpLeftVideo.Controls.Add(this.btnLeftVideoSubdir);
      this.grpLeftVideo.Controls.Add(this.lblLeftVideoFile);
      this.grpLeftVideo.Controls.Add(this.tbLeftVideoFile);
      this.grpLeftVideo.Controls.Add(this.lblLeftVideoSubdir);
      this.grpLeftVideo.Controls.Add(this.tbLeftVideoSubdir);
      this.grpLeftVideo.Controls.Add(this.lblLeftVideoRoot);
      this.grpLeftVideo.Controls.Add(this.tbLeftVideoRoot);
      this.grpLeftVideo.Location = new System.Drawing.Point(6, 6);
      this.grpLeftVideo.Name = "grpLeftVideo";
      this.grpLeftVideo.Size = new System.Drawing.Size(470, 94);
      this.grpLeftVideo.TabIndex = 48;
      this.grpLeftVideo.TabStop = false;
      this.grpLeftVideo.Text = "Left";
      // 
      // btnLeftVideoRoot
      // 
      this.btnLeftVideoRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftVideoRoot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftVideoRoot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftVideoRoot.FlatAppearance.BorderSize = 0;
      this.btnLeftVideoRoot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftVideoRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftVideoRoot.Image = global::Kinovea.Root.Properties.Resources.folder;
      this.btnLeftVideoRoot.Location = new System.Drawing.Point(440, 12);
      this.btnLeftVideoRoot.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftVideoRoot.Name = "btnLeftVideoRoot";
      this.btnLeftVideoRoot.Size = new System.Drawing.Size(20, 20);
      this.btnLeftVideoRoot.TabIndex = 49;
      this.btnLeftVideoRoot.Tag = "";
      this.btnLeftVideoRoot.UseVisualStyleBackColor = true;
      this.btnLeftVideoRoot.Click += new System.EventHandler(this.btnFolderSelection_Click);
      // 
      // btnLeftVideoFile
      // 
      this.btnLeftVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftVideoFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftVideoFile.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftVideoFile.FlatAppearance.BorderSize = 0;
      this.btnLeftVideoFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftVideoFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftVideoFile.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnLeftVideoFile.Location = new System.Drawing.Point(440, 64);
      this.btnLeftVideoFile.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftVideoFile.Name = "btnLeftVideoFile";
      this.btnLeftVideoFile.Size = new System.Drawing.Size(20, 20);
      this.btnLeftVideoFile.TabIndex = 48;
      this.btnLeftVideoFile.Tag = "";
      this.btnLeftVideoFile.UseVisualStyleBackColor = true;
      this.btnLeftVideoFile.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // btnLeftVideoSubdir
      // 
      this.btnLeftVideoSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLeftVideoSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnLeftVideoSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnLeftVideoSubdir.FlatAppearance.BorderSize = 0;
      this.btnLeftVideoSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnLeftVideoSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnLeftVideoSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnLeftVideoSubdir.Location = new System.Drawing.Point(440, 38);
      this.btnLeftVideoSubdir.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnLeftVideoSubdir.Name = "btnLeftVideoSubdir";
      this.btnLeftVideoSubdir.Size = new System.Drawing.Size(20, 20);
      this.btnLeftVideoSubdir.TabIndex = 47;
      this.btnLeftVideoSubdir.Tag = "";
      this.btnLeftVideoSubdir.UseVisualStyleBackColor = true;
      this.btnLeftVideoSubdir.Click += new System.EventHandler(this.btnMacroReference_Click);
      // 
      // lblLeftVideoFile
      // 
      this.lblLeftVideoFile.AutoSize = true;
      this.lblLeftVideoFile.Location = new System.Drawing.Point(6, 68);
      this.lblLeftVideoFile.Name = "lblLeftVideoFile";
      this.lblLeftVideoFile.Size = new System.Drawing.Size(29, 13);
      this.lblLeftVideoFile.TabIndex = 45;
      this.lblLeftVideoFile.Text = "File :";
      // 
      // tbLeftVideoFile
      // 
      this.tbLeftVideoFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftVideoFile.Location = new System.Drawing.Point(145, 65);
      this.tbLeftVideoFile.Name = "tbLeftVideoFile";
      this.tbLeftVideoFile.Size = new System.Drawing.Size(289, 20);
      this.tbLeftVideoFile.TabIndex = 46;
      // 
      // lblLeftVideoSubdir
      // 
      this.lblLeftVideoSubdir.AutoSize = true;
      this.lblLeftVideoSubdir.Location = new System.Drawing.Point(6, 42);
      this.lblLeftVideoSubdir.Name = "lblLeftVideoSubdir";
      this.lblLeftVideoSubdir.Size = new System.Drawing.Size(75, 13);
      this.lblLeftVideoSubdir.TabIndex = 43;
      this.lblLeftVideoSubdir.Text = "Sub directory :";
      // 
      // tbLeftVideoSubdir
      // 
      this.tbLeftVideoSubdir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftVideoSubdir.Location = new System.Drawing.Point(145, 39);
      this.tbLeftVideoSubdir.Name = "tbLeftVideoSubdir";
      this.tbLeftVideoSubdir.Size = new System.Drawing.Size(289, 20);
      this.tbLeftVideoSubdir.TabIndex = 44;
      // 
      // lblLeftVideoRoot
      // 
      this.lblLeftVideoRoot.AutoSize = true;
      this.lblLeftVideoRoot.Location = new System.Drawing.Point(6, 16);
      this.lblLeftVideoRoot.Name = "lblLeftVideoRoot";
      this.lblLeftVideoRoot.Size = new System.Drawing.Size(36, 13);
      this.lblLeftVideoRoot.TabIndex = 38;
      this.lblLeftVideoRoot.Text = "Root :";
      // 
      // tbLeftVideoRoot
      // 
      this.tbLeftVideoRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLeftVideoRoot.Location = new System.Drawing.Point(145, 13);
      this.tbLeftVideoRoot.Name = "tbLeftVideoRoot";
      this.tbLeftVideoRoot.Size = new System.Drawing.Size(289, 20);
      this.tbLeftVideoRoot.TabIndex = 40;
      // 
      // tabAutomation
      // 
      this.tabAutomation.Controls.Add(this.label1);
      this.tabAutomation.Controls.Add(this.nudRecordingTime);
      this.tabAutomation.Controls.Add(this.chkIgnoreOverwriteWarning);
      this.tabAutomation.Controls.Add(this.btnPostRecordCommand);
      this.tabAutomation.Controls.Add(this.lblPostRecordCommand);
      this.tabAutomation.Controls.Add(this.tbPostRecordCommand);
      this.tabAutomation.Controls.Add(this.lblRecordingTime);
      this.tabAutomation.Location = new System.Drawing.Point(4, 22);
      this.tabAutomation.Name = "tabAutomation";
      this.tabAutomation.Size = new System.Drawing.Size(482, 296);
      this.tabAutomation.TabIndex = 5;
      this.tabAutomation.Text = "Automation";
      this.tabAutomation.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.Gray;
      this.label1.Location = new System.Drawing.Point(206, 81);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(219, 12);
      this.label1.TabIndex = 57;
      this.label1.Text = "(Ex: robocopy \"%directory\" \"D:\\backup\" \"%filename\")";
      // 
      // nudRecordingTime
      // 
      this.nudRecordingTime.DecimalPlaces = 1;
      this.nudRecordingTime.Location = new System.Drawing.Point(208, 22);
      this.nudRecordingTime.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.nudRecordingTime.Name = "nudRecordingTime";
      this.nudRecordingTime.Size = new System.Drawing.Size(48, 20);
      this.nudRecordingTime.TabIndex = 53;
      this.nudRecordingTime.ValueChanged += new System.EventHandler(this.NudRecordingTime_ValueChanged);
      // 
      // chkIgnoreOverwriteWarning
      // 
      this.chkIgnoreOverwriteWarning.AutoSize = true;
      this.chkIgnoreOverwriteWarning.Location = new System.Drawing.Point(19, 104);
      this.chkIgnoreOverwriteWarning.Name = "chkIgnoreOverwriteWarning";
      this.chkIgnoreOverwriteWarning.Size = new System.Drawing.Size(142, 17);
      this.chkIgnoreOverwriteWarning.TabIndex = 56;
      this.chkIgnoreOverwriteWarning.Text = "Ignore overwrite warning";
      this.chkIgnoreOverwriteWarning.UseVisualStyleBackColor = true;
      this.chkIgnoreOverwriteWarning.CheckedChanged += new System.EventHandler(this.chkIgnoreOverwriteWarning_CheckedChanged);
      // 
      // btnPostRecordCommand
      // 
      this.btnPostRecordCommand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnPostRecordCommand.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnPostRecordCommand.FlatAppearance.BorderSize = 0;
      this.btnPostRecordCommand.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnPostRecordCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnPostRecordCommand.Image = global::Kinovea.Root.Properties.Resources.percent;
      this.btnPostRecordCommand.Location = new System.Drawing.Point(441, 54);
      this.btnPostRecordCommand.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnPostRecordCommand.Name = "btnPostRecordCommand";
      this.btnPostRecordCommand.Size = new System.Drawing.Size(20, 20);
      this.btnPostRecordCommand.TabIndex = 54;
      this.btnPostRecordCommand.Tag = "";
      this.btnPostRecordCommand.UseVisualStyleBackColor = true;
      this.btnPostRecordCommand.Click += new System.EventHandler(this.btnPostRecordCommand_Click);
      // 
      // lblPostRecordCommand
      // 
      this.lblPostRecordCommand.Location = new System.Drawing.Point(16, 58);
      this.lblPostRecordCommand.Name = "lblPostRecordCommand";
      this.lblPostRecordCommand.Size = new System.Drawing.Size(186, 42);
      this.lblPostRecordCommand.TabIndex = 52;
      this.lblPostRecordCommand.Text = "Post recording command:";
      // 
      // tbPostRecordCommand
      // 
      this.tbPostRecordCommand.Location = new System.Drawing.Point(208, 55);
      this.tbPostRecordCommand.Name = "tbPostRecordCommand";
      this.tbPostRecordCommand.Size = new System.Drawing.Size(227, 20);
      this.tbPostRecordCommand.TabIndex = 53;
      this.tbPostRecordCommand.TextChanged += new System.EventHandler(this.tbPostRecordCommand_TextChanged);
      // 
      // lblRecordingTime
      // 
      this.lblRecordingTime.AutoSize = true;
      this.lblRecordingTime.Location = new System.Drawing.Point(16, 24);
      this.lblRecordingTime.Name = "lblRecordingTime";
      this.lblRecordingTime.Size = new System.Drawing.Size(98, 13);
      this.lblRecordingTime.TabIndex = 43;
      this.lblRecordingTime.Text = "Recording time (s) :";
      // 
      // PreferencePanelCapture
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabSubPages);
      this.Name = "PreferencePanelCapture";
      this.Size = new System.Drawing.Size(490, 322);
      this.tabSubPages.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.tabGeneral.PerformLayout();
      this.tabMemory.ResumeLayout(false);
      this.tabMemory.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).EndInit();
      this.tabRecording.ResumeLayout(false);
      this.gbHighspeedCameras.ResumeLayout(false);
      this.gbHighspeedCameras.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudReplacementFramerate)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudReplacementThreshold)).EndInit();
      this.grpRecordingMode.ResumeLayout(false);
      this.grpRecordingMode.PerformLayout();
      this.tabTrigger.ResumeLayout(false);
      this.tabTrigger.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudUDPPort)).EndInit();
      this.gbAudioTrigger.ResumeLayout(false);
      this.gbAudioTrigger.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudAudioTriggerThreshold)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.nudQuietPeriod)).EndInit();
      this.tabImageNaming.ResumeLayout(false);
      this.grpRightImage.ResumeLayout(false);
      this.grpRightImage.PerformLayout();
      this.grpLeftImage.ResumeLayout(false);
      this.grpLeftImage.PerformLayout();
      this.tabVideoNaming.ResumeLayout(false);
      this.grpRightVideo.ResumeLayout(false);
      this.grpRightVideo.PerformLayout();
      this.grpLeftVideo.ResumeLayout(false);
      this.grpLeftVideo.PerformLayout();
      this.tabAutomation.ResumeLayout(false);
      this.tabAutomation.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudRecordingTime)).EndInit();
      this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblMemoryBuffer;
		private System.Windows.Forms.TrackBar trkMemoryBuffer;
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cmbImageFormat;
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabImageNaming;
        private System.Windows.Forms.Label lblFramerate;
        private System.Windows.Forms.TextBox tbFramerate;
        private System.Windows.Forms.ComboBox cmbVideoFormat;
        private System.Windows.Forms.Label lblVideoFormat;
        private System.Windows.Forms.GroupBox grpRightImage;
        private System.Windows.Forms.Label lblRightImageFile;
        private System.Windows.Forms.TextBox tbRightImageFile;
        private System.Windows.Forms.Label lblRightImageSubdir;
        private System.Windows.Forms.TextBox tbRightImageSubdir;
        private System.Windows.Forms.Label lblRightImageRoot;
        private System.Windows.Forms.TextBox tbRightImageRoot;
        private System.Windows.Forms.GroupBox grpLeftImage;
        private System.Windows.Forms.Label lblLeftImageFile;
        private System.Windows.Forms.TextBox tbLeftImageFile;
        private System.Windows.Forms.Label lblLeftImageSubdir;
        private System.Windows.Forms.TextBox tbLeftImageSubdir;
        private System.Windows.Forms.Label lblLeftImageRoot;
        private System.Windows.Forms.TextBox tbLeftImageRoot;
        private System.Windows.Forms.Button btnLeftImageRoot;
        private System.Windows.Forms.Button btnRightImageFile;
        private System.Windows.Forms.Button btnRightImageSubdir;
        private System.Windows.Forms.Button btnRightImageRoot;
        private System.Windows.Forms.Button btnLeftImageFile;
        private System.Windows.Forms.Button btnLeftImageSubdir;
        private System.Windows.Forms.TabPage tabVideoNaming;
        private System.Windows.Forms.GroupBox grpRightVideo;
        private System.Windows.Forms.Button btnRightVideoFile;
        private System.Windows.Forms.Button btnRightVideoSubdir;
        private System.Windows.Forms.Button btnRightVideoRoot;
        private System.Windows.Forms.Label lblRightVideoFile;
        private System.Windows.Forms.TextBox tbRightVideoFile;
        private System.Windows.Forms.Label lblRightVideoSubdir;
        private System.Windows.Forms.TextBox tbRightVideoSubdir;
        private System.Windows.Forms.Label lblRightVideoRoot;
        private System.Windows.Forms.TextBox tbRightVideoRoot;
        private System.Windows.Forms.GroupBox grpLeftVideo;
        private System.Windows.Forms.Button btnLeftVideoFile;
        private System.Windows.Forms.Button btnLeftVideoSubdir;
        private System.Windows.Forms.Label lblLeftVideoFile;
        private System.Windows.Forms.TextBox tbLeftVideoFile;
        private System.Windows.Forms.Label lblLeftVideoSubdir;
        private System.Windows.Forms.TextBox tbLeftVideoSubdir;
        private System.Windows.Forms.Label lblLeftVideoRoot;
        private System.Windows.Forms.TextBox tbLeftVideoRoot;
        private System.Windows.Forms.Button btnLeftVideoRoot;
        private System.Windows.Forms.TabPage tabRecording;
        private System.Windows.Forms.GroupBox grpRecordingMode;
        private System.Windows.Forms.RadioButton rbRecordingDelayed;
        private System.Windows.Forms.RadioButton rbRecordingCamera;
        private System.Windows.Forms.ComboBox cmbUncompressedVideoFormat;
        private System.Windows.Forms.Label lblUncompressedVideoFormat;
        private System.Windows.Forms.TabPage tabAutomation;
        private System.Windows.Forms.Label lblRecordingTime;
        private System.Windows.Forms.Button btnPostRecordCommand;
        private System.Windows.Forms.Label lblPostRecordCommand;
        private System.Windows.Forms.TextBox tbPostRecordCommand;
        private System.Windows.Forms.RadioButton rbRecordingScheduled;
        private System.Windows.Forms.CheckBox chkUncompressedVideo;
        private System.Windows.Forms.CheckBox chkIgnoreOverwriteWarning;
        private System.Windows.Forms.GroupBox gbHighspeedCameras;
        private System.Windows.Forms.NumericUpDown nudReplacementFramerate;
        private System.Windows.Forms.Label lblReplacementFramerate;
        private System.Windows.Forms.NumericUpDown nudReplacementThreshold;
        private System.Windows.Forms.Label lblReplacementThreshold;
        private System.Windows.Forms.NumericUpDown nudRecordingTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblCaptureKVA;
        private System.Windows.Forms.TextBox tbCaptureKVA;
        private System.Windows.Forms.Button btnCaptureKVA;
        private System.Windows.Forms.TabPage tabTrigger;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nudUDPPort;
        private System.Windows.Forms.CheckBox chkEnableUDPTrigger;
        private System.Windows.Forms.Label lblUDPPort;
        private System.Windows.Forms.Label lblUDPTriggerHits;
        private System.Windows.Forms.ComboBox cmbTriggerAction;
        private System.Windows.Forms.GroupBox gbAudioTrigger;
        private System.Windows.Forms.NumericUpDown nudAudioTriggerThreshold;
        private System.Windows.Forms.CheckBox chkEnableAudioTrigger;
        private System.Windows.Forms.Label lblAudioTriggerThreshold;
        private System.Windows.Forms.Label lblInputDevice;
        private Services.VolumeMeterThreshold vumeter;
        private System.Windows.Forms.ComboBox cmbInputDevice;
        private System.Windows.Forms.Label lblAudioTriggerHits;
        private System.Windows.Forms.Label lblTriggerAction;
        private System.Windows.Forms.Label lblQuietPeriod;
        private System.Windows.Forms.NumericUpDown nudQuietPeriod;
        private System.Windows.Forms.ComboBox cmbDefaultTriggerState;
        private System.Windows.Forms.Label lblDefaultTriggerState;
    }
}
