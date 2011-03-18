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
	partial class PreferencePanelGeneral
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
			this.cmbHistoryCount = new System.Windows.Forms.ComboBox();
			this.lblLanguage = new System.Windows.Forms.Label();
			this.lblHistoryCount = new System.Windows.Forms.Label();
			this.cmbLanguage = new System.Windows.Forms.ComboBox();
			this.cmbTimeCodeFormat = new System.Windows.Forms.ComboBox();
			this.lblTimeMarkersFormat = new System.Windows.Forms.Label();
			this.cmbSpeedUnit = new System.Windows.Forms.ComboBox();
			this.lblSpeedUnit = new System.Windows.Forms.Label();
			this.cmbImageFormats = new System.Windows.Forms.ComboBox();
			this.lblImageFormat = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cmbHistoryCount
			// 
			this.cmbHistoryCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbHistoryCount.FormattingEnabled = true;
			this.cmbHistoryCount.Items.AddRange(new object[] {
									"0",
									"1",
									"2",
									"3",
									"4",
									"5",
									"6",
									"7",
									"8",
									"9",
									"10"});
			this.cmbHistoryCount.Location = new System.Drawing.Point(369, 60);
			this.cmbHistoryCount.Name = "cmbHistoryCount";
			this.cmbHistoryCount.Size = new System.Drawing.Size(36, 21);
			this.cmbHistoryCount.TabIndex = 13;
			this.cmbHistoryCount.SelectedIndexChanged += new System.EventHandler(this.cmbHistoryCount_SelectedIndexChanged);
			// 
			// lblLanguage
			// 
			this.lblLanguage.AutoSize = true;
			this.lblLanguage.Location = new System.Drawing.Point(28, 24);
			this.lblLanguage.Name = "lblLanguage";
			this.lblLanguage.Size = new System.Drawing.Size(61, 13);
			this.lblLanguage.TabIndex = 12;
			this.lblLanguage.Text = "Language :";
			// 
			// lblHistoryCount
			// 
			this.lblHistoryCount.AutoSize = true;
			this.lblHistoryCount.Location = new System.Drawing.Point(28, 63);
			this.lblHistoryCount.Name = "lblHistoryCount";
			this.lblHistoryCount.Size = new System.Drawing.Size(160, 13);
			this.lblHistoryCount.TabIndex = 14;
			this.lblHistoryCount.Text = "Number of files in recent history :";
			// 
			// cmbLanguage
			// 
			this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbLanguage.FormattingEnabled = true;
			this.cmbLanguage.Items.AddRange(new object[] {
									"English",
									"Français"});
			this.cmbLanguage.Location = new System.Drawing.Point(301, 24);
			this.cmbLanguage.Name = "cmbLanguage";
			this.cmbLanguage.Size = new System.Drawing.Size(104, 21);
			this.cmbLanguage.TabIndex = 11;
			this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
			// 
			// cmbTimeCodeFormat
			// 
			this.cmbTimeCodeFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTimeCodeFormat.Location = new System.Drawing.Point(221, 98);
			this.cmbTimeCodeFormat.Name = "cmbTimeCodeFormat";
			this.cmbTimeCodeFormat.Size = new System.Drawing.Size(184, 21);
			this.cmbTimeCodeFormat.TabIndex = 17;
			this.cmbTimeCodeFormat.SelectedIndexChanged += new System.EventHandler(this.cmbTimeCodeFormat_SelectedIndexChanged);
			// 
			// lblTimeMarkersFormat
			// 
			this.lblTimeMarkersFormat.AutoSize = true;
			this.lblTimeMarkersFormat.Location = new System.Drawing.Point(28, 101);
			this.lblTimeMarkersFormat.Name = "lblTimeMarkersFormat";
			this.lblTimeMarkersFormat.Size = new System.Drawing.Size(108, 13);
			this.lblTimeMarkersFormat.TabIndex = 16;
			this.lblTimeMarkersFormat.Text = "Time markers format :";
			// 
			// cmbSpeedUnit
			// 
			this.cmbSpeedUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSpeedUnit.Location = new System.Drawing.Point(221, 178);
			this.cmbSpeedUnit.Name = "cmbSpeedUnit";
			this.cmbSpeedUnit.Size = new System.Drawing.Size(184, 21);
			this.cmbSpeedUnit.TabIndex = 29;
			this.cmbSpeedUnit.SelectedIndexChanged += new System.EventHandler(this.cmbSpeedUnit_SelectedIndexChanged);
			// 
			// lblSpeedUnit
			// 
			this.lblSpeedUnit.AutoSize = true;
			this.lblSpeedUnit.Location = new System.Drawing.Point(28, 183);
			this.lblSpeedUnit.Name = "lblSpeedUnit";
			this.lblSpeedUnit.Size = new System.Drawing.Size(123, 13);
			this.lblSpeedUnit.TabIndex = 28;
			this.lblSpeedUnit.Text = "Preferred unit for speed :";
			// 
			// cmbImageFormats
			// 
			this.cmbImageFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbImageFormats.Location = new System.Drawing.Point(221, 139);
			this.cmbImageFormats.Name = "cmbImageFormats";
			this.cmbImageFormats.Size = new System.Drawing.Size(183, 21);
			this.cmbImageFormats.TabIndex = 27;
			this.cmbImageFormats.SelectedIndexChanged += new System.EventHandler(this.cmbImageAspectRatio_SelectedIndexChanged);
			// 
			// lblImageFormat
			// 
			this.lblImageFormat.AutoSize = true;
			this.lblImageFormat.Location = new System.Drawing.Point(28, 143);
			this.lblImageFormat.Name = "lblImageFormat";
			this.lblImageFormat.Size = new System.Drawing.Size(110, 13);
			this.lblImageFormat.TabIndex = 26;
			this.lblImageFormat.Text = "Default image format :";
			// 
			// PreferencePanelGeneral
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Gainsboro;
			this.Controls.Add(this.cmbSpeedUnit);
			this.Controls.Add(this.lblSpeedUnit);
			this.Controls.Add(this.cmbImageFormats);
			this.Controls.Add(this.lblImageFormat);
			this.Controls.Add(this.cmbTimeCodeFormat);
			this.Controls.Add(this.lblTimeMarkersFormat);
			this.Controls.Add(this.cmbHistoryCount);
			this.Controls.Add(this.lblLanguage);
			this.Controls.Add(this.lblHistoryCount);
			this.Controls.Add(this.cmbLanguage);
			this.Name = "PreferencePanelGeneral";
			this.Size = new System.Drawing.Size(432, 236);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label lblImageFormat;
		private System.Windows.Forms.ComboBox cmbImageFormats;
		private System.Windows.Forms.Label lblSpeedUnit;
		private System.Windows.Forms.ComboBox cmbSpeedUnit;
		private System.Windows.Forms.Label lblTimeMarkersFormat;
		private System.Windows.Forms.ComboBox cmbTimeCodeFormat;
		private System.Windows.Forms.ComboBox cmbLanguage;
		private System.Windows.Forms.Label lblHistoryCount;
		private System.Windows.Forms.Label lblLanguage;
		private System.Windows.Forms.ComboBox cmbHistoryCount;
	}
}
