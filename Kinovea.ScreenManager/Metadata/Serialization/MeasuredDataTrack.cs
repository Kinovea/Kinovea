using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class MeasuredDataTrack
    {
        public string Name { get; set; }

        public float Start { get; set; }

        public List<MeasuredDataCoordinate> Coords { get; set; }
    }
}
