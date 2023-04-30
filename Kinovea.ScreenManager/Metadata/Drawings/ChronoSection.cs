using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class ChronoSection
    {
        /// <summary>
        /// The start and end timestamps of the section.
        /// </summary>
        public VideoSection Section { get; set; }

        /// <summary>
        /// Name of the section.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A tag associated with the section.
        /// This can be used to attach a small piece of information to the section. 
        /// </summary>
        public string Tag;

        // The following properties are only used by the FormTimeSections dialog to 
        // display the sections in the list view.
        public string Start { get; set; }
        public string End { get; set; }
        public string Duration { get; set; }
        public string Cumul { get; set; }
        public bool IsCurrent { get; set; }

        public ChronoSection(VideoSection section, string name, string tag)
        {
            this.Section = section;
            this.Name = name;
            this.Tag = tag;
        }
    }
}
