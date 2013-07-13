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

namespace Kinovea.Camera.HTTP
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
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
        private IVideoSource device;
        private string error;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public SnapshotRetriever(CameraManagerHTTP manager, CameraSummary summary)
        {
            this.summary = summary;

            string url = "";
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if(specific != null)
                url = manager.BuildURL(specific);
            
            if(specific.Format == "MJPEG")
                device = new MJPEGStream(url);
            else if(specific.Format == "JPEG")
                device = new JPEGStream(url);
        }

        public void Run(object data)
        {
            if(device == null)
                return;

            device.NewFrame += Device_NewFrame;
            device.VideoSourceError += Device_VideoSourceError;
            
            device.Start();
            waitHandle.WaitOne(5000, false);
            
            device.NewFrame -= Device_NewFrame;
            device.VideoSourceError -= Device_VideoSourceError;
            device.SignalToStop();
            
            if(!cancelled && !string.IsNullOrEmpty(error) && CameraImageError != null)
                CameraImageError(this, EventArgs.Empty);
            else if(!cancelled && image != null && CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
            else if(!cancelled && image == null && CameraImageTimedOut != null)
                CameraImageTimedOut(this, EventArgs.Empty);
        }
        
        public void Cancel()
        {
            if(device == null)
                return;

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
            log.DebugFormat("Error received");
            error = e.Description;
            waitHandle.Set();
        }

    }
}

