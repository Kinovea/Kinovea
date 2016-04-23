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
            this.grpSwitchToAnalysis = new System.Windows.Forms.GroupBox();
            this.lblWorkingZoneLogic = new System.Windows.Forms.Label();
            this.trkWorkingZoneSeconds = new System.Windows.Forms.TrackBar();
            this.lblWorkingZoneSeconds = new System.Windows.Forms.Label();
            this.trkWorkingZoneMemory = new System.Windows.Forms.TrackBar();
            this.lblWorkingZoneMemory = new System.Windows.Forms.Label();
            this.tabSubPages = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkInteractiveTracker = new System.Windows.Forms.CheckBox();
            this.cmbImageFormats = new System.Windows.Forms.ComboBox();
            this.lblImageFormat = new System.Windows.Forms.Label();
            this.chkLockSpeeds = new System.Windows.Forms.CheckBox();
            this.tabUnits = new System.Windows.Forms.TabPage();
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
            this.tabMemory = new System.Windows.Forms.TabPage();
            this.grpSwitchToAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).BeginInit();
            this.tabSubPages.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabUnits.SuspendLayout();
            this.tabMemory.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkDeinterlace
            // 
            this.chkDeinterlace.Location = new System.Drawing.Point(29, 24);
            this.chkDeinterlace.Name = "chkDeinterlace";
            this.chkDeinterlace.Size = new System.Drawing.Size(369, 20);
            this.chkDeinterlace.TabIndex = 23;
            this.chkDeinterlace.Text = "dlgPreferences_DeinterlaceByDefault";
            this.chkDeinterlace.UseVisualStyleBackColor = true;
            this.chkDeinterlace.CheckedChanged += new System.EventHandler(this.ChkDeinterlaceCheckedChanged);
            // 
            // grpSwitchToAnalysis
            // 
            this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneLogic);
            this.grpSwitchToAnalysis.Controls.Add(this.trkWorkingZoneSeconds);
            this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneSeconds);
            this.grpSwitchToAnalysis.Controls.Add(this.trkWorkingZoneMemory);
            this.grpSwitchToAnalysis.Controls.Add(this.lblWorkingZoneMemory);
            this.grpSwitchToAnalysis.Location = new System.Drawing.Point(7, 11);
            this.grpSwitchToAnalysis.Name = "grpSwitchToAnalysis";
            this.grpSwitchToAnalysis.Size = new System.Drawing.Size(405, 193);
            this.grpSwitchToAnalysis.TabIndex = 26;
            this.grpSwitchToAnalysis.TabStop = false;
            this.grpSwitchToAnalysis.Text = "Switch to Analysis Mode";
            // 
            // lblWorkingZoneLogic
            // 
            this.lblWorkingZoneLogic.AutoSize = true;
            this.lblWorkingZoneLogic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWorkingZoneLogic.Location = new System.Drawing.Point(12, 93);
            this.lblWorkingZoneLogic.Name = "lblWorkingZoneLogic";
            this.lblWorkingZoneLogic.Size = new System.Drawing.Size(29, 13);
            this.lblWorkingZoneLogic.TabIndex = 37;
            this.lblWorkingZoneLogic.Text = "And";
            // 
            // trkWorkingZoneSeconds
            // 
            this.trkWorkingZoneSeconds.BackColor = System.Drawing.Color.White;
            this.trkWorkingZoneSeconds.Location = new System.Drawing.Point(9, 43);
            this.trkWorkingZoneSeconds.Maximum = 30;
            this.trkWorkingZoneSeconds.Minimum = 1;
            this.trkWorkingZoneSeconds.Name = "trkWorkingZoneSeconds";
            this.trkWorkingZoneSeconds.Size = new System.Drawing.Size(386, 45);
            this.trkWorkingZoneSeconds.TabIndex = 38;
            this.trkWorkingZoneSeconds.Value = 12;
            this.trkWorkingZoneSeconds.ValueChanged += new System.EventHandler(this.trkWorkingZoneSeconds_ValueChanged);
            // 
            // lblWorkingZoneSeconds
            // 
            this.lblWorkingZoneSeconds.AutoSize = true;
            this.lblWorkingZoneSeconds.Location = new System.Drawing.Point(14, 26);
            this.lblWorkingZoneSeconds.Name = "lblWorkingZoneSeconds";
            this.lblWorkingZoneSeconds.Size = new System.Drawing.Size(191, 13);
            this.lblWorkingZoneSeconds.TabIndex = 36;
            this.lblWorkingZoneSeconds.Text = "Working Zone is less than 12 seconds.";
            // 
            // trkWorkingZoneMemory
            // 
            this.trkWorkingZoneMemory.BackColor = System.Drawing.Color.White;
            this.trkWorkingZoneMemory.Location = new System.Drawing.Point(10, 137);
            this.trkWorkingZoneMemory.Maximum = 1024;
            this.trkWorkingZoneMemory.Minimum = 16;
            this.trkWorkingZoneMemory.Name = "trkWorkingZoneMemory";
            this.trkWorkingZoneMemory.Size = new System.Drawing.Size(390, 45);
            this.trkWorkingZoneMemory.TabIndex = 35;
            this.trkWorkingZoneMemory.TickFrequency = 50;
            this.trkWorkingZoneMemory.Value = 512;
            this.trkWorkingZoneMemory.ValueChanged += new System.EventHandler(this.trkWorkingZoneMemory_ValueChanged);
            // 
            // lblWorkingZoneMemory
            // 
            this.lblWorkingZoneMemory.AutoSize = true;
            this.lblWorkingZoneMemory.Location = new System.Drawing.Point(14, 119);
            this.lblWorkingZoneMemory.Name = "lblWorkingZoneMemory";
            this.lblWorkingZoneMemory.Size = new System.Drawing.Size(257, 13);
            this.lblWorkingZoneMemory.TabIndex = 17;
            this.lblWorkingZoneMemory.Text = "Working Zone will take less than 512 Mib of Memory.";
            // 
            // tabSubPages
            // 
            this.tabSubPages.Controls.Add(this.tabGeneral);
            this.tabSubPages.Controls.Add(this.tabUnits);
            this.tabSubPages.Controls.Add(this.tabMemory);
            this.tabSubPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSubPages.Location = new System.Drawing.Point(0, 0);
            this.tabSubPages.Name = "tabSubPages";
            this.tabSubPages.SelectedIndex = 0;
            this.tabSubPages.Size = new System.Drawing.Size(432, 236);
            this.tabSubPages.TabIndex = 27;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.chkInteractiveTracker);
            this.tabGeneral.Controls.Add(this.cmbImageFormats);
            this.tabGeneral.Controls.Add(this.lblImageFormat);
            this.tabGeneral.Controls.Add(this.chkLockSpeeds);
            this.tabGeneral.Controls.Add(this.chkDeinterlace);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(424, 210);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // chkInteractiveTracker
            // 
            this.chkInteractiveTracker.Location = new System.Drawing.Point(27, 76);
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
            this.cmbImageFormats.Location = new System.Drawing.Point(217, 130);
            this.cmbImageFormats.Name = "cmbImageFormats";
            this.cmbImageFormats.Size = new System.Drawing.Size(183, 21);
            this.cmbImageFormats.TabIndex = 29;
            this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
            // 
            // lblImageFormat
            // 
            this.lblImageFormat.AutoSize = true;
            this.lblImageFormat.Location = new System.Drawing.Point(24, 134);
            this.lblImageFormat.Name = "lblImageFormat";
            this.lblImageFormat.Size = new System.Drawing.Size(110, 13);
            this.lblImageFormat.TabIndex = 28;
            this.lblImageFormat.Text = "Default image format :";
            // 
            // chkLockSpeeds
            // 
            this.chkLockSpeeds.Location = new System.Drawing.Point(29, 50);
            this.chkLockSpeeds.Name = "chkLockSpeeds";
            this.chkLockSpeeds.Size = new System.Drawing.Size(369, 20);
            this.chkLockSpeeds.TabIndex = 24;
            this.chkLockSpeeds.Text = "dlgPreferences_SyncLockSpeeds";
            this.chkLockSpeeds.UseVisualStyleBackColor = true;
            this.chkLockSpeeds.CheckedChanged += new System.EventHandler(this.ChkLockSpeedsCheckedChanged);
            // 
            // tabUnits
            // 
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
            this.tabUnits.Size = new System.Drawing.Size(424, 210);
            this.tabUnits.TabIndex = 2;
            this.tabUnits.Text = "Units";
            this.tabUnits.UseVisualStyleBackColor = true;
            // 
            // cmbAngularAccelerationUnit
            // 
            this.cmbAngularAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAngularAccelerationUnit.Location = new System.Drawing.Point(177, 167);
            this.cmbAngularAccelerationUnit.Name = "cmbAngularAccelerationUnit";
            this.cmbAngularAccelerationUnit.Size = new System.Drawing.Size(221, 21);
            this.cmbAngularAccelerationUnit.TabIndex = 41;
            this.cmbAngularAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularAccelerationUnit_SelectedIndexChanged);
            // 
            // lblAngularAcceleration
            // 
            this.lblAngularAcceleration.AutoSize = true;
            this.lblAngularAcceleration.Location = new System.Drawing.Point(21, 172);
            this.lblAngularAcceleration.Name = "lblAngularAcceleration";
            this.lblAngularAcceleration.Size = new System.Drawing.Size(110, 13);
            this.lblAngularAcceleration.TabIndex = 40;
            this.lblAngularAcceleration.Text = "Angular acceleration :";
            // 
            // cmbAngularVelocityUnit
            // 
            this.cmbAngularVelocityUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAngularVelocityUnit.Location = new System.Drawing.Point(177, 138);
            this.cmbAngularVelocityUnit.Name = "cmbAngularVelocityUnit";
            this.cmbAngularVelocityUnit.Size = new System.Drawing.Size(221, 21);
            this.cmbAngularVelocityUnit.TabIndex = 39;
            this.cmbAngularVelocityUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngularVelocityUnit_SelectedIndexChanged);
            // 
            // lblAngularVelocityUnit
            // 
            this.lblAngularVelocityUnit.AutoSize = true;
            this.lblAngularVelocityUnit.Location = new System.Drawing.Point(21, 143);
            this.lblAngularVelocityUnit.Name = "lblAngularVelocityUnit";
            this.lblAngularVelocityUnit.Size = new System.Drawing.Size(81, 13);
            this.lblAngularVelocityUnit.TabIndex = 38;
            this.lblAngularVelocityUnit.Text = "Angular speed :";
            // 
            // cmbAngleUnit
            // 
            this.cmbAngleUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAngleUnit.Location = new System.Drawing.Point(177, 109);
            this.cmbAngleUnit.Name = "cmbAngleUnit";
            this.cmbAngleUnit.Size = new System.Drawing.Size(221, 21);
            this.cmbAngleUnit.TabIndex = 37;
            this.cmbAngleUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAngleUnit_SelectedIndexChanged);
            // 
            // lblAngleUnit
            // 
            this.lblAngleUnit.AutoSize = true;
            this.lblAngleUnit.Location = new System.Drawing.Point(21, 114);
            this.lblAngleUnit.Name = "lblAngleUnit";
            this.lblAngleUnit.Size = new System.Drawing.Size(40, 13);
            this.lblAngleUnit.TabIndex = 36;
            this.lblAngleUnit.Text = "Angle :";
            // 
            // cmbAccelerationUnit
            // 
            this.cmbAccelerationUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAccelerationUnit.Location = new System.Drawing.Point(177, 80);
            this.cmbAccelerationUnit.Name = "cmbAccelerationUnit";
            this.cmbAccelerationUnit.Size = new System.Drawing.Size(221, 21);
            this.cmbAccelerationUnit.TabIndex = 35;
            this.cmbAccelerationUnit.SelectedIndexChanged += new System.EventHandler(this.cmbAccelerationUnit_SelectedIndexChanged);
            // 
            // lblAccelerationUnit
            // 
            this.lblAccelerationUnit.AutoSize = true;
            this.lblAccelerationUnit.Location = new System.Drawing.Point(21, 85);
            this.lblAccelerationUnit.Name = "lblAccelerationUnit";
            this.lblAccelerationUnit.Size = new System.Drawing.Size(72, 13);
            this.lblAccelerationUnit.TabIndex = 34;
            this.lblAccelerationUnit.Text = "Acceleration :";
            // 
            // cmbSpeedUnit
            // 
            this.cmbSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpeedUnit.Location = new System.Drawing.Point(177, 51);
            this.cmbSpeedUnit.Name = "cmbSpeedUnit";
            this.cmbSpeedUnit.Size = new System.Drawing.Size(221, 21);
            this.cmbSpeedUnit.TabIndex = 33;
            this.cmbSpeedUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSpeedUnit_SelectedIndexChanged);
            // 
            // lblSpeedUnit
            // 
            this.lblSpeedUnit.AutoSize = true;
            this.lblSpeedUnit.Location = new System.Drawing.Point(21, 56);
            this.lblSpeedUnit.Name = "lblSpeedUnit";
            this.lblSpeedUnit.Size = new System.Drawing.Size(50, 13);
            this.lblSpeedUnit.TabIndex = 32;
            this.lblSpeedUnit.Text = "Velocity :";
            // 
            // cmbTimeCodeFormat
            // 
            this.cmbTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTimeCodeFormat.Location = new System.Drawing.Point(177, 22);
            this.cmbTimeCodeFormat.Name = "cmbTimeCodeFormat";
            this.cmbTimeCodeFormat.Size = new System.Drawing.Size(221, 21);
            this.cmbTimeCodeFormat.TabIndex = 28;
            this.cmbTimeCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeCodeFormat_SelectedIndexChanged);
            // 
            // lblTimeMarkersFormat
            // 
            this.lblTimeMarkersFormat.AutoSize = true;
            this.lblTimeMarkersFormat.Location = new System.Drawing.Point(21, 25);
            this.lblTimeMarkersFormat.Name = "lblTimeMarkersFormat";
            this.lblTimeMarkersFormat.Size = new System.Drawing.Size(108, 13);
            this.lblTimeMarkersFormat.TabIndex = 27;
            this.lblTimeMarkersFormat.Text = "Time markers format :";
            // 
            // tabMemory
            // 
            this.tabMemory.Controls.Add(this.grpSwitchToAnalysis);
            this.tabMemory.Location = new System.Drawing.Point(4, 22);
            this.tabMemory.Name = "tabMemory";
            this.tabMemory.Padding = new System.Windows.Forms.Padding(3);
            this.tabMemory.Size = new System.Drawing.Size(424, 210);
            this.tabMemory.TabIndex = 1;
            this.tabMemory.Text = "Memory";
            this.tabMemory.UseVisualStyleBackColor = true;
            // 
            // PreferencePanelPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.tabSubPages);
            this.Name = "PreferencePanelPlayer";
            this.Size = new System.Drawing.Size(432, 236);
            this.grpSwitchToAnalysis.ResumeLayout(false);
            this.grpSwitchToAnalysis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).EndInit();
            this.tabSubPages.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabUnits.ResumeLayout(false);
            this.tabUnits.PerformLayout();
            this.tabMemory.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Label lblImageFormat;
        private System.Windows.Forms.ComboBox cmbImageFormats;
        private System.Windows.Forms.CheckBox chkLockSpeeds;
        private System.Windows.Forms.TabPage tabMemory;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabControl tabSubPages;
        private System.Windows.Forms.Label lblWorkingZoneSeconds;
        private System.Windows.Forms.TrackBar trkWorkingZoneSeconds;
        private System.Windows.Forms.Label lblWorkingZoneLogic;
        private System.Windows.Forms.Label lblWorkingZoneMemory;
        private System.Windows.Forms.TrackBar trkWorkingZoneMemory;
        private System.Windows.Forms.GroupBox grpSwitchToAnalysis;
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
    }
}
