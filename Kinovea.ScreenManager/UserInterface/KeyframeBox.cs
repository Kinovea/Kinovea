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
        /// <summary>
        /// Select this keyframe and move the global playhead there.
        /// </summary>
        public event EventHandler SelectAsked;
        /// <summary>
        /// Same as select plus bring up the comment editor.
        /// </summary>
        public event EventHandler ActivateAsked;
        /// <summary>
        /// Move this keyframe the current time of the playhead.
        /// </summary>
        public event EventHandler MoveToCurrentTimeAsked;
        /// <summary>
        /// Delete this keyframe.
        /// </summary>
        public event EventHandler DeleteAsked;
        #endregion

        #region Properties
        public Keyframe Keyframe
        {
            get { return keyframe; }
        }

        public bool Editing
        {
            get { return editing; }
        }
        #endregion

        #region Members
        private bool editing;
        private Keyframe keyframe;
        private bool manualUpdate;
        private bool isSelected;
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuComments = new ToolStripMenuItem();
        private ToolStripMenuItem mnuMove = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
        #endregion

        public KeyframeBox(Keyframe keyframe)
        {
            this.keyframe = keyframe;
            
            InitializeComponent();
            lblTimecode.Text = keyframe.Title;
            
            BackColor = keyframe.Color;
            btnClose.Parent = pbThumbnail;
            btnComment.Parent = pbThumbnail;

            pbThumbnail.BackColor = Color.Black;
            pbThumbnail.SizeMode = PictureBoxSizeMode.CenterImage;

            manualUpdate = true;
            tbTitle.Text = keyframe.Title;
            manualUpdate = false;

            BuildContextMenu();
            ReloadMenusCulture();
        }
        
        public void DisplayAsSelected(bool selected)
        {
            isSelected = selected;
            pbThumbnail.BackColor = selected ? keyframe.Color : Color.Black;
        }
        public void UpdateTitle(string title)
        {
            lblTimecode.Text = title;
            BackColor = keyframe.Color;
            DisplayAsSelected(isSelected);
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

        public void RefreshUICulture()
        {
            ReloadMenusCulture();
        }

        #region Event Handlers
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
        private void Controls_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // Drag and drop conflicts with the double click event.
            // Use explicit click counting.
            if (e.Clicks == 2)
            {
                ActivateAsked?.Invoke(this, e);
            }
            else
            {
                SelectAsked?.Invoke(this, e);
            }
        }
        private void Controls_MouseUp(object sender, MouseEventArgs e)
        {
        }
        private void Controls_MouseMove(object sender, MouseEventArgs e)
        {
            // Support for drag and drop between the keyframe list and the timeline,
            // to move a keyframe to a specific time.
            if (e.Button != MouseButtons.Left)
                return;

            SelectAsked?.Invoke(this, e);
            this.DoDragDrop(this, DragDropEffects.Move);
        }
        private void Controls_DragDrop(object sender, DragEventArgs e)
        {
            // Called when we "drop" an object on the thumbnail.
            SelectAsked?.Invoke(this, e);
        }
        private void Controls_DragOver(object sender, DragEventArgs e)
        {
            // Called when we move over the thumbnail during a drag and drop op.
            e.Effect = DragDropEffects.Move;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            DeleteAsked?.Invoke(this, e);
        }
        private void btnComment_Click(object sender, EventArgs e)
        {
            ActivateAsked?.Invoke(this, e);
        }
        private void TbTitleTextChanged(object sender, EventArgs e)
        {
            if(!manualUpdate)
            {
                keyframe.Title = tbTitle.Text;
                UpdateToolTip();
            }
        }
        private void pbThumbnail_MouseDoubleClick(object sender, MouseEventArgs e)
        {

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
        private void BuildContextMenu()
        {
            mnuComments.Image = Properties.Resources.balloon_ellipsis;
            mnuComments.Click += (s, e) => ActivateAsked?.Invoke(this, e);
            mnuMove.Image = Properties.Drawings.move_keyframe;
            mnuMove.Click += (s, e) => MoveToCurrentTimeAsked?.Invoke(this, e);
            mnuDelete.Image = Properties.Drawings.delete;
            mnuDelete.Click += (s, e) => DeleteAsked?.Invoke(this, e);

            popMenu.Items.AddRange(new ToolStripItem[] { 
                mnuComments,
                mnuMove,
                new ToolStripSeparator(),
                mnuDelete
            });

            this.ContextMenuStrip = popMenu;
        }

        private void ReloadMenusCulture()
        {
            // Reload the text for each menu.
            // this is done at construction time and at RefreshUICulture time.
            mnuComments.Text = Languages.ScreenManagerLang.dlgKeyframeComment_Title;
            mnuMove.Text = "Move to current time";
            mnuDelete.Text = Languages.ScreenManagerLang.mnuThumbnailDelete;
        }

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
