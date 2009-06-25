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
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// This is the loader for files.
	/// Launches the background thread that calls into ffmpeg.
	/// Display a progress bar while running.
	/// </summary>
    public partial class formFileLoader : Form
    {
        #region Members
        private String          m_FilePath;
        private PlayerScreen    m_PlayerScreen;
        private bool            m_IsIdle;
		#endregion

        public formFileLoader(PlayerScreen _PlayerScreen, String _FilePath)
        {
            InitializeComponent();

            m_PlayerScreen = _PlayerScreen;
            m_FilePath = _FilePath;

            Text = "   " + m_PlayerScreen.m_ResourceManager.GetString("LoadMovieProgress_Title", Thread.CurrentThread.CurrentUICulture);
            buttonCancel.Text = m_PlayerScreen.m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture); ;

            progressBar.Minimum  = 0;
            progressBar.Maximum  = 100;
            progressBar.MarqueeAnimationSpeed = 100;
            progressBar.Step     = 1;
            progressBar.Style    = ProgressBarStyle.Blocks;
            progressBar.Value    = 0;

            m_IsIdle = false;

            this.Load += this.formFileLoader_Load;
            Application.Idle += new EventHandler(this.IdleDetector);
        }

        private void formFileLoader_Load(object sender, EventArgs e)
        {
            //-----------------------------------
            // Le Handle existe, on peut y aller.
            //-----------------------------------
            DoLoad();
        }

        private void IdleDetector(object sender, EventArgs e)
        {
            m_IsIdle = true;
        }
        
        public void DoLoad()
        {
            //--------------------------------------------------
            // Lancer le worker (déclenche bgMovieLoader_DoWork)
            //--------------------------------------------------
            bgMovieLoader.RunWorkerAsync(m_FilePath);
        }

        private void bgMovieLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            //-------------------------------------------------------------
            // /!\ Cette fonction s'execute dans l'espace du WORKER THREAD.
            // Les fonctions appelées d'ici ne doivent pas toucher l'UI.
            //-------------------------------------------------------------
            m_PlayerScreen.m_PlayerScreenUI.m_VideoFile.BgWorker = bgMovieLoader;
            LoadResult res = m_PlayerScreen.m_PlayerScreenUI.m_VideoFile.Load((String)e.Argument);

            if(res == LoadResult.Cancelled)
                e.Cancel = true;

            e.Result = res;
        }

        private void bgMovieLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
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

                if ((iTotal == 0) || (iValue > iTotal))
                {
                    //----------------------------------------------------------------------
                    // Le PlayerServer n'a pas pu estimer le nombre de frames à l'avance.
                    //----------------------------------------------------------------------
                    if (progressBar.Style != ProgressBarStyle.Marquee)
                        progressBar.Style = ProgressBarStyle.Marquee;

                    labelInfos.Text = m_PlayerScreen.m_ResourceManager.GetString("LoadMovieProgress_Infos", Thread.CurrentThread.CurrentUICulture)
                                                     + " " + iValue + " / " +
                                                     m_PlayerScreen.m_ResourceManager.GetString("LoadMovieProgress_TotalUnknown", Thread.CurrentThread.CurrentUICulture);
                }
                else
                {
                    //--------------------------------------------------------
                    // Le PlayerServer à réussi à estimer le nombre de frames.
                    // On s'en sert, même s'il est faux.
                    //--------------------------------------------------------
                    if (progressBar.Style != ProgressBarStyle.Blocks)
                        progressBar.Style = ProgressBarStyle.Blocks;

                    progressBar.Maximum = iTotal;
                    progressBar.Value = iValue;

                    labelInfos.Text = m_PlayerScreen.m_ResourceManager.GetString("LoadMovieProgress_Infos", Thread.CurrentThread.CurrentUICulture)
                                                        + " " + iValue + " / ~" + iTotal;
                }
            }
        }
        
        private void bgMovieLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Handling of the result of the load is done in DirectLoad() of CommandLoadMovie
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (bgMovieLoader.IsBusy)
            {
                //-------------------------
                // Demande la cancellation.
                //-------------------------
                bgMovieLoader.CancelAsync();
            }
        }
   
    }
}
