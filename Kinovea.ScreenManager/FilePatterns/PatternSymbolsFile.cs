using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Maps context variables with the corresponding pattern symbol.
    /// </summary>
    public static class PatternSymbolsFile
    {
        public static Dictionary<PatternContext, string> Symbols;

        static PatternSymbolsFile()
        {
            Symbols = new Dictionary<PatternContext, string>
            {
                { PatternContext.Year, "%year" },
                { PatternContext.Month, "%month" },
                { PatternContext.Day, "%day" },
                { PatternContext.Hour, "%hour" },
                { PatternContext.Minute, "%minute" },
                { PatternContext.Second, "%second" },

                { PatternContext.CameraAlias, "%camalias" },
                { PatternContext.ConfiguredFramerate, "%camfps" },
                { PatternContext.ReceivedFramerate, "%recvfps" },
                { PatternContext.Escape, "%%" }
            };

        }

    }
}
