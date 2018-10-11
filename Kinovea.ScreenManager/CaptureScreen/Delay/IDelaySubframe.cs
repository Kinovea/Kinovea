using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public interface IDelaySubframe
    {
        /// <summary>
        /// Location and size of the rectangle to extract from the frame.
        /// </summary>
        Rectangle Source { get; set; }

        /// <summary>
        /// The location and size of destination rectangle in the composite.
        /// </summary>
        Rectangle Destination { get; set; }
    }
}

