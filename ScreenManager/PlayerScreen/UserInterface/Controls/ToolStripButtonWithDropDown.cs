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
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A replacement for ToolStripSplitButton.
    /// Like ToolStripSplitButton, it contains a list of sub menus, but there is no separate arrow button (eats too much space and is ugly).
    /// The way to bring up the menu items is to do a "long click".
    /// </summary>
    public class ToolStripButtonWithDropDown : ToolStripButton
    {
        #region Properties
        public ToolStripItemCollection DropDownItems {
            get { return dropDownMenuContainer.Items; }
        }
        public int LongClickInterval {
            get { return longClickTimer.Interval; }
            set { longClickTimer.Interval = value; }
        }
        public int SelectedIndex {
            get { return selectedIndex; }
            set 
            { 
                selectedIndex = value;
                UpdateSelected();
            }
        }
        #endregion
        
        #region Members
        private DropDownMenuContainer dropDownMenuContainer = new DropDownMenuContainer();
        private Timer longClickTimer = new Timer();
        private int selectedIndex = -1;
        private bool longClicking;
        #endregion
        
        #region Constructor
        public ToolStripButtonWithDropDown()
        {
            longClickTimer.Interval = 300;
            longClickTimer.Tick += longClickTimer_Tick;
        }
        #endregion
        
        #region Events and overrides
        protected override void OnMouseDown(MouseEventArgs e)
        {
            longClickTimer.Enabled = true;
            longClicking = false;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            longClickTimer.Enabled = false;
        }
        protected override void OnClick(EventArgs e)
        {
            if(longClicking)
                return;
            
            longClickTimer.Enabled = false;
            if(dropDownMenuContainer.Items.Count < 1 || selectedIndex < 0 || selectedIndex >= dropDownMenuContainer.Items.Count)
                return;
            
            ToolStripItem item = dropDownMenuContainer.Items[selectedIndex];
            item.PerformClick();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
           
            // Arrow in the corner
            /*PointF[] points = new PointF[]{
                new Point(this.Width - 5, 3),
                new Point(this.Width - 2, 3),
                new Point(this.Width - 2, 6)};*/
            
            // Arrow in top-left corner.
            PointF[] points = new PointF[]{
                new Point(2, 3),
                new Point(5, 3),
                new Point(2, 6)};
            
            e.Graphics.FillPolygon((SolidBrush)Brushes.Black, points);
        }
        private void longClickTimer_Tick(object sender, EventArgs e)
        {
            longClickTimer.Enabled = false;
            dropDownMenuContainer.Show(GetDropDownLocation());
            longClicking = true;
        }
        #endregion
        
        private Point GetDropDownLocation()
        {
            // Return the top left location of the button.
            // The dropdown container is responsible for placing itself above or below.
            
            if (this.Owner == null) 
                return new Point(5, 5);
            
            int x = 0;
            foreach (ToolStripItem item in this.Parent.Items)
            {
                if (item == this) 
                    break;
                
                x+=item.Width;
            }
            
            return this.Owner.PointToScreen(new Point(x, -4));
        }
        private void UpdateSelected()
        {
            if(dropDownMenuContainer.Items.Count < 1 || selectedIndex < 0 || selectedIndex >= dropDownMenuContainer.Items.Count)
                return;
            
            dropDownMenuContainer.SelectIndex(selectedIndex);
            
            ToolStripItem item = dropDownMenuContainer.Items[selectedIndex];
            this.Image = item.Image;
            this.Tag = item.Tag;
            this.ToolTipText = item.Text;
        }
    }
}


