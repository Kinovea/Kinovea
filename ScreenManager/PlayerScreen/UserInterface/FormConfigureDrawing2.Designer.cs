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
	partial class FormConfigureDrawing2
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
			this.lblSecondElement = new System.Windows.Forms.Label();
			this.btnSecondElement = new System.Windows.Forms.Button();
			this.lblFirstElement = new System.Windows.Forms.Label();
			this.btnFirstElement = new System.Windows.Forms.Button();
			this.grpConfig.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(44, 120);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(99, 24);
			this.btnOK.TabIndex = 31;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(149, 120);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 24);
			this.btnCancel.TabIndex = 32;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// grpConfig
			// 
			this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.grpConfig.Controls.Add(this.lblSecondElement);
			this.grpConfig.Controls.Add(this.btnSecondElement);
			this.grpConfig.Controls.Add(this.lblFirstElement);
			this.grpConfig.Controls.Add(this.btnFirstElement);
			this.grpConfig.Location = new System.Drawing.Point(12, 12);
			this.grpConfig.Name = "grpConfig";
			this.grpConfig.Size = new System.Drawing.Size(236, 97);
			this.grpConfig.TabIndex = 33;
			this.grpConfig.TabStop = false;
			this.grpConfig.Text = "Configuration";
			// 
			// lblSecondElement
			// 
			this.lblSecondElement.AutoSize = true;
			this.lblSecondElement.Location = new System.Drawing.Point(38, 62);
			this.lblSecondElement.Name = "lblSecondElement";
			this.lblSecondElement.Size = new System.Drawing.Size(91, 13);
			this.lblSecondElement.TabIndex = 3;
			this.lblSecondElement.Text = "Second Element :";
			// 
			// btnSecondElement
			// 
			this.btnSecondElement.BackColor = System.Drawing.Color.Black;
			this.btnSecondElement.FlatAppearance.BorderSize = 0;
			this.btnSecondElement.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSecondElement.Location = new System.Drawing.Point(11, 58);
			this.btnSecondElement.Name = "btnSecondElement";
			this.btnSecondElement.Size = new System.Drawing.Size(21, 20);
			this.btnSecondElement.TabIndex = 2;
			this.btnSecondElement.UseVisualStyleBackColor = false;
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
			// FormConfigureDrawing2
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(260, 156);
			this.Controls.Add(this.grpConfig);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormConfigureDrawing2";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FormConfigureDrawing2";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
			this.grpConfig.ResumeLayout(false);
			this.grpConfig.PerformLayout();
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblSecondElement;
		private System.Windows.Forms.Button btnFirstElement;
		private System.Windows.Forms.Label lblFirstElement;
		private System.Windows.Forms.Button btnSecondElement;
		private System.Windows.Forms.GroupBox grpConfig;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
	}
}
