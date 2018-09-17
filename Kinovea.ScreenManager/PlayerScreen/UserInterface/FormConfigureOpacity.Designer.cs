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
namespace Kinovea.ScreenManager
{
	partial class formConfigureOpacity
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
			this.grpConfig = new System.Windows.Forms.GroupBox();
			this.lblValue = new System.Windows.Forms.Label();
			this.trkValue = new System.Windows.Forms.TrackBar();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.grpConfig.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkValue)).BeginInit();
			this.SuspendLayout();
			// 
			// grpConfig
			// 
			this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.grpConfig.Controls.Add(this.lblValue);
			this.grpConfig.Controls.Add(this.trkValue);
			this.grpConfig.Location = new System.Drawing.Point(11, 11);
			this.grpConfig.Name = "grpConfig";
			this.grpConfig.Size = new System.Drawing.Size(208, 85);
			this.grpConfig.TabIndex = 32;
			this.grpConfig.TabStop = false;
			this.grpConfig.Text = "Configuration";
			// 
			// lblValue
			// 
			this.lblValue.Location = new System.Drawing.Point(18, 16);
			this.lblValue.Name = "lblValue";
			this.lblValue.Size = new System.Drawing.Size(174, 13);
			this.lblValue.TabIndex = 4;
			this.lblValue.Text = "100%";
			this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// trkValue
			// 
			this.trkValue.Location = new System.Drawing.Point(6, 32);
			this.trkValue.Maximum = 100;
			this.trkValue.Minimum = 1;
			this.trkValue.Name = "trkValue";
			this.trkValue.Size = new System.Drawing.Size(196, 45);
			this.trkValue.TabIndex = 15;
			this.trkValue.TickFrequency = 4;
			this.trkValue.Value = 100;
			this.trkValue.ValueChanged += new System.EventHandler(this.trkValue_ValueChanged);
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(13, 111);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(99, 24);
			this.btnOK.TabIndex = 31;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(118, 111);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 24);
			this.btnCancel.TabIndex = 33;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// formConfigureOpacity
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(230, 146);
			this.Controls.Add(this.grpConfig);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formConfigureOpacity";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "  Configure Opacity";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formConfigureOpacity_FormClosing);
			this.grpConfig.ResumeLayout(false);
			this.grpConfig.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trkValue)).EndInit();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TrackBar trkValue;
		private System.Windows.Forms.Label lblValue;
		private System.Windows.Forms.GroupBox grpConfig;
	}
}