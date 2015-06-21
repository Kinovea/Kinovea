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
            this.Text = "Configure delay composite";

            cbType.Items.Add("Basic");
            cbType.Items.Add("Multiple reviews");
            cbType.Items.Add("Slow motion");
            cbType.Items.Add("Frozen mosaic");
            cbType.Items.Add("Mixed");
            
            // Each panel has its own configuration instance, initialized with default values.
            // The panel of the current configuration is populated with the current data.
            configurationPanels.Add(DelayCompositeType.MultiReview, new DelayCompositeMultiReviewControl(currentConfiguration));
            configurationPanels.Add(DelayCompositeType.SlowMotion, new DelayCompositeSlowMotionControl(currentConfiguration));
            configurationPanels.Add(DelayCompositeType.FrozenMosaic, new DelayCompositeFrozenMosaicControl(currentConfiguration));
            
            int currentType = (int)currentConfiguration.CompositeType;
            cbType.SelectedIndex = (currentType < cbType.Items.Count) ? currentType : 0;
        }

        private DelayCompositeConfiguration GetConfiguration()
        {
            DelayCompositeConfiguration config = new DelayCompositeConfiguration();
            DelayCompositeType t = (DelayCompositeType)cbType.SelectedIndex;
            config.CompositeType = t;

            switch (t)
            {
                case DelayCompositeType.MultiReview:
                    DelayCompositeMultiReviewControl dcmrc = configurationPanels[DelayCompositeType.MultiReview] as DelayCompositeMultiReviewControl;
                    return dcmrc.Configuration;
                case DelayCompositeType.SlowMotion:
                    DelayCompositeSlowMotionControl dcsmc = configurationPanels[DelayCompositeType.SlowMotion] as DelayCompositeSlowMotionControl;
                    return dcsmc.Configuration;
                case DelayCompositeType.FrozenMosaic:
                    DelayCompositeFrozenMosaicControl dcfmc = configurationPanels[DelayCompositeType.FrozenMosaic] as DelayCompositeFrozenMosaicControl;
                    return dcfmc.Configuration;
                default:
                    break;
            }

            return config;
        }
        
        private void BtnOKClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DelayCompositeType t = (DelayCompositeType)cbType.SelectedIndex;
            
            gbConfiguration.Controls.Clear();

            switch (t)
            {
                case DelayCompositeType.MultiReview:
                    gbConfiguration.Enabled = true;
                    gbConfiguration.Controls.Add(configurationPanels[DelayCompositeType.MultiReview]);                    
                    break;
                case DelayCompositeType.SlowMotion:
                    gbConfiguration.Enabled = true;
                    gbConfiguration.Controls.Add(configurationPanels[DelayCompositeType.SlowMotion]);
                    break;
                case DelayCompositeType.FrozenMosaic:
                    gbConfiguration.Enabled = true;
                    gbConfiguration.Controls.Add(configurationPanels[DelayCompositeType.FrozenMosaic]);
                    break;
                case DelayCompositeType.Basic:
                case DelayCompositeType.Mixed:
                default:
                    gbConfiguration.Enabled = false;
                    break;
            }

            if (gbConfiguration.Controls.Count == 1)
            {
                Control panel = gbConfiguration.Controls[0];
                panel.Top = 20;
                panel.Left = 10;
                panel.Width = 288;
                panel.Height = 125;
            }
        }
    }
}
