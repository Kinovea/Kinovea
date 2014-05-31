using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class DualPlayerController
    {
        #region Properties
        public bool Synching
        {
            get { return synching; }
            set { synching = value; }
        }
        public bool Merging
        {
            get { return view.SyncMerging; }
            set { view.SyncMerging = value; }
        }
        public bool Playing
        {
            get { return view.Playing; }
            set { view.Playing = value; }
        }
        public CommonControlsPlayers View
        {
            get { return view; }
        }
        #endregion

        #region Members
        private bool active;
        private bool synching;
        private CommonControlsPlayers view = new CommonControlsPlayers();
        private List<PlayerScreen> players = new List<PlayerScreen>();
        #endregion

        #region Constructor
        public DualPlayerController()
        {
            view.Dock = DockStyle.Fill;
        }
        #endregion

        #region Public methods
        public void ScreenListChanged(List<AbstractScreen> screenList)
        {
            if (screenList.Count == 2 && screenList[0] is PlayerScreen && screenList[1] is PlayerScreen)
                Enter(screenList);
            else
                Exit();
        }
        public void RefreshUICulture()
        {
            view.RefreshUICulture();
        }
        public void SetupTrkFrame(long min, long max, long pos)
        {
            view.SetupTrkFrame(min, max, pos);
        }
        public void UpdateTrkFrame(long position)
        {
            view.UpdateTrkFrame(position);
        }
        public void UpdateSyncPosition(long position)
        {
            view.UpdateSyncPosition(position);
        }
        public void DisplayAsPaused()
        {
            view.Playing = false;
        }
        #endregion

        #region Private methods
        private void Enter(List<AbstractScreen> screenList)
        {
            Exit();

            players.Clear();
            players.Add((PlayerScreen)screenList[0]);
            players.Add((PlayerScreen)screenList[1]);
            // Add event handlers.
            
            active = true;
        }
        private void Exit()
        {
            if (active)
            {
                // Remove event handlers.
                players.Clear();
            }

            active = false;
        }
        #endregion
    }
}
