using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Maps context variables with the corresponding pattern symbol.
    /// </summary>
    public static class PatternSymbolsCommand
    {
        public static Dictionary<PatternContext, string> Symbols;

        static PatternSymbolsCommand()
        {
            Symbols = new Dictionary<PatternContext, string>
            {
                { PatternContext.CaptureDirectory, "%directory" },
                { PatternContext.CaptureFilename, "%filename" },
                { PatternContext.CaptureKVA, "%kva" }
            };

        }

    }
}
