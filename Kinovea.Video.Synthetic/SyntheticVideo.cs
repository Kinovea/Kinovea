using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Video
{
    public class SyntheticVideo
    {
        public Size ImageSize { get; set; }
        public double FramePerSecond { get; set; }
        public int DurationFrames { get; set; }
        public Color BackgroundColor { get; set; }
        public bool FrameNumber { get; set; }

        public SyntheticVideo()
        {
            ImageSize = new Size(800, 600);
            FramePerSecond = 20;
            DurationFrames = 100;
            BackgroundColor = Color.White;
            FrameNumber = true;
        }
    }
}
