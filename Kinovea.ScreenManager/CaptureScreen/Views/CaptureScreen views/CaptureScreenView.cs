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
        private bool recording;
        private bool grabbing;
        private bool armed = true;
        private DelayCompositeType delayCompositeType = DelayCompositeType.Basic;
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

            ConfigureDisplayControl(delayCompositeType);

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
            lblDelay.Text = ScreenManagerLang.lblDelay_Text;
            ReloadTooltipsCulture();
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
                btnGrab.Image = Properties.Capture.grab_pause;
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_PauseCamera);
            }
            else
            {
                btnGrab.Image = Properties.Capture.grab_start;
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_StartCamera);
            }
        }

        public void UpdateArmedStatus(bool armed)
        {
            this.armed = armed;

            if (armed)
            {
                btnArm.Image = Properties.Resources.microphone_16;
                toolTips.SetToolTip(btnArm, ScreenManagerLang.ToolTip_DisarmTrigger);
            }
            else
            {
                btnArm.Image = Properties.Resources.block_microphone_16;
                toolTips.SetToolTip(btnArm, ScreenManagerLang.ToolTip_ArmTrigger);
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
                btnRecord.Image = Properties.Capture.record_start;
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StartRecording);
            }
            
            btnSettings.Enabled = !recording;
            fnbImage.Enabled = !recording;
            fnbVideo.Enabled = !recording;
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
        }

        public void ForceDelaySeconds(float delaySeconds)
        {
            delaySeconds = Math.Min(Math.Max(delaySeconds, (float)nudDelay.Minimum), (float)nudDelay.Maximum);
            nudDelay.Value = (decimal)delaySeconds;
        }

        public void UpdateNextImageFilename(string filename)
        {
            fnbImage.Filename = filename;
            fnbImage.Editable = true;
        }
        public void UpdateNextVideoFilename(string filename)
        {
            fnbVideo.Filename = filename;
            fnbVideo.Editable = true;
        }
        
        public void ShowThumbnails()
        {
            if(!pnlCapturedVideos.Visible)
                ToggleCapturedVideosPanel();
        }

        /// <summary>
        /// Show the correct control based on display type.
        /// </summary>
        public void ConfigureDisplayControl(DelayCompositeType type)
        {
            delayCompositeType = type;

            sldrDelay.Visible = false;
            lblDelay.Visible = false;
            
            switch (type)
            {
                case DelayCompositeType.Basic:
                    sldrDelay.Visible = true;
                    lblDelay.Visible = true;
                    break;
                default:
                    break;
            }
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
            presenter.View_EditPathConfiguration(false);
        }
        private void FNBVideo_ImageClick(object sender, EventArgs e)
        {
            presenter.View_EditPathConfiguration(true);
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

            if (recording)
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StopRecording);
            else
                toolTips.SetToolTip(btnRecord, ScreenManagerLang.ToolTip_StartRecording);

            if (grabbing)
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_PauseCamera);
            else
                toolTips.SetToolTip(btnGrab, ScreenManagerLang.ToolTip_StartCamera);

            if (armed)
                toolTips.SetToolTip(btnArm, ScreenManagerLang.ToolTip_DisarmTrigger);
            else
                toolTips.SetToolTip(btnArm, ScreenManagerLang.ToolTip_ArmTrigger);
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(int commandCode)
        {
            if (fnbImage.Focused || fnbVideo.Focused)
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
