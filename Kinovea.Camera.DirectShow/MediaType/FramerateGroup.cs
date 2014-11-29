using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Camera.DirectShow
{
    public class FramerateGroup
    {
        public Dictionary<float, MediaTypeSelection> Framerates { get; private set; }
        public Size Size { get; private set; }

        public FramerateGroup(Size size)
        {
            this.Framerates = new Dictionary<float, MediaTypeSelection>();
            this.Size = size;
        }

        public override string ToString()
        {
            return string.Format("{0}×{1}", Size.Width, Size.Height);
        }
    }
}
