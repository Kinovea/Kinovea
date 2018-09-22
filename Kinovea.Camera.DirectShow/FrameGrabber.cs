#region License
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
using System.Linq;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Collections.Generic;
using Kinovea.Services;
using Kinovea.Pipeline;
using Kinovea.Video;
using Kinovea.Base;
using System.Diagnostics;
using System.Globalization;

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
        public double LiveDataRate
        {
            // Note: this variable is written by the stream thread and read by the UI thread.
            // We don't lock because freshness of values is not paramount and torn reads are not catastrophic either.
            // We eventually get an approximate value good enough for the purpose.
            get { return dataRateAverager.Average; }
        }
        #endregion
        
        #region Members
        private CameraSummary summary;
        private string moniker;
        private VideoCaptureDevice device;
        private bool grabbing;
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private bool receivedFirstFrame;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary, string moniker)
        {
            this.summary = summary;
            this.moniker = moniker;
            
            device = new VideoCaptureDevice(moniker);
            device.SetDirectConnectFormats(new List<string>() { "RGB24", "MJPG" });
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            ConfigureDevice();

            AForge.Video.DirectShow.VideoCapabilities cap = null;
            if (device.VideoResolution == null)
            {
                // This device was never connected to in Kinovea, use the first media type.
                AForge.Video.DirectShow.VideoCapabilities[] caps = device.VideoCapabilities;
                if (caps.Length == 0)
                {
                    log.ErrorFormat("Cannot get any media type for the device.");
                    return ImageDescriptor.Invalid;
                }

                cap = caps[0];

                device.SetMediaTypeAndFramerate(cap.Index, (float)cap.AverageFrameRate);
                log.DebugFormat("Device set to default configuration: Index:{0}. ({1}×{2} @ {3:0.###} fps ({4})).",
                    cap.Index, cap.FrameSize.Width, cap.FrameSize.Height, cap.AverageFrameRate, cap.Compression);
            }
            else
            {
                cap = device.VideoResolution;
            }

            int width = cap.FrameSize.Width;
            int height = cap.FrameSize.Height;

            ImageFormat format = ImageFormat.RGB24;

            switch (cap.Compression)
            {
                case "RGB24":
                default:
                    format = ImageFormat.RGB24;
                    break;
                case "MJPG":
                    format = ImageFormat.JPEG;
                    break;
            }

            int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            bool topDown = false;

            return new ImageDescriptor(format, width, height, topDown, bufferSize);
        }

        /// <summary>
        /// In case of configure failure, we would have retrieved a single image and the corresponding image descriptor.
        /// A limitation of the single snapshot retriever is that the format is always RGB24, even though the grabber may
        /// use a different format.
        /// </summary>
        public ImageDescriptor GetPrepareFailedImageDescriptor(ImageDescriptor input)
        {
            return input;
        }

        public void Start()
        {
            log.DebugFormat("Starting device {0}.", summary.Alias);
            
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

            log.DebugFormat("Stopping device {0}.", summary.Alias);
            device.NewFrameBuffer -= device_NewFrameBuffer;
            device.VideoSourceError -= device_VideoSourceError;

            DeviceHelper.StopDevice(device);
            log.DebugFormat("{0} stopped.", summary.Alias);
            
            receivedFirstFrame = false;
            
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
        }

        /// <summary>
        /// Configure the device according to what is saved in the preferences for it.
        /// </summary>
        private void ConfigureDevice()
        {
            SpecificInfo info = summary.Specific as SpecificInfo;
            if (info == null || info.MediaTypeIndex < 0)
            {
                log.DebugFormat("No configuration saved in preferences for this device.");
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

            log.DebugFormat("Device set to saved configuration: Index:{0}. ({1}×{2} @ {3:0.###} fps ({4})).", 
                info.MediaTypeIndex, match.FrameSize.Width, match.FrameSize.Height, info.SelectedFramerate, match.Compression);

            // Reload camera properties in case the firmware "forgot" them.
            // This means changes done in other softwares will be overwritten.
            try
            {
                CameraPropertyManager.Write(device, info.CameraProperties);
            }
            catch
            {
                log.ErrorFormat("An error occured while reloading camera properties.");
            }
        }
        
        private void device_NewFrameBuffer(object sender, NewFrameBufferEventArgs e)
        {
            if (!receivedFirstFrame)
            {
                SetPostConnectionOptions();
                receivedFirstFrame = true;
            }

            ComputeDataRate(e.PayloadLength);

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
            // For example logitech exposure. Probably due to a bug in Logitech firmware.
            SpecificInfo info = summary.Specific as SpecificInfo;

            // Only do this for Logitech devices.
            if (!summary.Identifier.Contains("usb#vid_046d") || info == null)
                return;

            if (info.CameraProperties.ContainsKey("exposure_logitech"))
            {
                if (info.CameraProperties["exposure_logitech"].Automatic)
                    return;

                int exposure = int.Parse(info.CameraProperties["exposure_logitech"].CurrentValue, CultureInfo.InvariantCulture);
                device.Logitech_SetExposure(exposure, true);
            }
            else if (info.CameraProperties.ContainsKey("exposure"))
            {
                if (info.CameraProperties["exposure"].Automatic)
                    return;

                int exposure = int.Parse(info.CameraProperties["exposure"].CurrentValue, CultureInfo.InvariantCulture);
                device.SetCameraProperty(CameraControlProperty.Exposure, exposure, CameraControlFlags.Manual);
            }
        }

        private void ComputeDataRate(int bytes)
        {
            double rate = (bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }
    }
}
