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
        /// Location and size of the rectangle to extract from the frame.
        /// </summary>
        public Rectangle Source { get; set; }

        /// <summary>
        /// The location and size of destination rectangle in the composite.
        /// </summary>
        public Rectangle Destination { get; set; }

        /// <summary>
        /// The constant age of the subframe.
        /// </summary>
        public int Age { get; private set; }

        public DelaySubframeConstant(Rectangle source, Rectangle destination, int age)
        {
            this.Source = source;
            this.Destination = destination;
            this.Age = age;
        }
    }
}
