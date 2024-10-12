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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A container for drop down menu items.
    /// Used in the context of ToolStripButtonWithDropDown, which is a replacement for ToolStripSplitButton.
    /// </summary>
    public partial class DropDownMenuContainer : Form
    {
        public ToolStripItemCollection Items {
            get { return contextMenuStrip.Items; }
        }
        public DropDownMenuContainer()
        {
            InitializeComponent();
        }
        public void Show(Point location)
        {
            contextMenuStrip.Show(new Point(location.X, location.Y - GetHeight()));
        }
        public void SelectIndex(int selectedIndex)
        {
            for(int i = 0; i<contextMenuStrip.Items.Count;i++)
            {
                ToolStripMenuItem menuItem = contextMenuStrip.Items[i] as ToolStripMenuItem;
                if (menuItem == null)
                    continue;

                menuItem.Checked = i == selectedIndex;
            }
        }
        private int GetHeight()
        {
            int result = 0;
            foreach(var item in contextMenuStrip.Items)
            {
                if (item is ToolStripMenuItem)
                {
                    result += ((ToolStripMenuItem)item).Height;
                }

                if (item is ToolStripSeparator)
                {
                    result += ((ToolStripSeparator)item).Height;
                }
            }

            return result;
        }
        private void DropDownFormLeave(object sender, EventArgs e)
        {
            HideAll();
        }
        private void DropDownFormDeactivate(object sender, EventArgs e)
        {
            HideAll();
        }
        private void HideAll()
        {
            contextMenuStrip.Hide();
        }
    }
}
