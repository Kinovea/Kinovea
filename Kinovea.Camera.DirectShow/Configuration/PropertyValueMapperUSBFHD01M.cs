using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class PropertyValueMapperUSBFHD01M : PropertyValueMapperDefault
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
            // These values were found using the following methods:
            // - Values -1 to -7: framerate automatic downgrade.
            // - Values -8 to -11: visual comparison with C920.
            // - Values -12 to -13: following the exponential trend.
            // They might be inaccurate for values less than -7, and even more innacurate for values under -11 that the C920 cannot match.
            // We could solve for the exponential but it's not clear how the values are generated in the first place.
            int microseconds = 0;
            
            switch (value)
            {
                case -1:
                    microseconds = 640000;
                    break;
                case -2:
                    // Limited to 3.12 fps.
                    microseconds = 320000;
                    break;
                case -3:
                    // Limited to 6.25 fps.
                    microseconds = 160000;
                    break;
                case -4:
                    // Limited to 12.5 fps.
                    microseconds = 80000;
                    break;
                case -5:
                    // Limited to 25 fps.
                    microseconds = 40000;
                    break;
                case -6:
                    // Limited to 50 fps.
                    microseconds = 20000;
                    break;
                case -7:
                    // Limited to 100 fps.
                    microseconds = 10000;
                    break;
                case -8:
                    microseconds = 5000;
                    break;
                case -9:
                    microseconds = 2500;
                    break;
                case -10:
                    microseconds = 1250;
                    break;

                // For the following values we limit the number of significant digits as we don't really have that level of precision.
                case -11:
                    // 625
                    microseconds = 600;
                    break;
                case -12:
                    // 312.5
                    microseconds = 300;
                    break;
                case -13:
                    // 156
                    microseconds = 150;
                    break;
            }

            if (microseconds == 0)
                return value.ToString();
            if (microseconds < 1000)
                return string.Format("{0} µs", microseconds);
            else
                return string.Format("{0:0.#} ms", microseconds / 1000F);
        }
    }
}
