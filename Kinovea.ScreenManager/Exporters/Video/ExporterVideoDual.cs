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

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Create and save a composite video with side by side synchronized images.
    /// If merge is active, only saves the left video.
    /// </summary>
    public class ExporterVideoDual
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private FormProgressBar formProgressBar = new FormProgressBar(true);
        private PlayerScreen leftPlayer;
        private PlayerScreen rightPlayer;
        private DualPlayerController dualPlayer;
        private string filePath;
        private bool horizontal = true;
        private bool merging = false;

        private CommonTimeline commonTimeline;
        private double fileFrameInterval;
        private bool cancelled;
        
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ExporterVideoDual()
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            formProgressBar.CancelAsked += FormProgressBar_CancelAsked;
        }

        public void Export(PlayerScreen leftPlayer, PlayerScreen rightPlayer, string filePath, bool horizontal, DualPlayerController dualPlayer)
        {
            this.leftPlayer = leftPlayer;
            this.rightPlayer = rightPlayer;
            this.dualPlayer = dualPlayer;
            this.filePath = filePath;
            this.horizontal = horizontal;

            this.merging = dualPlayer.View.Merging;
            this.commonTimeline = dualPlayer.CommonTimeline;

            // During saving we move through the common timeline by a time unit based on framerate and high speed factor, but not based on user custom slow motion factor.
            // For the framerate saved in the file metadata we take user custom slow motion into account and not high speed factor.
            fileFrameInterval = Math.Max(leftPlayer.PlaybackFrameInterval, rightPlayer.PlaybackFrameInterval);
            
            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            leftPlayer.DualSaveInProgress = true;
            rightPlayer.DualSaveInProgress = true;
            dualPlayer.DualSaveInProgress = true;

            // Start the background worker.
            formProgressBar.Reset();
            worker.RunWorkerAsync();

            // Show the progress bar.
            formProgressBar.ShowDialog();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This runs in the background thread.
            Thread.CurrentThread.Name = "VideoExporter";
            BackgroundWorker worker = sender as BackgroundWorker;
            SavingSettings s = e.Argument as SavingSettings;

            leftPlayer.view.BeforeExportVideo();
            rightPlayer.view.BeforeExportVideo();

            leftPlayer.FrameServer.VideoReader.BeforeFrameEnumeration();
            rightPlayer.FrameServer.VideoReader.BeforeFrameEnumeration();
            
            int threadResult = 0;
            
            // Get first frame outside the loop to set up the saving context.
            long currentTime = 0;
            Bitmap bmpComposite = GetCompositeImage(currentTime);
            
            log.DebugFormat("Composite size: {0}.", bmpComposite.Size);

            VideoInfo info = new VideoInfo
            {
                ReferenceSize = bmpComposite.Size
            };

            string formatString = FilenameHelper.GetFormatString(filePath);

            SaveResult result = videoFileWriter.OpenSavingContext(filePath, info, formatString, fileFrameInterval);

            if (result != SaveResult.Success)
            {
                e.Result = 2;
                return;
            }

            videoFileWriter.SaveFrame(bmpComposite);
            bmpComposite.Dispose();
            
            while (currentTime < commonTimeline.LastTime && !cancelled)
            {
                currentTime += commonTimeline.FrameTime;

                if (worker.CancellationPending)
                {
                    threadResult = 1;
                    cancelled = true;
                    break;
                }

                bmpComposite = GetCompositeImage(currentTime);
                videoFileWriter.SaveFrame(bmpComposite);
                bmpComposite.Dispose();

                int percent = (int)((double)currentTime * 100 / commonTimeline.LastTime);
                worker.ReportProgress(percent);
            }

            if (!cancelled)
                threadResult = 0;
            
            e.Result = threadResult;
        }

        private Bitmap GetCompositeImage(long currentTime)
        {
            Bitmap bmpComposite;

            GotoTime(leftPlayer, currentTime);
            GotoTime(rightPlayer, currentTime);

            Bitmap bmpLeft = leftPlayer.GetFlushedImage();

            if (!merging)
            {
                Bitmap bmpRight = rightPlayer.GetFlushedImage();
                bmpComposite = ImageHelper.GetSideBySideComposite(bmpLeft, bmpRight, true, horizontal);
                bmpRight.Dispose();
            }
            else
            {
                int height = bmpLeft.Height;
                int width = bmpLeft.Width;

                if (bmpLeft.Height % 2 != 0)
                    height++;

                if (width % 4 != 0)
                    width += 4 - (width % 4);

                bmpComposite = new Bitmap(width, height, bmpLeft.PixelFormat);
                Graphics g = Graphics.FromImage(bmpComposite);
                g.DrawImage(bmpLeft, Point.Empty);
            }

            bmpLeft.Dispose();
            
            return bmpComposite;
        }

        private void GotoTime(PlayerScreen player, long commonTime)
        {
            long localTime = commonTimeline.GetLocalTime(player, commonTime);
            localTime = Math.Max(0, localTime);
            player.GotoTime(localTime, false);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (worker.CancellationPending)
                return;

            formProgressBar.Update(Math.Min(e.ProgressPercentage, 100), 100, true);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            formProgressBar.Close();
            formProgressBar.Dispose();

            leftPlayer.FrameServer.VideoReader.AfterFrameEnumeration();
            rightPlayer.FrameServer.VideoReader.AfterFrameEnumeration();

            leftPlayer.view.AfterExportVideo();
            rightPlayer.view.AfterExportVideo();

            leftPlayer.FrameServer.AfterSave();
            rightPlayer.FrameServer.AfterSave();

            dualPlayer.DualSaveInProgress = false;

            //GotoTime(currentTime, true);

            try
            {
                if (cancelled)
                    DeleteTemporaryFile(filePath);

                if (!cancelled && (int)e.Result != 1 && videoFileWriter != null)
                    videoFileWriter.CloseSavingContext((int)e.Result == 0);
            }
            catch (Exception exception)
            {
                log.ErrorFormat("Error while completing dual save. {0}", exception);
            }

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }

        private void FormProgressBar_CancelAsked(object sender, EventArgs e)
        {
            // This will set worker.CancellationPending to true, which we check periodically in the saving loop.
            // This will also end the worker immediately, maybe before we check for the cancellation in the other thread. 
            videoFileWriter.CloseSavingContext(false);
            cancelled = true;
            worker.CancelAsync();
        }

        private void DeleteTemporaryFile(string filename)
        {
            log.Debug("Video saving cancelled. Deleting file.");
            if (!File.Exists(filename))
                return;

            try
            {
                File.Delete(filename);
            }
            catch (Exception exp)
            {
                log.Error("Error while deleting file.");
                log.Error(exp.Message);
                log.Error(exp.StackTrace);
            }
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
