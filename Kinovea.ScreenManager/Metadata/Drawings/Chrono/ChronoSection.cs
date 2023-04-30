using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This class represent one time section in the multi-time chronometer.
    /// </summary>
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

        //-------------------------------------------------------------------------
        // The following properties are only used to draw the object or display it
        // in a list view.
        // These properties may depend on the current timestamp.
        //-------------------------------------------------------------------------

        public bool IsCurrent { get; set; }

        /// <summary>
        /// The name of the section, but guaranteed to be non empty. 
        /// If there is no user-provided name it falls back to the index of the section.
        /// </summary>
        public string NameOrIndex { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Duration { get; set; }
        public string Cumul { get; set; }

        public ChronoSection(VideoSection section, string name, string tag)
        {
            this.Section = section;
            this.Name = name;
            this.Tag = tag;
        }
    }
}
