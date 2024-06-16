using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Small class used to wrap the core data of marker drawings in 3D,
    /// in the context of calibration validation.
    /// </summary>
    public class NamedPoint
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public NamedPoint(string name, float x, float y, float z)
        {
            this.Name = name;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
