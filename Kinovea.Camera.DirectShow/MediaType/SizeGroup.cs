using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.DirectShow
{
    public class SizeGroup
    {
        public Dictionary<string, FramerateGroup> FramerateGroups { get; private set; }
        public string Format { get; private set; }

        public SizeGroup(string format)
        {
            this.FramerateGroups = new Dictionary<string, FramerateGroup>();
            this.Format = format;
        }

        public override string ToString()
        {
            return Format;
        }
    }
}
