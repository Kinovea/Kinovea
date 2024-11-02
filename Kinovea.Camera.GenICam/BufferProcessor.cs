using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BGAPI2;
using OpenCvSharp;
using System.Drawing;
using System.Runtime.InteropServices;
using Kinovea.Services;
using Kinovea.Pipeline;

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// Class to convert from BGAPI buffers to byte buffers used by Kinovea pipeline.
    /// With debayering and HDR to LDR conversion in the middle if needed.
    /// </summary>
    public class BufferProcessor
    {
        #region Members
        private int frameCount = 0;
        private ImageProcessor imgProcessor = new ImageProcessor();
        private Stopwatch swConvert = new Stopwatch();
        private Averager averager = new Averager(0.02);
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public void Prepare()
        {
            frameCount = 0;
        }

        /// <summary>
        /// Determines the output image format based on input format and conversion options.
        /// We output in three possible formats: Y800, RGB24 or JPEG.
        /// </summary>
        public static ImageFormat GetImageFormat(string pixelFormat, bool isJpeg, bool demosaicing)
        {
            if (isJpeg)
                return ImageFormat.JPEG;

            if (IsBayer(pixelFormat))
            {
                if (!demosaicing)
                    return ImageFormat.Y800;
                else
                    return ImageFormat.RGB24;
            }
            else if (pixelFormat.StartsWith("Mono"))
            {
                return ImageFormat.Y800;
            }
            else
            {
                return ImageFormat.RGB24;
            }
        }

        /// <summary>
        /// Returns true if the stream format is a Bayer format (HDR or LDR).
        /// </summary>
        public static bool IsBayer(string pixelFormat)
        {
            return pixelFormat.StartsWith("Bayer");
        }

        /// <summary>
        /// Fill the destimation buffer (must be pre-allocated to the max size possible) with the incoming data.
        /// Apply pixel format conversion if needed.
        /// Update the payload length to the outgoing payload.
        /// </summary>
        public unsafe void Process(byte[] dst, ImageFormat dstFormat, BGAPI2.Buffer src, out int payloadLength)
        {

            if ((dstFormat == ImageFormat.JPEG) || 
                (dstFormat == ImageFormat.Y800 && IsY800(src.PixelFormat)))
            {
                // Straight copy.
                CopyBuffer(dst, src);
                payloadLength = (int)src.SizeFilled;
                return;
            }

            if (dstFormat == ImageFormat.RGB24 && src.PixelFormat == "BGR8")
            {
                // Straight copy.
                CopyBuffer(dst, src);
                payloadLength = (int)src.Width * (int)src.Height * 3;
                return;
            }

            // Other combos require conversion.
            frameCount++;
            swConvert.Restart();
            bool isBayer = src.PixelFormat.StartsWith("Bayer");
            int width = (int)src.Width;
            int height = (int)src.Height;
            int bpp = GetBPPSupported(src.PixelFormat);
            
            // Fallback for formats not directly supported by straight copy or OpenCV conversion.
            // Use BGAPI image processor. This is typically slower.
            if (bpp == -1)
            {
                BGAPI2.Image image = imgProcessor.CreateImage((uint)width, (uint)height, src.PixelFormat, src.MemPtr, src.MemSize);

                if (dstFormat == ImageFormat.Y800)
                {
                    payloadLength = width * height;
                    BGAPI2.Image transformedImage = imgProcessor.CreateTransformedImage(image, "Mono8");
                    image.Release();
                    CopyImage(dst, transformedImage, payloadLength);
                    transformedImage.Release();
                }
                else if (dstFormat == ImageFormat.RGB24)
                {
                    payloadLength = width * height * 3;
                    BGAPI2.Image transformedImage = imgProcessor.CreateTransformedImage(image, "BGR8");
                    image.Release();
                    CopyImage(dst, transformedImage, payloadLength);
                    transformedImage.Release();
                }
                else
                {
                    // This should never happen.
                    image.Release();
                    payloadLength = 0;
                    throw new InvalidProgramException();
                }

                return;
            }

            // Create an OpenCV Mat with the right size and pixel format to hold the incoming buffer.
            Mat mat = null;
            if (bpp == 1)
            {
                // Bayer**8 (Mono8 is always handled via straight copy).
                mat = new Mat(height, width, MatType.CV_8UC1);
            }
            else if (bpp == 2)
            {
                // Mono HDR or Bayer HDR.
                mat = new Mat(height, width, MatType.CV_16UC1);
            }
            else if (bpp == 3)
            {
                // RGB.
                mat = new Mat(height, width, MatType.CV_8UC3);
            }

            // Copy the incoming buffer.
            NativeMethods.memcpy(mat.Data.ToPointer(), src.MemPtr.ToPointer(), (int)src.SizeFilled);

            // Convert HDR to LDR (Mono and Bayer)
            if (src.PixelFormat.EndsWith("10"))
            {
                mat.ConvertTo(mat, MatType.CV_8UC1, 1.0 / 4.0);
            }
            else if (src.PixelFormat.EndsWith("12"))
            {
                mat.ConvertTo(mat, MatType.CV_8UC1, 1.0 / 16.0);
            }

            if (dstFormat == ImageFormat.Y800)
            {
                // Mono or Bayer, HDR or LDR -> Y800.
                payloadLength = width * height;

                FillBufferFromMat(dst, mat, payloadLength);
            }
            else if (dstFormat == ImageFormat.RGB24 && isBayer)
            {
                payloadLength = width * height * 3;

                // Open CV debayering enum shenanigan.
                // The original enum has mangled naming compared to what is expected, 
                // they added new aliases with corrected names but not available in 
                // OpenCvSharp yet. This is the mapping:
                // cv::COLOR_BayerRGGB2BGR = COLOR_BayerBG2BGR,
                // cv::COLOR_BayerGRBG2BGR = COLOR_BayerGB2BGR,
                // cv::COLOR_BayerBGGR2BGR = COLOR_BayerRG2BGR,
                // cv::COLOR_BayerGBRG2BGR = COLOR_BayerGR2BGR, 
                // https://docs.opencv.org/3.4/d8/d01/group__imgproc__color__conversions.html
                // https://github.com/opencv/opencv/issues/19629

                // Bayer LDR 
                if (src.PixelFormat.StartsWith("BayerRG"))
                {
                    mat = mat.CvtColor(ColorConversionCodes.BayerBG2BGR);
                }
                else if (src.PixelFormat.StartsWith("BayerGR"))
                {
                    mat = mat.CvtColor(ColorConversionCodes.BayerGB2BGR);
                }
                else if (src.PixelFormat.StartsWith("BayerBG"))
                {
                    mat = mat.CvtColor(ColorConversionCodes.BayerRG2BGR);
                }
                else if (src.PixelFormat.StartsWith("BayerGB"))
                {
                    mat = mat.CvtColor(ColorConversionCodes.BayerGR2BGR);
                }

                FillBufferFromMat(dst, mat, payloadLength);
            }
            else if (dstFormat == ImageFormat.RGB24 && src.PixelFormat == "RGB8")
            {
                payloadLength = width * height * 3;
                mat = mat.CvtColor(ColorConversionCodes.RGB2BGR);
                FillBufferFromMat(dst, mat, payloadLength);
            }
            else
            {
                // This should never happen.
                // Unsupported formats should use the image processor fallback.
                payloadLength = 0;
#if DEBUG
                mat.Dispose();
                throw new InvalidProgramException();
#endif
            }

            mat.Dispose();

            // Instrumentation
            //averager.Post(swConvert.Elapsed.TotalMilliseconds);
            //if (frameCount % 1000 == 0)
            //{
            //    log.DebugFormat("Frame: {0}, Avg: {1} ms", frameCount, averager.Average);
            //}
        }

        /// <summary>
        /// Takes the raw input buffer and copy it into the output buffer, no conversion.
        /// </summary>
        private unsafe void CopyBuffer(byte[] outBuffer, BGAPI2.Buffer buffer)
        {
            if ((ulong)outBuffer.Length < buffer.SizeFilled)
                return;

            fixed (byte* p = outBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                NativeMethods.memcpy(ptrDst.ToPointer(), buffer.MemPtr.ToPointer(), (int)buffer.SizeFilled);
            }
        }
        
        /// <summary>
        /// Takes a converted input buffer and copy it into the output buffer.
        /// </summary>
        private static unsafe void CopyImage(byte[] frameBuffer, BGAPI2.Image image, int length)
        {
            if (frameBuffer.Length < length)
                return;

            fixed (byte* p = frameBuffer)
            {
                IntPtr ptrDst = (IntPtr)p;
                NativeMethods.memcpy(ptrDst.ToPointer(), image.Buffer.ToPointer(), length);
            }
        }
    
        /// <summary>
        /// Copy an OpenCV Mat into an already allocated byte buffer.
        /// </summary>
        private unsafe void FillBufferFromMat(byte[] dst, OpenCvSharp.Mat src, int length)
        {
            if (dst.Length < length)
                return;

            try
            {
                Marshal.Copy(src.Data, dst, 0, length);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error while copying Mat to buffer. {0}", e.Message);
            }
        }

        /// <summary>
        /// Returns true if the input buffer format is 8-bit per pixel.
        /// This is true for Mono8 and Bayer**8 formats.
        /// This means it may directly be put into the Y800 output frame without conversion.
        /// </summary>
        private bool IsY800(string pixelFormat)
        {
            // HDR Bayer is not Y800, it needs to be converted.
            return pixelFormat == "Mono8" ||
                pixelFormat == "BayerBG8" ||
                pixelFormat == "BayerGB8" ||
                pixelFormat == "BayerGR8" ||
                pixelFormat == "BayerRG8";
        }

        /// <summary>
        /// Return the number of bytes per pixel from the passed pixel format, 
        /// if the format is not supported returns -1.
        /// </summary>
        private int GetBPPSupported(string fmt)
        {
            if (fmt == "Mono8" || (fmt.StartsWith("Bayer") && fmt.EndsWith("8")))
            {
                return 1;
            }
            else if (fmt.StartsWith("Mono") || fmt.StartsWith("Bayer"))
            {
                // HDR mono or bayer (Mono10, BayerRG12, etc).
                return 2;
            }
            else if ((fmt.StartsWith("RGB") || fmt.StartsWith("BGR")) && fmt.EndsWith("8"))
            {
                return 3;
            }

            // Any other format is not directly supported and will have to use the image processor fallback.

            return -1;
        }
    }
}
