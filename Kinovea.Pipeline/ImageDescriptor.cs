using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Pipeline
{
    public class ImageDescriptor
    {
        public ImageFormat Format { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BufferSize { get; private set; }

        public ImageDescriptor(ImageFormat format, int width, int height, int bufferSize)
        {
            this.Format = format;
            this.Width = width;
            this.Height = height;
            this.BufferSize = bufferSize;
        }
    }
}
