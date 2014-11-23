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
        private double computedFps = 0;
        private Control dummy = new Control();
        private System.Windows.Forms.Timer nonGrabbingInteractionTimer = new System.Windows.Forms.Timer();
        private DateTime lastImageTime;
        private Averager averager = new Averager(25);
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
            view.DualCommandReceived += (s, e) => OnDualCommandReceived(e);
            
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

            //ToggleGrabbing();
        }

        public void ForceRecordingStatus(bool record)
        {
            if (recording == record)
                return;

            //ToggleRecording(view.CurrentVideoFilename);
        }

        public void PerformSnapshot()
        {
            //MakeSnapshot(view.CurrentImageFilename);
        }

        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool active)
        {
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

            // Destroy resources (symmetric to constructor).
            pipelineManager.FrameSignaled -= pipelineManager_FrameSignaled;
            nonGrabbingInteractionTimer.Tick -= NonGrabbingInteractionTimer_Tick;
            viewportController.DisplayRectangleUpdated -= ViewportController_DisplayRectangleUpdated;
            //view.DualCommandReceived += (s, e) => OnDualCommandReceived(e);
            view.BeforeClose();
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
            //MakeSnapshot(filename);
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

            // Start recorder thread. 
            // It will be dormant until recording is started but it has the same lifetime as the pipeline.
            consumerRecord = new ConsumerMJPEGRecorder();
            recorderThread = new Thread(consumerRecord.Run) { IsBackground = true };
            recorderThread.Name = consumerRecord.GetType().Name;
            recorderThread.Start();

            // Initialize pipeline.
            imageDescriptor = cameraGrabber.Prepare();
            pipelineManager.Connect(imageDescriptor, (IFrameProducer)cameraGrabber, consumerDisplay, consumerRecord);
            
            viewportController.InitializeDisplayRectangle(cameraSummary.DisplayRectangle, new Size(imageDescriptor.Width, imageDescriptor.Height));

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

            if (recording)
                StopRecording();

            consumerRecord.Stop();
            if (recorderThread != null && recorderThread.IsAlive)
                recorderThread.Join(500);

            if (recorderThread.IsAlive)
                log.ErrorFormat("Time out while waiting for recorder thread to join.");

            pipelineManager.Disconnect();

            if (cameraGrabber != null)
            {
                cameraGrabber.Stop();
                cameraGrabber.GrabbingStatusChanged -= Grabber_GrabbingStatusChanged;
                nonGrabbingInteractionTimer.Enabled = true;
            }

            UpdateTitle();
            cameraConnected = false;
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
            // A frame was received by the camera.
            // We use this as a clock tick to update the consumer responsible for display.
            // This consumer cannot block because it's on the UI thread.
            consumerDisplay.ConsumeOne();

            viewportController.Bitmap = consumerDisplay.Bitmap;
            viewportController.Refresh();
        }

        #endregion



        #region Private methods
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
        
        /*private void Grabber_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            try
            {
                dummy.BeginInvoke((Action)delegate { ImageReceived(e.Image); });
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Begin invoke failed on dummy control.", ex.ToString());
                dummy = new Control();
            }
        }
        private void ImageReceived(Bitmap image)
        {
            ComputeFPS();
            
            if(!cameraGrabber.Grabbing)
                return;

            buffer.Write(image);
            
            Bitmap recordImage = buffer.Read(displayImageAge);
            Bitmap displayImage = buffer.Read(displayImageAge);
            
            if(currentImageSize != displayImage.Size)
            {
                log.DebugFormat("new size of image received, {0}, {1}", image.Size, displayImage.Size);
                viewportController.InitializeDisplayRectangle(cameraSummary.DisplayRectangle, displayImage.Size);
                firstImageReceived = true;
                currentImageSize = displayImage.Size;
                metadata.ImageSize = currentImageSize;
                frameMemory = (double)currentImageSize.Height * currentImageSize.Width * BytesPerPixel(image.PixelFormat);
                UpdateBufferCapacity();
            }
            
            viewportController.Bitmap = displayImage;
            viewportController.Timestamp = 0;
            
            if(recording)
                recorder.EnqueueFrame(CopyImage(displayImage));
            
            UpdateInfo();
            
            viewportController.Refresh();
        }*/
        
        /*private void ComputeFPS()
        {
            DateTime now = DateTime.Now;
            
            if(firstImageReceived)
            {
                TimeSpan span = now - lastImageTime;
                averager.Add(span.TotalSeconds);
                computedFps = 1.0/averager.Average;
            }
        
            lastImageTime = now;
        }*/
        
        private void UpdateTitle()
        {
            view.UpdateTitle(cameraManager.GetSummaryAsText(cameraSummary));
        }
        
        private void UpdateInfo()
        {
            string info = "";
            //float fill = buffer.Fill * 100;
            float fill = 0;
            
            if(fill == 100)
                info = string.Format("Actual: {0}×{1} @ {2:0.00}fps, Buffer: 100%", currentImageSize.Width, currentImageSize.Height, computedFps);
            else
                info = string.Format("Actual: {0}×{1} @ {2:0.00}fps, Buffer: {3:0.00}%", currentImageSize.Width, currentImageSize.Height, computedFps, fill);
            
            view.UpdateInfo(info);
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
            if(computedFps == 0)
                return 0;
                
            return age / computedFps;
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
        /*private void MakeSnapshot(string filename)
        {
            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(filename, false);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;
            
            Bitmap outputImage = buffer.Read(displayImageAge);
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
        }*/
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
            string filepath = directory + "\\" + filename + extension;
            
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

            view.UpdateRecordingStatus(recording);
        }
        
        private void StartRecording(string filename)
        {
            if (!cameraLoaded || !cameraConnected || recording)
                return;

            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(filename, true);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;

            if (!cameraConnected)
                Connect();

            if (consumerRecord.Active)
                consumerRecord.Deactivate();
            
            double interval = cameraGrabber.Framerate > 0 ? 1000.0 / cameraGrabber.Framerate : 40;

            SaveResult result = consumerRecord.Prepare(filepath, interval);
            consumerRecord.Activate();

            recording = result == SaveResult.Success;
            
            if(recording)
            {
                string next = filenameHelper.Next(filename, true);
                view.UpdateNextVideoFilename(next, !PreferencesManager.CapturePreferences.CaptureUsePattern);
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
            if(!cameraLoaded || !cameraConnected || !recording || !consumerRecord.Active)
               return;
             
            recording = false;
            consumerRecord.Deactivate();

            //PreferencesManager.CapturePreferences.VideoFile = recorder.Filename;
            //PreferencesManager.Save();
             
            /*if(recorder.CaptureThumb != null)
            {
                AddCapturedFile(recorder.Filepath, recorder.CaptureThumb, true);
                recorder.CaptureThumb.Dispose();
            }*/
             
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
