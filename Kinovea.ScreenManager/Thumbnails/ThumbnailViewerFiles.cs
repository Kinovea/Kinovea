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

using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A thumbnail viewer for files.
    /// Used for explorer and shortcuts content.
    /// </summary>
    public partial class ThumbnailViewerFiles : KinoveaControl
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
        private bool externalSelection;
        private string lastSelectedFile;
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public ThumbnailViewerFiles()
        {
            log.Debug("Constructing ThumbnailViewerFiles");
            
            InitializeComponent();
            RefreshUICulture();
            this.Dock = DockStyle.Fill;

            NotificationCenter.FileSelected += NotificationCenter_FileSelected;

            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("ThumbnailViewerFiles");

            this.ContextMenuStrip = popMenu;
            BuildContextMenus();
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
            NotificationCenter.RaiseFileSelected(this, null);
            
            for (int i = thumbnails.Count - 1; i >= 0; i--)
            {
                ThumbnailFile thumbnail = thumbnails[i];

                thumbnail.LaunchVideo -= ThumbListViewItem_LaunchVideo;
                thumbnail.VideoSelected -= ThumbListViewItem_VideoSelected;
                thumbnail.FileNameEditing -= ThumbListViewItem_FileNameEditing;

                thumbnails.Remove(thumbnail);
                this.Controls.Remove(thumbnail);
                
                thumbnail.DisposeImages();
                thumbnail.Dispose();
            }
        }

        public void RefreshUICulture()
        {
            foreach(ThumbnailFile tlvi in thumbnails)
                tlvi.RefreshUICulture();

            foreach (ToolStripMenuItem mnu in popMenu.Items)
            {
                FileProperty prop = (FileProperty)mnu.Tag;
                string resourceName = "FileProperty_" + prop.ToString();
                string text = ScreenManagerLang.ResourceManager.GetString(resourceName);
                mnu.Text = text;
            }
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

        private void BuildContextMenus()
        {
            foreach (FileProperty prop in Enum.GetValues(typeof(FileProperty)))
            {
                if (!PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible.ContainsKey(prop))
                    continue;

                ToolStripMenuItem mnu = new ToolStripMenuItem();
                
                string resourceName = "FileProperty_" + prop.ToString();
                string text = ScreenManagerLang.ResourceManager.GetString(resourceName);
                mnu.Text = text;
                mnu.Tag = prop;

                bool value = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible[prop];
                mnu.Checked = value;

                FileProperty closureProp = prop;
                mnu.Click += (s, e) =>
                {
                    bool v = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible[closureProp];
                    PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible[closureProp] = !v;
                    mnu.Checked = !v;
                    InvalidateThumbnails();
                };

                popMenu.Items.Add(mnu);
            }
        }

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
                    
                    if (thumbnail.FileName == lastSelectedFile)
                        thumbnail.SetSelected();
                }
            }
            
            int done = e.Progress+1;
            int percentage = (int)(((float)done / thumbnails.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));

            if (done >= thumbnails.Count && AfterLoad != null)
            {
                if (selectedThumbnail == null && thumbnails.Count > 0)
                    thumbnails[0].SetSelected();

                AfterLoad(this, EventArgs.Empty);
            }
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
        private void InvalidateThumbnails()
        {
            // When the thumbnails must be redrawn but the file hasn't changed. 
            // For example when the user changes the visibility of file properties.
            foreach (ThumbnailFile tf in thumbnails)
                tf.Invalidate();
        }
        #endregion

        private void NotificationCenter_FileSelected(object sender, FileActionEventArgs e)
        {
            if (sender == this)
                return;

            if(string.IsNullOrEmpty(e.File))
            {
                Deselect(false);
                return;
            }

            foreach (ThumbnailFile tlvi in thumbnails)
            {
                if (tlvi.FileName == e.File)
                {
                    externalSelection = true;
                    tlvi.SetSelected();
                    break;
                }
            }
        }

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

            if (tlvi == null || selectedThumbnail == tlvi)
                return;

            Deselect(false);
            selectedThumbnail = tlvi;
            lastSelectedFile = tlvi.FileName;
            if (!externalSelection)
            {
                // Force focus so the hotkeys can be received.
                // Select the control so the whole page doesn't jump up to the top when we set focus.
                tlvi.Select();
                this.Focus();
                NotificationCenter.RaiseFileSelected(this, tlvi.FileName);
            }
            else
            {
                this.ScrollControlIntoView(tlvi);
            }

            externalSelection = false;
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
            Deselect(true);
            CancelEditMode();
        }

        private void Deselect(bool raiseEvent)
        {
            if (selectedThumbnail == null)
                return;
            
            selectedThumbnail.SetUnselected();
            selectedThumbnail = null;

            if (raiseEvent)
                NotificationCenter.RaiseFileSelected(this, null);
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
        #endregion
        
        private void ThumbnailViewerFiles_Resize(object sender, EventArgs e)
        {
            // When manually resizing the control, we don't trigger the full populate.
            if(this.Visible)
                DoLayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (editing)
                return base.ProcessCmdKey(ref msg, keyData);

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
            int index = (int)selectedThumbnail.Tag;
            int row = index / columns;
            int col = index - (row * columns);
            bool handled = false;

            switch (keyData)
            {
                case Keys.Left:
                    {
                        if (col > 0)
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
                        if (index + columns < thumbnails.Count)
                            thumbnails[index + columns].SetSelected();
                        this.ScrollControlIntoView(selectedThumbnail);
                        handled = true;
                        break;
                    }
                case Keys.Home:
                    {
                        thumbnails[0].SetSelected();
                        break;
                    }
                case Keys.End:
                    {
                        thumbnails[thumbnails.Count - 1].SetSelected();
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
            ThumbnailViewerFilesCommands command = (ThumbnailViewerFilesCommands)cmd;

            switch (command)
            {
                case ThumbnailViewerFilesCommands.RenameSelected:
                    CommandRename();
                    break;
                case ThumbnailViewerFilesCommands.LaunchSelected:
                    CommandLaunch();
                    break;
                case ThumbnailViewerFilesCommands.DeleteSelected:
                    CommandDelete();
                    break;
                case ThumbnailViewerFilesCommands.Refresh:
                    NotificationCenter.RaiseRefreshFileExplorer(this, true);
                    break;
                default:
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        private void CommandRename()
        {
            if (selectedThumbnail != null && !selectedThumbnail.IsError)
                selectedThumbnail.StartRenaming();
        }

        private void CommandLaunch()
        {
            if (selectedThumbnail != null && !selectedThumbnail.IsError && FileLoadAsked != null)
                FileLoadAsked(this, new FileLoadAskedEventArgs(selectedThumbnail.FileName, -1));
        }
        
        private void CommandDelete()
        {
            if (selectedThumbnail != null && !selectedThumbnail.IsError)
                selectedThumbnail.Delete();
        }
        #endregion
    }
}
