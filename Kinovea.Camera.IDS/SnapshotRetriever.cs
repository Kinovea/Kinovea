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
using System.Drawing.Imaging;
using System.Threading;
using Kinovea.Pipeline;
using Kinovea.Video;

namespace Kinovea.Camera.IDS
{
    /// <summary>
    /// Retrieve a single snapshot, simulating a synchronous function. Used for thumbnails.
    /// We use whatever settings are currently configured in the camera.
    /// </summary>
    public class SnapshotRetriever
    {
        public event EventHandler<CameraThumbnailProducedEventArgs> CameraThumbnailProduced;

        public string Identifier
        {
            get { return this.summary.Identifier; }
        }

        #region Members
        private static readonly int timeoutGrabbing = 5000;
        private static readonly int timeoutOpening = 100;

        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private uEye.Camera camera = new uEye.Camera();
        private bool cancelled;
        private bool hadError;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary, long deviceId)
        {
            this.summary = summary;

            camera.EventFrame += camera_EventFrame;
            camera.EventDeviceRemove += camera_EventDeviceRemove;
            camera.EventDeviceUnPlugged += camera_EventDeviceUnPlugged;

            try
            {
                uEye.Defines.Status status = camera.Init((Int32)deviceId | (Int32)uEye.Defines.DeviceEnumeration.UseDeviceID);

                if (status != uEye.Defines.Status.SUCCESS)
                {
                    log.ErrorFormat("Camera {0} could not be opened for thumbnail capture.", summary.Alias);
                    return;
                }

                // We do not load the camera-specific profile for the thumbnail at the moment.
                // For some reason the .ToBitmap method doesn't work well on the RGB32 format, so in order to at least have something we 
                // load the camera on the default profile for the thumbnail.
                //ProfileHelper.Load(camera, ProfileHelper.GetProfileFilename(summary.Identifier));

                status = camera.Memory.Allocate();
                if (status != uEye.Defines.Status.SUCCESS)
                {
                    log.ErrorFormat("Camera {0} could not have its buffer allocated for thumbnail capture.", summary.Alias); 
                    return;
                }
            }
            catch (Exception e) 
            {
                LogError(e, "");
            }
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            Thread.CurrentThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            if (!camera.IsOpened)
                Thread.Sleep(timeoutOpening);

            if (!camera.IsOpened)
            {
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, ImageDescriptor.Invalid, true, false));

                return;
            }

            try
            {
                uEye.Defines.Status statusRet = camera.Acquisition.Capture();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }

            waitHandle.WaitOne(timeoutGrabbing, false);

            camera.EventFrame -= camera_EventFrame;
            camera.EventDeviceRemove -= camera_EventDeviceRemove;
            camera.EventDeviceUnPlugged -= camera_EventDeviceUnPlugged;
            
            Stop();
            Close();
            log.DebugFormat("Camera {0} closed after thumbnail capture.", summary.Alias);

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            if (!camera.IsOpened)
                return;

            cancelled = true;
            waitHandle.Set();
        }

        private void Stop()
        {
            try
            {
                camera.Acquisition.Freeze();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }
        }

        private void Close()
        {
            try
            {
                camera.Exit();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Camera {0} failure during thumbnail capture.", summary.Alias);
            log.Error(e.ToString());

            if (!string.IsNullOrEmpty(additionalErrorMessage))
                log.Error(additionalErrorMessage);
        }

        #region Camera events
        private void camera_EventFrame(object sender, EventArgs e)
        {
            uEye.Camera camera = sender as uEye.Camera;

            if (camera == null || !camera.IsOpened)
                return;

            uEye.Defines.DisplayMode mode;
            camera.Display.Mode.Get(out mode);

            if (mode != uEye.Defines.DisplayMode.DiB)
                return;

            Int32 s32MemID;
            camera.Memory.GetActive(out s32MemID);
            camera.Memory.Lock(s32MemID);

            Bitmap bitmap;
            camera.Memory.ToBitmap(s32MemID, out bitmap);

            if (bitmap != null)
            {
                if (image != null)
                    image.Dispose();

                // Force output into an RGB24 bitmap.
                image = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

                using (Graphics g = Graphics.FromImage(image))
                {
                    g.DrawImage(bitmap, Point.Empty);
                }

                int bufferSize = ImageFormatHelper.ComputeBufferSize(image.Width, image.Height, Video.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);

                bitmap.Dispose();
            }
            
            camera.Memory.Unlock(s32MemID);

            waitHandle.Set();
        }

        private void camera_EventDeviceUnPlugged(object sender, EventArgs e)
        {
            hadError = true;
            log.ErrorFormat("Camera {0} unplugged during thumbnail capture.", summary.Alias);
            waitHandle.Set();
        }

        private void camera_EventDeviceRemove(object sender, EventArgs e)
        {
            hadError = true;
            log.ErrorFormat("Camera {0} removed during thumbnail capture.", summary.Alias);
            waitHandle.Set();
        }
        #endregion
    }
}

