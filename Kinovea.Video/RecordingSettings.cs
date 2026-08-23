using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Settings passed to the ffmpeg wrapper for setting up the recording of a camera stream.
    /// </summary>
    public class RecordingSettings
    {
        public string FilePath { get; set; }

        public Size ImageSize { get; set; }

        /// <summary>
        /// Value for the quantizer parameter of the FFmpeg encoder.
        /// Range from 1 to 51. Lower is better quality, higher is more compression.
        /// </summary>
        public int Quality { get; set; }

        public ImageFormat ImageFormat { get; set; }
        
        public bool Uncompressed { get; set; }
        
        public double FrameInterval { get; set; }
        
        public double FileFrameInterval { get; set; }
        
        public ImageRotation Rotation { get; set; }
    }
}
