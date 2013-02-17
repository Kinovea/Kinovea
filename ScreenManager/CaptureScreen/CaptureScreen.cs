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
using System.Windows.Forms;
using System.Linq;

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;
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
        //private IFrameBuffer buffer;
        private CircularBufferMemory<Bitmap> buffer = new CircularBufferMemory<Bitmap>();
        private VideoRecorder recorder;
        private ViewportController viewportController;
        
        private CameraSummary summary;
        private Metadata metadata;
        private List<CapturedVideo> capturedVideos = new List<CapturedVideo>();
        private bool loaded;
        private int displayImageAge = 0;
        private int recordImageAge = 0;
        private bool firstImageReceived;
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
            
            this.summary = summary;
            manager = summary.Manager;
            grabber = manager.Connect(summary);
            grabber.CameraImageReceived += Grabber_CameraImageReceived;
            //grabber.CameraErrorReceived += Grabber_CameraErrorReceived;
            grabber.Start();
            
            UpdateTitle();
        }

        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool active)
        {
            view.DisplayAsActiveScreen(active);
        }
        public override void refreshUICulture() 
        {
            view.RefreshUICulture();
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
            view.AddImageDrawing(filename, svg);
        }
        public override void AddImageDrawing(Bitmap bmp)
        {
            view.AddImageDrawing(bmp);
        }
        public override void FullScreen(bool fullScreen)
        {
            view.FullScreen(fullScreen);
        }
        #endregion
        
        public void SetShared(bool shared)
        {
            // Compute new buffer size.
            //buffer.UpdateMemoryCapacity();
        }
        
        #region Methods called from the view. These could be events or commands.
        public void ViewClose()
        {
            OnCloseAsked(EventArgs.Empty);
        }
        public void ViewConfigure()
        {
            FormsHelper.BeforeShow();
            bool needsReconnect = manager.Configure(summary);
            FormsHelper.AfterShow();
            
            log.DebugFormat("After configure, summary:{0}", manager.GetSummaryAsText(summary));
            
            if(needsReconnect)
                Reconnect();
            
            UpdateTitle();
        }
        public void ViewToggleGrabbing()
        {
             if(grabber.Grabbing)
             {
                grabber.Stop();
                nonGrabbingInteractionTimer.Enabled = true;
             }
             else
             {
                nonGrabbingInteractionTimer.Enabled = false;
                grabber.Start();
             }
             
             view.UpdateGrabbingStatus(grabber.Grabbing);
        }
        #endregion
        #endregion
        
        #region Private methods
        private void Clean()
        {
            // Clean all resources before switching camera.
            // Close camera, empty buffers, etc.
            
            loaded = false;
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
                
            // Push to circular buffer.
            buffer.Write(image);
            
            Bitmap recordImage = buffer.Read(recordImageAge);
            Bitmap displayImage = buffer.Read(displayImageAge);

            viewportController.Bitmap = displayImage;
            
            if(!firstImageReceived)
            {
                log.DebugFormat("first image received, {0}, {1}", image.Size, displayImage.Size);
                viewportController.SetImageSize(displayImage.Size);
                firstImageReceived = true;
            }
            
            viewportController.Refresh();
        }
        private void ComputeFPS()
        {
            DateTime now = DateTime.Now;
            
            if(firstImageReceived)
            {
                TimeSpan span = now - lastImageTime;
                averager.Add(span.TotalSeconds);
                double fps = 1.0/averager.Average;
                
                // Find a way to report the measured fps.
                //view.UpdateTitle(string.Format("{0} - (measured: {1:0.00})", manager.GetSummaryAsText(summary), fps));
            }
        
            lastImageTime = now;
        }
        private void UpdateTitle()
        {
            view.UpdateTitle(manager.GetSummaryAsText(summary));
        }
        private void Reconnect()
        {
            if(grabber.Grabbing)
                grabber.Stop();
                
            buffer.Clear();
            
            firstImageReceived = false;
            grabber.Start();
        }
        private void NonGrabbingInteractionTimer_Tick(object sender, EventArgs e)
        {
            viewportController.Refresh();
        }
        #endregion
    }
}
