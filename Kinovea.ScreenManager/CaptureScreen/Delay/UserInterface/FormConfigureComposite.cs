#region License
/*
Copyright © Joan Charmant 2012.
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
using System.IO;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class FormConfigureComposite : Form
    {
        public DelayCompositeConfiguration Configuration
        {
            get { return GetConfiguration(); }
        }

        private DelayCompositeConfiguration currentConfiguration;
        private Dictionary<DelayCompositeType, UserControl> configurationPanels = new Dictionary<DelayCompositeType, UserControl>();

        public FormConfigureComposite(DelayCompositeConfiguration current)
        {
            this.currentConfiguration = current;

            InitializeComponent();
            InitializeUI();
        }
        private void InitializeUI()
        {
            //this.Text = ScreenManagerLang.FormConfigureComposite_Title;
            this.Text = "Configure delay mode";
            gbConfiguration.Text = ScreenManagerLang.Generic_Configuration;
            rbDelay.Text = "Delay";
            rbSlowMotion.Text = "Slow motion";
            rbQuadrants.Text = "Quadrants";
            
            int currentType = (int)currentConfiguration.CompositeType;

            switch (currentConfiguration.CompositeType)
            {
                case DelayCompositeType.SlowMotion:
                    rbSlowMotion.Checked = true;
                    break;
                case DelayCompositeType.MultiReview:
                    rbQuadrants.Checked = true;
                    break;
                default:
                case DelayCompositeType.Basic:
                    rbDelay.Checked = true;
                    break;
            }
        }

        private DelayCompositeConfiguration GetConfiguration()
        {
            DelayCompositeConfiguration config = PreferencesManager.CapturePreferences.DelayCompositeConfiguration;

            if (rbSlowMotion.Checked)
                config.CompositeType = DelayCompositeType.SlowMotion;
            else if (rbQuadrants.Checked)
                config.CompositeType = DelayCompositeType.MultiReview;
            else
                config.CompositeType = DelayCompositeType.Basic;
            
            return config;
        }
        
        private void BtnOKClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
