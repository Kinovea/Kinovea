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
        private TrackingProfile trackingProfile;
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
            //defaultFading = PreferencesManager.PlayerPreferences.DefaultFading;
            trackingProfile = PreferencesManager.PlayerPreferences.TrackingProfile;
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

            tabTracking.Text = "Tracking";
            lblObjectWindow.Text = "Object window :";
            lblSearchWindow.Text = "Search window :";
            cmbBlockWindowUnit.Items.Add("Percentage");
            cmbBlockWindowUnit.Items.Add("Pixels");
            cmbSearchWindowUnit.Items.Add("Percentage");
            cmbSearchWindowUnit.Items.Add("Pixels");

            //------------
            int blockWindowUnit = (int)trackingProfile.BlockWindowUnit;
            cmbBlockWindowUnit.SelectedIndex = blockWindowUnit < cmbBlockWindowUnit.Items.Count ? blockWindowUnit : 0;

            int searchWindowUnit = (int)trackingProfile.SearchWindowUnit;
            cmbSearchWindowUnit.SelectedIndex = searchWindowUnit < cmbSearchWindowUnit.Items.Count ? searchWindowUnit : 0;

            tbBlockWidth.Text = trackingProfile.BlockWindow.Width.ToString();
            tbBlockHeight.Text = trackingProfile.BlockWindow.Height.ToString();
            tbSearchWidth.Text = trackingProfile.SearchWindow.Width.ToString();
            tbSearchHeight.Text = trackingProfile.SearchWindow.Height.ToString();
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

        #region Tracking
        private void tbBlockWidth_TextChanged(object sender, EventArgs e)
        {
            int width;
            bool parsed = ExtractTrackerParameter(tbBlockWidth, out width);
            if (!parsed)
                return;

            trackingProfile.BlockWindow = new Size(width, trackingProfile.BlockWindow.Height);
        }

        private void tbBlockHeight_TextChanged(object sender, EventArgs e)
        {
            int height;
            bool parsed = ExtractTrackerParameter(tbBlockHeight, out height);
            if (!parsed)
                return;

            trackingProfile.BlockWindow = new Size(trackingProfile.BlockWindow.Width, height);
        }

        private void tbSearchWidth_TextChanged(object sender, EventArgs e)
        {
            int width;
            bool parsed = ExtractTrackerParameter(tbSearchWidth, out width);
            if (!parsed)
                return;

            trackingProfile.SearchWindow = new Size(width, trackingProfile.SearchWindow.Height);
        }

        private void tbSearchHeight_TextChanged(object sender, EventArgs e)
        {
            int height;
            bool parsed = ExtractTrackerParameter(tbSearchHeight, out height);
            if (!parsed)
                return;

            trackingProfile.SearchWindow = new Size(trackingProfile.SearchWindow.Width, height);
        }

        private bool ExtractTrackerParameter(TextBox tb, out int value)
        {
            int v;
            bool parsed = int.TryParse(tb.Text, out v);
            tbBlockWidth.ForeColor = parsed ? Color.Black : Color.Red;
            value = parsed ? v : 10;
            return parsed;
        }

        private void cmbBlockWindowUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackingProfile.BlockWindowUnit = (TrackerParameterUnit)cmbBlockWindowUnit.SelectedIndex;
        }

        private void cmbSearchWindowUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            trackingProfile.SearchWindowUnit = (TrackerParameterUnit)cmbSearchWindowUnit.SelectedIndex;
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
            PreferencesManager.PlayerPreferences.TrackingProfile = trackingProfile;
        }

        

        

        

        

        

        
    }
}
