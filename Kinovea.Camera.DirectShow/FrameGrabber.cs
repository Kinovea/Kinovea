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
using Kinovea.Services;
using Kinovea.Pipeline;
using Kinovea.Video;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// The main grabbing class for devices with a DirectShow interface.
    /// </summary>
    public class FrameGrabber : ICaptureSource
    {
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler GrabbingStatusChanged;
        
        #region Property
        public bool Grabbing
        { 
            get { return grabbing;}
        }
        
        public Size Size
        {
            get 
            { 
                return Size.Empty; 
            }
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
        private CameraSummary summary;
        private string moniker;
        private VideoCaptureDevice device;
        private bool grabbing;
        private bool receivedFirstFrame;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary, string moniker)
        {
            this.summary = summary;
            this.moniker = moniker;
            device = new VideoCaptureDevice(moniker);
        }

        public ImageDescriptor Prepare()
        {
            ConfigureDevice();

            AForge.Video.DirectShow.VideoCapabilities cap = null;
            if (device.VideoResolution == null)
            {
                // This device was never connected to in Kinovea, use the first media type.
                AForge.Video.DirectShow.VideoCapabilities[] caps = device.VideoCapabilities;
                cap = caps[0];
            }
            else
            {
                cap = device.VideoResolution;
            }

            int width = cap.FrameSize.Width;
            int height = cap.FrameSize.Height;
            
            // RGB24 is the only supported format until the pipeline is fully integrated.
            ImageFormat format = ImageFormat.RGB24;
            int bufferSize = width * height * 3;

            return new ImageDescriptor(format, width, height, bufferSize);
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);
            
            device.NewFrameBuffer += device_NewFrameBuffer;
            device.VideoSourceError += device_VideoSourceError;
            grabbing = true;
            device.Start();

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (!grabbing)
                return;

            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.NewFrameBuffer -= device_NewFrameBuffer;
            device.VideoSourceError -= device_VideoSourceError;
            
            device.SignalToStop();
            device.WaitForStop();

            receivedFirstFrame = false;
            
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void ConfigureDevice()
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null || info.MediaTypeIndex < 0)
            {
                log.DebugFormat("The device has never been configured in Kinovea.");
                return;
            }

            // Initialize device configuration (Extract and cache media types on the output pin).
            // Double check we have an existing index and set the format.
            AForge.Video.DirectShow.VideoCapabilities[] capabilities = device.VideoCapabilities;
            AForge.Video.DirectShow.VideoCapabilities match = capabilities.FirstOrDefault(c => c.Index == info.MediaTypeIndex);
            if (match == null)
            {
                log.ErrorFormat("Could not match the saved media type.");
                return;
            }

            device.SetMediaTypeAndFramerate(info.MediaTypeIndex, info.SelectedFramerate);

            log.DebugFormat("Device set to saved configuration: {0}.", info.MediaTypeIndex);
        }
        
        private void device_NewFrameBuffer(object sender, NewFrameBufferEventArgs e)
        {
            if (!receivedFirstFrame)
            {
                SetPostConnectionOptions();
                receivedFirstFrame = true;
            }

            if (FrameProduced != null)
                FrameProduced(this, new FrameProducedEventArgs(e.Buffer, e.PayloadLength));
        }
        
        private void device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
        {
            log.ErrorFormat("Error from device {0}: {1}", summary.Alias, e.Description);
        }

        private void SetPostConnectionOptions()
        {
            // Some options only work after the graph is actually connected.
            // For example exposure time. It might be due to a bug in Logitech drivers though.
            SpecificInfo info = summary.Specific as SpecificInfo;
            
            if (info == null || !info.HasExposureControl || !info.ManualExposure)
                return;

            if (info.UseLogitechExposure)
                device.Logitech_SetExposure((int)info.ExposureValue, true);
            else
                device.SetCameraProperty(CameraControlProperty.Exposure, (int)info.ExposureValue, CameraControlFlags.Manual);
        }
    }
}
