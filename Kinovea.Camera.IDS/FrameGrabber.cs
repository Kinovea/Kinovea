#region License
/*
Copyright © Joan Charmant 2017.
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
using Kinovea.Base;
using System.IO;

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// The main grabbing class for IDS uEye devices.
    /// </summary>
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
            get { return resultingFramerate; }
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
        private uEye.Camera camera = new uEye.Camera();
        private long deviceId;
        private bool grabbing;
        private bool firstOpen = true;
        private float resultingFramerate = 0;
        private Finishline finishline = new Finishline();
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private int incomingBufferSize = 0;
        private byte[] incomingBuffer;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public methods
        public FrameGrabber(CameraSummary summary, long deviceId)
        {
            this.summary = summary;
            this.deviceId = deviceId;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            Open();
            
            if (!camera.IsOpened)
                return ImageDescriptor.Invalid;

            firstOpen = false;

            ImageFormat format = IDSHelper.GetImageFormat(camera);
            
            // FIXME: Force a supported format if the current one is unsuitable.
            if (format == ImageFormat.None)
                return ImageDescriptor.Invalid;

            // FIXME: RGB24 should allocate buffers aligned to 4 bytes.
            // It is usually the case because none of the UI let the user choose a non aligned width.
            Rectangle rect;
            camera.Size.AOI.Get(out rect);
            incomingBufferSize = ImageFormatHelper.ComputeBufferSize(rect.Width, rect.Height, format);
            incomingBuffer = new byte[incomingBufferSize];

            resultingFramerate = IDSHelper.GetFramerate(camera);
            int width = rect.Width;
            int height = rect.Height;

            finishline.Prepare(width, height, format, resultingFramerate);
            if (finishline.Enabled)
            {
                height = finishline.Height;
                resultingFramerate = finishline.ResultingFramerate;
            }
            
            int outgoingBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            bool topDown = true;

            resultingFramerate = IDSHelper.GetFramerate(camera);
            
            return new ImageDescriptor(format, width, height, topDown, outgoingBufferSize);
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
            if (!camera.IsOpened)
                Open();

            if (!camera.IsOpened)
                return;

            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);

            camera.EventFrame += camera_EventFrame;
            camera.EventDeviceRemove += camera_EventDeviceRemove;
            camera.EventDeviceUnPlugged += camera_EventDeviceUnPlugged;
            
            try
            {
                camera.Acquisition.Capture();
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error trying to start {0}.", summary.Alias);
                log.Error(e.Message);
            }
        }

        public void Stop()
        {
            log.DebugFormat("Stopping device {0}", summary.Alias);
            
            camera.EventFrame -= camera_EventFrame;
            camera.EventDeviceRemove -= camera_EventDeviceRemove;
            camera.EventDeviceUnPlugged -= camera_EventDeviceUnPlugged;

            try
            {
                camera.Acquisition.Stop();
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
            Stop();

            try
            {
                camera.Exit();
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }
        #endregion

        #region Private methods

        private void Open()
        {
            // Unlike in the DirectShow module, we do not backup and restore camera configuration.
            // If the user configured the camera outside of Kinovea we respect the new settings.
            // Two reasons:
            // 1. In DirectShow we must do the backup/restore to work around drivers that inadvertently reset the camera properties.
            // 2. Industrial cameras have many properties that won't be configurable in Kinovea 
            // so the user is more likely to configure the camera from the outside.

            if (grabbing)
                Stop();

            try
            {
                uEye.Defines.Status status = camera.Init((Int32)deviceId | (Int32)uEye.Defines.DeviceEnumeration.UseDeviceID);

                if (status != uEye.Defines.Status.SUCCESS)
                {
                    log.ErrorFormat("Error trying to open IDS uEye camera for snapshot.");
                    return;
                }

                // Load parameter set.
                ProfileHelper.Load(camera, summary.Identifier);
            }
            catch (Exception e)
            {
                log.Error("Could not open IDS uEye camera.", e);
                return;
            }

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null)
                return;

            // Store the camera object into the specific info so that we can retrieve device informations from the configuration dialog.
            specific.Camera = camera;

            int currentColorMode = IDSHelper.ReadCurrentStreamFormat(camera);

            // Some properties can only be changed when the camera is opened but not streaming. Now is the time.
            // We store them in the summary when coming back from FormConfiguration, and we write them to the camera here.
            // Only do this if it's not the first time we open the camera, to respect any change that could have been done outside Kinovea.
            if (firstOpen)
            {
                specific.StreamFormat = currentColorMode;
            }
            else
            {
                if (specific.StreamFormat != currentColorMode)
                    IDSHelper.WriteStreamFormat(camera, specific.StreamFormat);

                CameraPropertyManager.WriteCriticalProperties(camera, specific.CameraProperties);

                // Save parameter set.
                ProfileHelper.Save(camera, ProfileHelper.GetProfileFilename(summary.Identifier));
            }
            
            // Reallocate IDS internal buffers after changing the format.
            Int32[] memList;
            camera.Memory.GetList(out memList);
            camera.Memory.Free(memList);
            camera.Memory.Allocate();

            int memId;
            camera.Memory.GetActive(out memId);

            int width, height, bitsPerPixel, pitch;
            camera.Memory.Inquire(memId, out width, out height, out bitsPerPixel, out pitch);
            
            log.DebugFormat("IDS internal buffers allocated: {0}x{1}, {2} bits per pixel, pitch:{3}.", width, height, bitsPerPixel, pitch);
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
        private unsafe void camera_EventFrame(object sender, EventArgs e)
        {
            uEye.Camera camera = sender as uEye.Camera;

            if (camera == null || !camera.IsOpened)
                return;

            uEye.Defines.DisplayMode mode;
            camera.Display.Mode.Get(out mode);

            if (mode != uEye.Defines.DisplayMode.DiB)
                return;

            int memId;
            camera.Memory.GetActive(out memId);
            camera.Memory.Lock(memId);

            System.IntPtr ptrSrc;
            camera.Memory.ToIntPtr(memId, out ptrSrc);

            fixed (byte* p = incomingBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                camera.Memory.CopyImageMem(ptrSrc, memId, ptrDst);
            }

            if (finishline.Enabled)
            {
                bool flush = finishline.Consolidate(incomingBuffer);
                if (flush)
                {
                    ComputeDataRate(finishline.BufferOutput.Length);

                    if (FrameProduced != null)
                        FrameProduced(this, new FrameProducedEventArgs(finishline.BufferOutput, finishline.BufferOutput.Length));
                }
            }
            else
            {
                ComputeDataRate(incomingBufferSize);

                if (FrameProduced != null)
                    FrameProduced(this, new FrameProducedEventArgs(incomingBuffer, incomingBufferSize));
            }
            
            camera.Memory.Unlock(memId);
        }

        private void camera_EventDeviceUnPlugged(object sender, EventArgs e)
        {
        }

        private void camera_EventDeviceRemove(object sender, EventArgs e)
        {
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error during IDS uEye camera operation. {0}", summary.Alias);
            log.Error(e.ToString());

            if (!string.IsNullOrEmpty(additionalErrorMessage))
                log.Error(additionalErrorMessage);
        }
        #endregion

    }
}
