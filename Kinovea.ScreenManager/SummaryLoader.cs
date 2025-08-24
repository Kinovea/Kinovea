#region License
/*
Copyright © Joan Charmant 2011. jcharmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

using Kinovea.Video;

namespace Kinovea.ScreenManager
{

    /// <summary>
    /// A summary loader processes a list of files in a background thread to get their summaries.
    /// Raises individual SummaryLoaded events as the summaries are extracted.
    /// </summary>
    public class SummaryLoader
    {
        public event EventHandler<SummaryLoadedEventArgs> SummaryLoaded;

        public bool IsAlive 
        {
            get { return isAlive; }
        }

        private bool isAlive;
        private bool cancellationPending;
        private List<String> filenames;
        private Size maxImageSize;
        private BackgroundWorker bgWorker = new BackgroundWorker();
        private Stopwatch stopwatch = new Stopwatch();
        private const int thumbnailsToExtract = 4;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public SummaryLoader(List<String> filenames, Size maxImageSize)
        {
            this.filenames = filenames;
            this.maxImageSize = maxImageSize;
        }

        /// <summary>
        /// Start the background thread to extract summaries.
        /// </summary>
        public void Run()
        {
            if(filenames.Count < 1)
                return;
            
            isAlive = true;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += bgWorker_ProgressChanged;
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync(filenames);
        }

        /// <summary>
        /// Cancel the background thread.
        /// </summary>
        public void Cancel()
        {
            cancellationPending = true;
            bgWorker.CancelAsync();
        }


        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            stopwatch.Restart();

            // Note: having one background worker per file and running them all in parallel was tested
            // but it does more harm than good.
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            List<string> filenames = e.Argument as List<string>;
            
            if(filenames.Count < 1 || bgWorker == null)
        	{
        	    e.Result = null;
        	    return;
        	}

            for (int i = 0; i<filenames.Count; i++)
            {
                if(bgWorker.CancellationPending)
                {
                    log.DebugFormat("Cancelled summary loader.");
                    break;
                }
                
                string filename = filenames[i];
                VideoSummary summary = null;
               
                try
                {
                    if(string.IsNullOrEmpty(filename))
                       continue;
                    
                    string extension = Path.GetExtension(filename);
                    VideoReader reader = VideoTypeManager.GetVideoReader(extension);

        			if(reader != null)
                        summary = reader.ExtractSummary(filename, thumbnailsToExtract, maxImageSize);
                }
                catch(Exception exp)
                {
                    log.ErrorFormat("Error while extracting video summary for {0}.", filename);
                    log.Error(exp);
                }

                if (summary == null)
                    summary = new VideoSummary(filename);
                
    			bgWorker.ReportProgress(i, summary);
            }

            log.DebugFormat("{0} video summaries loaded in {1} ms", filenames.Count, stopwatch.ElapsedMilliseconds);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(cancellationPending || SummaryLoaded == null)
                return;
            
            SummaryLoaded(this, new SummaryLoadedEventArgs(e.UserState as VideoSummary, e.ProgressPercentage));
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isAlive = false;
        }
    }
}
