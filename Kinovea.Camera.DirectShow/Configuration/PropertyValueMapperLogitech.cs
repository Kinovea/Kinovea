using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class PropertyValueMapperLogitech : PropertyValueMapperDefault
    {
        public override Func<int, string> GetMapper(string property)
        {
            if (property == "exposure_logitech")
                return MapExposure;
            else
                return base.GetMapper(property);
        }

        private string MapExposure(int value)
        {
            if (value < 10)
                return string.Format("{0} µs", value * 100);
            else
                return string.Format("{0:0.#} ms", value / 10F);
        }
    }
}
