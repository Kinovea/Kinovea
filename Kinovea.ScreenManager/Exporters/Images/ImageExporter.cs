using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Kinovea.Video;
using Kinovea.Services;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.Drawing;
using System.IO;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This is the router to specialized exporters for the different image export formats.
    /// The "formats" at this level are the different types of image export we support (single, sequence, etc.),
    /// not the final file format. All exporters should be able to export to all the supported file formats,
    /// which will be chosen in the save file dialog.
    /// </summary>
    public class ImageExporter
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen player1;
        private PlayerScreen player2;
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ImageExporter()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            formProgressBar.CancelAsked += FormProgressBar_CancelAsked;
        }

        // Get a filename from the user and export an image or image sequence.
        public void Export(ImageExportFormat format, PlayerScreen player1, PlayerScreen player2)
        {
            if (player1 == null)
                return;

            // Special case for video filter with custom save mechanics.
            if (format == ImageExportFormat.Image && 
                player1.ActiveVideoFilterType != VideoFilterType.None &&
                player1.FrameServer.Metadata.ActiveVideoFilter.CanExportImage)
            {
                player1.FrameServer.Metadata.ActiveVideoFilter.ExportImage(player1.view);
                return;
            }

            // Immediately get a file name to save to.
            // Any configuration of the save happens later.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = ScreenManagerLang.Generic_SaveImage;
            sfd.RestoreDirectory = true;
            sfd.Filter = FilesystemHelper.SaveImageFilter();
            sfd.FilterIndex = FilesystemHelper.GetFilterIndex(sfd.Filter, PreferencesManager.PlayerPreferences.ImageFormat);
            sfd.FileName = SuggestFilename(format, player1, player2);
            
            if (sfd.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(sfd.FileName))
                return;

            // Save this as the new preferred image format.
            PreferencesManager.PlayerPreferences.ImageFormat = FilesystemHelper.GetImageFormat(sfd.FileName);
            PreferencesManager.Save();
            
            SavingSettings s = new SavingSettings();

            try
            {
                switch (format)
                {
                    case ImageExportFormat.Image:
                        ExporterImage exporterImage = new ExporterImage();
                        exporterImage.Export(sfd.FileName, player1);
                        player1.FrameServer.AfterSave();
                        break;
                    case ImageExportFormat.SideBySide:

                        // Show a configuration dialog to get the layout.
                        FormConfigureExportImageSideBySide fceisbs = new FormConfigureExportImageSideBySide();
                        fceisbs.StartPosition = FormStartPosition.CenterScreen;
                        if (fceisbs.ShowDialog() != DialogResult.OK)
                        {
                            fceisbs.Dispose();
                            return;
                        }

                        bool horizontal = fceisbs.Horizontal;

                        // Save this as the new preferred layout.
                        PreferencesManager.PlayerPreferences.SideBySideHorizontal = horizontal;
                        PreferencesManager.Save();

                        // Export
                        ExporterImageSideBySide exporterSidebySide = new ExporterImageSideBySide();
                        exporterSidebySide.Export(sfd.FileName, horizontal, player1, player2);
                        
                        player1.FrameServer.AfterSave();
                        player2.FrameServer.AfterSave();
                        break;

                    case ImageExportFormat.ImageSequence:
                        
                        // Show a configuration dialog to get the interval.
                        FormConfigureExportImageSequence fceis = new FormConfigureExportImageSequence(player1);
                        fceis.StartPosition = FormStartPosition.CenterScreen;
                        if (fceis.ShowDialog() != DialogResult.OK)
                        {
                            fceis.Dispose();
                            return;
                        }

                        s.Section = new VideoSection(player1.FrameServer.Metadata.SelectionStart, player1.FrameServer.Metadata.SelectionEnd);
                        s.KeyframesOnly = false;
                        s.File = sfd.FileName;
                        s.ImageRetriever = player1.view.GetFlushedImage;
                        s.OutputIntervalTimestamps = fceis.IntervalTimestamps;
                        s.EstimatedTotal = fceis.RemainingFrames;

                        fceis.Dispose();

                        ExportSequence(s, player1);
                        break;
                    case ImageExportFormat.KeyImages:
                        
                        // No dialog needed.
                        
                        s.Section = new VideoSection(player1.FrameServer.Metadata.SelectionStart, player1.FrameServer.Metadata.SelectionEnd);
                        s.KeyframesOnly = true;
                        s.File = sfd.FileName;
                        s.ImageRetriever = player1.view.GetFlushedImage;
                        s.EstimatedTotal = player1.FrameServer.Metadata.Keyframes.Count;

                        ExportSequence(s, player1);
                        break;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception encountered while exporting images.", e);
            }
        }

        /// <summary>
        /// Returns a suggested filename (without directory nor extension), to be used in the save file dialog.
        /// </summary>
        private string SuggestFilename(ImageExportFormat format, PlayerScreen player1, PlayerScreen player2)
        {
            string filename = "";
            string videoFilePath = player1.FrameServer.VideoReader.FilePath;
            switch (format)
            {
                case ImageExportFormat.Image:
                    // Video name + time code.
                    filename = player1.FrameServer.GetImageFilename(videoFilePath, player1.view.CurrentTimestamp, PreferencesManager.PlayerPreferences.TimecodeFormat);
                    break;
                case ImageExportFormat.ImageSequence:
                case ImageExportFormat.KeyImages:
                    // Video name alone.
                    filename = Path.GetFileNameWithoutExtension(videoFilePath);
                    break;
                case ImageExportFormat.SideBySide:
                    // Double video name.
                    filename = ExporterImageSideBySide.SuggestFilename(player1, player2);
                    break;
            }

            return filename;
        }

        private void ExportSequence(SavingSettings settings, PlayerScreen player1)
        {
            // Setup global variables we'll use from inside the background thread.
            this.player1 = player1;
            
            // Start the background worker.
            formProgressBar.Reset();
            worker.RunWorkerAsync(settings);

            // Show the progress bar.
            // This is the end of this function and the UI thread is now in the progress bar.
            // Anything else should be done from the background thread,
            // until we come back in `Worker_RunWorkerCompleted`.
            formProgressBar.ShowDialog();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "ImageExporter";
            BackgroundWorker worker = sender as BackgroundWorker;
            SavingSettings s = e.Argument as SavingSettings;

            // Get the image enumerator.
            player1.FrameServer.VideoReader.BeforeFrameEnumeration();
            IEnumerable<Bitmap> images = player1.FrameServer.EnumerateImages(s, s.OutputIntervalTimestamps);

            // Enumerate and save the images.
            string dir = Path.GetDirectoryName(s.File);
            string extension = Path.GetExtension(s.File);
            int i = 0;
            foreach (var image in images)
            {
                if (worker.CancellationPending)
                    break;

                // The timestamp should be stored in the Bitmap.Tag.
                long timestamp = 0;
                if (image.Tag is long)
                    timestamp = (long)image.Tag;

                string filename = player1.FrameServer.GetImageFilename(s.File, timestamp, PreferencesManager.PlayerPreferences.TimecodeFormat);
                string filePath = Path.Combine(dir, filename + extension);

                image.Save(filePath);

                i++;
                worker.ReportProgress(i, s.EstimatedTotal);
            }

            player1.FrameServer.VideoReader.AfterFrameEnumeration();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // This runs in the UI thread.
            // This method is called from the background thread for each processed frame.
            int value = e.ProgressPercentage;
            int max = (int)e.UserState;
            formProgressBar.Update(value, max, false);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // We are back in the UI thread after the work is complete or cancelled.
            formProgressBar.Close();
            formProgressBar.Dispose();

            player1.FrameServer.AfterSave();
            // Return to the start of the zone.
            //m_iFramesToDecode = 1;
            //ShowNextFrame(m_iSelStart, true);
            //ActivateKeyframe(m_iCurrentPosition, true);
        }

        private void FormProgressBar_CancelAsked(object sender, EventArgs e)
        {
            // Turn the CancellationPending flag on.
            worker.CancelAsync();
        }
    }
}
