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
    }
}
