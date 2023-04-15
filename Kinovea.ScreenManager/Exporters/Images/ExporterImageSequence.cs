using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

using Kinovea.Video;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exporter for image sequences (normal sequence and key images).
    /// </summary>
    public class ExporterImageSequence
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen player;

        public ExporterImageSequence()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            formProgressBar.CancelAsked += FormProgressBar_CancelAsked;
        }

        public void Export(SavingSettings settings, PlayerScreen player)
        {
            // Setup global variables we'll use from inside the background thread.
            this.player = player;

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
            player.FrameServer.VideoReader.BeforeFrameEnumeration();
            IEnumerable<Bitmap> images = player.FrameServer.EnumerateImages(s);

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

                string filename = player.FrameServer.GetImageFilename(s.File, timestamp, PreferencesManager.PlayerPreferences.TimecodeFormat);
                string filePath = Path.Combine(dir, filename + extension);

                image.Save(filePath);

                i++;
                worker.ReportProgress(i, s.EstimatedTotal);
            }

            player.FrameServer.VideoReader.AfterFrameEnumeration();
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

            player.FrameServer.AfterSave();
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
