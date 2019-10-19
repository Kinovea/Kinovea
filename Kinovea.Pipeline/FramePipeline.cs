using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Threading;
using Kinovea.Pipeline.MemoryLayout;

namespace Kinovea.Pipeline
{
    /// <summary>
    /// Ensures the continuous flux of frames between the producer and consumers.
    /// This class is responsible for hooking the producer and consumers together and
    /// making the ringbuffer accessible to them.
    ///
    /// Inspired by the disruptor pattern.
    /// </summary>
    public class FramePipeline
    {
        public int FrameLength
        {
            get { return frameLength; }
        }

        public long Drops
        {
            get { return drops; }
        }

        public bool Allocated
        {
            get { return ringBuffer.Allocated; }
        }

        public double Frequency
        {
            // Note: this variable is written by the stream thread and read by the UI thread.
            // We don't lock because freshness of values is not paramount and torn reads are not catastrophic either.
            // We eventually get an approximate value good enough for the purpose.
            get 
            {
                return frequencyCounter.Frequency;
            }
        }

        private IFrameProducer producer;
        private List<IFrameConsumer> consumers;
        private RingBuffer ringBuffer;
        private int frameLength;

        // Note: the benchmark counters are always filled.
        // The benchmark mode determines the code path taken.
        //private BenchmarkMode benchmarkMode = BenchmarkMode.Heartbeat;
        //private Dictionary<string, BenchmarkCounterIntervals> counters = new Dictionary<string, BenchmarkCounterIntervals>();
        //private BenchmarkCounterIntervals heartbeat = new BenchmarkCounterIntervals();
        //private BenchmarkCounterIntervals commitbeat = new BenchmarkCounterIntervals();
        private FrequencyCounter frequencyCounter = new FrequencyCounter(24, 48, true);

        // Note: we lock drops on write as it's written from UI thread and producer thread.
        // The freshness of the value is not paramount so we do not lock on read to avoid slowing down the producer thread.
        private int drops;
        private object lockerDrops = new object();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FramePipeline(IFrameProducer producer, List<IFrameConsumer> consumers, int buffers, int bufferSize)
        {
            log.DebugFormat("Starting frame pipeline.");

            this.producer = producer;
            this.consumers = consumers;

            InitializeBenchmarkCounters();

            ringBuffer = new RingBuffer(buffers, bufferSize);

            if (ringBuffer.Allocated)
            {
                frameLength = bufferSize;
                log.DebugFormat("Ring buffer allocated.");

                Bind();
                GC.Collect();
            }
        }

        public void ResetDrops()
        {
            lock (lockerDrops)
                drops = 0;
        }

        public void Teardown()
        {
            Unbind();
            frameLength = 0;
            ringBuffer.Teardown();

            log.DebugFormat("Ring buffer torn down.");
        }

        private void Bind()
        {
            // Make sure all consumers threads are running.
            // Bind both ends of the pipeline.

            foreach (IFrameConsumer consumer in consumers)
            {
                while (!consumer.Started)
                {
                    // Busy spin to make sure the consumer is started.
                }

                consumer.SetRingBuffer(ringBuffer);
            }

            ringBuffer.SetConsumers(new List<IFrameConsumer>(consumers));

            producer.FrameProduced += producer_FrameProduced;

            log.DebugFormat("Pipeline connected to producer and consumers.");
        }

        private void Unbind()
        {
            producer.FrameProduced -= producer_FrameProduced;
            ringBuffer.ClearConsumers();

            foreach (IFrameConsumer consumer in consumers)
                consumer.ClearRingBuffer();

            log.DebugFormat("Pipeline disconnected from producer and consumers.");
        }

        private void producer_FrameProduced(object sender, FrameProducedEventArgs e)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            //heartbeat.Tick();
            
            //if (benchmarkMode == BenchmarkMode.Heartbeat)
              //return;

            frequencyCounter.Tick();

            // Claim the next slot in the ring buffer.
            Frame entry;
            bool claimed = true;
            /*if (benchmarkMode == BenchmarkMode.Bradycardia)
                ringBuffer.Claim(out entry);
            else*/
            
            claimed = ringBuffer.TryClaim(out entry);

            if (!claimed)
            {
                // At least one consumer is still reading the slot we would like to write to.
                lock (lockerDrops)
                    drops++;
            }
            else
            {
                WriteSlot(e.Buffer, e.PayloadLength, entry);
            }
        }

        private void WriteSlot(byte[] bytes, int payloadLength, Frame entry)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            // The slot is writeable, let's stuff it with camera bytes.
            if (payloadLength <= entry.Buffer.Length)
            {
                Buffer.BlockCopy(bytes, 0, entry.Buffer, 0, payloadLength);
                entry.PayloadLength = payloadLength;
            }
            else
            {
                // Unexpected
            }

            ringBuffer.Commit();
            //commitbeat.Tick();
        }

        #region Benchmarking support
        public void SetBenchmarkMode(BenchmarkMode benchmarkMode)
        {
            //this.benchmarkMode = benchmarkMode;
            //ringBuffer.SetBenchmarkMode(benchmarkMode);
        }

        public Dictionary<string, IBenchmarkCounter> StopBenchmark()
        {
            return null;
            /*foreach (BenchmarkCounterIntervals counter in counters.Values)
                counter.Stop();

            Dictionary<string, IBenchmarkCounter> result = new Dictionary<string, IBenchmarkCounter>();
            foreach (var pair in counters)
                result.Add(pair.Key, pair.Value as IBenchmarkCounter);

            return result;*/
        }
        private void InitializeBenchmarkCounters()
        {
            //counters.Add("Heartbeat", heartbeat);
            //counters.Add("Commitbeat", commitbeat);
        }
        #endregion
        
    }
}
