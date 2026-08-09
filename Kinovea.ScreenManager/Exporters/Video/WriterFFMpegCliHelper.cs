using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Kinovea.ScreenManager
{
    public class WriterFFMpegCliHelper
    {
        public static string BuildArguments(Size inputSize, Size outputSize, double frameRate, string formatString, string outputPath)
        {
            StringBuilder arguments = new StringBuilder();

            AddArgument(arguments, "-hide_banner");
            AddArgument(arguments, "-loglevel");
            AddArgument(arguments, "error");

            // Overwrite output file without asking.
            AddArgument(arguments, "-y");

            // Input: headerless raw frames through stdin.
            AddArgument(arguments, "-f");
            AddArgument(arguments, "rawvideo");

            AddArgument(arguments, "-pixel_format");
            AddArgument(arguments, "bgr24");

            AddArgument(arguments, "-video_size");
            AddArgument(arguments, string.Format("{0}x{1}", inputSize.Width, inputSize.Height));

            AddArgument(arguments, "-framerate");
            AddArgument(arguments, frameRate.ToString("0.########", CultureInfo.InvariantCulture));

            AddArgument(arguments, "-i");
            AddArgument(arguments, "pipe:0");

            // No audio stream.
            AddArgument(arguments, "-an");

            if (outputSize.Width != inputSize.Width || outputSize.Height != inputSize.Height)
            {
                AddArgument(arguments, "-vf");
                AddArgument(arguments, string.Format("scale={0}:{1}:flags=bicubic", outputSize.Width, outputSize.Height));
            }

            AppendVideoEncodingArguments(arguments);

            if (!string.IsNullOrWhiteSpace(formatString))
            {
                AddArgument(arguments, "-f");
                AddArgument(arguments, formatString);
            }

            // The output filename must be the final argument.
            AddArgument(arguments, outputPath);

            return arguments.ToString();
        }

        private static void AppendVideoEncodingArguments(StringBuilder arguments)
        {
            // TODO: Read from SavingSettings.
            AddArgument(arguments, "-c:v");
            AddArgument(arguments, "libx264");

            AddArgument(arguments, "-pix_fmt");
            AddArgument(arguments, "yuv420p");

            // intra-only frames and no B-frames:
            // AddArgument(arguments, "-g");
            // AddArgument(arguments, "1");
            // AddArgument(arguments, "-bf");
            // AddArgument(arguments, "0");
        }

        private static void AddArgument(StringBuilder commandLine, string value)
        {
            if (commandLine.Length > 0)
            {
                commandLine.Append(' ');
            }

            commandLine.Append(QuoteCommandLineArgument(value));
        }

        private static string QuoteCommandLineArgument(string value)
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
