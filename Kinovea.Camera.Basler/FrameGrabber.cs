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
using System.Windows.Forms;

using PylonC.NET;
using PylonC.NETSupportLibrary;
using Kinovea.Base;
using Kinovea.Services;
using Kinovea.Pipeline;
using Kinovea.Video;
using System.Diagnostics;

namespace Kinovea.Camera.Basler
{
    /// <summary>
    /// The main grabbing class for Basler devices.
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
            get { return Size.Empty; }
        }

        public float Framerate
        {
            get { return 30; }
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
        private uint deviceIndex;
        private PYLON_DEVICE_HANDLE deviceHandle;
        private ImageProvider device;
        private bool grabbing;
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
    
        public FrameGrabber(CameraSummary summary, uint deviceIndex)
        {
            this.summary = summary;
            this.deviceIndex = deviceIndex;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            ConfigureDevice();

            if (device == null || !deviceHandle.IsValid)
                return ImageDescriptor.Invalid;

            CameraPropertyManager.ReadIntegerValue(deviceHandle, "Width");

            bool hasWidth = Pylon.DeviceFeatureIsReadable(deviceHandle, "Width");
            bool hasHeight = Pylon.DeviceFeatureIsReadable(deviceHandle, "Height");

            if (!hasWidth || !hasHeight)
                return ImageDescriptor.Invalid;

            int width = (int)Pylon.DeviceGetIntegerFeature(deviceHandle, "Width");
            int height = (int)Pylon.DeviceGetIntegerFeature(deviceHandle, "Height");
            string pixelFormat = Pylon.DeviceFeatureToString(deviceHandle, "PixelFormat");

            // The ImageProvider will perform pixel format conversion and only output either Y800 or RGB24.
            // Notably:
            // - Y16 will be converted to Y8.
            // - Bayer pattern will be converted to color image.
            EPylonPixelType pixelType = Pylon.PixelTypeFromString(pixelFormat);
            if (pixelType == EPylonPixelType.PixelType_Undefined)
                return ImageDescriptor.Invalid;

            bool monochrome = Pylon.IsMono(pixelType) && !Pylon.IsBayer(pixelType);
            ImageFormat format = monochrome ? format = ImageFormat.Y800 : ImageFormat.RGB24;
            
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
            
            device.ImageReadyEvent += ImageProvider_ImageReadyEvent;
            device.GrabErrorEvent += ImageProvider_GrabErrorEvent;
            device.GrabbingStartedEvent += ImageProvider_GrabbingStartedEvent;
            
            device.Continuous();
        }
        
        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            device.ImageReadyEvent -= ImageProvider_ImageReadyEvent;
            device.GrabErrorEvent -= ImageProvider_GrabErrorEvent;
            device.GrabbingStartedEvent -= ImageProvider_GrabbingStartedEvent;
            Close();
            
            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        private void Close()
        {
            SpecificInfo specific = summary.Specific as SpecificInfo;
            specific.Handle = null;
            device.Close();
        }
        
        private void SoftwareTrigger()
        {
            if(!grabbing)
                return;
                
            device.Trigger();
        }

        /// <summary>
        /// Configure the device according to what is saved in the preferences.
        /// </summary>
        private void ConfigureDevice()
        {
            if(grabbing)
                Stop();
                
            CreateDevice();
            if(device == null || !deviceHandle.IsValid)
                return;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if(specific == null)
                return;

            specific.Handle = deviceHandle;

            if (specific.StreamFormat != null)
                Pylon.DeviceFeatureFromString(deviceHandle, "PixelFormat", specific.StreamFormat);

            foreach (CameraProperty property in specific.CameraProperties.Values)
                CameraPropertyManager.Write(deviceHandle, property);
        }
        
        private void CreateDevice()
        {
            device = new ImageProvider();

            try
            {
                deviceHandle = Pylon.CreateDeviceByIndex(deviceIndex);
                device.Open(deviceHandle);
            }
            catch (Exception)
            {
                log.Error(PylonHelper.GetLastError());
            }
        }
        
        private void ImageProvider_GrabbingStartedEvent()
        {
            grabbing = true;

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }
        
        private void ImageProvider_ImageReadyEvent()
        {
            // Consume the Pylon queue (no copy).
            ImageProvider.Image pylonImage = device.GetLatestImage();
            if (pylonImage == null)
                return;

            ComputeDataRate(pylonImage.Buffer.Length);

            if (FrameProduced != null)
                FrameProduced(this, new FrameProducedEventArgs(pylonImage.Buffer, pylonImage.Buffer.Length));

            device.ReleaseImage();
        }

        private void ImageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            log.ErrorFormat("Error from device {0}: {1}", summary.Alias, additionalErrorMessage);
        }

        private void ComputeDataRate(int bytes)
        {
            double rate = ((double)bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }
    }
}
