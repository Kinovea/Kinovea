#region License
/*
Copyright © Joan Charmant 2010.
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
	partial class formDevicePicker
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
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lstOtherDevices = new System.Windows.Forms.ListBox();
			this.lblSelectAnother = new System.Windows.Forms.Label();
			this.lblCurrentlySelected = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnApply.Enabled = false;
			this.btnApply.Location = new System.Drawing.Point(105, 179);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(99, 24);
			this.btnApply.TabIndex = 76;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(210, 179);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(99, 24);
			this.btnCancel.TabIndex = 77;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.lstOtherDevices);
			this.groupBox1.Controls.Add(this.lblSelectAnother);
			this.groupBox1.Controls.Add(this.lblCurrentlySelected);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(297, 161);
			this.groupBox1.TabIndex = 78;
			this.groupBox1.TabStop = false;
			// 
			// lstOtherDevices
			// 
			this.lstOtherDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.lstOtherDevices.FormattingEnabled = true;
			this.lstOtherDevices.Location = new System.Drawing.Point(20, 86);
			this.lstOtherDevices.Name = "lstOtherDevices";
			this.lstOtherDevices.Size = new System.Drawing.Size(259, 56);
			this.lstOtherDevices.TabIndex = 2;
			this.lstOtherDevices.SelectedIndexChanged += new System.EventHandler(this.lstOtherDevices_SelectedIndexChanged);
			// 
			// lblSelectAnother
			// 
			this.lblSelectAnother.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.lblSelectAnother.BackColor = System.Drawing.Color.WhiteSmoke;
			this.lblSelectAnother.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSelectAnother.ForeColor = System.Drawing.Color.SteelBlue;
			this.lblSelectAnother.Location = new System.Drawing.Point(20, 60);
			this.lblSelectAnother.Name = "lblSelectAnother";
			this.lblSelectAnother.Size = new System.Drawing.Size(259, 23);
			this.lblSelectAnother.TabIndex = 1;
			this.lblSelectAnother.Text = "Select another device :";
			this.lblSelectAnother.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblCurrentlySelected
			// 
			this.lblCurrentlySelected.Location = new System.Drawing.Point(20, 23);
			this.lblCurrentlySelected.Name = "lblCurrentlySelected";
			this.lblCurrentlySelected.Size = new System.Drawing.Size(259, 20);
			this.lblCurrentlySelected.TabIndex = 0;
			this.lblCurrentlySelected.Text = "Currently selected : ...";
			// 
			// formDevicePicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(321, 215);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
            this.MinimizeBox = false;
			this.Name = "formDevicePicker";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "FormDevicePicker";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label lblSelectAnother;
		private System.Windows.Forms.ListBox lstOtherDevices;
		private System.Windows.Forms.Label lblCurrentlySelected;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnApply;
	}
}
