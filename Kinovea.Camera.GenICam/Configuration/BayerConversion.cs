using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Camera.GenICam
{
    /// <summary>
    /// Hardware conversion of Bayer streams.
    /// </summary>
    public enum BayerConversion
    {
        Raw,
        Mono,
        Color
    }
}
