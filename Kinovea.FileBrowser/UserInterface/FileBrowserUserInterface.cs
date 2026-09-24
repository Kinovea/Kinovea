#region license
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
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;

using Kinovea.Camera;
using Kinovea.FileBrowser.Languages;
using Kinovea.Services;
using Kinovea.Video;
using BrightIdeasSoftware;

namespace Kinovea.FileBrowser
{
    /// <summary>
    /// The user interface for the navigation pane.
    /// </summary>
    public partial class FileBrowserUserInterface : KinoveaControl
    {
        #region Members

        private BrowserTreeController explorerTree;

        private bool initializing = true;
        private bool isClosing = false;
        
        private BrowserLocation currentLocation;
        private long browserContentRevision = 0;


        private List<CameraSummary> cameraSummaries = new List<CameraSummary>();
        private Dictionary<string, int> cameraSummaryMap = new Dictionary<string, int>();
        private ImageList imgListCameras = new ImageList();

        private bool programmaticTabChange;
        private bool externalSelection;
        private BrowserContentType activeTab;
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();
        private Stopwatch stopwatch = new Stopwatch();

        #region Menu
        private ContextMenuStrip popMenuFolders = new ContextMenuStrip();
        private ToolStripMenuItem mnuLocateFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAddToShortcuts = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteShortcut = new ToolStripMenuItem();

        private ContextMenuStrip popMenuFiles = new ContextMenuStrip();
        private ToolStripMenuItem mnuSortBy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByName = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByDate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortBySize = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortAscending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortDescending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLaunchFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocateFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteFile = new ToolStripMenuItem();

        private ContextMenuStrip popMenuCameras = new ContextMenuStrip();
        private ToolStripMenuItem mnuLaunchCamera = new ToolStripMenuItem();
        private ToolStripMenuItem mnuForgetCamera = new ToolStripMenuItem();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Construction & initialization
        public FileBrowserUserInterface()
        {
            InitializeComponent();

            // Restore UI state.
            splitExplorerFiles.SplitterDistance = (int)(splitExplorerFiles.Height * WindowManager.ActiveWindow.ExplorerFilesSplitterRatio);
            
            // Build the tree view.
            explorerTree = new BrowserTreeController(tvExplorer);
            var shortcuts = PreferencesManager.FileExplorerPreferences.ShortcutFolders;
            List<BrowserLocation> shortcutLocations = shortcuts.Select(s => new BrowserLocation(s.Path)).ToList();
            explorerTree.Build(shortcutLocations);
            explorerTree.LocationSelected += ExplorerTree_LocationSelected;

            PrepareCameraListView();
            BuildContextMenu();

            // Hook events from UI
            splitExplorerFiles.SplitterMoved += Splitters_SplitterMoved;
            lvExplorer.ItemDrag += listView_ItemDrag;
            lvCaptured.ItemDrag += listView_ItemDrag;

            // Hook events from other modules.
            NotificationCenter.BrowserContentTypeChanged += NotificationCenter_ExplorerTabChangeAsked;
            NotificationCenter.RefreshFileList += NotificationCenter_RefreshNavigationPane;
            NotificationCenter.FileSelected += NotificationCenter_FileSelected;
            NotificationCenter.FileOpened += NotificationCenter_FileOpened;

            // Reload stored persistent information.
            InitializeFileWatcher();
            
            // Reload last tab from prefs.
            tabControl.SelectedIndex = (int)WindowManager.ActiveWindow.ActiveTab;
            activeTab = WindowManager.ActiveWindow.ActiveTab;
            
            Application.Idle += new EventHandler(this.IdleDetector);
            this.Hotkeys = HotkeySettingsManager.ActiveBindings.GetCommandBindings("FileExplorer");
        }

        private void BuildContextMenu()
        {
            #region Tree view

            mnuLocateFolder.Image = Properties.Resources.folder_explore;
            mnuLocateFolder.Click += mnuLocateFolder_Click;
            mnuLocateFolder.Visible = true;
            
            mnuAddToShortcuts.Image = Properties.Resources.star;
            mnuAddToShortcuts.Click += mnuAddToShortcuts_Click;
            mnuAddToShortcuts.Visible = false;

            mnuDeleteShortcut.Image = Properties.Resources.folder_delete;
            mnuDeleteShortcut.Click += mnuDeleteShortcut_Click;
            mnuDeleteShortcut.Visible = false;
            
            popMenuFolders.Items.AddRange(new ToolStripItem[] 
            { 
                mnuLocateFolder, 
                mnuAddToShortcuts, 
                mnuDeleteShortcut 
            });
            
            tvExplorer.ContextMenuStrip = popMenuFolders;
            tvExplorer.MouseDown += ExplorerTree_MouseDown;

            #endregion

            #region Camera list

            mnuLaunchCamera.Image = Properties.Resources.camera_video;
            mnuForgetCamera.Image = Properties.Resources.delete;
            mnuLaunchCamera.Click += (s, e) => LaunchSelectedCamera();
            mnuForgetCamera.Click += (s, e) => ForgetSelectedCamera();
            popMenuCameras.Items.AddRange(new ToolStripItem[]
            {
                mnuLaunchCamera,
                mnuForgetCamera
            });

            olvCameras.ContextMenuStrip = popMenuCameras;
            #endregion

            #region File lists
            // Sort menus
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

            mnuLaunchFile.Image = Properties.Resources.television;
            mnuLocateFile.Image = Properties.Resources.folder_explore;
            mnuDeleteFile.Image = Properties.Resources.delete;

            mnuLaunchFile.Click += (s, e) => CommandLaunchVideo();
            mnuLocateFile.Click += mnuLocate_Click;
            mnuDeleteFile.Click += (s, e) => CommandDelete();
            
            mnuLaunchFile.Visible = false;
            mnuLocateFile.Visible = false;
            mnuDeleteFile.Visible = false;

            popMenuFiles.Items.AddRange(new ToolStripItem[] 
            {
                mnuSortBy,
                new ToolStripSeparator(),
                mnuLaunchFile,
                mnuLocateFile,
                new ToolStripSeparator(), 
                mnuDeleteFile
            });

            lvExplorer.ContextMenuStrip = popMenuFiles;
            lvCaptured.ContextMenuStrip = popMenuFiles;

            #endregion
        }

        private void IdleDetector(object sender, EventArgs e)
        {
            // Oh, we are idle. The ScreenManager should be loaded now,
            // and thus will have registered its DisplayThumbnails delegate.
            
            log.Debug("Application is idle in FileBrowserUserInterface.");
            
            // This is a one time only routine.
            Application.Idle -= new EventHandler(this.IdleDetector);
            initializing = false;
            
            // Prune any captured file removed since last run.
            PreferencesManager.FileExplorerPreferences.ConsolidateRecentCapturedFiles();

            if (this.Visible)
            {
                // Visible and on camera tab, ask for one step of discovery to fill the list view.
                // TODO: make asynchronous in a background thread.
                var tab = (BrowserContentType)tabControl.SelectedIndex;
                if (tab == BrowserContentType.Cameras)
                {
                    CameraTypeManager.DiscoveryStep();
                }

                // Show the right browser panel.
                NotificationCenter.RaiseBrowserContentTypeChanged(this, tab);
                NotificationCenter.RaiseUpdateStatus();
            }
                
            DoRefreshFileList(true);
        }
        #endregion

        #region Navigation
        private void NavigateTo(BrowserLocation location)
        {
            if (location == null)
                return;

            log.DebugFormat("Navigate to: {0}", location.Path);
            stopwatch.Restart();
            BrowserContentSnapshot snapshot = BuildBrowserContent(location);
            CommitBrowserContent(snapshot);
            log.DebugFormat("After commit browser content: {0} ms", stopwatch.ElapsedMilliseconds);
        }

        private BrowserContentSnapshot BuildBrowserContent(BrowserLocation location)
        {   
            List<BrowserItem> items = new List<BrowserItem>();

            if (location.Type == BrowserLocationType.RecentFiles)
            {
                // TODO: get recent files.
                return new BrowserContentSnapshot(location, items, NextBrowserContentRevision());
            }
            else
            {
                string path = location.Path;
                if (!Directory.Exists(path))
                {
                    return new BrowserContentSnapshot(location, items, NextBrowserContentRevision());
                }

                IEnumerable<string> filePaths = Directory.EnumerateFiles(path);

                // Filter out unsupported files.
                List<string> supportedFiles = filePaths.Where(f => VideoTypeManager.IsSupported(Path.GetExtension(f))).ToList();

                // Sort.
                try
                {
                    FileSortAxis axis = PreferencesManager.FileExplorerPreferences.FileSortAxis;
                    bool ascending = PreferencesManager.FileExplorerPreferences.FileSortAscending;
                    supportedFiles.Sort(new FileComparator(axis, ascending));

                    foreach (string filePath in supportedFiles)
                    {
                        BrowserItem item = BrowserItem.FromFile(filePath);
                        items.Add(item);
                    }

                    return new BrowserContentSnapshot(location, items, NextBrowserContentRevision());
                }
                catch (Exception e)
                {
                    // Sometimes when renaming a file this might throw with "FileNotFoundException.
                    log.ErrorFormat("An error happened while trying to sort files : {0}", e.Message);
                }
            }

            return null;
        }

        private void CommitBrowserContent(BrowserContentSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            currentLocation = snapshot.Location;

            UpdateFileList(snapshot);

            if (snapshot.Location.IsFileSystem)
            {
                UpdateFileWatcher(snapshot.Location.Path);

                NotificationCenter.RaiseBrowserContentUpdated(snapshot);
                NotificationCenter.RaiseUpdateStatus();
            }

            //if (!expanding && !initializing && !isClosing)
            //{
            //    UpdateFileList(folderPath, lvExplorer, true);
            //}
        }

        public long NextBrowserContentRevision()
        {
            return ++browserContentRevision;
        }
        #endregion

        private void NotificationCenter_ExplorerTabChangeAsked(object sender, EventArgs<BrowserContentType> e)
        {
            if (sender == this)
                return;

            programmaticTabChange = true;
            tabControl.SelectedIndex = (int)e.Value;
        }
        private void NotificationCenter_RefreshNavigationPane(object sender, EventArgs<bool> e)
        {
            DoRefreshFileList(e.Value);
        }
        private void NotificationCenter_FileSelected(object sender, EventArgs<string> e)
        {
            if (sender == this)
                return;

            if (activeTab == BrowserContentType.Cameras)
                return;

            // A file has been selected via its thumbnail.
            // In theory we are already on the right folder and the file is somewhere in the list.
            lvExplorer.SelectedItems.Clear();

            if (string.IsNullOrEmpty(e.Value))
                return;

            foreach (ListViewItem item in lvExplorer.Items)
            {
                if ((string)item.Tag != e.Value)
                    continue;
                
                externalSelection = true;
                item.Selected = true;
                item.EnsureVisible();
                break;
            }
        }
        private void NotificationCenter_FileOpened(object sender, EventArgs<string> e)
        {
            // Create a virtual shortcut for the folder of the opened video and select it.
            string parent = Path.GetDirectoryName(e.Value);
            BrowserLocation location = new BrowserLocation(parent);
            if (currentLocation != null && currentLocation.Path == location.Path)
                return;

            // Move the tree to the new location.
            // This will trigger a NavigateTo and update the browser content snapshot.
            explorerTree.TryReveal(location);
            tvExplorer.Invalidate();
        }

        private void DoRefreshFileList(bool refreshThumbnails)
        {
            // Called when:
            // - the user changes the sort option.
            // - a file modification happens in the thumbnails page. (delete/rename)
            // - a capture is completed.
            
            // We don't update during app start up, because we would most probably
            // end up loading the desktop, and then the saved folder.
            if(initializing || isClosing)
                return;

            log.DebugFormat("DoRefreshFileList");

            if (!refreshThumbnails)
            {
                log.DebugFormat("do not refresh thumbnails");
            }

            if (activeTab == BrowserContentType.FileSystem)
            {
                NavigateTo(currentLocation);
            }
            else if (activeTab == BrowserContentType.Cameras)
            {
                UpdateCapturedFileList(PreferencesManager.FileExplorerPreferences.RecentCapturedFiles);
            }
        }
        
        #region Public interface
        public void RefreshUICulture()
        {
            tabPageClassic.Text = "";
            tabPageCameras.Text = "";

            btnManual.Text = FileBrowserLang.FormCameraWizard_Title;
            lblCaptureHistory.Text = FileBrowserLang.lblCaptureHistory;

            // Menus
            mnuLocateFolder.Text = FileBrowserLang.mnuVideoLocate;
            mnuAddToShortcuts.Text = FileBrowserLang.mnuAddToShortcuts;
            mnuDeleteShortcut.Text = FileBrowserLang.mnuRemoveFromShortcuts;

            mnuSortBy.Text = FileBrowserLang.mnuSortBy;
            mnuSortByName.Text = FileBrowserLang.mnuSortBy_Name;
            mnuSortByDate.Text = FileBrowserLang.mnuSortBy_Date;
            mnuSortBySize.Text = FileBrowserLang.mnuSortBy_Size;
            mnuSortAscending.Text = FileBrowserLang.mnuSortBy_Ascending;
            mnuSortDescending.Text = FileBrowserLang.mnuSortBy_Descending;
            mnuLaunchFile.Text = FileBrowserLang.Generic_Open;
            mnuLocateFile.Text = FileBrowserLang.mnuVideoLocate;
            mnuDeleteFile.Text = FileBrowserLang.mnuVideoDelete;
            mnuLaunchCamera.Text = FileBrowserLang.Generic_Open;
            mnuForgetCamera.Text = FileBrowserLang.ForgetCustomSettings;

            // ToolTips
            //ttTabs.SetToolTip(tabPageClassic, FileBrowserLang.tabExplorer);
            ttTabs.SetToolTip(tabPageClassic, Kinovea.FileBrowser.Languages.FileBrowserLang.navPane_FileSystem);
            ttTabs.SetToolTip(tabPageCameras, Kinovea.FileBrowser.Languages.FileBrowserLang.tabCameras);
            ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
        }
        
        /// <summary>
        /// Reload the shortcut root in the tree view.
        /// </summary>
        private void ReloadShortcuts()
        {
            var shortcuts = PreferencesManager.FileExplorerPreferences.ShortcutFolders;
            List<BrowserLocation> shortcutLocations = shortcuts.Select(s => new BrowserLocation(s.Path)).ToList();
            explorerTree.UpdateShortcuts(shortcutLocations);
        }

        public void CamerasDiscovered(List<CameraSummary> summaries)
        {
            UpdateCameraList(summaries);
        }
        
        public void CameraForgotten(CameraSummary summary)
        {
            ForgetCamera(summary);
            List<CameraSummary> newList = new List<CameraSummary>(cameraSummaries);
            UpdateCameraList(newList);
        }
        
        public void Closing()
        {
            isClosing = true;

            // Remember the last browsed location.
            if (currentLocation == null || currentLocation.Type != BrowserLocationType.FileSystem)
                return;

            if (string.IsNullOrEmpty(currentLocation.Path))
                return;

            PreferencesManager.FileExplorerPreferences.LastBrowsedDirectory = currentLocation.Path;
        }

        #endregion
        
        private void Splitters_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (initializing || isClosing)
                return;

            WindowManager.ActiveWindow.ExplorerFilesSplitterRatio = (float)splitExplorerFiles.SplitterDistance / splitExplorerFiles.Height;
            WindowManager.SaveActiveWindow();
        }

        private void TabControlSelected_IndexChanged(object sender, EventArgs e)
        {
            activeTab = (BrowserContentType)tabControl.SelectedIndex;

            WindowManager.ActiveWindow.ActiveTab = activeTab;
            WindowManager.SaveActiveWindow();

            if (programmaticTabChange)
            {
                programmaticTabChange = false;
            }
            else if (this.Visible)
            {
                if (activeTab == BrowserContentType.Cameras)
                {
                    CameraTypeManager.DiscoveryStep();
                }

                // Show the right browser panel.
                NotificationCenter.RaiseBrowserContentTypeChanged(this, activeTab);
                NotificationCenter.RaiseUpdateStatus();
            }

            DoRefreshFileList(true);
        }


        #region File system tab

        #region Add/Remove shortcuts

        /// <summary>
        /// Add a shortcut by picking a folder in the file system via FolderBrowserDialog.
        /// </summary>
        private void btnAddShortcut_Click(object sender, EventArgs e)
        {
            string pathToAdd = FilesystemHelper.OpenFolderBrowserDialog("");
            if (string.IsNullOrWhiteSpace(pathToAdd))
                return;

            ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(pathToAdd), pathToAdd);
            PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
            
            ReloadShortcuts();

            // Move to the newly added shortcut.
            BrowserLocation location = new BrowserLocation(sf.Path);
            explorerTree.TryRevealInShortcuts(location);
        }

        /// <summary>
        /// Add a shortcut by right click on a node in the tree.
        /// </summary>
        private void mnuAddToShortcuts_Click(object sender, EventArgs e)
        {
            // Add the active folder to the shortcuts.
            if (currentLocation == null || currentLocation.Type != BrowserLocationType.FileSystem)
                return;

            if (string.IsNullOrWhiteSpace(currentLocation.Path))
                return;

            bool isKnown = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(currentLocation.Path);

            if (isKnown)
                return;

            ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(currentLocation.Path), currentLocation.Path);
            PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
            ReloadShortcuts();

            // Move to the newly added shortcut.
            explorerTree.TryRevealInShortcuts(currentLocation);
        }

        /// <summary>
        /// Delete the active folder from the shortcuts.
        /// </summary>
        private void mnuDeleteShortcut_Click(object sender, EventArgs e)
        {
            // In theory the delete menu is only shown when the user right
            // clicked on the active folder and it's a known shortcut.
            if (currentLocation == null || currentLocation.Type != BrowserLocationType.FileSystem)
                return;

            if (string.IsNullOrWhiteSpace(currentLocation.Path))
                return;

            // Look for the location in the shortcuts.
            foreach (ShortcutFolder sf in PreferencesManager.FileExplorerPreferences.ShortcutFolders)
            {
                if (sf.Path != currentLocation.Path)
                    continue;

                // Remove from preferences and from tree view.
                PreferencesManager.FileExplorerPreferences.RemoveShortcut(sf);
                ReloadShortcuts();
                break;
            }

            // What folder should we fall back to?
            // If we don't do anyting the tree will TryReveal() the old selection, and it will find
            // it somewhere else in the tree, either under another shortcut or in the drives.
            // This looks a bit weird, we delete a shortcut and it jumps to that same folder by
            // a different way.
        }

        #endregion

        #region TreeView
        private void ExplorerTree_LocationSelected(object sender, EventArgs<BrowserLocation> e)
        {
            if (e.Value == null)
                return;

            log.DebugFormat("ExplorerTree_LocationSelected");

            NavigateTo(e.Value);
        }

        private void ExplorerTree_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            TreeView tv = sender as TreeView;
            string path;

            bool isOnSelected = TryGetSelectedPathAt(tv, e.Location, out path);

            if (!isOnSelected)
            {
                mnuLocateFolder.Visible = false;
                mnuAddToShortcuts.Visible = false;
                mnuDeleteShortcut.Visible = false;
            }
            else
            {
                bool knownShortcut = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(path);

                // Name the path directly in the menu as feedback.
                string name = Path.GetFileName(path);
                mnuLocateFolder.Text = string.Format("Locate \"{0}\" in Windows explorer", name);
                mnuAddToShortcuts.Text = string.Format("Add \"{0}\" to shortcuts", name);
                mnuDeleteShortcut.Text = string.Format("Remove \"{0}\" from shortcuts", name);

                mnuLocateFolder.Visible = true;
                mnuAddToShortcuts.Visible = !knownShortcut;
                mnuDeleteShortcut.Visible = knownShortcut;
            }
        }

        /// <summary>
        /// Checks if the mouse is over the currently selected node and fills the path.
        /// </summary>
        private bool TryGetSelectedPathAt(TreeView tv, Point loc, out string path)
        {
            TreeNode node = tv.GetNodeAt(loc);
            if (node == null)
            {
                path = null;
                return false;
            }

            if (node != tv.SelectedNode)
            {
                path = null;
                return false;
            }

            BrowserLocation browserLocation = node.Tag as BrowserLocation;
            if (browserLocation == null)
            {
                path = null;
                return false;
            }

            path = browserLocation.Path;
            return true;
        }
        #endregion

        #endregion

        #region Camera list

        private void PrepareCameraListView()
        {
            // Column level options
            var colCameraIcon = new OLVColumn();
            colCameraIcon.AspectName = "Icon";
            colCameraIcon.Groupable = false;
            colCameraIcon.Sortable = false;
            colCameraIcon.IsEditable = false;
            colCameraIcon.MinimumWidth = 25;
            colCameraIcon.MaximumWidth = 25;
            colCameraIcon.TextAlign = HorizontalAlignment.Center;
            colCameraIcon.AspectGetter = delegate (object rowObject)
            {
                return ((CameraSummary)rowObject).Identifier;
            };

            colCameraIcon.AspectToStringConverter = delegate (object rowObject)
            {
                return string.Empty;
            };

            colCameraIcon.ImageGetter = delegate (object rowObject)
            {
                // The image list for the icons is indexed by the identifier.
                return ((CameraSummary)rowObject).Identifier;
            };

            var colName = new OLVColumn();
            colName.AspectName = "Alias";
            colName.Groupable = false;
            colName.Sortable = false;
            colName.IsEditable = false;
            colName.MinimumWidth = 100;
            colName.FillsFreeSpace = true;
            colName.FreeSpaceProportion = 2;
            colName.TextAlign = HorizontalAlignment.Left;

            olvCameras.AllColumns.AddRange(new OLVColumn[] {
                colCameraIcon,
                colName,
                });

            olvCameras.Columns.AddRange(new ColumnHeader[] {
                colCameraIcon,
                colName,
                });

            // List view level options
            olvCameras.HeaderStyle = ColumnHeaderStyle.None;
            olvCameras.RowHeight = 22;
            olvCameras.FullRowSelect = true;
            olvCameras.SmallImageList = imgListCameras;
        }

        private void UpdateCameraList(List<CameraSummary> newSummaries)
        {
            // Consolidate the list of cameras.
            // We maintain a local list in parallel to the list view, indexed by ids.
            // 3 steps:
            // 1. Remove lost cameras.
            // 2. Update existing cameras.
            // 3. Add new cameras.

            bool needsRefresh = false;
            List<string> lost = new List<string>();
            foreach (var known in cameraSummaries)
            {
                var c = newSummaries.FirstOrDefault(s => s.Identifier == known.Identifier);
                if (c == null)
                {
                    lost.Add(known.Identifier);
                }
            }

            needsRefresh = lost.Count > 0;

            foreach (string id in lost)
            {
                RemoveCamera(id);
            }

            // Second pass, update existing and add new.
            foreach (CameraSummary newSummary in newSummaries)
            {
                if (cameraSummaryMap.ContainsKey(newSummary.Identifier))
                {
                    // Update existing only if needed.
                    int index = cameraSummaryMap[newSummary.Identifier];

                    if (cameraSummaries[index].Alias == newSummary.Alias &&
                        cameraSummaries[index].Icon.GetHashCode() == newSummary.Icon.GetHashCode())
                    {
                        continue;
                    }
                    else
                    {
                        needsRefresh = true;
                        imgListCameras.Images.RemoveByKey(newSummary.Identifier);
                        imgListCameras.Images.Add(newSummary.Identifier, newSummary.Icon);
                        cameraSummaries[index] = newSummary;
                    }
                }
                else
                {
                    // Add new.
                    needsRefresh = true;
                    imgListCameras.Images.Add(newSummary.Identifier, newSummary.Icon);
                    cameraSummaries.Add(newSummary);
                    cameraSummaryMap[newSummary.Identifier] = cameraSummaries.Count - 1;
                }
            }

            if (needsRefresh)
            {
                olvCameras.SetObjects(cameraSummaries);
            }
        }
        
        private void ForgetCamera(CameraSummary summary)
        {
            // Remove a camera from the list.
            // If it's a connected camera it will be added back at next discovery step.
            if (!cameraSummaryMap.ContainsKey(summary.Identifier))
                return;

            RemoveCamera(summary.Identifier);
            olvCameras.SetObjects(cameraSummaries);
        }

        private void RemoveCamera(string id)
        {
            imgListCameras.Images.RemoveByKey(id);
            cameraSummaries.RemoveAt(cameraSummaryMap[id]);
            RebuildCameraSummaryMap();
        }
        
        private void RebuildCameraSummaryMap()
        {
            cameraSummaryMap.Clear();
            for (int i = 0; i < cameraSummaries.Count; i++)
            {
                cameraSummaryMap[cameraSummaries[i].Identifier] = i;
            }
        }

        private void LaunchSelectedCamera()
        {
            var cameraSummary = olvCameras.SelectedObject as CameraSummary;
            if (cameraSummary == null)
                return;

            CameraTypeManager.LoadCamera(cameraSummary, -1);
        }

        private void ForgetSelectedCamera()
        {
            var cameraSummary = olvCameras.SelectedObject as CameraSummary;
            if (cameraSummary == null)
                return;

            CameraTypeManager.ForgetCamera(cameraSummary);
        }

        private void olvCameras_DoubleClick(object sender, EventArgs e)
        {
            var cameraSummary = olvCameras.SelectedObject as CameraSummary;
            if (cameraSummary == null)
                return;

            CameraTypeManager.LoadCamera(cameraSummary, -1);
        }

        private void olvCameras_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var cameraSummary = olvCameras.SelectedObject as CameraSummary;
            if (cameraSummary == null)
                return;

            DoDragDrop(cameraSummary, DragDropEffects.All);
        }

        private void BtnManualClick(object sender, EventArgs e)
        {
            FormCameraWizard wizard = new FormCameraWizard();
            if (wizard.ShowDialog() == DialogResult.OK)
            {
                CameraSummary summary = wizard.Result;
                if (summary != null)
                    CameraTypeManager.UpdatedCameraSummary(summary);
            }

            wizard.Dispose();
        }

        private void btnCameraRefresh_Click(object sender, EventArgs e)
        {
            // Run one step of discovery. This can be used to manually refresh the list
            // as we don't run auto-discovery while a camera is streaming/recording.
            CameraTypeManager.DiscoveryStep();
        }
        #endregion

        #region File lists
        
        /// <summary>
        /// Locate the selected video in Windows Explorer.
        /// </summary>
        private void mnuLocate_Click(object sender, EventArgs e)
        {
            LocateSelectedVideo(GetFileListview());
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lvi = e.Item as ListViewItem;
            if (lvi == null)
                return;
            
            string path = lvi.Tag as string;
            if(path == null)
                return;
            
            DoDragDrop(path, DragDropEffects.All);
        }

        private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            PrepareSortMenus();
            ShowHideListMenu(false);
            
            ListView lv = sender as ListView;
            if (lv == null)
                return;

            if (e.Button != MouseButtons.Right)
                return;

            ListViewItem lvi = lv.GetItemAt(e.X, e.Y);
            if (lvi == null)
                return;

            ShowHideListMenu(true);
        }

        private void listView_SizeChanged(object sender, EventArgs e)
        {
            // Make sure the column takes all the space.
            var lv = sender as ListView;
            if (lv.Columns.Count == 0)
                return;

            lv.Columns[0].Width = lv.Width;
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = sender as ListView;
            if (lv == null || lv.SelectedItems.Count != 1)
                return;

            string file = lv.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(file))
                return;

            foreach (ListViewItem item in lv.Items)
            {
                item.BackColor = Color.White;
                item.ForeColor = Color.Black;
            }

            lv.SelectedItems[0].BackColor = SystemColors.Highlight;
            lv.SelectedItems[0].ForeColor = SystemColors.HighlightText;

            if (!externalSelection)
                NotificationCenter.RaiseFileSelected(this, file);

            externalSelection = false;
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView lv = sender as ListView;

            ListViewItem lvi = lv.GetItemAt(e.X, e.Y);
            if (lvi == null || lv.SelectedItems == null || lv.SelectedItems.Count != 1)
                return;

            string path = lvi.Tag as string;
            if (path == null)
                return;

            NotificationCenter.RaiseLoadVideoAsked(path, -1);
        }

        /// <summary>
        /// Get the right ListView based on the active tab.
        /// </summary>
        private ListView GetFileListview()
        {
            switch (activeTab)
            {
                case BrowserContentType.Cameras:
                    return lvCaptured;
                case BrowserContentType.FileSystem:
                default:
                    return lvExplorer;
            }
        }

        /// <summary>
        /// Update the file system browser file list.
        /// </summary>
        private void UpdateFileList(BrowserContentSnapshot snapshot)
        {
            log.DebugFormat("UpdateFileList");

            // Configure the list view.
            lvExplorer.BeginUpdate();

            lvExplorer.View = View.Details;
            lvExplorer.Items.Clear();
            lvExplorer.Columns.Clear();
            lvExplorer.Columns.Add("", lvExplorer.Width);
            lvExplorer.GridLines = true;
            lvExplorer.HeaderStyle = ColumnHeaderStyle.None;

            // Push them to the list view.
            foreach (var item in snapshot.Items)
            {
                string path = item.Path;

                ListViewItem lvi = new ListViewItem(Path.GetFileName(path));
                lvi.Tag = path;
                lvi.ImageIndex = 0;
                lvExplorer.Items.Add(lvi);
            }

            lvExplorer.EndUpdate();
        }

        /// <summary>
        /// Updates the captured files file list.
        /// </summary>
        private void UpdateCapturedFileList(List<string> filenames)
        {
            lvCaptured.BeginUpdate();

            lvCaptured.View = View.Details;
            lvCaptured.Items.Clear();
            lvCaptured.Columns.Clear();
            lvCaptured.Columns.Add("", lvCaptured.Width);
            lvCaptured.GridLines = true;
            lvCaptured.HeaderStyle = ColumnHeaderStyle.None;

            foreach (string filename in filenames)
            {
                ListViewItem lvi = new ListViewItem(Path.GetFileName(filename));
                lvi.Tag = filename;
                lvi.ImageIndex = 0;
                lvCaptured.Items.Add(lvi);
            }

            lvCaptured.Invalidate();
            lvCaptured.EndUpdate();
        }

        /// <summary>
        /// Set the "Sort by" sub menus checks according to current preferences.
        /// </summary>
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

        /// <summary>
        /// Show or hide all the menus of the file list.
        /// </summary>
        private void ShowHideListMenu(bool visible)
        {
            foreach (ToolStripItem menu in popMenuFiles.Items)
                menu.Visible = visible;
        }

        #endregion
        
        #region Menu Event Handlers
        private void mnuLocateFolder_Click(object sender, EventArgs e)
        {
            if (currentLocation == null)
                return;

            if (currentLocation.Type != BrowserLocationType.FileSystem)
                return;

            if (string.IsNullOrWhiteSpace(currentLocation.Path))
                return;

            FilesystemHelper.LocateDirectory(currentLocation.Path);
        }
        
        private void UpdateSortAxis(FileSortAxis axis)
        {
            PreferencesManager.FileExplorerPreferences.FileSortAxis = axis;
            DoRefreshFileList(true);
        }

        private void UpdateSortAscending(bool ascending)
        {
            PreferencesManager.FileExplorerPreferences.FileSortAscending = ascending;
            DoRefreshFileList(true);
        }
        #endregion

        #region File watcher
        private void InitializeFileWatcher()
        {
            fileWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
            fileWatcher.Filter = "*.*";
            fileWatcher.IncludeSubdirectories = false;
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Changed += fileWatcher_Changed;
            fileWatcher.Created += fileWatcher_Created;
            fileWatcher.Deleted += fileWatcher_Deleted;
            fileWatcher.Renamed += fileWatcher_Renamed;
        }

        private void UpdateFileWatcher(string folderPath)
        {
            fileWatcher.EnableRaisingEvents = false;

            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                fileWatcher.Path = folderPath;
                fileWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                // This happens with archive files, considered directories by Windows.
                log.ErrorFormat("Error while adding path to file watcher. {0}", folderPath);
            }
        }
        
        private void fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { DoRefreshFileList(true); });
        }

        private void fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { DoRefreshFileList(true); });
        }

        private void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { DoRefreshFileList(true); });
        }

        private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { DoRefreshFileList(true); });
        }
        #endregion

        #region Commands
        protected override bool ExecuteCommand(string name)
        {
            FileExplorerCommands command = (FileExplorerCommands)Enum.Parse(typeof(FileExplorerCommands), name);

            switch (command)
            {
                case FileExplorerCommands.RenameSelected:
                    // TODO.
                    break;
                case FileExplorerCommands.LaunchSelected:
                    CommandLaunchVideo();
                    break;
                case FileExplorerCommands.DeleteSelected:
                    CommandDelete();
                    break;
                default:
                    return base.ExecuteCommand(name);
            }

            return true;
        }

        private void CommandLaunchVideo()
        {
            ListView lv = GetFileListview();
            string path = GetSelectedVideoPath(lv);
            if (path != null)
                NotificationCenter.RaiseLoadVideoAsked(path, -1);
        }

        private string GetSelectedVideoPath(ListView lv)
        {
            if (lv == null || lv.SelectedItems == null || lv.SelectedItems.Count != 1)
                return null;

            return lv.SelectedItems[0].Tag as string;
        }

        private void CommandDelete()
        {
            ListView lv = GetFileListview();
            string path = GetSelectedVideoPath(lv);
            if (path == null)
                return;

            FilesystemHelper.DeleteFile(path);
            if (!File.Exists(path))
            {
                if (activeTab == BrowserContentType.Cameras)
                    PreferencesManager.FileExplorerPreferences.ConsolidateRecentCapturedFiles();

                DoRefreshFileList(true);
            }
        }

        #region Locate the selected file in Windows Explorer
        private void LocateSelectedVideo(ListView lv)
        {
            string path = GetSelectedVideoPath(lv);
            if (path != null)
                FilesystemHelper.LocateFile(path);
        }
        #endregion

        #endregion
    }
}
