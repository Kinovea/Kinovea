using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Pipeline.MemoryLayout;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Kinovea.Services;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// Stores the frame number and arrival datetime to the file.
    /// </summary>
    public class ConsumerFrameNumber : AbstractConsumer
    {
        public override BenchmarkCounterBandwidth BenchmarkCounter
        {
            get { return counter; }
        }

        private Stopwatch sw = new Stopwatch();
        private BenchmarkCounterBandwidth counter = new BenchmarkCounterBandwidth();


        // Frame disk store
        private string filename;
        private TextWriter writer;

        protected override void BeforeActivate() 
        {
            filename = string.Format("{0:yyyyMMdd-HHmmss-fff}.txt", DateTime.UtcNow);
            if (writer != null)
                writer.Dispose();

            writer = File.AppendText(filename);

            writer.WriteLine("Header");
            writer.WriteLine("---------------------");
        }

        protected override void AfterDeactivate()
        {
            writer.WriteLine("---------------------");
            writer.WriteLine("Footer");

            writer.Close();
            writer = null;
        }

        protected override void ProcessEntry(long position, Frame entry)
        {
            sw.Reset();
            sw.Start();

            string line = string.Format("{0:yyyyMMddTHH:mm:ss.fff}: position:{1}.", DateTime.UtcNow, position);
            writer.WriteLine(line);

            counter.Post((int)sw.ElapsedMilliseconds, frameLength);
        }
    }
}
