/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.Video;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
    // TODO:
    // - Use an actual Queue object.
    // - Make thread safe.
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
        private List<VideoSummary> m_VideoSummaryQueue;
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
            m_VideoSummaryQueue = new List<VideoSummary>();

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
            // /!\ This is WORKER THREAD space. Do not update UI.
            Thread.CurrentThread.Name = String.Format("Thumbnail Loader ({0})", Thread.CurrentThread.ManagedThreadId);
            List<String> fileNames = (List<String>)e.Argument;
            m_VideoSummaryQueue.Clear();
            m_iLastFilled = -1;

            BackgroundWorker bgWorker = sender as BackgroundWorker;
            e.Cancel = false;

            for (int i = 0; i < fileNames.Count; i++)
            {
                if (!bgWorker.CancellationPending)
                {
                    try
                    {
                        VideoSummary summary = m_VideoFile.ExtractSummary(fileNames[i], 5, 200);
                        m_VideoSummaryQueue.Insert(0, summary);
                    }
                    catch (Exception exp)
                    {
                        log.Error("Error while extracting video summary");
                        log.Error(exp);
                        m_VideoSummaryQueue.Insert(0, null);
                    }
                    bgWorker.ReportProgress(i, null);
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
            // Back into main thread. Update UI here.
            //Console.WriteLine("[ProgressChanged] - queue:{0}, total:{1}", m_BitmapQueue.Count, m_iTotalFilesToLoad);
            
            if (m_bIsIdle && !m_bgThumbsLoader.CancellationPending && (m_iLastFilled + 1 < m_Panel.Controls.Count))
            {
                m_bIsIdle = false;

                // Copy the queue, because it is still being filled by the bg worker.
                List<VideoSummary> tmpQueue = new List<VideoSummary>();
                foreach (VideoSummary summary in m_VideoSummaryQueue)
                    tmpQueue.Add(summary);

                // bg worker can start re fueling now, if a bitmap was queued during the copy, don't clear it.
                m_VideoSummaryQueue.RemoveRange(m_VideoSummaryQueue.Count - tmpQueue.Count, tmpQueue.Count);

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
            if (m_VideoSummaryQueue.Count > 0 && !e.Cancelled)
            {
                PopulateControls(m_VideoSummaryQueue);
            }

            Application.Idle -= new EventHandler(this.IdleDetector);
            m_bUnused = true;
        }
        private void PopulateControls(List<VideoSummary> _summariesQueue)
        {
            // Unqueue bitmaps and populate the controls
            for (int i = _summariesQueue.Count - 1; i >= 0; i--)
            {
                // Double check.
                if (m_iLastFilled + 1 < m_Panel.Controls.Count)
                {
                    m_iLastFilled++;
                    ThumbListViewItem tlvi = m_Panel.Controls[m_iLastFilled] as ThumbListViewItem;
                    if(tlvi != null)
                    {
	                    if (_summariesQueue[i] != null)
	                    {
	                    	if(_summariesQueue[i].Thumbs.Count > 0)
	                    	{
		                    	tlvi.Thumbnails = _summariesQueue[i].Thumbs;
		                    	if(_summariesQueue[i].IsImage)
		                    	{
		                    		tlvi.IsImage = true;
		                    		tlvi.Duration = "0";
		                    	}
		                    	else
		                    	{
		                    		tlvi.Duration = TimeHelper.MillisecondsToTimecode((double)_summariesQueue[i].DurationMilliseconds, false, true);
		                    	}
		                    	
		                    	tlvi.ImageSize = _summariesQueue[i].ImageSize;
		                    	tlvi.HasKva = _summariesQueue[i].HasKva;
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
                    
                    _summariesQueue.RemoveAt(i);
                    
                }
            }
            m_Panel.Invalidate();
        }
        #endregion
    }
}
