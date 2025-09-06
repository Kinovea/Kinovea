using Kinovea.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        /// This re-imports from CSV and will reset the current selected profile.
        /// </summary>
        public static void Initialize()
        {
            VariableTables.Clear();

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
                    LoadFile(file);
                }
            }
        }

        /// <summary>
        /// Load one variable table into the repository.
        /// </summary>
        public static void LoadFile(string file)
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
        }

    }
}
