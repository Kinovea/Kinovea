using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Static class that produces instances of Video filters.
    /// The filters are owned by Player screens.
    /// </summary>
    public static class VideoFilterFactory
    {
        public static Dictionary<VideoFilterType, VideoFilterInfo> Info 
        {
            get; private set;
        }

        static VideoFilterFactory()
        {
            Info = new Dictionary<VideoFilterType, VideoFilterInfo>();
            Info.Add(VideoFilterType.Kinogram, new VideoFilterInfo("Kinogram", Properties.Resources.mosaic, false));
        }

        public static IVideoFilter CreateFilter(VideoFilterType type)
        {
            switch (type)
            {
                case VideoFilterType.Kinogram:
                    return new VideoFilterKinogram();
                default:
                    return null;
            }
        }
    }
}
