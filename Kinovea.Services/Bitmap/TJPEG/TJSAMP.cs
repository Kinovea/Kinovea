using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services.TurboJpeg
{
    public enum TJSAMP
    {
        /**
        * 4:4:4 chrominance subsampling (no chrominance subsampling).  The JPEG or
        * YUV image will contain one chrominance component for every pixel in the
        * source image.
        */
        TJSAMP_444 = 0,
        /**
         * 4:2:2 chrominance subsampling.  The JPEG or YUV image will contain one
         * chrominance component for every 2x1 block of pixels in the source image.
         */
        TJSAMP_422,
        /**
         * 4:2:0 chrominance subsampling.  The JPEG or YUV image will contain one
         * chrominance component for every 2x2 block of pixels in the source image.
         */
        TJSAMP_420,
        /**
         * Grayscale.  The JPEG or YUV image will contain no chrominance components.
         */
        TJSAMP_GRAY,
        /**
         * 4:4:0 chrominance subsampling.  The JPEG or YUV image will contain one
         * chrominance component for every 1x2 block of pixels in the source image.
         * Note that 4:4:0 subsampling is not fully accelerated in libjpeg-turbo.
         */
        TJSAMP_440
    }
}
