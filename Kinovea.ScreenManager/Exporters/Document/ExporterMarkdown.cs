using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Kinovea.Services;
using System.Windows.Forms;

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

        /// <summary>
        /// Export to a markdown document.
        /// </summary>
        public void Export(string path, List<string> filePaths, Metadata metadata)
        {
            StringBuilder sb = new StringBuilder();

            // We are currently in the background thread, we need that control used for conversion to
            // also be on the background thread.
            RichTextBox rtb = new RichTextBox();

            sb.Append(Heading1Prefix + Path.GetFileNameWithoutExtension(metadata.VideoPath) + LineBreak);
            sb.Append(ParagraphBreak);
            int current = 0;
            for (int i = 0; i < metadata.Keyframes.Count; i++)
            {
                var keyframe = metadata.Keyframes[i];
                
                // Only include the keyframes that are within the selection to stay
                // consistent with the images we just exported and with the way video/image export works.
                if (keyframe.Timestamp < metadata.SelectionStart || keyframe.Timestamp > metadata.SelectionEnd)
                    continue;

                sb.Append(Heading2Prefix + keyframe.Name + LineBreak);
                sb.Append(ItalicWrap + keyframe.TimeCode + ItalicWrap + ParagraphBreak);
                sb.Append(string.Format("![]({0}){{width=100%}}", filePaths[current]) + ParagraphBreak);

                // Extract comments from rich text to simple text.
                rtb.Rtf = keyframe.Comments;
                string text = rtb.Text;

                // Enforce Markdown linebreaks.
                text = text.Replace("\n", LineBreak);
                sb.Append(text + ParagraphBreak);
                current++;
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
