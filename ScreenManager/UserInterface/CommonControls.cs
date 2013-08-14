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

using Kinovea.ScreenManager.Languages;
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
        #region Properties
        public IScreenManagerUIContainer Controller
        {
            set { controller = value; }
        }
        public bool Playing
        {
            get { return playing;  }
            set 
            { 
                playing = value;
                RefreshPlayButton();
            }
        }
        public bool SyncMerging
        {
            get { return syncMerging; }
            set 
            { 
                syncMerging = value; 
                RefreshMergeTooltip();
            }
        }
        #endregion

        #region Members
        private bool playing;
        private bool syncMerging;
        private long oldPosition;
        private IScreenManagerUIContainer controller;
        private Button btnSnapShot = new Button();
        private Button btnDualVideo = new Button();
        #endregion

        #region Construction
        public CommonControls()
        {
            InitializeComponent();
            PostInit();
        }
        private void PostInit()
        {
            BackColor = Color.White;
            
            btnSnapShot.BackColor = System.Drawing.Color.Transparent;
            btnSnapShot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            btnSnapShot.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSnapShot.FlatAppearance.BorderSize = 0;
            btnSnapShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnSnapShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSnapShot.Image = ScreenManager.Properties.Resources.snapsingle_1;
            btnSnapShot.Location = new System.Drawing.Point(trkFrame.Right + 10, btnMerge.Top);
            btnSnapShot.MinimumSize = new System.Drawing.Size(25, 25);
            btnSnapShot.Name = "btnSnapShot";
            btnSnapShot.Size = new System.Drawing.Size(30, 25);
            btnSnapShot.UseVisualStyleBackColor = false;
            btnSnapShot.Click += new System.EventHandler(btnSnapshot_Click);
            
            btnDualVideo.BackColor = System.Drawing.Color.Transparent;
            btnDualVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            btnDualVideo.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDualVideo.FlatAppearance.BorderSize = 0;
            btnDualVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnDualVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDualVideo.Image = ScreenManager.Properties.Resources.savevideo;
            btnDualVideo.Location = new System.Drawing.Point(btnSnapShot.Right + 10, btnSnapShot.Top);
            btnDualVideo.MinimumSize = new System.Drawing.Size(25, 25);
            btnDualVideo.Name = "btnDualVideo";
            btnDualVideo.Size = new System.Drawing.Size(30, 25);
            btnDualVideo.UseVisualStyleBackColor = false;
            btnDualVideo.Click += new System.EventHandler(btnDualVideo_Click);
            
            this.Controls.Add(btnSnapShot);
            this.Controls.Add(btnDualVideo);
        }
        #endregion
        
        #region Public methods
        public void RefreshUICulture()
        {
            // Labels
            lblInfo.Text = ScreenManagerLang.lblInfo_Text;

            // ToolTips
            toolTips.SetToolTip(buttonGotoFirst, ScreenManagerLang.buttonGotoFirst_ToolTip);
            toolTips.SetToolTip(buttonGotoLast, ScreenManagerLang.buttonGotoLast_ToolTip);
            toolTips.SetToolTip(buttonGotoNext, ScreenManagerLang.buttonGotoNext_ToolTip);
            toolTips.SetToolTip(buttonGotoPrevious, ScreenManagerLang.buttonGotoPrevious_ToolTip);
            toolTips.SetToolTip(buttonPlay, ScreenManagerLang.buttonPlay_ToolTip);
            toolTips.SetToolTip(btnSwap, ScreenManagerLang.mnuSwapScreens);
            toolTips.SetToolTip(btnSync, ScreenManagerLang.btnSync_ToolTip);
            toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.ToolTip_SideBySideSnapshot);
            toolTips.SetToolTip(btnDualVideo, ScreenManagerLang.ToolTip_SideBySideVideo);
            
            RefreshMergeTooltip();
        }
        public void First()
        {
            if(controller != null)
            {
                controller.CommonCtrl_GotoFirst();
                trkFrame.Position = trkFrame.Minimum;
                trkFrame.Invalidate();
                PlayStopped();
            }
        }
        public void Previous()
        {
            if(controller != null)
            { 
                controller.CommonCtrl_GotoPrev(); 
                trkFrame.Position--;
                trkFrame.Invalidate();
            }
        }
        public void Play()
        {
            if(controller != null)
            {
                playing = !playing;
                RefreshPlayButton();
                controller.CommonCtrl_Play(); 
            }
        }
        public void Next()
        {
            if(controller != null)
            { 
                controller.CommonCtrl_GotoNext(); 
                trkFrame.Position++;
                trkFrame.Invalidate();
            }
        }
        public void Last()
        {
            if(controller != null)
            {
                controller.CommonCtrl_GotoLast(); 
                trkFrame.Position = trkFrame.Maximum;
                trkFrame.Invalidate();
            }
        }
        public void UpdateSyncPosition(long position)
        {
            trkFrame.UpdateSyncPointMarker(position);
            trkFrame.Invalidate();
        }
        public void SetupTrkFrame(long min, long max, long pos)
        {
            trkFrame.Minimum = min;
            trkFrame.Maximum = max;
            trkFrame.Position = pos;
            trkFrame.Invalidate();
        }
        public void UpdateTrkFrame(long position)
        {
            trkFrame.Position = position;
            trkFrame.Invalidate();
        }
        #endregion
        
        #region UI Handlers
        private void CommonControls_Resize(object sender, EventArgs e)
        {
            btnSnapShot.Location = new System.Drawing.Point(trkFrame.Right + 10, btnMerge.Top);
            btnDualVideo.Location = new System.Drawing.Point(btnSnapShot.Right + 10, btnMerge.Top);
        }
        private void buttonGotoFirst_Click(object sender, EventArgs e)
        {
            First();
        }
        private void buttonGotoPrevious_Click(object sender, EventArgs e)
        {
            Previous();
        }
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            Play();
        }
        private void buttonGotoNext_Click(object sender, EventArgs e)
        {
            Next();
        }
        private void buttonGotoLast_Click(object sender, EventArgs e)
        {
            Last();
        }
        private void btnSwap_Click(object sender, EventArgs e)
        {
            if(controller != null)
                controller.CommonCtrl_Swap(); 
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            if(controller != null)
                controller.CommonCtrl_Sync(); 
        }
        private void btnMerge_Click(object sender, EventArgs e)
        {
            if(controller != null)
            { 
                syncMerging = !syncMerging;
                controller.CommonCtrl_Merge();
                RefreshMergeTooltip();
            }
        }
        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            if(controller != null)
                controller.CommonCtrl_Snapshot();
        }
        private void btnDualVideo_Click(object sender, EventArgs e)
        {
            if(controller != null)
                controller.CommonCtrl_DualVideo();
        }
        private void trkFrame_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if(e.Position != oldPosition)
            {
                oldPosition = e.Position;
                if(controller != null)
                    controller.CommonCtrl_PositionChanged(e.Position); 
            }
        }
        #endregion
        
        #region TrkFrame Handlers
        
        #endregion
        
        #region Lower level helpers
        private void RefreshPlayButton()
        {
            if (playing)
                buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqpause6;
            else
                buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqplay17;
        }
        private void PlayStopped()
        {
            buttonPlay.Image = Kinovea.ScreenManager.Properties.Resources.liqplay17;
        }
        private void RefreshMergeTooltip()
        {
            if(syncMerging)
                toolTips.SetToolTip(btnMerge, ScreenManagerLang.ToolTip_CommonCtrl_DisableMerge);
            else
                toolTips.SetToolTip(btnMerge, ScreenManagerLang.ToolTip_CommonCtrl_EnableMerge);	
        }
        #endregion
    }
}
