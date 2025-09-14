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
    /// <summary>
    /// A thumbnail viewer for cameras.
    /// </summary>
    public partial class ThumbnailViewerCameras : KinoveaControl
    {
        #region Events
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        #endregion

        #region Members
        private ExplorerThumbSize thumbSize = ExplorerThumbSize.Medium;
        private List<ThumbnailCamera> thumbnails = new List<ThumbnailCamera>();
        private HashSet<ThumbnailCamera> imageReceived = new HashSet<ThumbnailCamera>();
        private ThumbnailCamera selectedThumbnail;
        private bool refreshThumbnailsOfKnownCameras;
        private bool hidden = true;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        public ThumbnailViewerCameras()
        {
            log.Debug("Constructing ThumbnailViewerCameras");

            InitializeComponent();
            //RefreshUICulture();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.pnlThumbs.BackColor = Color.FromArgb(250, 250, 250);

            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("ThumbnailViewerCamera");
            thumbSize = PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
            refreshThumbnailsOfKnownCameras = true;
        }
        #endregion
        
        #region Public methods

        /// <summary>
        /// We just got unhidden, restart discovering cameras.
        /// </summary>
        public void Unhide()
        {
            hidden = false;
            refreshThumbnailsOfKnownCameras = true;
            CameraTypeManager.StartDiscoveringCameras();
            this.Focus();
        }

        /// <summary>
        /// Indicate that the camera browser will not be visible until further notice.
        /// This makes sure we don't ask the cameras to produce thumbnails, even if 
        /// we are still receiving the discovery events.
        /// </summary>
        public void SetHidden()
        {
            refreshThumbnailsOfKnownCameras = false;
            hidden = true;
        }

        /// <summary>
        /// Camera discovery step.
        /// </summary>
        public void CamerasDiscovered(List<CameraSummary> summaries)
        {
            bool updated = UpdateThumbnailList(summaries);
            if(updated)
                DoLayout();
        }

        /// <summary>
        /// One thumbnail control is receiving its image.
        /// </summary>
        public void CameraImageReceived(CameraSummary summary, Bitmap image)
        {
            if(this.InvokeRequired)
                this.BeginInvoke((Action) delegate {UpdateThumbnailImage(summary, image);});
            else
                UpdateThumbnailImage(summary, image);
        }

        /// <summary>
        /// The summary got updated.
        /// </summary>
        public void CameraSummaryUpdated(CameraSummary summary)
        {
            int index = IndexOf(summary.Identifier);
            if(index < 0)
            return;
                
            thumbnails[index].UpdateSummary(summary);
        }

        public void CameraForgotten(CameraSummary summary)
        {
            int index = IndexOf(summary.Identifier);
            if (index < 0)
                return;

            RemoveThumbnail(thumbnails[index]);
            Refresh();
        }

        /// <summary>
        /// Called right before we are switching to this browser type.
        /// </summary>
        public void BeforeSwitch()
        {
            refreshThumbnailsOfKnownCameras = true;
            hidden = false;
        }
        
        public void UpdateThumbnailsSize(ExplorerThumbSize thumbSize)
        {
            this.thumbSize = thumbSize;
            Size size = ThumbnailHelper.GetThumbnailControlSize(thumbSize);

            this.pnlThumbs.SuspendLayout();

            foreach (var thumbnail in thumbnails)
                thumbnail.SetSize(size.Width, size.Height);

            this.pnlThumbs.ResumeLayout();
        }

        public void RefreshUICulture()
        {
        }
        #endregion

        #region Layout

        /// <summary>
        /// Reorganize the list of thumbnails to match the summaries.
        /// </summary>
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
                    // We already have a thumbnail control for this camera.
                    thumbnails[index].UpdateSummary(summary);
                    
                    if (!hidden && refreshThumbnailsOfKnownCameras)
                    {
                        summary.Manager.StartThumbnail(summary);
                    }

                    continue;
                }
                
                // New camera, add it to the list and start async thumbnail retrieval.
                updated = true;
                AddThumbnail(new ThumbnailCamera(summary));

                if (!hidden)
                {
                    summary.Manager.StartThumbnail(summary);
                }
            }
            
            refreshThumbnailsOfKnownCameras = false;
            
            // Remove cameras that were disconnected.
            List<ThumbnailCamera> lost = new List<ThumbnailCamera>();
            foreach(ThumbnailCamera thumbnail in thumbnails)
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

            if (selectedThumbnail == null && thumbnails.Count > 0)
                thumbnails[0].SetSelected();
            
            return updated;
        }
        
        /// <summary>
        /// Find the index of a thumbnail control by the camera identifier.
        /// </summary>
        private int IndexOf(string identifier)
        {
            for(int i = 0; i<thumbnails.Count; i++)
                if(thumbnails[i].Summary.Identifier == identifier)
                    return i;
            
            return -1;
        }
        
        private void DoLayout()
        {
            this.pnlThumbs.SuspendLayout();
            this.pnlThumbs.Controls.Clear();
            this.pnlThumbs.Controls.AddRange(thumbnails.ToArray());
            this.pnlThumbs.ResumeLayout();
        }

        private void AddThumbnail(ThumbnailCamera thumbnail)
        {
            thumbnail.LaunchCamera += Thumbnail_LaunchCamera;
            thumbnail.CameraSelected += Thumbnail_CameraSelected;
            thumbnail.SummaryUpdated += Thumbnail_SummaryUpdated;
            thumbnail.DeleteCamera += Thumbnail_DeleteCamera;

            Size size = ThumbnailHelper.GetThumbnailControlSize(thumbSize);
            thumbnail.SetSize(size.Width, size.Height);
            thumbnails.Add(thumbnail);
            this.pnlThumbs.Controls.Add(thumbnail);
        }

        private void RemoveThumbnail(ThumbnailCamera thumbnail)
        {
            if (imageReceived.Contains(thumbnail))
                imageReceived.Remove(thumbnail);

            thumbnail.LaunchCamera -= Thumbnail_LaunchCamera;
            thumbnail.CameraSelected -= Thumbnail_CameraSelected;
            thumbnail.SummaryUpdated -= Thumbnail_SummaryUpdated;
            thumbnail.DeleteCamera -= Thumbnail_DeleteCamera;

            this.pnlThumbs.Controls.Remove(thumbnail);
            thumbnails.Remove(thumbnail);
            if (selectedThumbnail == thumbnail)
                selectedThumbnail = null;

            thumbnail.Dispose();
        }
        #endregion


        private void UpdateThumbnailImage(CameraSummary summary, Bitmap image)
        {
            if(summary == null)
                return;
            
            log.DebugFormat("UpdateThumbnailImage for {0}.", summary.Alias);
            int index = IndexOf(summary.Identifier);
            if(index < 0)
                return;
                
            bool hasImage = thumbnails[index].Image != null;
            thumbnails[index].UpdateImage(image);
            
            if(hasImage)
                return;

            imageReceived.Add(thumbnails[index]);
            int percentage = (int)(((float)imageReceived.Count / thumbnails.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            
            if(imageReceived.Count >= thumbnails.Count && AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);
        }

        #region Thumbnail control event handlers
        private void Thumbnail_LaunchCamera(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;

            if (thumbnail != null)
                CameraTypeManager.LoadCamera(thumbnail.Summary, -1);
        }
        
        private void Thumbnail_DeleteCamera(object sender, EventArgs e)
        {
            // Delete camera in prefs (blurbs).
            // Should be enough to remove the thumbnail at next discovery step.
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
            CameraTypeManager.ForgetCamera(thumbnail.Summary);

            refreshThumbnailsOfKnownCameras = true;

            // Call one discovery step on that specific manager.
            var blurbs = PreferencesManager.CapturePreferences.CameraBlurbs;
            thumbnail.Summary.Manager.DiscoverCameras(blurbs);
        }
        
        private void Thumbnail_CameraSelected(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
        
            if(thumbnail == null || selectedThumbnail == thumbnail)
                return;
                
            if (selectedThumbnail != null)
                selectedThumbnail.SetUnselected();
        
            selectedThumbnail = thumbnail;
        }
        
        private void Thumbnail_SummaryUpdated(object sender, EventArgs e)
        {
            ThumbnailCamera thumbnail = sender as ThumbnailCamera;
            CameraTypeManager.UpdatedCameraSummary(thumbnail.Summary);
        }
        #endregion

        #region Commands
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (thumbnails.Count == 0)
                return base.ProcessCmdKey(ref msg, keyData);

            if (selectedThumbnail == null)
            {
                if (thumbnails.Count > 0 && (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down))
                {
                    thumbnails[0].SetSelected();
                    return true;
                }
                else
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }
            }

            // Keyboard navigation (bypass the hotkey system).
            //int index = IndexOf(selectedThumbnail.Summary.Identifier);
            //int row = index / columns;
            //int col = index - (row * columns);
            bool handled = false;

            switch (keyData)
            {
                //case Keys.Left:
                //    {
                //        if (col > 0)
                //            thumbnails[index - 1].SetSelected();
                //        handled = true;
                //        break;
                //    }
                //case Keys.Right:
                //    {
                //        if (col < columns - 1 && index + 1 < thumbnails.Count)
                //            thumbnails[index + 1].SetSelected();
                //        handled = true;
                //        break;
                //    }
                //case Keys.Up:
                //    {
                //        if (row > 0)
                //            thumbnails[index - columns].SetSelected();
                //        this.ScrollControlIntoView(selectedThumbnail);
                //        handled = true;
                //        break;
                //    }
                //case Keys.Down:
                //    {
                //        if (index + columns < thumbnails.Count)
                //            thumbnails[index + columns].SetSelected();
                //        this.ScrollControlIntoView(selectedThumbnail);
                //        handled = true;
                //        break;
                //    }
                //case Keys.Home:
                //    {
                //        thumbnails[0].SetSelected();
                //        break;
                //    }
                //case Keys.End:
                //    {
                //        thumbnails[thumbnails.Count - 1].SetSelected();
                //        break;
                //    }
                default:
                    break;
            }

            return handled || base.ProcessCmdKey(ref msg, keyData);
        }

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
                    refreshThumbnailsOfKnownCameras = true;
                    CameraTypeManager.DiscoveryStep();
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
