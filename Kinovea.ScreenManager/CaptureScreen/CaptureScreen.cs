#region License
/*
Copyright © Joan Charmant 2013.
joan.charmant@gmail.com 
 
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
        private bool firstImageReceived;
        private bool recording;
        private ImageDescriptor imageDescriptor;

        private CameraSummary cameraSummary;
        private CameraManager cameraManager;
        private ICaptureSource cameraGrabber;
        private PipelineManager pipelineManager = new PipelineManager();
        private ConsumerDisplay consumerDisplay = new ConsumerDisplay();
        private ConsumerMJPEGRecorder consumerRecord;
        private Thread recorderThread;
        private Bitmap recordingThumbnail;

        private ViewportController viewportController;
        private FilenameHelper filenameHelper = new FilenameHelper();
        private CapturedFiles capturedFiles = new CapturedFiles();
        
        private int bufferCapacity = 1;
        private double availableMemory;
        private bool shared;
        private bool synched;
        
        private Metadata metadata;
        private MetadataRenderer metadataRenderer;
        private MetadataManipulator metadataManipulator;
        private ScreenToolManager screenToolManager = new ScreenToolManager();
        private DrawingToolbarPresenter drawingToolbarPresenter = new DrawingToolbarPresenter();
        private int displayImageAge = 0;
        private int recordImageAge = 0;
        private Size currentImageSize;
        private Control dummy = new Control();
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

            view.SetViewport(viewportController.View);
            view.SetCapturedFilesView(capturedFiles.View);
            
            InitializeCaptureFilenames();
            InitializeTools();            
            InitializeMetadata();
            
            view.SetToolbarView(drawingToolbarPresenter.View);
            
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
            
            nonGrabbingInteractionTimer.Interval = 15;
            nonGrabbingInteractionTimer.Tick += NonGrabbingInteractionTimer_Tick;

            pipelineManager.FrameSignaled += pipelineManager_FrameSignaled;
        }

        #region Public methods
        
        public void SetShared(bool shared)
        {
            this.shared = shared;
            //UpdateMemory();
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

            ToggleRecording(view.CurrentVideoFilename);
        }

        public void PerformSnapshot()
        {
            MakeSnapshot(view.CurrentImageFilename);
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
            //UpdateMemory();
            InitializeCaptureFilenames();
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

            nonGrabbingInteractionTimer.Tick -= NonGrabbingInteractionTimer_Tick;
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
        public void View_ToggleGrabbing()
        {
            if (!cameraLoaded)
                return;

            ToggleConnection();
        }
        public void View_DelayChanged(double value)
        {
            /*displayImageAge = (int)Math.Round(value);
            view.UpdateDelayLabel(AgeToSeconds(displayImageAge), displayImageAge);
            
            if(cameraGrabber != null && !cameraGrabber.Grabbing)
            {
                Bitmap displayImage = buffer.Read(displayImageAge);
                viewportController.Bitmap = displayImage;
                viewportController.Timestamp = 0;
                viewportController.Refresh();
            }*/
        }
        public void View_SnapshotAsked(string filename)
        {
            MakeSnapshot(filename);
        }
        public void View_ToggleRecording(string filename)
        {
            ToggleRecording(filename);
        }
        
        public void View_ValidateFilename(string filename)
        {
            bool allowEmpty = true;
            if(!filenameHelper.ValidateFilename(filename, allowEmpty))
                ScreenManagerKernel.AlertInvalidFileName();
        }
        public void View_OpenInExplorer(string path)
        {
            FilesystemHelper.LocateDirectory(path);
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

            firstImageReceived = false;
            //currentImageSize = Size.Empty;

            imageDescriptor = cameraGrabber.Prepare();
            if (imageDescriptor == null || imageDescriptor.Format == Video.ImageFormat.None || imageDescriptor.Width <= 0 || imageDescriptor.Height <= 0)
            {
                log.ErrorFormat("The camera does not support configuration so we cannot preallocate buffers.");
                UpdateTitle();
                return;
            }

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
            cameraGrabber.GrabbingStatusChanged += Grabber_GrabbingStatusChanged;
            cameraGrabber.Start();
            
            UpdateTitle();
            cameraConnected = true;
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
                nonGrabbingInteractionTimer.Enabled = true;
            }

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

            // A frame was received by the camera.
            // We use this as a clock tick to update the consumer responsible for display.
            // This consumer cannot block because it's on the UI thread.
            consumerDisplay.ConsumeOne();

            // Note: the viewport has a reference on the consumerDisplay allocated bitmap.
            // This means that when the consumerDisplay disposes the bitmap, the viewport must be alerted 
            // so that it avoids trying to draw the image. The alternative is to copy the image into the viewport.
            viewportController.Bitmap = consumerDisplay.Bitmap;
            viewportController.Refresh();

            UpdateStats();

            if (recording && recordingThumbnail == null)
                recordingThumbnail = BitmapHelper.Copy(consumerDisplay.Bitmap);
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
            view.UpdateGrabbingStatus(cameraGrabber.Grabbing);
        }
        
        private void UpdateTitle()
        {
            view.UpdateTitle(cameraManager.GetSummaryAsText(cameraSummary));
        }
        
        private void UpdateStats()
        {
            if (pipelineManager.Frequency == 0)
                return;

            view.UpdateInfo(string.Format("Signal: {0:0.00} fps. Data: {1:0.00} MB/s. Drops: {2}.",
                pipelineManager.Frequency, cameraGrabber.LiveDataRate, pipelineManager.Drops));
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
        private void ViewportController_DisplayRectangleUpdated(object sender, EventArgs e)
        {
            if (!cameraLoaded || cameraSummary == null)
                return;

            cameraSummary.UpdateDisplayRectangle(viewportController.DisplayRectangle);
            CameraTypeManager.UpdatedCameraSummary(cameraSummary);
        }
        private double AgeToSeconds(int age)
        {
            if(pipelineManager.Frequency == 0)
                return 0;

            return age / pipelineManager.Frequency;
        }
        
        private void UpdateDelayMaxAge()
        {
            double maxAge = bufferCapacity - 1;
            if(maxAge == 0)
                maxAge = 0.9999;

            view.UpdateDelayMaxAge(maxAge);
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
        
        #region Recording/Snapshoting
        private void InitializeCaptureFilenames()
        {
            string imageFilename = filenameHelper.GetImageFilename();
            view.UpdateNextImageFilename(imageFilename, !PreferencesManager.CapturePreferences.CaptureUsePattern);
            string videoFilename = filenameHelper.GetVideoFilename();
            view.UpdateNextVideoFilename(videoFilename, !PreferencesManager.CapturePreferences.CaptureUsePattern);
        }
        private void MakeSnapshot(string filename)
        {
            if (!cameraLoaded || consumerDisplay.Bitmap == null)
                return;

            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(filename, false);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;
            
            Bitmap outputImage = BitmapHelper.Copy(consumerDisplay.Bitmap);
            if(outputImage == null)
                return;
            
            ImageHelper.Save(filepath, outputImage);
            
            AddCapturedFile(filepath, outputImage, false);

            if(PreferencesManager.CapturePreferences.CaptureUsePattern)
                filenameHelper.AutoIncrement(false);
            
            PreferencesManager.CapturePreferences.ImageFile = filename;
            PreferencesManager.Save();
            
            string next = filenameHelper.Next(filename, false);
            view.UpdateNextImageFilename(next, !PreferencesManager.CapturePreferences.CaptureUsePattern);
            
            view.Toast(ScreenManagerLang.Toast_ImageSaved, 750);
            
            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }
        
        private bool SanityCheckRecording(string filename)
        {
            if(cameraGrabber == null)
                return false;
            
            if(!filenameHelper.ValidateFilename(filename, false))
            {
                ScreenManagerKernel.AlertInvalidFileName();
                return false;
            }
            
            return true;
        }
        private string GetFilePath(string filename, bool video)
        {
            string directory = video ? PreferencesManager.CapturePreferences.VideoDirectory : PreferencesManager.CapturePreferences.ImageDirectory;
            
            if(PreferencesManager.CapturePreferences.CaptureUsePattern)
                filename = video ? filenameHelper.GetVideoFilename() : filenameHelper.GetImageFilename();
            
            string extension = video ? filenameHelper.GetVideoFileExtension() : filenameHelper.GetImageFileExtension();
            string filepath = Path.Combine(directory, filename + extension);
            
            filenameHelper.CreateDirectory(filepath);
            
            return filepath;
        }
        private bool OverwriteCheck(string filepath)
        {
            if(!File.Exists(filepath))
                return true;
            
            string msgTitle = ScreenManagerLang.Error_Capture_FileExists_Title;
            string msgText = String.Format(ScreenManagerLang.Error_Capture_FileExists_Text, filepath).Replace("\\n", "\n");
        
            DialogResult result = MessageBox.Show(msgText, msgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            return result == DialogResult.Yes;
        }

        private void ToggleRecording(string filename)
        {
            if (recording)
                StopRecording();
            else
                StartRecording(filename);
        }
        
        private void StartRecording(string filename)
        {
            if (!cameraLoaded || !cameraConnected || recording)
                return;

            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(Path.GetFileName(filename), true);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;

            if (!cameraConnected)
                Connect();

            if (!cameraConnected)
                return;

            if (consumerRecord.Active)
                consumerRecord.Deactivate();
            
            if (recordingThumbnail != null)
            {
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }
            
            double interval = cameraGrabber.Framerate > 0 ? 1000.0 / cameraGrabber.Framerate : 40;
            SaveResult result = pipelineManager.StartRecord(filepath, interval);
            recording = result == SaveResult.Success;
            
            if(recording)
            {
                string next = filenameHelper.Next(filename, true);
                view.UpdateNextVideoFilename(next, !PreferencesManager.CapturePreferences.CaptureUsePattern);
                view.UpdateRecordingStatus(recording);
                view.Toast(ScreenManagerLang.Toast_StartRecord, 1000);
                NotificationCenter.RaiseRefreshFileExplorer(this, false);
            }
            else
            {
                //DisplayError(result);
            }
        }

        private void StopRecording()
        {
            if(!cameraLoaded || !recording || !consumerRecord.Active)
               return;
             
            recording = false;
            consumerRecord.Deactivate();

            PreferencesManager.CapturePreferences.VideoFile = Path.GetFileNameWithoutExtension(consumerRecord.Filename);
            PreferencesManager.Save();
             
            if(recordingThumbnail != null)
            {
                AddCapturedFile(consumerRecord.Filename, recordingThumbnail, true);
                recordingThumbnail.Dispose();
                recordingThumbnail = null;
            }
             
            view.UpdateRecordingStatus(recording);
            view.Toast(ScreenManagerLang.Toast_StopRecord, 750);
        }

        private void AddCapturedFile(string filepath, Bitmap image, bool video)
        {
            if(!capturedFiles.HasThumbnails)
                view.ShowThumbnails();
            
            capturedFiles.AddFile(filepath, image, video);
        }
        #endregion
        
        #endregion
    }
}
