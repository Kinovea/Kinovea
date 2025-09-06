using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services.TurboJpeg
{
    public enum TJXOP
    {
        TJXOP_NONE = 0,
        
        /// <summary>
        /// Flip (mirror) image horizontally.  This transform is imperfect if there 
        /// are any partial MCU blocks on the right edge (see #TJXOPT_PERFECT.)
        /// </summary>
        TJXOP_HFLIP,
        
        /**
         * Flip (mirror) image vertically.  This transform is imperfect if there are
         * any partial MCU blocks on the bottom edge (see #TJXOPT_PERFECT.)
         */
        TJXOP_VFLIP,
        
        /**
         * Transpose image (flip/mirror along upper left to lower right axis.)  This
         * transform is always perfect.
         */
        TJXOP_TRANSPOSE,
        
        /**
         * Transverse transpose image (flip/mirror along upper right to lower left
         * axis.)  This transform is imperfect if there are any partial MCU blocks in
         * the image (see #TJXOPT_PERFECT.)
         */
        TJXOP_TRANSVERSE,
        
        /**
         * Rotate image clockwise by 90 degrees.  This transform is imperfect if
         * there are any partial MCU blocks on the bottom edge (see
         * #TJXOPT_PERFECT.)
         */
        TJXOP_ROT90,
        
        /**
         * Rotate image 180 degrees.  This transform is imperfect if there are any
         * partial MCU blocks in the image (see #TJXOPT_PERFECT.)
         */
        TJXOP_ROT180,
        
        /**
         * Rotate image counter-clockwise by 90 degrees.  This transform is imperfect
         * if there are any partial MCU blocks on the right edge (see
         * #TJXOPT_PERFECT.)
         */
        TJXOP_ROT270
    }
}
