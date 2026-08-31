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
        #region Properties
        public bool Cancelled { get; set; } = false;
        #endregion

        #region Members
        private bool showAsPercentage;
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private bool isIdle;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public formProgressBar2(bool isCancellable, bool showAsPercentage, DoWorkEventHandler workEventHandler)
        {
            this.showAsPercentage = showAsPercentage;
            
            InitializeComponent();
            Application.Idle += Application_Idle;
            btnCancel.Visible = isCancellable;
            
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += workEventHandler;
            backgroundWorker.ProgressChanged += ProgressChanged;
            backgroundWorker.RunWorkerCompleted += WorkCompleted;
            
            // Start the work on form load.
            this.Load += (s,e) => backgroundWorker.RunWorkerAsync();
            
            this.Text = "   " + ScreenManagerLang.FormProgressBar_Title;
            labelInfos.Text = ScreenManagerLang.FormFileSave_Infos + " 0%";
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!isIdle)
                return;
            
            isIdle = false;
            
            int total = (int)e.UserState;
            int value = Math.Max(0, Math.Min(e.ProgressPercentage, total));
            
            progressBar.Maximum = total;
            progressBar.Value = value;

            if(showAsPercentage)
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
            Application.Idle -= Application_Idle;
        }
        private void Application_Idle(object sender, EventArgs e)
        {
            isIdle = true;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // This will switch to the UI thread but it will still be freezed as it's a modal dialog.
            // Any cleanup must be done directly in the background thread upon detecting cancellation.
            btnCancel.Enabled = false;
            Cancelled = true;
            backgroundWorker.CancelAsync();
        }
    }
}
