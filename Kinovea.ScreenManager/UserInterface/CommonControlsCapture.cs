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

namespace Kinovea.ScreenManager
{
    public partial class CommonControlsCapture : UserControl
    {
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

        private ICommonControlsManager screenManager;
        private bool grabbing = true;
        private bool recording;

        public CommonControlsCapture()
        {
            InitializeComponent();
            BackColor = Color.White;
        }
        
        public void SetManager(ICommonControlsManager screenManager)
        {
            this.screenManager = screenManager;
        }

        public void RefreshUICulture()
        {
            // Labels
            lblInfo.Text = ScreenManagerLang.lblInfo_Text;

            // ToolTips
        }


        private void btnGrab_Click(object sender, EventArgs e)
        {
            if (screenManager == null)
                return;

            grabbing = !grabbing;
            RefreshGrabbingButton();
            screenManager.CommonCtrl_GrabbingChanged(grabbing);
        }

        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            if (screenManager != null)
                screenManager.CommonCtrl_Snapshot();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (screenManager == null)
                return;

            recording = !recording;
            RefreshRecordingButton();
            screenManager.CommonCtrl_RecordingChanged(recording);
        }
        
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if(screenManager != null)
                screenManager.CommonCtrl_Swap(); 
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
