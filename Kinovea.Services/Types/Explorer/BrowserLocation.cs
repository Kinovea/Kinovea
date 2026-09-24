using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// Abstraction over a location that can be browsed to in the navigation pane.
    /// Typically a folder on the file system, but could be a virtual folder like "recent files".
    /// </summary>
    public class BrowserLocation
    {
        /// <summary>
        /// The type of location.
        /// </summary>
        public BrowserLocationType Type { get; }

        /// <summary>
        /// Used for comparison and persistence.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// If the location is a folder on the file system.
        /// </summary>
        public string Path { get; }

        public BrowserLocation(string path)
        {
            Type = BrowserLocationType.FileSystem;
            Key = path;
            Path = path;
        }

        public static BrowserLocation FromFileSystem(string path)
        {
            return new BrowserLocation(path);
        }
    }
}
