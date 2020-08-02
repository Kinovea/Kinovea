using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// When using Calibration by line, this defines whether the line represents the X, Y or neither axes.
    /// </summary>
    public enum CalibrationAxis
    {
        /// <summary>
        /// The calibration line is horizontal in the real world.
        /// </summary>
        LineHorizontal,

        /// <summary>
        /// The calibration line is vertical in the real world.
        /// </summary>
        LineVertical, 

        /// <summary>
        /// The coordinate system should be aligned with the image axes.
        /// </summary>
        ImageAxes,
    }
}
