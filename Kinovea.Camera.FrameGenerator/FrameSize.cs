using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.Camera.FrameGenerator
{
    public class FrameSize
    {
        public string Text { get; set; }
        public Size Value { get; set; }

        public FrameSize(string text, Size value)
        {
            this.Text = text;
            this.Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
