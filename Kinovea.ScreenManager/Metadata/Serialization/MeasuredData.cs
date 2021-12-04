using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public class MeasuredData
    {
        public string Producer { get; set; }
        public string FullPath { get; set; }
        public string OriginalFilename { get; set; }
        public Size ImageSize { get; set; }
        public float CaptureFramerate { get; set; }
        public float UserFramerate { get; set; }

        public List<MeasuredDataKeyframe> Keyframes { get; set; } = new List<MeasuredDataKeyframe>();
        public List<MeasuredDataPosition> Positions { get; set; } = new List<MeasuredDataPosition>();
        public List<MeasuredDataDistance> Distances { get; set; } = new List<MeasuredDataDistance>();
    }
}
