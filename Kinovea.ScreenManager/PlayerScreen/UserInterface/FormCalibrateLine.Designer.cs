#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
	partial class FormCalibrateLine
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
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.grpConfig = new System.Windows.Forms.GroupBox();
			this.tbMeasure = new System.Windows.Forms.TextBox();
			this.lblRealSize = new System.Windows.Forms.Label();
			this.cbUnit = new System.Windows.Forms.ComboBox();
			this.grpConfig.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(71, 115);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(99, 24);
			this.btnOK.TabIndex = 25;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(176, 115);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 24);
			this.btnCancel.TabIndex = 30;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// grpConfig
			// 
			this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.grpConfig.Controls.Add(this.cbUnit);
			this.grpConfig.Controls.Add(this.tbMeasure);
			this.grpConfig.Controls.Add(this.lblRealSize);
			this.grpConfig.Location = new System.Drawing.Point(12, 12);
			this.grpConfig.Name = "grpConfig";
			this.grpConfig.Size = new System.Drawing.Size(263, 95);
			this.grpConfig.TabIndex = 29;
			this.grpConfig.TabStop = false;
			this.grpConfig.Text = "Configuration";
			// 
			// tbMeasure
			// 
			this.tbMeasure.AcceptsReturn = true;
			this.tbMeasure.Location = new System.Drawing.Point(28, 57);
			this.tbMeasure.MaxLength = 10;
			this.tbMeasure.Name = "tbMeasure";
			this.tbMeasure.Size = new System.Drawing.Size(65, 20);
			this.tbMeasure.TabIndex = 24;
			this.tbMeasure.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
			// 
			// lblRealSize
			// 
			this.lblRealSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.lblRealSize.Location = new System.Drawing.Point(17, 22);
			this.lblRealSize.Name = "lblRealSize";
			this.lblRealSize.Size = new System.Drawing.Size(229, 20);
			this.lblRealSize.TabIndex = 21;
			this.lblRealSize.Text = "dlgConfigureMeasure_lblRealSize";
			this.lblRealSize.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// cbUnit
			// 
			this.cbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbUnit.FormattingEnabled = true;
			this.cbUnit.Location = new System.Drawing.Point(99, 56);
			this.cbUnit.Name = "cbUnit";
			this.cbUnit.Size = new System.Drawing.Size(125, 21);
			this.cbUnit.TabIndex = 25;
			// 
			// formConfigureMeasure
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(285, 147);
			this.Controls.Add(this.grpConfig);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formConfigureMeasure";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "dlgConfigureMeasure_Title";
			this.grpConfig.ResumeLayout(false);
			this.grpConfig.PerformLayout();
			this.ResumeLayout(false);
        }
		private System.Windows.Forms.TextBox tbMeasure;
		private System.Windows.Forms.Label lblRealSize;
		private System.Windows.Forms.ComboBox cbUnit;

        

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
	}
}
