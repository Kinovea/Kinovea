using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This represents data of one trackable object such as an angle.
    /// </summary>
    public class MeasuredDataTimeline
    {
        /// <summary>
        /// Name of the object.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Time vector shared by all trackable points in the object.
        /// </summary>
        public List<float> Times { get; set; }

        /// <summary>
        /// Time series containing the actual data.
        /// Each entry in the dictionary is a trackable point on the object.
        /// The data here is always the 2D coordinate, not higher level Kinematics.
        /// </summary>
        public Dictionary<string, List<PointF>> Data{ get; set; }
    }
}
