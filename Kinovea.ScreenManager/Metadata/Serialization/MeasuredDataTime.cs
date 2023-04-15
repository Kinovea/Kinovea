using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Collects the time information for a chronometer, with possibly multiple time sections.
    /// </summary>
    public class MeasuredDataTime
    {
        /// <summary>
        /// The name of the chronometer object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether any section has a non-empty tag.
        /// </summary>
        public bool HasTags { get; set; } = false;

        /// <summary>
        /// The list of time sections.
        /// </summary>
        public List<MeasuredDataTimeSection> Sections { get; set; } = new List<MeasuredDataTimeSection>();
    }
}
