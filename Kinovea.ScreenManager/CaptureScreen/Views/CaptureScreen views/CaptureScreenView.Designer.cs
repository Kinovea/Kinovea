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
    partial class CaptureScreenView
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
      this.components = new System.ComponentModel.Container();
      this.pnlControls = new System.Windows.Forms.Panel();
      this.nudDelay = new System.Windows.Forms.NumericUpDown();
      this.pnlCaptureDock = new System.Windows.Forms.Panel();
      this.btnGrab = new System.Windows.Forms.Button();
      this.btnSettings = new System.Windows.Forms.Button();
      this.btnRecord = new System.Windows.Forms.Button();
      this.btnSnapshot = new System.Windows.Forms.Button();
      this.lblDelay = new System.Windows.Forms.Label();
      this.pnlCapturedVideos = new System.Windows.Forms.Panel();
      this.pnlTitle = new System.Windows.Forms.Panel();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnIcon = new System.Windows.Forms.Button();
      this.lblCameraTitle = new System.Windows.Forms.Label();
      this.pnlViewport = new System.Windows.Forms.Panel();
      this.pnlDrawingToolsBar = new System.Windows.Forms.Panel();
      this.btnFoldCapturedVideosPanel = new System.Windows.Forms.Button();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.infobarCapture = new Kinovea.ScreenManager.InfobarCapture();
      this.sldrDelay = new Kinovea.ScreenManager.SliderLinear();
      this.fnbVideo = new Kinovea.ScreenManager.FilenameBox();
      this.fnbImage = new Kinovea.ScreenManager.FilenameBox();
      this.pnlControls.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).BeginInit();
      this.pnlCaptureDock.SuspendLayout();
      this.pnlTitle.SuspendLayout();
      this.pnlDrawingToolsBar.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlControls
      // 
      this.pnlControls.BackColor = System.Drawing.Color.WhiteSmoke;
      this.pnlControls.Controls.Add(this.nudDelay);
      this.pnlControls.Controls.Add(this.sldrDelay);
      this.pnlControls.Controls.Add(this.fnbVideo);
      this.pnlControls.Controls.Add(this.fnbImage);
      this.pnlControls.Controls.Add(this.pnlCaptureDock);
      this.pnlControls.Controls.Add(this.lblDelay);
      this.pnlControls.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlControls.Location = new System.Drawing.Point(0, 446);
      this.pnlControls.MinimumSize = new System.Drawing.Size(175, 70);
      this.pnlControls.Name = "pnlControls";
      this.pnlControls.Size = new System.Drawing.Size(865, 92);
      this.pnlControls.TabIndex = 3;
      // 
      // nudDelay
      // 
      this.nudDelay.DecimalPlaces = 1;
      this.nudDelay.Location = new System.Drawing.Point(275, 16);
      this.nudDelay.Name = "nudDelay";
      this.nudDelay.Size = new System.Drawing.Size(52, 20);
      this.nudDelay.TabIndex = 48;
      this.nudDelay.ValueChanged += new System.EventHandler(this.NudDelay_ValueChanged);
      // 
      // pnlCaptureDock
      // 
      this.pnlCaptureDock.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.capturedock5;
      this.pnlCaptureDock.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.pnlCaptureDock.Controls.Add(this.btnGrab);
      this.pnlCaptureDock.Controls.Add(this.btnSettings);
      this.pnlCaptureDock.Controls.Add(this.btnRecord);
      this.pnlCaptureDock.Controls.Add(this.btnSnapshot);
      this.pnlCaptureDock.Location = new System.Drawing.Point(0, 6);
      this.pnlCaptureDock.Name = "pnlCaptureDock";
      this.pnlCaptureDock.Size = new System.Drawing.Size(209, 42);
      this.pnlCaptureDock.TabIndex = 40;
      // 
      // btnGrab
      // 
      this.btnGrab.BackColor = System.Drawing.Color.Transparent;
      this.btnGrab.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnGrab.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnGrab.FlatAppearance.BorderSize = 0;
      this.btnGrab.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnGrab.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnGrab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnGrab.Image = global::Kinovea.ScreenManager.Properties.Capture.grab_pause;
      this.btnGrab.Location = new System.Drawing.Point(33, 6);
      this.btnGrab.MinimumSize = new System.Drawing.Size(30, 25);
      this.btnGrab.Name = "btnGrab";
      this.btnGrab.Size = new System.Drawing.Size(30, 25);
      this.btnGrab.TabIndex = 0;
      this.btnGrab.UseVisualStyleBackColor = false;
      this.btnGrab.Click += new System.EventHandler(this.BtnGrabClick);
      // 
      // btnSettings
      // 
      this.btnSettings.BackColor = System.Drawing.Color.Transparent;
      this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSettings.FlatAppearance.BorderSize = 0;
      this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSettings.Image = global::Kinovea.ScreenManager.Properties.Capture.settings;
      this.btnSettings.Location = new System.Drawing.Point(5, 6);
      this.btnSettings.MinimumSize = new System.Drawing.Size(30, 25);
      this.btnSettings.Name = "btnSettings";
      this.btnSettings.Size = new System.Drawing.Size(30, 25);
      this.btnSettings.TabIndex = 39;
      this.btnSettings.UseVisualStyleBackColor = false;
      this.btnSettings.Click += new System.EventHandler(this.BtnSettingsClick);
      // 
      // btnRecord
      // 
      this.btnRecord.BackColor = System.Drawing.Color.Transparent;
      this.btnRecord.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnRecord.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnRecord.FlatAppearance.BorderSize = 0;
      this.btnRecord.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnRecord.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnRecord.Image = global::Kinovea.ScreenManager.Properties.Capture.record_start;
      this.btnRecord.Location = new System.Drawing.Point(155, 6);
      this.btnRecord.MinimumSize = new System.Drawing.Size(20, 25);
      this.btnRecord.Name = "btnRecord";
      this.btnRecord.Size = new System.Drawing.Size(30, 25);
      this.btnRecord.TabIndex = 24;
      this.btnRecord.UseVisualStyleBackColor = false;
      this.btnRecord.Click += new System.EventHandler(this.BtnRecordClick);
      // 
      // btnSnapshot
      // 
      this.btnSnapshot.BackColor = System.Drawing.Color.Transparent;
      this.btnSnapshot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.btnSnapshot.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnSnapshot.FlatAppearance.BorderSize = 0;
      this.btnSnapshot.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnSnapshot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnSnapshot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnSnapshot.Image = global::Kinovea.ScreenManager.Properties.Resources.camerasingle;
      this.btnSnapshot.Location = new System.Drawing.Point(125, 6);
      this.btnSnapshot.MinimumSize = new System.Drawing.Size(25, 25);
      this.btnSnapshot.Name = "btnSnapshot";
      this.btnSnapshot.Size = new System.Drawing.Size(30, 25);
      this.btnSnapshot.TabIndex = 30;
      this.btnSnapshot.Tag = "";
      this.btnSnapshot.UseVisualStyleBackColor = false;
      this.btnSnapshot.Click += new System.EventHandler(this.BtnSnapshot_Click);
      // 
      // lblDelay
      // 
      this.lblDelay.AutoSize = true;
      this.lblDelay.BackColor = System.Drawing.Color.Transparent;
      this.lblDelay.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblDelay.Location = new System.Drawing.Point(224, 20);
      this.lblDelay.Name = "lblDelay";
      this.lblDelay.Size = new System.Drawing.Size(45, 12);
      this.lblDelay.TabIndex = 2;
      this.lblDelay.Text = "Delay (s):";
      // 
      // pnlCapturedVideos
      // 
      this.pnlCapturedVideos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlCapturedVideos.AutoScroll = true;
      this.pnlCapturedVideos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
      this.pnlCapturedVideos.Location = new System.Drawing.Point(0, 345);
      this.pnlCapturedVideos.Name = "pnlCapturedVideos";
      this.pnlCapturedVideos.Size = new System.Drawing.Size(865, 100);
      this.pnlCapturedVideos.TabIndex = 4;
      // 
      // pnlTitle
      // 
      this.pnlTitle.BackColor = System.Drawing.Color.White;
      this.pnlTitle.Controls.Add(this.btnClose);
      this.pnlTitle.Controls.Add(this.infobarCapture);
      this.pnlTitle.Controls.Add(this.btnIcon);
      this.pnlTitle.Controls.Add(this.lblCameraTitle);
      this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlTitle.Location = new System.Drawing.Point(0, 0);
      this.pnlTitle.Name = "pnlTitle";
      this.pnlTitle.Size = new System.Drawing.Size(865, 24);
      this.pnlTitle.TabIndex = 5;
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.BackColor = System.Drawing.Color.Transparent;
      this.btnClose.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.closegrey;
      this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnClose.FlatAppearance.BorderSize = 0;
      this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClose.Location = new System.Drawing.Point(843, 2);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(20, 20);
      this.btnClose.TabIndex = 2;
      this.btnClose.UseVisualStyleBackColor = false;
      this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
      // 
      // btnIcon
      // 
      this.btnIcon.BackColor = System.Drawing.Color.Transparent;
      this.btnIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnIcon.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnIcon.FlatAppearance.BorderSize = 0;
      this.btnIcon.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnIcon.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnIcon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnIcon.Location = new System.Drawing.Point(5, 2);
      this.btnIcon.Name = "btnIcon";
      this.btnIcon.Size = new System.Drawing.Size(20, 20);
      this.btnIcon.TabIndex = 6;
      this.btnIcon.UseVisualStyleBackColor = false;
      this.btnIcon.Click += new System.EventHandler(this.LblCameraInfoClick);
      // 
      // lblCameraTitle
      // 
      this.lblCameraTitle.AutoSize = true;
      this.lblCameraTitle.Cursor = System.Windows.Forms.Cursors.Hand;
      this.lblCameraTitle.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblCameraTitle.Location = new System.Drawing.Point(30, 6);
      this.lblCameraTitle.Name = "lblCameraTitle";
      this.lblCameraTitle.Size = new System.Drawing.Size(79, 13);
      this.lblCameraTitle.TabIndex = 4;
      this.lblCameraTitle.Text = "Camera title";
      this.lblCameraTitle.Click += new System.EventHandler(this.LblCameraInfoClick);
      // 
      // pnlViewport
      // 
      this.pnlViewport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlViewport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(44)))));
      this.pnlViewport.Location = new System.Drawing.Point(0, 25);
      this.pnlViewport.MinimumSize = new System.Drawing.Size(345, 25);
      this.pnlViewport.Name = "pnlViewport";
      this.pnlViewport.Size = new System.Drawing.Size(865, 292);
      this.pnlViewport.TabIndex = 6;
      // 
      // pnlDrawingToolsBar
      // 
      this.pnlDrawingToolsBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlDrawingToolsBar.AutoScroll = true;
      this.pnlDrawingToolsBar.BackColor = System.Drawing.Color.White;
      this.pnlDrawingToolsBar.Controls.Add(this.btnFoldCapturedVideosPanel);
      this.pnlDrawingToolsBar.Location = new System.Drawing.Point(0, 317);
      this.pnlDrawingToolsBar.Name = "pnlDrawingToolsBar";
      this.pnlDrawingToolsBar.Size = new System.Drawing.Size(865, 28);
      this.pnlDrawingToolsBar.TabIndex = 5;
      this.pnlDrawingToolsBar.DoubleClick += new System.EventHandler(this.BtnCapturedVideosFold_Click);
      // 
      // btnFoldCapturedVideosPanel
      // 
      this.btnFoldCapturedVideosPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFoldCapturedVideosPanel.BackColor = System.Drawing.Color.Transparent;
      this.btnFoldCapturedVideosPanel.BackgroundImage = global::Kinovea.ScreenManager.Properties.Resources.dock16x16;
      this.btnFoldCapturedVideosPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.btnFoldCapturedVideosPanel.Cursor = System.Windows.Forms.Cursors.Hand;
      this.btnFoldCapturedVideosPanel.FlatAppearance.BorderSize = 0;
      this.btnFoldCapturedVideosPanel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.btnFoldCapturedVideosPanel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.btnFoldCapturedVideosPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnFoldCapturedVideosPanel.Location = new System.Drawing.Point(842, 2);
      this.btnFoldCapturedVideosPanel.Name = "btnFoldCapturedVideosPanel";
      this.btnFoldCapturedVideosPanel.Size = new System.Drawing.Size(20, 20);
      this.btnFoldCapturedVideosPanel.TabIndex = 17;
      this.btnFoldCapturedVideosPanel.UseVisualStyleBackColor = false;
      this.btnFoldCapturedVideosPanel.Click += new System.EventHandler(this.BtnCapturedVideosFold_Click);
      // 
      // infobarCapture
      // 
      this.infobarCapture.AutoSize = true;
      this.infobarCapture.BackColor = System.Drawing.Color.Transparent;
      this.infobarCapture.Location = new System.Drawing.Point(125, 2);
      this.infobarCapture.Name = "infobarCapture";
      this.infobarCapture.Size = new System.Drawing.Size(579, 22);
      this.infobarCapture.TabIndex = 0;
      this.infobarCapture.Visible = false;
      // 
      // sldrDelay
      // 
      this.sldrDelay.Cursor = System.Windows.Forms.Cursors.Hand;
      this.sldrDelay.Location = new System.Drawing.Point(343, 15);
      this.sldrDelay.Maximum = 100D;
      this.sldrDelay.Minimum = 0D;
      this.sldrDelay.Name = "sldrDelay";
      this.sldrDelay.Size = new System.Drawing.Size(189, 23);
      this.sldrDelay.Sticky = false;
      this.sldrDelay.StickyValue = 0D;
      this.sldrDelay.TabIndex = 43;
      this.sldrDelay.Text = "sliderLinear1";
      this.sldrDelay.Value = 0D;
      // 
      // fnbVideo
      // 
      this.fnbVideo.BackColor = System.Drawing.Color.Transparent;
      this.fnbVideo.Editable = true;
      this.fnbVideo.Filename = "";
      this.fnbVideo.Image = global::Kinovea.ScreenManager.Properties.Capture.folder_camera;
      this.fnbVideo.InfoText = "Video:";
      this.fnbVideo.Location = new System.Drawing.Point(296, 56);
      this.fnbVideo.Name = "fnbVideo";
      this.fnbVideo.Size = new System.Drawing.Size(250, 27);
      this.fnbVideo.TabIndex = 42;
      this.fnbVideo.ImageClick += new System.EventHandler(this.FNBVideo_ImageClick);
      this.fnbVideo.FilenameChanged += new System.EventHandler(this.FnbVideo_FilenameChanged);
      // 
      // fnbImage
      // 
      this.fnbImage.BackColor = System.Drawing.Color.Transparent;
      this.fnbImage.Editable = true;
      this.fnbImage.Filename = "";
      this.fnbImage.Image = global::Kinovea.ScreenManager.Properties.Capture.folder_image;
      this.fnbImage.InfoText = "Image:";
      this.fnbImage.Location = new System.Drawing.Point(12, 56);
      this.fnbImage.Name = "fnbImage";
      this.fnbImage.Size = new System.Drawing.Size(250, 27);
      this.fnbImage.TabIndex = 41;
      this.fnbImage.ImageClick += new System.EventHandler(this.FNBImage_ImageClick);
      this.fnbImage.FilenameChanged += new System.EventHandler(this.FnbImage_FilenameChanged);
      // 
      // CaptureScreenView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Gainsboro;
      this.Controls.Add(this.pnlDrawingToolsBar);
      this.Controls.Add(this.pnlViewport);
      this.Controls.Add(this.pnlTitle);
      this.Controls.Add(this.pnlCapturedVideos);
      this.Controls.Add(this.pnlControls);
      this.Name = "CaptureScreenView";
      this.Size = new System.Drawing.Size(865, 538);
      this.pnlControls.ResumeLayout(false);
      this.pnlControls.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.nudDelay)).EndInit();
      this.pnlCaptureDock.ResumeLayout(false);
      this.pnlTitle.ResumeLayout(false);
      this.pnlTitle.PerformLayout();
      this.pnlDrawingToolsBar.ResumeLayout(false);
      this.ResumeLayout(false);

        }
        //private Kinovea.ScreenManager.SliderLogScale sldrDelay;
        private Kinovea.ScreenManager.SliderLinear sldrDelay;
        private Kinovea.ScreenManager.FilenameBox fnbImage;
        private Kinovea.ScreenManager.FilenameBox fnbVideo;
        private System.Windows.Forms.Button btnFoldCapturedVideosPanel;
        private System.Windows.Forms.Panel pnlDrawingToolsBar;
        private System.Windows.Forms.Label lblCameraTitle;
        private System.Windows.Forms.Panel pnlViewport;
        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.Panel pnlCapturedVideos;
        private System.Windows.Forms.Label lblDelay;
        public System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSnapshot;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnGrab;
        private System.Windows.Forms.Panel pnlCaptureDock;
        private System.Windows.Forms.Button btnIcon;
        private System.Windows.Forms.ToolTip toolTips;
        private InfobarCapture infobarCapture;
        private System.Windows.Forms.NumericUpDown nudDelay;
    }
}
