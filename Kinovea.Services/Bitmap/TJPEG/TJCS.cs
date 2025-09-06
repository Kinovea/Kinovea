using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services.TurboJpeg
{
    /// <summary>
    /// JPEG colorspaces.
    /// </summary>
    public enum TJCS
    {
        /// <summary>
        /// RGB colorspace.
        /// When compressing the JPEG image, the R, G, and B components in the source image are 
        /// reordered into image planes, but no colorspace conversion or subsampling is performed. 
        /// RGB JPEG images can be decompressed to any of the extended RGB pixel formats or grayscale, 
        /// but they cannot be decompressed to YUV images.
        /// </summary>
        TJCS_RGB,

        /// <summary>
        /// YCbCr colorspace.
        /// YCbCr is not an absolute colorspace but rather a mathematical transformation of RGB designed solely for storage and transmission. 
        /// YCbCr images must be converted to RGB before they can actually be displayed. 
        /// In the YCbCr colorspace, the Y (luminance) component represents the black & white portion of the original image, 
        /// and the Cb and Cr (chrominance) components represent the color portion of the original image. 
        /// Originally, the analog equivalent of this transformation allowed the same signal to drive both black & white and color televisions, 
        /// but JPEG images use YCbCr primarily because it allows the color data to be optionally subsampled for the purposes of reducing bandwidth or disk space. 
        /// YCbCr is the most common JPEG colorspace, and YCbCr JPEG images can be compressed 
        /// from and decompressed to any of the extended RGB pixel formats or grayscale, or they can be decompressed to YUV planar images.
        /// </summary>
        TJCS_YCbCr,
        
        /// <summary>
        /// Grayscale colorspace.
        /// The JPEG image retains only the luminance data (Y component), and any color data from the source image is discarded. 
        /// Grayscale JPEG images can be compressed from and decompressed to any of the extended RGB pixel formats or grayscale, 
        /// or they can be decompressed to YUV planar images.
        /// </summary>
        TJCS_GRAY, 
        
        /// <summary>
        /// CMYK colorspace.
        /// When compressing the JPEG image, the C, M, Y, and K components in the source image are reordered into image planes, 
        /// but no colorspace conversion or subsampling is performed. 
        /// CMYK JPEG images can only be decompressed to CMYK pixels. 
        /// </summary>
        TJCS_CMYK, 
        
        /// <summary>
        /// YCCK colorspace.
        /// YCCK (AKA "YCbCrK") is not an absolute colorspace but rather a mathematical transformation of CMYK designed solely for storage and transmission. 
        /// It is to CMYK as YCbCr is to RGB. 
        /// CMYK pixels can be reversibly transformed into YCCK, and as with YCbCr, 
        /// the chrominance components in the YCCK pixels can be subsampled without incurring major perceptual loss. 
        /// YCCK JPEG images can only be compressed from and decompressed to CMYK pixels. 
        /// </summary>
        TJCS_YCCK
    }
}
