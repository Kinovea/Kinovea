using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// This is the abstraction of the DirectShow media type (AMMediaType) for the output pin of the capture filter.
    /// The output pin will publish a list of the media types it supports.
    /// At the moment it is mostly a wrapper around AForge VideoCapabilities.
    /// The Index property can be used to match the corresponding AForge VideoCapabilities or the DirectShow media type.
    /// </summary>
    public class MediaType
    {
        public string Compression { get; private set; }
        public Size FrameSize { get; private set; }
        public int MediaTypeIndex { get; private set; }
        public int BitsPerPixel { get; private set; }

        public string Description
        {
            get 
            {
                return string.Format("Compression:{0}, FrameSize:{1}x{2}, BPP:{4}, Index:{5}.",
                    Compression, FrameSize.Width, FrameSize.Height, BitsPerPixel, MediaTypeIndex);
            }
        }

        public MediaType(string compression, Size frameSize, int index, int bpp)
        {
            this.Compression = compression;
            this.FrameSize = frameSize;
            this.MediaTypeIndex = index;
            this.BitsPerPixel = bpp;
        }

        public override string ToString()
        {
            return string.Format("{0} × {1}", FrameSize.Width, FrameSize.Height);
        }
    }
}
