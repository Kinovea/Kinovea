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


using Kinovea.ScreenManager.Languages;
using System;
using System.ComponentModel;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.Video.FFMpeg;     // <- remove when possible to use the base Reader class.

namespace Kinovea.ScreenManager
{
    public partial class formFramesImport : Form
    {
    	#region Properties
		public bool Canceled
		{
			get { return m_bCanceled; }
		}    	
    	#endregion
    	
    	#region Members
        private VideoReaderFFMpeg m_VideoReader      = null;
        private long m_iSelStart;
        private long m_iSelEnd;
        private bool m_IsIdle = true;
        private bool m_bForceReload;
		private bool m_bCanceled;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public formFramesImport(VideoReaderFFMpeg _videoReader, long _iSelStart, long _iSelEnd, bool _bForceReload)
        {
            InitializeComponent();

            m_VideoReader = _videoReader;
            m_iSelStart = _iSelStart;
            m_iSelEnd = _iSelEnd;
            m_bForceReload = _bForceReload;

            this.Text       = "   " + ScreenManagerLang.FormFramesImport_Title;
            labelInfos.Text = ScreenManagerLang.FormFramesImport_Infos + " 0 / ~?";
			buttonCancel.Text = ScreenManagerLang.Generic_Cancel;
            
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            Application.Idle += new EventHandler(this.IdleDetector);
        }

        private void formFramesImport_Load(object sender, EventArgs e)
        {
            //-----------------------------------
            // Le Handle existe, on peut y aller.
            //-----------------------------------
            DoImport();
        }

        private void IdleDetector(object sender, EventArgs e)
        {
            m_IsIdle = true;
        }

        public void DoImport()
        {
            //--------------------------------------------------
            // Lancer le worker (déclenche bgWorker_DoWork)
            //--------------------------------------------------
            bgWorker.RunWorkerAsync();
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //-------------------------------------------------------------
            // /!\ Cette fonction s'execute dans l'espace du WORKER THREAD.
            // Les fonctions appelées d'ici ne doivent pas toucher l'UI.
            // Les appels ici sont synchrones mais on peut remonter de 
            // l'information par bgWorker_ProgressChanged().
            //-------------------------------------------------------------
            m_VideoReader.BgWorker = bgWorker;
            try
            {
            	m_VideoReader.ExtractToMemory(m_iSelStart, m_iSelEnd, m_bForceReload);
            }
            catch(Exception exp)
            {
            	log.Error("Exception thrown : " + exp.GetType().ToString() + " in " + exp.Source.ToString() + exp.TargetSite.Name.ToString());
            	log.Error("Message : " + exp.Message.ToString());
            	Exception inner = exp.InnerException;
     			while( inner != null )
     			{
          			log.Error("Inner exception : " + inner.Message.ToString());
          			inner = inner.InnerException;
     			}
            }
            e.Result = 0;
        }
        
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //--------------------------------------------------------------------------------
            // Problème possible : 
            // Le worker thread va vouloir mettre à jour les données très souvent.
            // Comme le traitement est asynchrone,il se peut qu'il poste des ReportProgress()
            // plus vite qu'ils ne soient traités ici.
            // Il faut donc attendre que la form soit idle.
            //--------------------------------------------------------------------------------
            if (m_IsIdle)
            {
                m_IsIdle = false;

                int iTotal = (int)e.UserState;
                int iValue = (int)e.ProgressPercentage;

                if (iValue > iTotal){ iValue = iTotal; }
                
                progressBar.Maximum = iTotal;
                progressBar.Value   = iValue;

                labelInfos.Text = ScreenManagerLang.FormFramesImport_Infos + " " + iValue + " / ~" + iTotal;
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //----------------------------------------------------------------------
            // On arrive ici lorsque la fonction bgWorker_DoWork() ressort.
            // Les données dans e doivent être mise en place dans bgWorker_DoWork();  
            //----------------------------------------------------------------------

            // Se décrocher de l'event Idle.
            Application.Idle -= new EventHandler(this.IdleDetector);
            Hide();
        }
        
        void ButtonCancelClick(object sender, EventArgs e)
        {
        	if(!m_bCanceled)
        	{
        		log.Debug("Cancel of extraction to memory asked.");
	        	if (bgWorker.IsBusy)
	            {
	        		// This will set bgWorker.CancellationPending to true,
	        		// which we check periodically in VideoFile.ExtractToMemory method.
	        		// This will also end the bgWorker immediately,
	        		// maybe before we check for the cancellation in the other thread. 
	        		// The VideoFile will be notified after we return to psui.ImportSelectionToMemory.
	        		bgWorker.CancelAsync();
	        		m_bCanceled = true;
	                buttonCancel.Enabled = false;
	            }	
        	}
        }
    }
}