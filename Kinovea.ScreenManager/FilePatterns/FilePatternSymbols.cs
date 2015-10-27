using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Maps context variables with the corresponding pattern symbol.
    /// </summary>
    public static class FilePatternSymbols
    {
        public static Dictionary<FilePatternContexts, string> Symbols;

        static FilePatternSymbols()
        {
            Symbols = new Dictionary<FilePatternContexts, string>
            {
                { FilePatternContexts.Year, "%year" },
                { FilePatternContexts.Month, "%month" },
                { FilePatternContexts.Day, "%day" },
                { FilePatternContexts.Hour, "%hour" },
                { FilePatternContexts.Minute, "%minute" },
                { FilePatternContexts.Second, "%second" },

                { FilePatternContexts.CameraAlias, "%camalias" },
                { FilePatternContexts.ConfiguredFramerate, "%camfps" },
                { FilePatternContexts.ReceivedFramerate, "%recvfps" }
            };

        }

    }
}
