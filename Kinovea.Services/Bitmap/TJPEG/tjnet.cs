using System;
using System.Runtime.InteropServices;

namespace Kinovea.Services.TurboJpeg
{
    public static class tjnet
    {
        private const string turbojpeg = "turbojpeg.dll";


        /// <summary>
        /// Create a TurboJPEG compressor instance. 
        /// </summary>
        /// <returns>a handle to the newly-created instance, or NULL if an error occurred (see tjGetErrorStr().)</returns>
        [DllImport(turbojpeg)]
        public static extern IntPtr tjInitCompress();

        /// <summary>
        /// Compress an RGB or grayscale image into a JPEG image. 
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG compressor or transformer instance </param>
        /// <param name="srcBuf">pointer to an image buffer containing RGB or grayscale pixels to be compressed </param>
        /// <param name="width">width (in pixels) of the source image </param>
        /// <param name="pitch">bytes per line of the source image. Normally, this should be width * tjPixelSize[pixelFormat] if the image is unpadded, or TJPAD(width * tjPixelSize[pixelFormat]) if each line of the image is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps. You can also be clever and use this parameter to skip lines, etc. Setting this parameter to 0 is the equivalent of setting it to width * tjPixelSize[pixelFormat].</param>
        /// <param name="height">height (in pixels) of the source image </param>
        /// <param name="pixelFormat">pixel format of the source image (see Pixel formats.) </param>
        /// <param name="jpegBuf">address of a pointer to an image buffer that will receive the JPEG image. TurboJPEG has the ability to reallocate the JPEG buffer to accommodate the size of the JPEG image. Thus, you can choose to:
        /// 1. pre-allocate the JPEG buffer with an arbitrary size using tjAlloc() and let TurboJPEG grow the buffer as needed,
        /// 2. set *jpegBuf to NULL to tell TurboJPEG to allocate the buffer for you, or
        /// 3. pre-allocate the buffer to a "worst case" size determined by calling tjBufSize(). This should ensure that the buffer never has to be re-allocated (setting TJFLAG_NOREALLOC guarantees this.)
        /// If you choose option 1, *jpegSize should be set to the size of your pre-allocated buffer. In any case, unless you have set TJFLAG_NOREALLOC, you should always check *jpegBuf upon return from this function, as it may have changed. </param>
        /// <param name="jpegSize">pointer to an unsigned long variable that holds the size of the JPEG image buffer. If *jpegBuf points to a pre-allocated buffer, then *jpegSize should be set to the size of the buffer. Upon return, *jpegSize will contain the size of the JPEG image (in bytes.) </param>
        /// <param name="jpegSubsamp">the level of chrominance subsampling to be used when generating the JPEG image (see Chrominance subsampling options.) </param>
        /// <param name="jpegQual">the image quality of the generated JPEG image (1 = worst, 100 = best) </param>
        /// <param name="flags">the bitwise OR of one or more of the flags.</param>
        /// <returns>0 if successful, or -1 if an error occurred (see tjGetErrorStr().) </returns>
        [DllImport(turbojpeg)]
        public static extern int tjCompress2(
            IntPtr handle, 
            byte[] srcBuf, 
            int width, 
            int pitch, 
            int height, 
            TJPF pixelFormat, 
            ref IntPtr jpegBuf, 
            ref uint jpegSize   , 
            TJSAMP jpegSubsamp, 
            int jpegQual, 
            TJFLAG flags
            );

        // tjBufSize
        // tjBufSizeYUV
        // tjEncodeYUV2

        /// <summary>
        /// Create a TurboJPEG decompressor instance. 
        /// </summary>
        /// <returns>a handle to the newly-created instance, or NULL if an error occurred (see tjGetErrorStr().) </returns>
        [DllImport(turbojpeg)]
        public static extern IntPtr tjInitDecompress();

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it. 
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG decompressor or transformer instance </param>
        /// <param name="jpegBuf">pointer to a buffer containing a JPEG image </param>
        /// <param name="jpegSize">	size of the JPEG image (in bytes) </param>
        /// <param name="width">pointer to an integer variable that will receive the width (in pixels) of the JPEG image</param>
        /// <param name="heigth">pointer to an integer variable that will receive the height (in pixels) of the JPEG image </param>
        /// <param name="jpegSubsamp">pointer to an integer variable that will receive the level of chrominance subsampling used when compressing the JPEG image (see Chrominance subsampling options.)</param>
        /// <returns>0 if successful, or -1 if an error occurred (see tjGetErrorStr().)</returns>
        [DllImport(turbojpeg)]
        public static extern IntPtr tjDecompressHeader2(
            IntPtr handle,
            byte[] jpegBuf,
            uint jpegSize,
            [Out] out int width,
            [Out] out int heigth,
            [Out] out TJSAMP jpegSubsamp
            );

        // tjGetScalingFactors

        /// <summary>
        /// Decompress a JPEG image to an RGB or grayscale image. 
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG decompressor or transformer instance </param>
        /// <param name="jpegBuf">pointer to a buffer containing the JPEG image to decompress</param>
        /// <param name="jpegSize">size of the JPEG image (in bytes) </param>
        /// <param name="dstBuf">pointer to an image buffer that will receive the decompressed image. 
        /// This buffer should normally be pitch * scaledHeight bytes in size, where scaledHeight can be determined by calling TJSCALED() 
        /// with the JPEG image height and one of the scaling factors returned by tjGetScalingFactors(). 
        /// The dstBuf pointer may also be used to decompress into a specific region of a larger buffer.</param>
        /// <param name="width">desired width (in pixels) of the destination image. 
        /// If this is different than the width of the JPEG image being decompressed, 
        /// then TurboJPEG will use scaling in the JPEG decompressor to generate the largest possible image that will fit within the desired width. 
        /// If width is set to 0, then only the height will be considered when determining the scaled image size.</param>
        /// <param name="pitch">bytes per line of the destination image. 
        /// Normally, this is scaledWidth * tjPixelSize[pixelFormat] if the decompressed image is unpadded, 
        /// else TJPAD(scaledWidth * tjPixelSize[pixelFormat]) if each line of the decompressed image is padded to the nearest 32-bit boundary, 
        /// as is the case for Windows bitmaps. 
        /// (NOTE: scaledWidth can be determined by calling TJSCALED() with the JPEG image width and one of the scaling factors 
        /// returned by tjGetScalingFactors().) You can also be clever and use the pitch parameter to skip lines, etc. 
        /// Setting this parameter to 0 is the equivalent of setting it to scaledWidth * tjPixelSize[pixelFormat].</param>
        /// <param name="height">desired height (in pixels) of the destination image. 
        /// If this is different than the height of the JPEG image being decompressed, then TurboJPEG will use scaling in the JPEG decompressor 
        /// to generate the largest possible image that will fit within the desired height. 
        /// If height is set to 0, then only the width will be considered when determining the scaled image size. </param>
        /// <param name="pixelFormat">pixel format of the destination image (see Pixel formats.) </param>
        /// <param name="flags">the bitwise OR of one or more of the flags.</param>
        /// <returns>0 if successful, or -1 if an error occurred (see tjGetErrorStr().) </returns>
        [DllImport(turbojpeg)]
        public static extern IntPtr tjDecompress2(
            IntPtr handle,
            byte[] jpegBuf,
            uint jpegSize,
            byte[] dstBuf,
            int width,
            int pitch,
            int height,
            TJPF pixelFormat,
            TJFLAG flags
            );

        // tjDecompressToYUV
        // tjInitTransform
        // tjTransform

        /// <summary>
        /// Destroy a TurboJPEG compressor, decompressor, or transformer instance. 
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG compressor, decompressor or transformer instance</param>
        /// <returns>0 if successful, or -1 if an error occurred (see tjGetErrorStr().) </returns>
        [DllImport(turbojpeg)]
        public static extern int tjDestroy(IntPtr handle);

        // tjAlloc

        /// <summary>
        /// Free an image buffer previously allocated by TurboJPEG.
        /// You should always use this function to free JPEG destination buffer(s) that were automatically (re)allocated by tjCompress2() 
        /// or tjTransform() or that were manually allocated using tjAlloc().
        /// </summary>
        /// <param name="buffer">address of the buffer to free</param>
        [DllImport(turbojpeg)]
        public static extern void tjFree(IntPtr buffer);

        /// <summary>
        /// Returns a descriptive error message explaining why the last command failed. 
        /// </summary>
        /// <returns>a descriptive error message explaining why the last command failed. </returns>
        [DllImport(turbojpeg)]
        public static extern string tjGetErrorStr();
    }
}
