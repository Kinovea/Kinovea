using System;
using System.Collections.Generic;
using PylonC.NET;
using System.Threading;
using Kinovea.Camera.Basler;

namespace PylonC.NETSupportLibrary
{
    /* The ImageProvider is responsible for opening and closing a device, it takes care of the grabbing and buffer handling,
     it notifies the user via events about state changes, and provides access to GenICam parameter nodes of the device.
     The grabbing is done in an internal thread. After an image is grabbed the image ready event is fired by the grab
     thread. The image can be acquired using GetCurrentImage(). After processing of the image it can be released via ReleaseImage.
     The image is then queued for the next grab.  */
    public class ImageProvider
    {
        /* Simple data class for holding image data. */
        public class Image
        {
            public Image(int newWidth, int newHeight, Byte[] newBuffer, bool color)
            {
                Width = newWidth;
                Height = newHeight;
                Buffer = newBuffer;
                Color = color;
            }

            public readonly int Width; /* The width of the image. */
            public readonly int Height; /* The height of the image. */
            public readonly Byte[] Buffer; /* The raw image data. */
            public readonly bool Color; /* If false the buffer contains a Mono8 image. Otherwise, RGBA8packed is provided. */
        }

        /* The class GrabResult is used internally to queue grab results. */
        protected class GrabResult
        {
            public Image ImageData; /* Holds the taken image. */
            public PYLON_STREAMBUFFER_HANDLE Handle; /* Holds the handle of the image registered at the stream grabber. It is used to queue the buffer associated with itself for the next grab. */
        }

        /* The members of ImageProvider: */
        protected bool m_converterOutputFormatIsColor = false;/* The output format of the format converter. */
        protected PYLON_IMAGE_FORMAT_CONVERTER_HANDLE m_hConverter; /* The format converter is used mainly for coverting color images. It is not used for Mono8 or RGBA8packed images. */
        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> m_convertedBuffers; /* Holds handles and buffers used for converted images. It is not used for Mono8 or RGBA8packed images.*/
        protected PYLON_DEVICE_HANDLE m_hDevice;           /* Handle for the pylon device. */
        protected PYLON_STREAMGRABBER_HANDLE m_hGrabber;   /* Handle for the pylon stream grabber. */
        protected PYLON_DEVICECALLBACK_HANDLE m_hRemovalCallback;    /* Required for deregistering the callback. */
        protected PYLON_WAITOBJECT_HANDLE m_hWait;         /* Handle used for waiting for a grab to be finished. */
        protected uint m_numberOfBuffersUsed = 5;          /* Number of m_buffers used in grab. */
        protected bool m_grabThreadRun = false;            /* Indicates that the grab thread is active.*/
        protected bool m_open = false;                     /* Indicates that the device is open and ready to grab.*/
        protected bool m_grabOnce = false;                 /* Use for single frame mode. */
        protected bool m_removed = false;                  /* Indicates that the device has been removed from the PC. */
        protected Thread m_grabThread;                     /* Thread for grabbing the images. */
        protected Object m_lockObject;                     /* Lock object used for thread synchronization. */
        protected Dictionary<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> m_buffers; /* Holds handles and buffers used for grabbing. */
        protected List<GrabResult> m_grabbedBuffers; /* List of grab results already grabbed. */
        protected DeviceCallbackHandler m_callbackHandler; /* Handles callbacks from a device .*/
        protected string m_lastError = "";                 /* Holds the error information belonging to the last exception thrown. */
        protected Bayer8Conversion m_bayer8Conversion = Bayer8Conversion.Color;

        /* Creates the last error text from message and detailed text. */
        private string GetLastErrorText()
        {
            string lastErrorMessage = GenApi.GetLastErrorMessage();
            string lastErrorDetail  = GenApi.GetLastErrorDetail();

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

        /* Constructor with creation of basic objects. */
        public ImageProvider()
        {
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
            m_hConverter = new PYLON_IMAGE_FORMAT_CONVERTER_HANDLE();
            /* Create callback handler and attach the method. */
            m_callbackHandler = new DeviceCallbackHandler();
            m_callbackHandler.CallbackEvent += new DeviceCallbackHandler.DeviceCallback(RemovalCallbackHandler);
        }

        /* Indicates that ImageProvider and device are open. */
        public bool IsOpen
        {
            get { return m_open; }
        }

        public void SetDebayering(Bayer8Conversion bayer8Conversion)
        {
            m_bayer8Conversion = bayer8Conversion;
        }

        /* Open using index. Before ImageProvider can be opened using the index, Pylon.EnumerateDevices() needs to be called. */
        public void Open(uint index)
        {
            /* Get a handle for the device and proceed. */
            Open(Pylon.CreateDeviceByIndex(index));
        }

        /* Close the device */
        public void Close()
        {
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
                catch (Exception e) { lastException = e; UpdateLastError(); }
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
                catch (Exception e) { lastException = e; UpdateLastError(); }

                /* Try to close the device. */
                try
                {
                    /* ... Close and release the pylon device. */
                    if (Pylon.DeviceIsOpen(m_hDevice))
                    {
                        Pylon.DeviceClose(m_hDevice);
                    }
                }
                catch (Exception e) { lastException = e; UpdateLastError(); }

                /* Try to destroy the device. */
                try
                {
                    Pylon.DestroyDevice(m_hDevice);
                }
                catch (Exception e) { lastException = e; UpdateLastError(); }
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
        public void ContinuousShot()
        {
            if (m_open && !m_grabThread.IsAlive)  /* Only start when open and not grabbing already. */
            {
                /* Set up the grabbing and start. */
                m_numberOfBuffersUsed = 5;
                m_grabOnce = false;
                m_grabThreadRun = true;
                m_grabThread = new Thread(Grab);
                m_grabThread.Start();
            }
        }

        /* Stops the grabbing of images. */
        public void Stop()
        {
            if (m_open && m_grabThread.IsAlive) /* Only start when open and grabbing. */
            {
                m_grabThreadRun = false; /* Causes the grab thread to stop. */
                m_grabThread.Join(); /* Wait for it to stop. */
            }
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

        /* Returns the latest image in the grab result queue. All older images are removed. Null is returned if no result is available.
           An image is available when the ImageReady event is fired. */
        public Image GetLatestImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                /* Release all images but the latest. */
                while (m_grabbedBuffers.Count > 1)
                {
                    ReleaseImage();
                }
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
                if (m_grabbedBuffers.Count > 0 ) /* If images are available and grabbing is in progress.*/
                {
                    if (m_grabThreadRun)
                    {
                        /* Requeue the buffer. */
                        Pylon.StreamGrabberQueueBuffer(m_hGrabber, m_grabbedBuffers[0].Handle, 0);
                    }
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
        public void Open(PYLON_DEVICE_HANDLE device)
        {
            try
            {
                /* Use provided device. */
                m_hDevice = device;

                /* Before using the device, it must be opened. Open it for configuring
                parameters and for grabbing images. */
                Pylon.DeviceOpen(m_hDevice, Pylon.cPylonAccessModeControl | Pylon.cPylonAccessModeStream);

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

                /* Disable frame burst start trigger if available */
                if (Pylon.DeviceFeatureIsAvailable(m_hDevice, "EnumEntry_TriggerSelector_FrameBurstStart"))
                {
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerSelector", "FrameBurstStart");
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerMode", "Off");
                }

                /* Disable frame start trigger if available. */
                if (Pylon.DeviceFeatureIsAvailable(m_hDevice, "EnumEntry_TriggerSelector_FrameStart"))
                {
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerSelector", "FrameStart");
                    Pylon.DeviceFeatureFromString(m_hDevice, "TriggerMode", "Off");
                }

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
            }
            catch
            {
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
        }

        /* Prepares everything for grabbing. */
        protected void SetupGrab()
        {
            /* Clear the grab result queue. This is not done when cleaning up to still be able to provide the
             images, e.g. in single frame mode.*/
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Clear();
            }

            /* Set the acquisition mode */
            if (m_grabOnce)
            {
                /* We will use the single frame mode, to take one image. */
                Pylon.DeviceFeatureFromString(m_hDevice, "AcquisitionMode", "SingleFrame");
            }
            else
            {
                /* We will use the Continuous frame mode, i.e., the camera delivers
                images continuously. */
                Pylon.DeviceFeatureFromString(m_hDevice, "AcquisitionMode", "Continuous");
            }

            /* Clear the grab buffers to assure proper operation (because they may
             still be filled if the last grab has thrown an exception). */
            foreach (KeyValuePair<PYLON_STREAMBUFFER_HANDLE, PylonBuffer<Byte>> pair in m_buffers)
            {
                pair.Value.Dispose();
            }
            m_buffers.Clear();

            /* Determine the required size of the grab buffer. */
            uint payloadSize = checked((uint)Pylon.DeviceGetIntegerFeature(m_hDevice, "PayloadSize"));

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
        }

        /* This method is executed using the grab thread and is responsible for grabbing, possible conversion of the image
        ,and queuing the image to the result queue. */
        protected void Grab()
        {
            /* Notify that grabbing has started. This event can be used to update the state of the GUI. */
            OnGrabbingStartedEvent();
            try
            {
                /* Set up everything needed for grabbing. */
                SetupGrab();

                while (m_grabThreadRun) /* Is set to false when stopping to end the grab thread. */
                {
                    /* Wait for the next buffer to be filled. Wait up to 15000 ms. */
                    if (!Pylon.WaitObjectWait(m_hWait, 15000))
                    {
                        lock (m_lockObject)
                        {
                            if (m_grabbedBuffers.Count != m_numberOfBuffersUsed)
                            {
                                /* A timeout occurred. This can happen if an external trigger is used or
                                   if the programmed exposure time is longer than the grab timeout. */
                                throw new Exception("A grab timeout occurred.");
                            }
                            continue;
                        }
                    }

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
                        /* Add result to the ready list. */
                        EnqueueTakenImage(grabResult);

                        /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image
                         and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/
                        OnImageReadyEvent();

                        /* Exit here for single frame mode. */
                        if (m_grabOnce)
                        {
                            m_grabThreadRun = false;
                            break;
                        }
                    } else if (grabResult.Status == EPylonGrabStatus.Failed)
                    {
                        /*
                            Grabbing an image can fail if the used network hardware, i.e. network adapter,
                            switch or Ethernet cable, experiences performance problems.
                            Increase the Inter-Packet Delay to reduce the required bandwidth.
                            It is recommended to enable Jumbo Frames on the network adapter and switch.
                            Adjust the Packet Size on the camera to the highest supported frame size.
                            If this did not resolve the problem, check if the recommended hardware is used.
                            Aggressive power saving settings for the CPU can also cause the image grab to fail.
                        */
                        throw new Exception(string.Format("A grab failure occurred. See the method ImageProvider::Grab for more information. The error code is {0:X08}.", grabResult.ErrorCode));
                    }
                }

                /* Tear down everything needed for grabbing. */
                CleanUpGrab();
            }
            catch (Exception e)
            {
                /* The grabbing stops due to an error. Set m_grabThreadRun to false to avoid that any more buffers are queued for grabbing. */
                m_grabThreadRun = false;

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
        }

        protected void EnqueueTakenImage(PylonGrabResult_t grabResult)
        {
            PylonBuffer<Byte> buffer;  /* Reference to the buffer attached to the grab result. */

            /* Get the buffer from the dictionary. */
            if (!m_buffers.TryGetValue(grabResult.hBuffer, out buffer))
            {
                /* Oops. No buffer available? We should never have reached this point. Since all buffers are
                   in the dictionary. */
                throw new Exception("Failed to find the buffer associated with the handle returned in grab result.");
            }

            /* Create a new grab result to enqueue to the grabbed buffers list. */
            GrabResult newGrabResultInternal = new GrabResult();
            newGrabResultInternal.Handle = grabResult.hBuffer; /* Add the handle to requeue the buffer in the stream grabber queue. */

            bool isBayer8 = PylonHelper.IsBayer8(grabResult.PixelType);
            if (isBayer8 && m_bayer8Conversion == Bayer8Conversion.Raw)
            {
                newGrabResultInternal.ImageData = new Image(grabResult.SizeX, grabResult.SizeY, buffer.Array, grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed);
            }
            else if (grabResult.PixelType == EPylonPixelType.PixelType_Mono8 || grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed)
            {
                newGrabResultInternal.ImageData = new Image(grabResult.SizeX, grabResult.SizeY, buffer.Array, grabResult.PixelType == EPylonPixelType.PixelType_RGBA8packed);
            }
            else /* Conversion is required. */
            {
                /* Create a new format converter if needed. */
                if (!m_hConverter.IsValid)
                {
                    m_convertedBuffers = new Dictionary<PYLON_STREAMBUFFER_HANDLE,PylonBuffer<byte>>(); /* Create a new dictionary for the converted buffers. */
                    m_hConverter = Pylon.ImageFormatConverterCreate(); /* Create the converter. */

                    // We force all non 8-bit Bayer formats to color.
                    // For Bayer 8, user may choose Raw, Mono or Color.
                    bool isBayer = Pylon.IsBayer(grabResult.PixelType);
                    bool bayerColor = (isBayer && !isBayer8) || (isBayer8 && m_bayer8Conversion == Bayer8Conversion.Color);
                    m_converterOutputFormatIsColor = !Pylon.IsMono(grabResult.PixelType) || bayerColor;
                }
                /* Reference to the buffer attached to the grab result handle. */
                PylonBuffer<Byte> convertedBuffer = null;
                /* Look up if a buffer is already attached to the handle. */
                bool bufferListed = m_convertedBuffers.TryGetValue(grabResult.hBuffer, out convertedBuffer);
                /* Perform the conversion. If the buffer is null a new one is automatically created. */
                Pylon.ImageFormatConverterSetOutputPixelFormat(m_hConverter, m_converterOutputFormatIsColor ? EPylonPixelType.PixelType_BGRA8packed : EPylonPixelType.PixelType_Mono8);
                Pylon.ImageFormatConverterConvert(m_hConverter, ref convertedBuffer, buffer, grabResult.PixelType, (uint)grabResult.SizeX, (uint)grabResult.SizeY, (uint)grabResult.PaddingX, EPylonImageOrientation.ImageOrientation_TopDown);
                if (!bufferListed) /* A new buffer has been created. Add it to the dictionary. */
                {
                    m_convertedBuffers.Add(grabResult.hBuffer, convertedBuffer);
                }
                /* Add the image data. */
                newGrabResultInternal.ImageData = new Image(grabResult.SizeX, grabResult.SizeY, convertedBuffer.Array, m_converterOutputFormatIsColor);
            }
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Add(newGrabResultInternal); /* Add the new grab result to the queue. */
            }
        }

        protected void CleanUpGrab()
        {
            /*  ... Stop the camera. */
            Pylon.DeviceExecuteCommandFeature(m_hDevice, "AcquisitionStop");

            /* Destroy the format converter if one was used. */
            if (m_hConverter.IsValid)
            {
                /* Destroy the converter. */
                Pylon.ImageFormatConverterDestroy(m_hConverter);
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
                    PylonGrabResult_t grabResult;  /* Stores the result of a grab operation. */
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
        public delegate void DeviceOpenedEventHandler();
        public event DeviceOpenedEventHandler DeviceOpenedEvent;

        public delegate void DeviceClosingEventHandler();
        public event DeviceClosingEventHandler DeviceClosingEvent;

        public delegate void DeviceClosedEventHandler();
        public event DeviceClosedEventHandler DeviceClosedEvent;

        public delegate void GrabbingStartedEventHandler();
        public event GrabbingStartedEventHandler GrabbingStartedEvent;

        public delegate void ImageReadyEventHandler();
        public event ImageReadyEventHandler ImageReadyEvent;

        public delegate void GrabbingStoppedEventHandler();
        public event GrabbingStoppedEventHandler GrabbingStoppedEvent;

        public delegate void GrabErrorEventHandler(Exception grabException, string additionalErrorMessage);
        public event GrabErrorEventHandler GrabErrorEvent;

        public delegate void DeviceRemovedEventHandler();
        public event DeviceRemovedEventHandler DeviceRemovedEvent;

        /* Notify that ImageProvider is open and ready for grabbing and configuration. */
        protected void OnDeviceOpenedEvent()
        {
            m_open = true;
            if (DeviceOpenedEvent != null)
            {
                DeviceOpenedEvent();
            }
        }

        /* Notify that ImageProvider is about to close the device to give other objects the chance to do clean up operations. */
        protected void OnDeviceClosingEvent()
        {
            m_open = false;
            if (DeviceClosingEvent != null)
            {
                DeviceClosingEvent();
            }
        }

        /* Notify that ImageProvider is now closed.*/
        protected void OnDeviceClosedEvent()
        {
            m_open = false;
            if (DeviceClosedEvent != null)
            {
                DeviceClosedEvent();
            }
        }

        /* Notify that grabbing has started. This event could be used to update the state of the GUI. */
        protected void OnGrabbingStartedEvent()
        {
            if (GrabbingStartedEvent != null)
            {
                GrabbingStartedEvent();
            }
        }

        /* Notify that an image has been added to the output queue. The receiver of the event can use GetCurrentImage() to acquire and process the image
         and ReleaseImage() to remove the image from the queue and return it to the stream grabber.*/
        protected void OnImageReadyEvent()
        {
            if (ImageReadyEvent != null)
            {
                ImageReadyEvent();
            }
        }

        /* Notify that grabbing has stopped. This event could be used to update the state of the GUI. */
        protected void OnGrabbingStoppedEvent()
        {
            if (GrabbingStoppedEvent != null)
            {
                GrabbingStoppedEvent();
            }
        }

         /* Notify that the grabbing had errors and deliver the information. */
        protected void OnGrabErrorEvent(Exception grabException, string additionalErrorMessage)
        {
            if (GrabErrorEvent != null)
            {
                GrabErrorEvent(grabException, additionalErrorMessage);
            }
        }

        /* Notify that the device has been removed from the PC. */
        protected void OnDeviceRemovedEvent()
        {
            m_removed = true;
            m_grabThreadRun = false;
            if (DeviceRemovedEvent != null)
            {
                DeviceRemovedEvent();
            }
        }
    }
}
