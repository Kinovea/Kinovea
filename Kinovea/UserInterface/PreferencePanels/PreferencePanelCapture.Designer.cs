#region License
/*
Copyright © Joan Charmant 2011.
joan.charmant@gmail.com 
 
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
            this.cmbVideoFormat = new System.Windows.Forms.ComboBox();
            this.lblVideoFormat = new System.Windows.Forms.Label();
            this.grpDSS = new System.Windows.Forms.GroupBox();
            this.lblFramerate = new System.Windows.Forms.Label();
            this.tbFramerate = new System.Windows.Forms.TextBox();
            this.rbForcedFramerate = new System.Windows.Forms.RadioButton();
            this.rbCameraFrameSignal = new System.Windows.Forms.RadioButton();
            this.cmbImageFormat = new System.Windows.Forms.ComboBox();
            this.lblImageFormat = new System.Windows.Forms.Label();
            this.tabImageNaming = new System.Windows.Forms.TabPage();
            this.grpRightImage = new System.Windows.Forms.GroupBox();
            this.lblRightImageFile = new System.Windows.Forms.Label();
            this.tbRightImageFile = new System.Windows.Forms.TextBox();
            this.lblRightImageSubdir = new System.Windows.Forms.Label();
            this.tbRightImageSubdir = new System.Windows.Forms.TextBox();
            this.lblRightImageRoot = new System.Windows.Forms.Label();
            this.tbRightImageRoot = new System.Windows.Forms.TextBox();
            this.grpLeftImage = new System.Windows.Forms.GroupBox();
            this.lblLeftImageFile = new System.Windows.Forms.Label();
            this.tbLeftImageFile = new System.Windows.Forms.TextBox();
            this.lblLeftImageSubdir = new System.Windows.Forms.Label();
            this.tbLeftImageSubdir = new System.Windows.Forms.TextBox();
            this.lblLeftImageRoot = new System.Windows.Forms.Label();
            this.tbLeftImageRoot = new System.Windows.Forms.TextBox();
            this.tabVideoNaming = new System.Windows.Forms.TabPage();
            this.grpRightVideo = new System.Windows.Forms.GroupBox();
            this.lblRightVideoFile = new System.Windows.Forms.Label();
            this.tbRightVideoFile = new System.Windows.Forms.TextBox();
            this.lblRightVideoSubdir = new System.Windows.Forms.Label();
            this.tbRightVideoSubdir = new System.Windows.Forms.TextBox();
            this.lblRightVideoRoot = new System.Windows.Forms.Label();
            this.tbRightVideoRoot = new System.Windows.Forms.TextBox();
            this.grpLeftVideo = new System.Windows.Forms.GroupBox();
            this.lblLeftVideoFile = new System.Windows.Forms.Label();
            this.tbLeftVideoFile = new System.Windows.Forms.TextBox();
            this.lblLeftVideoSubdir = new System.Windows.Forms.Label();
            this.tbLeftVideoSubdir = new System.Windows.Forms.TextBox();
            this.lblLeftVideoRoot = new System.Windows.Forms.Label();
            this.tbLeftVideoRoot = new System.Windows.Forms.TextBox();
            this.tabMemory = new System.Windows.Forms.TabPage();
            this.lblMemoryBuffer = new System.Windows.Forms.Label();
            this.trkMemoryBuffer = new System.Windows.Forms.TrackBar();
            this.btnRightImageFile = new System.Windows.Forms.Button();
            this.btnRightImageSubdir = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.btnLeftImageFile = new System.Windows.Forms.Button();
            this.btnLeftImageSubdir = new System.Windows.Forms.Button();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            this.btnRightVideoFile = new System.Windows.Forms.Button();
            this.btnRightVideoSubdir = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.btnLeftVideoFile = new System.Windows.Forms.Button();
            this.btnLeftVideoSubdir = new System.Windows.Forms.Button();
            this.tabSubPages.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.grpDSS.SuspendLayout();
            this.tabImageNaming.SuspendLayout();
            this.grpRightImage.SuspendLayout();
            this.grpLeftImage.SuspendLayout();
            this.tabVideoNaming.SuspendLayout();
            this.grpRightVideo.SuspendLayout();
            this.grpLeftVideo.SuspendLayout();
            this.tabMemory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // tabSubPages
            // 
            this.tabSubPages.Controls.Add(this.tabGeneral);
            this.tabSubPages.Controls.Add(this.tabImageNaming);
            this.tabSubPages.Controls.Add(this.tabVideoNaming);
            this.tabSubPages.Controls.Add(this.tabMemory);
            this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSubPages.Location = new System.Drawing.Point(0, 0);
            this.tabSubPages.Name = "tabSubPages";
            this.tabSubPages.SelectedIndex = 0;
            this.tabSubPages.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.cmbVideoFormat);
            this.tabGeneral.Controls.Add(this.lblVideoFormat);
            this.tabGeneral.Controls.Add(this.grpDSS);
            this.tabGeneral.Controls.Add(this.cmbImageFormat);
            this.tabGeneral.Controls.Add(this.lblImageFormat);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(424, 210);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // cmbVideoFormat
            // 
            this.cmbVideoFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVideoFormat.FormattingEnabled = true;
            this.cmbVideoFormat.Location = new System.Drawing.Point(170, 48);
            this.cmbVideoFormat.Name = "cmbVideoFormat";
            this.cmbVideoFormat.Size = new System.Drawing.Size(52, 21);
            this.cmbVideoFormat.TabIndex = 41;
            // 
            // lblVideoFormat
            // 
            this.lblVideoFormat.Location = new System.Drawing.Point(15, 51);
            this.lblVideoFormat.Name = "lblVideoFormat";
            this.lblVideoFormat.Size = new System.Drawing.Size(149, 18);
            this.lblVideoFormat.TabIndex = 40;
            this.lblVideoFormat.Text = "Video format :";
            // 
            // grpDSS
            // 
            this.grpDSS.Controls.Add(this.lblFramerate);
            this.grpDSS.Controls.Add(this.tbFramerate);
            this.grpDSS.Controls.Add(this.rbForcedFramerate);
            this.grpDSS.Controls.Add(this.rbCameraFrameSignal);
            this.grpDSS.Location = new System.Drawing.Point(18, 89);
            this.grpDSS.Name = "grpDSS";
            this.grpDSS.Size = new System.Drawing.Size(386, 102);
            this.grpDSS.TabIndex = 39;
            this.grpDSS.TabStop = false;
            this.grpDSS.Text = "Display synchronization strategy";
            // 
            // lblFramerate
            // 
            this.lblFramerate.AutoSize = true;
            this.lblFramerate.Location = new System.Drawing.Point(52, 73);
            this.lblFramerate.Name = "lblFramerate";
            this.lblFramerate.Size = new System.Drawing.Size(83, 13);
            this.lblFramerate.TabIndex = 41;
            this.lblFramerate.Text = "Framerate (fps) :";
            // 
            // tbFramerate
            // 
            this.tbFramerate.Location = new System.Drawing.Point(157, 70);
            this.tbFramerate.Name = "tbFramerate";
            this.tbFramerate.Size = new System.Drawing.Size(61, 20);
            this.tbFramerate.TabIndex = 40;
            // 
            // rbForcedFramerate
            // 
            this.rbForcedFramerate.AutoSize = true;
            this.rbForcedFramerate.Location = new System.Drawing.Point(16, 49);
            this.rbForcedFramerate.Name = "rbForcedFramerate";
            this.rbForcedFramerate.Size = new System.Drawing.Size(105, 17);
            this.rbForcedFramerate.TabIndex = 39;
            this.rbForcedFramerate.TabStop = true;
            this.rbForcedFramerate.Text = "Forced framerate";
            this.rbForcedFramerate.UseVisualStyleBackColor = true;
            // 
            // rbCameraFrameSignal
            // 
            this.rbCameraFrameSignal.AutoSize = true;
            this.rbCameraFrameSignal.Location = new System.Drawing.Point(16, 26);
            this.rbCameraFrameSignal.Name = "rbCameraFrameSignal";
            this.rbCameraFrameSignal.Size = new System.Drawing.Size(120, 17);
            this.rbCameraFrameSignal.TabIndex = 38;
            this.rbCameraFrameSignal.TabStop = true;
            this.rbCameraFrameSignal.Text = "Camera frame signal";
            this.rbCameraFrameSignal.UseVisualStyleBackColor = true;
            // 
            // cmbImageFormat
            // 
            this.cmbImageFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbImageFormat.FormattingEnabled = true;
            this.cmbImageFormat.Location = new System.Drawing.Point(170, 21);
            this.cmbImageFormat.Name = "cmbImageFormat";
            this.cmbImageFormat.Size = new System.Drawing.Size(52, 21);
            this.cmbImageFormat.TabIndex = 5;
            // 
            // lblImageFormat
            // 
            this.lblImageFormat.Location = new System.Drawing.Point(15, 24);
            this.lblImageFormat.Name = "lblImageFormat";
            this.lblImageFormat.Size = new System.Drawing.Size(149, 18);
            this.lblImageFormat.TabIndex = 2;
            this.lblImageFormat.Text = "Image format :";
            // 
            // tabImageNaming
            // 
            this.tabImageNaming.Controls.Add(this.grpRightImage);
            this.tabImageNaming.Controls.Add(this.grpLeftImage);
            this.tabImageNaming.Location = new System.Drawing.Point(4, 22);
            this.tabImageNaming.Name = "tabImageNaming";
            this.tabImageNaming.Padding = new System.Windows.Forms.Padding(3);
            this.tabImageNaming.Size = new System.Drawing.Size(424, 210);
            this.tabImageNaming.TabIndex = 1;
            this.tabImageNaming.Text = "Image naming";
            this.tabImageNaming.UseVisualStyleBackColor = true;
            // 
            // grpRightImage
            // 
            this.grpRightImage.Controls.Add(this.btnRightImageFile);
            this.grpRightImage.Controls.Add(this.btnRightImageSubdir);
            this.grpRightImage.Controls.Add(this.button5);
            this.grpRightImage.Controls.Add(this.lblRightImageFile);
            this.grpRightImage.Controls.Add(this.tbRightImageFile);
            this.grpRightImage.Controls.Add(this.lblRightImageSubdir);
            this.grpRightImage.Controls.Add(this.tbRightImageSubdir);
            this.grpRightImage.Controls.Add(this.lblRightImageRoot);
            this.grpRightImage.Controls.Add(this.tbRightImageRoot);
            this.grpRightImage.Location = new System.Drawing.Point(6, 106);
            this.grpRightImage.Name = "grpRightImage";
            this.grpRightImage.Size = new System.Drawing.Size(412, 94);
            this.grpRightImage.TabIndex = 47;
            this.grpRightImage.TabStop = false;
            this.grpRightImage.Text = "Right";
            // 
            // lblRightImageFile
            // 
            this.lblRightImageFile.Location = new System.Drawing.Point(6, 68);
            this.lblRightImageFile.Name = "lblRightImageFile";
            this.lblRightImageFile.Size = new System.Drawing.Size(103, 17);
            this.lblRightImageFile.TabIndex = 45;
            this.lblRightImageFile.Text = "File :";
            // 
            // tbRightImageFile
            // 
            this.tbRightImageFile.Location = new System.Drawing.Point(145, 65);
            this.tbRightImageFile.Name = "tbRightImageFile";
            this.tbRightImageFile.Size = new System.Drawing.Size(231, 20);
            this.tbRightImageFile.TabIndex = 46;
            // 
            // lblRightImageSubdir
            // 
            this.lblRightImageSubdir.Location = new System.Drawing.Point(6, 42);
            this.lblRightImageSubdir.Name = "lblRightImageSubdir";
            this.lblRightImageSubdir.Size = new System.Drawing.Size(103, 17);
            this.lblRightImageSubdir.TabIndex = 43;
            this.lblRightImageSubdir.Text = "Sub directory :";
            // 
            // tbRightImageSubdir
            // 
            this.tbRightImageSubdir.Location = new System.Drawing.Point(145, 39);
            this.tbRightImageSubdir.Name = "tbRightImageSubdir";
            this.tbRightImageSubdir.Size = new System.Drawing.Size(231, 20);
            this.tbRightImageSubdir.TabIndex = 44;
            // 
            // lblRightImageRoot
            // 
            this.lblRightImageRoot.Location = new System.Drawing.Point(6, 16);
            this.lblRightImageRoot.Name = "lblRightImageRoot";
            this.lblRightImageRoot.Size = new System.Drawing.Size(58, 17);
            this.lblRightImageRoot.TabIndex = 38;
            this.lblRightImageRoot.Text = "Root :";
            // 
            // tbRightImageRoot
            // 
            this.tbRightImageRoot.Location = new System.Drawing.Point(145, 13);
            this.tbRightImageRoot.Name = "tbRightImageRoot";
            this.tbRightImageRoot.Size = new System.Drawing.Size(231, 20);
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
            this.grpLeftImage.Controls.Add(this.btnBrowseImage);
            this.grpLeftImage.Location = new System.Drawing.Point(6, 6);
            this.grpLeftImage.Name = "grpLeftImage";
            this.grpLeftImage.Size = new System.Drawing.Size(412, 94);
            this.grpLeftImage.TabIndex = 44;
            this.grpLeftImage.TabStop = false;
            this.grpLeftImage.Text = "Left";
            // 
            // lblLeftImageFile
            // 
            this.lblLeftImageFile.Location = new System.Drawing.Point(6, 68);
            this.lblLeftImageFile.Name = "lblLeftImageFile";
            this.lblLeftImageFile.Size = new System.Drawing.Size(103, 17);
            this.lblLeftImageFile.TabIndex = 45;
            this.lblLeftImageFile.Text = "File :";
            // 
            // tbLeftImageFile
            // 
            this.tbLeftImageFile.Location = new System.Drawing.Point(145, 65);
            this.tbLeftImageFile.Name = "tbLeftImageFile";
            this.tbLeftImageFile.Size = new System.Drawing.Size(231, 20);
            this.tbLeftImageFile.TabIndex = 46;
            // 
            // lblLeftImageSubdir
            // 
            this.lblLeftImageSubdir.Location = new System.Drawing.Point(6, 42);
            this.lblLeftImageSubdir.Name = "lblLeftImageSubdir";
            this.lblLeftImageSubdir.Size = new System.Drawing.Size(103, 17);
            this.lblLeftImageSubdir.TabIndex = 43;
            this.lblLeftImageSubdir.Text = "Sub directory :";
            // 
            // tbLeftImageSubdir
            // 
            this.tbLeftImageSubdir.Location = new System.Drawing.Point(145, 39);
            this.tbLeftImageSubdir.Name = "tbLeftImageSubdir";
            this.tbLeftImageSubdir.Size = new System.Drawing.Size(231, 20);
            this.tbLeftImageSubdir.TabIndex = 44;
            // 
            // lblLeftImageRoot
            // 
            this.lblLeftImageRoot.Location = new System.Drawing.Point(6, 16);
            this.lblLeftImageRoot.Name = "lblLeftImageRoot";
            this.lblLeftImageRoot.Size = new System.Drawing.Size(58, 17);
            this.lblLeftImageRoot.TabIndex = 38;
            this.lblLeftImageRoot.Text = "Root :";
            // 
            // tbLeftImageRoot
            // 
            this.tbLeftImageRoot.Location = new System.Drawing.Point(145, 13);
            this.tbLeftImageRoot.Name = "tbLeftImageRoot";
            this.tbLeftImageRoot.Size = new System.Drawing.Size(231, 20);
            this.tbLeftImageRoot.TabIndex = 40;
            // 
            // tabVideoNaming
            // 
            this.tabVideoNaming.Controls.Add(this.grpRightVideo);
            this.tabVideoNaming.Controls.Add(this.grpLeftVideo);
            this.tabVideoNaming.Location = new System.Drawing.Point(4, 22);
            this.tabVideoNaming.Name = "tabVideoNaming";
            this.tabVideoNaming.Padding = new System.Windows.Forms.Padding(3);
            this.tabVideoNaming.Size = new System.Drawing.Size(424, 210);
            this.tabVideoNaming.TabIndex = 3;
            this.tabVideoNaming.Text = "Video naming";
            this.tabVideoNaming.UseVisualStyleBackColor = true;
            // 
            // grpRightVideo
            // 
            this.grpRightVideo.Controls.Add(this.btnRightVideoFile);
            this.grpRightVideo.Controls.Add(this.btnRightVideoSubdir);
            this.grpRightVideo.Controls.Add(this.button8);
            this.grpRightVideo.Controls.Add(this.lblRightVideoFile);
            this.grpRightVideo.Controls.Add(this.tbRightVideoFile);
            this.grpRightVideo.Controls.Add(this.lblRightVideoSubdir);
            this.grpRightVideo.Controls.Add(this.tbRightVideoSubdir);
            this.grpRightVideo.Controls.Add(this.lblRightVideoRoot);
            this.grpRightVideo.Controls.Add(this.tbRightVideoRoot);
            this.grpRightVideo.Location = new System.Drawing.Point(6, 106);
            this.grpRightVideo.Name = "grpRightVideo";
            this.grpRightVideo.Size = new System.Drawing.Size(412, 94);
            this.grpRightVideo.TabIndex = 49;
            this.grpRightVideo.TabStop = false;
            this.grpRightVideo.Text = "Right";
            // 
            // lblRightVideoFile
            // 
            this.lblRightVideoFile.Location = new System.Drawing.Point(6, 68);
            this.lblRightVideoFile.Name = "lblRightVideoFile";
            this.lblRightVideoFile.Size = new System.Drawing.Size(103, 17);
            this.lblRightVideoFile.TabIndex = 45;
            this.lblRightVideoFile.Text = "File :";
            // 
            // tbRightVideoFile
            // 
            this.tbRightVideoFile.Location = new System.Drawing.Point(145, 65);
            this.tbRightVideoFile.Name = "tbRightVideoFile";
            this.tbRightVideoFile.Size = new System.Drawing.Size(231, 20);
            this.tbRightVideoFile.TabIndex = 46;
            // 
            // lblRightVideoSubdir
            // 
            this.lblRightVideoSubdir.Location = new System.Drawing.Point(6, 42);
            this.lblRightVideoSubdir.Name = "lblRightVideoSubdir";
            this.lblRightVideoSubdir.Size = new System.Drawing.Size(103, 17);
            this.lblRightVideoSubdir.TabIndex = 43;
            this.lblRightVideoSubdir.Text = "Sub directory :";
            // 
            // tbRightVideoSubdir
            // 
            this.tbRightVideoSubdir.Location = new System.Drawing.Point(145, 39);
            this.tbRightVideoSubdir.Name = "tbRightVideoSubdir";
            this.tbRightVideoSubdir.Size = new System.Drawing.Size(231, 20);
            this.tbRightVideoSubdir.TabIndex = 44;
            // 
            // lblRightVideoRoot
            // 
            this.lblRightVideoRoot.Location = new System.Drawing.Point(6, 16);
            this.lblRightVideoRoot.Name = "lblRightVideoRoot";
            this.lblRightVideoRoot.Size = new System.Drawing.Size(58, 17);
            this.lblRightVideoRoot.TabIndex = 38;
            this.lblRightVideoRoot.Text = "Root :";
            // 
            // tbRightVideoRoot
            // 
            this.tbRightVideoRoot.Location = new System.Drawing.Point(145, 13);
            this.tbRightVideoRoot.Name = "tbRightVideoRoot";
            this.tbRightVideoRoot.Size = new System.Drawing.Size(231, 20);
            this.tbRightVideoRoot.TabIndex = 40;
            // 
            // grpLeftVideo
            // 
            this.grpLeftVideo.Controls.Add(this.button12);
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
            this.grpLeftVideo.Size = new System.Drawing.Size(412, 94);
            this.grpLeftVideo.TabIndex = 48;
            this.grpLeftVideo.TabStop = false;
            this.grpLeftVideo.Text = "Left";
            // 
            // lblLeftVideoFile
            // 
            this.lblLeftVideoFile.Location = new System.Drawing.Point(6, 68);
            this.lblLeftVideoFile.Name = "lblLeftVideoFile";
            this.lblLeftVideoFile.Size = new System.Drawing.Size(103, 17);
            this.lblLeftVideoFile.TabIndex = 45;
            this.lblLeftVideoFile.Text = "File :";
            // 
            // tbLeftVideoFile
            // 
            this.tbLeftVideoFile.Location = new System.Drawing.Point(145, 65);
            this.tbLeftVideoFile.Name = "tbLeftVideoFile";
            this.tbLeftVideoFile.Size = new System.Drawing.Size(231, 20);
            this.tbLeftVideoFile.TabIndex = 46;
            // 
            // lblLeftVideoSubdir
            // 
            this.lblLeftVideoSubdir.Location = new System.Drawing.Point(6, 42);
            this.lblLeftVideoSubdir.Name = "lblLeftVideoSubdir";
            this.lblLeftVideoSubdir.Size = new System.Drawing.Size(103, 17);
            this.lblLeftVideoSubdir.TabIndex = 43;
            this.lblLeftVideoSubdir.Text = "Sub directory :";
            // 
            // tbLeftVideoSubdir
            // 
            this.tbLeftVideoSubdir.Location = new System.Drawing.Point(145, 39);
            this.tbLeftVideoSubdir.Name = "tbLeftVideoSubdir";
            this.tbLeftVideoSubdir.Size = new System.Drawing.Size(231, 20);
            this.tbLeftVideoSubdir.TabIndex = 44;
            // 
            // lblLeftVideoRoot
            // 
            this.lblLeftVideoRoot.Location = new System.Drawing.Point(6, 16);
            this.lblLeftVideoRoot.Name = "lblLeftVideoRoot";
            this.lblLeftVideoRoot.Size = new System.Drawing.Size(58, 17);
            this.lblLeftVideoRoot.TabIndex = 38;
            this.lblLeftVideoRoot.Text = "Root :";
            // 
            // tbLeftVideoRoot
            // 
            this.tbLeftVideoRoot.Location = new System.Drawing.Point(145, 13);
            this.tbLeftVideoRoot.Name = "tbLeftVideoRoot";
            this.tbLeftVideoRoot.Size = new System.Drawing.Size(230, 20);
            this.tbLeftVideoRoot.TabIndex = 40;
            // 
            // tabMemory
            // 
            this.tabMemory.Controls.Add(this.lblMemoryBuffer);
            this.tabMemory.Controls.Add(this.trkMemoryBuffer);
            this.tabMemory.Location = new System.Drawing.Point(4, 22);
            this.tabMemory.Name = "tabMemory";
            this.tabMemory.Size = new System.Drawing.Size(424, 210);
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
            this.trkMemoryBuffer.Size = new System.Drawing.Size(386, 45);
            this.trkMemoryBuffer.TabIndex = 38;
            this.trkMemoryBuffer.TickFrequency = 50;
            this.trkMemoryBuffer.Value = 16;
            // 
            // btnRightImageFile
            // 
            this.btnRightImageFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRightImageFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRightImageFile.FlatAppearance.BorderSize = 0;
            this.btnRightImageFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRightImageFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRightImageFile.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnRightImageFile.Location = new System.Drawing.Point(382, 64);
            this.btnRightImageFile.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnRightImageFile.Name = "btnRightImageFile";
            this.btnRightImageFile.Size = new System.Drawing.Size(20, 20);
            this.btnRightImageFile.TabIndex = 51;
            this.btnRightImageFile.Tag = "";
            this.btnRightImageFile.UseVisualStyleBackColor = true;
            // 
            // btnRightImageSubdir
            // 
            this.btnRightImageSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRightImageSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRightImageSubdir.FlatAppearance.BorderSize = 0;
            this.btnRightImageSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRightImageSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRightImageSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnRightImageSubdir.Location = new System.Drawing.Point(382, 38);
            this.btnRightImageSubdir.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnRightImageSubdir.Name = "btnRightImageSubdir";
            this.btnRightImageSubdir.Size = new System.Drawing.Size(20, 20);
            this.btnRightImageSubdir.TabIndex = 50;
            this.btnRightImageSubdir.Tag = "";
            this.btnRightImageSubdir.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.button5.Location = new System.Drawing.Point(382, 12);
            this.button5.MinimumSize = new System.Drawing.Size(20, 20);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(20, 20);
            this.button5.TabIndex = 49;
            this.button5.Tag = "";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // btnLeftImageFile
            // 
            this.btnLeftImageFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLeftImageFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLeftImageFile.FlatAppearance.BorderSize = 0;
            this.btnLeftImageFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnLeftImageFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeftImageFile.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnLeftImageFile.Location = new System.Drawing.Point(382, 64);
            this.btnLeftImageFile.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnLeftImageFile.Name = "btnLeftImageFile";
            this.btnLeftImageFile.Size = new System.Drawing.Size(20, 20);
            this.btnLeftImageFile.TabIndex = 48;
            this.btnLeftImageFile.Tag = "";
            this.btnLeftImageFile.UseVisualStyleBackColor = true;
            // 
            // btnLeftImageSubdir
            // 
            this.btnLeftImageSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLeftImageSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLeftImageSubdir.FlatAppearance.BorderSize = 0;
            this.btnLeftImageSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnLeftImageSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeftImageSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnLeftImageSubdir.Location = new System.Drawing.Point(382, 38);
            this.btnLeftImageSubdir.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnLeftImageSubdir.Name = "btnLeftImageSubdir";
            this.btnLeftImageSubdir.Size = new System.Drawing.Size(20, 20);
            this.btnLeftImageSubdir.TabIndex = 47;
            this.btnLeftImageSubdir.Tag = "";
            this.btnLeftImageSubdir.UseVisualStyleBackColor = true;
            // 
            // btnBrowseImage
            // 
            this.btnBrowseImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnBrowseImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseImage.FlatAppearance.BorderSize = 0;
            this.btnBrowseImage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnBrowseImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseImage.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.btnBrowseImage.Location = new System.Drawing.Point(382, 12);
            this.btnBrowseImage.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Size = new System.Drawing.Size(20, 20);
            this.btnBrowseImage.TabIndex = 42;
            this.btnBrowseImage.Tag = "";
            this.btnBrowseImage.UseVisualStyleBackColor = true;
            // 
            // btnRightVideoFile
            // 
            this.btnRightVideoFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRightVideoFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRightVideoFile.FlatAppearance.BorderSize = 0;
            this.btnRightVideoFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRightVideoFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRightVideoFile.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnRightVideoFile.Location = new System.Drawing.Point(382, 64);
            this.btnRightVideoFile.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnRightVideoFile.Name = "btnRightVideoFile";
            this.btnRightVideoFile.Size = new System.Drawing.Size(20, 20);
            this.btnRightVideoFile.TabIndex = 51;
            this.btnRightVideoFile.Tag = "";
            this.btnRightVideoFile.UseVisualStyleBackColor = true;
            // 
            // btnRightVideoSubdir
            // 
            this.btnRightVideoSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnRightVideoSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRightVideoSubdir.FlatAppearance.BorderSize = 0;
            this.btnRightVideoSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnRightVideoSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRightVideoSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnRightVideoSubdir.Location = new System.Drawing.Point(382, 38);
            this.btnRightVideoSubdir.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnRightVideoSubdir.Name = "btnRightVideoSubdir";
            this.btnRightVideoSubdir.Size = new System.Drawing.Size(20, 20);
            this.btnRightVideoSubdir.TabIndex = 50;
            this.btnRightVideoSubdir.Tag = "";
            this.btnRightVideoSubdir.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            this.button8.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button8.FlatAppearance.BorderSize = 0;
            this.button8.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button8.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.button8.Location = new System.Drawing.Point(382, 12);
            this.button8.MinimumSize = new System.Drawing.Size(20, 20);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(20, 20);
            this.button8.TabIndex = 49;
            this.button8.Tag = "";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            this.button12.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button12.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button12.FlatAppearance.BorderSize = 0;
            this.button12.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.button12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button12.Image = global::Kinovea.Root.Properties.Resources.folder;
            this.button12.Location = new System.Drawing.Point(381, 12);
            this.button12.MinimumSize = new System.Drawing.Size(20, 20);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(20, 20);
            this.button12.TabIndex = 49;
            this.button12.Tag = "";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // btnLeftVideoFile
            // 
            this.btnLeftVideoFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLeftVideoFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLeftVideoFile.FlatAppearance.BorderSize = 0;
            this.btnLeftVideoFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnLeftVideoFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeftVideoFile.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnLeftVideoFile.Location = new System.Drawing.Point(382, 64);
            this.btnLeftVideoFile.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnLeftVideoFile.Name = "btnLeftVideoFile";
            this.btnLeftVideoFile.Size = new System.Drawing.Size(20, 20);
            this.btnLeftVideoFile.TabIndex = 48;
            this.btnLeftVideoFile.Tag = "";
            this.btnLeftVideoFile.UseVisualStyleBackColor = true;
            // 
            // btnLeftVideoSubdir
            // 
            this.btnLeftVideoSubdir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLeftVideoSubdir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLeftVideoSubdir.FlatAppearance.BorderSize = 0;
            this.btnLeftVideoSubdir.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnLeftVideoSubdir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeftVideoSubdir.Image = global::Kinovea.Root.Properties.Resources.percent;
            this.btnLeftVideoSubdir.Location = new System.Drawing.Point(382, 38);
            this.btnLeftVideoSubdir.MinimumSize = new System.Drawing.Size(20, 20);
            this.btnLeftVideoSubdir.Name = "btnLeftVideoSubdir";
            this.btnLeftVideoSubdir.Size = new System.Drawing.Size(20, 20);
            this.btnLeftVideoSubdir.TabIndex = 47;
            this.btnLeftVideoSubdir.Tag = "";
            this.btnLeftVideoSubdir.UseVisualStyleBackColor = true;
            // 
            // PreferencePanelCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabSubPages);
            this.Name = "PreferencePanelCapture";
            this.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.grpDSS.ResumeLayout(false);
            this.grpDSS.PerformLayout();
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
            this.tabMemory.ResumeLayout(false);
            this.tabMemory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).EndInit();
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
        private System.Windows.Forms.GroupBox grpDSS;
        private System.Windows.Forms.Label lblFramerate;
        private System.Windows.Forms.TextBox tbFramerate;
        private System.Windows.Forms.RadioButton rbForcedFramerate;
        private System.Windows.Forms.RadioButton rbCameraFrameSignal;
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
        private System.Windows.Forms.Button btnBrowseImage;
        private System.Windows.Forms.Button btnRightImageFile;
        private System.Windows.Forms.Button btnRightImageSubdir;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btnLeftImageFile;
        private System.Windows.Forms.Button btnLeftImageSubdir;
        private System.Windows.Forms.TabPage tabVideoNaming;
        private System.Windows.Forms.GroupBox grpRightVideo;
        private System.Windows.Forms.Button btnRightVideoFile;
        private System.Windows.Forms.Button btnRightVideoSubdir;
        private System.Windows.Forms.Button button8;
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
        private System.Windows.Forms.Button button12;
	}
}
