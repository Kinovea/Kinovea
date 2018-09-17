#region License
/*
Copyright © Joan Charmant 2014.
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
using System.Timers;
using System.Runtime.InteropServices;
using Kinovea.Services;
using Kinovea.Pipeline;
using System.Diagnostics;
using Kinovea.Video;

namespace Kinovea.Camera.FrameGenerator
{
    public class FrameGrabber : ICaptureSource
    {
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler GrabbingStatusChanged;

        #region Property
        public bool Grabbing
        {
            get { return grabbing; }
        }
        public float Framerate
        {
            get { return 0; }
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
        private FrameGeneratorDevice device;
        private bool grabbing;
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #endregion

        #region Public methods
        public FrameGrabber(CameraSummary summary)
        {
            this.summary = summary;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            // Configure according to saved preferences.
            CreateAndConfigureDevice();

            if (device == null)
                return ImageDescriptor.Invalid;

            // Read final values from device.
            int width = device.Configuration.Width;
            int height = device.Configuration.Height;
            ImageFormat format = device.Configuration.ImageFormat;
            int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            bool topDown = true;

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
            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);

            device.FrameProduced += device_FrameProduced;
            device.FrameError += device_FrameError;
            device.GrabbingStarted += device_GrabbingStarted;

            device.Start();
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.FrameProduced -= device_FrameProduced;
            device.FrameError -= device_FrameError;
            device.GrabbingStarted -= device_GrabbingStarted;

            device.Stop();

            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Configure the device according to what is saved in the preferences.
        /// </summary>
        private void CreateAndConfigureDevice()
        {
            if (grabbing)
                Stop();

            device = new FrameGeneratorDevice();

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null)
                return;

            DeviceConfiguration configuration = new DeviceConfiguration(specific.FrameSize.Width, specific.FrameSize.Height, specific.FrameInterval, ImageFormat.RGB24);
            device.Configuration = configuration;
        }

        private void ComputeDataRate(int bytes)
        {
            double rate = ((double)bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }
        #endregion

        #region device event handlers
        private void device_GrabbingStarted(object sender, EventArgs e)
        {
            grabbing = true;

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void device_FrameProduced(object sender, FrameProducedEventArgs e)
        {
            ComputeDataRate(e.PayloadLength);

            if (FrameProduced != null)
                FrameProduced(this, e);
        }

        private void device_FrameError(object sender, FrameErrorEventArgs e)
        {
            log.ErrorFormat("Error from device {0}: {1}", summary.Alias, e.Description);
        }
        #endregion

    }
}
