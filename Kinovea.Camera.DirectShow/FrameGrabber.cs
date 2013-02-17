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
using AForge.Video.DirectShow;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// The main grabbing class for devices with a DirectShow interface.
    /// </summary>
    public class FrameGrabber : IFrameGrabber
    {
        public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
        
        #region Property
        public bool Grabbing
        { 
            get { return grabbing;}
        }
        
        public Size Size
        {
            get { return device.DesiredFrameSize; }
        }
        
        public float Framerate
        {
            get { return device.DesiredFrameRate;}
        }
        /*public string ErrorDescription
        {
            get { return errorDescription;}
        }*/
        #endregion
        
        #region Members
        private Bitmap image;
        private string moniker;
        private CameraSummary summary;
        private object locker = new object();
        private VideoCaptureDevice device;
        private bool grabbing;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary, string moniker)
        {
            this.moniker = moniker;
            this.summary = summary;
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            CreateDevice();
            device.NewFrame += Device_NewFrame;
            device.VideoSourceError += Device_VideoSourceError;
            grabbing = true;
            device.Start();
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.NewFrame -= Device_NewFrame;
            device.VideoSourceError -= Device_VideoSourceError;
            device.Stop();
            grabbing = false;
        }
        
        private void CreateDevice()
        {
            device = new VideoCaptureDevice(moniker);
            
            SpecificInfo info = summary.Specific as SpecificInfo;
            if(info != null && info.SelectedCapability != null)
            {
                device.DesiredFrameSize = info.SelectedCapability.FrameSize;
                device.DesiredFrameRate = info.SelectedCapability.FrameRate;
                log.DebugFormat("Device desired configuration: {0} @ {1} fps", device.DesiredFrameSize, device.DesiredFrameRate);
            }
        }
        
        private void Device_NewFrame(object sender, NewFrameEventArgs e)
        {
            // TODO: see if unsafe deep copy from AForge is faster.
            image = new Bitmap(e.Frame.Width, e.Frame.Height, e.Frame.PixelFormat);
            Graphics g = Graphics.FromImage(image);
            g.DrawImageUnscaled(e.Frame, Point.Empty);
            
            if(CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
        }
        
        private void Device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            log.DebugFormat("Error from device {0}: {1}", summary.Alias, e.Description);
        }
    }
}
