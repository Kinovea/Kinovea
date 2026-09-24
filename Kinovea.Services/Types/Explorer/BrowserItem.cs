using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// Abstraction over an item at a browser location.
    /// Typically a file or folder on the file system.
    /// </summary>
    public class BrowserItem
    {
        public string DisplayName { get; }
        
        public BrowserItemType Type { get; }

        /// <summary>
        /// Path to the item.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Clicking the item should navigate to this location.
        /// </summary>
        public BrowserLocation Target { get; }

        public BrowserItem(string displayName, BrowserItemType type, string path, BrowserLocation target)
        {
            DisplayName = displayName;
            Type = type;
            Path = path;
            Target = target;
        }

        public static BrowserItem FromFile(string path)
        {
            string displayName = System.IO.Path.GetFileName(path);
            BrowserItemType type = BrowserItemType.File;
            BrowserLocation target = new BrowserLocation(path);

            return new BrowserItem(displayName, type, path, target);

        }
    }
}
