using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public enum BenchmarkMode
    {
        None,

        /// <summary>
        /// Stops processing right after the frame event is received from the producer.
        /// This is used to monitor the framerate variability.
        /// No reader should be instanciated in this mode.
        /// </summary>
        Heartbeat,

        /// <summary>
        /// Stops processing after the frame bytes have been copied to the corresponding slot.
        /// No reader should be instanciated in this mode.
        /// </summary>
        Commitbeat,
        
        /// <summary>
        /// The producer thread is artificially blocked for longer than the camera frame interval.
        /// This tests how the camera driver responds to being blocked.
        /// This is not necessarily a real life scenario if we use TryClaim and manages drops ourselves.
        /// No reader should be instanciated in this mode.
        /// </summary>
        Bradycardia,

        /// <summary>
        /// The claimed slot is made to not be writeable by the producer at random times.
        /// This tests how the producer reacts to the "mostly-empty-buffer" scenario.
        /// No reader should be instanciated in this mode.
        /// </summary>
        FrameDrops,

        /// <summary>
        /// A consumer thread that does nothing.
        /// </summary>
        Noop,

        /// <summary>
        /// A consumer thread that has occasional hiccups.
        /// </summary>
        OccasionallySlow,

        /// <summary>
        /// A consumer thread that is systematically slower than the producer.
        /// </summary>
        Slow,

        /// <summary>
        /// A consumer thread that compress images using the LZ4 algorithm, without writing data to disk.
        /// </summary>
        LZ4,

        /// <summary>
        /// A long lived consumer thread that stores the frame number to disk.
        /// </summary>
        FrameNumberToDisk,
    }
}
