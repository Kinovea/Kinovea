#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Root.Languages;
using Kinovea.Root.Properties;
using Kinovea.ScreenManager;
using Kinovea.Services;

namespace Kinovea.Root
{
    /// <summary>
    /// Description of PreferencePanelDrawings.
    /// </summary>
    public partial class PreferencePanelDrawings : UserControl, IPreferencePanel
    {
        #region IPreferencePanel properties
        public string Description
        {
            get { return description;}
        }
        public Bitmap Icon
        {
            get { return icon;}
        }
        #endregion
        
        #region Members
        private string description;
        private Bitmap icon;
        private InfosFading defaultFading;
        private bool drawOnPlay;
        #endregion
        
        #region Construction & Initialization
        public PreferencePanelDrawings()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            
            description = RootLang.dlgPreferences_btnDrawings;
            icon = Resources.drawings;
            
            ImportPreferences();
            InitPage();
        }
        private void ImportPreferences()
        {
            drawOnPlay = PreferencesManager.PlayerPreferences.DrawOnPlay;
            defaultFading = new InfosFading(0, 0);
        }
        private void InitPage()
        {
            tabGeneral.Text = RootLang.dlgPreferences_ButtonGeneral;
            chkDrawOnPlay.Text = RootLang.dlgPreferences_chkDrawOnPlay;
            
            tabPersistence.Text = RootLang.dlgPreferences_grpPersistence;
            chkEnablePersistence.Text = RootLang.dlgPreferences_chkEnablePersistence;
            chkAlwaysVisible.Text = RootLang.dlgPreferences_chkAlwaysVisible;
            
            chkDrawOnPlay.Checked = drawOnPlay;
            chkEnablePersistence.Checked = defaultFading.Enabled;
            trkFading.Maximum = PreferencesManager.PlayerPreferences.MaxFading;
            trkFading.Value = Math.Min(defaultFading.FadingFrames, trkFading.Maximum);
            chkAlwaysVisible.Checked = defaultFading.AlwaysVisible;
            EnableDisableFadingOptions();
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);
        }
        #endregion
        
        #region Handlers
        #region General
        private void chkDrawOnPlay_CheckedChanged(object sender, EventArgs e)
        {
            drawOnPlay = chkDrawOnPlay.Checked;
        }
        #endregion
        
        #region Persistence
        private void chkFading_CheckedChanged(object sender, EventArgs e)
        {
            defaultFading.Enabled = chkEnablePersistence.Checked;
            EnableDisableFadingOptions();
        }
        private void trkFading_ValueChanged(object sender, EventArgs e)
        {
            lblFading.Text = String.Format(RootLang.dlgPreferences_lblFading, trkFading.Value);
            defaultFading.FadingFrames = trkFading.Value;
            chkAlwaysVisible.Checked = false;
        }
        private void chkAlwaysVisible_CheckedChanged(object sender, EventArgs e)
        {
            defaultFading.AlwaysVisible = chkAlwaysVisible.Checked;
        }
        #endregion
        #endregion
        
        private void EnableDisableFadingOptions()
        {
            trkFading.Enabled = chkEnablePersistence.Checked;
            lblFading.Enabled = chkEnablePersistence.Checked;
            chkAlwaysVisible.Enabled = chkEnablePersistence.Checked;
        }
    
        public void CommitChanges()
        {
            PreferencesManager.PlayerPreferences.DrawOnPlay = drawOnPlay;
            PreferencesManager.PlayerPreferences.DefaultFading.FromInfosFading(defaultFading);
        }
    }
}
