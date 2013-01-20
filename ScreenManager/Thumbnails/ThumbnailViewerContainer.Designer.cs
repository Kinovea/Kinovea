#region License
/*
Copyright © Joan Charmant 2012.
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
    partial class ThumbnailViewerContainer
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
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnLarge = new System.Windows.Forms.Button();
            this.btnMedium = new System.Windows.Forms.Button();
            this.btnSmall = new System.Windows.Forms.Button();
            this.btnExtraLarge = new System.Windows.Forms.Button();
            this.btnExtraSmall = new System.Windows.Forms.Button();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.BackColor = System.Drawing.Color.White;
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.IsSplitterFixed = true;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.splitMain.Panel1.Controls.Add(this.progressBar);
            this.splitMain.Panel1.Controls.Add(this.btnLarge);
            this.splitMain.Panel1.Controls.Add(this.btnMedium);
            this.splitMain.Panel1.Controls.Add(this.btnSmall);
            this.splitMain.Panel1.Controls.Add(this.btnExtraLarge);
            this.splitMain.Panel1.Controls.Add(this.btnExtraSmall);
            this.splitMain.Size = new System.Drawing.Size(553, 387);
            this.splitMain.SplitterDistance = 30;
            this.splitMain.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.progressBar.Location = new System.Drawing.Point(273, 8);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(154, 15);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 24;
            // 
            // btnLarge
            // 
            this.btnLarge.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLarge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLarge.FlatAppearance.BorderSize = 0;
            this.btnLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLarge.Location = new System.Drawing.Point(87, 10);
            this.btnLarge.Name = "btnLarge";
            this.btnLarge.Size = new System.Drawing.Size(20, 15);
            this.btnLarge.TabIndex = 23;
            this.btnLarge.UseVisualStyleBackColor = false;
            // 
            // btnMedium
            // 
            this.btnMedium.BackColor = System.Drawing.Color.SteelBlue;
            this.btnMedium.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMedium.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMedium.FlatAppearance.BorderSize = 0;
            this.btnMedium.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnMedium.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnMedium.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMedium.Location = new System.Drawing.Point(65, 13);
            this.btnMedium.Name = "btnMedium";
            this.btnMedium.Size = new System.Drawing.Size(16, 12);
            this.btnMedium.TabIndex = 22;
            this.btnMedium.UseVisualStyleBackColor = false;
            // 
            // btnSmall
            // 
            this.btnSmall.BackColor = System.Drawing.Color.SteelBlue;
            this.btnSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSmall.FlatAppearance.BorderSize = 0;
            this.btnSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSmall.Location = new System.Drawing.Point(47, 16);
            this.btnSmall.Name = "btnSmall";
            this.btnSmall.Size = new System.Drawing.Size(12, 9);
            this.btnSmall.TabIndex = 21;
            this.btnSmall.UseVisualStyleBackColor = false;
            // 
            // btnExtraLarge
            // 
            this.btnExtraLarge.BackColor = System.Drawing.Color.SteelBlue;
            this.btnExtraLarge.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExtraLarge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExtraLarge.FlatAppearance.BorderSize = 0;
            this.btnExtraLarge.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraLarge.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraLarge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExtraLarge.Location = new System.Drawing.Point(113, 7);
            this.btnExtraLarge.Name = "btnExtraLarge";
            this.btnExtraLarge.Size = new System.Drawing.Size(24, 18);
            this.btnExtraLarge.TabIndex = 20;
            this.btnExtraLarge.UseVisualStyleBackColor = false;
            // 
            // btnExtraSmall
            // 
            this.btnExtraSmall.BackColor = System.Drawing.Color.SteelBlue;
            this.btnExtraSmall.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExtraSmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExtraSmall.FlatAppearance.BorderSize = 0;
            this.btnExtraSmall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraSmall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btnExtraSmall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExtraSmall.Location = new System.Drawing.Point(33, 19);
            this.btnExtraSmall.Name = "btnExtraSmall";
            this.btnExtraSmall.Size = new System.Drawing.Size(8, 6);
            this.btnExtraSmall.TabIndex = 19;
            this.btnExtraSmall.UseVisualStyleBackColor = false;
            // 
            // ThumbnailViewerContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitMain);
            this.Name = "ThumbnailViewerContainer";
            this.Size = new System.Drawing.Size(553, 387);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button btnExtraSmall;
        private System.Windows.Forms.Button btnExtraLarge;
        private System.Windows.Forms.Button btnSmall;
        private System.Windows.Forms.Button btnMedium;
        private System.Windows.Forms.Button btnLarge;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitMain;
    }
}
