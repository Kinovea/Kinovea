using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Threading;

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

        private IFrameProducer producer;
        private List<IFrameConsumer> consumers;
        private RingBuffer ringBuffer;
        private int frameLength;

        // Note: the benchmark counters are always filled.
        // The benchmark mode determines the code path taken.
        private BenchmarkMode benchmarkMode = BenchmarkMode.Heartbeat;
        private Dictionary<string, BenchmarkCounterIntervals> counters = new Dictionary<string, BenchmarkCounterIntervals>();
        private BenchmarkCounterIntervals heartbeat = new BenchmarkCounterIntervals();
        private BenchmarkCounterIntervals commitbeat = new BenchmarkCounterIntervals();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FramePipeline(IFrameProducer producer, List<IFrameConsumer> consumers, int buffers, int bufferSize)
        {
            log.DebugFormat("Starting frame pipeline.");

            this.producer = producer;
            this.consumers = consumers;

            InitializeBenchmarkCounters();

            ringBuffer = new RingBuffer(buffers, bufferSize);
            frameLength = bufferSize;
            
            Bind();
            GC.Collect();
        }

        public void Teardown()
        {
            Unbind();
            frameLength = 0;
            ringBuffer.Teardown();
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

            ringBuffer.SetConsumers(consumers);

            producer.FrameProduced += producer_FrameProduced;
        }

        private void Unbind()
        {
            producer.FrameProduced -= producer_FrameProduced;
            ringBuffer.ClearConsumers();

            foreach (IFrameConsumer consumer in consumers)
                consumer.ClearRingBuffer();
        }

        private void producer_FrameProduced(object sender, FrameProducedEventArgs e)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            heartbeat.Tick();
            
            if (benchmarkMode == BenchmarkMode.Heartbeat)
                return;

            // Claim the next slot in the ring buffer.
            Frame entry;
            bool claimed = true;
            if (benchmarkMode == BenchmarkMode.Bradycardia)
                ringBuffer.Claim(out entry);
            else
                claimed = ringBuffer.TryClaim(out entry);

            if (!claimed)
            {
                // Frame drop.
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
            commitbeat.Tick();
        }

        #region Benchmarking support
        public void SetBenchmarkMode(BenchmarkMode benchmarkMode)
        {
            this.benchmarkMode = benchmarkMode;
            ringBuffer.SetBenchmarkMode(benchmarkMode);
        }

        public Dictionary<string, IBenchmarkCounter> StopBenchmark()
        {
            foreach (BenchmarkCounterIntervals counter in counters.Values)
                counter.Stop();

            Dictionary<string, IBenchmarkCounter> result = new Dictionary<string, IBenchmarkCounter>();
            foreach (var pair in counters)
                result.Add(pair.Key, pair.Value as IBenchmarkCounter);

            return result;
        }
        private void InitializeBenchmarkCounters()
        {
            counters.Add("Heartbeat", heartbeat);
            counters.Add("Commitbeat", commitbeat);
        }
        #endregion
        
    }
}
