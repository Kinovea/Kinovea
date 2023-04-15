using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Kinovea.ScreenManager.Languages;
using Kinovea.Video;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public static class DocumentExporter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Export a document from the video and metadata.
        /// </summary>
        public static void Export(DocumentExportFormat format, PlayerScreen player)
        {
            if (player == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export document";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Filter = "LibreOffice writer (*.odt)|*.odt|Microsoft Word (*.docx)|*.docx|Markdown (*.md)|*.md";
            int filterIndex;
            switch (format)
            {
                case DocumentExportFormat.ODT:
                    filterIndex = 1;
                    break;
                case DocumentExportFormat.DOCX:
                    filterIndex = 2;
                    break;
                case DocumentExportFormat.Mardown:
                default:
                    filterIndex = 3;
                    break;
            }

            saveFileDialog.FilterIndex = filterIndex;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(player.FrameServer.Metadata.VideoPath);

            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName))
                return;

            try
            {
                Export(saveFileDialog.FileName, format, player.FrameServer.Metadata, player);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception encountered while exporting document.", e);
            }
        }

        /// <summary>
        /// Export a document from the video and metadata.
        /// </summary>
        private static void Export(string file, DocumentExportFormat format, Metadata metadata, PlayerScreen player)
        {
            // Always export to Markdown first.
            // For other formats we delegate the conversion to Pandoc.

            SavingSettings s = new SavingSettings();

            s.Section = new VideoSection(metadata.SelectionStart, metadata.SelectionEnd);
            s.KeyframesOnly = true;

            s.File = file;
            s.ImageRetriever = player.view.GetFlushedImage;
            s.EstimatedTotal = metadata.Keyframes.Count;

            // Get the key image enumerator.
            player.FrameServer.VideoReader.BeforeFrameEnumeration();
            IEnumerable<Bitmap> images = player.FrameServer.EnumerateImages(s);

            // TODO: start background thread with progress bar.
            // TODO: Check if enumeration got cancelled.
            // TODO: if the format is not markdown, save assets to a temporary directory.

            string assetsDir = "images";
            string assetsPath = Path.Combine(Path.GetDirectoryName(file), assetsDir);
            if (!Directory.Exists(assetsPath))
                Directory.CreateDirectory(assetsPath);

            // Enumerate and save the images. Collect the relative filenames for later.
            List<string> filePathsRelative = new List<string>();
            int magnitude = (int)Math.Ceiling(Math.Log10(metadata.Keyframes.Count));
            int i = 0;
            foreach (var image in images)
            {
                string filename = string.Format("{0}.png", i.ToString("D" + magnitude));
                filePathsRelative.Add(Path.Combine(assetsDir, filename));
                
                string filePath = Path.Combine(assetsPath, filename);
                image.Save(filePath);

                i++;
            }

            player.FrameServer.VideoReader.AfterFrameEnumeration();

            ExporterMarkdown exporterMarkdown = new ExporterMarkdown();
            exporterMarkdown.Export(file, filePathsRelative, metadata);

            // TODO: handle other formats.

        }
    }
}
