using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BGAPI2;

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// Helper class to wrap GenICam camera (Based on Baumer API).
    /// Provide open/close and acquisition once / acquisition continuous in a separate thread.
    /// Get the buffers and raise BufferProduced event.
    /// </summary>
    public class GenICamProvider
    {
        public event EventHandler<BufferEventArgs> BufferProduced;

        #region Properties
        public bool IsOpen 
        {
            get { return opened; }
        }

        public Device Device
        {
            get { return device; }
        }
        #endregion

        #region Members
        private Device device;
        private DataStream dataStream;
        private BufferList bufferList;
        private int bufferCount = 16;
        private ulong bufferFilledTimeoutMS = 1000;
        private bool opened;
        private bool started;
        private bool grabThreadRun = true;
        private Thread grabThread;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion


        public bool Open(Device device)
        {
            Close();

            try
            {
                this.device = device;
                if (device == null)
                    return false;

                if (!device.IsOpen)
                    device.Open();

                if (!device.IsOpen)
                    return false;

                DataStreamList dataStreamList = device.DataStreams;
                dataStreamList.Refresh();
                foreach (KeyValuePair<string, DataStream> dataStreamPair in dataStreamList)
                {
                    if (string.IsNullOrEmpty(dataStreamPair.Key))
                        continue;

                    dataStream = dataStreamPair.Value;
                    dataStream.Open();
                    break;
                }

                if (dataStream == null)
                {
                    CloseDevice();
                    return false;
                }

                QueueBuffers();

                log.DebugFormat("Opened device: {0} ({1})", device.DisplayName, device.Vendor);
                opened = true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("Failed to open device. {0}", e);
                DiscardBuffers();
                CloseDataStream();
                CloseDevice();
            }
            
            return opened;
        }

        /// <summary>
        /// Close the currently opened device and deallocate buffers.
        /// Does not close the corresponding interface and system.
        /// </summary>
        public void Close()
        {
            if (!opened)
                return;

            if (started)
                Stop();

            opened = false;

            DiscardBuffers();
            CloseDataStream();
            CloseDevice();
        }

        /// <summary>
        /// Start acquisition, get one frame synchronously, convert to RGB24 Bitmap, stop acquisition.
        /// </summary>
        public void AcquireOne()
        {
            if (!opened)
                return;

            if (started)
                Stop();

            dataStream.StartAcquisition();
            CameraPropertyManager.ExecuteCommand(device, "AcquisitionStart");
            started = true;

            // Wait for one frame.
            BGAPI2.Buffer bufferFilled = dataStream.GetFilledBuffer(bufferFilledTimeoutMS);
            if (bufferFilled == null || bufferFilled.IsIncomplete || bufferFilled.MemPtr == IntPtr.Zero)
            {
                // Timeout or error while waiting for the frame.
                Stop();
                return;
            }

            // At this point we have an image buffer available.
            // .MemPtr contains native memory of the raw frame.
            if (BufferProduced != null)
                BufferProduced(this, new BufferEventArgs(bufferFilled));

            // Make the buffer available again.
            bufferFilled.QueueBuffer();
            Stop();
        }

        public void AcquireContinuous()
        {
            // Start a background thread and post new buffers.
            if (!opened)
                return;

            if (!dataStream.IsOpen)
                return;

            // setup
            dataStream.StartAcquisition();
            CameraPropertyManager.WriteEnum(device, "AcquisitionMode", "Continuous");
            CameraPropertyManager.ExecuteCommand(device, "AcquisitionStart");
            started = true;
            
            // TODO: use ThreadPool instead ?
            grabThreadRun = true;
            grabThread = new Thread(Grab);
            grabThread.Start();
        }

        public void Stop()
        {
            if (!opened || !started)
                return;

            if (grabThread != null && grabThread.IsAlive)
            {
                grabThreadRun = false;
                grabThread.Join();
            }

            CameraPropertyManager.ExecuteCommand(device, "AcquisitionAbort");
            CameraPropertyManager.ExecuteCommand(device, "AcquisitionStop");
            dataStream.StopAcquisition();
        }

        /// <summary>
        /// Thread method.
        /// </summary>
        private void Grab()
        {
            // raise event start grabbing.
            log.DebugFormat("Start grabbing thread.");
            try
            {
                // setup.

                while (grabThreadRun)
                {
                    // Wait for the next buffer.
                    BGAPI2.Buffer buffer = dataStream.GetFilledBuffer(bufferFilledTimeoutMS);
                    if (buffer == null)
                    {
                        // Timeout to get the buffer.
                        //log.Error("A grab timeout or error occurred");
                        //throw new Exception("A grab timeout or error occurred.");

                        log.ErrorFormat("Null: Queued:{0}, Started:{1}, AwaitDelivery:{2}, Delivered:{3}, Underrun:{4}",
                            bufferList.QueuedCount, bufferList.StartedCount, bufferList.AwaitDeliveryCount, bufferList.DeliveredCount, bufferList.UnderrunCount);

                        // Requeue all the buffers.
                        //bufferList.DiscardAllBuffers();
                        //foreach (KeyValuePair<string, BGAPI2.Buffer> bufferPair in bufferList)
                        //    bufferPair.Value.QueueBuffer();
                        bufferList.FlushAllToInputQueue();
                    }
                    else if (buffer.MemPtr == IntPtr.Zero)
                    {
                        log.Warn("Buffer pointing to invalid memory.");
                        buffer.QueueBuffer();
                    }
                    else if (buffer.IsIncomplete)
                    {
                        log.WarnFormat("Incomplete: Queued:{0}, Started:{1}, AwaitDelivery:{2}, Delivered:{3}, Underrun:{4}",
                            bufferList.QueuedCount, bufferList.StartedCount, bufferList.AwaitDeliveryCount, bufferList.DeliveredCount, bufferList.UnderrunCount);

                        // This situation is generally critical, requeue all the buffers.
                        //bufferList.DiscardAllBuffers();
                        //foreach (KeyValuePair<string, BGAPI2.Buffer> bufferPair in bufferList)
                        //    bufferPair.Value.QueueBuffer();
                        bufferList.FlushAllToInputQueue();
                    }
                    else
                    {
                        //log.DebugFormat("Received: Queued:{0}, Started:{1}, AwaitDelivery:{2}, Delivered:{3}, Underrun:{4}",
                        //        bufferList.QueuedCount, bufferList.StartedCount, bufferList.AwaitDeliveryCount, bufferList.DeliveredCount, bufferList.UnderrunCount);

                        // Post image event.
                        if (BufferProduced != null)
                            BufferProduced(this, new BufferEventArgs(buffer));
                        
                        // Make the buffer available to be filled again.
                        buffer.QueueBuffer();
                    }
                }

                // Normal cancellation of the grabbing thread.
                // Cleanup.
            }
            catch(Exception)
            {
                grabThreadRun = false;

                // Cleanup.
            }

            // Normal thread death.
        }

        private void DiscardBuffers()
        {
            try
            {
                if (bufferList != null)
                {
                    bufferList.DiscardAllBuffers();
                    while (bufferList.Count > 0)
                    {
                        BGAPI2.Buffer buffer = (BGAPI2.Buffer)bufferList.Values.First();
                        bufferList.RevokeBuffer(buffer);
                    }

                    bufferList = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void QueueBuffers()
        {
            // Allocate buffers.
            // These buffers are always allocated at the full image resolution of 
            // the camera and will be partially filled if we use a smaller resolution.
            bufferList = dataStream.BufferList;
            for (int i = 0; i < bufferCount; i++)
            {
                BGAPI2.Buffer buffer = new BGAPI2.Buffer();
                bufferList.Add(buffer);
                buffer.QueueBuffer();
            }
        }

        private void CloseDataStream()
        {
            try
            {
                if (dataStream != null)
                {
                    if (dataStream.IsOpen)
                        dataStream.Close();

                    dataStream = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        private void CloseDevice()
        {
            try
            {
                if (device != null)
                {
                    if (device.IsOpen)
                    {
                        device.Close();
                        log.DebugFormat("Closed device: {0} ({1})", device.DisplayName, device.Vendor);
                    }
                    
                    device = null;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }
    }
}
