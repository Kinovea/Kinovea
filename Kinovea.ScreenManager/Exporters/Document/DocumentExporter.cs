using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kinovea.ScreenManager
{
    public static class DocumentExporter
    {
        public static void Export(string file, DocumentExportFormat format, List<Bitmap> images, Metadata metadata)
        {
            // Always export to Markdown first.
            // For other formats we delegate the conversion to Pandoc.

            string assetsPath = "";
            string filePath = "";

            ExporterMarkdown exporterMarkdown = new ExporterMarkdown();
            exporterMarkdown.Export(file, images, metadata);

        }
    }
}
