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
using Kinovea.Pipeline;
using Kinovea.Video;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// We use whatever settings are currently configured in the camera.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;

        public string Identifier
        {
            get { return this.summary.Identifier; }
        }
        
        #region Members
        private static readonly int timeout = 5000;
        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private object locker = new object();
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private bool cancelled;
        private bool hadError;
        private PYLON_DEVICE_HANDLE deviceHandle;
        private ImageProvider device;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SnapshotRetriever(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;

            device = new ImageProvider();
            
            try
            {
                deviceHandle = Pylon.CreateDeviceByIndex(deviceIndex);                
                device.Open(deviceHandle);
            }
            catch(Exception)
            {
                log.Error(PylonHelper.GetLastError());
            }
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            Thread.CurrentThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            if (!device.IsOpen)
            {
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, imageDescriptor, true, false));

                return;
            }
             
            device.ImageReadyEvent += ImageProvider_ImageReadyEvent;
            device.GrabErrorEvent += ImageProvider_GrabErrorEvent;
            device.GrabbingStartedEvent += ImageProvider_GrabbingStartedEvent;
            
            try
            {
                device.BeforeSingleFrameAuto();
                device.SingleFrameAuto();
            }
            catch (Exception)
            {
                log.Error(device.GetLastErrorMessage());
            }
            
            waitHandle.WaitOne(timeout, false);
            
            device.GrabbingStartedEvent -= ImageProvider_GrabbingStartedEvent;
            device.GrabErrorEvent -= ImageProvider_GrabErrorEvent;
            device.ImageReadyEvent -= ImageProvider_ImageReadyEvent;
            
            device.AfterSingleFrameAuto();
            device.Close();

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            if (!device.IsOpen)
                return;

            cancelled = true;
            waitHandle.Set();
        }

        private void ImageProvider_GrabbingStartedEvent()
        {
            device.Trigger();
        }

        private void ImageProvider_ImageReadyEvent()
        {
            ImageProvider.Image pylonImage = device.GetLatestImage();
            
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
            log.ErrorFormat("Error received trying to get a thumbnail for {0}", summary.Alias);
            log.Error(grabException.ToString());
            log.Error(additionalErrorMessage);

            hadError = true;
            waitHandle.Set();
        }
        
        private Bitmap CreateBitmap(ImageProvider.Image pylonImage)
        {
            if (pylonImage == null)
                return null;
            
            Bitmap bitmap = null;
            BitmapFactory.CreateBitmap(out bitmap, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            BitmapFactory.UpdateBitmap(bitmap, pylonImage.Buffer, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            device.ReleaseImage();

            if (bitmap != null)
            {
                int bufferSize = ImageFormatHelper.ComputeBufferSize(bitmap.Width, bitmap.Height, Video.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, bitmap.Width, bitmap.Height, true, bufferSize); 
            }

            return bitmap;
        }
    }
}
