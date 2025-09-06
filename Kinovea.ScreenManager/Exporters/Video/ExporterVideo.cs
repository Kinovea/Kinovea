using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kinovea.Video;
using Kinovea.Services;
using Kinovea.Video.FFMpeg;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exporter for single video and special videos (slideshow, pauses).
    /// This is basically a bridge between the VideoExporter broker which 
    /// sets up the save settings based on the type of export, and the actual video writer code 
    /// which uses the settings to configure the output file.
    /// </summary>
    public class ExporterVideo
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen player;
        private SaveResult saveResult;

        public ExporterVideo()
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
            Thread.CurrentThread.Name = "VideoExporter";
            BackgroundWorker worker = sender as BackgroundWorker;
            SavingSettings s = e.Argument as SavingSettings;

            player.view.BeforeExportVideo();

            // Get the image enumerator.
            player.FrameServer.VideoReader.BeforeFrameEnumeration();
            IEnumerable<Bitmap> images = player.FrameServer.EnumerateImages(s);

            // Export loop.
            VideoFileWriter w = new VideoFileWriter();
            string formatString = FilesystemHelper.GetFormatStringPlayback(s.File);
            saveResult = w.Save(s, player.FrameServer.VideoReader.Info, formatString, images, worker);
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

            player.FrameServer.VideoReader.AfterFrameEnumeration();
            player.view.AfterExportVideo();
            player.FrameServer.AfterSave();

            if (saveResult != SaveResult.Success)
                player.FrameServer.ReportError(saveResult);
        }

        private void FormProgressBar_CancelAsked(object sender, EventArgs e)
        {
            // Turn the CancellationPending flag on.
            worker.CancelAsync();
        }
    }
}
