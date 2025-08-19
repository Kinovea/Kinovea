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
using System.Drawing;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class CommonControlsCapture : KinoveaControl
    {
        #region Events
        public event EventHandler SwapAsked;
        public event EventHandler<EventArgs<bool>> GrabbingChanged;
        public event EventHandler SnapshotAsked;
        public event EventHandler<EventArgs<bool>> RecordingChanged;
        #endregion

        #region Properties
        public bool Grabbing
        {
            get { return grabbing; }
            set { grabbing = value; }
        }

        public bool Recording
        {
            get { return recording; }
            set { recording = value; }
        }
        #endregion

        #region Members
        private bool grabbing = true;
        private bool recording;
        #endregion

        public CommonControlsCapture()
        {
            InitializeComponent();
            BackColor = Color.White;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("DualCapture");
        }

        public void RefreshUICulture()
        {
            // Labels
            lblInfo.Text = ScreenManagerLang.lblInfo_Text;

            // ToolTips
        }

        public void UpdateRecordingStatus(bool status)
        {
            recording = status;
            RefreshRecordingButton();
        }

        #region Commands

        protected override bool ExecuteCommand(int cmd)
        {
            DualCaptureCommands command = (DualCaptureCommands)cmd;

            switch (command)
            {
                case DualCaptureCommands.ToggleGrabbing:
                    ToggleGrabbing();
                    break;
                case DualCaptureCommands.ToggleRecording:
                    ToggleRecording();
                    break;
                case DualCaptureCommands.TakeSnapshot:
                    TakeSnapshot();
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public void ExecuteDualCommand(int cmd)
        {
            ExecuteCommand(cmd);
        }

        #endregion

        private void ToggleGrabbing()
        {
            grabbing = !grabbing;
            RefreshGrabbingButton();

            if (GrabbingChanged != null)
                GrabbingChanged(this, new EventArgs<bool>(grabbing));
        }

        private void ToggleRecording()
        {
            recording = !recording;
            RefreshRecordingButton();

            if (RecordingChanged != null)
                RecordingChanged(this, new EventArgs<bool>(recording));
        }

        private void TakeSnapshot()
        {
            if (SnapshotAsked != null)
                SnapshotAsked(this, EventArgs.Empty);
        }

        #region UI Handlers
        private void btnGrab_Click(object sender, EventArgs e)
        {
            ToggleGrabbing();
        }

        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            TakeSnapshot();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            ToggleRecording();
        }
        
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if (SwapAsked != null)
                SwapAsked(this, EventArgs.Empty);
        }
        #endregion

        #region Lower level helpers
        private void RefreshGrabbingButton()
        {
            btnGrab.Image = grabbing ? Properties.Capture.pause_16 : Properties.Capture.circled_play_16;
            btnRecord.Enabled = grabbing;
        }

        private void RefreshRecordingButton()
        {
            btnRecord.Image = recording ? Properties.Capture.record_stop : Properties.Capture.circle_16;
        }
        #endregion
    }
}
