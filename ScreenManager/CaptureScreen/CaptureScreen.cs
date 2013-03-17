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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Main presenter class for the capture ui.
    /// Responsible for managing and synching a grabber, a circular buffer, a recorder and a viewport.
    /// </summary>
    public class CaptureScreen : AbstractScreen
    {
        #region events
        #endregion
        
        #region Properties
        public override Guid UniqueId
        {
            get { return uid; }
            set { uid = value;}
        }
        public override bool Full
        {
            get { return grabber == null ? false : grabber.Grabbing; }
        }
        public override string FileName
        {
            get 
            {
                if(!loaded)
                    return ScreenManagerLang.statusEmptyScreen;
                else
                    return summary.Alias;
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
            //get { return frameServer.AspectRatio; }
            //set { frameServer.AspectRatio = value; }
            get { return ImageAspectRatio.Auto;}
            set { }
        }
        #endregion
        
        #region Members
        private Guid uid = System.Guid.NewGuid();
        private ICaptureScreenView view;
        
        private CameraManager manager;
        private IFrameGrabber grabber;
        private CircularBufferMemory<Bitmap> buffer = new CircularBufferMemory<Bitmap>();
        private VideoRecorder recorder;
        private ViewportController viewportController;
        private FilenameHelper filenameHelper = new FilenameHelper();
        private CapturedFiles capturedFiles = new CapturedFiles();
        private bool recording;
        
        private int bufferCapacity = 1;
        private double availableMemory;
        private double frameMemory;
        private bool shared;
        
        private CameraSummary summary;
        private Metadata metadata;
        private bool loaded;
        private int displayImageAge = 0;
        private int recordImageAge = 0;
        private Size currentImageSize;
        private bool firstImageReceived;
        private double computedFps = 0;
        private Control dummy = new Control();
        private Timer nonGrabbingInteractionTimer = new Timer();
        private DateTime lastImageTime;
        private Averager averager = new Averager(25);
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public CaptureScreen()
        {
            log.Debug("Constructing a CaptureScreen.");
            view = new CaptureScreenView(this);
            
            viewportController = new ViewportController();
            view.SetViewport(viewportController.View);
            view.SetCapturedFilesView(capturedFiles.View);
            
            InitializeCaptureFilenames();
            
            IntPtr forceHandleCreation = dummy.Handle; // Needed to show that the main thread "owns" this Control.
            
            nonGrabbingInteractionTimer.Interval = 15;
            nonGrabbingInteractionTimer.Tick += NonGrabbingInteractionTimer_Tick;
        }
        
        #region Public methods
        
        public void LoadCamera(CameraSummary summary)
        {
            // Initialize everything and start grabbing.
            if(loaded)
                Clean();
            
            loaded = true;
            this.summary = summary;
            manager = summary.Manager;
            grabber = manager.Connect(summary);
            
            viewportController.DisplayRectangleUpdated += ViewportController_DisplayRectangleUpdated;
            
            StartGrabber();
            UpdateTitle();
            
            OnActivated(EventArgs.Empty);
        }
        
        public void SetShared(bool shared)
        {
            this.shared = shared;
            UpdateMemory();
        }

        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool active)
        {
            view.DisplayAsActiveScreen(active);
        }
        public override void RefreshUICulture() 
        {
            view.RefreshUICulture();
        }
        public override void PreferencesUpdated()
        {
            UpdateMemory();
            InitializeCaptureFilenames();
        }
        public override void BeforeClose()
        {
            // recorder.Stop();
            
            if(grabber != null)
                grabber.Stop();
            
            buffer.Clear();
            view.BeforeClose();
        }
        public override void AfterClose()
        {
            // Fixme: all the stopping and cleaning is implemented in BeforeClose instead of AfterClose. 
            // It works while there is no cancellation possible.
        }
        public override bool OnKeyPress(Keys key)
        {
            return view.OnKeyPress(key);
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
        #endregion
        
        #region Methods called from the view. These could also be events or commands.
        public void View_Close()
        {
            OnCloseAsked(EventArgs.Empty);
        }
        public void View_Configure()
        {
            FormsHelper.BeforeShow();
            bool needsReconnect = manager.Configure(summary);
            FormsHelper.AfterShow();
            
            log.DebugFormat("After configure, summary:{0}", manager.GetSummaryAsText(summary));
            
            if(needsReconnect)
                Reconnect();
            
            UpdateTitle();
        }
        public void View_ToggleGrabbing()
        {
             if(grabber.Grabbing)
             {
                StopGrabber(true);
                view.Toast(ScreenManagerLang.Toast_Pause, 750);
             }
             else
             {
                StartGrabber();
             }
             
             view.UpdateGrabbingStatus(grabber.Grabbing);
             
        }
        public void View_DelayChanged(double value)
        {
            displayImageAge = (int)Math.Round(value);
            view.UpdateDelayLabel(AgeToSeconds(displayImageAge), displayImageAge);
            
            if(grabber != null && !grabber.Grabbing)
            {
                Bitmap displayImage = buffer.Read(displayImageAge);
                viewportController.Bitmap = displayImage;
                viewportController.Refresh();
            }
        }
        public void View_SnapshotAsked(string filename)
        {
            MakeSnapshot(filename);
        }
        public void View_ToggleRecording(string filename)
        {
            if(recording)
                StopRecording();
            else
                StartRecording(filename);
            
            view.UpdateRecordingStatus(recording);
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
        #endregion
        #endregion
        
        #region Private methods
        private void Clean()
        {
            // Clean all resources before switching camera.
            StopGrabber(false);
            buffer.Clear();
            firstImageReceived = false;
            currentImageSize = Size.Empty;
            frameMemory = 0;
            
            // Reset buffer capacity until we have the official image size and depth.
            bufferCapacity = 1;
            buffer.ChangeCapacity(bufferCapacity);
            UpdateDelayMaxAge();
            log.ErrorFormat("Buffer capacity changed to {0}", bufferCapacity);
        }
        private void UpdateMemory()
        {
            double totalMemory = PreferencesManager.CapturePreferences.CaptureMemoryBuffer * 1024 * 1024;
            double availableMemory = shared ? totalMemory / 2 : totalMemory;
            
            if(this.availableMemory != availableMemory)
            {
                this.availableMemory = availableMemory;
                UpdateBufferCapacity();
            }
        }
        private void Grabber_CameraImageReceived(object sender, CameraImageReceivedEventArgs e)
        {
            dummy.BeginInvoke((Action) delegate { ImageReceived(e.Image);});
        }
        private void ImageReceived(Bitmap image)
        {
            ComputeFPS();
            
            if(!grabber.Grabbing)
                return;

            buffer.Write(image);
            
            Bitmap recordImage = buffer.Read(displayImageAge);
            Bitmap displayImage = buffer.Read(displayImageAge);
            
            if(currentImageSize != displayImage.Size)
            {
                log.DebugFormat("new size of image received, {0}, {1}", image.Size, displayImage.Size);
                viewportController.InitializeDisplayRectangle(summary.DisplayRectangle, displayImage.Size);
                firstImageReceived = true;
                currentImageSize = displayImage.Size;
                frameMemory = (double)currentImageSize.Height * currentImageSize.Width * BytesPerPixel(image.PixelFormat);
                UpdateBufferCapacity();
            }
            
            viewportController.Bitmap = displayImage;
            
            if(recording)
                recorder.EnqueueFrame(CopyImage(displayImage));
            
            UpdateInfo();
            
            viewportController.Refresh();
        }
        private void ComputeFPS()
        {
            DateTime now = DateTime.Now;
            
            if(firstImageReceived)
            {
                TimeSpan span = now - lastImageTime;
                averager.Add(span.TotalSeconds);
                computedFps = 1.0/averager.Average;
            }
        
            lastImageTime = now;
        }
        private Bitmap CopyImage(Bitmap original)
        {
            Bitmap copy = new Bitmap(original.Width, original.Height, original.PixelFormat);
            Graphics g = Graphics.FromImage(copy);
            g.DrawImageUnscaled(original, Point.Empty);
            return copy;
        }
        private void UpdateTitle()
        {
            view.UpdateTitle(manager.GetSummaryAsText(summary));
        }
        private void UpdateInfo()
        {
            string info = "";
            float fill = buffer.Fill * 100;
            
            if(fill == 100)
                info = string.Format("Actual: {0}×{1} @ {2:0.00}fps, Buffer: 100%", currentImageSize.Width, currentImageSize.Height, computedFps);
            else
                info = string.Format("Actual: {0}×{1} @ {2:0.00}fps, Buffer: {3:0.00}%", currentImageSize.Width, currentImageSize.Height, computedFps, fill);
            
            view.UpdateInfo(info);
        }
        private void StartGrabber()
        {
            nonGrabbingInteractionTimer.Enabled = false;
            grabber.CameraImageReceived += Grabber_CameraImageReceived;
            grabber.Start();
        }
        private void StopGrabber(bool monitorInteraction)
        {
            if(grabber == null)
                return;

            grabber.CameraImageReceived -= Grabber_CameraImageReceived;

            if(grabber.Grabbing)
                grabber.Stop();
           
            nonGrabbingInteractionTimer.Enabled = monitorInteraction;
        }
        private void Reconnect()
        {
            Clean();
            StartGrabber();
            UpdateTitle();
        }
        private void NonGrabbingInteractionTimer_Tick(object sender, EventArgs e)
        {
            viewportController.Refresh();
        }
        private void ViewportController_DisplayRectangleUpdated(object sender, EventArgs e)
        {
            summary.UpdateDisplayRectangle(viewportController.DisplayRectangle);
            CameraTypeManager.UpdatedCameraSummary(summary);
        }
        private double AgeToSeconds(int age)
        {
            return age / computedFps;
        }
        private void UpdateBufferCapacity()
        {
            // This should only be done when we are actually sure of what the camera is sending us,
            // and not solely based on the desired options.
            
            if(frameMemory == 0)
                bufferCapacity = 1;
            else
                bufferCapacity = (int)(Math.Max(1, availableMemory / frameMemory));
            
            buffer.ChangeCapacity(bufferCapacity);
            UpdateDelayMaxAge();
            log.DebugFormat("Buffer capacity changed to {0}", bufferCapacity);
        }
        private int BytesPerPixel(PixelFormat pixelFormat)
        {
            switch(pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 4;
                default:
                    return 0;
            }
        }
        private void UpdateDelayMaxAge()
        {
            double maxAge = bufferCapacity - 1;
            if(maxAge == 0)
                maxAge = 0.9999;

            view.UpdateDelayMaxAge(maxAge);
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
            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(filename, false);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;
            
            Bitmap outputImage = buffer.Read(displayImageAge);
            
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
            if(grabber == null)
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
        private void StartRecording(string filename)
        {
            if(recording)
                return;

            bool ok = SanityCheckRecording(filename);
            if(!ok)
                return;
                
            string filepath = GetFilePath(filename, true);
            filename = Path.GetFileNameWithoutExtension(filepath);
            if(!OverwriteCheck(filepath))
                return;
            
            if(!grabber.Grabbing)
                StartGrabber();
                
            if(recorder != null)
                recorder.Close();
            
            recorder = new VideoRecorder();
            
            double interval = grabber.Framerate > 0 ? 1000.0 / grabber.Framerate : 40;
            SaveResult result = recorder.Initialize(filepath, interval, grabber.Size);
            
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
             if(!recording || grabber == null || recorder == null)
                return;
             
             recording = false;
             recorder.Close();
             PreferencesManager.CapturePreferences.VideoFile = recorder.Filename;
             PreferencesManager.Save();
             
             if(recorder.CaptureThumb != null)
             {
                 AddCapturedFile(recorder.Filepath, recorder.CaptureThumb, true);
                 recorder.CaptureThumb.Dispose();
             }
             
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
