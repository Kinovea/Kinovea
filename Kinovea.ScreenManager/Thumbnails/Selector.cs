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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A simple generic visual selector with buttons.
    /// </summary>
    public partial class Selector : UserControl
    {
        public event EventHandler SelectionChanged;
        
        public SelectorOption Selected
        {
            get { return selected; }
        }
        
        private List<SelectorOption> options;
        private SelectorOption selected = null;
        
        public Selector(List<SelectorOption> options, int defaultSelection)
        {
            if(options == null || options.Count == 0)
                throw new ArgumentException("options");
            
            if(defaultSelection < 0 || defaultSelection > options.Count)
                throw new ArgumentOutOfRangeException("defaultSelection");
                
            InitializeComponent();
            
            this.options = options;
            
            int index = 0;
            int left = 0;
            int spacing = 2;
            foreach(SelectorOption option in options)
            {
                Button btn = new Button();
                btn.BackgroundImage = option.Image;
                btn.BackgroundImageLayout = ImageLayout.Center;
                btn.BackColor = Color.WhiteSmoke;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.MouseDownBackColor = Color.WhiteSmoke;
                btn.FlatAppearance.MouseOverBackColor = Color.WhiteSmoke;
                btn.FlatAppearance.BorderSize = 0;
                btn.Size = new Size(18,18);
                btn.Left = left;
                btn.Cursor = Cursors.Hand;
                btn.Tag = option;
                btn.Click += OptionButton_Click;
                this.Controls.Add(btn);
                if(index == defaultSelection)
                    selected = option;

                left += btn.Width + spacing;
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
            SelectorOption option = btn.Tag as SelectorOption;
            if(option != null)
            {
                selected = option;
                SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
