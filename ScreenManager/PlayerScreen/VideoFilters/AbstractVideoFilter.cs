#region License
/*
Copyright © Joan Charmant 2009.
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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// AbstractVideoFilter define the way all Video Filters should behave.
	/// A VideoFilter is an entity that can be used to modify a subset (or all)
	/// of the images contained in the images collection when in analysis mode.
	/// 
	/// It is the basis of all special effects in Kinovea.
	/// 
	/// The output of a filter can be : 
	/// - a single frame (eg. mosaic)
	/// - same set of images but internally modified (colors adjust, contrast adjust, edges only, etc.)
	/// - a set with more images (eg. smooth slow motion) 
	/// - a special complex object (kinorama : background + motion layer segmented)
	/// 
	/// This class should encapsulate all there is to a VideoFilter except localization.
	/// Ideally a concrete video filter should be loadable dynamically from an external assembly.
	/// 
	/// The class also provide support for threading and progress bar. 
	/// </summary>
	public abstract class AbstractVideoFilter
	{
		#region Abstract Properties
        public abstract ToolStripMenuItem Menu
        {
        	get;
        }
        public abstract List<DecompressedFrame> FrameList
        {
        	set;
        }
        public abstract bool Experimental
        {
        	get;
        }
        #endregion
        
        #region Abstract Methods
        public abstract void Menu_OnClick(object sender, EventArgs e);
        protected abstract void Process();
        #endregion
        
        #region Concrete Members
        protected BackgroundWorker m_BackgroundWorker = new BackgroundWorker();
        private formProgressBar m_FormProgressBar;
        #endregion
        
        #region Concrete Constructor
        public AbstractVideoFilter()
        {
        	m_BackgroundWorker.WorkerReportsProgress = true;
        	m_BackgroundWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
        	m_BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            m_BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
        }
        #endregion
        
        #region Concrete Methods
        protected void StartProcessing()
        {
        	// This method is called by concrete filters to start applying the filter.
        	// It should be called whenever the filter takes time to process.
        	m_FormProgressBar = new formProgressBar(false);
        	m_BackgroundWorker.RunWorkerAsync();
        	m_FormProgressBar.ShowDialog();
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
        	// This is executed in Worker Thread space. 
        	// (Do not call any UI methods)
        	Process();
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	// This method should be called by concrete filters to update progress bar.
        	// call snippet : m_BackgroundWorker.ReportProgress(iCurrentValue, iMaximum);
        	
        	int iMaximum = (int)e.UserState;
            int iValue = (int)e.ProgressPercentage;

            if (iValue > iMaximum) { iValue = iMaximum; }
        	
            m_FormProgressBar.Update(iValue, iMaximum, false);
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	m_FormProgressBar.Close();
        	m_FormProgressBar.Dispose();
        	ProcessingOver();
        }
        protected void ProcessingOver()
        {
        	// This method will be automatically called if StartProcessing() was used.
        	// (asynchronous + progress bar) 
        	ProcessingOver(null);
        }
        protected void ProcessingOver(DrawtimeFilterOutput _dfo)
        {
        	// Notify the ScreenManager that we are done.
        	DelegatesPool dp = DelegatesPool.Instance();
        	if (dp.VideoProcessingDone != null)
            {
                dp.VideoProcessingDone(_dfo);
            }
        }
        #endregion
	}
}
