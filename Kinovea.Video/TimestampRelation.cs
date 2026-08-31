using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Relates a request timestamp with a reference timestamp.
    /// </summary>
    public enum TimestampRelation
    {
        Unknown,
        Match,
        FarBehind,
        Behind,
        Ahead,
        FarAhead,
    }
}
