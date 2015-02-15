using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.FrameGenerator
{
    public class Framerate
    {
        public int FrameInterval { get; private set; }

        public Framerate(int frameInterval)
        {
            this.FrameInterval = frameInterval;
        }

        public override string ToString()
        {
            return string.Format("{0:0.00} fps", 1000000.0 / FrameInterval);
        }
    }
}
