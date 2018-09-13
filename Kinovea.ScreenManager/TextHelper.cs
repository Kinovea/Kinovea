using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Kinovea.ScreenManager
{
    public static class TextHelper
    {
        private static Graphics dummyGraphics;
        private static Control dummyControl;

        static TextHelper()
        {
            dummyControl = new Control();
            dummyGraphics = dummyControl.CreateGraphics();
        }

        public static SizeF MeasureString(string text, Font font)
        {
            return dummyGraphics.MeasureString(text, font);
        }

        public static string FixMissingCarriageReturns(string text)
        {
            return Regex.Replace(text, "(?<!\r)\n", "\r\n");
        }
    }
}
