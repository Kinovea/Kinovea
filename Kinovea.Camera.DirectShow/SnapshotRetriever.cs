﻿#region License
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
using System.Drawing;
using System.Threading;

using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using Kinovea.Video;
using Kinovea.Pipeline;

namespace Kinovea.Camera.DirectShow
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
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private bool cancelled;
        private bool hadError;
        private string moniker;
        private VideoCaptureDevice device;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SnapshotRetriever(CameraSummary summary, string moniker)
        {
            this.moniker = moniker;
            this.summary = summary;
            
            device = new VideoCaptureDevice(moniker);
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            Thread.CurrentThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            device.NewFrameBuffer += device_NewFrameBuffer;
            device.VideoSourceError += device_VideoSourceError;

            device.Start();

            waitHandle.WaitOne(timeout, false);

            device.NewFrameBuffer -= device_NewFrameBuffer;
            device.VideoSourceError -= device_VideoSourceError;
            
            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));

            DeviceHelper.StopDevice(device);
        }
        
        public void Cancel()
        {
            cancelled = true;
            waitHandle.Set();
        }

        private void device_NewFrameBuffer(object sender, NewFrameBufferEventArgs e)
        {
            // As we didn't specify any media type, the buffer is guaranteed to come back in RGB24.
            image = new Bitmap(e.Width, e.Height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapHelper.FillFromRGB24(image, rect, false, e.Buffer);
            imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, true, ImageFormatHelper.ComputeBufferSize(image.Width, image.Height, Video.ImageFormat.RGB24));

            waitHandle.Set();
        }
        
        private void device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            log.ErrorFormat("Error received trying to get a thumbnail for {0}", summary.Alias);
            log.Error(e.Description);
            
            hadError = true;
            waitHandle.Set();
        }
    }
}
