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

        private BGAPI2.System system;
        private BGAPI2.Interface interf;
        private BGAPI2.Device device;
        private BGAPI2.DataStream dataStream;
        private BGAPI2.BufferList bufferList;

        //private int width;
        //private int height;
        //private bool isColor;
        private bool cancelled;
        private bool hadError;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public SnapshotRetriever(CameraSummary summary)
        {
            this.summary = summary;
            SpecificInfo specific = summary.Specific as SpecificInfo;

            try
            {
                // Look for the device.
                SystemList systemList = SystemList.Instance;
                systemList.Refresh();

                foreach (KeyValuePair<string, BGAPI2.System> systemPair in systemList)
                {
                    if (systemPair.Key != specific.SystemKey)
                        continue;

                    system = systemPair.Value;
                    system.Open();
                    break;
                }

                if (system == null)
                    return;
                    
                system.Interfaces.Refresh(100);
                foreach (KeyValuePair<string, BGAPI2.Interface> interfacePair in system.Interfaces)
                {
                    if (interfacePair.Key != specific.InterfaceKey)
                        continue;

                    interf = interfacePair.Value;
                    interf.Open();
                    break;
                }

                if (interf == null)
                {
                    system.Close();
                    return;
                }
        
                interf.Devices.Refresh(100);
                foreach (KeyValuePair<string, BGAPI2.Device> devicePair in interf.Devices)
                {
                    if (devicePair.Key != specific.DeviceKey)
                        continue;

                    device = devicePair.Value;
                    device.Open();
                    break;
                }

                if (device == null)
                {
                    interf.Close();
                    system.Close();
                    return;
                }

                DataStreamList dataStreamList = device.DataStreams;
                dataStreamList.Refresh();
                foreach (KeyValuePair<string, BGAPI2.DataStream> dataStreamPair in dataStreamList)
                {
                    if (string.IsNullOrEmpty(dataStreamPair.Key))
                        continue;

                    dataStream = dataStreamPair.Value;
                    dataStream.Open();
                    break;
                }

                if (dataStream == null)
                {
                    device.Close();
                    interf.Close();
                    system.Close();
                    return;
                }

                // Use buffers internal to the API.
                bufferList = dataStream.BufferList;
                int countBuffers = 4;
                for (int i = 0; i < countBuffers; i++)
                {
                    BGAPI2.Buffer buffer = new BGAPI2.Buffer();
                    
                    bufferList.Add(buffer);

                    ulong memSize = buffer.MemSize;
                    log.DebugFormat("Buffer mem size: {0}", memSize);
                }

                if (bufferList != null && bufferList.Count == countBuffers)
                {
                    foreach (KeyValuePair<string, BGAPI2.Buffer> bufferPair in bufferList)
                        bufferPair.Value.QueueBuffer();
                }

                //width = (int)device.RemoteNodeList["Width"].Value.ToLong();
                //height = (int)device.RemoteNodeList["Height"].Value.ToLong();
            }
            catch (Exception e)
            {
                LogError(e, "Failed to open device");
            }
        }

        /// <summary>
        /// Start the device for a frame grab, wait a bit and then return the result.
        /// This method MUST raise a CameraThumbnailProduced event, even in case of error.
        /// </summary>
        public void Run(object data)
        {
            if (system == null || interf == null || device == null || dataStream == null || bufferList == null || bufferList.Count == 0)
            {
                Close();
                if (CameraThumbnailProduced != null)
                    CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, null, ImageDescriptor.Invalid, true, false));

                return;
            }

            Thread.CurrentThread.Name = string.Format("{0} thumbnailer", summary.Alias);
            log.DebugFormat("Starting {0} for thumbnail.", summary.Alias);

            try
            {
                // Start continuous acquisition.
                dataStream.StartAcquisition();

                device.RemoteNodeList["AcquisitionStart"].Execute();

                BGAPI2.Buffer bufferFilled = bufferFilled = dataStream.GetFilledBuffer(1000);
                if (bufferFilled != null && !bufferFilled.IsIncomplete)
                {
                    ulong width = bufferFilled.Width;
                    ulong height = bufferFilled.Height;
                    string pixFmt = bufferFilled.PixelFormat;
                    IntPtr bytes = bufferFilled.MemPtr;
                    ulong byteCount = (ulong)bufferFilled.MemSize;

                    // bufferFilled.MemPtr contains native memory of the Bayer pattern frame.

                    BGAPI2.ImageProcessor imgProcessor = new BGAPI2.ImageProcessor();
                    if (imgProcessor.NodeList.GetNodePresent("DemosaicingMethod") == true)
                    {
                        imgProcessor.NodeList["DemosaicingMethod"].Value = "NearestNeighbor";
                        //imgProcessor.NodeList["DemosaicingMethod"].Value = "Bilinear3x3";
                    }

                    //BGAPI2.Node pixelFormatInfoSelector = imgProcessor.NodeList["PixelFormatInfoSelector"];
                    BGAPI2.Node bytesPerPixel = imgProcessor.NodeList["BytesPerPixel"];
                    long bpp = bytesPerPixel.IsAvailable ? bytesPerPixel.Value.ToLong() : 1;

                    //BGAPI2.Image transformImage = null;
                    //byte[] transformImageBufferCopy;

                    // Demosaicing of the image.
                    // TODO: only do this if the image is Bayer pattern.
                    BGAPI2.Image img = imgProcessor.CreateImage((uint)width, (uint)height, pixFmt, bytes, byteCount);
                    BGAPI2.Image img2 = imgProcessor.CreateTransformedImage(img, "BGR8");


                    //image = imgProcessor.CreateBitmap(img, true);

                    //if (bpp == 3)
                        FillRGB24(img2.Buffer, (int)img2.Width, (int)img2.Height);
                    //else if (bpp == 1)
                      //  FillY800(img2.Buffer, (int)img2.Width, (int)img2.Height);

                    if (img != null) 
                        img.Release();
                    
                    if (img2 != null) 
                        img2.Release();

                    //mTransformImage = imgProcessor.CreateTransformedImage(mImage, "Mono8");

                    //byte[] imageBufferCopy;
                    //imageBufferCopy = new byte[(uint)((uint)img.Width * (uint)img.Height * fBytesPerPixel)];
                    //Marshal.Copy(img.Buffer, imageBufferCopy, 0, (int)((int)img.Width * (int)img.Height * fBytesPerPixel));
                    //ulong imageBufferAddress = (ulong)img.Buffer;

                    //if (img.PixelFormat.StartsWith("Mono"))  // if pixel format starts with "Mono"
                    //{
                    //    //transform to Mono8
                    //    //mTransformImage = imgProcessor.CreateTransformedImage(mImage, "Mono8");

                    //}




                    bufferFilled.QueueBuffer();
                }
                
                if (device.RemoteNodeList.GetNodePresent("AcquisitionAbort"))
                {
                    device.RemoteNodeList["AcquisitionAbort"].Execute();
                }

                device.RemoteNodeList["AcquisitionStop"].Execute();


                dataStream.StopAcquisition();

                bufferList.DiscardAllBuffers();

                
                while (bufferList.Count > 0)
                {
                    BGAPI2.Buffer buffer = (BGAPI2.Buffer)bufferList.Values.First();
                    bufferList.RevokeBuffer(buffer);
                }
            }
            catch (Exception e)
            {
                LogError(e, null);
            }

            //waitHandle.WaitOne(timeoutGrabbing, false);

            Close();

            if (CameraThumbnailProduced != null)
                CameraThumbnailProduced(this, new CameraThumbnailProducedEventArgs(summary, image, imageDescriptor, hadError, cancelled));
        }

        public void Cancel()
        {
        }

        private void Close()
        {
            if (device == null)
                return;

            // Stop everything and destroy resources.

            // Stop acquisition.
            //try
            //{
            //    if (featureControl != null)
            //    {
            //        featureControl.GetCommandFeature("AcquisitionStop").Execute();
            //        featureControl = null;
            //    }
            //}
            //catch (Exception e)
            //{
            //    log.Error(e.Message);
            //}

            try
            {
                if (dataStream != null)
                {
                    //stream.StopGrab();
                    //stream.UnregisterCaptureCallback();
                    if (dataStream.IsOpen)
                        dataStream.Close();
                    
                    dataStream = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            try
            {
                if (device != null)
                {
                    if (device.IsOpen)
                        device.Close();
                    
                    device = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            try
            {
                if (interf != null)
                {
                    if (interf.IsOpen)
                        interf.Close();
                    
                    interf = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            try
            {
                if (system != null)
                {
                    if (system.IsOpen)
                        system.Close();
                    
                    system = null;
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

        
        private unsafe void FillRGB24(IntPtr buffer, int width, int height)
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

                int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, Video.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
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

        /// <summary>
        /// Convertes Y800 buffer into RGB24 .NET bitmap.
        /// </summary>
        private unsafe void FillY800(IntPtr buffer, int width, int height)
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

                int bufferSize = ImageFormatHelper.ComputeBufferSize(width, height, Video.ImageFormat.RGB24);
                imageDescriptor = new ImageDescriptor(Video.ImageFormat.RGB24, image.Width, image.Height, true, bufferSize);
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

    }
}

