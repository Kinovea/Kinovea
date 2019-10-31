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
    partial class FilenameBox
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
      this.btnDirectoryLocation = new System.Windows.Forms.Button();
      this.tbFilename = new System.Windows.Forms.TextBox();
      this.lblFilename = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnDirectoryLocation
      // 
      this.btnDirectoryLocation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnDirectoryLocation.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnDirectoryLocation.FlatAppearance.BorderSize = 0;
      this.btnDirectoryLocation.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnDirectoryLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnDirectoryLocation.Image = global::Kinovea.ScreenManager.Properties.Resources.image;
      this.btnDirectoryLocation.Location = new System.Drawing.Point(-1, -1);
      this.btnDirectoryLocation.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnDirectoryLocation.Name = "btnDirectoryLocation";
      this.btnDirectoryLocation.Size = new System.Drawing.Size(30, 25);
      this.btnDirectoryLocation.TabIndex = 39;
      this.btnDirectoryLocation.Tag = "";
      this.btnDirectoryLocation.UseVisualStyleBackColor = true;
      this.btnDirectoryLocation.Click += new System.EventHandler(this.BtnDirectoryLocationClick);
      // 
      // tbFilename
      // 
      this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbFilename.Location = new System.Drawing.Point(73, 3);
      this.tbFilename.Name = "tbFilename";
      this.tbFilename.Size = new System.Drawing.Size(177, 20);
      this.tbFilename.TabIndex = 38;
      this.tbFilename.Text = "computed";
      this.tbFilename.TextChanged += new System.EventHandler(this.TbFilenameTextChanged);
      // 
      // lblFilename
      // 
      this.lblFilename.AutoSize = true;
      this.lblFilename.BackColor = System.Drawing.Color.Transparent;
      this.lblFilename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.lblFilename.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblFilename.ForeColor = System.Drawing.Color.Black;
      this.lblFilename.Location = new System.Drawing.Point(30, 6);
      this.lblFilename.Margin = new System.Windows.Forms.Padding(0);
      this.lblFilename.Name = "lblFilename";
      this.lblFilename.Size = new System.Drawing.Size(34, 12);
      this.lblFilename.TabIndex = 37;
      this.lblFilename.Text = "Image:";
      // 
      // FilenameBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.btnDirectoryLocation);
      this.Controls.Add(this.tbFilename);
      this.Controls.Add(this.lblFilename);
      this.Name = "FilenameBox";
      this.Size = new System.Drawing.Size(271, 27);
      this.ResumeLayout(false);
      this.PerformLayout();

        }
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.TextBox tbFilename;
        private System.Windows.Forms.Button btnDirectoryLocation;
    }
}
