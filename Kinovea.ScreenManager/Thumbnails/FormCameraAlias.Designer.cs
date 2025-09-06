#region License
/*
Copyright © Joan Charmant 2013.
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
    partial class FormCameraAlias
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
      this.lblAlias = new System.Windows.Forms.Label();
      this.lblIcon = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.tbAlias = new System.Windows.Forms.TextBox();
      this.btnIcon = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnReset = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblAlias
      // 
      this.lblAlias.AutoSize = true;
      this.lblAlias.Location = new System.Drawing.Point(18, 23);
      this.lblAlias.Name = "lblAlias";
      this.lblAlias.Size = new System.Drawing.Size(35, 13);
      this.lblAlias.TabIndex = 0;
      this.lblAlias.Text = "Alias :";
      // 
      // lblIcon
      // 
      this.lblIcon.AutoSize = true;
      this.lblIcon.Location = new System.Drawing.Point(18, 57);
      this.lblIcon.Name = "lblIcon";
      this.lblIcon.Size = new System.Drawing.Size(34, 13);
      this.lblIcon.TabIndex = 1;
      this.lblIcon.Text = "Icon :";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Location = new System.Drawing.Point(133, 113);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(99, 24);
      this.btnOK.TabIndex = 33;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(238, 113);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // tbAlias
      // 
      this.tbAlias.Location = new System.Drawing.Point(99, 20);
      this.tbAlias.Name = "tbAlias";
      this.tbAlias.Size = new System.Drawing.Size(185, 20);
      this.tbAlias.TabIndex = 35;
      this.tbAlias.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbAlias_KeyDown);
      // 
      // btnIcon
      // 
      this.btnIcon.BackColor = System.Drawing.Color.Transparent;
      this.btnIcon.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.webcam2b_16;
      this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnIcon.FlatAppearance.BorderSize = 0;
      this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnIcon.Location = new System.Drawing.Point(99, 55);
      this.btnIcon.Name = "btnIcon";
      this.btnIcon.Size = new System.Drawing.Size(16, 16);
      this.btnIcon.TabIndex = 36;
      this.btnIcon.UseVisualStyleBackColor = false;
      this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.btnReset);
      this.groupBox1.Controls.Add(this.tbAlias);
      this.groupBox1.Controls.Add(this.btnIcon);
      this.groupBox1.Controls.Add(this.lblAlias);
      this.groupBox1.Controls.Add(this.lblIcon);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(325, 89);
      this.groupBox1.TabIndex = 37;
      this.groupBox1.TabStop = false;
      // 
      // btnReset
      // 
      this.btnReset.BackColor = System.Drawing.Color.Transparent;
      this.btnReset.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.bin_empty;
      this.btnReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnReset.FlatAppearance.BorderSize = 0;
      this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnReset.Location = new System.Drawing.Point(290, 22);
      this.btnReset.Name = "btnReset";
      this.btnReset.Size = new System.Drawing.Size(16, 16);
      this.btnReset.TabIndex = 37;
      this.btnReset.UseVisualStyleBackColor = false;
      this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
      // 
      // FormCameraAlias
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(349, 149);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormCameraAlias";
      this.Text = "FormCameraAlias";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Label lblAlias;
    }
}
