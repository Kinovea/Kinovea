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
            get { return view.Merging; }
        }
        public bool Playing
        {
            get { return view.Playing; }
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
        public void Pause()
        {
            view.Pause();
        }
        public void StopMerge()
        {
            view.StopMerge();
        }
        #endregion

        #region Players event handlers
        private void Player_PauseAsked(object sender, EventArgs e)
        {
            // An individual player asks for a global pause.
            if (synching && view.Playing)
                Pause();
        }
        #endregion

        #region Private methods
        private void Enter(List<AbstractScreen> screenList)
        {
            Exit();

            players.Clear();
            players.Add((PlayerScreen)screenList[0]);
            players.Add((PlayerScreen)screenList[1]);

            foreach (PlayerScreen player in players)
                AddEventHandlers(player);

            active = true;
        }
        private void Exit()
        {
            if (active)
            {
                foreach (PlayerScreen player in players)
                    RemoveEventHandlers(player);

                players.Clear();
            }

            active = false;
        }
        private void AddEventHandlers(PlayerScreen player)
        {
            player.PauseAsked += Player_PauseAsked;
        }
        private void RemoveEventHandlers(PlayerScreen player)
        {
            player.PauseAsked -= Player_PauseAsked;
        }
        #endregion
    }
}
