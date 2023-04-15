using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Kinovea.Services
{
    public static class TextHelper
    {
        private static RichTextBox rtb = new RichTextBox();
        private static Control dummyControl = new Control();
        private static Graphics dummyGraphics;

        static TextHelper()
        {
            dummyGraphics = dummyControl.CreateGraphics();
        }

        /// <summary>
        /// Extract plain text from rich text.
        /// </summary>
        public static string GetText(string richText)
        {
            rtb.Rtf = richText;
            return rtb.Text;
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
