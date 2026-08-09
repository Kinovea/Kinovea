using Microsoft.VisualBasic.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

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

        public static List<ExportProfile> ExportProfiles { get; } = new List<ExportProfile>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static ExportProfile()
        {
            ExportProfiles.Clear();
            ExportProfiles.Add(new ExportProfile()
            {
                Name = "Web",
                Codec = VideoCodec.H264,
                UseConstantBitrate = false,
                EncodingQuality = EncodingQuality.High,
                EncodingSpeed = EncodingSpeed.Medium,
                Bitrate = 6000,
                MinBitrate = 0,
                MaxBitrate = 9000,
                GOPSize = 12,
            }
            );

            ExportProfiles.Add(new ExportProfile() 
            {
                Name = "Frame by frame",
                Codec = VideoCodec.MJPEG,
                UseConstantBitrate = false,
                EncodingQuality = EncodingQuality.High,
                EncodingSpeed = EncodingSpeed.Medium,
                Bitrate = 6000,
                MinBitrate = 0,
                MaxBitrate = 9000,
                GOPSize = 1,
            });
        }

        /// <summary>
        /// Mapping from quality to MJPEG quantizer.
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
        /// Mapping from quality to CRF value for H.264 and H.265.
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

        public static string GetEncodingSpeedPreset(EncodingSpeed speed)
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

        #region Serialization
        public void ReadXml(XmlReader r)
        {
            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Name":
                        Name = r.ReadElementContentAsString();
                        break;
                    case "Container":
                        Container = XmlHelper.ParseEnum<VideoContainer>(r.ReadElementContentAsString(), VideoContainer.MP4);
                        break;
                    case "Codec":
                        Codec = XmlHelper.ParseEnum<VideoCodec>(r.ReadElementContentAsString(), VideoCodec.H264);
                        break;
                    case "UseConstantBitrate":
                        UseConstantBitrate = XmlHelper.ParseBoolean(r.ReadElementContentAsString());
                        break;
                    case "EncodingQuality":
                        EncodingQuality = XmlHelper.ParseEnum<EncodingQuality>(r.ReadElementContentAsString(), EncodingQuality.High);
                        break;
                    case "EncodingSpeed":
                        EncodingSpeed = XmlHelper.ParseEnum<EncodingSpeed>(r.ReadElementContentAsString(), EncodingSpeed.Medium);
                        break;
                    case "Bitrate":
                        Bitrate = long.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "MinBitrate":
                        MinBitrate = long.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "MaxBitrate":
                        MaxBitrate = long.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    case "GOPSize":
                        GOPSize = int.Parse(r.ReadElementContentAsString(), CultureInfo.InvariantCulture);
                        break;
                    default:
                        string outerXml = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in XML: {0}", outerXml);
                        break;
                }
            }

            r.ReadEndElement();
        }

        public void WriteXml(XmlWriter w)
        {
            w.WriteElementString("Name", Name);
            w.WriteElementString("Container", Container.ToString());
            w.WriteElementString("Codec", Codec.ToString());
            w.WriteElementString("UseConstantBitrate", XmlHelper.WriteBoolean(UseConstantBitrate));
            w.WriteElementString("EncodingQuality", EncodingQuality.ToString());
            w.WriteElementString("EncodingSpeed", EncodingSpeed.ToString());
            w.WriteElementString("Bitrate", Bitrate.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("MinBitrate", MinBitrate.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("MaxBitrate", MaxBitrate.ToString(CultureInfo.InvariantCulture));
            w.WriteElementString("GOPSize", GOPSize.ToString(CultureInfo.InvariantCulture));
        }
        #endregion
    }
}
