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

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Pipeline;
using Kinovea.Pipeline.Consumers;
using System.Threading;
using System.Diagnostics;
using Kinovea.Video.FFMpeg;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Main presenter class for the capture ui.
    /// Responsible for managing and synching a grabber, a circular buffer, a recorder and a viewport.
    /// </summary>
    public class CaptureScreen : AbstractScreen
    {
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
            get { return ImageRotation.Rotate0; }
            set { }
        }
        public override bool Mirrored
        {
            get
            {
                return metadata.Mirrored;
            }

            set
            {
                // Note: mirroring works at the end frame for perfs reasons.
                // This means that if the quadrants view is active, the most recent will be on the right.
                // The alternative is to redraw the mirrored image before pushing it to the delay buffer, 
                // but that seems to be too taxing as it currently runs on the UI thread.
                metadata.Mirrored = value;
                viewportController.SetMirrored(value);
                viewportController.Refresh();
            }
        }
        public bool TestGridVisible
        {
            get { return metadata.TestGridVisible; }
            set 
            { 
                metadata.TestGridVisible = value;
                //ToggleImageProcessing();
            }
        }
        public HistoryStack HistoryStack
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
        private PipelineManager pipelineManager = new PipelineManager();
        private ConsumerDisplay consumerDisplay = new ConsumerDisplay();
        private ConsumerMJPEGRecorder consumerRecord;
        private Thread recorderThread;
        private Bitmap recordingThumbnail;
        private DateTime recordingStart;
        private CaptureRecordingMode recordingMode;
        private VideoFileWriter videoFileWriter = new VideoFileWriter();

        private OIPRollingShutterCalibration imageProcessor = new OIPRollingShutterCalibration();

        private Delayer delayer = new Delayer();
        private DelayCompositer delayCompositer;
        private DelayCompositeConfiguration delayCompositeConfiguration;
        private DelayCompositeType delayCompositeType;
        private Dictionary<DelayCompositeType, IDelayComposite> delayComposites = new Dictionary<DelayCompositeType, IDelayComposite>();
        private int delay; // The current image age in number of frames.

        private ViewportController viewportController;
        private FilenameHelper filenameHelper = new FilenameHelper();
        private CapturedFiles capturedFiles = new CapturedFiles();
        
        private bool shared;
        private bool synched;
        private int index;
        
        private Metadata metadata;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private ScreenToolManager screenToolManager = new ScreenToolManager();
        private DrawingToolbarPresenter drawingToolbarPresenter = new DrawingToolbarPresenter();
        private Control dummy = new Control();

        private System.Windows.Forms.Timer grabTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer nonGrabbingInteractionTimer = new System.Windows.Forms.Timer();

        private HistoryStack historyStack = new HistoryStack();
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

            delayCompositer = new DelayCompositer(delayer);
            delayCompositeConfiguration = PreferencesManager.CapturePreferences.DelayCompositeConfiguration;
            delayCompositeType = delayCompositeConfiguration.CompositeType;
            delayComposites[delayCompositeType] = GetComposite(delayCompositeConfiguration);
            delayCompositer.SetComposite(delayComposites[delayCompositeType]);
            view.ConfigureDisplayControl(delayCompositeConfiguration.CompositeType);
            
            if (delayCompositeType == DelayCompositeType.SlowMotion)
                view.UpdateSlomoRefreshRate(((DelayCompositeSlowMotion)delayComposites[delayCompositeType]).RefreshRate);
            
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
            
            view.SetToolbarView(drawingToolbarPresenter.View);
            
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
            
            nonGrabbingInteractionTimer.Interval = 40;
            nonGrabbingInteractionTimer.Tick += NonGrabbingInteractionTimer_Tick;

            grabTimer.Tick += grabTimer_Tick;
            
            pipelineManager.FrameSignaled += pipelineManager_FrameSignaled;
        }

        #region Public methods
        
        public void SetShared(bool shared)
        {
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
            AllocateDelayer();
            InitializeCaptureFilenames();
            recordingMode = PreferencesManager.CapturePreferences.RecordingMode;
        }
        public override void BeforeClose()
        {
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
            grabTimer.Stop();
            grabTimer.Tick -= grabTimer_Tick;

            viewportController.DisplayRectangleUpdated -= ViewportController_DisplayRectangleUpdated;

            if (view != null)
            {
                view.DualCommandReceived -= OnDualCommandReceived;
                view.BeforeClose();
                view = null;
            }
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
            // execute local command.
        }

        public override void LoadKVA(string path)
        {
            if (!File.Exists(path))
                return;
            
            MetadataSerializer s = new MetadataSerializer();
            s.Load(metadata, path, true);
            
            if (metadata.Count > 1)
                metadata.Keyframes.RemoveRange(1, metadata.Keyframes.Count - 1);
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
            if (!cameraLoaded || !cameraConnected)
                return;
            
            ConfigureComposite();
        }
        public void View_ToggleGrabbing()
        {
            if (!cameraLoaded)
                return;

            ToggleConnection();
        }
        public void View_DelayChanged(double value)
        {
            DelayChanged(value);
        }
        public void View_RefreshRateChanged(float value)
        {
            RefreshRateChanged(value);
        }
        public void View_ForceDelaySynchronization()
        {
            ForceDelaySynchronization();
        }
        public void View_SnapshotAsked()
        {
            MakeSnapshot();
        }
        public void View_ToggleRecording()
        {
            ToggleRecording();
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
        #endregion
        #endregion

        #region Camera and pipeline management
        
        /// <summary>
        /// Associate this screen with a camera.
        /// </summary>
        /// <param name="_cameraSummary"></param>
        public void LoadCamera(CameraSummary _cameraSummary)
        {
            if (cameraLoaded)
                UnloadCamera();

            cameraSummary = _cameraSummary;
            cameraManager = cameraSummary.Manager;
            cameraGrabber = cameraManager.CreateCaptureSource(cameraSummary);

            if (cameraGrabber == null)
                return;

            UpdateTitle();
            cameraLoaded = true;
            
            OnActivated(EventArgs.Empty);

            // Automatically connect to the camera upon association.
            Connect();
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

            cameraGrabber = null;

            delayer.Free();
            delayCompositer.Free();
            UpdateDelayMaxAge();

            UpdateTitle();
            cameraLoaded = false;
        }

        /// <summary>
        /// Configure the stream and start receiving frames.
        /// </summary>
        private void Connect()
        {
            if (!cameraLoaded)
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

                if (imageDescriptor == null || imageDescriptor.Format == Video.ImageFormat.None || imageDescriptor.Width <= 0 || imageDescriptor.Height <= 0)
                {
                    cameraGrabber.Close();

                    imageDescriptor = ImageDescriptor.Invalid;
                    prepareFailed = true;
                    log.ErrorFormat("The camera does not support configuration and we could not preallocate buffers.");
                    
                    // Attempt to retrieve an image and look up its format on the fly.
                    // This is asynchronous. We'll come back here after the image has been captured or a timeout expired.
                    cameraManager.CameraThumbnailProduced += cameraManager_CameraThumbnailProduced;
                    cameraManager.GetSingleImage(cameraSummary);
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

            metadata.ImageSize = new Size(imageDescriptor.Width, imageDescriptor.Height);
            metadata.PostSetupCapture();

            AllocateDelayer();

            // Start recorder thread. 
            // It will be dormant until recording is started but it has the same lifetime as the pipeline.
            consumerRecord = new ConsumerMJPEGRecorder();
            recorderThread = new Thread(consumerRecord.Run) { IsBackground = true };
            recorderThread.Name = consumerRecord.GetType().Name;
            recorderThread.Start();

            // Make sure the viewport will not use the bitmap allocated by the consumerDisplay as it is about to be disposed.
            viewportController.ForgetBitmap();
            viewportController.InitializeDisplayRectangle(cameraSummary.DisplayRectangle, new Size(imageDescriptor.Width, imageDescriptor.Height));

            // Initialize pipeline.
            pipelineManager.Connect(imageDescriptor, (IFrameProducer)cameraGrabber, consumerDisplay, consumerRecord);

            nonGrabbingInteractionTimer.Enabled = false;

            double framerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            if (framerate == 0)
                framerate = 25;

            grabTimer.Interval = (int)(1000.0 / framerate);
            grabTimer.Enabled = true;

            cameraGrabber.GrabbingStatusChanged += Grabber_GrabbingStatusChanged;
            cameraGrabber.Start();

            UpdateTitle();
            cameraConnected = true;

            log.DebugFormat("Connected to camera.");
            log.DebugFormat("Image: {0}, {1}x{2}px, top-down:{3}, nominal framerate:{4}.",
                imageDescriptor.Format, imageDescriptor.Width, imageDescriptor.Height, imageDescriptor.TopDown, cameraGrabber.Framerate);

            log.DebugFormat("Display synchronization framerate: {0}.", PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate);
            log.DebugFormat("Delay compositor mode: {0}.", PreferencesManager.CapturePreferences.DelayCompositeConfiguration.CompositeType);
        }
        
        private void cameraManager_CameraThumbnailProduced(object sender, CameraThumbnailProducedEventArgs e)
        {
            // This handler is only hit during connection workflow when the connection preparation failed due to insufficient configuration information.
            // We get a single snapshot back with its image descriptor.
            cameraManager.CameraThumbnailProduced -= cameraManager_CameraThumbnailProduced;
            prepareFailedImageDescriptor = e.ImageDescriptor;
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
                StopRecording();

            consumerRecord.Stop();
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

                grabTimer.Stop();
                nonGrabbingInteractionTimer.Start();
            }

            if (imageProcessor.Active)
                imageProcessor.Stop();

            prepareFailedImageDescriptor = ImageDescriptor.Invalid;
            UpdateTitle();
        }

        private void ConfigureCamera()
        {
            if (!cameraLoaded || cameraManager == null)
                return;

            bool needsReconnect = cameraManager.Configure(cameraSummary);

            if (needsReconnect)
            {
                Disconnect();
                Connect();
            }
        }

        private void ConfigureComposite()
        {
            if (!cameraLoaded || cameraManager == null)
                return;

            FormConfigureComposite form = new FormConfigureComposite(delayCompositeConfiguration);
            form.StartPosition = FormStartPosition.CenterScreen;

            if (form.ShowDialog() == DialogResult.OK)
            {
                delayCompositeConfiguration = form.Configuration;

                delayCompositeType = delayCompositeConfiguration.CompositeType;
                if (!delayComposites.ContainsKey(delayCompositeType))
                    delayComposites[delayCompositeType] = GetComposite(delayCompositeConfiguration);

                if (delayCompositeType == DelayCompositeType.SlowMotion)
                    view.UpdateSlomoRefreshRate(((DelayCompositeSlowMotion)delayComposites[delayCompositeType]).RefreshRate);

                delayCompositer.ResetComposite(delayComposites[delayCompositeType]);
                PreferencesManager.CapturePreferences.DelayCompositeConfiguration = delayCompositeConfiguration;
                PreferencesManager.Save();

                view.ConfigureDisplayControl(delayCompositeConfiguration.CompositeType);
            }

            form.Dispose();
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
                view.Toast(ScreenManagerLang.Toast_Pause, 750);
            }
            else
            {
                Connect();
            }
        }
        
        private void Grabber_GrabbingStatusChanged(object sender, EventArgs e)
        {
            if (dummy.InvokeRequired)
                dummy.BeginInvoke((Action)delegate { view.UpdateGrabbingStatus(cameraGrabber.Grabbing); });
            else
                view.UpdateGrabbingStatus(cameraGrabber.Grabbing);
        }
        
        private void UpdateTitle()
        {
            if (view == null)
                return;

            view.UpdateTitle(cameraManager.GetSummaryAsText(cameraSummary), cameraSummary.Icon);
        }
        
        private void UpdateStats()
        {
            if (pipelineManager.Frequency == 0)
                return;

            StringBuilder sb = new StringBuilder();

            if (prepareFailed && imageDescriptor != null)
                sb.AppendFormat("Signal: {0}×{1} @ {2:0.00} fps.", imageDescriptor.Width, imageDescriptor.Height, pipelineManager.Frequency);
            else
                sb.AppendFormat("Signal: {0:0.00} fps.", pipelineManager.Frequency);

            sb.AppendFormat(" Bandwidth: {0:0.00} MB/s.", cameraGrabber.LiveDataRate);
            
            sb.AppendFormat(" Drops: {0}.", pipelineManager.Drops);
            
            view.UpdateInfo(sb.ToString());
            

            if (delayCompositeType == DelayCompositeType.SlowMotion)
            {
                DelayCompositeSlowMotion dcsm = delayComposites[DelayCompositeType.SlowMotion] as DelayCompositeSlowMotion;
                if (dcsm != null)
                    view.UpdateSlomoCountdown(AgeToSeconds(dcsm.GetCountdown()));
            }
        }

        private void ChangeAspectRatio(ImageAspectRatio aspectRatio)
        {
            CaptureAspectRatio ratio = Convert(aspectRatio);
            if(ratio == cameraSummary.AspectRatio)
                return;
            
            cameraSummary.UpdateAspectRatio(ratio);
            cameraSummary.UpdateDisplayRectangle(Rectangle.Empty);
            
            // update display rectangle.
            Disconnect();
            Connect();
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
        
        private void grabTimer_Tick(object sender, EventArgs e)
        {
            GrabFrame();
            viewportController.Refresh();
        }

        private void GrabFrame()
        {
            // 1. Consume a frame for display purposes (= push to delay buffer).
            // 2. Get a frame to display (= pull from delay buffer). 
            // 
            // This can be synchronized with the camera frame signals or it can be on an independant timer.
            // The consumer display never blocks and always get the latest frame from the ring buffer.
            // This means that it may miss some frames, especially if it is on a lower frequency timer. 
            // It is considered acceptable for display purposes, the recorder consumer works differently.

            if (!cameraConnected)
                return;
            
            consumerDisplay.ConsumeOne();
            Bitmap fresh = consumerDisplay.Bitmap;

            if (fresh == null)
                return;

            delayer.Push(fresh);
            
            if (imageProcessor.Active)
                imageProcessor.Update(fresh);

            if (recording && recordingThumbnail == null)
                recordingThumbnail = BitmapHelper.Copy(fresh);

            // Get the image to display.
            Bitmap delayed = delayCompositer.Get(delay);
            viewportController.Bitmap = delayed;

            if (recording && recordingMode == CaptureRecordingMode.Display)
                videoFileWriter.SaveFrame(delayed);
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
            metadata = new Metadata(historyStack, null);
            // TODO: hook to events raised by metadata.
            
            LoadCompanionKVA();
            
            if(metadata.Count == 0)
            {
                Keyframe kf = new Keyframe(0, "capture", metadata);
                metadata.AddKeyframe(kf);
            }
            
            metadataRenderer = new MetadataRenderer(metadata);
            metadataManipulator = new MetadataManipulator(metadata, screenToolManager);
            
            viewportController.MetadataRenderer = metadataRenderer;
            viewportController.MetadataManipulator = metadataManipulator;
        }
        private void LoadCompanionKVA()
        {
            string startupFile = Path.Combine(Software.SettingsDirectory, "capture.kva");
            LoadKVA(startupFile);
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

        private void ToggleImageProcessing()
        {
            /*Bitmap delayed = delayer.Get(delay);

            // Test
            if (metadata.TestGridVisible)
                imageProcessor.Start(delayed.Width, delayed.Height, delayed.PixelFormat);
            else
                imageProcessor.Stop();*/
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
            if (!cameraLoaded || consumerDisplay.Bitmap == null)
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
            
            Dictionary<FilePatternContexts, string> context = BuildCaptureContext();

            string path = Filenamer.GetFilePath(root, subdir, filenameWithoutExtension, extension, context);
            
            FilesystemHelper.CreateDirectory(path);

            if(!FilePathSanityCheck(path))
                return;
                
            if(!OverwriteCheck(path))
                return;

            //Actual save.
            Bitmap outputImage = BitmapHelper.Copy(consumerDisplay.Bitmap);
            if(outputImage == null)
                return;
            
            ImageHelper.Save(path, outputImage);
            view.Toast(ScreenManagerLang.Toast_ImageSaved, 750);

            // After save routines.
            NotificationCenter.RaiseRefreshFileExplorer(this, false);

            AddCapturedFile(path, outputImage, false);
            CaptureHistoryEntry entry = CreateHistoryEntrySnapshot(path);
            CaptureHistory.AddEntry(entry);

            if (index == 0)
                PreferencesManager.CapturePreferences.CapturePathConfiguration.LeftImageFile = filenameWithoutExtension;
            else
                PreferencesManager.CapturePreferences.CapturePathConfiguration.RightImageFile = filenameWithoutExtension;

            PreferencesManager.Save();
 
            // Compute next name for user feedback.
            string next = Filenamer.ComputeNextFilename(filenameWithoutExtension);
            view.UpdateNextImageFilename(next);
        }
        
        private Dictionary<FilePatternContexts, string> BuildCaptureContext()
        {
            // TODO: 
            // We need to know if we are left or right screen to grab the correct top level variables from prefs.

            Dictionary<FilePatternContexts, string> context = new Dictionary<FilePatternContexts, string>();

            DateTime now = DateTime.Now;

            context[FilePatternContexts.Year] = string.Format("{0:yyyy}", now);
            context[FilePatternContexts.Month] = string.Format("{0:MM}", now);
            context[FilePatternContexts.Day] = string.Format("{0:dd}", now);
            context[FilePatternContexts.Hour] = string.Format("{0:HH}", now);
            context[FilePatternContexts.Minute] = string.Format("{0:mm}", now);
            context[FilePatternContexts.Second] = string.Format("{0:ss}", now);

            context[FilePatternContexts.CameraAlias] = cameraSummary.Alias;
            context[FilePatternContexts.ConfiguredFramerate] = string.Format("{0:0.00}", cameraGrabber.Framerate); 
            context[FilePatternContexts.ReceivedFramerate] = string.Format("{0:0.00}", pipelineManager.Frequency);

            return context;
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

        private bool OverwriteCheck(string path)
        {
            if (!File.Exists(path))
                return true;

            string msgTitle = ScreenManagerLang.Error_Capture_FileExists_Title;
            string msgText = String.Format(ScreenManagerLang.Error_Capture_FileExists_Text, path).Replace("\\n", "\n");

            DialogResult result = MessageBox.Show(msgText, msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            return result == DialogResult.Yes;
        }
        
        private void ToggleRecording()
        {
            if (recording)
                StopRecording();
            else
                StartRecording();
        }
        
        private void StartRecording()
        {
            if (!cameraLoaded || !cameraConnected || recording)
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
            string extension = Filenamer.GetVideoFileExtension();

            Dictionary<FilePatternContexts, string> context = BuildCaptureContext();

            string path = Filenamer.GetFilePath(root, subdir, filenameWithoutExtension, extension, context);

            FilesystemHelper.CreateDirectory(path);

            if (!FilePathSanityCheck(path))
                return;

            if (!OverwriteCheck(path))
                return;

            if (consumerRecord.Active)
                consumerRecord.Deactivate();
            
            if (recordingThumbnail != null)
            {
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }

            log.DebugFormat("Starting recording.");
            log.DebugFormat("Recording mode: {0}", PreferencesManager.CapturePreferences.RecordingMode);
            log.DebugFormat("Nominal framerate: {0}, Received framerate: {1}, Display framerate: {2}.", 
                cameraGrabber.Framerate, pipelineManager.Frequency, PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate);
            
            SaveResult result;
            if (recordingMode == CaptureRecordingMode.Camera)
            {
                double framerate = cameraGrabber.Framerate;
                if (framerate == 0)
                    framerate = 25;
                double interval = 1000.0 / cameraGrabber.Framerate;
                result = pipelineManager.StartRecord(path, interval);
            }
            else
            {
                // In RecordingMode.Display we use a simple VideoFileWriter that will push the displayed bitmap to a file.
                VideoInfo info = new VideoInfo();
                info.OriginalSize = new Size(imageDescriptor.Width, imageDescriptor.Height);
                string formatString = FilenameHelper.GetFormatStringCapture();

                // We have 3 possible framerates: the configured camera framerate, the measured camera framerate and the display framerate.
                // Since we now force the usage of a separate timer for frame grabbing and pushing to the delay buffer, 
                // the frames in the delay buffer are roughly at the forced framerate.
                //double measuredInterval = 1000 / pipelineManager.Frequency;
                double framerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
                if (framerate == 0)
                    framerate = 25;
                //framerate = Math.Min(framerate, pipelineManager.Frequency);
                double interval = 1000.0 / framerate;
                result = videoFileWriter.OpenSavingContext(path, info, formatString, interval);
            }

            recording = result == SaveResult.Success;
            
            if(recording)
            {
                recordingStart = DateTime.Now;
                                
                view.UpdateRecordingStatus(recording);
                view.Toast(ScreenManagerLang.Toast_StartRecord, 1000);
            }
            else
            {
                //DisplayError(result);
            }
        }

        private void StopRecording()
        {
            if(!cameraLoaded || !recording)
               return;

            log.DebugFormat("Stopping recording.");

            if (recordingMode == CaptureRecordingMode.Camera && !consumerRecord.Active)
                return;

            string finalFilename = null;
            if (recordingMode == CaptureRecordingMode.Camera)
            {
                consumerRecord.Deactivate();
                finalFilename = consumerRecord.Filename;
            }
            else
            {
                videoFileWriter.CloseSavingContext(true);
                finalFilename = videoFileWriter.Filename;
            }
            
            recording = false;

            view.Toast(ScreenManagerLang.Toast_StopRecord, 750);
            NotificationCenter.RaiseRefreshFileExplorer(this, false);
             
            if(recordingThumbnail != null)
            {
                AddCapturedFile(finalFilename, recordingThumbnail, true);
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }

            CaptureHistoryEntry entry = CreateHistoryEntry(finalFilename);
            CaptureHistory.AddEntry(entry);

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
        }

        private CaptureHistoryEntry CreateHistoryEntry(string filename)
        {
            string captureFile = filename;
            DateTime start = recordingStart;
            DateTime end = DateTime.Now;
            string cameraAlias = cameraSummary.Alias;
            string cameraIdentifier = cameraSummary.Identifier;
            double configuredFramerate = cameraGrabber.Framerate;
            double receivedFramerate = pipelineManager.Frequency;
            int drops = (int)pipelineManager.Drops;

            CaptureHistoryEntry entry = new CaptureHistoryEntry(captureFile, start, end, cameraAlias, cameraIdentifier, configuredFramerate, receivedFramerate, drops);
            return entry;
        }

        private CaptureHistoryEntry CreateHistoryEntrySnapshot(string filename)
        {
            string captureFile = filename;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            string cameraAlias = cameraSummary.Alias;
            string cameraIdentifier = cameraSummary.Identifier;
            double configuredFramerate = 0;
            double receivedFramerate = 0;
            int drops = 0;

            CaptureHistoryEntry entry = new CaptureHistoryEntry(captureFile, start, end, cameraAlias, cameraIdentifier, configuredFramerate, receivedFramerate, drops);
            return entry;
        }

        private void AddCapturedFile(string filepath, Bitmap image, bool video)
        {
            if(!capturedFiles.HasThumbnails)
                view.ShowThumbnails();
            
            capturedFiles.AddFile(filepath, image, video);
        }
        #endregion

        #region Delayer
        /// <summary>
        /// Allocates or reallocates the delay buffer.
        /// This must be done each time the image descriptor changes or when available memory changes.
        /// </summary>
        private void AllocateDelayer()
        {
            if (!cameraLoaded || imageDescriptor == null || imageDescriptor == ImageDescriptor.Invalid)
                return;

            long totalMemory = ((long)PreferencesManager.CapturePreferences.CaptureMemoryBuffer * 1024 * 1024);
            long availableMemory = shared ? totalMemory / 2 : totalMemory;
            
            // FIXME: get the size of ring buffer from outside.
            availableMemory -= (imageDescriptor.BufferSize * 8);
            
            delayer.AllocateBuffers(imageDescriptor, availableMemory);
            delayCompositer.AllocateBuffers(imageDescriptor);
            UpdateDelayMaxAge();
        }
        private void DelayChanged(double age)
        {
            this.delay = (int)Math.Round(age);
            view.UpdateDelay(AgeToSeconds(delay), delay);
            
            // Force a refresh if we are not connected to the camera to enable "pause and browse".
            if (cameraLoaded && !cameraConnected)
            {
                Bitmap delayed = delayCompositer.Get(delay);
                viewportController.Bitmap = delayed;
                viewportController.Refresh();
            }
        }
        
        private void RefreshRateChanged(float rate)
        {
            rate = Math.Max(rate, 0.01f);
            view.UpdateSlomoRefreshRate(rate);

            DelayCompositeSlowMotion dcsm = delayComposites[delayCompositeType] as DelayCompositeSlowMotion;
            if (dcsm == null)
                return;

            dcsm.UpdateRefreshRate(rate);
        }

        private void ForceDelaySynchronization()
        {
            if (delayCompositeType == DelayCompositeType.SlowMotion)
            {
                DelayCompositeSlowMotion dcsm = delayComposites[delayCompositeType] as DelayCompositeSlowMotion;
                if (dcsm == null)
                    return;

                dcsm.Sync();
            }
        }

        private void UpdateDelayMaxAge()
        {
            view.UpdateDelayMaxAge(delayer.Capacity);
        }
        
        private double AgeToSeconds(int age)
        {
            if(pipelineManager.Frequency == 0)
                return 0;

            double framerate = PreferencesManager.CapturePreferences.DisplaySynchronizationFramerate;
            if (framerate == 0)
                return 0;

            framerate = Math.Min(framerate, pipelineManager.Frequency);

            return age / framerate;
        }
        
        private IDelayComposite GetComposite(DelayCompositeConfiguration configuration)
        {
            switch (configuration.CompositeType)
            {
                case DelayCompositeType.SlowMotion:
                    return new DelayCompositeSlowMotion();
                case DelayCompositeType.MultiReview:
                    return new DelayCompositeMultiReview();
                default:
                    return new DelayCompositeBasic();
            }
        }
        #endregion

        #endregion
    }
}
