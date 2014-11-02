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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using PylonC.NET;
using PylonC.NETSupportLibrary;
using Kinovea.Base;
using Kinovea.Services;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// The main grabbing class for Basler devices.
    /// </summary>
    public class FrameGrabber : IFrameGrabber
    {
        //public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
        public event EventHandler<EventArgs<byte[]>> FrameProduced;
        public event EventHandler GrabbingStatusChanged;
        
        #region Property
        public bool Grabbing
        { 
            get { return grabbing;}
        }
        
        public Size Size
        {
            get { return actualSize; }
        }

        public int Depth
        {
            get { return 1; }
        }


        
        public float Framerate
        {
            get { return framerate; }
        }
        
        /*public string ErrorDescription
        {
            get { return errorDescription;}
        }*/
        #endregion
        
        #region Members
        private Bitmap image;
        private CameraSummary summary;
        private object locker = new object();
        private uint deviceIndex;
        private ImageProvider imageProvider;
        private bool grabbing;
        private Size actualSize;
        private float framerate;
        private LoopWatcher watcher = new LoopWatcher();
        private System.Windows.Forms.Timer triggerTimer = new System.Windows.Forms.Timer();
        private Random random = new Random();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
    
        public FrameGrabber(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;
            this.deviceIndex = deviceIndex;
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            ConfigureDevice();
            
            imageProvider.ImageReadyEvent += ImageProvider_ImageReadyEvent;
            imageProvider.GrabErrorEvent += ImageProvider_GrabErrorEvent;
            imageProvider.GrabbingStartedEvent += ImageProvider_GrabbingStartedEvent;
            
            imageProvider.Continuous();
        }
        
        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            imageProvider.ImageReadyEvent -= ImageProvider_ImageReadyEvent;
            imageProvider.GrabErrorEvent -= ImageProvider_GrabErrorEvent;
            imageProvider.GrabbingStartedEvent -= ImageProvider_GrabbingStartedEvent;
            Close();
            
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void Close()
        {
            // Close and destroy.
            SpecificInfo specific = summary.Specific as SpecificInfo;
            specific.Handle = null;
            
            imageProvider.Close();
            
            log.DebugFormat("bitmap creation avg : {0}", watcher.Average);
        }
        
        private void SoftwareTrigger()
        {
            if(!grabbing)
                return;
                
            imageProvider.Trigger();
        }
        
        private void ConfigureDevice()
        {
            if(grabbing)
                Stop();
                
            CreateDevice();
            if(imageProvider == null)
                return;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if(specific == null)
                return;
           
            framerate = imageProvider.GetFrameRate();
            actualSize = Size.Empty;
        }
        
        private void CreateDevice()
        {
            imageProvider = new ImageProvider();
            
            try
            {
                PYLON_DEVICE_HANDLE handle = Pylon.CreateDeviceByIndex(deviceIndex);
                imageProvider.Open(handle);
                
                SpecificInfo specific = summary.Specific as SpecificInfo;
                specific.Handle = handle;
            }
            catch(Exception)
            {
                log.Error(imageProvider.GetLastErrorMessage());
            }
        }
        
        private void ImageProvider_GrabbingStartedEvent()
        {
            grabbing = true;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }
        
        private void ImageProvider_ImageReadyEvent()
        {
            // Consume the Pylon queue (no copy).
            ImageProvider.Image pylonImage = imageProvider.GetLatestImage();
            if (pylonImage == null)
                return;

            if (actualSize == Size.Empty)
                actualSize = new Size(pylonImage.Width, pylonImage.Height);
            
            // At that point we have a reference on the Pylon-owned bytes.
            // Rather than using Pylon's BitmapFactory to build a Bitmap from the bytes, we transmit the bytes directly downstream.
            
            if (FrameProduced != null)
                FrameProduced(this, new EventArgs<byte[]>(pylonImage.Buffer));
            
            // When we are back from the event handler, the bytes have been copied to the shared queue.
            imageProvider.ReleaseImage();

            //----------
            //if(CameraImageReceived != null)
            //    CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, pylonImage.Buffer));

            //image = CreateBitmap(pylonImage);
            //actualSize = image.Size;
            //if(CameraImageReceived != null)
            //    CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
        }

        private void ImageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            log.ErrorFormat("Error from device {0}: {1}", summary.Alias, additionalErrorMessage);
        }
        
        private Bitmap CreateBitmap(ImageProvider.Image pylonImage)
        {
            // TODO code duplicated with SnapshotRetriever.
        
            if (pylonImage == null)
                return null;
            
            Bitmap bitmap = null;
            BitmapFactory.CreateBitmap(out bitmap, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            BitmapFactory.UpdateBitmap(bitmap, pylonImage.Buffer, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            imageProvider.ReleaseImage();

            return bitmap;
        }
    }
}
