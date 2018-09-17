/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
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
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class FormFramesExport : Form
    {
        private PlayerScreenUserInterface m_psui;
        private string m_FilePath;
        private long m_iIntervalTimeStamps;
        private bool m_bKeyframesOnly;
        private bool m_IsIdle = true;
        private int m_iEstimatedTotal;

        public FormFramesExport(PlayerScreenUserInterface _psui, string _FilePath, long _iIntervalTimeStamps, bool _bKeyframesOnly, int _iEstimatedTotal)
        {
            InitializeComponent();

            m_psui = _psui;
            m_FilePath = _FilePath;
            m_iIntervalTimeStamps = _iIntervalTimeStamps;
            m_bKeyframesOnly = _bKeyframesOnly;
            m_iEstimatedTotal = _iEstimatedTotal;
    
            this.Text = "   " + ScreenManagerLang.FormFramesExport_Title;
            labelInfos.Text = ScreenManagerLang.FormFramesExport_Infos + " 0 / ~?";

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            Application.Idle += new EventHandler(this.IdleDetector);
        }
        private void formFramesExport_Load(object sender, EventArgs e)
        {
            //-----------------------------------
            // Le Handle existe, on peut y aller.
            //-----------------------------------
            DoExport();
        }
        private void IdleDetector(object sender, EventArgs e)
        {
            m_IsIdle = true;
        }
        public void DoExport()
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
            m_psui.SaveImageSequence(bgWorker, m_FilePath, m_iIntervalTimeStamps, m_bKeyframesOnly, m_iEstimatedTotal);

            e.Result = 0;
        }
        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //--------------------------------------------------------------------------------
            // Cette fonction s'execute dans le thread local. 
            // On a le droit de mettre à jour l'UI.
            //--------------------------------------------------------------------------------

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

                labelInfos.Text = ScreenManagerLang.FormFramesExport_Infos + " " + iValue + " / ~" + iTotal;
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
    }
}