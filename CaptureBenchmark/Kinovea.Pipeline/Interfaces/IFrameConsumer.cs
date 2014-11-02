using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Pipeline
{
    public interface IFrameConsumer
    {
        bool Started { get; }
        bool Active { get; }
        long ConsumerPosition { get; }

        void Run();
        void SetRingBuffer(RingBuffer buffer);
        void Activate();
        void Deactivate();
    }
}
