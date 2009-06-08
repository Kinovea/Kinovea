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
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Videa.ScreenManager;
using Videa.Services;

namespace Videa.Root
{
    public partial class HelpVideosDialog : Form
    {
        #region Members
        private ResourceManager m_ResourceManager;
        private HelpIndex m_HelpIndex;
        private ScreenManagerKernel m_ScreenManagerKernel;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public HelpVideosDialog(ResourceManager resManager, HelpIndex helpIndex, ScreenManagerKernel smk)
        {
            InitializeComponent();

            m_ResourceManager = resManager;
            m_HelpIndex = helpIndex;
            m_ScreenManagerKernel = smk;

            InitializeInterface();
        }
        #endregion

        #region EventHandlers
        private void btnWatch_Click(object sender, EventArgs e)
        {
            Hide();
            LaunchVideo((HelpItem)lstVideos.Items[lstVideos.SelectedIndex]);
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Setup Interface
        public void InitializeInterface()
        {
            // Localized strings and Video list.

            // Titre Fenetre principale.
            this.Text = "   " + m_ResourceManager.GetString("dlgHelpVideos_Title", Thread.CurrentThread.CurrentUICulture);

            //----------------------------
            // Labels Statiques Communs
            //----------------------------
            btnCancel.Text = m_ResourceManager.GetString("Generic_Quit", Thread.CurrentThread.CurrentUICulture);
            btnWatch.Text = m_ResourceManager.GetString("dlgHelpVideos_Watch", Thread.CurrentThread.CurrentUICulture);
            lblSelectVideos.Text = m_ResourceManager.GetString("dlgHelpVideos_LblSelectVideos", Thread.CurrentThread.CurrentUICulture);
            lblAboutThisVideo.Text = m_ResourceManager.GetString("dlgHelpVideos_LblAboutThisVideo", Thread.CurrentThread.CurrentUICulture);
            lblInstructionGetMore.Text = m_ResourceManager.GetString("dlgHelpVideos_LabelGetMore", Thread.CurrentThread.CurrentUICulture);
            lblFilterByLanguage.Text = m_ResourceManager.GetString("dlgHelpVideos_LblFilterByLang", Thread.CurrentThread.CurrentUICulture);
            PopulateFilterComboBox();

            if (cmbLanguageFilter.SelectedIndex >= 0)
            {
                string szIsoLang = ((LanguageIdentifier)cmbLanguageFilter.Items[cmbLanguageFilter.SelectedIndex]).szTwoLetterISOLanguageName;
                PopulateListBox(lstVideos, m_HelpIndex.HelpVideos, szIsoLang);
            }
            
        }
        private void PopulateFilterComboBox()
        {
            cmbLanguageFilter.Items.Clear();

            LanguageIdentifier liAll = new LanguageIdentifier("", m_ResourceManager.GetString("dlgHelpVideos_FilterAll", Thread.CurrentThread.CurrentUICulture));
            LanguageIdentifier liEnglish = new LanguageIdentifier("en", PreferencesManager.LanguageEnglish);
            LanguageIdentifier liFrench = new LanguageIdentifier("fr", PreferencesManager.LanguageFrench);

            cmbLanguageFilter.Items.Add(liAll);
            cmbLanguageFilter.Items.Add(liEnglish);
            cmbLanguageFilter.Items.Add(liFrench);

            cmbLanguageFilter.SelectedIndex = 0;
        }
        private void PopulateListBox(ListBox _lstbox, List<HelpItem> _Videos, string _szTwoLetterISOLanguageName)
        {
            _lstbox.Items.Clear();
            foreach (HelpItem Item in _Videos)
            {
                // Les description dans le système d'aide et dans le système d'update
                // peuvent être différentes.
                // On les reconstruit à chaque fois qu'on en a besoin en fc des infos.

                Item.Description = Item.LocalizedTitle;

                // Filtre langue
                if (_szTwoLetterISOLanguageName.Length == 0 || Item.Language == _szTwoLetterISOLanguageName)
                {
                    _lstbox.Items.Add(Item);
                }
            }

            if (_lstbox.Items.Count > 0)
            {
                lstVideos.SelectedIndex = 0;
            }
            else
            {
                rtbVideoComment.Text = m_ResourceManager.GetString("dlgHelpVideos_NoComment", Thread.CurrentThread.CurrentUICulture);
            }
        }
        private void cmbLanguageFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Changement de langue.
            if (cmbLanguageFilter.SelectedIndex >= 0)
            {
                string szIsoLang = ((LanguageIdentifier)cmbLanguageFilter.Items[cmbLanguageFilter.SelectedIndex]).szTwoLetterISOLanguageName;
                PopulateListBox(lstVideos, m_HelpIndex.HelpVideos, szIsoLang);
            }
        }
        #endregion

        #region ListBox Handlers
        private void lstVideos_SelectedIndexChanged(object sender, EventArgs e)
        {
        	// Update the comment for this video.
            string szComment = ((HelpItem)lstVideos.Items[lstVideos.SelectedIndex]).Comment;
            rtbVideoComment.Clear();
            if (szComment.Length == 0)
            {
                rtbVideoComment.Text = m_ResourceManager.GetString("dlgHelpVideos_NoComment", Thread.CurrentThread.CurrentUICulture);
            }
            else
            {
                rtbVideoComment.Text = szComment;
            }
        }
        private void LstVideosMouseDoubleClick(object sender, MouseEventArgs e)
        {
        	int index = lstVideos.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
				// Launch Video.
                Hide();
                LaunchVideo((HelpItem)lstVideos.Items[index]);
                Close();
            }	
        }
        #endregion

        #region Launch Video
        private void LaunchVideo(HelpItem _Video)
        {
        	if(File.Exists(_Video.FileLocation))
        	{
	        	//--------------------------------------------------------------------------
	            // CommandLoadMovieInScreen est une commande du ScreenManager.
	            // elle gère la création du screen si besoin, et demande 
	            // si on veut charger surplace ou dans un nouveau en fonction de l'existant.
	            //--------------------------------------------------------------------------
	            IUndoableCommand clmis = new CommandLoadMovieInScreen(m_ScreenManagerKernel, _Video.FileLocation, -1, true);
	            CommandManager cm = CommandManager.Instance();
	            cm.LaunchUndoableCommand(clmis);
        	}
        	else
        	{
        		log.Error(String.Format("Cannot find the video tutorial file. ({0}).", _Video.FileLocation));
        	}
        }
        #endregion

        
        
    }
}