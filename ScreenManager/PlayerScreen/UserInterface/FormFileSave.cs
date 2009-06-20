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
using System.Data;
using System.Drawing;
using System.Text;
using System.Resources;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using Kinovea.VideoFiles;

namespace Kinovea.ScreenManager 
{
    public partial class formFileSave : Form
    {
        #region Members
        private PlayerServer m_PlayerServer = null;
        private Int64 m_iSelStart;
        private Int64 m_iSelEnd;
        private String m_FilePath;
        private Metadata m_Metadata;
        private int m_iFramesInterval;
        private bool m_IsIdle = true;
        private ResourceManager m_ResourceManager;
        private bool m_bFlushDrawings;
        private bool m_bKeyframesOnly;
        //private DelegateFlushDrawings m_DelegateFlushDrawings;
        private DelegateGetOutputBitmap m_DelegateOutputBitmap;
        #endregion

        public formFileSave(PlayerServer _PlayerServer, String _FilePath, int _iFramesInterval, Int64 _iSelStart, Int64 _iSelEnd, Metadata _metadata, bool _bFlushDrawings, bool _bKeyframesOnly, DelegateGetOutputBitmap _DelegateOutputBitmap)
        {
            InitializeComponent();

            m_PlayerServer = _PlayerServer;
            m_iSelStart = _iSelStart;
            m_iSelEnd = _iSelEnd;
            m_FilePath = _FilePath;
            m_Metadata = _metadata;
            m_iFramesInterval = _iFramesInterval;
            m_bFlushDrawings = _bFlushDrawings;
            m_bKeyframesOnly = _bKeyframesOnly;
            //m_DelegateFlushDrawings = _DelegateFlushDrawings;
            m_DelegateOutputBitmap = _DelegateOutputBitmap;

            m_ResourceManager = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());

            this.Text = "   " + m_ResourceManager.GetString("FormFileSave_Title", Thread.CurrentThread.CurrentUICulture);
            labelInfos.Text = m_ResourceManager.GetString("FormFileSave_Infos", Thread.CurrentThread.CurrentUICulture) + " 0%";

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            Application.Idle += new EventHandler(this.IdleDetector);
        }

        private void formFileSave_Load(object sender, EventArgs e)
        {
            //-----------------------------------
            // Le Handle existe, on peut y aller.
            //-----------------------------------
            DoSave();
        }

        private void IdleDetector(object sender, EventArgs e)
        {
            m_IsIdle = true;
        }

        public void DoSave()
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
            // L'appel ici est synchrone, (on ne ressort que quand on a fini) 
            // mais on peut remonter de l'information par bgWorker_ProgressChanged().
            //-------------------------------------------------------------
            m_PlayerServer.m_bgWorker = bgWorker;
            if (m_Metadata == null)
            {
                m_PlayerServer.SaveMovie(m_FilePath, m_iFramesInterval, m_iSelStart, m_iSelEnd, "", m_bFlushDrawings, m_bKeyframesOnly, m_DelegateOutputBitmap);
            }
            else
            {
                m_PlayerServer.SaveMovie(m_FilePath, m_iFramesInterval, m_iSelStart, m_iSelEnd, m_Metadata.ToXmlString(), m_bFlushDrawings, m_bKeyframesOnly, m_DelegateOutputBitmap);
            }
            e.Result = 0;
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //--------------------------------------------------------------------------------
            // Problème possible : 
            // Le worker thread va vouloir mettre à jour les données très souvent.
            // Comme le traitement est asynchrone,il se peut qu'il poste des ReportProgress()
            // plus vite qu'ils ne soient traités ici, à cause du repaint de la progressbar.
            // Il faut donc attendre que la form soit idle.
            //--------------------------------------------------------------------------------
            if (m_IsIdle)
            {
                m_IsIdle = false;

                int iTotal = (int)e.UserState;
                int iValue = (int)e.ProgressPercentage;

                if (iValue > iTotal) { iValue = iTotal; }

                progressBar.Maximum = iTotal;
                progressBar.Value = iValue;

                int iPercentage = (iValue * 100) / iTotal;

                labelInfos.Text = m_ResourceManager.GetString("FormFileSave_Infos", Thread.CurrentThread.CurrentUICulture) + " " + iPercentage + "%";
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
            // Le Close(); sera fait par le dispose() de l'appelant.
        }
    }
}