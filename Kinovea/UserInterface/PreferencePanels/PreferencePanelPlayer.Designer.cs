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
      this.chkSyncByMotion = new System.Windows.Forms.CheckBox();
      this.chkDetectImageSequences = new System.Windows.Forms.CheckBox();
      this.chkInteractiveTracker = new System.Windows.Forms.CheckBox();
      this.cmbImageFormats = new System.Windows.Forms.ComboBox();
      this.lblImageFormat = new System.Windows.Forms.Label();
      this.chkLockSpeeds = new System.Windows.Forms.CheckBox();
      this.tabMemory = new System.Windows.Forms.TabPage();
      this.tabUnits = new System.Windows.Forms.TabPage();
      this.tbCustomLengthAb = new System.Windows.Forms.TextBox();
      this.tbCustomLengthUnit = new System.Windows.Forms.TextBox();
      this.lblCustomLength = new System.Windows.Forms.Label();
      this.cmbAngularAccelerationUnit = new System.Windows.Forms.ComboBox();
      this.lblAngularAcceleration = new System.Windows.Forms.Label();
      this.cmbAngularVelocityUnit = new System.Windows.Forms.ComboBox();
      this.lblAngularVelocityUnit = new System.Windows.Forms.Label();
      this.cmbAngleUnit = new System.Windows.Forms.ComboBox();
      this.lblAngleUnit = new System.Windows.Forms.Label();
      this.cmbAccelerationUnit = new System.Windows.Forms.ComboBox();
      this.lblAccelerationUnit = new System.Windows.Forms.Label();
      this.cmbSpeedUnit = new System.Windows.Forms.ComboBox();
      this.lblSpeedUnit = new System.Windows.Forms.Label();
      this.cmbTimeCodeFormat = new System.Windows.Forms.ComboBox();
      this.lblTimeMarkersFormat = new System.Windows.Forms.Label();
      this.lblPlaybackKVA = new System.Windows.Forms.Label();
      this.tbPlaybackKVA = new System.Windows.Forms.TextBox();
      this.btnPlaybackKVA = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.trkMemoryBuffer)).BeginInit();
      this.tabSubPages.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabMemory.SuspendLayout();
      this.tabUnits.SuspendLayout();
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
      this.tabSubPages.Controls.Add(this.tabUnits);
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
      // tabUnits
      // 
      this.tabUnits.Controls.Add(this.tbCustomLengthAb);
      this.tabUnits.Controls.Add(this.tbCustomLengthUnit);
      this.tabUnits.Controls.Add(this.lblCustomLength);
      this.tabUnits.Controls.Add(this.cmbAngularAccelerationUnit);
      this.tabUnits.Controls.Add(this.lblAngularAcceleration);
      this.tabUnits.Controls.Add(this.cmbAngularVelocityUnit);
      this.tabUnits.Controls.Add(this.lblAngularVelocityUnit);
      this.tabUnits.Controls.Add(this.cmbAngleUnit);
      this.tabUnits.Controls.Add(this.lblAngleUnit);
      this.tabUnits.Controls.Add(this.cmbAccelerationUnit);
      this.tabUnits.Controls.Add(this.lblAccelerationUnit);
      this.tabUnits.Controls.Add(this.cmbSpeedUnit);
      this.tabUnits.Controls.Add(this.lblSpeedUnit);
      this.tabUnits.Controls.Add(this.cmbTimeCodeFormat);
      this.tabUnits.Controls.Add(this.lblTimeMarkersFormat);
      this.tabUnits.Location = new System.Drawing.Point(4, 22);
      this.tabUnits.Name = "tabUnits";
      this.tabUnits.Padding = new System.Windows.Forms.Padding(3);
      this.tabUnits.Size = new System.Drawing.Size(482, 296);
      this.tabUnits.TabIndex = 2;
      this.tabUnits.Text = "Units";
      this.tabUnits.UseVisualStyleBackColor = true;
      // 
      // tbCustomLengthAb
      // 
      this.tbCustomLengthAb.Location = new System.Drawing.Point(349, 237);
      this.tbCustomLengthAb.Name = "tbCustomLengthAb";
      this.tbCustomLengthAb.Size = new System.Drawing.Size(49, 20);
      this.tbCustomLengthAb.TabIndex = 44;
      this.tbCustomLengthAb.TextChanged += new System.EventHandler(this.tbCustomLengthAb_TextChanged);
      // 
      // tbCustomLengthUnit
      // 
      this.tbCustomLengthUnit.Location = new System.Drawing.Point(205, 237);
      this.tbCustomLengthUnit.Name = "tbCustomLengthUnit";
      this.tbCustomLengthUnit.Size = new System.Drawing.Size(138, 20);
      this.tbCustomLengthUnit.TabIndex = 43;
      this.tbCustomLengthUnit.TextChanged += new System.EventHandler(this.tbCustomLengthUnit_TextChanged);
      // 
      // lblCustomLength
      // 
      this.lblCustomLength.AutoSize = true;
      this.lblCustomLength.Location = new System.Drawing.Point(21, 241);
      this.lblCustomLength.Name = "lblCustomLength";
      this.lblCustomLength.Size = new System.Drawing.Size(100, 13);
      this.lblCustomLength.TabIndex = 42;
      this.lblCustomLength.Text = "Custom length unit :";
      // 
      // cmbAngularAccelerationUnit
      // 
      this.cmbAngularAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngularAccelerationUnit.Location = new System.Drawing.Point(177, 195);
      this.cmbAngularAccelerationUnit.Name = "cmbAngularAccelerationUnit";
      this.cmbAngularAccelerationUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngularAccelerationUnit.TabIndex = 41;
      this.cmbAngularAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularAccelerationUnit_SelectedIndexChanged);
      // 
      // lblAngularAcceleration
      // 
      this.lblAngularAcceleration.AutoSize = true;
      this.lblAngularAcceleration.Location = new System.Drawing.Point(21, 200);
      this.lblAngularAcceleration.Name = "lblAngularAcceleration";
      this.lblAngularAcceleration.Size = new System.Drawing.Size(110, 13);
      this.lblAngularAcceleration.TabIndex = 40;
      this.lblAngularAcceleration.Text = "Angular acceleration :";
      // 
      // cmbAngularVelocityUnit
      // 
      this.cmbAngularVelocityUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngularVelocityUnit.Location = new System.Drawing.Point(177, 161);
      this.cmbAngularVelocityUnit.Name = "cmbAngularVelocityUnit";
      this.cmbAngularVelocityUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngularVelocityUnit.TabIndex = 39;
      this.cmbAngularVelocityUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularVelocityUnit_SelectedIndexChanged);
      // 
      // lblAngularVelocityUnit
      // 
      this.lblAngularVelocityUnit.AutoSize = true;
      this.lblAngularVelocityUnit.Location = new System.Drawing.Point(21, 166);
      this.lblAngularVelocityUnit.Name = "lblAngularVelocityUnit";
      this.lblAngularVelocityUnit.Size = new System.Drawing.Size(81, 13);
      this.lblAngularVelocityUnit.TabIndex = 38;
      this.lblAngularVelocityUnit.Text = "Angular speed :";
      // 
      // cmbAngleUnit
      // 
      this.cmbAngleUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAngleUnit.Location = new System.Drawing.Point(177, 127);
      this.cmbAngleUnit.Name = "cmbAngleUnit";
      this.cmbAngleUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAngleUnit.TabIndex = 37;
      this.cmbAngleUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngleUnit_SelectedIndexChanged);
      // 
      // lblAngleUnit
      // 
      this.lblAngleUnit.AutoSize = true;
      this.lblAngleUnit.Location = new System.Drawing.Point(21, 132);
      this.lblAngleUnit.Name = "lblAngleUnit";
      this.lblAngleUnit.Size = new System.Drawing.Size(40, 13);
      this.lblAngleUnit.TabIndex = 36;
      this.lblAngleUnit.Text = "Angle :";
      // 
      // cmbAccelerationUnit
      // 
      this.cmbAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbAccelerationUnit.Location = new System.Drawing.Point(177, 93);
      this.cmbAccelerationUnit.Name = "cmbAccelerationUnit";
      this.cmbAccelerationUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbAccelerationUnit.TabIndex = 35;
      this.cmbAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAccelerationUnit_SelectedIndexChanged);
      // 
      // lblAccelerationUnit
      // 
      this.lblAccelerationUnit.AutoSize = true;
      this.lblAccelerationUnit.Location = new System.Drawing.Point(21, 98);
      this.lblAccelerationUnit.Name = "lblAccelerationUnit";
      this.lblAccelerationUnit.Size = new System.Drawing.Size(72, 13);
      this.lblAccelerationUnit.TabIndex = 34;
      this.lblAccelerationUnit.Text = "Acceleration :";
      // 
      // cmbSpeedUnit
      // 
      this.cmbSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbSpeedUnit.Location = new System.Drawing.Point(177, 59);
      this.cmbSpeedUnit.Name = "cmbSpeedUnit";
      this.cmbSpeedUnit.Size = new System.Drawing.Size(221, 21);
      this.cmbSpeedUnit.TabIndex = 33;
      this.cmbSpeedUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSpeedUnit_SelectedIndexChanged);
      // 
      // lblSpeedUnit
      // 
      this.lblSpeedUnit.AutoSize = true;
      this.lblSpeedUnit.Location = new System.Drawing.Point(21, 64);
      this.lblSpeedUnit.Name = "lblSpeedUnit";
      this.lblSpeedUnit.Size = new System.Drawing.Size(50, 13);
      this.lblSpeedUnit.TabIndex = 32;
      this.lblSpeedUnit.Text = "Velocity :";
      // 
      // cmbTimeCodeFormat
      // 
      this.cmbTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbTimeCodeFormat.Location = new System.Drawing.Point(177, 25);
      this.cmbTimeCodeFormat.Name = "cmbTimeCodeFormat";
      this.cmbTimeCodeFormat.Size = new System.Drawing.Size(221, 21);
      this.cmbTimeCodeFormat.TabIndex = 28;
      this.cmbTimeCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeCodeFormat_SelectedIndexChanged);
      // 
      // lblTimeMarkersFormat
      // 
      this.lblTimeMarkersFormat.AutoSize = true;
      this.lblTimeMarkersFormat.Location = new System.Drawing.Point(21, 28);
      this.lblTimeMarkersFormat.Name = "lblTimeMarkersFormat";
      this.lblTimeMarkersFormat.Size = new System.Drawing.Size(108, 13);
      this.lblTimeMarkersFormat.TabIndex = 27;
      this.lblTimeMarkersFormat.Text = "Time markers format :";
      // 
      // lblPlaybackKVA
      // 
      this.lblPlaybackKVA.Location = new System.Drawing.Point(20, 227);
      this.lblPlaybackKVA.Name = "lblPlaybackKVA";
      this.lblPlaybackKVA.Size = new System.Drawing.Size(185, 18);
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
      this.tabUnits.ResumeLayout(false);
      this.tabUnits.PerformLayout();
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
        private System.Windows.Forms.TabPage tabUnits;
        private System.Windows.Forms.ComboBox cmbAngleUnit;
        private System.Windows.Forms.Label lblAngleUnit;
        private System.Windows.Forms.ComboBox cmbAccelerationUnit;
        private System.Windows.Forms.Label lblAccelerationUnit;
        private System.Windows.Forms.ComboBox cmbSpeedUnit;
        private System.Windows.Forms.Label lblSpeedUnit;
        private System.Windows.Forms.ComboBox cmbTimeCodeFormat;
        private System.Windows.Forms.Label lblTimeMarkersFormat;
        private System.Windows.Forms.ComboBox cmbAngularVelocityUnit;
        private System.Windows.Forms.Label lblAngularVelocityUnit;
        private System.Windows.Forms.ComboBox cmbAngularAccelerationUnit;
        private System.Windows.Forms.Label lblAngularAcceleration;
        private System.Windows.Forms.CheckBox chkInteractiveTracker;
        private System.Windows.Forms.Label lblCustomLength;
        private System.Windows.Forms.TextBox tbCustomLengthAb;
        private System.Windows.Forms.TextBox tbCustomLengthUnit;
        private System.Windows.Forms.CheckBox chkDetectImageSequences;
        private System.Windows.Forms.CheckBox chkSyncByMotion;
        private System.Windows.Forms.Label lblPlaybackKVA;
        private System.Windows.Forms.TextBox tbPlaybackKVA;
        private System.Windows.Forms.Button btnPlaybackKVA;
    }
}
