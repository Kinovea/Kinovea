#region License
/*
Copyright © Joan Charmant 2020.
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
using System.Threading;
using Kinovea.Pipeline;
using Kinovea.Video;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using BGAPI2;
using System.Collections.Generic;
using System.Linq;

namespace Kinovea.Camera.Baumer
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

        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private BaumerProvider baumerProvider = new BaumerProvider();
        private bool cancelled;
        private bool hadError;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary)
        {
            this.summary = summary;
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            Thread.CurrentThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            SpecificInfo specific = summary.Specific as SpecificInfo;
            bool opened = baumerProvider.Open(specific.SystemKey, specific.InterfaceKey, specific.DeviceKey);

            if (!opened)
            {
                log.DebugFormat("Could not open {0} for thumbnail.", summary.Alias);
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, ImageDescriptor.Invalid, true, false));

                return;
            }

            baumerProvider.BufferProduced += BaumerProducer_BufferProduced;

            // Do not use JPEG compression for the thumbnail.
            bool wasJpegEnabled = BaumerHelper.GetJPEG(baumerProvider.Device);
            if (wasJpegEnabled)
                BaumerHelper.SetJPEG(baumerProvider.Device, false);

            try
            {
                baumerProvider.AcquireOne();
            }
            catch (Exception e)
            {
                hadError = true;
                LogError(e, null);
            }

            if (!hadError)
                waitHandle.WaitOne(timeoutGrabbing, false);

            baumerProvider.BufferProduced -= BaumerProducer_BufferProduced;
            
            baumerProvider.Stop();

            if (wasJpegEnabled)
                BaumerHelper.SetJPEG(baumerProvider.Device, true);

            Close();
            log.DebugFormat("{0} closed.", summary.Alias);

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            if (!baumerProvider.IsOpen)
                return;

            cancelled = true;
            waitHandle.Set();
        }

        private void Close()
        {
            if (baumerProvider == null || !baumerProvider.IsOpen)
                return;

            baumerProvider.Close();
        }

        #region Camera events

        private void BaumerProducer_BufferProduced(object sender, BufferEventArgs e)
        {
            BGAPI2.Buffer buffer = e.Buffer;
            if (buffer == null)
            {
                hadError = true;
                waitHandle.Set();
                return;
            }

            image = new Bitmap((int)buffer.Width, (int)buffer.Height, PixelFormat.Format24bppRgb);
            bool filled = FillRGB24(e.Buffer, image);
            if (filled)
            {
                int bufferSize = ImageFormatHelper.ComputeBufferSize(image.Width, image.Height, Video.ImageFormat.RGB24);
                bool topDown = true;
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, topDown, bufferSize);
            }

            waitHandle.Set();
        }
        #endregion

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Camera {0} failure during thumbnail capture.", summary.Alias);
            log.Error(e.Message);

            if (!string.IsNullOrEmpty(additionalErrorMessage))
                log.Error(additionalErrorMessage);
        }

        /// <summary>
        /// Takes a raw buffer and copy it into an existing RGB24 Bitmap.
        /// </summary>
        public unsafe bool FillRGB24(BGAPI2.Buffer buffer, Bitmap outputImage)
        {
            if (buffer == null || buffer.IsIncomplete || buffer.MemPtr == IntPtr.Zero)
                return false;
                
            if (buffer.Width != (ulong)outputImage.Width || buffer.Height != (ulong)outputImage.Height)
                return false;

            bool filled = false;

            // If the input image is a bayer pattern it will be debayered by default by the image processor.
            // If it's Mono it will be converted to RGB by the image processor.
            // The input image cannot be JPEG because we temporarily switch off that option before acquisition.
            BGAPI2.ImageProcessor imgProcessor = new BGAPI2.ImageProcessor();
            BGAPI2.Image img = imgProcessor.CreateImage((uint)buffer.Width, (uint)buffer.Height, buffer.PixelFormat, buffer.MemPtr, buffer.SizeFilled);
            BGAPI2.Image transformedImage = imgProcessor.CreateTransformedImage(img, "BGR8");
            img.Release();
            img = transformedImage;

            // Push the transformed image into the passed output bitmap.
            Rectangle rect = new Rectangle(0, 0, outputImage.Width, outputImage.Height);
            BitmapData bmpData = null;
            try
            {
                bmpData = outputImage.LockBits(rect, ImageLockMode.WriteOnly, outputImage.PixelFormat);
                IntPtr[] ptrBmp = new IntPtr[] { bmpData.Scan0 };
                NativeMethods.memcpy(bmpData.Scan0.ToPointer(), img.Buffer.ToPointer(), rect.Width * rect.Height * 3);
                filled = true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying bitmap. {0}", e.Message);
            }
            finally
            {
                if (bmpData != null)
                    outputImage.UnlockBits(bmpData);
            }

            img.Release();

            return filled;
        }
    }
}

