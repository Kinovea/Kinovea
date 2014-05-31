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
using System.Drawing;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class CommonControlsCapture : UserControl
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
        }

        public void RefreshUICulture()
        {
            // Labels
            lblInfo.Text = ScreenManagerLang.lblInfo_Text;

            // ToolTips
        }


        private void btnGrab_Click(object sender, EventArgs e)
        {
            grabbing = !grabbing;
            RefreshGrabbingButton();

            if (GrabbingChanged != null)
                GrabbingChanged(this, new EventArgs<bool>(grabbing));
        }

        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            if (SnapshotAsked != null)
                SnapshotAsked(this, EventArgs.Empty);
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            recording = !recording;
            RefreshRecordingButton();

            if (RecordingChanged != null)
                RecordingChanged(this, new EventArgs<bool>(recording));
        }
        
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if (SwapAsked != null)
                SwapAsked(this, EventArgs.Empty);
        }

        private void RefreshGrabbingButton()
        {
            btnGrab.Image = grabbing ? Properties.Capture.grab_pause : Properties.Capture.grab_start;
        }

        private void RefreshRecordingButton()
        {
            btnRecord.Image = recording ? Properties.Capture.record_stop : Properties.Capture.record_start;
        }
    }
}
