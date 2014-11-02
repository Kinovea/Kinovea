using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Pipeline
{
    public class Frame
    {
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }
        public byte[] Bytes { get; private set; }

        public Frame(int width, int height, int depth)
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;

            // TODO: align on disk block size.
            this.Bytes = new byte[width * height * depth];
        }
    }
}
