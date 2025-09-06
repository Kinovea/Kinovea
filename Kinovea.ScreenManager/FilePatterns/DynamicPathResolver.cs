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
    /// <summary>
    /// Helper class to interpolate variables (builtin and custom) in paths.
    /// </summary>
    public static class DynamicPathResolver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Get the default KVA path of either player or capture, based on the current profile context.
        /// These don't support built-in variables. The path to a default shouldn't move with the date.
        /// If found returns true and put the path in `path`, otherwise returns false.
        /// This is for reading, not for saving.
        /// For saving use AbstractScreen.SaveDefaultAnnotations().
        /// </summary>
        public static bool GetDefaultKVAPath(ref string path, VariablesRepository variablesRepository, bool forPlayer)
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
                path = Resolve(path, variablesRepository, null);

                if (!FilesystemHelper.IsValidPath(path))
                {
                    log.ErrorFormat("The default KVA path is invalid. \"{0}\"", path);
                    return false;
                }

                if (!Path.IsPathRooted(path))
                {
                    log.ErrorFormat("The default KVA path must be rooted. \"{0}\"", path);
                    return false;
                }

                if (!File.Exists(path))
                {
                    log.ErrorFormat("Default KVA path not found: \"{0}\"", path);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Build the basic context for the current date.
        /// Suitable for folders and files.
        /// </summary>
        public static Dictionary<string, string> BuildDateContext()
        {
            Dictionary<string, string> context = new Dictionary<string, string>();
            DateTime now = DateTime.Now;
            context["date"] = string.Format("{0:yyyy-MM-dd}", now);
            context["dateb"] = string.Format("{0:yyyyMMdd}", now);
            context["year"] = string.Format("{0:yyyy}", now);
            context["month"] = string.Format("{0:MM}", now);
            context["day"] = string.Format("{0:dd}", now);
            return context;
        }

        /// <summary>
        /// Gets the next filename with auto-numbering.
        /// This is for people that don't care about auto-naming with date/time variables, 
        /// so that it still "just works" without having to manually change the file name.
        /// Heuristic:
        /// - If the filename contains a variable marker we do nothing.
        /// - If not try to find a number and increment that number.
        /// - If no number is found add one at the end.
        /// </summary>
        public static string ComputeNextFilename(string previousWithoutExtension)
        {
            if (string.IsNullOrEmpty(previousWithoutExtension))
                return "";

            // Bail out if we are using a variable-based dynamic file name.
            if (previousWithoutExtension.Contains("%"))
                return previousWithoutExtension;

            // The user may set the file name to include a sub-folder of the capture folder.
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

        /// <summary>
        /// Replaces all the variables found by their current value.
        /// It's possible for the result to still not be a valid path.
        /// The input doesn't have to be a path (eg: post-recording command line).
        ///
        /// This is used by several features:
        /// - target path of capture (via a known capture folder)
        /// - target path of replay watcher (via a known capture folder)
        /// - post-recording command line.
        /// - default KVA (loading and saving).
        /// </summary>
        public static string Resolve(string text, VariablesRepository variablesRepository, Dictionary<string, string> builtinVariables)
        {
            string result = text;

            // Replace custom variables first, this way they can override the built-in variables.
            result = ReplaceCustomVariables(result, variablesRepository);

            if (builtinVariables == null || builtinVariables.Count == 0)
                return result;

            foreach (var pair in builtinVariables)
            {
                string symbol = string.Format("%{0}%", pair.Key);
                result = result.Replace(symbol, pair.Value);
            }

            log.DebugFormat("Dynamic path interpolation: {0} -> {1}", text, result);

            return result;
        }

        /// <summary>
        /// Replace the custom variables in the passed string using the active context.
        /// </summary>
        private static string ReplaceCustomVariables(string text, VariablesRepository variablesRepository)
        {
            // Note that we don't check if two tables have the same variable name.
            // The first one loaded will take precedence.
            foreach (var pair in variablesRepository.VariableTables)
            {
                VariableTable variableTable = pair.Value;

                if (string.IsNullOrEmpty(variableTable.CurrentKey))
                    continue;

                // Replace all variables using the active context.
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
