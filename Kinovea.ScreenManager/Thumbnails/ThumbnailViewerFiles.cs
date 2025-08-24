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
using System.Threading;
using System.IO;

using Kinovea.Services;
using Kinovea.ScreenManager.Languages;
using System.Diagnostics;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// A thumbnail viewer for files.
    /// Used for explorer and shortcuts content.
    /// </summary>
    public partial class ThumbnailViewerFiles : KinoveaControl
    {
        #region Events
        public event EventHandler<FileLoadAskedEventArgs> FileLoadAsked;
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        #endregion

        #region Members
        private string identifier;
        private int columns = (int)ExplorerThumbSize.Large;
        private object locker = new object();
        private List<SummaryLoader> loaders = new List<SummaryLoader>();
        private List<ThumbnailFile> thumbnails = new List<ThumbnailFile>();
        private string path;
        private List<string> files;
        private ThumbnailFile selectedThumbnail;
        private bool editing;
        private bool externalSelection;
        private string lastSelectedFile;
        private bool sortOperationInProgress;
        private bool forcedRefreshInProgress;
        private Dictionary<string, ThumbnailFile> mapPathToThumbnail = new Dictionary<string, ThumbnailFile>();
        private Dictionary<string, int> mapPathToIndex = new Dictionary<string, int>();
        private Stopwatch stopwatch = new Stopwatch();


        #region Menus
        private ContextMenuStrip popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuSortBy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByName = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByDate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortBySize = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortAscending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortDescending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuProperties = new ToolStripMenuItem();
        #endregion
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction/Destruction
        public ThumbnailViewerFiles(string identifier)
        {
            log.Debug("Constructing ThumbnailViewerFiles");
            
            this.identifier = identifier;
            InitializeComponent();
            RefreshUICulture();
            this.Dock = DockStyle.Fill;

            NotificationCenter.FileSelected += NotificationCenter_FileSelected;

            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("ThumbnailViewerFiles");

            this.ContextMenuStrip = popMenu;
            BuildContextMenus();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Reload the panel with a new list of files.
        /// </summary>
        public void CurrentDirectoryChanged(string path, List<string> files)
        {
            log.DebugFormat("Thumbnail viewer directory change:{0}. Size:{1}. ---------------------------", 
                path, this.Size);

            if (this.Width < 200 || this.Height < 200)
            {
                // This happens when we switch for the very first time to this panel.
                // It's not fully initialized yet. We'll come back later.
                return;
            }

            int visibleThumbnails = thumbnails.Count(th => th.Visible);

            // Bail out if we consider that we don't have to do anything.
            // Same directory, same number of files, not a sort, not a forced refresh.
            // This happens when we come back here after a screen is closed for example.
            if (path == this.path && 
                files.Count == this.files.Count &&
                visibleThumbnails >= files.Count &&
                !sortOperationInProgress &&
                !forcedRefreshInProgress)
            {
                log.DebugFormat("Reloading current directory in place.");
                return;
            }

            this.path = path;
            this.files = files;
            this.sortOperationInProgress = false;
            this.forcedRefreshInProgress = false;
            PopulateViewer(false);
        }

        /// <summary>
        /// Loading was cancelled from the outside, for example when we change folder or start a video.
        /// </summary>
        public void CancelLoading()
        {
            if(AfterLoad != null)
                AfterLoad(this, EventArgs.Empty);

            CleanupLoaders();
        }
        
        /// <summary>
        /// Clear all thumbnails memory and remove all controls from the panel.
        /// </summary>
        public void Clear()
        {
            // It is not clear that this is needed.
            // Currently we only come here when changing tabs.

            log.DebugFormat("Clearing thumbnails from {0}", identifier);
            selectedThumbnail = null;
            NotificationCenter.RaiseFileSelected(this, null);

            mapPathToIndex.Clear();
            mapPathToThumbnail.Clear();

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

        public void UpdateThumbnailsSize(ExplorerThumbSize newSize)
        {
            this.columns = (int)newSize;
            if (thumbnails.Count == 0)
                return;

            PopulateViewer(true);
        }
        #endregion

        #region Private methods

        private void BuildContextMenus()
        {

            // Sort by options.
            mnuSortBy.Image = Properties.Resources.sort;
            mnuSortByName.Click += (s, e) => UpdateSortAxis(FileSortAxis.Name);
            mnuSortByDate.Click += (s, e) => UpdateSortAxis(FileSortAxis.Date);
            mnuSortBySize.Click += (s, e) => UpdateSortAxis(FileSortAxis.Size);
            mnuSortAscending.Click += (s, e) => UpdateSortAscending(true);
            mnuSortDescending.Click += (s, e) => UpdateSortAscending(false);

            mnuSortBy.DropDownItems.AddRange(new ToolStripItem[]
            {
                mnuSortByName,
                mnuSortByDate,
                mnuSortBySize,
                new ToolStripSeparator(),
                mnuSortAscending,
                mnuSortDescending
            });

            mnuProperties.Image = Properties.Resources.information;

            // Properties menus are built dynamically from the enum.
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
                    var visibilityOptions = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible;

                    bool v = visibilityOptions[closureProp];
                    visibilityOptions[closureProp] = !v;
                    mnu.Checked = !v;
                    InvalidateThumbnails(visibilityOptions);
                };

                mnuProperties.DropDownItems.Add(mnu);
            }


            popMenu.Items.Add(mnuSortBy);
            popMenu.Items.Add(mnuProperties);
        }

        private void PrepareSortMenus()
        {
            FileSortAxis axis = PreferencesManager.FileExplorerPreferences.FileSortAxis;
            bool ascending = PreferencesManager.FileExplorerPreferences.FileSortAscending;

            mnuSortByName.Checked = axis == FileSortAxis.Name;
            mnuSortByDate.Checked = axis == FileSortAxis.Date;
            mnuSortBySize.Checked = axis == FileSortAxis.Size;
            mnuSortAscending.Checked = ascending;
            mnuSortDescending.Checked = !ascending;
        }

        #region Organize and Display
        
        
        /// <summary>
        /// Populate the panel with the thumbnail controls and start loading the summaries.
        /// </summary>
        private void PopulateViewer(bool changedSize)
        {
            stopwatch.Restart();
            CleanupLoaders();
            log.DebugFormat("After loaders cleaned up: {0} ms.", stopwatch.ElapsedMilliseconds);

            selectedThumbnail = null;
            UpdateThumbnailList(files);
            log.DebugFormat("After thumbnail list updated: {0} in {1} ms.", files.Count, stopwatch.ElapsedMilliseconds);

            if (files.Count == 0)
                return;

            Size maxImageSize = DoLayout(changedSize || forcedRefreshInProgress);
            log.DebugFormat("After thumbnail layout: {0} in {1} ms. Max img size:{2}.", 
                files.Count, stopwatch.ElapsedMilliseconds, maxImageSize);

            // Filter out files that are already loaded.
            List<string> filesToLoad = new List<string>();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (!mapPathToIndex.ContainsKey(file))
                {
                    // This should never happen.
                    continue;
                }

                // Check last write time.
                // If the control is being recycled from a different file it will have reset this.
                bool updated = thumbnails[mapPathToIndex[file]].LastWriteUTC != File.GetLastWriteTimeUtc(file);
                if (updated || changedSize || forcedRefreshInProgress)
                {
                    filesToLoad.Add(file);
                }
            }

            log.DebugFormat("Summaries to load: {0}/{1}. Active loaders:{2}.", 
                filesToLoad.Count, files.Count, loaders.Count);

            if (filesToLoad.Count == 0)
                return;

            SummaryLoader sl = new SummaryLoader(filesToLoad, maxImageSize);
            sl.SummaryLoaded += SummaryLoader_SummaryLoaded;
            loaders.Add(sl);

            if (BeforeLoad != null)
                BeforeLoad(this, EventArgs.Empty);

            log.DebugFormat("Thumbnail panel populated in {0} ms. Starting summary loader.", stopwatch.ElapsedMilliseconds);
            sl.Run();
        }

        /// <summary>
        /// Reorganize the list of thumbnails to match the requested list of files.
        /// Makes sure we have enough thumbnail controls and hide/show them as necessary.
        /// Set the correct filename on each control.
        /// Rearrange the controls holding returning thumbnails to match the requested order.
        /// </summary>
        private void UpdateThumbnailList(List<string> files)
        {
            this.SuspendLayout();

            // If we come here after a sort, add or delete, we already have most of the thumbnails already.
            // We try to swap the existing ones around.
            // We don't verify if the file has been modified in place just yet, we'll do that later.
            // - The list `files` is the target list of files to display, in the correct order.
            // - thumbnails is our internal list of thumbnail controls, in the current order.
            // - this.Controls is the same list as thumbnails.

            // Algo
            // - First we figure which files we already have and which ones are new.
            // - Then we update the map of target paths to thumbnails.
            // - During this loop we may recycle existing thumbnail controls or create new ones.
            // - Next we rearrange the thumbnails list to match the target file list.
            // - Finally we hide the extra thumbnails we won't need.

            if (thumbnails.Count != mapPathToIndex.Count)
            {
                // invalid program.
                mapPathToThumbnail.Clear();
                mapPathToIndex.Clear();
            }

            List<bool> inUse = new List<bool>();
            foreach (ThumbnailFile tlvi in thumbnails)
            {
                // We start with all thumbnails flagged as unused.
                inUse.Add(false);
            }

            // Maps target indices in the goal list to source indices in the current list.
            Dictionary<int, int> mapIndices = new Dictionary<int, int>();
            for (int i = 0; i < files.Count; i++)
            {
                if (mapPathToIndex.ContainsKey(files[i]))
                {
                    // We already have this file.
                    int knownIndex = mapPathToIndex[files[i]];
                    mapIndices.Add(i, knownIndex);
                    inUse[knownIndex] = true;
                }
                else
                {
                    mapIndices.Add(i, -1);
                }
            }

            // The goal of this step is to update the mapPathToIndex list,
            // recycle existing thumbnail controls if possible, create new ones if necessary.
            mapPathToIndex.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                if (mapIndices[i] != -1)
                {
                    // We already know this file, point to it.
                    mapPathToIndex.Add(files[i], mapIndices[i]);
                }
                else
                {
                    // We don't know this file, find the first thumbnail control
                    // that won't be used and recycle it, or create a new one.
                    // Note: we don't really need to update mapIndices at this point, useful for debugging.
                    int foundUnused = -1;
                    for (int j = 0; j < thumbnails.Count; j++)
                    {
                        if (!inUse[j])
                        {
                            foundUnused = j;
                            break;
                        }
                    }

                    if (foundUnused != -1)
                    {
                        // We found a thumbnail control that we won't be using, reassign it.
                        thumbnails[foundUnused].FilePath = files[i];
                        mapIndices[i] = foundUnused;
                        inUse[foundUnused] = true;

                        mapPathToIndex.Add(files[i], foundUnused);
                    }
                    else
                    {
                        // We couldn't find any thumbnail control to use, create a new one.
                        ThumbnailFile tlvi = new ThumbnailFile(files[i]);
                        tlvi.LaunchVideo += ThumbListViewItem_LaunchVideo;
                        tlvi.VideoSelected += ThumbListViewItem_VideoSelected;
                        tlvi.FileNameEditing += ThumbListViewItem_FileNameEditing;
                        thumbnails.Add(tlvi);
                        tlvi.Tag = thumbnails.Count - 1;
                        this.Controls.Add(tlvi);
                        mapIndices[i] = thumbnails.Count - 1;
                        inUse.Add(true);

                        mapPathToIndex.Add(files[i], thumbnails.Count - 1);
                    }
                }
            }

            // At this point we are sure to have controls for all the required files.
            // And we may even have extra controls that we won't be using.
            // Arrange the thumbnails list so that it matches the order of the target files.
            mapPathToThumbnail.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                string path = files[i];
                if (mapPathToIndex[path] != i)
                {
                    // Swap.
                    var temp = thumbnails[i];
                    int oldIndex = mapPathToIndex[path];
                    thumbnails[i] = thumbnails[oldIndex];
                    thumbnails[i].Tag = i;
                    thumbnails[oldIndex] = temp;
                    thumbnails[oldIndex].Tag = oldIndex;

                    mapPathToIndex[path] = i;
                    mapPathToIndex[thumbnails[oldIndex].FilePath] = oldIndex;
                }

                mapPathToThumbnail.Add(path, thumbnails[i]);
                thumbnails[i].Visible = true;
            }

            // Hide the extra unused controls.
            for (int i = files.Count; i < thumbnails.Count; i++)
            {
                thumbnails[i].Visible = false;
            }

            this.ResumeLayout();
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


        /// <summary>
        /// One of the summaries was extracted, push it into its thumbnail.
        /// </summary>
        private void SummaryLoader_SummaryLoaded(object sender, SummaryLoadedEventArgs e)
        {
            // This runs in the UI thread.
            if (e.Summary == null)
            {
                // Unexpected error.
            }
            else if (!mapPathToThumbnail.ContainsKey(e.Summary.Filename))
            {
                log.ErrorFormat("Thumbnail control not found for file: {0}", e.Summary.Filename);
            }
            else if (mapPathToThumbnail[e.Summary.Filename].FilePath != e.Summary.Filename)
            {
                log.ErrorFormat("Thumbnail control found but assigned the wrong file: {0}, expected:{1}.",
                    Path.GetFileName(mapPathToThumbnail[e.Summary.Filename].FilePath), Path.GetFileName(e.Summary.Filename));
            }
            else
            {
                ThumbnailFile thumbnail = mapPathToThumbnail[e.Summary.Filename];
                thumbnail.Populate(e.Summary);
                thumbnail.Invalidate();
                if (thumbnail.FilePath == lastSelectedFile)
                    thumbnail.SetSelected();
            }

            // Update the progress bar.
            int done = e.Progress+1;
            int percentage = (int)(((float)done / files.Count) * 100);
            
            if(ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(percentage, null));

            // Check if we are done.
            if (done >= thumbnails.Count && AfterLoad != null)
            {
                if (selectedThumbnail == null && thumbnails.Count > 0)
                    thumbnails[0].SetSelected();

                AfterLoad(this, EventArgs.Empty);
            }
        }
        

        /// <summary>
        /// Layout the list of individual thumbnail controls on the panel.
        /// </summary>
        /// <returns></returns>
        private Size DoLayout(bool changedSize)
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
            foreach(ThumbnailFile tlvi in thumbnails)
            {
                // Size update is relatively costly so only do it if strictly necessary.
                // If we are reloading the list of files it will be done later anyway.
                if (changedSize)
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

        /// <summary>
        /// When the thumbnails must be redrawn but the path and files haven't changed.
        /// This happens when we change the visible properties.
        /// </summary>
        private void InvalidateThumbnails(Dictionary<FileProperty, bool> visibilityOptions)
        {
            foreach (ThumbnailFile tf in thumbnails)
            {
                tf.RefreshUICulture(visibilityOptions);
                tf.Invalidate();
            }
        }
        #endregion

        /// <summary>
        /// A file was selected from the explorer, alert the corresponding thumbnail.
        /// </summary>
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
                if (tlvi.FilePath == e.File)
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
                FileLoadAsked(this, new FileLoadAskedEventArgs(tlvi.FilePath, -1));
        }
        private void ThumbListViewItem_VideoSelected(object sender, EventArgs e)
        {
            CancelEditMode();
            ThumbnailFile tlvi = sender as ThumbnailFile;

            if (tlvi == null || selectedThumbnail == tlvi)
                return;

            Deselect(false);
            selectedThumbnail = tlvi;
            lastSelectedFile = tlvi.FilePath;
            if (!externalSelection)
            {
                // Force focus so the hotkeys can be received.
                // Select the control so the whole page doesn't jump up to the top when we set focus.
                tlvi.Select();
                this.Focus();
                NotificationCenter.RaiseFileSelected(this, tlvi.FilePath);
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

        public void RefreshUICulture()
        {
            var visibilityOptions = PreferencesManager.FileExplorerPreferences.FilePropertyVisibility.Visible;
            foreach (ThumbnailFile tf in thumbnails)
            {
                tf.RefreshUICulture(visibilityOptions);
            }

            mnuSortBy.Text = ScreenManagerLang.mnuSortBy;
            mnuSortByName.Text = ScreenManagerLang.mnuSortBy_Name;
            mnuSortByDate.Text = ScreenManagerLang.mnuSortBy_Date;
            mnuSortBySize.Text = ScreenManagerLang.mnuSortBy_Size;
            mnuSortAscending.Text = ScreenManagerLang.mnuSortBy_Ascending;
            mnuSortDescending.Text = ScreenManagerLang.mnuSortBy_Descending;

            mnuProperties.Text = ScreenManagerLang.mnuProperties;

            foreach (ToolStripMenuItem mnu in mnuProperties.DropDownItems)
            {
                FileProperty prop = (FileProperty)mnu.Tag;
                string resourceName = "FileProperty_" + prop.ToString();
                string text = ScreenManagerLang.ResourceManager.GetString(resourceName);
                mnu.Text = text;
            }
        }

        /// <summary>
        /// Clicked in the background of the panel.
        /// </summary>
        private void Panel2MouseDown(object sender, MouseEventArgs e)
        {
            Deselect(true);
            CancelEditMode();
            PrepareSortMenus();
        }

        /// <summary>
        /// Deselect the currently selected thumbnail if any.
        /// </summary>
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
        
        /// <summary>
        /// The whole control was resized, we just need to redo layout.
        /// </summary>
        private void ThumbnailViewerFiles_Resize(object sender, EventArgs e)
        {
            if(this.Visible)
                DoLayout(false);
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

        private void UpdateSortAxis(FileSortAxis axis)
        {
            PreferencesManager.FileExplorerPreferences.FileSortAxis = axis;
            sortOperationInProgress = true;
            NotificationCenter.RaiseRefreshFileExplorer(this, true);
        }

        private void UpdateSortAscending(bool ascending)
        {
            PreferencesManager.FileExplorerPreferences.FileSortAscending = ascending;
            sortOperationInProgress = true;
            NotificationCenter.RaiseRefreshFileExplorer(this, true);
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
                    forcedRefreshInProgress = true;
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
                FileLoadAsked(this, new FileLoadAskedEventArgs(selectedThumbnail.FilePath, -1));
        }
        
        private void CommandDelete()
        {
            if (selectedThumbnail != null && !selectedThumbnail.IsError)
                selectedThumbnail.Delete();
        }
        #endregion
    }
}
