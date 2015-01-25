using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.Camera.Basler
{
    public class StreamFormat
    {
        public string Symbol { get; private set; }
        public string DisplayName { get; private set; }

        public StreamFormat(string symbol, string displayName)
        {
            this.Symbol = symbol;
            this.DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
