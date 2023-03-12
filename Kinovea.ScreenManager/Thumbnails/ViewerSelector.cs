#region License
/*
Copyright © Joan Charmant 2012.
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
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public partial class ViewerSelector : UserControl
    {
        [Category("Behavior"), Browsable(true)]
        public event EventHandler SelectionChanged;
        
        public ViewerSelectorOption Selected
        {
            get { return selected; }
        }
        
        private List<ViewerSelectorOption> options;
        private ViewerSelectorOption selected = null;
        
        public ViewerSelector()
        {
            ViewerSelectorOption optionFiles = new ViewerSelectorOption(ScreenManager.Properties.Resources.explorer_video, "", ThumbnailViewerType.Files);
            ViewerSelectorOption optionShortcuts = new ViewerSelectorOption(ScreenManager.Properties.Resources.explorer_shortcut, "", ThumbnailViewerType.Shortcuts);
            ViewerSelectorOption optionCameras = new ViewerSelectorOption(ScreenManager.Properties.Resources.explorer_camera, "", ThumbnailViewerType.Cameras);

            List<ViewerSelectorOption> options = new List<ViewerSelectorOption>();
            options.Add(optionFiles);
            options.Add(optionShortcuts);
            options.Add(optionCameras);

            int defaultSelection = 0;
                
            InitializeComponent();
            
            this.options = options;
            
            int index = 0;
            int left = 0;
            int spacing = 8;
            foreach(ViewerSelectorOption option in options)
            {
                Button btn = new Button();
                btn.BackgroundImage = option.Image;
                btn.BackgroundImageLayout = ImageLayout.Center;
                btn.BackColor = Color.WhiteSmoke;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.MouseDownBackColor = Color.WhiteSmoke;
                btn.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
                btn.FlatAppearance.BorderSize = 0;
                btn.Size = new Size(20, 20);
                btn.Left = left;
                btn.Cursor = Cursors.Hand;
                btn.Tag = option;
                btn.Click += OptionButton_Click;
                this.Controls.Add(btn);
                if(index == defaultSelection)
                    selected = option;

                left = btn.Right + spacing;
                index++;
            }
            
            this.Size = new Size(left, options[0].Image.Height + 2);
            this.BackColor = Color.WhiteSmoke;
        }

        private void OptionButton_Click(object sender, EventArgs e)
        {
            if(SelectionChanged == null)
                return;
                
            Button btn = sender as Button;
            ViewerSelectorOption option = btn.Tag as ViewerSelectorOption;
            if(option != null)
            {
                selected = option;
                SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
