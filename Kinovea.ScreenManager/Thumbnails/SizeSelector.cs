#region License
/*
Copyright © Joan Charmant 2013.
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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class SizeSelector : UserControl
    {
        public event EventHandler SelectionChanged;
        public ExplorerThumbSize SelectedSize { get; private set;}
        
        private Dictionary<ExplorerThumbSize, Button> buttons = new Dictionary<ExplorerThumbSize, Button>();
        private Color unselectedColor = Color.SteelBlue;
        private Color selectedColor = Color.LightSteelBlue;
        
        public SizeSelector()
        {
            InitializeComponent();
            InitButtons();
            SelectedSize = ExplorerThumbSize.Medium;
        }
        
        public void ForceSelect(ExplorerThumbSize newSize)
        {
            DeselectAll();
            SelectedSize = newSize;
            buttons[newSize].BackColor = selectedColor;
        }
        
        public void Increase()
        {
            if (SelectedSize == ExplorerThumbSize.ExtraLarge)
                return;

            DeselectAll();
            
            switch(SelectedSize)
            {
                case ExplorerThumbSize.ExtraSmall:
                    SelectedSize = ExplorerThumbSize.Small;
                    break;
                case ExplorerThumbSize.Small:
                    SelectedSize = ExplorerThumbSize.Medium;
                    break;
                case ExplorerThumbSize.Medium:
                    SelectedSize = ExplorerThumbSize.Large;
                    break;
                case ExplorerThumbSize.Large:
                    SelectedSize = ExplorerThumbSize.ExtraLarge;
                    break;
            }
            
            buttons[SelectedSize].BackColor = selectedColor;
            
            if(SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }
        
        public void Decrease()
        {
            if (SelectedSize == ExplorerThumbSize.ExtraSmall)
                return;

            DeselectAll();
            
            switch(SelectedSize)
            {
                case ExplorerThumbSize.Small:
                    SelectedSize = ExplorerThumbSize.ExtraSmall;
                    break;
                case ExplorerThumbSize.Medium:
                    SelectedSize = ExplorerThumbSize.Small;
                    break;
                case ExplorerThumbSize.Large:
                    SelectedSize = ExplorerThumbSize.Medium;
                    break;
                case ExplorerThumbSize.ExtraLarge:
                    SelectedSize = ExplorerThumbSize.Large;
                    break;
            }
            
            buttons[SelectedSize].BackColor = selectedColor;
            
            if(SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }
        
        private void InitButtons()
        {
            buttons.Add(ExplorerThumbSize.ExtraSmall, btnExtraSmall);
            buttons.Add(ExplorerThumbSize.Small, btnSmall);
            buttons.Add(ExplorerThumbSize.Medium, btnMedium);
            buttons.Add(ExplorerThumbSize.Large, btnLarge);
            buttons.Add(ExplorerThumbSize.ExtraLarge, btnExtraLarge);
            
            foreach(Button button in buttons.Values)
                button.Click += buttons_Click;
        }
        
        private void DeselectAll()
        {
            foreach(Button button in buttons.Values)
                button.BackColor = unselectedColor;
        }
        
        private void buttons_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if(btn == null || SelectionChanged == null)
                return;
            
            foreach(ExplorerThumbSize ets in buttons.Keys)
            {
                if(buttons[ets] != btn)
                    continue;
                
                ForceSelect(ets);
                SelectedSize = ets;
                SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
