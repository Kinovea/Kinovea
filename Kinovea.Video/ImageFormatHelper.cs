using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Video
{
    public static class ImageFormatHelper
    {
        /// <summary>
        /// Returns the number of bytes taken by an image depending on its size and format.
        /// For color images, buffer size is always the full RGB24 size, even for compressed formats.
        /// </summary>
        public static int ComputeBufferSize(int width, int height, ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.RGB24:
                    return width * height * 3; // FIXME: many image providers will align to 4 bytes.
                case ImageFormat.RGB32:
                    return width * height * 4;
                case ImageFormat.JPEG:
                    // Assume size of uncompressed frame as a ceiling.
                    return width * height * 3;
                case ImageFormat.Y800:
                    return width * height * 1;
                case ImageFormat.I420:
                case ImageFormat.None:
                default:
                    return width * height * 3;
            }
        }

        public static int BytesPerPixel(ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.RGB24:
                    return 3;
                case ImageFormat.RGB32:
                    return 4;
                case ImageFormat.Y800:
                    return 1;
                case ImageFormat.JPEG:
                case ImageFormat.I420:
                case ImageFormat.None:
                default:
                    // We should really not be asking this for these formats.
                    return 3;
            }
        }
    }
}
