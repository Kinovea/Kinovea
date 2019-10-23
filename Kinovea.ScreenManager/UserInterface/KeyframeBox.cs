/*
Copyright © Joan Charmant 2008.
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

using Kinovea.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// Represent a key image as a thumbnail.
    /// Holds the key image id in the .Tag prop.
    /// </summary>
    public partial class KeyframeBox : UserControl
    {
        #region Events
        [Category("Action"), Browsable(true)]
        public event EventHandler CloseThumb;
        [Category("Action"), Browsable(true)]
        public event EventHandler ClickThumb;
        [Category("Action"), Browsable(true)]
        public event EventHandler ClickInfos;
        #endregion

        public Keyframe Keyframe
        {
            get { return keyframe; }
        }

        public bool Editing
        {
            get { return editing; }
        }
        
        private bool editing;
        private Keyframe keyframe;
        private bool manualUpdate;
        
        #region Constructor
        public KeyframeBox(Keyframe keyframe)
        {
            this.keyframe = keyframe;
            
            InitializeComponent();
            lblTimecode.Text = keyframe.Title;
            
            BackColor = Color.Black;
            btnClose.Parent = pbThumbnail;
            btnComment.Parent = pbThumbnail;

            pbThumbnail.BackColor = Color.Black;
            pbThumbnail.SizeMode = PictureBoxSizeMode.CenterImage;

            manualUpdate = true;
            tbTitle.Text = keyframe.Title;
            manualUpdate = false;
        }
        #endregion
        
        #region Public Methods
        public void DisplayAsSelected(bool selected)
        {
            BackColor = selected ? Color.SteelBlue : Color.Black;
        }
        public void UpdateTitle(string title)
        {
            lblTimecode.Text = title;
            
            manualUpdate = true;
            tbTitle.Text = title;
            manualUpdate = false;
        
            UpdateToolTip();	
        }
        public void UpdateEnableStatus()
        {
            this.Enabled = !keyframe.Disabled;
            UpdateTitle(keyframe.Title);
            UpdateImage();
        }

        public void UpdateImage()
        {
            this.pbThumbnail.Image = keyframe.Disabled ? keyframe.DisabledThumbnail : keyframe.Thumbnail;
            this.Invalidate();
        }
        #endregion
        
        #region Event Handlers - Mouse Enter / Leave
        private void Controls_MouseEnter(object sender, EventArgs e)
        {
            ShowButtons();
        }
        private void Controls_MouseLeave(object sender, EventArgs e)
        {
            // We hide the close button only if we left the whole control.
            Point clientMouse = PointToClient(Control.MousePosition);
            
            if(!pbThumbnail.ClientRectangle.Contains(clientMouse))
            {
                HideButtons();
                editing = false;

                // Fixme: Doesn't prevent typing until another control takes focus.
                StopEditing();
            }
        }
        private void Controls_MouseDoubleClick(object sender, EventArgs e)
        {
            if (ClickInfos != null) ClickInfos(this, e);	
        }
        #endregion

        #region Event Handlers - Buttons / Text
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (CloseThumb != null) 
                CloseThumb(this, e);
        }
        private void pbThumbnail_Click(object sender, EventArgs e)
        {
            if (ClickThumb != null) 
                ClickThumb(this, e);
        }
        private void btnComment_Click(object sender, EventArgs e)
        {
            if (ClickInfos != null) 
                ClickInfos(this, e);
        }
        private void TbTitleTextChanged(object sender, EventArgs e)
        {
            if(!manualUpdate)
            {
                keyframe.Title = tbTitle.Text;
                UpdateToolTip();
            }
        }
        private void lblTimecode_Click(object sender, EventArgs e)
        {
            editing = true;
        }

        private void tbTitle_Click(object sender, EventArgs e)
        {
            editing = true;
        }
        #endregion
        
        #region Private helpers
        private void ShowButtons()
        {
            btnClose.Visible = true;
            btnComment.Visible = true;
        }
        private void HideButtons()
        {
            btnClose.Visible = false;
            btnComment.Visible = false;
        }
        
        private void StopEditing()
        {
            if(tbTitle.Text.Length == 0)
            {
                // We reseted the title. We should now display the timecode.
                manualUpdate = true;
                tbTitle.Text = keyframe.Title;
                manualUpdate = false;
            }

            UpdateToolTip();
        }
        private void UpdateToolTip()
        {
            if(keyframe.TimeCode != keyframe.Title)
                toolTips.SetToolTip(pbThumbnail, keyframe.Title + "\n" + keyframe.TimeCode);
            else
                toolTips.SetToolTip(pbThumbnail, "");
        }
        #endregion
    }
}
