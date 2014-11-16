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
        private RingBuffer buffer;
        private int frameLength;

        // Note: the benchmark counters are always filled.
        // The benchmark mode determines the code path taken.
        private BenchmarkMode benchmarkMode = BenchmarkMode.Heartbeat;
        private Dictionary<string, BenchmarkCounterIntervals> counters = new Dictionary<string, BenchmarkCounterIntervals>();
        private BenchmarkCounterIntervals heartbeat = new BenchmarkCounterIntervals();
        private BenchmarkCounterIntervals commitbeat = new BenchmarkCounterIntervals();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public FramePipeline(IFrameProducer producer, List<IFrameConsumer> consumers, int width, int height, int depth)
        {
            log.DebugFormat("Starting frame pipeline.");

            this.producer = producer;
            this.consumers = consumers;

            InitializeBenchmarkCounters();
            
            buffer = new RingBuffer(8, width, height, depth);
            frameLength = buffer.FrameLength;
            
            Bind();
            
            GC.Collect(2);
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

                consumer.SetRingBuffer(buffer);
            }

            buffer.SetConsumers(consumers);

            producer.FrameProduced += producer_FrameProduced;
        }

        private void producer_FrameProduced(object sender, EventArgs<byte[]> e)
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
                buffer.Claim(out entry);
            else
                claimed = buffer.TryClaim(out entry);

            if (!claimed)
            {
                // Frame drop.
            }
            else
            {
                WriteSlot(e.Value, entry);
            }
        }

        private void WriteSlot(byte[] bytes, Frame entry)
        {
            //-------------------------
            // Runs in producer thread.
            //-------------------------

            // The slot is writeable, let's stuff it with camera bytes.
            if (bytes.Length <= entry.Bytes.Length)
            {
                Buffer.BlockCopy(bytes, 0, entry.Bytes, 0, bytes.Length);
            }
            else
            {
                // Unexpected
            }

            buffer.Commit();
            commitbeat.Tick();
        }

        #region Benchmarking support
        public void SetBenchmarkMode(BenchmarkMode benchmarkMode)
        {
            this.benchmarkMode = benchmarkMode;
            buffer.SetBenchmarkMode(benchmarkMode);
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
