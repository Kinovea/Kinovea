using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CSVParser;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// The variables repository is a collection of tables with custom variables.
    /// Each table contains one or more variables (columns) and their values (rows).
    /// 
    /// For example we can have a table "athletes" with variables "bib" and "name".
    /// The first column always contains the key and is used in the UI for the user
    /// to select the active context from the menus or drop downs.
    /// 
    /// What we call "the context" is the union of the selected row of each table.
    /// 
    /// A variable table (csv file) may contain a single column but the header row must always be present.
    /// 
    /// This is effectively a database of custom variables but we keep them as CSV files for ease 
    /// of modification and inspection by external tools.
    /// 
    /// Each variable table object maintains its currently active record.
    /// </summary>
    public static class VariablesRepository
    {
        #region Properties

        public static bool HasVariables
        {
            get { return VariableTables.Count > 0; }
        }

        /// <summary>
        /// List of variable tables indexed by the csv file name.
        /// </summary>
        public static Dictionary<string, VariableTable> VariableTables { get; private set; } = new Dictionary<string, VariableTable>();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Load the tables.
        /// This re-imports all from CSV.
        /// Resets the current selection to what's in core preferences.
        /// Should only be called once during general construction.
        /// </summary>
        public static void Initialize()
        {
            VariableTables.Clear();

            // Import the saved context if any.
            Dictionary<string, string> savedContext = ParseContextString(PreferencesManager.CapturePreferences.ContextString);

            // Load all variable tables from the CSV files.
            string path = Software.VariablesDirectory;

            if (!Directory.Exists(path))
                return;

            // Don't search in sub-directories, to make it easy to "exclude" files by moving them to a sub-directory.
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(file).ToLower().Equals(".csv") ||
                    Path.GetExtension(file).ToLower().Equals(".txt"))
                {
                    LoadFile(file, savedContext);
                }
            }
        }

        /// <summary>
        /// Load one variable table into the repository.
        /// </summary>
        public static void LoadFile(string file, Dictionary<string, string> savedContext = null)
        {
            string tableName = Path.GetFileNameWithoutExtension(file);
            if (VariableTables.ContainsKey(tableName))
            {
                log.ErrorFormat("Variable table \"{0}\" already exists.", tableName);
                return;
            }

            VariableTable table = new VariableTable();
            table.Import(file);
            VariableTables.Add(tableName, table);

            // Restore the saved context if any.
            if (savedContext != null && savedContext.ContainsKey(tableName))
            {
                table.CurrentKey = savedContext[tableName];
            }
        }

        /// <summary>
        /// Save the current context to shared preferences.
        /// Raises a global event to notify other modules, and other instances.
        /// The passed action is run after the preferences update but before the event is raised.
        /// This is used to update the local UI asap before other instances.
        /// </summary>
        public static void SaveContext(Action action)
        {
            log.DebugFormat("Saving context");
            string contextString = GetContextString();
            PreferencesManager.BeforeRead();
            PreferencesManager.CapturePreferences.ContextString = contextString;
            action?.Invoke();
            NotificationCenter.RaiseTriggerPreferencesUpdated(null, true);
        }

        /// <summary>
        /// Save the current state of the context enabled flag to shared preferences.
        /// Raises a global event to notify other modules, and other instances.
        /// The passed action is run after the preferences update but before the event is raised.
        /// This is used to update the local UI asap before other instances.
        /// </summary>
        public static void SaveContextEnabled(bool enabled, Action action)
        {
            log.DebugFormat("Saving context enabled flag (enabled:{0})", enabled);
            PreferencesManager.BeforeRead();
            PreferencesManager.CapturePreferences.ContextEnabled = enabled;
            action?.Invoke();
            NotificationCenter.RaiseTriggerPreferencesUpdated(null, true);
        }

        /// <summary>
        /// Force context from the outside.
        /// This is used after a change in preferences in another window.
        /// </summary>
        public static void SetContextFromString(string contextString)
        {
            var savedContext = ParseContextString(contextString);
            foreach (var table in VariableTables)
            {
                if (savedContext != null && savedContext.ContainsKey(table.Key))
                {
                    table.Value.CurrentKey = savedContext[table.Key];
                }
            }
        }

        /// <summary>
        /// Return a string representation of the currently selected context.
        /// </summary>
        private static string GetContextString()
        {
            // Stores the context as a comma separated list of "key:value" pairs in CSV compliant format.
            // Enclosed in quotes to handle commas in names. No spacing between cells.
            List<string> contexts = new List<string>();
            foreach (var table in VariableTables)
            {
                contexts.Add(string.Format("\"{0}:{1}\"", table.Key, table.Value.CurrentKey));
            }

            return string.Join(",", contexts);
        }

        /// <summary>
        /// Convert the stored context string into a dictionary of table/key pairs.
        /// </summary>
        private static Dictionary<string, string> ParseContextString(string contextString)
        {
            Dictionary<string, string> savedContext = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(contextString))
                return savedContext;

            // Parse the context string.
            using (TextReader sr = new StringReader(contextString))
            using (var parser = new CsvTextFieldParser(sr))
            {
                string[] cells = parser.ReadFields();
                foreach (var c in cells)
                {
                    // Group everything after the first colon to allow colons in the value part.
                    var kv = c.Split(new char[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        string table = kv[0];
                        string key = kv[1];
                        if (!savedContext.ContainsKey(table))
                            savedContext.Add(table, key);
                    }
                }
            }

            //var subContexts = contextString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            
            return savedContext;
        }

    }
}
