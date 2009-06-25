/*
Copyright © Joan Charmant 2008.
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class CommonControls : UserControl
    {

        #region EventDelegates
        // Déclarations de Types
        public delegate void GotoFirstHandler(object sender, EventArgs e);
        public delegate void GotoLastHandler(object sender, EventArgs e);
        public delegate void GotoPrevHandler(object sender, EventArgs e);
        public delegate void GotoNextHandler(object sender, EventArgs e);
        public delegate void PlayHandler(object sender, EventArgs e);
        public delegate void SwapHandler(object sender, EventArgs e);
        public delegate void SyncHandler(object sender, EventArgs e);
        public delegate void PositionChangedHandler(object sender, long _iPosition);
          
        // Déclarations des évènements
        [Category("Action"), Browsable(true)]
        public event GotoFirstHandler GotoFirst;
        [Category("Action"), Browsable(true)]
        public event GotoLastHandler GotoLast;
        [Category("Action"), Browsable(true)]
        public event GotoPrevHandler GotoPrev;
        [Category("Action"), Browsable(true)]
        public event GotoNextHandler GotoNext;
        [Category("Action"), Browsable(true)]
        public event PlayHandler Play;
        [Category("Action"), Browsable(true)]
        public event SwapHandler Swap;
        [Category("Action"), Browsable(true)]
        public event SyncHandler Sync;
        [Category("Action"), Browsable(true)]
        public event PositionChangedHandler PositionChanged;
        #endregion

        #region Properties
        public bool Playing
        {
            get { return m_bPlaying;  }
            set 
            { 
                m_bPlaying = value;
                RefreshPlayButton();
            }
        }
        public int SyncOffset
        {
            set 
            {
                int iValue = value;
                lblSyncOffset.Text = "SyncOffset : " + iValue;
                lblSyncOffset.Invalidate();
            }
        }
        #endregion
     
        #region Members
        private bool m_bPlaying = false;
        #endregion

        public CommonControls()
        {
            InitializeComponent();
            BackColor = Color.White;
        }

        public void RefreshUICulture(ResourceManager _resManager)
        {
            // Labels
            lblInfo.Text = _resManager.GetString("lblInfo_Text", Thread.CurrentThread.CurrentUICulture);

            // ToolTips
            toolTips.SetToolTip(buttonGotoFirst, _resManager.GetString("buttonGotoFirst_ToolTip", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(buttonGotoLast, _resManager.GetString("buttonGotoLast_ToolTip", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(buttonGotoNext, _resManager.GetString("buttonGotoNext_ToolTip", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(buttonGotoPrevious, _resManager.GetString("buttonGotoPrevious_ToolTip", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(buttonPlay, _resManager.GetString("buttonPlay_ToolTip", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnSwap, _resManager.GetString("mnuSwapScreens", Thread.CurrentThread.CurrentUICulture));
            toolTips.SetToolTip(btnSync, _resManager.GetString("btnSync_ToolTip", Thread.CurrentThread.CurrentUICulture));
        }

        public void buttonGotoFirst_Click(object sender, EventArgs e)
        {
            
            if (GotoFirst != null) { GotoFirst(this, EventArgs.Empty); }
            PlayStopped();
        }
        public void buttonGotoPrevious_Click(object sender, EventArgs e)
        {
            if (GotoPrev != null) { GotoPrev(this, EventArgs.Empty); }
        }
        public void buttonPlay_Click(object sender, EventArgs e)
        {
            if (Play != null) 
            {
                m_bPlaying = !m_bPlaying;
                RefreshPlayButton();
                Play(this, EventArgs.Empty); 
            }
        }
        public void buttonGotoNext_Click(object sender, EventArgs e)
        {
            if (GotoNext != null) { GotoNext(this, EventArgs.Empty); }
        }
        public void buttonGotoLast_Click(object sender, EventArgs e)
        {
            if (GotoLast != null) { GotoLast(this, EventArgs.Empty); }
        }
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if (Swap != null) { Swap(this, EventArgs.Empty); }
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            if (Sync != null) { Sync(this, EventArgs.Empty); }
        }
        private void PlayStopped()
        {
            buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqplay17;
        }
        private void trkFrame_PositionChanging(object sender, long _iPosition)
        {
            //UpdateDebug();
            if (PositionChanged != null) { PositionChanged(sender, _iPosition); }
        }

        private void trkFrame_PositionChanged(object sender, long _iPosition)
        {
            //UpdateDebug();
            if (PositionChanged != null) { PositionChanged(sender, _iPosition); }
        }

        public void UpdateDebug()
        {
            lblTrkFrameInfos.Text = "Min : " + trkFrame.Minimum + ", Max : " + trkFrame.Maximum + ", Pos : " + trkFrame.Position;
            lblTrkFrameInfos.Invalidate();
        }
        private void RefreshPlayButton()
        {
            if (m_bPlaying)
            {
                buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqpause6;
            }
            else
            {
                buttonPlay.BackgroundImage = Kinovea.ScreenManager.Properties.Resources.liqplay17;
            }
        }

            
    }
}
