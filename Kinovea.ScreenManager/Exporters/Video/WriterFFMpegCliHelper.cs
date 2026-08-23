using Kinovea.Services;
using Kinovea.Video;
using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Converts export settings to command line arguments for ffmpeg.
    /// </summary>
    public class WriterFFMpegCliHelper
    {
        public static string BuildArguments(VideoExportSettings settings, Size inputSize, Size outputSize)
        {
            StringBuilder args = new StringBuilder();

            // Prevent the banner from appearing in the console output.
            Add(args, "-hide_banner");
            Add(args, "-loglevel");
            Add(args, "error");

            // Overwrite output file without asking.
            Add(args, "-y");

            // Input is headerless raw frames through stdin.
            Add(args, "-f");
            Add(args, "rawvideo");

            Add(args, "-pixel_format");
            Add(args, "bgr24");

            Add(args, "-video_size");
            Add(args, string.Format("{0}x{1}", inputSize.Width, inputSize.Height));

            double frameRate = 1000.0 / settings.OutputIntervalMilliseconds;
            Add(args, "-framerate");
            Add(args, frameRate.ToString("0.########", CultureInfo.InvariantCulture));

            Add(args, "-i");
            Add(args, "pipe:0");

            // No audio stream.
            Add(args, "-an");

            if (outputSize.Width != inputSize.Width || outputSize.Height != inputSize.Height)
            {
                Add(args, "-vf");
                Add(args, string.Format("scale={0}:{1}:flags=bicubic", outputSize.Width, outputSize.Height));
            }

            AddVideoEncodingArgs(args, settings);

            string formatString = ExportProfile.GetFormatString(settings.ExportProfile.Container);
            Add(args, "-f");
            Add(args, formatString);
            
            // The output filename must be the final argument.
            Add(args, settings.File);

            return args.ToString();
        }

        private static void AddVideoEncodingArgs(StringBuilder args, VideoExportSettings settings)
        {
            ExportProfile p = settings.ExportProfile;

            switch (p.Codec)
            {
                case VideoCodec.MJPEG:
                    {
                        AddMjpegArgs(args, p);
                        break;
                    }
                case VideoCodec.H264:
                    {
                        AddH26xArgs(args, p, "libx264");
                        break;
                    }
                case VideoCodec.H265:
                    {
                        AddH26xArgs(args, p, "libx265");
                        break;
                    }
                default:
                    break;
            }
        }

        private static void AddMjpegArgs(StringBuilder args, ExportProfile p)
        {
            Add(args, "-c:v");
            Add(args, "mjpeg");

            Add(args, "-pix_fmt");
            Add(args, "yuvj420p");

            int q = ExportProfile.GetMJPEGQuality(p.EncodingQuality);
            Add(args, "-q:v");
            Add(args, q.ToString());

            // No preset.
            // No GOP size since we are always intra-only.
        }

        private static void AddH26xArgs(StringBuilder args, ExportProfile p, string name)
        {
            Add(args, "-c:v");
            Add(args, name);

            Add(args, "-pix_fmt");
            Add(args, "yuv420p");
            
            // Speed/compression preset.
            // = Compression effort. Slower preset spends more time seeking better compression.
            // Does not raise or lower the requested quality.
            Add(args, "-preset");
            Add(args, ExportProfile.GetEncodingSpeedPreset(p.EncodingSpeed));

            int crf = ExportProfile.GetCRF(p.EncodingQuality, p.Codec);
            Add(args, "-crf");
            Add(args, crf.ToString());

            // GOP size.
            // 0: encoder default, 1: intra-only.
            if (p.GOPSize > 0)
            {
                Add(args, "-g");
                Add(args, p.GOPSize.ToString());
            }
        }

        private static void Add(StringBuilder args, string value)
        {
            if (args.Length > 0)
            {
                args.Append(' ');
            }

            args.Append(QuoteArg(value));
        }

        private static string QuoteArg(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            bool requiresQuotes = value.Length == 0 || value.IndexOfAny(new[] { ' ', '\t', '\n', '\v', '"' }) >= 0;
            if (!requiresQuotes)
            {
                return value;
            }

            StringBuilder result = new StringBuilder();

            result.Append('"');

            int backslashCount = 0;
            foreach (char character in value)
            {
                if (character == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (character == '"')
                {
                    result.Append('\\', backslashCount * 2 + 1);
                    result.Append('"');
                    backslashCount = 0;
                    continue;
                }

                result.Append('\\', backslashCount);
                backslashCount = 0;
                result.Append(character);
            }

            // Backslashes immediately preceding the closing quote must themselves be doubled.
            result.Append('\\', backslashCount * 2);
            result.Append('"');

            return result.ToString();
        }
    }
}
