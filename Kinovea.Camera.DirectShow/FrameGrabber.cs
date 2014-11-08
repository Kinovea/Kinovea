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
using System.Linq;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Collections.Generic;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// The main grabbing class for devices with a DirectShow interface.
    /// </summary>
    public class FrameGrabber : IFrameGrabber
    {
        public event EventHandler<CameraImageReceivedEventArgs> CameraImageReceived;
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
            get 
            {
                if (device.VideoResolution != null)
                    return device.VideoResolution.AverageFrameRate;
                else
                    return 30F;
            }
        }
        #endregion
        
        #region Members
        private string moniker;
        private VideoCaptureDevice device;
        private List<MediaType> mediaTypes;
        private CameraSummary summary;
        private bool grabbing;
        private Size actualSize;
        private Bitmap image;
        private object locker = new object();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary, string moniker)
        {
            this.moniker = moniker;
            this.summary = summary;
            device = new VideoCaptureDevice(moniker);
            mediaTypes = MediaTypeImporter.Import(device);
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            
            ConfigureDevice();
            
            device.NewFrame += Device_NewFrame;
            device.VideoSourceError += Device_VideoSourceError;
            grabbing = true;
            device.Start();
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.NewFrame -= Device_NewFrame;
            device.VideoSourceError -= Device_VideoSourceError;
            device.Stop();
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void ConfigureDevice()
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null || info.MediaType == null)
            {
                log.DebugFormat("The device has never been configured in Kinovea. Use the current configuration.");
                return;
            }

            // We have a configuration on record. Find the corresponding VideoCapability and apply it.
            MediaType mt = info.MediaType;
            VideoCapabilities[] capabilities = device.VideoCapabilities;
            VideoCapabilities match = capabilities.FirstOrDefault(c => c.Index == mt.MediaTypeIndex);
            if (match == null)
            {
                log.ErrorFormat("Could not match the saved stream configuration.");
                return;
            }

            device.VideoResolution = match;
            log.DebugFormat("Device set to saved configuration: {0}.", mt.ToString());
        }
        
        private void Device_NewFrame(object sender, NewFrameEventArgs e)
        {
            // TODO: see if unsafe deep copy from AForge is faster.
            //log.DebugFormat("New frame received, size:{0}", e.Frame.Size);
            
            int finalHeight = SetFinalHeight(e.Frame.Width, e.Frame.Height);
            actualSize = new Size(e.Frame.Width, finalHeight);
            bool anamorphic = e.Frame.Height != finalHeight;
            
            image = new Bitmap(e.Frame.Width, finalHeight, e.Frame.PixelFormat);
            Graphics g = Graphics.FromImage(image);
            if(!anamorphic)
                g.DrawImageUnscaled(e.Frame, Point.Empty);
            else
                g.DrawImage(e.Frame, 0, 0, e.Frame.Width, finalHeight);
            
            if(CameraImageReceived != null)
                CameraImageReceived(this, new CameraImageReceivedEventArgs(summary, image));
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
                default:
                    // Hack for DV rectangular pixels. Force 4:3 image ratio by default.
                    if(width == 720 && height == 576)
                        finalHeight = 540;
                    break;
            }
            
            return finalHeight;
        }
    }
}
