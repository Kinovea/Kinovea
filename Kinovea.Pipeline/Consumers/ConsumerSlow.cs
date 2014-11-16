using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kinovea.Services;
using System.Diagnostics;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// A consumer that is systematically slower than the producer.
    /// The camera heartbeat should not be impacted.
    /// The camera commitbeat should reflect the FPS that we set here.
    /// </summary>
    public class ConsumerSlow : AbstractConsumer
    {
        public override BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return counter; }
        }

        private int slowInterval = 100;
        private Stopwatch sw = new Stopwatch();
        private BenchmarkCounterBandwidth counter = new BenchmarkCounterBandwidth();

        protected override void ProcessEntry(long position, Frame entry)
        {
            sw.Reset();
            sw.Start();

            Thread.Sleep(slowInterval);

            counter.Post((int)sw.ElapsedMilliseconds, frameLength);
        }
    }
}
