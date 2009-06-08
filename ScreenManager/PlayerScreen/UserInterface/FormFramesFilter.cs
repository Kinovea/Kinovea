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
using System.Windows.Forms;
using System.Resources;
using System.Threading;

using VideaPlayerServer;


namespace Videa.ScreenManager
{
    public partial class formFramesFilter : Form
    {
        ResourceManager                 m_ResourceManager   = null;
        PlayerServer                    m_PlayerServer      = null;
        PlayerScreen.ImageFilterType    m_ImageFilterType = PlayerScreen.ImageFilterType.Unknown;
        bool            m_IsIdle            = true;

        public formFramesFilter(ResourceManager _ResourceManager, PlayerServer _PlayerServer, PlayerScreen.ImageFilterType _ImageFilterType)
        {
            InitializeComponent();

            m_ResourceManager   = _ResourceManager;
            m_PlayerServer      = _PlayerServer;
            m_ImageFilterType   = _ImageFilterType;

            this.Text       = "   " + m_ResourceManager.GetString("FormFramesFilter_Title", Thread.CurrentThread.CurrentUICulture);
            labelInfos.Text = m_ResourceManager.GetString("FormFramesFilter_Infos", Thread.CurrentThread.CurrentUICulture)
                                  + " 0 / ~?";

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            Application.Idle += new EventHandler(this.IdleDetector);
        }

        private void formFramesFilter_Load(object sender, EventArgs e)
        {
            //-----------------------------------
            // Le Handle existe, on peut y aller.
            //-----------------------------------
            DoFilter();
        }

        private void IdleDetector(object sender, EventArgs e)
        {
            m_IsIdle = true;
        }

        public void DoFilter()
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
            m_PlayerServer.m_bgWorker = bgWorker;
            
            // Les paramètres du filtre ont déjà été mis en place.
            // 0: Colors, 1: Brightness, 2: Contrast, 3:Sharpen, 4: Mirror.
            switch (m_ImageFilterType)
            {
                case PlayerScreen.ImageFilterType.Colors:
                    m_PlayerServer.FilterImage(0);
                    break;
                case PlayerScreen.ImageFilterType.Brightness:
                    m_PlayerServer.FilterImage(1);
                    break;

                case PlayerScreen.ImageFilterType.Contrast:
                    m_PlayerServer.FilterImage(2);
                    break;

                case PlayerScreen.ImageFilterType.Sharpen:
                    m_PlayerServer.FilterImage(3);
                    break;

                case PlayerScreen.ImageFilterType.Mirror:
                    m_PlayerServer.FilterImage(4);
                    break;

                case PlayerScreen.ImageFilterType.Edges:
                    m_PlayerServer.FilterImage(5);
                    break;

                default:
                    break;
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

                if (iValue > iTotal) { iValue = iTotal; }

                progressBar.Maximum = iTotal;
                progressBar.Value = iValue;

                labelInfos.Text = m_ResourceManager.GetString("FormFramesFilter_Infos", Thread.CurrentThread.CurrentUICulture)
                                  + " " + iValue + " / ~" + iTotal;
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

            Close();
        }
    }
}