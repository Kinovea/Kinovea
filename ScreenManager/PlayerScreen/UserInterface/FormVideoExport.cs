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
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class formVideoExport : Form
    {
        private PlayerScreen m_PlayerScreen;
        private int m_iSlowmotionPercentage;

        public formVideoExport(PlayerScreen _ActiveScreen)
        {
        	m_iSlowmotionPercentage = ((PlayerScreenUserInterface)_ActiveScreen.UI).SlowmotionPercentage;
        	m_PlayerScreen = _ActiveScreen;
        	
            InitializeComponent();
            InitCulture();
        }

        private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgSaveAnalysisOrVideo_Title;
            groupSaveMethod.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_GroupSaveMethod;
            
            radioSaveVideo.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioVideo;
            radioSaveAnalysis.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioAnalysis;
            radioSaveMuxed.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioMuxed;
            radioSaveBoth.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioBoth;


            groupOptions.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_GroupOptions;

            checkSlowMotion.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_CheckSlow;
            checkSlowMotion.Text = checkSlowMotion.Text + m_iSlowmotionPercentage.ToString() + "%).";

            checkBlendDrawings.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_CheckBlend;

            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;

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
            saveFileDialog.Title = ScreenManagerLang.dlgSaveVideoTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;

            // File filter & type of mux.
            bool bVideoAlone;
            if (radioSaveMuxed.Checked)
            {
                saveFileDialog.Filter = ScreenManagerLang.dlgSaveVideoFilterMuxed;
                bVideoAlone = false;
            }
            else
            {
                saveFileDialog.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
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
            saveFileDialog.Title = ScreenManagerLang.dlgSaveAnalysisTitle;


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
            saveFileDialog.Filter = ScreenManagerLang.dlgSaveAnalysisFilter;

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