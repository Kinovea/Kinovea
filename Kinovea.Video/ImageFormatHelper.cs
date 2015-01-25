using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Video
{
    public static class ImageFormatHelper
    {
        public static int ComputeBufferSize(int width, int height, ImageFormat format)
        {
            // For color images, buffer size is always the full RGB24 size, even for compressed formats.
            
            switch (format)
            {
                case ImageFormat.RGB24:
                    return width * height * 3;
                case ImageFormat.JPEG:
                    return width * height * 3;
                case ImageFormat.Y800:
                    return width * height * 1;
                case ImageFormat.I420:
                case ImageFormat.None:
                default:
                    return width * height * 3;
            }
        }
    }
}
