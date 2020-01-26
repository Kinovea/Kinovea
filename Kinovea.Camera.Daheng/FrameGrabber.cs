using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GxIAPINET;
using System.Runtime.InteropServices;
using Kinovea.Base;
using Kinovea.Pipeline;
using Kinovea.Video;

namespace Kinovea.Camera.Daheng
{
    /// <summary>
    /// Main grabbing class for Daheng Imaging devices.
    /// </summary>
    public class FrameGrabber : ICaptureSource
    {
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler GrabbingStatusChanged;

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
        private IGXFactory igxFactory;
        private bool grabbing;
        private bool firstOpen = true;
        private float resultingFramerate = 0;
        private Finishline finishline = new Finishline();
        private Stopwatch swDataRate = new Stopwatch();
        private Averager dataRateAverager = new Averager(0.02);
        private const double megabyte = 1024 * 1024;
        private int incomingBufferSize = 0;
        private byte[] incomingBuffer;

        private IGXDevice device;
        private IGXFeatureControl featureControl;
        private IGXStream stream;
        private int width;
        private int height;
        private bool isColor;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public methods
        public FrameGrabber(CameraSummary summary, IGXFactory igxFactory)
        {
            this.summary = summary;
            this.igxFactory = igxFactory;
        }

        /// <summary>
        /// Configure device and report frame format that will be used during streaming.
        /// This method must return a proper ImageDescriptor so we can pre-allocate buffers.
        /// </summary>
        public ImageDescriptor Prepare()
        {
            Open();

            if (device == null || featureControl == null)
                return ImageDescriptor.Invalid;

            firstOpen = false;
            resultingFramerate = (float)DahengHelper.GetResultingFramerate(device);

            width = (int)featureControl.GetIntFeature("Width").GetValue();
            height = (int)featureControl.GetIntFeature("Height").GetValue();
            isColor = DahengHelper.IsColor(featureControl);

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
            if (device == null || stream == null || featureControl == null)
                return;

            stream.RegisterCaptureCallback(this, stream_OnFrame);
            stream.StartGrab();
            featureControl.GetCommandFeature("AcquisitionStart").Execute();
            grabbing = true;

            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (device == null)
                return;

            try
            {
                if (featureControl != null)
                {
                    featureControl.GetCommandFeature("AcquisitionStop").Execute();
                }

                if (stream != null)
                {
                    stream.StopGrab();
                    stream.UnregisterCaptureCallback();
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }

            grabbing = false;
            if (GrabbingStatusChanged != null)
                GrabbingStatusChanged(this, EventArgs.Empty);
        }

        public void Close()
        {
            if (device == null)
                return;

            try
            {
                if (featureControl != null)
                {
                    featureControl.GetCommandFeature("AcquisitionStop").Execute();
                    featureControl = null;
                }

                if (stream != null)
                {
                    stream.StopGrab();
                    stream.UnregisterCaptureCallback();
                    stream.Close();
                    stream = null;
                }

                if (device != null)
                {
                    device.Close();
                    device = null;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
            }
        }
        #endregion

        private void Open()
        {
            if (device != null)
                Close();

            bool open = false;
            try
            {
                device = igxFactory.OpenDeviceBySN(summary.Identifier, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                featureControl = device.GetRemoteFeatureControl();
                DahengHelper.AfterOpen(featureControl);
                open = true;
            }
            catch
            {
                log.DebugFormat("Could not open Daheng device.");
            }

            if (!open)
                return;

            SpecificInfo specific = summary.Specific as SpecificInfo;
            if (specific == null)
                return;

            // Store the camera object into the specific info so that we can retrieve device informations from the configuration dialog.
            specific.Device = device;

            if (firstOpen)
            {
                // Grab current values.
                Dictionary<string, CameraProperty> cameraProperties = CameraPropertyManager.Read(device);
                specific.CameraProperties = cameraProperties;
            }
            else
            {
                CameraPropertyManager.WriteCriticalProperties(device, specific.CameraProperties);
            }

            try
            {
                stream = device.OpenStream(0);
            }
            catch
            {
                log.DebugFormat("Could not start Daheng device.");
            }
        }

        private void ComputeDataRate(int bytes)
        {
            double rate = ((double)bytes / megabyte) / swDataRate.Elapsed.TotalSeconds;
            dataRateAverager.Post(rate);
            swDataRate.Reset();
            swDataRate.Start();
        }

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
                            //IntPtr pBufferMono = IntPtr.Zero;
                            //if (IsPixelFormat8(objIBaseData.GetPixelFormat()))
                            //{
                            //    pBufferMono = objIBaseData.GetBuffer();
                            //}
                            //else
                            //{
                            //    pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                            //}

                            //Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, width * height);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private unsafe void FillRGB24(IntPtr buffer)
        {
            fixed (byte* p = incomingBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                NativeMethods.memcpy(ptrDst.ToPointer(), buffer.ToPointer(), width * 3 * height);
            }

            ComputeDataRate(incomingBufferSize);

            if (FrameProduced != null)
                FrameProduced(this, new FrameProducedEventArgs(incomingBuffer, incomingBufferSize));
        }
    }
}
