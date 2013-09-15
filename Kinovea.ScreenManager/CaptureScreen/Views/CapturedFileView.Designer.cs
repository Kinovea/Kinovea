#region License
/*
Copyright © Joan Charmant 2013.
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
    partial class CapturedFileView
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
            this.lblFilename = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFilename
            // 
            this.lblFilename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
            this.lblFilename.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilename.ForeColor = System.Drawing.Color.LightGray;
            this.lblFilename.Location = new System.Drawing.Point(0, 67);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(102, 13);
            this.lblFilename.TabIndex = 7;
            this.lblFilename.Text = "test.jpg";
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFilename.Click += new System.EventHandler(this.LblFilename_Click);
            this.lblFilename.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
            this.lblFilename.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
            // 
            // CapturedFileView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.lblFilename);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Name = "CapturedFileView";
            this.Size = new System.Drawing.Size(102, 80);
            this.Click += new System.EventHandler(this.CapturedFileView_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CapturedFileViewPaint);
            this.DoubleClick += new System.EventHandler(this.CapturedFileViewDoubleClick);
            this.MouseEnter += new System.EventHandler(this.Controls_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.Controls_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CapturedFileView_MouseMove);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label lblFilename;
    }
}
