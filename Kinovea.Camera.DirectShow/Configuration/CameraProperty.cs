using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class CameraProperty
    {
        public bool Supported { get; set; }
        public bool Automatic { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public int Value { get; set; }
        public CameraPropertyType Type { get; set; }
    }
}
