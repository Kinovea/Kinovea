using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BGAPI2;
using Kinovea.Pipeline;
using Kinovea.Services;

namespace Kinovea.Camera.GenICam
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
        private GenICamProvider genicamProvider = new GenICamProvider();
        private bool demosaicing = false;
        private bool isJpeg = false;
        private ImageFormat imageFormat = ImageFormat.None;
        private bool grabbing;
        private bool firstOpen = true;
        private float resultingFramerate = 0;
        private Finishline finishline = new Finishline();
        private int frameBufferSize = 0;
        private byte[] frameBuffer;
        private BufferProcessor bufferProcessor = new BufferProcessor();

        // Diagnostics & debug.
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
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

            if (!genicamProvider.IsOpen)
                return ImageDescriptor.Invalid;

            firstOpen = false;
            Device device = genicamProvider.Device;

            // Get the configured framerate for recording support.
            resultingFramerate = CameraPropertyManager.GetResultingFramerate(device);

            bool hasWidth = CameraPropertyManager.NodeIsReadable(device, "Width");
            bool hasHeight = CameraPropertyManager.NodeIsReadable(device, "Height");
            bool hasPixelFormat = CameraPropertyManager.NodeIsReadable(device, "PixelFormat");
            bool canComputeImageDescriptor = hasWidth && hasHeight && hasPixelFormat;

            if (!canComputeImageDescriptor)
                return ImageDescriptor.Invalid;

            int width = CameraPropertyManager.ReadInteger(device, "Width");
            int height = CameraPropertyManager.ReadInteger(device, "Height");
            string pixelFormat = CameraPropertyManager.ReadString(device, "PixelFormat");

            // We output in three possible formats: Y800, RGB24 or JPEG.
            // The output format depends on the stream format and debayering options.
            // This switch will update the `imageFormat` global variable.
            SetImageFormat(device, pixelFormat);
            bufferProcessor.Prepare();

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

        private void SetImageFormat(Device device, string pixelFormat)
        {
            if (device.Vendor == "Baumer")
            {
                // Hardware JPEG compression.
                isJpeg = specific.Compression;
                if (CameraPropertyManager.SupportsJPEG(device))
                {
                    if (CameraPropertyManager.FormatCanCompress(device, pixelFormat))
                    {
                        CameraPropertyManager.SetJPEG(device, isJpeg);
                    }
                    else
                    {
                        CameraPropertyManager.SetJPEG(device, false);
                        isJpeg = false;
                    }
                }
                else
                {
                    isJpeg = false;
                }
            }

            // Software debayering.
            demosaicing = specific.Demosaicing;
            imageFormat = BufferProcessor.GetImageFormat(pixelFormat, isJpeg, demosaicing);
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

            if (!genicamProvider.IsOpen)
                Open();

            if (!genicamProvider.IsOpen)
                return;

            log.DebugFormat("Starting device {0}, {1}", summary.Alias, summary.Identifier);

            genicamProvider.BufferProduced += GenICamProvider_BufferProduced;

            try
            {
                genicamProvider.AcquireContinuous();

                grabbing = true;
                GrabbingStatusChanged?.Invoke(this, EventArgs.Empty);
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

            genicamProvider.BufferProduced -= GenICamProvider_BufferProduced;

            try
            {
                genicamProvider.Stop();
            }
            catch (Exception e)
            {
                LogError(e, "");
            }

            grabbing = false;
            GrabbingStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            Stop();

            try
            {
                genicamProvider.Close();
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
                genicamProvider.Open(specific.Device);
                CameraPropertyManager.AfterOpen(genicamProvider.Device);
            }
            catch (Exception e)
            {
                log.Error("Could not open GenICam device.");
                LogError(e, "");
                return;
            }

            if (!genicamProvider.IsOpen)
                return;

            // Store the device into the specific info so that we can retrieve device informations from the configuration dialog.
            specific.Device = genicamProvider.Device;
            
            if (!string.IsNullOrEmpty(specific.StreamFormat))
                CameraPropertyManager.WriteEnum(specific.Device, "PixelFormat", specific.StreamFormat);

            if (firstOpen)
            {
                // Restore camera parameters from the XML blurb.
                // Regular properties, including image size.
                // First we read the current properties from the API to get fully formed properties.
                // We merge the values saved in the XML into the properties.
                // (The restoration from the XML doesn't create fully formed properties, it just contains the values).
                // Then commit the properties to the camera.
                Dictionary<string, CameraProperty> cameraProperties = CameraPropertyManager.ReadAll(specific.Device, summary.Identifier);
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
        private void GenICamProvider_BufferProduced(object sender, BufferEventArgs e)
        {
            BGAPI2.Buffer buffer = e.Buffer;
            if (buffer == null || buffer.IsIncomplete || buffer.MemPtr == IntPtr.Zero)
            {
                return;
            }

            // Initialize the payload length with the incoming buffer size.
            // It may be updated up or down if we do a pixel format conversion.
            int payloadLength = (int)buffer.SizeFilled;

            // Fill our byte buffer from BGAPI buffer
            bufferProcessor.Process(frameBuffer, imageFormat, buffer, out payloadLength);

            // At this point we have the payload in our frameBuffer variable.
            if (imageFormat != ImageFormat.JPEG && finishline.Enabled)
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
                ComputeDataRate(payloadLength);

                if (FrameProduced != null)
                    FrameProduced(this, new FrameProducedEventArgs(frameBuffer, payloadLength));
            }
        }


        private void LogError(Exception e, string additionalErrorMessage)
        {
            log.ErrorFormat("Error during GenICam camera operation. {0}", summary.Alias);
            log.Error(e.ToString());
            log.Error(additionalErrorMessage);
        }
        #endregion
    }
}

