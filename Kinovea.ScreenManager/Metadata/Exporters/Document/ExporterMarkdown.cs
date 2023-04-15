using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class ExporterMarkdown
    {
        public static readonly string Tab = new string(' ', 4);
        public static readonly string LineBreak = new string(' ', 2) + "\r\n";
        public static readonly string ParagraphBreak = "\r\n\r\n";
        public static readonly string ItalicWrap = "*";
        public static readonly string BoldWrap = "**";
        public static readonly string CodeWrap = "`";
        public static readonly string StrikeThroughWrap = "~~";
        public static readonly string Heading1Prefix = "# ";
        public static readonly string Heading2Prefix = "## ";
        public static readonly string Heading3Prefix = "### ";
        public static readonly string Heading4Prefix = "#### ";
        public static readonly string Heading5Prefix = "##### ";
        public static readonly string Heading6Prefix = "###### ";
        public static readonly string QuotePrefix = "> ";
        public static readonly string UnorderedListItemPrefix = "- ";
        public static readonly int DefaultListItemNumber = 1;
        public static readonly string OrderedListItemPrefix = DefaultListItemNumber + ". ";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(string path, List<Bitmap> images, Metadata metadata)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var keyframe in metadata.Keyframes)
            {
                // Insert image.
                sb.Append(Heading2Prefix + keyframe.Name + LineBreak);
                sb.Append(ItalicWrap + keyframe.TimeCode + ItalicWrap + ParagraphBreak);
                sb.Append(RichTextHelper.GetText(keyframe.Comments) + ParagraphBreak);
            }

            File.WriteAllText(path, sb.ToString());
        }

    }
}
