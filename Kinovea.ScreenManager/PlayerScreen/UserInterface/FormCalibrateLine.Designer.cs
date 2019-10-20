#region License
/*
Copyright © Joan Charmant 2008-2009.
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
      this.nudMeasure = new System.Windows.Forms.NumericUpDown();
      this.cbUnit = new System.Windows.Forms.ComboBox();
      this.lblRealSize = new System.Windows.Forms.Label();
      this.grpConfig.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudMeasure)).BeginInit();
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
      this.grpConfig.Controls.Add(this.nudMeasure);
      this.grpConfig.Controls.Add(this.cbUnit);
      this.grpConfig.Controls.Add(this.lblRealSize);
      this.grpConfig.Location = new System.Drawing.Point(12, 12);
      this.grpConfig.Name = "grpConfig";
      this.grpConfig.Size = new System.Drawing.Size(263, 95);
      this.grpConfig.TabIndex = 29;
      this.grpConfig.TabStop = false;
      this.grpConfig.Text = "Configuration";
      // 
      // nudMeasure
      // 
      this.nudMeasure.DecimalPlaces = 2;
      this.nudMeasure.Location = new System.Drawing.Point(20, 56);
      this.nudMeasure.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.nudMeasure.Name = "nudMeasure";
      this.nudMeasure.Size = new System.Drawing.Size(56, 20);
      this.nudMeasure.TabIndex = 26;
      this.nudMeasure.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      // 
      // cbUnit
      // 
      this.cbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbUnit.FormattingEnabled = true;
      this.cbUnit.Location = new System.Drawing.Point(82, 55);
      this.cbUnit.Name = "cbUnit";
      this.cbUnit.Size = new System.Drawing.Size(150, 21);
      this.cbUnit.TabIndex = 25;
      // 
      // lblRealSize
      // 
      this.lblRealSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblRealSize.Location = new System.Drawing.Point(20, 22);
      this.lblRealSize.Name = "lblRealSize";
      this.lblRealSize.Size = new System.Drawing.Size(226, 20);
      this.lblRealSize.TabIndex = 21;
      this.lblRealSize.Text = "dlgConfigureMeasure_lblRealSize";
      this.lblRealSize.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // FormCalibrateLine
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
      this.Name = "FormCalibrateLine";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "dlgConfigureMeasure_Title";
      this.grpConfig.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.nudMeasure)).EndInit();
      this.ResumeLayout(false);

        }
		private System.Windows.Forms.Label lblRealSize;
		private System.Windows.Forms.ComboBox cbUnit;

        

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpConfig;
        private NumericUpDown nudMeasure;
    }
}
