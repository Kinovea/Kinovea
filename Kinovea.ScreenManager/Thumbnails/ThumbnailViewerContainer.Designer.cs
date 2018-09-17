#region License
/*
Copyright © Joan Charmant 2012.
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
            this.btnCloseFullscreen = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.viewerSelector = new Kinovea.ScreenManager.ViewerSelector();
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
            this.splitMain.Panel1.Controls.Add(this.viewerSelector);
            this.splitMain.Panel1.Controls.Add(this.btnCloseFullscreen);
            this.splitMain.Panel1.Controls.Add(this.progressBar);
            this.splitMain.Size = new System.Drawing.Size(553, 387);
            this.splitMain.SplitterDistance = 30;
            this.splitMain.TabIndex = 0;
            // 
            // btnCloseFullscreen
            // 
            this.btnCloseFullscreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseFullscreen.BackColor = System.Drawing.Color.Transparent;
            this.btnCloseFullscreen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCloseFullscreen.FlatAppearance.BorderSize = 0;
            this.btnCloseFullscreen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCloseFullscreen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCloseFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseFullscreen.Image = global::Kinovea.ScreenManager.Properties.Resources.arrow_inout;
            this.btnCloseFullscreen.Location = new System.Drawing.Point(524, 2);
            this.btnCloseFullscreen.Name = "btnCloseFullscreen";
            this.btnCloseFullscreen.Size = new System.Drawing.Size(25, 25);
            this.btnCloseFullscreen.TabIndex = 25;
            this.btnCloseFullscreen.UseVisualStyleBackColor = false;
            this.btnCloseFullscreen.Click += new System.EventHandler(this.btnCloseFullscreen_Click);
            // 
            // progressBar
            // 
            this.progressBar.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.progressBar.Location = new System.Drawing.Point(197, 7);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(154, 15);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 24;
            // 
            // viewerSelector
            // 
            this.viewerSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.viewerSelector.BackColor = System.Drawing.Color.WhiteSmoke;
            this.viewerSelector.Location = new System.Drawing.Point(433, 6);
            this.viewerSelector.Name = "viewerSelector";
            this.viewerSelector.Size = new System.Drawing.Size(84, 18);
            this.viewerSelector.TabIndex = 26;
            this.viewerSelector.SelectionChanged += new System.EventHandler(this.Selector_SelectionChanged);
            // 
            // ThumbnailViewerContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitMain);
            this.Name = "ThumbnailViewerContainer";
            this.Size = new System.Drawing.Size(553, 387);
            this.Load += new System.EventHandler(this.ThumbnailViewerContainer_Load);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Button btnCloseFullscreen;
        private ViewerSelector viewerSelector;
    }
}
