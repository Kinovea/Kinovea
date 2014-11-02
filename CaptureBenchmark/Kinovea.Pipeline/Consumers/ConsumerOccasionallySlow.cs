using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Kinovea.Services;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// A consumer that is occasionally slower than the producer.
    /// The camera average heartbeat should not be impacted.
    /// The camera average commitbeat should not be impacted.
    /// </summary>
    public class ConsumerOccasionallySlow : AbstractConsumer
    {
        public override BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return counter; }
        }

        private Random random = new Random();
        private double slowRate = 0.90;
        private int slowInterval = 100;
        private Stopwatch sw = new Stopwatch();
        private BenchmarkCounterBandwidth counter = new BenchmarkCounterBandwidth();

        protected override void ProcessEntry(long position, Frame entry)
        {
            sw.Reset();
            sw.Start();

            if (random.NextDouble() >= slowRate)
            {
                Thread.Sleep(slowInterval);
            }

            counter.Post((int)sw.ElapsedMilliseconds, frameLength);
        }
    }
}
