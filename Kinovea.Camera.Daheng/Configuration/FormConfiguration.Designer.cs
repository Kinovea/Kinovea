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
namespace Kinovea.Camera.Daheng
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
      this.gbProperties = new System.Windows.Forms.GroupBox();
      this.lblResultingFramerateValue = new System.Windows.Forms.Label();
      this.lblResultingFramerate = new System.Windows.Forms.Label();
      this.lblAuto = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tbAlias = new System.Windows.Forms.TextBox();
      this.lblSystemName = new System.Windows.Forms.Label();
      this.btnIcon = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.btnReconnect = new System.Windows.Forms.Button();
      this.gbProperties.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbProperties
      // 
      this.gbProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gbProperties.Controls.Add(this.lblResultingFramerateValue);
      this.gbProperties.Controls.Add(this.lblResultingFramerate);
      this.gbProperties.Controls.Add(this.lblAuto);
      this.gbProperties.Location = new System.Drawing.Point(12, 94);
      this.gbProperties.Name = "gbProperties";
      this.gbProperties.Size = new System.Drawing.Size(434, 259);
      this.gbProperties.TabIndex = 80;
      this.gbProperties.TabStop = false;
      // 
      // lblResultingFramerateValue
      // 
      this.lblResultingFramerateValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblResultingFramerateValue.AutoSize = true;
      this.lblResultingFramerateValue.BackColor = System.Drawing.Color.Transparent;
      this.lblResultingFramerateValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblResultingFramerateValue.ForeColor = System.Drawing.Color.Black;
      this.lblResultingFramerateValue.Location = new System.Drawing.Point(154, 221);
      this.lblResultingFramerateValue.Name = "lblResultingFramerateValue";
      this.lblResultingFramerateValue.Size = new System.Drawing.Size(13, 13);
      this.lblResultingFramerateValue.TabIndex = 105;
      this.lblResultingFramerateValue.Text = "0";
      this.lblResultingFramerateValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblResultingFramerate
      // 
      this.lblResultingFramerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblResultingFramerate.AutoSize = true;
      this.lblResultingFramerate.BackColor = System.Drawing.Color.Transparent;
      this.lblResultingFramerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblResultingFramerate.ForeColor = System.Drawing.Color.Black;
      this.lblResultingFramerate.Location = new System.Drawing.Point(21, 221);
      this.lblResultingFramerate.Name = "lblResultingFramerate";
      this.lblResultingFramerate.Size = new System.Drawing.Size(101, 13);
      this.lblResultingFramerate.TabIndex = 104;
      this.lblResultingFramerate.Text = "Resulting framerate:";
      this.lblResultingFramerate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblAuto
      // 
      this.lblAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblAuto.BackColor = System.Drawing.Color.Transparent;
      this.lblAuto.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblAuto.ForeColor = System.Drawing.Color.Black;
      this.lblAuto.Location = new System.Drawing.Point(384, 19);
      this.lblAuto.Name = "lblAuto";
      this.lblAuto.Size = new System.Drawing.Size(47, 23);
      this.lblAuto.TabIndex = 101;
      this.lblAuto.Text = "Auto";
      this.lblAuto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.tbAlias);
      this.groupBox2.Controls.Add(this.lblSystemName);
      this.groupBox2.Controls.Add(this.btnIcon);
      this.groupBox2.Location = new System.Drawing.Point(12, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(435, 76);
      this.groupBox2.TabIndex = 50;
      this.groupBox2.TabStop = false;
      // 
      // tbAlias
      // 
      this.tbAlias.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tbAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbAlias.ForeColor = System.Drawing.Color.CornflowerBlue;
      this.tbAlias.Location = new System.Drawing.Point(73, 22);
      this.tbAlias.Name = "tbAlias";
      this.tbAlias.Size = new System.Drawing.Size(342, 15);
      this.tbAlias.TabIndex = 52;
      this.tbAlias.Text = "Custom alias";
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
      this.lblSystemName.TabIndex = 53;
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
      this.btnIcon.TabIndex = 51;
      this.btnIcon.UseVisualStyleBackColor = false;
      this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnApply.Location = new System.Drawing.Point(349, 359);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(99, 24);
      this.btnApply.TabIndex = 200;
      this.btnApply.Text = "Apply";
      this.btnApply.UseVisualStyleBackColor = true;
      // 
      // btnReconnect
      // 
      this.btnReconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnReconnect.Location = new System.Drawing.Point(244, 359);
      this.btnReconnect.Name = "btnReconnect";
      this.btnReconnect.Size = new System.Drawing.Size(99, 24);
      this.btnReconnect.TabIndex = 203;
      this.btnReconnect.Text = "Reconnect";
      this.btnReconnect.UseVisualStyleBackColor = true;
      this.btnReconnect.Click += new System.EventHandler(this.BtnReconnect_Click);
      // 
      // FormConfiguration
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(458, 395);
      this.Controls.Add(this.btnReconnect);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.gbProperties);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormConfiguration";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Camera configuration";
      this.gbProperties.ResumeLayout(false);
      this.gbProperties.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.Label lblSystemName;
        private System.Windows.Forms.TextBox tbAlias;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox gbProperties;
        private System.Windows.Forms.Label lblAuto;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReconnect;
        private System.Windows.Forms.Label lblResultingFramerateValue;
        private System.Windows.Forms.Label lblResultingFramerate;
    }
}
