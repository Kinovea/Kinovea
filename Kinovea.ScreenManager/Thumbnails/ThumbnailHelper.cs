using System;
using System.Drawing;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class ThumbnailHelper
    {
        /// <summary>
        /// Mapping from size category to the actual size in pixels.
        /// This is the full control size, the image should be ratio-stretched inside.
        /// </summary>
        public static Size GetThumbnailControlSize(ExplorerThumbSize sizeType)
        {
            switch (sizeType)
            {
                case ExplorerThumbSize.ExtraSmall:
                    return new Size(96, 80);
                case ExplorerThumbSize.Small:
                    return new Size(192, 160);
                case ExplorerThumbSize.Medium:
                    return new Size(240, 200);
                case ExplorerThumbSize.Large:
                    return new Size(288, 240);
                case ExplorerThumbSize.ExtraLarge:
                    return new Size(432, 360);
                default:
                    return new Size(240, 200);
            }
        }
    }
}
