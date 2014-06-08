using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.IO;
using Kinovea.Video.FFMpeg;
using System.ComponentModel;
using Kinovea.Services;
using System.Drawing;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Create and save a composite video with side by side synchronized images.
    /// If merge is active, only saves the left video.
    /// </summary>
    public class DualVideoExporter
    {
        private CommonTimeline commonTimeline;
        private PlayerScreen leftPlayer;
        private PlayerScreen rightPlayer;
        private string dualSaveFileName;
        private bool dualSaveCancelled;
        private bool merging;
        
        private VideoFileWriter videoFileWriter = new VideoFileWriter();
        private BackgroundWorker bgWorkerDualSave;
        private formProgressBar dualSaveProgressBar;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Export(CommonTimeline commonTimeline, PlayerScreen leftPlayer, PlayerScreen rightPlayer, bool merging)
        {
            this.commonTimeline = commonTimeline;
            this.leftPlayer = leftPlayer;
            this.rightPlayer = rightPlayer;
            this.merging = merging;

            dualSaveFileName = GetFilename(leftPlayer, rightPlayer);
            if (string.IsNullOrEmpty(dualSaveFileName))
                return;

            dualSaveCancelled = false;
            
            // Instanciate and configure the bgWorker.
            bgWorkerDualSave = new BackgroundWorker();
            bgWorkerDualSave.WorkerReportsProgress = true;
            bgWorkerDualSave.WorkerSupportsCancellation = true;
            bgWorkerDualSave.DoWork += bgWorkerDualSave_DoWork;
            bgWorkerDualSave.ProgressChanged += bgWorkerDualSave_ProgressChanged;
            bgWorkerDualSave.RunWorkerCompleted += bgWorkerDualSave_RunWorkerCompleted;

            // Make sure none of the screen will try to update itself.
            // Otherwise it will cause access to the other screen image (in case of merge), which can cause a crash.
            
            leftPlayer.DualSaveInProgress = true;
            rightPlayer.DualSaveInProgress = true;

            dualSaveProgressBar = new formProgressBar(true);
            dualSaveProgressBar.Cancel = dualSave_CancelAsked;
            
            // The worker thread runs in the background while the UI thread is in the progress bar dialog.
            // We only continue after these two lines once the video has been saved or the saving cancelled.
            bgWorkerDualSave.RunWorkerAsync();
            dualSaveProgressBar.ShowDialog();

            if (dualSaveCancelled)
                DeleteTemporaryFile(dualSaveFileName);

            leftPlayer.DualSaveInProgress = false;
            rightPlayer.DualSaveInProgress = false;
        }

        private string GetFilename(PlayerScreen leftPlayer, PlayerScreen rightPlayer)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Title = ScreenManagerLang.dlgSaveVideoTitle;
            dlgSave.RestoreDirectory = true;
            dlgSave.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
            dlgSave.FilterIndex = 1;
            dlgSave.FileName = String.Format("{0} - {1}", Path.GetFileNameWithoutExtension(leftPlayer.FilePath), Path.GetFileNameWithoutExtension(rightPlayer.FilePath));

            if (dlgSave.ShowDialog() != DialogResult.OK)
                return null;

            return dlgSave.FileName;
        }

        private void bgWorkerDualSave_DoWork(object sender, DoWorkEventArgs e)
        {
            // This is executed in Worker Thread space. (Do not call any UI methods)
            log.Debug("Saving side by side video.");

            int threadResult = 0;
            
            // Get first frame outside the loop to set up the saving context.
            long currentTime = 0;
            Bitmap composite = GetCompositeImage(currentTime);
            
            log.Debug(String.Format("Composite size: {0}.", composite.Size));

            VideoInfo info = new VideoInfo 
            { 
                OriginalSize = composite.Size 
            };
            
            double frameInterval = (double)commonTimeline.FrameTime / 1000;
            SaveResult result = videoFileWriter.OpenSavingContext(dualSaveFileName, info, frameInterval, false);

            if (result != SaveResult.Success)
            {
                e.Result = 2;
                return;
            }

            videoFileWriter.SaveFrame(composite);
            composite.Dispose();
            
            while (currentTime < commonTimeline.LastTime && !dualSaveCancelled)
            {
                currentTime += commonTimeline.FrameTime;

                if (bgWorkerDualSave.CancellationPending)
                {
                    threadResult = 1;
                    dualSaveCancelled = true;
                    break;
                }

                composite = GetCompositeImage(currentTime);
                videoFileWriter.SaveFrame(composite);
                composite.Dispose();

                int percent = (int)((double)currentTime * 100 / commonTimeline.LastTime);
                bgWorkerDualSave.ReportProgress(percent);
            }

            if (!dualSaveCancelled)
                threadResult = 0;
            
            e.Result = threadResult;
        }
        
        private void GotoTime(PlayerScreen player, long commonTime)
        {
            long localTime = commonTimeline.GetLocalTime(player, commonTime);
            localTime = Math.Max(0, localTime);
            player.GotoTime(localTime, false);
        }

        private Bitmap GetCompositeImage(long currentTime)
        {
            Bitmap composite;

            GotoTime(leftPlayer, currentTime);
            GotoTime(rightPlayer, currentTime);

            Bitmap img1 = leftPlayer.GetFlushedImage();

            if (!merging)
            {
                Bitmap img2 = rightPlayer.GetFlushedImage();
                composite = ImageHelper.GetSideBySideComposite(img1, img2, true, true);
                img2.Dispose();
            }
            else
            {
                int height = img1.Height;

                if (img1.Height % 2 != 0)
                    height++;

                composite = new Bitmap(img1.Width, height, img1.PixelFormat);
                Graphics g = Graphics.FromImage(composite);
                g.DrawImage(img1, Point.Empty);
            }

            img1.Dispose();
            
            return composite;
        }

        private void bgWorkerDualSave_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (bgWorkerDualSave.CancellationPending)
                return;

            dualSaveProgressBar.Update(Math.Min(e.ProgressPercentage, 100), 100, true);
        }

        private void bgWorkerDualSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                dualSaveProgressBar.Close();
                dualSaveProgressBar.Dispose();

                if (!dualSaveCancelled && (int)e.Result != 1 && videoFileWriter != null)
                    videoFileWriter.CloseSavingContext((int)e.Result == 0);
            }
            catch (Exception exception)
            {
                log.ErrorFormat("Error while completing dual save. {0}", exception);
            }

            NotificationCenter.RaiseRefreshFileExplorer(this, false);
        }

        private void dualSave_CancelAsked(object sender, EventArgs e)
        {
            // This will simply set BgWorker.CancellationPending to true, which we check periodically in the saving loop.
            // This will also end the bgWorker immediately, maybe before we check for the cancellation in the other thread. 
            
            videoFileWriter.CloseSavingContext(false);
            dualSaveCancelled = true;
            bgWorkerDualSave.CancelAsync();
        }

        private void DeleteTemporaryFile(string filename)
        {
            log.Debug("Dual video saving cancelled. Deleting file.");
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
        
    }
}
