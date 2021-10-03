using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Static class that produces instances of Video filters and provide information about filter types.
    /// This is used to build the menu or get the XML tag for example.
    /// The filters are owned by the metadata object of each player screen.
    /// There can only be one active filter for each screen.
    /// Filters keep their data even when they are not active.
    /// </summary>
    public static class VideoFilterFactory
    {
        private static Dictionary<VideoFilterType, VideoFilterInfo> info = new Dictionary<VideoFilterType, VideoFilterInfo>();
        
        static VideoFilterFactory()
        {
            info.Add(VideoFilterType.Kinogram, new VideoFilterInfo("Kinogram", Properties.Resources.mosaic, false));
        }

        /// <summary>
        /// Create a new filter of the specified type.
        /// There should be one filter of each type per screen.
        /// </summary>
        public static IVideoFilter CreateFilter(VideoFilterType type, Metadata metadata)
        {
            switch (type)
            {
                case VideoFilterType.Kinogram:
                    return new VideoFilterKinogram(metadata);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve the internal name for this filter type.
        /// </summary>
        public static string GetName(VideoFilterType type)
        {
            if (type == VideoFilterType.None)
                return "";

            return info[type].Name;
        }

        /// <summary>
        /// Retrieve the user-facing name for this filter type.
        /// </summary>
        public static string GetFriendlyName(VideoFilterType type)
        {
            // TODO: localization.
            return info[type].Name;
        }

        public static Bitmap GetIcon(VideoFilterType type)
        {
            return info[type].Icon;
        }

        public static bool GetExperimental(VideoFilterType type)
        {
            return info[type].Experimental;
        }

        /// <summary>
        /// Retrieve the filter type from the name.
        /// </summary>
        public static VideoFilterType GetFilterType(string name)
        {
            foreach (var pair in info)
            {
                if (pair.Value.Name == name)
                    return pair.Key;
            }

            return VideoFilterType.None;
        }
    }
}
