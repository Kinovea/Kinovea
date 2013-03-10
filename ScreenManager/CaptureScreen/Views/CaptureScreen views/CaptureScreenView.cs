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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the main implementation of ICaptureScreenView.
    /// This implementation is used in the actual capture screens.
    /// </summary>
    public partial class CaptureScreenView : UserControl, ICaptureScreenView
    {
        #region Members
        private CaptureScreen presenter;
        #endregion
        
        public CaptureScreenView(CaptureScreen presenter)
        {
            InitializeComponent();
            this.presenter = presenter;
            ToggleCapturedVideosPanel();
            
            //sldrDelay.Maximum = 250;
            sldrDelay.ValueChanged += SldrDelay_ValueChanged;
        }

        #region Public methods
        
        #region ICaptureScreenView
        public void DisplayAsActiveScreen(bool active)
        {
        
        }
        
        public void FullScreen(bool fullScreen)
        {
        
        }
        
        public void RefreshUICulture()
        {
        
        }
        
        public bool OnKeyPress(Keys key)
        {
            return false;
        }
        
        public void AddImageDrawing(string filename, bool svg)
        {
        
        }
        
        public void AddImageDrawing(Bitmap bmp)
        {
        
        }
        
        public void BeforeClose()
        {
        
        }
        
        public void SetViewport(Viewport viewport)
        {
            pnlViewport.Controls.Add(viewport);
            viewport.Dock = DockStyle.Fill;
        }
        
        public void UpdateTitle(string title)
        {
            lblCameraTitle.Text = title;
            lblCameraInfo.Left = lblCameraTitle.Right + 5;
        }
        
        public void UpdateInfo(string info)
        {
            lblCameraInfo.Text = info;
        }
        
        public void UpdateGrabbingStatus(bool grabbing)
        {
            if(grabbing)
                btnGrab.Image = Properties.Capture.grab_pause;
            else
                btnGrab.Image = Properties.Capture.grab_start;
        }
        public void UpdateDelayLabel(double delaySeconds, int delayFrames)
        {
            double round = Math.Round(delaySeconds);
            
            if(round < 10)
                lblDelay.Text = string.Format("Delay: {0:0.00} ({1})", delaySeconds, delayFrames);
            else
                lblDelay.Text = string.Format("Delay: {0} ({1})", round, delayFrames);
            
            //lblDelay.Text = String.Format(ScreenManagerLang.lblDelay_Text, delay);
        }
        public void UpdateDelayMaxAge(double delay)
        {
            sldrDelay.Maximum = delay;
        }
        public void UpdateNextImageFilename(string filename, bool editable)
        {
            fnbImage.Filename = filename;
            fnbImage.Editable = editable;
        }
        public void UpdateNextVideoFilename(string filename, bool editable)
        {
            fnbVideo.Filename = filename;
            fnbVideo.Editable = editable;
        }
        public void Toast(string message, int duration)
        {
            
        }
        #endregion
        
        #endregion
        
        #region Event handlers
        private void BtnCapturedVideosFold_Click(object sender, EventArgs e)
        {
            ToggleCapturedVideosPanel();
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            presenter.ViewClose();
        }
        private void SldrDelay_ValueChanged(object sender, EventArgs e)
        {
            presenter.ViewDelayChanged(sldrDelay.Value);
        }
        private void BtnSettingsClick(object sender, EventArgs e)
        {
            presenter.ViewConfigure();
        }
        private void BtnGrabClick(object sender, EventArgs e)
        {
            presenter.ViewToggleGrabbing();
        }
        private void LblCameraInfoClick(object sender, EventArgs e)
        {
            presenter.ViewConfigure();
        }
        private void FNBImage_ImageClick(object sender, EventArgs e)
        {
            presenter.OpenInExplorer(PreferencesManager.CapturePreferences.ImageDirectory);
        }
        private void FNBVideo_ImageClick(object sender, EventArgs e)
        {
            presenter.OpenInExplorer(PreferencesManager.CapturePreferences.VideoDirectory);
        }
        private void BtnSnapshot_Click(object sender, EventArgs e)
        {
            presenter.ViewSnapshot(fnbImage.Filename);
        }
        #endregion
        
        #region Private methods (pure Form logic)
        private void ToggleCapturedVideosPanel()
        {
            pnlCapturedVideos.Visible = !pnlCapturedVideos.Visible;
            
            if(pnlCapturedVideos.Visible)
            {
                btnFoldCapturedVideosPanel.BackgroundImage = Properties.Capture.section_fold;
                pnlDrawingToolsBar.Top = pnlCapturedVideos.Top - pnlDrawingToolsBar.Height;
                pnlViewport.Height = pnlCapturedVideos.Top - pnlViewport.Top;
            }
            else
            {
                btnFoldCapturedVideosPanel.BackgroundImage = Properties.Capture.section_unfold;
                pnlDrawingToolsBar.Top = pnlControls.Top - pnlDrawingToolsBar.Height;
                pnlViewport.Height = pnlControls.Top - pnlViewport.Top;
            }
        }
        #endregion
        
        
    }
}
