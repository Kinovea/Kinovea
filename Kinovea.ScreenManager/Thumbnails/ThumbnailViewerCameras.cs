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
using System.Linq;

using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class ThumbnailViewerCameras : KinoveaControl
    {
        #region Events
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        #endregion
        
        #region Members
        private ThumbnailCamera selectedThumbnail;
        private int columns = (int)ExplorerThumbSize.Large;
        private List<ThumbnailCamera> thumbnailControls = new List<ThumbnailCamera>();
        private HashSet<ThumbnailCamera> imageReceived = new HashSet<ThumbnailCamera>();
        private bool refreshImages;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ThumbnailViewerCameras()
        {
            InitializeComponent();
            //RefreshUICulture();
            this.Dock = DockStyle.Fill;
            refreshImages = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("ThumbnailViewerCamera");
        }
        
        #region Public methods
        public void Unhide()
        {
            refreshImages = true;
            CameraTypeManager.StartDiscoveringCameras();
            this.Focus();
        }
        public void CamerasDiscovered(List<CameraSummary> summaries)
        {
            bool updated = UpdateThumbnailList(summaries);
            if(updated)
                DoLayout();
        }
        public void CameraImageReceived(CameraSummary summary, Bitmap image)
        {
            if(this.InvokeRequired)
                this.BeginInvoke((Action) delegate {UpdateThumbnailImage(summary, image);});
            else
                UpdateThumbnailImage(summary, image);
        }
        public void CameraSummaryUpdated(CameraSummary summary)
        {
            int index = IndexOf(summary.Identifier);
            if(index < 0)
            return;
                
            thumbnailControls[index].UpdateSummary(summary);
        }

        public void CameraForgotten(CameraSummary summary)
        {
            int index = IndexOf(summary.Identifier);
            if (index < 0)
                return;

            RemoveThumbnail(thumbnailControls[index]);
            Refresh();
        }
        
        public void UpdateThumbnailsSize(ExplorerThumbSize newSize)
        {
            this.columns = (int)newSize;
            if(thumbnailControls.Count > 0)
                DoLayout();
        }

        public void RefreshUICulture()
        {
        }
        #endregion

        #region Private methods
        private bool UpdateThumbnailList(List<CameraSummary> summaries)
        {
            bool updated = false;
            
            if(summaries.Count == 0)
            {
                if(BeforeLoad != null)
                    BeforeLoad(this, EventArgs.Empty);
                    
                imageReceived.Clear();
            }
            
            // Add new cameras.
            List<string> found = new List<string>();
            foreach(CameraSummary summary in summaries)
            {
                found.Add(summary.Identifier);
                
                int index = IndexOf(summary.Identifier);
                if(index >= 0)
                {
                    if (refreshImages)
                        summary.Manager.StartThumbnail(summary);

                    continue;
                }
                
                // New camera, add it to the list and start async thumbnail retrieval.
                updated = true;
                summary.Manager.StartThumbnail(summary);
                AddThumbnail(new ThumbnailCamera(summary));
            }
            
            refreshImages = false;
            
            // Remove cameras that were disconnected.
            List<ThumbnailCamera> lost = new List<ThumbnailCamera>();
            foreach(ThumbnailCamera thumbnail in thumbnailControls)
            {
                if(!found.Contains(thumbnail.Summary.Identifier))
                    lost.Add(thumbnail);
            }
            
            if(lost.Count > 0)
                updated = true;
                
            foreach(ThumbnailCamera thumbnail in lost)
                RemoveThumbnail(thumbnail);
            
            if(summaries.Count == 0)
            {
                if(AfterLoad != null)
                    AfterLoad(this, EventArgs.Empty);
            }

            if (selectedThumbnail == null && thumbnailControls.Count > 0)
                thumbnailControls[0].SetSelected();
            
            return updated;
        }
        
        private int IndexOf(string identifier)
        {
            for(int i = 0; i<thumbnailControls.Count; i++)
                if(thumbnailControls[i].Summary.Identifier == identifier)
                    return i;
            
            return -1;
        }
        
        private void DoLayout()
        {
            int leftMargin = 30;
            int rightMargin = 20;
            int topMargin = 5;
            
            int colWidth = (this.Width - leftMargin - rightMargin) / columns;
            int spacing = colWidth / 20;
            
            int thumbWidth = colWidth - spacing;
            int thumbHeight = (thumbWidth * 3 / 4) + 15;
            
            int current = 0;
            
            this.SuspendLayout();
            foreach(ThumbnailCamera thumbnail in thumbnailControls)
            {
                thumbnail.SetSize(thumbWidth, thumbHeight);

                int row = current / columns;
                int col = current - (row * columns);
                int left = col * colWidth + leftMargin;
                int top = topMargin + (row * (thumbHeight + spacing));
                thumbnail.Location = new Point(left, top);
                current++;
            }
            
            this.ResumeLayout();
        }

        private void AddThumbnail(ThumbnailCamera thumbnail)
        {
            thumbnail.LaunchCamera += Thumbnail_LaunchCamera;
            thumbnail.CameraSelected += Thumbnail_CameraSelected;
            thumbnail.SummaryUpdated += Thumbnail_SummaryUpdated;
            thumbnail.DeleteCamera += Thumbnail_DeleteCamera;

            thumbnailControls.Add(thumbnail);
            this.Controls.Add(thumbnail);
        }

        private void RemoveThumbnail(ThumbnailCamera thumbnail)
        {
            if (imageReceived.Contains(thumbnail))
                imageReceived.Remove(thumbnail);

            thumbnail.LaunchCamera -= Thumbnail_LaunchCamera;
            thumbnail.CameraSelected -= Thumbnail_CameraSelected;
            thumbnail.SummaryUpdated -= Thumbnail_SummaryUpdated;
            thumbnail.DeleteCamera -= Thumbnail_DeleteCamera;

            this.Controls.Remove(thumbnail);
            thumbnailControls.Remove(thumbnail);
            if (selectedThumbnail == thumbnail)
                selectedThumbnail = null;

            thumbnail.Dispose();
        }
        
        private void UpdateThumbnailImage(CameraSummary summary, Bitmap image)
        {
            if(summary == null)
                return;
            
            int index = IndexOf(summary.Identifier);
            if(index < 0)
                return;
                
            bool hasImage = thumbnailControls[index].Image != null;
            thumbnailControls[index].UpdateImage(image);
            
            if(hasImage)
                return;

            imageReceived.Add(thumbnailControls[index]);
            int percentage = (int)(((float)imageReceived.Count / thumbnailControls.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            
            if(imageReceived.Count >= thumbnailControls.Count && AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);
        }
        
        private void ThumbnailViewerCameras_Resize(object sender, EventArgs e)
        {
            DoLayout();
        }
        
        private void Thumbnail_LaunchCamera(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;

            if (thumbnail != null)
                CameraTypeManager.LoadCamera(thumbnail.Summary, -1);
        }
        
        private void Thumbnail_DeleteCamera(object sender, EventArgs e)
        {
            // Delete camera in prefs (blurbs).
            // Should be enough to remove the thumbnail at next discovery heart beat.
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
            CameraTypeManager.ForgetCamera(thumbnail.Summary);

            refreshImages = true;
            CameraTypeManager.StartDiscoveringCameras();
        }
        
        private void Thumbnail_CameraSelected(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
        
            if(thumbnail == null)
                return;
                
            if (selectedThumbnail != null && selectedThumbnail != thumbnail )
                selectedThumbnail.SetUnselected();
        
            selectedThumbnail = thumbnail;
        }
        
        private void Thumbnail_SummaryUpdated(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
            CameraTypeManager.UpdatedCameraSummary(thumbnail.Summary);
        }
        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (thumbnailControls.Count == 0)
                return base.ProcessCmdKey(ref msg, keyData);

            if (selectedThumbnail == null)
            {
                if (thumbnailControls.Count > 0 && (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down))
                {
                    thumbnailControls[0].SetSelected();
                    return true;
                }
                else
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }
            }

            // Keyboard navigation (bypass the hotkey system).
            int index = IndexOf(selectedThumbnail.Summary.Identifier);
            int row = index / columns;
            int col = index - (row * columns);
            bool handled = false;

            switch (keyData)
            {
                case Keys.Left:
                    {
                        if (col > 0)
                            thumbnailControls[index - 1].SetSelected();
                        handled = true;
                        break;
                    }
                case Keys.Right:
                    {
                        if (col < columns - 1 && index + 1 < thumbnailControls.Count)
                            thumbnailControls[index + 1].SetSelected();
                        handled = true;
                        break;
                    }
                case Keys.Up:
                    {
                        if (row > 0)
                            thumbnailControls[index - columns].SetSelected();
                        this.ScrollControlIntoView(selectedThumbnail);
                        handled = true;
                        break;
                    }
                case Keys.Down:
                    {
                        if (index + columns < thumbnailControls.Count)
                            thumbnailControls[index + columns].SetSelected();
                        this.ScrollControlIntoView(selectedThumbnail);
                        handled = true;
                        break;
                    }
                case Keys.Home:
                    {
                        thumbnailControls[0].SetSelected();
                        break;
                    }
                case Keys.End:
                    {
                        thumbnailControls[thumbnailControls.Count - 1].SetSelected();
                        break;
                    }
                default:
                    break;
            }

            return handled || base.ProcessCmdKey(ref msg, keyData);
        }

        #region Commands
        protected override bool ExecuteCommand(int cmd)
        {
            ThumbnailViewerCameraCommands command = (ThumbnailViewerCameraCommands)cmd;

            switch (command)
            {
                case ThumbnailViewerCameraCommands.RenameSelected:
                    if (selectedThumbnail != null)
                        selectedThumbnail.StartRenaming();
                    break;
                case ThumbnailViewerCameraCommands.LaunchSelected:
                    CameraTypeManager.LoadCamera(selectedThumbnail.Summary, -1);
                    break;
                case ThumbnailViewerCameraCommands.Refresh:
                    refreshImages = true;
                    CameraTypeManager.StartDiscoveringCameras();
                    this.Focus();
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }
        #endregion
    }
}
