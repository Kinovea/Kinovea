using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Services
{
    /// <summary>
    /// This is an encoding speed to compression ratio.
    /// Slower = better compression = better quality for the same EncodingQuality setting.
    /// = How hard the encoder works to hit the quality target.
    /// </summary>
    public enum EncodingSpeed
    {
        Fast,
        Medium,
        Slow,
    }
}
