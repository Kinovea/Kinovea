using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    /// <summary>
    /// Groups the media types by format and frame size.
    /// At the bottom of the hierarchy are pairs of media type + framerate.
    /// This abstraction is used to model the two different ways that framerates are presented in filters,
    /// the correct way: a list of media types on the capture pin and a list of associated framerates on the filter,
    /// the other way: a massive list of media type, one for each framerate.
    /// </summary>
    public class MediaTypeOrganizer
    {
        public Dictionary<string, SizeGroup> FormatGroups
        {
            get { return formatGroups; }
        }
        
        private Dictionary<string, SizeGroup> formatGroups = new Dictionary<string, SizeGroup>();

        /// <summary>
        /// Organize all media types into a nice hierarchy.
        /// </summary>
        public void Organize(Dictionary<int, MediaType> mediaTypes, Dictionary<int, List<float>> framerates)
        {
            foreach (var pair in mediaTypes)
            {
                if (!framerates.ContainsKey(pair.Key))
                    continue;

                foreach (float rate in framerates[pair.Key])
                {
                    MediaType mt = pair.Value;
                    MediaTypeSelection selectable = new MediaTypeSelection(mt, rate);

                    Import(mt, rate, selectable);
                }
            }
        }

        /// <summary>
        /// Add the selectable to the hierarchy, creating parents as needed.
        /// </summary>
        private void Import(MediaType mt, float rate, MediaTypeSelection selectable)
        {
            if (rate <= 0)
                return;

            string formatKey = mt.Compression;
            if (!formatGroups.ContainsKey(formatKey))
                formatGroups.Add(formatKey, new SizeGroup(mt.Compression));

            SizeGroup sizeGroup = formatGroups[mt.Compression];
            string sizeKey = string.Format("{0}×{1}", mt.FrameSize.Width, mt.FrameSize.Height);

            if (!sizeGroup.FramerateGroups.ContainsKey(sizeKey))
                sizeGroup.FramerateGroups.Add(sizeKey, new FramerateGroup(mt.FrameSize));

            FramerateGroup framerateGroup = sizeGroup.FramerateGroups[sizeKey];
            if (framerateGroup.Framerates.ContainsKey(rate))
            {
                // Duplicate {format, size, framerate} triplet.
                return;
            }

            framerateGroup.Framerates.Add(rate, selectable);
        }
    }
}
