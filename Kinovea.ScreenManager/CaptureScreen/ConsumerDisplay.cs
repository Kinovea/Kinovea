using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kinovea.Pipeline.MemoryLayout;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Kinovea.Pipeline;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Pipeline consumer wrapping the main UI thread.
    /// </summary>
    public class ConsumerDisplay : IFrameConsumer
    {
        public bool Started
        {
            get { return true; }
        }

        public bool Active
        {
            get { return true; }
        }
        
        public long ConsumerPosition
        {
            get 
            {
                // Always report that we are up to date so we never clog the pipe.
                return buffer.ProducerPosition;
            }
        }

        public Frame Frame
        {
            get { return frame; }
        }

        public long Ellapsed { get; private set; }

        private RingBuffer buffer;
        private ImageDescriptor imageDescriptor;
        private Frame frame;
        private bool allocated;
        private Stopwatch stopwatch = new Stopwatch();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ConsumerDisplay()
        {
            stopwatch.Start();
        }

        public void Run()
        {
            throw new NotSupportedException();
        }

        public void SetRingBuffer(RingBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void ClearRingBuffer()
        {
            buffer = null;
        }

        public void Activate()
        {
            throw new NotSupportedException();
        }

        public void Deactivate()
        {
            throw new NotSupportedException();
        }

        public void SetImageDescriptor(ImageDescriptor imageDescriptor)
        {
            allocated = false;

            try
            {
                this.imageDescriptor = imageDescriptor;
                int bufferSize = ImageFormatHelper.ComputeBufferSize(imageDescriptor.Width, imageDescriptor.Height, imageDescriptor.Format);
                frame = new Frame(bufferSize);
                GC.Collect();

                allocated = true;
            }
            catch (Exception e)
            {
                log.Error("The buffer could not be allocated.");
                log.Error(e);
            }
        }

        /// <summary>
        /// Consume a single frame and return immediately.
        /// Publish the Bitmap in Bitmap property.
        /// Returns immediately if no frame are available in the pipe.
        /// This should be called after the main thread received a notification of a frame event from the camera.
        /// </summary>
        public void ConsumeOne()
        {
            // We only consume a single frame to avoid slowing down the chain.
            // Dropping frames is not critical for display.
            // Always consume the very latest frame.

            if (!allocated || buffer.ProducerPosition < 0)
                return;

            long then = stopwatch.ElapsedMilliseconds;

            long next = buffer.ProducerPosition;
            
            Frame entry = buffer.GetEntry(next);
            if (allocated)
                frame.Import(entry);

            Ellapsed = stopwatch.ElapsedMilliseconds - then;
        }
    }
}
