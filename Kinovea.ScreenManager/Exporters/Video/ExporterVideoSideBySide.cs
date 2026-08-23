using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Drawing;
using Kinovea.Services;
using Kinovea.Video;
using Kinovea.Video.FFMpeg;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Exporter for side by side videos. Either horizontal or vertical layout.
    /// </summary>
    public class ExporterVideoSideBySide
    {
        #region Members
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen leftPlayer;
        private PlayerScreen rightPlayer;
        private DualPlayerController dualPlayer;
        private VideoExportResult exportResult;
        
        private CommonTimeline commonTimeline;
        private double fileFrameInterval;

        // Reusable staging bitmaps filled for each output frame.
        private Bitmap bmpComposite;
        private Bitmap bmpLeft;
        private Bitmap bmpRight;

        long currentTime = 0;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public ExporterVideoSideBySide()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            formProgressBar.CancelAsked += FormProgressBar_CancelAsked;
        }

        public void Export(VideoExportSettings settings, PlayerScreen leftPlayer, PlayerScreen rightPlayer, DualPlayerController dualPlayer)
        {
            this.leftPlayer = leftPlayer;
            this.rightPlayer = rightPlayer;
            this.dualPlayer = dualPlayer;

            this.commonTimeline = dualPlayer.CommonTimeline;

            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            leftPlayer.DualSaveInProgress = true;
            rightPlayer.DualSaveInProgress = true;
            dualPlayer.DualSaveInProgress = true;

            // Start the background worker.
            formProgressBar.Reset();
            worker.RunWorkerAsync(settings);

            // Show the progress bar.
            formProgressBar.ShowDialog();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "VideoExporter";
            BackgroundWorker worker = sender as BackgroundWorker;
            VideoExportSettings s = e.Argument as VideoExportSettings;

            // Stop playing and disable custom decoding size.
            leftPlayer.view.BeforeExportVideo();
            rightPlayer.view.BeforeExportVideo();

            PrepareStagingBitmaps(s);
            log.DebugFormat("Composite size: {0}.", bmpComposite.Size);

            // Get the image enumerator.
            // Do not call VideoReader.BeforeFrameEnumeration here.
            // We are not doing a frame enumeration, we are moving the playhead.
            // We can keep pre-buffering.
            IEnumerable<Bitmap> images = EnumerateComposite(s);

            s.RenderingSize = bmpComposite.Size;

            WriterFFMpegCLI w = new WriterFFMpegCLI();
            exportResult = w.Save(s, images, worker);
        }

        /// <summary>
        /// Enumerate composite frames by moving through the common timeline.
        /// </summary>
        IEnumerable<Bitmap> EnumerateComposite(VideoExportSettings s)
        {
            currentTime = 0;
            while (currentTime < commonTimeline.LastTime)
            {
                PaintCompositeImage(currentTime, s);
                yield return bmpComposite;
                currentTime += commonTimeline.FrameTime;
            }
        }

        /// <summary>
        /// Create the staging bitmaps we'll use to gather the frames and paint the composite.
        /// </summary>
        private void PrepareStagingBitmaps(VideoExportSettings s)
        {
            Size sizeLeft = leftPlayer.FrameServer.VideoReader.Geometry.ReferenceSize;
            Size sizeRight = rightPlayer.FrameServer.VideoReader.Geometry.ReferenceSize;
            Size sizeComp = ImageHelper.GetSideBySideCompositeSize(sizeLeft, sizeRight, true, s.Merged, s.Horizontal);

            bmpLeft = new Bitmap(sizeLeft.Width, sizeLeft.Height, PixelFormat.Format24bppRgb);
            bmpRight = new Bitmap(sizeRight.Width, sizeRight.Height, PixelFormat.Format24bppRgb);
            bmpComposite = new Bitmap(sizeComp.Width, sizeComp.Height, PixelFormat.Format24bppRgb);
        }

        /// <summary>
        /// Move the playhead in both players, get the images with drawings, paint the composite.
        /// </summary>
        private void PaintCompositeImage(long currentTime, VideoExportSettings s)
        {
            GotoTime(leftPlayer, currentTime);
            GotoTime(rightPlayer, currentTime);

            leftPlayer.PaintFlushedImage(bmpLeft);

            if (!s.Merged)
            {
                rightPlayer.PaintFlushedImage(bmpRight);
                ImageHelper.PaintSideBySideComposite(bmpComposite, bmpLeft, bmpRight, s.Horizontal);
            }
            else
            {
                Graphics g = Graphics.FromImage(bmpComposite);
                g.DrawImage(bmpLeft, Point.Empty);
            }
        }

        private void GotoTime(PlayerScreen player, long commonTime)
        {
            long localTime = commonTimeline.GetLocalTime(player, commonTime);
            localTime = Math.Max(0, localTime);
            player.GotoTime(localTime, false);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Ignore the frame count / total and recompute the percentage.
            int percent = (int)((double)currentTime * 100 / commonTimeline.LastTime);
            percent = Math.Min(percent, 100);
            formProgressBar.Update(percent, 100, true);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bmpLeft.Dispose();
            bmpRight.Dispose();
            bmpComposite.Dispose();

            formProgressBar.Close();
            formProgressBar.Dispose();

            leftPlayer.FrameServer.VideoReader.AfterFrameEnumeration();
            rightPlayer.FrameServer.VideoReader.AfterFrameEnumeration();

            leftPlayer.view.AfterExportVideo();
            rightPlayer.view.AfterExportVideo();

            leftPlayer.FrameServer.AfterSave();
            rightPlayer.FrameServer.AfterSave();

            dualPlayer.DualSaveInProgress = false;

            NotificationCenter.RaiseRefreshFileList(false);
        }

        private void FormProgressBar_CancelAsked(object sender, EventArgs e)
        {
            // This will set worker.CancellationPending to true, which we check periodically in the saving loop.
            // This will also end the worker immediately, maybe before we check for the cancellation in the other thread. 
            worker.CancelAsync();
        }

        public static string SuggestFilename(PlayerScreen player1, PlayerScreen player2)
        {
            if (player1 == null || player2 == null || !player1.Full || !player2.Full)
                return null;

            return String.Format("{0} - {1}",
                Path.GetFileNameWithoutExtension(player1.FilePath),
                Path.GetFileNameWithoutExtension(player2.FilePath));
        }
    }
}
