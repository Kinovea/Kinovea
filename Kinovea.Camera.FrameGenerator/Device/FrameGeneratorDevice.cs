using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using Kinovea.Pipeline;
using Kinovea.Services;

namespace Kinovea.Camera.FrameGenerator
{
    /// <summary>
    /// Software-defined camera. 
    /// Tries to match what a real camera integration code would do.
    /// </summary>
    public class FrameGeneratorDevice
    {
        #region Events
        public event EventHandler<FrameProducedEventArgs> FrameProduced;
        public event EventHandler<FrameErrorEventArgs> FrameError;
        public event EventHandler GrabbingStarted;
        #endregion

        #region Properties
        public ImageDescriptor ImageDescriptor
        {
            get { return imageDescriptor; } 
        }
        public DeviceConfiguration Configuration
        {
            get { return configuration; }
            set
            {
                configuration = value;
                UpdateImageDescriptor();
            }
        }
        #endregion

        #region Members
        private DeviceConfiguration configuration = DeviceConfiguration.Default;
        private bool topDown = true;
        private ImageDescriptor imageDescriptor;
        private Thread grabThread;
        private Generator generator;
        private long generatedFrames;
        private ManualResetEvent cancellationEvent = null;
        private Stopwatch stopwatch = new Stopwatch();
        private double frameIntervalMilliseconds;
        private double dueTime;

        private NativeMethods.TimerCallback timerCallback;
        private uint timerId;
        #endregion

        #region Public methods
        public FrameGeneratorDevice()
        {
            configuration = DeviceConfiguration.Default;
            UpdateImageDescriptor();

            grabThread = new Thread(Grab);
            timerCallback = TimerCallback_Tick;
        }

        public void Start()
        {
            if (generator != null)
                generator.Dispose();

            generator = new Generator(configuration);
            cancellationEvent = new ManualResetEvent(false);
            frameIntervalMilliseconds = 1000.0 / configuration.Framerate;
            dueTime = 1 * frameIntervalMilliseconds;
            stopwatch.Start();
            grabThread.Start();
        }

        public void Stop()
        {
            stopwatch.Stop();
            cancellationEvent?.Set();

            if (grabThread != null && grabThread.IsAlive)
            {
                grabThread.Join();
            }
        }

        /// <summary>
        /// Helper method to create a full bitmap from the current frame buffer.
        /// Used in the context of thumbnail creation.
        /// Returns an RGB24 Bitmap.
        /// Creates a copy of the frame buffer and is safe to use downstream.
        /// </summary>
        public Bitmap GetCurrentBitmap()
        {
            // This still runs in the grabbing thread.
            // In theory we should get the generator frame, and possibly convert it from JPEG into RGB24.
            // For simplicity, since we now the source is blank, just return a blank bitmap.
            return new Bitmap(imageDescriptor.Width, imageDescriptor.Height, PixelFormat.Format24bppRgb); ;
        }
        #endregion

        #region Private methods

        private void UpdateImageDescriptor()
        {
            int bufferSize = ImageFormatHelper.ComputeBufferSize(configuration.Width, configuration.Height, configuration.ImageFormat);
            imageDescriptor = new ImageDescriptor(configuration.ImageFormat, configuration.Width, configuration.Height, topDown, bufferSize);
        }

        /// <summary>
        /// The main grabbing thread method.
        /// </summary>
        private void Grab()
        {
            try
            {
                Thread.CurrentThread.Name = "Grabber - Frame generator";
                if (GrabbingStarted != null)
                    GrabbingStarted(this, EventArgs.Empty);

                StartMultimediaTimer();

                cancellationEvent.WaitOne();

                StopMultimediaTimer();
            }
            catch (Exception e)
            {
                if (FrameError != null)
                    FrameError(this, new FrameErrorEventArgs(e.Message));
            }
        }

        private void StartMultimediaTimer()
        {
            uint intervalMilliseconds = 1;

            uint eventType = NativeMethods.TIME_PERIODIC | NativeMethods.TIME_KILL_SYNCHRONOUS;
            timerId = NativeMethods.timeSetEvent(intervalMilliseconds, intervalMilliseconds, timerCallback, UIntPtr.Zero, eventType);
            if (timerId == 0)
                throw new Exception("timeSetEvent error");
        }

        private void StopMultimediaTimer()
        {
            if (timerId != 0)
                NativeMethods.timeKillEvent(timerId);

            timerId = 0;
        }

        private void TimerCallback_Tick(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            if (stopwatch.Elapsed.TotalMilliseconds < dueTime)
                return;

            Frame frame = generator.GetFrame();
            generatedFrames++;

            dueTime = (generatedFrames + 1) * frameIntervalMilliseconds;

            if (FrameProduced == null)
                return;

            if (frame == null)
                FrameProduced(this, new FrameProducedEventArgs(null, 0));
            else
                FrameProduced(this, new FrameProducedEventArgs(frame.Buffer, frame.PayloadLength));
        }

        #endregion
    }
}
