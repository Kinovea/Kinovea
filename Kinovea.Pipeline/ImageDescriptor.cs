using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kinovea.Services;

namespace Kinovea.Pipeline
{
    public class ImageDescriptor
    {
        public static ImageDescriptor Invalid
        {
            get { return invalid; }
        }

        public ImageFormat Format { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool TopDown { get; private set; }
        public int BufferSize { get; private set; }
        
        private static readonly ImageDescriptor invalid = new ImageDescriptor(ImageFormat.None, 0, 0, true, 0);

        public ImageDescriptor(ImageFormat format, int width, int height, bool topDown, int bufferSize)
        {
            this.Format = format;
            this.Width = width;
            this.Height = height;
            this.TopDown = topDown;
            this.BufferSize = bufferSize;
        }

        public static bool Compatible(ImageDescriptor a, ImageDescriptor b)
        {
            if (a == null || b == null || a == invalid || b == invalid)
                return false;

            return a.Format == b.Format && a.Width == b.Width && a.Height == b.Height && a.TopDown == b.TopDown && a.BufferSize == b.BufferSize;
        }
    }
}
