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
        BGAPI2.ImageProcessor imgProcessor = new BGAPI2.ImageProcessor();
        private bool demosaicing = true;
        private ImageFormat imageFormat = ImageFormat.None;
        private bool grabbing;
        private bool firstOpen = true;
        private float resultingFramerate = 0;
        private Finishline finishline = new Finishline();
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private int frameBufferSize = 0;
        private byte[] frameBuffer;
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
            Device device = baumerProvider.Device;

            // Get the configured framerate for recording support.
            resultingFramerate = BaumerHelper.GetResultingFramerate(device);

            bool hasWidth = BaumerHelper.NodeIsReadable(device, "Width");
            bool hasHeight = BaumerHelper.NodeIsReadable(device, "Height");
            bool hasPixelFormat = BaumerHelper.NodeIsReadable(device, "PixelFormat");
            bool canComputeImageDescriptor = hasWidth && hasHeight && hasPixelFormat;

            if (!canComputeImageDescriptor)
                return ImageDescriptor.Invalid;

            int width = BaumerHelper.GetInteger(device, "Width");
            int height = BaumerHelper.GetInteger(device, "Height");
            string pixelFormat = BaumerHelper.GetString(device, "PixelFormat");
             
            // We only output in two possible formats: Y800 or RGB24.
            // The output format depends on the input format and the demosaicing option.
            // Mono or raw -> Y800, Otherwise -> RGB24.
            demosaicing = specific.Demosaicing;
            if (demosaicing)
            {
                if (imgProcessor.NodeList.GetNodePresent("DemosaicingMethod"))
                {
                    // Options: NearestNeighbor, Bilinear3x3, Baumer5x5
                    imgProcessor.NodeList["DemosaicingMethod"].Value = "NearestNeighbor";
                }
                else
                {
                    demosaicing = false;
                }
            }
            
            imageFormat = BaumerHelper.ConvertImageFormat(pixelFormat, demosaicing);
            frameBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, imageFormat);
            frameBuffer = new byte[frameBufferSize];

            finishline.Prepare(width, height, imageFormat, resultingFramerate);
            if (finishline.Enabled)
            {
                height = finishline.Height;
                resultingFramerate = finishline.ResultingFramerate;
            }

            int outgoingBufferSize = ImageFormatHelper.ComputeBufferSize(width, height, imageFormat);
            bool topDown = true;
            return new ImageDescriptor(imageFormat, width, height, topDown, outgoingBufferSize);
        }

        /// <summary>
        /// In case of configure failure, we would have retrieved a single image and the corresponding image descriptor.
        /// A limitation of the single snapshot retriever is that the format is always RGB24, even though the grabber may
        /// use a different format.
        /// </summary>
        public ImageDescriptor GetPrepareFailedImageDescriptor(ImageDescriptor input)
        {
            frameBufferSize = ImageFormatHelper.ComputeBufferSize(input.Width, input.Height, input.Format);
            frameBuffer = new byte[frameBufferSize];

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
            }
            catch (Exception e)
            {
                log.Error("Could not open Baumer device.");
                LogError(e, "");
                return;
            }

            if (!baumerProvider.IsOpen)
                return;

            // Store the device into the specific info so that we can retrieve device informations from the configuration dialog.
            specific.Device = baumerProvider.Device;

            if (!string.IsNullOrEmpty(specific.StreamFormat))
                BaumerHelper.WriteEnum(specific.Device, "PixelFormat", specific.StreamFormat);

            if (firstOpen)
            {
                // Restore camera parameters from the XML blurb.
                // Regular properties, including image size.
                // First we read the current properties from the API to get fully formed properties.
                // We merge the values saved in the XML into the properties.
                // (The restoration from the XML doesn't create fully formed properties, it just contains the values).
                // Then commit the properties to the camera.
                Dictionary<string, CameraProperty> cameraProperties = CameraPropertyManager.Read(specific.Device, summary.Identifier);
                CameraPropertyManager.MergeProperties(cameraProperties, specific.CameraProperties);
                specific.CameraProperties = cameraProperties;
                CameraPropertyManager.WriteCriticalProperties(specific.Device, specific.CameraProperties);
            }
            else
            {
                CameraPropertyManager.WriteCriticalProperties(specific.Device, specific.CameraProperties);
            }
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
            BGAPI2.Buffer buffer = e.Buffer;
            if (buffer == null || buffer.IsIncomplete || buffer.MemPtr == IntPtr.Zero)
                return;

            // Wrap the buffer in an image, convert if needed.
            BGAPI2.Image image = imgProcessor.CreateImage((uint)buffer.Width, (uint)buffer.Height, buffer.PixelFormat, buffer.MemPtr, buffer.MemSize);
            if (imageFormat != ImageFormat.Y800 || !BaumerHelper.IsY800(image.PixelFormat))
            {
                // Color conversion is required.
                BGAPI2.Image transformedImage = GetTransformedImage(image);
                image.Release();
                image = transformedImage;
            }
            
            CopyFrame(image);
            image.Release();

            if (finishline.Enabled)
            {
                bool flush = finishline.Consolidate(frameBuffer);
                if (flush)
                {
                    ComputeDataRate(finishline.BufferOutput.Length);

                    if (FrameProduced != null)
                        FrameProduced(this, new FrameProducedEventArgs(finishline.BufferOutput, finishline.BufferOutput.Length));
                }
            }
            else
            {
                ComputeDataRate(frameBufferSize);

                if (FrameProduced != null)
                    FrameProduced(this, new FrameProducedEventArgs(frameBuffer, frameBufferSize));
            }
        }


        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error during Baumer camera operation. {0}", summary.Alias);
            log.Error(e.ToString());
            log.Error(additionalErrorMessage);
        }
        #endregion

        private BGAPI2.Image GetTransformedImage(BGAPI2.Image image)
        {
            BGAPI2.Image transformedImage = null;
            if (!demosaicing && image.PixelFormat.StartsWith("Bayer"))
            {
                // HDR Bayer. Convert to 8-bit while retaining the format.
                // Transformation from a bayer pattern to another is not supported by the API.
                if (image.PixelFormat.StartsWith("BayerBG"))
                    transformedImage = imgProcessor.CreateTransformedImage(image, "BayerBG8");
                else if (image.PixelFormat.StartsWith("BayerGB"))
                    transformedImage = imgProcessor.CreateTransformedImage(image, "BayerGB8");
                else if (image.PixelFormat.StartsWith("BayerGR"))
                    transformedImage = imgProcessor.CreateTransformedImage(image, "BayerGR8");
                else
                    transformedImage = imgProcessor.CreateTransformedImage(image, "BayerRG8");
            }
            else
            {
                // HDR Mono and all other cases (RGB & YUV).
                if (image.PixelFormat.StartsWith("Mono"))
                    transformedImage = imgProcessor.CreateTransformedImage(image, "Mono8");
                else
                    transformedImage = imgProcessor.CreateTransformedImage(image, "BGR8");
            }

            return transformedImage;
        }

        /// <summary>
        /// Takes a converted input buffer and copy it into the output buffer.
        /// </summary>
        private unsafe void CopyFrame(BGAPI2.Image image)
        {
            // At this point the image is either in Mono8, Bayer**8 or BGR8.
            int bpp = image.PixelFormat == "BGR8" ? 3 : 1;
            fixed (byte* p = frameBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                NativeMethods.memcpy(ptrDst.ToPointer(), image.Buffer.ToPointer(), (int)(image.Width * bpp * image.Height));
            }
        }
    }
}

