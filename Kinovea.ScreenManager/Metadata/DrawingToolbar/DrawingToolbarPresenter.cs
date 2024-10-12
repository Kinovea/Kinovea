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
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Kinovea.ScreenManager
{
    public class DrawingToolbarPresenter : IDisposable
    {
        public Control View
        {
            get { return view;}
        }
        
        private ToolStrip view;
        
        public DrawingToolbarPresenter()
        {
            view = new ToolStrip();
            view.BackColor = Color.White;
        }
        
        public void ForceView(ToolStrip view)
        {
            // This method will be removed when the Playerscreen refactoring is complete.
            this.view = view;
        }

        public void AddToolButton(AbstractDrawingTool tool, EventHandler handler)
        {
            ToolStripButton button = CreateToolButton();
            button.Image = tool.Icon;
            button.Tag = tool;
            button.Click += handler;
            button.ToolTipText = tool.DisplayName;
            
            view.Items.Add(button);
        }

        public void AddSpecialButton(ToolStripButton button)
        {
            view.Items.Add(button);
        }
        
        public void AddSeparator()
        {
            view.Items.Add(new ToolStripSeparator());
        }
        
        public void AddToolButtonGroup(AbstractDrawingTool[] tools, int selectedIndex, EventHandler handler)
        {
            // Each menu item will act as a button, and the master button will take the icon of the selected menu.
            ToolStripButtonWithDropDown button = new ToolStripButtonWithDropDown();
            button.AutoSize = false;
            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.ImageScaling = ToolStripItemImageScaling.None;
            button.Size = new Size(25, 25);
            button.AutoToolTip = false;
        
            for(int i = tools.Length-1; i >= 0; i--)
            {
                AbstractDrawingTool tool = tools[i];
                

                if (tool is DrawingToolSeparator)
                {
                    ToolStripSeparator separator = new ToolStripSeparator();
                    button.DropDownItems.Add(separator);
                }
                else
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Image = tool.Icon;
                    item.Text = tool.DisplayName;
                    item.Tag = tool;
                    int indexClosure = tools.Length - 1 - i;
                    item.Click += (s,e) =>
                    {
                        button.SelectedIndex = indexClosure;
                        handler(s,e);
                    };
                    button.DropDownItems.Add(item);
                }
        
            }
        
            button.SelectedIndex = tools.Length - 1 - selectedIndex;
        
            view.Items.Add(button);
        }
        
        public void RefreshUICulture()
        {
            // TODO: Move to view.
            foreach(ToolStripItem tsi in view.Items)
            {
                if(tsi is ToolStripSeparator)
                    continue;
        
                if(tsi is ToolStripButtonWithDropDown)
                {
                    foreach(ToolStripItem subItem in ((ToolStripButtonWithDropDown)tsi).DropDownItems)
                    {
                        if(!(subItem is ToolStripMenuItem))
                            continue;
        
                        AbstractDrawingTool tool = subItem.Tag as AbstractDrawingTool;
                        if(tool != null)
                        {
                            subItem.Text = tool.DisplayName;
                            subItem.ToolTipText = tool.DisplayName;
                        }
                    }
        
                    ((ToolStripButtonWithDropDown)tsi).UpdateToolTip();
                }
                else if(tsi is ToolStripButton)
                {
                    AbstractDrawingTool tool = tsi.Tag as AbstractDrawingTool;
                    if(tool != null)
                        tsi.ToolTipText = tool.DisplayName;
                }
            }
        }
        
        private ToolStripButton CreateToolButton()
        {
            ToolStripButton btn = new ToolStripButton();
            btn.AutoSize = false;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btn.ImageScaling = ToolStripItemImageScaling.None;
            btn.Size = new Size(25, 25);
            btn.AutoToolTip = false;
            return btn;
        }

        public void Dispose()
        {
            view.Items.Clear();
            view.Dispose();
        }
    }
}
