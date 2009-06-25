using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
    public class ThumbListLoader
    {
        #region Properties
        public bool Unused
        {
            get { return m_bUnused; }
            set { m_bUnused = value; }
        }
        #endregion

        #region Members
        private bool m_bUnused = true;
        List<String> m_FileNames;

        BackgroundWorker m_bgThumbsLoader;
        private bool m_bIsIdle = false;
        private List<InfosThumbnail> m_InfosThumbnailQueue;
        private int m_iLastFilled = -1;
        private SplitterPanel m_Panel;
        private VideoFile m_VideoFile;
        private int m_iTotalFilesToLoad = 0;    // only for debug info
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public ThumbListLoader(List<String> _fileNames, SplitterPanel _panel, VideoFile _PlayerServer)
        {
            m_FileNames = _fileNames;
            m_Panel = _panel;
            m_VideoFile = _PlayerServer;

            m_iTotalFilesToLoad = _fileNames.Count;
            m_InfosThumbnailQueue = new List<InfosThumbnail>();

            m_bgThumbsLoader = new BackgroundWorker();
            m_bgThumbsLoader.WorkerReportsProgress = true;
            m_bgThumbsLoader.WorkerSupportsCancellation = true;
            m_bgThumbsLoader.DoWork += new DoWorkEventHandler(bgThumbsLoader_DoWork);
            m_bgThumbsLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgThumbsLoader_RunWorkerCompleted);
            m_bgThumbsLoader.ProgressChanged += new ProgressChangedEventHandler(bgThumbsLoader_ProgressChanged);
        }
        #endregion

        #region Public interface
        public void Launch()
        {
            log.Debug(String.Format("ThumbListLoader preparing to load {0} files.", m_iTotalFilesToLoad));

            m_bUnused = false;
            Application.Idle += new EventHandler(this.IdleDetector);
            if (!m_bgThumbsLoader.IsBusy)
            {
                m_bgThumbsLoader.RunWorkerAsync(m_FileNames);
            }
        }
        public void Cancel()
        {
            m_bgThumbsLoader.CancelAsync();
        }
        #endregion

        #region Background Thread Work and display.
        private void IdleDetector(object sender, EventArgs e)
        {
            // Used to know when it is safe to update the ui.
            m_bIsIdle = true;
        }
        private void bgThumbsLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            //-------------------------------------------------------------
            // /!\ This is WORKER THREAD space. Do not update UI.
            //-------------------------------------------------------------
            List<String> fileNames = (List<String>)e.Argument;
            m_InfosThumbnailQueue.Clear();
            m_iLastFilled = -1;

            e.Cancel = false;

            for (int i = 0; i < fileNames.Count; i++)
            {
                if (!m_bgThumbsLoader.CancellationPending)
                {
                    try
                    {
                    	InfosThumbnail it = m_VideoFile.GetThumbnail(fileNames[i], 200);
                        m_InfosThumbnailQueue.Insert(0, it);
                    }
                    catch (Exception)
                    {
                        m_InfosThumbnailQueue.Insert(0, null);
                    }
                    m_bgThumbsLoader.ReportProgress(i, null);
                }
                else
                {
                    log.Debug("bgThumbsLoader_DoWork - cancelling");
                    e.Cancel = true;
                    break;
                }
            }
            e.Result = 0;
        }
        private void bgThumbsLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //-------------------------------------------------
            // This is not Worker Thread space. Update UI here.
            //-------------------------------------------------
            //Console.WriteLine("[ProgressChanged] - queue:{0}, total:{1}", m_BitmapQueue.Count, m_iTotalFilesToLoad);
            
            if (m_bIsIdle && !m_bgThumbsLoader.CancellationPending && (m_iLastFilled + 1 < m_Panel.Controls.Count))
            {
                m_bIsIdle = false;

                // Copy the queue, because it is still being filled by the bg worker.
                List<InfosThumbnail> tmpQueue = new List<InfosThumbnail>();
                foreach (InfosThumbnail it in m_InfosThumbnailQueue)
                {
                    tmpQueue.Add(it);
                }

                // bg worker can start re fueling now, if a bitmap was queued during the copy, don't clear it.
                m_InfosThumbnailQueue.RemoveRange(m_InfosThumbnailQueue.Count - tmpQueue.Count, tmpQueue.Count);

                PopulateControls(tmpQueue);                
            }
            else
            {
                // Console.WriteLine("[ProgressChanged] - Thumb not added. (Idle:{0}, cancelling:{1})", m_bIsIdle.ToString(), m_bgThumbsLoader.CancellationPending.ToString(), m_iLastFilled + 1);
            }
        }
        private void bgThumbsLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //---------------------------------------------------------------------------
            // Check if the queue has been completely flushed.
            // (in case the last thumbs loaded couldn't be added because we weren't idle.
            //---------------------------------------------------------------------------
            if (m_InfosThumbnailQueue.Count > 0 && !e.Cancelled)
            {
                PopulateControls(m_InfosThumbnailQueue);
            }

            Application.Idle -= new EventHandler(this.IdleDetector);
            m_bUnused = true;
        }
        private void PopulateControls(List<InfosThumbnail> _infosThumbQueue)
        {
            // Unqueue bitmaps and populate the controls
            for (int i = _infosThumbQueue.Count - 1; i >= 0; i--)
            {
                // Double check.
                if (m_iLastFilled + 1 < m_Panel.Controls.Count)
                {
                    m_iLastFilled++;
                    ThumbListViewItem tlvi = m_Panel.Controls[m_iLastFilled] as ThumbListViewItem;
                    if(tlvi != null)
                    {
	                    if (_infosThumbQueue[i] != null)
	                    {
	                    	if(_infosThumbQueue[i].Thumbnails.Count > 0)
	                    	{
		                    	tlvi.Thumbnails = _infosThumbQueue[i].Thumbnails;
		                    	tlvi.Duration = TimeHelper.MillisecondsToTimecode(_infosThumbQueue[i].iDurationMilliseconds, false);
		                    }
	                    	else
	                    	{
	                    		tlvi.DisplayAsError();
	                    	}
	                    }
	                    else
	                    {
	                         tlvi.DisplayAsError();
	                    }
	                    
	                    // Issue: We computed the .top coord of the thumb when the panel wasn't moving.
            			// If we are scrolling, the .top of the panel is moving, 
            			// so the thumbnails will be activated at the wrong spot.
	                    if(m_Panel.AutoScrollPosition.Y != 0)
	                    {
	                    	tlvi.Top = tlvi.Top + m_Panel.AutoScrollPosition.Y;
	                    }

                    	m_Panel.Controls[m_iLastFilled].Visible = true;
                    }
                    
                    _infosThumbQueue.RemoveAt(i);
                    
                }
            }
            m_Panel.Invalidate();
        }
        #endregion
    }
}
