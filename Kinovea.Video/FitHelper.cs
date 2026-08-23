using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    public static class FitHelper
    {
        /// <summary>
        /// Fit the payload to the container.
        /// </summary>
        public static Size Fit(Size source, Size bounds, bool allowUpscale)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return source;

            double scaleX = (double)bounds.Width / source.Width;
            double scaleY = (double)bounds.Height / source.Height;
            double scale = Math.Min(scaleX, scaleY);

            if (!allowUpscale)
                scale = Math.Min(scale, 1.0);

            int width = Math.Max(1, (int)Math.Round(source.Width * scale));
            int height = Math.Max(1, (int)Math.Round(source.Height * scale));

            return new Size(width, height);
        }
    }
}
