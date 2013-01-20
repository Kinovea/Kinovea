#region License
/*
Copyright © Joan Charmant 2013.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class ThumbnailViewerCameras : UserControl
    {
        #region Events
        //public event EventHandler<CameraLoadAskedEventArgs> CameraLoadAsked;
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        #endregion
        
        #region Members
        private ThumbnailFile selectedThumbnail;
        private int columns = (int)ExplorerThumbSize.Large;
        private Dictionary<string, ThumbnailCamera> thumbnails = new Dictionary<string, ThumbnailCamera>();
        private int imageReceived;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ThumbnailViewerCameras()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            columns = (int)PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize;
        }
        
        public void CamerasDiscovered(List<CameraSummary> summaries)
        {
            UpdateThumbnailList(summaries);
            DoLayout();
        }
        public void CameraImageReceived(CameraSummary summary, Bitmap image)
        {
            UpdateThumbnail(summary, image);
        }
        
        private void UpdateThumbnailList(List<CameraSummary> summaries)
        {
            // TODO: merge instead of reset.
            
            thumbnails.Clear();
            imageReceived = 0;
            if(BeforeLoad != null)
                BeforeLoad(this, EventArgs.Empty);

            foreach(CameraSummary summary in summaries)
            {
                ThumbnailCamera thumbnail = new ThumbnailCamera(summary);
                thumbnail.BackColor = Color.LightGray;
                thumbnails.Add(summary.Identifier, thumbnail);
                this.Controls.Add(thumbnail);
            }
            
            selectedThumbnail = null;
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
            foreach(ThumbnailCamera thumbnail in thumbnails.Values)
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
        
        private void UpdateThumbnail(CameraSummary summary, Bitmap image)
        {
            if(summary == null)
                return;
            
            if(thumbnails.ContainsKey(summary.Identifier))
                thumbnails[summary.Identifier].UpdateImage(image);
            
            imageReceived++;
            int percentage = (int)(((float)imageReceived / thumbnails.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            
            if(imageReceived >= thumbnails.Count && AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);
        }
        
        private void ThumbnailViewerCameras_Resize(object sender, EventArgs e)
        {
            DoLayout();
        }
    }
}
