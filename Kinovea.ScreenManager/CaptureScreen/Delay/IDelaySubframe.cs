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
        /// The location and size of the subframe into the composite.
        /// </summary>
        Rectangle Bounds { get; }
    }
}

