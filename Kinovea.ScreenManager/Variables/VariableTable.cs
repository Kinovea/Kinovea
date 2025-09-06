using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;
using CSVParser;

namespace Kinovea.ScreenManager
{
    /// <summary>
    ///  A variable "table" (one csv file) contains a set of custom variables (columns) and 
    ///  the corresponding values for each configuration (rows).
    ///  These variables can be used in various places like capture folder paths.
    ///
    ///  Tables are stored as CSV files.
    ///  - Each column header is a variable name.
    ///  - The first column contains the keys that identify the profile in the menus.
    ///  - Each row contains the values for each variable for a specific profile.
    ///  - The file might contain a single column.
    /// </summary>
    public class VariableTable
    {
        #region Properties
        /// <summary>
        /// Values of the first column. 
        /// This is used to identify and select the context on this table.
        /// </summary>
        public List<string> Keys { get; private set; } = new List<string>();

        /// <summary>
        /// Gets or sets the key of the currently selected context on this table.
        /// </summary>
        public string CurrentKey 
        {
            get
            {
                return currentKey;
            }
            set
            {
                if (!Keys.Contains(value))
                {
                    log.ErrorFormat("Key '{0}' is not in the list of keys.", value);
                }
                else
                {
                    currentKey = value;
                }
            }
        }

        /// <summary>
        /// List of defined variables (column headers).
        /// </summary>
        public List<string> VariableNames { get; private set; } = new List<string>();
        #endregion

        #region Members
        private string currentKey = string.Empty;
        private Dictionary<string, List<string>> rowsDict = new Dictionary<string, List<string>>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Import a CSV file containing variables and their values.
        /// </summary>
        public bool Import(string csvFile)
        {
            Clear();

            try
            {
                if (!File.Exists(csvFile))
                {
                    log.Error("File not found.");
                    return false;
                }

                using (var parser = new CsvTextFieldParser(csvFile))
                {
                    parser.TrimWhiteSpace = true;
                    List<List<string>> records = new List<List<string>>();
                    int count = 0;
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (count++ == 0)
                        {
                            VariableNames = fields.ToList();
                            continue;
                        }

                        if (fields.Length != VariableNames.Count)
                        {
                            log.ErrorFormat("Error: CSV error at line: {0}. The row does not contain the expected number of cells.", count);
                            continue;
                        }

                        // Each row is one profile.
                        List<string> record = new List<string>();
                        for (int i = 0; i < VariableNames.Count; i++)
                        {
                            string value = fields[i];
                            record.Add(value);
                        }

                        if (Keys.Contains(record[0]))
                        {
                            // Error, duplicate profile.
                            log.ErrorFormat("Error: found duplicate profile \"{0}\"", record[0]);
                        }
                        else
                        {
                            Keys.Add(record[0]);
                            rowsDict.Add(record[0], record);
                        }

                    }
                }

                if (Keys.Count == 0)
                {
                    log.Error("No profiles were loaded from the CSV file.");
                    return false;
                }

                // Set the first key as the current key.
                currentKey = Keys[0];
            }
            catch (Exception ex)
            {
                log.Error("Error importing profiles from CSV file.", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the value of a variable for the current profile.
        /// </summary>
        public string GetValue(string variableName)
        {
            if (string.IsNullOrEmpty(currentKey) || !rowsDict.ContainsKey(currentKey))
            {
                log.Error("Current key is not set or does not exist in profiles.");
                return string.Empty;
            }

            if (!VariableNames.Contains(variableName))
            {
                log.ErrorFormat("Variable \"{0}\" does not exist in the profile.", variableName);
                return string.Empty;
            }

            int index = VariableNames.IndexOf(variableName);
            return rowsDict[currentKey][index];
        }

        /// <summary>
        /// Clear all data.
        /// </summary>
        private void Clear()
        {
            VariableNames.Clear();
            Keys.Clear();
            currentKey = string.Empty;
            rowsDict.Clear();
        }
    }
}
