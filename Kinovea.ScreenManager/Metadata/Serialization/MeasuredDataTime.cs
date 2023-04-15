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
        public string Name { get; set; }

        public List<MeasuredDataTimeSection> Sections { get; set; } = new List<MeasuredDataTimeSection>();
    }
}
