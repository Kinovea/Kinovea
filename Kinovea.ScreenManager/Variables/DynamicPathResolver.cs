using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Kinovea.Services;
using System.Globalization;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Helper class to interpolate variables (builtin and custom) in paths.
    /// </summary>
    public static class DynamicPathResolver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Get the default KVA path of either player or capture, with interpolation of context variables.
        /// If found returns true and put the path in `path`, otherwise returns false.
        /// This is for reading, not for saving.
        /// For saving use AbstractScreen.SaveDefaultAnnotations().
        /// </summary>
        public static bool GetDefaultKVAPath(ref string path, AbstractScreen screen, bool forPlayer)
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
                // Non-standard location, it may contain variables.
                var builtinVariables = BuildKVAPathContext(screen);
                path = Resolve(path, builtinVariables);

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
        /// Returns true the default KVA for the passed screen type is dynamic.
        /// ex: in player screen, do we show a menu to save the default capture KVA?
        /// When the KVA path contains variables it becomes screen-specific.
        /// In this case to avoid any confusion we only show the menu of the corresponding screen type.
        /// Uses the presence of "%" character as a proxy for "contains variables".
        /// </summary>
        public static bool CanSaveCrossDefaultKVA(bool forPlayer)
        {
            // In a player screen check if the capture kva is screen specific.
            string defaultCaptureKVA = PreferencesManager.CapturePreferences.CaptureKVA;
            if (forPlayer && !string.IsNullOrEmpty(defaultCaptureKVA))
            {
                return IsDynamicPath(defaultCaptureKVA);
            }

            // In a capture screen check if the player kva is screen specific.
            string defaultPlaybackKVA = PreferencesManager.PlayerPreferences.PlaybackKVA;
            if (!forPlayer && !string.IsNullOrEmpty(defaultPlaybackKVA))
            {
                return IsDynamicPath(defaultPlaybackKVA);
            }

            return true;
        }

        public static bool IsDynamicPath(string path)
        {
            return !string.IsNullOrEmpty(path) && path.Contains("%");
        }


        /// <summary>
        /// Build the basic context for the current date.
        /// Suitable for folders and files.
        /// </summary>
        public static Dictionary<string, string> BuildDateContext(bool includeTime = false)
        {
            Dictionary<string, string> context = new Dictionary<string, string>();
            
            DateTime now = DateTime.Now;

            context["date"]     = string.Format("{0:yyyy-MM-dd}", now);
            context["dateb"]    = string.Format("{0:yyyyMMdd}", now);
            context["year"]     = string.Format("{0:yyyy}", now);
            context["month"]    = string.Format("{0:MM}", now);
            context["day"]      = string.Format("{0:dd}", now);

            if (includeTime)
            {
                context["time"]         = string.Format("{0:HHmmss}", now);
                context["hour"]         = string.Format("{0:HH}", now);
                context["minute"]       = string.Format("{0:mm}", now);
                context["second"]       = string.Format("{0:ss}", now);
                context["millisecond"]  = string.Format("{0:fff}", now);
            }

            return context;
        }

        /// <summary>
        /// Build the basic context for KVA path interpolation.
        /// </summary>
        public static Dictionary<string, string> BuildKVAPathContext(AbstractScreen screen)
        {
            Dictionary<string, string> context = new Dictionary<string, string>();

            // The only built-in variable we support is a stable screen identifier.
            // This is not the screen id itself otherwise closing and reopening a screen
            // in the same window would lose the link to the annotations.
            // We use something more stable, based on the window id and the screen index.
            string windowIdName = WindowManager.ActiveWindow.Id.ToString().Substring(0, 8);
            string screenIndex = screen.Index.ToString();
            string screenFolder = string.Format("{0}-{1}", windowIdName, screenIndex);
            context["screen"] = screenFolder;

            // Add the standard path to the KVA repository + screen derived identifier.
            // This is what we'll actually use internally.
            // The other one is kept for convenience in case the user wants to recreate
            // the feature manually at an other location.
            context["kvarepo"] = Path.Combine(Software.KVARepository, screenFolder);

            return context;
        }

        /// <summary>
        /// Gets the next filename with possible auto-numbering.
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

            // Bail out if auto-numbering is disabled.
            if (!PreferencesManager.CapturePreferences.CaptureAutomationConfiguration.EnableAutoNumbering)
                return previousWithoutExtension;

            // Bail out if we are using a variable-based dynamic file name.
            if (previousWithoutExtension.Contains("%"))
                return previousWithoutExtension;

            // The user may set the file name to include a sub-folder of the capture folder.
            bool hasEmbeddedDirectory = false;
            string embeddedDirectory = Path.GetDirectoryName(previousWithoutExtension);
            if (!string.IsNullOrEmpty(embeddedDirectory))
                hasEmbeddedDirectory = true;

            string previous = hasEmbeddedDirectory ? Path.GetFileName(previousWithoutExtension) : previousWithoutExtension;
            string next = "";
            bool numberFound = false;
            try
            {
                // Find all numbers in the name, if any.
                Regex r = new Regex(@"\d+");
                MatchCollection mc = r.Matches(previous);
                
                if (mc.Count > 0)
                {
                    // Number(s) found. Increment the last one retaining leading zeroes.
                    // Note that the parameter passed in is without extension, to avoid incrementing "mp4" for example.
                    Match m = mc[mc.Count - 1];

                    int value;
                    bool parsed = int.TryParse(m.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                    if (parsed)
                    {
                        value++;
                        string token = value.ToString().PadLeft(m.Value.Length, '0');

                        // Replace the number in the original.
                        next = r.Replace(previous, token, 1, m.Index);
                        numberFound = true;
                    }
                    else
                    {
                        log.DebugFormat("Number found but not parseable as 32 bit integer");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while computing next filename.", ex);
            }

            if (!numberFound)
            {
                next = string.Format("{0} - 2", previous);
            }

            if (hasEmbeddedDirectory)
            {
                next = Path.Combine(embeddedDirectory, next);
            }

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
        public static string Resolve(string text, Dictionary<string, string> builtinVariables, bool forInsertDialog = false)
        {
            string result = text;

            // Replace custom variables first, this way they can override the built-in variables.
            result = ReplaceCustomVariables(result, forInsertDialog);

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
        private static string ReplaceCustomVariables(string text, bool forInsertDialog)
        {
            if (!forInsertDialog && !PreferencesManager.CapturePreferences.ContextEnabled)
                return text;

            // Note that we don't check if two tables have the same variable name.
            // The first one loaded will take precedence.
            foreach (var pair in VariablesRepository.VariableTables)
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
