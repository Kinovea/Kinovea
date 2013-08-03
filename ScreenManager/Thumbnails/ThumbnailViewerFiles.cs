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

using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A thumbnail viewer for files.
    /// Used for explorer and shortcuts content.
    /// </summary>
    public partial class ThumbnailViewerFiles : UserControl
    {
        public event EventHandler<FileLoadAskedEventArgs> FileLoadAsked;
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        
        #region Members
        private int columns = (int)ExplorerThumbSize.Large;
        private object locker = new object();
        private List<SummaryLoader> loaders = new List<SummaryLoader>();
        private List<ThumbnailFile> thumbnails = new List<ThumbnailFile>();
        private List<string> files;
        private ThumbnailFile selectedThumbnail;
        private bool editing;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ThumbnailViewerFiles()
        {
            log.Debug("Constructing ThumbListView");
            
            InitializeComponent();
            RefreshUICulture();
            this.Dock = DockStyle.Fill;
        }
        
        #region Public methods
        public void CurrentDirectoryChanged(List<string> files)
        {
            this.files = files;

            PopulateViewer();
        }
        public void CancelLoading()
        {
            if(AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);

            CleanupLoaders();
        }
        public void Clear()
        {
            selectedThumbnail = null;
            foreach(ThumbnailFile tlvi in thumbnails)
                tlvi.DisposeImages();
            
            thumbnails.Clear();
            this.Controls.Clear();
        }
        public bool OnKeyPress(Keys keyCode)
        {
            bool handled = false;
            
            if(selectedThumbnail == null)
            {
                if(thumbnails.Count > 0)
                    thumbnails[0].SetSelected();
                
                return true;
            }
            
            if(editing)
                return true;
                
            int index = (int)selectedThumbnail.Tag;
            int row = index / columns;
            int col = index - (row * columns);
            
            switch (keyCode)
            {
                case Keys.Left:
                {
                    if(col > 0)
                        thumbnails[index - 1].SetSelected();
                    handled = true;
                    break;
                }
                case Keys.Right:
                {
                    if (col < columns - 1 && index + 1 < thumbnails.Count)
                        thumbnails[index + 1].SetSelected();
                    handled = true;
                    break;
                }
                case Keys.Up:
                {
                    if (row > 0)
                        thumbnails[index - columns].SetSelected();
                    this.ScrollControlIntoView(selectedThumbnail);
                    handled = true;
                    break;
                }
                case Keys.Down:
                {
                    if (index + columns  < thumbnails.Count)
                        thumbnails[index + columns].SetSelected();
                    this.ScrollControlIntoView(selectedThumbnail);
                    handled = true;
                    break;
                }
                case Keys.Return:
                {
                    if (!selectedThumbnail.IsError && FileLoadAsked != null)
                        FileLoadAsked(this, new FileLoadAskedEventArgs(selectedThumbnail.FileName, -1));
                    handled = true;
                    break;
                }   
                case Keys.F2:
                {
                    if(!selectedThumbnail.IsError)
                        selectedThumbnail.StartRenaming();
                    handled = true;
                    break;
                }
                default:
                    break;
            }
            
            return handled;
        }
        public void RefreshUICulture()
        {
            foreach(ThumbnailFile tlvi in thumbnails)
                tlvi.RefreshUICulture();
        }
        public void UpdateThumbnailsSize(ExplorerThumbSize newSize)
        {
            this.columns = (int)newSize;
            if (thumbnails.Count == 0)
                return;

            PopulateViewer();
        }
        #endregion
        
        #region Private methods
        
        #region Organize and Display
        private void PopulateViewer()
        {
            CleanupLoaders();
            Clear();

            if (files.Count == 0)
                return;
            
            CreateThumbs(files);
            Size maxImageSize = DoLayout();

            SummaryLoader sl = new SummaryLoader(files, maxImageSize);
            sl.SummaryLoaded += SummaryLoader_SummaryLoaded;
            loaders.Add(sl);
            if (BeforeLoad != null)
                BeforeLoad(this, EventArgs.Empty);

            sl.Run();
        }
        private void CleanupLoaders()
        {
            for(int i=loaders.Count-1;i>=0;i--)
            {
                loaders[i].SummaryLoaded -= SummaryLoader_SummaryLoaded;
                
                if (loaders[i].IsAlive)
                    loaders[i].Cancel();
                else
                    loaders.RemoveAt(i);
            }
        }
        private void CreateThumbs(List<String> _fileNames)
        {
            int index = 0;
            foreach(string file in _fileNames)
            {
                ThumbnailFile tlvi = new ThumbnailFile(file);
                tlvi.LaunchVideo += ThumbListViewItem_LaunchVideo;
                tlvi.VideoSelected += ThumbListViewItem_VideoSelected;
                tlvi.FileNameEditing += ThumbListViewItem_FileNameEditing;
                tlvi.Tag = index;
                thumbnails.Add(tlvi);
                this.Controls.Add(tlvi);
                index++;
            }
        }
        private void SummaryLoader_SummaryLoaded(object sender, SummaryLoadedEventArgs e)
        {
            // One of the summaries was loaded, push it into its thumbnail.
            if(e.Summary == null)
                return;
         
            // TODO: keep the controls in a dictionary indexed by the filename instead of a raw list.
            foreach(ThumbnailFile thumbnail in thumbnails)
            {
                if(thumbnail.FileName == e.Summary.Filename)
                {
                    thumbnail.Populate(e.Summary);
                    thumbnail.Invalidate();
                }
            }
            
            int done = e.Progress+1;
            int percentage = (int)(((float)done / thumbnails.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
            
            if(done >= thumbnails.Count && AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);
        }
        
        private Size DoLayout()
        {
            int leftMargin = 30;
            int rightMargin = 20;
            int topMargin = 5;
            
            int colWidth = (this.Width - leftMargin - rightMargin) / columns;
            int spacing = colWidth / 20;

            int thumbWidth = colWidth - spacing;
            int thumbHeight = (thumbWidth * 3 / 4) + 15;
            Size maxImageSize = new Size(thumbWidth, thumbHeight);

            int current = 0;
            this.SuspendLayout();
            foreach(ThumbnailFile tlvi in SortedAndFilteredThumbs())
            {
                tlvi.SetSize(thumbWidth, thumbHeight);
                maxImageSize = tlvi.MaxImageSize(tlvi.Size);

                int row = current / columns;
                int col = current - (row * columns);
                int left = col * colWidth + leftMargin;
                int top = topMargin + (row * (thumbHeight + spacing));
                tlvi.Location = new Point(left, top);
                current++;
            }
            this.ResumeLayout();

            return maxImageSize;
        }
        private IEnumerable<ThumbnailFile> SortedAndFilteredThumbs()
        {
            foreach(ThumbnailFile tlvi in thumbnails)
                yield return tlvi;
        }
        #endregion

        #region Thumbnails items events handlers
        private void ThumbListViewItem_LaunchVideo(object sender, EventArgs e)
        {
            CancelEditMode();
            ThumbnailFile tlvi = sender as ThumbnailFile;
            
            if (tlvi != null && !tlvi.IsError && FileLoadAsked != null)
                FileLoadAsked(this, new FileLoadAskedEventArgs(tlvi.FileName, -1));
        }
        private void ThumbListViewItem_VideoSelected(object sender, EventArgs e)
        {
            CancelEditMode();
            ThumbnailFile tlvi = sender as ThumbnailFile;
            
            if(tlvi != null)
            {
                if (selectedThumbnail != null && selectedThumbnail != tlvi )
                    selectedThumbnail.SetUnselected();
            
                selectedThumbnail = tlvi;
            }
        }
        private void ThumbListViewItem_FileNameEditing(object sender, EditingEventArgs e)
        {
            // Make sure the keyboard handling doesn't interfere 
            // if one thumbnail is in edit mode.
            // There should only be one thumbnail in edit mode at a time.
            editing = e.Editing;
        }
        #endregion

        private void SavePrefs()
        {
            PreferencesManager.FileExplorerPreferences.ExplorerThumbsSize = (ExplorerThumbSize)columns;
            PreferencesManager.Save();
        }
        
        private void Panel2MouseDown(object sender, MouseEventArgs e)
        {
            // Clicked off nowhere.
            if(selectedThumbnail != null)
            {
                selectedThumbnail.SetUnselected();
                selectedThumbnail = null;
            }
            
            CancelEditMode();
        }
        
        private void CancelEditMode()
        {
            editing = false;
            
            // Browse all thumbs and make sure they are all in normal mode.
            for (int i = 0; i < this.Controls.Count; i++)
            {
                ThumbnailFile tlvi = this.Controls[i] as ThumbnailFile;
                if(tlvi != null)
                    tlvi.CancelEditMode();
            }	
        }
        private void Panel2MouseEnter(object sender, EventArgs e)
        {
            // Give focus to enbale mouse scroll
            //if(!editMode)
            //	this.Focus();
        }
        #endregion
        
        
        private void ThumbnailViewerFiles_Resize(object sender, EventArgs e)
        {
            // When manually resizing the control, we don't trigger the full populate.
            if(this.Visible)
                DoLayout();
        }
    }
}
