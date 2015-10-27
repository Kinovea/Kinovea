using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class Filenamer
    {
        /// <summary>
        /// Gets the full path, including root directory, filename and extension.
        /// </summary>
        public static string GetFilePath(string root, string subdir, string filename, string extension, Dictionary<FilePatternContexts, string> context)
        {
            root = ReplacePatterns(root, context);
            subdir = ReplacePatterns(subdir, context);
            filename = ReplacePatterns(filename, context);

            return Path.Combine(root, Path.Combine(subdir, filename + extension));
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
                // Number(s) found. Increment the last one.
                // Note that the parameter passed in is without extension, to avoid incrementing "mp4" for example.
                // TODO: handle leading zeroes in the original (001 -> 002).
                Match m = mc[mc.Count - 1];
                int number = int.Parse(m.Value);
                number++;

                // Replace the number in the original.
                next = r.Replace(previous, number.ToString(), 1, m.Index);
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

        public static string GetVideoFileExtension()
        {
            switch (PreferencesManager.CapturePreferences.CapturePathConfiguration.VideoFormat)
            {
                case KinoveaVideoFormat.MKV: return ".mkv";
                case KinoveaVideoFormat.AVI: return ".avi";
                default: return ".mp4";
            }
        }

        /// <summary>
        /// Replaces all the variables found by their current values.
        /// </summary>
        private static string ReplacePatterns(string pattern, Dictionary<FilePatternContexts, string> context)
        {
            string result = pattern;

            foreach (FilePatternContexts key in FilePatternSymbols.Symbols.Keys)
            {
                if (!context.ContainsKey(key))
                    continue;

                result = result.Replace(FilePatternSymbols.Symbols[key], context[key]);
            }

            return result;
        }
    }
}
