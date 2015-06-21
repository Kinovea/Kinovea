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
using System.Linq;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the main implementation of ICaptureScreenView.
    /// This implementation is used in the actual capture screens.
    /// </summary>
    public partial class CaptureScreenView : KinoveaControl, ICaptureScreenView
    {
        #region Properties
        public string CurrentImageFilename
        {
            get { return fnbImage.Filename; }
        }

        public string CurrentVideoFilename
        {
            get { return fnbVideo.Filename; }
        }
        #endregion

        #region Events
        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        #endregion

        #region Members
        private CaptureScreen presenter;
        private CapturedFilesView capturedFilesView;
        #endregion
        
        public CaptureScreenView(CaptureScreen presenter)
        {
            InitializeComponent();
            lblCameraTitle.Text = "";
            lblCameraInfo.Text = "";
            this.presenter = presenter;
            ToggleCapturedVideosPanel();
            sldrDelay.ValueChanged += SldrDelay_ValueChanged;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("CaptureScreen");
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
            capturedFilesView.RefreshUICulture();
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
        public void SetCapturedFilesView(CapturedFilesView capturedFilesView)
        {
            this.capturedFilesView = capturedFilesView;
            pnlCapturedVideos.Controls.Add(capturedFilesView);
            capturedFilesView.Dock = DockStyle.Fill;
        }
        public void SetToolbarView(Control toolbar)
        {
            pnlDrawingToolsBar.Controls.Add(toolbar);
        }
        
        public void UpdateTitle(string title, Bitmap icon)
        {
            btnIcon.BackgroundImage = icon;
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
        public void UpdateRecordingStatus(bool recording)
        {
            if(recording)
            {
                btnRecord.Image = Properties.Capture.record_stop;
                //toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_RecordStop);
            }
            else
            {
                btnRecord.Image = Properties.Capture.record_start;
                //toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_RecordStart);
            }
            
            btnSettings.Enabled = !recording;
            fnbImage.Enabled = !recording;
            fnbVideo.Enabled = !recording;
        }
        public void UpdateDelayLabel(double delaySeconds, int delayFrames)
        {
            double round = Math.Round(delaySeconds);
            
            if(round < 10)
                lblDelay.Text = string.Format("Delay: {0:0.00}s ({1})", delaySeconds, delayFrames);
            else
                lblDelay.Text = string.Format("Delay: {0}s ({1})", round, delayFrames);
            
            //lblDelay.Text = String.Format(ScreenManagerLang.lblDelay_Text, delay);
        }
        public void UpdateDelayMaxAge(double delay)
        {
            // If the delayer was not allocated, fake a number so that we have a slider stuck at the 0th image.
            sldrDelay.Maximum = delay == 0 ? 0.9 : delay;
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
        public void ShowThumbnails()
        {
            if(!pnlCapturedVideos.Visible)
                ToggleCapturedVideosPanel();
        }

        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                capturedFilesView.Dispose();
            }

            base.Dispose(disposing);
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
            presenter.View_Close();
        }
        private void SldrDelay_ValueChanged(object sender, EventArgs e)
        {
            presenter.View_DelayChanged(sldrDelay.Value);
        }
        private void BtnSettingsClick(object sender, EventArgs e)
        {
            presenter.View_Configure();
        }
        private void BtnGrabClick(object sender, EventArgs e)
        {
            presenter.View_ToggleGrabbing();
        }
        private void LblCameraInfoClick(object sender, EventArgs e)
        {
            presenter.View_Configure();
        }
        private void FNBImage_ImageClick(object sender, EventArgs e)
        {
            presenter.View_OpenInExplorer(PreferencesManager.CapturePreferences.ImageDirectory);
        }
        private void FNBVideo_ImageClick(object sender, EventArgs e)
        {
            presenter.View_OpenInExplorer(PreferencesManager.CapturePreferences.VideoDirectory);
        }
        private void FnbImage_FilenameChanged(object sender, EventArgs e)
        {
            presenter.View_ValidateFilename(fnbImage.Filename);
        }
        private void FnbVideo_FilenameChanged(object sender, EventArgs e)
        {
            presenter.View_ValidateFilename(fnbVideo.Filename);
        }
        private void BtnSnapshot_Click(object sender, EventArgs e)
        {
            presenter.View_SnapshotAsked(fnbImage.Filename);
        }
        private void BtnRecordClick(object sender, EventArgs e)
        {
            presenter.View_ToggleRecording(fnbVideo.Filename);
        }
        private void btnConfigureComposite_Click(object sender, EventArgs e)
        {
            presenter.View_ConfigureComposite();
        }
        #endregion
        
        #region Private methods (pure Form logic)
        private void ToggleCapturedVideosPanel()
        {
            pnlCapturedVideos.Visible = !pnlCapturedVideos.Visible;
            AfterCapturedVideosChange();
        }
        private void AfterCapturedVideosChange()
        {
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

        #region Commands
        protected override bool ExecuteCommand(int cmd)
        {
            if (fnbImage.Focused || fnbVideo.Focused)
                return false;

            if (capturedFilesView.Editing)
                return false;

            if (!presenter.Synched)
                return ExecuteScreenCommand(cmd);

            // If we are in dual capture mode, check if the command is handled by the common controls.
            HotkeyCommand command = Hotkeys.FirstOrDefault(h => h != null && h.CommandCode == cmd);
            if (command == null)
                return false;

            bool dualCaptureHandled = HotkeySettingsManager.IsHandler("DualCapture", command.KeyData);

            if (dualCaptureHandled && DualCommandReceived != null)
            {
                DualCommandReceived(this, new EventArgs<HotkeyCommand>(command));
                return true;
            }
            else
            {
                return ExecuteScreenCommand(cmd);
            }
        }

        private bool ExecuteScreenCommand(int cmd)
        {
            CaptureScreenCommands command = (CaptureScreenCommands)cmd;

            switch (command)
            {
                case CaptureScreenCommands.ToggleGrabbing:
                    presenter.View_ToggleGrabbing();
                    break;
                case CaptureScreenCommands.ToggleRecording:
                    presenter.View_ToggleRecording(fnbVideo.Filename);
                    break;
                case CaptureScreenCommands.TakeSnapshot:
                    presenter.View_SnapshotAsked(fnbImage.Filename);
                    break;
                case CaptureScreenCommands.ResetViewport:
                    presenter.View_DeselectTool();
                    break;
                case CaptureScreenCommands.IncreaseZoom:
                    // Not supported currently, will need to be a command at viewport level.
                    break;
                case CaptureScreenCommands.DecreaseZoom:
                    // Not supported currently, will need to be a command at viewport level.
                    break;
                case CaptureScreenCommands.ResetZoom:
                    // Not supported currently, will need to be a command at viewport level.
                    break;
                case CaptureScreenCommands.OpenConfiguration:
                    presenter.View_Configure();
                    break;
                case CaptureScreenCommands.IncreaseDelay:
                    sldrDelay.Value = sldrDelay.Value + 1;
                    sldrDelay.Invalidate();
                    break;
                case CaptureScreenCommands.DecreaseDelay:
                    sldrDelay.Value = sldrDelay.Value - 1;
                    sldrDelay.Invalidate();
                    break;
                case CaptureScreenCommands.Close:
                    presenter.View_Close();
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion

        
    }
}
