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

        private long syncLag; 	                // Sync Lag in Frames, for static sync.
        private long syncLagMilliseconds;		// Sync lag in Milliseconds, for dynamic sync.

        // Static Sync Positions
        private long currentFrame = 0;            // Current frame in trkFrame...
        private long leftSyncFrame = 0;           // Sync reference in the left video
        private long rightSyncFrame = 0;          // Sync reference in the right video
        private long maxFrame = 0;                // Max du trkFrame

        // Dynamic Sync Flags.
        private bool rightIsStarting = false;    // true when the video is between [0] and [1] frames.
        private bool leftIsStarting = false;
        private bool leftIsCatchingUp = false;   // CatchingUp is when the video is the only one left running,
        private bool rightIsCatchingUp = false;  // heading towards end, the other video is waiting the lag.

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
            dynamicSynching = false;
            view.Pause();
        }
        public void StopMerge()
        {
            view.StopMerge();
        }
        public void SwapSync()
        {
            if (!synching)
                return;

            long temp = leftSyncFrame;
            leftSyncFrame = rightSyncFrame;
            rightSyncFrame = temp;

            ResetDynamicSyncFlags();

            SetSyncPoint(true);
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
            {
                int otherPlayer = player == players[0] ? 1 : 0;
                players[otherPlayer].RealtimePercentage = player.RealtimePercentage;
            }

            SetSyncPoint(true);
        }
        private void Player_ImageChanged(object sender, EventArgs<Bitmap> e)
        {
            if (!active || !synching)
                return;

            PlayerScreen player = sender as PlayerScreen;
            if (player == null)
                return;

            if (dynamicSynching)
                DynamicSync();

            // Transfer the caller's image to the other screen.
            // The image has been cloned and transformed in the caller screen.
            if (!view.Merging || e.Value == null)
                return;

            foreach (PlayerScreen p in players)
            {
                if (p != player)
                    p.SetSyncMergeImage(e.Value, !dualSaveInProgress);
            }
        }
        #endregion

        #region View event handlers
        private void CCtrl_PlayToggled(object sender, EventArgs e)
        {
            if (synching)
            {
                if (view.Playing)
                {
                    // On play, simply launch the dynamic sync.
                    // It will handle which video can start right away.
                    StartDynamicSync();
                }
                else
                {
                    dynamicSynching = false;
                    leftIsStarting = false;
                    rightIsStarting = false;
                }
            }

            // On stop, propagate the call to screens.
            if (!view.Playing)
            {
                EnsurePause(0);
                EnsurePause(1);
            }
        }
        private void CCtrl_GotoFirst(object sender, EventArgs e)
        {
            Pause();

            if (synching)
            {
                currentFrame = 0;
                OnCommonPositionChanged(currentFrame, true);
                UpdateTrkFrame(currentFrame);
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
                if (currentFrame > 0)
                {
                    currentFrame--;
                    OnCommonPositionChanged(currentFrame, true);
                    UpdateTrkFrame(currentFrame);
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
                if (currentFrame < maxFrame)
                {
                    currentFrame++;
                    OnCommonPositionChanged(-1, true);
                    UpdateTrkFrame(currentFrame);
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
                currentFrame = maxFrame;
                OnCommonPositionChanged(currentFrame, true);
                UpdateTrkFrame(currentFrame);

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

            log.Debug("Sync point change.");
            SetSyncPoint(false);
            SetSyncLimits();
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_MergeAsked(object sender, EventArgs e)
        {
            if (!synching)
                return;

            log.Debug(String.Format("SyncMerge videos is now {0}", view.Merging.ToString()));

            // This will also do a full refresh, and triggers Player_ImageChanged().
            players[0].SyncMerge = view.Merging;
            players[1].SyncMerge = view.Merging;
        }
        private void CCtrl_PositionChanged(object sender, TimeEventArgs e)
        {
            // Manual static sync.
            if (!synching)
                return;

            Pause();
            EnsurePause(0);
            EnsurePause(1);

            currentFrame = e.Time;
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_DualSaveAsked(object sender, EventArgs e)
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

            long iCurrentFrame = currentFrame;
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
            currentFrame = iCurrentFrame;
            OnCommonPositionChanged(currentFrame, true);
        }
        private void CCtrl_DualSnapshotAsked(object sender, EventArgs e)
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
        public void PrepareSync(bool initialization)
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

            if (initialization)
            {
                log.Debug("PrepareSync() - Initialization (reset of sync point).");
                
                // Static Sync
                rightSyncFrame = 0;
                leftSyncFrame = 0;
                syncLag = 0;
                currentFrame = 0;

                players[0].SyncPosition = 0;
                players[1].SyncPosition = 0;
                UpdateSyncPosition(currentFrame);

                // Dynamic Sync
                ResetDynamicSyncFlags();

                // Sync Merging
                players[0].SyncMerge = false;
                players[1].SyncMerge = false;
                StopMerge();
            }

            SetSyncLimits();
            OnCommonPositionChanged(currentFrame, true);
        }
        
        private void SetSyncPoint(bool intervalOnly)
        {
            //--------------------------------------------------------------------------------------------------
            // Registers the current position of each video as its sync frame. (Optional)
            // Computes the lag in common timestamps between positions.
            // Computes the lag in milliseconds between positions. (using current framerate of each video)
            // Update current common position.
            // (public only because accessed from the Swap command.)
            //--------------------------------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            // Par défaut les deux vidéos sont synchronisées sur {0}.
            // Le paramètre de synchro se lit comme suit : 
            // {+2} : La vidéo de droite à 2 frames d'avance sur celle de gauche.
            // {-4} : La vidéo de droite à 4 frames de retard.
            //
            // Si le décalage est positif, la vidéo de droite doit partir en premier.
            // La pause de terminaison dépend à la fois du paramètre de synchro et 
            // des durées (en frames) respectives des deux vidéos.
            //
            // Si _bIntervalOnly, on ne veut pas changer les frames de référence
            // (Généralement après une modification du framerate de l'une des vidéos ou swap)
            //----------------------------------------------------------------------------
            if (!synching)
                return;
            
            
            // Registers current positions.
            if (!intervalOnly)
            {
                // For timing label only
                players[0].SyncPosition = players[0].Position;
                players[1].SyncPosition = players[1].Position;

                leftSyncFrame = players[0].CurrentFrame;
                rightSyncFrame = players[1].CurrentFrame;

                log.Debug(String.Format("New Sync Points:[{0}][{1}], Sync lag:{2}", leftSyncFrame, rightSyncFrame, rightSyncFrame - leftSyncFrame));
            }

            // Sync Lag is expressed in frames.
            syncLag = rightSyncFrame - leftSyncFrame;

            // We need to recompute the lag in milliseconds because it can change even when 
            // the references positions don't change. For exemple when varying framerate (speed).
            long iLeftSyncMilliseconds = (long)(players[0].FrameInterval * leftSyncFrame);
            long iRightSyncMilliseconds = (long)(players[1].FrameInterval * rightSyncFrame);
            syncLagMilliseconds = iRightSyncMilliseconds - iLeftSyncMilliseconds;

            // Update common position (sign of m_iSyncLag might have changed.)
            currentFrame = syncLag > 0 ? rightSyncFrame : leftSyncFrame;

            UpdateSyncPosition(currentFrame);  // <-- expects timestamp ?
        }
        private void DynamicSync()
        {
            // This is where the dynamic sync is done.
            // It was used in timer loop at some point but now it's called directly.
            // When a screen finishes decoding its image, we call in here to verify if the other screen
            // needs to be started, paused, or something else.

            // Get each video positions in common timebase and milliseconds.
            // Figure if a restart or pause is needed, considering current positions.

            // When the user press the common play button, we just propagate the play to the screens.
            // The common timer is just set to try to be notified of each frame change.
            // It is not used to provoke frame change itself.
            // We just start and stop the players timers when we detect one of the video has reached the end,
            // to prevent it from auto restarting.

            // Glossary:
            // XIsStarting 	: currently on [0] but a Play was asked.
            // XIsCatchingUp 	: video is between [0] and the point where both video will be running. 

            if (!synching)
            {
                // This can happen when a screen is closed on the fly while synching.
                dynamicSynching = false;
                synching = false;
                Pause();
                return;
            }

            // L'ensemble de la supervision est réalisée en TimeStamps.
            // Seul les décision de lancer / arrêter sont établies par rapport
            // au temps auquel on est.

            long leftPosition = players[0].CurrentFrame;
            long rightPosition = players[1].CurrentFrame;
            long leftMilliseconds = (long)(leftPosition * players[0].FrameInterval);
            long rightMilliseconds = (long)(rightPosition * players[1].FrameInterval);

            //-----------------------------------------------------------------------
            // Dans cette fonction, on part du principe que les deux vidéos tournent.
            // Et on fait des 'Ensure Pause' quand nécessaire.
            // On évite les Ensure Play' car l'utilisateur a pu 
            // manuellement pauser une vidéo.
            //-----------------------------------------------------------------------
            #region [i][0]
            if (leftPosition > 0 && rightPosition == 0)
            {
                EnsurePlay(0);

                // Etat 4. [i][0]
                leftIsStarting = false;

                if (syncLag == 0)
                {
                    //-----------------------------------------------------
                    // La vidéo de droite 
                    // - vient de boucler et on doit attendre l'autre
                    // - est en train de repartir.
                    //-----------------------------------------------------
                    if (!rightIsStarting)
                    {
                        // Stop pour bouclage
                        EnsurePause(1);
                    }

                    currentFrame = leftPosition;
                }
                else if (syncLagMilliseconds > 0)
                {
                    // La vidéo de droite est sur 0 et doit partir en premier.
                    // Si elle n'est pas en train de repartir, c'est qu'on 
                    // doit attendre que la vidéo de gauche ait finit son tour.
                    if (!rightIsStarting)
                    {
                        EnsurePause(1);
                        currentFrame = leftPosition + syncLag;
                    }
                    else
                    {
                        currentFrame = leftPosition;
                    }
                }
                else if (syncLagMilliseconds < 0)
                {
                    // La vidéo de droite est sur 0, en train de prendre son retard.
                    // On la relance si celle de gauche a fait son décalage.

                    // Attention, ne pas relancer si celle de gauche est en fait en train de terminer son tour
                    if (!leftIsCatchingUp && !rightIsStarting)
                    {
                        EnsurePause(1);
                        currentFrame = leftPosition;
                    }
                    else if (leftMilliseconds > (-syncLagMilliseconds) - 24)
                    {
                        // La vidéo de gauche est sur le point de franchir le sync point.
                        // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                        // la vidéo qui est partie en premier...
                        EnsurePlay(1);
                        rightIsStarting = true;
                        leftIsCatchingUp = false;
                        currentFrame = leftPosition;
                    }
                    else
                    {
                        // La vidéo de gauche n'a pas encore fait son décalage.
                        // On ne force pas sa lecture. (Pause manuelle possible).
                        leftIsCatchingUp = true;
                        currentFrame = leftPosition;
                    }
                }
            }
            #endregion
            #region [0][0]
            else if (leftPosition == 0 && rightPosition == 0)
            {
                // Etat 1. [0][0]
                currentFrame = 0;

                // Les deux vidéos viennent de boucler ou sont en train de repartir.
                if (syncLag == 0)
                {
                    //---------------------
                    // Redemmarrage commun.
                    //---------------------
                    if (!leftIsStarting && !rightIsStarting)
                    {
                        EnsurePlay(0);
                        EnsurePlay(1);

                        rightIsStarting = true;
                        leftIsStarting = true;
                    }
                }
                else if (syncLagMilliseconds > 0)
                {
                    // Redemarrage uniquement de la vidéo de droite, 
                    // qui doit faire son décalage

                    EnsurePause(0);
                    EnsurePlay(1);
                    rightIsStarting = true;
                    rightIsCatchingUp = true;
                }
                else if (syncLagMilliseconds < 0)
                {
                    // Redemarrage uniquement de la vidéo de gauche, 
                    // qui doit faire son décalage

                    EnsurePlay(0);
                    EnsurePause(1);
                    leftIsStarting = true;
                    leftIsCatchingUp = true;
                }
            }
            #endregion
            #region [0][i]
            else if (leftPosition == 0 && rightPosition > 0)
            {
                // Etat [0][i]
                EnsurePlay(1);

                rightIsStarting = false;

                if (syncLag == 0)
                {
                    currentFrame = rightPosition;

                    //--------------------------------------------------------------------
                    // Configuration possible : la vidéo de gauche vient de boucler.
                    // On la stoppe en attendant le redemmarrage commun.
                    //--------------------------------------------------------------------
                    if (!leftIsStarting)
                    {
                        EnsurePause(0);
                    }
                }
                else if (syncLagMilliseconds > 0)
                {
                    // La vidéo de gauche est sur 0, en train de prendre son retard.
                    // On la relance si celle de droite a fait son décalage.

                    // Attention ne pas relancer si la vidéo de droite est en train de finir son tour
                    if (!rightIsCatchingUp && !leftIsStarting)
                    {
                        // La vidéo de droite est en train de finir son tour tandisque celle de gauche a déjà bouclé.
                        EnsurePause(0);
                        currentFrame = rightPosition;
                    }
                    else if (rightMilliseconds > syncLagMilliseconds - 24)
                    {
                        // La vidéo de droite est sur le point de franchir le sync point.
                        // les 24 ms supplémentaires sont pour tenir compte de l'inertie qu'à généralement
                        // la vidéo qui est partie en premier...
                        EnsurePlay(0);
                        leftIsStarting = true;
                        rightIsCatchingUp = false;
                        currentFrame = rightPosition;
                    }
                    else
                    {
                        // La vidéo de droite n'a pas encore fait son décalage.
                        // On ne force pas sa lecture. (Pause manuelle possible).
                        rightIsCatchingUp = true;
                        currentFrame = rightPosition;
                    }
                }
                else if (syncLagMilliseconds < 0)
                {
                    // La vidéo de gauche est sur 0 et doit partir en premier.
                    // Si elle n'est pas en train de repartir, c'est qu'on 
                    // doit attendre que la vidéo de droite ait finit son tour.
                    if (!leftIsStarting)
                    {
                        EnsurePause(0);
                        currentFrame = rightPosition + syncLag;
                    }
                    else
                    {
                        // Rare, les deux première frames de chaque vidéo n'arrivent pas en même temps
                        currentFrame = rightPosition;
                    }
                }
            }
            #endregion
            #region [i][i]
            else
            {
                // Etat [i][i]
                EnsurePlay(0);
                EnsurePlay(1);

                leftIsStarting = false;
                rightIsStarting = false;

                currentFrame = Math.Max(leftPosition, rightPosition);
            }
            #endregion

            // Update position for trkFrame.
            object[] parameters = new object[] { currentFrame };

            // Note: do we need to begin invoke here ?
            //view.BeginInvoke(view.delegateUpdateTrackerFrame, parameters);
            UpdateTrkFrame(currentFrame);

            //log.Debug(String.Format("Tick:[{0}][{1}], Starting:[{2}][{3}], Catching up:[{4}][{5}]", iLeftPosition, iRightPosition, m_bLeftIsStarting, m_bRightIsStarting, m_bLeftIsCatchingUp, m_bRightIsCatchingUp));
        }
        private void EnsurePause(int screenIndex)
        {
            if (screenIndex < players.Count)
            {
                if (players[screenIndex].IsPlaying)
                    players[screenIndex].view.OnButtonPlay();
            }
            else
            {
                synching = false;
                Pause();
            }
        }
        private void EnsurePlay(int screenIndex)
        {
            if (screenIndex < players.Count)
            {
                if (!players[screenIndex].IsPlaying)
                    players[screenIndex].view.OnButtonPlay();
            }
            else
            {
                synching = false;
                Pause();
            }
        }
        private void StartDynamicSync()
        {
            dynamicSynching = true;
            DynamicSync();
        }
        private void ResetDynamicSyncFlags()
        {
            rightIsStarting = false;
            leftIsStarting = false;
            leftIsCatchingUp = false;
            rightIsCatchingUp = false;
        }
        private void SyncCatch()
        {
            // We sync back the videos.
            // Used when one video has been moved individually.
            log.Debug("SyncCatch() called.");
            long leftFrame = players[0].CurrentFrame;
            long rightFrame = players[1].CurrentFrame;

            if (syncLag > 0)
            {
                // Right video goes ahead.
                if (leftFrame + syncLag == currentFrame || (currentFrame < syncLag && leftFrame == 0))
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    currentFrame = rightFrame;
                }
                else if (rightFrame == currentFrame)
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    currentFrame = leftFrame + syncLag;
                }
                else
                {
                    // Both videos were moved.
                    currentFrame = leftFrame + syncLag;
                }
            }
            else
            {
                // Left video goes ahead.
                if (rightFrame - syncLag == currentFrame || (currentFrame < -syncLag && rightFrame == 0))
                {
                    // Right video wasn't moved, we'll move it according to left video.
                    currentFrame = leftFrame;
                }
                else if (leftFrame == currentFrame)
                {
                    // Left video wasn't moved, we'll move it according to right video.
                    currentFrame = rightFrame - syncLag;
                }
                else
                {
                    // Both videos were moved.
                    currentFrame = leftFrame;
                }
            }

            OnCommonPositionChanged(currentFrame, true);
            UpdateTrkFrame(currentFrame);
        }
        private void OnCommonPositionChanged(long frame, bool allowUIUpdate)
        {
            //------------------------------------------------------------------------------
            // This is where the "static sync" is done.
            // Updates each video to reflect current common position.
            // Used to handle GotoNext, GotoPrev, trkFrame, etc.
            // 
            // note: m_iSyncLag and _iFrame are expressed in frames.
            //------------------------------------------------------------------------------

            //log.Debug(String.Format("Static Sync, common position changed to {0}",_iFrame));

            // Get corresponding position in each video, in frames
            long leftFrame = 0;
            long rightFrame = 0;

            if (frame >= 0)
            {
                if (syncLag > 0)
                {
                    // Right video must go ahead.

                    rightFrame = frame;
                    leftFrame = frame - syncLag;
                    if (leftFrame < 0)
                        leftFrame = 0;
                }
                else
                {
                    // Left video must go ahead.

                    leftFrame = frame;
                    rightFrame = frame - (-syncLag);
                    if (rightFrame < 0)
                        rightFrame = 0;
                }

                // Force positions.
                players[0].GotoFrame(leftFrame, allowUIUpdate);
                players[1].GotoFrame(rightFrame, allowUIUpdate);
            }
            else
            {
                // Special case for ++.
                if (syncLag > 0)
                {
                    // Right video must go ahead.
                    players[1].GotoNextFrame(allowUIUpdate);

                    if (currentFrame > syncLag)
                        players[0].GotoNextFrame(allowUIUpdate);
                }
                else
                {
                    // Left video must go ahead.
                    players[0].GotoNextFrame(allowUIUpdate);

                    if (currentFrame > -syncLag)
                        players[1].GotoNextFrame(allowUIUpdate);
                }
            }
        }
        private void SetSyncLimits()
        {
            //-----------------------------------------------------------------------------------
            // Computes the real max of the trkFrame, considering the lag and original durations.
            // Updates trkFrame bounds, expressed in *Frames*.
            // impact : m_iMaxFrame.
            //-----------------------------------------------------------------------------------
            log.Debug("SetSyncLimits() called.");
            long leftEstimatedFrames = players[0].EstimatedFrames;
            long rightEstimatedFrames = players[1].EstimatedFrames;

            if (syncLag > 0)
            {
                // Lag is positive. Right video starts first and its duration stay the same as original.
                // Left video has to wait for an ammount of time.

                // Check if lag is still valid. (?) Why is this needed ?
                if (syncLag > rightEstimatedFrames)
                    syncLag = 0;

                leftEstimatedFrames += syncLag;
            }
            else
            {
                // Lag is negative. Left video starts first and its duration stay the same as original.
                // Right video has to wait for an ammount of time.

                // Get Lag in frames of right video
                //int iSyncLagFrames = ((PlayerScreen)screenList[1]).NormalizedToFrame(m_iSyncLag);

                // Check if lag is still valid.(?)
                if (-syncLag > leftEstimatedFrames)
                    syncLag = 0;

                rightEstimatedFrames += (-syncLag);
            }

            maxFrame = (int)Math.Max(leftEstimatedFrames, rightEstimatedFrames);
            SetupTrkFrame(0, maxFrame, currentFrame);

            log.DebugFormat("m_iSyncLag:{0}, m_iSyncLagMilliseconds:{1}, MaxFrames:{2}", syncLag, syncLagMilliseconds, maxFrame);
        }
        #endregion

        #region Side by side saving
        private void bgWorkerDualSave_DoWork(object sender, DoWorkEventArgs e)
        {
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
            currentFrame = 0;
            OnCommonPositionChanged(currentFrame, false);

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
            while (currentFrame < maxFrame && !dualSaveCancelled)
            {
                currentFrame++;

                if (bgWorkerDualSave.CancellationPending)
                {
                    e.Result = 1;
                    dualSaveCancelled = true;
                    break;
                }

                // Move both playheads and get the composite image.
                OnCommonPositionChanged(-1, false);
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

                int percent = (int)(((double)(currentFrame + 1) / maxFrame) * 100);
                bgWorkerDualSave.ReportProgress(percent);
            }

            if (!dualSaveCancelled)
                e.Result = 0;
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
    }
}
