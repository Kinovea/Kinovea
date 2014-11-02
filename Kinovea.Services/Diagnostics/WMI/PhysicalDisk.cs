using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Services
{
    public class PhysicalDisk
    {
        public string Model { get; private set; }
        public string Type { get; private set; }
        public string InterfaceType { get; private set; }
        public float Size { get; private set; }
    
        public PhysicalDisk(string model, string type, string interfaceType, float size)
        {
            this.Model = model;
            this.Type = type;
            this.InterfaceType = interfaceType;
            this.Size = size;
        }
    }
}
