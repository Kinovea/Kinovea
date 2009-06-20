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
using System.Reflection;
using System.Threading;
using System.IO;

namespace Kinovea.ScreenManager
{
    public partial class formVideoExport : Form
    {
        private PlayerScreen m_PlayerScreen;
        private ResourceManager m_ResourceManager;

        public formVideoExport(PlayerScreen _ActiveScreen)
        {
            InitializeComponent();
            
            ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
            m_ResourceManager = rm;

            m_PlayerScreen = _ActiveScreen;

            InitCulture();
        }

        private void InitCulture()
        {
            this.Text = "   " + m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_Title", Thread.CurrentThread.CurrentUICulture);
            groupSaveMethod.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_GroupSaveMethod", Thread.CurrentThread.CurrentUICulture);
            
            radioSaveVideo.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_RadioVideo", Thread.CurrentThread.CurrentUICulture);
            radioSaveAnalysis.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_RadioAnalysis", Thread.CurrentThread.CurrentUICulture);
            radioSaveMuxed.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_RadioMuxed", Thread.CurrentThread.CurrentUICulture);
            radioSaveBoth.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_RadioBoth", Thread.CurrentThread.CurrentUICulture);


            groupOptions.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_GroupOptions", Thread.CurrentThread.CurrentUICulture);

            checkSlowMotion.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_CheckSlow", Thread.CurrentThread.CurrentUICulture);
            checkSlowMotion.Text = checkSlowMotion.Text + m_PlayerScreen.m_PlayerScreenUI.SlowmotionPercentage.ToString() + "%).";

            checkBlendDrawings.Text = m_ResourceManager.GetString("dlgSaveAnalysisOrVideo_CheckBlend", Thread.CurrentThread.CurrentUICulture);

            btnOK.Text = m_ResourceManager.GetString("Generic_Save", Thread.CurrentThread.CurrentUICulture);
            btnCancel.Text = m_ResourceManager.GetString("Generic_Cancel", Thread.CurrentThread.CurrentUICulture);

            EnableDisableOptions();

            // default option
            // Dépend si on a une analyse ou pas.
            radioSaveVideo.Checked = true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Hide/Close logic:
            // We start by hiding the current dialog.
            // If the user cancels on the video dialog, we show back ourselves.
            // When user cancels at Analysis Dialog, we show back from there. 

            Hide();

            if(!radioSaveAnalysis.Checked)
            {
                // Either save Both or Video alone (including muxed).
                string filePath = DoSaveVideo();
                if (filePath != null)
                {
                    if (radioSaveBoth.Checked)
                    {
                        // If both, reuse the path we just choose.
                        // If canceled, will do a Show(), If not, will Close();
                        DoSaveAnalysis(filePath);
                    }
                    else
                    {
                        // Work over.
                        Close();
                    }
                }
                else
                {
                    Show();
                }
            }
            else
            {
                // If canceled, will do a Show(), If not, will Close();
                DoSaveAnalysis(null);
            }
        }
        private string DoSaveVideo()
        {
            //--------------------------------------------------------------------------
            // Save Video file. (Either Alone or along with the Analysis muxed into it.)
            //--------------------------------------------------------------------------

            string filePath = null;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = m_ResourceManager.GetString("dlgSaveVideoTitle", Thread.CurrentThread.CurrentUICulture);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;

            // File filter & type of mux.
            bool bVideoAlone;
            if (radioSaveMuxed.Checked)
            {
                saveFileDialog.Filter = m_ResourceManager.GetString("dlgSaveVideoFilterMuxed", Thread.CurrentThread.CurrentUICulture);
                bVideoAlone = false;
            }
            else
            {
                saveFileDialog.Filter = m_ResourceManager.GetString("dlgSaveVideoFilterAlone", Thread.CurrentThread.CurrentUICulture);
                bVideoAlone = true;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    m_PlayerScreen.m_PlayerScreenUI.SaveMovie(filePath, bVideoAlone, checkSlowMotion.Checked, checkBlendDrawings.Checked);
                }
                else
                {
                    filePath = null;
                }
            }
            
            return filePath;
        }
        private void DoSaveAnalysis(string _filePath)
        {
            // Analysis only.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = m_ResourceManager.GetString("dlgSaveAnalysisTitle", Thread.CurrentThread.CurrentUICulture);


            if (_filePath == null)
            {
                // Goto this video directory and suggest filename for saving.
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(m_PlayerScreen.FilePath);
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(m_PlayerScreen.FilePath);
            }
            else
            {
                // Reuse the infos from the new video we just saved.
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(_filePath);
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(_filePath);
            }
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.Filter = m_ResourceManager.GetString("dlgSaveAnalysisFilter", Thread.CurrentThread.CurrentUICulture);

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                    if(!filePath.ToLower().EndsWith(".kva") && !filePath.ToLower().EndsWith(".xml"))
                    {
                        filePath = filePath + ".kva";
                    }
                    m_PlayerScreen.m_PlayerScreenUI.Metadata.ToXmlFile(filePath);
                }

                // Work is always over when we come here.
                Close();
            }
            else
            {
                Show();
            }
        }

        private void radioSaveAnalysis_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableOptions();
        }
        private void EnableDisableOptions()
        {
            if (!m_PlayerScreen.m_PlayerScreenUI.Metadata.HasData)
            {
                // Can only save the video.

                radioSaveVideo.Enabled = true;
                radioSaveAnalysis.Enabled = false;
                radioSaveMuxed.Enabled = false;
                radioSaveBoth.Enabled = false;
                checkBlendDrawings.Enabled = false;
                checkSlowMotion.Enabled = (m_PlayerScreen.m_PlayerScreenUI.SlowmotionPercentage != 100);
            }
            else
            {
                // Can save video and/or data

                radioSaveVideo.Enabled = true;
                radioSaveAnalysis.Enabled = true;
                radioSaveMuxed.Enabled = true;
                radioSaveBoth.Enabled = true;

                if (radioSaveAnalysis.Checked)
                {
                    // Only save analysis, blending drawings and slowmotion disabled.

                    checkSlowMotion.Enabled = false;
                    checkBlendDrawings.Enabled = false;
                }
                else
                {
                    // Save video and or analysis, blending and slowmotion enabled

                    checkBlendDrawings.Enabled = true;
                    checkSlowMotion.Enabled = (m_PlayerScreen.m_PlayerScreenUI.SlowmotionPercentage != 100);
                }
            }
        }
    }
}