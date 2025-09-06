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
using System.Globalization;
using Kinovea.ScreenManager.Languages;
using Kinovea.ScreenManager.Properties;
using Kinovea.Video;
using Kinovea.Services;
using System.Xml;
using System.Text;
#endregion

namespace Kinovea.ScreenManager
{
    public partial class PlayerScreenUserInterface : KinoveaControl, IDrawingHostView
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
        public event EventHandler OpenVideoAsked;
        public event EventHandler<EventArgs<CaptureFolder>> OpenReplayWatcherAsked;
        public event EventHandler LoadAnnotationsAsked;
        public event EventHandler SaveAnnotationsAsked;
        public event EventHandler SaveAnnotationsAsAsked;
        public event EventHandler SaveDefaultPlayerAnnotationsAsked;
        public event EventHandler SaveDefaultCaptureAnnotationsAsked;
        public event EventHandler UnloadAnnotationsAsked;
        public event EventHandler ReloadDefaultPlayerAnnotationsAsked;
        public event EventHandler CloseAsked;
        public event EventHandler StopWatcherAsked;
        public event EventHandler<EventArgs<CaptureFolder>> StartWatcherAsked;
        public event EventHandler SetAsActiveScreen;
        public event EventHandler SpeedChanged;
        public event EventHandler TimeOriginChanged;
        public event EventHandler KVAImported;
        public event EventHandler PlayStarted;
        public event EventHandler PauseAsked;
        public event EventHandler ResetAsked;
        public event EventHandler FilterExited;
        public event EventHandler Loaded;
        public event EventHandler<EventArgs<bool>> SelectionChanged;
        public event EventHandler<EventArgs<Bitmap>> ImageChanged;
        public event EventHandler<KeyframeAddEventArgs> KeyframeAdding;
        public event EventHandler<KeyframeEventArgs> KeyframeDeleting;
        public event EventHandler<DrawingEventArgs> DrawingAdding;
        public event EventHandler<DrawingEventArgs> DrawingDeleting;
        public event EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemAdding;
        public event EventHandler<MultiDrawingItemEventArgs> MultiDrawingItemDeleting;
        public event EventHandler<TrackableDrawingEventArgs> TrackableDrawingAdded;
        public event EventHandler<EventArgs<HotkeyCommand>> DualCommandReceived;
        public event EventHandler ExportImageAsked;
        public event EventHandler ExportImageSequenceAsked;
        public event EventHandler ExportKeyImagesAsked;
        public event EventHandler ExportVideoAsked;
        public event EventHandler ExportVideoWithPausesAsked;
        public event EventHandler ExportVideoSlideshowAsked;
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
        public bool IsWaitingForIdle { get; private set; }

        public bool ImageFill
        {
            get { return m_fill; }
        }

        /// <summary>
        /// Returns the interval between frames in milliseconds, taking slow motion slider into account.
        /// This is suitable for a playback loop timer or metadata in saved file.
        /// </summary>
        public double PlaybackFrameInterval
        {
            get
            {
                return timeMapper.GetInterval(sldrSpeed.Value);
            }
        }

        /// <summary>
        /// Returns the playback speed as a percentage of the real time speed of the captured action.
        /// This is not the same as the raw slider percentage when the video is not real time.
        /// </summary>
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

        /// <summary>
        /// Returns the raw percentage of the slider.
        /// This is the percentage of nominal framerate of the video.
        /// </summary>
        public double SpeedPercentage
        {
            get { return slowMotion * 100; }
        }

        public ScreenDescriptorPlayback ScreenDescriptor
        {
            get { return screenDescriptor; }
            set 
            {
                screenDescriptor = value; 
                infobar.ScreenDescriptor = value;
            }
        }

        public long CurrentTimestamp
        {
            get { return m_iCurrentPosition; }
        }

        public Bitmap CurrentImage 
        {
            get { return m_FrameServer?.CurrentImage; }
        }

        public bool Synched
        {
            //get { return m_bSynched; }
            set
            {
                m_bSynched = value;

                if (!m_bSynched)
                {
                    // We do not reset the time origin.
                    trkFrame.UpdateMarkers(m_FrameServer.Metadata);
                    UpdateCurrentPositionLabel();

                    m_bSyncMerge = false;
                    if (m_SyncMergeImage != null)
                        m_SyncMergeImage.Dispose();
                }

                // If we are a replay watcher then consider we are in dual replay context.
                // This is used to ignore the auto-play and wait until the dual player triggers it.
                // If we are no longer synched, disable this to start honoring the auto-play flag again.
                if (screenDescriptor != null && screenDescriptor.IsReplayWatcher)
                {
                    screenDescriptor.IsDualReplay = m_bSynched;
                }
            }
        }

        /// <summary>
        /// Returns the current frame time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTime
        {
            get
            {
                return TimestampToRealtime(m_iCurrentPosition - m_iSelStart);
            }
        }

        /// <summary>
        /// Returns the last valid time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalLastTime
        {
            get
            {
                return TimestampToRealtime(m_iSelEnd - m_iSelStart);
            }
        }

        /// <summary>
        /// Returns the average time of one frame.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalFrameTime
        {
            get
            {
                return TimestampToRealtime(m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
            }

        }

        /// <summary>
        /// Returns the time origin relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTimeOriginPhysical
        {
            get
            {
                return TimestampToRealtime(m_FrameServer.Metadata.TimeOrigin - m_iSelStart);
            }
        }

        /// <summary>
        /// Gets or sets whether we should draw the other screen image on top of this one.
        /// </summary>
        public bool SyncMerge
        {
            get { return m_bSyncMerge; }
            set
            {
                m_bSyncMerge = value;

                m_FrameServer.ImageTransform.AllowOutOfScreen = m_bSyncMerge;

                if (!m_bSyncMerge && m_SyncMergeImage != null)
                {
                    m_SyncMergeImage.Dispose();
                }

                DoInvalidate();
            }
        }
        public bool DualSaveInProgress
        {
            set { dualSaveInProgress = value; }
        }
        #endregion

        #region Members
        private FrameServerPlayer m_FrameServer;
        private VariablesRepository variablesRepository;

        // Playback current state
        private bool m_bIsCurrentlyPlaying;
        private int m_iFramesToDecode = 1;
        private uint m_IdMultimediaTimer;
        private PlayingMode m_ePlayingMode = PlayingMode.Loop;
        private bool m_bIsBusyRendering;
        private int m_RenderingDrops;
        private object m_TimingSync = new object();
        private bool interactiveFrameTracker = true;
        private bool showCacheInTimeline = false;

        // Timing
        // Time mapper links the speed slider, the playback frame rate and the capture frame rate.
        // slowMotion is the ratio to the nominal playback speed of the video.
        // ex: 0.5 plays the video at half speed, irrespectively of whether the video itself is in slow motion.
        // The capture frame rate is encoded itself as a ratio in in m_FrameServer.Metadata.HighSpeedFactor.
        // ex: 0.5 means one second of video covers 0.5 seconds of real time action.
        // When the user manipulates the speed slider we change the internal playback speed.
        // The value we show on the speed slider is the final ratio to real time.
        // So if the user set the slider to 0.5 on a video with a high speed factor of 0.5, it will display 0.25.
        private TimeMapper timeMapper = new TimeMapper();
        private double slowMotion = 1;  // Current scaling relatively to the nominal speed of the video.
        private float timeGrabSpeed = 25.0f / 500.0f; // Speed of time grab in frames per pixel.
        private TimecodeFormat timecodeFormat = TimecodeFormat.ClassicTime;

        // Synchronisation
        private bool m_bSynched;
        private bool m_bSyncMerge;
        private Bitmap m_SyncMergeImage;
        private ColorMatrix m_SyncMergeMatrix = new ColorMatrix();
        private ImageAttributes m_SyncMergeImgAttr = new ImageAttributes();
        private float m_SyncAlpha = 0.5f;
        private bool dualSaveInProgress;
        private bool saveInProgress;

        // Image
        private ViewportManipulator m_viewportManipulator = new ViewportManipulator();
        private bool m_fill;
        private double m_lastUserStretch = 1.0f;
        private bool m_bShowImageBorder;
        private bool m_bManualSqueeze = true; // If it's allowed to manually reduce the rendering surface under the aspect ratio size.
        private static readonly Pen m_PenImageBorder = Pens.SteelBlue;
        private static readonly Size m_MinimalSize = new Size(160, 120);
        private bool customDecodingSizeIsEnabled = true;

        // Selection and current position. All values in absolute timestamps.
        // trkSelection.minimum and maximum are also in absolute timestamps.
        private long m_iTotalDuration = 100;
        private long m_iSelStart;
        private long m_iSelEnd = 99;
        private long m_iSelDuration = 100;
        private long m_iCurrentPosition;    // Current timestamp.
        private long m_iStartingPosition;   // Timestamp of the first decoded frame of video.
        private bool m_bHandlersLocked;
        private long memoPosition;          // Used during export to backup/restore the current position.

        // Keyframes, Drawings, etc.
        private List<KeyframeBox> keyframeBoxes = new List<KeyframeBox>();
        private int m_iActiveKeyFrameIndex = -1;	// The index of the keyframe we are on, or -1 if not a KF.
        private AbstractDrawingTool m_ActiveTool;
        private DrawingToolPointer m_PointerTool;
        private bool m_bKeyframePanelCollapsed = true;
        private bool m_bKeyframePanelCollapsedManual = false;
        private bool m_bTextEdit;
        private PointF m_DescaledMouse;    // The current mouse point expressed in the original image size coordinates.
        private bool showDrawings = true;
        private bool drawOnPlay = true;
        private bool defaultFadingEnabled = true;

        // Others
        private NativeMethods.TimerCallback m_TimerCallback;
        private ScreenDescriptorPlayback screenDescriptor;
        private bool videoFilterIsActive;
        private ZoomHelper zoomHelper = new ZoomHelper();
        private const int m_MaxRenderingDrops = 6;
        private const int m_MaxDecodingDrops = 6;
        private System.Windows.Forms.Timer selectionTimer = new System.Windows.Forms.Timer();
        private MessageToaster m_MessageToaster;
        private bool m_Constructed;
        private bool workingZoneLoaded;
        private ScreenPointerManager cursorManager = new ScreenPointerManager();

        #region Context Menus
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuTimeOrigin = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDirectTrack = new ToolStripMenuItem();
        private ToolStripMenuItem mnuBackground = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCopyPic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPastePic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuPasteDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenReplayWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenReplayWatcherFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLoadAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveAnnotationsAs = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveDefaultPlayerAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSaveDefaultCaptureAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUnloadAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuReloadDefaultPlayerAnnotations = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExportImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCloseScreen = new ToolStripMenuItem();
        private ToolStripMenuItem mnuExitFilter = new ToolStripMenuItem();

        private ContextMenuStrip popMenuDrawings = new ContextMenuStrip();
        private ToolStripMenuItem mnuConfigureDrawing = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVisibility = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVisibilityAlways = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVisibilityDefault = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVisibilityCustom = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVisibilityConfigure = new ToolStripMenuItem();
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
        private ToolStripMenuItem mnuConfigureTrajectory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteTrajectory = new ToolStripMenuItem();

        private ContextMenuStrip popMenuMagnifier = new ContextMenuStrip();
        private ToolStripMenuItem mnuMagnifierFreeze = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMagnifierTrack = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMagnifierDirect = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMagnifierQuit = new ToolStripMenuItem();

        private ContextMenuStrip popMenuFilter = new ContextMenuStrip();
        #endregion

        private ToolStripButton btnAddKeyFrame;
        private ToolStripButton btnToggleSidePanel;
        private ToolStripButton btnToolPresets;
        private InfobarPlayer infobar = new InfobarPlayer();
        private bool isSidePanelVisible;
        private SidePanelKeyframes sidePanelKeyframes = new SidePanelKeyframes();
        private SidePanelDrawing sidePanelDrawing = new SidePanelDrawing();
        private SidePanelTracking sidePanelTracking;

        private DropWatcher m_DropWatcher = new DropWatcher();
        private TimeWatcher m_TimeWatcher = new TimeWatcher();
        private LoopWatcher m_LoopWatcher = new LoopWatcher();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreenUserInterface(FrameServerPlayer frameServer, DrawingToolbarPresenter drawingToolbarPresenter, VariablesRepository variablesRepository)
        {
            log.Debug("Constructing the PlayerScreen user interface.");

            m_FrameServer = frameServer;
            this.variablesRepository = variablesRepository;

            m_FrameServer.Metadata = new Metadata(m_FrameServer.HistoryStack, m_FrameServer.TimeStampsToTimecode);
            m_FrameServer.Metadata.KVAImported += (s, e) => AfterKVAImported();
            m_FrameServer.Metadata.KeyframeAdded += (s, e) => AfterKeyframeAdded(e.KeyframeId);
            m_FrameServer.Metadata.KeyframeModified += (s, e) => AfterKeyframeModified(e.KeyframeId);
            m_FrameServer.Metadata.KeyframeDeleted += (s, e) => AfterKeyframeDeleted();
            m_FrameServer.Metadata.DrawingAdded += (s, e) => AfterDrawingAdded(e.Drawing);
            m_FrameServer.Metadata.DrawingModified += (s, e) => AfterDrawingModified(e.Drawing);
            m_FrameServer.Metadata.DrawingDeleted += (s, e) => AfterDrawingDeleted();
            m_FrameServer.Metadata.MultiDrawingItemAdded += (s, e) => AfterMultiDrawingItemAdded();
            m_FrameServer.Metadata.MultiDrawingItemDeleted += (s, e) => AfterMultiDrawingItemDeleted();
            m_FrameServer.Metadata.VideoFilterModified += (s, e) => AfterVideoFilterModified();

            InitializeComponent();
            InitializeInfobar();
            InitializePropertiesPanel();
            InitializeDrawingTools(drawingToolbarPresenter);
            BuildContextMenus();
            BuildExportButtons();
            AfterSyncAlphaChange();
            m_MessageToaster = new MessageToaster(pbSurfaceScreen);

            // Drag & drop between keyframe list and bottom panel.
            trkFrame.KeyframeDropped += trkFrame_KeyframeDropped;
            panelVideoControls.AllowDrop = true;
            panelVideoControls.DragOver += PanelVideoControls_DragOver;
            panelVideoControls.DragDrop += PanelVideoControls_DragDrop;

            // Most members and controls should be initialized with the right value.
            // So we don't need to do an extra ResetData here.

            // Controls that renders differently between run time and design time.
            this.Dock = DockStyle.Fill;
            ShowHideRenderingSurface(false);
            SetupPrimarySelectionPanel();
            pnlThumbnails.Controls.Clear();
            keyframeBoxes.Clear();
            CollapseKeyframePanel(true);

            m_TimerCallback = MultimediaTimer_Tick;
            selectionTimer.Interval = 10000;
            selectionTimer.Tick += SelectionTimer_OnTick;

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

            // For replay observers we can still keep some state we would like to maintain between loads.
            // The mechanism for this is to backup the data in the screen descriptor before unloading.
            // It will be restored in PostLoadProcess.

            // 1. Reset all data.
            m_FrameServer.Unload();
            ResetData();
            videoFilterIsActive = false;
            btnExitFilter.Visible = false;

            // 2. Reset all interface.
            ShowHideRenderingSurface(false);
            SetupPrimarySelectionPanel();
            ClearKeyframeBoxes();
            sidePanelKeyframes.Clear();
            sidePanelDrawing.ClearMetadata();
            sidePanelTracking.ClearMetadata();
            CollapseKeyframePanel(true);
            UpdateFramesMarkers();
            EnableDisableAllPlayingControls(true);
            EnableDisableDrawingTools(true);
            EnableDisableExportButtons(true);
            buttonPlay.Image = Resources.flatplay;
            sldrSpeed.Enabled = false;
            screenDescriptor = null;
            infobar.ScreenDescriptor = null;
            infobar.Visible = false;

            if (ResetAsked != null)
                ResetAsked(this, EventArgs.Empty);
        }
        private void ClearKeyframeBoxes()
        {
            for (int i = keyframeBoxes.Count - 1; i >= 0; i--)
            {
                KeyframeBox box = keyframeBoxes[i];

                box.Selected -= KeyframeControl_Selected;
                box.ShowCommentsAsked -= KeyframeControl_ShowCommentsAsked;
                box.MoveToCurrentTimeAsked -= KeyframeControl_MoveToCurrentTimeAsked;
                box.DeleteAsked -= KeyframeControl_DeleteAsked;

                keyframeBoxes.Remove(box);
                pnlThumbnails.Controls.Remove(box);
                box.Dispose();
            }
        }
        public void EnableDisableActions(bool enable)
        {
            // Called back after a load error.
            // Prevent any actions.
            if (!enable)
                DisablePlayAndDraw();

            EnableDisableDrawingTools(enable);
            EnableDisableExportButtons(enable);

            if (enable && m_FrameServer.Loaded && m_FrameServer.VideoReader.IsSingleFrame)
                EnableDisableAllPlayingControls(false);
            else
                EnableDisableAllPlayingControls(enable);
        }
        public int PostLoadProcess()
        {
            //---------------------------------------------------------------------------
            // Configure the interface according to he video and try to read first frame.
            // Called from CommandLoadMovie when VideoFile.Load() is successful.
            //---------------------------------------------------------------------------
            log.DebugFormat("Post load process.");

            ShowNextFrame(-1, true);
            UpdatePositionUI();

            if (m_FrameServer.VideoReader.Current == null)
            {
                m_FrameServer.Unload();
                log.Error("First frame couldn't be loaded - aborting");
                return -1;
            }
            else if (m_iCurrentPosition < 0)
            {
                // First frame loaded but inconsistency. (Seen with some AVCHD)
                m_FrameServer.Unload();
                log.Error(String.Format("First frame loaded but negative timestamp ({0}) - aborting", m_iCurrentPosition));
                return -2;
            }

            log.DebugFormat("First frame loaded. Adjusted ts: {0}.", m_iCurrentPosition);

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
            m_iSelDuration = m_iTotalDuration;

            if (!m_FrameServer.VideoReader.CanChangeWorkingZone)
                EnableDisableWorkingZoneControls(false);

            // Update the control.
            // FIXME - already done in ImportSelectionToMemory ?
            SetupPrimarySelectionPanel();

            // Other various infos.
            m_FrameServer.SetupMetadata(true);
            m_FrameServer.Metadata.VideoPath = m_FrameServer.VideoReader.FilePath;
            m_FrameServer.Metadata.InitTime(m_iSelStart, m_iSelEnd, m_iSelStart);
            m_PointerTool.SetImageSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_viewportManipulator.Initialize(m_FrameServer.VideoReader);
            
            sidePanelKeyframes.Reset(m_FrameServer.Metadata);
            sidePanelDrawing.SetMetadata(m_FrameServer.Metadata);
            sidePanelTracking.SetMetadata(m_FrameServer.Metadata);

            // Screen position and size.
            m_FrameServer.ImageTransform.SetReferenceSize(m_FrameServer.VideoReader.Info.ReferenceSize);
            m_FrameServer.ImageTransform.ResetZoom();
            zoomHelper.Value = 1.0f;
            m_PointerTool.SetZoomLocation(new Point(-1, -1));
            SetUpForNewMovie();

            // Check for launch description and startup kva.
            // This is also how we can backup and restore stuff between loads in the same screen.
            bool recoveredMetadata = false;
            if (screenDescriptor != null)
            {
                // Starting the filesystem watcher for .IsReplayWatcher is done in PlayerScreen.
                // Starting the video for .Play is done later at first Idle.
                if (screenDescriptor.Id != Guid.Empty)
                    recoveredMetadata = m_FrameServer.Metadata.Recover(screenDescriptor.Id);

                if (screenDescriptor.Stretch)
                {
                    m_fill = true;
                    ResizeUpdate(true);
                }
            }

            if (!recoveredMetadata)
            {
                // Note: the order of load between the sidecar kva and the default player kva is important.
                // Generally we want to load the more general file first (default kva) and the more specific one later, 
                // as the last to load overwrites the values.
                // This is not relevant for drawings because keyframes and detached drawings are merged, not replaced.
                // It is important for the top level data like time origin, working zone, calibration, etc. as well 
                // as the singleton drawings like the coordinate system.
                // If the overwrite is undesirable the file should not contain the info in the first place.
                // See for example the case of capture recording, things like working zone bounds are not 
                // included, and attached drawings and calibration are optionally included according to preferences.

                Metadata metadata = m_FrameServer.Metadata;

                // 1. Load the default player KVA.
                LoadDefaultKVA();

                // 2. Load the sidecar KVA if it exists.
                // Note: we don't stop at the first one found, load all of them.
                foreach (string extension in MetadataSerializer.SupportedFileFormats())
                {
                    string pathSidecar = Path.Combine(
                        Path.GetDirectoryName(m_FrameServer.VideoReader.FilePath), 
                        Path.GetFileNameWithoutExtension(m_FrameServer.VideoReader.FilePath) + extension);

                    LoadKVA(pathSidecar);
                }
            }

            if (screenDescriptor != null)
            {
                // We assume this is a speed percentage of video framerate, not real time.
                // We must do this after KVA loading because it may reset the slowmotion.
                slowMotion = screenDescriptor.SpeedPercentage / 100.0;
            }

            UpdateTimebase();
            UpdateInfobar();

            sldrSpeed.Force(timeMapper.GetInputFromSlowMotion(slowMotion));
            sldrSpeed.Enabled = true;

            if (!recoveredMetadata)
                m_FrameServer.Metadata.ResetContentHash();

            m_FrameServer.Metadata.StartAutosave();

            log.DebugFormat("End of post load process, waiting for idle.");
            IsWaitingForIdle = true;
            Application.Idle += PostLoad_Idle;

            return 0;
        }

        /// <summary>
        /// Load the default KVA if any.
        /// </summary>
        private void LoadDefaultKVA()
        {
            string path = "";
            bool forPlayer = true;
            bool found = DynamicPathResolver.GetDefaultKVAPath(ref path, variablesRepository, forPlayer);

            if (!found)
                return;

            LoadKVA(path);

            // Never let the default file become the working file.
            m_FrameServer.Metadata.ResetKVAPath();
        }

        private void AfterKVAImported()
        {
            InitializeKeyframes();

            // Restore things like aspect ratio, image rotation, deinterlacing, stabilization, etc.
            m_FrameServer.RestoreImageOptions();

            // Restore selection.
            // Force a reload of the cache to account for possible changes in aspect ratio, image rotation, etc.
            m_iSelStart = m_FrameServer.Metadata.SelectionStart;
            m_iSelEnd = m_FrameServer.Metadata.SelectionEnd;
            m_iSelDuration = m_iSelEnd - m_iSelStart + m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;

            bool invalidateCache = !screenDescriptor.IsReplayWatcher;
            UpdateWorkingZone(invalidateCache);

            // Remember that we already loaded the working zone, to avoid an unnecessary caching operation during the
            // initialization of the screen. This is only for the first load into this screen.
            workingZoneLoaded = true;

            RestoreActiveVideoFilter();

            UpdateInfobar();
            OrganizeKeyframes();
            if (m_FrameServer.Metadata.Count > 0 && !m_bKeyframePanelCollapsedManual)
                CollapseKeyframePanel(false);

            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);

            double oldHSF = m_FrameServer.Metadata.HighSpeedFactor;
            double captureInterval = 1000 / m_FrameServer.Metadata.CalibrationHelper.CaptureFramesPerSecond;
            m_FrameServer.Metadata.HighSpeedFactor = m_FrameServer.Metadata.BaselineFrameInterval / captureInterval;
            UpdateTimebase();

            m_FrameServer.SetupMetadata(false);

            ImportEditboxes();
            m_PointerTool.SetImageSize(m_FrameServer.Metadata.ImageSize);

            if (KVAImported != null)
                KVAImported(this, EventArgs.Empty);

            trkFrame.UpdateMarkers(m_FrameServer.Metadata);
            UpdateTimeLabels();
            DoInvalidate();
        }

        /// <summary>
        /// Restore the active video filter after KVA import.
        /// </summary>
        private void RestoreActiveVideoFilter()
        {
            if (m_FrameServer.VideoReader.DecodingMode != VideoDecodingMode.Caching)
            {
                // The filter is not allowed to be activated.
                // This may happen if we load a KVA after having lowered the cache size.
                m_FrameServer.DeactivateVideoFilter();
                DeactivateVideoFilter();
            }
            else if (m_FrameServer.Metadata.ActiveVideoFilterType == VideoFilterType.None)
            {
                // Exiting filter.
                m_FrameServer.DeactivateVideoFilter();
                DeactivateVideoFilter();
            }
            else
            {
                // Re-entering filter.
                // It may be a different one so make sure to send it the cached frames.
                m_FrameServer.ActivateVideoFilter(m_FrameServer.Metadata.ActiveVideoFilterType);
                ActivateVideoFilter();
            }
        }
        public void UpdateTimebase()
        {
            timeMapper.FileInterval = m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds;
            timeMapper.UserInterval = m_FrameServer.Metadata.BaselineFrameInterval;
            timeMapper.CaptureInterval = timeMapper.UserInterval / m_FrameServer.Metadata.HighSpeedFactor;
        }
        public void UpdateTimeLabels()
        {
            UpdateSelectionLabels();
            UpdateCurrentPositionLabel();
            UpdateSpeedLabel();
            UpdateInfobar();
        }

        /// <summary>
        /// Update the infobar after switching the screen from replay to normal or vice versa.
        /// watchedFolderPath should be the real folder being watched on the file system, or null.
        /// </summary>
        public void UpdateReplayWatcher(string watchedFolderPath)
        {
            infobar.ScreenDescriptor = screenDescriptor;
            infobar.UpdateReplayWatcher(watchedFolderPath);
            UpdateInfobar();
        }

        /// <summary>
        /// Called after the common controls updated the sync position, impacting time origin in both videos.
        /// </summary>
        public void TimeOriginUpdatedFromSync()
        {
            trkFrame.UpdateMarkers(m_FrameServer.Metadata);
            UpdateCurrentPositionLabel();
        }

        /// <summary>
        /// Try to load the working zone into the cache if possible
        /// and consolidate the boundary values afterwards.
        /// </summary>
        public void UpdateWorkingZone(bool invalidateCache)
        {
            if (!m_FrameServer.Loaded)
                return;

            if (m_FrameServer.VideoReader.CanChangeWorkingZone)
            {
                StopPlaying();
                OnPauseAsked();
                VideoSection newZone = new VideoSection(m_iSelStart, m_iSelEnd);
                m_FrameServer.VideoReader.UpdateWorkingZone(newZone, invalidateCache, PreferencesManager.PlayerPreferences.WorkingZoneMemory, ProgressWorker);
                ResizeUpdate(true);
            }

            // Time origin: we try to maintain user-defined time origin, but we don't want the origin to stay at the absolute zero when the zone changes.
            // Check if we were previously aligned with the start of the zone, if so, keep it that way, otherwise keep the absolute value.
            // A side effect of this approach is that when the start of the zone is moved forward so as to overtake the current time origin,
            // it will scoop it and drag it along with it.
            if (m_FrameServer.Metadata.TimeOrigin == m_iSelStart)
                m_FrameServer.Metadata.TimeOrigin = m_FrameServer.VideoReader.WorkingZone.Start;

            // Reupdate back the locals as the reader uses more precise values.
            m_iCurrentPosition = m_iCurrentPosition + (m_FrameServer.VideoReader.WorkingZone.Start - m_iSelStart);
            m_iSelStart = m_FrameServer.VideoReader.WorkingZone.Start;
            m_iSelEnd = m_FrameServer.VideoReader.WorkingZone.End;
            m_iSelDuration = m_iSelEnd - m_iSelStart + m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame;

            if (trkSelection.SelStart != m_iSelStart)
                trkSelection.SelStart = m_iSelStart;

            if (trkSelection.SelEnd != m_iSelEnd)
                trkSelection.SelEnd = m_iSelEnd;

            trkFrame.Remap(m_iSelStart, m_iSelEnd, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);

            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);

            UpdatePositionUI();
            UpdateSelectionLabels();
            OnPoke();
            RestoreActiveVideoFilter();
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

            if (allowUIUpdate)
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
            // Update from core preferences.
            interactiveFrameTracker = PreferencesManager.PlayerPreferences.InteractiveFrameTracker;
            drawOnPlay = PreferencesManager.PlayerPreferences.DrawOnPlay;
            timecodeFormat = PreferencesManager.PlayerPreferences.TimecodeFormat;
            showCacheInTimeline = PreferencesManager.PlayerPreferences.ShowCacheInTimeline;
            trkFrame.ShowCacheInTimeline = showCacheInTimeline;
            defaultFadingEnabled = PreferencesManager.PlayerPreferences.DefaultFading.Enabled;

            // Update default fading for all drawings.
            

            // Labels
            lblSelStartSelection.AutoSize = true;
            lblSelDuration.AutoSize = true;

            UpdateTimeLabels();
            sidePanelKeyframes.UpdateTimecodes();
            
            ReloadTooltipsCulture();
            ReloadToolsCulture();
            ReloadMenusCulture();
            for (int i = 0; i < keyframeBoxes.Count; i++)
                keyframeBoxes[i].RefreshUICulture();

            // Keyframes positions.
            if (m_FrameServer.Metadata.Count > 0)
            {
                EnableDisableKeyframes();
            }

            m_FrameServer.Metadata.CalibrationHelper.RefreshUnits();
            m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
            m_FrameServer.Metadata.UpdateDefaultFading();

            // Refresh image to update timecode in chronos, grids colors, default fading, etc.
            DoInvalidate();
        }
        public void ActivateVideoFilter()
        {
            videoFilterIsActive = true;
            CollapseKeyframePanel(true);
            m_fill = true;
            ResizeUpdate(true);
            UpdateInfobar();
            btnExitFilter.Visible = true;

            string name = VideoFilterFactory.GetFriendlyName(m_FrameServer.Metadata.ActiveVideoFilter.Type);
            m_MessageToaster.SetDuration(750);
            m_MessageToaster.Show(name);
        }
        public void DeactivateVideoFilter()
        {
            if (!videoFilterIsActive)
                return;
            
            videoFilterIsActive = false;
            StretchSqueezeSurface(true);
            DoInvalidate();
            UpdateInfobar();
            btnExitFilter.Visible = false;
            
            string name = VideoFilterFactory.GetFriendlyName(VideoFilterType.None);
            m_MessageToaster.SetDuration(750);
            m_MessageToaster.Show(name);
        }

        public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
        {
            m_SyncMergeImage = _SyncMergeImage;

            if (_bUpdateUI)
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
            ResetZoom(false);
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
            if (m_bIsCurrentlyPlaying)
            {
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);
            }

            PrepareKeyframesDock();

            m_FrameServer.Metadata.AllDrawingTextToNormalMode();
            m_FrameServer.Metadata.DeselectAll();
            AddKeyframe();
        }
        #endregion

        #region Various Inits & Setups
        private void InitializeInfobar()
        {
            this.panelTop.Controls.Add(infobar);
            infobar.Visible = false;
            infobar.StopWatcherAsked += (s, e) => StopWatcherAsked?.Invoke(s, e);
            infobar.StartWatcherAsked += (s, e) => StartWatcherAsked?.Invoke(s, e);
        }

        private void InitializePropertiesPanel()
        {
            // Restore splitter distance and hook preferences save.
            splitViewport_Properties.SplitterDistance = (int)(splitViewport_Properties.Width * WindowManager.ActiveWindow.SidePanelSplitterRatio);
            splitViewport_Properties.SplitterMoved += (s, e) => {
                WindowManager.ActiveWindow.SidePanelSplitterRatio = (float)e.SplitX / splitViewport_Properties.Width;
            };

            // Create and add all the side panels.
            TabControl tabContainer = splitViewport_Properties.Panel2.Controls[0] as TabControl;
            if (tabContainer == null)
                return;

            tabContainer.TabPages[0].Controls.Add(sidePanelKeyframes);
            sidePanelKeyframes.Reset(m_FrameServer.Metadata);
            sidePanelKeyframes.Dock = DockStyle.Fill;
            sidePanelKeyframes.KeyframeSelected += KeyframeControl_Selected;
            sidePanelKeyframes.KeyframeUpdated += KeyframeControl_KeyframeUpdated;
            sidePanelKeyframes.KeyframeDeletionAsked += KeyframeControl_KeyframeDeletionAsked;

            tabContainer.TabPages[1].Controls.Add(sidePanelDrawing);
            sidePanelDrawing.SetMetadata(m_FrameServer.Metadata);
            sidePanelDrawing.Dock = DockStyle.Fill;
            sidePanelDrawing.DrawingModified += DrawingControl_DrawingUpdated;

            sidePanelTracking = new SidePanelTracking(this);
            tabContainer.TabPages[2].Controls.Add(sidePanelTracking);
            sidePanelTracking.SetMetadata(m_FrameServer.Metadata);
            sidePanelTracking.Dock = DockStyle.Fill;
            sidePanelTracking.DrawingModified += DrawingControl_DrawingUpdated;

            isSidePanelVisible = WindowManager.ActiveWindow.SidePanelVisible;
            splitViewport_Properties.Panel2Collapsed = !isSidePanelVisible;
        }

        private void InitializeDrawingTools(DrawingToolbarPresenter drawingToolbarPresenter)
        {
            m_PointerTool = new DrawingToolPointer();
            m_ActiveTool = m_PointerTool;

            drawingToolbarPresenter.ForceView(stripDrawingTools);

            // Hand tool.
            drawingToolbarPresenter.AddToolButton(m_PointerTool, drawingTool_Click);

            // Create key image.
            btnAddKeyFrame = CreateToolButton();
            btnAddKeyFrame.Image = Resources.createkeyframe;
            btnAddKeyFrame.Click += btnAddKeyframe_Click;
            btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
            drawingToolbarPresenter.AddSpecialButton(btnAddKeyFrame);

            // Side panel toggle.
            btnToggleSidePanel = CreateToolButton();
            btnToggleSidePanel.Image = Resources.sidepanel;
            btnToggleSidePanel.Click += (s, e) => ToggleSidePanelVisibility();
            btnToggleSidePanel.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
            drawingToolbarPresenter.AddSpecialButton(btnToggleSidePanel);

            drawingToolbarPresenter.AddSeparator();

            // All drawing tools.
            DrawingToolbarImporter importer = new DrawingToolbarImporter();
            importer.Import("player.xml", drawingToolbarPresenter, drawingTool_Click);

            drawingToolbarPresenter.AddToolButton(ToolManager.Tools["Magnifier"], btnMagnifier_Click);

            // Special button: Tool presets
            btnToolPresets = CreateToolButton();
            btnToolPresets.Image = Resources.SwatchIcon3;
            btnToolPresets.Click += btnColorProfile_Click;
            btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
            //drawingToolbarPresenter.AddSpecialButton(btnToolPresets);

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
            workingZoneLoaded = false;

            m_bIsCurrentlyPlaying = false;
            m_ePlayingMode = PlayingMode.Loop;
            m_fill = false;
            m_FrameServer.ImageTransform.Reset();
            m_lastUserStretch = 1.0f;

            // Sync
            m_bSynched = false;
            m_bSyncMerge = false;
            if (m_SyncMergeImage != null)
                m_SyncMergeImage.Dispose();

            m_bShowImageBorder = false;

            SetupPrimarySelectionData();    // Should not be necessary when every data is coming from m_FrameServer.

            m_bHandlersLocked = false;

            m_iActiveKeyFrameIndex = -1;
            m_ActiveTool = m_PointerTool;

            m_bKeyframePanelCollapsed = true;
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

            m_FrameServer.Metadata.TimeOrigin = m_iSelStart;
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
        
        private void LoadKVA(string path)
        {
            if (!File.Exists(path))
                return;

            MetadataSerializer s = new MetadataSerializer();
            s.Load(m_FrameServer.Metadata, path, true);
        }

        private void UpdateInfobar()
        {
            if (!m_FrameServer.Loaded)
                return;

            string size = string.Format("{0}×{1} px", m_FrameServer.Metadata.ImageSize.Width, m_FrameServer.Metadata.ImageSize.Height);
            string fps = string.Format("{0:0.00} fps", 1000 / timeMapper.UserInterval);

            infobar.Visible = true;
            infobar.Dock = DockStyle.Fill;
            infobar.UpdateValues(m_FrameServer.VideoReader.FilePath, size, fps, m_FrameServer.Metadata.ActiveVideoFilter);
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
            // Depending on the context, more menus are added and configured on the fly in SurfaceScreen_RightDown.

            // Background context menu.
            mnuTimeOrigin.Image = Properties.Resources.marker;
            mnuDirectTrack.Image = Properties.Resources.point3_16;
            mnuBackground.Image = Properties.Resources.shading;
            mnuCopyPic.Image = Properties.Resources.clipboard_block;
            mnuPastePic.Image = Properties.Drawings.paste;
            mnuPasteDrawing.Image = Properties.Drawings.paste;
            mnuOpenVideo.Image = Properties.Resources.folder;
            mnuOpenReplayWatcher.Image = Properties.Resources.replaywatcher;
            mnuOpenReplayWatcherFolder.Image = Properties.Resources.folder;
            mnuLoadAnnotations.Image = Properties.Resources.notes2_16;
            mnuSaveAnnotations.Image = Properties.Resources.save_16;
            mnuSaveAnnotationsAs.Image = Properties.Resources.save_as_16;
            mnuSaveDefaultPlayerAnnotations.Image = Properties.Resources.save_player_16;
            mnuSaveDefaultCaptureAnnotations.Image = Properties.Resources.save_capture_16;
            mnuUnloadAnnotations.Image = Properties.Resources.delete_notes;
            mnuReloadDefaultPlayerAnnotations.Image = Properties.Resources.notes2_16;
            mnuExportVideo.Image = Properties.Resources.film_save;
            mnuExportImage.Image = Properties.Resources.picture_save;
            mnuCloseScreen.Image = Properties.Resources.closeplayer;
            mnuExitFilter.Image = Properties.Resources.exit_filter;

            mnuTimeOrigin.Click += mnuTimeOrigin_Click;
            mnuDirectTrack.Click += mnuDirectTrack_Click;
            mnuBackground.Click += mnuBackground_Click;
            mnuCopyPic.Click += (s, e) => { CopyImageToClipboard(); };
            mnuPastePic.Click += mnuPastePic_Click;
            mnuPasteDrawing.Click += mnuPasteDrawing_Click;
            mnuOpenVideo.Click += (s, e) => OpenVideoAsked?.Invoke(this, EventArgs.Empty);
            mnuOpenReplayWatcherFolder.Click += (s, e) => OpenReplayWatcherAsked?.Invoke(this, new EventArgs<CaptureFolder>(null));
            mnuLoadAnnotations.Click += (s, e) => LoadAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            mnuSaveAnnotations.Click += mnuSaveAnnotations_Click;
            mnuSaveAnnotationsAs.Click += mnuSaveAnnotationsAs_Click;
            mnuSaveDefaultPlayerAnnotations.Click += mnuSaveDefaultPlayerAnnotations_Click;
            mnuSaveDefaultCaptureAnnotations.Click += mnuSaveDefaultCaptureAnnotations_Click;
            mnuUnloadAnnotations.Click += mnuUnloadAnnotations_Click;
            mnuReloadDefaultPlayerAnnotations.Click += mnuReloadDefaultPlayerAnnotations_Click;
            mnuExportVideo.Click += (s, e) => ExportVideoAsked?.Invoke(s, e);
            mnuExportImage.Click += (s, e) => ExportImageAsked?.Invoke(s, e);
            mnuCloseScreen.Click += btnClose_Click;
            mnuExitFilter.Click += MnuExitFilter_Click;

            // Drawings context menu (Configure, Delete, Tracking)
            mnuConfigureDrawing.Click += new EventHandler(mnuConfigureDrawing_Click);
            mnuConfigureDrawing.Image = Properties.Drawings.configure;
            
            mnuVisibility.Image = Properties.Drawings.persistence;
            mnuVisibilityAlways.Image = Properties.Drawings.persistence;
            mnuVisibilityDefault.Image = Properties.Drawings.persistence;
            mnuVisibilityCustom.Image = Properties.Drawings.persistence;
            mnuVisibilityConfigure.Image = Properties.Drawings.configure;
            mnuVisibilityAlways.Click += mnuVisibilityAlways_Click;
            mnuVisibilityDefault.Click += mnuVisibilityDefault_Click;
            mnuVisibilityCustom.Click += mnuVisibilityCustom_Click;
            mnuVisibilityConfigure.Click += mnuVisibilityConfigure_Click;
            mnuVisibility.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuVisibilityAlways,
                mnuVisibilityDefault,
                mnuVisibilityCustom,
                new ToolStripSeparator(),
                mnuVisibilityConfigure
            });

            mnuGotoKeyframe.Click += new EventHandler(mnuGotoKeyframe_Click);
            mnuGotoKeyframe.Image = Properties.Resources.page_white_go;

            mnuDrawingTracking.Image = Properties.Resources.point3_16;
            mnuDrawingTrackingConfigure.Click += mnuDrawingTrackingConfigure_Click;
            mnuDrawingTrackingConfigure.Image = Properties.Drawings.configure;
            mnuDrawingTrackingStart.Click += mnuDrawingTrackingToggle_Click;
            mnuDrawingTrackingStart.Image = Properties.Drawings.play_green2;
            mnuDrawingTrackingStop.Click += mnuDrawingTrackingToggle_Click;
            mnuDrawingTrackingStop.Image = Properties.Drawings.stop_16;
            mnuDrawingTracking.DropDownItems.AddRange(new ToolStripItem[] {
                mnuDrawingTrackingStart,
                mnuDrawingTrackingStop
            });

            mnuCutDrawing.Click += new EventHandler(mnuCutDrawing_Click);
            mnuCutDrawing.Image = Properties.Drawings.cut;
            mnuCopyDrawing.Click += new EventHandler(mnuCopyDrawing_Click);
            mnuCopyDrawing.Image = Properties.Drawings.copy;
            mnuDeleteDrawing.Click += new EventHandler(mnuDeleteDrawing_Click);
            mnuDeleteDrawing.Image = Properties.Drawings.delete;

            // Tracks.
            mnuConfigureTrajectory.Click += new EventHandler(mnuConfigureTrajectory_Click);
            mnuConfigureTrajectory.Image = Properties.Drawings.configure;
            mnuDeleteTrajectory.Click += new EventHandler(mnuDeleteTrajectory_Click);
            mnuDeleteTrajectory.Image = Properties.Drawings.delete;

            // Magnifier
            mnuMagnifierFreeze.Click += mnuMagnifierFreeze_Click;
            mnuMagnifierFreeze.Image = Properties.Resources.image;
            mnuMagnifierTrack.Click += mnuMagnifierTrack_Click;
            mnuMagnifierTrack.Image = Properties.Resources.point3_16;
            mnuMagnifierDirect.Click += mnuMagnifierDirect_Click;
            mnuMagnifierDirect.Image = Properties.Resources.arrow_out;
            mnuMagnifierQuit.Click += mnuMagnifierQuit_Click;
            mnuMagnifierQuit.Image = Properties.Resources.hide;

            // The right context menu and its content will be choosen on MouseDown.
            panelCenter.ContextMenuStrip = popMenu;

            // Load the menu labels.
            ReloadMenusCulture();
        }

        private void BuildExportButtons()
        {
            btnExportImage.Click += (s, e) => ExportImageAsked?.Invoke(s, e);
            btnExportImageSequence.Click += (s, e) => ExportImageSequenceAsked?.Invoke(s, e);
            btnExportVideo.Click += (s, e) => ExportVideoAsked?.Invoke(s, e);
            btnExportVideoSlideshow.Click += (s, e) => ExportVideoSlideshowAsked?.Invoke(s, e);
            btnExportVideoWithPauses.Click += (s, e) => ExportVideoWithPausesAsked?.Invoke(s, e);
        }

        private void PostLoad_Idle(object sender, EventArgs e)
        {
            Application.Idle -= PostLoad_Idle;
            m_Constructed = true;
            IsWaitingForIdle = false;

            log.DebugFormat("Post load idle event.");

            if (!m_FrameServer.Loaded)
                return;

            // This is a good time to start the prebuffering/caching if supported.
            m_FrameServer.VideoReader.PostLoad();

            if (!workingZoneLoaded)
            {
                // In replay mode we focus on simple playback and synchronization,
                // and we want the video to load as fast as possible. So no caching.
                bool isReplayWatcher = screenDescriptor != null && screenDescriptor.IsReplayWatcher;
                UpdateWorkingZone(!isReplayWatcher);
            }

            // Signal post-load idle event to listeners.
            // This is used to setup synchronization in case of launching a workspace with two videos.
            // This is what might trigger the dual player to finally start playback, if we are in 
            // a dual replay context.
            Loaded?.Invoke(this, EventArgs.Empty);

            UpdateFramesMarkers();
            ShowHideRenderingSurface(true);
            ResizeUpdate(true);

            // Handle auto-playback for replay watchers.
            if (screenDescriptor != null && screenDescriptor.Autoplay)
            {
                // Ignore autoplay for dual replay watchers.
                //
                // We can't know if we are the first video of a dual recording or just a singular event.
                // And even if we are the first video we don't want to start immediately it will desynchronize everything.
                // So it becomes the responsibility of the dual player to start playing when it's sure
                // that it's a dual recording event and both screens are ready.
                //
                // We generally use the "synched" flag for this, assuming that if one is a replay, both are.
                // The case of a normal video and a replay is not handled and will also ignores auto-play.
                // 
                // A further complication, during the very first load of the first screen, the value of "synched"
                // is not yet true, but we still don't want to start that video as it will start way before the other. 
                // For this case we use the flag in the descriptor.
                bool dualReplay = m_bSynched || screenDescriptor.IsDualReplay;
                if (!dualReplay)
                {
                    // A new video just loaded in a single replay watcher, start it.
                    buttonPlay.Image = Resources.flatpause3b;
                    StartMultimediaTimer(GetPlaybackFrameInterval());
                    PlayStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(int commandCode)
        {
            // Method called by KinoveaControl in the context of preprocessing hotkeys.
            // If the hotkey can be handled by the dual player, we defer to it instead.

            if (m_FrameServer.Metadata.TextEditingInProgress)
                return false;

            if (keyframeBoxes.Any(t => t.Editing))
                return false;

            if (sidePanelKeyframes.Editing || sidePanelDrawing.Editing || sidePanelTracking.Editing)
                return false;

            // If we are not in a dual screen context just run the command for this screen.
            if (!m_bSynched || DualCommandReceived == null)
                return ExecuteScreenCommand(commandCode);

            // Try to see if that command is handled by the dual capture controller.
            // At this point the command code is still the one from the single screen context.
            // Get the full command with the target shortcut key.
            HotkeyCommand command = Hotkeys.FirstOrDefault(h => h != null && h.CommandCode == commandCode);
            if (command == null)
                return false;

            // Look for a matching handler in the dual player context.
            HotkeyCommand command2 = HotkeySettingsManager.FindCommand("DualPlayer", command.KeyData);
            if (command2 == null)
            {
                // The shortcut isn't handled at the dual screen level, run it normally.
                return ExecuteScreenCommand(commandCode);
            }
            else
            {
                DualCommandReceived(this, new EventArgs<HotkeyCommand>(command2));
                return true;
            }
        }

        public bool ExecuteScreenCommand(int cmd)
        {
            if (!m_FrameServer.Loaded)
                return false;

            PlayerScreenCommands command = (PlayerScreenCommands)cmd;

            switch (command)
            {
                // General
                case PlayerScreenCommands.ResetViewport:
                    DisablePlayAndDraw();
                    DoInvalidate();
                    break;
                case PlayerScreenCommands.Close:
                    btnClose_Click(this, EventArgs.Empty);
                    break;

                // Playback control
                case PlayerScreenCommands.TogglePlay:
                    OnButtonPlay();
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

                // Frame by frame navigation
                case PlayerScreenCommands.GotoPreviousImage:
                    buttonGotoPrevious_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoNextImage:
                    buttonGotoNext_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoFirstImage:
                    buttonGotoFirst_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoLastImage:
                    buttonGotoLast_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.GotoPreviousImageForceLoop:
                    if (m_iCurrentPosition <= m_iSelStart)
                        buttonGotoLast_Click(null, EventArgs.Empty);
                    else
                        buttonGotoPrevious_Click(null, EventArgs.Empty);
                    break;
                case PlayerScreenCommands.BackwardRound10Percent:
                    JumpToPercent(10, false);
                    break;
                case PlayerScreenCommands.ForwardRound10Percent:
                    JumpToPercent(10, true);
                    break;
                case PlayerScreenCommands.BackwardRound1Percent:
                    JumpToPercent(1, false);
                    break;
                case PlayerScreenCommands.ForwardRound1Percent:
                    JumpToPercent(1, true);
                    break;
                case PlayerScreenCommands.GotoPreviousKeyframe:
                    GotoPreviousKeyframe();
                    break;
                case PlayerScreenCommands.GotoNextKeyframe:
                    GotoNextKeyframe();
                    break;
                case PlayerScreenCommands.GotoSyncPoint:
                    ForceCurrentFrame(m_FrameServer.Metadata.TimeOrigin, true);
                    break;

                // Synchronization
                case PlayerScreenCommands.IncreaseSyncAlpha:
                    IncreaseSyncAlpha();
                    break;
                case PlayerScreenCommands.DecreaseSyncAlpha:
                    DecreaseSyncAlpha();
                    break;

                // Zoom
                case PlayerScreenCommands.IncreaseZoom:
                    IncreaseDirectZoom(new Point(pbSurfaceScreen.Width / 2, pbSurfaceScreen.Height / 2));
                    break;
                case PlayerScreenCommands.DecreaseZoom:
                    DecreaseDirectZoom(new Point(pbSurfaceScreen.Width / 2, pbSurfaceScreen.Height / 2));
                    break;
                case PlayerScreenCommands.ResetZoom:
                    ResetZoom(true);
                    break;

                // Keyframes
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
                case PlayerScreenCommands.Preset1:
                case PlayerScreenCommands.Preset2:
                case PlayerScreenCommands.Preset3:
                case PlayerScreenCommands.Preset4:
                case PlayerScreenCommands.Preset5:
                case PlayerScreenCommands.Preset6:
                case PlayerScreenCommands.Preset7:
                case PlayerScreenCommands.Preset8:
                case PlayerScreenCommands.Preset9:
                case PlayerScreenCommands.Preset10:
                    // Get user-defined keyframe preset.
                    KeyframePreset preset = PreferencesManager.PlayerPreferences.KeyframePresets.GetPreset(command);
                    AddPresetKeyframe(preset.Name, preset.Color);
                    break;

                // Annotations
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
                case PlayerScreenCommands.ValidateDrawing:
                    ValidateDrawing();
                    break;
                case PlayerScreenCommands.CopyImage:
                    CopyImageToClipboard();
                    break;
                case PlayerScreenCommands.ToggleDrawingsVisibility:
                    showDrawings = !showDrawings;
                    DoInvalidate();
                    break;
                case PlayerScreenCommands.ChronometerStartStop:
                    ChronometerStartStop();
                    break;
                case PlayerScreenCommands.ChronometerSplit:
                    ChronometerSplit();
                    break;
                case PlayerScreenCommands.CadenceBeat:
                    CadenceBeat();
                    break;
                case PlayerScreenCommands.StartAllTracking:
                    StartAllTracking();
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public void AfterClose()
        {
            selectionTimer.Tick -= SelectionTimer_OnTick;
            selectionTimer.Dispose();
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
                popMenuMagnifier.Dispose();
                popMenuFilter.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Misc Events
        private void btnClose_Click(object sender, EventArgs e)
        {
            // Propagate to PlayerScreen which will report to ScreenManager.
            if (CloseAsked != null)
                CloseAsked(this, EventArgs.Empty);
        }
        private void btnSidePanel_Click(object sender, EventArgs e)
        {
            ToggleSidePanelVisibility();
        }
        private void btnExitFilter_Click(object sender, EventArgs e)
        {
            m_FrameServer.DeactivateVideoFilter();
            DeactivateVideoFilter();
            FilterExited?.Invoke(this, EventArgs.Empty);
        }

        private void PanelVideoControls_MouseEnter(object sender, EventArgs e)
        {
            // Set focus to enable mouse scroll
            panelVideoControls.Focus();
        }
        private void MnuExitFilter_Click(object sender, EventArgs e)
        {
            m_FrameServer.DeactivateVideoFilter();
            DeactivateVideoFilter();
            FilterExited?.Invoke(this, EventArgs.Empty);
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
        private void RaiseSetAsActiveScreenEvent()
        {
            SetAsActiveScreen?.Invoke(this, EventArgs.Empty);
        }

        private void OnPoke()
        {
            //------------------------------------------------------------------------------
            // This function is a hub event handler for all button press, mouse clicks, etc.
            // Signal itself as the active screen to the ScreenManager
            // This will trigger an update of the top-level menu to enable/disable specific menus.
            //---------------------------------------------------------------------
            RaiseSetAsActiveScreenEvent();

            // 1. Ensure no DrawingText is in edit mode.
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();

            m_ActiveTool = m_ActiveTool.KeepToolFrameChanged ? m_ActiveTool : m_PointerTool;
            if (m_ActiveTool == m_PointerTool)
            {
                SetCursor(m_PointerTool.GetCursor(-1));
            }

            // 3. Dock Keyf panel if nothing to see.
            if (m_FrameServer.Metadata.Count < 1)
            {
                CollapseKeyframePanel(true);
            }
        }

        /// <summary>
        /// Update the markers in the main timeline.
        /// </summary>
        public void UpdateFramesMarkers()
        {
            trkFrame.UpdateMarkers(m_FrameServer.Metadata);
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
            ResetZoom(false);
            m_FrameServer.Metadata.InitializeEnd(true);
            m_FrameServer.Metadata.StopAllTracking();
            m_FrameServer.Metadata.DeselectAll();
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

        private void ChronometerStartStop()
        {
            foreach (var drawing in m_FrameServer.Metadata.ChronoManager.Drawings)
            {
                var timeable = drawing as ITimeable;
                if (timeable == null)
                    continue;

                timeable.StartStop(m_iCurrentPosition);
            }

            DoInvalidate();
            UpdateFramesMarkers();
        }

        private void ChronometerSplit()
        {
            foreach (var drawing in m_FrameServer.Metadata.ChronoManager.Drawings)
            {
                var timeable = drawing as ITimeable;
                if (timeable == null)
                    continue;

                timeable.Split(m_iCurrentPosition);
            }

            DoInvalidate();
            UpdateFramesMarkers();
        }

        private void CadenceBeat()
        {
            foreach (var drawing in m_FrameServer.Metadata.ChronoManager.Drawings)
            {
                var timeable = drawing as ITimeable;
                if (timeable == null)
                    continue;

                timeable.Beat(m_iCurrentPosition);
            }

            DoInvalidate();
            UpdateFramesMarkers();
        }

        private void StartAllTracking()
        {
            if (!m_FrameServer.Loaded)
                return;
            
            m_FrameServer.Metadata.StartAllTracking();
            DoInvalidate();
        }

        /// <summary>
        /// Returns the physical time in microseconds for this timestamp.
        /// Used in the context of synchronization.
        /// Input in timestamps relative to sel start.
        /// convert it into video time then to real time using high speed factor.
        private long TimestampToRealtime(long timestamp)
        {
            double correctedTPS = m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds * m_FrameServer.VideoReader.Info.AverageTimeStampsPerSeconds / m_FrameServer.Metadata.BaselineFrameInterval;

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
                // If we are outside the primary selection or we are about to leave it,
                // reset to the start point.
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
                    if (m_iCurrentPosition == oldPos)
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

            bool wasPlaying = m_bIsCurrentlyPlaying;
            StopPlaying();
            OnPauseAsked();
            m_iFramesToDecode = 1;

            float normalized = ((float)m_iCurrentPosition - m_iSelStart) / m_iSelDuration;
            int currentPercentage = (int)Math.Round(normalized * 100);
            int maxSteps = 100 / round;
            int currentStep = currentPercentage / round;
            int nextStep = forward ? currentStep + 1 : currentStep - 1;
            nextStep = Math.Max(Math.Min(nextStep, maxSteps), 0);
            int newPercentage = nextStep * round;
            long newPosition = m_iSelStart + (long)(((float)newPercentage / 100) * m_iSelDuration);

            ShowNextFrame(newPosition, true);

            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);

            if (wasPlaying)
                EnsurePlaying();
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
                // Pause playback.
                StopPlaying();
                OnPauseAsked();
                buttonPlay.Image = Resources.flatplay;
                ActivateKeyframe(m_iCurrentPosition);
            }
            else
            {
                // Start playback.
                buttonPlay.Image = Resources.flatpause3b;
                StartMultimediaTimer(GetPlaybackFrameInterval());
                PlayStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Make sure we are playing.
        /// Does not raise the play asked event.
        /// Used for continuing playback after a jump or synchronization.
        /// </summary>
        public void EnsurePlaying()
        {
            if (!m_FrameServer.Loaded || m_FrameServer.Metadata.DrawingInitializing || m_bIsCurrentlyPlaying)
                return;

            buttonPlay.Image = Resources.flatpause3b;
            StartMultimediaTimer(GetPlaybackFrameInterval());
        }

        public void Common_MouseWheel(object sender, MouseEventArgs e)
        {
            // MouseWheel was recorded on one of the controls.
            int steps = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            bool isAlt = (ModifierKeys & Keys.Alt) == Keys.Alt;
            bool isCtrl = (ModifierKeys & Keys.Control) == Keys.Control;

            if (videoFilterIsActive && isAlt)
            {
                PointF descaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
                m_FrameServer.Metadata.ActiveVideoFilter.Scroll(steps, descaledMouse, ModifierKeys);
                DoInvalidate();
            }
            else if (isCtrl)
            {
                if (steps > 0)
                    IncreaseDirectZoom(e.Location);
                else
                    DecreaseDirectZoom(e.Location);

            }
            else if (isAlt)
            {
                if (steps > 0)
                    IncreaseSyncAlpha();
                else
                    DecreaseSyncAlpha();
            }
            else
            {
                if (steps > 0)
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
        private void BtnTimeOrigin_Click(object sender, EventArgs e)
        {
            MarkTimeOrigin();
        }

        private void MarkTimeOrigin()
        {
            // Set time origin to current time.
            log.DebugFormat("Changing time origin from player. {0} -> {1}.", m_FrameServer.Metadata.TimeOrigin, m_iCurrentPosition);

            m_FrameServer.Metadata.TimeOrigin = m_iCurrentPosition;
            trkFrame.UpdateMarkers(m_FrameServer.Metadata);
            UpdateCurrentPositionLabel();
            sidePanelKeyframes.UpdateTimecodes();
            if (videoFilterIsActive)
                m_FrameServer.Metadata.ActiveVideoFilter.UpdateTimeOrigin(m_FrameServer.Metadata.TimeOrigin);

            // This will update the timecode on keyframe boxes if the user hasn't changed the kf name.
            EnableDisableKeyframes();

            // This will update the timecode on any clock object still using the overall time origin.
            DoInvalidate();

            if (TimeOriginChanged != null)
                TimeOriginChanged(this, EventArgs.Empty);
        }

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
                trkFrame.Remap(m_iSelStart, m_iSelEnd, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
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
                trkFrame.Remap(m_iSelStart, m_iSelEnd, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
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
                trkFrame.Remap(m_iSelStart, m_iSelEnd, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
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
                if (m_FrameServer.VideoReader.Current == null)
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

            if (m_FrameServer.Loaded)
            {
                start = m_iSelStart - m_iStartingPosition;
                duration = m_iSelDuration;
            }

            string startTimecode = m_FrameServer.TimeStampsToTimecode(start, TimeType.Absolute, timecodeFormat, true);
            lblSelStartSelection.Text = "◢ " + startTimecode;

            duration -= m_FrameServer.Metadata.AverageTimeStampsPerFrame;
            string durationTimecode = m_FrameServer.TimeStampsToTimecode(duration, TimeType.Duration, timecodeFormat, true);
            int right = lblSelDuration.Right;
            lblSelDuration.Text = "[" + durationTimecode + "]";
            lblSelDuration.Left = right - lblSelDuration.Width;

        }
        private void UpdateSelectionDataFromControl()
        {
            // Update WorkingZone data according to control.
            if ((m_iSelStart != trkSelection.SelStart) || (m_iSelEnd != trkSelection.SelEnd))
            {
                // Time origin: we try to maintain user-defined time origin, but we don't want the origin to stay at the absolute zero when the zone changes.
                // Check if we were previously aligned with the start of the zone, if so, keep it that way, otherwise keep the absolute value.
                if (m_FrameServer.Metadata.TimeOrigin == m_iSelStart)
                    m_FrameServer.Metadata.TimeOrigin = trkSelection.SelStart;

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
        private void trkFrame_PositionChanging(object sender, TimeEventArgs e)
        {
            if (!interactiveFrameTracker)
                return;

            if (m_FrameServer.Loaded)
            {
                // Update image but do not touch cursor, as the user is manipulating it.
                // If the position needs to be adjusted to an actual timestamp, it'll be done later.
                StopPlaying();
                UpdateFrameCurrentPosition(false);
                UpdateCurrentPositionLabel();
                lblTimeTip.Visible = true;

                ActivateKeyframe(m_iCurrentPosition);
            }
        }
        private void trkFrame_PositionChanged(object sender, TimeEventArgs e)
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
                lblTimeTip.Visible = false;
                ActivateKeyframe(m_iCurrentPosition);

                // Update WorkingZone hairline.
                trkSelection.SelPos = m_iCurrentPosition;
                trkSelection.Invalidate();
            }
        }
        private void trkFrame_KeyframeDropped(object sender, EventArgs e)
        {
            // A keyframe was dropped on the frame timeline.
            // By this point we should be on the target time.
            // This is now similar to the "move keyframe here" action.
            KeyframeControl_MoveToCurrentTimeAsked(sender, e);
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
            string timecode = m_FrameServer.TimeStampsToTimecode(m_iCurrentPosition, TimeType.UserOrigin, timecodeFormat, true);
            lblTimeCode.Text = "▼ " + timecode;
            lblTimeTip.Text = timecode;
            lblTimeTip.Left = trkFrame.PixelPosition;
        }
        private void UpdatePositionUI()
        {
            // Update markers and label for position.
            if (showCacheInTimeline)
            {
                VideoSection section;
                if (m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    section = new VideoSection(m_iSelStart, m_iSelEnd);
                else if (m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.PreBuffering)
                    section = m_FrameServer.VideoReader.PreBufferingSegment;
                else
                    section = new VideoSection(m_iCurrentPosition, m_iCurrentPosition);

                trkFrame.UpdateCacheSegmentMarker(section);
            }

            trkFrame.Position = m_iCurrentPosition;
            trkFrame.Invalidate();
            trkSelection.SelPos = m_iCurrentPosition;
            trkSelection.Invalidate();
            UpdateCurrentPositionLabel();
        }

        private void PanelVideoControls_DragDrop(object sender, DragEventArgs e)
        {
            // Dropping a keyframe somewhere in the bottom part.
            trkFrame.Commit();

            // Handle the drop.
            object keyframeBox = e.Data.GetData(typeof(KeyframeBox));
            if (keyframeBox != null && keyframeBox is KeyframeBox)
            {
                KeyframeControl_MoveToCurrentTimeAsked(keyframeBox, EventArgs.Empty);
            }
        }

        private void PanelVideoControls_DragOver(object sender, DragEventArgs e)
        {
            // Dragging a keyframe anywhere on the video controls panel.
            // We turn the whole panel into a timeline.
            e.Effect = DragDropEffects.Move;
            trkFrame.Scrub();
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

            lblSpeedTuner.Text = speedValue;
        }
        #endregion

        #endregion

        #region Auto Stretch & Manual Resize
        private void StretchSqueezeSurface(bool finished)
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
            if (!m_fill && m_lastUserStretch != m_viewportManipulator.Stretch)
                targetStretch = m_lastUserStretch;

            // Stretch factor, zoom, or container size have been updated, update the rendering and decoding sizes.
            // During the process, stretch and fill may be forced to different values.
            // Custom scaling vs decoding modes:
            // - We try to decode the images at the smallest size possible.
            // - Some states of the applications like tracking prevent this, this is stored in m_bEnableCustomDecodingSize.
            // - Some decoding modes also prevent changing the decoding size, this is set in scalable here.
            // Note: do not update decoding scale here, as this function is called during stretching of the rendering surface,
            // while the decoding size isn't updated.

            // TODO: move this to a function on video readers.
            bool scalable = m_FrameServer.VideoReader.CanScaleIndefinitely || m_FrameServer.VideoReader.DecodingMode == VideoDecodingMode.PreBuffering;
            bool canCustomDecodingSize = customDecodingSizeIsEnabled && scalable;

            bool rotatedCanvas = false;
            if (videoFilterIsActive)
                rotatedCanvas = m_FrameServer.Metadata.ActiveVideoFilter.RotatedCanvas;

            m_viewportManipulator.Manipulate(finished, panelCenter.Size, targetStretch, m_fill, m_FrameServer.ImageTransform.Zoom, canCustomDecodingSize, rotatedCanvas);

            pbSurfaceScreen.Location = m_viewportManipulator.RenderingLocation;
            pbSurfaceScreen.Size = m_viewportManipulator.RenderingSize;
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

            ImageResizerNW.Left = pbSurfaceScreen.Left - ImageResizerNW.Width / 2;
            ImageResizerNW.Top = pbSurfaceScreen.Top - ImageResizerNW.Height / 2;
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

            ResetZoom(false);
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
            if (!targetSize.FitsIn(panelCenter.Size))
                return;

            if (!m_bManualSqueeze && !m_FrameServer.VideoReader.Info.ReferenceSize.FitsIn(targetSize))
                return;

            // Area of the original size is sticky on the inside.
            if (!m_FrameServer.VideoReader.Info.ReferenceSize.FitsIn(targetSize) &&
               (m_FrameServer.VideoReader.Info.ReferenceSize.Width - _iTargetWidth < 40 &&
                m_FrameServer.VideoReader.Info.ReferenceSize.Height - _iTargetHeight < 40))
            {
                _iTargetWidth = m_FrameServer.VideoReader.Info.ReferenceSize.Width;
                _iTargetHeight = m_FrameServer.VideoReader.Info.ReferenceSize.Height;
            }

            if (!m_MinimalSize.FitsIn(targetSize))
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
        private void ResizeUpdate(bool finished)
        {
            if (!m_FrameServer.Loaded)
                return;

            StretchSqueezeSurface(finished);

            if (finished)
            {
                // Update the decoding size at the file reader level.
                // This may clear and restart the prebuffering.
                // It may not be honored by the video reader.
                if (m_FrameServer.VideoReader.CanChangeDecodingSize)
                {
                    bool accepted = m_FrameServer.VideoReader.ChangeDecodingSize(m_viewportManipulator.PreferredDecodingSize);
                    if (accepted)
                        m_FrameServer.ImageTransform.DecodingScale = m_viewportManipulator.PreferredDecodingScale;
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
            
            bool wasEnabled = customDecodingSizeIsEnabled;
            customDecodingSizeIsEnabled = !_forceDisable && !m_FrameServer.Metadata.Tracking;

            if (wasEnabled && !customDecodingSizeIsEnabled)
            {
                m_FrameServer.VideoReader.DisableCustomDecodingSize();
                ResizeUpdate(true);
                log.DebugFormat("Custom decoding size: DISABLED");
            }
            else if (!wasEnabled && customDecodingSizeIsEnabled)
            {
                ResizeUpdate(true);
                log.DebugFormat("Custom decoding size: ENABLED");
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

            log.DebugFormat("Playback paused. Avg frame time: {0:0.000} ms. Drop ratio: {1:0.00}", m_LoopWatcher.Average, m_DropWatcher.Ratio);
        }
        private void MultimediaTimer_Tick(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            if (!m_FrameServer.Loaded)
                return;

            // We cannot change the pointer to current here in case the UI is painting it,
            // so we will pass the number of drops along to the rendering.
            // The rendering will then ask for an update of the pointer to current, skipping as
            // many frames we missed during the interval.
            lock (m_TimingSync)
            {
                if (!m_bIsBusyRendering)
                {
                    int drops = m_RenderingDrops;
                    BeginInvoke((Action)delegate { Rendering_Invoked(drops); });
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
                if (m_FrameServer.VideoReader.Drops > 0)
                {
                    if (m_FrameServer.VideoReader.Drops > m_MaxDecodingDrops)
                    {
                        log.DebugFormat("Failsafe triggered on Decoding Drops ({0})", m_FrameServer.VideoReader.Drops);
                        ForceSlowdown();
                    }
                    else
                    {
                        lock (m_TimingSync)
                            m_RenderingDrops = missedFrames;
                    }
                }
                else if (m_FrameServer.VideoReader.Current != null)
                {
                    if (videoFilterIsActive)
                        m_FrameServer.Metadata.ActiveVideoFilter.UpdateTime(m_FrameServer.VideoReader.Current.Timestamp);

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

            if (m_bSynched)
            {
                StopPlaying();
                ShowNextFrame(m_iSelStart, true);
            }
            else if (m_ePlayingMode == PlayingMode.Loop)
            {
                StopMultimediaTimer();
                bool rewound = ShowNextFrame(m_iSelStart, true);

                if (rewound)
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
            
            if (!m_FrameServer.Metadata.Tracking)
            {
                sidePanelTracking.UpdateContent();
                return;
            }

            // Fixme: Tracking only supports contiguous frames,
            // but this should be the responsibility of the track tool anyway.
            if (!_contiguous)
                m_FrameServer.Metadata.StopAllTracking();
            else
                m_FrameServer.Metadata.PerformTracking(m_FrameServer.VideoReader.Current);

            sidePanelTracking.UpdateContent();
            UpdateFramesMarkers();
            CheckCustomDecodingSize(false);
        }
        private void Application_Idle(object sender, EventArgs e)
        {
            // This event fires when the window has consumed all its messages.
            // Forcing the rendering to synchronize with this event allows
            // the UI to have a chance to process non-rendering related events like
            // button clicks, mouse move, etc.
            lock (m_TimingSync)
                m_bIsBusyRendering = false;

            m_TimeWatcher.LogTime("Back to idleness");
            //m_TimeWatcher.DumpTimes();
            m_LoopWatcher.AddLoopTime(m_TimeWatcher.RawTime("Back to idleness"));
        }
        private bool ShowNextFrame(long _iSeekTarget, bool _bAllowUIUpdate)
        {
            if (!m_FrameServer.VideoReader.Loaded)
                return false;

            // TODO: More refactoring needed.
            // Eradicate the scheme where we use the _iSeekTarget parameter to mean two things.
            if (m_bIsCurrentlyPlaying)
                throw new ThreadStateException("ShowNextFrame called while play loop.");

            bool refreshInPlace = _iSeekTarget == m_iCurrentPosition;
            bool hasMore = false;

            if (_iSeekTarget < 0)
            {
                hasMore = m_FrameServer.VideoReader.MoveBy(m_iFramesToDecode, true);
            }
            else
            {
                hasMore = m_FrameServer.VideoReader.MoveTo(m_iCurrentPosition, _iSeekTarget);
            }

            if (m_FrameServer.VideoReader.Current != null)
            {
                if (videoFilterIsActive)
                    m_FrameServer.Metadata.ActiveVideoFilter.UpdateTime(m_FrameServer.VideoReader.Current.Timestamp);

                m_iCurrentPosition = m_FrameServer.VideoReader.Current.Timestamp;

                TrackDrawingsCommand.Execute(null);

                bool contiguous = _iSeekTarget < 0 && m_iFramesToDecode <= 1;
                if (!refreshInPlace)
                {
                    ComputeOrStopTracking(contiguous);
                }
                else
                {
                    // Not sure why when manually navigating refresh in place is true.
                    sidePanelTracking.UpdateContent();
                }

                if (_bAllowUIUpdate)
                    DoInvalidate();

                ReportForSyncMerge();
            }

            if (!hasMore)
            {
                // End of working zone reached.
                m_iCurrentPosition = m_iSelEnd;
                if (_bAllowUIUpdate)
                {
                    trkSelection.SelPos = m_iCurrentPosition;
                    DoInvalidate();
                }

                m_FrameServer.Metadata.StopAllTracking();
            }

            return hasMore;
        }
        private void StopPlaying(bool _bAllowUIUpdate)
        {
            if (!m_FrameServer.Loaded || !m_bIsCurrentlyPlaying)
                return;

            StopMultimediaTimer();

            lock (m_TimingSync)
            {
                m_bIsBusyRendering = false;
                m_RenderingDrops = 0;
            }

            m_iFramesToDecode = 0;

            if (_bAllowUIUpdate)
            {
                buttonPlay.Image = Resources.flatplay;
                DoInvalidate();
                UpdatePositionUI();
            }
        }
        private int GetPlaybackFrameInterval()
        {
            return (int)Math.Round(timeMapper.GetInterval(sldrSpeed.Value));
        }
        private void SelectionTimer_OnTick(object sender, EventArgs e)
        {
            if (m_FrameServer.Metadata.TextEditingInProgress)
            {
                // Ignore the timer if we are editing text, so we don't close the text editor under the user.
                selectionTimer.Stop();
                return;
            }

            // Deselect the currently selected drawing.
            // This is used for drawings that must show extra stuff for being transformed, but we
            // don't want to show the extra stuff all the time for clarity.
            m_FrameServer.Metadata.DeselectAll();
            selectionTimer.Stop();
            DoInvalidate();
            OnPoke();
        }
        #endregion

        #region Culture
        private void ReloadMenusCulture()
        {
            // Reload the text for each menu.
            // this is done at construction time and at RefreshUICulture time.

            // Background context menu.
            mnuTimeOrigin.Text = ScreenManagerLang.mnuMarkTimeAsOrigin;
            mnuDirectTrack.Text = ScreenManagerLang.mnuTrackTrajectory;
            mnuBackground.Text = ScreenManagerLang.PlayerScreenUserInterface_Background;
            mnuPasteDrawing.Text = ScreenManagerLang.mnuPasteDrawing;
            mnuPasteDrawing.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.PasteDrawing);
            mnuOpenVideo.Text = ScreenManagerLang.mnuOpenVideo;
            mnuOpenReplayWatcher.Text = ScreenManagerLang.mnuOpenReplayWatcher;
            mnuOpenReplayWatcherFolder.Text = "Open folder…";
            mnuLoadAnnotations.Text = ScreenManagerLang.mnuLoadAnalysis;
            mnuSaveAnnotations.Text = ScreenManagerLang.Generic_SaveKVA;
            mnuSaveAnnotationsAs.Text = ScreenManagerLang.Generic_SaveKVAAs;
            mnuSaveDefaultPlayerAnnotations.Text = "Save as default player annotations";
            mnuSaveDefaultCaptureAnnotations.Text = "Save as default capture annotations";
            mnuUnloadAnnotations.Text = "Unload annotations";
            mnuReloadDefaultPlayerAnnotations.Text = "Reload default player annotations";
            mnuExportVideo.Text = ScreenManagerLang.Generic_ExportVideo;
            mnuExportImage.Text = ScreenManagerLang.Generic_SaveImage;
            mnuCopyPic.Text = ScreenManagerLang.mnuCopyImageToClipboard;
            mnuCopyPic.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.CopyImage);
            mnuPastePic.Text = ScreenManagerLang.mnuPasteImage;
            mnuCloseScreen.Text = ScreenManagerLang.mnuCloseScreen;
            mnuCloseScreen.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.Close);

            // Drawings context menu.
            mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuVisibility.Text = ScreenManagerLang.Generic_Visibility;
            mnuVisibilityAlways.Text = ScreenManagerLang.dlgConfigureFading_chkAlwaysVisible;
            mnuVisibilityDefault.Text = ScreenManagerLang.mnuVisibilityDefault;
            mnuVisibilityCustom.Text = ScreenManagerLang.mnuVisibilityCustom;
            mnuVisibilityConfigure.Text = ScreenManagerLang.mnuVisibilityConfigure;
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

            // Tracking pop menu (Restart, Stop tracking)
            mnuConfigureTrajectory.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
            mnuDeleteTrajectory.Text = ScreenManagerLang.mnuDeleteDrawing;
            mnuDeleteTrajectory.ShortcutKeys = HotkeySettingsManager.GetMenuShortcut("PlayerScreen", (int)PlayerScreenCommands.DeleteDrawing);

            // Magnifier.
            mnuMagnifierFreeze.Text = ScreenManagerLang.mnuMagnifierFreeze;
            mnuMagnifierTrack.Text = ScreenManagerLang.mnuTrackTrajectory;
            mnuMagnifierDirect.Text = ScreenManagerLang.mnuMagnifierDirect;
            mnuMagnifierQuit.Text = ScreenManagerLang.mnuMagnifierQuit;
        }


        private void ReloadTooltipsCulture()
        {
            // Video controls
            toolTips.SetToolTip(buttonPlay, ScreenManagerLang.Generic_PlayPause);
            toolTips.SetToolTip(buttonGotoPrevious, ScreenManagerLang.ToolTip_Back);
            toolTips.SetToolTip(buttonGotoNext, ScreenManagerLang.ToolTip_Next);
            toolTips.SetToolTip(buttonGotoFirst, ScreenManagerLang.ToolTip_First);
            toolTips.SetToolTip(buttonGotoLast, ScreenManagerLang.ToolTip_Last);

            // Export buttons
            toolTips.SetToolTip(btnExportImage, ScreenManagerLang.Generic_SaveImage);
            toolTips.SetToolTip(btnExportImageSequence, ScreenManagerLang.ToolTip_Rafale);
            toolTips.SetToolTip(btnExportVideo, ScreenManagerLang.CommandExportVideo_FriendlyName);
            toolTips.SetToolTip(btnExportVideoSlideshow, ScreenManagerLang.ToolTip_SaveDiaporama);
            toolTips.SetToolTip(btnExportVideoWithPauses, ScreenManagerLang.ToolTip_SavePausedVideo);

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

            toolTips.SetToolTip(btnTimeOrigin, ScreenManagerLang.mnuMarkTimeAsOrigin);

            toolTips.SetToolTip(lblTimeCode, ScreenManagerLang.lblTimeCode_Text);
            toolTips.SetToolTip(lblSpeedTuner, ScreenManagerLang.toolTip_Speed);
            toolTips.SetToolTip(sldrSpeed, ScreenManagerLang.toolTip_Speed);
            toolTips.SetToolTip(lblSelStartSelection, ScreenManagerLang.lblSelStartSelection_Text);
            toolTips.SetToolTip(lblSelDuration, ScreenManagerLang.lblSelDuration_Text);
        }
        private void ReloadToolsCulture()
        {
            foreach (ToolStripItem tsi in stripDrawingTools.Items)
            {
                if (tsi is ToolStripSeparator)
                    continue;

                if (tsi is ToolStripButtonWithDropDown)
                {
                    foreach (ToolStripItem subItem in ((ToolStripButtonWithDropDown)tsi).DropDownItems)
                    {
                        if (!(subItem is ToolStripMenuItem))
                            continue;

                        AbstractDrawingTool tool = subItem.Tag as AbstractDrawingTool;
                        if (tool != null)
                        {
                            subItem.Text = tool.DisplayName;
                            subItem.ToolTipText = tool.DisplayName;
                        }
                    }

                    ((ToolStripButtonWithDropDown)tsi).UpdateToolTip();
                }
                else if (tsi is ToolStripButton)
                {
                    AbstractDrawingTool tool = tsi.Tag as AbstractDrawingTool;
                    if (tool != null)
                        tsi.ToolTipText = tool.DisplayName;
                }
            }

            btnAddKeyFrame.ToolTipText = ScreenManagerLang.ToolTip_AddKeyframe;
            btnToggleSidePanel.ToolTipText = ScreenManagerLang.ToolTip_ShowComments;
            btnToolPresets.ToolTipText = ScreenManagerLang.ToolTip_ColorProfile;
        }
        #endregion

        #region SurfaceScreen Events
        private void SurfaceScreen_MouseDown(object sender, MouseEventArgs e)
        {
            RaiseSetAsActiveScreenEvent();

            if (!m_FrameServer.Loaded)
            {
                if (e.Button == MouseButtons.Right)
                {
                    PrepareEmptyScreenContextMenu(popMenu);
                    panelCenter.ContextMenuStrip = popMenu;
                }
                return;
            }

            selectionTimer.Stop();
            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);

            if (e.Button == MouseButtons.Left)
                SurfaceScreen_LeftDown();
            else if (e.Button == MouseButtons.Middle)
                SurfaceScreen_MiddleDown();
            else if (e.Button == MouseButtons.Right)
                SurfaceScreen_RightDown();

            DoInvalidate();
        }
        private void SurfaceScreen_LeftDown()
        {
            if (m_bIsCurrentlyPlaying)
            {
                // MouseDown while playing: pause the video.
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);
            }

            m_FrameServer.Metadata.AllDrawingTextToNormalMode();

            if (m_ActiveTool == m_PointerTool)
            {
                HandToolDown();
            }
            else if (m_ActiveTool == ToolManager.Tools["Spotlight"])
            {
                CreateNewMultiDrawingItem(m_FrameServer.Metadata.DrawingSpotlight);
            }
            else if (m_ActiveTool == ToolManager.Tools["NumberSequence"])
            {
                CreateNewMultiDrawingItem(m_FrameServer.Metadata.DrawingNumberSequence);
            }
            else if (m_ActiveTool == ToolManager.Tools["Chrono"] || 
                m_ActiveTool == ToolManager.Tools["Clock"] || 
                m_ActiveTool == ToolManager.Tools["ChronoMulti"] || 
                m_ActiveTool == ToolManager.Tools["Counter"])
            {
                CreateNewDrawing(m_FrameServer.Metadata.ChronoManager.Id);
            }
            else
            {
                // Note: if the active drawing is at initialization stage, it will receive the point commit during mouse up.
                if (!m_FrameServer.Metadata.DrawingInitializing)
                {
                    AddKeyframe();
                    if (m_iActiveKeyFrameIndex >= 0)
                        CreateNewDrawing(m_FrameServer.Metadata.GetKeyframeId(m_iActiveKeyFrameIndex));
                }
            }
        }

        private void SurfaceScreen_MiddleDown()
        {
            // Middle mouse button is a shortcut to temporary use the hand tool, disregarding the selected tool.
            // It should provide exactly the same interaction mechanics as if we were using Left mouse button with hand tool selected.

            if (m_bIsCurrentlyPlaying)
            {
                // MouseDown while playing: Halt the video.
                StopPlaying();
                OnPauseAsked();
                ActivateKeyframe(m_iCurrentPosition);
            }

            HandToolDown();
        }

        private void HandToolDown()
        {
            IImageToViewportTransformer transformer = m_FrameServer.Metadata.ImageTransform;

            m_PointerTool.OnMouseDown(m_FrameServer.Metadata, transformer, m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition, defaultFadingEnabled);

            if (m_FrameServer.Metadata.HitDrawing != null)
            {
                SetCursor(cursorManager.GetManipulationCursor(m_FrameServer.Metadata.HitDrawing));
            }
            else
            {
                SetCursor(m_PointerTool.GetCursor(1));

                bool hitMagnifier = false;
                if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Active)
                {
                    hitMagnifier = m_FrameServer.Metadata.Magnifier.OnMouseDown(m_DescaledMouse, m_FrameServer.Metadata.ImageTransform);
                }

                if (!hitMagnifier)
                {
                    if (videoFilterIsActive)
                        m_FrameServer.Metadata.ActiveVideoFilter.StartMove(m_DescaledMouse);
                }
            }
        }

        private void CreateNewDrawing(Guid managerId)
        {
            m_FrameServer.Metadata.DeselectAll();

            IImageToViewportTransformer transformer = m_FrameServer.Metadata.ImageTransform;
            DistortionHelper distorter = m_FrameServer.Metadata.CalibrationHelper.DistortionHelper;

            // Special case for the text tool: if we hit on another label we go into edit mode instead of adding a new one on top of it.
            bool editingLabel = false;
            if (m_ActiveTool == ToolManager.Tools["Label"])
            {
                foreach (DrawingText label in m_FrameServer.Metadata.Labels())
                {
                    int hit = label.HitTest(m_DescaledMouse, m_iCurrentPosition, distorter, transformer);
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
                ((DrawingTrack)drawing).DisplayClosestFrame = DisplayClosestFrame;
                ((DrawingTrack)drawing).CheckCustomDecodingSize = CheckCustomDecodingSize;

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

        /// <summary>
        /// A drawing was modified without user interaction (undo/redo).
        /// </summary>
        private void AfterDrawingModified(AbstractDrawing drawing)
        {
            if (drawing == null)
                return;

            UpdateFramesMarkers();
            RefreshImage();

            // Update the side panel.
            // We don't really care if it's the same drawing or not.
            // This means that when the restored state is for another drawing 
            // that drawing will be pushed to the side panel. This feels natural 
            // in the sense that it reverts the selection action itself.
            var drawingId = drawing.Id;
            var metadata = m_FrameServer.Metadata;
            var managerId = metadata.FindManagerId(drawing);
            sidePanelDrawing.SetDrawing(drawing, managerId, drawingId);
            sidePanelTracking.SetDrawing(drawing, managerId, drawingId);
        }
        private void AfterVideoFilterModified()
        {
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
            m_FrameServer.Metadata.DeselectAll();
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
                PrepareBackgroundContextMenu(popMenu);

                mnuTimeOrigin.Enabled = false;
                mnuDirectTrack.Enabled = false;
                mnuBackground.Enabled = false;
                mnuPasteDrawing.Enabled = false;
                mnuPastePic.Enabled = false;
                panelCenter.ContextMenuStrip = popMenu;
                return;
            }

            m_FrameServer.Metadata.DeselectAll();
            AbstractDrawing hitDrawing = null;

            if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
            {
                AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
                PrepareDrawingContextMenu(drawing, popMenuDrawings);

                popMenuDrawings.Items.Add(mnuDeleteDrawing);
                panelCenter.ContextMenuStrip = popMenuDrawings;
            }
            else if ((hitDrawing = m_FrameServer.Metadata.IsOnDetachedDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
            {
                // Some extra drawing types have their own context menu for now.
                // TODO: Always use the custom menus system to host these menus inside the drawing instead of here.
                // Only the drawing itself knows what to do upon click anyway.

                if (m_FrameServer.Metadata.IsChronoLike(hitDrawing))
                {
                    AbstractDrawing drawing = hitDrawing;
                    PrepareDrawingContextMenu(drawing, popMenuDrawings);
                    popMenuDrawings.Items.Add(mnuDeleteDrawing);
                    panelCenter.ContextMenuStrip = popMenuDrawings;
                }
                else if (hitDrawing is DrawingTrack)
                {
                    DrawingTrack track = (DrawingTrack)hitDrawing;
                    PrepareTrackContextMenu(track, popMenuTrack);
                    popMenuTrack.Items.Add(mnuDeleteTrajectory);
                    panelCenter.ContextMenuStrip = popMenuTrack;
                }
                else if (hitDrawing is DrawingCoordinateSystem || hitDrawing is DrawingTestGrid)
                {
                    PrepareDrawingContextMenu(hitDrawing, popMenuDrawings);
                    panelCenter.ContextMenuStrip = popMenuDrawings;
                }
                else if (hitDrawing is AbstractMultiDrawing)
                {
                    PrepareDrawingContextMenu(hitDrawing, popMenuDrawings);
                    popMenuDrawings.Items.Add(mnuDeleteDrawing);
                    panelCenter.ContextMenuStrip = popMenuDrawings;
                }
            }
            else if (m_FrameServer.Metadata.IsOnMagnifier(m_DescaledMouse))
            {
                PrepareMagnifierContextMenu(popMenuMagnifier);

                popMenuMagnifier.Items.AddRange(new ToolStripItem[] {
                    new ToolStripSeparator(),
                    mnuMagnifierFreeze,
                    mnuMagnifierTrack,
                    new ToolStripSeparator(),
                    mnuMagnifierDirect,
                    mnuMagnifierQuit });

                panelCenter.ContextMenuStrip = popMenuMagnifier;
            }
            else if (m_ActiveTool != m_PointerTool)
            {
                // Right click in the background with tool active: tool preset configuration.
                FormToolPresets ftp = new FormToolPresets(m_ActiveTool);
                FormsHelper.Locate(ftp);
                ftp.ShowDialog();
                ftp.Dispose();
                UpdateCursor();
            }
            else
            {
                // Right click in the background with hand tool.
                if (videoFilterIsActive)
                {
                    PrepareFilterContextMenu(m_FrameServer.Metadata.ActiveVideoFilter, popMenuFilter);

                    popMenuFilter.Items.Add(new ToolStripSeparator());
                    
                    if (m_FrameServer.Metadata.ActiveVideoFilter.CanExportData)
                    {
                        List<ToolStripItem> items = m_FrameServer.Metadata.ActiveVideoFilter.GetExportDataMenu();
                        popMenuFilter.Items.AddRange(items.ToArray());
                    }
                        
                    popMenuFilter.Items.Add(mnuSaveAnnotations);
                    popMenuFilter.Items.Add(mnuSaveAnnotationsAs);

                    if (m_FrameServer.Metadata.ActiveVideoFilter.CanExportVideo)
                        popMenuFilter.Items.Add(mnuExportVideo);

                    if (m_FrameServer.Metadata.ActiveVideoFilter.CanExportImage)
                        popMenuFilter.Items.Add(mnuExportImage);

                    popMenuFilter.Items.Add(new ToolStripSeparator());
                    string filterName = VideoFilterFactory.GetFriendlyName(m_FrameServer.Metadata.ActiveVideoFilter.Type);
                    mnuExitFilter.Text = string.Format(ScreenManagerLang.mnuExitFilter, filterName);
                    popMenuFilter.Items.Add(mnuExitFilter);
                    popMenuFilter.Items.Add(mnuCloseScreen);
                    panelCenter.ContextMenuStrip = popMenuFilter;
                }
                else
                {
                    PrepareBackgroundContextMenu(popMenu);

                    mnuTimeOrigin.Visible = true;
                    mnuDirectTrack.Visible = true;
                    mnuDirectTrack.Enabled = true;
                    mnuBackground.Visible = true;
                    mnuBackground.Enabled = true;
                    mnuPasteDrawing.Visible = true;
                    mnuPasteDrawing.Enabled = DrawingClipboard.HasContent;
                    mnuPastePic.Visible = true;
                    mnuPastePic.Enabled = Clipboard.ContainsImage();

                    panelCenter.ContextMenuStrip = popMenu;
                }
            }
        }
        private void PrepareEmptyScreenContextMenu(ContextMenuStrip popMenu)
        {
            BuildReplayWatcherMenus();

            // Menu when no video is loaded
            popMenu.Items.Clear();
            popMenu.Items.AddRange(new ToolStripItem[]
            {
                mnuOpenVideo,
                mnuOpenReplayWatcher,
                new ToolStripSeparator(),
                mnuCloseScreen
            });

        }

        private void BuildReplayWatcherMenus()
        {
            mnuOpenReplayWatcher.DropDown.Items.Clear();
            mnuOpenReplayWatcher.DropDown.Items.Add(mnuOpenReplayWatcherFolder);
            mnuOpenReplayWatcher.DropDown.Items.Add(new ToolStripSeparator());

            List<CaptureFolder> ccff = PreferencesManager.CapturePreferences.CapturePathConfiguration.CaptureFolders;
            if (ccff.Count == 0)
            {
                AddConfigureCaptureFoldersMenu(mnuOpenReplayWatcher);
                return;
            }

            foreach (var cf in ccff)
            {
                CaptureFolder captureFolder = cf;
                ToolStripMenuItem mnuCaptureFolder = new ToolStripMenuItem();
                mnuCaptureFolder.Image = Properties.Resources.camera_video;
                mnuCaptureFolder.Text = captureFolder.FriendlyName;
                
                // Note: instead of hiding the corresponding menu we just check it.
                // This provides feedback of which capture folder is being watched.
                if (screenDescriptor != null && screenDescriptor.IsReplayWatcher && screenDescriptor.FullPath == captureFolder.Id.ToString())
                {
                    mnuCaptureFolder.Checked = true;
                }
                else
                {
                    mnuCaptureFolder.Click += (s, e) => OpenReplayWatcherAsked?.Invoke(this, new EventArgs<CaptureFolder>(captureFolder));
                }

                mnuOpenReplayWatcher.DropDown.Items.Add(mnuCaptureFolder);
            }

            AddConfigureCaptureFoldersMenu(mnuOpenReplayWatcher);
        }

        private void AddConfigureCaptureFoldersMenu(ToolStripMenuItem mnu)
        {
            ToolStripMenuItem mnuConfigureCaptureFolders = new ToolStripMenuItem();
            mnuConfigureCaptureFolders.Image = Properties.Capture.explorer_video;
            mnuConfigureCaptureFolders.Text = "Configure capture folders";

            mnuConfigureCaptureFolders.Click += (s, e) => {
                NotificationCenter.RaisePreferenceTabAsked(this, PreferenceTab.Capture_Paths);
            };

            mnu.DropDown.Items.Add(new ToolStripSeparator());
            mnu.DropDown.Items.Add(mnuConfigureCaptureFolders);
        }

        private void PrepareBackgroundContextMenu(ContextMenuStrip popMenu)
        {
            // Inject the target file name to avoid surprises.
            if (!string.IsNullOrEmpty(m_FrameServer.Metadata.LastKVAPath))
            {
                string filename = Path.GetFileName(m_FrameServer.Metadata.LastKVAPath);
                mnuSaveAnnotations.Text = string.Format("{0} ({1})",
                    ScreenManagerLang.Generic_SaveKVA, filename);
            }
            else
            {
                mnuSaveAnnotations.Text = ScreenManagerLang.Generic_SaveKVA;
            }

            bool hasDefaultPlayerKVA = !string.IsNullOrEmpty(PreferencesManager.PlayerPreferences.PlaybackKVA);
            mnuReloadDefaultPlayerAnnotations.Enabled = hasDefaultPlayerKVA;
            
            BuildReplayWatcherMenus();

            popMenu.Items.Clear();
            popMenu.Items.AddRange(new ToolStripItem[]
            {
                mnuTimeOrigin,
                mnuDirectTrack,
                mnuBackground,
                new ToolStripSeparator(),
                mnuCopyPic,
                mnuPastePic,
                mnuPasteDrawing,
                new ToolStripSeparator(),
                mnuOpenVideo,
                mnuOpenReplayWatcher,
                new ToolStripSeparator(),
                mnuExportVideo,
                mnuExportImage,
                new ToolStripSeparator(),
                mnuLoadAnnotations,
                mnuReloadDefaultPlayerAnnotations,
                new ToolStripSeparator(),
                mnuSaveAnnotations,
                mnuSaveAnnotationsAs,
                mnuSaveDefaultPlayerAnnotations,
                mnuSaveDefaultCaptureAnnotations,
                new ToolStripSeparator(),
                mnuUnloadAnnotations,
                new ToolStripSeparator(),
                mnuCloseScreen
            });
        }
        private void PrepareDrawingContextMenu(AbstractDrawing drawing, ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();

            // Generic menus based on the drawing capabilities: configuration (style), visibility, tracking.
            if (!m_FrameServer.Metadata.DrawingInitializing)
                PrepareDrawingContextMenuCapabilities(drawing, popMenu);

            // Custom menu handlers implemented by the drawing itself.
            // These change the drawing core state. (ex: angle orientation, measurement display option, start/stop chrono, etc.).
            bool hasExtraMenus = AddDrawingCustomMenus(drawing, popMenu.Items);

            // "Goto parent keyframe" menu.
            if (!m_FrameServer.Metadata.DrawingInitializing && drawing.InfosFading != null && m_FrameServer.Metadata.IsAttachedDrawing(drawing))
            {
                bool gotoVisible = defaultFadingEnabled && (drawing.InfosFading.ReferenceTimestamp != m_iCurrentPosition);
                if (gotoVisible)
                {
                    popMenu.Items.Add(mnuGotoKeyframe);
                    hasExtraMenus = true;
                }
            }

            // Below the custom menus and the goto keyframe we have the generic copy-paste and the delete menu.
            // Some singleton drawings cannot be deleted nor copy-pasted, so they don't need this.
            if (drawing is DrawingCoordinateSystem || drawing is DrawingTestGrid)
                return;

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
            if ((drawing.Caps & DrawingCapabilities.ConfigureColor) == DrawingCapabilities.ConfigureColor ||
               (drawing.Caps & DrawingCapabilities.ConfigureColorSize) == DrawingCapabilities.ConfigureColorSize)
            {
                mnuConfigureDrawing.Text = ScreenManagerLang.Generic_ConfigurationElipsis;
                popMenu.Items.Add(mnuConfigureDrawing);
                popMenu.Items.Add(mnuSepDrawing);
            }

            if (defaultFadingEnabled && ((drawing.Caps & DrawingCapabilities.Fading) == DrawingCapabilities.Fading))
            {
                mnuVisibilityDefault.Checked = drawing.InfosFading.UseDefault;
                mnuVisibilityAlways.Checked = !drawing.InfosFading.UseDefault && drawing.InfosFading.AlwaysVisible;
                mnuVisibilityCustom.Checked = !drawing.InfosFading.UseDefault && !drawing.InfosFading.AlwaysVisible;
                popMenu.Items.Add(mnuVisibility);
            }

            if ((drawing.Caps & DrawingCapabilities.Opacity) == DrawingCapabilities.Opacity)
            {
                popMenu.Items.Add(mnuVisibility);
            }

            if ((drawing.Caps & DrawingCapabilities.Track) == DrawingCapabilities.Track)
            {
                bool tracked = ToggleTrackingCommand.CurrentState(drawing);
                mnuDrawingTrackingStart.Visible = !tracked;
                mnuDrawingTrackingStop.Visible = tracked;
                popMenu.Items.Add(mnuDrawingTracking);
            }
        }
        private bool AddDrawingCustomMenus(AbstractDrawing drawing, ToolStripItemCollection menuItems)
        {
            List<ToolStripItem> extraMenu;

            // FIXME: some drawings use the ContextMenu property and others have a GetContextMenu function.
            // Generalize the usage of the function, it gives more room in the implementation.
            if (drawing is DrawingChronoMulti)
                extraMenu = ((DrawingChronoMulti)drawing).GetContextMenu(m_iCurrentPosition);
            else if (drawing is DrawingCounter)
                extraMenu = ((DrawingCounter)drawing).GetContextMenu(m_iCurrentPosition);
            else
                extraMenu = drawing.ContextMenu;

            bool hasExtraMenu = (extraMenu != null && extraMenu.Count > 0);
            if (!hasExtraMenu)
                return false;

            foreach (ToolStripItem tsmi in extraMenu)
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
        private void PrepareTrackContextMenu(DrawingTrack track, ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();
            popMenu.Items.Add(mnuConfigureTrajectory);
            popMenu.Items.Add(new ToolStripSeparator());

            bool customMenus = AddDrawingCustomMenus(track, popMenu.Items);
            if (customMenus)
                popMenu.Items.Add(new ToolStripSeparator());
        }
        private void PrepareMagnifierContextMenu(ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();
            Magnifier magnifier = m_FrameServer.Metadata.Magnifier;

            foreach (ToolStripItem tsmi in magnifier.ContextMenu)
            {
                ToolStripMenuItem menuItem = tsmi as ToolStripMenuItem;

                // Inject dependency on the UI for invalidation.
                tsmi.Tag = this;
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem subMenu in menuItem.DropDownItems)
                        subMenu.Tag = this;
                }

                popMenu.Items.Add(tsmi);
            }

            mnuMagnifierFreeze.Text = magnifier.Frozen ? ScreenManagerLang.mnuMagnifierUnfreeze : ScreenManagerLang.mnuMagnifierFreeze;
            mnuMagnifierTrack.Checked = ToggleTrackingCommand.CurrentState(m_FrameServer.Metadata.Magnifier);
        }
        private void PrepareFilterContextMenu(IVideoFilter filter, ContextMenuStrip popMenu)
        {
            popMenu.Items.Clear();

            if (filter == null || !filter.HasContextMenu)
                return;

            List< ToolStripItem> menus = filter.GetContextMenu(m_DescaledMouse, m_iCurrentPosition);
            foreach (ToolStripItem tsmi in menus)
            {
                ToolStripMenuItem menuItem = tsmi as ToolStripMenuItem;

                // Inject dependency on the UI into the menu for invalidation.
                tsmi.Tag = this;
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    foreach (ToolStripItem subMenu in menuItem.DropDownItems)
                        subMenu.Tag = this;
                }

                if (tsmi.MergeIndex >= 0)
                    popMenu.Items.Insert(tsmi.MergeIndex, tsmi);
                else
                    popMenu.Items.Add(tsmi);
            }
        }
        private void SurfaceScreen_MouseMove(object sender, MouseEventArgs e)
        {
            // We must keep the same Z order.
            // 1:Magnifier, 2:Drawings, 3:Chronos/Tracks
            // When creating a drawing, the active tool will stay on this drawing until its setup is over.
            // After the drawing is created, we either fall back to Pointer tool or stay on the same tool.

            if (!m_FrameServer.Loaded)
                return;

            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);

            if (e.Button == MouseButtons.None && m_FrameServer.Metadata.Magnifier.Initializing)
            {
                // Moving the magnifier source area around.
                m_FrameServer.Metadata.Magnifier.InitializeMove(m_DescaledMouse, ModifierKeys);
                DoInvalidate();
            }
            else if (e.Button == MouseButtons.None && m_FrameServer.Metadata.DrawingInitializing)
            {
                // Moving the third+ point of a drawing that was just created.
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
                    // Moving the second point of a drawing that was just created.
                    // Tools that are not IInitializable should reset to Pointer tool right after creation.
                    if (m_ActiveTool == ToolManager.Tools["Spotlight"])
                    {
                        IInitializable initializableDrawing = m_FrameServer.Metadata.DrawingSpotlight as IInitializable;
                        initializableDrawing.InitializeMove(m_DescaledMouse, ModifierKeys);
                    }
                    else if (!m_bIsCurrentlyPlaying && m_iActiveKeyFrameIndex >= 0 && m_FrameServer.Metadata.HitDrawing != null)
                    {
                        IInitializable initializableDrawing = m_FrameServer.Metadata.HitDrawing as IInitializable;
                        if (initializableDrawing != null)
                            initializableDrawing.InitializeMove(m_DescaledMouse, ModifierKeys);
                    }

                    if (!m_bIsCurrentlyPlaying)
                    {
                        DoInvalidate();
                    }
                }
                else if (!m_bIsCurrentlyPlaying)
                {
                    HandMove();
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                // Middle mouse button: supercedes the selected tool to provide manipulation.
                if (!m_bIsCurrentlyPlaying)
                {
                    HandMove();
                }
            }
        }

        private void HandMove()
        {
            // Hand tool interaction.
            // - Manipulation of an existing drawing via a handle.
            // - Time grab.
            // - Manipulation in a video filter.
            // - Panning the video while zoomed in.

            bool movedObject = m_PointerTool.OnMouseMove(m_FrameServer.Metadata, m_DescaledMouse, m_FrameServer.ImageTransform.ZoomWindow.Location, ModifierKeys);
            if (movedObject)
            {
                DoInvalidate();
                return;
            }

            if (m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Active)
            {
                movedObject = m_FrameServer.Metadata.Magnifier.OnMouseMove(m_DescaledMouse, ModifierKeys);
                if (movedObject)
                {
                    DoInvalidate();
                    return;
                }
            }

            // User is not moving anything: time-grab, filter interaction, pan.
            bool isAlt = (ModifierKeys & Keys.Alt) == Keys.Alt;
            bool isCtrl = (ModifierKeys & Keys.Control) == Keys.Control;
            if (isAlt)
            {
                // Time grab.
                float dtx = m_PointerTool.MouseDeltaOrigin.X * timeGrabSpeed;
                float dty = m_PointerTool.MouseDeltaOrigin.Y * timeGrabSpeed;
                float dt = Math.Abs(dtx) > Math.Abs(dty) ? dtx : dty;
                long target = m_PointerTool.OriginTime - (long)(dt * m_FrameServer.Metadata.AverageTimeStampsPerFrame);
                target = Math.Min(Math.Max(m_iSelStart, target), m_iSelEnd);

                // FIXME: Ignore / skip if busy.
                m_iFramesToDecode = 1;
                ShowNextFrame(target, true);
                UpdatePositionUI();
            }
            else if (videoFilterIsActive && !isCtrl)
            {
                // Filter-specific.
                float dx = m_PointerTool.MouseDelta.X;
                float dy = m_PointerTool.MouseDelta.Y;
                m_FrameServer.Metadata.ActiveVideoFilter.Move(dx, dy, ModifierKeys);
            }
            else
            {
                // CTRL or no modifiers on background: pan.
                float dx = m_PointerTool.MouseDelta.X;
                float dy = m_PointerTool.MouseDelta.Y;
                bool contain = m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.Inactive;
                m_FrameServer.ImageTransform.MoveZoomWindow(dx, dy, contain);
            }

            DoInvalidate();
        }

        private void SurfaceScreen_MouseUp(object sender, MouseEventArgs e)
        {
            // End of an action.
            // Depending on the active tool we have various things to do.

            if (!m_FrameServer.Loaded)
                return;

            if (videoFilterIsActive && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle))
                m_FrameServer.Metadata.ActiveVideoFilter.StopMove();

            if (e.Button == MouseButtons.Middle)
            {
                // Special case where we pan around with an active tool that is not the hand tool.
                // Restore the cursor of the active tool.
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
                m_PointerTool.OnMouseUp(m_FrameServer.Metadata);
                m_FrameServer.Metadata.Magnifier.OnMouseUp();

                // If we were resizing an SVG drawing, trigger a render.
                // TODO: this is currently triggered on every mouse up, not only on resize !
                DrawingSVG d = m_FrameServer.Metadata.HitDrawing as DrawingSVG;
                if (d != null)
                    d.ResizeFinished();
            }

            if (m_FrameServer.Metadata.HitDrawing != null && !m_FrameServer.Metadata.DrawingInitializing)
            {
                // A drawing was just selected.
                selectionTimer.Start();
            }

            DoInvalidate();
        }
        private void SurfaceScreen_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!m_FrameServer.Loaded || e.Button != MouseButtons.Left || m_ActiveTool != m_PointerTool)
                return;

            OnPoke();

            m_DescaledMouse = m_FrameServer.ImageTransform.Untransform(e.Location);
            m_FrameServer.Metadata.AllDrawingTextToNormalMode();
            m_FrameServer.Metadata.DeselectAll();

            AbstractDrawing hitDrawing = null;

            //------------------------------------------------------------------------------------
            // - If on text, switch to edit mode.
            // - If on sticker, show sticker selector.
            // - If on other drawing, launch the configuration dialog.
            // - Otherwise -> Maximize/Reduce image.
            //------------------------------------------------------------------------------------
            if (m_FrameServer.Metadata.IsOnDrawing(m_iActiveKeyFrameIndex, m_DescaledMouse, m_iCurrentPosition))
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
                else if (drawing is DrawingBitmap)
                {
                    DrawingBitmap db = drawing as DrawingBitmap;
                    if (db.IsSticker)
                    {
                        // TODO: UNDO/REDO support.
                        //var drawingId = metadata.HitDrawing.Id;
                        //var managerId = metadata.FindManagerId(metadata.HitDrawing);
                        //var memento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, metadata.HitDrawing.Name, SerializationFilter.Style);

                        bool changed = db.SelectSticker();
                        DoInvalidate();
                        //m_FrameServer.HistoryStack.PushNewCommand(memento);
                    }
                }
                else
                {
                    ShowSidePanel(1);
                }
            }
            else if ((hitDrawing = m_FrameServer.Metadata.IsOnDetachedDrawing(m_DescaledMouse, m_iCurrentPosition)) != null)
            {
                if (m_FrameServer.Metadata.IsChronoLike(hitDrawing))
                {
                    ShowSidePanel(1);
                }
                else if (hitDrawing is DrawingTrack)
                {
                    ShowSidePanel(2);
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
            if (!m_FrameServer.Loaded || saveInProgress || dualSaveInProgress)
                return;

            m_TimeWatcher.LogTime("Actual start of paint");

            if (m_FrameServer.CurrentImage != null)
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

                    if (m_MessageToaster.Enabled)
                        m_MessageToaster.Draw(e.Graphics);

                    //log.DebugFormat("play loop to end of paint: {0}/{1}", m_Stopwatch.ElapsedMilliseconds, m_FrameServer.VideoReader.Info.FrameIntervalMilliseconds);
                }
                catch (System.InvalidOperationException)
                {
                    log.Error("Error while painting image. Object is currently in use elsewhere.");
                }
                catch
                {
                    log.Error("Error while painting image.");
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

            // New.
            Rectangle rDst = new Rectangle(Point.Empty, _renderingSize);

            bool drawn = false;
            if (m_viewportManipulator.MayDrawUnscaled && m_FrameServer.VideoReader.CanDrawUnscaled)
            {
                // Source image should be at the right size, unless it has been temporarily disabled.
                // This is an optimization where the video reader is asked to decode images that might be smaller than the original size,
                // in order to match the rendering size.
                if (!m_FrameServer.Metadata.Mirrored  && _transform.ZoomWindowInDecodedImage.Size.CloseTo(_renderingSize, 4))
                {
                    g.DrawImageUnscaled(_sourceImage, -_transform.ZoomWindowInDecodedImage.Left, -_transform.ZoomWindowInDecodedImage.Top);
                    drawn = true;
                }
            }
            else if (!m_FrameServer.Metadata.Mirrored && !_transform.Zooming && _transform.Stretch == 1.0f && _transform.DecodingScale == 1.0)
            {
                // This allow to draw unscaled while tracking or caching for example, provided we are rendering at original size.
                g.DrawImageUnscaled(_sourceImage, -_transform.ZoomWindowInDecodedImage.Left, -_transform.ZoomWindowInDecodedImage.Top);
                drawn = true;
            }

            if (!drawn)
            {
                Rectangle rSrc;
                if (m_FrameServer.Metadata.Mirrored)
                {
                    rSrc = new Rectangle(
                        _sourceImage.Width - 1 - _transform.ZoomWindowInDecodedImage.X,
                        _transform.ZoomWindowInDecodedImage.Top,
                        -_transform.ZoomWindowInDecodedImage.Width,
                        _transform.ZoomWindowInDecodedImage.Height
                     );
                }
                else
                {
                    rSrc = _transform.ZoomWindowInDecodedImage;
                }

                g.DrawImage(_sourceImage, rDst, rSrc, GraphicsUnit.Pixel);
            }

            m_TimeWatcher.LogTime("After DrawImage");

            // .Sync superposition.
            if (m_bSynched && m_bSyncMerge && m_SyncMergeImage != null)
            {
                // The mirroring, if any, will have been done already and applied to the sync image.
                // (because to draw the other image, we take into account its own mirroring option,
                // not the option in this screen.)
                Rectangle rSyncDst = new Rectangle(0, 0, _renderingSize.Width, _renderingSize.Height);
                g.DrawImage(m_SyncMergeImage, rSyncDst, 0, 0, m_SyncMergeImage.Width, m_SyncMergeImage.Height, GraphicsUnit.Pixel, m_SyncMergeImgAttr);
            }

            // Background color and alpha.
            Color backgroundColor = m_FrameServer.Metadata.BackgroundColor;
            if (backgroundColor.A != 0)
            {
                using (SolidBrush brush = new SolidBrush(backgroundColor))
                    g.FillRectangle(brush, rDst);
            }

            if (
                (showDrawings && m_bIsCurrentlyPlaying && drawOnPlay) ||
                (showDrawings && !m_bIsCurrentlyPlaying))
            {
                // First draw the magnifier, this includes drawing the objects that are under
                // the source area onto the destination area, and then draw the objects on the
                // image. This way we can still have drawings on top of the magnifier destination area.
                FlushMagnifierOnGraphics(_sourceImage, g, _transform, _iKeyFrameIndex, _iPosition);
                FlushDrawingsOnGraphics(g, _transform, _iKeyFrameIndex, _iPosition);
            }
        }
        private void FlushDrawingsOnGraphics(Graphics canvas, ImageTransform transformer, int keyFrameIndex, long timestamp)
        {
            DistortionHelper distorter = m_FrameServer.Metadata.CalibrationHelper.DistortionHelper;
            CameraTransformer camTransformer = m_FrameServer.Metadata.CameraTransformer;

            // Prepare for drawings
            canvas.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            bool drawAttached = true;
            bool drawDetached = true;
            if (m_FrameServer.Metadata.ActiveVideoFilter != null)
            {
                m_FrameServer.Metadata.ActiveVideoFilter.DrawExtra(canvas, distorter, transformer, timestamp, false);
                drawAttached = m_FrameServer.Metadata.ActiveVideoFilter.DrawAttachedDrawings;
                drawDetached = m_FrameServer.Metadata.ActiveVideoFilter.DrawDetachedDrawings;
            }

            if (drawDetached)
            {
                foreach (AbstractDrawing chrono in m_FrameServer.Metadata.ChronoManager.Drawings)
                {
                    bool selected = m_FrameServer.Metadata.HitDrawing == chrono;
                    chrono.Draw(canvas, distorter, camTransformer, transformer, selected, timestamp);
                }

                foreach (DrawingTrack track in m_FrameServer.Metadata.TrackManager.Drawings)
                {
                    bool selected = m_FrameServer.Metadata.HitDrawing == track;
                    track.Draw(canvas, distorter, camTransformer, transformer, selected, timestamp);
                }

                foreach (AbstractDrawing drawing in m_FrameServer.Metadata.SingletonDrawingsManager.Drawings)
                {
                    bool selected = m_FrameServer.Metadata.HitDrawing == drawing;
                    drawing.Draw(canvas, distorter, camTransformer, transformer, selected, timestamp);
                }
            }

            if (drawAttached)
            {
                if (defaultFadingEnabled)
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
                            keyframe.Drawings[drawingIndex].Draw(canvas, distorter, camTransformer, transformer, selected, timestamp);
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
                        keyframe.Drawings[drawingIndex].Draw(canvas, distorter, camTransformer, transformer, selected, timestamp);
                    }
                }
                else
                {
                    // This is not a Keyframe, and fading is off.
                    // Hence, there is no drawings to draw here.
                }
            }
        }
        private void FlushMagnifierOnGraphics(Bitmap currentImage, Graphics canvas, ImageTransform transform, int keyFrameIndex, long timestamp)
        {
            // Note: the Graphics object must not be the one extracted from the image itself.
            // If needed, clone the image.
            if (currentImage == null || m_FrameServer.Metadata.Magnifier.Mode == MagnifierMode.Inactive)
                return;

            // Draw the magnifier source rectangle and magnified area.
            m_FrameServer.Metadata.Magnifier.Draw(currentImage, canvas, transform, m_FrameServer.Metadata.Mirrored, m_FrameServer.VideoReader.Info.ReferenceSize);

            // Redraw the annotations on top of the magnified area.
            m_FrameServer.Metadata.Magnifier.TransformCanvas(canvas, transform);
            FlushDrawingsOnGraphics(canvas, transform, keyFrameIndex, timestamp);
            canvas.ResetTransform();
            canvas.ResetClip();
        }
        public void DoInvalidate()
        {
            // This function should be the single point where we call for rendering.
            // Here we can decide to render directly on the surface, go through the Windows message pump, force the refresh, etc.
            //log.DebugFormat("DoInvalidate main viewport.");

            // Invalidate is asynchronous and several Invalidate calls will be grouped together. (Only one repaint will be done).
            pbSurfaceScreen.Invalidate();
        }
        public void InvalidateFromMenu()
        {
            if (SetAsActiveScreen != null)
                SetAsActiveScreen(this, EventArgs.Empty);

            DoInvalidate();
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
            if (m_Constructed)
                ResizeUpdate(true);
        }
        private void PanelCenter_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                RaiseSetAsActiveScreenEvent();
                return;
            }

            SurfaceScreen_MouseDown(sender, e);
            
            // The following only make sense if the mouse is somewhere above the image.
            mnuDirectTrack.Enabled = false;
            mnuPasteDrawing.Enabled = false;
            mnuPastePic.Enabled = false;
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
            CollapseKeyframePanel(m_bKeyframePanelCollapsed);
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
            sidePanelKeyframes.Clear();

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
                    box.Selected += KeyframeControl_Selected;
                    box.ShowCommentsAsked += KeyframeControl_ShowCommentsAsked;
                    box.MoveToCurrentTimeAsked += KeyframeControl_MoveToCurrentTimeAsked;
                    box.DeleteAsked += KeyframeControl_DeleteAsked;

                    pixelsOffset += (pixelsSpacing + box.Width);

                    pnlThumbnails.Controls.Add(box);
                    keyframeBoxes.Add(box);
                }

                EnableDisableKeyframes();
                pnlThumbnails.Refresh();
            }
            else
            {
                CollapseKeyframePanel(true);
                m_iActiveKeyFrameIndex = -1;
            }

            sidePanelKeyframes.Reset(m_FrameServer.Metadata);
            
            UpdateFramesMarkers();
            DoInvalidate(); // Because of trajectories with keyframes labels.
        }
        private void SetupDefaultThumbBox(UserControl _box)
        {
            _box.Top = 10;
            _box.Cursor = Cursors.Hand;
        }
        private void ActivateKeyframe(long timestamp)
        {
            ActivateKeyframe(timestamp, true);
            sidePanelKeyframes.HighlightKeyframe(timestamp);
        }
        private void ActivateKeyframe(long _iPosition, bool _bAllowUIUpdate)
        {
            //--------------------------------------------------------------
            // Black border every keyframe, unless it is at the given position.
            // This method might be called with -1 to force complete blackout.
            //--------------------------------------------------------------

            // This method is called on each frame during frame-by-frame navigation.
            // keep it fast or fix the strategy.

            sidePanelKeyframes.HighlightKeyframe(_iPosition);

            m_iActiveKeyFrameIndex = -1;
            if (keyframeBoxes.Count != m_FrameServer.Metadata.Count)
                return;

            for (int i = 0; i < keyframeBoxes.Count; i++)
            {
                if (m_FrameServer.Metadata[i].Timestamp == _iPosition)
                {
                    m_iActiveKeyFrameIndex = i;
                    if (_bAllowUIUpdate)
                    {
                        keyframeBoxes[i].DisplayAsSelected(true);
                        pnlThumbnails.ScrollControlIntoView(keyframeBoxes[i]);

                        if (!m_FrameServer.Metadata[i].HasThumbnails && m_FrameServer.CurrentImage != null)
                        {
                            m_FrameServer.Metadata[i].InitializeImage(m_FrameServer.CurrentImage);
                            keyframeBoxes[i].UpdateImage();
                        }
                    }
                }
                else
                {
                    if (_bAllowUIUpdate)
                        keyframeBoxes[i].DisplayAsSelected(false);
                }
            }
        }
        private void EnableDisableKeyframes()
        {
            m_FrameServer.Metadata.EnableDisableKeyframes();

            foreach (KeyframeBox box in keyframeBoxes)
                box.UpdateEnableStatus();
        }

        // The keyframe name or color was changed.
        public void OnKeyframeNameChanged()
        {
            m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();
            EnableDisableKeyframes();
            UpdateFramesMarkers();
            DoInvalidate();
        }
        public void GotoNextKeyframe()
        {
            if (m_FrameServer.Metadata.Count == 0)
                return;

            bool wasPlaying = m_bIsCurrentlyPlaying;
            int next = -1;
            for (int i = 0; i < m_FrameServer.Metadata.Count; i++)
            {
                if (m_iCurrentPosition < m_FrameServer.Metadata[i].Timestamp)
                {
                    next = i;
                    break;
                }
            }

            if (next >= 0 && m_FrameServer.Metadata[next].Timestamp <= m_iSelEnd)
                KeyframeControl_Selected(null, new TimeEventArgs(m_FrameServer.Metadata[next].Timestamp));

            if (wasPlaying)
                EnsurePlaying();
        }
        public void GotoPreviousKeyframe()
        {
            if (m_FrameServer.Metadata.Count == 0)
                return;

            bool wasPlaying = m_bIsCurrentlyPlaying;
            int prev = -1;
            for (int i = m_FrameServer.Metadata.Count - 1; i >= 0; i--)
            {
                if (m_iCurrentPosition > m_FrameServer.Metadata[i].Timestamp)
                {
                    prev = i;
                    break;
                }
            }

            if (prev >= 0 && m_FrameServer.Metadata[prev].Timestamp >= m_iSelStart)
                KeyframeControl_Selected(null, new TimeEventArgs(m_FrameServer.Metadata[prev].Timestamp));

            if (wasPlaying)
                EnsurePlaying();
        }

        public void AddKeyframe()
        {
            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            if (keyframeIndex >= 0)
            {
                // There is already a keyframe here, just select it.
                m_iActiveKeyFrameIndex = keyframeIndex;
                Keyframe keyframe = m_FrameServer.Metadata.GetKeyframe(m_FrameServer.Metadata.GetKeyframeId(keyframeIndex));
                m_FrameServer.Metadata.SelectKeyframe(keyframe);
                return;
            }

            if (KeyframeAdding != null)
                KeyframeAdding(this, new KeyframeAddEventArgs(m_iCurrentPosition, null, Keyframe.DefaultColor));
        }

        public void AddPresetKeyframe(string name, Color color)
        {
            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            if (keyframeIndex >= 0)
            {
                // If there is already a keyframe here, do not overwrite it.
                return;
            }

            if (KeyframeAdding != null)
                KeyframeAdding(this, new KeyframeAddEventArgs(m_iCurrentPosition, name, color));
        }

        private void AfterKeyframeAdded(Guid keyframeId)
        {
            if (m_FrameServer.Metadata.KVAImporting)
                return;

            Keyframe keyframe = m_FrameServer.Metadata.GetKeyframe(keyframeId);
            if (keyframe == null)
                return;

            if (!keyframe.HasThumbnails)
                InitializeKeyframe(keyframe);

            OrganizeKeyframes();
            UpdateFramesMarkers();

            if (m_FrameServer.Metadata.Count == 1)
                CollapseKeyframePanel(false);

            if (!m_bIsCurrentlyPlaying)
                ActivateKeyframe(m_iCurrentPosition);
        }

        private void AfterKeyframeModified(Guid id)
        {
            // A keyframe was modified from the outside. This happens on undo for example.
            // Update the UI version of the keyframe.
            sidePanelKeyframes.UpdateKeyframe(id);
            KeyframeControl_KeyframeUpdated(null, new EventArgs<Guid>(id));
        }

        /// <summary>
        /// Initialize keyframes after KVA file import.
        /// </summary>
        private void InitializeKeyframes()
        {
            int firstOutOfRange = -1;
            int currentKeyframe = -1;
            long lastTimestamp = m_FrameServer.VideoReader.Info.FirstTimeStamp + m_FrameServer.VideoReader.Info.DurationTimeStamps;

            // We only create thumbnails for a few keyframes to avoid freezing on large load.
            // The other ones will be initialized later when the play head lands on them.
            int preloaded = 0;
            int maxPreload = PreferencesManager.PlayerPreferences.PreloadKeyframes;
            foreach (Keyframe kf in m_FrameServer.Metadata.Keyframes)
            {
                currentKeyframe++;

                if (kf.Timestamp < lastTimestamp)
                {
                    if (!kf.HasThumbnails && preloaded < maxPreload)
                        InitializeKeyframe(kf);

                    preloaded++;
                    continue;
                }

                if (firstOutOfRange < 0)
                {
                    firstOutOfRange = currentKeyframe;
                    break;
                }
            }

            if (firstOutOfRange != -1)
                m_FrameServer.Metadata.Keyframes.RemoveRange(firstOutOfRange, m_FrameServer.Metadata.Keyframes.Count - firstOutOfRange);
        }

        /// <summary>
        /// Fully initialize a keyframe thunbmail by seeking to the keyframe position and getting the image.
        /// </summary>
        private void InitializeKeyframe(Keyframe keyframe)
        {
            if (m_iCurrentPosition != keyframe.Timestamp)
            {
                m_iFramesToDecode = 1;
                ShowNextFrame(keyframe.Timestamp, true);
                UpdatePositionUI();
            }

            if (m_FrameServer.CurrentImage == null)
                return;

            // The actual position may differ from what was originally stored in the keyframe.
            keyframe.InitializePosition(m_iCurrentPosition);
            keyframe.InitializeImage(m_FrameServer.CurrentImage);
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
                if (kf.Timestamp >= m_iSelStart && kf.Timestamp <= m_iSelEnd)
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

        #region Thumbnail box event Handlers
        private void KeyframeControl_DeleteAsked(object sender, EventArgs e)
        {
            KeyframeBox keyframeBox = sender as KeyframeBox;
            if (keyframeBox == null)
                return;

            DeleteKeyframe(keyframeBox.Keyframe.Id);

            // Set as active screen is done after in case we don't have any keyframes left.
            OnPoke();
        }

        private void KeyframeControl_MoveToCurrentTimeAsked(object sender, EventArgs e)
        {
            log.DebugFormat("Moving existing keyframe to a new time.");

            KeyframeBox keyframeBox = sender as KeyframeBox;
            if (keyframeBox == null)
                return;

            Keyframe keyframe = keyframeBox.Keyframe;
            if (keyframe == null)
                return;

            // If there is already a keyframe at the current time we ignore the request.
            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            if (keyframeIndex >= 0)
            {
                log.WarnFormat("Ignored move request: there is already a keyframe at the current time.");
                return;
            }

            // Check if this keyframe is ours.
            var knownKeyframe = m_FrameServer.Metadata.GetKeyframe(keyframe.Id);
            if (knownKeyframe == null)
            {
                // The keyframe is coming from outside.
                // Create a brand new one here and import the data.
                log.DebugFormat("Keyframe move: importing an external keyframe.");

                AddKeyframe();
                Keyframe newKf = m_FrameServer.Metadata.HitKeyframe;
                if (newKf == null)
                {
                    log.ErrorFormat("Keyframe move: a problem occurred while creating the recipient keyframe.");
                    return;
                }

                // Serialize the external keyframe.
                // This is mainly to get a clean clone of the drawing list.
                string serialized = "";
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.Indent = false;
                writerSettings.CloseOutput = true;
                StringBuilder builder = new StringBuilder();
                using (XmlWriter w = XmlWriter.Create(builder, writerSettings))
                {
                    w.WriteStartElement("KeyframeMemento");
                    KeyframeSerializer.Serialize(w, keyframe, SerializationFilter.KVA);
                    w.WriteEndElement();
                    w.Flush();
                    serialized = builder.ToString();
                }

                // Deserialize it back.
                Keyframe copy = KeyframeSerializer.DeserializeMemento(serialized, m_FrameServer.Metadata);
                if (copy == null)
                {
                    log.ErrorFormat("Keyframe move: a problem occurred while deserializing the imported keyframe.");
                    return;
                }

                // Import the data manually.
                // Doing a global Metadata.MergeInsertKeyframe wouldn't work as the original keyframe has a different timestamp.
                newKf.Name = copy.Name;
                newKf.Color = copy.Color;
                newKf.Comments = copy.Comments;
                foreach (var d in copy.Drawings)
                    newKf.Drawings.Add(d);

                // Make sure the drawings are anchored to the right time for fading.
                newKf.Timestamp = m_iCurrentPosition;
            }
            else
            {
                // Change the keyframe reference time.
                keyframe.Timestamp = m_iCurrentPosition;
            }

            m_FrameServer.Metadata.Keyframes.Sort();
            OrganizeKeyframes();
            ActivateKeyframe(m_iCurrentPosition);
            UpdateFramesMarkers();
            DoInvalidate();
        }

        private void KeyframeControl_Selected(object sender, TimeEventArgs e)
        {
            // A keyframe was selected from a keyframe control (thumbnail or side panel),
            // or from a command jumping from keyframe to keyframe.
            // Move to the corresponding time.
            if (e.Time < m_iSelStart || e.Time > m_iSelEnd)
                return;

            OnPoke();
            StopPlaying();
            OnPauseAsked();

            long targetPosition = e.Time;

            trkSelection.SelPos = targetPosition;
            m_iFramesToDecode = 1;

            ShowNextFrame(targetPosition, true);
            m_iCurrentPosition = targetPosition;

            UpdatePositionUI();
            ActivateKeyframe(m_iCurrentPosition);
        }

        private void KeyframeControl_ShowCommentsAsked(object sender, EventArgs e)
        {
            // Make sure the properties panel is visible.
            if (!isSidePanelVisible)
                ToggleSidePanelVisibility();
        }

        /// <summary>
        /// A keyframe core data was updated from a keyframe control (side panel).
        /// This is raised when we change the name, color or comment from the side panel.
        /// Update whatever is impacted by this.
        /// </summary>
        private void KeyframeControl_KeyframeUpdated(object sender, EventArgs<Guid> e)
        {
            UpdateKeyframeBox(e.Value);
            UpdateFramesMarkers();
            m_FrameServer.Metadata.UpdateTrajectoriesForKeyframes();

            Invalidate();
        }

        private void KeyframeControl_KeyframeDeletionAsked(object sender, EventArgs<Guid> e)
        {
            DeleteKeyframe(e.Value);

            // Set as active screen is done after in case we don't have any keyframes left.
            OnPoke();
        }

        /// <summary>
        /// A drawing was modified from a drawing style configurator control (side panel).
        /// Update the preset and the drawing on the screen.
        /// </summary>
        private void DrawingControl_DrawingUpdated(object sender, DrawingEventArgs e)
        {
            // Sanity check (should be invalid program if fails).
            if (!(e.Drawing is IDecorable))
                return;

            // The sender is either the drawing side panel or the tracking side panel.
            // They both can change the drawing name so make sure they are updated.
            if (sender == sidePanelDrawing)
            {
                // Auto save the drawing style as the new preset (for this drawing tool).
                ToolManager.SetToolStyleFromDrawing(e.Drawing, ((IDecorable)e.Drawing).StyleElements);
                ToolManager.SavePresets();

                // Possibly update cursor color.
                UpdateCursor();

                if (e.Drawing is DrawingTrack)
                {
                    // Synchronize with the tracking panel.
                    sidePanelTracking.UpdateName();

                    // Update track color in main navbar.
                    UpdateFramesMarkers();
                }
            }
            else if (sender == sidePanelTracking)
            {
                if (e.DrawingAction == DrawingAction.StateChanged)
                {
                    // Synchronize with the style panel.
                    sidePanelDrawing.UpdateName();

                    // Update track length in main navbar.
                    UpdateFramesMarkers();
                }
                else if (e.DrawingAction == DrawingAction.Resized || e.DrawingAction == DrawingAction.TrackingParametersChanged)
                {
                    // Auto save the tracking parameters as the new preset.
                    if (e.Drawing is DrawingTrack)
                    {
                        DrawingTrack track = e.Drawing as DrawingTrack;
                        if (PreferencesManager.PlayerPreferences.TrackingParameters.ContentHash != track.TrackingParameters.ContentHash)
                        {
                            PreferencesManager.PlayerPreferences.TrackingParameters = track.TrackingParameters.Clone();
                        }
                    }
                }
                else 
                {
                    // Moving, Moved, etc. nothing else to do, just invalidate the viewport.
                }
            }

            // Update the image.
            DoInvalidate();
        }

        /// <summary>
        /// Update the keyframe box holding this keyframe after an external change.
        /// </summary>
        private void UpdateKeyframeBox(Guid id)
        {
            foreach (KeyframeBox box in keyframeBoxes)
            {
                if (box.Keyframe.Id == id)
                {
                    box.UpdateContent();
                    break;
                }
            }
        }
        #endregion

        #region Docking Undocking
        private void btnDockBottom_Click(object sender, EventArgs e)
        {
            m_bKeyframePanelCollapsedManual = !m_bKeyframePanelCollapsed;
            CollapseKeyframePanel(!m_bKeyframePanelCollapsed);
        }
        private void splitKeyframes_Panel2_DoubleClick(object sender, EventArgs e)
        {
            m_bKeyframePanelCollapsedManual = !m_bKeyframePanelCollapsed;
            CollapseKeyframePanel(!m_bKeyframePanelCollapsed);
        }
        private void CollapseKeyframePanel(bool collapse)
        {
            if (collapse)
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

            m_bKeyframePanelCollapsed = collapse;
        }
        private void PrepareKeyframesDock()
        {
            // If there's no keyframe, and we will be using a tool,
            // the keyframes dock should be raised.
            // This way we don't surprise the user when he click the screen and the image moves around.
            // (especially problematic when using the Pencil).

            // this is only done for the very first keyframe.
            if (m_FrameServer.Metadata.Count < 1)
            {
                CollapseKeyframePanel(false);
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
            if (m_FrameServer.Metadata.Magnifier.Initializing)
                DisableMagnifier();

            OnPoke();

            AbstractDrawingTool tool = ((ToolStripItem)sender).Tag as AbstractDrawingTool;
            m_ActiveTool = tool ?? m_PointerTool;
            UpdateCursor();

            // Ensure there's a key image at this position, unless the tool creates unattached drawings.
            if (m_ActiveTool == m_PointerTool && m_FrameServer.Metadata.Count < 1)
                CollapseKeyframePanel(true);
            else if (m_ActiveTool.Attached)
                PrepareKeyframesDock();

            DoInvalidate();
        }
        private void btnMagnifier_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            m_ActiveTool = m_PointerTool;

            switch (m_FrameServer.Metadata.Magnifier.Mode)
            {
                case MagnifierMode.Inactive:
                {
                    ResetZoom(false);
                    m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.Initializing;
                    SetCursor(cursorManager.GetManipulationCursorMagnifier());

                    if (TrackableDrawingAdded != null)
                        TrackableDrawingAdded(this, new TrackableDrawingEventArgs(m_FrameServer.Metadata.Magnifier as ITrackable));

                    break;
                }
                case MagnifierMode.Initializing:
                {
                    // Revert to no magnification.
                    ResetZoom(false);
                    m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.Inactive;
                    //btnMagnifier.Image = Drawings.magnifier;
                    SetCursor(m_PointerTool.GetCursor(0));
                    DoInvalidate();
                    break;
                }
                case MagnifierMode.Active:
                default:
                {
                    DisableMagnifier();
                    DoInvalidate();
                    break;
                }
            }
        }
        private void ToggleSidePanelVisibility()
        {
            OnPoke();

            if (!m_FrameServer.Loaded)
                return;

            isSidePanelVisible = !isSidePanelVisible;
            splitViewport_Properties.Panel2Collapsed = !isSidePanelVisible;
            WindowManager.ActiveWindow.SidePanelVisible = isSidePanelVisible;
        }
        /// <summary>
        /// Force show the side panel at the drawing properties tab.
        /// 0: keyframes, 1: drawings, 2: tracking.
        /// </summary>
        private void ShowSidePanel(int index)
        {
            if (!isSidePanelVisible)
                ToggleSidePanelVisibility();

            TabControl tabContainer = splitViewport_Properties.Panel2.Controls[0] as TabControl;
            tabContainer.SelectedIndex = index;
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
            if (m_ActiveTool == m_PointerTool)
            {
                SetCursor(m_PointerTool.GetCursor(0));
            }
            else
            {
                Cursor cursor = cursorManager.GetToolCursor(m_ActiveTool, m_FrameServer.ImageTransform.Scale);
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
        private void mnuTimeOrigin_Click(object sender, EventArgs e)
        {
            MarkTimeOrigin();
        }
        private void mnuDirectTrack_Click(object sender, EventArgs e)
        {
            // Track the point.
            // m_DescaledMouse would have been set during the MouseDown event.
            CheckCustomDecodingSize(true);

            DrawingTrack track = new DrawingTrack(m_DescaledMouse, m_iCurrentPosition, m_FrameServer.VideoReader.Info.AverageTimeStampsPerFrame);
            track.Status = TrackStatus.Edit;

            if (DrawingAdding != null)
                DrawingAdding(this, new DrawingEventArgs(track, m_FrameServer.Metadata.TrackManager.Id));
        }

        private void mnuBackground_Click(object sender, EventArgs e)
        {
            Color memo = m_FrameServer.Metadata.BackgroundColor;
            FormBackgroundColor ffc = new FormBackgroundColor(m_FrameServer.Metadata, this);
            ffc.StartPosition = FormStartPosition.CenterScreen;
            ffc.ShowDialog();
            if (ffc.DialogResult != DialogResult.OK)
            {
                m_FrameServer.ChangeBackgroundColor(memo);
            }

            ffc.Dispose();

            DoInvalidate();
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
            Metadata metadata = m_FrameServer.Metadata;
            Keyframe kf = metadata.HitKeyframe;
            IDecorable drawing = metadata.HitDrawing as IDecorable;
            if (drawing == null || drawing.StyleElements == null || drawing.StyleElements.Elements.Count == 0)
                return;

            var drawingId = metadata.HitDrawing.Id;
            var managerId = metadata.FindManagerId(metadata.HitDrawing);
            var memento = new HistoryMementoModifyDrawing(metadata, managerId, drawingId, metadata.HitDrawing.Name, SerializationFilter.Style);

            FormConfigureDrawing2 fcd = new FormConfigureDrawing2(drawing, DoInvalidate);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();

            if (fcd.DialogResult == DialogResult.OK)
            {
                memento.UpdateCommandName(drawing.Name);
                m_FrameServer.HistoryStack.PushNewCommand(memento);

                // Update the style preset for the parent tool of this drawing
                // so the next time we use this tool it will have the style we just set.
                ToolManager.SetToolStyleFromDrawing(metadata.HitDrawing, drawing.StyleElements);
                ToolManager.SavePresets();
                UpdateCursor();

                sidePanelDrawing.SetDrawing(metadata.HitDrawing, managerId, drawingId);
                sidePanelTracking.SetDrawing(metadata.HitDrawing, managerId, drawingId);
            }

            fcd.Dispose();
            DoInvalidate();
            UpdateFramesMarkers();
        }

        private void mnuVisibilityAlways_Click(object sender, EventArgs e)
        {
            if (mnuVisibilityAlways.Checked)
                return;

            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            Guid managerId = m_FrameServer.Metadata.FindManagerId(drawing);
            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, managerId, drawing.Id, drawing.Name, SerializationFilter.Fading);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            drawing.InfosFading.AlwaysVisible = true;
            drawing.InfosFading.UseDefault = false;

            mnuVisibilityAlways.Checked = true;
            mnuVisibilityDefault.Checked = false;
            mnuVisibilityCustom.Checked = false;
            DoInvalidate();
        }
        private void mnuVisibilityDefault_Click(object sender, EventArgs e)
        {
            if (mnuVisibilityDefault.Checked)
                return;

            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            Guid managerId = m_FrameServer.Metadata.FindManagerId(drawing);
            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, managerId, drawing.Id, drawing.Name, SerializationFilter.Fading);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            drawing.InfosFading.AlwaysVisible = false;
            drawing.InfosFading.UseDefault = true;

            mnuVisibilityAlways.Checked = false;
            mnuVisibilityDefault.Checked = true;
            mnuVisibilityCustom.Checked = false;
            DoInvalidate();
        }
        private void mnuVisibilityCustom_Click(object sender, EventArgs e)
        {
            if (mnuVisibilityCustom.Checked)
            {
                mnuVisibilityConfigure_Click(sender, e);
                return;
            }

            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;
            Guid managerId = m_FrameServer.Metadata.FindManagerId(drawing);
            HistoryMemento memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, managerId, drawing.Id, drawing.Name, SerializationFilter.Fading);
            m_FrameServer.HistoryStack.PushNewCommand(memento);

            drawing.InfosFading.AlwaysVisible = false;
            drawing.InfosFading.UseDefault = false;

            mnuVisibilityAlways.Checked = false;
            mnuVisibilityDefault.Checked = false;
            mnuVisibilityCustom.Checked = true;
            DoInvalidate();

            // Go to configuration immediately.
            mnuVisibilityConfigure_Click(sender, e);
        }
        private void mnuVisibilityConfigure_Click(object sender, EventArgs e)
        {
            AbstractDrawing drawing = m_FrameServer.Metadata.HitDrawing;

            FormConfigureVisibility f = new FormConfigureVisibility(drawing, pbSurfaceScreen);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
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

            Guid managerId = m_FrameServer.Metadata.FindManagerId(m_FrameServer.Metadata.HitDrawing);
            AbstractDrawingManager manager = m_FrameServer.Metadata.GetDrawingManager(managerId);
            string data = DrawingSerializer.SerializeMemento(m_FrameServer.Metadata, manager.GetDrawing(drawing.Id), SerializationFilter.KVA, false);

            DrawingClipboard.Put(data, drawing.GetCopyPoint(), drawing.Name);

            if (DrawingDeleting != null)
                DrawingDeleting(this, new DrawingEventArgs(drawing, managerId));

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

            Guid managerId = m_FrameServer.Metadata.FindManagerId(m_FrameServer.Metadata.HitDrawing);
            AbstractDrawingManager manager = m_FrameServer.Metadata.GetDrawingManager(managerId);
            string data = DrawingSerializer.SerializeMemento(m_FrameServer.Metadata, manager.GetDrawing(drawing.Id), SerializationFilter.KVA, false);

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
            if (data == null)
                return;

            AbstractDrawing drawing = DrawingSerializer.DeserializeMemento(data, m_FrameServer.Metadata);
            if (drawing == null || !drawing.IsCopyPasteable)
                return;

            Guid managerId = m_FrameServer.Metadata.FindManagerId(drawing);

            // Create a new keyframe if needed.
            if (m_FrameServer.Metadata.IsAttachedDrawing(drawing))
            {
                AddKeyframe();
                Keyframe kf = m_FrameServer.Metadata.HitKeyframe;
                managerId = kf.Id;
            }

            drawing.AfterCopy();

            if (!inPlace)
            {
                // Relocate the drawing under the mouse based on relative motion since the "copy" or "cut" action.
                float dx = m_DescaledMouse.X - DrawingClipboard.Position.X;
                float dy = m_DescaledMouse.Y - DrawingClipboard.Position.Y;
                drawing.MoveDrawing(dx, dy, Keys.None);
                log.DebugFormat("Pasted drawing [{0}] under the mouse.", DrawingClipboard.Name);
            }
            else
            {
                log.DebugFormat("Pasted drawing [{0}] in place.", DrawingClipboard.Name);
            }

            if (DrawingAdding != null)
                DrawingAdding(this, new DrawingEventArgs(drawing, managerId));
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
                Guid managerId = m_FrameServer.Metadata.FindManagerId(m_FrameServer.Metadata.HitDrawing);
                if (DrawingDeleting != null)
                    DrawingDeleting(this, new DrawingEventArgs(drawing, managerId));
            }
        }
        #endregion

        #region Trajectory tool menus
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

            // Note that we use SerializationFilter.KVA to backup all data as the dialog allows to modify not only style option but also tracker parameters.
            HistoryMementoModifyDrawing memento = new HistoryMementoModifyDrawing(m_FrameServer.Metadata, m_FrameServer.Metadata.TrackManager.Id, track.Id, track.Name, SerializationFilter.KVA);

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
            UpdateFramesMarkers();
        }
        private void DisplayClosestFrame(Point p, List<TimedPoint> trackPoints, float timeScale, bool use3D)
        {
            //--------------------------------------------------------------------------
            // This is where the interactivity of the trajectory is done.
            // The user has draged or clicked the trajectory, we find the closest point
            // and we update to the corresponding frame.
            //--------------------------------------------------------------------------

            // Compute the 3D distance (x,y,t) of each point in the path.

            float minDistance = float.MaxValue;
            int closestPointIndex = 0;

            if (use3D)
            {
                // Find closest location on screen in 3D (X, Y, T).
                for (int i = 0; i < trackPoints.Count; i++)
                {
                    float dx = p.X - trackPoints[i].X;
                    float dy = p.Y - trackPoints[i].Y;
                    float dt = m_iCurrentPosition - trackPoints[i].T;
                    dt /= timeScale;

                    float dist = (float)Math.Sqrt((dx * dx) + (dy * dy) + (dt * dt));
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPointIndex = i;
                    }
                }
            }
            else
            {
                // Find closest location on screen in 2D.
                for (int i = 0; i < trackPoints.Count; i++)
                {
                    float dx = p.X - trackPoints[i].X;
                    float dy = p.Y - trackPoints[i].Y;
                    float dist = (float)Math.Sqrt((dx * dx) + (dy * dy));

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPointIndex = i;
                    }
                }
            }

            // move to corresponding timestamp.
            m_iFramesToDecode = 1;
            ShowNextFrame(trackPoints[closestPointIndex].T, true);
            UpdatePositionUI();
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
            m_FrameServer.ImageTransform.Zoom = m_FrameServer.Metadata.Magnifier.Zoom;
            m_FrameServer.ImageTransform.UpdateZoomWindow(m_FrameServer.Metadata.Magnifier.Center, false);
            DisableMagnifier();
            ToastZoom();

            ResizeUpdate(true);
        }
        private void mnuMagnifierFreeze_Click(object sender, EventArgs e)
        {
            Magnifier m = m_FrameServer.Metadata.Magnifier;
            if (m.Frozen)
                m.Unfreeze();
            else
                m.Freeze(m_FrameServer.CurrentImage);

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
            m_FrameServer.Metadata.Magnifier.Mode = MagnifierMode.Inactive;
            SetCursor(m_PointerTool.GetCursor(0));
        }
        #endregion

        #endregion

        #region DirectZoom
        private void ResetZoom(bool toast)
        {
            m_FrameServer.ImageTransform.ResetZoom();
            zoomHelper.Value = 1.0f;

            m_PointerTool.SetZoomLocation(m_FrameServer.ImageTransform.ZoomWindow.Location);

            if(toast)
                ToastZoom();

            ReportForSyncMerge();
            ResizeUpdate(true);
        }
        private void IncreaseDirectZoom(Point mouseLocation)
        {
            if (m_FrameServer.Metadata.Magnifier.Mode != MagnifierMode.Inactive)
                DisableMagnifier();

            zoomHelper.Increase();
            m_FrameServer.ImageTransform.Zoom = zoomHelper.Value;
            AfterZoomChange(mouseLocation);
        }
        private void DecreaseDirectZoom(Point mouseLocation)
        {
            if (!m_FrameServer.ImageTransform.Zooming)
            {
                // If we are already at the lowest zoom level, recenter the window.
                ResetZoom(false);
                return;
            }

            zoomHelper.Decrease();
            m_FrameServer.ImageTransform.Zoom = zoomHelper.Value;
            AfterZoomChange(mouseLocation);
        }
        private void AfterZoomChange(Point mouseLocation)
        {
            // Mouse location is given in the system of the picture box control.
            m_FrameServer.ImageTransform.UpdateZoomWindow(mouseLocation, false);
            m_PointerTool.SetZoomLocation(m_FrameServer.ImageTransform.ZoomWindow.Location);
            ToastZoom();
            UpdateCursor();
            ReportForSyncMerge();
            ResizeUpdate(true);
        }
        #endregion

        #region Toasts
        private void ToastZoom()
        {
            string message = string.Format("Zoom:{0}", zoomHelper.GetLabel());
            m_MessageToaster.SetDuration(750);
            m_MessageToaster.Show(message);
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
                g.DrawImage(m_FrameServer.CurrentImage, rDst, m_FrameServer.ImageTransform.ZoomWindowInDecodedImage, GraphicsUnit.Pixel);
            else
                g.DrawImage(m_FrameServer.CurrentImage, rDst, m_FrameServer.ImageTransform.ZoomWindow, GraphicsUnit.Pixel);

            return copy;
        }
        #endregion

        #region VideoFilters Management
        private void EnableDisableAllPlayingControls(bool enable)
        {
            // Disable playback controls and some other controls for the case
            // of a one-frame rendering. (mosaic, single image)

            if(m_FrameServer.Loaded && !m_FrameServer.VideoReader.CanChangeWorkingZone)
                EnableDisableWorkingZoneControls(false);
            else
                EnableDisableWorkingZoneControls(enable);

            buttonGotoFirst.Enabled = enable;
            buttonGotoLast.Enabled = enable;
            buttonGotoNext.Enabled = enable;
            buttonGotoPrevious.Enabled = enable;
            buttonPlay.Enabled = enable;

            lblSpeedTuner.Enabled = enable;
            trkFrame.EnableDisable(enable);

            trkFrame.Enabled = enable;
            trkSelection.Enabled = enable;
            sldrSpeed.Enabled = enable;

            mnuTimeOrigin.Enabled = enable;
            mnuDirectTrack.Enabled = enable;
            mnuBackground.Enabled = enable;
        }
        private void EnableDisableWorkingZoneControls(bool enable)
        {
            btnSetHandlerLeft.Enabled = enable;
            btnSetHandlerRight.Enabled = enable;
            btnHandlersReset.Enabled = enable;
            btn_HandlersLock.Enabled = enable;
            btnTimeOrigin.Enabled = enable;
            trkSelection.EnableDisable(enable);
        }
        private void EnableDisableExportButtons(bool enable)
        {
            btnExportImage.Enabled = enable;
            btnExportImageSequence.Enabled = enable;
            btnExportVideo.Enabled = enable;
            btnExportVideoSlideshow.Enabled = enable;
            btnExportVideoWithPauses.Enabled = enable;
        }
        private void EnableDisableDrawingTools(bool enable)
        {
            foreach(ToolStripItem tsi in stripDrawingTools.Items)
            {
                tsi.Enabled = enable;
            }
        }
        #endregion

        #region Saving annotations
        private void mnuSaveAnnotations_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            SaveAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void mnuSaveAnnotationsAs_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            SaveAnnotationsAsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void mnuSaveDefaultPlayerAnnotations_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            SaveDefaultPlayerAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void mnuSaveDefaultCaptureAnnotations_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            SaveDefaultCaptureAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void mnuUnloadAnnotations_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            UnloadAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void mnuReloadDefaultPlayerAnnotations_Click(object sender, EventArgs e)
        {
            if (!m_FrameServer.Loaded)
                return;

            BeforeAnnotationsFileOp();
            ReloadDefaultPlayerAnnotationsAsked?.Invoke(this, EventArgs.Empty);
            AfterAnnotationsFileOp();
        }

        private void BeforeAnnotationsFileOp()
        {
            StopPlaying();
            OnPauseAsked();
        }

        private void AfterAnnotationsFileOp()
        {
            m_iFramesToDecode = 1;
            ShowNextFrame(m_iSelStart, true);
            ActivateKeyframe(m_iCurrentPosition, true);
        }
        #endregion

        #region Export

        /// <summary>
        /// Export the current frame with drawings to the clipboard.
        /// </summary>
        private void CopyImageToClipboard()
        {
            if (!m_FrameServer.Loaded || m_FrameServer.CurrentImage == null)
                return;

            BeforeExportVideo();
            Size size = m_FrameServer.VideoReader.Info.ReferenceSize;
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);
            PaintFlushedImage(bmp);
            Clipboard.SetImage(bmp);
            bmp.Dispose();
            AfterExportVideo();
        }

        /// <summary>
        /// Called before we start exporting video.
        /// Stop playing and disable custom decoding size.
        /// </summary>
        public void BeforeExportVideo()
        {
            StopPlaying();
            OnPauseAsked();

            // Force disable custom decoding size as we want to export at the original size.
            CheckCustomDecodingSize(true);
            memoPosition = m_iCurrentPosition;
            saveInProgress = true;
        }

        /// <summary>
        /// Called after we finish exporting video.
        /// </summary>
        public void AfterExportVideo()
        {
            saveInProgress = false;
            dualSaveInProgress = false;

            // Restore custom decoding size if possible.
            CheckCustomDecodingSize(false);

            m_iFramesToDecode = 1;
            ShowNextFrame(memoPosition, true);
            ActivateKeyframe(m_iCurrentPosition, true);
        }

        /// <summary>
        /// Paint the current frame with all drawings flushed, at the reference size, onto the passed Bitmap.
        /// </summary>
        public void PaintFlushedImage(Bitmap output)
        {
            Size inputSize = m_FrameServer.VideoReader.Info.ReferenceSize;
            if (inputSize != output.Size)
            {
                log.ErrorFormat("Exporting unscaled images: passed bitmap has the wrong size.");
                return;
            }

            int keyframeIndex = m_FrameServer.Metadata.GetKeyframeIndex(m_iCurrentPosition);
            
            using (Graphics canvas = Graphics.FromImage(output))
            {
                canvas.Clear(Color.Black);
                FlushOnGraphics(m_FrameServer.CurrentImage, canvas, output.Size, keyframeIndex, m_iCurrentPosition, m_FrameServer.ImageTransform.Identity);
            }
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
            {
                canvas.Clear(Color.Black);
                FlushOnGraphics(vf.Image, canvas, output.Size, keyframeIndex, vf.Timestamp, m_FrameServer.ImageTransform.Identity);
            }

            return keyframeIndex != -1;
        }


        #endregion
    }
}
