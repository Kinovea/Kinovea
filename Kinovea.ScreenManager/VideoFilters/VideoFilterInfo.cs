using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Piece of info about a filter type that is used to build the menu.
    /// </summary>
    public class VideoFilterInfo
    {
        /// <summary>
        /// Internal name of the filter.
        /// This name will be used internally to identify the filter and as an XML tag.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Resource string for the friendly name of the filter, visible in the menus
        /// This name should be localized if it makes sense.
        /// </summary>
        public string FriendlyNameResource { get; private set; }

        /// <summary>
        /// Icon for the menu.
        /// </summary>
        public Bitmap Icon { get; private set; }

        /// <summary>
        /// Controls whether the filter should be available in non-experimental releases.
        /// </summary>
        public bool Experimental { get; private set; }

        public VideoFilterInfo(string name, string friendlyNameResource, Bitmap icon, bool experimental)
        {
            Name = name;
            FriendlyNameResource = friendlyNameResource;
            Icon = icon;
            Experimental = experimental;
        }
    }
}
