using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kinovea.Pipeline.MemoryLayout;
using Kinovea.Services;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// Base class for batch consumers.
    /// These consumers should have the same lifetime as the pipeline itself.
    /// </summary>
    public abstract class AbstractConsumer : IFrameConsumer
    {
        public bool Started
        {
            get { return started.Data; }
        }

        public bool Active
        {
            get { return active.Data; }
        }

        public long ConsumerPosition
        {
            get { return consumerPosition.Data; }
        }

        public virtual BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return null; }
        }
        
        // Synchronization
        private CacheLineStorageBool started = new CacheLineStorageBool(false); 
        private CacheLineStorageBool stopAsked = new CacheLineStorageBool(false);
        private EventWaitHandle activateEventHandle = new AutoResetEvent(false);
        private CacheLineStorageBool active = new CacheLineStorageBool(false);
        private CacheLineStorageBool deactivateAsked = new CacheLineStorageBool(false);
        private CacheLineStorageLong consumerPosition = new CacheLineStorageLong(-1); 
        
        // Frame memory storage
        private RingBuffer buffer;
        protected int frameLength;

        public void Run()
        {
            started.Data = true;

            while (!stopAsked.Data)
            {
                activateEventHandle.WaitOne();
                
                if (stopAsked.Data)
                    break;

                BeforeActivate();
                
                Loop();
                
                AfterDeactivate();
            }

            started.Data = false;
        }

        /// <summary>
        /// Hook the ring buffer. This must be called before the thread is activated.
        /// </summary>
        public void SetRingBuffer(RingBuffer buffer)
        {
            this.buffer = buffer;
            this.frameLength = buffer.FrameLength;
            this.Initialize();
        }

        public void ClearRingBuffer()
        {
            buffer = null;
            frameLength = 0;
            // Uninitialize();
        }

        public void Activate()
        {
            activateEventHandle.Set();
        }

        public void Deactivate()
        {
            deactivateAsked.Data = true;
        }

        public void Stop()
        {
            if (!started.Data)
                return;

            stopAsked.Data = true;
            
            // stopAsked is checked after activation and after deactivation.
            if (active.Data)
                deactivateAsked.Data = true;
            else
                activateEventHandle.Set();
        }

        private void Loop()
        {
            // The main frame-consumption loop.

            // Upon activation we pretend that we are almost up to date, rather than start at 0.
            consumerPosition.Data = Math.Max(buffer.ProducerPosition - 1, -1);

            active.Data = true;
            
            long next = consumerPosition.Data + 1;

            while(!deactivateAsked.Data)
            {
                // Wait until at least the next frame is available, but if more than one is available consume everything in batch.
                long readable = buffer.WaitFor(next);

                while (next <= readable)
                {
                    Frame entry = buffer.GetEntry(next);
                    ProcessEntry(next, entry);
                    next++;
                }

                // Update our current position so the producer knows not to wrap.
                consumerPosition.Data = next - 1;
            }

            active.Data = false;
        }

        protected virtual void BeforeActivate()
        {
        }

        protected virtual void AfterDeactivate()
        {
        }

        protected virtual void Initialize()
        {
        }

        protected abstract void ProcessEntry(long position, Frame entry);
    }
}
