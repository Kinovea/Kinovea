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
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public partial class CapturedFilesView : UserControl
    {
        public bool HasThumbnails
        {
            get { return capturedFiles.Count > 0; }
        }
        
        private List<CapturedFile> capturedFiles = new List<CapturedFile>();
        private SortedDictionary<DateTime, CapturedFileView> capturedFileViews = new SortedDictionary<DateTime, CapturedFileView>();
        
        public CapturedFilesView()
        {
            InitializeComponent();
        }
        
        public void RefreshUICulture()
        {
            foreach(CapturedFileView capturedFileView in capturedFileViews.Values)
                capturedFileView.RefreshUICulture();
        }
        
        public void AddFile(CapturedFile capturedFile)
        {
            this.capturedFiles.Add(capturedFile);
            
            CapturedFileView view = new CapturedFileView(capturedFile);
            view.SelectAsked += View_Clicked;
            view.LaunchAsked += View_LaunchAsked;
            view.LocateAsked += view_LocateAsked;
            view.HideAsked += View_HideAsked;
            view.DeleteAsked += View_DeleteAsked;
            
            capturedFileViews.Add(capturedFile.Time, view);
            this.Controls.Add(view);
            
            OrganizeView();
        }

        #region view events
        private void View_Clicked(object sender, EventArgs e)
        {
            foreach(CapturedFileView view in capturedFileViews.Values)
            {
                view.UpdateSelected(view == sender);
                view.Invalidate();
            }
        }
        private void View_HideAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if(view == null)
                return;
            
            HideCapturedFileView(view.CapturedFile);
        }
        private void View_DeleteAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if(view == null || view.CapturedFile == null || string.IsNullOrEmpty(view.CapturedFile.Filepath))
                return;
            
            FilesystemHelper.DeleteFile(view.CapturedFile.Filepath);
            
            if(File.Exists(view.CapturedFile.Filepath))
                return;
            
            HideCapturedFileView(view.CapturedFile);
            NotificationCenter.RaiseRefreshFileExplorer(this, true);
        }
        private void View_LaunchAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if(view == null || view.CapturedFile == null || string.IsNullOrEmpty(view.CapturedFile.Filepath))
                return;
            
            VideoTypeManager.LoadVideo(view.CapturedFile.Filepath, -1);
        }
        private void view_LocateAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if(view == null || view.CapturedFile == null || string.IsNullOrEmpty(view.CapturedFile.Filepath))
                return;
            
            FilesystemHelper.LocateFile(view.CapturedFile.Filepath);
        }
        #endregion
        
        private void HideCapturedFileView(CapturedFile capturedFile)
        {
            this.Controls.Remove(capturedFileViews[capturedFile.Time]);
            capturedFileViews.Remove(capturedFile.Time);
            
            capturedFiles.Remove(capturedFile);
            capturedFile.Dispose();
            
            OrganizeView();
            Invalidate();
        }
        private void OrganizeView()
        {
            // Entries in capturedFileViews are sorted on creation time.
            int top = 7;
            int margin = 10;
            int item = 0;
            
            for(int i = capturedFileViews.Count - 1; i>=0; i--)
            {
                CapturedFileView view = capturedFileViews.ElementAt(i).Value;
                int left = margin + (item * (view.Width + margin));
                view.Left = left;
                view.Top = top;
                item++;
            }
        }
        
        private void CapturedFilesViewClick(object sender, EventArgs e)
        {
            foreach(CapturedFileView view in capturedFileViews.Values)
            {
                view.UpdateSelected(false);
                view.Invalidate();
            }
        }
    }
}
