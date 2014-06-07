using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;
using System.Drawing;
using Kinovea.Video.FFMpeg;
using System.ComponentModel;
using Kinovea.ScreenManager.Languages;
using System.IO;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Synchronization logic.
    /// </summary>
    public class DualPlayerController
    {
        #region Properties
        public CommonControlsPlayers View
        {
            get { return view; }
        }
        #endregion

        #region Members
        private bool active;
        private bool synching;
        private bool dynamicSynching;

        private CommonTimeline commonTimeline = new CommonTimeline();   
        private long currentTime;

        // Dual saving
        private string dualSaveFileName;
        private bool dualSaveCancelled;
        private bool dualSaveInProgress;
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private BackgroundWorker bgWorkerDualSave;
        private formProgressBar dualSaveProgressBar;

        private CommonControlsPlayers view = new CommonControlsPlayers();
        private List<PlayerScreen> players = new List<PlayerScreen>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public DualPlayerController()
        {
            view.Dock = DockStyle.Fill;

            view.PlayToggled += CCtrl_PlayToggled;
            view.GotoFirst += CCtrl_GotoFirst;
            view.GotoFirst += CCtrl_GotoFirst;
            view.GotoPrev += CCtrl_GotoPrev;
            view.GotoNext += CCtrl_GotoNext;
            view.GotoLast += CCtrl_GotoLast;
            view.SyncAsked += CCtrl_SyncAsked;
            view.MergeAsked += CCtrl_MergeAsked;
            view.PositionChanged += CCtrl_PositionChanged;
            view.DualSaveAsked += CCtrl_DualSaveAsked;
            view.DualSnapshotAsked += CCtrl_DualSnapshotAsked;
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

            // Inject launch settings sync point into actual screens.
            // We can't do this earlier because the sync points are reset to 0 during initialization.
            int recovered = 0;
            foreach (ScreenDescriptionPlayback description in LaunchSettingsManager.ScreenDescriptions)
            {
                PlayerScreen player = players.FirstOrDefault(p => p.Id == description.Id);
                if (player != null)
                {
                    player.LocalSyncTime = description.LocalSyncTime;
                    recovered++;
                }
            }

            if (recovered != 2)
                return;

            InitializeSyncFromCurrentPositions();
        }
        #endregion

        #region Players event handlers
        private void Player_PauseAsked(object sender, EventArgs e)
        {
            if (active && synching && view.Playing)
                Pause();
        }
        
        private void Player_SpeedChanged(object sender, EventArgs e)
        {
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            if (PreferencesManager.PlayerPreferences.SyncLockSpeed)
                GetOtherPlayer(player).RealtimePercentage = player.RealtimePercentage;
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
                    currentTime = commonTimeline.GetCommonTime(player, player.LocalTime);
                    Synchronize();
                }
                else if (!otherPlayer.IsPlaying)
                {
                    // Both players have completed a loop and are waiting.
                    currentTime = 0;
                    Synchronize();
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
                dynamicSynching = view.Playing;
                if (dynamicSynching)
                {
                    // Both players may have been moved independently from the common tracker.
                    // Reset common frame to the one most early in the common timeline.
                    long leftTime = commonTimeline.GetCommonTime(players[0], players[0].LocalTime);
                    long rightTime = commonTimeline.GetCommonTime(players[1], players[1].LocalTime);
                    currentTime = Math.Min(leftTime, rightTime);
                    GotoTime(currentTime, true);

                    Synchronize();
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
        private void CCtrl_DualSaveAsked(object sender, EventArgs e)
        {
            //DualSave();
        }
        private void CCtrl_DualSnapshotAsked(object sender, EventArgs e)
        {
            //DualSnapshot();
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
            player.PauseAsked += Player_PauseAsked;
            player.SpeedChanged += Player_SpeedChanged;
            player.ImageChanged += Player_ImageChanged;
        }
        private void RemoveEventHandlers(PlayerScreen player)
        {
            player.PauseAsked -= Player_PauseAsked;
            player.SpeedChanged -= Player_SpeedChanged;
            player.ImageChanged -= Player_ImageChanged;
        }

        #endregion

        #region Synchronization
        public void PrepareSync()
        {
            if (!active)
                return;

            synching = false;
            dynamicSynching = false;
            Pause();

            if (!players[0].Full || !players[1].Full)
                return;

            synching = true;
            players[0].Synched = true;
            players[1].Synched = true;

            InitializeSync();

            players[0].SyncMerge = false;
            players[1].SyncMerge = false;
            StopMerge();

            GotoTime(currentTime, true);
        }
        
        private void InitializeSync()
        {
            commonTimeline.Initialize(players[0], 0, players[1], 0);

            currentTime = 0;

            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(currentTime); 
        }

        private void InitializeSyncFromCurrentPositions()
        {
            commonTimeline.Initialize(players[0], players[0].LocalSyncTime, players[1], players[1].LocalSyncTime);

            currentTime = 0;

            view.SetupTrkFrame(0, commonTimeline.LastTime, currentTime);
            view.UpdateSyncPosition(commonTimeline.GetCommonTime(players[0], players[0].LocalSyncTime));
            UpdateHairLines();
        }

        private void SetSyncPoint(bool intervalOnly)
        {
            log.DebugFormat("Setting sync point. [0]:{0}, [1]{1}", players[0].LocalTime, players[1].LocalTime);

            commonTimeline.Initialize(players[0], players[0].LocalTime, players[1], players[1].LocalTime);

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
        
        private void Synchronize()
        {
            SynchronizePlayer(players[0]);
            SynchronizePlayer(players[1]);
        }

        private void SynchronizePlayer(PlayerScreen player)
        {
            if (!player.IsPlaying && !commonTimeline.IsOutOfBounds(player, currentTime))
                player.StartPlaying();
        }

        #endregion

        private PlayerScreen GetOtherPlayer(PlayerScreen player)
        {
            return player == players[0] ? players[1] : players[0];
        }

        #region Side by side video save
        private void DualSave()
        {
            // Create and save a composite video with side by side synchronized images.
            // If merge is active, just save one video.
            if (!synching)
                return;

            PlayerScreen ps1 = players[0];
            PlayerScreen ps2 = players[1];
            if (ps1 == null || ps2 == null)
                return;

            Pause();

            // Get file name from user.
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.dlgSaveVideoTitle;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
            dlgSave.FilterIndex = 1;
            dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(ps1.FilePath), Path.GetFileNameWithoutExtension(ps2.FilePath));

            if (dlgSave.ShowDialog() != DialogResult.OK)
                return;

            long memoCurrentTime = currentTime;
            dualSaveCancelled = false;
            dualSaveFileName = dlgSave.FileName;

            // Instanciate and configure the bgWorker.
            bgWorkerDualSave = new BackgroundWorker();
            bgWorkerDualSave.WorkerReportsProgress = true;
            bgWorkerDualSave.WorkerSupportsCancellation = true;
            bgWorkerDualSave.DoWork += bgWorkerDualSave_DoWork;
            bgWorkerDualSave.ProgressChanged += bgWorkerDualSave_ProgressChanged;
            bgWorkerDualSave.RunWorkerCompleted += bgWorkerDualSave_RunWorkerCompleted;

            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            dualSaveInProgress = true;
            ps1.DualSaveInProgress = true;
            ps2.DualSaveInProgress = true;

            // Create the progress bar and launch the worker.
            dualSaveProgressBar = new formProgressBar(true);
            dualSaveProgressBar.Cancel = dualSave_CancelAsked;
            bgWorkerDualSave.RunWorkerAsync();
            dualSaveProgressBar.ShowDialog();

            // If cancelled, delete temporary file.
            if (dualSaveCancelled)
                DeleteTemporaryFile(dualSaveFileName);

            // Reset to where we were.
            dualSaveInProgress = false;
            ps1.DualSaveInProgress = false;
            ps2.DualSaveInProgress = false;
            currentTime = memoCurrentTime;
            GotoTime(currentTime, true);
        }
        private void bgWorkerDualSave_DoWork(object sender, DoWorkEventArgs e)
        {
            /*
            // This is executed in Worker Thread space. (Do not call any UI methods)

            // For each position: get both images, compute the composite, save it to the file.
            // If blending is activated, only get the image from left screen, since it already contains both images.
            log.Debug("Saving side by side video.");

            if (!synching)
                return;

            PlayerScreen ps1 = players[0];
            PlayerScreen ps2 = players[1];
            if (ps1 == null && ps2 == null)
                return;

            // Todo: get frame interval from one of the videos.

            // Get first frame outside the loop, to be able to set video size.
            commonTime = 0;
            GotoFrame(commonTime, true);

            Bitmap img1 = ps1.GetFlushedImage();
            Bitmap img2 = null;
            Bitmap composite;
            if (!view.Merging)
            {
                img2 = ps2.GetFlushedImage();
                composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
            }
            else
            {
                composite = img1;
            }

            log.Debug(String.Format("Composite size: {0}.", composite.Size));

            // Configure a fake InfoVideo to setup image size.
            VideoInfo vi = new VideoInfo { OriginalSize = composite.Size };
            SaveResult result = videoFileWriter.OpenSavingContext(dualSaveFileName, vi, -1, false);

            if (result != SaveResult.Success)
            {
                e.Result = 2;
                return;
            }

            videoFileWriter.SaveFrame(composite);

            img1.Dispose();
            if (!view.Merging)
            {
                img2.Dispose();
                composite.Dispose();
            }

            // Loop all remaining frames in static sync mode, but without refreshing the UI.
            while (commonTime < maxFrame && !dualSaveCancelled)
            {
                commonTime++;

                if (bgWorkerDualSave.CancellationPending)
                {
                    e.Result = 1;
                    dualSaveCancelled = true;
                    break;
                }

                // Move both playheads and get the composite image.
                GotoNext(commonTime, false);

                img1 = ps1.GetFlushedImage();
                composite = img1;
                if (!view.Merging)
                {
                    img2 = ps2.GetFlushedImage();
                    composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
                }

                videoFileWriter.SaveFrame(composite);

                img1.Dispose();
                if (!view.Merging)
                {
                    img2.Dispose();
                    composite.Dispose();
                }

                int percent = (int)(((double)(commonTime + 1) / maxFrame) * 100);
                bgWorkerDualSave.ReportProgress(percent);
            }

            if (!dualSaveCancelled)
                e.Result = 0;
            */
        }
        private void bgWorkerDualSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (bgWorkerDualSave.CancellationPending)
                return;

            dualSaveProgressBar.Update(Math.Min(e.ProgressPercentage, 100), 100, true);
        }
        private void bgWorkerDualSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dualSaveProgressBar.Close();
            dualSaveProgressBar.Dispose();

            if (!dualSaveCancelled && (int)e.Result != 1)
                videoFileWriter.CloseSavingContext((int)e.Result == 0);

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }
        private void dualSave_CancelAsked(object sender, EventArgs e)
        {
            // This will simply set BgWorker.CancellationPending to true,
            // which we check periodically in the saving loop.
            // This will also end the bgWorker immediately,
            // maybe before we check for the cancellation in the other thread. 
            videoFileWriter.CloseSavingContext(false);
            dualSaveCancelled = true;
            bgWorkerDualSave.CancelAsync();
        }
        private void DeleteTemporaryFile(string filename)
        {
            log.Debug("Side by side video saving cancelled. Deleting temporary file.");
            if (!File.Exists(filename))
                return;

            try
            {
                File.Delete(filename);
            }
            catch (Exception exp)
            {
                log.Error("Error while deleting temporary file.");
                log.Error(exp.Message);
                log.Error(exp.StackTrace);
            }
        }
        #endregion

        #region Side by side snapshot
        private void DualSnapshot()
        {
            // Retrieve current images and create a composite out of them.
            if (!synching)
                return;

            PlayerScreen ps1 = players[0];
            PlayerScreen ps2 = players[1];
            if (ps1 == null || ps2 == null)
                return;

            Pause();

            // get a copy of the images with drawings flushed on.
            Bitmap leftImage = ps1.GetFlushedImage();
            Bitmap rightImage = ps2.GetFlushedImage();
            Bitmap composite = ImageHelper.GetSideBySideComposite(leftImage, rightImage, false, true);

            // Configure Save dialog.
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = ScreenManagerLang.dlgSaveFilter;
            dlgSave.FilterIndex = 1;
            dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(ps1.FilePath), Path.GetFileNameWithoutExtension(ps2.FilePath));

            // Launch the dialog and save image.
            if (dlgSave.ShowDialog() == DialogResult.OK)
                ImageHelper.Save(dlgSave.FileName, composite);

            composite.Dispose();
            leftImage.Dispose();
            rightImage.Dispose();

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }
        #endregion
    }
}
