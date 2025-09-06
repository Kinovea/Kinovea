using System.Windows.Forms;
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
    partial class ThumbnailCamera
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblAlias = new System.Windows.Forms.Label();
            this.picBox = new System.Windows.Forms.PictureBox();
            this.btnIcon = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            this.SuspendLayout();
            // 
            // lblAlias
            // 
            this.lblAlias.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAlias.AutoSize = true;
            this.lblAlias.BackColor = System.Drawing.Color.White;
            this.lblAlias.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAlias.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAlias.ForeColor = System.Drawing.Color.Black;
            this.lblAlias.Location = new System.Drawing.Point(27, 178);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(61, 12);
            this.lblAlias.TabIndex = 5;
            this.lblAlias.Text = "Camera Alias";
            this.lblAlias.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAlias.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_Click);
            this.lblAlias.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_DoubleClick);
            // 
            // picBox
            // 
            this.picBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                                    | System.Windows.Forms.AnchorStyles.Right)));
            this.picBox.BackColor = System.Drawing.Color.Transparent;
            this.picBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picBox.Location = new System.Drawing.Point(3, 1);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(234, 174);
            this.picBox.TabIndex = 4;
            this.picBox.TabStop = false;
            this.picBox.Paint += new System.Windows.Forms.PaintEventHandler(this.PicBox_Paint);
            this.picBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_Click);
            this.picBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_DoubleClick);
            // 
            // btnIcon
            // 
            this.btnIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnIcon.BackColor = System.Drawing.Color.Transparent;
            this.btnIcon.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.webcam2b_16;
            this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIcon.FlatAppearance.BorderSize = 0;
            this.btnIcon.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnIcon.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIcon.Location = new System.Drawing.Point(5, 176);
            this.btnIcon.Name = "btnIcon";
            this.btnIcon.Size = new System.Drawing.Size(16, 16);
            this.btnIcon.TabIndex = 37;
            this.btnIcon.UseVisualStyleBackColor = false;
            this.btnIcon.Click += new System.EventHandler(this.BtnIconClick);
            // 
            // ThumbnailCamera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.btnIcon);
            this.Controls.Add(this.lblAlias);
            this.Controls.Add(this.picBox);
            this.Name = "ThumbnailCamera";
            this.Size = new System.Drawing.Size(240, 195);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ThumbnailCamera_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_Click);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.AllControls_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.Button btnIcon;
        public System.Windows.Forms.PictureBox picBox;
        public System.Windows.Forms.Label lblAlias;
    }
}
