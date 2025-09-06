using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Kinovea.Services;
using System.Web.Profile;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace Kinovea.ScreenManager
{
    public static class Filenamer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets the full path to a capture folder.
        /// </summary>
        public static string ResolveCaptureFolder(string folder, VariablesRepository profileManager, Dictionary<string, string> context)
        {
            return ReplacePatterns(folder, profileManager, context);
        }

        /// <summary>
        /// Gets the full path to save recorded images or videos.
        /// This includes the directory, filename and extension.
        /// The directory and filename may contain variables to be interpolated.
        /// </summary>
        public static string ResolveOutputFilePath(string folder, string filename, string extension, VariablesRepository profileManager, Dictionary<string, string> context)
        {
            folder = ReplacePatterns(folder, profileManager, context);
            filename = ReplacePatterns(filename, profileManager, context);

            return Path.Combine(folder, filename + extension);
        }

        /// <summary>
        /// Gets the command line for post-recording command.
        /// The command may contain variables to be interpolated.
        /// </summary>
        public static string ResolveCommandLine(string command, VariablesRepository profileManager, Dictionary<string, string> context)
        {
            return ReplacePatterns(command, profileManager, context);
        }

        /// <summary>
        /// Get the default KVA path of either player or capture, based on the current profile context.
        /// If found returns true and put the path in `path`, otherwise returns false.
        /// This is for loading, not for saving.
        /// For saving use AbstractScreen.SaveDefaultAnnotations().
        /// </summary>
        public static bool GetDefaultKVAPath(ref string path, VariablesRepository profileManager, bool forPlayer)
        {
            path = forPlayer ? PreferencesManager.PlayerPreferences.PlaybackKVA : PreferencesManager.CapturePreferences.CaptureKVA;
            if (string.IsNullOrEmpty(path))
                return false;

            string filename = forPlayer ? "player.kva" : "capture.kva";
            string standardPath = Path.Combine(Software.SettingsDirectory, filename);

            if (path == filename)
            {
                // Standard location.
                path = standardPath;
                return true;
            }
            else
            {
                path = ResolveDefaultKVAPath(path, profileManager);

                if (!Path.IsPathRooted(path))
                {
                    log.ErrorFormat("The default path must resolve to a full directory and file. {0}", path);
                    return false;
                }

                if (!File.Exists(path))
                {
                    log.ErrorFormat("Default annotations file not found: {0}", path);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Resolve variables for a default KVA path.
        /// </summary>
        public static string ResolveDefaultKVAPath(string path, VariablesRepository profileManager)
        {
            return ReplacePatterns(path, profileManager, null);
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
        /// Replaces all the variables found by their current value.
        /// </summary>
        private static string ReplacePatterns(string text, VariablesRepository profileManager, Dictionary<string, string> context)
        {
            string result = text;

            log.DebugFormat("Replace pattern input: {0}", text);

            // Replace custom variables first, this way they can override the built-in variables.
            result = ReplaceCustomVariables(result, profileManager);

            if (context == null || context.Count == 0)
                return result;

            log.DebugFormat("Replace pattern after profile: {0}", result);

            // Sort variables in descending length order so we test for %datetime before
            // testing for %date or %time.
            // A better way might be to support %{date} and %{time} flavor.
            var sortedContext = context.OrderBy(pair => -pair.Value.Length);
            foreach (var pair in sortedContext)
            {
                string symbol = string.Format("%{0}%", pair.Key);
                result = result.Replace(symbol, pair.Value);
            }

            log.DebugFormat("Replace pattern after context: {0}", result);

            return result;
        }

        /// <summary>
        /// Replace the custom variables in the passed string using the active profile.
        /// </summary>
        private static string ReplaceCustomVariables(string text, VariablesRepository profileManager)
        {
            // Note that we don't check if two tables have the same variable name.
            // The first one loaded will take precedence.
            foreach (var pair in profileManager.VariableTables)
            {
                VariableTable variableTable = pair.Value;

                if (string.IsNullOrEmpty(variableTable.CurrentKey))
                    continue;

                // Replace all variables using the active profile.
                foreach (var variable in variableTable.VariableNames)
                {
                    // We keep them verbatim so this is case sensitive.
                    string symbol = string.Format("%{0}%", variable);
                    text = text.Replace(symbol, variableTable.GetValue(variable));
                }
            }

            return text;
        }
    }
}
