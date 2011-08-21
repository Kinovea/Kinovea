/*
Copyright � Joan Charmant 2008.
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
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
	/// <summary>
	/// A dialog to help the user on what to save and how.
	/// </summary>
    public partial class formVideoExport : Form
    {
    	#region Properties
		public bool SaveAnalysis
		{
			get { return m_bSaveAnalysis; }
		}
		public bool BlendDrawings
		{
			get { return m_bBlendDrawings; }
		}
		public bool MuxDrawings
		{
			get { return m_bMuxDrawings; }
		}
		public string Filename
		{
			get { return m_Filename; }
		}    	
		public bool UseSlowMotion
		{
			get { return m_bUseSlowMotion; }
		}
		#endregion
    	    	
    	#region Members
    	//private PlayerScreen m_PlayerScreen;
        
    	private string m_OriginalFilename;
    	private double m_fSlowmotionPercentage;
		private Metadata m_Metadata;
    	
        private bool m_bSaveAnalysis;
		private bool m_bBlendDrawings;
		private bool m_bMuxDrawings;
		private bool m_bUseSlowMotion;
		private string m_Filename;
        #endregion
        
		#region constructor and initialisation
		public formVideoExport(string _OriginalFilename, Metadata _Metadata, double _fSlowmotionPercentage)
        {
        	m_fSlowmotionPercentage = _fSlowmotionPercentage;
        	m_Metadata = _Metadata;
        	m_OriginalFilename = _OriginalFilename;
        	
            InitializeComponent();
            
            if(m_fSlowmotionPercentage == 100)
            {
            	groupOptions.Visible = false;
            	this.Height -= groupOptions.Height;
            }
            
            InitCulture();
        }
		private void InitCulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgSaveAnalysisOrVideo_Title;
            groupSaveMethod.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_GroupSaveMethod;            
            radioSaveMuxed.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioMuxed;
            tbSaveMuxed.Lines = ScreenManagerLang.dlgSaveAnalysisOrVideo_HintMuxed.Split('#');
            radioSaveBlended.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioBlended;
			tbSaveBlended.Lines = ScreenManagerLang.dlgSaveAnalysisOrVideo_HintBlended.Split('#');
            radioSaveAnalysis.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_RadioAnalysis;
            tbSaveAnalysis.Lines = ScreenManagerLang.dlgSaveAnalysisOrVideo_HintAnalysis.Split('#');
            
            groupOptions.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_GroupOptions;
            checkSlowMotion.Text = ScreenManagerLang.dlgSaveAnalysisOrVideo_CheckSlow;
            checkSlowMotion.Text = checkSlowMotion.Text + m_fSlowmotionPercentage.ToString() + "%).";

            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;

            EnableDisableOptions();

            // default option
            radioSaveMuxed.Checked = true;
        }
		#endregion
        
		#region Public methods
		public DialogResult Spawn()
		{
			// We use this method instead of directly calling ShowDialog()
			// in order to catch for the special case where the user has no choice.
			if(!m_Metadata.HasData && m_fSlowmotionPercentage == 100)
			{
				// Directly ask for a filename.
				m_bBlendDrawings = true;
				m_bMuxDrawings = false;
                m_bUseSlowMotion = false;
				return DoSaveVideo();
			}
			else
			{
				return ShowDialog();
			}
		}
		#endregion
		
		#region event handlers
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Hide/Close logic:
            // We start by hiding the current dialog.
            // If the user cancels on the file choosing dialog, we show back ourselves.

            Hide();
			DialogResult dr;
			
			m_bBlendDrawings = false;
            m_bMuxDrawings = false;
            m_bSaveAnalysis = false;
            m_bUseSlowMotion = false;
            
            if(radioSaveAnalysis.Checked)
            {
                m_bSaveAnalysis = true;
                dr = DoSaveAnalysis();
            }
            else
            {
                m_bBlendDrawings = radioSaveBlended.Checked;
                m_bMuxDrawings = radioSaveMuxed.Checked;
                m_bUseSlowMotion = checkSlowMotion.Checked;
            	dr = DoSaveVideo();	
            }
            
            if(dr == DialogResult.OK)
            {
            	Close();
            }
            else 
            {
            	//If cancelled, we display the wizard again.
                Show();	
            }
        }
        private void BtnSaveAnalysisClick(object sender, EventArgs e)
        {
        	UncheckAllOptions();
        	radioSaveAnalysis.Checked = true;	
        }
        private void BtnSaveMuxedClick(object sender, EventArgs e)
        {
        	UncheckAllOptions();
        	radioSaveMuxed.Checked = true;
        }
        private void BtnSaveBothClick(object sender, EventArgs e)
        {
        	UncheckAllOptions();
        	radioSaveBlended.Checked = true;
        }
        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableOptions();
        }
        #endregion
        
        #region lower levels helpers
        private void EnableDisableOptions()
        {
            radioSaveMuxed.Enabled = true;
            radioSaveBlended.Enabled = true;
			radioSaveAnalysis.Enabled = true;            
			checkSlowMotion.Enabled = radioSaveAnalysis.Checked ? false : (m_fSlowmotionPercentage != 100);
        }
        private void UncheckAllOptions()
        {
            radioSaveAnalysis.Checked = false;
            radioSaveMuxed.Checked = false;
            radioSaveBlended.Checked = false;	
        }
    
        private DialogResult DoSaveVideo()
        {
            //--------------------------------------------------------------------------
            // Save Video file. (Either Alone or along with the Analysis muxed into it.)
            //--------------------------------------------------------------------------
			DialogResult result = DialogResult.Cancel;
            string filePath = null;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveVideoTitle;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;

            if (radioSaveMuxed.Checked)
            {
                saveFileDialog.Filter = ScreenManagerLang.dlgSaveVideoFilterMuxed;
            }
            else
            {
                saveFileDialog.Filter = ScreenManagerLang.dlgSaveVideoFilterAlone;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
                if (filePath.Length > 0)
                {
                	m_Filename = filePath;                	
                	DialogResult = DialogResult.OK;
					result = DialogResult.OK;
                }
        	}
            return result;
        }
        private DialogResult DoSaveAnalysis()
        {
            // Analysis only.
            DialogResult result = DialogResult.Cancel;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = ScreenManagerLang.dlgSaveAnalysisTitle;

            // Goto this video directory and suggest filename for saving.
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(m_OriginalFilename);
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(m_OriginalFilename);
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
                    
                    // Commit output props
                    m_Filename = filePath;
                    DialogResult = DialogResult.OK;
                    result = DialogResult.OK;
                }
            }
            
            return result;
        }
    	#endregion
    }
}