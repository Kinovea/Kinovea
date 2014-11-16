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

using AForge.Video;
using AForge.Video.DirectShow;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// We use whatever settings are currently configured in the camera.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
        
        public string Identifier 
        { 
            get { return this.summary.Identifier;}
        }
        
        #region Members
        private static readonly int timeout = 5000;
        private Bitmap image;
        private string moniker;
        private CameraSummary summary;
        private object locker = new object();
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private bool cancelled;
        private bool hadError;
        private VideoCaptureDevice device;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SnapshotRetriever(CameraSummary summary, string moniker)
        {
            this.moniker = moniker;
            this.summary = summary;
            
            device = new VideoCaptureDevice(moniker);
            device.NewFrame += Device_NewFrame;
            device.VideoSourceError += Device_VideoSourceError;
        }

        public void Run(object data)
        {
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);
            device.Start();
            waitHandle.WaitOne(timeout, false);
            
            device.NewFrame -= Device_NewFrame;
            device.VideoSourceError -= Device_VideoSourceError;
            device.SignalToStop();
            
            if (image == null)
                log.DebugFormat("Timeout waiting for thumbnail of {0}", summary.Alias);

            if(CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
        }
        
        public void Cancel()
        {
            cancelled = true;
            waitHandle.Set();
        }
        
        private void Device_NewFrame(object sender, NewFrameEventArgs e)
        {
            // Note: unfortunately some devices need several frames to have a usable image. (e.g: PS3 Eye).
            image = new Bitmap(e.Frame.Width, e.Frame.Height, e.Frame.PixelFormat);
            Graphics g = Graphics.FromImage(image);
            g.DrawImageUnscaled(e.Frame, Point.Empty);
            waitHandle.Set();
        }
        
        private void Device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            log.ErrorFormat("Error received trying to get a thumbnail for {0}", summary.Alias);
            log.Error(e.Description);
            
            hadError = true;
            waitHandle.Set();
        }

    }
}
