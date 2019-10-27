using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Pipeline
{
    /// <summary>
    /// Simple byte buffer. Format agnostic.
    /// The whole buffer might not be filled with payload.
    /// </summary>
    public class Frame
    {
        public byte[] Buffer { get; private set; }
        public int PayloadLength { get; set; }

        public Frame(int bufferSize)
        {
            this.Buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Copy the source frame into this frame.
        /// Assumes pre-allocation and compatible sizes.
        /// </summary>
        public void Import(Frame source)
        {
            System.Buffer.BlockCopy(source.Buffer, 0, this.Buffer, 0, source.PayloadLength);
            this.PayloadLength = source.PayloadLength;
        }
    }
}
