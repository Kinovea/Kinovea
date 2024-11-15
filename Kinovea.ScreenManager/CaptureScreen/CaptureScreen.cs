#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using Kinovea.Pipeline;
using Kinovea.Video.FFMpeg;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Main presenter class for the capture ui.
    /// Responsible for managing and synching a grabber, a circular buffer, a recorder and a viewport.
    /// </summary>
    public class CaptureScreen : AbstractScreen
    {
        #region Events
        public event EventHandler<EventArgs<string>> CameraDiscoveryComplete;
        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        #endregion

        #region Properties
        public override Guid Id
        {
            get { return id; }
            set { id = value;}
        }
        public override bool Full
        {
            get { return cameraLoaded; }
        }
        public override string FileName
        {
            get 
            {
                if(!cameraLoaded)
                    return ScreenManagerLang.statusEmptyScreen;
                else
                    return cameraSummary.Alias;
            }
        }
        public override string Status
        {
            get	{ return ""; /*frameServer.Status;*/}
        }
        public override UserControl UI
        {
            get 
            { 
                if(view is UserControl)
                    return view as UserControl;
                else
                    return null;
            }
        }
        public override string FilePath
        {
            get { return ""; }
        }
        public override bool CapabilityDrawings
        {
            get { return true;}
        }
        public override ImageAspectRatio AspectRatio
        {
            get { return cameraSummary == null ? ImageAspectRatio.Auto : Convert(cameraSummary.AspectRatio); }
            set { ChangeAspectRatio(value); }
        }
        public override ImageRotation ImageRotation
        {
            get { return cameraSummary == null ? ImageRotation.Rotate0 : cameraSummary.Rotation; }
            set { ChangeRotation(value); }
        }
        public override Demosaicing Demosaicing
        {
            get { return Demosaicing.None; }
            set { }
        }
        public override bool Mirrored
        {
            get { return metadata.Mirrored; }
            set { ChangeMirror(value); }
        }
        public override bool CoordinateSystemVisible
        {
            get { return metadata.DrawingCoordinateSystem.Visible; }
            set { metadata.DrawingCoordinateSystem.Visible = value; }
        }
        public override bool TestGridVisible
        {
            get { return metadata.TestGridVisible; }
            set { metadata.TestGridVisible = value; }
        }

        public override HistoryStack HistoryStack
        {
            get { return historyStack; }
        }
        public bool Shared
        {
            get { return shared; }
        }
        public bool Synched
        {
            get { return synched; }
            set { synched = value; }
        }
        public bool Recording
        {
            get { return recording; }
        }
        #endregion
        
        #region Members
        private Guid id = Guid.NewGuid();
        private ICaptureScreenView view;

        private bool cameraLoaded;
        private bool cameraConnected;
        private bool recording;

        private bool prepareFailed;
        private ImageDescriptor prepareFailedImageDescriptor;
        private ImageDescriptor imageDescriptor;
        
        private CameraSummary cameraSummary;
        private CameraManager cameraManager;
        private ICaptureSource cameraGrabber;
        private Stopwatch stopwatchDiscovery = new Stopwatch();
        private const long discoveryTimeout = 5000;
        private ScreenDescriptionCapture screenDescription;
        private PipelineManager pipelineManager = new PipelineManager();
        private ConsumerDisplay consumerDisplay = new ConsumerDisplay();
        private ConsumerRealtime consumerRealtime;
        private ConsumerDelayer consumerDelayer;
        private Thread recorderThread;
        private Bitmap recordingThumbnail;
        private DateTime recordingStart;
        private CaptureRecordingMode recordingMode;
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private Stopwatch stopwatchRecording = new Stopwatch();
        private bool triggerArmed = false;  // This indicates whether we are currently armed or not and is used to discard capture trigger commands.
        private bool manualArmed = false;   // This indicates whether the user manually armed/disarmed the audio/software trigger.
        private bool inQuietPeriod = false;

        private Delayer delayer = new Delayer();
        private int delay; // The current image age in number of frames.
        private bool delayedDisplay = true;

        private ViewportController viewportController;
        private CapturedFiles capturedFiles = new CapturedFiles();
        private string lastExportedMetadata;
        private MetadataWatcher metadataWatcher = new MetadataWatcher();

        private bool shared;
        private bool synched;
        private int index;
        
        private Metadata metadata;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private ScreenToolManager screenToolManager = new ScreenToolManager();
        private DrawingToolbarPresenter drawingToolbarPresenter = new DrawingToolbarPresenter();
        private Control dummy = new Control();

        private System.Windows.Forms.Timer displayTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer nonGrabbingInteractionTimer = new System.Windows.Forms.Timer();
        
        private HistoryStack historyStack = new HistoryStack();
        private string shortId;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public CaptureScreen()
        {
            // There are several nested lifetimes with symetric setup/teardown methods:
            // Screen -> ctor / BeforeClose.
            // Camera association -> LoadCamera / UnloadCamera.
            // Connection (frame grab) -> Connect / Disconnect.
            // Recording -> StartRecord / StopRecord.

            log.Debug("Constructing a CaptureScreen.");
            view = new CaptureScreenView(this);
            view.DualCommandReceived += OnDualCommandReceived;
            
            viewportController = new ViewportController();
            viewportController.DisplayRectangleUpdated += ViewportController_DisplayRectangleUpdated;
            viewportController.Poked += viewportController_Poked;

            view.SetViewport(viewportController.View);
            view.SetCapturedFilesView(capturedFiles.View);
            
            InitializeCaptureFilenames();
            InitializeTools();            
            InitializeMetadata();

            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
            
            view.UpdateArmedStatus(triggerArmed);
            UpdateRecordingIndicator();
            UpdateArmableTrigger();

            view.SetToolbarView(drawingToolbarPresenter.View);
            
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
            
            nonGrabbingInteractionTimer.Interval = 40;
            nonGrabbingInteractionTimer.Tick += NonGrabbingInteractionTimer_Tick;
            displayTimer.Tick += displayTimer_Tick;
            pipelineManager.FrameSignaled += pipelineManager_FrameSignaled;
            metadataWatcher.Changed += MetadataWatcher_Changed;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            shortId = this.id.ToString().Substring(0, 4);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            // The computer is about to be suspended, disconnect from the cameras.
            if (e.Mode == PowerModes.Suspend)
                ForceGrabbingStatus(false);
        }

        #region Public methods

        public void SetShared(bool shared)
        {
            log.DebugFormat("Set shared: {0}", shared);
            this.shared = shared;
            AllocateDelayer();
        }

        public void ForceGrabbingStatus(bool grab)
        {
            if (cameraGrabber == null || cameraGrabber.Grabbing == grab)
                return;

            ToggleConnection();
        }

        public void ForceRecordingStatus(bool record)
        {
            if (recording == record)
                return;

            ToggleRecording();
        }

        public void PerformSnapshot()
        {
            MakeSnapshot();
        }

        /// <summary>
        /// Start capture if armed.
        /// </summary>
        public void TriggerCapture()
        {
            if (!cameraConnected || !triggerArmed || recording)
                return;
            
            switch (PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerAction)
            {
                case CaptureTriggerAction.SaveSnapshot:
                    MakeSnapshot();
                    break;
                case CaptureTriggerAction.RecordVideo:
                default:
                    ToggleRecording();
                    break;
            }
        }

        /// <summary>
        /// Alert that there was a problem with the audio device.
        /// This prevents audio trigger to work.
        /// </summary>
        public void AudioDeviceLost()
        {
            if (cameraConnected)
                viewportController.ToastMessage(ScreenManagerLang.Toast_AudioLost, 5000);
        }

        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool active)
        {
            if (view == null)
                return;

            view.DisplayAsActiveScreen(active);
        }
        public override void RefreshUICulture() 
        {
            metadata.CalibrationHelper.AngleUnit = PreferencesManager.PlayerPreferences.AngleUnit;
            view.RefreshUICulture();
            drawingToolbarPresenter.RefreshUICulture();
        }
        public override void PreferencesUpdated()
        {
            InitializeCaptureFilenames();

            // For simplicity's sake we always reconnect the camera after the master preferences change.
            // This accounts for change in recording mode, display framerate, etc. that requires passing
            // through connect() for proper initialization of threads and timers.
            if (!cameraConnected)
                return;
            
            log.DebugFormat("Master preferences changed, reconnecting the camera.");
            Disconnect();
            Connect();

            UpdateArmableTrigger();
        }
        public override void BeforeClose()
        {
            if (stopwatchDiscovery.IsRunning)
            {
                stopwatchDiscovery.Stop();
                CameraTypeManager.CamerasDiscovered -= CameraTypeManager_CamerasDiscovered;
            }

            if (cameraLoaded)
                UnloadCamera();

            if (pipelineManager != null)
            {
                // Destroy resources (symmetric to constructor).
                pipelineManager.FrameSignaled -= pipelineManager_FrameSignaled;
                pipelineManager = null;
            }

            consumerDisplay = null;

            nonGrabbingInteractionTimer.Stop();
            nonGrabbingInteractionTimer.Tick -= NonGrabbingInteractionTimer_Tick;
            displayTimer.Stop();
            displayTimer.Tick -= displayTimer_Tick;

            viewportController.DisplayRectangleUpdated -= ViewportController_DisplayRectangleUpdated;

            if (view != null)
            {
                view.DualCommandReceived -= OnDualCommandReceived;
                view.BeforeClose();
                view = null;
            }

            metadata.Close();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

            log.DebugFormat("Capture screen ready to be closed.");
        }
        public override void AfterClose()
        {
            // All the stopping and cleaning is implemented in BeforeClose.
            // It works while there is no cancellation possible.
        }
        public override void RefreshImage()
        {
            // Not implemented.
        }
        public override void AddImageDrawing(string filename, bool svg)
        {
            // Adding drawing should go directly to the metadata.
            //view.AddImageDrawing(filename, svg);
        }
        public override void AddImageDrawing(Bitmap bmp)
        {
            //view.AddImageDrawing(bmp);
        }
        public override void FullScreen(bool fullScreen)
        {
            view.FullScreen(fullScreen);
        }

        public override void Identify(int index)
        {
            this.index = index;
            InitializeCaptureFilenames();
        }
        
        public override void ExecuteScreenCommand(int cmd)
        {
            view.ExecuteScreenCommand(cmd);
        }

        public override IScreenDescription GetScreenDescription()
        {
            ScreenDescriptionCapture sd = new ScreenDescriptionCapture();
            sd.Autostream = true;
            sd.CameraName = cameraSummary == null ? "" : cameraSummary.Alias;
            sd.Delay = (float)AgeToSeconds(delay);
            sd.DelayedDisplay = delayedDisplay;
            return sd;
        }

        public override void LoadKVA(string path)
        {
            if (!File.Exists(path))
                return;
            
            MetadataSerializer s = new MetadataSerializer();
            s.Load(metadata, path, true);
            AfterLoadKVA();
        }

        private void MetadataWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // This runs in the watcher thread.

            if (dummy.InvokeRequired)
                dummy.BeginInvoke((Action)delegate { ReloadKVA(e.FullPath); });
        }

        private void ReloadKVA(string path)
        {
            // Reload the KVA.
            // This is in the context of exporting a KVA from capture and modifying it from the player.
            // We detect the change and want to update the drawings here.
            // This is not a merge but a replacement.
            if (path != lastExportedMetadata)
                return;

            // Reset the currently loaded metadata.
            metadata.Keyframes.Clear();
            
            LoadKVA(path);
        }

        private void AfterLoadKVA()
        {
            if (metadata.Count == 0)
            {
                // Make sure we have at least one keyframe.
                // This can happen when we reload the existing KVA after changes from the player side 
                // and the user has deleted all keyframes.
                Keyframe kf = new Keyframe(0, "", Keyframe.DefaultColor, metadata);
                metadata.AddKeyframe(kf);
            }
        }
        #endregion

        #region Methods called from the view. These could also be events or commands.
        public void View_SetAsActive()
        {
            OnActivated(EventArgs.Empty);
        }
        public void View_Close()
        {
            OnCloseAsked(EventArgs.Empty);
        }
        public void View_Configure()
        {
            ConfigureCamera();
        }
        public void View_ConfigureComposite()
        {
        }
        public void View_ToggleGrabbing()
        {
            if (!cameraLoaded)
                return;

            ToggleConnection();
        }
        public void View_DelayChanged(double delayFrames)
        {
            DelayChanged(delayFrames);
        }
        public void View_SnapshotAsked()
        {
            MakeSnapshot();
        }
        public void View_ToggleRecording()
        {
            ToggleRecording();
        }
        
        public void View_ToggleDelayedDisplay()
        {
            delayedDisplay = !delayedDisplay;
        }

        public void View_ValidateFilename(string filename)
        {
            bool allowEmpty = true;
            if (!FilenameHelper.IsFilenameValid(filename, allowEmpty))
                ScreenManagerKernel.AlertInvalidFileName();
        }
        public void View_EditPathConfiguration(bool video)
        {
            if (video)
                NotificationCenter.RaisePreferenceTabAsked(this, PreferenceTab.Capture_VideoNaming);
            else
                NotificationCenter.RaisePreferenceTabAsked(this, PreferenceTab.Capture_ImageNaming);
        }
        public void View_DeselectTool()
        {
            metadataManipulator.DeselectTool();
        }
        public void View_ToggleArmingTrigger()
        {
            // Manual toggle.
            if (IsTriggerEnabled())
            {
                ToggleArmingTrigger(true, true);
            }
        }
        #endregion
        #endregion

        #region Camera and pipeline management
        
        /// <summary>
        /// Associate this screen with a camera.
        /// </summary>
        public void LoadCamera(CameraSummary _cameraSummary, ScreenDescriptionCapture screenDescription)
        {
            if (cameraLoaded)
                UnloadCamera();

            cameraSummary = _cameraSummary;

            if (cameraSummary.Manager != null)
            {
                cameraManager = cameraSummary.Manager;
                cameraGrabber = cameraManager.CreateCaptureSource(cameraSummary);
                AssociateCamera(true);
                return;
            }
            
            // No camera manager in the camera summary: special case for when we want to load a camera from launch settings.
            
            if (string.IsNullOrEmpty(cameraSummary.Alias))
            {
                // Loading an empty screen through launch settings. Our job is done here.
                return;
            }
                
            // Loading a camera through launch settings.
            // At this point we don't know if the camera has been discovered yet or not.
            log.DebugFormat("Restoring camera: {0}", cameraSummary.Alias);

            CameraSummary summary2 = CameraTypeManager.GetCameraSummary(cameraSummary.Alias);
            if (summary2 != null)
            {
                log.DebugFormat("Camera is already known.");

                if (CameraDiscoveryComplete != null)
                    CameraDiscoveryComplete(this, new EventArgs<string>(cameraSummary.Alias));

                // Finish loading the screen.
                cameraSummary = summary2;
                cameraManager = cameraSummary.Manager;
                cameraGrabber = cameraManager.CreateCaptureSource(cameraSummary);

                bool connect = screenDescription != null ? screenDescription.Autostream : true;
                AssociateCamera(connect);

                if (screenDescription != null && cameraLoaded && cameraConnected)
                {
                    view.ForceDelaySeconds(screenDescription.Delay);
                    delayedDisplay = screenDescription.DelayedDisplay;
                }
            }
            else
            {
                // We don't know about this camera yet. Go through normal discovery.
                this.screenDescription = screenDescription;
                stopwatchDiscovery.Start();
                CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
                CameraTypeManager.StartDiscoveringCameras();
            }
        }

        private void AssociateCamera(bool connect)
        {
            if (cameraGrabber == null)
                return;

            UpdateTitle();
            cameraLoaded = true;

            OnActivated(EventArgs.Empty);

            if (connect)
                Connect();
        }

        private void CameraTypeManager_CamerasDiscovered(object sender, CamerasDiscoveredEventArgs e)
        {
            if (!stopwatchDiscovery.IsRunning)
                return;

            // Go through all cameras and see if we find our match.
            bool discovered = false;
            foreach (CameraSummary summary in e.Summaries)
            {
                if (summary.Alias != cameraSummary.Alias)
                    continue;

                // We found our camera.
                log.DebugFormat("Camera discovery: found {0}", cameraSummary.Alias);
                CameraTypeManager.CancelThumbnails();

                discovered = true;
                stopwatchDiscovery.Stop();
                CameraTypeManager.CamerasDiscovered -= CameraTypeManager_CamerasDiscovered;
                if (CameraDiscoveryComplete != null)
                    CameraDiscoveryComplete(this, new EventArgs<string>(cameraSummary.Alias));

                // Finish loading the screen.
                cameraSummary = summary;
                cameraManager = cameraSummary.Manager;
                cameraGrabber = cameraManager.CreateCaptureSource(cameraSummary);

                bool connect = screenDescription != null ? screenDescription.Autostream : true;
                AssociateCamera(connect);

                if (screenDescription != null && cameraLoaded && cameraConnected)
                {
                    view.ForceDelaySeconds(screenDescription.Delay);
                    delayedDisplay = screenDescription.DelayedDisplay;
                }
                
                break;
            }

            if (!discovered && stopwatchDiscovery.ElapsedMilliseconds > discoveryTimeout)
            {
                // Stop trying to find our camera.
                // Turns this back into a regular empty capture screen.
                log.DebugFormat("Camera discovery: time out while trying to find {0}", cameraSummary.Alias);
                stopwatchDiscovery.Stop();
                CameraTypeManager.CamerasDiscovered -= CameraTypeManager_CamerasDiscovered;
                if (CameraDiscoveryComplete != null)
                    CameraDiscoveryComplete(this, new EventArgs<string>(cameraSummary.Alias));
            }
        }

        /// <summary>
        /// Drop the association of this screen with the camera.
        /// </summary>
        private void UnloadCamera()
        {
            if (!cameraLoaded)
                return;

            if (cameraConnected)
                Disconnect();

            screenDescription = null;
            cameraGrabber = null;

            delayer.FreeAll();
            UpdateDelayMaxAge();
            UpdateRecordingIndicator();
            UpdateTitle();
            cameraLoaded = false;
        }

        /// <summary>
        /// Configure the stream and start receiving frames.
        /// </summary>
        private void Connect()
        {
           if (!cameraLoaded || cameraGrabber == null)
                return;

            if (cameraConnected)
                Disconnect();

            // First we try to prepare the grabber by using the preferences and checking if it succeeded.
            // If the configuration cannot be known in advance by an API, we try to read a single frame and check its configuration.
            imageDescriptor = ImageDescriptor.Invalid;
            if (prepareFailed && prepareFailedImageDescriptor != ImageDescriptor.Invalid)
            {
                imageDescriptor = cameraGrabber.GetPrepareFailedImageDescriptor(prepareFailedImageDescriptor);
            }
            else
            {
                imageDescriptor = cameraGrabber.Prepare();

                if (imageDescriptor == null || imageDescriptor.Format == Kinovea.Services.ImageFormat.None || imageDescriptor.Width <= 0 || imageDescriptor.Height <= 0)
                {
                    cameraGrabber.Close();

                    imageDescriptor = ImageDescriptor.Invalid;
                    prepareFailed = true;
                    log.ErrorFormat("The camera does not support configuration and we could not preallocate buffers.");

                    // Attempt to retrieve an image and look up its format on the fly.
                    // This is asynchronous. We'll come back here after the image has been captured or a timeout expired.
                    cameraManager.CameraThumbnailProduced += cameraManager_CameraThumbnailProduced;
                    cameraManager.StartThumbnail(cameraSummary);
                }
            }

            if (imageDescriptor == ImageDescriptor.Invalid)
            {
                UpdateTitle();
                return;
            }

            // At this point we have a proper image descriptor, but it is possible that we are on a Thumbnailer thread.

            if (dummy.InvokeRequired)
                dummy.BeginInvoke((Action)delegate { Connect2(); });
            else
                Connect2();
        }

        private void Connect2()
        {
            // Second part of Connect function. 
            // The function is split because the first part might need to be run repeatedly and from non UI thread, 
            // while this part must run on the UI thread.
            AllocateDelayer();
            bool sideways = ImageRotation == ImageRotation.Rotate90 || ImageRotation == ImageRotation.Rotate270;
            Size referenceSize = sideways ? new Size(imageDescriptor.Height, imageDescriptor.Width) : new Size(imageDescriptor.Width, imageDescriptor.Height);
            SanityCheckDisplayRectangle(cameraSummary, referenceSize);

            metadata.ImageSize = referenceSize;
            metadata.ImageAspect = Convert(cameraSummary.AspectRatio);
            metadata.ImageRotation = cameraSummary.Rotation;
            metadata.Mirrored = cameraSummary.Mirror;
            metadata.PostSetupCapture();

            // Make sure the viewport will not use the bitmap allocated by the consumerDisplay as it is about to be disposed.
            viewportController.ForgetBitmap();
            viewportController.InitializeDisplayRectangle(cameraSummary.DisplayRectangle, referenceSize);

            // The behavior of how we pull frames from the pipeline, push them to the delayer, record them to disk and display them is dependent 
            // on the recording mode (even while not recording). The recoring mode does not change for the camera connection session. 
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;

            if (recordingMode == CaptureRecordingMode.Camera)
            {
                // Start consumer thread for recording mode "camera".
                // This is used to pull frames from the pipeline and push them directly to disk.
                // It will be dormant until recording is started but it has the same lifetime as the pipeline.
                consumerRealtime = new ConsumerRealtime(shortId);
                recorderThread = new Thread(consumerRealtime.Run) { IsBackground = true };
                recorderThread.Name = consumerRealtime.GetType().Name + "-" + shortId;
                recorderThread.Start();

                pipelineManager.Connect(imageDescriptor, cameraGrabber, consumerDisplay, consumerRealtime);
            }
            else if (recordingMode == CaptureRecordingMode.Delay || recordingMode == CaptureRecordingMode.Scheduled)
            {
                // Start consumer thread for recording mode "delay".
                // This is used to pull frames from the pipeline and push them in the delayer, 
                // and then pull frames from the delayer and write them to disk.
                consumerDelayer = new ConsumerDelayer(shortId);
                recorderThread = new Thread(consumerDelayer.Run) { IsBackground = true };
                recorderThread.Name = consumerDelayer.GetType().Name + "-" + shortId;
                recorderThread.Start();

                pipelineManager.Connect(imageDescriptor, cameraGrabber, consumerDisplay, consumerDelayer);

                // The delayer life is synched with the grabbing, which is connect/disconnect.
                // So we can activate the consumer right away.
                consumerDelayer.PrepareDelay(delayer);
                consumerDelayer.Activate();
            }

            nonGrabbingInteractionTimer.Enabled = false;

            // Keep ts per frame in sync.
            // This means that if we have imported keyframes, their time should be kept the same.
            if (cameraGrabber.Framerate != 0)
                metadata.AverageTimeStampsPerFrame = (long)(metadata.AverageTimeStampsPerSecond / cameraGrabber.Framerate);

            // Start the low frequency / low precision timer.
            // This timer is used for display and to feed the delay buffer when using recording mode "Camera".
            // No point displaying images faster than what the camera produces, or that the monitor can show, but floor at 1 fps.
            double displayFramerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            double monitorFramerate = GetMonitorFramerate();

            double slowFramerate = Math.Min(displayFramerate, monitorFramerate);
            if (cameraGrabber.Framerate != 0)
                slowFramerate = Math.Min(slowFramerate, cameraGrabber.Framerate);
            
            slowFramerate = Math.Max(slowFramerate, 1);

            displayTimer.Interval = (int)(1000.0 / slowFramerate);
            displayTimer.Enabled = true;
            cameraGrabber.GrabbingStatusChanged += Grabber_GrabbingStatusChanged;
            cameraGrabber.Start();

            UpdateTitle();
            cameraConnected = true;

            log.DebugFormat("--------------------------------------------------");
            log.DebugFormat("Connected to camera.");
            log.DebugFormat("Image: {0}, {1}x{2}px, top-down: {3}.", imageDescriptor.Format, imageDescriptor.Width, imageDescriptor.Height, imageDescriptor.TopDown);
            log.DebugFormat("Nominal camera framerate: {0:0.###} fps, Monitor framerate: {1:0.###} fps, Custom display framerate: {2:0.###} fps, Final display framerate: {3:0.###} fps.",
                cameraGrabber.Framerate, monitorFramerate, displayFramerate, slowFramerate);
            log.DebugFormat("Recording mode: {0}, Compositor mode: {1}.", recordingMode, PreferencesManager.CapturePreferences.DelayCompositeConfiguration.CompositeType);
            log.DebugFormat("--------------------------------------------------");
        }
        
        /// <summary>
        /// Ensure the display rectangle has a matching aspect ratio to the incoming images.
        /// </summary>
        private void SanityCheckDisplayRectangle(CameraSummary summary, Size referenceSize)
        {
            if (summary.DisplayRectangle.IsEmpty)
                return;

            // The display rectangle can change its size based on user zoom, 
            // but the image size can be modified from the outside in some scenarios.
            double dspAR = (double)summary.DisplayRectangle.Width / summary.DisplayRectangle.Height;
            double camAR = (double)referenceSize.Width / referenceSize.Height;
            int expectedWidth = (int)Math.Round(summary.DisplayRectangle.Height * camAR);

            double epsilon = 1;
            if (Math.Abs(summary.DisplayRectangle.Width - expectedWidth) > epsilon)
                summary.DisplayRectangle = Rectangle.Empty;
        }

        private void cameraManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            // This handler is only hit during connection workflow when the connection preparation failed due to insufficient configuration information.
            // We get a single snapshot back with its image descriptor.
            cameraManager.CameraThumbnailProduced -= cameraManager_CameraThumbnailProduced;
            prepareFailedImageDescriptor = e.ImageDescriptor;

            if (e.Cancelled || e.HadError || e.Thumbnail == null)
                log.ErrorFormat("Abandon trying to connect to camera {0}", e.Summary.Alias);
            else
                Connect();
        }

        /// <summary>
        /// Stop receiving frames from the camera.
        /// </summary>
        private void Disconnect()
        {
            if (!cameraLoaded || !cameraConnected)
                return;

            cameraConnected = false;

            if (recording)
                StopRecording(false);

            if (consumerRealtime != null)
                consumerRealtime.Stop();

            if (consumerDelayer != null)
                consumerDelayer.Stop();

            if (recorderThread != null && recorderThread.IsAlive)
                recorderThread.Join(500);

            if (recorderThread.IsAlive)
            {
                log.ErrorFormat("Time out while waiting for recorder thread to join.");
                recorderThread.Abort();
            }

            pipelineManager.Disconnect();

            if (cameraGrabber != null)
            {
                cameraGrabber.Stop();
                cameraGrabber.GrabbingStatusChanged -= Grabber_GrabbingStatusChanged;
                cameraGrabber.Close();

                displayTimer.Stop();
                nonGrabbingInteractionTimer.Start();
            }

            prepareFailedImageDescriptor = ImageDescriptor.Invalid;
            UpdateTitle();
            UpdateRecordingIndicator();
        }

        private void ConfigureCamera()
        {
            if (!cameraLoaded || cameraManager == null)
                return;

            // Make sure we are live during configuration so the changes are immediately apparent.
            bool memoDelayedDisplay = delayedDisplay;
            delayedDisplay = false;
            bool needsReconnect = cameraManager.Configure(cameraSummary, Disconnect, Connect);
            delayedDisplay = memoDelayedDisplay;

            if (needsReconnect)
            {
                Disconnect();
                Connect();
            }

            UpdateTitle();
        }

        private void pipelineManager_FrameSignaled(object sender, EventArgs e)
        {
            // Runs in producer thread.

            try
            {            
                dummy.BeginInvoke((Action)delegate { FrameSignaled(); });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Begin invoke failed.", ex.ToString());
                dummy = new Control();
            }
        }

        private void FrameSignaled()
        {
            if (!cameraConnected)
                return;

            UpdateStats();
        }

        #endregion

        #region Private methods
        private void OnDualCommandReceived(object sender, EventArgs<HotkeyCommand> e)
        {
            OnDualCommandReceived(e);
        }
        private void ToggleConnection()
        {
            if (cameraConnected)
            {
                Disconnect();
                viewportController.ToastMessage(ScreenManagerLang.Toast_Pause, 750);
            }
            else
            {
                Connect();
            }
        }

        /// <summary>
        /// Returns true if any software trigger method is enabled.
        /// </summary>
        /// <returns></returns>
        private bool IsTriggerEnabled()
        {
            return PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAudioTrigger ||
                   PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableUDPTrigger;
        }
        /// <summary>
        /// Update the arming button based on preferences.
        /// Does not raise toast message.
        /// This should be used at the screen creation or after preferences changes.
        /// </summary>
        private void UpdateArmableTrigger()
        {
            if (!IsTriggerEnabled())
            {
                // Already disarmed.
                if (!triggerArmed)
                    return;

                ToggleArmingTrigger(false, false);
            }
            else
            {
                // Already armed.
                if (triggerArmed)
                    return;

                // Not already armed but the user hasn't explicitely armed earlier.
                // When we get out of preferences we may have changed something else, 
                // so we can't use the EnableAudioTrigger value to override what the user may have manually set.
                if (!manualArmed)
                    return;

                // No explicit opposition to re-arming.
                ToggleArmingTrigger(false, false);
            }
        }

        /// <summary>
        /// Disarm trigger for the quiet period.
        /// </summary>
        private void StartQuietPeriod()
        {
            if (PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.TriggerQuietPeriod > 0)
            {
                // We will monitor the end of the quiet period in the slow tick event.
                QuietPeriodHelper.StartQuietPeriod();
                inQuietPeriod = true;
                if (triggerArmed)
                    ToggleArmingTrigger(false, false);
            }
        }

        /// <summary>
        /// Re-enable the audio trigger if the quiet period is over.
        /// </summary>
        private void CheckQuietPeriod()
        {
            float quietProgress = QuietPeriodHelper.QuietProgress();
            if (quietProgress < 1.0f)
            {
                if (!manualArmed)
                {
                    // The user has manually disarmed, this means that when exiting the quiet period we'll still be in disarmed mode.
                    // In this case it is more confusing than anything to show the quiet period reverse progress.
                    viewportController.UpdateRecordingIndicator(RecordingStatus.Quiet, 1.0f);
                }
                else
                {
                    float progress = Math.Max(0.0f, 1.0f - quietProgress);
                    viewportController.UpdateRecordingIndicator(RecordingStatus.Quiet, progress);
                }
                return;
            }
            
            // Exiting quiet period. Re-arm only if not manually disarmed and prefs authorize it.
            log.DebugFormat("Detected end of quiet period.");
            inQuietPeriod = false;
            UpdateRecordingIndicator();

            if (!IsTriggerEnabled())
                return;

            if (!manualArmed)
                return;
                
            if (!triggerArmed)
                ToggleArmingTrigger(true, false);
        }
        
        private void Grabber_GrabbingStatusChanged(object sender, EventArgs e)
        {
            if (dummy.InvokeRequired)
            {
                dummy.BeginInvoke((Action)delegate 
                { 
                    view.UpdateGrabbingStatus(cameraGrabber.Grabbing);
                    UpdateRecordingIndicator();
                });
            }
            else
            {
                view.UpdateGrabbingStatus(cameraGrabber.Grabbing);
                UpdateRecordingIndicator();
            }
        }
        
        private void UpdateTitle()
        {
            if (view == null)
                return;

            view.UpdateTitle(cameraManager.GetSummaryAsText(cameraSummary), cameraSummary.Icon);
        }
        
        private void UpdateStats()
        {
            if (pipelineManager.Frequency <= 0)
                return;

            // Compute load (processing time vs frame budget).
            long ellapsed = 0;
            if (recordingMode == CaptureRecordingMode.Camera)
            {
                // Here we don't report load if not recording as it's non-blocking.
                if (recording && consumerRealtime != null)
                    ellapsed = consumerRealtime.Ellapsed;
            }
            else if ((recordingMode == CaptureRecordingMode.Delay || recordingMode == CaptureRecordingMode.Scheduled) && consumerDelayer != null)
            {
                ellapsed = consumerDelayer.Ellapsed;
            }

            float load = (ellapsed / (1000.0f / (float)pipelineManager.Frequency)) * 100;
             
            string signal = string.Format(" {0:0.00} fps", pipelineManager.Frequency);
            string bandwidth = string.Format(" {0:0.00} MB/s", cameraGrabber.LiveDataRate);
            bandwidth = bandwidth.PadLeft(12);
            string strLoad = string.Format(" {0:0} %", load);
            strLoad = strLoad.PadLeft(6);
            string drops = string.Format(" {0}", pipelineManager.Drops);
            view.UpdateInfo(signal, bandwidth, strLoad, drops);
            view.UpdateLoadStatus(load);
        }

        private void ChangeAspectRatio(ImageAspectRatio aspectRatio)
        {
            metadata.ImageAspect = aspectRatio;

            CaptureAspectRatio ratio = Convert(aspectRatio);
            if(ratio == cameraSummary.AspectRatio)
                return;
            
            cameraSummary.UpdateAspectRatio(ratio);
            cameraSummary.UpdateDisplayRectangle(Rectangle.Empty);
            
            // update display rectangle.
            Disconnect();
            Connect();
        }

        private void ChangeRotation(ImageRotation rotation)
        {
            metadata.ImageRotation = rotation;

            if (rotation == cameraSummary.Rotation)
                return;

            cameraSummary.UpdateRotation(rotation);
            cameraSummary.UpdateDisplayRectangle(Rectangle.Empty);

            Disconnect();
            Connect();
        }

        private void ChangeMirror(bool mirror)
        {
            metadata.Mirrored = mirror;
            cameraSummary.UpdateMirror(mirror);
        }

        private CaptureAspectRatio Convert(ImageAspectRatio aspectRatio)
        {
            switch(aspectRatio)
            {
                case ImageAspectRatio.Auto: return CaptureAspectRatio.Auto;
                case ImageAspectRatio.Force43: return CaptureAspectRatio.Force43;
                case ImageAspectRatio.Force169: return CaptureAspectRatio.Force169;
                default: return CaptureAspectRatio.Auto;
            }
        }
        
        private ImageAspectRatio Convert(CaptureAspectRatio aspectRatio)
        {
            switch(aspectRatio)
            {
                case CaptureAspectRatio.Auto: return ImageAspectRatio.Auto;
                case CaptureAspectRatio.Force43: return ImageAspectRatio.Force43;
                case CaptureAspectRatio.Force169: return ImageAspectRatio.Force169;
                default: return ImageAspectRatio.Auto;
            }
        }
        
        private void NonGrabbingInteractionTimer_Tick(object sender, EventArgs e)
        { 
            viewportController.Refresh();
        }
        
        private void displayTimer_Tick(object sender, EventArgs e)
        {
            // Runs in the UI thread.
            SlowTick();
            viewportController.Refresh();
        }

        private void SlowTick()
        {
            //--------------------------------------------------
            // Low frequency loop.
            // Anything done here must be non-blocking to the pipeline/frame producer.
            // Any recording to disk must be done in a high frequency loop (= at camera fps).
            // In the case of recording mode "camera", we don't care too much about the delay buffer correctness, 
            // so we only fill it here, sparsely, with less frames than the full time resolution.
            // In the case of recording mode "delay/display" it's the opposite, since the recording will pull frames
            // from the delay buffer, it must be filled densely, with all the frames.
            // 
            // Note on delay: the user is setting the delay in frames.
            // Here, whether the delay buffer is sparse or dense, the correct frame to pull is the one specified by this delay in frames, 
            // we don't need to take into account the difference in display framerate vs camera framerate.
            //--------------------------------------------------

            if (inQuietPeriod)
                CheckQuietPeriod();

            if (!cameraConnected)
                return;
            
            if (recordingMode == CaptureRecordingMode.Camera)
            {
                consumerDisplay.ConsumeOne();
                Frame freshFrame = consumerDisplay.Frame;
                if (freshFrame == null)
                    return;

                delayer.Push(freshFrame);
            }

            // Get the displayed frame.
            int target = 0;
            Bitmap displayFrame = delayedDisplay ? delayer.GetWeak(delay, ImageRotation, Mirrored, out target) : delayer.GetWeak(0, ImageRotation, Mirrored, out target);
            
            if (displayFrame == null && target < 0)
                displayFrame = CreateWaitImage(-target);
            
            if (displayFrame != null)
            {
                viewportController.ForgetBitmap();
                viewportController.Bitmap = displayFrame;
            }
            
            if (recording && recordingThumbnail == null && displayFrame != null)
                recordingThumbnail = BitmapHelper.Copy(displayFrame);

            float maxRecordingSeconds = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds;
            if (recording && maxRecordingSeconds > 0)
            {
                // Test if recording duration threshold is passed.
                float recordingSeconds = stopwatchRecording.ElapsedMilliseconds / 1000.0f;
                float progress = Math.Max(0.0f, 1.0f - (recordingSeconds / maxRecordingSeconds));
                viewportController.UpdateRecordingIndicator(RecordingStatus.Recording, progress);

                if (recordingMode == CaptureRecordingMode.Scheduled)
                {
                    // Always stop recording before the oldest frame drops off the bandwagon.
                    if (recordingSeconds + AgeToSeconds(this.delay) >= maxRecordingSeconds)
                    {
                        log.DebugFormat("Scheduled mode: forced stop recording. The buffer contains enough frames to save the max duration.");
                        StopRecording(true);
                    }
                }
                else
                {
                    if (recordingSeconds >= maxRecordingSeconds)
                    {
                        log.DebugFormat("Non scheduled mode: forced stop recording. Duration threshold passed. {0:0.000}/{1:0.000}.", recordingSeconds, maxRecordingSeconds);
                        StopRecording(false);
                    }
                }
            }
        }

        /// <summary>
        /// Create a wait image to signal that the camera itself is ready but the delay is larger than the first frame available.
        /// </summary>
        private Bitmap CreateWaitImage(int frames)
        {
            bool sideways = ImageRotation == ImageRotation.Rotate90 || ImageRotation == ImageRotation.Rotate270;
            int width = sideways ? imageDescriptor.Height : imageDescriptor.Width;
            int height = sideways ? imageDescriptor.Width : imageDescriptor.Height;

            Bitmap displayFrame = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            string text = string.Format(@"- {0:0.000} s", AgeToSeconds(frames));
            int fontSize = (int)(((float)24 / 480) * imageDescriptor.Height);

            using (Graphics g = Graphics.FromImage(displayFrame))
            using (Font font = new Font("Arial", fontSize, FontStyle.Regular))
            {
                g.DrawString(text, font, Brushes.White, new Point(50, 50));
            }

            return displayFrame;
        }

        private void ViewportController_DisplayRectangleUpdated(object sender, EventArgs e)
        {
            if (!cameraLoaded || cameraSummary == null)
                return;

            cameraSummary.UpdateDisplayRectangle(viewportController.DisplayRectangle);
            CameraTypeManager.UpdatedCameraSummary(cameraSummary);
        }

        private void viewportController_Poked(object sender, EventArgs e)
        {
            View_SetAsActive();
        }

        private void InitializeMetadata()
        {
            metadata = new Metadata(historyStack, TimeStampsToTimecode);
            // TODO: hook to events raised by metadata.
            
            // Use microseconds as the general time scale.
            // This is used when importing keyframes from external KVA,
            // and when exporting the KVA next to the saved videos.
            metadata.AverageTimeStampsPerSecond = 1000000;
            metadata.AverageTimeStampsPerFrame = (long)(metadata.AverageTimeStampsPerSecond / 25.0);

            LoadCompanionKVA();
            
            metadataRenderer = new MetadataRenderer(metadata, false);
            metadataManipulator = new MetadataManipulator(metadata, screenToolManager);
            
            viewportController.MetadataRenderer = metadataRenderer;
            viewportController.MetadataManipulator = metadataManipulator;
        }
        private void LoadCompanionKVA()
        {
            // Note: anything after the first keyframe will be ignored.
            string startupFile = PreferencesManager.CapturePreferences.CaptureKVA;
            if (!string.IsNullOrEmpty(startupFile))
            {
                if (Path.IsPathRooted(startupFile))
                    LoadKVA(startupFile);
                else
                    LoadKVA(Path.Combine(Software.SettingsDirectory, startupFile));
            }
        }
        private void InitializeTools()
        {
            drawingToolbarPresenter.AddToolButton(screenToolManager.HandTool, DrawingTool_Click);
            drawingToolbarPresenter.AddSeparator();

            DrawingToolbarImporter importer = new DrawingToolbarImporter();
            importer.Import("capture.xml", drawingToolbarPresenter, DrawingTool_Click);
        }
        
        private void DrawingTool_Click(object sender, EventArgs e)
        {
            // Disable magnifier.
            // TODO: when we have a user control for the whole strip, it should directly
            // pass the tool as sender.
            AbstractDrawingTool tool = ((ToolStripItem)sender).Tag as AbstractDrawingTool;
            screenToolManager.SetActiveTool(tool);
            // Update cursor
            // refresh for cursor.
        }
        
        private void MagnifierTool_Click(object sender, EventArgs e)
        {
        
        }

        private double GetMonitorFramerate()
        {
            // Based on https://github.com/rickbrew/RefreshRateWpf/blob/master/RefreshRateWpfApp/MainWindow.xaml.cs
            double defaultFramerate = 60;

            IntPtr hmonitor = NativeMethods.MonitorFromWindow(viewportController.View.Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (hmonitor == IntPtr.Zero)
                return defaultFramerate;

            // Get more info about the monitor.
            NativeMethods.MONITORINFOEXW monitorInfo = new NativeMethods.MONITORINFOEXW();
            monitorInfo.cbSize = (uint)Marshal.SizeOf<NativeMethods.MONITORINFOEXW>();
            bool result = NativeMethods.GetMonitorInfoW(hmonitor, ref monitorInfo);
            if (!result)
                return defaultFramerate;

            // Get the current display settings for that monitor.
            NativeMethods.DEVMODEW devMode = new NativeMethods.DEVMODEW();
            devMode.dmSize = (ushort)Marshal.SizeOf<NativeMethods.DEVMODEW>();
            result = NativeMethods.EnumDisplaySettingsW(monitorInfo.szDevice, NativeMethods.ENUM_CURRENT_SETTINGS, out devMode);
            if (!result)
                return defaultFramerate;

            return (double)devMode.dmDisplayFrequency;
        }

        /// <summary>
        /// Returns a textual representation of a time or duration in the user-preferred format.
        /// In the capture context this is only used to export user time for some drawings.
        /// </summary>
        public string TimeStampsToTimecode(long timestamps, TimeType type, TimecodeFormat format, bool symbol)
        {
            TimecodeFormat tcf = format == TimecodeFormat.Unknown ? TimecodeFormat.Milliseconds : format;
            double averageTimestampsPerFrame = metadata.AverageTimeStampsPerFrame;
            int frames = 0;
            if (averageTimestampsPerFrame != 0)
                frames = (int)Math.Round(timestamps/ averageTimestampsPerFrame);

            if (type == TimeType.Duration)
                frames++;

            double milliseconds = frames * metadata.BaselineFrameInterval / metadata.HighSpeedFactor;
            double framerate = 1000.0 / metadata.BaselineFrameInterval * metadata.HighSpeedFactor;
            double durationTimestamps = 1.0;
            double totalFrames = durationTimestamps / averageTimestampsPerFrame;

            return TimeHelper.GetTimestring(framerate, frames, milliseconds, timestamps, durationTimestamps, totalFrames, tcf, symbol);
        }

        #region Recording/Snapshoting
        private void InitializeCaptureFilenames()
        {
            string defaultName = "Capture";
            
            string image;
            string video;

            if (index == 0)
            {
                image = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftImageFile;
                video = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftVideoFile;
            }
            else
            {
                image = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightImageFile;
                video = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightVideoFile;
            }

            string nextImage = string.IsNullOrEmpty(image) ? defaultName : Filenamer.ComputeNextFilename(image);
            view.UpdateNextImageFilename(nextImage);

            string nextVideo = string.IsNullOrEmpty(video) ? defaultName : Filenamer.ComputeNextFilename(video);
            view.UpdateNextVideoFilename(nextVideo);
        }
        
        private void MakeSnapshot()
        {
            if (!cameraLoaded)
                return;

            Bitmap bitmap = delayer.GetWeak(delay, ImageRotation, Mirrored, out _);
            if (bitmap == null)
                return;

            string root;
            string subdir;
            if (index == 0)
            {
                root = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftImageRoot;
                subdir = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftImageSubdir;
            }
            else
            {
                root = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightImageRoot;
                subdir = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightImageSubdir;
            }

            string filenameWithoutExtension = view.CurrentImageFilename;
            string extension = Filenamer.GetImageFileExtension();
            
            Dictionary<PatternContext, string> context = BuildCaptureContext();

            string path = Filenamer.GetFilePath(root, subdir, filenameWithoutExtension, extension, context);
            
            if (!DirectoryExistsCheck(path) || !FilePathSanityCheck(path) || !OverwriteCheck(path))
            {
                bitmap.Dispose();
                return;
            }

            ImageHelper.Save(path, bitmap);
            viewportController.ToastMessage(ScreenManagerLang.Toast_ImageSaved, 750);

            // After save routines.
            AddCapturedFile(path, bitmap, false);
            NotificationCenter.RaiseRefreshFileExplorer(this, false);

            if (index == 0)
                PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftImageFile = filenameWithoutExtension;
            else
                PreferencesManager.CapturePreferences.CapturePathConfiguration.RightImageFile = filenameWithoutExtension;

            PreferencesManager.Save();
 
            // Compute next name for user feedback.
            string next = Filenamer.ComputeNextFilename(filenameWithoutExtension);
            view.UpdateNextImageFilename(next);

            bitmap.Dispose();
        }
        
        private Dictionary<PatternContext, string> BuildCaptureContext()
        {
            // TODO: 
            // We need to know if we are left or right screen to grab the correct top level variables from prefs.

            Dictionary<PatternContext, string> context = new Dictionary<PatternContext, string>();

            DateTime now = DateTime.Now;

            context[PatternContext.Year] = string.Format("{0:yyyy}", now);
            context[PatternContext.Month] = string.Format("{0:MM}", now);
            context[PatternContext.Day] = string.Format("{0:dd}", now);
            context[PatternContext.Hour] = string.Format("{0:HH}", now);
            context[PatternContext.Minute] = string.Format("{0:mm}", now);
            context[PatternContext.Second] = string.Format("{0:ss}", now);
            context[PatternContext.Millisecond] = string.Format("{0:fff}", now);

            context[PatternContext.Date] = string.Format("{0:yyyyMMdd}", now);
            context[PatternContext.Time] = string.Format("{0:HHmmss}", now);
            context[PatternContext.DateTime] = string.Format("{0:yyyyMMdd-HHmmss}", now);

            context[PatternContext.CameraAlias] = cameraSummary.Alias;
            context[PatternContext.ConfiguredFramerate] = string.Format("{0:0.00}", cameraGrabber.Framerate); 
            context[PatternContext.ReceivedFramerate] = string.Format("{0:0.00}", pipelineManager.Frequency);

            context[PatternContext.Escape] = "";

            return context;
        }

        private bool DirectoryExistsCheck(string path)
        {
            if (cameraGrabber == null)
                return false;
            
            if (!FilesystemHelper.CreateDirectory(path))
            {
                ScreenManagerKernel.AlertDirectoryNotCreated();
                return false;
            }

            return true;
        }

        private bool FilePathSanityCheck(string path)
        {
            if(cameraGrabber == null)
                return false;

            if (!FilenameHelper.IsFilenameValid(path, false))
            {
                ScreenManagerKernel.AlertInvalidFileName();
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Check if the file can be written safely or overwrite is explicitly ignored anyway.
        /// If not, raises an confirmation dialog.
        /// Returns true if the file can be written.
        /// </summary>
        private bool OverwriteCheck(string path)
        {
            if (cameraGrabber == null)
                return false;

            if (PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.IgnoreOverwrite || !File.Exists(path))
                return true;
            
            string msgTitle = ScreenManagerLang.Error_Capture_FileExists_Title;
            string msgText = String.Format(ScreenManagerLang.Error_Capture_FileExists_Text, path).Replace("\\n", "\n");

            DialogResult result = MessageBox.Show(msgText, msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            return result == DialogResult.Yes;
        }

        private void ToggleArmingTrigger(bool toast, bool manual)
        {
            triggerArmed = !triggerArmed;
            view.UpdateArmedStatus(triggerArmed);
            UpdateRecordingIndicator();
            
            if (manual)
                manualArmed = triggerArmed;

            if (toast)
            {
                if (triggerArmed)
                    viewportController.ToastMessage(ScreenManagerLang.Toast_TriggerArmed, 1000);
                else
                    viewportController.ToastMessage(ScreenManagerLang.Toast_TriggerDisarmed, 1000);
            }
        }

        private void UpdateRecordingIndicator()
        {
            // Generic update based on the current state.
            // There are other calls to viewportController.UpdateRecordingIndicator when we need to pass different progress value.
            RecordingStatus status = RecordingStatus.Disarmed;
            
            if (!cameraLoaded)
                status = RecordingStatus.Disconnected;
            else if (cameraGrabber != null && !cameraGrabber.Grabbing)
                status = RecordingStatus.Paused;
            else if (recording)
                status = RecordingStatus.Recording;
            else if (inQuietPeriod)
                status = RecordingStatus.Quiet;
            else if (triggerArmed)
                status = RecordingStatus.Armed;
            else
                status = RecordingStatus.Disarmed;

            viewportController.UpdateRecordingIndicator(status, 1.0f);
        }
        
        private void ToggleRecording()
        {
            if (recording)
                StopRecording(false);
            else
                StartRecording();
        }
        
        private void StartRecording()
        {
            if (!cameraLoaded || recording)
                return;

            string root;
            string subdir;

            if (index == 0)
            {
                root = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftVideoRoot;
                subdir = PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftVideoSubdir;
            }
            else
            {
                root = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightVideoRoot;
                subdir = PreferencesManager.CapturePreferences.CapturePathConfiguration.RightVideoSubdir;
            }
            
            string filenameWithoutExtension = view.CurrentVideoFilename;
            bool uncompressed = PreferencesManager.CapturePreferences.SaveUncompressedVideo && imageDescriptor.Format != Kinovea.Services.ImageFormat.JPEG;
            string extension = Filenamer.GetVideoFileExtension(uncompressed);

            Dictionary<PatternContext, string> context = BuildCaptureContext();

            string path = Filenamer.GetFilePath(root, subdir, filenameWithoutExtension, extension, context);

            if (!DirectoryExistsCheck(path))
                return;
            
            if (!FilePathSanityCheck(path))
                return;

            if (!OverwriteCheck(path))
                return;

            // Stop any current recording.
            switch (recordingMode)
            {
                case CaptureRecordingMode.Camera:
                    if (consumerRealtime != null && consumerRealtime.Active)
                        consumerRealtime.Deactivate();
                    break;
                case CaptureRecordingMode.Delay:
                case CaptureRecordingMode.Scheduled:
                    if (consumerDelayer != null && consumerDelayer.Active && consumerDelayer.Recording)
                        consumerDelayer.StopRecord();
                    break;
            }

            if (recordingThumbnail != null)
            {
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }

            log.DebugFormat("--------------------------------------------------");
            log.DebugFormat("Starting recording. Recording mode: {0}, Compression: {1}. Image size: {2}x{3} px. Rotation: {4}",
                recordingMode, !PreferencesManager.CapturePreferences.SaveUncompressedVideo, imageDescriptor.Width, imageDescriptor.Height, ImageRotation);
            log.DebugFormat("Nominal framerate: {0:0.###} fps, Received framerate: {1:0.###} fps, Display framerate: {2:0.###} fps.", 
                cameraGrabber.Framerate, pipelineManager.Frequency, 1000.0f / displayTimer.Interval);
            
            SaveResult result;
            double framerate = cameraGrabber.Framerate;
            if (framerate == 0)
            {
                framerate = pipelineManager.Frequency;
                if (framerate == 0)
                    framerate = 25;
            }

            // We must save the KVA before the end of the recording for it to get picked up by replay observers.
            // Let's save it right now, before we start collecting frames, to avoid any further pressure on the machine during recording.
            metadataWatcher.Close();
            SaveKva(path);

            if (cameraConnected)
            {
                pipelineManager.SetRecordingPath(path);

                if (recordingMode != CaptureRecordingMode.Scheduled)
                {
                    double interval = 1000.0 / framerate;
                    result = pipelineManager.StartRecord(path, interval, delay, ImageRotation);
                    recording = result == SaveResult.Success;
                }
                else
                {
                    // In Scheduled mode all the work will be done later, at the end of the recording duration or on manual stop.
                    recording = true;
                }
            
                if(recording)
                {
                    recordingStart = DateTime.Now;
                    stopwatchRecording.Restart();
                
                    view.UpdateRecordingStatus(recording);
                    viewportController.StartingRecording();
                    viewportController.UpdateRecordingIndicator(RecordingStatus.Recording, 1.0f);

                    if (RecordingStarted != null)
                        RecordingStarted(this, EventArgs.Empty);
                }
                else
                {
                    //DisplayError(result);
                }
            }
            else
            {
                SaveBuffer(path, uncompressed, false, 0);
            }
        }

        private void StopRecording(bool forcedStop)
        {
            if (!cameraLoaded || !recording)
                return;

            log.DebugFormat("Stopping recording.");

            StartQuietPeriod();

            string finalFilename = pipelineManager.Path;

            if (recordingMode != CaptureRecordingMode.Scheduled)
            {
                if (recordingMode == CaptureRecordingMode.Camera)
                {
                    if (consumerRealtime == null || (consumerRealtime != null && !consumerRealtime.Active))
                        return;

                    pipelineManager.StopRecord();
                }
                else //(recordingMode == CaptureRecordingMode.Delay)
                {
                    if (consumerDelayer == null || (consumerDelayer != null && !consumerDelayer.Recording))
                        return;

                    pipelineManager.StopRecord();
                }

                recording = false;
                string dropMessage = string.Format("Dropped frames: {0}.", pipelineManager.Drops);
                if (pipelineManager.Drops > 0)
                    log.Warn(dropMessage);
                else
                    log.Debug(dropMessage);

                viewportController.StoppingRecording();
                AfterStopRecording(finalFilename);
            }
            else // recordingMode == CaptureRecordingMode.Scheduled
            {
                // Save buffer to disk.
                // Avoid reentry in StopRecording when we disconnect.
                recording = false;
                float recordingSeconds = stopwatchRecording.ElapsedMilliseconds / 1000.0f;
                Disconnect();
                bool uncompressed = PreferencesManager.CapturePreferences.SaveUncompressedVideo && imageDescriptor.Format != Kinovea.Services.ImageFormat.JPEG;
                SaveBuffer(finalFilename, uncompressed, forcedStop, recordingSeconds);
                Connect();

                string dropMessage = string.Format("Dropped frames: {0}.", pipelineManager.Drops);
                log.Debug(dropMessage);
                viewportController.ToastMessage(ScreenManagerLang.Toast_StopRecord, 750);
            }
        }

        private void AfterStopRecording(string finalFilename)
        { 
            if (recordingThumbnail != null)
            {
                AddCapturedFile(finalFilename, recordingThumbnail, true);
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }

            PreferencesManager.FileExplorerPreferences.AddRecentCapturedFile(finalFilename);
            NotificationCenter.RaiseRefreshFileExplorer(this, false);

            // Start watching changes in the exported KVA.
            // We do this before running the post-recording command in case it wants to modify the data.
            if (!string.IsNullOrEmpty(lastExportedMetadata))
                metadataWatcher.Start(lastExportedMetadata);

            // Execute post-recording command.
            string command = PreferencesManager.CapturePreferences.PostRecordCommand;
            if (!string.IsNullOrEmpty(command))
                ExecutePostCaptureCommand(command, finalFilename);
            
            // We need to use the original filename with patterns still in it.
            string filenamePattern = view.CurrentVideoFilename;

            if (index == 0)
                PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftVideoFile = filenamePattern;
            else
                PreferencesManager.CapturePreferences.CapturePathConfiguration.RightVideoFile = filenamePattern;

            PreferencesManager.Save();

            string next = Filenamer.ComputeNextFilename(filenamePattern);
            view.UpdateNextVideoFilename(next);

            view.UpdateRecordingStatus(recording);
            UpdateRecordingIndicator();

            if (RecordingStopped != null)
                RecordingStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Save a KVA file along the recorded video.
        /// </summary>
        private void SaveKva(string path)
        {
            // Updates to the KVA before saving.
            lastExportedMetadata = "";
            double fpsDiff = Math.Abs(1.0 - (pipelineManager.Frequency / cameraGrabber.Framerate));
            bool setCaptureFramerate = fpsDiff > 0.01 && fpsDiff < 0.5;

            metadata.CalibrationHelper.CaptureFramesPerSecond = setCaptureFramerate ? pipelineManager.Frequency : cameraGrabber.Framerate;
            double userInterval = 1000.0 / cameraGrabber.Framerate;
            metadata.BaselineFrameInterval = CalibrationHelper.ComputeFileFrameInterval(userInterval);
            bool setUserInterval = userInterval != metadata.BaselineFrameInterval;

            // Set the time origin to match the real time of the recording trigger.
            // This will also help synchronizing videos with different delays.
            // If the camera isn't currently streaming we are in "pause & browse" mode and delay isn't relevant.
            metadata.TimeOrigin = 0;
            if (cameraConnected && (recordingMode == CaptureRecordingMode.Delay || recordingMode == CaptureRecordingMode.Scheduled) && delay > 0)
                metadata.TimeOrigin = delay * metadata.AverageTimeStampsPerFrame;
            
            // Only save the kva if there is interesting information that can't be found from the video file alone.
            if (setCaptureFramerate || setUserInterval || metadata.TimeOrigin != 0 || metadata.Count > 0 || 
                metadata.ImageAspect != ImageAspectRatio.Auto || metadata.ImageRotation != ImageRotation.Rotate0 || metadata.Mirrored)
            {
                MetadataSerializer serializer = new MetadataSerializer();
                string kvaFilename = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".kva";
                serializer.SaveToFile(metadata, kvaFilename);

                lastExportedMetadata = kvaFilename;
            }
        }

        /// <summary>
        /// Save the delay buffer to a file.
        /// </summary>
        private void SaveBuffer(string path, bool uncompressed, bool forcedStop, float recordingSeconds)
        {
            //------------------------------------------------------------------
            // "Pause & Browse" scenario. The user has paused the camera stream and is browsing the recently buffered action.
            // If they hit record now, we record the content of the buffer to storage as fast as possible.
            // This is similar to the Scheduled recording mode but manually triggered.
            // The active delay is ignored.
            // The frequency of the buffer and thus the resulting file depends on the recording mode: 
            // Camera (low frequency), Delay/Scheduled (high frequency).
            //------------------------------------------------------------------

            // TODO:
            // Use a background worker, a progress bar, and allow cancellation.
            log.DebugFormat("Manual scheduled recording: saving delay buffer content.");

            MJPEGWriter writer = new MJPEGWriter();
            VideoInfo info = new VideoInfo();
            info.OriginalSize = new Size(imageDescriptor.Width, imageDescriptor.Height);

            double framerate = GetDelayBufferFramerate();
            if (framerate == 0)
                framerate = 25;

            double interval = 1000.0 / framerate;
            string formatString = FilenameHelper.GetFormatStringCapture(uncompressed);
            double fileInterval = CalibrationHelper.ComputeFileFrameInterval(interval);
            SaveResult openResult = writer.OpenSavingContext(path, info, formatString, imageDescriptor.Format, uncompressed, interval, fileInterval, ImageRotation);

            if (openResult != SaveResult.Success)
                return;

            // Loop through the delay buffer and save the frames to storage.
            int minAge = 0;
            int maxAge = delayer.SafeCapacity - 1;
            float maxSeconds = PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.RecordingSeconds;

            log.DebugFormat("Recording delay buffer. Delay:{0:0.000}s, Recording seconds:{1:0.000}s, Max duration allowed:{2:0.000}s, Buffer capacity:{3} frames.",
                    AgeToSeconds(delay), recordingSeconds, maxSeconds, delayer.SafeCapacity - 1);

            // Figure the section of the buffer to save.
            if (forcedStop || recordingSeconds > 0)
            {
                // Scheduled mode.
                
                if (forcedStop && maxSeconds > 0)
                {
                    //----------------------------------------------------------------------------------------------
                    // We have force stopped recording because the max duration set in preferences is already available as frames in the buffer,
                    // starting from the oldest interesting frame. (trigger + delay).
                    // recordingSeconds is the real duration of pseudo-recording. It may be near 0, in this case the video may not include the trigger event.
                    //
                    // Exemple 1: 
                    // Buffer = 10s, Delay set at 8s, max duration = 2s.
                    // Recording allowed for 0s.
                    // Result: Duration: 2s, Position: -8s -> -6s. Not containing the trigger event.
                    // 
                    // Exemple 2:
                    // Buffer = 10s, Delay set at 2s, max duration = 3s.
                    // Recording allowed for 1s.
                    // Result: Duration: 3s, Position: -2s -> +1s. Does contain the trigger event (time 0).
                    //----------------------------------------------------------------------------------------------
                    int recordingFrames = Math.Max(SecondsToAge(recordingSeconds) - 1, 0);
                    maxAge = delay + recordingFrames;
                 
                    // This case always implies the entire "max configured duration" is available.
                    minAge = maxAge - SecondsToAge(maxSeconds);
                }
                else
                {
                    //----------------------------------------------------------------------------------------------
                    // The user has stopped recording manually before the max configured duration was available as frames in the buffer.
                    // 
                    // Exemple 1:
                    // Buffer = 10s, Delay set at 5s.
                    // Recording for 2s.
                    // Result: Duration: 2s, Position: -5s -> -3s. Does not contain the trigger event.
                    // 
                    // Exemple 2:
                    // Buffer = 10s, Delay set at 2s.
                    // Recording for 3s.
                    // Result: Duration: 3s, Position: -2s -> +1s. Contains the trigger event.
                    //----------------------------------------------------------------------------------------------
                    int recordingFrames = Math.Max(SecondsToAge(recordingSeconds) - 1, 0);
                    maxAge = delay + recordingFrames;
                    
                    // Special case to handle the case where the user let the oldest interesting frame fall off.
                    maxAge = Math.Min(maxAge, delayer.SafeCapacity - 1);

                    minAge = maxAge - recordingFrames;
                }

                log.DebugFormat("Scheduled recording of buffer section: Delay:{0}, Min age:{1}, Max age:{2}.", this.delay, minAge, maxAge);

                // Final clamp.
                maxAge = Math.Min(maxAge, delayer.SafeCapacity - 1);
                minAge = Math.Max(minAge, 0);
            }
            else
            {
                // Pause-and-browse recording.
                // Delay is not considered.
                // Always take the section between the most recent frame until max allowed recording duration.
                if (maxSeconds > 0)
                    maxAge = Math.Min(maxAge, SecondsToAge(maxSeconds) - 1);

                log.DebugFormat("Recording delay buffer while the camera is paused. Min age:{0}, Max age:{1}.", minAge, maxAge);
            }

            // Actual saving of the frames.
            Frame delayedFrame = new Frame(imageDescriptor.BufferSize);
            for (int age = maxAge; age >= minAge; age--)
            {
                if (recordingThumbnail == null)
                {
                    Bitmap delayed = delayer.GetWeak(age, ImageRotation, Mirrored, out _);
                    if (delayed != null)
                        recordingThumbnail = BitmapHelper.Copy(delayed);
                }

                bool copied = delayer.GetStrong(age, delayedFrame);
                if (copied)
                    writer.SaveFrame(imageDescriptor.Format, delayedFrame.Buffer, delayedFrame.PayloadLength, imageDescriptor.TopDown);
            }

            writer.CloseSavingContext(true);
            writer.Dispose();

            AfterStopRecording(path);
        }
        private void ExecutePostCaptureCommand(string command, string path)
        {
            // Build replacement context.
            Dictionary<PatternContext, string> context = new Dictionary<PatternContext, string>();
            context.Add(PatternContext.CaptureDirectory, Path.GetDirectoryName(path));
            context.Add(PatternContext.CaptureFilename, Path.GetFileName(path));

            string fullCommand = Filenamer.GetCommandLine(command, context);

            Process process = new Process();
            try
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/C " + fullCommand;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                //process.WorkingDirectory = ""; // app data CaptureCommands.

                log.DebugFormat("Running post capture command:");
                log.DebugFormat(">cmd.exe /C {0}", fullCommand);

                process.Start();
            }
            catch (Exception e)
            {
                log.ErrorFormat("Could not execute post capture command. {0}", e.Message);
            }
        }

        private void AddCapturedFile(string filepath, Bitmap image, bool video)
        {
            if(!capturedFiles.HasThumbnails)
                view.ShowThumbnails();
            
            capturedFiles.AddFile(filepath, image, video, ImageRotation);
        }
        #endregion

        #region Delayer
        /// <summary>
        /// Allocates or reallocates the delay buffer.
        /// This must be done each time the image descriptor, or available memory or framerate changes.
        /// </summary>
        private bool AllocateDelayer()
        {
            if (!cameraLoaded || imageDescriptor == null || imageDescriptor == ImageDescriptor.Invalid)
                return false;

            long totalMemoryMB = (long)PreferencesManager.CapturePreferences.CaptureMemoryBuffer;
            long availableMemory = shared ? totalMemoryMB / 2 : totalMemoryMB;
            log.DebugFormat("Allocating or reallocating delay buffer for {0}. Shared: {1}, Available memory: {2}/{3}", cameraSummary.Alias, shared, availableMemory, totalMemoryMB);

            long megabyte = 1024 * 1024;
            availableMemory *= megabyte;

            // FIXME: get the size of ring buffer from outside.
            availableMemory -= (imageDescriptor.BufferSize * 8);

            if (!delayer.NeedsReallocation(imageDescriptor, availableMemory))
            {
                // Make sure the delay UI agrees with the framerate.
                UpdateDelayMaxAge();
                return true;
            }

            if ((recordingMode == CaptureRecordingMode.Delay || recordingMode == CaptureRecordingMode.Scheduled) && 
                consumerDelayer != null && consumerDelayer.Active)
            {
                // Wait for the consumer to deactivate so it doesn't try to push frames while we are destroying them.
                consumerDelayer.Deactivate();

                // It is more appropriate to wait here for a while (in the middle of a user interaction anyway),
                // rather than put locks everywhere and interfere with the normal use-case of pushing frames as fast as possible to the delay.
                int maxAttempts = 25;
                int attempts = 0;
                while (consumerDelayer.Active && attempts < maxAttempts)
                {
                    Thread.Sleep(50);
                    attempts++;
                }

                if (consumerDelayer.Active)
                {
                    log.ErrorFormat("Failure to deactivate consumer delayer before memory re-allocation.");
                    UpdateDelayMaxAge();
                    return false;
                }
            }

            delayer.AllocateBuffers(imageDescriptor, availableMemory);

            if ((recordingMode == CaptureRecordingMode.Delay || recordingMode == CaptureRecordingMode.Scheduled) && consumerDelayer != null)
                consumerDelayer.Activate();

            UpdateDelayMaxAge();

            return true;
        }

        private void DelayChanged(double age)
        {
            this.delay = (int)Math.Round(age);
            
            // Force a refresh if we are not connected to the camera to enable "pause and browse".
            if (cameraLoaded && !cameraConnected)
            {
                Bitmap delayed = delayer.GetWeak(delay, ImageRotation, Mirrored, out _);
                viewportController.Bitmap = delayed;
                viewportController.Refresh();
            }
        }
        
        private void UpdateDelayMaxAge()
        {
            int frames = Math.Max(delayer.SafeCapacity - 1, 0);
            double seconds = AgeToSeconds(frames);
            view.UpdateDelayMax(seconds, frames);
        }
        
        /// <summary>
        /// Returns the number of seconds corresponding to a number of frames of delay.
        /// This is used purely for UI labels, all internal code uses frames.
        /// This depends on how dense the delay buffer is.
        /// </summary>
        private double AgeToSeconds(int age)
        {
            double framerate = GetDelayBufferFramerate();
            if (framerate == 0 || age == 0)
                return 0;

            return age / framerate;
        }

        /// <summary>
        /// Returns the number of frames of age corresponding to a delay in seconds.
        /// This is used to convert the max recording time into a position in the delay buffer.
        /// </summary>
        private int SecondsToAge(float seconds)
        {
            double framerate = GetDelayBufferFramerate();
            if (framerate == 0 || seconds == 0)
                return 0;

            return (int)Math.Floor(seconds * framerate);
        }

        private double GetDelayBufferFramerate()
        {
            if (cameraGrabber == null || !cameraLoaded)
                return 0;

            if (recordingMode == CaptureRecordingMode.Camera)
            {
                // In recording mode "camera" we don't really care about the delayer, and we just feed it at display fps.
                return PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            }
            else
            {
                // In recording mode Delayed and Scheduled, we put all the produced frames into the delayer, so we must use camera fps.
                double framerate = cameraGrabber.Framerate;

                if (framerate == 0)
                    framerate = pipelineManager.Frequency;

                if (framerate == 0)
                    framerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;

                return framerate;
            }
        }
        #endregion

        #endregion
    }
}

