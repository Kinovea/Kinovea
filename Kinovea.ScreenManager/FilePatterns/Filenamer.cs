using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public static class Filenamer
    {
        /// <summary>
        /// Gets the full path, including root directory, filename and extension.
        /// </summary>
        public static string GetFilePath(string root, string subdir, string filename, string extension, Dictionary<PatternContext, string> context)
        {
            root = ReplacePatterns(root, PatternSymbolsFile.Symbols, context);
            subdir = ReplacePatterns(subdir, PatternSymbolsFile.Symbols, context);
            filename = ReplacePatterns(filename, PatternSymbolsFile.Symbols, context);

            return Path.Combine(root, Path.Combine(subdir, filename + extension));
        }

        /// <summary>
        /// Gets the full command line.
        /// </summary>
        public static string GetCommandLine(string command, Dictionary<PatternContext, string> context)
        {
            return ReplacePatterns(command, PatternSymbolsCommand.Symbols, context);
        }


        /// <summary>
        /// Gets the next filename to allow continued recording without requiring the user to update the filename manually.
        /// If the filename contains a pattern we assume the pattern will contain the time and do nothing.
        /// If not, we try to find a number and increment that number.
        /// If no number is found in the filename, we add one.
        /// </summary>
        public static string ComputeNextFilename(string previousWithoutExtension)
        {
            if (string.IsNullOrEmpty(previousWithoutExtension))
                return "";

            if (previousWithoutExtension.Contains("%"))
                return previousWithoutExtension;

            bool hasEmbeddedDirectory = false;
            string embeddedDirectory = Path.GetDirectoryName(previousWithoutExtension);
            if (!string.IsNullOrEmpty(embeddedDirectory))
                hasEmbeddedDirectory = true;

            string previous = hasEmbeddedDirectory ? Path.GetFileName(previousWithoutExtension) : previousWithoutExtension;

            // Find all numbers in the name, if any.
            Regex r = new Regex(@"\d+");
            MatchCollection mc = r.Matches(previous);

            string next = "";
            if (mc.Count > 0)
            {
                // Number(s) found. Increment the last one retaining leading zeroes.
                // Note that the parameter passed in is without extension, to avoid incrementing "mp4" for example.
                Match m = mc[mc.Count - 1];
                int value = int.Parse(m.Value);
                value++;

                string token = value.ToString().PadLeft(m.Value.Length, '0');

                // Replace the number in the original.
                next = r.Replace(previous, token, 1, m.Index);
            }
            else
            {
                // No number found, add suffix.
                next = string.Format("{0} - 2", previous);
            }

            if (hasEmbeddedDirectory)
                next = Path.Combine(embeddedDirectory, next);

            return next;
        }

        public static string GetImageFileExtension()
        {
            switch (PreferencesManager.CapturePreferences.CapturePathConfiguration.ImageFormat)
            {
                case KinoveaImageFormat.PNG: return ".png";
                case KinoveaImageFormat.BMP: return ".bmp";
                default: return ".jpg";
            }
        }

        public static string GetVideoFileExtension(bool uncompressed)
        {
            if (uncompressed)
            {
                switch (PreferencesManager.CapturePreferences.CapturePathConfiguration.UncompressedVideoFormat)
                {
                    case KinoveaUncompressedVideoFormat.AVI: return ".avi";
                    case KinoveaUncompressedVideoFormat.MKV: 
                    default: return ".mkv";
                }
            }
            else
            {
                switch (PreferencesManager.CapturePreferences.CapturePathConfiguration.VideoFormat)
                {
                    case KinoveaVideoFormat.MKV: return ".mkv";
                    case KinoveaVideoFormat.AVI: return ".avi";
                    case KinoveaVideoFormat.MP4:
                    default: return ".mp4";
                }
            }
        }

        /// <summary>
        /// Replaces all the variables found by their current values.
        /// </summary>
        private static string ReplacePatterns(string text, Dictionary<PatternContext, string> symbols, Dictionary<PatternContext, string> context)
        {
            string result = text;

            // Sort symbols in descending length so we test for %datetime before testing for %date or %time.
            var sortedSymbols = symbols.OrderBy(pair => -pair.Value.Length);

            foreach (var symbol in sortedSymbols)
            {
                if (!context.ContainsKey(symbol.Key))
                    continue;

                result = result.Replace(symbols[symbol.Key], context[symbol.Key]);
            }

            return result;
        }
    }
}
