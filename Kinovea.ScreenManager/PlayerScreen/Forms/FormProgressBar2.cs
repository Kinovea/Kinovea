#region License
/*
Copyright © Joan Charmant 2008-2009.
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
#endregion
using System;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple dialog wrapping a progress bar and a background worker.
    /// </summary>
    public partial class formProgressBar2 : Form
    {
        #region Members
        private bool m_Idle;
        private bool m_AsPercentage;
        private EventHandler m_IdleDetector;
        private BackgroundWorker m_bgWorker = new BackgroundWorker();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public formProgressBar2(bool _cancellable, bool _asPercentage, DoWorkEventHandler _doWork)
        {
            m_AsPercentage = _asPercentage;
            m_IdleDetector = (s,e) => m_Idle = true;
            
            InitializeComponent();
            Application.Idle += m_IdleDetector;
            btnCancel.Visible = _cancellable;
            
            m_bgWorker.WorkerReportsProgress = true;
            m_bgWorker.WorkerSupportsCancellation = true;
            m_bgWorker.DoWork += _doWork;
            m_bgWorker.ProgressChanged += ProgressChanged;
            m_bgWorker.RunWorkerCompleted += WorkCompleted;
            this.Load += (s,e) => m_bgWorker.RunWorkerAsync();
            
            this.Text = "   " + ScreenManagerLang.FormProgressBar_Title;
            labelInfos.Text = ScreenManagerLang.FormFileSave_Infos + " 0%";
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!m_Idle)
                return;
            
            m_Idle = false;
            
            int total = (int)e.UserState;
            int value = Math.Max(0, Math.Min(e.ProgressPercentage, total));
            
            progressBar.Maximum = total;
            progressBar.Value = value;

            if(m_AsPercentage)
                labelInfos.Text = String.Format("{0} {1}%", ScreenManagerLang.FormFileSave_Infos, (value * 100) / total);
            else
                labelInfos.Text = String.Format("{0} {1}/{2}", ScreenManagerLang.FormFileSave_Infos, value, total);
        }
        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }
        private void formProgressBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Idle -= m_IdleDetector;
        }
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // This will switch to the UI thread but it will still be freezed as it's a modal dialog.
            // Any cleanup must be done directly in the background thread upon detecting cancellation.
            btnCancel.Enabled = false;
            m_bgWorker.CancelAsync();
        }
    }
}
