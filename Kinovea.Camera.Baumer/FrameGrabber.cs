using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Kinovea.Pipeline;
using Kinovea.Video;
using Kinovea.Base;
using BGAPI2;

namespace Kinovea.Camera.Baumer
{
    public class FrameGrabber : ICaptureSource
    {
        public event EventHandler GrabbingStatusChanged;
        public event EventHandler<FrameProducedEventArgs> FrameProduced;

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
        private SpecificInfo specific;
        private BaumerProvider baumerProvider = new BaumerProvider();
        //private PYLON_DEVICE_HANDLE deviceHandle;
        //private ImageProvider imageProvider = new ImageProvider();
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
        public FrameGrabber(CameraSummary summary)
        {
            this.summary = summary;
            this.specific = summary.Specific as SpecificInfo;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            Open();

            if (!baumerProvider.IsOpen)
                return ImageDescriptor.Invalid;

            firstOpen = false;

            // Get the configured framerate for recording support.
            resultingFramerate = baumerProvider.GetResultingFramerate();

            Device device = baumerProvider.Device;
            bool hasWidth = BaumerHelper.NodeIsReadable(device, "Width");
            bool hasHeight = BaumerHelper.NodeIsReadable(device, "Height");
            bool hasPixelFormat = BaumerHelper.NodeIsReadable(device, "PixelFormat");
            bool canComputeImageDescriptor = hasWidth && hasHeight && hasPixelFormat;

            if (!canComputeImageDescriptor)
                return ImageDescriptor.Invalid;

            int width = BaumerHelper.GetInteger(device, "Width");
            int height = BaumerHelper.GetInteger(device, "Height");
            string pixelFormat = BaumerHelper.GetString(device, "PixelFormat");

            
            
            // For now only support RGB24.
            // TODO: read whatever is the currently configured format.
            // Determine if we will end up as RGB24 or Y800.
            // Support finishline mode.


            //ImageFormat format = BaumerHelper.ConvertImageFormat(pixelFormat, bayerMode);
            ImageFormat format = ImageFormat.RGB24;

            incomingBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            incomingBuffer = new byte[incomingBufferSize];

            int outgoingBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, format);
            bool topDown = false;

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
            // Register grabbing events and start continuous capture.

            if (!baumerProvider.IsOpen)
                Open();

            if (!baumerProvider.IsOpen)
                return;

            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);

            baumerProvider.BufferProduced += BaumerProvider_BufferProduced;

            try
            {
                baumerProvider.AcquireContinuous();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }
        }

        public void Stop()
        {
            // Stop continous capture and unregister events.

            log.DebugFormat("Stopping device {0}", summary.Alias);

            baumerProvider.BufferProduced -= BaumerProvider_BufferProduced;

            try
            {
                baumerProvider.Stop();
            }
            catch (Exception e)
            {
                LogError(e, "");
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
                baumerProvider.Close();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }
        }
        #endregion

        #region Private methods

        private void Open()
        {
            if (grabbing)
                Stop();

            try
            {
                baumerProvider.Open(specific.SystemKey, specific.InterfaceKey, specific.DeviceKey);
            //    imageProvider.Open(deviceHandle);
            }
            catch (Exception e)
            {
                log.Error("Could not open Baumer device.");
                LogError(e, "");
                return;
            }

            //if (!deviceHandle.IsValid)
            //    return;

            //SpecificInfo specific = summary.Specific as SpecificInfo;
            //if (specific == null)
            //    return;

            //// Store the handle into the specific info so that we can retrieve device informations from the configuration dialog.
            //specific.Handle = deviceHandle;
            //GenApiEnum currentStreamFormat = PylonHelper.ReadEnumCurrentValue(deviceHandle, "PixelFormat");

            //if (!string.IsNullOrEmpty(specific.StreamFormat) && specific.StreamFormat != currentStreamFormat.Symbol)
            //    PylonHelper.WriteEnum(deviceHandle, "PixelFormat", specific.StreamFormat);

            //// The bayer conversion mode will be set during Prepare().

            if (firstOpen)
            {
                // Restore camera parameters from the XML blurb.
                // Regular properties, including image size.
                // First we read the current properties from the API to get fully formed properties.
                // We merge the values saved in the XML into the properties.
                // (The restoration from the XML doesn't create fully formed properties, it just contains the values).
                // Then commit the properties to the camera.
                //Dictionary<string, CameraProperty> cameraProperties = CameraPropertyManager.Read(deviceHandle, summary.Identifier);
            //    CameraPropertyManager.MergeProperties(cameraProperties, specific.CameraProperties);
            //    specific.CameraProperties = cameraProperties;
            //    CameraPropertyManager.WriteCriticalProperties(deviceHandle, specific.CameraProperties);
            }
            //else
            //{
            //    CameraPropertyManager.WriteCriticalProperties(deviceHandle, specific.CameraProperties);
            //}
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
        //private void imageProvider_GrabbingStartedEvent()
        //{
        //    grabbing = true;

        //    if (GrabbingStatusChanged != null)
        //        GrabbingStatusChanged(this, EventArgs.Empty);
        //}

        private void BaumerProvider_BufferProduced(object sender, BufferEventArgs e)
        {
            log.DebugFormat("Received frame from Baumer camera");

            // TODO: copy/convert the frame and raise FrameProduced with it.

            BGAPI2.Buffer buffer = e.Buffer;
            if (buffer == null || buffer.IsIncomplete || buffer.MemPtr == IntPtr.Zero)
                return;

            ProduceRGB24(buffer);

            //if (finishline.Enabled)
            //{
            //    bool flush = finishline.Consolidate(pylonImage.Buffer);
            //    imageProvider.ReleaseImage();

            //    if (flush)
            //    {
            //        ComputeDataRate(finishline.BufferOutput.Length);

            //        if (FrameProduced != null)
            //            FrameProduced(this, new FrameProducedEventArgs(finishline.BufferOutput, finishline.BufferOutput.Length));
            //    }
            //}
            //else
            //{
            //ComputeDataRate(pylonImage.Buffer.Length);

            //if (FrameProduced != null)
            //  FrameProduced(this, new FrameProducedEventArgs(pylonImage.Buffer, pylonImage.Buffer.Length));

            //imageProvider.ReleaseImage();
            //}
        }

        //private void imageProvider_GrabErrorEvent(Exception grabException, string additionalErrorMessage)
        //{
        //    LogError(grabException, additionalErrorMessage);
        //}

        //private void imageProvider_DeviceRemovedEvent()
        //{

        //}

        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error during Baumer camera operation. {0}", summary.Alias);
            log.Error(e.ToString());
            log.Error(additionalErrorMessage);
        }
        #endregion

        private unsafe void ProduceRGB24(BGAPI2.Buffer buffer)
        {
            bool filled = false;
            ulong width = buffer.Width;
            ulong height = buffer.Height;
            string pixFmt = buffer.PixelFormat;
            IntPtr byteBuffer = buffer.MemPtr;
            ulong byteCount = buffer.MemSize;

            // Todo: avoid copies.

            BGAPI2.ImageProcessor imgProcessor = new BGAPI2.ImageProcessor();
            if (imgProcessor.NodeList.GetNodePresent("DemosaicingMethod") == true)
            {
                imgProcessor.NodeList["DemosaicingMethod"].Value = "NearestNeighbor";
                //imgProcessor.NodeList["DemosaicingMethod"].Value = "Bilinear3x3";
            }

            //BGAPI2.Node pixelFormatInfoSelector = imgProcessor.NodeList["PixelFormatInfoSelector"];
            BGAPI2.Node bytesPerPixel = imgProcessor.NodeList["BytesPerPixel"];
            long bpp = bytesPerPixel.IsAvailable ? bytesPerPixel.Value.ToLong() : 1;

            // Demosaicing of the image.
            // TODO: only do this if the image is a Bayer pattern.
            // How can we avoid copies here?
            // Is an image a simple wrapper around the MemPtr or does it make a copy?
            BGAPI2.Image img = imgProcessor.CreateImage((uint)width, (uint)height, pixFmt, byteBuffer, byteCount);
            BGAPI2.Image img2 = imgProcessor.CreateTransformedImage(img, "BGR8");



            fixed (byte* p = incomingBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                NativeMethods.memcpy(ptrDst.ToPointer(), img2.Buffer.ToPointer(), (int)width * 3 * (int)height);
            }

            ComputeDataRate(incomingBufferSize);

            if (FrameProduced != null)
                FrameProduced(this, new FrameProducedEventArgs(incomingBuffer, incomingBufferSize));


        }

    }
}

