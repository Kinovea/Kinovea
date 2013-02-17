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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the main implementation of ICaptureScreenView.
    /// This implementation is used in the actual capture screens.
    /// </summary>
    public partial class CaptureScreenView : UserControl, ICaptureScreenView
    {
        #region Members
        private SliderLogScale slider = new SliderLogScale();
        private CaptureScreen presenter;
        #endregion
        
        public CaptureScreenView(CaptureScreen presenter)
        {
            InitializeComponent();
            this.presenter = presenter;
            
            // TEST
            sldrDelay.Minimum = 0;
            sldrDelay.Maximum = 3600;
            sldrDelay.Value = 0;
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
            lblCameraInfo.Text = title;
        }
        
        public void UpdateGrabbingStatus(bool grabbing)
        {
            if(grabbing)
                btnGrab.Image = Properties.Capture.grab_pause;
            else
                btnGrab.Image = Properties.Capture.grab_start;
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
            //throw new NotImplementedException();
            lblDelay.Text = sldrDelay.Value.ToString("0.00");
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
