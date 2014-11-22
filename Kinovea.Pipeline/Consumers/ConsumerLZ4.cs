using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LZ4n;
using System.Diagnostics;
using Kinovea.Services;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// Compress images using LZ4 algorithm.
    /// </summary>
    public class ConsumerLZ4 : AbstractConsumer
    {
        public override BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return counter; }
        }

        private byte[] output;
        private Stopwatch sw = new Stopwatch();
        private BenchmarkCounterBandwidth counter = new BenchmarkCounterBandwidth();

        protected override void Initialize()
        {
            base.Initialize();

            int maxLength = LZ4Codec.MaximumOutputLength(frameLength);
            this.output = new byte[maxLength];
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            sw.Reset();
            sw.Start();
            
            LZ4Codec.Encode32(entry.Buffer, 0, entry.Buffer.Length, output, 0, output.Length);
            
            counter.Post((int)sw.ElapsedMilliseconds, frameLength);
        }
    }
}
