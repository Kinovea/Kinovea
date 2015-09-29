using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class TickMark
    {
        public float Value { get; private set; }
        public PointF ImageLocation { get; private set; }
        public TextAlignment TextAlignment { get; private set; }

        public TickMark(float value, PointF imageLocation, TextAlignment textAlignment)
        {
            this.Value = value;
            this.ImageLocation = imageLocation;
            this.TextAlignment = textAlignment;
        }
    }
}
