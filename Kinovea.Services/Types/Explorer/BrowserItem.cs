using System;
using System.Collections.Generic;
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
        public string Id { get; }
        
        public string DisplayName { get; }
        
        public BrowserItemType Type { get; }

        /// <summary>
        /// Path to the item.
        /// </summary>
        public string FileSystemPath { get; }

        /// <summary>
        /// Clicking the item should navigate to this location.
        /// </summary>
        public BrowserLocation TargetLocation { get; }
    }
}
