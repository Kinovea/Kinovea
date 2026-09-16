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

using ExpTreeLib;
using Kinovea.Camera;
using Kinovea.FileBrowser.Languages;
using Kinovea.Services;
using Kinovea.Video;
using BrightIdeasSoftware;

namespace Kinovea.FileBrowser
{
    /// <summary>
    /// The user interface for the navigation pane.
    /// We maintain the synchronization between the shortcut and exptree tab
    /// when we move between shortcuts. We don't maintain it the other way around.
    /// </summary>
    public partial class FileBrowserUserInterface : KinoveaControl
    {
        #region Members

        private FileSystemTreeController explorerTree;
        private FileSystemTreeController shortcutsTree;

        private string currentExplorerPath; // Current path in exptree tab.
        private string currentShortcutPath; // Current path in shortcuts tab.

        private bool expanding; // True if the exptree is currently auto expanding. To avoid reentry.
        private bool initializing = true;
        private bool isClosing = false;
        
        private List<CameraSummary> cameraSummaries = new List<CameraSummary>();
        private Dictionary<string, int> cameraSummaryMap = new Dictionary<string, int>();
        private ImageList imgListCameras = new ImageList();

        private bool programmaticTabChange;
        private bool externalSelection;
        private string lastOpenedDirectory;
        private BrowserContentType activeTab;
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();
        private Stopwatch stopwatch = new Stopwatch();

        #region Menu
        private ContextMenuStrip popMenuFolders = new ContextMenuStrip();
        private ToolStripMenuItem mnuAddToShortcuts = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocateFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDeleteShortcut = new ToolStripMenuItem();

        private ContextMenuStrip popMenuFiles = new ContextMenuStrip();
        private ToolStripMenuItem mnuSortBy = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByName = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortByDate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortBySize = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortAscending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuSortDescending = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();

        private ContextMenuStrip popMenuCameras = new ContextMenuStrip();
        private ToolStripMenuItem mnuCameraLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCameraForget = new ToolStripMenuItem();
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & Initialization
        public FileBrowserUserInterface()
        {
            InitializeComponent();

            // Restore UI state.
            splitExplorerFiles.SplitterDistance = (int)(splitExplorerFiles.Height * WindowManager.ActiveWindow.ExplorerFilesSplitterRatio);
            splitShortcutsFiles.SplitterDistance = (int)(splitShortcutsFiles.Height * WindowManager.ActiveWindow.ShortcutsFilesSplitterRatio);
            
            explorerTree = new FileSystemTreeController(etExplorer.tv1);
            explorerTree.SelectedPathChanged += ExplorerTree_SelectedPathChanged;
            explorerTree.BuildComputer();

            shortcutsTree = new FileSystemTreeController(etShortcuts.tv1);
            shortcutsTree.SelectedPathChanged += ShortcutsTree_SelectedPathChanged;
            ReloadShortcuts();

            PrepareCameraListView();
            BuildContextMenu();

            // Hook events from UI
            splitExplorerFiles.SplitterMoved += Splitters_SplitterMoved;
            splitShortcutsFiles.SplitterMoved += Splitters_SplitterMoved;
            lvExplorer.ItemDrag += listView_ItemDrag;
            lvShortcuts.ItemDrag += listView_ItemDrag;
            lvCaptured.ItemDrag += listView_ItemDrag;

            // Hook events from other modules.
            NotificationCenter.BrowserContentTypeChanged += NotificationCenter_ExplorerTabChangeAsked;
            NotificationCenter.RefreshFileList += NotificationCenter_RefreshNavigationPane;
            NotificationCenter.FileSelected += NotificationCenter_FileSelected;
            NotificationCenter.FileOpened += NotificationCenter_FileOpened;
            NotificationCenter.FolderChangeAsked += NotificationCenter_FolderChangeAsked;
            NotificationCenter.FolderNavigationAsked += NotificationCenter_FolderNavigationAsked;

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
            // Add an item to shortcuts
            mnuAddToShortcuts.Image = Properties.Resources.star;
            mnuAddToShortcuts.Click += mnuAddToShortcuts_Click;
            mnuAddToShortcuts.Visible = false;

            mnuLocateFolder.Image = Properties.Resources.folder_explore;
            mnuLocateFolder.Click += mnuLocateFolder_Click;
            mnuLocateFolder.Visible = true;

            // Delete selected shortcut
            mnuDeleteShortcut.Image = Properties.Resources.folder_delete;
            mnuDeleteShortcut.Click += mnuDeleteShortcut_Click;
            mnuDeleteShortcut.Visible = false;
            
            popMenuFolders.Items.AddRange(new ToolStripItem[] 
            { 
                mnuAddToShortcuts, 
                mnuLocateFolder, 
                mnuDeleteShortcut 
            });
            
            // The context menus will be configured on a per event basis.
            etShortcuts.ContextMenuStrip = popMenuFolders;
            etExplorer.ContextMenuStrip = popMenuFolders;

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

            mnuLaunch.Image = Properties.Resources.television;
            mnuLocate.Image = Properties.Resources.folder_explore;
            mnuDelete.Image = Properties.Resources.delete;

            mnuLaunch.Click += (s, e) => CommandLaunch();
            mnuLocate.Click += mnuLocate_Click;
            mnuDelete.Click += (s, e) => CommandDelete();
            
            mnuLaunch.Visible = false;
            mnuLocate.Visible = false;
            mnuDelete.Visible = false;

            popMenuFiles.Items.AddRange(new ToolStripItem[] 
            {
                mnuSortBy,
                new ToolStripSeparator(),
                mnuLaunch,
                mnuLocate,
                new ToolStripSeparator(), 
                mnuDelete
            });

            mnuCameraLaunch.Image = Properties.Resources.camera_video;
            mnuCameraForget.Image = Properties.Resources.delete;
            mnuCameraLaunch.Click += (s, e) => LaunchSelectedCamera();
            mnuCameraForget.Click += (s, e) => ForgetSelectedCamera();
            popMenuCameras.Items.AddRange(new ToolStripItem[] 
            { 
                mnuCameraLaunch, 
                mnuCameraForget 
            });

            lvShortcuts.ContextMenuStrip = popMenuFiles;
            lvExplorer.ContextMenuStrip = popMenuFiles;
            lvCaptured.ContextMenuStrip = popMenuFiles;
            olvCameras.ContextMenuStrip = popMenuCameras;
        }

        private void mnuLocate_Click(object sender, EventArgs e)
        {
            CommandLocate();
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

        #region Public interface
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

            // Find the file and select it here.
            ListView lv = GetFileListview();
            lv.SelectedItems.Clear();

            if (string.IsNullOrEmpty(e.Value))
                return;

            foreach (ListViewItem item in lv.Items)
            {
                if ((string)item.Tag != e.Value)
                    continue;
                
                externalSelection = true;
                item.Selected = true;
                item.EnsureVisible();
                break;
            }
        }
        private ListView GetFileListview()
        {
            switch (activeTab)
            {
                case BrowserContentType.Shortcuts:
                    return lvShortcuts;
                case BrowserContentType.Cameras:
                    return lvCaptured;
                case BrowserContentType.Files:
                default:
                    return lvExplorer;
            }
        }

        private void NotificationCenter_FileOpened(object sender, EventArgs<string> e)
        {
            // Create a virtual shortcut for the folder of the opened video and select it.
            string pathFolder = Path.GetDirectoryName(e.Value);
            AddVirtualShortcut(pathFolder);
            shortcutsTree.SelectRootChild(pathFolder);
        }
        private void NotificationCenter_FolderChangeAsked(object sender, EventArgs<string> e)
        {
            // The thumbnail viewer is asking for a different folder to be shown.
            // Note: the path to the new folder is stored in the File property of the event arg.
            string pathFolder = e.Value;
            AddVirtualShortcut(pathFolder);
            shortcutsTree.SelectRootChild(pathFolder);
        }

        /// <summary>
        /// Back/Forward navigation in the session history requested by the file browser.
        /// </summary>
        private void NotificationCenter_FolderNavigationAsked(object sender, EventArgs<FolderNavigationType> e)
        {
        }

        /// <summary>
        /// Add the passed folder as a virtual shortcut.
        /// </summary>
        private void AddVirtualShortcut(string folderPath)
        {
            if (folderPath == lastOpenedDirectory)
                return;

            string oldLastOpenedDirectory = lastOpenedDirectory;
            lastOpenedDirectory = folderPath;

            // If the shortcuts list is already on the right folder don't do anything.
            if (activeTab == BrowserContentType.Shortcuts && currentShortcutPath == folderPath)
                return;

            if (folderPath.StartsWith("."))
                return;

            // Check if the previous opened directory was a true shortcut or a virtual one.

            bool oldWasKnown = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(oldLastOpenedDirectory);
            if (!oldWasKnown)
            {
                // The previous opened directory was a virtual shortcut, remove it.
                shortcutsTree.RemoveRootChild(oldLastOpenedDirectory);
            }
            
            bool newIsKnown = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(folderPath);
            if (!newIsKnown)
            {
                shortcutsTree.AddRootChild(folderPath);
            }
        }

        private void DoRefreshFileList(bool refreshThumbnails)
        {
            // Called when:
            // - the user changes node in exptree, either explorer or shortcuts,
            // - the user changes the sort option.
            // - a file modification happens in the thumbnails page. (delete/rename)
            // - a capture is completed.
            
            // We don't update during app start up, because we would most probably
            // end up loading the desktop, and then the saved folder.
            if(initializing || isClosing)
                return;
            
            // Figure out which tab we are on to update the right listview.
            if(activeTab == BrowserContentType.Files)
            {
                UpdateFileList(currentExplorerPath, lvExplorer, refreshThumbnails);
                // TODO: synchronize shortcuts tab.
            }
            else if(activeTab == BrowserContentType.Shortcuts)
            {
                if (!string.IsNullOrWhiteSpace(currentShortcutPath))
                {
                    UpdateFileList(currentShortcutPath, lvShortcuts, refreshThumbnails);
                }
                else if (!string.IsNullOrWhiteSpace(currentExplorerPath))
                {
                    // Case where we select a folder on the explorer tab
                    // and then move to the shortcuts tab.
                    // -> reload the hidden list of the exptree tab.
                    // We also force the thumbnail refresh, because in this case it is the only way to update the
                    // filename list held in ScreenManager.
                    UpdateFileList(currentExplorerPath, lvExplorer, true);
                }
            }
            else if(activeTab == BrowserContentType.Cameras)
            {
                UpdateFileList(PreferencesManager.FileExplorerPreferences.RecentCapturedFiles, lvCaptured, false, false);
            }
        }
        public void RefreshUICulture()
        {
            // ExpTree tab.
            tabPageClassic.Text = "";
            lblFolders.Text = FileBrowserLang.lblFolders;
            lblVideoFiles.Text = FileBrowserLang.lblVideoFiles;

            // Shortcut tab.
            tabPageShortcuts.Text = "";
            lblFavFolders.Text = lblFolders.Text;
            lblFavFiles.Text = lblVideoFiles.Text;
            etShortcuts.RootDisplayName = FileBrowserLang.tabShortcuts;
            etShortcuts.tv1.Refresh();

            tabPageCameras.Text = "";
            label1.Text = FileBrowserLang.lblCameras;
            btnManual.Text = FileBrowserLang.FormCameraWizard_Title;
            lblCaptureHistory.Text = FileBrowserLang.lblCaptureHistory;

            // Menus
            mnuAddToShortcuts.Text = FileBrowserLang.mnuAddToShortcuts;
            mnuLocateFolder.Text = FileBrowserLang.mnuVideoLocate;
            mnuDeleteShortcut.Text = Kinovea.FileBrowser.Languages.FileBrowserLang.mnuRemoveFromShortcuts;
            mnuSortBy.Text = FileBrowserLang.mnuSortBy;
            mnuSortByName.Text = FileBrowserLang.mnuSortBy_Name;
            mnuSortByDate.Text = FileBrowserLang.mnuSortBy_Date;
            mnuSortBySize.Text = FileBrowserLang.mnuSortBy_Size;
            mnuSortAscending.Text = FileBrowserLang.mnuSortBy_Ascending;
            mnuSortDescending.Text = FileBrowserLang.mnuSortBy_Descending;
            mnuLaunch.Text = FileBrowserLang.Generic_Open;
            mnuLocate.Text = FileBrowserLang.mnuVideoLocate;
            mnuDelete.Text = FileBrowserLang.mnuVideoDelete;
            mnuCameraLaunch.Text = FileBrowserLang.Generic_Open;
            mnuCameraForget.Text = FileBrowserLang.ForgetCustomSettings;

            // ToolTips
            //ttTabs.SetToolTip(tabPageClassic, FileBrowserLang.tabExplorer);
            ttTabs.SetToolTip(tabPageClassic, Kinovea.FileBrowser.Languages.FileBrowserLang.navPane_FileSystem);
            ttTabs.SetToolTip(tabPageShortcuts, Kinovea.FileBrowser.Languages.FileBrowserLang.tabShortcuts);
            ttTabs.SetToolTip(tabPageCameras, Kinovea.FileBrowser.Languages.FileBrowserLang.tabCameras);
            ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
            ttTabs.SetToolTip(btnDeleteShortcut, FileBrowserLang.mnuDeleteShortcut);
        }
        
        /// <summary>
        /// Reload the shortcut tree view (users shortcuts + current folder).
        /// </summary>
        public void ReloadShortcuts()
        {
            shortcutsTree.BuildFavorites(GetShortcuts());
        }

        /// <summary>
        /// Get the saved shortcut plus the last opened directory.
        /// If the last opened directory wasn't in the existing shortcuts it's added at the top.
        /// </summary>
        private List<string> GetShortcuts()
        {
            List<string> shortcuts = new List<string>();
            List<ShortcutFolder> savedShortcuts = PreferencesManager.FileExplorerPreferences.ShortcutFolders;

            bool found = false;
            foreach (ShortcutFolder shortcut in savedShortcuts)
            {
                if (Directory.Exists(shortcut.Location))
                {
                    shortcuts.Add(shortcut.Location);

                    if (shortcut.Location == lastOpenedDirectory)
                    {
                        found = true;
                    }
                }
            }

            if (!found)
            {
                shortcuts.Insert(0, lastOpenedDirectory);
            }

            return shortcuts;
        }

        public void ResetShortcutList()
        {
            lvShortcuts.Clear();
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
            if(!string.IsNullOrEmpty(currentExplorerPath))
            {
                PreferencesManager.FileExplorerPreferences.LastBrowsedDirectory = currentExplorerPath;
            }
        }

        private void Splitters_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (initializing || isClosing)
                return;

            WindowManager.ActiveWindow.ExplorerFilesSplitterRatio = (float)splitExplorerFiles.SplitterDistance / splitExplorerFiles.Height;
            WindowManager.ActiveWindow.ShortcutsFilesSplitterRatio = (float)splitShortcutsFiles.SplitterDistance / splitShortcutsFiles.Height;
            WindowManager.SaveActiveWindow();
        }
        #endregion

        #region File system tab

        #region TreeView
        private void ExplorerTree_SelectedPathChanged(string folderPath)
        {
            currentExplorerPath = folderPath;

            if (!expanding && !initializing && !isClosing)
            {
                // We don't maintain synchronization in the direction file system -> shortcuts.
                ResetShortcutList();
                UpdateFileList(folderPath, lvExplorer, true);
            }
        }
        private void etExplorer_MouseEnter(object sender, EventArgs e)
        {
            // Give focus to enable mouse scroll.
            //etExplorer.Focus();
        }
        private void etExplorer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool isOnSelected = explorerTree.IsOnSelected(e.Location);
            mnuAddToShortcuts.Visible = isOnSelected;
            mnuLocateFolder.Visible = isOnSelected;
            mnuDeleteShortcut.Visible = false;
        }
        #endregion
        
        #region ListView
        private void lvExplorer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LaunchItemAt(lvExplorer, e);
        }
        #endregion
        
        #endregion

        #region Shortcuts tab
        
        #region Shortcuts add/remove Handling
        private void btnAddShortcut_Click(object sender, EventArgs e)
        {
            AddShortcut();
        }
        private void btnDeleteShortcut_Click(object sender, EventArgs e)
        {
            RemoveSelectedShortcut();
        }
        private void AddShortcut()
        {
            string selectedPath = FilesystemHelper.OpenFolderBrowserDialog("");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(selectedPath), selectedPath);
                PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
                ReloadShortcuts();
            }
        }
        private void RemoveSelectedShortcut()
        {
            if(string.IsNullOrWhiteSpace(currentShortcutPath))
                return;
            
            foreach(ShortcutFolder sf in PreferencesManager.FileExplorerPreferences.ShortcutFolders)
            {
                if(sf.Location != currentShortcutPath)
                    continue;

                PreferencesManager.FileExplorerPreferences.RemoveShortcut(sf);
                shortcutsTree.RemoveRootChild(currentShortcutPath);
                break;
            }
        }
        #endregion
        
        #region TreeView
        private void ShortcutsTree_SelectedPathChanged(string folderPath)
        {
            currentShortcutPath = folderPath;
            //if (initializing || isClosing)
            if (isClosing)
                return;

            // This is also called in the context of adding the virtual shortcut
            // when a file is opened, so we could be on any tab right now.

            if (activeTab == BrowserContentType.Shortcuts)
            {
                // Load the thumbnails in the back.
                UpdateFileList(currentShortcutPath, lvShortcuts, true);
                
                // Sync the explorer tab tree.
                explorerTree.ExpandToPath(currentShortcutPath);
                UpdateFileList(currentShortcutPath, lvExplorer, false);
            }
            else if (activeTab == BrowserContentType.Files)
            {
                // Sync the explorer tab tree view and update its file list and thumbnails in the back.
                explorerTree.ExpandToPath(currentShortcutPath);
                UpdateFileList(currentShortcutPath, lvExplorer, true);
            }
            else if (activeTab == BrowserContentType.Cameras)
            {
                // Keep the file lists in sync, don't refresh the thumbnails.
                UpdateFileList(currentShortcutPath, lvShortcuts, false);

                explorerTree.ExpandToPath(currentShortcutPath);
                UpdateFileList(currentShortcutPath, lvExplorer, false);
            }
        }

        private void etShortcuts_MouseEnter(object sender, EventArgs e)
        {
            // Give focus to enable mouse scroll.
            //etShortcuts.Focus();	
        }
        private void etShortcuts_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right)
                return;

            if (string.IsNullOrWhiteSpace(currentShortcutPath))
                return;

            bool isOnSelected = shortcutsTree.IsOnSelected(e.Location);
            if (!isOnSelected)
            {
                mnuAddToShortcuts.Visible = false;
                mnuLocateFolder.Visible = false;
                mnuDeleteShortcut.Visible = false;
                return;
            }

            bool known = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(currentShortcutPath);
            mnuAddToShortcuts.Visible = !known;
            mnuLocateFolder.Visible = true;
            mnuDeleteShortcut.Visible = known;
        }
        #endregion
        
        #region ListView
        private void lvShortcuts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LaunchItemAt(lvShortcuts, e);
        }
        #endregion

        #endregion

        #region Camera tab

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

        #region File list
        private void LvCaptured_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LaunchItemAt(lvCaptured, e);
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

        #endregion

        #endregion

        #region Common
        private void TabControlSelected_IndexChanged(object sender, EventArgs e)
        {
            // Active tab changed.
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

        /// <summary>
        /// Update a list view with the files from the passed folder.
        /// Optionally triggers an update of the thumbnails pane.
        /// </summary>
        private void UpdateFileList(string folderPath, ListView listView, bool doRefresh)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            bool isShortcuts = listView == lvShortcuts;

            string logPrefix = isShortcuts ? "Shortcuts" : "Filesystem";
            log.DebugFormat("[{0}] - Updating the file list.", logPrefix);
            stopwatch.Restart();

            this.Cursor = Cursors.WaitCursor;
            
            // Configure the list view.
            listView.BeginUpdate();
            listView.View = View.Details;
            listView.Items.Clear();
            listView.Columns.Clear();
            listView.Columns.Add("", listView.Width);
            listView.GridLines = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;


            IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath);

            // Filter out unsupported files.
            List<string> supportedFiles = filePaths.Where(f => VideoTypeManager.IsSupported(Path.GetExtension(f))).ToList();

            // Sort.
            try
            {
                FileSortAxis axis = PreferencesManager.FileExplorerPreferences.FileSortAxis;
                bool ascending = PreferencesManager.FileExplorerPreferences.FileSortAscending;
                supportedFiles.Sort(new FileComparator(axis, ascending));
            }
            catch(Exception e)
            {
                // Sometimes when renaming a file this might throw with "FileNotFoundException.
                log.ErrorFormat("An error happened while trying to sort files : {0}", e.Message);
            }

            log.DebugFormat("[{0}] - Sorted files: {1} ms.", logPrefix, stopwatch.ElapsedMilliseconds);

            // Push them to the list view.
            foreach (string path in supportedFiles)
            {
                ListViewItem lvi = new ListViewItem(Path.GetFileName(path));
                lvi.Tag = path;
                lvi.ImageIndex = 0;
                listView.Items.Add(lvi);
            }
            
            listView.EndUpdate();

            log.DebugFormat("[{0}] - Updated list view: {1} ms.", logPrefix, stopwatch.ElapsedMilliseconds);

            UpdateFileWatcher(folderPath);

            // Even if we don't want to reload the thumbnails, we must ensure that 
            // the screen manager backup list is in sync with the actual file list.
            // desync can happen in case of renaming and deleting files.
            // the screenmanager backup list is used at Unhide(), when we close a screen.
            log.DebugFormat("[{0}] - Before sending event to thumbnail viewer: {1} ms.", logPrefix, stopwatch.ElapsedMilliseconds);

            NotificationCenter.RaiseCurrentDirectoryChanged(folderPath, supportedFiles, isShortcuts, doRefresh);
            NotificationCenter.RaiseUpdateStatus();
            this.Cursor = Cursors.Default;

            log.DebugFormat("[{0}] - Updated file list: {1} ms.", logPrefix, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Updates a file list with an explicit list of files.
        /// </summary>
        private void UpdateFileList(List<string> filenames, ListView listView, bool refreshThumbnails, bool shortcuts)
        {
            listView.BeginUpdate();
            listView.View = View.Details;
            listView.Items.Clear();
            listView.Columns.Clear();
            listView.Columns.Add("", listView.Width);
            listView.GridLines = true;
            listView.HeaderStyle = ColumnHeaderStyle.None;

            foreach (string filename in filenames)
            {
                ListViewItem lvi = new ListViewItem(Path.GetFileName(filename));
                lvi.Tag = filename;
                lvi.ImageIndex = 0;
                listView.Items.Add(lvi);
            }

            listView.Invalidate();
            listView.EndUpdate();
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

        private void LaunchItemAt(ListView listView, MouseEventArgs e)
        {
            ListViewItem lvi = listView.GetItemAt(e.X, e.Y);
            
            if(lvi == null || listView.SelectedItems == null || listView.SelectedItems.Count != 1)
                return;
            
            string path = lvi.Tag as string;
            if(path == null)
                return;
                
            NotificationCenter.RaiseLoadVideoAsked(path, -1);
        }
        #endregion
        
        #region Menu Event Handlers
        private void mnuAddToShortcuts_Click(object sender, EventArgs e)
        {
            string selectedPath = activeTab == BrowserContentType.Files ? currentExplorerPath : currentShortcutPath;
            if(string.IsNullOrWhiteSpace(selectedPath))
                return;

            ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(selectedPath), selectedPath);
            PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
            ReloadShortcuts();
        }
        private void mnuLocateFolder_Click(object sender, EventArgs e)
        {
            string selectedPath = activeTab == BrowserContentType.Files ? currentExplorerPath : currentShortcutPath;
            if (string.IsNullOrWhiteSpace(selectedPath))
                return;

            FilesystemHelper.LocateDirectory(selectedPath);
        }
        private void mnuDeleteShortcut_Click(object sender, EventArgs e)
        {
            RemoveSelectedShortcut();
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
                    CommandLaunch();
                    break;
                case FileExplorerCommands.DeleteSelected:
                    CommandDelete();
                    break;
                default:
                    return base.ExecuteCommand(name);
            }

            return true;
        }

        private void CommandLaunch()
        {
            ListView lv = GetActiveListView();
            LaunchSelectedVideo(lv);
        }

        private ListView GetActiveListView()
        {
            switch (activeTab)
            {
                case BrowserContentType.Shortcuts:
                    return lvShortcuts;
                case BrowserContentType.Cameras:
                    return lvCaptured;
                case BrowserContentType.Files:
                default:
                    return lvExplorer;
            }
        }

        private string GetSelectedVideoPath(ListView lv)
        {
            if (lv == null || lv.SelectedItems == null || lv.SelectedItems.Count != 1)
                return null;

            return lv.SelectedItems[0].Tag as string;
        }

        private void LaunchSelectedVideo(ListView lv)
        {
            string path = GetSelectedVideoPath(lv);
            if (path != null)
                NotificationCenter.RaiseLoadVideoAsked(path, -1);
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

        private void CommandLocate()
        {
            if (activeTab == BrowserContentType.Files)
                LocateSelectedVideo(lvExplorer);
            else if (activeTab == BrowserContentType.Shortcuts)
                LocateSelectedVideo(lvShortcuts);
            else if (activeTab == BrowserContentType.Cameras)
                LocateSelectedVideo(lvCaptured);
        }

        private void LocateSelectedVideo(ListView lv)
        {
            string path = GetSelectedVideoPath(lv);
            if (path != null)
                FilesystemHelper.LocateFile(path);
        }
        #endregion
    }
}
