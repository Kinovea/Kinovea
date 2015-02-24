using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class PropertyValueMapperUSB8MP02G : PropertyValueMapperDefault
    {
        public override Func<int, string> GetMapper(string property)
        {
            if (property == "exposure")
                return MapExposure;
            else
                return base.GetMapper(property);
        }

        private string MapExposure(int value)
        {
            // This must be the one camera to honor the DirectShow spec: exposure time in seconds = 2^value.
            // These values were found using framerate limitation, for -1 to -5, then extrapolated on the exponential. 

            double seconds = Math.Pow(2, value);

            if (seconds < 0.001)
                return string.Format("{0} µs", (int)(seconds * 1000000));
            else
                return string.Format("{0:0.#} ms", seconds * 1000);
        }
    }
}
