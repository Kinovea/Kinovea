using Kinovea.Services.Types.Export;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    public class ExportProfile
    {
        #region Properties
        public string Name { get; set; } = "Default";

        public VideoContainer Container { get; set; } = VideoContainer.MP4;

        public VideoCodec Codec { get; set; } = VideoCodec.H264;

        /// <summary>
        /// If true, use constant bitrate mode, otherwise use constant quality mode.
        /// </summary>
        public bool UseConstantBitrate { get; set; } = false;

        /// <summary>
        /// Quality setting.
        /// Either constant bitrate or 
        /// </summary>
        public EncodingQuality EncodingQuality { get; set; } = EncodingQuality.High;

        public EncodingSpeed EncodingSpeed { get; set; } = EncodingSpeed.Medium;

        /// <summary>
        /// Bitrate in kbits/s for constant bitrate mode.
        /// </summary>
        public long Bitrate { get; set; } = 6000;
        public long MinBitrate { get; set; } = 0;
        public long MaxBitrate { get; set; } = 9000;

        public int GOPSize { get; set; } = 12;
        #endregion

        public static Dictionary<string, ExportProfile> ExportProfiles { get; } = new Dictionary<string, ExportProfile>();

        static ExportProfile()
        {
            ExportProfiles.Clear();
            ExportProfiles.Add("web", new ExportProfile()
            {
                Name = "Web",
                Codec = VideoCodec.H264,
                UseConstantBitrate = false,
                EncodingQuality = EncodingQuality.High,
                EncodingSpeed = EncodingSpeed.Medium,
                GOPSize = 12,
            }
            );

            ExportProfiles.Add("fbf", new ExportProfile() 
            {
                Name = "Frame by frame",
                Codec = VideoCodec.MJPEG,
                UseConstantBitrate = false,
                EncodingQuality = EncodingQuality.High,
            });
        }

        /// <summary>
        /// Mapping from user-friendly quality setting to MJPEG quantizer.
        /// Practical range is 1 through 31. 
        /// Values around 2 through 5 represent high-quality MJPEG.
        /// </summary>
        public static int GetMJPEGQuality(EncodingQuality quality)
        {
            switch (quality)
            {
                case EncodingQuality.PerceptuallyLossless:
                    {
                        return 1;
                    }
                case EncodingQuality.High:
                    {
                        return 2;
                    }
                case EncodingQuality.Medium:
                    {
                        return 5;
                    }
                case EncodingQuality.Low:
                    {
                        return 10;
                    }
                default:
                    return 2;
            }
        }

        /// <summary>
        /// Mapping from user friendly name to CRF value for H.264 and H.265.
        /// See: https://trac.ffmpeg.org/wiki/Encode/H.264
        /// CRF = Constant Rate Factor = constant quality, as opposed to constant bitrate.
        /// CRF scale is 0–51, default = 23 for H.264, 28 for H.265.
        /// </summary>
        public static int GetCRF(EncodingQuality quality, VideoCodec codec)
        {
            switch (quality)
            {
                case EncodingQuality.PerceptuallyLossless:
                    {
                        return codec == VideoCodec.H264 ? 17 : 20;
                    }
                case EncodingQuality.High:
                    {
                        return codec == VideoCodec.H264 ? 20 : 25;
                    }
                case EncodingQuality.Medium:
                    {
                        return codec == VideoCodec.H264 ? 23 : 28;
                    }
                case EncodingQuality.Low:
                    {
                        return codec == VideoCodec.H264 ? 29 : 32;
                    }
                default:
                    return 20;
            }
        }

        public static string GetPreset(EncodingSpeed speed)
        {
            switch (speed)
            {
                case EncodingSpeed.Fast:
                    return "fast";
                case EncodingSpeed.Medium:
                    return "medium";
                case EncodingSpeed.Slow:
                    return "slow";
                default:
                    return "medium";
            }
        }
    }
}
