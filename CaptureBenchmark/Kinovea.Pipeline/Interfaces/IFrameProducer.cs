using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;

namespace Kinovea.Pipeline
{
    public interface IFrameProducer
    {
        /// <summary>
        /// The camera received a new frame.
        /// The event is called from within the grabbing thread and the frame bytes are owned by grabbing.
        /// The event handler should make a copy of the bytes push them to a queue and return as soon as possible.
        /// </summary>
        event EventHandler<EventArgs<byte[]>> FrameProduced;
    }
}
