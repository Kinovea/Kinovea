using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;
using System.Diagnostics;

namespace Kinovea.Pipeline.Consumers
{
    /// <summary>
    /// For testing purpose only.
    /// A consumer that is always faster than the producer.
    /// The camera average heartbeat should not be impacted.
    /// The camera average commitbeat should not be impacted.
    /// </summary>
    public class ConsumerNoop : AbstractConsumer
    {
        protected override void ProcessEntry(long position, Frame entry)
        {
        }
    }
}
