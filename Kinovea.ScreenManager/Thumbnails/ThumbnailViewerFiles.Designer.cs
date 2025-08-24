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
    partial class ThumbnailViewerFiles
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
      this.pnlThumbs = new System.Windows.Forms.FlowLayoutPanel();
      this.SuspendLayout();
      // 
      // pnlThumbs
      // 
      this.pnlThumbs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlThumbs.AutoScroll = true;
      this.pnlThumbs.BackColor = System.Drawing.Color.White;
      this.pnlThumbs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlThumbs.ForeColor = System.Drawing.Color.RosyBrown;
      this.pnlThumbs.Location = new System.Drawing.Point(10, 10);
      this.pnlThumbs.Name = "pnlThumbs";
      this.pnlThumbs.Padding = new System.Windows.Forms.Padding(15, 0, 30, 0);
      this.pnlThumbs.Size = new System.Drawing.Size(580, 432);
      this.pnlThumbs.TabIndex = 0;
      this.pnlThumbs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlThumbnails_MouseDown);
      // 
      // ThumbnailViewerFiles
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.pnlThumbs);
      this.Name = "ThumbnailViewerFiles";
      this.Size = new System.Drawing.Size(600, 450);
      this.ResumeLayout(false);

        }

        private System.Windows.Forms.FlowLayoutPanel pnlThumbs;
    }
}
