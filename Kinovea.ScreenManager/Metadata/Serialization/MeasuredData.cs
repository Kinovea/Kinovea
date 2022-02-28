using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Intermediate representation of all the data we export for scientific purposes.
    /// The data here has already been converted to the user coordinate system and time system.
    /// This is different from the KVA file format which contains the data in internal units like pixels and timestamps.
    /// 
    /// All times are converted to a numerical representation rather than timecodes.
    /// Tracks and Trackable objects are merged into one timeseries list.
    /// </summary>
    public class MeasuredData
    {
        public string Producer { get; set; }
        public string FullPath { get; set; }
        public string OriginalFilename { get; set; }
        public Size ImageSize { get; set; }
        public float CaptureFramerate { get; set; }
        public float UserFramerate { get; set; }
        public MeasuredDataUnits Units { get; set; }

        public List<MeasuredDataKeyframe> Keyframes { get; set; } = new List<MeasuredDataKeyframe>();
        public List<MeasuredDataPosition> Positions { get; set; } = new List<MeasuredDataPosition>();
        public List<MeasuredDataDistance> Distances { get; set; } = new List<MeasuredDataDistance>();
        public List<MeasuredDataAngle> Angles { get; set; } = new List<MeasuredDataAngle>();
        public List<MeasuredDataTime> Times { get; set; } = new List<MeasuredDataTime>();
        public List<MeasuredDataTimeseries> Timeseries { get; set; } = new List<MeasuredDataTimeseries>();

    }
}
