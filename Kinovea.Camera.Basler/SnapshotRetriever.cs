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

using PylonC.NET;
using PylonC.NETSupportLibrary;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;
        public event EventHandler CameraImageTimedOut;
        public event EventHandler CameraImageError;
        
        public string Identifier 
        { 
            get { return this.summary.Identifier;}
        }
        public string Error
        {
            get { return error;}
        }
        
        #region Members
        private Bitmap image;
        private CameraSummary summary;
        private object locker = new object();
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private bool cancelled;
        private PYLON_DEVICE_HANDLE handle;
        private ImageProvider imageProvider;
        private string error;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SnapshotRetriever(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;

            imageProvider = new ImageProvider();
            
            try
            {
                handle = Pylon.CreateDeviceByIndex(deviceIndex);                
                imageProvider.Open(handle);
            }
            catch(Exception)
            {
                log.Error(PylonHelper.GetLastError());
            }
        }

        public void Run(object data)
        {
            if(!imageProvider.IsOpen)
                return;
            
            imageProvider.ImageReadyEvent += ImageProvider_ImageReadyEvent;
            imageProvider.GrabErrorEvent += ImageProvider_GrabErrorEvent;
            imageProvider.GrabbingStartedEvent += ImageProvider_GrabbingStartedEvent;
            
            try
            {
                imageProvider.BeforeSingleFrameAuto();
                imageProvider.SingleFrameAuto();
            }
            catch (Exception)
            {
                log.Error(imageProvider.GetLastErrorMessage());
            }
            
            waitHandle.WaitOne(500000);
            
            // Detach 
            imageProvider.GrabbingStartedEvent -= ImageProvider_GrabbingStartedEvent;
            imageProvider.GrabErrorEvent -= ImageProvider_GrabErrorEvent;
            imageProvider.ImageReadyEvent -= ImageProvider_ImageReadyEvent;
            
            imageProvider.AfterSingleFrameAuto();
            imageProvider.Close();
            
            if(!cancelled && !string.IsNullOrEmpty(error) && CameraImageError != null)
                CameraImageError(this, EventArgs.Empty);
            else if(!cancelled && image != null && CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, false, false));
            else if(!cancelled && image == null && CameraImageTimedOut != null)
                CameraImageTimedOut(this, EventArgs.Empty);
        }

        private void ImageProvider_GrabbingStartedEvent()
        {
            imageProvider.Trigger();
        }

        private void ImageProvider_ImageReadyEvent()
        {
            ImageProvider.Image pylonImage = imageProvider.GetLatestImage();
            
            if(pylonImage == null)
            {
                waitHandle.Set();
                return;
            }
            
            image = CreateBitmap(pylonImage);
            waitHandle.Set();
        }

        private void ImageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            log.ErrorFormat("Error received");
            log.Error(additionalErrorMessage);
            waitHandle.Set();
        }
        
        public void Cancel()
        {
            if(!imageProvider.IsOpen)
                return;
            
            cancelled = true;
            waitHandle.Set();
        }
        
        public Bitmap CreateBitmap(ImageProvider.Image pylonImage)
        {
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
