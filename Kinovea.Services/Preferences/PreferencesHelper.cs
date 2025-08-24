using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    public static class PreferencesHelper
    {
        /// <summary>
        /// Adds an entry to a list of recent objects. 
        /// Remove the last one if overflow, and handle duplication.
        /// Used for recent files, recent colors, etc.
        /// </summary>
        public static void UpdateRecents<T>(T entry, List<T> recentEntries, int max)
        {
            if (max == 0)
                return;

            int found = -1;
            for (int i = 0; i < max; i++)
            {
                if (i >= recentEntries.Count)
                    break;

                if (recentEntries[i].Equals(entry))
                {
                    found = i;
                    break;
                }
            }

            if (found >= 0)
                recentEntries.RemoveAt(found);
            else if (recentEntries.Count == max)
                recentEntries.RemoveAt(recentEntries.Count - 1);

            recentEntries.Insert(0, entry);
        }

        /// <summary>
        /// Remove entries from a recent files list if they no longer exist in the file system.
        /// Returns true if the list was modified.
        /// </summary>
        public static bool ConsolidateRecentFiles(List<string> recentFiles)
        {
            int oldCount = recentFiles.Count;
            for (int i = recentFiles.Count - 1; i >= 0; i--)
            {
                if (Path.GetFileName(recentFiles[i]).Contains("*"))
                {
                    // Special case for replay folders, which contain a wildcard.
                    if (!Directory.Exists(Path.GetDirectoryName(recentFiles[i])))
                        recentFiles.RemoveAt(i);
                }
                else if (!File.Exists(recentFiles[i]))
                {
                    recentFiles.RemoveAt(i);
                }
            }

            return recentFiles.Count < oldCount;
        }
    }

}
