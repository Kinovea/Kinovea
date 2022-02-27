using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This represent data of one trajectory object.
    /// </summary>
    public class MeasuredDataTrack
    {
        public string Name { get; set; }

        public float Start { get; set; }

        public List<MeasuredDataCoordinate> Coords { get; set; }
    }
}
