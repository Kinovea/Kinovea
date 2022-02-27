using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    public class MeasuredDataCoordinate
    {
        public PointF Point { get; set; }

        public float Time { get; set; }

        public MeasuredDataCoordinate(PointF p, float t)
        {
            this.Point = p;
            this.Time = t;
        }
    }
}
