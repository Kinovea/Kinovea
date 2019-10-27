#region License
/*
Copyright © Joan Charmant 2008.
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
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class CommonControlsPlayers : KinoveaControl
    {
        #region Events
        public event EventHandler PlayToggled;
        public event EventHandler GotoFirst;
        public event EventHandler GotoPrev;
        public event EventHandler GotoPrevKeyframe;
        public event EventHandler GotoNext;
        public event EventHandler GotoLast;
        public event EventHandler GotoNextKeyframe;
        public event EventHandler GotoSync;
        public event EventHandler SwapAsked;
        public event EventHandler AddKeyframe;
        public event EventHandler SyncAsked;
        public event EventHandler MergeAsked;
        public event EventHandler<TimeEventArgs> PositionChanged;
        public event EventHandler DualSaveAsked;
        public event EventHandler DualSnapshotAsked;
        #endregion

        #region Properties
        public bool Playing
        {
            get { return playing;  }
        }
        public bool Merging
        {
            get { return merging; }
        }
        #endregion

        #region Members
        private bool playing;
        private bool merging;
        private long oldPosition;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction
        public CommonControlsPlayers()
        {
            InitializeComponent();
            PostInit();
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("DualPlayer");
        }

        private void PostInit()
        {
            BackColor = Color.White;
            trkFrame.SetAsCommonTimeline(true);
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
            toolTips.SetToolTip(btnSnapshot, ScreenManagerLang.ToolTip_SideBySideSnapshot);
            toolTips.SetToolTip(btnDualSave, ScreenManagerLang.ToolTip_SideBySideVideo);
            
            RefreshMergeTooltip();
        }

        public void SetupTrkFrame(long min, long max, long pos)
        {
            trkFrame.Minimum = min;
            trkFrame.Maximum = max;
            trkFrame.Position = pos;
            trkFrame.Invalidate();
            UpdateDebug();
        }

        public void UpdateSyncPosition(long position)
        {
            trkFrame.UpdateSyncPointMarker(position);
            trkFrame.Invalidate();
            UpdateDebug();
        }
        
        public void UpdateCurrentPosition(long position)
        {
            trkFrame.Position = position;
            trkFrame.Invalidate();
            UpdateDebug();
        }

        public void UpdateHairline(long position, bool left)
        {
            if (left)
                trkFrame.LeftHairline = position;
            else
                trkFrame.RightHairline = position;

            trkFrame.UpdatePlayHeadMarkers();
            trkFrame.Invalidate();
        }

        private void UpdateDebug()
        {
            //lblInfo.Text = string.Format("{0}/{1}, s:{2}", trkFrame.Position, trkFrame.Maximum, trkFrame.SyncPosition);
        }
        
        public void Pause()
        {
            playing = false;
            RefreshPlayButton();
        }

        public void Play()
        {
            playing = true;
            RefreshPlayButton();
        }

        public void StopMerge()
        {
            merging = false;
            RefreshMergeTooltip();
        }
        #endregion

        #region Commands

        protected override bool ExecuteCommand(int cmd)
        {
            DualPlayerCommands command = (DualPlayerCommands)cmd;

            switch (command)
            {
                case DualPlayerCommands.TogglePlay:
                    PlayPause();
                    break;
                case DualPlayerCommands.GotoPreviousImage:
                    Previous();
                    break;
                case DualPlayerCommands.GotoFirstImage:
                    First();
                    break;
                case DualPlayerCommands.GotoPreviousKeyframe:
                    if (GotoPrevKeyframe != null)
                        GotoPrevKeyframe(this, EventArgs.Empty);
                    break;
                case DualPlayerCommands.GotoNextImage:
                    Next();
                    break;
                case DualPlayerCommands.GotoLastImage:
                    Last();
                    break;
                case DualPlayerCommands.GotoNextKeyframe:
                    if (GotoNextKeyframe != null)
                        GotoNextKeyframe(this, EventArgs.Empty);
                    break;
                case DualPlayerCommands.GotoSyncPoint:
                    if (GotoSync != null)
                        GotoSync(this, EventArgs.Empty);
                    break;
                case DualPlayerCommands.ToggleSyncMerge:
                    ToggleMerge();
                    break;
                case DualPlayerCommands.AddKeyframe:
                    if (AddKeyframe != null)
                        AddKeyframe(this, EventArgs.Empty);
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

        private void First()
        {
            if (GotoFirst != null)
                GotoFirst(this, EventArgs.Empty);
        }
        private void Previous()
        {
            if (GotoPrev != null)
                GotoPrev(this, EventArgs.Empty);
        }
        private void PlayPause()
        {
            playing = !playing;
            RefreshPlayButton();

            if (PlayToggled != null)
                PlayToggled(this, EventArgs.Empty);
        }
        private void Next()
        {
            if (GotoNext != null)
                GotoNext(this, EventArgs.Empty);
        }
        private void Last()
        {
            if (GotoLast != null)
                GotoLast(this, EventArgs.Empty);
        }
        private void ToggleMerge()
        {
            merging = !merging;

            if (MergeAsked != null)
                MergeAsked(this, EventArgs.Empty);

            RefreshMergeTooltip();
        }
        #region UI Handlers
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
            PlayPause();
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
            if (SwapAsked != null)
                SwapAsked(this, EventArgs.Empty);
        }
        private void btnSync_Click(object sender, EventArgs e)
        {
            if (SyncAsked != null)
                SyncAsked(this, EventArgs.Empty);
        }
        private void btnMerge_Click(object sender, EventArgs e)
        {
            ToggleMerge();
        }
        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            if (DualSnapshotAsked != null)
                DualSnapshotAsked(this, EventArgs.Empty);
        }
        private void btnDualVideo_Click(object sender, EventArgs e)
        {
            if (DualSaveAsked != null)
                DualSaveAsked(this, EventArgs.Empty);
        }
        private void trkFrame_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if (e.Position == oldPosition)
                return;
            
            oldPosition = e.Position;

            if (PositionChanged != null)
                PositionChanged(this, new TimeEventArgs(e.Position));
        }
        #endregion
        
        #region Lower level helpers
        private void RefreshPlayButton()
        {
            buttonPlay.Image = playing ? Resources.flatpause3b : Player.flatplay;
        }
        private void RefreshMergeTooltip()
        {
            string text = merging ? ScreenManagerLang.ToolTip_CommonCtrl_DisableMerge : ScreenManagerLang.ToolTip_CommonCtrl_EnableMerge;
            toolTips.SetToolTip(btnMerge, text);
        }
        #endregion
    }
}
