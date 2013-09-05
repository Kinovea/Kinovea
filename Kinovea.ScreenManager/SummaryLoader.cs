#region License
/*
Copyright © Joan Charmant 2011. joan.charmant@gmail.com 
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;

using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class SummaryLoader
    {
        public bool IsAlive 
        {
            get { return isAlive; }
        }

        public event EventHandler<SummaryLoadedEventArgs> SummaryLoaded;
        
        private bool isAlive;
        private bool cancellationPending;
        private List<String> filenames;
        private Size maxImageSize;
        private BackgroundWorker bgWorker = new BackgroundWorker();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public SummaryLoader(List<String> filenames, Size maxImageSize)
        {
            this.filenames = filenames;
            this.maxImageSize = maxImageSize;
        }
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
        public void Cancel()
        {
            cancellationPending = true;
            bgWorker.CancelAsync();
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Note: having one background worker per file and running them all in parallel was tested
            // but it does more harm than good.
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            List<string> filenames = e.Argument as List<string>;
            
            if(filenames.Count < 1 || bgWorker == null)
        	{
        	    e.Result = null;
        	    return;
        	}
            
            for(int i = 0; i<filenames.Count; i++)
            {
                if(bgWorker.CancellationPending)
                    break;
                
                string filename = filenames[i];
                VideoSummary summary = null;
               
                try
                {
                    if(string.IsNullOrEmpty(filename))
                       continue;
                    
                    string extension = Path.GetExtension(filename);
                    VideoReader reader = VideoTypeManager.GetVideoReader(extension);

                    int numberOfThumbnails = 5;
                    
        			if(reader != null)
                        summary = reader.ExtractSummary(filename, numberOfThumbnails, maxImageSize);
                }
                catch(Exception exp)
                {
                    log.ErrorFormat("Error while extracting video summary for {0}.", filename);
                    log.Error(exp);
                }
                
                if(summary == null)
                    summary = VideoSummary.GetInvalid(filename);
                
    			bgWorker.ReportProgress(i, summary);
            }
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
