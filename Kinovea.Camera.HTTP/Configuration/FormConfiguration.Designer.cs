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
namespace Kinovea.Camera.HTTP
{
    partial class FormConfiguration
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tbAlias = new System.Windows.Forms.TextBox();
      this.lblSystemName = new System.Windows.Forms.Label();
      this.btnIcon = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.gpParameters = new System.Windows.Forms.GroupBox();
      this.btnTest = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.groupBox1.Controls.Add(this.tbAlias);
      this.groupBox1.Controls.Add(this.lblSystemName);
      this.groupBox1.Controls.Add(this.btnIcon);
      this.groupBox1.Location = new System.Drawing.Point(12, 7);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(419, 76);
      this.groupBox1.TabIndex = 87;
      this.groupBox1.TabStop = false;
      // 
      // tbAlias
      // 
      this.tbAlias.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbAlias.ForeColor = System.Drawing.Color.CornflowerBlue;
      this.tbAlias.Location = new System.Drawing.Point(73, 22);
      this.tbAlias.Name = "tbAlias";
      this.tbAlias.Size = new System.Drawing.Size(223, 15);
      this.tbAlias.TabIndex = 52;
      this.tbAlias.Text = "Alias";
      // 
      // lblSystemName
      // 
      this.lblSystemName.AutoSize = true;
      this.lblSystemName.BackColor = System.Drawing.Color.Transparent;
      this.lblSystemName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblSystemName.ForeColor = System.Drawing.Color.Black;
      this.lblSystemName.Location = new System.Drawing.Point(68, 45);
      this.lblSystemName.Name = "lblSystemName";
      this.lblSystemName.Size = new System.Drawing.Size(70, 13);
      this.lblSystemName.TabIndex = 85;
      this.lblSystemName.Text = "System name";
      this.lblSystemName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnIcon
      // 
      this.btnIcon.BackColor = System.Drawing.Color.Transparent;
      this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnIcon.FlatAppearance.BorderSize = 0;
      this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnIcon.Location = new System.Drawing.Point(24, 26);
      this.btnIcon.Name = "btnIcon";
      this.btnIcon.Size = new System.Drawing.Size(16, 16);
      this.btnIcon.TabIndex = 50;
      this.btnIcon.UseVisualStyleBackColor = false;
      this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnApply.Location = new System.Drawing.Point(227, 457);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(99, 24);
      this.btnApply.TabIndex = 202;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(332, 457);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(99, 24);
      this.btnCancel.TabIndex = 204;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // gpParameters
      // 
      this.gpParameters.Location = new System.Drawing.Point(12, 89);
      this.gpParameters.Name = "gpParameters";
      this.gpParameters.Size = new System.Drawing.Size(419, 357);
      this.gpParameters.TabIndex = 88;
      this.gpParameters.TabStop = false;
      // 
      // btnTest
      // 
      this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTest.Location = new System.Drawing.Point(12, 457);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(99, 24);
      this.btnTest.TabIndex = 200;
      this.btnTest.Text = "Test";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new System.EventHandler(this.BtnTest_Click);
      // 
      // FormConfiguration
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(446, 493);
      this.Controls.Add(this.btnTest);
      this.Controls.Add(this.gpParameters);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfiguration";
      this.Text = "FormConfiguration";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.GroupBox gpParameters;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.Label lblSystemName;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
