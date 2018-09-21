#region License
/*
Copyright © Joan Charmant 2008-2009.
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

#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

using Kinovea.Base;
using Kinovea.ScreenManager.Languages;
using Kinovea.ScreenManager.Properties;
using Kinovea.Services;
using Kinovea.Video;
using System.Globalization;

#endregion

namespace Kinovea.ScreenManager
{
    public partial class PlayerScreenUserInterface : KinoveaControl
    {
        #region Enums
        private enum PlayingMode
        {
            Once,
            Loop,
            Bounce
        }
        #endregion

        #region Events
        public event EventHandler CloseAsked;
        public event EventHandler SetAsActiveScreen;
        public event EventHandler SpeedChanged;
        public event EventHandler KVAImported;
        public event EventHandler PauseAsked;
        public event EventHandler ResetAsked;
        public event EventHandler<EventArgs<bool>> SelectionChanged;
        public event EventHandler<EventArgs<Bitmap>> ImageChanged;
        public event EventHandler<TimeEventArgs> KeyframeAdding;
        public event EventHandler<KeyframeEventArgs> KeyframeDeleting;
        public event EventHandler<DrawingEventArgs> DrawingAdding;
        public event EventHandler<DrawingEventArgs> DrawingDeleting;
        public event EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemAdding;
        public event EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemDeleting;
        public event EventHandler<TrackableDrawingEventArgs> TrackableDrawingAdded;
        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        public event EventHandler<EventArgs<AbstractDrawing>> DataAnalysisAsked; 
        #endregion
        
        #region Commands encapsulating domain logic implemented in the presenter.
        public ToggleCommand ToggleTrackingCommand { get; set; }
        public RelayCommand<VideoFrame> TrackDrawingsCommand { get; set; }
        #endregion
        
        #region Properties
        public bool IsCurrentlyPlaying 
        {
            get { return m_bIsCurrentlyPlaying; }
        }
        public bool InteractiveFiltering 
        {
            get 
            { 
                return m_InteractiveEffect != null && 
                       m_InteractiveEffect.Draw != null && 
                       m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching; 
            }
        }
        public double FrameInterval 
        {
            get 
            {
                return timeMapper.GetInterval(sldrSpeed.Value);
            }
        }
        public double RealtimePercentage
        {
            get 
            { 
                return timeMapper.GetRealtimeMultiplier(sldrSpeed.Value) * 100;
            }
            set
            {
                // This happens only in the context of synching 
                // when the other video changed its speed percentage (user or forced).
                // We must NOT trigger the SpeedChanged event here, or it will impact the other screen in an infinite loop.
                
                slowMotion = value * m_FrameServer.Metadata.HighSpeedFactor / 100;
                sldrSpeed.Value = timeMapper.GetInputFromSlowMotion(slowMotion);
                sldrSpeed.Invalidate();

                // Reset timer with new value.
                if (m_bIsCurrentlyPlaying)
                {
                    StopMultimediaTimer();
                    StartMultimediaTimer(GetPlaybackFrameInterval());
                }

                UpdateSpeedLabel();
            }
        }
        public bool Synched
        {
            //get { return m_bSynched; }
            set
            {
                m_bSynched = value;
                
                if(!m_bSynched)
                {
                    m_iSyncPosition = 0;
                    trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
                    trkFrame.Invalidate();
                    UpdateCurrentPositionLabel();
                    
                    m_bSyncMerge = false;
                    if(m_SyncMergeImage != null)
                        m_SyncMergeImage.Dispose();
                }
                
                btnPlayingMode.Enabled = !m_bSynched;
            }
        }
        
        public long LocalSyncTimestamp
        {
            // The absolute ts of the sync point for this video.
            get 
            { 
                return m_iSyncPosition; 
            }

            set
            {
                m_iSyncPosition = value;
                trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
                trkFrame.Invalidate();
                UpdateCurrentPositionLabel();
            }
        }
        
        public long LocalTime
        {
            get
            {
                return TimestampToRealtime(m_iCurrentPosition - m_iSelStart);
            }
        }

        public long LocalLastTime
        {
            get 
            {
                return TimestampToRealtime(m_iSelEnd - m_iSelStart);
            }
        }

        public long LocalFrameTime
        {
            get
            {
                return TimestampToRealtime(m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
            }
            
        }

        public long LocalSyncTime
        {
            get 
            {
                return TimestampToRealtime(m_iSyncPosition - m_iSelStart);
            }
        }
        

        public bool SyncMerge
        {
            // Idicates whether we should draw the other screen image on top of this one.
            get { return m_bSyncMerge; }
            set
            {
                m_bSyncMerge = value;
                
                m_FrameServer.ImageTransform.FreeMove = m_bSyncMerge;
                
                if(!m_bSyncMerge && m_SyncMergeImage != null)
                {
                    m_SyncMergeImage.Dispose();
                }
                
                DoInvalidate();
            }
        }
        public bool DualSaveInProgress
        {
            set { m_DualSaveInProgress = value; }
        }
        #endregion

        #region Members
        private FrameServerPlayer m_FrameServer;
        
        // Playback current state
        private bool m_bIsCurrentlyPlaying;
        private int m_iFramesToDecode = 1;
        private uint m_IdMultimediaTimer;
        private PlayingMode m_ePlayingMode = PlayingMode.Loop;
        private bool m_bIsBusyRendering;
        private int m_RenderingDrops;
        private object m_TimingSync = new object();

        // Timing
        private TimeMapper timeMapper = new TimeMapper();
        private double slowMotion = 1;

        // Synchronisation
        private bool m_bSynched;
        private long m_iSyncPosition;
        private bool m_bSyncMerge;
        private Bitmap m_SyncMergeImage;
        private ColorMatrix m_SyncMergeMatrix = new ColorMatrix();
        private ImageAttributes m_SyncMergeImgAttr = new ImageAttributes();
        private float m_SyncAlpha = 0.5f;
        private bool m_DualSaveInProgress;
        private bool saveInProgress;
        
        // Image
        private ViewportManipulator m_viewportManipulator = new ViewportManipulator();
        private bool m_fill;
        private double m_lastUserStretch = 1.0f;
        private bool m_bShowImageBorder;
        private bool m_bManualSqueeze = true; // If it's allowed to manually reduce the rendering surface under the aspect ratio size.
        private static readonly Pen m_PenImageBorder = Pens.SteelBlue;
        private static readonly Size m_MinimalSize = new Size(160,120);
        private bool m_bEnableCustomDecodingSize = true;
        
        // Selection (All values in TimeStamps)
        // trkSelection.minimum and maximum are also in absolute timestamps.
        private long m_iTotalDuration = 100;
        private long m_iSelStart;          	// Valeur absolue, par défaut égale à m_iStartingPosition. (pas 0)
        private long m_iSelEnd = 99;          // Value absolue
        private long m_iSelDuration = 100;
        private long m_iCurrentPosition;    	// Valeur absolue dans l'ensemble des timestamps.
        private long m_iStartingPosition;   	// Valeur absolue correspond au timestamp de la première frame.
        private bool m_bHandlersLocked;
        
        // Keyframes, Drawings, etc.
        private List<KeyframeBox> thumbnails = new List<KeyframeBox>();
        private int m_iActiveKeyFrameIndex = -1;	// The index of the keyframe we are on, or -1 if not a KF.
        private AbstractDrawingTool m_ActiveTool;
        private DrawingToolPointer m_PointerTool;
        
        private formKeyframeComments m_KeyframeCommentsHub;
        private bool m_bDocked = true;
        private bool m_bTextEdit;
        private PointF m_DescaledMouse;    // The current mouse point expressed in the original image size coordinates.

        // Others
        private NativeMethods.TimerCallback m_TimerCallback;
        private ScreenDescriptionPlayback m_LaunchDescription;
        private InteractiveEffect m_InteractiveEffect;
        private const float m_MaxZoomFactor = 6.0F;
        private const int m_MaxRenderingDrops = 6;
        private const int m_MaxDecodingDrops = 6;
        private System.Windows.Forms.Timer m_DeselectionTimer = new System.Windows.Forms.Timer();
        private MessageToaster m_MessageToaster;
        private bool m_Constructed;
        
        #region Context Menus
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuDirectTrack = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPasteDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPlayPause = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSavePic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopyPic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPastePic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();

        private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
        private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSetStyleAsDefault = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAlwaysVisible = new ToolStripMenuItem();
        private ToolStripMenuItem mnuConfigureOpacity = new ToolStripMenuItem();
        private ToolStripMenuItem mnuGotoKeyframe = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDrawingTracking = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDrawingTrackingConfigure = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDrawingTrackingStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDrawingTrackingStop = new ToolStripMenuItem();
        private ToolStripSeparator mnuSepDrawing = new ToolStripSeparator();
        private ToolStripSeparator mnuSepDrawing2 = new ToolStripSeparator();
        private ToolStripSeparator mnuSepDrawing3 = new ToolStripSeparator();
        private ToolStripMenuItem mnuCutDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopyDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteDrawing = new ToolStripMenuItem();
        
        private ContextMenuStrip popMenuTrack = new ContextMenuStrip();
        private ToolStripMenuItem mnuRestartTracking = new ToolStripMenuItem();
        private ToolStripMenuItem mnuStopTracking = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteTrajectory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteEndOfTrajectory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuConfigureTrajectory = new ToolStripMenuItem();
        
        private ContextMenuStrip popMenuChrono = new ContextMenuStrip();
        private ToolStripMenuItem mnuChronoStart = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChronoStop = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChronoHide = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChronoCountdown = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChronoDelete = new ToolStripMenuItem();
        private ToolStripMenuItem mnuChronoConfigure = new ToolStripMenuItem();
        
        private ContextMenuStrip popMenuMagnifier = new ContextMenuStrip();
        private List<ToolStripMenuItem> maginificationMenus = new List<ToolStripMenuItem>();
        private ToolStripMenuItem mnuMagnifierTrack = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMagnifierDirect = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMagnifierQuit = new ToolStripMenuItem();
        #endregion

        private ToolStripButton m_btnAddKeyFrame;
        private ToolStripButton m_btnShowComments;
        private ToolStripButton m_btnToolPresets;
        private Infobar infobar = new Infobar();
        
        private DropWatcher m_DropWatcher = new DropWatcher();
        private TimeWatcher m_TimeWatcher = new TimeWatcher();
        private LoopWatcher m_LoopWatcher = new LoopWatcher();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreenUserInterface(FrameServerPlayer _FrameServer, DrawingToolbarPresenter drawingToolbarPresenter)
        {
            log.Debug("Constructing the PlayerScreen user interface.");
            
            m_FrameServer = _FrameServer;
            
            m_FrameServer.Metadata = new Metadata(m_FrameServer.HistoryStack, m_FrameServer.TimeStampsToTimecode);
            m_FrameServer.Metadata.KVAImported += (s, e) => AfterKVAImported();
            m_FrameServer.Metadata.KeyframeAdded += (s, e) => AfterKeyframeAdded(e.KeyframeId);
            m_FrameServer.Metadata.KeyframeDeleted += (s, e) => AfterKeyframeDeleted();
            m_FrameServer.Metadata.DrawingAdded += (s, e) => AfterDrawingAdded(e.Drawing);
            m_FrameServer.Metadata.DrawingModified += (s, e) => AfterDrawingModified(e.Drawing);
            m_FrameServer.Metadata.DrawingDeleted += (s, e) => AfterDrawingDeleted();
            m_FrameServer.Metadata.MultiDrawingItemAdded += (s, e) => AfterMultiDrawingItemAdded();
            m_FrameServer.Metadata.MultiDrawingItemDeleted += (s, e) => AfterMultiDrawingItemDeleted();

            InitializeComponent();
            InitializeInfobar();
            InitializeDrawingTools(drawingToolbarPresenter);
            BuildContextMenus();
            AfterSyncAlphaChange();
            m_MessageToaster = new MessageToaster(pbSurfaceScreen);
            
            // Most members and controls should be initialized with the right value.
            // So we don't need to do an extra ResetData here.
            
            // Controls that renders differently between run time and design time.
            this.Dock = DockStyle.Fill;
            ShowHideRenderingSurface(false);
            SetupPrimarySelectionPanel();
            SetupKeyframeCommentsHub();
            pnlThumbnails.Controls.Clear();
            thumbnails.Clear();
            DockKeyframePanel(true);

            m_TimerCallback = MultimediaTimer_Tick;
            m_DeselectionTimer.Interval = 10000;
            m_DeselectionTimer.Tick += DeselectionTimer_OnTick;

            sldrSpeed.Minimum = 0;
            sldrSpeed.Maximum = 1000;
            timeMapper.SetInputRange(sldrSpeed.Minimum, sldrSpeed.Maximum);
            timeMapper.SetSlowMotionRange(0, 2);
            slowMotion = 1;
            sldrSpeed.Initialize(timeMapper.GetInputFromSlowMotion(slowMotion));

            EnableDisableActions(false);

            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("PlayerScreen");
        }
        
        #endregion
        
        #region Public Methods
        public void ResetToEmptyState()
        {
            // Called when we load a new video over an already loaded screen.
            // also recalled if the video loaded but the first frame cannot be displayed.

            log.Debug("Reset screen to empty state.");
            
            // 1. Reset all data.
            m_FrameServer.Unload();
            ResetData();

            // 2. Reset all interface.
            ShowHideRenderingSurface(false);
            SetupPrimarySelectionPanel();
            ClearKeyframeBoxes();
            DockKeyframePanel(true);
            UpdateFramesMarkers();
            trkFrame.UpdateSyncPointMarker(m_iSyncPosition);
            trkFrame.Invalidate();
            EnableDisableAllPlayingControls(true);
            EnableDisableDrawingTools(true);
            EnableDisableSnapshot(true);
            buttonPlay.Image = Player.flatplay;
            slowMotion = 1;
            sldrSpeed.Force(timeMapper.GetInputFromSlowMotion(slowMotion));
            sldrSpeed.Enabled = false;
            m_KeyframeCommentsHub.Hide();
            UpdatePlayingModeButton();
            m_LaunchDescription = null;
            infobar.Visible = false;

            if (ResetAsked != null)
                ResetAsked(this, EventArgs.Empty);
        }
        private void ClearKeyframeBoxes()
        {
            for (int i = thumbnails.Count - 1; i >= 0; i--)
            {
                KeyframeBox thumbnail = thumbnails[i];
                
                thumbnail.CloseThumb -= ThumbBoxClose;
                thumbnail.ClickThumb -= ThumbBoxClick;
                thumbnail.ClickInfos -= ThumbBoxInfosClick;
                    
                thumbnails.Remove(thumbnail);
                pnlThumbnails.Controls.Remove(thumbnail);
                thumbnail.Dispose();
            }
        }
        public void SetLaunchDescription(ScreenDescriptionPlayback description)
        {
            m_LaunchDescription = description;
        }
        public void EnableDisableActions(bool _bEnable)
        {
            // Called back after a load error.
            // Prevent any actions.
            if(!_bEnable)
                DisablePlayAndDraw();
            
            EnableDisableSnapshot(_bEnable);
            EnableDisableDrawingTools(_bEnable);
            
            if(_bEnable && m_FrameServer.Loaded && m_FrameServer.VideoReader.IsSingleFrame)
                EnableDisableAllPlayingControls(false);
            else
                EnableDisableAllPlayingControls(_bEnable);				
        }
        public int PostLoadProcess()
        {
            //---------------------------------------------------------------------------
            // Configure the interface according to he video and try to read first frame.
            // Called from CommandLoadMovie when VideoFile.Load() is successful.
            //---------------------------------------------------------------------------
            ShowNextFrame(-1, true);
            UpdatePositionUI();

            if (m_FrameServer.VideoReader.Current == null)
            {
                m_FrameServer.Unload();
                log.Error("First frame couldn't be loaded - aborting");
                return -1;
            }
            else if(m_iCurrentPosition < 0)
            {
                // First frame loaded but inconsistency. (Seen with some AVCHD)
                m_FrameServer.Unload();
                log.Error(String.Format("First frame loaded but negative timestamp ({0}) - aborting", m_iCurrentPosition));
                return -2;
            }
            
            //---------------------------------------------------------------------------------------
            // First frame loaded.
            //
            // We will now update the internal data of the screen ui and
            // set up the various child controls (like the timelines).
            // Call order matters.
            // Some bugs come from variations between what the file infos advertised and the reality.
            // We fix what we can with the help of data read from the first frame or 
            // from the analysis mode switch if successful.
            //---------------------------------------------------------------------------------------
            
            DoInvalidate();

            m_iStartingPosition = m_iCurrentPosition;
            m_iTotalDuration = m_FrameServer.VideoReader.Info.DurationTimeStamps;
            m_iSelStart = m_iStartingPosition;
            m_iSelEnd = m_FrameServer.VideoReader.WorkingZone.End;
            m_iSelDuration  = m_iTotalDuration;
            
            if(!m_FrameServer.VideoReader.CanChangeWorkingZone)
                EnableDisableWorkingZoneControls(false);

            // Update the control.
            // FIXME - already done in ImportSelectionToMemory ?
            SetupPrimarySelectionPanel();
            
            // Other various infos.
            m_FrameServer.SetupMetadata(true);
            m_FrameServer.Metadata.FullPath = m_FrameServer.VideoReader.FilePath;
            m_FrameServer.Metadata.SelectionStart = m_iSelStart;
            m_FrameServer.Metadata.SelectionEnd = m_iSelEnd;
            m_PointerTool.SetImageSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_viewportManipulator.Initialize(m_FrameServer.VideoReader);

            // Screen position and size.
            m_FrameServer.ImageTransform.SetReferenceSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_FrameServer.ImageTransform.ReinitZoom();
            SetUpForNewMovie();
            m_KeyframeCommentsHub.UserActivated = false;

            // Check for launch description and startup kva
            bool recoveredMetadata = false;
            if (m_LaunchDescription != null)
            {
                if (m_LaunchDescription.Id != Guid.Empty)
                {
                    m_FrameServer.Metadata.Recover(m_LaunchDescription.Id);
                    recoveredMetadata = true;
                }

                if (m_LaunchDescription.SpeedPercentage != (slowMotion * 100))
                    slowMotion = m_LaunchDescription.SpeedPercentage / 100.0;
            }
            else
            {
                string kvaFile = Path.Combine(Path.GetDirectoryName(m_FrameServer.VideoReader.FilePath), Path.GetFileNameWithoutExtension(m_FrameServer.VideoReader.FilePath) + ".kva");
                LookForLinkedAnalysis(kvaFile);

                string startupFile = Path.Combine(Software.SettingsDirectory, "playback.kva");
                LookForLinkedAnalysis(startupFile);
            }

            UpdateTimebase();
            UpdateFilenameLabel();

            sldrSpeed.Force(timeMapper.GetInputFromSlowMotion(slowMotion));
            sldrSpeed.Enabled = true;

            if (!recoveredMetadata)
                m_FrameServer.Metadata.CleanupHash();
            
            m_FrameServer.Metadata.StartAutosave();
            
            Application.Idle += PostLoad_Idle;
            
            return 0;
        }
        private void AfterKVAImported()
        {
            int firstOutOfRange = -1;
            int currentKeyframe = -1;
            long lastTimestamp = m_FrameServer.VideoReader.Info.FirstTimeStamp + m_FrameServer.VideoReader.Info.DurationTimeStamps;

            foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
            {
                currentKeyframe++;

                if (kf.Position < lastTimestamp)
                {
                    if (!kf.Initialized)
                        InitializeKeyframe(kf);
                }
                else if (firstOutOfRange < 0)
                {
                    firstOutOfRange = currentKeyframe;
                }
            }

            if (firstOutOfRange != -1)
                m_FrameServer.Metadata.Keyframes.RemoveRange(firstOutOfRange, m_FrameServer.Metadata.Keyframes.Count - firstOutOfRange);
            
            UpdateFilenameLabel();
            OrganizeKeyframes();
            if(m_FrameServer.Metadata.Count > 0)
                DockKeyframePanel(false);
            
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);

            double oldHSF = m_FrameServer.Metadata.HighSpeedFactor;
            double captureInterval = 1000 / m_FrameServer.Metadata.CalibrationHelper.CaptureFramesPerSecond;
            m_FrameServer.Metadata.HighSpeedFactor = m_FrameServer.Metadata.UserInterval / captureInterval;
            UpdateTimebase();

            if (oldHSF != m_FrameServer.Metadata.HighSpeedFactor)
            {
                slowMotion = 1;
                sldrSpeed.Force(timeMapper.GetInputFromSlowMotion(slowMotion));
            }
            
            m_FrameServer.SetupMetadata(false);
            ImportEditboxes();
            m_PointerTool.SetImageSize(m_FrameServer.Metadata.ImageSize);

            if (KVAImported != null)
                KVAImported(this, EventArgs.Empty);

            UpdateTimeLabels();
            DoInvalidate();
        }
        public void UpdateTimebase()
        {
            timeMapper.FileInterval = m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds;
            timeMapper.UserInterval = m_FrameServer.Metadata.UserInterval;
            timeMapper.CaptureInterval = timeMapper.UserInterval / m_FrameServer.Metadata.HighSpeedFactor;
        }
        public void UpdateTimeLabels()
        {
            UpdateSelectionLabels();
            UpdateCurrentPositionLabel();
            UpdateSpeedLabel();
            UpdateFilenameLabel();
        }
        public void UpdateWorkingZone(bool _bForceReload)
        {
            if (!m_FrameServer.Loaded)
                return;

            if(m_FrameServer.VideoReader.CanChangeWorkingZone)
            {
                StopPlaying();
                OnPauseAsked();
                VideoSection newZone = new VideoSection(m_iSelStart, m_iSelEnd);
                m_FrameServer.VideoReader.UpdateWorkingZone(newZone, _bForceReload, PreferencesManager.PlayerPreferences.WorkingZoneSeconds, PreferencesManager.PlayerPreferences.WorkingZoneMemory, ProgressWorker);
                ResizeUpdate(true);
            }
            
            // Reupdate back the locals as the reader uses more precise values.
            m_iCurrentPosition = m_iCurrentPosition + (m_FrameServer.VideoReader.WorkingZone.Start - m_iSelStart);
            m_iSelStart = m_FrameServer.VideoReader.WorkingZone.Start;
            m_iSelEnd = m_FrameServer.VideoReader.WorkingZone.End;
            m_iSelDuration = m_iSelEnd - m_iSelStart + m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;
            
            if(trkSelection.SelStart != m_iSelStart)
                trkSelection.SelStart = m_iSelStart;

            if(trkSelection.SelEnd != m_iSelEnd)
                trkSelection.SelEnd = m_iSelEnd;
                    
            trkFrame.Remap(m_iSelStart, m_iSelEnd);
            
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            
            UpdatePositionUI();
            UpdateSelectionLabels();
            OnPoke();
            OnSelectionChanged(true);
        }
        private void ProgressWorker(DoWorkEventHandler _doWork)
        {
            formProgressBar2 fpb = new formProgressBar2(true, false, _doWork);
            fpb.ShowDialog();
            fpb.Dispose();
        }
        public void DisplayAsActiveScreen(bool _bActive)
        {
            // Called from ScreenManager.
            ShowBorder(_bActive);
        }
        public void StopPlaying()
        {
            StopPlaying(true);
        }
        public void ForcePosition(long timestamp, bool allowUIUpdate)
        {
            m_iFramesToDecode = 1;
            StopPlaying();

            m_iCurrentPosition = timestamp;

            if (m_iCurrentPosition > m_iSelEnd)
                m_iCurrentPosition = m_iSelEnd;

            ShowNextFrame(m_iCurrentPosition, allowUIUpdate);

            if (allowUIUpdate)
            {
                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        public void ForceCurrentFrame(long frame, bool allowUIUpdate)
        {
            // Called during static sync.
            // Common position changed, we get a new frame to jump to.
            // target frame may be over the total.

            if (!m_FrameServer.Loaded)
                return;

            m_iFramesToDecode = 1;
            StopPlaying();
                
            if (frame == -1)
            {
                // Special case for +1 frame.
                if (m_iCurrentPosition < m_iSelEnd)
                {
                    ShowNextFrame(-1, allowUIUpdate);
                }
            }
            else
            {
                m_iCurrentPosition = frame * m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;
                m_iCurrentPosition += m_iSelStart;
                    
                if (m_iCurrentPosition > m_iSelEnd) 
                    m_iCurrentPosition = m_iSelEnd;
                    
                ShowNextFrame(m_iCurrentPosition, allowUIUpdate);
            }

            if(allowUIUpdate)
            {
                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        public void RefreshImage()
        {
            // For cases where surfaceScreen.Invalidate() is not enough.
            // Not needed if we are playing.
            if (m_FrameServer.Loaded && !m_bIsCurrentlyPlaying)
                ShowNextFrame(m_iCurrentPosition, true);
        }
        public void RefreshUICulture()
        {
            // Labels
            lblSelStartSelection.AutoSize = true;
            lblSelDuration.AutoSize = true;

            lblWorkingZone.Text = ScreenManagerLang.lblWorkingZone_Text;
            UpdateTimeLabels();
            
            RepositionSpeedControl();			
            ReloadTooltipsCulture();
            ReloadToolsCulture();
            ReloadMenusCulture();
            m_KeyframeCommentsHub.RefreshUICulture();

            // Because this method is called when we change the general preferences,
            // we can use it to update data too.
            
            // Keyframes positions.
            if (m_FrameServer.Metadata.Count > 0)
            {
                EnableDisableKeyframes();
            }
            
            m_FrameServer.Metadata.CalibrationHelper.SpeedUnit = PreferencesManager.PlayerPreferences.SpeedUnit;
            m_FrameServer.Metadata.CalibrationHelper.AccelerationUnit = PreferencesManager.PlayerPreferences.AccelerationUnit;
            m_FrameServer.Metadata.CalibrationHelper.AngleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            m_FrameServer.Metadata.CalibrationHelper.AngularVelocityUnit = PreferencesManager.PlayerPreferences.AngularVelocityUnit;
            m_FrameServer.Metadata.CalibrationHelper.AngularAccelerationUnit = PreferencesManager.PlayerPreferences.AngularAccelerationUnit;

            m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();

            // Refresh image to update timecode in chronos, grids colors, default fading, etc.
            DoInvalidate();
        }
        public void SetInteractiveEffect(InteractiveEffect _effect)
        {
            if(_effect == null)
                return;
            
            m_InteractiveEffect = _effect;
            
            DisablePlayAndDraw();
            EnableDisableAllPlayingControls(false);
            EnableDisableDrawingTools(false);
            DockKeyframePanel(true);
            m_fill = true;
            ResizeUpdate(true);
        }
        public void DeactivateInteractiveEffect()
        {
            m_InteractiveEffect = null;
            EnableDisableAllPlayingControls(true);
            EnableDisableDrawingTools(true);
            DoInvalidate();
        }
        public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
        {
            m_SyncMergeImage = _SyncMergeImage;
                
            if(_bUpdateUI)
            {
                // Ask for a repaint. We don't wait for the next frame to be drawn
                // because the user may be manually moving the other video.
                DoInvalidate();
            }
        }
        public void ReferenceImageSizeChanged()
        {
            m_FrameServer.Metadata.ImageSize = m_FrameServer.VideoReader.Info.ReferenceSize;
            m_PointerTool.SetImageSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_FrameServer.ImageTransform.SetReferenceSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_FrameServer.ImageTransform.ReinitZoom();

            ResizeUpdate(true);
        }
        public void FullScreen(bool _bFullScreen)
        {
            if (_bFullScreen && !m_fill)
            {
                m_fill = true;
                ResizeUpdate(true);
            }
        }
        public void BeforeAddImageDrawing()
        {
            if(m_bIsCurrentlyPlaying)
            {
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);	
            }
                    
            PrepareKeyframesDock();
            
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();
            m_FrameServer.Metadata.UnselectAll();
            AddKeyframe();
        }
        #endregion
        
        #region Various Inits & Setups
        public void InitializeInfobar()
        {
            this.panelTop.Controls.Add(infobar);
            infobar.Visible = false;
        }
        public void InitializeDrawingTools(DrawingToolbarPresenter drawingToolbarPresenter)
        {
            m_PointerTool = new DrawingToolPointer();
            m_ActiveTool = m_PointerTool;

            drawingToolbarPresenter.ForceView(stripDrawingTools);

            drawingToolbarPresenter.AddToolButton(m_PointerTool, drawingTool_Click);
            drawingToolbarPresenter.AddSeparator();

            // Special button: Add key image
            m_btnAddKeyFrame = CreateToolButton();
            m_btnAddKeyFrame.Image = Drawings.addkeyimage;
            m_btnAddKeyFrame.Click += btnAddKeyframe_Click;
            m_btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
            drawingToolbarPresenter.AddSpecialButton(m_btnAddKeyFrame);
            
            // Special button: Key image comments
            m_btnShowComments = CreateToolButton();
            m_btnShowComments.Image = Resources.comments2;
            m_btnShowComments.Click += btnShowComments_Click;
            m_btnShowComments.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
            drawingToolbarPresenter.AddSpecialButton(m_btnShowComments);

            // All drawing tools.
            DrawingToolbarImporter importer = new DrawingToolbarImporter();
            importer.Import("player.xml", drawingToolbarPresenter, drawingTool_Click);

            drawingToolbarPresenter.AddToolButton(ToolManager.Tools["Magnifier"], btnMagnifier_Click);

            // Special button: Tool presets
            m_btnToolPresets = CreateToolButton();
            m_btnToolPresets.Image = Resources.SwatchIcon3;
            m_btnToolPresets.Click += btnColorProfile_Click;
            m_btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
            drawingToolbarPresenter.AddSpecialButton(m_btnToolPresets);

            stripDrawingTools.Left = 3;
        }
        private ToolStripButton CreateToolButton()
        {
            ToolStripButton btn = new ToolStripButton();
            btn.AutoSize = false;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btn.ImageScaling = ToolStripItemImageScaling.None;
            btn.Size = new Size(25, 25);
            btn.AutoToolTip = false;
            return btn;
        }
        private void ResetData()
        {
            m_iFramesToDecode = 1;
            
            slowMotion = 1;
            DeactivateInteractiveEffect();
            m_bIsCurrentlyPlaying = false;
            m_ePlayingMode = PlayingMode.Loop;
            m_fill = false;
            m_FrameServer.ImageTransform.Reset();
            m_lastUserStretch = 1.0f;
            
            // Sync
            m_bSynched = false;
            m_iSyncPosition = 0;
            m_bSyncMerge = false;
            if(m_SyncMergeImage != null)
                m_SyncMergeImage.Dispose();
            
            m_bShowImageBorder = false;
            
            SetupPrimarySelectionData(); 	// Should not be necessary when every data is coming from m_FrameServer.
            
            m_bHandlersLocked = false;
            
            m_iActiveKeyFrameIndex = -1;
            m_ActiveTool = m_PointerTool;
            
            m_bDocked = true;
            m_bTextEdit = false;
            
            m_FrameServer.Metadata.HighSpeedFactor = 1.0f;
            UpdateTimebase();
            UpdateTimeLabels();
        }
        private void SetupPrimarySelectionData()
        {
            // Setup data
            if (m_FrameServer.Loaded)
            {
                m_iSelStart = m_iStartingPosition;
                m_iSelEnd = m_iStartingPosition + m_iTotalDuration - m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;
                m_iSelDuration = m_iTotalDuration;
            }
            else
            {
                m_iSelStart = 0;
                m_iSelEnd = 99;
                m_iSelDuration = 100;
                m_iTotalDuration = 100;
                
                m_iCurrentPosition = 0;
                m_iStartingPosition = 0;
            }
        }
        private void SetupPrimarySelectionPanel()
        {
            // Setup controls & labels.
            // Update internal state only, doesn't trigger the events.
            trkSelection.UpdateInternalState(m_iSelStart, m_iSelEnd, m_iSelStart, m_iSelEnd, m_iSelStart);
            UpdateSelectionLabels();
        }
        private void SetUpForNewMovie()
        {
            OnPoke();
        }
        private void SetupKeyframeCommentsHub()
        {
            m_KeyframeCommentsHub = new formKeyframeComments(this);
            FormsHelper.MakeTopmost(m_KeyframeCommentsHub);
        }
        private void LookForLinkedAnalysis(string file)
        {
            if (File.Exists(file))
            {
                MetadataSerializer s = new MetadataSerializer();
                s.Load(m_FrameServer.Metadata, file, true);
            }
        }
        private void UpdateFilenameLabel()
        {
            if (!m_FrameServer.Loaded)
                return;

            string name = Path.GetFileNameWithoutExtension(m_FrameServer.VideoReader.FilePath);
            string size = string.Format("{0} × {1} px", m_FrameServer.Metadata.ImageSize.Width, m_FrameServer.Metadata.ImageSize.Height);
            string fps = string.Format("{0:0.00} fps", 1000 / timeMapper.UserInterval);
                
            infobar.Visible = true;
            infobar.Dock = DockStyle.Fill;
            infobar.UpdateValues(name, size, fps);
        }
        private void ShowHideRenderingSurface(bool _bShow)
        {
            ImageResizerNE.Visible = _bShow;
            ImageResizerNW.Visible = _bShow;
            ImageResizerSE.Visible = _bShow;
            ImageResizerSW.Visible = _bShow;
            pbSurfaceScreen.Visible = _bShow;
        }
        private void BuildContextMenus()
        {
            // Attach the event handlers and build the menus.
            
            // 1. Default context menu.
            mnuDirectTrack.Click += new EventHandler(mnuDirectTrack_Click);
            mnuDirectTrack.Image = Properties.Drawings.track;
            mnuPasteDrawing.Click += new EventHandler(mnuPasteDrawing_Click);
            mnuPasteDrawing.Image = Properties.Drawings.paste;
            mnuPlayPause.Click += new EventHandler(buttonPlay_Click);
            mnuSavePic.Click += new EventHandler(btnSnapShot_Click);
            mnuSavePic.Image = Properties.Resources.picture_save;
            mnuCopyPic.Click += (s, e) => { CopyImageToClipboard(); };
            mnuCopyPic.Image = Properties.Resources.clipboard_block;
            mnuPastePic.Click += mnuPastePic_Click;
            mnuPastePic.Image = Properties.Drawings.paste;
            mnuCloseScreen.Click += new EventHandler(btnClose_Click);
            mnuCloseScreen.Image = Properties.Resources.film_close3;
            popMenu.Items.AddRange(new ToolStripItem[] { mnuDirectTrack, mnuPasteDrawing, mnuSavePic, mnuCopyPic, mnuPastePic, new ToolStripSeparator(), mnuCloseScreen });

            // 2. Drawings context menu (Configure, Delete, Track this)
            mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);
            mnuConfigureDrawing.Image = Properties.Drawings.configure;
            mnuSetStyleAsDefault.Click += new EventHandler(mnuSetStyleAsDefault_Click);
            mnuSetStyleAsDefault.Image = Resources.SwatchIcon3;
            mnuAlwaysVisible.Click += mnuAlwaysVisible_Click;
            mnuAlwaysVisible.Image = Properties.Drawings.persistence;
            mnuConfigureOpacity.Click += new EventHandler(mnuConfigureOpacity_Click);
            mnuConfigureOpacity.Image = Properties.Drawings.persistence;
            mnuGotoKeyframe.Click += new EventHandler(mnuGotoKeyframe_Click);
            mnuGotoKeyframe.Image = Properties.Resources.page_white_go;

            mnuDrawingTrackingConfigure.Click += mnuDrawingTrackingConfigure_Click;
            mnuDrawingTrackingConfigure.Image = Properties.Drawings.configure;
            mnuDrawingTrackingStart.Click += mnuDrawingTrackingToggle_Click;
            mnuDrawingTrackingStart.Image = Properties.Drawings.trackingplay;
            mnuDrawingTrackingStop.Click += mnuDrawingTrackingToggle_Click;
            mnuDrawingTrackingStop.Image = Properties.Drawings.trackstop;
            mnuDrawingTracking.Image = Properties.Drawings.track;
            //mnuDrawingTracking.DropDownItems.AddRange(new ToolStripItem[] { mnuDrawingTrackingConfigure, new ToolStripSeparator(), mnuDrawingTrackingStart, mnuDrawingTrackingStop, new ToolStripSeparator(), mnuDrawingTrackingShowNotTracked });
            mnuDrawingTracking.DropDownItems.AddRange(new ToolStripItem[] { mnuDrawingTrackingStart, mnuDrawingTrackingStop });

            mnuCutDrawing.Click += new EventHandler(mnuCutDrawing_Click);
            mnuCutDrawing.Image = Properties.Drawings.cut;
            mnuCopyDrawing.Click += new EventHandler(mnuCopyDrawing_Click);
            mnuCopyDrawing.Image = Properties.Drawings.copy;
            mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);
            mnuDeleteDrawing.Image = Properties.Drawings.delete;
            
            // 3. Tracking pop menu (Restart, Stop tracking)
            mnuStopTracking.Click += new EventHandler(mnuStopTracking_Click);
            mnuStopTracking.Visible = false;
            mnuStopTracking.Image = Properties.Drawings.trackstop;
            mnuRestartTracking.Click += new EventHandler(mnuRestartTracking_Click);
            mnuRestartTracking.Visible = false;
            mnuRestartTracking.Image = Properties.Drawings.trackingplay;
            mnuDeleteTrajectory.Click += new EventHandler(mnuDeleteTrajectory_Click);
            mnuDeleteTrajectory.Image = Properties.Drawings.delete;
            mnuDeleteEndOfTrajectory.Click += new EventHandler(mnuDeleteEndOfTrajectory_Click);
            mnuConfigureTrajectory.Click += new EventHandler(mnuConfigureTrajectory_Click);
            mnuConfigureTrajectory.Image = Properties.Drawings.configure;
            
            // 4. Chrono pop menu (Start, Stop, Hide, etc.)
            mnuChronoConfigure.Click += new EventHandler(mnuChronoConfigure_Click);
            mnuChronoConfigure.Image = Properties.Drawings.configure;
            mnuChronoStart.Click += new EventHandler(mnuChronoStart_Click);
            mnuChronoStart.Image = Properties.Drawings.chronostart;
            mnuChronoStop.Click += new EventHandler(mnuChronoStop_Click);
            mnuChronoStop.Image = Properties.Drawings.chronostop;
            mnuChronoCountdown.Click += new EventHandler(mnuChronoCountdown_Click);
            mnuChronoCountdown.Checked = false;
            mnuChronoCountdown.Enabled = false;
            mnuChronoHide.Click += new EventHandler(mnuChronoHide_Click);
            mnuChronoHide.Image = Properties.Drawings.hide;
            mnuChronoDelete.Click += new EventHandler(mnuChronoDelete_Click);
            mnuChronoDelete.Image = Properties.Drawings.delete;
            popMenuChrono.Items.AddRange(new ToolStripItem[] { mnuChronoConfigure, new ToolStripSeparator(), mnuChronoStart, mnuChronoStop, mnuChronoCountdown, new ToolStripSeparator(), mnuChronoHide, mnuChronoDelete, });

            // 5. Magnifier
            foreach(double factor in Magnifier.MagnificationFactors)
                maginificationMenus.Add(CreateMagnificationMenu(factor));
            maginificationMenus[1].Checked = true;
            popMenuMagnifier.Items.AddRange(maginificationMenus.ToArray());
            
            mnuMagnifierTrack.Click += mnuMagnifierTrack_Click;
            mnuMagnifierTrack.Image = Properties.Drawings.track;
            mnuMagnifierDirect.Click += mnuMagnifierDirect_Click;
            mnuMagnifierDirect.Image = Properties.Resources.arrow_out;
            mnuMagnifierQuit.Click += mnuMagnifierQuit_Click;
            mnuMagnifierQuit.Image = Properties.Resources.hide;
            popMenuMagnifier.Items.AddRange(new ToolStripItem[] { new ToolStripSeparator(), mnuMagnifierTrack, new ToolStripSeparator(), mnuMagnifierDirect, mnuMagnifierQuit });
            
            // The right context menu and its content will be choosen upon MouseDown.
            panelCenter.ContextMenuStrip = popMenu;
            
            // Load texts
            ReloadMenusCulture();
        }

        private ToolStripMenuItem CreateMagnificationMenu(double magnificationFactor)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Tag = magnificationFactor;
            mnu.Text = String.Format(ScreenManagerLang.mnuMagnification, magnificationFactor.ToString());
            mnu.Click += mnuMagnifierChangeMagnification;
            return mnu;
        }
        private void PostLoad_Idle(object sender, EventArgs e)
        {
            Application.Idle -= PostLoad_Idle;
            m_Constructed = true;

            if(!m_FrameServer.Loaded)
                return;
            
            log.DebugFormat("Post load event.");
            
            // This would be a good time to start the prebuffering if supported.
            // The UpdateWorkingZone call may try to go full cache if possible.
            m_FrameServer.VideoReader.PostLoad();
            UpdateWorkingZone(true);
            UpdateFramesMarkers();
            
            ShowHideRenderingSurface(true);
            
            ResizeUpdate(true);
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(int cmd)
        {
            // Method called by KinoveaControl in the context of preprocessing hotkeys.
            // If the hotkey can be handled by the dual player, we defer to it instead.

            if (m_FrameServer.Metadata.TextEditingInProgress)
                return false;

            if (thumbnails.Any(t => t.Editing))
                return false;

            if (!m_bSynched)
                return ExecuteScreenCommand(cmd);

            HotkeyCommand command = Hotkeys.FirstOrDefault(h => h != null && h.CommandCode == cmd);
            if (command == null)
                return false;

            bool dualPlayerHandled = HotkeySettingsManager.IsHandler("DualPlayer", command.KeyData);

            if (dualPlayerHandled && DualCommandReceived != null)
            {
                DualCommandReceived(this, new EventArgs<HotkeyCommand>(command));
                return true;
            }
            else
            {
                return ExecuteScreenCommand(cmd);
            }
        }

        public bool ExecuteScreenCommand(int cmd)
        {
            if (!m_FrameServer.Loaded)
                return false;

            PlayerScreenCommands command = (PlayerScreenCommands)cmd;

            switch (command)
            {
                case PlayerScreenCommands.TogglePlay:
                    OnButtonPlay();
                    break;
                case PlayerScreenCommands.ResetViewport:
                    DisablePlayAndDraw();
                    DoInvalidate();
                    break;
                case PlayerScreenCommands.GotoPreviousImage:
                    buttonGotoPrevious_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoPreviousImageForceLoop:
                    if (m_iCurrentPosition <= m_iSelStart)
                        buttonGotoLast_Click(null, EventArgs.Empty);
                    else
                        buttonGotoPrevious_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoFirstImage:
                    buttonGotoFirst_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoPreviousKeyframe:
                    GotoPreviousKeyframe();
                    break;
                case PlayerScreenCommands.BackwardRound10Percent:
                    JumpToPercent(10, false);
                    break;
                case PlayerScreenCommands.BackwardRound1Percent:
                    JumpToPercent(1, false);
                    break;
                case PlayerScreenCommands.GotoNextImage:
                    buttonGotoNext_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.ForwardRound10Percent:
                    JumpToPercent(10, true);
                    break;
                case PlayerScreenCommands.ForwardRound1Percent:
                    JumpToPercent(1, true);
                    break;
                case PlayerScreenCommands.GotoLastImage:
                    buttonGotoLast_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoNextKeyframe:
                    GotoNextKeyframe();
                    break;
                case PlayerScreenCommands.GotoSyncPoint:
                    if (m_bSynched)
                        ForceCurrentFrame(m_iSyncPosition, true);
                    break;
                case PlayerScreenCommands.IncreaseZoom:
                    IncreaseDirectZoom();
                    break;
                case PlayerScreenCommands.DecreaseZoom:
                    DecreaseDirectZoom();
                    break;
                case PlayerScreenCommands.ResetZoom:
                    UnzoomDirectZoom(true);
                    break;
                case PlayerScreenCommands.IncreaseSyncAlpha:
                    IncreaseSyncAlpha();
                    break;
                case PlayerScreenCommands.DecreaseSyncAlpha:
                    DecreaseSyncAlpha();
                    break;
                case PlayerScreenCommands.AddKeyframe:
                    AddKeyframe();
                    break;
                case PlayerScreenCommands.DeleteKeyframe:
                    if (m_iActiveKeyFrameIndex >= 0)
                    {
                        Guid id = m_FrameServer.Metadata.GetKeyframeId(m_iActiveKeyFrameIndex);
                        DeleteKeyframe(id);
                    }
                    break;
                case PlayerScreenCommands.CutDrawing:
                    CutDrawing();
                    break;
                case PlayerScreenCommands.CopyDrawing:
                    CopyDrawing();
                    break;
                case PlayerScreenCommands.PasteDrawing:
                    PasteDrawing(false);
                    break;
                case PlayerScreenCommands.PasteInPlaceDrawing:
                    PasteDrawing(true);
                    break;
                case PlayerScreenCommands.DeleteDrawing:
                    DeleteSelectedDrawing();
                    break;
                case PlayerScreenCommands.CopyImage:
                    CopyImageToClipboard();
                    break;
                case PlayerScreenCommands.ValidateDrawing:
                    ValidateDrawing();
                    break;
                case PlayerScreenCommands.IncreaseSpeed1:
                    ChangeSpeed(1);
                    break;
                case PlayerScreenCommands.IncreaseSpeedRoundTo10:
                    ChangeSpeed(10);
                    break;
                case PlayerScreenCommands.IncreaseSpeedRoundTo25:
                    ChangeSpeed(25);
                    break;
                case PlayerScreenCommands.DecreaseSpeed1:
                    ChangeSpeed(-1);
                    break;
                case PlayerScreenCommands.DecreaseSpeedRoundTo10:
                    ChangeSpeed(-10);
                    break;
                case PlayerScreenCommands.DecreaseSpeedRoundTo25:
                    ChangeSpeed(-25);
                    break;
                case PlayerScreenCommands.Close:
                    btnClose_Click(this, EventArgs.Empty);
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public void AfterClose()
        {
            m_KeyframeCommentsHub.Owner = null;
            m_KeyframeCommentsHub.Dispose();
            m_KeyframeCommentsHub = null;

            m_DeselectionTimer.Tick -= DeselectionTimer_OnTick;
            m_DeselectionTimer.Dispose();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                panelCenter.ContextMenuStrip = null;

                popMenu.Dispose();
                popMenuDrawings.Dispose();
                popMenuTrack.Dispose();
                popMenuChrono.Dispose();
                popMenuMagnifier.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Misc Events
        private void btnClose_Click(object sender, EventArgs e)
        {
            // If we currently are in DrawTime filter, we just close this and return to normal playback.
            // Propagate to PlayerScreen which will report to ScreenManager.
            if (CloseAsked != null)
                CloseAsked(this, EventArgs.Empty);
        }
        private void PanelVideoControls_MouseEnter(object sender, EventArgs e)
        {
            // Set focus to enable mouse scroll
            panelVideoControls.Focus();
        }
        #endregion
        
        #region Misc private helpers
        private void OnPauseAsked()
        {
            if (PauseAsked != null)
                PauseAsked(this, EventArgs.Empty);
        }
        private void OnSelectionChanged(bool initialization)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new EventArgs<bool>(initialization));
        }
        private void OnPoke()
        {
            //------------------------------------------------------------------------------
            // This function is a hub event handler for all button press, mouse clicks, etc.
            // Signal itself as the active screen to the ScreenManager
            // This will trigger an update of the top-level menu to enable/disable specific menus.
            //---------------------------------------------------------------------
            if (SetAsActiveScreen != null)
                SetAsActiveScreen(this, EventArgs.Empty);
            
            // 1. Ensure no DrawingText is in edit mode.
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();

            m_ActiveTool = m_ActiveTool.KeepToolFrameChanged ? m_ActiveTool : m_PointerTool;
            if(m_ActiveTool == m_PointerTool)
            {
                SetCursor(m_PointerTool.GetCursor(-1));
            }
            
            // 3. Dock Keyf panel if nothing to see.
            if (m_FrameServer.Metadata.Count < 1)
            {
                DockKeyframePanel(true);
            }
        }
        private void UpdateFramesMarkers()
        {
            // Updates the markers coordinates and redraw the trkFrame.
            trkFrame.UpdateMarkers(m_FrameServer.Metadata);
            trkFrame.Invalidate();
        }
        private void ShowBorder(bool _bShow)
        {
            m_bShowImageBorder = _bShow;
            DoInvalidate();
        }
        private void DrawImageBorder(Graphics _canvas)
        {
            // Draw the border around the screen to mark it as selected.
            // Called back from main drawing routine.
            _canvas.DrawRectangle(m_PenImageBorder, 0, 0, pbSurfaceScreen.Width - m_PenImageBorder.Width, pbSurfaceScreen.Height - m_PenImageBorder.Width);
        }
        private void DisablePlayAndDraw()
        {
            StopPlaying();
            m_ActiveTool = m_PointerTool;
            SetCursor(m_PointerTool.GetCursor(0));
            DisableMagnifier();
            UnzoomDirectZoom(false);
            m_FrameServer.Metadata.InitializeEnd(true);
            m_FrameServer.Metadata.StopAllTracking();
            m_FrameServer.Metadata.UnselectAll();
            CheckCustomDecodingSize(false);
        }
        private void ValidateDrawing()
        {
            if (m_FrameServer.Metadata.DrawingInitializing)
            {
                m_FrameServer.Metadata.InitializeEnd(true);
                DoInvalidate();
            }
        }
        private long TimestampToRealtime(long timestamp)
        {
            // This is used in the context of synchronization.
            // Takes input in timestamps relative to sel start.
            // convert it into video time then to real time using high speed factor.
            // returned value is in microseconds.

            double correctedTPS = m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds * m_FrameServer.VideoReader.Info.AverageTimeStampsPerSeconds / m_FrameServer.Metadata.UserInterval;

            if (correctedTPS == 0 || m_FrameServer.Metadata.HighSpeedFactor == 0)
                return 0;

            double videoSeconds = (double)timestamp / correctedTPS;
            double realSeconds = videoSeconds / m_FrameServer.Metadata.HighSpeedFactor;
            double realMicroseconds = realSeconds * 1000000;
            return (long)realMicroseconds;
        }
        #endregion

        #region Video Controls

        #region Playback Controls
        public void buttonGotoFirst_Click(object sender, EventArgs e)
        {
            // Jump to start.
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                StopPlaying();
                OnPauseAsked();
                
                m_iFramesToDecode = 1;
                ShowNextFrame(m_iSelStart, true);
                
                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        public void buttonGotoPrevious_Click(object sender, EventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                StopPlaying();
                OnPauseAsked();
                
                //---------------------------------------------------------------------------
                // Si on est en dehors de la zone primaire, ou qu'on va en sortir,
                // se replacer au début de celle-ci.
                //---------------------------------------------------------------------------
                if ((m_iCurrentPosition <= m_iSelStart) || (m_iCurrentPosition > m_iSelEnd))
                {
                    m_iFramesToDecode = 1;
                    ShowNextFrame(m_iSelStart, true);
                }
                else
                {
                    long oldPos = m_iCurrentPosition;
                    m_iFramesToDecode = -1;
                    ShowNextFrame(-1, true);
                    
                    // If it didn't work, try going back two frames to unstuck the situation.
                    // Todo: check if we're going to endup outside the working zone ?
                    if(m_iCurrentPosition == oldPos)
                    {
                        log.Debug("Seeking to previous frame did not work. Moving backward 2 frames.");
                        m_iFramesToDecode = -2;
                        ShowNextFrame(-1, true);
                    }
                        
                    // Reset to normal.
                    m_iFramesToDecode = 1;
                }
                
                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
            
        }
        private void buttonPlay_Click(object sender, EventArgs e)
        {
            //----------------------------------------------------------------------------
            // L'appui sur le bouton play ne fait qu'activer ou désactiver le Timer
            // La lecture est ensuite automatique et c'est dans la fonction du Timer
            // que l'on gère la NextFrame à afficher en fonction du ralentit,
            // du mode de bouclage etc...
            //----------------------------------------------------------------------------
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                OnButtonPlay();
            }
        }
        public void buttonGotoNext_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;
            
            OnPoke();
            StopPlaying();
            OnPauseAsked();
            m_iFramesToDecode = 1;

            // If we are outside the primary zone or going to get out, seek to start.
            // We also only do the seek if we are after the m_iStartingPosition,
            // Sometimes, the second frame will have a time stamp inferior to the first,
            // which sort of breaks our sentinels.
            if (((m_iCurrentPosition < m_iSelStart) || (m_iCurrentPosition >= m_iSelEnd)) &&
                (m_iCurrentPosition >= m_iStartingPosition))
                ShowNextFrame(m_iSelStart, true);
            else
                ShowNextFrame(-1, true);

            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);
        }
        public void JumpToPercent(int round, bool forward)
        {
            if (!m_FrameServer.Loaded)
                return;

            StopPlaying();
            OnPauseAsked();
            m_iFramesToDecode = 1;

            float normalized = ((float)m_iCurrentPosition - m_iSelStart) / m_iSelDuration;
            int currentPercentage = (int)Math.Round(normalized * 100);
            int maxSteps = 100/round;
            int currentStep = currentPercentage / round;
            int nextStep = forward ? currentStep + 1 : currentStep - 1;
            nextStep = Math.Max(Math.Min(nextStep, maxSteps), 0);
            int newPercentage = nextStep * round;
            long newPosition = m_iSelStart + (long)(((float)newPercentage / 100) * m_iSelDuration);

            ShowNextFrame(newPosition, true);

            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);
        }
        public void buttonGotoLast_Click(object sender, EventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                StopPlaying();
                OnPauseAsked();

                m_iFramesToDecode = 1;
                ShowNextFrame(m_iSelEnd, true);

                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        public void OnButtonPlay()
        {
            //--------------------------------------------------------------
            // This function is accessed from ScreenManager.
            // Eventually from a worker thread. (no SetAsActiveScreen here).
            //--------------------------------------------------------------
            if (!m_FrameServer.Loaded)
                return;

            if (m_FrameServer.Metadata.DrawingInitializing)
                return;

            if (m_bIsCurrentlyPlaying)
            {
                // Go into Pause mode.
                StopPlaying();
                OnPauseAsked();
                buttonPlay.Image = Player.flatplay;
                ActivateKeyframe(m_iCurrentPosition);
                ToastPause();
            }
            else
            {
                // Go into Play mode
                buttonPlay.Image = Resources.flatpause3b;
                StartMultimediaTimer(GetPlaybackFrameInterval());
            }
        }
        public void Common_MouseWheel(object sender, MouseEventArgs e)
        {
            // MouseWheel was recorded on one of the controls.
            int iScrollOffset = e.Delta * SystemInformation.MouseWheelScrollLines / 120;

            if(InteractiveFiltering)
            {
                if(m_InteractiveEffect.MouseWheel != null)
                {
                    m_InteractiveEffect.MouseWheel(iScrollOffset);
                    DoInvalidate();
                }
                return;
            }
            
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (iScrollOffset > 0)
                    IncreaseDirectZoom();
                else
                    DecreaseDirectZoom();
            }
            else if((ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                if (iScrollOffset > 0)
                    IncreaseSyncAlpha();
                else
                    DecreaseSyncAlpha();
            }
            else
            {
                if (iScrollOffset > 0)
                {
                    buttonGotoNext_Click(null, EventArgs.Empty);
                }
                else
                {
                    // Shift + Left on first => loop backward.
                    if (((ModifierKeys & Keys.Shift) == Keys.Shift) && m_iCurrentPosition <= m_iSelStart)
                        buttonGotoLast_Click(null, EventArgs.Empty);
                    else
                        buttonGotoPrevious_Click(null, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Working Zone Selection
        private void trkSelection_SelectionChanging(object sender, EventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                StopPlaying();
                OnPauseAsked();

                // Update selection timestamps and labels.
                UpdateSelectionDataFromControl();
                UpdateSelectionLabels();

                // Update the frame tracker internal timestamps (including position if needed).
                trkFrame.Remap(m_iSelStart, m_iSelEnd);
            }
        }
        private void trkSelection_SelectionChanged(object sender, EventArgs e)
        {
            // Actual update.
            if (m_FrameServer.Loaded)
            {
                UpdateSelectionDataFromControl();
                UpdateWorkingZone(false);

                AfterSelectionChanged();
            }
        }
        private void trkSelection_TargetAcquired(object sender, EventArgs e)
        {
            // User clicked inside selection: Jump to position.
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                StopPlaying();
                OnPauseAsked();
                m_iFramesToDecode = 1;

                ShowNextFrame(trkSelection.SelPos, true);
                m_iCurrentPosition = trkSelection.SelPos + trkSelection.Minimum;
                
                UpdatePositionUI();
                ActivateKeyframe(m_iCurrentPosition);
            }
            
        }
        private void btn_HandlersLock_Click(object sender, EventArgs e)
        {
            // Lock the selection handlers.
            if (m_FrameServer.Loaded)
            {
                m_bHandlersLocked = !m_bHandlersLocked;
                trkSelection.SelLocked = m_bHandlersLocked;

                // Update UI accordingly.
                if (m_bHandlersLocked)
                {
                    btn_HandlersLock.Image = Resources.primselec_locked3;
                    toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionUnlock);
                }
                else
                {
                    btn_HandlersLock.Image = Resources.primselec_unlocked3;
                    toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionLock);
                }
            }
        }
        private void btnSetHandlerLeft_Click(object sender, EventArgs e)
        {
            // Set the left handler of the selection at the current frame.
            if (m_FrameServer.Loaded && !m_bHandlersLocked)
            {
                trkSelection.SelStart = m_iCurrentPosition;
                UpdateSelectionDataFromControl();
                UpdateSelectionLabels();
                trkFrame.Remap(m_iSelStart,m_iSelEnd);
                UpdateWorkingZone(false);
                
                AfterSelectionChanged();
            }
        }
        private void btnSetHandlerRight_Click(object sender, EventArgs e)
        {
            // Set the right handler of the selection at the current frame.
            if (m_FrameServer.Loaded && !m_bHandlersLocked)
            {
                trkSelection.SelEnd = m_iCurrentPosition;
                UpdateSelectionDataFromControl();
                UpdateSelectionLabels();
                trkFrame.Remap(m_iSelStart,m_iSelEnd);
                UpdateWorkingZone(false);
                
                AfterSelectionChanged();
            }
        }
        private void btnHandlersReset_Click(object sender, EventArgs e)
        {
            // Reset both selection sentinels to their max values.
            if (m_FrameServer.Loaded && !m_bHandlersLocked)
            {
                trkSelection.Reset();
                UpdateSelectionDataFromControl();
                
                // We need to force the reloading of all frames.
                UpdateWorkingZone(true);
                
                AfterSelectionChanged();
            }
        }
        
        private void UpdateFramePrimarySelection()
        {
            //--------------------------------------------------------------
            // Update the visible image to reflect the new selection.
            // Checks that the previous current frame is still within selection,
            // jumps to closest sentinel otherwise.
            //--------------------------------------------------------------
            
            if (m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
            {
                if(m_FrameServer.VideoReader.Current == null)
                    ShowNextFrame(m_iSelStart, true);
                else
                    ShowNextFrame(m_FrameServer.VideoReader.Current.Timestamp, true);
            }
            else if (m_iCurrentPosition < m_iSelStart || m_iCurrentPosition > m_iSelEnd)
            {
                m_iFramesToDecode = 1;
                if (m_iCurrentPosition < m_iSelStart)
                    ShowNextFrame(m_iSelStart, true);
                else
                    ShowNextFrame(m_iSelEnd, true);
            }

            UpdatePositionUI();
        }
        private void UpdateSelectionLabels()
        {
            long start = 0;
            long duration = 0;
            
            if(m_FrameServer.Loaded)
            {
                start = m_iSelStart - m_iStartingPosition;
                duration = m_iSelDuration;
            }
            
            string startTimecode = m_FrameServer.TimeStampsToTimecode(start, TimeType.Time, PreferencesManager.PlayerPreferences.TimecodeFormat, false);
            lblSelStartSelection.Text = ScreenManagerLang.lblSelStartSelection_Text + " : " + startTimecode;

            duration -= m_FrameServer.Metadata.AverageTimeStampsPerFrame;
            string durationTimecode = m_FrameServer.TimeStampsToTimecode(duration, TimeType.Duration, PreferencesManager.PlayerPreferences.TimecodeFormat, false);
            lblSelDuration.Text = ScreenManagerLang.lblSelDuration_Text + " : " + durationTimecode;
        }
        private void UpdateSelectionDataFromControl()
        {
            // Update WorkingZone data according to control.
            if ((m_iSelStart != trkSelection.SelStart) || (m_iSelEnd != trkSelection.SelEnd))
            {
                m_iSelStart = trkSelection.SelStart;
                m_iSelEnd = trkSelection.SelEnd;
                m_iSelDuration = m_iSelEnd - m_iSelStart + m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;
            }
        }
        private void AfterSelectionChanged()
        {
            // Update everything as if we moved the handlers manually.
            m_FrameServer.Metadata.SelectionStart = m_iSelStart;
            m_FrameServer.Metadata.SelectionEnd = m_iSelEnd;
            
            UpdateFramesMarkers();
            
            OnPoke();
            OnSelectionChanged(true);

            // Update current image and keyframe  status.
            UpdateFramePrimarySelection();
            EnableDisableKeyframes();
            ActivateKeyframe(m_iCurrentPosition);	
        }
        #endregion
        
        #region Frame Tracker
        private void trkFrame_PositionChanging(object sender, PositionChangedEventArgs e)
        {
            if (!PreferencesManager.PlayerPreferences.InteractiveFrameTracker)
                return;

            if (m_FrameServer.Loaded)
            {
                // Update image but do not touch cursor, as the user is manipulating it.
                // If the position needs to be adjusted to an actual timestamp, it'll be done later.
                StopPlaying();
                UpdateFrameCurrentPosition(false);
                UpdateCurrentPositionLabel();
                
                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        private void trkFrame_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                OnPoke();
                
                m_iCurrentPosition = trkFrame.Position;

                StopPlaying();
                OnPauseAsked();

                // Update image and cursor.
                UpdateFrameCurrentPosition(true);
                UpdateCurrentPositionLabel();
                ActivateKeyframe(m_iCurrentPosition);

                // Update WorkingZone hairline.
                trkSelection.SelPos =  m_iCurrentPosition;
                trkSelection.Invalidate();
            }
        }
        private void UpdateFrameCurrentPosition(bool _bUpdateNavCursor)
        {
            // Displays the image corresponding to the current position within working zone.
            // Trigerred by user (or first load). i.e: cursor moved, show frame.
            if (m_FrameServer.VideoReader.DecodingMode != VideoDecodingMode.Caching)
                this.Cursor = Cursors.WaitCursor;

            m_iCurrentPosition = trkFrame.Position;
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iCurrentPosition, true);

            // The following may readjust the cursor in case the mouse wasn't on a valid timestamp value.
            if (_bUpdateNavCursor)
                UpdatePositionUI();

            if (m_FrameServer.VideoReader.DecodingMode != VideoDecodingMode.Caching)
                this.Cursor = Cursors.Default;
        }
        private void UpdateCurrentPositionLabel()
        {
            // Note: among other places, this is run inside the playloop.
            // Position is relative to working zone.
            string timecode = m_FrameServer.TimeStampsToTimecode(m_iCurrentPosition - m_iSelStart, TimeType.Time, PreferencesManager.PlayerPreferences.TimecodeFormat, m_bSynched);
            lblTimeCode.Text = string.Format("{0} : {1}", ScreenManagerLang.lblTimeCode_Text, timecode);
        }
        private void UpdatePositionUI()
        {
            // Update markers and label for position.
            
            //trkFrame.UpdateCacheSegmentMarker(m_FrameServer.VideoReader.Cache.Segment);
            trkFrame.Position = m_iCurrentPosition;
            trkFrame.UpdateCacheSegmentMarker(m_FrameServer.VideoReader.PreBufferingSegment);
            trkFrame.Invalidate();
            trkSelection.SelPos = m_iCurrentPosition;
            trkSelection.Invalidate();
            UpdateCurrentPositionLabel();
            RepositionSpeedControl();
        }
        #endregion

        #region Speed Slider
        private void sldrSpeed_ValueChanged(object sender, EventArgs e)
        {
            slowMotion = timeMapper.GetSlowMotion(sldrSpeed.Value);
            
            if (m_FrameServer.Loaded)
            {
                // Reset timer with new value.
                if (m_bIsCurrentlyPlaying)
                {
                    StopMultimediaTimer();
                    StartMultimediaTimer(GetPlaybackFrameInterval());
                }

                if (SpeedChanged != null)
                    SpeedChanged(this, EventArgs.Empty);
            }

            UpdateSpeedLabel();
        }
        private void ChangeSpeed(int change)
        {
            // The value is a target diff percentage.
            // Ex: we are on 86%, value = -25, the target is 75%.

            if (change == 0)
                return;
            
            sldrSpeed.StepJump(change / 200.0);
        }
        private void lblSpeedTuner_DoubleClick(object sender, EventArgs e)
        {
            slowMotion = 1;
            sldrSpeed.Force(timeMapper.GetInputFromSlowMotion(slowMotion));
        }
        private void UpdateSpeedLabel()
        {
            double multiplier = timeMapper.GetRealtimeMultiplier(sldrSpeed.Value);
            string speedValue = "";

            if (multiplier < 1.0)
                speedValue = string.Format("{0:0.##}%", multiplier * 100);
            else
                speedValue = string.Format("{0:0.##}x", multiplier);

            lblSpeedTuner.Text = string.Format(ScreenManagerLang.lblSpeedTuner_Text, speedValue);
        }
        private void RepositionSpeedControl()
        {
            lblSpeedTuner.Left = lblTimeCode.Right + 8;

            // Fake the longest speed string possible for positioning.
            string temp = lblSpeedTuner.Text;
            lblSpeedTuner.Text = string.Format("{0} {1:0.00}%", ScreenManagerLang.lblSpeedTuner_Text, 99.99);

            sldrSpeed.Left = lblSpeedTuner.Right + 8;

            lblSpeedTuner.Text = temp;
        }
        #endregion

        #region Loop Mode
        private void buttonPlayingMode_Click(object sender, EventArgs e)
        {
            // Playback mode ('Once' or 'Loop').
            if (m_FrameServer.Loaded)
            {
                OnPoke();

                if (m_ePlayingMode == PlayingMode.Once)
                {
                    m_ePlayingMode = PlayingMode.Loop;
                }
                else if (m_ePlayingMode == PlayingMode.Loop)
                {
                    m_ePlayingMode = PlayingMode.Once;
                }
                
                UpdatePlayingModeButton();
            }
        }
        private void UpdatePlayingModeButton()
        {
            if (m_ePlayingMode == PlayingMode.Once)
            {
                btnPlayingMode.Image = Resources.playmodeonce;
                toolTips.SetToolTip(btnPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Once);		
            }
            else if(m_ePlayingMode == PlayingMode.Loop)
            {
                btnPlayingMode.Image = Resources.playmodeloop;
                toolTips.SetToolTip(btnPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Loop);	
            }
        }
        #endregion

        #endregion

        #region Auto Stretch & Manual Resize
        private void StretchSqueezeSurface()
        {
            // Compute the rendering size, and the corresponding optimal decoding size.
            // We don't ask the VideoReader to update its decoding size here.
            // (We might want to wait the end of a resizing process for example.).
            // Similarly, we don't update the rendering zoom factor, so that during resizing process,
            // the zoom window is still computed based on the current decoding size.
            
            if (!m_FrameServer.Loaded)
                return;

            double targetStretch = m_FrameServer.ImageTransform.Stretch;
            
            // If we have been forced to a different stretch (due to application resizing or minimizing), 
            // make sure we aim for the user's last requested value.
            if(!m_fill && m_lastUserStretch != m_viewportManipulator.Stretch)
                targetStretch = m_lastUserStretch;
            
            // Stretch factor, zoom, or container size have been updated, update the rendering and decoding sizes.
            // During the process, stretch and fill may be forced to different values.
            bool scalable = m_FrameServer.VideoReader.CanScaleIndefinitely;
            m_viewportManipulator.Manipulate(panelCenter.Size, targetStretch, m_fill, m_FrameServer.ImageTransform.Zoom, m_bEnableCustomDecodingSize, scalable);
            pbSurfaceScreen.Size = m_viewportManipulator.RenderingSize;
            pbSurfaceScreen.Location = m_viewportManipulator.RenderingLocation;
            m_FrameServer.ImageTransform.Stretch = m_viewportManipulator.Stretch;
            
            ReplaceResizers();
        }
        private void ReplaceResizers()
        {
            ImageResizerSE.Left = pbSurfaceScreen.Right - (ImageResizerSE.Width / 2);
            ImageResizerSE.Top = pbSurfaceScreen.Bottom - (ImageResizerSE.Height / 2);
            
            ImageResizerSW.Left = pbSurfaceScreen.Left - (ImageResizerSW.Width / 2);
            ImageResizerSW.Top = pbSurfaceScreen.Bottom - (ImageResizerSW.Height / 2);
            
            ImageResizerNE.Left = pbSurfaceScreen.Right - (ImageResizerNE.Width / 2);
            ImageResizerNE.Top = pbSurfaceScreen.Top - (ImageResizerNE.Height / 2);

            ImageResizerNW.Left = pbSurfaceScreen.Left - ImageResizerNW.Width/2;
            ImageResizerNW.Top = pbSurfaceScreen.Top - ImageResizerNW.Height/2;
        }
        private void ToggleImageFillMode()
        {
            if (!m_fill)
            {
                m_fill = true;
            }
            else
            {
                // If the image doesn't fit in the container, we stay in fill mode.
                if (m_FrameServer.ImageTransform.Stretch >= 1)
                {
                    m_FrameServer.ImageTransform.Stretch = 1;
                    m_fill = false;
                }
            }
            
            ResizeUpdate(true);
        }
        private void ImageResizerSE_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            
            int iTargetHeight = (ImageResizerSE.Top - pbSurfaceScreen.Top + e.Y);
            int iTargetWidth = (ImageResizerSE.Left - pbSurfaceScreen.Left + e.X);
            ManualResizeImage(iTargetWidth, iTargetHeight);
        }
        private void ImageResizerSW_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int iTargetHeight = (ImageResizerSW.Top - pbSurfaceScreen.Top + e.Y);
                int iTargetWidth = pbSurfaceScreen.Width + (pbSurfaceScreen.Left - (ImageResizerSW.Left + e.X));
                ManualResizeImage(iTargetWidth, iTargetHeight);
            }
        }
        private void ImageResizerNW_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int iTargetHeight = pbSurfaceScreen.Height + (pbSurfaceScreen.Top - (ImageResizerNW.Top + e.Y));
                int iTargetWidth = pbSurfaceScreen.Width + (pbSurfaceScreen.Left - (ImageResizerNW.Left + e.X));
                ManualResizeImage(iTargetWidth, iTargetHeight);
            }
        }
        private void ImageResizerNE_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int iTargetHeight = pbSurfaceScreen.Height + (pbSurfaceScreen.Top - (ImageResizerNE.Top + e.Y));
                int iTargetWidth = (ImageResizerNE.Left - pbSurfaceScreen.Left + e.X);
                ManualResizeImage(iTargetWidth, iTargetHeight);
            }
        }
        private void ManualResizeImage(int _iTargetWidth, int _iTargetHeight)
        {
            Size targetSize = new Size(_iTargetWidth, _iTargetHeight);
            if(!targetSize.FitsIn(panelCenter.Size))
                return;
            
            if(!m_bManualSqueeze && !m_FrameServer.VideoReader.Info.ReferenceSize.FitsIn(targetSize))
                return;
            
            // Area of the original size is sticky on the inside.
            if(!m_FrameServer.VideoReader.Info.ReferenceSize.FitsIn(targetSize) && 
               (m_FrameServer.VideoReader.Info.ReferenceSize.Width - _iTargetWidth < 40 &&
                m_FrameServer.VideoReader.Info.ReferenceSize.Height - _iTargetHeight < 40))
            {
                _iTargetWidth = m_FrameServer.VideoReader.Info.ReferenceSize.Width;
                _iTargetHeight = m_FrameServer.VideoReader.Info.ReferenceSize.Height;
            }
            
            if(!m_MinimalSize.FitsIn(targetSize))
                return;
            
            double fHeightFactor = ((_iTargetHeight) / (double)m_FrameServer.VideoReader.Info.ReferenceSize.Height);
            double fWidthFactor = ((_iTargetWidth) / (double)m_FrameServer.VideoReader.Info.ReferenceSize.Width);

            m_FrameServer.ImageTransform.Stretch = (fWidthFactor + fHeightFactor) / 2;
            m_fill = false;
            m_lastUserStretch = m_FrameServer.ImageTransform.Stretch;

            ResizeUpdate(false);
        }
        private void Resizers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ToggleImageFillMode();
        }
        private void Resizers_MouseUp(object sender, MouseEventArgs e)
        {
            ResizeUpdate(true);
        }
        private void ResizeUpdate(bool _finished)
        {
            if(!m_FrameServer.Loaded)
                return;
            
            StretchSqueezeSurface();

            if(_finished)
            {
                // Update the decoding size. (May clear and restart the prebuffering).
                if(m_FrameServer.VideoReader.CanChangeDecodingSize)
                {
                    m_FrameServer.VideoReader.ChangeDecodingSize(m_viewportManipulator.DecodingSize);
                    m_FrameServer.ImageTransform.SetRenderingZoomFactor(m_viewportManipulator.RenderingZoomFactor);
                }
                m_FrameServer.Metadata.ResizeFinished();
                RefreshImage();
            }
            else
            {
                DoInvalidate();
            }
        }
        private void CheckCustomDecodingSize(bool _forceDisable)
        {
            // Enable or disable custom decoding size depending on current state.
            // Custom decoding size is not compatible with tracking.
            // The boolean will later be used each time we attempt to change decoding size in StretchSqueezeSurface.
            // This is not concerned with decoding mode (prebuffering, caching, etc.) as this will be checked inside the reader.
            bool wasCustomDecodingSize = m_bEnableCustomDecodingSize;
            m_bEnableCustomDecodingSize = !m_FrameServer.Metadata.Tracking && !_forceDisable;
            
            if(wasCustomDecodingSize && !m_bEnableCustomDecodingSize)
            {
                m_FrameServer.VideoReader.DisableCustomDecodingSize();
                ResizeUpdate(true);
            }
            else if(!wasCustomDecodingSize && m_bEnableCustomDecodingSize)
            {
                ResizeUpdate(true);
            }
        }
        #endregion
        
        #region Timers & Playloop
        private void StartMultimediaTimer(int _interval)
        {
            //log.DebugFormat("starting playback timer at {0} ms interval.", _interval);
            ActivateKeyframe(-1);
            m_DropWatcher.Restart();
            m_LoopWatcher.Restart();
            
            Application.Idle += Application_Idle;
            m_FrameServer.VideoReader.BeforePlayloop();
            m_FrameServer.Metadata.PauseAutosave();

            uint eventType = NativeMethods.TIME_PERIODIC | NativeMethods.TIME_KILL_SYNCHRONOUS;
            m_IdMultimediaTimer = NativeMethods.timeSetEvent((uint)_interval, (uint)_interval, m_TimerCallback, UIntPtr.Zero, eventType);
            m_bIsCurrentlyPlaying = true;
        }
        private void StopMultimediaTimer()
        {
            if (m_IdMultimediaTimer != 0)
                NativeMethods.timeKillEvent(m_IdMultimediaTimer);
            m_IdMultimediaTimer = 0;
            m_bIsCurrentlyPlaying = false;
            Application.Idle -= Application_Idle;
            m_FrameServer.Metadata.UnpauseAutosave();
            
            log.DebugFormat("Rendering drops ratio: {0:0.00}", m_DropWatcher.Ratio);
            log.DebugFormat("Average rendering loop time: {0:0.000}ms", m_LoopWatcher.Average);
        }
        private void MultimediaTimer_Tick(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            if(!m_FrameServer.Loaded)
                return;
            
            // We cannot change the pointer to current here in case the UI is painting it,
            // so we will pass the number of drops along to the rendering.
            // The rendering will then ask for an update of the pointer to current, skipping as
            // many frames we missed during the interval.
            lock(m_TimingSync)
            {
                if(!m_bIsBusyRendering)
                {
                    int drops = m_RenderingDrops;
                    BeginInvoke((Action) delegate {Rendering_Invoked(drops);});
                    m_bIsBusyRendering = true;
                    m_RenderingDrops = 0;
                    m_DropWatcher.AddDropStatus(false);
                }
                else
                {
                    m_RenderingDrops++;
                    m_DropWatcher.AddDropStatus(true);
                }
            }
        }
        private void Rendering_Invoked(int missedFrames)
        {
            // This is in UI thread space.
            // Rendering in the context of continuous playback (play loop).
            m_TimeWatcher.Restart();

            bool tracking = m_FrameServer.Metadata.Tracking;
            int skip = tracking ? 0 : missedFrames;
            
            long estimateNext = m_iCurrentPosition + ((skip + 1) * m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);

            if (estimateNext > m_iSelEnd)
            {
                EndOfFile();
            }
            else
            {
                long oldPosition = m_iCurrentPosition;

                // This may be slow (several ms) due to delete call when dequeuing the pre-buffer. To investigate.
                m_FrameServer.VideoReader.MoveNext(skip, false);

                // In case the frame wasn't available in the pre-buffer, don't render anything.
                // This means if we missed the previous frame because the UI was busy, we won't 
                // render it now either. On the other hand, it means we will have less chance to
                // miss the next frame while trying to render an already outdated one.
                // We must also "unreset" the rendering drop counter, since we didn't actually render the frame.
                if(m_FrameServer.VideoReader.Drops > 0)
                {
                    if(m_FrameServer.VideoReader.Drops > m_MaxDecodingDrops)
                    {
                        log.DebugFormat("Failsafe triggered on Decoding Drops ({0})", m_FrameServer.VideoReader.Drops);
                        ForceSlowdown();
                    }
                    else
                    {
                       lock(m_TimingSync)
                            m_RenderingDrops = missedFrames;
                    }
                }
                else if(m_FrameServer.VideoReader.Current != null)
                {
                    DoInvalidate();
                    m_iCurrentPosition = m_FrameServer.VideoReader.Current.Timestamp;
                    
                    TrackDrawingsCommand.Execute(null);
                    ComputeOrStopTracking(skip == 0);
                    
                    // This causes Invalidates and will postpone the idle event.
                    // Update UI. For speed purposes, we don't update Selection Tracker hairline.
                    trkFrame.Position = m_iCurrentPosition;
                    trkFrame.UpdateCacheSegmentMarker(m_FrameServer.VideoReader.PreBufferingSegment);
                    trkFrame.Invalidate();
                    UpdateCurrentPositionLabel();
                    
                    ReportForSyncMerge();
                }

                if (m_iCurrentPosition < oldPosition && m_bSynched)
                {
                    // Sometimes the test to preemptively detect the end of file won't work.
                    StopPlaying();
                    ShowNextFrame(m_iSelStart, true);
                    UpdatePositionUI();
                    m_iFramesToDecode = 1;
                }
            }
        }
        private void EndOfFile()
        {
            m_FrameServer.Metadata.StopAllTracking();

            if(m_bSynched)
            {
                StopPlaying();
                ShowNextFrame(m_iSelStart, true);
            }
            else if(m_ePlayingMode == PlayingMode.Loop)
            {
                StopMultimediaTimer();
                bool rewound = ShowNextFrame(m_iSelStart, true);
                
                if(rewound)
                    StartMultimediaTimer(GetPlaybackFrameInterval());
                else
                    StopPlaying();
            }
            else
            {
                StopPlaying();
            }
            
            UpdatePositionUI();
            m_iFramesToDecode = 1;
        }
        private void ForceSlowdown()
        {
            m_FrameServer.VideoReader.ResetDrops();
            m_iFramesToDecode = 0;
            sldrSpeed.StepJump(-0.05);
        }
        private void ComputeOrStopTracking(bool _contiguous)
        {
            if(!m_FrameServer.Metadata.Tracking)
                return;
            
            // Fixme: Tracking only supports contiguous frames,
            // but this should be the responsibility of the track tool anyway.
            if (!_contiguous)
                m_FrameServer.Metadata.StopAllTracking();
            else
                m_FrameServer.Metadata.PerformTracking(m_FrameServer.VideoReader.Current);

            UpdateFramesMarkers();
            CheckCustomDecodingSize(false);
        }
        private void Application_Idle(object sender, EventArgs e)
        {
            // This event fires when the window has consumed all its messages.
            // Forcing the rendering to synchronize with this event allows
            // the UI to have a chance to process non-rendering related events like
            // button clicks, mouse move, etc.
            lock(m_TimingSync)
                m_bIsBusyRendering = false;

            m_TimeWatcher.LogTime("Back to idleness");
            //m_TimeWatcher.DumpTimes();
            m_LoopWatcher.AddLoopTime(m_TimeWatcher.RawTime("Back to idleness"));
        }
        private bool ShowNextFrame(long _iSeekTarget, bool _bAllowUIUpdate)
        {
            // TODO: More refactoring needed.
            // Eradicate the scheme where we use the _iSeekTarget parameter to mean two things.
            if(m_bIsCurrentlyPlaying)
                throw new ThreadStateException("ShowNextFrame called while play loop.");
            
            if(!m_FrameServer.VideoReader.Loaded)
                return false;
            
            bool refreshInPlace = _iSeekTarget == m_iCurrentPosition;
            bool hasMore = false;
            
            if(_iSeekTarget < 0)
                hasMore = m_FrameServer.VideoReader.MoveBy(m_iFramesToDecode, true);
            else
                hasMore = m_FrameServer.VideoReader.MoveTo(_iSeekTarget);
            
            if(m_FrameServer.VideoReader.Current != null)
            {
                m_iCurrentPosition = m_FrameServer.VideoReader.Current.Timestamp;
                
                TrackDrawingsCommand.Execute(null);
                    
                bool contiguous = _iSeekTarget < 0 && m_iFramesToDecode <= 1;
                if(!refreshInPlace)
                    ComputeOrStopTracking(contiguous);
                
                if(_bAllowUIUpdate) 
                    DoInvalidate();
                
                ReportForSyncMerge();
            }
            
            if(!hasMore)
            {
                // End of working zone reached.
                m_iCurrentPosition = m_iSelEnd;
                if(_bAllowUIUpdate)
                {
                    trkSelection.SelPos = m_iCurrentPosition;
                    DoInvalidate();
                }

                m_FrameServer.Metadata.StopAllTracking();
            }
            
            //m_Stopwatch.Stop();
            //log.Debug(String.Format("ShowNextFrame: {0} ms.", m_Stopwatch.ElapsedMilliseconds));
            
            return hasMore;
        }
        private void StopPlaying(bool _bAllowUIUpdate)
        {
            if (!m_FrameServer.Loaded || !m_bIsCurrentlyPlaying)
                return;
            
            StopMultimediaTimer();

            lock(m_TimingSync)
            {
                m_bIsBusyRendering = false;
                m_RenderingDrops = 0;
            }

            m_iFramesToDecode = 0;
            
            if (_bAllowUIUpdate)
            {
                buttonPlay.Image = Player.flatplay;
                DoInvalidate();
                UpdatePositionUI();
            }
        }
        private int GetPlaybackFrameInterval()
        {
            return (int)Math.Round(timeMapper.GetInterval(sldrSpeed.Value));
        }
        private void DeselectionTimer_OnTick(object sender, EventArgs e) 
        {
            if (m_FrameServer.Metadata.TextEditingInProgress)
            {
                // Ignore the timer if we are editing text, so we don't close the text editor under the user.
                m_DeselectionTimer.Stop();
                return;
            }
            
            // Deselect the currently selected drawing.
            // This is used for drawings that must show extra stuff for being transformed, but we 
            // don't want to show the extra stuff all the time for clarity.
            m_FrameServer.Metadata.UnselectAll();
            m_DeselectionTimer.Stop();
            DoInvalidate();
            OnPoke();
        }
        #endregion
        
        #region Culture
        private void ReloadMenusCulture()
        {
            // Reload the text for each menu.
            // this is done at construction time and at RefreshUICulture time.
            
            // 1. Default context menu.
            mnuDirectTrack.Text = ScreenManagerLang.mnuTrackTrajectory;
            mnuPasteDrawing.Text = ScreenManagerLang.mnuPasteDrawing;
            mnuPasteDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.PasteDrawing);
            mnuPlayPause.Text = ScreenManagerLang.mnuPlayPause;
            mnuSavePic.Text = ScreenManagerLang.Generic_SaveImage;
            mnuCopyPic.Text = ScreenManagerLang.mnuCopyImageToClipboard;
            mnuCopyPic.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CopyImage);
            mnuPastePic.Text = ScreenManagerLang.mnuPasteImage;
            mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
            mnuCloseScreen.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.Close);
            
            // 2. Drawings context menu.
            mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuSetStyleAsDefault.Text = ScreenManagerLang.mnuSetStyleAsDefault;
            mnuAlwaysVisible.Text = ScreenManagerLang.dlgConfigureFading_chkAlwaysVisible;
            mnuConfigureOpacity.Text = ScreenManagerLang.Generic_Opacity;
            mnuGotoKeyframe.Text = ScreenManagerLang.mnuGotoKeyframe;
            mnuCutDrawing.Text = ScreenManagerLang.Generic_Cut;
            mnuCutDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CutDrawing);
            mnuCopyDrawing.Text = ScreenManagerLang.Generic_Copy;
            mnuCopyDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CopyDrawing);
            mnuDeleteDrawing.Text = ScreenManagerLang.mnuDeleteDrawing;
            mnuDeleteDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.DeleteDrawing);

            mnuDrawingTracking.Text = ScreenManagerLang.dlgConfigureTrajectory_Tracking;
            mnuDrawingTrackingConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuDrawingTrackingStart.Text = ScreenManagerLang.mnuDrawingTrackingStart;
            mnuDrawingTrackingStop.Text = ScreenManagerLang.mnuDrawingTrackingStop;

            // 3. Tracking pop menu (Restart, Stop tracking)
            mnuStopTracking.Text = ScreenManagerLang.mnuStopTracking;
            mnuRestartTracking.Text = ScreenManagerLang.mnuRestartTracking;
            mnuDeleteTrajectory.Text = ScreenManagerLang.mnuDeleteTrajectory;
            mnuDeleteTrajectory.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.DeleteDrawing);
            mnuDeleteEndOfTrajectory.Text = ScreenManagerLang.mnuDeleteEndOfTrajectory;
            mnuConfigureTrajectory.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            
            // 4. Chrono pop menu (Start, Stop, Hide, etc.)
            mnuChronoConfigure.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuChronoStart.Text = ScreenManagerLang.mnuChronoStart;
            mnuChronoStop.Text = ScreenManagerLang.mnuChronoStop;
            mnuChronoHide.Text = ScreenManagerLang.mnuChronoHide;
            mnuChronoCountdown.Text = ScreenManagerLang.mnuChronoCountdown;
            mnuChronoDelete.Text = ScreenManagerLang.mnuChronoDelete;
            mnuChronoDelete.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.DeleteDrawing);

            // 5. Magnifier
            foreach (ToolStripMenuItem m in maginificationMenus)
            {
                double factor = (double)m.Tag;
                m.Text = String.Format(ScreenManagerLang.mnuMagnification, factor.ToString());
            }
            mnuMagnifierTrack.Text = ScreenManagerLang.mnuTrackTrajectory;
            mnuMagnifierDirect.Text = ScreenManagerLang.mnuMagnifierDirect;
            mnuMagnifierQuit.Text = ScreenManagerLang.mnuMagnifierQuit;
        }
        private void ReloadTooltipsCulture()
        {
            // Video controls
            toolTips.SetToolTip(buttonPlay, ScreenManagerLang.ToolTip_Play);
            toolTips.SetToolTip(buttonGotoPrevious, ScreenManagerLang.ToolTip_Back);
            toolTips.SetToolTip(buttonGotoNext, ScreenManagerLang.ToolTip_Next);
            toolTips.SetToolTip(buttonGotoFirst, ScreenManagerLang.ToolTip_First);
            toolTips.SetToolTip(buttonGotoLast, ScreenManagerLang.ToolTip_Last);
            if (m_ePlayingMode == PlayingMode.Once)
            {
                toolTips.SetToolTip(btnPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Once);
            }
            else
            {
                toolTips.SetToolTip(btnPlayingMode, ScreenManagerLang.ToolTip_PlayingMode_Loop);
            }
            
            // Export buttons
            toolTips.SetToolTip(btnSnapShot, ScreenManagerLang.Generic_SaveImage);
            toolTips.SetToolTip(btnRafale, ScreenManagerLang.ToolTip_Rafale);
            toolTips.SetToolTip(btnDiaporama, ScreenManagerLang.ToolTip_SaveDiaporama);
            toolTips.SetToolTip(btnSaveVideo, ScreenManagerLang.dlgSaveVideoTitle);
            toolTips.SetToolTip(btnPausedVideo, ScreenManagerLang.ToolTip_SavePausedVideo);
            
            // Working zone and sliders.
            if (m_bHandlersLocked)
            {
                toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionUnlock);
            }
            else
            {
                toolTips.SetToolTip(btn_HandlersLock, ScreenManagerLang.LockSelectionLock);
            }
            toolTips.SetToolTip(btnSetHandlerLeft, ScreenManagerLang.ToolTip_SetHandlerLeft);
            toolTips.SetToolTip(btnSetHandlerRight, ScreenManagerLang.ToolTip_SetHandlerRight);
            toolTips.SetToolTip(btnHandlersReset, ScreenManagerLang.ToolTip_ResetWorkingZone);
            trkSelection.ToolTip = ScreenManagerLang.ToolTip_trkSelection;
        }
        private void ReloadToolsCulture()
        {
            foreach(ToolStripItem tsi in stripDrawingTools.Items)
            {
                if(tsi is ToolStripSeparator)
                    continue;
                
                if(tsi is ToolStripButtonWithDropDown)
                {
                    foreach(ToolStripItem subItem in ((ToolStripButtonWithDropDown)tsi).DropDownItems)
                    {
                        if(!(subItem is ToolStripMenuItem))
                            continue;
                        
                        AbstractDrawingTool tool = subItem.Tag as AbstractDrawingTool;
                        if(tool != null)
                        {
                            subItem.Text = tool.DisplayName;
                            subItem.ToolTipText = tool.DisplayName;
                        }
                    }
                    
                    ((ToolStripButtonWithDropDown)tsi).UpdateToolTip();
                }
                else if(tsi is ToolStripButton)
                {
                    AbstractDrawingTool tool = tsi.Tag as AbstractDrawingTool;
                    if(tool != null)
                        tsi.ToolTipText = tool.DisplayName;
                }
            }
            
            m_btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
            m_btnShowComments.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
            m_btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
        }
        #endregion

        #region SurfaceScreen Events
        private void SurfaceScreen_MouseDown(object sender, MouseEventArgs e)
        {
            if(!m_FrameServer.Loaded)
                return;
                
            m_DeselectionTimer.Stop();
            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
            
            if (e.Button == MouseButtons.Left)
                SurfaceScreen_LeftDown();
            else if (e.Button == MouseButtons.Right)
                SurfaceScreen_RightDown();
            else if (e.Button == MouseButtons.Middle)
                SurfaceScreen_MiddleDown();

            DoInvalidate();
        }
        private void SurfaceScreen_LeftDown()
        {
            bool hitMagnifier = false;
            if(m_ActiveTool == m_PointerTool)
            {
                hitMagnifier = m_FrameServer.Metadata.Magnifier.OnMouseDown(m_DescaledMouse, m_FrameServer.Metadata.ImageTransform);
                if (hitMagnifier)
                    SetCursor(CursorManager.GetManipulationCursorMagnifier());
            }
                
            if (hitMagnifier || InteractiveFiltering)
                return;
            
            if (m_bIsCurrentlyPlaying)
            {
                // MouseDown while playing: Halt the video.
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);
                ToastPause();
            }
            
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();
            
            if (m_ActiveTool == m_PointerTool)
            {
                m_PointerTool.OnMouseDown(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition, PreferencesManager.PlayerPreferences.DefaultFading.Enabled);

                if (m_FrameServer.Metadata.HitDrawing != null)
                    SetCursor(CursorManager.GetManipulationCursor(m_FrameServer.Metadata.HitDrawing));
                else
                    SetCursor(m_PointerTool.GetCursor(1));
            }
            else if(m_ActiveTool == ToolManager.Tools["Spotlight"])
            {
                CreateNewMultiDrawingItem(m_FrameServer.Metadata.SpotlightManager);
            }
            else if(m_ActiveTool == ToolManager.Tools["AutoNumbers"])
            {
                CreateNewMultiDrawingItem(m_FrameServer.Metadata.AutoNumberManager);
            }
            else if (m_ActiveTool == ToolManager.Tools["Chrono"])
            {
                CreateNewDrawing(m_FrameServer.Metadata.ChronoManager.Id);
            }
            else 
            {
                // Note: if the active drawing is at initialization stage, it will receive the point commit during mouse up.
                if (!m_FrameServer.Metadata.DrawingInitializing)
                {
                    AddKeyframe();
                    CreateNewDrawing(m_FrameServer.Metadata.GetKeyframeId(m_iActiveKeyFrameIndex));
                }
            }
        }
        private void SurfaceScreen_MiddleDown()
        {
            // Middle mouse button is used to pan the image or move a drawing while the active tool is not the hand tool.
            if (m_bIsCurrentlyPlaying)
            {
                // MouseDown while playing: Halt the video.
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);
                ToastPause();
            }

            m_PointerTool.OnMouseDown(m_FrameServer.Metadata, m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition, PreferencesManager.PlayerPreferences.DefaultFading.Enabled);
            if (m_FrameServer.Metadata.HitDrawing != null)
                SetCursor(CursorManager.GetManipulationCursor(m_FrameServer.Metadata.HitDrawing));
            else
                SetCursor(m_PointerTool.GetCursor(1));
        }
        private void CreateNewDrawing(Guid managerId)
        {
            m_FrameServer.Metadata.UnselectAll();

            IImageToViewportTransformer transformer = m_FrameServer.Metadata.ImageTransform;
            bool zooming = m_FrameServer.Metadata.ImageTransform.Zooming;
            DistortionHelper distorter = m_FrameServer.Metadata.CalibrationHelper.DistortionHelper;

            // Special case for the text tool: if we hit on another label we go into edit mode instead of adding a new one on top of it.
            bool editingLabel = false;
            if (m_ActiveTool == ToolManager.Tools["Label"])
            {
                foreach (DrawingText label in m_FrameServer.Metadata.Labels())
                {
                    int hit = label.HitTest(m_DescaledMouse, m_iCurrentPosition, distorter, transformer, zooming);
                    if (hit < 0)
                        continue;
                    
                    label.SetEditMode(true, m_DescaledMouse, m_FrameServer.ImageTransform);
                    editingLabel = true;
                    break;
                }
            }
           
            if (!editingLabel)
            {
                AbstractDrawing drawing = m_ActiveTool.GetNewDrawing(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame, m_FrameServer.Metadata.ImageTransform);
                if (DrawingAdding != null)
                    DrawingAdding(this, new DrawingEventArgs(drawing, managerId));
            }
        }
        private void AfterDrawingAdded(AbstractDrawing drawing)
        {
            if (drawing is DrawingText)
            {
                DrawingText drawingText = drawing as DrawingText;
                drawingText.InitializeText();
                ImportEditbox(drawingText);
            }

            if (drawing is DrawingTrack)
            {
                ((DrawingTrack)drawing).ClosestFrameDisplayer = OnShowClosestFrame;
                
                // TODO: move this to a tool.
                m_ActiveTool = m_PointerTool;
                SetCursor(m_PointerTool.GetCursor(0));
            }

            if (!m_FrameServer.Metadata.KVAImporting)
            {
                m_FrameServer.Metadata.UpdateTrackPoint(m_FrameServer.CurrentImage);
                UpdateFramesMarkers();
                RefreshImage();
            }
        }
        private void AfterDrawingModified(AbstractDrawing drawing)
        {
            UpdateFramesMarkers();
            RefreshImage();
        }
        private void ImportEditboxes()
        {
            // Import edit boxes of all drawing text after a KVA import.
            foreach (DrawingText drawingText in m_FrameServer.Metadata.Labels())
            {
                ImportEditbox(drawingText);
            }
        }
        private void ImportEditbox(DrawingText drawing)
        {
            if (panelCenter.Controls.Contains(drawing.EditBox))
                return;

            drawing.ContainerScreen = pbSurfaceScreen;
            panelCenter.Controls.Add(drawing.EditBox);
            drawing.EditBox.BringToFront();
            drawing.EditBox.Focus();
            drawing.EditBox.Tag = this;
        }
        private void AfterDrawingDeleted()
        {
            if (!m_FrameServer.Metadata.KVAImporting)
            {
                UpdateFramesMarkers();
                RefreshImage();
            }
        }
        private void CreateNewMultiDrawingItem(AbstractMultiDrawing manager)
        {
            m_FrameServer.Metadata.UnselectAll();
            AddKeyframe();

            AbstractMultiDrawingItem item = manager.GetNewItem(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.Metadata.AverageTimeStampsPerFrame);

            if (MultiDrawingItemAdding != null)
                MultiDrawingItemAdding(this, new MultiDrawingItemEventArgs(item, manager));
        }
        private void AfterMultiDrawingItemAdded()
        {
            if (!m_FrameServer.Metadata.KVAImporting)
                RefreshImage();
        }
        private void AfterMultiDrawingItemDeleted()
        {
            if (!m_FrameServer.Metadata.KVAImporting)
                RefreshImage();
        }
        private void SurfaceScreen_RightDown()
        {
            // Show the right Pop Menu depending on context.
            // (Drawing, Trajectory, Chronometer, Magnifier, Nothing)
            if (m_bIsCurrentlyPlaying)
            {
                mnuDirectTrack.Visible = false;
                mnuPasteDrawing.Visible = false;
                mnuPastePic.Visible = false;
                panelCenter.ContextMenuStrip = popMenu;
                return;
            }
            
            m_FrameServer.Metadata.UnselectAll();
            AbstractDrawing hitDrawing = null;
                
            if(InteractiveFiltering)
            {
                mnuDirectTrack.Visible = false;
                mnuPasteDrawing.Visible = false;
                mnuPastePic.Visible = false;
                panelCenter.ContextMenuStrip = popMenu;
            }
            else if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
            {
                AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
                PrepareDrawingContextMenu(drawing, popMenuDrawings);
                
                popMenuDrawings.Items.Add(mnuDeleteDrawing);
                panelCenter.ContextMenuStrip = popMenuDrawings;
            }
            else if ((hitDrawing = m_FrameServer.Metadata.IsOnExtraDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
            { 
                // Unlike attached drawings, each extra drawing type has its own context menu for now.
                // TODO: Maybe we could use the custom menus system to host these menus in the drawing instead of here.
                // Only the drawing itself knows what to do upon click anyway.
                
                if(hitDrawing is DrawingChrono)
                {
                    // Toggle to countdown is active only if we have a stop time.
                    mnuChronoCountdown.Enabled = ((DrawingChrono)hitDrawing).HasTimeStop;
                    mnuChronoCountdown.Checked = ((DrawingChrono)hitDrawing).CountDown;
                    panelCenter.ContextMenuStrip = popMenuChrono;
                }
                else if(hitDrawing is DrawingTrack)
                {
                    DrawingTrack track = (DrawingTrack)hitDrawing;
                    popMenuTrack.Items.Clear();
                    popMenuTrack.Items.Add(mnuConfigureTrajectory);
                    
                    bool customMenus = AddDrawingCustomMenus(hitDrawing, popMenuTrack.Items);
                    if (customMenus)
                        popMenuTrack.Items.Add(new ToolStripSeparator());
                    
                    popMenuTrack.Items.AddRange(new ToolStripItem[] { mnuStopTracking, mnuRestartTracking, new ToolStripSeparator(), mnuDeleteEndOfTrajectory, mnuDeleteTrajectory });

                    if (track.Status == TrackStatus.Edit)
                    {
                        mnuStopTracking.Visible = true;
                        mnuRestartTracking.Visible = false;
                    }
                    else
                    {
                        mnuStopTracking.Visible = false;
                        mnuRestartTracking.Visible = true;
                    }	
                    
                    panelCenter.ContextMenuStrip = popMenuTrack;
                }
                else if(hitDrawing is DrawingCoordinateSystem)
                {
                    PrepareDrawingContextMenu(hitDrawing, popMenuDrawings);
                    panelCenter.ContextMenuStrip = popMenuDrawings;
                }
                else if(hitDrawing is AbstractMultiDrawing)
                {
                    PrepareDrawingContextMenu(hitDrawing, popMenuDrawings);
                    popMenuDrawings.Items.Add(mnuDeleteDrawing);
                    panelCenter.ContextMenuStrip = popMenuDrawings;
                }
            }
            else if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Indirect && 
                     m_FrameServer.Metadata.Magnifier.IsOnObject(m_DescaledMouse, m_FrameServer.Metadata.ImageTransform))
            {
                mnuMagnifierTrack.Checked = ToggleTrackingCommand.CurrentState(m_FrameServer.Metadata.Magnifier);
                panelCenter.ContextMenuStrip = popMenuMagnifier;
            }
            else if(m_ActiveTool != m_PointerTool)
            {
                // Launch FormToolPreset.
                FormToolPresets ftp = new FormToolPresets(m_ActiveTool);
                FormsHelper.Locate(ftp);
                ftp.ShowDialog();
                ftp.Dispose();
                UpdateCursor();
            }
            else
            {
                // No drawing touched and no tool selected, but not currently playing. Default menu.
                mnuDirectTrack.Visible = true;
                mnuPasteDrawing.Visible = true;
                mnuPasteDrawing.Enabled = DrawingClipboard.HasContent;
                mnuPastePic.Visible = true;
                mnuPastePic.Enabled = Clipboard.ContainsImage();
                panelCenter.ContextMenuStrip = popMenu;
            }
        }
        private void PrepareDrawingContextMenu(AbstractDrawing drawing, ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();

            if (!m_FrameServer.Metadata.DrawingInitializing)
                PrepareDrawingContextMenuCapabilities(drawing, popMenu);

            if (popMenu.Items.Count > 0)
                popMenu.Items.Add(mnuSepDrawing);

            bool hasExtraMenus = AddDrawingCustomMenus(drawing, popMenu.Items);

            if (!m_FrameServer.Metadata.DrawingInitializing && drawing.InfosFading != null)
            {
                bool gotoVisible = (PreferencesManager.PlayerPreferences.DefaultFading.Enabled && (drawing.InfosFading.ReferenceTimestamp != m_iCurrentPosition));
                if (gotoVisible)
                {
                    popMenu.Items.Add(mnuGotoKeyframe);
                    hasExtraMenus = true;
                }
            }

            if (hasExtraMenus)
                popMenu.Items.Add(mnuSepDrawing2);

            if (drawing.IsCopyPasteable)
            {
                popMenuDrawings.Items.Add(mnuCutDrawing);
                popMenuDrawings.Items.Add(mnuCopyDrawing);
                popMenuDrawings.Items.Add(mnuSepDrawing3);
            }
        }

        private void PrepareDrawingContextMenuCapabilities(AbstractDrawing drawing, ContextMenuStrip popMenu)
        {
            // Generic context menu from drawing capabilities.
            if((drawing.Caps & DrawingCapabilities.ConfigureColor) == DrawingCapabilities.ConfigureColor ||
               (drawing.Caps & DrawingCapabilities.ConfigureColorSize) == DrawingCapabilities.ConfigureColorSize)
            {
                mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
                popMenu.Items.Add(mnuConfigureDrawing);
                
                mnuSetStyleAsDefault.Text = ScreenManagerLang.mnuSetStyleAsDefault;
                popMenu.Items.Add(mnuSetStyleAsDefault);
            }
            
            if(PreferencesManager.PlayerPreferences.DefaultFading.Enabled && ((drawing.Caps & DrawingCapabilities.Fading) == DrawingCapabilities.Fading))
            {
                mnuAlwaysVisible.Checked = drawing.InfosFading.AlwaysVisible;
                popMenu.Items.Add(mnuAlwaysVisible);
            }
            
            if((drawing.Caps & DrawingCapabilities.Opacity) == DrawingCapabilities.Opacity)
            {
                popMenu.Items.Add(mnuConfigureOpacity);
            }
            
            if((drawing.Caps & DrawingCapabilities.Track) == DrawingCapabilities.Track)
            {
                bool tracked = ToggleTrackingCommand.CurrentState(drawing);
                mnuDrawingTrackingStart.Visible = !tracked;
                mnuDrawingTrackingStop.Visible = tracked;
                popMenu.Items.Add(mnuDrawingTracking);
            }
        }
        private bool AddDrawingCustomMenus(AbstractDrawing drawing, ToolStripItemCollection menuItems)
        {
            bool hasExtraMenu = (drawing.ContextMenu != null && drawing.ContextMenu.Count > 0);
            if(!hasExtraMenu)
                return false;
            
            foreach(ToolStripItem tsmi in drawing.ContextMenu)
            {
                ToolStripMenuItem menuItem = tsmi as ToolStripMenuItem;

                // Inject a dependency on this screen into the drawing.
                // Since the drawing now owns a piece of the UI, it may need to call back into functions here.
                // This is used to invalidate the view and complete operations that are normally handled here and 
                // require calls to other objects that the drawing itself doesn't have access to, like when the 
                // polyline drawing handles InitializeEnd and needs to remove the last point added to tracking.
                tsmi.Tag = this;

                // Also inject for all the sub menus.
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem subMenu in menuItem.DropDownItems)
                        subMenu.Tag = this;
                }
                
                if (tsmi.MergeIndex >= 0)
                    menuItems.Insert(tsmi.MergeIndex, tsmi);
                else
                    menuItems.Add(tsmi);
            }
            
            return true;
        }
        private void SurfaceScreen_MouseMove(object sender, MouseEventArgs e)
        {
            // We must keep the same Z order.
            // 1:Magnifier, 2:Drawings, 3:Chronos/Tracks
            // When creating a drawing, the active tool will stay on this drawing until its setup is over.
            // After the drawing is created, we either fall back to Pointer tool or stay on the same tool.

            if(!m_FrameServer.Loaded)
                return;

            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
                        
            if (e.Button == MouseButtons.None && m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
            {
                m_FrameServer.Metadata.Magnifier.Move(m_DescaledMouse);
                
                if (!m_bIsCurrentlyPlaying)
                    DoInvalidate();
            }
            else if (e.Button == MouseButtons.None && m_FrameServer.Metadata.DrawingInitializing)
            {
                IInitializable initializableDrawing = m_FrameServer.Metadata.HitDrawing as IInitializable;
                if (initializableDrawing != null)
                {
                    initializableDrawing.InitializeMove(m_DescaledMouse, ModifierKeys);
                    DoInvalidate();
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (m_ActiveTool != m_PointerTool)
                {
                    // Tools that are not IInitializable should reset to Pointer tool right after creation.

                    if (m_ActiveTool == ToolManager.Tools["Spotlight"])
                    {
                        IInitializable initializableDrawing = m_FrameServer.Metadata.SpotlightManager as IInitializable;
                        initializableDrawing.InitializeMove(m_DescaledMouse, ModifierKeys);
                    }
                    else if (!m_bIsCurrentlyPlaying && m_iActiveKeyFrameIndex >= 0 && m_FrameServer.Metadata.HitDrawing != null)
                    {
                        IInitializable initializableDrawing = m_FrameServer.Metadata.HitDrawing as IInitializable;
                        if (initializableDrawing != null)
                            initializableDrawing.InitializeMove(m_DescaledMouse, ModifierKeys);
                    }
                }
                else
                {
                    bool bMovingMagnifier = false;
                    if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Indirect)
                    {
                        bMovingMagnifier = m_FrameServer.Metadata.Magnifier.Move(m_DescaledMouse);
                    }

                    if (!bMovingMagnifier && m_ActiveTool == m_PointerTool)
                    {
                        if (!m_bIsCurrentlyPlaying)
                        {
                            // Magnifier is not being moved or is invisible, try drawings through pointer tool.
                            // (including chronos, tracks and grids)
                            bool bMovingObject = m_PointerTool.OnMouseMove(m_FrameServer.Metadata, m_DescaledMouse, m_FrameServer.ImageTransform.Location, ModifierKeys);

                            if (!bMovingObject)
                            {
                                // User is not moving anything: move the whole image.
                                // This may not have any effect if we try to move outside the original size and not in "free move" mode.

                                // Get mouse deltas (descaled=in image coords).
                                double fDeltaX = (double)m_PointerTool.MouseDelta.X;
                                double fDeltaY = (double)m_PointerTool.MouseDelta.Y;

                                if (m_FrameServer.Metadata.Mirrored)
                                {
                                    fDeltaX = -fDeltaX;
                                }

                                m_FrameServer.ImageTransform.MoveZoomWindow(fDeltaX, fDeltaY);
                            }
                        }
                    }
                }

                if (!m_bIsCurrentlyPlaying)
                {
                    DoInvalidate();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Middle mouse button: allow to move stuff even if we have a tool selected.
                // This allow to zoom and pan while having an active tool.
                if (!m_bIsCurrentlyPlaying)
                {
                    bool bMovingObject = m_PointerTool.OnMouseMove(m_FrameServer.Metadata, m_DescaledMouse, m_FrameServer.ImageTransform.Location, ModifierKeys);
                    if (!bMovingObject)
                    {
                        // Move the whole image.
                        double fDeltaX = (double)m_PointerTool.MouseDelta.X;
                        double fDeltaY = (double)m_PointerTool.MouseDelta.Y;
                        if (m_FrameServer.Metadata.Mirrored)
                            fDeltaX = -fDeltaX;

                        m_FrameServer.ImageTransform.MoveZoomWindow(fDeltaX, fDeltaY);
                    }

                    DoInvalidate();
                }
            }
        }
        private void SurfaceScreen_MouseUp(object sender, MouseEventArgs e)
        {
            // End of an action.
            // Depending on the active tool we have various things to do.
            
            if (!m_FrameServer.Loaded)
                return;

            if (e.Button == MouseButtons.Middle)
            {
                // Special case where we move around even with an active tool.
                // On mouse up we need to restore the cursor of the active tool.
                UpdateCursor();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;
            
            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
            
            if (m_ActiveTool == m_PointerTool)
            {
                OnPoke();
                m_FrameServer.Metadata.UpdateTrackPoint(m_FrameServer.CurrentImage);
                ReportForSyncMerge();
            }

            m_FrameServer.Metadata.InitializeCommit(m_FrameServer.VideoReader.Current, m_DescaledMouse);

            if (m_bTextEdit && m_ActiveTool != m_PointerTool && m_iActiveKeyFrameIndex >= 0)
                m_bTextEdit = false;

            // The fact that we stay on this tool or fall back to pointer tool, depends on the tool.
            m_ActiveTool = m_ActiveTool.KeepTool ? m_ActiveTool : m_PointerTool;
            
            if (m_ActiveTool == m_PointerTool)
            {
                SetCursor(m_PointerTool.GetCursor(0));
                m_PointerTool.OnMouseUp();
                
                // If we were resizing an SVG drawing, trigger a render.
                // TODO: this is currently triggered on every mouse up, not only on resize !
                DrawingSVG d = m_FrameServer.Metadata.HitDrawing as DrawingSVG;
                if(d != null)
                        d.ResizeFinished();
            }

            if (m_FrameServer.Metadata.HitDrawing != null && !m_FrameServer.Metadata.DrawingInitializing)
                m_DeselectionTimer.Start();
            
            DoInvalidate();
        }
        private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(!m_FrameServer.Loaded || e.Button != MouseButtons.Left || m_ActiveTool != m_PointerTool)
                return;
                
            OnPoke();
            
            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();
            m_FrameServer.Metadata.UnselectAll();
            
            AbstractDrawing hitDrawing = null;
            
            //------------------------------------------------------------------------------------
            // - If on text, switch to edit mode.
            // - If on other drawing, launch the configuration dialog.
            // - Otherwise -> Maximize/Reduce image.
            //------------------------------------------------------------------------------------
            if(InteractiveFiltering)
            {
                ToggleImageFillMode();
            }
            else if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
            {
                // Double click on a drawing:
                // turn text tool into edit mode, launch config for others.
                AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
                if (drawing is DrawingText)
                {
                    ((DrawingText)drawing).SetEditMode(true, m_DescaledMouse, m_FrameServer.ImageTransform);
                    m_ActiveTool = ToolManager.Tools["Label"];
                    m_bTextEdit = true;
                }
                else
                {
                    mnuConfigureDrawing_Click(null, EventArgs.Empty);
                }
            }
            else if((hitDrawing = m_FrameServer.Metadata.IsOnExtraDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
            {
                if(hitDrawing is DrawingChrono)
                {
                    mnuChronoConfigure_Click(null, EventArgs.Empty);	
                }
                else if(hitDrawing is DrawingTrack)
                {
                    mnuConfigureTrajectory_Click(null, EventArgs.Empty);	
                }
            }
            else
            {
                ToggleImageFillMode();
            }
        }
        private void SurfaceScreen_Paint(object sender, PaintEventArgs e)
        {
            //-------------------------------------------------------------------
            // We always draw at full SurfaceScreen size.
            // It is the SurfaceScreen itself that is resized if needed.
            //-------------------------------------------------------------------
            if(!m_FrameServer.Loaded || saveInProgress || m_DualSaveInProgress)
                return;
            
            m_TimeWatcher.LogTime("Actual start of paint");
            
            if(InteractiveFiltering)
            {
                m_InteractiveEffect.Draw(e.Graphics, m_FrameServer.VideoReader.WorkingZoneFrames);
            }
            else if(m_FrameServer.CurrentImage != null)
            {
                try
                {
                    // If we are on a keyframe, see if it has any drawing.
                    int iKeyFrameIndex = -1;
                    if (m_iActiveKeyFrameIndex >= 0)
                    {
                        if (m_FrameServer.Metadata[m_iActiveKeyFrameIndex].Drawings.Count > 0)
                        {
                            iKeyFrameIndex = m_iActiveKeyFrameIndex;
                        }
                    }

                    FlushOnGraphics(m_FrameServer.CurrentImage, e.Graphics, m_viewportManipulator.RenderingSize, iKeyFrameIndex, m_iCurrentPosition, m_FrameServer.ImageTransform);
                    
                    if(m_MessageToaster.Enabled)
                        m_MessageToaster.Draw(e.Graphics);
                   
                    //log.DebugFormat("play loop to end of paint: {0}/{1}", m_Stopwatch.ElapsedMilliseconds, m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds);
                }
                catch (System.InvalidOperationException)
                {
                    log.Error("Error while painting image. Object is currently in use elsewhere. ATI Drivers ?");
                }
                catch (Exception exp)
                {
                    log.Error("Error while painting image.");
                    log.Error(exp.Message);
                    log.Error(exp.StackTrace);
                    
                    #if DEBUG
                    //throw;
                    #endif
                }
            }
            else
            {
                log.Error("Painting screen - no image to display.");
            }
            
            // Draw Selection Border if needed.
            if (m_bShowImageBorder)
            {
                DrawImageBorder(e.Graphics);
            }
            
            m_TimeWatcher.LogTime("Finished painting.");
        }
        private void SurfaceScreen_MouseEnter(object sender, EventArgs e)
        {
            // Set focus to surfacescreen to enable mouse scroll
            if (!m_FrameServer.Metadata.TextEditingInProgress)
                pbSurfaceScreen.Focus();
        }
        private void FlushOnGraphics(Bitmap _sourceImage, Graphics g, Size _renderingSize, int _iKeyFrameIndex, long _iPosition, ImageTransform _transform)
        {
            // This function is used both by the main rendering loop and by image export functions.
            // Video export get its image from the VideoReader or the cache.

            // Notes on performances:
            // - The global performance depends on the size of the *source* image. Not destination.
            //   (rendering 1 pixel from an HD source will still be slow)
            // - Using a matrix transform instead of the buit in interpolation doesn't seem to do much.
            // - InterpolationMode has a sensible effect. but can look ugly for lowest values.
            // - Using unmanaged BitBlt or StretchBlt doesn't seem to do much... (!?)
            // - the scaling and interpolation better be done directly from ffmpeg. (cut on memory usage too)
            // - furthermore ffmpeg has a mode called 'FastBilinear' that seems more promising.
            // - Drawing unscaled avoid the interpolation altogether and provide ~8x perfs.
            
            // 1. Image
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            //g.CompositingQuality = CompositingQuality.HighSpeed;
            //g.InterpolationMode = InterpolationMode.Bilinear;
            //g.InterpolationMode = InterpolationMode.NearestNeighbor;
            //g.SmoothingMode = SmoothingMode.None;
            
            m_TimeWatcher.LogTime("Before DrawImage");
            
            if(m_viewportManipulator.MayDrawUnscaled && m_FrameServer.VideoReader.CanDrawUnscaled)
            {
                // Source image should be at the right size, unless it has been temporarily disabled.
                if (_transform.RenderingZoomWindow.Size.CloseTo(_renderingSize) && !m_FrameServer.Metadata.Mirrored)
                {
                    if (!_transform.Zooming)
                    {
                        g.DrawImageUnscaled(_sourceImage, 0, 0);
                        //log.DebugFormat("draw unscaled.");
                    }
                    else
                    {
                        int left = -_transform.RenderingZoomWindow.Left;
                        int top = -_transform.RenderingZoomWindow.Top;
                        g.DrawImageUnscaled(_sourceImage, left, top);
                        //log.DebugFormat("draw unscaled with zoom.");
                    }
                }
                else
                {
                    // Image was decoded at customized size, but can't be rendered unscaled.
                    Rectangle rDst;
                    if(m_FrameServer.Metadata.Mirrored)
                        rDst = new Rectangle(_renderingSize.Width, 0, -_renderingSize.Width, _renderingSize.Height);
                    else
                        rDst = new Rectangle(0, 0, _renderingSize.Width, _renderingSize.Height);
                    
                    // TODO: integrate the mirror flag into the ImageTransform.

                    g.DrawImage(_sourceImage, rDst, _transform.RenderingZoomWindow, GraphicsUnit.Pixel);
                    //log.DebugFormat("draw scaled at custom decoding size.");
                }
            }
            else
            {
                if (!_transform.Zooming && !m_FrameServer.Metadata.Mirrored && _transform.Stretch == 1.0f)
                {
                    // This allow to draw unscaled while tracking or caching for example, provided we are rendering at original size.
                    g.DrawImageUnscaled(_sourceImage, 0, 0);
                    //log.DebugFormat("drawing unscaled because at the right size.");
                }
                else
                {
                    Rectangle rDst;
                    if(m_FrameServer.Metadata.Mirrored)
                        rDst = new Rectangle(_renderingSize.Width, 0, -_renderingSize.Width, _renderingSize.Height);
                    else
                        rDst = new Rectangle(0, 0, _renderingSize.Width, _renderingSize.Height);
                    
                    g.DrawImage(_sourceImage, rDst, _transform.RenderingZoomWindow, GraphicsUnit.Pixel);
                    //log.DebugFormat("drawing scaled.");
                }
            }
            
            m_TimeWatcher.LogTime("After DrawImage");
            
            // .Sync superposition.
            if(m_bSynched && m_bSyncMerge && m_SyncMergeImage != null)
            {
                // The mirroring, if any, will have been done already and applied to the sync image.
                // (because to draw the other image, we take into account its own mirroring option,
                // not the option in this screen.)
                Rectangle rSyncDst = new Rectangle(0, 0, _renderingSize.Width, _renderingSize.Height);
                g.DrawImage(m_SyncMergeImage, rSyncDst, 0, 0, m_SyncMergeImage.Width, m_SyncMergeImage.Height, GraphicsUnit.Pixel, m_SyncMergeImgAttr);
            }
            
            if ((m_bIsCurrentlyPlaying && PreferencesManager.PlayerPreferences.DrawOnPlay) || !m_bIsCurrentlyPlaying)
            {
                FlushDrawingsOnGraphics(g, _transform, _iKeyFrameIndex, _iPosition);
                FlushMagnifierOnGraphics(_sourceImage, g, _transform);
            }
        }
        private void FlushDrawingsOnGraphics(Graphics canvas, ImageTransform transformer, int keyFrameIndex, long timestamp)
        {
            DistortionHelper distorter = m_FrameServer.Metadata.CalibrationHelper.DistortionHelper;

            // Prepare for drawings
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            foreach (DrawingChrono chrono in m_FrameServer.Metadata.ChronoManager.Drawings)
            {
                bool selected = m_FrameServer.Metadata.HitDrawing == chrono;
                chrono.Draw(canvas, distorter, transformer, selected, timestamp);
            }

            foreach (DrawingTrack track in m_FrameServer.Metadata.TrackManager.Drawings)
            {
                bool selected = m_FrameServer.Metadata.HitDrawing == track;
                track.Draw(canvas, distorter, transformer, selected, timestamp);
            }

            foreach (AbstractDrawing drawing in m_FrameServer.Metadata.ExtraDrawings)
            {
                bool selected = m_FrameServer.Metadata.HitDrawing == drawing;
                drawing.Draw(canvas, distorter, transformer, selected, timestamp);
            }
            
            if (PreferencesManager.PlayerPreferences.DefaultFading.Enabled)
            {
                // If fading is on, we ask all drawings to draw themselves with their respective
                // fading factor for this position.

                int[] zOrder = m_FrameServer.Metadata.GetKeyframesZOrder(timestamp);

                // Draw in reverse keyframes z order so the closest next keyframe gets drawn on top (last).
                for (int kfIndex = zOrder.Length - 1; kfIndex >= 0; kfIndex--)
                {
                    Keyframe keyframe = m_FrameServer.Metadata.Keyframes[zOrder[kfIndex]];
                    for (int drawingIndex = keyframe.Drawings.Count - 1; drawingIndex >= 0; drawingIndex--)
                    {
                        bool selected = keyframe.Drawings[drawingIndex] == m_FrameServer.Metadata.HitDrawing;
                        keyframe.Drawings[drawingIndex].Draw(canvas, distorter, transformer, selected, timestamp);
                    }
                }
            }
            else if (keyFrameIndex >= 0)
            {
                // if fading is off, only draw the current keyframe.
                // Draw all drawings in reverse order to get first object on the top of Z-order.
                Keyframe keyframe = m_FrameServer.Metadata.Keyframes[keyFrameIndex];
                for (int drawingIndex = keyframe.Drawings.Count - 1; drawingIndex >= 0; drawingIndex--)
                {
                    bool selected = keyframe.Drawings[drawingIndex] == m_FrameServer.Metadata.HitDrawing;
                    keyframe.Drawings[drawingIndex].Draw(canvas, distorter, transformer, selected, timestamp);
                }
            }
            else
            {
                // This is not a Keyframe, and fading is off.
                // Hence, there is no drawings to draw here.
            }
        }
        private void FlushMagnifierOnGraphics(Bitmap currentImage, Graphics canvas, ImageTransform transform)
        {
            // Note: the Graphics object must not be the one extracted from the image itself.
            // If needed, clone the image.
            if (currentImage != null && m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.None)
                m_FrameServer.Metadata.Magnifier.Draw(currentImage, canvas, transform, m_FrameServer.Metadata.Mirrored, m_FrameServer.VideoReader.Info.ReferenceSize);
        }
        public void DoInvalidate()
        {
            // This function should be the single point where we call for rendering.
            // Here we can decide to render directly on the surface, go through the Windows message pump, force the refresh, etc.
            
            // Invalidate is asynchronous and several Invalidate calls will be grouped together. (Only one repaint will be done).
            pbSurfaceScreen.Invalidate();
        }
        public void InitializeEndFromMenu(bool cancelLastPoint)
        {
            m_FrameServer.Metadata.InitializeEnd(cancelLastPoint);
        }
        #endregion

        #region PanelCenter Events
        private void PanelCenter_MouseEnter(object sender, EventArgs e)
        {
            panelCenter.Focus();
        }
        private void PanelCenter_MouseClick(object sender, MouseEventArgs e)
        {
            OnPoke();
        }
        private void PanelCenter_Resize(object sender, EventArgs e)
        {
            if(m_Constructed)
                ResizeUpdate(true);
        }
        private void PanelCenter_MouseDown(object sender, MouseEventArgs e)
        {
            mnuDirectTrack.Visible = false;
            mnuPasteDrawing.Visible = false;
            mnuPastePic.Visible = false;
            panelCenter.ContextMenuStrip = popMenu;
        }
        #endregion
        
        #region Keyframes Panel
        private void pnlThumbnails_MouseEnter(object sender, EventArgs e)
        {
            // Give focus to disable keyframe box editing.
            pnlThumbnails.Focus();
        }
        private void splitKeyframes_Resize(object sender, EventArgs e)
        {
            // Redo the dock/undock if needed to be at the right place.
            // (Could be handled by layout ?)
            DockKeyframePanel(m_bDocked);
        }
        private void btnAddKeyframe_Click(object sender, EventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                AddKeyframe();

                // Set as active screen is done afterwards, so the export as pdf menu is activated
                // even if we had no keyframes yet.
                OnPoke();
            }
        }
        public void OrganizeKeyframes()
        {
            // Should only be called when adding/removing a Thumbnail

            ClearKeyframeBoxes();

            if (m_FrameServer.Metadata.Count > 0)
            {
                int pixelsOffset = 0;
                int pixelsSpacing = 20;

                foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
                {
                    KeyframeBox box = new KeyframeBox(kf);
                    SetupDefaultThumbBox(box);
                    
                    // Finish the setup
                    box.Left = pixelsOffset + pixelsSpacing;
                    box.CloseThumb += ThumbBoxClose;
                    box.ClickThumb += ThumbBoxClick;
                    box.ClickInfos += ThumbBoxInfosClick;
                    
                    pixelsOffset += (pixelsSpacing + box.Width);

                    pnlThumbnails.Controls.Add(box);
                    thumbnails.Add(box);
                }
                
                EnableDisableKeyframes();
                pnlThumbnails.Refresh();
            }
            else
            {
                DockKeyframePanel(true);
                m_iActiveKeyFrameIndex = -1;
            }
            
            UpdateFramesMarkers();
            DoInvalidate(); // Because of trajectories with keyframes labels.
        }
        private void SetupDefaultThumbBox(UserControl _box)
        {
            _box.Top = 10;
            _box.Cursor = Cursors.Hand;
        }
        private void ActivateKeyframe(long _iPosition)
        {
            ActivateKeyframe(_iPosition, true);
        }
        private void ActivateKeyframe(long _iPosition, bool _bAllowUIUpdate)
        {
            //--------------------------------------------------------------
            // Black border every keyframe, unless it is at the given position.
            // This method might be called with -1 to force complete blackout.
            //--------------------------------------------------------------

            // This method is called on each frame during frametracker browsing
            // keep it fast or fix the strategy.

            m_iActiveKeyFrameIndex = -1;
            for (int i = 0; i < thumbnails.Count; i++)
            {
                if (m_FrameServer.Metadata[i].Position == _iPosition)
                {
                    m_iActiveKeyFrameIndex = i;
                    if(_bAllowUIUpdate)
                    {
                        thumbnails[i].DisplayAsSelected(true);
                        pnlThumbnails.ScrollControlIntoView(thumbnails[i]);
                    }
                }
                else
                {
                    if(_bAllowUIUpdate)
                        thumbnails[i].DisplayAsSelected(false);
                }
            }

            if (_bAllowUIUpdate && m_KeyframeCommentsHub.UserActivated && m_iActiveKeyFrameIndex >= 0)
            {
                m_KeyframeCommentsHub.UpdateContent(m_FrameServer.Metadata[m_iActiveKeyFrameIndex]);
                m_KeyframeCommentsHub.Visible = true;
            }
            else
            {
                if(m_KeyframeCommentsHub.Visible)
                    m_KeyframeCommentsHub.CommitChanges();
                
                m_KeyframeCommentsHub.Visible = false;
            }
        }
        private void EnableDisableKeyframes()
        {
            m_FrameServer.Metadata.EnableDisableKeyframes();

            foreach (KeyframeBox box in thumbnails)
                box.UpdateEnableStatus();
        }
        public void OnKeyframesTitleChanged()
        {
            m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
            EnableDisableKeyframes();
            DoInvalidate();
        }
        public void GotoNextKeyframe()
        {
            if (m_FrameServer.Metadata.Count == 0)
                return;
            
            int next = -1;
            for (int i = 0; i < m_FrameServer.Metadata.Count; i++)
            {
                if (m_iCurrentPosition < m_FrameServer.Metadata[i].Position)
                {
                    next = i;
                    break;
                }
            }

            if (next >= 0 && m_FrameServer.Metadata[next].Position <= m_iSelEnd)
                ThumbBoxClick(thumbnails[next], EventArgs.Empty);
        }
        public void GotoPreviousKeyframe()
        {
            if (m_FrameServer.Metadata.Count == 0)
                return;
            
            int prev = -1;
            for (int i = m_FrameServer.Metadata.Count - 1; i >= 0; i--)
            {
                if (m_iCurrentPosition > m_FrameServer.Metadata[i].Position)
                {
                    prev = i;
                    break;
                }
            }

            if (prev >= 0 && m_FrameServer.Metadata[prev].Position >= m_iSelStart)
                ThumbBoxClick(thumbnails[prev], EventArgs.Empty);
        }

        public void AddKeyframe()
        {
            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            if (keyframeIndex >= 0)
            {
                m_iActiveKeyFrameIndex = keyframeIndex;
                Keyframe keyframe = m_FrameServer.Metadata.GetKeyframe(m_FrameServer.Metadata.GetKeyframeId(keyframeIndex));
                m_FrameServer.Metadata.SelectKeyframe(keyframe);
                return;
            }
            
            if (KeyframeAdding != null)
                KeyframeAdding(this, new TimeEventArgs(m_iCurrentPosition));
        }
        private void AfterKeyframeAdded(Guid keyframeId)
        {
            if (m_FrameServer.Metadata.KVAImporting)
                return;

            Keyframe keyframe = m_FrameServer.Metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                return;

            if (!keyframe.Initialized)
                InitializeKeyframe(keyframe);

            OrganizeKeyframes();
            UpdateFramesMarkers();
            
            if (m_FrameServer.Metadata.Count == 1)
                DockKeyframePanel(false);

            if (!m_bIsCurrentlyPlaying)
                ActivateKeyframe(m_iCurrentPosition);
        }
        private void InitializeKeyframe(Keyframe keyframe)
        {
            // Move the playhead to the keyframe position to import the image and build thumbnail.
            if (m_iCurrentPosition != keyframe.Position)
            {
                m_iFramesToDecode = 1;
                ShowNextFrame(keyframe.Position, true);
                UpdatePositionUI();
            }

            if (m_FrameServer.CurrentImage == null)
                return;

            keyframe.Initialize(m_iCurrentPosition, m_FrameServer.CurrentImage);
        }
        private void DeleteKeyframe(Guid keyframeId)
        {
            if (KeyframeDeleting != null)
                KeyframeDeleting(this, new KeyframeEventArgs(keyframeId));
        }
        private void AfterKeyframeDeleted()
        {
            m_iActiveKeyFrameIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            OrganizeKeyframes();
            UpdateFramesMarkers();
            DoInvalidate();
        }
        public void UpdateKeyframes()
        {
            // Primary selection has been image-adjusted,
            // some keyframes may have been impacted.

            bool bAtLeastOne = false;

            foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
            {
                if (kf.Position >= m_iSelStart && kf.Position <= m_iSelEnd)
                {
                    //kf.ImportImage(m_FrameServer.VideoReader.FrameList[(int)m_FrameServer.VideoReader.GetFrameNumber(kf.Position)].BmpImage);
                    //kf.GenerateDisabledThumbnail();
                    bAtLeastOne = true;
                }
                else
                {
                    // Outside selection : couldn't possibly be impacted.
                }
            }

            if (bAtLeastOne)
                OrganizeKeyframes();

        }
        private void pnlThumbnails_DoubleClick(object sender, EventArgs e)
        {
            if (m_FrameServer.Loaded)
            {
                // On double click in the thumbs panel : Add a keyframe at current pos.
                AddKeyframe();
                OnPoke();
            }
        }

        #region ThumbBox event Handlers
        private void ThumbBoxClose(object sender, EventArgs e)
        {
            KeyframeBox keyframeBox = sender as KeyframeBox;
            if (keyframeBox == null)
                return;

            DeleteKeyframe(keyframeBox.Keyframe.Id);

            // Set as active screen is done after in case we don't have any keyframes left.
            OnPoke();
        }
        private void ThumbBoxClick(object sender, EventArgs e)
        {
            KeyframeBox keyframeBox = sender as KeyframeBox;
            if (keyframeBox == null)
                return;

            // Move to the right spot.
            OnPoke();
            StopPlaying();
            OnPauseAsked();

            long targetPosition = keyframeBox.Keyframe.Position;

            trkSelection.SelPos = targetPosition;
            m_iFramesToDecode = 1;

            ShowNextFrame(targetPosition, true);
            m_iCurrentPosition = targetPosition;

            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);
        }
        private void ThumbBoxInfosClick(object sender, EventArgs e)
        {
            ThumbBoxClick(sender, e);
            m_KeyframeCommentsHub.UserActivated = true;
            ActivateKeyframe(m_iCurrentPosition);
        }
        #endregion

        #region Docking Undocking
        private void btnDockBottom_Click(object sender, EventArgs e)
        {
            DockKeyframePanel(!m_bDocked);
        }
        private void splitKeyframes_Panel2_DoubleClick(object sender, EventArgs e)
        {
            DockKeyframePanel(!m_bDocked);
        }
        private void DockKeyframePanel(bool _bDock)
        {
            if(_bDock)
            {
                // hide the keyframes, change image.
                splitKeyframes.SplitterDistance = splitKeyframes.Height - 25;
                btnDockBottom.BackgroundImage = Resources.undock16x16;
                btnDockBottom.Visible = m_FrameServer.Metadata.Count > 0;
            }
            else
            {
                // show the keyframes, change image.
                splitKeyframes.SplitterDistance = splitKeyframes.Height - 140;
                btnDockBottom.BackgroundImage = Resources.dock16x16;
                btnDockBottom.Visible = true;
            }
            
            m_bDocked = _bDock;
        }
        private void PrepareKeyframesDock()
        {
            // If there's no keyframe, and we will be using a tool,
            // the keyframes dock should be raised.
            // This way we don't surprise the user when he click the screen and the image moves around.
            // (especially problematic when using the Pencil.
            
            // this is only done for the very first keyframe.
            if (m_FrameServer.Metadata.Count < 1)
            {
                DockKeyframePanel(false);
            }
        }
        #endregion

        #endregion

        #region Drawings Toolbar Events
        private void drawingTool_Click(object sender, EventArgs e)
        {
            // User clicked on a drawing tool button. A reference to the tool is stored in .Tag
            // Set this tool as the active tool (waiting for the actual use) and set the cursor accordingly.
            
            // Deactivate magnifier if not commited.
            if(m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
            {
                DisableMagnifier();
            }
            
            OnPoke();
            
            AbstractDrawingTool tool = ((ToolStripItem)sender).Tag as AbstractDrawingTool;
            m_ActiveTool = tool ?? m_PointerTool;
            UpdateCursor();
            
            // Ensure there's a key image at this position, unless the tool creates unattached drawings.
            if(m_ActiveTool == m_PointerTool && m_FrameServer.Metadata.Count < 1)
                DockKeyframePanel(true);
            else if(m_ActiveTool.Attached)
                PrepareKeyframesDock();
            
            DoInvalidate();
        }
        private void btnMagnifier_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;
            
            m_ActiveTool = m_PointerTool;

            if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.None)
            {
                UnzoomDirectZoom(false);
                m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.Direct;
                SetCursor(CursorManager.GetManipulationCursorMagnifier());
                
                if(TrackableDrawingAdded != null)
                    TrackableDrawingAdded(this, new TrackableDrawingEventArgs(m_FrameServer.Metadata.Magnifier as ITrackable));
            }
            else if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Direct)
            {
                // Revert to no magnification.
                UnzoomDirectZoom(false);
                m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.None;
                //btnMagnifier.Image = Drawings.magnifier;
                SetCursor(m_PointerTool.GetCursor(0));
                DoInvalidate();
            }
            else
            {
                DisableMagnifier();
                DoInvalidate();
            }
        }
        private void btnShowComments_Click(object sender, EventArgs e)
        {
            OnPoke();

            if (m_FrameServer.Loaded)
            {
                // If the video is currently playing, the comments are not visible.
                // We stop the video and show them.
                bool bWasPlaying = m_bIsCurrentlyPlaying;
                if (m_bIsCurrentlyPlaying)
                {
                    StopPlaying();
                    OnPauseAsked();
                    ActivateKeyframe(m_iCurrentPosition);
                }
                
                if(m_iActiveKeyFrameIndex < 0 || !m_KeyframeCommentsHub.UserActivated || bWasPlaying)
                {
                    // As of now, Keyframes infobox should display when we are on a keyframe
                    m_KeyframeCommentsHub.UserActivated = true;
                    
                    if (m_iActiveKeyFrameIndex < 0)
                    {
                        // We are not on a keyframe but user asked to show the infos...
                        // did he want to create a keyframe here and put some infos,
                        // or did he only want to activate the infobox for next keyframes ?
                        //
                        // Since he clicked on the DrawingTools bar, we will act as if it was a Drawing,
                        // and add a keyframe here in case there isn't already one.
                        AddKeyframe();
                    }

                    m_KeyframeCommentsHub.UpdateContent(m_FrameServer.Metadata[m_iActiveKeyFrameIndex]);
                    m_KeyframeCommentsHub.Visible = true;
                }
                else
                {
                    m_KeyframeCommentsHub.UserActivated = false;
                    m_KeyframeCommentsHub.CommitChanges();
                    m_KeyframeCommentsHub.Visible = false;
                }
                
            }
        }
        private void btnColorProfile_Click(object sender, EventArgs e)
        {
            OnPoke();

            // Load, save or modify current profile.
            FormToolPresets ftp = new FormToolPresets();
            FormsHelper.Locate(ftp);
            ftp.ShowDialog();
            ftp.Dispose();

            UpdateCursor();
            DoInvalidate();
        }
        private void UpdateCursor()
        {
            if(m_ActiveTool == m_PointerTool)
            {
                SetCursor(m_PointerTool.GetCursor(0));
            }
            else
            {
                Cursor cursor = CursorManager.GetToolCursor(m_ActiveTool, m_FrameServer.ImageTransform.Scale);
                SetCursor(cursor);
            }
        }
        private void SetCursor(Cursor _cur)
        {
            pbSurfaceScreen.Cursor = _cur;
        }
        #endregion

        #region Context Menus Events
        
        #region Main
        private void mnuDirectTrack_Click(object sender, EventArgs e)
        {
            // Track the point.
            // m_DescaledMouse would have been set during the MouseDown event.
            CheckCustomDecodingSize(true);

            Color color = TrackColorCycler.Next();
            DrawingStyle style = new DrawingStyle();
            style.Elements.Add("color", new StyleElementColor(color));
            style.Elements.Add("line size", new StyleElementLineSize(3));
            style.Elements.Add("track shape", new StyleElementTrackShape(TrackShape.Solid));

            DrawingTrack track = new DrawingTrack(m_DescaledMouse, m_iCurrentPosition, style);
            track.Status = TrackStatus.Edit;

            if (DrawingAdding != null)
                DrawingAdding(this, new DrawingEventArgs(track, m_FrameServer.Metadata.TrackManager.Id));
        }
        private void mnuPastePic_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
                return;

            Image img = Clipboard.GetImage();
            if (img == null)
                return;

            Bitmap bmp = new Bitmap(img);

            BeforeAddImageDrawing();
            if (m_FrameServer.Metadata.HitKeyframe == null)
                return;

            AbstractDrawing drawing = new DrawingBitmap(m_FrameServer.VideoReader.Current.Timestamp, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame, bmp);
            
            if (drawing != null && DrawingAdding != null)
                DrawingAdding(this, new DrawingEventArgs(drawing, m_FrameServer.Metadata.HitKeyframe.Id));
        }
        #endregion

        #region Drawings Menus
        private void mnuConfigureDrawing_Click(object sender, EventArgs e)
        {
            Keyframe kf = m_FrameServer.Metadata.HitKeyframe;
            IDecorable drawing = m_FrameServer.Metadata.HitDrawing as IDecorable;
            if(drawing == null || drawing.DrawingStyle == null || drawing.DrawingStyle.Elements.Count == 0)
                return;

            // FIXME: memento for coordinate system and autonumbers.
            bool canMemento = kf != null && !(m_FrameServer.Metadata.HitDrawing is DrawingCoordinateSystem);

            HistoryMementoModifyDrawing memento = null;
            if (canMemento)
                memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.HitKeyframe.Id, m_FrameServer.Metadata.HitDrawing.Id, m_FrameServer.Metadata.HitDrawing.Name, SerializationFilter.Style);
            
            FormConfigureDrawing2 fcd = new FormConfigureDrawing2(drawing, DoInvalidate);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();

            if (canMemento && fcd.DialogResult == DialogResult.OK)
            {
                memento.UpdateCommandName(drawing.Name);
                m_FrameServer.HistoryStack.PushNewCommand(memento);
            }
            
            fcd.Dispose();
            DoInvalidate();
        }
        private void mnuSetStyleAsDefault_Click(object sender, EventArgs e)
        {
            // Assign the style of the active drawing to the drawing tool that generated it.
            Keyframe kf = m_FrameServer.Metadata.HitKeyframe;
            IDecorable drawing = m_FrameServer.Metadata.HitDrawing as IDecorable;
            if (drawing == null || drawing.DrawingStyle == null || drawing.DrawingStyle.Elements.Count == 0)
                return;

            ToolManager.SetStylePreset(m_FrameServer.Metadata.HitDrawing, drawing.DrawingStyle);
            ToolManager.SavePresets();

            UpdateCursor();
        }
        private void mnuAlwaysVisible_Click(object sender, EventArgs e)
        {
            mnuAlwaysVisible.Checked = !mnuAlwaysVisible.Checked;
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.HitKeyframe.Id, drawing.Id, drawing.Name, SerializationFilter.Fading);
            drawing.InfosFading.UseDefault = false;
            drawing.InfosFading.AlwaysVisible = mnuAlwaysVisible.Checked;
            m_FrameServer.HistoryStack.PushNewCommand(memento);
            DoInvalidate();
        }
        private void mnuConfigureOpacity_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            
            formConfigureOpacity fco = new formConfigureOpacity(drawing, pbSurfaceScreen);
            FormsHelper.Locate(fco);
            fco.ShowDialog();
            fco.Dispose();
            DoInvalidate();
        }
        private void mnuGotoKeyframe_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if(drawing.InfosFading == null)
                return;
            
            long target = drawing.InfosFading.ReferenceTimestamp;
            m_iFramesToDecode = 1;
            ShowNextFrame(target, true);
            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);
        }
        private void mnuDrawingTrackingToggle_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            
            // Tracking is not compatible with custom decoding size, force the use of the original size.
            CheckCustomDecodingSize(true);
            ShowNextFrame(m_iCurrentPosition, true);
            ToggleTrackingCommand.Execute(drawing);

            // Force always visible to make sure we continue seeing the drawing during tracking.
            bool tracked = ToggleTrackingCommand.CurrentState(drawing);
            if (tracked && (drawing.Caps & DrawingCapabilities.Fading) == DrawingCapabilities.Fading)
            {
                drawing.InfosFading.UseDefault = false;
                drawing.InfosFading.AlwaysVisible = true;
            }
            
            RefreshImage();
        }

        private void mnuDrawingTrackingConfigure_Click(object sender, EventArgs e)
        {

        }

        private void mnuCutDrawing_Click(object sender, EventArgs e)
        {
            CutDrawing();
        }

        private void CutDrawing()
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if (drawing == null || !drawing.IsCopyPasteable)
                return;

            Guid keyframeId = m_FrameServer.Metadata.FindAttachmentKeyframeId(m_FrameServer.Metadata.HitDrawing);
            AbstractDrawingManager manager = m_FrameServer.Metadata.GetDrawingManager(keyframeId);
            string data = DrawingSerializer.SerializeMemento(m_FrameServer.Metadata, manager.GetDrawing(drawing.Id), SerializationFilter.All, false);

            DrawingClipboard.Put(data, drawing.GetCopyPoint(), drawing.Name);
            
            if (DrawingDeleting != null)
                DrawingDeleting(this, new DrawingEventArgs(drawing, keyframeId));

            OnPoke();
        }

        private void mnuCopyDrawing_Click(object sender, EventArgs e)
        {
            CopyDrawing();
        }

        private void CopyDrawing()
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if (drawing == null || !drawing.IsCopyPasteable)
                return;

            Guid keyframeId = m_FrameServer.Metadata.FindAttachmentKeyframeId(m_FrameServer.Metadata.HitDrawing);
            AbstractDrawingManager manager = m_FrameServer.Metadata.GetDrawingManager(keyframeId);
            string data = DrawingSerializer.SerializeMemento(m_FrameServer.Metadata, manager.GetDrawing(drawing.Id), SerializationFilter.All, false);

            DrawingClipboard.Put(data, drawing.GetCopyPoint(), drawing.Name);

            OnPoke();
        }

        private void mnuPasteDrawing_Click(object sender, EventArgs e)
        {
            PasteDrawing(false);
        }
        private void PasteDrawing(bool inPlace)
        {
            string data = DrawingClipboard.Content;

            AbstractDrawing drawing = DrawingSerializer.DeserializeMemento(data, m_FrameServer.Metadata);
            if (!drawing.IsCopyPasteable)
                return;

            Keyframe kf = m_FrameServer.Metadata.HitKeyframe;
            if (kf == null)
            {
                AddKeyframe();
                kf = m_FrameServer.Metadata.HitKeyframe;
            }

            drawing.AfterCopy();
            
            if (!inPlace)
            {
                // Relocate the drawing under the mouse based on relative motion since the "copy" or "cut" action.
                float dx = m_DescaledMouse.X - DrawingClipboard.Position.X;
                float dy = m_DescaledMouse.Y - DrawingClipboard.Position.Y;
                drawing.MoveDrawing(dx, dy, Keys.None, m_FrameServer.Metadata.ImageTransform.Zooming);
                log.DebugFormat("Pasted drawing [{0}] under the mouse.", DrawingClipboard.Name);
            }
            else
            {
                log.DebugFormat("Pasted drawing [{0}] in place.", DrawingClipboard.Name);
            }

            if (DrawingAdding != null)
                DrawingAdding(this, new DrawingEventArgs(drawing, kf.Id));
        }
        
        private void mnuDeleteDrawing_Click(object sender, EventArgs e)
        {
            DeleteSelectedDrawing();
        }
        private void DeleteSelectedDrawing()
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if(drawing == null)
                return;
            
            if(drawing is AbstractMultiDrawing)
            {
                AbstractMultiDrawing manager = drawing as AbstractMultiDrawing;

                if (MultiDrawingItemDeleting != null)
                    MultiDrawingItemDeleting(this, new MultiDrawingItemEventArgs(manager.SelectedItem, manager));
            }
            else
            {
                Guid keyframeId = m_FrameServer.Metadata.FindAttachmentKeyframeId(m_FrameServer.Metadata.HitDrawing);
                if (DrawingDeleting != null)
                    DrawingDeleting(this, new DrawingEventArgs(drawing, keyframeId));
            }
        }
        #endregion
        
        #region Trajectory tool menus
        private void mnuStopTracking_Click(object sender, EventArgs e)
        {
            DrawingTrack track = m_FrameServer.Metadata.HitDrawing as DrawingTrack;
            if (track == null)
                return;

            track.StopTracking();
            CheckCustomDecodingSize(false);
            DoInvalidate();
        }
        private void mnuDeleteEndOfTrajectory_Click(object sender, EventArgs e)
        {
            DrawingTrack track = m_FrameServer.Metadata.HitDrawing as DrawingTrack;
            if (track == null)
                return;

            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.TrackManager.Id, track.Id, track.Name, SerializationFilter.Core);

            track.ChopTrajectory(m_iCurrentPosition);

            m_FrameServer.HistoryStack.PushNewCommand(memento);

            DoInvalidate();
            UpdateFramesMarkers();
        }
        private void mnuRestartTracking_Click(object sender, EventArgs e)
        {
            DrawingTrack track = m_FrameServer.Metadata.HitDrawing as DrawingTrack;
            if(track == null)
                return;
            
            CheckCustomDecodingSize(true);
            track.RestartTracking();
            DoInvalidate();
        }
        private void mnuDeleteTrajectory_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if (drawing == null || !(drawing is DrawingTrack))
                return;

            if (DrawingDeleting != null)
                DrawingDeleting(this, new DrawingEventArgs(drawing, m_FrameServer.Metadata.TrackManager.Id));

            // Trigger a refresh of the export to spreadsheet menu, in case we don't have any more trajectory left to export.
            OnPoke();
            CheckCustomDecodingSize(false);
        }
        private void mnuConfigureTrajectory_Click(object sender, EventArgs e)
        {
            DrawingTrack track = m_FrameServer.Metadata.HitDrawing as DrawingTrack;
            if(track == null)
                return;

            // Note that we use SerializationFilter.All to backup all data as the dialog allows to modify not only style option but also tracker parameters.
            HistoryMementoModifyDrawing memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.TrackManager.Id, track.Id, track.Name, SerializationFilter.All);

            formConfigureTrajectoryDisplay fctd = new formConfigureTrajectoryDisplay(track, m_FrameServer.Metadata, m_FrameServer.CurrentImage, m_iCurrentPosition, DoInvalidate);
            fctd.StartPosition = FormStartPosition.CenterScreen;
            fctd.ShowDialog();

            if (fctd.DialogResult == DialogResult.OK)
            {
                memento.UpdateCommandName(track.Name);
                m_FrameServer.HistoryStack.PushNewCommand(memento);
            }

            fctd.Dispose();
            DoInvalidate();
        }
        private void OnShowClosestFrame(Point _mouse, List<AbstractTrackPoint> _positions, int _iPixelTotalDistance, bool _b2DOnly)
        {
            //--------------------------------------------------------------------------
            // This is where the interactivity of the trajectory is done.
            // The user has draged or clicked the trajectory, we find the closest point
            // and we update to the corresponding frame.
            //--------------------------------------------------------------------------

            // Compute the 3D distance (x,y,t) of each point in the path.
            // unscaled coordinates.

            double minDistance = double.MaxValue;
            int iClosestPoint = 0;

            if (_b2DOnly)
            {
                // Check the closest location on screen.
                for (int i = 0; i < _positions.Count; i++)
                {
                    double dist = Math.Sqrt(((_mouse.X - _positions[i].X) * (_mouse.X - _positions[i].X))
                                            + ((_mouse.Y - _positions[i].Y) * (_mouse.Y - _positions[i].Y)));


                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        iClosestPoint = i;
                    }
                }
            }
            else
            {
                // Check closest location on screen, but giving priority to the one also close in time.
                // = distance in 3D.
                // Distance on t is not in the same unit as distance on x and y.
                // So first step is to normalize t.

                // _iPixelTotalDistance should be the flat distance (distance from topleft to bottomright)
                // not the added distances of each segments, otherwise it will be biased towards time.

                long TimeTotalDistance = _positions[_positions.Count -1].T - _positions[0].T;
                double scaleFactor = (double)TimeTotalDistance / (double)_iPixelTotalDistance;

                for (int i = 0; i < _positions.Count; i++)
                {
                    double fTimeDistance = (double)(m_iCurrentPosition - _positions[i].T);

                    double dist = Math.Sqrt(((_mouse.X - _positions[i].X) * (_mouse.X - _positions[i].X))
                                            + ((_mouse.Y - _positions[i].Y) * (_mouse.Y - _positions[i].Y))
                                            + ((long)(fTimeDistance / scaleFactor) * (long)(fTimeDistance / scaleFactor)));

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        iClosestPoint = i;
                    }
                }

            }

            // move to corresponding timestamp.
            m_iFramesToDecode = 1;
            ShowNextFrame(_positions[iClosestPoint].T, true);
            UpdatePositionUI();
        }
        #endregion

        #region Chronometers Menus
        private void mnuChronoStart_Click(object sender, EventArgs e)
        {
            DrawingChrono chrono = m_FrameServer.Metadata.HitDrawing as DrawingChrono;
            if (chrono == null)
                return;

            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.ChronoManager.Id, chrono.Id, chrono.Name, SerializationFilter.Core);
            chrono.Start(m_iCurrentPosition);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            UpdateFramesMarkers();
        }
        private void mnuChronoStop_Click(object sender, EventArgs e)
        {
            DrawingChrono chrono = m_FrameServer.Metadata.HitDrawing as DrawingChrono;
            if (chrono == null)
                return;

            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.ChronoManager.Id, chrono.Id, chrono.Name, SerializationFilter.Core);
            chrono.Stop(m_iCurrentPosition);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            UpdateFramesMarkers();
        }
        private void mnuChronoHide_Click(object sender, EventArgs e)
        {
            DrawingChrono chrono = m_FrameServer.Metadata.HitDrawing as DrawingChrono;
            if (chrono == null)
                return;

            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.ChronoManager.Id, chrono.Id, chrono.Name, SerializationFilter.Core);
            chrono.Hide(m_iCurrentPosition);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            UpdateFramesMarkers();
        }
        private void mnuChronoCountdown_Click(object sender, EventArgs e)
        {
            // This menu should only be accessible if we have a "Stop" value.

            DrawingChrono chrono = m_FrameServer.Metadata.HitDrawing as DrawingChrono;
            if (chrono == null)
                return;

            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.ChronoManager.Id, chrono.Id, chrono.Name, SerializationFilter.Core);
            mnuChronoCountdown.Checked = !mnuChronoCountdown.Checked;
            chrono.CountDown = mnuChronoCountdown.Checked;
            m_FrameServer.HistoryStack.PushNewCommand(memento);
            
            DoInvalidate();
        }
        private void mnuChronoDelete_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            if(drawing == null || !(drawing is DrawingChrono))
                return;
            
            if (DrawingDeleting != null)
                DrawingDeleting(this, new DrawingEventArgs(drawing, m_FrameServer.Metadata.ChronoManager.Id));
        }
        private void mnuChronoConfigure_Click(object sender, EventArgs e)
        {
            DrawingChrono chrono = m_FrameServer.Metadata.HitDrawing as DrawingChrono;
            if (chrono == null)
                return;

            HistoryMementoModifyDrawing memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.ChronoManager.Id, chrono.Id, chrono.Name, SerializationFilter.Style);
            
            formConfigureChrono fcc = new formConfigureChrono(chrono, DoInvalidate);
            FormsHelper.Locate(fcc);
            fcc.ShowDialog();
            
            if (fcc.DialogResult == DialogResult.OK)
            {
                memento.UpdateCommandName(chrono.Name);
                m_FrameServer.HistoryStack.PushNewCommand(memento);
            }
            
            fcc.Dispose();
            DoInvalidate();
        }
        #endregion

        #region Magnifier Menus
        private void mnuMagnifierQuit_Click(object sender, EventArgs e)
        {
            DisableMagnifier();
            DoInvalidate();
        }
        private void mnuMagnifierDirect_Click(object sender, EventArgs e)
        {
            // Use position and magnification to Direct Zoom.
            // Go to direct zoom, at magnifier zoom factor, centered on same point as magnifier.
            m_FrameServer.ImageTransform.Zoom = m_FrameServer.Metadata.Magnifier.MagnificationFactor;
            m_FrameServer.ImageTransform.RelocateZoomWindow(m_FrameServer.Metadata.Magnifier.Center);
            DisableMagnifier();
            ToastZoom();
            
            ResizeUpdate(true);
        }
        private void mnuMagnifierChangeMagnification(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if(menu == null)
                return;
            
            foreach(ToolStripMenuItem m in maginificationMenus)
                m.Checked = false;
            
            menu.Checked = true;
            
            m_FrameServer.Metadata.Magnifier.MagnificationFactor = (double)menu.Tag;
            DoInvalidate();
        }
        private void mnuMagnifierTrack_Click(object sender, EventArgs e)
        {
            ITrackable drawing = m_FrameServer.Metadata.Magnifier as ITrackable;
            
            // Tracking is not compatible with custom decoding size, force the use of the original size.
            CheckCustomDecodingSize(true);
            ShowNextFrame(m_iCurrentPosition, true);
            ToggleTrackingCommand.Execute(drawing);
        }
        
        private void DisableMagnifier()
        {
            // Revert to no magnification.
            m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.None;
            SetCursor(m_PointerTool.GetCursor(0));
        }
        #endregion
        
        #endregion
        
        #region DirectZoom
        private void UnzoomDirectZoom(bool _toast)
        {
            m_FrameServer.ImageTransform.ReinitZoom();
            
            m_PointerTool.SetZoomLocation(m_FrameServer.ImageTransform.Location);
            if(_toast)
                ToastZoom();
            ReportForSyncMerge();
            ResizeUpdate(true);
        }
        private void IncreaseDirectZoom()
        {
            if (m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.None)
                DisableMagnifier();
            
            m_FrameServer.ImageTransform.Zoom = Math.Min(m_FrameServer.ImageTransform.Zoom + 0.10f, m_MaxZoomFactor);
            AfterZoomChange();
        }
        private void DecreaseDirectZoom()
        {
            if (!m_FrameServer.ImageTransform.Zooming)
                return;

            m_FrameServer.ImageTransform.Zoom = Math.Max(m_FrameServer.ImageTransform.Zoom - 0.10f, 1.0f);
            AfterZoomChange();
        }
        private void AfterZoomChange()
        {
            m_FrameServer.ImageTransform.RelocateZoomWindow();
            m_PointerTool.SetZoomLocation(m_FrameServer.ImageTransform.Location);
            ToastZoom();
            UpdateCursor();
            ReportForSyncMerge();
            
            ResizeUpdate(true);
        }
        #endregion
        
        #region Toasts
        private void ToastZoom()
        {
            m_MessageToaster.SetDuration(750);
            int percentage = (int)(m_FrameServer.ImageTransform.Zoom * 100);
            m_MessageToaster.Show(String.Format(ScreenManagerLang.Toast_Zoom, percentage.ToString()));
        }
        private void ToastPause()
        {
            m_MessageToaster.SetDuration(750);
            m_MessageToaster.Show(ScreenManagerLang.Toast_Pause);
        }
        #endregion

        #region Synchronisation specifics
        private void AfterSyncAlphaChange()
        {
            m_SyncMergeMatrix.Matrix00 = 1.0f;
            m_SyncMergeMatrix.Matrix11 = 1.0f;
            m_SyncMergeMatrix.Matrix22 = 1.0f;
            m_SyncMergeMatrix.Matrix33 = m_SyncAlpha;
            m_SyncMergeMatrix.Matrix44 = 1.0f;
            m_SyncMergeImgAttr.SetColorMatrix(m_SyncMergeMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
        private void IncreaseSyncAlpha()
        {
            if(!m_bSyncMerge)
                return;
            m_SyncAlpha = Math.Max(m_SyncAlpha - 0.1f, 0.0f);
            AfterSyncAlphaChange();
            DoInvalidate();
        }
        private void DecreaseSyncAlpha()
        {
            if(!m_bSyncMerge)
                return;
            m_SyncAlpha = Math.Min(m_SyncAlpha + 0.1f, 1.0f);
            AfterSyncAlphaChange();
            DoInvalidate();
        }
        private void ReportForSyncMerge()
        {
            if(!m_bSynched)
                return;
            
            // If we are not actually merging, we don't need to clone and send the image.
            // But we still need to report to the screen manager to trigger sync operations.
            Bitmap img = null;
            
            if(m_bSyncMerge && m_FrameServer.CurrentImage != null)
            {
                // We have to re-apply the transformations here, because when drawing in this screen we draw directly on the canvas.
                // (there is no intermediate image that we could reuse here, this might be a future optimization).
                // We need to clone it anyway, so we might aswell do the transform.
                img = CloneTransformedImage();
            }

            if (ImageChanged != null)
                ImageChanged(this, new EventArgs<Bitmap>(img));
        }
        private Bitmap CloneTransformedImage()
        {
            // TODO: try to render unscaled here as well when possible.
            Size copySize = m_viewportManipulator.RenderingSize;
            Bitmap copy = new Bitmap(copySize.Width, copySize.Height);
            Graphics g = Graphics.FromImage(copy);
            
            Rectangle rDst;
            if(m_FrameServer.Metadata.Mirrored)
                rDst = new Rectangle(copySize.Width, 0, -copySize.Width, copySize.Height);
            else
                rDst = new Rectangle(0, 0, copySize.Width, copySize.Height);
            
            if(m_viewportManipulator.MayDrawUnscaled && m_FrameServer.VideoReader.CanDrawUnscaled)
                g.DrawImage(m_FrameServer.CurrentImage, rDst, m_FrameServer.ImageTransform.RenderingZoomWindow, GraphicsUnit.Pixel);
            else
                g.DrawImage(m_FrameServer.CurrentImage, rDst, m_FrameServer.ImageTransform.ZoomWindow, GraphicsUnit.Pixel);
                
            return copy;
        }
        #endregion
        
        #region VideoFilters Management
        private void EnableDisableAllPlayingControls(bool _bEnable)
        {
            // Disable playback controls and some other controls for the case
            // of a one-frame rendering. (mosaic, single image)
            
            if(m_FrameServer.Loaded && !m_FrameServer.VideoReader.CanChangeWorkingZone)
                EnableDisableWorkingZoneControls(false);
            else
                EnableDisableWorkingZoneControls(_bEnable);
            
            buttonGotoFirst.Enabled = _bEnable;
            buttonGotoLast.Enabled = _bEnable;
            buttonGotoNext.Enabled = _bEnable;
            buttonGotoPrevious.Enabled = _bEnable;
            buttonPlay.Enabled = _bEnable;
            btnPlayingMode.Enabled = _bEnable;
            
            lblSpeedTuner.Enabled = _bEnable;
            trkFrame.EnableDisable(_bEnable);

            trkFrame.Enabled = _bEnable;
            trkSelection.Enabled = _bEnable;
            sldrSpeed.Enabled = _bEnable;
            
            btnRafale.Enabled = _bEnable;
            btnSaveVideo.Enabled = _bEnable;
            btnDiaporama.Enabled = _bEnable;
            btnPausedVideo.Enabled = _bEnable;
            
            mnuPlayPause.Visible = _bEnable;
            mnuDirectTrack.Visible = _bEnable;
        }
        private void EnableDisableWorkingZoneControls(bool _bEnable)
        {
            btnSetHandlerLeft.Enabled = _bEnable;
            btnSetHandlerRight.Enabled = _bEnable;
            btnHandlersReset.Enabled = _bEnable;
            btn_HandlersLock.Enabled = _bEnable;
            trkSelection.EnableDisable(_bEnable);
        }
        private void EnableDisableSnapshot(bool _bEnable)
        {
            btnSnapShot.Enabled = _bEnable;
        }
        private void EnableDisableDrawingTools(bool _bEnable)
        {
            foreach(ToolStripItem tsi in stripDrawingTools.Items)
            {
                tsi.Enabled = _bEnable;
            }
        }
        #endregion
        
        #region Export images and videos

        /// <summary>
        /// Export the current frame with drawings to the clipboard.
        /// </summary>
        private void CopyImageToClipboard()
        {
            if (!m_FrameServer.Loaded || m_FrameServer.CurrentImage == null)
                return;

            StopPlaying();
            OnPauseAsked();

            Bitmap outputImage = GetFlushedImage();
            Clipboard.SetImage(outputImage);
            outputImage.Dispose();
        }

        /// <summary>
        /// Export the current frame with drawings to a file.
        /// </summary>
        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded || m_FrameServer.CurrentImage == null)
                return;
            
            StopPlaying();
            OnPauseAsked();
            
            try
            {
                SaveFileDialog dlgSave = new SaveFileDialog();
                dlgSave.Title = ScreenManagerLang.Generic_SaveImage;
                dlgSave.RestoreDirectory = true;
                dlgSave.Filter = ScreenManagerLang.FileFilter_SaveImage;
                dlgSave.FilterIndex = FilesystemHelper.GetFilterIndex(dlgSave.Filter, PreferencesManager.PlayerPreferences.ImageFormat);
                
                if(InteractiveFiltering)
                    dlgSave.FileName = Path.GetFileNameWithoutExtension(m_FrameServer.VideoReader.FilePath);
                else
                    dlgSave.FileName = BuildFilename(m_FrameServer.VideoReader.FilePath, m_iCurrentPosition, PreferencesManager.PlayerPreferences.TimecodeFormat);
                
                if (dlgSave.ShowDialog() == DialogResult.OK)
                {
                    Bitmap outputImage = GetFlushedImage();
                    ImageHelper.Save(dlgSave.FileName, outputImage);
                    outputImage.Dispose();

                    PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(dlgSave.FileName);
                    PreferencesManager.Save();

                    m_FrameServer.AfterSave();
                }
            }
            catch (Exception exp)
            {
                log.Error(exp.StackTrace);
            }
        }

        /// <summary>
        /// Local wrapper for Save, which triggers the main saving pipeline.
        /// </summary>
        private void btnVideo_Click(object sender, EventArgs e)
        {
            if(!m_FrameServer.Loaded)
                return;
            
            StopPlaying();
            OnPauseAsked();

            Save();
            
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            ActivateKeyframe(m_iCurrentPosition, true);
        }

        /// <summary>
        /// Triggers the rafale export pipeline.
        /// Ultimately this enumerates frames and comes back to GetFlushedImage(VideoFrame, Bitmap).
        /// </summary>
        private void btnRafale_Click(object sender, EventArgs e)
        {
            //---------------------------------------------------------------------------------
            // Workflow:
            // 1. FormRafaleExport  : configure the export, calls:
            // 2. FileSaveDialog    : choose the file name, then:
            // 3. FormFramesExport   : Progress bar holder and updater, calls:
            // 4. SaveImageSequence (below): Perform the real work.
            //---------------------------------------------------------------------------------

            if (!m_FrameServer.Loaded || m_FrameServer.CurrentImage == null)
                return;

            StopPlaying();
            OnPauseAsked();

            FormRafaleExport fre = new FormRafaleExport(
                this,
                m_FrameServer.Metadata,
                m_FrameServer.VideoReader.FilePath,
                m_FrameServer.VideoReader.Info);

            fre.ShowDialog();
            fre.Dispose();
            m_FrameServer.AfterSave();

            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            ActivateKeyframe(m_iCurrentPosition, true);
        }

        /// <summary>
        /// Triggers the special video export pipeline.
        /// Ultimately this enumerates frames and comes back to GetFlushedImage(VideoFrame, Bitmap).
        /// </summary>
        private void btnDiaporama_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded || m_FrameServer.CurrentImage == null)
                return;
                
            bool diaporama = sender == btnDiaporama;
            
            StopPlaying();
            OnPauseAsked();
            
            if(m_FrameServer.Metadata.Keyframes.Count < 1)
            {
                string error = diaporama ? ScreenManagerLang.Error_SavePausedVideo : ScreenManagerLang.Error_SavePausedVideo;
                MessageBox.Show(ScreenManagerLang.Error_SaveDiaporama_NoKeyframes.Replace("\\n", "\n"),
                                error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return;
            }
            
            saveInProgress = true;
            m_FrameServer.SaveDiaporama(GetFlushedImage, diaporama);
            saveInProgress = false;
            
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            ActivateKeyframe(m_iCurrentPosition, true);
        }

        /// <summary>
        /// Triggers the main video saving pipeline. 
        /// Ultimately this enumerates frames and comes back to GetFlushedImage(VideoFrame, Bitmap).
        /// </summary>
        public void Save()
        {
            // This function is public because it is also accessed from the top-level menu.
            saveInProgress = true;
            m_FrameServer.Save(timeMapper.GetInterval(sldrSpeed.Value), slowMotion * 100, GetFlushedImage);
            saveInProgress = false;
        }

        /// <summary>
        /// Save several images at once. Called back for rafale export.
        /// </summary>
        public void SaveImageSequence(BackgroundWorker bgWorker, string filepath, long interval, bool keyframesOnly, int total)
        {
            // This function works similarly to the video export in FrameServerPlayer.EnumerateImages.
            // The images are saved at original video size.
            int frameCount = keyframesOnly ? m_FrameServer.Metadata.Keyframes.Count : total;
            int iCurrent = 0;

            m_FrameServer.VideoReader.BeforeFrameEnumeration();

            // We do not use the cached Bitmap in keyframe.FullImage because it is saved at the display size of the time of the creation of the keyframe.
            IEnumerable<VideoFrame> frames = keyframesOnly ? m_FrameServer.VideoReader.FrameEnumerator() : m_FrameServer.VideoReader.FrameEnumerator(interval);

            foreach (VideoFrame vf in frames)
            {
                Bitmap output = null;

                try
                {
                    if (vf == null)
                    {
                        log.Error("Frame enumerator yield null.");
                        break;
                    }

                    output = new Bitmap(vf.Image.Width, vf.Image.Height, vf.Image.PixelFormat);

                    bool onKeyframe = GetFlushedImage(vf, output);
                    bool savable = onKeyframe || !keyframesOnly;

                    if (savable)
                    {
                        string filename = string.Format("{0}\\{1}{2}",
                            Path.GetDirectoryName(filepath),
                            BuildFilename(filepath, vf.Timestamp, PreferencesManager.PlayerPreferences.TimecodeFormat),
                            Path.GetExtension(filepath));

                        ImageHelper.Save(filename, output);
                    }

                    bgWorker.ReportProgress(iCurrent++, frameCount);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (output != null)
                        output.Dispose();
                }

            }

            m_FrameServer.VideoReader.AfterFrameEnumeration();
        }
        
        /// <summary>
        /// Returns the image currently on screen with all drawings flushed, including grids, magnifier, mirroring, etc.
        /// The resulting Bitmap will be at the same size as the image currently on screen.
        /// This is used to export individual images or get the image for dual video export.
        /// </summary>
        public Bitmap GetFlushedImage()
        {
            Size renderingSize = m_viewportManipulator.RenderingSize;
            Bitmap output = new Bitmap(renderingSize.Width, renderingSize.Height, PixelFormat.Format24bppRgb);
            output.SetResolution(m_FrameServer.CurrentImage.HorizontalResolution, m_FrameServer.CurrentImage.VerticalResolution);

            if(InteractiveFiltering)
            {
                using (Graphics canvas = Graphics.FromImage(output))
                    m_InteractiveEffect.Draw(canvas, m_FrameServer.VideoReader.WorkingZoneFrames);
            }
            else
            {
                int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
                using (Graphics canvas = Graphics.FromImage(output))
                    FlushOnGraphics(m_FrameServer.CurrentImage, canvas, output.Size, keyframeIndex, m_iCurrentPosition, m_FrameServer.ImageTransform);
            }

            return output;
        }

        /// <summary>
        /// Paint the passed bitmap with the content of video frame passed in, plus the complete compositing pipeline.
        /// The painting is done without zoom. 
        /// The passed bitmap should have the same size as the video frame.
        /// This is used to export videos or sequence of images.
        /// Returns true if the passed frame is a keyframe.
        /// </summary>
        public bool GetFlushedImage(VideoFrame vf, Bitmap output)
        {
            if (vf.Image.Size != output.Size)
            {
                log.ErrorFormat("Exporting unscaled images: passed bitmap has the wrong size.");
                return false;
            }

            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(vf.Timestamp);
            
            // Make sure the trackable drawings are on the right context.
            TrackDrawingsCommand.Execute(vf);

            using (Graphics canvas = Graphics.FromImage(output))
                FlushOnGraphics(vf.Image, canvas, output.Size, keyframeIndex, vf.Timestamp, m_FrameServer.ImageTransform.Identity);

            return keyframeIndex != -1;
        }

        /// <summary>
        /// Builds a file name with the current timecode and the extension.
        /// </summary>
        private string BuildFilename(string _FilePath, long _position, TimecodeFormat _timeCodeFormat)
        {
            TimecodeFormat tcf;
            if(_timeCodeFormat == TimecodeFormat.TimeAndFrames)
                tcf = TimecodeFormat.ClassicTime;
            else
                tcf = _timeCodeFormat;
            
            // Timecode string (Not relative to sync position)
            string suffix = m_FrameServer.TimeStampsToTimecode(_position - m_iSelStart, TimeType.Time, tcf, false);
            string maxSuffix = m_FrameServer.TimeStampsToTimecode(m_iSelEnd - m_iSelStart, TimeType.Time, tcf, false);

            switch (tcf)
            {
                case TimecodeFormat.Frames:
                case TimecodeFormat.Milliseconds:
                case TimecodeFormat.Microseconds:
                case TimecodeFormat.TenThousandthOfHours:
                case TimecodeFormat.HundredthOfMinutes:
                    
                    int iZerosToPad = maxSuffix.Length - suffix.Length;
                    for (int i = 0; i < iZerosToPad; i++)
                    {
                        // Add a leading zero.
                        suffix = suffix.Insert(0, "0");
                    }
                    break;
                default:
                    break;
            }

            // Reconstruct filename
            return Path.GetFileNameWithoutExtension(_FilePath) + "-" + suffix.Replace(':', '.');
        }
        #endregion

        #region Memo & Reset
        public MemoPlayerScreen GetMemo()
        {
            return new MemoPlayerScreen(m_iSelStart, m_iSelEnd);
        }
        public void ResetSelectionImages(MemoPlayerScreen _memo)
        {
            // This is typically called when undoing image adjustments.
            // We do not actually undo the adjustment because we don't have the original data anymore.
            // We emulate it by reloading the selection.
            
            // Memorize the current selection boundaries.
            MemoPlayerScreen mps = new MemoPlayerScreen(m_iSelStart, m_iSelEnd);

            // Reset the selection to whatever it was when we did the image adjustment.
            m_iSelStart = _memo.SelStart;
            m_iSelEnd = _memo.SelEnd;

            // Undo all adjustments made on this portion.
            UpdateWorkingZone(true);
            UpdateKeyframes();

            // Reset to the current selection.
            m_iSelStart = mps.SelStart;
            m_iSelEnd = mps.SelEnd;
        }
        #endregion

    }
}
