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
      this.grpFormats = new System.Windows.Forms.GroupBox();
      this.chkUncompressedVideo = new System.Windows.Forms.CheckBox();
      this.lblImageFormat = new System.Windows.Forms.Label();
      this.cmbImageFormat = new System.Windows.Forms.ComboBox();
      this.lblVideoFormat = new System.Windows.Forms.Label();
      this.cmbUncompressedVideoFormat = new System.Windows.Forms.ComboBox();
      this.lblUncompressedVideoFormat = new System.Windows.Forms.Label();
      this.cmbVideoFormat = new System.Windows.Forms.ComboBox();
      this.lblCaptureKVA = new System.Windows.Forms.Label();
      this.tbCaptureKVA = new System.Windows.Forms.TextBox();
      this.btnCaptureKVA = new System.Windows.Forms.Button();
      this.lblFramerate = new System.Windows.Forms.Label();
      this.tbFramerate = new System.Windows.Forms.TextBox();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.lblMemoryBuffer = new System.Windows.Forms.Label();
      this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
      this.tabRecording = new System.Windows.Forms.TabPage();
      this.grpAnnotations = new System.Windows.Forms.GroupBox();
      this.chkExportCalibration = new System.Windows.Forms.CheckBox();
      this.chkExportDrawings = new System.Windows.Forms.CheckBox();
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
      this.tabPaths = new System.Windows.Forms.TabPage();
      this.grpCaptureFolderDetails = new System.Windows.Forms.GroupBox();
      this.btnCaptureFolderInterpolate = new System.Windows.Forms.Button();
      this.btnCaptureFolderInsertUnderscore = new System.Windows.Forms.Button();
      this.btnCaptureFolderInsertDash = new System.Windows.Forms.Button();
      this.btnCaptureFolderInsertBackslash = new System.Windows.Forms.Button();
      this.btnCaptureFolderInsertVariable = new System.Windows.Forms.Button();
      this.btnRightImageRoot = new System.Windows.Forms.Button();
      this.lblCaptureFolderPath = new System.Windows.Forms.Label();
      this.tbCaptureFolderPath = new System.Windows.Forms.TextBox();
      this.lblCaptureFolderShortName = new System.Windows.Forms.Label();
      this.tbCaptureFolderShortName = new System.Windows.Forms.TextBox();
      this.grpLeftImage = new System.Windows.Forms.GroupBox();
      this.btnAddCaptureFolder = new System.Windows.Forms.Button();
      this.btnDeleteCaptureFolder = new System.Windows.Forms.Button();
      this.btnSortFolderDown = new System.Windows.Forms.Button();
      this.olvCaptureFolders = new BrightIdeasSoftware.ObjectListView();
      this.btnSortFolderUp = new System.Windows.Forms.Button();
      this.tabAutomation = new System.Windows.Forms.TabPage();
      this.label1 = new System.Windows.Forms.Label();
      this.chkIgnoreOverwriteWarning = new System.Windows.Forms.CheckBox();
      this.btnPostRecordCommand = new System.Windows.Forms.Button();
      this.lblPostRecordCommand = new System.Windows.Forms.Label();
      this.tbPostRecordCommand = new System.Windows.Forms.TextBox();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.grpFormats.SuspendLayout();
      this.tabMemory.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
      this.tabRecording.SuspendLayout();
      this.grpAnnotations.SuspendLayout();
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
      this.tabPaths.SuspendLayout();
      this.grpCaptureFolderDetails.SuspendLayout();
      this.grpLeftImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.olvCaptureFolders)).BeginInit();
      this.tabAutomation.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabSubPages
      // 
      this.tabSubPages.Controls.Add(this.tabGeneral);
      this.tabSubPages.Controls.Add(this.tabMemory);
      this.tabSubPages.Controls.Add(this.tabRecording);
      this.tabSubPages.Controls.Add(this.tabTrigger);
      this.tabSubPages.Controls.Add(this.tabPaths);
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
      this.tabGeneral.Controls.Add(this.grpFormats);
      this.tabGeneral.Controls.Add(this.lblCaptureKVA);
      this.tabGeneral.Controls.Add(this.tbCaptureKVA);
      this.tabGeneral.Controls.Add(this.btnCaptureKVA);
      this.tabGeneral.Controls.Add(this.lblFramerate);
      this.tabGeneral.Controls.Add(this.tbFramerate);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
      this.tabGeneral.Size = new System.Drawing.Size(482, 296);
      this.tabGeneral.TabIndex = 0;
      this.tabGeneral.Text = "General";
      this.tabGeneral.UseVisualStyleBackColor = true;
      // 
      // grpFormats
      // 
      this.grpFormats.Controls.Add(this.chkUncompressedVideo);
      this.grpFormats.Controls.Add(this.lblImageFormat);
      this.grpFormats.Controls.Add(this.cmbImageFormat);
      this.grpFormats.Controls.Add(this.lblVideoFormat);
      this.grpFormats.Controls.Add(this.cmbUncompressedVideoFormat);
      this.grpFormats.Controls.Add(this.lblUncompressedVideoFormat);
      this.grpFormats.Controls.Add(this.cmbVideoFormat);
      this.grpFormats.Location = new System.Drawing.Point(6, 95);
      this.grpFormats.Name = "grpFormats";
      this.grpFormats.Size = new System.Drawing.Size(470, 154);
      this.grpFormats.TabIndex = 61;
      this.grpFormats.TabStop = false;
      this.grpFormats.Text = "Formats";
      // 
      // chkUncompressedVideo
      // 
      this.chkUncompressedVideo.AutoSize = true;
      this.chkUncompressedVideo.Location = new System.Drawing.Point(13, 27);
      this.chkUncompressedVideo.Name = "chkUncompressedVideo";
      this.chkUncompressedVideo.Size = new System.Drawing.Size(152, 17);
      this.chkUncompressedVideo.TabIndex = 47;
      this.chkUncompressedVideo.Text = "Save uncompressed video";
      this.chkUncompressedVideo.UseVisualStyleBackColor = true;
      this.chkUncompressedVideo.CheckedChanged += new System.EventHandler(this.chkUncompressedVideo_CheckedChanged);
      // 
      // lblImageFormat
      // 
      this.lblImageFormat.AutoSize = true;
      this.lblImageFormat.Location = new System.Drawing.Point(10, 57);
      this.lblImageFormat.Name = "lblImageFormat";
      this.lblImageFormat.Size = new System.Drawing.Size(74, 13);
      this.lblImageFormat.TabIndex = 2;
      this.lblImageFormat.Text = "Image format :";
      // 
      // cmbImageFormat
      // 
      this.cmbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbImageFormat.FormattingEnabled = true;
      this.cmbImageFormat.Location = new System.Drawing.Point(253, 57);
      this.cmbImageFormat.Name = "cmbImageFormat";
      this.cmbImageFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbImageFormat.TabIndex = 5;
      this.cmbImageFormat.SelectedIndexChanged += new System.EventHandler(this.cmbImageFormat_SelectedIndexChanged);
      // 
      // lblVideoFormat
      // 
      this.lblVideoFormat.AutoSize = true;
      this.lblVideoFormat.Location = new System.Drawing.Point(10, 87);
      this.lblVideoFormat.Name = "lblVideoFormat";
      this.lblVideoFormat.Size = new System.Drawing.Size(72, 13);
      this.lblVideoFormat.TabIndex = 40;
      this.lblVideoFormat.Text = "Video format :";
      // 
      // cmbUncompressedVideoFormat
      // 
      this.cmbUncompressedVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbUncompressedVideoFormat.FormattingEnabled = true;
      this.cmbUncompressedVideoFormat.Location = new System.Drawing.Point(253, 120);
      this.cmbUncompressedVideoFormat.Name = "cmbUncompressedVideoFormat";
      this.cmbUncompressedVideoFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbUncompressedVideoFormat.TabIndex = 43;
      this.cmbUncompressedVideoFormat.SelectedIndexChanged += new System.EventHandler(this.cmbUncompressedVideoFormat_SelectedIndexChanged);
      // 
      // lblUncompressedVideoFormat
      // 
      this.lblUncompressedVideoFormat.AutoSize = true;
      this.lblUncompressedVideoFormat.Location = new System.Drawing.Point(10, 120);
      this.lblUncompressedVideoFormat.Name = "lblUncompressedVideoFormat";
      this.lblUncompressedVideoFormat.Size = new System.Drawing.Size(145, 13);
      this.lblUncompressedVideoFormat.TabIndex = 42;
      this.lblUncompressedVideoFormat.Text = "Uncompressed video format :";
      // 
      // cmbVideoFormat
      // 
      this.cmbVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbVideoFormat.FormattingEnabled = true;
      this.cmbVideoFormat.Location = new System.Drawing.Point(253, 87);
      this.cmbVideoFormat.Name = "cmbVideoFormat";
      this.cmbVideoFormat.Size = new System.Drawing.Size(52, 21);
      this.cmbVideoFormat.TabIndex = 41;
      this.cmbVideoFormat.SelectedIndexChanged += new System.EventHandler(this.cmbVideoFormat_SelectedIndexChanged);
      // 
      // lblCaptureKVA
      // 
      this.lblCaptureKVA.AutoSize = true;
      this.lblCaptureKVA.Location = new System.Drawing.Point(19, 60);
      this.lblCaptureKVA.Name = "lblCaptureKVA";
      this.lblCaptureKVA.Size = new System.Drawing.Size(121, 13);
      this.lblCaptureKVA.TabIndex = 58;
      this.lblCaptureKVA.Text = "Default annotations file :";
      // 
      // tbCaptureKVA
      // 
      this.tbCaptureKVA.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbCaptureKVA.Location = new System.Drawing.Point(262, 58);
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
      this.btnCaptureKVA.Location = new System.Drawing.Point(443, 57);
      this.btnCaptureKVA.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnCaptureKVA.Name = "btnCaptureKVA";
      this.btnCaptureKVA.Size = new System.Drawing.Size(20, 20);
      this.btnCaptureKVA.TabIndex = 60;
      this.btnCaptureKVA.Tag = "";
      this.btnCaptureKVA.UseVisualStyleBackColor = true;
      this.btnCaptureKVA.Click += new System.EventHandler(this.btnCaptureKVA_Click);
      // 
      // lblFramerate
      // 
      this.lblFramerate.AutoSize = true;
      this.lblFramerate.Location = new System.Drawing.Point(19, 20);
      this.lblFramerate.Name = "lblFramerate";
      this.lblFramerate.Size = new System.Drawing.Size(117, 13);
      this.lblFramerate.TabIndex = 41;
      this.lblFramerate.Text = "Display framerate (fps) :";
      // 
      // tbFramerate
      // 
      this.tbFramerate.Location = new System.Drawing.Point(262, 20);
      this.tbFramerate.Name = "tbFramerate";
      this.tbFramerate.Size = new System.Drawing.Size(30, 20);
      this.tbFramerate.TabIndex = 40;
      this.tbFramerate.TextChanged += new System.EventHandler(this.tbFramerate_TextChanged);
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
      this.tabRecording.Controls.Add(this.grpAnnotations);
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
      // grpAnnotations
      // 
      this.grpAnnotations.Controls.Add(this.chkExportCalibration);
      this.grpAnnotations.Controls.Add(this.chkExportDrawings);
      this.grpAnnotations.Location = new System.Drawing.Point(6, 200);
      this.grpAnnotations.Name = "grpAnnotations";
      this.grpAnnotations.Size = new System.Drawing.Size(470, 81);
      this.grpAnnotations.TabIndex = 57;
      this.grpAnnotations.TabStop = false;
      this.grpAnnotations.Text = "Exported annotations";
      // 
      // chkExportCalibration
      // 
      this.chkExportCalibration.AutoSize = true;
      this.chkExportCalibration.Location = new System.Drawing.Point(20, 51);
      this.chkExportCalibration.Name = "chkExportCalibration";
      this.chkExportCalibration.Size = new System.Drawing.Size(107, 17);
      this.chkExportCalibration.TabIndex = 58;
      this.chkExportCalibration.Text = "Export calibration";
      this.chkExportCalibration.UseVisualStyleBackColor = true;
      this.chkExportCalibration.CheckedChanged += new System.EventHandler(this.chkExcludeCalibration_CheckedChanged);
      // 
      // chkExportDrawings
      // 
      this.chkExportDrawings.AutoSize = true;
      this.chkExportDrawings.Location = new System.Drawing.Point(20, 26);
      this.chkExportDrawings.Name = "chkExportDrawings";
      this.chkExportDrawings.Size = new System.Drawing.Size(101, 17);
      this.chkExportDrawings.TabIndex = 57;
      this.chkExportDrawings.Text = "Export drawings";
      this.chkExportDrawings.UseVisualStyleBackColor = true;
      this.chkExportDrawings.CheckedChanged += new System.EventHandler(this.chkExcludeDrawings_CheckedChanged);
      // 
      // gbHighspeedCameras
      // 
      this.gbHighspeedCameras.Controls.Add(this.nudReplacementFramerate);
      this.gbHighspeedCameras.Controls.Add(this.lblReplacementFramerate);
      this.gbHighspeedCameras.Controls.Add(this.nudReplacementThreshold);
      this.gbHighspeedCameras.Controls.Add(this.lblReplacementThreshold);
      this.gbHighspeedCameras.Location = new System.Drawing.Point(6, 113);
      this.gbHighspeedCameras.Name = "gbHighspeedCameras";
      this.gbHighspeedCameras.Size = new System.Drawing.Size(470, 81);
      this.gbHighspeedCameras.TabIndex = 41;
      this.gbHighspeedCameras.TabStop = false;
      this.gbHighspeedCameras.Text = "High speed cameras";
      // 
      // nudReplacementFramerate
      // 
      this.nudReplacementFramerate.Location = new System.Drawing.Point(281, 48);
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
      this.lblReplacementFramerate.Location = new System.Drawing.Point(17, 50);
      this.lblReplacementFramerate.Name = "lblReplacementFramerate";
      this.lblReplacementFramerate.Size = new System.Drawing.Size(120, 13);
      this.lblReplacementFramerate.TabIndex = 55;
      this.lblReplacementFramerate.Text = "Replacement framerate:";
      // 
      // nudReplacementThreshold
      // 
      this.nudReplacementThreshold.Location = new System.Drawing.Point(281, 22);
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
      this.lblReplacementThreshold.Location = new System.Drawing.Point(17, 24);
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
      // tabPaths
      // 
      this.tabPaths.Controls.Add(this.grpCaptureFolderDetails);
      this.tabPaths.Controls.Add(this.grpLeftImage);
      this.tabPaths.Location = new System.Drawing.Point(4, 22);
      this.tabPaths.Name = "tabPaths";
      this.tabPaths.Padding = new System.Windows.Forms.Padding(3);
      this.tabPaths.Size = new System.Drawing.Size(482, 296);
      this.tabPaths.TabIndex = 1;
      this.tabPaths.Text = "Paths";
      this.tabPaths.UseVisualStyleBackColor = true;
      // 
      // grpCaptureFolderDetails
      // 
      this.grpCaptureFolderDetails.Controls.Add(this.btnCaptureFolderInterpolate);
      this.grpCaptureFolderDetails.Controls.Add(this.btnCaptureFolderInsertUnderscore);
      this.grpCaptureFolderDetails.Controls.Add(this.btnCaptureFolderInsertDash);
      this.grpCaptureFolderDetails.Controls.Add(this.btnCaptureFolderInsertBackslash);
      this.grpCaptureFolderDetails.Controls.Add(this.btnCaptureFolderInsertVariable);
      this.grpCaptureFolderDetails.Controls.Add(this.btnRightImageRoot);
      this.grpCaptureFolderDetails.Controls.Add(this.lblCaptureFolderPath);
      this.grpCaptureFolderDetails.Controls.Add(this.tbCaptureFolderPath);
      this.grpCaptureFolderDetails.Controls.Add(this.lblCaptureFolderShortName);
      this.grpCaptureFolderDetails.Controls.Add(this.tbCaptureFolderShortName);
      this.grpCaptureFolderDetails.Location = new System.Drawing.Point(6, 170);
      this.grpCaptureFolderDetails.Name = "grpCaptureFolderDetails";
      this.grpCaptureFolderDetails.Size = new System.Drawing.Size(470, 120);
      this.grpCaptureFolderDetails.TabIndex = 47;
      this.grpCaptureFolderDetails.TabStop = false;
      this.grpCaptureFolderDetails.Text = "Folder detail";
      // 
      // btnCaptureFolderInterpolate
      // 
      this.btnCaptureFolderInterpolate.BackColor = System.Drawing.Color.Transparent;
      this.btnCaptureFolderInterpolate.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnCaptureFolderInterpolate.FlatAppearance.BorderSize = 0;
      this.btnCaptureFolderInterpolate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnCaptureFolderInterpolate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnCaptureFolderInterpolate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCaptureFolderInterpolate.Image = global::Kinovea.Root.Properties.Resources.variable_16;
      this.btnCaptureFolderInterpolate.Location = new System.Drawing.Point(408, 87);
      this.btnCaptureFolderInterpolate.Name = "btnCaptureFolderInterpolate";
      this.btnCaptureFolderInterpolate.Size = new System.Drawing.Size(26, 23);
      this.btnCaptureFolderInterpolate.TabIndex = 55;
      this.btnCaptureFolderInterpolate.UseVisualStyleBackColor = false;
      this.btnCaptureFolderInterpolate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnCaptureFolderInterpolate_MouseDown);
      this.btnCaptureFolderInterpolate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnCaptureFolderInterpolate_MouseUp);
      // 
      // btnCaptureFolderInsertUnderscore
      // 
      this.btnCaptureFolderInsertUnderscore.BackColor = System.Drawing.Color.WhiteSmoke;
      this.btnCaptureFolderInsertUnderscore.FlatAppearance.BorderSize = 0;
      this.btnCaptureFolderInsertUnderscore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCaptureFolderInsertUnderscore.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCaptureFolderInsertUnderscore.Location = new System.Drawing.Point(346, 87);
      this.btnCaptureFolderInsertUnderscore.Name = "btnCaptureFolderInsertUnderscore";
      this.btnCaptureFolderInsertUnderscore.Size = new System.Drawing.Size(26, 23);
      this.btnCaptureFolderInsertUnderscore.TabIndex = 54;
      this.btnCaptureFolderInsertUnderscore.Text = "_";
      this.btnCaptureFolderInsertUnderscore.UseVisualStyleBackColor = false;
      this.btnCaptureFolderInsertUnderscore.Click += new System.EventHandler(this.btnCaptureFolderInsertUnderscore_Click);
      // 
      // btnCaptureFolderInsertDash
      // 
      this.btnCaptureFolderInsertDash.BackColor = System.Drawing.Color.WhiteSmoke;
      this.btnCaptureFolderInsertDash.FlatAppearance.BorderSize = 0;
      this.btnCaptureFolderInsertDash.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCaptureFolderInsertDash.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCaptureFolderInsertDash.Location = new System.Drawing.Point(314, 87);
      this.btnCaptureFolderInsertDash.Name = "btnCaptureFolderInsertDash";
      this.btnCaptureFolderInsertDash.Size = new System.Drawing.Size(26, 23);
      this.btnCaptureFolderInsertDash.TabIndex = 53;
      this.btnCaptureFolderInsertDash.Text = "-";
      this.btnCaptureFolderInsertDash.UseVisualStyleBackColor = false;
      this.btnCaptureFolderInsertDash.Click += new System.EventHandler(this.btnCaptureFolderInsertDash_Click);
      // 
      // btnCaptureFolderInsertBackslash
      // 
      this.btnCaptureFolderInsertBackslash.BackColor = System.Drawing.Color.WhiteSmoke;
      this.btnCaptureFolderInsertBackslash.FlatAppearance.BorderSize = 0;
      this.btnCaptureFolderInsertBackslash.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnCaptureFolderInsertBackslash.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnCaptureFolderInsertBackslash.Location = new System.Drawing.Point(282, 87);
      this.btnCaptureFolderInsertBackslash.Name = "btnCaptureFolderInsertBackslash";
      this.btnCaptureFolderInsertBackslash.Size = new System.Drawing.Size(26, 23);
      this.btnCaptureFolderInsertBackslash.TabIndex = 52;
      this.btnCaptureFolderInsertBackslash.Text = "\\";
      this.btnCaptureFolderInsertBackslash.UseVisualStyleBackColor = false;
      this.btnCaptureFolderInsertBackslash.Click += new System.EventHandler(this.btnCaptureFolderInsertBackslash_Click);
      // 
      // btnCaptureFolderInsertVariable
      // 
      this.btnCaptureFolderInsertVariable.Location = new System.Drawing.Point(144, 87);
      this.btnCaptureFolderInsertVariable.Name = "btnCaptureFolderInsertVariable";
      this.btnCaptureFolderInsertVariable.Size = new System.Drawing.Size(131, 23);
      this.btnCaptureFolderInsertVariable.TabIndex = 51;
      this.btnCaptureFolderInsertVariable.Text = "Insert variable…";
      this.btnCaptureFolderInsertVariable.UseVisualStyleBackColor = true;
      this.btnCaptureFolderInsertVariable.Click += new System.EventHandler(this.btnCaptureFolderInsertVariable_Click);
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
      this.btnRightImageRoot.Location = new System.Drawing.Point(440, 55);
      this.btnRightImageRoot.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnRightImageRoot.Name = "btnRightImageRoot";
      this.btnRightImageRoot.Size = new System.Drawing.Size(20, 20);
      this.btnRightImageRoot.TabIndex = 49;
      this.btnRightImageRoot.Tag = "";
      this.btnRightImageRoot.UseVisualStyleBackColor = true;
      this.btnRightImageRoot.Click += new System.EventHandler(this.btnSortFolderUp_Click);
      // 
      // lblCaptureFolderPath
      // 
      this.lblCaptureFolderPath.AutoSize = true;
      this.lblCaptureFolderPath.Location = new System.Drawing.Point(20, 59);
      this.lblCaptureFolderPath.Name = "lblCaptureFolderPath";
      this.lblCaptureFolderPath.Size = new System.Drawing.Size(32, 13);
      this.lblCaptureFolderPath.TabIndex = 43;
      this.lblCaptureFolderPath.Text = "Path:";
      // 
      // tbCaptureFolderPath
      // 
      this.tbCaptureFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbCaptureFolderPath.Location = new System.Drawing.Point(145, 56);
      this.tbCaptureFolderPath.Name = "tbCaptureFolderPath";
      this.tbCaptureFolderPath.Size = new System.Drawing.Size(289, 20);
      this.tbCaptureFolderPath.TabIndex = 44;
      this.tbCaptureFolderPath.TextChanged += new System.EventHandler(this.tbCaptureFolderPath_TextChanged);
      // 
      // lblCaptureFolderShortName
      // 
      this.lblCaptureFolderShortName.AutoSize = true;
      this.lblCaptureFolderShortName.Location = new System.Drawing.Point(20, 26);
      this.lblCaptureFolderShortName.Name = "lblCaptureFolderShortName";
      this.lblCaptureFolderShortName.Size = new System.Drawing.Size(64, 13);
      this.lblCaptureFolderShortName.TabIndex = 38;
      this.lblCaptureFolderShortName.Text = "Short name:";
      // 
      // tbCaptureFolderShortName
      // 
      this.tbCaptureFolderShortName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbCaptureFolderShortName.Location = new System.Drawing.Point(145, 23);
      this.tbCaptureFolderShortName.Name = "tbCaptureFolderShortName";
      this.tbCaptureFolderShortName.Size = new System.Drawing.Size(163, 20);
      this.tbCaptureFolderShortName.TabIndex = 40;
      this.tbCaptureFolderShortName.TextChanged += new System.EventHandler(this.tbCaptureFolderShortName_TextChanged);
      // 
      // grpLeftImage
      // 
      this.grpLeftImage.Controls.Add(this.btnAddCaptureFolder);
      this.grpLeftImage.Controls.Add(this.btnDeleteCaptureFolder);
      this.grpLeftImage.Controls.Add(this.btnSortFolderDown);
      this.grpLeftImage.Controls.Add(this.olvCaptureFolders);
      this.grpLeftImage.Controls.Add(this.btnSortFolderUp);
      this.grpLeftImage.Location = new System.Drawing.Point(6, 6);
      this.grpLeftImage.Name = "grpLeftImage";
      this.grpLeftImage.Size = new System.Drawing.Size(470, 158);
      this.grpLeftImage.TabIndex = 44;
      this.grpLeftImage.TabStop = false;
      this.grpLeftImage.Text = "Capture folders";
      // 
      // btnAddCaptureFolder
      // 
      this.btnAddCaptureFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAddCaptureFolder.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnAddCaptureFolder.FlatAppearance.BorderSize = 0;
      this.btnAddCaptureFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnAddCaptureFolder.Image = global::Kinovea.Root.Properties.Resources.add_16;
      this.btnAddCaptureFolder.Location = new System.Drawing.Point(439, 79);
      this.btnAddCaptureFolder.Name = "btnAddCaptureFolder";
      this.btnAddCaptureFolder.Size = new System.Drawing.Size(25, 25);
      this.btnAddCaptureFolder.TabIndex = 79;
      this.btnAddCaptureFolder.UseVisualStyleBackColor = true;
      this.btnAddCaptureFolder.Click += new System.EventHandler(this.btnAddCaptureFolder_Click);
      // 
      // btnDeleteCaptureFolder
      // 
      this.btnDeleteCaptureFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDeleteCaptureFolder.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnDeleteCaptureFolder.FlatAppearance.BorderSize = 0;
      this.btnDeleteCaptureFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDeleteCaptureFolder.Image = global::Kinovea.Root.Properties.Resources.bin_empty;
      this.btnDeleteCaptureFolder.Location = new System.Drawing.Point(439, 121);
      this.btnDeleteCaptureFolder.Name = "btnDeleteCaptureFolder";
      this.btnDeleteCaptureFolder.Size = new System.Drawing.Size(25, 25);
      this.btnDeleteCaptureFolder.TabIndex = 78;
      this.btnDeleteCaptureFolder.UseVisualStyleBackColor = true;
      this.btnDeleteCaptureFolder.Click += new System.EventHandler(this.btnDeleteCaptureFolder_Click);
      // 
      // btnSortFolderDown
      // 
      this.btnSortFolderDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSortFolderDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSortFolderDown.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSortFolderDown.FlatAppearance.BorderSize = 0;
      this.btnSortFolderDown.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnSortFolderDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSortFolderDown.Image = global::Kinovea.Root.Properties.Resources.thick_arrow_pointing_down_16;
      this.btnSortFolderDown.Location = new System.Drawing.Point(440, 53);
      this.btnSortFolderDown.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnSortFolderDown.Name = "btnSortFolderDown";
      this.btnSortFolderDown.Size = new System.Drawing.Size(20, 20);
      this.btnSortFolderDown.TabIndex = 77;
      this.btnSortFolderDown.Tag = "";
      this.btnSortFolderDown.UseVisualStyleBackColor = true;
      this.btnSortFolderDown.Click += new System.EventHandler(this.btnSortFolderDown_Click);
      // 
      // olvCaptureFolders
      // 
      this.olvCaptureFolders.AlternateRowBackColor = System.Drawing.Color.Gainsboro;
      this.olvCaptureFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.olvCaptureFolders.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClickAlways;
      this.olvCaptureFolders.CellEditUseWholeCell = false;
      this.olvCaptureFolders.Cursor = System.Windows.Forms.Cursors.Default;
      this.olvCaptureFolders.FullRowSelect = true;
      this.olvCaptureFolders.GridLines = true;
      this.olvCaptureFolders.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.olvCaptureFolders.HideSelection = false;
      this.olvCaptureFolders.Location = new System.Drawing.Point(13, 23);
      this.olvCaptureFolders.Name = "olvCaptureFolders";
      this.olvCaptureFolders.Size = new System.Drawing.Size(421, 123);
      this.olvCaptureFolders.TabIndex = 76;
      this.olvCaptureFolders.UseCompatibleStateImageBehavior = false;
      this.olvCaptureFolders.View = System.Windows.Forms.View.Details;
      this.olvCaptureFolders.SelectedIndexChanged += new System.EventHandler(this.olvCaptureFolders_SelectedIndexChanged);
      // 
      // btnSortFolderUp
      // 
      this.btnSortFolderUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSortFolderUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSortFolderUp.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSortFolderUp.FlatAppearance.BorderSize = 0;
      this.btnSortFolderUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnSortFolderUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSortFolderUp.Image = global::Kinovea.Root.Properties.Resources.thick_arrow_pointing_up_16;
      this.btnSortFolderUp.Location = new System.Drawing.Point(440, 27);
      this.btnSortFolderUp.MinimumSize = new System.Drawing.Size(20, 20);
      this.btnSortFolderUp.Name = "btnSortFolderUp";
      this.btnSortFolderUp.Size = new System.Drawing.Size(20, 20);
      this.btnSortFolderUp.TabIndex = 42;
      this.btnSortFolderUp.Tag = "";
      this.btnSortFolderUp.UseVisualStyleBackColor = true;
      this.btnSortFolderUp.Click += new System.EventHandler(this.btnSortFolderUp_Click);
      // 
      // tabAutomation
      // 
      this.tabAutomation.Controls.Add(this.label1);
      this.tabAutomation.Controls.Add(this.chkIgnoreOverwriteWarning);
      this.tabAutomation.Controls.Add(this.btnPostRecordCommand);
      this.tabAutomation.Controls.Add(this.lblPostRecordCommand);
      this.tabAutomation.Controls.Add(this.tbPostRecordCommand);
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
      this.label1.Location = new System.Drawing.Point(206, 43);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(219, 12);
      this.label1.TabIndex = 57;
      this.label1.Text = "(Ex: robocopy \"%directory\" \"D:\\backup\" \"%filename\")";
      // 
      // chkIgnoreOverwriteWarning
      // 
      this.chkIgnoreOverwriteWarning.AutoSize = true;
      this.chkIgnoreOverwriteWarning.Location = new System.Drawing.Point(19, 66);
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
      this.btnPostRecordCommand.Location = new System.Drawing.Point(441, 16);
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
      this.lblPostRecordCommand.Location = new System.Drawing.Point(16, 20);
      this.lblPostRecordCommand.Name = "lblPostRecordCommand";
      this.lblPostRecordCommand.Size = new System.Drawing.Size(186, 42);
      this.lblPostRecordCommand.TabIndex = 52;
      this.lblPostRecordCommand.Text = "Post recording command:";
      // 
      // tbPostRecordCommand
      // 
      this.tbPostRecordCommand.Location = new System.Drawing.Point(208, 17);
      this.tbPostRecordCommand.Name = "tbPostRecordCommand";
      this.tbPostRecordCommand.Size = new System.Drawing.Size(227, 20);
      this.tbPostRecordCommand.TabIndex = 53;
      this.tbPostRecordCommand.TextChanged += new System.EventHandler(this.tbPostRecordCommand_TextChanged);
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
      this.grpFormats.ResumeLayout(false);
      this.grpFormats.PerformLayout();
      this.tabMemory.ResumeLayout(false);
      this.tabMemory.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).EndInit();
      this.tabRecording.ResumeLayout(false);
      this.grpAnnotations.ResumeLayout(false);
      this.grpAnnotations.PerformLayout();
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
      this.tabPaths.ResumeLayout(false);
      this.grpCaptureFolderDetails.ResumeLayout(false);
      this.grpCaptureFolderDetails.PerformLayout();
      this.grpLeftImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.olvCaptureFolders)).EndInit();
      this.tabAutomation.ResumeLayout(false);
      this.tabAutomation.PerformLayout();
      this.ResumeLayout(false);

		}
		private System.Windows.Forms.Label lblMemoryBuffer;
		private System.Windows.Forms.TrackBar trkMemoryBuffer;
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cmbImageFormat;
		private System.Windows.Forms.TabControl tabSubPages;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabPaths;
        private System.Windows.Forms.Label lblFramerate;
        private System.Windows.Forms.TextBox tbFramerate;
        private System.Windows.Forms.ComboBox cmbVideoFormat;
        private System.Windows.Forms.Label lblVideoFormat;
        private System.Windows.Forms.GroupBox grpLeftImage;
        private System.Windows.Forms.Button btnSortFolderUp;
        private System.Windows.Forms.TabPage tabRecording;
        private System.Windows.Forms.GroupBox grpRecordingMode;
        private System.Windows.Forms.RadioButton rbRecordingDelayed;
        private System.Windows.Forms.RadioButton rbRecordingCamera;
        private System.Windows.Forms.ComboBox cmbUncompressedVideoFormat;
        private System.Windows.Forms.Label lblUncompressedVideoFormat;
        private System.Windows.Forms.TabPage tabAutomation;
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
        private System.Windows.Forms.GroupBox grpAnnotations;
        private System.Windows.Forms.CheckBox chkExportCalibration;
        private System.Windows.Forms.CheckBox chkExportDrawings;
        private System.Windows.Forms.GroupBox grpFormats;
        private System.Windows.Forms.GroupBox grpCaptureFolderDetails;
        private System.Windows.Forms.Button btnRightImageRoot;
        private System.Windows.Forms.Label lblCaptureFolderPath;
        private System.Windows.Forms.TextBox tbCaptureFolderPath;
        private System.Windows.Forms.Label lblCaptureFolderShortName;
        private System.Windows.Forms.TextBox tbCaptureFolderShortName;
        private System.Windows.Forms.Button btnCaptureFolderInsertDash;
        private System.Windows.Forms.Button btnCaptureFolderInsertBackslash;
        private System.Windows.Forms.Button btnCaptureFolderInsertVariable;
        private BrightIdeasSoftware.ObjectListView olvCaptureFolders;
        private System.Windows.Forms.Button btnSortFolderDown;
        private System.Windows.Forms.Button btnDeleteCaptureFolder;
        private System.Windows.Forms.Button btnAddCaptureFolder;
        private System.Windows.Forms.Button btnCaptureFolderInsertUnderscore;
        private System.Windows.Forms.Button btnCaptureFolderInterpolate;
    }
}
