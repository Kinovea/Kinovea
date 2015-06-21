using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A subframe with a constant age.
    /// </summary>
    public class DelaySubframeConstant : IDelaySubframe
    {
        /// <summary>
        /// The location and size of the subframe into the composite.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// The constant age of the subframe.
        /// </summary>
        public int Age { get; private set; }

        public DelaySubframeConstant(Rectangle bounds, int age)
        {
            this.Bounds = bounds;
            this.Age = age;
        }
    }
}
