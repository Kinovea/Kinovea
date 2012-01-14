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
        public bool IsAlive {
            get { return m_IsAlive; }
        }
        public event EventHandler<SummaryLoadedEventArgs> SummaryLoaded;
        
        private bool m_IsAlive;
        private bool m_CancellationPending;
        private List<String> m_FileNames;
        private BackgroundWorker m_Worker = new BackgroundWorker();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public SummaryLoader(List<String> _fileNames)
        {
            m_FileNames = _fileNames;
        }
        public void Run()
        {
            if(m_FileNames.Count < 1)
                return;
            
            m_IsAlive = true;
            m_Worker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            m_Worker.ProgressChanged += bgWorker_ProgressChanged;
            m_Worker.DoWork += bgWorker_DoWork;
            m_Worker.WorkerSupportsCancellation = true;
            m_Worker.WorkerReportsProgress = true;
            m_Worker.RunWorkerAsync(m_FileNames);
        }
        public void Cancel()
        {
            m_CancellationPending = true;
            m_Worker.CancelAsync();
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
        			
        			if(reader != null)
                        summary = reader.ExtractSummary(filename, 5, 200);
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
            if(m_CancellationPending || SummaryLoaded == null)
                return;
            
            SummaryLoaded(this, new SummaryLoadedEventArgs(e.UserState as VideoSummary, e.ProgressPercentage));
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_IsAlive = false;
        }
    }
}
