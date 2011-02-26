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
			this.grpSwitchToAnalysis.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).BeginInit();
			this.SuspendLayout();
			// 
			// chkDeinterlace
			// 
			this.chkDeinterlace.Location = new System.Drawing.Point(24, 198);
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
			this.grpSwitchToAnalysis.Location = new System.Drawing.Point(24, 20);
			this.grpSwitchToAnalysis.Name = "grpSwitchToAnalysis";
			this.grpSwitchToAnalysis.Size = new System.Drawing.Size(405, 163);
			this.grpSwitchToAnalysis.TabIndex = 26;
			this.grpSwitchToAnalysis.TabStop = false;
			this.grpSwitchToAnalysis.Text = "Switch to Analysis Mode";
			// 
			// lblWorkingZoneLogic
			// 
			this.lblWorkingZoneLogic.AutoSize = true;
			this.lblWorkingZoneLogic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorkingZoneLogic.Location = new System.Drawing.Point(6, 72);
			this.lblWorkingZoneLogic.Name = "lblWorkingZoneLogic";
			this.lblWorkingZoneLogic.Size = new System.Drawing.Size(29, 13);
			this.lblWorkingZoneLogic.TabIndex = 37;
			this.lblWorkingZoneLogic.Text = "And";
			// 
			// trkWorkingZoneSeconds
			// 
			this.trkWorkingZoneSeconds.Location = new System.Drawing.Point(9, 40);
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
			this.lblWorkingZoneSeconds.Location = new System.Drawing.Point(14, 23);
			this.lblWorkingZoneSeconds.Name = "lblWorkingZoneSeconds";
			this.lblWorkingZoneSeconds.Size = new System.Drawing.Size(191, 13);
			this.lblWorkingZoneSeconds.TabIndex = 36;
			this.lblWorkingZoneSeconds.Text = "Working Zone is less than 12 seconds.";
			// 
			// trkWorkingZoneMemory
			// 
			this.trkWorkingZoneMemory.Location = new System.Drawing.Point(10, 110);
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
			this.lblWorkingZoneMemory.Location = new System.Drawing.Point(14, 92);
			this.lblWorkingZoneMemory.Name = "lblWorkingZoneMemory";
			this.lblWorkingZoneMemory.Size = new System.Drawing.Size(257, 13);
			this.lblWorkingZoneMemory.TabIndex = 17;
			this.lblWorkingZoneMemory.Text = "Working Zone will take less than 512 Mib of Memory.";
			// 
			// PreferencePanelPlayer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Gainsboro;
			this.Controls.Add(this.grpSwitchToAnalysis);
			this.Controls.Add(this.chkDeinterlace);
			this.Name = "PreferencePanelPlayer";
			this.Size = new System.Drawing.Size(432, 236);
			this.grpSwitchToAnalysis.ResumeLayout(false);
			this.grpSwitchToAnalysis.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneSeconds)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trkWorkingZoneMemory)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblWorkingZoneSeconds;
		private System.Windows.Forms.TrackBar trkWorkingZoneSeconds;
		private System.Windows.Forms.Label lblWorkingZoneLogic;
		private System.Windows.Forms.Label lblWorkingZoneMemory;
		private System.Windows.Forms.TrackBar trkWorkingZoneMemory;
		private System.Windows.Forms.GroupBox grpSwitchToAnalysis;
		private System.Windows.Forms.CheckBox chkDeinterlace;
	}
}
