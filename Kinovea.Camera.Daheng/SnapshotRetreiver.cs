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
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using GxIAPINET;
using Kinovea.Pipeline;
using Kinovea.Services;
using System.Diagnostics;

namespace Kinovea.Camera.Daheng
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

        public string Alias
        {
            get { return summary.Alias; }
        }

        public Thread Thread
        {
            get { return snapperThread; }
        }

        #region Members
        private static readonly int timeoutGrabbing = 5000;

        private Bitmap image;
        private ImageDescriptor imageDescriptor = ImageDescriptor.Invalid;
        private CameraSummary summary;
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private IGXDevice device;
        private IGXFeatureControl featureControl;
        private IGXStream stream;
        private int width;
        private int height;
        private bool isColor;
        const uint PIXEL_FORMATE_BIT = 0x00FF0000;                    ///<For the current data format and operation to get the current data bits
        const uint GX_PIXEL_8BIT = 0x00080000;                        ///<8 bit data image format
        private bool cancelled;
        private bool hadError;
        private Thread snapperThread;
        private object locker = new object();
        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary, IGXFactory igxFactory)
        {
            this.summary = summary;

            try
            {
                stopwatch.Start();
                device = igxFactory.OpenDeviceBySN(summary.Identifier, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                log.DebugFormat("{0} opened in {1} ms.", summary.Alias, stopwatch.ElapsedMilliseconds);
                stopwatch.Stop();

                featureControl = device.GetRemoteFeatureControl();
                DahengHelper.AfterOpen(featureControl);

                width = (int)featureControl.GetIntFeature("Width").GetValue();
                height = (int)featureControl.GetIntFeature("Height").GetValue();
                isColor = DahengHelper.IsColor(featureControl);

                stream = device.OpenStream(0);
            }
            catch (Exception e)
            {
                LogError(e, "Failed to open device");
            }
        }

        public void Start()
        {
            snapperThread = new Thread(Run) { IsBackground = true };
            snapperThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            snapperThread.Start();
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            if (device == null || stream == null)
            {
                Close();
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, ImageDescriptor.Invalid, true, false));

                return;
            }

            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            try
            {
                // Start continuous acquisition.
                stream.RegisterCaptureCallback(this, stream_OnFrame);
                stream.StartGrab();

                if (featureControl != null)
                    featureControl.GetCommandFeature("AcquisitionStart").Execute();
            }
            catch (Exception e)
            {
                LogError(e, null);
            }

            waitHandle.WaitOne(timeoutGrabbing, false);

            lock (locker)
            {
                if (!cancelled)
                    Close();
            }

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
            log.DebugFormat("Cancelling thumbnail for {0}.", Alias);

            if (device == null || stream == null)
                return;
            
            lock (locker)
            {
                Close();
                cancelled = true;
            }

            waitHandle.Set();
        }

        private void Close()
        {
            if (device == null)
                return;

            // Stop everything and destroy resources.

            // Stop acquisition.
            try
            {
                if (featureControl != null)
                {
                    featureControl.GetCommandFeature("AcquisitionStop").Execute();
                    featureControl = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            // Close stream.
            try
            {
                if (stream != null)
                {
                    stream.StopGrab();
                    stream.UnregisterCaptureCallback();
                    stream.Close();
                    stream = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            // Close device.
            try
            {
                if (device != null)
                {
                    device.Close();
                    device = null;

                    SpecificInfo specific = summary.Specific as SpecificInfo;
                    if (specific != null && specific.Device != null)
                        specific.Device = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Camera {0} failure during thumbnail capture.", summary.Alias);
            log.Error(e.Message);

            if (!string.IsNullOrEmpty(additionalErrorMessage))
                log.Error(additionalErrorMessage);
        }

        #region Camera events
        private void stream_OnFrame(object objUserParam, IFrameData objIFrameData)
        {
            UpdateImageData(objIFrameData);
        }

        private void UpdateImageData(IBaseData objIBaseData)
        { 
            try
            {
                GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
                if (null != objIBaseData)
                {
                    emValidBits = DahengHelper.GetBestValidBit(objIBaseData.GetPixelFormat());
                    if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                    {
                        if (isColor)
                        {
                            IntPtr buffer = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, true);
                            FillRGB24(buffer);
                        }
                        else
                        {
                            IntPtr buffer = objIBaseData.GetBuffer();
                            FillY800(buffer);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            waitHandle.Set();
        }

        private unsafe void FillRGB24(IntPtr buffer)
        {
            image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = null;

            try
            {
                bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, image.PixelFormat);
                IntPtr[] ptrBmp = new IntPtr[] { bmpData.Scan0 };
                int stride = rect.Width * 3;
                NativeMethods.memcpy(bmpData.Scan0.ToPointer(), buffer.ToPointer(), stride * height);

                int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, Kinovea.Services.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Kinovea.Services.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying bitmaps. {0}", e.Message);
            }
            finally
            {
                if (bmpData != null)
                    image.UnlockBits(bmpData);
            }
        }

        private unsafe void FillY800(IntPtr buffer)
        {
            image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = null;

            try
            {
                bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, image.PixelFormat);
                int dstOffset = bmpData.Stride - (rect.Width * 3);

                byte* src = (byte*)buffer.ToPointer();
                byte* dst = (byte*)bmpData.Scan0.ToPointer();

                for (int i = 0; i < rect.Height; i++)
                {
                    for (int j = 0; j < rect.Width; j++)
                    {
                        dst[0] = dst[1] = dst[2] = *src;
                        src++;
                        dst += 3;
                    }

                    dst += dstOffset;
                }
                
                int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, Kinovea.Services.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Kinovea.Services.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying bitmaps. {0}", e.Message);
            }
            finally
            {
                if (bmpData != null)
                    image.UnlockBits(bmpData);
            }
        }

        #endregion
    }
}

