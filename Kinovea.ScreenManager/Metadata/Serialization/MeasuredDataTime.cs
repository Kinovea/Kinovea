using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class MeasuredDataTime
    {
        public string Name { get; set; }
        public float Duration { get; set; }

        public float Cumul { get; set; }
        public float Start { get; set; }
        public float Stop { get; set; }

        /// <summary>
        /// Whether the source of time is a simple clock.
        /// </summary>
        public bool IsClock { get; set; }

        /// <summary>
        /// Whether the source of time is a multi-chronometer.
        /// </summary>
        public bool IsMulti { get; set; }
    }
}
