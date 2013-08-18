using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using PylonC.NET;
using SharpCap.Base;
using SharpCap.Cameras.Basler;
using SharpCap.Cameras.Basler.FeatureControl;
using SharpCap.Cameras.Basler.SupportLib;

namespace PylonC.NETSupportLibrary
{
    /* The ImageProvider is responsible for opening and closing a device, it takes care of the grabbing and buffer handling, 
     it notifies the user via events about state changes, and provides access to GenICam parameter nodes of the device. 
     The grabbing is done in an internal thread. After an image is grabbed the image ready event is fired by the grab 
     thread. The image can be acquired using GetCurrentImage(). After processing of the image it can be released via ReleaseImage.
     The image is then queued for the next grab.  */

    public class ImageProvider : IImageProvider
    {
        #region Delegates

        public delegate void DeviceOpenedEventHandler(object sender, EventArgs args);

        public delegate void GrabErrorEventHandler(object sender, GrabErrorEventArgs e);

        #endregion

        private const int BufferCount = 5;

        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> m_buffers;
        /* Holds handles and buffers used for grabbing. */

        protected DeviceCallbackHandler m_callbackHandler; /* Handles callbacks from a device .*/

        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> m_convertedBuffers;
        /* Holds handles and buffers used for converted images. It is not used for Mono8 or RGBA8packed images.*/

        /* Simple data class for holding image data. */

        /* The members of ImageProvider: */
        protected bool m_converterOutputFormatIsColor; /* The output format of the format converter. */
        private DateTime m_dtLastSent = DateTime.MinValue;
        protected bool m_grabOnce; /* Use for single frame mode. */
        protected Thread m_grabThread; /* Thread for grabbing the images. */
        protected bool m_grabThreadRun; /* Indicates that the grab thread is active.*/
        protected List<GrabResult> m_grabbedBuffers; /* List of grab results already grabbed. */

        protected PYLON_FORMAT_CONVERTER_HANDLE m_hConverter;
        /* The format converter is used mainly for coverting color images. It is not used for Mono8 or RGBA8packed images. */

        protected PYLON_DEVICE_HANDLE m_hDevice; /* Handle for the pylon device. */
        protected PYLON_STREAMGRABBER_HANDLE m_hGrabber; /* Handle for the pylon stream grabber. */
        protected PYLON_DEVICECALLBACK_HANDLE m_hRemovalCallback; /* Required for deregistering the callback. */
        protected PYLON_WAITOBJECT_HANDLE m_hWait; /* Handle used for waiting for a grab to be finished. */
        protected string m_lastError = ""; /* Holds the error information belonging to the last exception thrown. */
        protected Object m_lockObject; /* Lock object used for thread synchronization. */
        protected uint m_numberOfBuffersUsed = BufferCount; /* Number of m_buffers used in grab. */
        protected bool m_open; /* Indicates that the device is open and ready to grab.*/
        protected bool m_removed; /* Indicates that the device has been removed from the PC. */

        /* Constructor with creation of basic objects. */

        public ImageProvider()
        {
            Tracer.TraceStart(LogLevel.Debug);
            /* Create a thread for image grabbing. */
            m_grabThread = new Thread(Grab);
            /* Create objects used for buffer handling. */
            m_lockObject = new Object();
            m_buffers = new Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>>();
            m_grabbedBuffers = new List<GrabResult>();
            /* Create handles. */
            m_hGrabber = new PYLON_STREAMGRABBER_HANDLE();
            m_hDevice = new PYLON_DEVICE_HANDLE();
            m_hRemovalCallback = new PYLON_DEVICECALLBACK_HANDLE();
            m_hConverter = new PYLON_FORMAT_CONVERTER_HANDLE();
            /* Create callback handler and attach the method. */
            m_callbackHandler = new DeviceCallbackHandler();
            m_callbackHandler.CallbackEvent += RemovalCallbackHandler;
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* Indicates that ImageProvider and device are open. */

        public bool IsOpen
        {
            get { return m_open; }
        }

        public int FrameCount { get; set; }
        public int BadFrameCount { get; set; }
        public ICaptureFileWriter CaptureWriter { get; set; }
        public IFrameControl ManualFrameControl { get; set; }

        internal FrameUnpacker FrameUnpacker { get; set; }

        private static string GetLastErrorText()
        {
            string lastErrorMessage = GenApi.GetLastErrorMessage();
            string lastErrorDetail = GenApi.GetLastErrorDetail();

            string lastErrorText = lastErrorMessage;
            if (lastErrorDetail.Length > 0)
            {
                lastErrorText += "\n\nDetails:\n";
            }
            lastErrorText += lastErrorDetail;
            return lastErrorText;
        }

        /* Sets the internal last error variable. */

        private void UpdateLastError()
        {
            m_lastError = GetLastErrorText();
        }

        /* Open using index. Before ImageProvider can be opened using the index, Pylon.EnumerateDevices() needs to be called. */

        public void Open(uint index)
        {
            Tracer.TraceStart(LogLevel.Debug);
            /* Get a handle for the device and proceed. */
            Open(Pylon.CreateDeviceByIndex(index));
            Tracer.TraceEnd(LogLevel.Debug);
        }

        public void Close()
        {
            try
            {
                Stop();
            }
            catch (Exception e)
            {
                m_lastError = e.ToString();
            }

            Tracer.TraceStart(LogLevel.Debug);
            /* Notify that ImageProvider is about to close the device to give other objects the chance to do clean up operations. */
            OnDeviceClosingEvent();

            /* Try to close everything even if exceptions occur. Keep the last exception to throw when it is done. */
            Exception lastException = null;

            /* Reset the removed flag. */
            m_removed = false;

            if (m_hGrabber.IsValid)
            {
                /* Try to close the stream grabber. */
                try
                {
                    Pylon.StreamGrabberClose(m_hGrabber);
                }
                catch (Exception e)
                {
                    lastException = e;
                    UpdateLastError();
                }
                Tracer.Trace(LogLevel.Debug, "Closed grabber");
            }
            else
            {
                Tracer.Trace(LogLevel.Debug, "No grabber to close");
            }


            if (m_hDevice.IsValid)
            {
                /* Try to deregister the removal callback. */
                try
                {
                    if (m_hRemovalCallback.IsValid)
                    {
                        Pylon.DeviceDeregisterRemovalCallback(m_hDevice, m_hRemovalCallback);
                    }
                }
                catch (Exception e)
                {
                    lastException = e;
                    UpdateLastError();
                }

                /* Try to close the device. */
                try
                {
                    /* ... Close and release the pylon device. */
                    if (Pylon.DeviceIsOpen(m_hDevice))
                    {
                        Pylon.DeviceClose(m_hDevice);
                    }
                }
                catch (Exception e)
                {
                    lastException = e;
                    UpdateLastError();
                }

                /* Try to destroy the device. */
                try
                {
                    Pylon.DestroyDevice(m_hDevice);
                }
                catch (Exception e)
                {
                    lastException = e;
                    UpdateLastError();
                }
                Tracer.Trace(LogLevel.Debug, "Closed device");
            }
            else
            {
                Tracer.Trace(LogLevel.Debug, "no device to close");
            }

            m_hGrabber.SetInvalid();
            m_hRemovalCallback.SetInvalid();
            m_hDevice.SetInvalid();

            /* Notify that ImageProvider is now closed.*/
            OnDeviceClosedEvent();

            /* If an exception occurred throw it. */
            if (lastException != null)
            {
                throw lastException;
            }
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* Start the grab of one image. */

        public void OneShot()
        {
            if (m_open && !m_grabThread.IsAlive) /* Only start when open and not grabbing already. */
            {
                /* Set up the grabbing and start. */
                m_numberOfBuffersUsed = 1;
                m_grabOnce = true;
                m_grabThreadRun = true;
                m_grabThread = new Thread(Grab);
                m_grabThread.Start();
            }
        }

        /* Start the grab of images until stopped. */

        public void Start()
        {
            Tracer.TraceStart(LogLevel.Debug);
            if (m_open && !m_grabThread.IsAlive) /* Only start when open and not grabbing already. */
            {
                /* Set up the grabbing and start. */
                m_numberOfBuffersUsed = BufferCount;
                m_grabOnce = false;
                m_grabThreadRun = true;
                m_grabThread = new Thread(Grab);
                m_grabThread.Start();
            }
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* Stops the grabbing of images. */

        public void Stop()
        {
            Tracer.TraceStart(LogLevel.Debug);
            if (m_open && m_grabThread.IsAlive) /* Only start when open and grabbing. */
            {
                m_grabThreadRun = false; /* Causes the grab thread to stop. */
                m_grabThread.Join(); /* Wait for it to stop. */
            }
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* Returns the next available image in the grab result queue. Null is returned if no result is available.
           An image is available when the ImageReady event is fired. */

        public Image GetCurrentImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images available. */
                {
                    return m_grabbedBuffers[0].ImageData;
                }
            }
            return null; /* No image available. */
        }

        /* After the ImageReady event has been received and the image was acquired by using GetCurrentImage, 
        the image must be removed from the grab result queue and added to the stream grabber queue for the next grabs. */

        public bool ReleaseImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0 && m_grabThreadRun)
                    /* If images are available and grabbing is in progress.*/
                {
                    /* Requeue the buffer. */
                    Pylon.StreamGrabberQueueBuffer(m_hGrabber, m_grabbedBuffers[0].Handle, 0);
                    /* Remove it from the grab result queue. */
                    m_grabbedBuffers.RemoveAt(0);
                    return true;
                }
            }
            return false;
        }

        /* Returns the last error message. Usually called after catching an exception. */

        public string GetLastErrorMessage()
        {
            if (m_lastError.Length == 0) /* No error set. */
            {
                UpdateLastError(); /* Try to get error information from the GenApi. */
            }
            string text = m_lastError;
            m_lastError = "";
            return text;
        }

        public void OnStopCapture()
        {
            if (FrameUnpacker != null)
            {
                FrameUnpacker.Dispose();
                FrameUnpacker = null;
            }
        }

        public void OnStartCapture(IColourSpace colourSpace, Size sz)
        {
            FrameUnpacker = colourSpace.IsPacked()
                                                           ? new FrameUnpacker(sz.Width, sz.Height, colourSpace.GetBitsPerPixel())
                                                           : null;
        }

        /* Returns a GenICam parameter node handle of the device identified by the name of the node. */

        public NODE_HANDLE GetNodeFromDevice(string name)
        {
            if (m_open && !m_removed)
            {
                NODEMAP_HANDLE hNodemap = Pylon.DeviceGetNodeMap(m_hDevice);
                return GenApi.NodeMapGetNode(hNodemap, name);
            }
            return new NODE_HANDLE();
        }

        /* Open using device.*/

        protected void Open(PYLON_DEVICE_HANDLE device)
        {
            Tracer.TraceStart(LogLevel.Debug);
            try
            {
                /* Use provided device. */
                m_hDevice = device;

                /* Before using the device, it must be opened. Open it for configuring
                parameters and for grabbing images. */
                Pylon.DeviceOpen(m_hDevice, Pylon.cPylonAccessModeControl | Pylon.cPylonAccessModeStream);

                Tracer.Trace(LogLevel.Debug, "Device opened");
                /* Register the callback function. */
                m_hRemovalCallback = Pylon.DeviceRegisterRemovalCallback(m_hDevice, m_callbackHandler);

                /* For GigE cameras, we recommend increasing the packet size for better 
                   performance. When the network adapter supports jumbo frames, set the packet 
                   size to a value > 1500, e.g., to 8192. In this sample, we only set the packet size
                   to 1500. */
                /* ... Check first to see if the GigE camera packet size parameter is supported and if it is writable. */
                if (Pylon.DeviceFeatureIsWritable(m_hDevice, "GevSCPSPacketSize"))
                {
                    /* ... The device supports the packet size feature. Set a value. */
                    Pylon.DeviceSetIntegerFeature(m_hDevice, "GevSCPSPacketSize", 1500);
                }

                /* The sample does not work in chunk mode. It must be disabled. */
                if (Pylon.DeviceFeatureIsWritable(m_hDevice, "ChunkModeActive"))
                {
                    /* Disable the chunk mode. */
                    Pylon.DeviceSetBooleanFeature(m_hDevice, "ChunkModeActive", false);
                }

                /* Disable acquisition start trigger if available. */
                if (Pylon.DeviceFeatureIsAvailable(m_hDevice, "EnumEntry_TriggerSelector_AcquisitionStart"))
                {
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerSelector", "AcquisitionStart");
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerMode", "Off");
                }

                /* Disable frame start trigger if available. */
                if (Pylon.DeviceFeatureIsAvailable(m_hDevice, "EnumEntry_TriggerSelector_FrameStart"))
                {
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerSelector", "FrameStart");
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerMode", "Off");
                }

                Tracer.Trace(LogLevel.Debug, "Device options set");


                /* Image grabbing is done using a stream grabber.  
                  A device may be able to provide different streams. A separate stream grabber must 
                  be used for each stream. In this sample, we create a stream grabber for the default 
                  stream, i.e., the first stream ( index == 0 ).
                  */

                /* Get the number of streams supported by the device and the transport layer. */
                if (Pylon.DeviceGetNumStreamGrabberChannels(m_hDevice) < 1)
                {
                    throw new Exception("The transport layer doesn't support image streams.");
                }

                /* Create and open a stream grabber for the first channel. */
                m_hGrabber = Pylon.DeviceGetStreamGrabber(m_hDevice, 0);
                Pylon.StreamGrabberOpen(m_hGrabber);

                /* Get a handle for the stream grabber's wait object. The wait object
                   allows waiting for m_buffers to be filled with grabbed data. */
                m_hWait = Pylon.StreamGrabberGetWaitObject(m_hGrabber);
                Tracer.Trace(LogLevel.Debug, "Device stream grabber initialized");
            }
            catch
            {
                Tracer.Trace(LogLevel.Error,
                             "Exception while opening device - clearing up - exception details to follow");
                /* Get the last error message here, because it could be overwritten by cleaning up. */
                UpdateLastError();

                try
                {
                    Close(); /* Try to close any open handles. */
                }
                catch
                {
                    /* Another exception cannot be handled. */
                }
                throw;
            }

            /* Notify that the ImageProvider is open and ready for grabbing and configuration. */
            OnDeviceOpenedEvent();
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* Prepares everything for grabbing. */

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected void SetupGrab()
        {
            Tracer.TraceStart(LogLevel.Debug);
            /* Clear the grab result queue. This is not done when cleaning up to still be able to provide the
               images, e.g. in single frame mode.*/
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Clear();
            }

            /* Set the acquisition mode */
            Pylon.DeviceFeatureFromString(m_hDevice, "AcquisitionMode", m_grabOnce ? "SingleFrame" : "Continuous");

            /* Clear the grab buffers to assure proper operation (because they may
             still be filled if the last grab has thrown an exception). */
            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_buffers)
            {
                pair.Value.Dispose();
            }
            m_buffers.Clear();

            /* Determine the required size of the grab buffer. */
            uint payloadSize = checked((uint) Pylon.DeviceGetIntegerFeature(m_hDevice, "PayloadSize"));

            /* We must tell the stream grabber the number and size of the m_buffers 
                we are using. */
            /* .. We will not use more than NUM_m_buffers for grabbing. */
            Pylon.StreamGrabberSetMaxNumBuffer(m_hGrabber, m_numberOfBuffersUsed);

            /* .. We will not use m_buffers bigger than payloadSize bytes. */
            Pylon.StreamGrabberSetMaxBufferSize(m_hGrabber, payloadSize);

            /*  Allocate the resources required for grabbing. After this, critical parameters 
                that impact the payload size must not be changed until FinishGrab() is called. */
            Pylon.StreamGrabberPrepareGrab(m_hGrabber);

            /* Before using the m_buffers for grabbing, they must be registered at
               the stream grabber. For each buffer registered, a buffer handle
               is returned. After registering, these handles are used instead of the
               buffer objects pointers. The buffer objects are held in a dictionary,
               that provides access to the buffer using a handle as key.
             */
            for (uint i = 0; i < m_numberOfBuffersUsed; ++i)
            {
                PylonBuffer<Byte> buffer = new PylonBuffer<byte>(payloadSize, true);
                PYLON_STREAMBUFFER_HANDLE handle = Pylon.StreamGrabberRegisterBuffer(m_hGrabber, ref buffer);
                m_buffers.Add(handle, buffer);
            }

            /* Feed the m_buffers into the stream grabber's input queue. For each buffer, the API 
               allows passing in an integer as additional context information. This integer
               will be returned unchanged when the grab is finished. In our example, we use the index of the 
               buffer as context information. */
            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_buffers)
            {
                Pylon.StreamGrabberQueueBuffer(m_hGrabber, pair.Key, 0);
            }

            /* The stream grabber is now prepared. As soon the camera starts acquiring images,
               the image data will be grabbed into the provided m_buffers.  */

            /* Set the handle of the image converter invalid to assure proper operation (because it may
             still be valid if the last grab has thrown an exception). */
            m_hConverter.SetInvalid();

            /* Let the camera acquire images. */
            Pylon.DeviceExecuteCommandFeature(m_hDevice, "AcquisitionStart");
            Tracer.TraceEnd(LogLevel.Debug);
        }

        /* This method is executed using the grab thread and is responsible for grabbing, possible conversion of the image
        ,and queuing the image to the result queue. */

        protected void Grab()
        {
            Tracer.TraceStart(LogLevel.Debug);
            /* Notify that grabbing has started. This event can be used to update the state of the GUI. */
            OnGrabbingStartedEvent();
            try
            {
                /* Set up everything needed for grabbing. */
                SetupGrab();

                uint waitTime = GetWaitTime();

                while (m_grabThreadRun) /* Is set to false when stopping to end the grab thread. */
                {
                    if (!Pylon.WaitObjectWait(m_hWait, waitTime))
                    {
                        waitTime = DealWithNoFrame(waitTime);
                        // TODO - if we waited a long time we might have dropped frames
                        continue;
                    }
                    // TODO - if the time is very short we should reduce the wait time, but we don't want to do this each frame


                    PylonGrabResult_t grabResult; /* Stores the result of a grab operation. */
                    /* Since the wait operation was successful, the result of at least one grab 
                       operation is available. Retrieve it. */
                    if (!Pylon.StreamGrabberRetrieveResult(m_hGrabber, out grabResult))
                    {
                        /* Oops. No grab result available? We should never have reached this point. 
                           Since the wait operation above returned without a timeout, a grab result 
                           should be available. */
                        throw new Exception("Failed to retrieve a grab result.");
                    }

                    /* Check to see if the image was grabbed successfully. */
                    if (grabResult.Status == EPylonGrabStatus.Grabbed)
                    {
                        HandleGrabbedFrame(grabResult);

                        /* Exit here for single frame mode. */
                        if (m_grabOnce)
                        {
                            m_grabThreadRun = false;
                            break;
                        }
                    }
                    else if (grabResult.Status == EPylonGrabStatus.Failed)
                    {
                        // grab didn't work properly - dropped frame. Make sure we free the buffer
                        if (grabResult.hBuffer != null)
                            Pylon.StreamGrabberQueueBuffer(m_hGrabber, grabResult.hBuffer, 0);

                        BadFrameCount++;
                    }
                }

                /* Tear down everything needed for grabbing. */
                CleanUpGrab();
            }
            catch (Exception e)
            {
                /* Get the last error message here, because it could be overwritten by cleaning up. */
                string lastErrorMessage = GetLastErrorText();

                try
                {
                    /* Try to tear down everything needed for grabbing. */
                    CleanUpGrab();
                }
                catch
                {
                    /* Another exception cannot be handled. */
                }

                /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
                OnGrabbingStoppedEvent();

                if (!m_removed) /* In case the device was removed from the PC suppress the notification. */
                {
                    /* Notify that the grabbing had errors and deliver the information. */
                    OnGrabErrorEvent(e, lastErrorMessage);
                }
                return;
            }
            /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
            OnGrabbingStoppedEvent();
            Tracer.TraceEnd(LogLevel.Debug);
        }

        private void HandleGrabbedFrame(PylonGrabResult_t grabResult)
        {
            // are we capturing, if so write straight away...
            ICaptureFileWriter cw = CaptureWriter;
            if (cw != null)
            {
                WriteFrame(cw, grabResult);
            }

            // is the UI short of frames to show?
            if (ShouldSendFrameToUI())
            {
                /* Add result to the ready list. */
                EnqueueTakenImage(grabResult);

                /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image 
                 and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/
                OnImageReadyEvent();
            }
            else
                Pylon.StreamGrabberQueueBuffer(m_hGrabber, grabResult.hBuffer, 0);

            FrameCount++;
        }

        private uint DealWithNoFrame(uint waitTime)
        {
            bool buffersFull = AreAllBuffersFull();
            if (!buffersFull)
            {
                if (waitTime < GetWaitTime()) // maybe we didn't wait long enough...
                {
                    waitTime = GetWaitTime();
                    return waitTime;
                }
                /* Timeout occurred. */
                throw new Exception("Grab timeout occurred");
            }
            // do not spin
            Thread.Sleep(1);
            return waitTime;
        }

        private bool AreAllBuffersFull()
        {
            return m_grabbedBuffers.Count == m_numberOfBuffersUsed;
        }

        private bool ShouldSendFrameToUI()
        {
            if (m_grabbedBuffers.Count != 0)
                return false;

            if ((DateTime.Now - m_dtLastSent).TotalMilliseconds < 15)
                return false;

            m_dtLastSent = DateTime.Now;

            return true;
        }

        private void WriteFrame(ICaptureFileWriter cw, PylonGrabResult_t grabResult)
        {
            lock (cw)
            {
                PylonBuffer<Byte> buffer; /* Reference to the buffer attached to the grab result. */

                /* Get the buffer from the dictionary. */
                if (!m_buffers.TryGetValue(grabResult.hBuffer, out buffer))
                {
                    /* Oops. No buffer available? We should never have reached this point. Since all buffers are
                       in the dictionary. */
                    throw new Exception("Failed to find the buffer associated with the handle returned in grab result.");
                }

                if (FrameUnpacker != null)
                {
                    FrameUnpacker.UnpackData(buffer.Pointer, (int) grabResult.PayloadSize);
                    cw.WriteFrame(FrameUnpacker.UnpackedData, FrameUnpacker.UnpackedLength);
                }
                else
                {
                    cw.WriteFrame(buffer.Pointer, (int) grabResult.PayloadSize);
                }
            }
        }

        private uint GetWaitTime()
        {
            if (ManualFrameControl != null)
                return (uint) (ManualFrameControl.GetExposureMS()*3);

            double exposure = Pylon.DeviceGetFloatFeature(m_hDevice, "ExposureTimeAbs");
            exposure /= 1000; // us to ms
            return (uint) (Math.Max(2000, 3.5*exposure));
            // allow for 2 drop frames  in a row when doing longer exposures
        }

        protected void EnqueueTakenImage(PylonGrabResult_t grabResult)
        {
            PylonBuffer<Byte> buffer; /* Reference to the buffer attached to the grab result. */

            /* Get the buffer from the dictionary. */
            if (!m_buffers.TryGetValue(grabResult.hBuffer, out buffer))
            {
                /* Oops. No buffer available? We should never have reached this point. Since all buffers are
                   in the dictionary. */
                throw new Exception("Failed to find the buffer associated with the handle returned in grab result.");
            }

            /* Create a new grab result to enqueue to the grabbed buffers list. */
            GrabResult newGrabResultInternal = new GrabResult {Handle = grabResult.hBuffer};
            /* Add the handle to requeue the buffer in the stream grabber queue. */

            /* If already in output format add the image data. */
            if (grabResult.PixelType == EPylonPixelType.PixelType_Mono8 ||
                grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed)
            {
                newGrabResultInternal.ImageData = new Image(grabResult.SizeX, grabResult.SizeY, buffer.Array,
                                                            grabResult.PixelType ==
                                                            EPylonPixelType.PixelType_RGBA8packed, buffer.Array);
            }
            else /* Conversion is required. */
            {
                /* Create a new format converter if needed. */
                if (!m_hConverter.IsValid)
                {
                    m_convertedBuffers = new Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<byte>>();
                    /* Create a new dictionary for the converted buffers. */
                    m_hConverter = Pylon.PixelFormatConverterCreate(m_hDevice, 1); /* Create the converter. */
                    m_converterOutputFormatIsColor = !Pylon.IsMono(grabResult.PixelType) ||
                                                     Pylon.IsBayer(grabResult.PixelType);
                }
                /* Reference to the buffer attached to the grab result handle. */
                PylonBuffer<Byte> convertedBuffer;
                /* Look up if a buffer is already attached to the handle. */
                bool bufferListed = m_convertedBuffers.TryGetValue(grabResult.hBuffer, out convertedBuffer);
                /* Perform the conversion. If the buffer is null a new one is automatically created. */
                Pylon.PixelFormatConverterConvert(m_hConverter, ref convertedBuffer, buffer);
                if (!bufferListed) /* A new buffer has been created. Add it to the dictionary. */
                {
                    m_convertedBuffers.Add(grabResult.hBuffer, convertedBuffer);
                }
                /* Add the image data. */
                newGrabResultInternal.ImageData = new Image(grabResult.SizeX, grabResult.SizeY, convertedBuffer.Array,
                                                            m_converterOutputFormatIsColor, buffer.Array);
            }
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Add(newGrabResultInternal); /* Add the new grab result to the queue. */
            }
        }

        protected void CleanUpGrab()
        {
            Tracer.TraceStart(LogLevel.Debug);
            /*  ... Stop the camera. */
            Pylon.DeviceExecuteCommandFeature(m_hDevice, "AcquisitionStop");

            /* Destroy the format converter if one was used. */
            if (m_hConverter.IsValid)
            {
                /* Destroy the converter. */
                Pylon.PixelFormatConverterDestroy(m_hConverter);
                /* Set the handle invalid. The next grab cycle may not need a converter. */
                m_hConverter.SetInvalid();
                /* Release the converted image buffers. */
                foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_convertedBuffers)
                {
                    pair.Value.Dispose();
                }
                m_convertedBuffers = null;
            }

            /* ... We must issue a cancel call to ensure that all pending m_buffers are put into the
               stream grabber's output queue. */
            Pylon.StreamGrabberCancelGrab(m_hGrabber);

            /* ... The m_buffers can now be retrieved from the stream grabber. */
            {
                bool isReady; /* Used as an output parameter. */
                do
                {
                    PylonGrabResult_t grabResult; /* Stores the result of a grab operation. */
                    isReady = Pylon.StreamGrabberRetrieveResult(m_hGrabber, out grabResult);
                } while (isReady);
            }

            /* ... When all m_buffers are retrieved from the stream grabber, they can be deregistered.
                   After deregistering the m_buffers, it is safe to free the memory. */

            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_buffers)
            {
                Pylon.StreamGrabberDeregisterBuffer(m_hGrabber, pair.Key);
            }

            /* The buffers can now be released. */
            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_buffers)
            {
                pair.Value.Dispose();
            }
            m_buffers.Clear();

            /* ... Release grabbing related resources. */
            Pylon.StreamGrabberFinishGrab(m_hGrabber);

            /* After calling PylonStreamGrabberFinishGrab(), parameters that impact the payload size (e.g., 
            the AOI width and height parameters) are unlocked and can be modified again. */
        }

        /* This callback is called by the pylon layer using DeviceCallbackHandler. */

        protected void RemovalCallbackHandler(PYLON_DEVICE_HANDLE hDevice)
        {
            /* Notify that the device has been removed from the PC. */
            OnDeviceRemovedEvent();
        }

        /* The events fired by ImageProvider. See the invocation methods below for further information, e.g. OnGrabErrorEvent. */

        public event EventHandler DeviceOpenedEvent;

        public event EventHandler DeviceClosingEvent;

        public event EventHandler DeviceClosedEvent;

        public event EventHandler GrabbingStartedEvent;

        public event EventHandler ImageReadyEvent;

        public event EventHandler GrabbingStoppedEvent;

        public event GrabErrorEventHandler GrabErrorEvent;

        public event EventHandler DeviceRemovedEvent;

        /* Notify that ImageProvider is open and ready for grabbing and configuration. */

        protected void OnDeviceOpenedEvent()
        {
            m_open = true;
            if (DeviceOpenedEvent != null)
            {
                DeviceOpenedEvent(this, null);
            }
        }

        /* Notify that ImageProvider is about to close the device to give other objects the chance to do clean up operations. */

        protected void OnDeviceClosingEvent()
        {
            m_open = false;
            if (DeviceClosingEvent != null)
            {
                DeviceClosingEvent(this, null);
            }
        }

        /* Notify that ImageProvider is now closed.*/

        protected void OnDeviceClosedEvent()
        {
            m_open = false;
            if (DeviceClosedEvent != null)
            {
                DeviceClosedEvent(this, null);
            }
        }

        /* Notify that grabbing has started. This event could be used to update the state of the GUI. */

        protected void OnGrabbingStartedEvent()
        {
            if (GrabbingStartedEvent != null)
            {
                GrabbingStartedEvent(this, null);
            }
        }

        /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image 
         and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/

        protected void OnImageReadyEvent()
        {
            if (ImageReadyEvent != null)
            {
                ImageReadyEvent(this, null);
            }
        }

        /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */

        protected void OnGrabbingStoppedEvent()
        {
            if (GrabbingStoppedEvent != null)
            {
                GrabbingStoppedEvent(this, null);
            }
        }

        /* Notify that the grabbing had errors and deliver the information. */

        protected void OnGrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            if (GrabErrorEvent != null)
            {
                GrabErrorEvent(this,
                               new GrabErrorEventArgs
                                   {grabException = grabException, additionalErrorMessage = additionalErrorMessage});
            }
        }

        /* Notify that the device has been removed from the PC. */

        protected void OnDeviceRemovedEvent()
        {
            m_removed = true;
            m_grabThreadRun = false;
            if (DeviceRemovedEvent != null)
            {
                DeviceRemovedEvent(this, null);
            }
        }

        internal FeatureFactory BuildFeatureFactory()
        {
            return new FeatureFactory(m_hDevice);
        }

        #region Nested type: GrabErrorEventArgs

        public class GrabErrorEventArgs : EventArgs
        {
            public string additionalErrorMessage;
            public Exception grabException;
        }

        #endregion

        #region Nested type: GrabResult

        protected class GrabResult
        {
            public PYLON_STREAMBUFFER_HANDLE Handle;
            /* Holds the handle of the image registered at the stream grabber. It is used to queue the buffer associated with itself for the next grab. */

            public Image ImageData; /* Holds the taken image. */
        }

        #endregion

        #region Nested type: Image

        public class Image
        {
            [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
            public readonly Byte[]
                Buffer; /* The raw image data. */

            public readonly bool Color;
            /* If false the buffer contains a Mono8 image. Otherwise, RGBA8packed is provided. */

            public readonly int Height; /* The height of the image. */

            [SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
            public readonly Byte[]
                RawBuffer; /* The raw image data. */

            public readonly int Width; /* The width of the image. */

            public Image(int newWidth, int newHeight, Byte[] newBuffer, bool color, Byte[] rawBuffer)
            {
                Width = newWidth;
                Height = newHeight;
                Buffer = newBuffer;
                Color = color;
                RawBuffer = rawBuffer;
            }
        }

        #endregion

        public Bitmap UpdateImageInto(Bitmap bitmap)
        {
            /* Acquire the image from the image provider. */
            ImageProvider.Image image = GetCurrentImage();

            /* Check if the image has been removed in the meantime. */
            if (image != null)
            {
                /* Check if the image is compatible with the currently used bitmap. */
                if (BitmapFactory.IsCompatible(bitmap, image.Width, image.Height, image.Color))
                {
                    /* Update the bitmap with the image data. */
                    BitmapFactory.UpdateBitmap(bitmap, image.Buffer, image.Width, image.Height, image.Color);
                }
                else /* A new bitmap is required. */
                {
                    BitmapFactory.CreateBitmap(ref bitmap, image.Width, image.Height, image.Color);
                    BitmapFactory.UpdateBitmap(bitmap, image.Buffer, image.Width, image.Height, image.Color);
                }
                /* The processing of the image is done. Release the image buffer. */
                ReleaseImage();
                /* The buffer can be used for the next image grabs. 
                    If another image is in the output queue it can be acquired now using GetCurrentImage(). */
            }

            return bitmap;
        }
    }
}