using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// When we navigate to a location in the browser tree or from opening a file from elsewhere, 
    /// we build a snapshot of the content at that location. 
    /// This snapshot is what is shared to the UI controls and the thumbnail viewer.
    /// </summary>
    public class BrowserContentSnapshot
    {
        public BrowserLocation Location { get; }
        
        public IReadOnlyList<BrowserItem> Items { get; }
        
        public long Revision { get; }

        public BrowserContentSnapshot(BrowserLocation location, IReadOnlyList<BrowserItem> items, long revision)
        {
            Location = location;
            Items = items;
            Revision = revision;
        }
    }
}
