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

        public bool Editing
        {
            get
            {
                return capturedFileViews.Values.Any(item => item.Editing);
            }
        }
        
        private List<CapturedFile> capturedFiles = new List<CapturedFile>();
        private SortedDictionary<DateTime, CapturedFileView> capturedFileViews = new SortedDictionary<DateTime, CapturedFileView>();
        private float spots = 0;
        private int first = -1;
        private int last = -1;
        private bool alignLeft = true;
        private int top = 10;
        private int margin = 10;
        private int masterMargin = 10;
        private int spotWidth = 100;
        private int baseDuration = 300;
        private ControlAnimator animator = new ControlAnimator();
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        
        public CapturedFilesView()
        {
            InitializeComponent();
            btnLeft.Visible = false;
            btnRight.Visible = false;
            animator.AnimationsFinished += animator_AnimationsFinished;
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

                for (int i = capturedFiles.Count - 1; i >= 0; i--)
                {
                    CapturedFile capturedFile = capturedFiles[i];
                    Control control = capturedFileViews[capturedFile.Time];

                    this.Controls.Remove(control);
                    control.Dispose();

                    capturedFileViews.Remove(capturedFile.Time);

                    capturedFiles.Remove(capturedFile);
                    capturedFile.Dispose();
                }
            }
            
            base.Dispose(disposing);
        }

        public void RefreshUICulture()
        {
            foreach(CapturedFileView capturedFileView in capturedFileViews.Values)
                capturedFileView.RefreshUICulture();
        }
        
        public void AddFile(CapturedFile capturedFile)
        {
            bool known = capturedFiles.Any(c => c.Filename == capturedFile.Filename);
            if (known)
                return;

            this.capturedFiles.Add(capturedFile);

            if (capturedFiles.Count > PreferencesManager.FileExplorerPreferences.MaxRecentCapturedFiles)
                HideCapturedFileView(capturedFiles[0]);

            CapturedFileView view = new CapturedFileView(capturedFile);
            view.LaunchAsked += View_LaunchAsked;
            view.LaunchWatcherAsked += View_LaunchWatcherAsked;
            view.LocateAsked += view_LocateAsked;
            view.SelectAsked += View_Clicked;
            view.HideAsked += View_HideAsked;
            view.DeleteAsked += View_DeleteAsked;
            
            // If we are still aligned with the first image, we keep updating the first visible,
            // otherwise we keep things as is.
            bool alignedToFirst = capturedFileViews.Count == 0 || first == capturedFileViews.Count - 1;
            
            capturedFileViews.Add(capturedFile.Time, view);
            this.Controls.Add(view);
            
            if(alignedToFirst)
            {
                first++;
                spotWidth = view.Width + margin;
            }
            
            RecomputeSpots();
            LayoutThumbnails();
            Invalidate();
        }

        #region Individual views events
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
            PreferencesManager.FileExplorerPreferences.ConsolidateRecentCapturedFiles();
            NotificationCenter.RaiseRefreshFileExplorer(this, true);
        }
        private void View_LaunchAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if(view == null || view.CapturedFile == null || string.IsNullOrEmpty(view.CapturedFile.Filepath))
                return;
            
            VideoTypeManager.LoadVideo(view.CapturedFile.Filepath, -1);
        }
        private void View_LaunchWatcherAsked(object sender, EventArgs e)
        {
            CapturedFileView view = sender as CapturedFileView;
            if (view == null || view.CapturedFile == null || string.IsNullOrEmpty(view.CapturedFile.Filepath))
                return;

            // Replace the filename with a wildcard to turn into a replay watcher over that folder.
            string path = Path.Combine(Path.GetDirectoryName(view.CapturedFile.Filepath), "*");
            VideoTypeManager.LoadVideo(path, -1);
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
            bool alignedToFirst = first == capturedFileViews.Count - 1;

            this.Controls.Remove(capturedFileViews[capturedFile.Time]);
            capturedFileViews.Remove(capturedFile.Time);
            
            capturedFiles.Remove(capturedFile);
            capturedFile.Dispose();
            
            if(alignedToFirst)
                first--;
            
            RecomputeSpots();
            LayoutThumbnails();
            Invalidate();
        }
        private void LayoutThumbnails()
        {
            if(capturedFileViews.Count == 0)
                return;
            
            this.SuspendLayout();
             
            if(alignLeft)
            {
                for(int i = capturedFileViews.Count - 1; i>=0;i--)
                {
                    CapturedFileView view = capturedFileViews.ElementAt(i).Value;
                    
                    // Index is relative to first in view and can be negative.
                    // Elements outside of view to the left will have a negative index and be drawn totally or partially off site.
                    // It's simpler to draw everything, because during animation several partially off site items may come into view.
                    int index = first - i;
                    int left = masterMargin + (index * (view.Width + margin));
                    
                    left += btnLeft.Width;
                    
                    view.Left = left;
                    view.Top = top;
                    view.Visible = true;
                }
            }
            else
            {
                for(int i = 0; i<capturedFileViews.Count;i++)
                {
                    CapturedFileView view = capturedFileViews.ElementAt(i).Value;
                    int index = i - last;
                    int total = masterMargin + (index * (view.Width + margin));
                    
                    total += btnRight.Width;
                        
                    int left = this.Width - total - view.Width;
                    
                    view.Left = left;
                    view.Top = top;
                    view.Visible = true;
                }
            }
            
            this.ResumeLayout();
        }
        private void CapturedFilesViewClick(object sender, EventArgs e)
        {
            foreach(CapturedFileView view in capturedFileViews.Values)
            {
                view.UpdateSelected(false);
                view.Invalidate();
            }
        }
        
        private void BtnRightClick(object sender, EventArgs e)
        {
            if(last > 0)
            {
                last--;
                int pixels = spotWidth;
                
                if(alignLeft)
                {
                    float fitting = (float)Math.Floor(spots);
                    float remainder = spots - fitting;
                    pixels = (int)(spotWidth * remainder);
                }
                
                AnimateRight(pixels);
            }
            
            alignLeft = false;
        }
        
        private void BtnLeftClick(object sender, EventArgs e)
        {
            if(first + 1 < capturedFileViews.Count)
            {
                first++;
                int pixels = spotWidth;
                
                if(!alignLeft)
                {
                    float fitting = (float)Math.Floor(spots);
                    float remainder = spots - fitting;
                    pixels = (int)(spotWidth * remainder);
                }
         
                AnimateLeft(pixels);
            }
            
            alignLeft = true;
            
        }
        
        private void CapturedFilesViewResize(object sender, EventArgs e)
        {
            RecomputeSpots();
            LayoutThumbnails();
            Invalidate();
        }
        private void RecomputeSpots()
        {
            if(capturedFileViews.Count == 0)
                return;

            spots = (float)(this.Width - masterMargin - btnLeft.Width - btnRight.Width) / spotWidth;
            
            btnRight.Visible = false;
            btnLeft.Visible = false;
            if(capturedFileViews.Count > (int)Math.Floor(spots))
            {
                if(alignLeft)
                    last = first - (int)Math.Floor(spots) + 1;
                else
                    first = last + (int)Math.Floor(spots) - 1;
                
                if(last > 0)
                    btnRight.Visible = true;
                
                if(first + 1 < capturedFileViews.Count)
                    btnLeft.Visible = true;
            }
            else
            {
                alignLeft = true;
                first = capturedFileViews.Count - 1;
                last = 0;
            }
        }
        private void AnimateLeft(int pixels)
        {
            animator.Clear();
            foreach(CapturedFileView view in capturedFileViews.Values)
                animator.Animate(view, new Point(pixels, 0), baseDuration);
        }
        private void AnimateRight(int pixels)
        {
            animator.Clear();
            foreach(CapturedFileView view in capturedFileViews.Values)
                animator.Animate(view, new Point(-pixels, 0), baseDuration);
        }
        private void animator_AnimationsFinished(object sender, EventArgs e)
        {
            RecomputeSpots();
            LayoutThumbnails();
            Invalidate();
        }
    }
}
