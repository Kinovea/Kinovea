using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Synchronization logic.
    /// </summary>
    public class DualPlayerController
    {
        #region Events
        public event EventHandler ExportImageAsked;
        public event EventHandler ExportVideoAsked;
        #endregion

        #region Properties
        public CommonControlsPlayers View
        {
            get { return view; }
        }
        public bool Active
        {
            get { return active; }
        }
        public bool DualSaveInProgress
        {
            get { return dualSaveInProgress; }
            set { dualSaveInProgress = value; }
        }

        public CommonTimeline CommonTimeline
        {
            get { return commonTimeline; }
        }
        #endregion

        #region Members
        private bool active;
        private bool synching;
        private bool dynamicSynching;
        private bool dualSaveInProgress;

        private CommonTimeline commonTimeline = new CommonTimeline();   
        private long currentTime;   // current time in common timeline, in microseconds.
        private CommonControlsPlayers view = new CommonControlsPlayers();
        private List<PlayerScreen> players = new List<PlayerScreen>();
        private int resyncOperations = 0;
        private int maxResyncOperations = 1;
        private HotkeyCommand[] hotkeys;

        // If two videos have creation dates within this many seconds we consider them part 
        // of the same dual recording and start them together in the replay watcher context.
        private const int spanDualRecording = 5;
                                                 
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DualPlayerController()
        {
            view.Dock = DockStyle.Fill;

            view.PlayToggled += CCtrl_PlayToggled;
            view.GotoFirst += CCtrl_GotoFirst;
            view.GotoPrev += CCtrl_GotoPrev;
            view.GotoPrevKeyframe += CCtrl_GotoPrevKeyframe;
            view.GotoNext += CCtrl_GotoNext;
            view.GotoLast += CCtrl_GotoLast;
            view.GotoNextKeyframe += CCtrl_GotoNextKeyframe;
            view.GotoSync += CCtrl_GotoSync;
            view.AddKeyframe += CCtrl_AddKeyframe;
            view.SyncAsked += CCtrl_SyncAsked;
            view.MergeAsked += CCtrl_MergeAsked;
            view.PositionChanged += CCtrl_PositionChanged;
            view.ExportImageAsked += (s, e) => ExportImageAsked?.Invoke(s, e);
            view.ExportvideoAsked += (s, e) => ExportVideoAsked?.Invoke(s, e);

            hotkeys = HotkeySettingsManager.LoadHotkeys("DualPlayer");
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

            if (synching)
                InitializeSync();
        }
        public void UpdateTrkFrame(long position)
        {
            view.UpdateCurrentPosition(position);
        }
        public void Pause()
        {
            if (!active || !synching)
                return;

            dynamicSynching = false;

            if (view.Playing)
                view.Pause();

            if (players[0].IsPlaying)
                players[0].view.OnButtonPlay();

            if (players[1].IsPlaying)
                players[1].view.OnButtonPlay();
        }
        public void StopMerge()
        {
            view.StopMerge();
        }
        public void SwapSync()
        {
            if (!synching)
                return;

            PlayerScreen temp = players[0];
            players[0] = players[1];
            players[1] = temp;

            UpdateHairLines();
        }
        public void CommitLaunchSettings()
        {
            if (!active)
                return;

            if (!players[0].Full || !players[1].Full)
                return;

            synching = true;
            players[0].Synched = true;
            players[1].Synched = true;

            InitializeSync();

            // Handle dual replay.
            // The screens know they are in the context of a dual replay and have ignored the auto-replay flag,
            // to avoid one video starting before the other. They wait to be started by the dual playback.
            if (!players[0].IsReplayWatcher || !players[1].IsReplayWatcher || !File.Exists(players[0].FilePath) || !File.Exists(players[1].FilePath))
                return;

            // Check that the creation dates are close enough to be considered part of the same recording.
            DateTime creation1 = File.GetCreationTime(players[0].FilePath);
            DateTime creation2 = File.GetCreationTime(players[1].FilePath);
            double span = Math.Abs((creation1 - creation2).TotalSeconds);
            if (span < spanDualRecording)
            {
                resyncOperations = 0;
                view.Play();
                dynamicSynching = true;
                EnsureBothPlaying();
            }
            else
            {
                log.DebugFormat("Dual replay detected but the videos are not considered part of the same recording. {0}s offset", span);
            }
        }
        #endregion

        #region Players event handlers
        private void Player_PauseAsked(object sender, EventArgs e)
        {
            if (active && synching && view.Playing)
                Pause();
        }

        private void Player_PlayStarted(object sender, EventArgs e)
        {
            // If both players are playing, we must activate the dynamic synching, even if they were started independently. 
            // This way they will continue playing on the next loop, this time in sync.
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            PlayerScreen otherPlayer = GetOtherPlayer(player);

            if (player.IsPlaying && otherPlayer.IsPlaying && !view.Playing)
            {
                // Immediately force synchronization.
                // This may not fully register for automatically started players, see `resyncOperations`.
                commonTimeline.Initialize(players[0], players[1]);
                view.UpdateSyncPosition(commonTimeline.GetCommonTime(players[0], players[0].LocalTimeOriginPhysical));

                AlignPlayers(false);

                resyncOperations = 0;
                view.Play();
                dynamicSynching = true;
                EnsureBothPlaying();
            }
        }

        private void Player_SpeedChanged(object sender, EventArgs e)
        {
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null || !PreferencesManager.PlayerPreferences.SyncLockSpeed)
                return;

            GetOtherPlayer(player).RealtimePercentage = player.RealtimePercentage;
        }

        private void Player_TimeOriginChanged(object sender, EventArgs e)
        {
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            // Reinit synchronization.
            commonTimeline.Initialize(players[0], players[1]);
            currentTime = commonTimeline.GetCommonTime(player, player.LocalTime);
            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(currentTime);
            UpdateHairLines();
        }

        private void Player_HighSpeedFactorChanged(object sender, EventArgs e)
        {
            if (!active || !synching)
                return;

            if (PreferencesManager.PlayerPreferences.SyncLockSpeed)
            {
                double percentage = Math.Min(players[0].RealtimePercentage, players[1].RealtimePercentage);
                players[0].RealtimePercentage = percentage;
                players[1].RealtimePercentage = percentage;
            }

            // Synchronization must be reinitialized.
            commonTimeline.Initialize(players[0], players[1]);

            // TODO: Check if current time is still in bounds.
            currentTime = Math.Min(currentTime, commonTimeline.GetCommonTime(players[0], players[0].LocalTime));

            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(currentTime); 
        }

        private void Player_ImageChanged(object sender, EventArgs<Bitmap> e)
        {
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            PlayerScreen otherPlayer = GetOtherPlayer(player);

            if (dynamicSynching)
            {
                if (player.IsPlaying)
                {
                    //log.DebugFormat("Received image from [{0}] ({1}).", GetPlayerIndex(player), player.LocalTime / 1000);
                    currentTime = commonTimeline.GetCommonTime(player, player.LocalTime);

                    if (otherPlayer.IsPlaying && resyncOperations < maxResyncOperations)
                    {
                        //----------------------------------------------------------------------------------
                        // Test for desync. 
                        // This is not the primary synchronization mechanism, videos should synchronize naturally from being started
                        // at the right time, based on their time origin and the fact that their playback rate relative to real time should match.
                        //
                        // However, for videos started automatically, we can't guarantee that one isn't ahead of the other.
                        // We allow ourselves one attempt at resync here.
                        // This kind of resync can easily misfire and cause judder so it's only used very sparingly.
                        //----------------------------------------------------------------------------------
                        long otherTime = commonTimeline.GetCommonTime(otherPlayer, otherPlayer.LocalTime);
                        long divergence = Math.Abs(currentTime - otherTime);
                        long frameTime = Math.Max(player.LocalFrameTime, otherPlayer.LocalFrameTime);
                        if (divergence > frameTime)
                        {
                            resyncOperations++;

                            log.WarnFormat("Synchronization divergence: [{0}]@{1} vs [{2}]@{3}. Resynchronizing.", 
                                GetPlayerIndex(player), currentTime / 1000, GetPlayerIndex(otherPlayer), otherTime / 1000);

                            AlignPlayers(true);
                        }
                    }

                    EnsureBothPlaying();
                }
                else if (!otherPlayer.IsPlaying)
                {
                    // Both players have completed a loop and are waiting.
                    currentTime = 0;
                    EnsureBothPlaying();
                }
            }

            UpdateHairLines();
            
            if (!view.Merging || e.Value == null)
                return;

            otherPlayer.SetSyncMergeImage(e.Value, !dualSaveInProgress);
        }
        #endregion

        #region View event handlers
        private void CCtrl_PlayToggled(object sender, EventArgs e)
        {
            if (synching)
            {
                AlignPlayers(false);

                dynamicSynching = view.Playing;
                if (dynamicSynching)
                {
                    resyncOperations = 0;
                    EnsureBothPlaying();
                }
            }

            // Propagate the stop call to screens.
            if (!view.Playing)
                Pause();
        }
        private void CCtrl_GotoFirst(object sender, EventArgs e)
        {
            Pause();

            if (synching)
            {
                currentTime = 0;
                GotoTime(currentTime, true);
                UpdateTrkFrame(currentTime);
            }
            else
            {
                foreach (PlayerScreen player in players)
                    player.view.buttonGotoFirst_Click(this, EventArgs.Empty);
            }
        }
        private void CCtrl_GotoPrev(object sender, EventArgs e)
        {
            Pause();

            if (synching)
            {
                if (currentTime > 0)
                {
                    currentTime -= commonTimeline.FrameTime;

                    GotoTime(currentTime, true);
                    UpdateTrkFrame(currentTime);
                }
            }
            else
            {
                foreach (PlayerScreen screen in players)
                    screen.view.buttonGotoPrevious_Click(this, EventArgs.Empty);
            }
        }
        private void CCtrl_GotoNext(object sender, EventArgs e)
        {
            Pause();

            if (synching)
            {
                if (currentTime < commonTimeline.LastTime)
                {
                    currentTime += commonTimeline.FrameTime;
                    
                    GotoTime(currentTime, true);
                    UpdateTrkFrame(currentTime);
                }
            }
            else
            {
                foreach (PlayerScreen player in players)
                    player.view.buttonGotoNext_Click(this, EventArgs.Empty);
            }
        }
        private void CCtrl_GotoLast(object sender, EventArgs e)
        {
            Pause();

            if (synching)
            {
                currentTime = commonTimeline.LastTime;
                GotoTime(currentTime, true);
                UpdateTrkFrame(currentTime);
            }
            else
            {
                foreach (PlayerScreen player in players)
                    player.view.buttonGotoLast_Click(this, EventArgs.Empty);
            }
        }
        private void CCtrl_SyncAsked(object sender, EventArgs e)
        {
            if (!synching)
                return;

            SetSyncPoint(false);
            GotoTime(currentTime, true);
        }
        private void CCtrl_MergeAsked(object sender, EventArgs e)
        {
            if (!synching)
                return;

            log.Debug(String.Format("SyncMerge videos is now {0}", view.Merging.ToString()));

            // This will also do a full refresh, and trigger back Player_ImageChanged().
            players[0].SyncMerge = view.Merging;
            players[1].SyncMerge = view.Merging;
        }
        private void CCtrl_PositionChanged(object sender, TimeEventArgs e)
        {
            if (!synching)
                return;

            Pause();
            
            currentTime = e.Time;
            GotoTime(currentTime, true);
        }
        
        private void CCtrl_GotoPrevKeyframe(object sender, EventArgs e)
        {
            Pause();

            if (!synching)
                return;

            players[0].GotoPrevKeyframe();
            players[1].GotoPrevKeyframe();
        }
        private void CCtrl_GotoNextKeyframe(object sender, EventArgs e)
        {
            Pause();

            if (!synching)
                return;

            players[0].GotoNextKeyframe();
            players[1].GotoNextKeyframe();
        }
        private void CCtrl_AddKeyframe(object sender, EventArgs e)
        {
            Pause();

            if (!synching)
                return;

            players[0].AddKeyframe();
            players[1].AddKeyframe();
        }
        private void CCtrl_GotoSync(object sender, EventArgs e)
        {
            Pause();

            if (!synching)
                return;
            
            currentTime = commonTimeline.GetCommonTime(players[0], players[0].LocalTimeOriginPhysical);
            GotoTime(currentTime, true);
            UpdateTrkFrame(currentTime);
        }
        
        #endregion

        #region Entering/Exiting dual player management
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
            synching = false;
            dynamicSynching = false;

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
            player.PlayStarted += Player_PlayStarted;
            player.PauseAsked += Player_PauseAsked;
            player.SpeedChanged += Player_SpeedChanged;
            player.HighSpeedFactorChanged += Player_HighSpeedFactorChanged;
            player.TimeOriginChanged += Player_TimeOriginChanged;
            player.ImageChanged += Player_ImageChanged;
        }
        private void RemoveEventHandlers(PlayerScreen player)
        {
            player.PlayStarted -= Player_PlayStarted;
            player.PauseAsked -= Player_PauseAsked;
            player.SpeedChanged -= Player_SpeedChanged;
            player.HighSpeedFactorChanged -= Player_HighSpeedFactorChanged;
            player.ImageChanged -= Player_ImageChanged;
        }
        #endregion

        public void ExecuteDualCommand(HotkeyCommand playerCommand)
        {
            // A player has detected that a hotkey it received should actually be handled at the dual player level.
            // At that point there is still two options, either it's a true dual player command,
            // something normally bound to controls in the common controls,
            // or it's a multiplexed command, a command that should simply be forwarded to each player.

            HotkeyCommand dualCommand = hotkeys.FirstOrDefault(hk => hk != null && hk.KeyData == playerCommand.KeyData);
            if (dualCommand == null)
                return;

            DualPlayerCommands command = (DualPlayerCommands)dualCommand.CommandCode;

            switch(command)
            {
                case DualPlayerCommands.GotoPreviousKeyframe:
                case DualPlayerCommands.GotoNextKeyframe:
                case DualPlayerCommands.GotoSyncPoint:
                case DualPlayerCommands.AddKeyframe:
                    players[0].ExecuteScreenCommand(playerCommand.CommandCode);
                    players[1].ExecuteScreenCommand(playerCommand.CommandCode);
                    break;

                default:
                    view.ExecuteDualCommand(dualCommand.CommandCode);
                    break;
            }
        }


        #region Synchronization
        public void ResetSync()
        {
            if (!active)
                return;
            
            log.DebugFormat("Re-initializing dual player synchronization");

            synching = false;
            dynamicSynching = false;
            Pause();

            if (!players[0].Full || !players[1].Full)
                return;

            synching = true;
            players[0].Synched = true;
            players[1].Synched = true;

            if (PreferencesManager.PlayerPreferences.SyncLockSpeed)
            {
                double percentage = Math.Min(players[0].RealtimePercentage, players[1].RealtimePercentage);
                players[0].RealtimePercentage = percentage;
                players[1].RealtimePercentage = percentage;
            }

            InitializeSync();

            players[0].SyncMerge = false;
            players[1].SyncMerge = false;
            StopMerge();

            GotoTime(currentTime, true);
        }

        private void InitializeSync()
        {
            commonTimeline.Initialize(players[0], players[1]);
            currentTime = 0;
            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(commonTimeline.GetCommonTime(players[0], players[0].LocalTimeOriginPhysical));
            UpdateHairLines();

            log.Debug("Synchronization initialized.");
        }

        private void SetSyncPoint(bool intervalOnly)
        {
            log.DebugFormat("Resetting time origins. [0]:{0}, [1]{1}", players[0].LocalTime, players[1].LocalTime);
            players[0].LocalTimeOriginPhysical = players[0].LocalTime;
            players[1].LocalTimeOriginPhysical = players[1].LocalTime;

            commonTimeline.Initialize(players[0], players[1]);
            currentTime = commonTimeline.GetCommonTime(players[0], players[0].LocalTime);
            
            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(currentTime); 
        }

        private void GotoTime(long commonTime, bool allowUIUpdate)
        {
            GotoTime(players[0], commonTime, allowUIUpdate);
            GotoTime(players[1], commonTime, allowUIUpdate);

            UpdateHairLines();
        }
        
        private void GotoTime(PlayerScreen player, long commonTime, bool allowUIUpdate)
        {
            long localTime = commonTimeline.GetLocalTime(player, commonTime);
            localTime = Math.Max(0, localTime);

            if (player.LocalTime != localTime)
                player.GotoTime(localTime, allowUIUpdate);
        }

        private void UpdateHairLines()
        {
            long leftTime = commonTimeline.GetCommonTime(players[0], players[0].LocalTime);
            long rightTime = commonTimeline.GetCommonTime(players[1], players[1].LocalTime);

            view.UpdateHairline(leftTime, true);
            view.UpdateHairline(rightTime, false);
        }
        
        /// <summary>
        /// Force both players to align on a common time.
        /// Used if players may have moved independently from the common tracker.
        /// Should not be used while playback is active.
        /// </summary>
        private void AlignPlayers(bool catchup)
        {
            long leftTime = commonTimeline.GetCommonTime(players[0], players[0].LocalTime);
            long rightTime = commonTimeline.GetCommonTime(players[1], players[1].LocalTime);

            if (catchup)
                currentTime = Math.Max(leftTime, rightTime);
            else
                currentTime = Math.Min(leftTime, rightTime);

            log.DebugFormat("Aligning players to {0}.", currentTime / 1000);
            GotoTime(currentTime, true);
        }

        private void EnsureBothPlaying()
        {
            EnsurePlaying(0);
            EnsurePlaying(1);
        }

        /// <summary>
        /// Make sure a player is playing if it needs to but does not start it if it shouldn't.
        /// </summary>
        private void EnsurePlaying(int index)
        {
            if (!players[index].IsPlaying && !commonTimeline.IsOutOfBounds(players[index], currentTime))
                players[index].EnsurePlaying();
        }

        #endregion

        private PlayerScreen GetOtherPlayer(PlayerScreen player)
        {
            return player == players[0] ? players[1] : players[0];
        }

        private int GetPlayerIndex(PlayerScreen player)
        {
            return player == players[0] ? 0 : 1;
        }
    }
}
