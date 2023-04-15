using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class RichTextHelper
    {
        private static RichTextBox rtb = new RichTextBox();

        /// <summary>
        /// Extract plain text from rich text.
        /// </summary>
        public static string GetText(string richText)
        {
            rtb.Rtf = richText;
            return rtb.Text;
        }
    }
}
