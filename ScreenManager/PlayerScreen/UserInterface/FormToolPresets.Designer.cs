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
namespace Kinovea.ScreenManager
{
	partial class FormToolPresets
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
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
			this.components = new System.ComponentModel.Container();
			this.btnDefault = new System.Windows.Forms.Button();
			this.btnSaveProfile = new System.Windows.Forms.Button();
			this.btnLoadProfile = new System.Windows.Forms.Button();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lstPresets = new System.Windows.Forms.ListBox();
			this.btnToolIcon = new System.Windows.Forms.Button();
			this.lblToolName = new System.Windows.Forms.Label();
			this.grpConfig = new System.Windows.Forms.GroupBox();
			this.lblFirstElement = new System.Windows.Forms.Label();
			this.btnFirstElement = new System.Windows.Forms.Button();
			this.toolTips = new System.Windows.Forms.ToolTip(this.components);
			this.grpConfig.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnDefault
			// 
			this.btnDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDefault.FlatAppearance.BorderSize = 0;
			this.btnDefault.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
			this.btnDefault.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.btnDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDefault.Image = global::Kinovea.ScreenManager.Properties.Resources.bin_empty;
			this.btnDefault.Location = new System.Drawing.Point(265, 12);
			this.btnDefault.Name = "btnDefault";
			this.btnDefault.Size = new System.Drawing.Size(25, 25);
			this.btnDefault.TabIndex = 18;
			this.btnDefault.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnDefault.UseVisualStyleBackColor = true;
			this.btnDefault.Click += new System.EventHandler(this.BtnDefaultClick);
			// 
			// btnSaveProfile
			// 
			this.btnSaveProfile.FlatAppearance.BorderSize = 0;
			this.btnSaveProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
			this.btnSaveProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.btnSaveProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSaveProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.filesave;
			this.btnSaveProfile.Location = new System.Drawing.Point(45, 12);
			this.btnSaveProfile.Name = "btnSaveProfile";
			this.btnSaveProfile.Size = new System.Drawing.Size(25, 25);
			this.btnSaveProfile.TabIndex = 17;
			this.btnSaveProfile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnSaveProfile.UseVisualStyleBackColor = true;
			this.btnSaveProfile.Click += new System.EventHandler(this.BtnSaveProfileClick);
			// 
			// btnLoadProfile
			// 
			this.btnLoadProfile.FlatAppearance.BorderSize = 0;
			this.btnLoadProfile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
			this.btnLoadProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.btnLoadProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLoadProfile.Image = global::Kinovea.ScreenManager.Properties.Resources.folder_new;
			this.btnLoadProfile.Location = new System.Drawing.Point(14, 12);
			this.btnLoadProfile.Name = "btnLoadProfile";
			this.btnLoadProfile.Size = new System.Drawing.Size(25, 25);
			this.btnLoadProfile.TabIndex = 16;
			this.btnLoadProfile.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.btnLoadProfile.UseVisualStyleBackColor = true;
			this.btnLoadProfile.Click += new System.EventHandler(this.BtnLoadProfileClick);
			// 
			// btnApply
			// 
			this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnApply.Location = new System.Drawing.Point(87, 204);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(99, 24);
			this.btnApply.TabIndex = 76;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(192, 204);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 24);
			this.btnCancel.TabIndex = 77;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// lstPresets
			// 
			this.lstPresets.FormattingEnabled = true;
			this.lstPresets.Location = new System.Drawing.Point(12, 48);
			this.lstPresets.Name = "lstPresets";
			this.lstPresets.Size = new System.Drawing.Size(114, 147);
			this.lstPresets.TabIndex = 79;
			this.lstPresets.SelectedIndexChanged += new System.EventHandler(this.LstPresetsSelectedIndexChanged);
			// 
			// btnToolIcon
			// 
			this.btnToolIcon.BackColor = System.Drawing.Color.Black;
			this.btnToolIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.btnToolIcon.FlatAppearance.BorderSize = 0;
			this.btnToolIcon.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
			this.btnToolIcon.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
			this.btnToolIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnToolIcon.ForeColor = System.Drawing.Color.Black;
			this.btnToolIcon.Location = new System.Drawing.Point(140, 50);
			this.btnToolIcon.Name = "btnToolIcon";
			this.btnToolIcon.Size = new System.Drawing.Size(25, 25);
			this.btnToolIcon.TabIndex = 32;
			this.btnToolIcon.TabStop = false;
			this.btnToolIcon.UseVisualStyleBackColor = false;
			// 
			// lblToolName
			// 
			this.lblToolName.AutoSize = true;
			this.lblToolName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblToolName.Location = new System.Drawing.Point(175, 55);
			this.lblToolName.Name = "lblToolName";
			this.lblToolName.Size = new System.Drawing.Size(73, 16);
			this.lblToolName.TabIndex = 80;
			this.lblToolName.Text = "Tool name";
			// 
			// grpConfig
			// 
			this.grpConfig.Controls.Add(this.lblFirstElement);
			this.grpConfig.Controls.Add(this.btnFirstElement);
			this.grpConfig.Location = new System.Drawing.Point(132, 85);
			this.grpConfig.Name = "grpConfig";
			this.grpConfig.Size = new System.Drawing.Size(159, 110);
			this.grpConfig.TabIndex = 81;
			this.grpConfig.TabStop = false;
			// 
			// lblFirstElement
			// 
			this.lblFirstElement.AutoSize = true;
			this.lblFirstElement.Location = new System.Drawing.Point(38, 29);
			this.lblFirstElement.Name = "lblFirstElement";
			this.lblFirstElement.Size = new System.Drawing.Size(73, 13);
			this.lblFirstElement.TabIndex = 1;
			this.lblFirstElement.Text = "First Element :";
			// 
			// btnFirstElement
			// 
			this.btnFirstElement.BackColor = System.Drawing.Color.Black;
			this.btnFirstElement.FlatAppearance.BorderSize = 0;
			this.btnFirstElement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnFirstElement.Location = new System.Drawing.Point(11, 25);
			this.btnFirstElement.Name = "btnFirstElement";
			this.btnFirstElement.Size = new System.Drawing.Size(21, 20);
			this.btnFirstElement.TabIndex = 0;
			this.btnFirstElement.UseVisualStyleBackColor = false;
			// 
			// FormToolPresets
			// 
			this.AcceptButton = this.btnApply;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(302, 240);
			this.Controls.Add(this.grpConfig);
			this.Controls.Add(this.lblToolName);
			this.Controls.Add(this.btnToolIcon);
			this.Controls.Add(this.lstPresets);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnDefault);
			this.Controls.Add(this.btnSaveProfile);
			this.Controls.Add(this.btnLoadProfile);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FormToolPresets";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FormToolPresets";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
			this.grpConfig.ResumeLayout(false);
			this.grpConfig.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolTip toolTips;
		private System.Windows.Forms.GroupBox grpConfig;
		private System.Windows.Forms.Button btnFirstElement;
		private System.Windows.Forms.Label lblFirstElement;
		private System.Windows.Forms.Button btnDefault;
		private System.Windows.Forms.Label lblToolName;
		private System.Windows.Forms.Button btnToolIcon;
		private System.Windows.Forms.ListBox lstPresets;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnLoadProfile;
		private System.Windows.Forms.Button btnSaveProfile;
	}
}
