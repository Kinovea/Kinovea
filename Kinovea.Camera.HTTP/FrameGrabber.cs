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
using AForge.Video;
using Kinovea.Services;

namespace Kinovea.Camera.HTTP
{
    /// <summary>
    /// The main grabbing class for devices connectable through HTTP.
    /// Note: the code looks very much like the DirectShow grabber, this is because
    /// they both use AForge to connect to the device. However it is 
    /// an implementation detail, so we don't factorize the code.
    /// </summary>
    public class FrameGrabber : ICaptureSource
    {
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
        
        public float Framerate
        {
            get { return 0;}
        }
        /*public string ErrorDescription
        {
            get { return errorDescription;}
        }*/
        #endregion
        
        #region Members
        private Bitmap image;
        private CameraSummary summary;
        private CameraManagerHTTP manager;
        private object locker = new object();
        private IVideoSource device;
        private bool grabbing;
        private Size actualSize;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraManagerHTTP manager, CameraSummary summary)
        {
            this.manager = manager;
            this.summary = summary;
        }

        public void Start()
        {
            // This is also used in the context of restart, where the url may have changed,
            // so it is the best place to grab the url and build the device.
            if(grabbing)
                return;
            
            string url = "";
            SpecificInfo specific = summary.Specific as SpecificInfo;
            if(specific == null)
                return;
                
            url = manager.BuildURL(specific);
                
            if(specific.Format == "MJPEG")
                device = new MJPEGStream(url);
            else if(specific.Format == "JPEG")
                device = new JPEGStream(url);
            
            if(device == null)
                return;
            
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            device.NewFrame += Device_NewFrame;
            device.VideoSourceError += Device_VideoSourceError;
            grabbing = true;
            device.Start();
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if(device == null)
                return;
                
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.NewFrame -= Device_NewFrame;
            device.VideoSourceError -= Device_VideoSourceError;
            device.Stop();
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }
        
        private void Device_NewFrame(object sender, NewFrameEventArgs e)
        {
            // WIP: use frame buffer event and send byte[] to client.

            /*int finalHeight = SetFinalHeight(e.Frame.Width, e.Frame.Height);
            bool anamorphic = e.Frame.Height != finalHeight;
            actualSize = new Size(e.Frame.Width, finalHeight);
            
            image = new Bitmap(e.Frame.Width, finalHeight, e.Frame.PixelFormat);
            Graphics g = Graphics.FromImage(image);
            if(!anamorphic)
                g.DrawImageUnscaled(e.Frame, Point.Empty);
            else
                g.DrawImage(e.Frame, 0, 0, e.Frame.Width, finalHeight);
            
            if(CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image, false, false));*/
        }
        
        private void Device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            log.ErrorFormat("Error from device {0}: {1}", summary.Alias, e.Description);
        }
        
        private int SetFinalHeight(int width, int height)
        {
            int finalHeight = height;
            
            switch(summary.AspectRatio)
            {
                case CaptureAspectRatio.Force43:
                    finalHeight = (int)((width / 4.0) * 3);
                    break;
                case CaptureAspectRatio.Force169:
                    finalHeight = (int)((width / 16.0) * 9);
                    break;
            }
            
            return finalHeight;
        }
    }
}

