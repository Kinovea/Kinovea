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
    /// The profile manager is the collection of variable tables.
    /// Each variable table contains one or more variables and their values for different profiles.
    /// 
    /// For example we can have a table "athletes" with variables "bib" and "name".
    /// The first column contains the key and is used to identify the profile in the menus.
    /// A variable table / csv file might contain a single column but the header row must always be present.
    /// 
    /// This is effectively a database of custom variables but we keep them as CSV files for ease 
    /// of modification and inspection by external tools.
    /// 
    /// Each variable table object maintains its currently active record.
    /// </summary>
    public class ProfileManager
    {
        #region Properties
        public Dictionary<string, VariableTable> VariableTables { get; private set; } = new Dictionary<string, VariableTable>();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Load the tables.
        /// This re-imports from CSV and will reset the current selected profile.
        /// For adding tables on the fly use AddTable.
        /// </summary>
        public void Initialize()
        {
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

        public void LoadFile(string file)
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
