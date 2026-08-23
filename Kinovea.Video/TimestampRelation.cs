using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinovea.Video
{
    /// <summary>
    /// Relates a request timestamp with a reference timestamp.
    /// Fuzzy matching.
    /// </summary>
    public enum TimestampRelation
    {
        Unknown,
        Behind,
        Match,
        Ahead,
        FarAhead,
    }
}
