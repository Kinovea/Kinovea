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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
        public CaptureFolder CaptureFolder
        {
            get 
            {
                if (cbCaptureFolder.SelectedItem == null)
                    return null;
                
                return (CaptureFolder)cbCaptureFolder.SelectedItem; 
            }
        }
        
        public string CurrentFilename
        {
            get 
            { 
                return tbFilename.Text;
            }
        }
        #endregion

        #region Events
        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        #endregion

        #region Members
        private CaptureScreen presenter;
        private CapturedFilesView capturedFilesView;
        private bool recording;
        private bool grabbing;
        private bool armed = true;
        private bool delayedDisplay = true;
        private bool delayUpdating;
        #endregion

        public CaptureScreenView(CaptureScreen presenter)
        {
            InitializeComponent();
            lblCameraTitle.Text = "";
            this.presenter = presenter;
            ToggleCapturedVideosPanel();
            sldrDelay.ValueChanged += SldrDelay_ValueChanged;
            
            nudDelay.Minimum = 0;
            nudDelay.Maximum = 100;
            NudHelper.FixNudScroll(nudDelay);

            nudDuration.Minimum = 0;
            nudDuration.Maximum = 300;
            NudHelper.FixNudScroll(nudDuration);

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
            ReloadTooltipsCulture();

            // Reload the capture folder selector.
            CaptureFolder memoCaptureFolder = this.CaptureFolder;

            // The selected capture folder may be null if it's the first time loading.
            // In this case we'll set it to the first value but we'll get the true value
            // from the window descriptor later in ForcePopulate().
            cbCaptureFolder.Items.Clear();
            List<CaptureFolder> ccff = PreferencesManager.CapturePreferences.CapturePathConfiguration.CaptureFolders;
            foreach (var cf in ccff)
            {
                cbCaptureFolder.Items.Add(cf);
                if (memoCaptureFolder != null && cf.Id == memoCaptureFolder.Id)
                    cbCaptureFolder.SelectedItem = cf;
            }

            if (cbCaptureFolder.SelectedIndex < 0 && cbCaptureFolder.Items.Count > 0)
            {
                cbCaptureFolder.SelectedIndex = 0;
            }
        }
        
        public void AddImageDrawing(string filename, bool svg)
        {
        }
        
        public void AddImageDrawing(Bitmap bmp)
        {
        }
        
        public void BeforeClose()
        {
            // Save the state of the screen to the window descriptor.
            // This is not the main screen descriptor that we save to the screen list
            // and reload in the context of "continue where you left off" or "load specfiic screens", 
            // these are considered "preferences" not "state".
            // We need to save the "state" of the screen so that if the user closes 
            // and reopens the screen during the same session we restore the state.
            // Or, in "continue where you left off" mode, if they close the screen, close the window,
            // and later reopen the window, and reopen a screen, present them with values they were using last time.
            WindowDescriptor d = WindowManager.ActiveWindow;
            d.LastCaptureDelay = (float)nudDelay.Value;
            d.LastCaptureDelayedDisplay = this.delayedDisplay;
            d.LastCaptureMaxDuration = (float)nudDuration.Value;
            if (this.CaptureFolder != null)
                d.LastCaptureFolder = this.CaptureFolder.Id;

            WindowManager.SaveActiveWindow();

            // For "continue where you left off", the new screen list after this close will be saved
            // will be saved in the window descriptor during ScreenManager.OrganizeScreens().
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
            infobarCapture.Left = lblCameraTitle.Right + 5;
        }
        
        public void UpdateInfo(string signal, string bandwidth, string load, string drops)
        {
            infobarCapture.Visible = true;
            infobarCapture.UpdateValues(signal, bandwidth, load, drops);
        }

        public void UpdateLoadStatus(float load)
        {
            if (load < 85)
                infobarCapture.UpdateLoadStatus(LoadStatus.OK);
            else if (load < 100)
                infobarCapture.UpdateLoadStatus(LoadStatus.Warning);
            else
                infobarCapture.UpdateLoadStatus(LoadStatus.Critical);
        }
        
        public void UpdateGrabbingStatus(bool grabbing)
        {
            this.grabbing = grabbing;

            if (grabbing)
            {
                btnGrab.Image = Properties.Capture.pause_16;
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_PauseCamera);
                btnDelayedDisplay.Enabled = true;
            }
            else
            {
                btnGrab.Image = Properties.Capture.circled_play_16;
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_StartCamera);
                btnDelayedDisplay.Enabled = false;
            }
        }

        public void UpdateDelayedDisplay(bool delayedDisplay)
        {
            this.delayedDisplay = delayedDisplay;

            // Only show as "delayed" if there is actual delay set.
            bool hasDelay = (sldrDelay.Value > sldrDelay.Minimum);
            if (this.delayedDisplay && hasDelay)
            {
                btnDelayedDisplay.Image = Properties.Capture.live_photos_16;
                toolTips.SetToolTip(btnDelayedDisplay, "The view is delayed");
            }
            else
            {
                btnDelayedDisplay.Image = Properties.Capture.live_orange;
                toolTips.SetToolTip(btnDelayedDisplay, "The view is live");
            }
        }

        public void UpdateArmedStatus(bool armed)
        {
            this.armed = armed;

            if (armed)
            {
                btnArm.Image = Properties.Capture.quick_mode_on_green_16;
                toolTips.SetToolTip(btnArm, "The capture trigger is armed");
            }
            else
            {
                btnArm.Image = Properties.Capture.quick_mode_off_16;
                toolTips.SetToolTip(btnArm, "The capture trigger is disarmed");
            }
        }

        public void UpdateRecordingStatus(bool recording)
        {
            this.recording = recording;

            if(recording)
            {
                btnRecord.Image = Properties.Capture.record_stop;
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StopRecording);
            }
            else
            {
                btnRecord.Image = Properties.Capture.circle_16;
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StartRecording);
            }
            
            btnSettings.Enabled = !recording;

            btnCaptureFolders.Enabled = !recording;
            cbCaptureFolder.Enabled = !recording;
            tbFilename.Enabled = !recording;
        }
        public void UpdateDelayMax(double delaySeconds, int delayFrames)
        {
            if (delayFrames <= 0)
            {
                sldrDelay.Enabled = false;
                nudDelay.Enabled = false;
                return;
            }

            sldrDelay.Enabled = true;
            nudDelay.Enabled = true;

            sldrDelay.Maximum = (double)delayFrames;
            nudDelay.Minimum = 0;
            nudDelay.Maximum = (decimal)delaySeconds;

            // Force back the current user value.
            // This ensures the coherence of the delay in seconds and in frames after a change in camera framerate.
            NudDelay_ValueChanged(nudDelay, EventArgs.Empty);
            UpdateDelayedDisplay(delayedDisplay);
        }

        public void ForcePopulate(float delaySeconds, float maxDuration, Guid captureFolderId)
        {
            // Delay
            float epsilon = 0.001f;
            delaySeconds = Math.Min(Math.Max(delaySeconds, (float)nudDelay.Minimum), (float)nudDelay.Maximum - epsilon);
            nudDelay.Value = (decimal)delaySeconds;
            UpdateDelayedDisplay(delayedDisplay);

            // Max recording duration.
            maxDuration = Math.Min(Math.Max(maxDuration, (float)nudDuration.Minimum), (float)nudDuration.Maximum);
            nudDuration.Value = (decimal)maxDuration;

            // Capture folder
            foreach (var item in cbCaptureFolder.Items)
            {
                CaptureFolder cf = (CaptureFolder)item;
                if (cf.Id == captureFolderId)
                {
                    cbCaptureFolder.SelectedItem = item;
                    return;
                }
            }
        }

        public void UpdateNextVideoFilename(string filename)
        {
            tbFilename.Text = filename;
            tbFilename.Enabled = true;
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
            if (!delayUpdating && nudDelay.Maximum != 0)
            {
                // recalculate nud value and update nud
                double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
                nudDelay.Value = (decimal)(sldrDelay.Value / framerate);
            }

            presenter.View_DelayChanged(sldrDelay.Value);
            UpdateDelayedDisplay(delayedDisplay);
        }
        private void NudDelay_ValueChanged(object sender, EventArgs e)
        {
            if (nudDelay.Maximum == 0)
                return;

            double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
            double frames = Math.Round((double)nudDelay.Value * framerate);
            delayUpdating = true;
            sldrDelay.Force(frames);
            delayUpdating = false;
            UpdateDelayedDisplay(delayedDisplay);
        }
        private void nudDuration_ValueChanged(object sender, EventArgs e)
        {
            presenter.View_DurationChanged((float)nudDuration.Value);
        }
        private void BtnSettingsClick(object sender, EventArgs e)
        {
            presenter.View_Configure();
        }
        private void BtnGrabClick(object sender, EventArgs e)
        {
            presenter.View_ToggleGrabbing();
        }
        private void btnDelayedDisplay_Click(object sender, EventArgs e)
        {
            presenter.View_ToggleDelayedDisplay();
        }
        private void LblCameraInfoClick(object sender, EventArgs e)
        {
            presenter.View_Configure();
        }
        private void FNBImage_ImageClick(object sender, EventArgs e)
        {
            
        }
        private void FNBVideo_ImageClick(object sender, EventArgs e)
        {
            
        }
        private void btnCaptureFolders_Click(object sender, EventArgs e)
        {
            presenter.View_OpenPreferences(PreferenceTab.Capture_Paths);
        }

        private void BtnSnapshot_Click(object sender, EventArgs e)
        {
            presenter.View_SnapshotAsked();
        }
        private void BtnRecordClick(object sender, EventArgs e)
        {
            presenter.View_ToggleRecording();
        }
        private void btnArm_Click(object sender, EventArgs e)
        {
            presenter.View_ToggleArmingTrigger();
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
        private void ReloadTooltipsCulture()
        {
            toolTips.SetToolTip(btnSettings, ScreenManagerLang.ToolTip_ConfigureCamera);
            toolTips.SetToolTip(btnSnapshot, ScreenManagerLang.Generic_SaveImage);

            toolTips.SetToolTip(btnDelay, "Delay in seconds");
            toolTips.SetToolTip(btnDuration, "Total length of the recording in seconds");

            if (recording)
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StopRecording);
            else
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StartRecording);

            if (grabbing)
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_PauseCamera);
            else
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_StartCamera);

            if (armed)
                toolTips.SetToolTip(btnArm, "The capture trigger is armed");
            else
                toolTips.SetToolTip(btnArm, "The capture trigger is disarmed");

            if (delayedDisplay)
                toolTips.SetToolTip(btnDelayedDisplay, "The view is delayed");
            else
                toolTips.SetToolTip(btnDelayedDisplay, "The view is live");

            toolTips.SetToolTip(btnCaptureFolders, "Configure capture folders");
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(int commandCode)
        {
            if (tbFilename.Focused)
                return false;

            if (capturedFilesView.Editing)
                return false;

            // If we are not in a dual screen context just run the command for this screen.
            if (!presenter.Synched || DualCommandReceived == null)
                return ExecuteScreenCommand(commandCode);

            // Try to see if that command is handled by the dual capture controller.
            // At this point the command code is still the one from the single screen context.
            // Get the full command with the target shortcut key.
            HotkeyCommand command = Hotkeys.FirstOrDefault(h => h != null && h.CommandCode == commandCode);
            if (command == null)
                return false;

            // Look for a matching handler in the dual capture context.
            HotkeyCommand command2 = HotkeySettingsManager.FindCommand("DualCapture", command.KeyData);
            if (command2 == null)
            {
                // The shortcut isn't handled at the dual screen level, run it normally.
                return ExecuteScreenCommand(commandCode);
            }
            else
            {
                DualCommandReceived(this, new EventArgs<HotkeyCommand>(command2));
                return true;
            }
        }

        public bool ExecuteScreenCommand(int cmd)
        {
            CaptureScreenCommands command = (CaptureScreenCommands)cmd;

            switch (command)
            {
                case CaptureScreenCommands.ToggleGrabbing:
                    presenter.View_ToggleGrabbing();
                    break;
                case CaptureScreenCommands.ToggleRecording:
                    presenter.View_ToggleRecording();
                    break;
                case CaptureScreenCommands.TakeSnapshot:
                    presenter.View_SnapshotAsked();
                    break;
                case CaptureScreenCommands.ToggleDelayedDisplay:
                    presenter.View_ToggleDelayedDisplay();
                    break;
                case CaptureScreenCommands.ToggleArmCaptureTrigger:
                    presenter.View_ToggleArmingTrigger();
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

                case CaptureScreenCommands.GotoPreviousImage:
                    sldrDelay.Force(sldrDelay.Value - 1);
                    break;
                case CaptureScreenCommands.GotoFirstImage:
                    sldrDelay.Force(sldrDelay.Minimum);
                    break;
                case CaptureScreenCommands.BackwardRound10Percent:
                        sldrDelay.StepJump(-0.1);
                        break;
                case CaptureScreenCommands.BackwardRound1Percent:
                    sldrDelay.StepJump(-0.01);
                    break;
                case CaptureScreenCommands.GotoNextImage:
                    sldrDelay.Force(sldrDelay.Value + 1);
                    break;
                case CaptureScreenCommands.GotoLastImage:
                    sldrDelay.Force(sldrDelay.Maximum);
                    break;
                case CaptureScreenCommands.ForwardRound10Percent:
                    sldrDelay.StepJump(0.1);
                    break;
                case CaptureScreenCommands.ForwardRound1Percent:
                    sldrDelay.StepJump(0.01);
                    break;
                case CaptureScreenCommands.IncreaseDelayOneFrame:
                    sldrDelay.Force(sldrDelay.Value + 1);
                    break;
                case CaptureScreenCommands.IncreaseDelayHalfSecond:
                    {
                        double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
                        double target = framerate / 2.0;
                        sldrDelay.StepJump(target / sldrDelay.Maximum);
                        break;
                    }
                case CaptureScreenCommands.IncreaseDelayOneSecond:
                    {
                        double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
                        double target = framerate;
                        sldrDelay.StepJump(target / sldrDelay.Maximum);
                        break;
                    }
                case CaptureScreenCommands.DecreaseDelayOneFrame:
                    sldrDelay.Force(sldrDelay.Value - 1);
                    break;
                case CaptureScreenCommands.DecreaseDelayHalfSecond:
                    {
                        double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
                        double target = framerate / 2.0;
                        sldrDelay.StepJump( - target / sldrDelay.Maximum);
                        break;
                    }
                case CaptureScreenCommands.DecreaseDelayOneSecond:
                    {
                        double framerate = sldrDelay.Maximum / (double)nudDelay.Maximum;
                        double target = framerate;
                        sldrDelay.StepJump( - target / sldrDelay.Maximum);
                        break;
                    }
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
