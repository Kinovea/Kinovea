using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Collects the time data for a single time section.
    /// The basic chronometer and clock have a single time section.
    /// The multi-chronometer has a list of these.
    /// </summary>
    public class MeasuredDataTimeSection
    {
        public string Name { get; set; }
        public float Duration { get; set; }
        public float Cumul { get; set; }
        public float Start { get; set; }
        public float Stop { get; set; }
    }
}
