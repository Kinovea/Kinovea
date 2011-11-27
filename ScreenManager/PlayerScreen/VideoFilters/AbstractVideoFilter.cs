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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.Video;

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
		#region Properties
        public abstract string Name {
        	get;
        }
        public virtual Bitmap Icon {
		    get { return null; }
		}
        public virtual bool Experimental {
            get { return false; }
        }
		#endregion
        
        #region Abstract Methods
        public abstract void Activate(IWorkingZoneFramesContainer _framesContainer, Action<InteractiveEffect> _setInteractiveEffect);
        protected abstract void Process(object sender, DoWorkEventArgs e);
        #endregion
        
        #region Members
        private formProgressBar m_FormProgressBar;
        #endregion
        
        #region Concrete Methods
        protected void StartProcessing()
        {
            // Spawn a new thread for the computation, and a modal dialog for progress bar.
            // This function is not mandatory, if the effect is really quick,
            // a VideoFilter may proceed within Activate() directly.
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Process;
            worker.ProgressChanged += bgWorker_ProgressChanged;
            worker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            worker.RunWorkerAsync();
            
            m_FormProgressBar = new formProgressBar(false);
            m_FormProgressBar.ShowDialog();
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        	// Should be called by concrete filters to update progress bar.
        	int iMaximum = (int)e.UserState;
            int iValue = (int)e.ProgressPercentage;

            if (iValue > iMaximum) { iValue = iMaximum; }
        	
            m_FormProgressBar.Update(iValue, iMaximum, false);
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        	m_FormProgressBar.Close();
        	m_FormProgressBar.Dispose();
        }
        #endregion
	}
}
