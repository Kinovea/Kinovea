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
using System.ComponentModel;
using System.Threading;

namespace Kinovea.ScreenManager
{
    public class DocumentExporter
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen player;
        private Metadata metadata;
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentExporter()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            formProgressBar.CancelAsked += FormProgressBar_CancelAsked;
        }

        /// <summary>
        /// Get a filename from the user and export a document out of the video and metadata.
        /// </summary>
        public void Export(DocumentExportFormat format, PlayerScreen player)
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
        private void Export(string file, DocumentExportFormat format, Metadata metadata, PlayerScreen player)
        {
            // Always export to Markdown first.
            // For other formats we delegate the conversion to Pandoc.
            SavingSettings s = new SavingSettings();
            s.Section = new VideoSection(metadata.SelectionStart, metadata.SelectionEnd);
            s.KeyframesOnly = true;

            s.File = file;
            s.ImageRetriever = player.view.GetFlushedImage;
            s.EstimatedTotal = metadata.Keyframes.Count;

            // Setup global variables we'll use from inside the background thread.
            this.player = player;
            this.metadata = metadata;

            // Start the background worker.
            formProgressBar.Reset();
            worker.RunWorkerAsync(s);

            // Finally, show the progress bar.
            // This is the end of this function and the UI thread is now in the progress bar.
            // Anything else should run from the background thread.
            formProgressBar.ShowDialog();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "DocumentExporter";
            BackgroundWorker worker = sender as BackgroundWorker;
            SavingSettings s = e.Argument as SavingSettings;

            // Get the key image enumerator.
            player.FrameServer.VideoReader.BeforeFrameEnumeration();
            IEnumerable<Bitmap> images = player.FrameServer.EnumerateImages(s);

            string assetsDir = "images";
            string assetsPath = Path.Combine(Path.GetDirectoryName(s.File), assetsDir);
            if (!Directory.Exists(assetsPath))
                Directory.CreateDirectory(assetsPath);

            // Enumerate and save the images. Collect the relative filenames for later.
            List<string> filePathsRelative = new List<string>();
            int magnitude = (int)Math.Ceiling(Math.Log10(metadata.Keyframes.Count));
            int i = 0;
            foreach (var image in images)
            {
                if (worker.CancellationPending)
                {
                    break;
                }

                string filename = string.Format("{0}.png", i.ToString("D" + magnitude));
                filePathsRelative.Add(Path.Combine(assetsDir, filename));
                
                string filePath = Path.Combine(assetsPath, filename);
                image.Save(filePath);

                i++;
                worker.ReportProgress(i, s.EstimatedTotal);
            }

            player.FrameServer.VideoReader.AfterFrameEnumeration();

            if (worker.CancellationPending)
            {
                // We got cancelled, nothing more to do.
            }
            else
            {
                ExporterMarkdown exporterMarkdown = new ExporterMarkdown();
                exporterMarkdown.Export(s.File, filePathsRelative, metadata);

                // TODO: handle other formats.
                
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This runs in the UI thread.
            // This method is called from the background thread for each processed frame.
            int value = e.ProgressPercentage;
            int max = (int)e.UserState;
            formProgressBar.Update(value, max, true);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // We are back in the UI thread after the work is complete or cancelled.
            formProgressBar.Close();
            formProgressBar.Dispose();
        }

        private void FormProgressBar_CancelAsked(object sender, EventArgs e)
        {
            // Turn the CancellationPending flag on.
            worker.CancelAsync();
        }
    }
}
