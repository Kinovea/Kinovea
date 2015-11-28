#region License
/*
Copyright © Joan Charmant 2014.
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
using Kinovea.Pipeline;
using PylonC.NETSupportLibrary;
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
        private static readonly int timeoutGrabbing = 5000;
        private static readonly int timeoutOpening = 100;

        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private ImageProvider imageProvider = new ImageProvider();
        private bool cancelled;
        private bool hadError;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;

            imageProvider.GrabErrorEvent += imageProvider_GrabErrorEvent;
            imageProvider.DeviceRemovedEvent += imageProvider_DeviceRemovedEvent;
            imageProvider.ImageReadyEvent += imageProvider_ImageReadyEvent;
            
            try
            {
                imageProvider.Open(deviceIndex);
            }
            catch (Exception e) 
            {
                LogError(e, imageProvider.GetLastErrorMessage());
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

            if (!imageProvider.IsOpen)
                Thread.Sleep(timeoutOpening);

            if (!imageProvider.IsOpen)
            {
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, imageDescriptor, true, false));

                return;
            }

            try
            {
                imageProvider.OneShot(); 
                //imageProvider.ContinuousShot();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }

            waitHandle.WaitOne(timeoutGrabbing, false);

            imageProvider.GrabErrorEvent -= imageProvider_GrabErrorEvent;
            imageProvider.DeviceRemovedEvent -= imageProvider_DeviceRemovedEvent;
            imageProvider.ImageReadyEvent -= imageProvider_ImageReadyEvent;

            Stop();
            Close();

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            if (!imageProvider.IsOpen)
                return;

            cancelled = true;
            waitHandle.Set();
        }

        private void Stop()
        {
            try
            {
                imageProvider.Stop();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        private void Close()
        {
            try
            {
                imageProvider.Close();
            }
            catch (Exception e)
            {
                LogError(e, imageProvider.GetLastErrorMessage());
            }
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error received trying to get a thumbnail for {0}", summary.Alias);
            log.Error(e.ToString());
            log.Error(additionalErrorMessage);
        }

        #region Camera events
        private void imageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            LogError(grabException, additionalErrorMessage);
            
            hadError = true;
            waitHandle.Set();
        }

        private void imageProvider_DeviceRemovedEvent()
        {            
            hadError = true;
            waitHandle.Set();
        }

        private void imageProvider_ImageReadyEvent()
        {
            ImageProvider.Image pylonImage = imageProvider.GetLatestImage();

            if (pylonImage == null)
            {
                waitHandle.Set();
                return;
            }

            image = null;
            BitmapFactory.CreateBitmap(out image, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            BitmapFactory.UpdateBitmap(image, pylonImage.Buffer, pylonImage.Width, pylonImage.Height, pylonImage.Color);
            imageProvider.ReleaseImage();

            if (image != null)
            {
                int bufferSize = ImageFormatHelper.ComputeBufferSize(image.Width, image.Height, Video.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
            }

            waitHandle.Set();
        }
        #endregion


    }
}

