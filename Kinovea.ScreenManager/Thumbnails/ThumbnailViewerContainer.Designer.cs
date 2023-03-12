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
      this.btnForward = new System.Windows.Forms.Button();
      this.btnBack = new System.Windows.Forms.Button();
      this.btnUp = new System.Windows.Forms.Button();
      this.lblAddress = new System.Windows.Forms.Label();
      this.viewerSelector = new Kinovea.ScreenManager.ViewerSelector();
      this.btnCloseFullscreen = new System.Windows.Forms.Button();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
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
      this.splitMain.Panel1.Controls.Add(this.btnForward);
      this.splitMain.Panel1.Controls.Add(this.btnBack);
      this.splitMain.Panel1.Controls.Add(this.btnUp);
      this.splitMain.Panel1.Controls.Add(this.lblAddress);
      this.splitMain.Panel1.Controls.Add(this.viewerSelector);
      this.splitMain.Panel1.Controls.Add(this.btnCloseFullscreen);
      this.splitMain.Panel1.Controls.Add(this.progressBar);
      this.splitMain.Size = new System.Drawing.Size(553, 387);
      this.splitMain.SplitterDistance = 30;
      this.splitMain.TabIndex = 0;
      // 
      // btnForward
      // 
      this.btnForward.BackColor = System.Drawing.Color.Transparent;
      this.btnForward.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnForward.FlatAppearance.BorderSize = 0;
      this.btnForward.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnForward.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnForward.Image = global::Kinovea.ScreenManager.Properties.Resources.navforward;
      this.btnForward.Location = new System.Drawing.Point(34, 2);
      this.btnForward.Name = "btnForward";
      this.btnForward.Size = new System.Drawing.Size(25, 25);
      this.btnForward.TabIndex = 30;
      this.btnForward.UseVisualStyleBackColor = false;
      this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
      // 
      // btnBack
      // 
      this.btnBack.BackColor = System.Drawing.Color.Transparent;
      this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnBack.FlatAppearance.BorderSize = 0;
      this.btnBack.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnBack.Image = global::Kinovea.ScreenManager.Properties.Resources.navback;
      this.btnBack.Location = new System.Drawing.Point(3, 2);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new System.Drawing.Size(25, 25);
      this.btnBack.TabIndex = 29;
      this.btnBack.UseVisualStyleBackColor = false;
      this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
      // 
      // btnUp
      // 
      this.btnUp.BackColor = System.Drawing.Color.Transparent;
      this.btnUp.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnUp.FlatAppearance.BorderSize = 0;
      this.btnUp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnUp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
      this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnUp.Image = global::Kinovea.ScreenManager.Properties.Resources.navup;
      this.btnUp.Location = new System.Drawing.Point(65, 2);
      this.btnUp.Name = "btnUp";
      this.btnUp.Size = new System.Drawing.Size(25, 25);
      this.btnUp.TabIndex = 28;
      this.btnUp.UseVisualStyleBackColor = false;
      this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
      // 
      // lblAddress
      // 
      this.lblAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblAddress.Location = new System.Drawing.Point(96, 9);
      this.lblAddress.Name = "lblAddress";
      this.lblAddress.Size = new System.Drawing.Size(171, 18);
      this.lblAddress.TabIndex = 27;
      this.lblAddress.Text = "url";
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
      this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.progressBar.ForeColor = System.Drawing.Color.LightSteelBlue;
      this.progressBar.Location = new System.Drawing.Point(273, 9);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(154, 15);
      this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBar.TabIndex = 24;
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
      ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
      this.splitMain.ResumeLayout(false);
      this.ResumeLayout(false);

        }
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Button btnCloseFullscreen;
        private ViewerSelector viewerSelector;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnBack;
    }
}
