using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.IDS
{
    public class IDSEnum
    {
        public int Value { get; private set; }
        public string DisplayName { get; private set; }

        public IDSEnum(int value, string displayName)
        {
            this.Value = value;
            this.DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}