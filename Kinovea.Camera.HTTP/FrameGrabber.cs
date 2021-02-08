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
using System.Diagnostics;
using Kinovea.Pipeline;
using Kinovea.Services;
using AForge.Video;

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
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler GrabbingStatusChanged;
        
        #region Property
        public bool Grabbing
        { 
            get { return grabbing;}
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
        private CameraManagerHTTP manager;
        private ICameraHTTPClient device;
        private bool grabbing;
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private bool receivedFirstFrame;
        private string format;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FrameGrabber(CameraSummary summary, CameraManagerHTTP manager)
        {
            this.summary = summary;
            this.manager = manager;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null)
                return;

            string url = manager.BuildURL(specific);
            this.format = specific.Format;
            
            if (format == "MJPEG")
                device = new CameraHTTPClientMJPEG(url, specific.User, specific.Password);
            else if (format == "JPEG")
                device = new CameraHTTPClientJPEG(url, specific.User, specific.Password);
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            // We cannot know the stream format in advance, it must always use the two-step process.
            return ImageDescriptor.Invalid;
        }

        /// <summary>
        /// In case of configure failure, we would have retrieved a single image and the corresponding image descriptor.
        /// A limitation of the single snapshot retriever is that the format is always RGB24, even though the grabber may
        /// use a different format.
        /// </summary>
        public ImageDescriptor GetPrepareFailedImageDescriptor(ImageDescriptor input)
        {
            if (format == "MJPEG" || format == "JPEG")
                return new ImageDescriptor(ImageFormat.JPEG, input.Width, input.Height, input.TopDown, input.BufferSize);
            else
                return input;
        }

        public void Start()
        {
            // This is also used in the context of restart, where the url may have changed,
            // so it is the best place to grab the url and build the device.
            if(grabbing)
                return;
            
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
            if(device == null || !grabbing)
                return;
                
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.NewFrameBuffer -= device_NewFrameBuffer;
            device.VideoSourceError -= device_VideoSourceError;
            device.Stop();

            if (device.IsRunning)
                log.DebugFormat("Stopping device {0}", summary.Alias);
    
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
        }

        private void device_NewFrameBuffer(object sender, NewFrameBufferEventArgs e)
        {
            if (!receivedFirstFrame)
                receivedFirstFrame = true;

            ComputeDataRate(e.PayloadLength);

            if (FrameProduced != null)
                FrameProduced(this, new FrameProducedEventArgs(e.Buffer, e.PayloadLength));
        }

        private void device_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
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

        private void ComputeDataRate(int bytes)
        {
            double rate = (bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }
    }
}

