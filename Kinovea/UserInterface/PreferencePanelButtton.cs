#region License
/*
Copyright © Joan Charmant 2011.
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.Root
{
    /// <summary>
    /// PreferencePanelButtton.
    /// A simple "image + label" control to be used for preferences pages.
    /// </summary>
    public partial class PreferencePanelButtton : UserControl
    {
        #region Properties
        public IPreferencePanel PreferencePanel
        {
            get { return preferencePanel; }
        }
        #endregion
        
        #region Members
        private bool isSelected;
        private IPreferencePanel preferencePanel;
        private static readonly Font fontLabel = new Font("Arial", 8, FontStyle.Regular);
        #endregion
        
        #region Construction
        public PreferencePanelButtton(IPreferencePanel preferencePanel)
        {
            InitializeComponent();
            this.preferencePanel = preferencePanel;
        }
        #endregion
        
        #region Public Methods
        public void SetSelected(bool isSelected)
        {
            this.isSelected = isSelected;
            this.BackColor = isSelected ? Color.LightSteelBlue : Color.White;
        }
        #endregion
        
        #region Private Methods
        private void preferencePanelButtton_Paint(object sender, PaintEventArgs e)
        {
            if(preferencePanel.Icon != null)
            {
                Point iconStart = new Point((this.Width - preferencePanel.Icon.Width) / 2, 10);
                e.Graphics.DrawImageUnscaled(preferencePanel.Icon, iconStart);
            }
            
            SizeF textSize = e.Graphics.MeasureString(preferencePanel.Description, fontLabel);
            PointF textStart = new PointF(((float)this.Width - textSize.Width) / 2, 50.0F);
            e.Graphics.DrawString(preferencePanel.Description, fontLabel, Brushes.Black, textStart);
        }
        private void PreferencePanelButttonMouseEnter(object sender, EventArgs e)
        {
            if(!isSelected)
                this.BackColor = Color.FromArgb(224,232,246);
        }
        void PreferencePanelButttonMouseLeave(object sender, EventArgs e)
        {
            if(!isSelected)
                this.BackColor = Color.White;
        }
        #endregion
    }
}
