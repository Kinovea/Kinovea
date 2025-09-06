using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services.TurboJpeg
{
    public enum TJPF
    {
        /// <summary>
        /// RGB pixel format.  
        /// The red, green, and blue components in the image are stored in 3-byte pixels 
        /// in the order R, G, B from lowest to highest byte address within each pixel.
        /// </summary>
        TJPF_RGB = 0,
       
        /// <summary>
        /// BGR pixel format.
        /// The red, green, and blue components in the image are stored in 3-byte pixels 
        /// in the order B, G, R from lowest to highest byte address within each pixel.
        /// </summary>
        TJPF_BGR,

        /// <summary>
        /// RGBX pixel format. 
        /// The red, green, and blue components in the image are stored in 4-byte pixels 
        /// in the order R, G, B from lowest to highest byte address within each pixel. 
        /// The X component is ignored when compressing and undefined when decompressing.
        /// </summary>
        TJPF_RGBX,

        /// <summary>
        /// BGRX pixel format. 
        /// The red, green, and blue components in the image are stored in 4-byte pixels 
        /// in the order B, G, R from lowest to highest byte address within each pixel. 
        /// The X component is ignored when compressing and undefined when decompressing.
        /// </summary>
        TJPF_BGRX,

        /// <summary>
        /// XBGR pixel format. 
        /// The red, green, and blue components in the image are stored in 4-byte pixels 
        /// in the order R, G, B from highest to lowest byte address within each pixel. 
        /// The X component is ignored when compressing and undefined when decompressing. 
        /// </summary>
        TJPF_XBGR,
        
        /// <summary>
        /// XRGB pixel format. 
        /// The red, green, and blue components in the image are stored in 4-byte pixels 
        /// in the order B, G, R from highest to lowest byte address within each pixel. 
        /// The X component is ignored when compressing and undefined when decompressing.
        /// </summary>
        TJPF_XRGB,
        
        /// <summary>
        /// Grayscale pixel format. 
        /// Each 1-byte pixel represents a luminance (brightness) level from 0 to 255.
        /// </summary>
        TJPF_GRAY,
        
        /// <summary>
        /// RGBA pixel format. 
        /// This is the same as TJPF_RGBX, except that when decompressing, 
        /// the X component is guaranteed to be 0xFF, which can be interpreted as an opaque alpha channel. 
        /// </summary>
        TJPF_RGBA,
        
        /// <summary>
        /// BGRA pixel format. 
        /// This is the same as TJPF_BGRX, except that when decompressing, 
        /// the X component is guaranteed to be 0xFF, which can be interpreted as an opaque alpha channel. 
        /// </summary>
        TJPF_BGRA,
        
        /// <summary>
        /// ABGR pixel format. 
        /// This is the same as TJPF_XBGR, except that when decompressing, 
        /// the X component is guaranteed to be 0xFF, which can be interpreted as an opaque alpha channel. 
        /// </summary>
        TJPF_ABGR,
        
        /// <summary>
        /// ARGB pixel format. 
        /// This is the same as TJPF_XRGB, except that when decompressing, 
        /// the X component is guaranteed to be 0xFF, which can be interpreted as an opaque alpha channel. 
        /// </summary>
        TJPF_ARGB
    }
}
