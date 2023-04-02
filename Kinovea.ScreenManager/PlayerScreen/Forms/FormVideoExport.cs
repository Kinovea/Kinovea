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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A dialog to export video with drawings painted on.
    /// Note: this dialog used to have many more options like saving KVA or saving a video with the KVA as a subtitle track. 
    /// It's now exclusively used to export video with drawings painted on.
    /// </summary>
    public partial class formVideoExport : Form
    {
        #region Properties
        public string Filename
        {
            get { return filename; }
        }    	
        public bool UseSlowMotion
        {
            get { return useSlomo; }
        }
        #endregion
                
        #region Members
        private string originalFilename;
        private double slomoPercentage;
        private bool useSlomo;
        private string filename;
        #endregion
        
        public formVideoExport(string originalFilename, double slomoPercentage)
        {
            this.slomoPercentage = slomoPercentage;
            this.originalFilename = originalFilename;
            InitializeComponent();
            InitCulture();
        }

        private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.CommandExportVideo_FriendlyName;
            lblVideoExport.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioBlended;
            tbSaveBlended.Lines = ScreenManagerLang.dlgSaveAnalysisOrVideo_HintBlended.Split('#');
            checkSlowMotion.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_CheckSlow;
            checkSlowMotion.Text = checkSlowMotion.Text + string.Format("{0:0.00} %).", slomoPercentage);

            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;

            checkSlowMotion.Enabled = (slomoPercentage != 100);
        }
        
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Hide/Close logic:
            // We start by hiding the current dialog and show the filename selection dialog.
            // If the user cancels on the filename selection, we show back ourselves.
            // If the user OKs on the filename selection, we close and the caller will read the dialog result and perform the actual saving.
            Hide();
            useSlomo = checkSlowMotion.Checked;
            
            DialogResult dr = SelectFilename();
            if(dr == DialogResult.OK)
                Close();
            else 
                Show();	
        }
        
        private DialogResult SelectFilename()
        {
            DialogResult result = DialogResult.Cancel;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.CommandExportVideo_FriendlyName;
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(originalFilename);
            saveFileDialog.Filter = FilesystemHelper.SaveVideoFilter();
            saveFileDialog.FilterIndex = FilesystemHelper.GetFilterIndex(saveFileDialog.Filter, PreferencesManager.PlayerPreferences.VideoFormat);

            if (saveFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                filename = saveFileDialog.FileName;
                DialogResult = DialogResult.OK;
                result = DialogResult.OK;
            }

            return result;
        }
    }
}