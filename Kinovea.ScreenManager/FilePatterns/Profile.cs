using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace Kinovea.ScreenManager
{
    /// <summary>
    ///  A profile is a set of custom variables and the corresponding values for each configuration.
    ///  These variables can be used in paths used by the preferences.
    ///
    ///  Profiles are stored as CSV files.
    ///  - Each column header is a variable name.
    ///  - The first column contains the keys that identify the profile.
    ///  - Each row contains the values for each variable for a specific profile.
    /// </summary>
    public class Profile
    {
        #region Properties
        /// <summary>
        /// Values of the first column. This is used to identify the profile.
        /// </summary>
        public List<string> Keys { get; private set; } = new List<string>();

        /// <summary>
        /// Key of the currently selected profile.
        /// </summary>
        public string CurrentKey { get; set; } = string.Empty;

        /// <summary>
        /// List of defined variables (column headers).
        /// </summary>
        public List<string> Variables { get; private set; } = new List<string>();
        #endregion

        #region Members
        public Dictionary<string, List<string>> profiles = new Dictionary<string, List<string>>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Import a CSV file containing profiles.
        /// Always unload the current set of profiles.
        /// </summary>
        public void Import(string csvFile)
        {
            ClearProfiles();


            using (var reader = new StreamReader(csvFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                List<List<string>> records = new List<List<string>>();

                // Column headers are the variable names.
                Variables = csv.HeaderRecord.ToList();

                while (csv.Read())
                {
                    // Each row is one profile.
                    List<string> record = new List<string>();
                    for (int i = 0; i < Variables.Count; i++)
                    {
                        string value = csv.GetField<string>(i);
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
                        profiles.Add(record[0], record);
                    }
                }
            }

            if (Keys.Count == 0)
            {
                log.Error("No profiles were loaded from the CSV file.");
                return;
            }

            // Set the first key as the current key.
            CurrentKey = Keys[0]; 
        }

        /// <summary>
        /// Get the value of a variable for the current profile.
        /// </summary>
        public string GetValue(string variableName)
        {
            if (string.IsNullOrEmpty(CurrentKey) || !profiles.ContainsKey(CurrentKey))
            {
                log.Error("Current key is not set or does not exist in profiles.");
                return string.Empty;
            }

            if (!Variables.Contains(variableName))
            {
                log.ErrorFormat("Variable '{0}' does not exist in the profile.", variableName);
                return string.Empty;
            }

            int index = Variables.IndexOf(variableName);
            return profiles[CurrentKey][index];
        }

        /// <summary>
        /// Clear all profile data.
        /// </summary>
        private void ClearProfiles()
        {
            Variables.Clear();
            Keys.Clear();
            CurrentKey = string.Empty;
            profiles.Clear();
        }
    }
}
