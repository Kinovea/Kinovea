#region License
/*
Copyright � Joan Charmant 2011.
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
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Services;

namespace Kinovea.Root
{
    /// <summary>
    /// FormPreferences2. A dynamically generated form to display preferences.
    /// It is a host for preferences pages.
    /// Preferences pages are UserControl conforming to a IPreferencePanel interface.
    /// The pages should be of size 432; 236 with white background.
    /// 
    /// _initPage can be passed to the constructor to directly load a specific page.
    /// </summary>
    public partial class FormPreferences2 : Form
    {
        #region Members
        private List<UserControl> pages = new List<UserControl>();
        private List<PreferencePanelButtton> buttons = new List<PreferencePanelButtton>();
        private int activePage;
        private static readonly int defaultPage = 0;
        #endregion
        
        #region Construction and Initialization
        public FormPreferences2()
        {
            InitializeComponent();
            
            this.Text = "   " + RootLang.dlgPreferences_Title;
            btnSave.Text = RootLang.Generic_Save;
            btnCancel.Text = RootLang.Generic_Cancel;

            int initPage = PreferencesManager.GeneralPreferences.PreferencesTab;

            ImportPages();
            DisplayPage(initPage);
        }
        private void ImportPages()
        {
            //-----------------------------------------------------------------------------------------------------------------
            // Note on architecture:
            // Apparently SharpDevelop designer has trouble loading classes that are not directly deriving from UserControl.
            // Ideally we would have had an "AbstractPreferencePanel" as a generic class and used this everywhere.
            // Unfortunately, in this case, #Develop wouldn't let us graphically design the individual panels.
            // To work around this and still retain the designer, we use UserControl as the base class.
            // Each panel should implement the IPreferencePanel interface to conform to the architecture.
            // 
            // To create a new Preference page: Add a new file from UserControl template, add IPreferencePanel as an interface.
            // Implement the functions and finally add it to the list here.
            //-----------------------------------------------------------------------------------------------------------------
            pages.Add(new PreferencePanelGeneral());
            pages.Add(new PreferencePanelPlayer());
            pages.Add(new PreferencePanelDrawings());
            pages.Add(new PreferencePanelCapture());
            pages.Add(new PreferencePanelKeyboard());
            AddPages();
        }
        private void AddPages()
        {
            pnlButtons.Controls.Clear();
            
            int nextLeft = 0;
            for(int i=0;i<pages.Count;i++)
            {
                IPreferencePanel page = pages[i] as IPreferencePanel;
                if (page == null)
                    continue;
                
                // Button
                PreferencePanelButtton ppb = new PreferencePanelButtton(page);
                ppb.Click += new EventHandler(preferencePanelButton_Click);
                    
                ppb.Left = nextLeft;
                nextLeft += ppb.Width;
                    
                pnlButtons.Controls.Add(ppb);
                buttons.Add(ppb);
                    
                // Page
                page.Location = new Point(14, this.pnlButtons.Bottom + 14);
                page.Visible = false;
                this.Controls.Add((UserControl)page);
            }
        }
        #endregion
        
        #region Save & Cancel Handlers
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Ask each page to commit its changes to the PreferencesManager.
            foreach(IPreferencePanel page in pages)
                page.CommitChanges();
            
            PreferencesManager.Save();
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            PreferencesManager.Save();
            Close();
        }
        #endregion
        
        #region Private Methods
        private void preferencePanelButton_Click(object sender, EventArgs e)
        {
            // A preference page button has been clicked.
            // Activate the button and load the page.
            PreferencePanelButtton selectedButton = sender as PreferencePanelButtton;
            if (selectedButton == null)
                return;
            
            foreach(PreferencePanelButtton pageButton in buttons)
            {
                pageButton.SetSelected(pageButton == selectedButton);
            }
            
            LoadPage(selectedButton);
        }
        private void DisplayPage(int page)
        {
            // This function can be used to directly load the pref dialog on a specific page.
            int pageToDisplay = defaultPage;
            
            if(page > 0 && page < pages.Count)
                pageToDisplay = page;
            
            activePage = pageToDisplay;
            for(int i=0;i<pages.Count;i++)
            {
                bool selected = (i == pageToDisplay);
                buttons[i].SetSelected(selected);	
                pages[i].Visible = selected;
            }
        }
        private void LoadPage(PreferencePanelButtton button)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                if (button.PreferencePanel == pages[i])
                    PreferencesManager.GeneralPreferences.PreferencesTab = i;

                pages[i].Visible = (pages[i] == button.PreferencePanel);
            }
        }
        #endregion
    }
}
