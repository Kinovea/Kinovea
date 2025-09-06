using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services.TurboJpeg
{
    [Flags]
    public enum TJFLAG
    {
        /**
         * The uncompressed source/destination image is stored in bottom-up (Windows,
         * OpenGL) order, not top-down (X11) order.
         */
        TJFLAG_BOTTOMUP = 2,
        /* Turn off CPU auto-detection and force TurboJPEG to use MMX code (if the
         * underlying codec supports it.)
         */
        TJFLAG_FORCEMMX = 8,
        /**
         * Turn off CPU auto-detection and force TurboJPEG to use SSE code (if the
         * underlying codec supports it.)
         */
        TJFLAG_FORCESSE = 16,
        /**
         * Turn off CPU auto-detection and force TurboJPEG to use SSE2 code (if the
         * underlying codec supports it.)
         */
        TJFLAG_FORCESSE2 = 32,
        /**
         * Turn off CPU auto-detection and force TurboJPEG to use SSE3 code (if the
         * underlying codec supports it.)
         */
        TJFLAG_FORCESSE3 = 128,
        /**
         * When decompressing an image that was compressed using chrominance
         * subsampling, use the fastest chrominance upsampling algorithm available in
         * the underlying codec.  The default is to use smooth upsampling, which
         * creates a smooth transition between neighboring chrominance components in
         * order to reduce upsampling artifacts in the decompressed image.
         */
        TJFLAG_FASTUPSAMPLE = 256,
        /**
         * Disable buffer (re)allocation.  If passed to #tjCompress2() or
         * #tjTransform(), this flag will cause those functions to generate an error if
         * the JPEG image buffer is invalid or too small rather than attempting to
         * allocate or reallocate that buffer.  This reproduces the behavior of earlier
         * versions of TurboJPEG.
         */
        TJFLAG_NOREALLOC = 1024,
        /**
         * Use the fastest DCT/IDCT algorithm available in the underlying codec.  The
         * default if this flag is not specified is implementation-specific.  For
         * example, the implementation of TurboJPEG for libjpeg[-turbo] uses the fast
         * algorithm by default when compressing, because this has been shown to have
         * only a very slight effect on accuracy, but it uses the accurate algorithm
         * when decompressing, because this has been shown to have a larger effect.
         */
        TJFLAG_FASTDCT = 2048,
        /**
         * Use the most accurate DCT/IDCT algorithm available in the underlying codec.
         * The default if this flag is not specified is implementation-specific.  For
         * example, the implementation of TurboJPEG for libjpeg[-turbo] uses the fast
         * algorithm by default when compressing, because this has been shown to have
         * only a very slight effect on accuracy, but it uses the accurate algorithm
         * when decompressing, because this has been shown to have a larger effect.
         */
        TJFLAG_ACCURATEDCT = 4096,
    }
}
