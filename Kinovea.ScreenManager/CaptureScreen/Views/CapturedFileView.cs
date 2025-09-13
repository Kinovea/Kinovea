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
using System.IO;
using System.Windows.Forms;

using Kinovea.ScreenManager.Languages;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class CapturedFileView : UserControl
    {
        #region Events
        public event EventHandler LaunchAsked;
        public event EventHandler LocateAsked;
        public event EventHandler SelectAsked;
        public event EventHandler HideAsked;
        public event EventHandler DeleteAsked;
        #endregion
        
        public CapturedFile CapturedFile
        {
            get { return capturedFile; }
        }

        public bool Editing
        {
            get { return editing; }
        }
        
        private CapturedFile capturedFile;
        private bool selected;
        private bool hovering;
        private bool editing;
        private Pen penBorder = new Pen(Color.SteelBlue, 2);
        private Pen penBorderHovering = new Pen(Color.SteelBlue, 2);
        private Pen penBorderNormal = new Pen(Color.Black, 1);
        private TextBox tbFilename = new TextBox();
        private EmbeddedButton btnClose;
        private List<EmbeddedButton> buttons = new List<EmbeddedButton>();
        
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuLoadVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRename = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHide = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public CapturedFileView(CapturedFile capturedFile)
        {
            this.capturedFile = capturedFile;
            InitializeComponent();
            this.BackColor = Color.FromArgb(255, 44, 44, 44);
            lblFilename.Text = capturedFile.Filename;
            tbFilename.KeyDown += tbFilename_KeyDown;
            tbFilename.MouseEnter += Controls_MouseEnter;
            tbFilename.MouseLeave += Controls_MouseLeave;
            btnClose = new EmbeddedButton(Properties.Capture.thumb_close, this.Width - Properties.Capture.thumb_close.Width - 2, 2);
            btnClose.Click += btnClose_Click;
            buttons.Add(btnClose);

            BuildContextMenus();
            ReloadMenusCulture();
        }

        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                tbFilename.KeyDown -= tbFilename_KeyDown;
                tbFilename.MouseEnter -= Controls_MouseEnter;
                tbFilename.MouseLeave -= Controls_MouseLeave;
                btnClose.Click -= btnClose_Click;

                mnuLoadVideo.Click -= mnuLoadVideo_Click;
                mnuLocate.Click -= mnuLocate_Click;
                mnuRename.Click -= mnuRename_Click;
                mnuHide.Click -= mnuHide_Click;
                mnuDelete.Click -= mnuDelete_Click;
            
                mnuLoadVideo.Dispose();
                mnuLocate.Dispose();
                mnuRename.Dispose();
                mnuHide.Dispose();
                mnuDelete.Dispose();

                popMenu.Items.Clear();
                popMenu.Dispose();
                this.ContextMenu = null;

                this.Controls.Remove(tbFilename);
                tbFilename.Dispose();

                penBorder.Dispose();
                penBorderHovering.Dispose();
                penBorderNormal.Dispose();
            }

            base.Dispose(disposing);
        }

        public void RefreshUICulture()
        {
            ReloadMenusCulture();
        }
        public void UpdateSelected(bool selected)
        {
            this.selected = selected;
            if(!selected)
            {
                hovering = false;
                if(editing)
                    StopEditing();
            }
        }
        
        #region Private methods
        private void BuildContextMenus()
        {
            mnuLoadVideo.Image = Properties.Resources.television;
            mnuLocate.Image = Properties.Resources.folder_new;
            mnuRename.Image = Properties.Capture.rename;
            mnuHide.Image = Properties.Resources.hide;
            mnuDelete.Image = Properties.Resources.delete;
            
            mnuLoadVideo.Click += mnuLoadVideo_Click;
            mnuLocate.Click += mnuLocate_Click;
            mnuRename.Click += mnuRename_Click;
            mnuHide.Click += mnuHide_Click;
            mnuDelete.Click += mnuDelete_Click;
            
            popMenu.Items.AddRange(new ToolStripItem[] 
            { 
                mnuLoadVideo, 
                mnuLocate, 
                mnuRename, 
                new ToolStripSeparator(), 
                mnuHide, 
                mnuDelete 
            }); 
            
            this.ContextMenuStrip = popMenu;
        }
        private void ReloadMenusCulture()
        {
            // Reload the text for each menu.
            // this is done at construction time and at RefreshUICulture time.
            mnuLoadVideo.Text = ScreenManagerLang.Generic_Open;
            mnuLocate.Text = ScreenManagerLang.mnuThumbnailLocate;
            mnuRename.Text = ScreenManagerLang.mnuThumbnailRename;
            mnuHide.Text = ScreenManagerLang.mnuGridsHide;
            mnuDelete.Text = ScreenManagerLang.mnuThumbnailDelete;
        }
        private void Close()
        {
            if(HideAsked != null)
                HideAsked(this, EventArgs.Empty);
        }
        private void LeaveTest()
        {
            Point clientMouse = PointToClient(Control.MousePosition);
            if(!this.ClientRectangle.Contains(clientMouse))
            {
                hovering = false;
                Invalidate();
            }
        }
        private void CapturedFileViewPaint(object sender, PaintEventArgs e)
        {
            int filenameHeight = editing ? tbFilename.Height : lblFilename.Height;
            Rectangle displayArea = new Rectangle(1, 1, this.Width - 2, this.Height - filenameHeight - 3);
            e.Graphics.DrawImage(capturedFile.Thumbnail, displayArea);
            
            if(hovering)
            {
                using(SolidBrush b = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                    e.Graphics.FillRectangle(b, new Rectangle(1, 2, this.Width - 2, 16));
                
                foreach(EmbeddedButton btn in buttons)
                    btn.Draw(e.Graphics);
            }
            
             if(selected)
                e.Graphics.DrawRectangle(penBorder, displayArea);
            else if(hovering)
                e.Graphics.DrawRectangle(penBorderHovering, displayArea);
            else
                e.Graphics.DrawRectangle(penBorderNormal, displayArea);
        }
        private void Controls_MouseEnter(object sender, EventArgs e)
        {
            hovering = true;
            Invalidate();
        }
        private void Controls_MouseLeave(object sender, EventArgs e)
        {
            // We hide the close button only if we left the whole control.
            Point clientMouse = PointToClient(Control.MousePosition);
            LeaveTest();
        }
        private void CapturedFileView_Click(object sender, EventArgs e)
        {
            Point clientMouse = PointToClient(Control.MousePosition);
            
            if(editing)
            {
                CommitEditing();
                StopEditing();
            }

            bool handled = false;
            foreach(EmbeddedButton btn in buttons)
            {
                handled = btn.ClickTest(clientMouse);
                if(handled)
                    break;
            }
                        
            if(handled)
                return;
                
            if(SelectAsked != null)
                SelectAsked(this, EventArgs.Empty);
        }
        private void CapturedFileViewDoubleClick(object sender, EventArgs e)
        {
            if(LaunchAsked != null)
                LaunchAsked(this, EventArgs.Empty);
        }
        private void CapturedFileView_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                DoDragDrop(capturedFile.Filepath, DragDropEffects.Copy);
                
            LeaveTest();
        }
        private void LblFilename_Click(object sender, EventArgs e)
        {
            EditingAsked();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        private void EditingAsked()
        {
            if(!selected && SelectAsked != null)
            {
                SelectAsked(this, EventArgs.Empty);
                StartEditing();
            }
            else if(selected && !editing)
            {
                StartEditing();
            }
        }
        private void StartEditing()
        {
            lblFilename.Visible = false;
            tbFilename.Visible = true;
            tbFilename.Text = lblFilename.Text;
            tbFilename.Location = new Point(lblFilename.Left, this.Height - tbFilename.Height);
            tbFilename.Size = new Size(lblFilename.Width, tbFilename.Height);
            tbFilename.Select(0, tbFilename.Text.Length);
            tbFilename.Focus();
            
            editing = true;
            this.Controls.Add(tbFilename);
            this.Invalidate();
        }
        private void StopEditing()
        {
            editing = false;
            this.Controls.Remove(tbFilename);
            lblFilename.Visible = true;
            tbFilename.Visible = false;
            lblFilename.Text = capturedFile.Filename;
            this.Invalidate();
        }
        private void tbFilename_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                StopEditing();
                return;
            }
            
            if(e.KeyCode == Keys.Return)
                CommitEditing();
        }
        private void CommitEditing()
        {
            string text = tbFilename.Text;
            if(string.IsNullOrEmpty(text))
                return;
                
            // Validate file.
            
            string newFilePath = Path.GetDirectoryName(capturedFile.Filepath) + "\\" + text;
            FilesystemHelper.RenameFile(capturedFile.Filepath, newFilePath);
            
            if(!File.Exists(capturedFile.Filepath) && File.Exists(newFilePath))
            {
                capturedFile.FileRenamed(newFilePath);
                NotificationCenter.RaiseRefreshFileList(false);
            }
            
            StopEditing();
        }

        private void mnuLoadVideo_Click(object sender, EventArgs e)
        {
            if (LaunchAsked != null)
                LaunchAsked(this, EventArgs.Empty);
        }

        private void mnuLocate_Click(object sender, EventArgs e)
        {
            if (LocateAsked != null)
                LocateAsked(this, EventArgs.Empty);
        }
        private void mnuRename_Click(object sender, EventArgs e)
        {
            EditingAsked();
        }
        private void mnuHide_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void mnuDelete_Click(object sender, EventArgs e)
        {
            if(DeleteAsked != null)
                DeleteAsked(this, EventArgs.Empty);
        }
        #endregion
    }
}
