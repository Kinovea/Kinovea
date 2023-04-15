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
    /// FormProgressBar is a simple form to display a progress bar.
    /// The progress is computed outside and communicated through Update() method.
    /// </summary>
    public partial class FormProgressBar : Form
    {
        #region Callbacks
        public event EventHandler CancelAsked;
        #endregion
        
        #region Members
        private bool isIdle;
        private bool isCancelling;
        #endregion
        
        #region Constructor
        public FormProgressBar(bool isCancellable)
        {
            InitializeComponent();
            Application.Idle += (s, e) => isIdle = true;
            btnCancel.Visible = isCancellable;
            this.Text = "   " + ScreenManagerLang.FormProgressBar_Title;
            labelInfo.Text = "0";
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        #endregion	
        
        public void Reset()
        {
            labelInfo.Text = "0";
            progressBar.Maximum = 100;
            progressBar.Value = 0;
        }

        public void Update(int value, int maximum, bool showAsPercentage)
        {
            if (!isIdle || isCancelling)
                return;

            isIdle = false;

            progressBar.Maximum = maximum;
            progressBar.Value = Math.Min(Math.Max(value, 0), maximum);

            string info;
            if (showAsPercentage)
            {
                float fraction = (float)value / maximum;
                int percent = (int)Math.Floor(fraction * 100.0f);
                info = string.Format("{0}%", percent);
            }
            else
            {
                info = string.Format("{0}/{1}", value, maximum);
            }

            labelInfo.Text = info;
        }
        
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // User clicked on cancel, trigger the callback that will cancel the ongoing operation.
            btnCancel.Enabled = false;
            isCancelling = true;
            CancelAsked?.Invoke(this, EventArgs.Empty);	
        }
    }
}
