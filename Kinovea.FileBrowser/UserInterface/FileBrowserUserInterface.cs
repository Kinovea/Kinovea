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
        private CShItem currentExptreeItem; // Current item in exptree tab.
        private CShItem currentShortcutItem; // Current item in shortcuts tab.
        private SessionHistory sessionHistory = new SessionHistory();
        private bool expanding; // True if the exptree is currently auto expanding. To avoid reentry.
        private bool initializing = true;
        private ImageList cameraIcons = new ImageList();
        private List<CameraSummary> cameraSummaries = new List<CameraSummary>();
        private bool programmaticTabChange;
        private bool externalSelection;
        private string lastOpenedDirectory;
        private BrowserContentType activeTab;
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();
        private Stopwatch stopwatch = new Stopwatch();

        #region Menu
        private ContextMenuStrip popMenuFolders = new ContextMenuStrip();
        private ToolStripMenuItem mnuAddToShortcuts = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenAsReplayWatcher = new ToolStripMenuItem();
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
        private ToolStripMenuItem mnuLaunchWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLocate = new ToolStripMenuItem();
        private ToolStripMenuItem mnuDelete = new ToolStripMenuItem();

        private ContextMenuStrip popMenuCameras = new ContextMenuStrip();
        private ToolStripMenuItem mnuCameraLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuCameraForget = new ToolStripMenuItem();
        #endregion

        private static string pathDesktop = "::{00021400-0000-0000-c000-000000000046}";
        private static string pathComputer = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & Initialization
        public FileBrowserUserInterface()
        {
            InitializeComponent();

            // Splitters
            
            splitExplorerFiles.SplitterDistance = (int)(splitExplorerFiles.Height * WindowManager.ActiveWindow.ExplorerFilesSplitterRatio);
            splitShortcutsFiles.SplitterDistance = (int)(splitShortcutsFiles.Height * WindowManager.ActiveWindow.ShortcutsFilesSplitterRatio);
            splitExplorerFiles.SplitterMoved += Splitters_SplitterMoved;
            splitShortcutsFiles.SplitterMoved += Splitters_SplitterMoved;

            lvCameras.SmallImageList = cameraIcons;
            cameraIcons.Images.Add("historyEntryDay", Properties.Resources.calendar_view_day);
            cameraIcons.Images.Add("historyEntryMonth", Properties.Resources.calendar_view_month);
            cameraIcons.Images.Add("unknownCamera", Properties.Resources.film_small);

            // Drag Drop handling.
            lvExplorer.ItemDrag += listView_ItemDrag;
            lvShortcuts.ItemDrag += listView_ItemDrag;
            lvCaptured.ItemDrag += listView_ItemDrag;

            InitializeTreeView(etExplorer);
            InitializeTreeView(etShortcuts);
            etExplorer.TreeViewBeforeExpand += etExplorer_TreeViewBeforeExpand;

            FilterOutDesktopChildren(etExplorer);

            lvCameras.ItemDrag += lvCameras_ItemDrag;
            
            BuildContextMenu();
            
            NotificationCenter.BrowserContentTypeChanged += NotificationCenter_ExplorerTabChangeAsked;
            NotificationCenter.RefreshFileList += NotificationCenter_RefreshNavigationPane;
            NotificationCenter.FileSelected += NotificationCenter_FileSelected;
            NotificationCenter.FileOpened += NotificationCenter_FileOpened;
            NotificationCenter.FolderChangeAsked += NotificationCenter_FolderChangeAsked;
            NotificationCenter.FolderNavigationAsked += NotificationCenter_FolderNavigationAsked;

            // Reload stored persistent information.
            ReloadShortcuts();
            InitializeFileWatcher();
            
            // Reload last tab from prefs.
            // We don't reload the splitters here, because we are not at full size yet and they are anchored.
            tabControl.SelectedIndex = (int)WindowManager.ActiveWindow.ActiveTab;
            activeTab = WindowManager.ActiveWindow.ActiveTab;
            
            Application.Idle += new EventHandler(this.IdleDetector);
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("FileExplorer");
        }

        private void InitializeTreeView(ExpTree tv)
        {
            tv.AllowDrop = false;
            tv.tv1.BorderStyle = BorderStyle.None;
            tv.tv1.ItemHeight = 20;
            tv.tv1.ShowLines = false;
            tv.tv1.ShowPlusMinus = true; // Can't get the chevron.
            tv.tv1.FullRowSelect = true;
            tv.tv1.HotTracking = false; // underline on hover.
            tv.tv1.Indent = 20;

            tv.tv1.KeyDown += (s, e) =>
            {
                // Disable the * key to expand all nodes.
                if (e.KeyCode == Keys.Multiply)
                    e.Handled = true;
            };
        }

        private void FilterOutDesktopChildren(ExpTree etv)
        {
            TreeView tv = etv.tv1;

            // Filter list for children of Desktop.
            List<string> toFilter = new List<string>
            {
                "::{21EC2020-3AEA-1069-A2DD-08002B30309D}", // Control panel
                "::{26EE0668-A00A-44D7-9371-BEB064C98683}", // Control panel.
                "::{2227A280-3AEA-1069-A2DE-08002B30309D}", // Printers
                "::{645FF040-5081-101B-9F08-00AA002F954E}", // Recycle bin
                "::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", // Network places
                "::{031E4825-7B94-4DC3-B131-E946B44C8DD5}", // Libraries
            };

            TreeNode computerNode = null;
            TreeNode rootNode = tv.Nodes[0];
            for (int i = rootNode.Nodes.Count - 1; i >= 0; i--)
            {
                CShItem item = (CShItem)rootNode.Nodes[i].Tag;
                if (item.Path == pathComputer)
                {
                    computerNode = rootNode.Nodes[i];
                    continue;
                }

                if (toFilter.Contains(item.Path))
                {
                    rootNode.Nodes.RemoveAt(i);
                    continue;
                }

                // Filter out the drives as they show up under computer again.
                // This list under Desktop doesn't have all of them anyway.
                if (item.IsDisk)
                {
                    rootNode.Nodes.RemoveAt(i);
                    continue;
                }
            }

            // Expand Computer.
            if (computerNode != null)
            {
                computerNode.Expand();
            }

            // Note: the drives have already been renamed from "System (C:)" to "C: (System)",
            // to align all the drive letters nicely. Done inside ExpTree.
            
            // Reselect the desktop node
            tv.SelectedNode = rootNode;
        }

        private void etExplorer_TreeViewBeforeExpand(object sender, TreeViewEventArgs e)
        {
            // This is raised after the node children have been added but before it is visually expanded.
            // Use this to filter out unwanted folders.
            var item = e.Node.Tag as CShItem;
            if (item == null)
                return;

            if (item.Parent == null || item.Parent.Path != pathDesktop)
                return;

            bool isComputer = item.Path == "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";

            // Immediate children of desktop.
            // Remove any dot folder in children, especially for the Home folder.
            // Remove any non-drive folder under Computer.
            for (int i = e.Node.Nodes.Count - 1; i >= 0; i--)
            {
                var childItem = e.Node.Nodes[i].Tag as CShItem;
                if (childItem == null)
                    continue;
                
                if (childItem.DisplayName.StartsWith("."))
                {
                    e.Node.Nodes.RemoveAt(i);
                    continue;
                }

                if (isComputer && !childItem.IsDisk)
                {
                    e.Node.Nodes.RemoveAt(i);
                    continue;
                }
            }
        }

        private void BuildContextMenu()
        {
            // Add an item to shortcuts
            mnuAddToShortcuts.Image = Properties.Resources.star;
            mnuAddToShortcuts.Click += new EventHandler(mnuAddToShortcuts_Click);
            mnuAddToShortcuts.Visible = false;

            mnuOpenAsReplayWatcher.Image = Properties.Resources.replaywatcher;
            mnuOpenAsReplayWatcher.Click += new EventHandler(mnuOpenAsReplayWatcher_Click);
            mnuOpenAsReplayWatcher.Visible = true;

            mnuLocateFolder.Image = Properties.Resources.folder_explore;
            mnuLocateFolder.Click += new EventHandler(mnuLocateFolder_Click);
            mnuLocateFolder.Visible = true;

            // Delete selected shortcut
            mnuDeleteShortcut.Image = Properties.Resources.folder_delete;
            mnuDeleteShortcut.Click += new EventHandler(mnuDeleteShortcut_Click);
            mnuDeleteShortcut.Visible = false;
            
            popMenuFolders.Items.AddRange(new ToolStripItem[] 
            { 
                mnuAddToShortcuts, 
                //mnuOpenAsReplayWatcher, 
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

            mnuLaunch.Image = Properties.Resources.film_go;
            mnuLaunch.Click += (s, e) => CommandLaunch();
            mnuLaunch.Visible = false;

            mnuLaunchWatcher.Image = Properties.Resources.replaywatcher;
            mnuLaunchWatcher.Click += (s, e) => CommandLaunchWatcher();
            mnuLaunchWatcher.Visible = false;

            mnuLocate.Image = Properties.Resources.folder_explore;
            mnuLocate.Click += mnuLocate_Click;
            mnuLocate.Visible = false;

            mnuDelete.Image = Properties.Resources.delete;
            mnuDelete.Click += (s, e) => CommandDelete();
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

            mnuCameraLaunch.Image = Properties.Resources.film_go;
            mnuCameraLaunch.Click += (s, e) => LaunchSelectedCamera(lvCameras);
            mnuCameraForget.Image = Properties.Resources.delete;
            mnuCameraForget.Click += (s, e) => DeleteSelectedCamera(lvCameras);
            popMenuCameras.Items.AddRange(new ToolStripItem[] { mnuCameraLaunch, new ToolStripSeparator(), mnuCameraForget });

            lvShortcuts.ContextMenuStrip = popMenuFiles;
            lvExplorer.ContextMenuStrip = popMenuFiles;
            lvCaptured.ContextMenuStrip = popMenuFiles;
            lvCameras.ContextMenuStrip = popMenuCameras;
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
            UpdateSessionHistory(pathFolder);
            SelectVirtualShortcut();
        }
        private void NotificationCenter_FolderChangeAsked(object sender, EventArgs<string> e)
        {
            // The thumbnail viewer is asking for a different folder to be shown.
            // Note: the path to the new folder is stored in the File property of the event arg.
            string pathFolder = e.Value;
            AddVirtualShortcut(pathFolder);
            UpdateSessionHistory(pathFolder);
            SelectVirtualShortcut();
        }

        /// <summary>
        /// Back/Forward navigation in the session history requested by the file browser.
        /// </summary>
        private void NotificationCenter_FolderNavigationAsked(object sender, EventArgs<FolderNavigationType> e)
        {
            // Move in the session history.
            if (e.Value == FolderNavigationType.Backward)
            {
                sessionHistory.Back();
            }
            else if (e.Value == FolderNavigationType.Forward)
            {
                sessionHistory.Forward();
            }

            AddVirtualShortcut(sessionHistory.Current.Path);
            SelectVirtualShortcut();

            // End the navigating operation.
            sessionHistory.Navigating = false;
        }

        /// <summary>
        /// Add the passed folder as a virtual shortcut.
        /// </summary>
        private void AddVirtualShortcut(string pathFolder)
        {
            lastOpenedDirectory = pathFolder;

            // If the shortcuts list is already on the right folder don't do anything.
            if (activeTab == BrowserContentType.Shortcuts && currentShortcutItem != null && currentShortcutItem.Path == pathFolder)
                return;

            if (pathFolder.StartsWith("."))
                return;

            // Reload the shortcut tree with the virtual shortcut in it (via lastOpenedDirectory variable).
            // This does not select any item in the tree and does not refresh the file list.
            ReloadShortcuts();
        }

        private void UpdateSessionHistory(string pathFolder)
        {
            if (!Directory.Exists(pathFolder))
                return;

            CShItem item = new CShItem(pathFolder);
            sessionHistory.Add(item);
        }

        /// <summary>
        /// Select the folder in the shortcuts tab and synchronize explorer tab.
        /// </summary>
        private void SelectVirtualShortcut()
        {
            // Whether the active tab is the explorer or the shortcuts, we always 
            // go through the shortcuts tree to select the folder, as we just added it to the shortcuts hierarchy.
            // this will trigger the synchro with the explorer tab so both will be ready.
            //
            // The only issue here is that we can't currently add special directories to the the shortcuts, except the desktop.
            // If the user adds special folders to the history stack the navigation is broken.
            if (sessionHistory.Current != null && !sessionHistory.Current.IsFileSystem)
            {
                etExplorer.ExpandANode(sessionHistory.Current);
            }
            else
            {
                // Normal case where the selected folder is a proper folder.
                // This will trigger the synchronization with the explorer tab.
                etShortcuts.SelectNode(lastOpenedDirectory);
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
            if(initializing)
                return;
            
            // Figure out which tab we are on to update the right listview.
            if(activeTab == BrowserContentType.Files)
            {
                if(currentExptreeItem != null)
                    UpdateFileList(currentExptreeItem, lvExplorer, refreshThumbnails, false);

                // TODO: synchronize shortcuts tab.

            }
            else if(activeTab == BrowserContentType.Shortcuts)
            {
                if (currentShortcutItem != null)
                {
                    UpdateFileList(currentShortcutItem, lvShortcuts, refreshThumbnails, true);
                }
                else if (currentExptreeItem != null)
                {
                    // This is the special case where we select a folder on the exptree tab
                    // and then move to the shortcuts tab.
                    // -> reload the hidden list of the exptree tab.
                    // We also force the thumbnail refresh, because in this case it is the only way to update the
                    // filename list held in ScreenManager...
                    UpdateFileList(currentExptreeItem, lvExplorer, true, false);
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
            
            tabPageCameras.Text = "";
            label1.Text = FileBrowserLang.lblCameras;
            btnManual.Text = FileBrowserLang.FormCameraWizard_Title;
            lblCaptureHistory.Text = FileBrowserLang.lblCaptureHistory;

            // Menus
            mnuAddToShortcuts.Text = FileBrowserLang.mnuAddToShortcuts;
            mnuOpenAsReplayWatcher.Text = FileBrowserLang.mnuOpenAsReplayWatcher;
            mnuLocateFolder.Text = FileBrowserLang.mnuVideoLocate;
            mnuDeleteShortcut.Text = FileBrowserLang.mnuDeleteShortcut;
            mnuSortBy.Text = FileBrowserLang.mnuSortBy;
            mnuSortByName.Text = FileBrowserLang.mnuSortBy_Name;
            mnuSortByDate.Text = FileBrowserLang.mnuSortBy_Date;
            mnuSortBySize.Text = FileBrowserLang.mnuSortBy_Size;
            mnuSortAscending.Text = FileBrowserLang.mnuSortBy_Ascending;
            mnuSortDescending.Text = FileBrowserLang.mnuSortBy_Descending;
            mnuLaunch.Text = FileBrowserLang.Generic_Open;
            mnuLaunchWatcher.Text = FileBrowserLang.mnuOpenAsReplayWatcher;
            mnuLocate.Text = FileBrowserLang.mnuVideoLocate;
            mnuDelete.Text = FileBrowserLang.mnuVideoDelete;
            mnuCameraLaunch.Text = FileBrowserLang.Generic_Open;
            mnuCameraForget.Text = FileBrowserLang.ForgetCustomSettings;

            // ToolTips
            //ttTabs.SetToolTip(tabPageClassic, FileBrowserLang.tabExplorer);
            ttTabs.SetToolTip(tabPageClassic, "File system");
            ttTabs.SetToolTip(tabPageShortcuts, "Shortcuts");
            ttTabs.SetToolTip(tabPageCameras, "Cameras");
            ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
            ttTabs.SetToolTip(btnDeleteShortcut, FileBrowserLang.mnuDeleteShortcut);
        }
        
        /// <summary>
        /// Reload the saved shortcuts plus the current folder as a transient shortcut into the shortcut tree.
        /// </summary>
        public void ReloadShortcuts()
        {
            // Get the list as paths.
            List<string> shortcuts = GetShortcuts();

            // Create items out of the paths and populate the tree.
            etShortcuts.SetShortcuts(new ArrayList(shortcuts));

            etShortcuts.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
        }

        /// <summary>
        /// Get a list of the saved shortcut paths including the last opened directory.
        /// </summary>
        private List<string> GetShortcuts()
        {
            List<string> shortcuts = new List<string>();
            List<ShortcutFolder> savedShortcuts = PreferencesManager.FileExplorerPreferences.ShortcutFolders;

            // Since we are loading the list from filenames, we can't currently
            // add the current directory if it's not from the filesystem (e.g: library folder).
            string dir = lastOpenedDirectory;
            if (!Directory.Exists(lastOpenedDirectory))
                dir = null;

            foreach (ShortcutFolder shortcut in savedShortcuts)
            {
                if (Directory.Exists(shortcut.Location))
                {
                    shortcuts.Add(shortcut.Location);

                    if (shortcut.Location == lastOpenedDirectory)
                        dir = null;
                }
            }

            // Inject the last opened directory if it's not already in the list of saved shortcuts.
            if (!string.IsNullOrEmpty(dir))
                shortcuts.Insert(0, dir);

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
        
        public void CameraSummaryUpdated(CameraSummary summary)
        {
            UpdateCamera(summary);
        }
        public void CameraForgotten(CameraSummary summary)
        {
            ForgetCamera(summary);
            List<CameraSummary> newList = new List<CameraSummary>(cameraSummaries);
            UpdateCameraList(newList);
        }
        
        public void Closing()
        {
            if(currentExptreeItem != null)
                PreferencesManager.FileExplorerPreferences.LastBrowsedDirectory = currentExptreeItem.Path;
        }

        private void Splitters_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (initializing)
                return;

            WindowManager.ActiveWindow.ExplorerFilesSplitterRatio = (float)splitExplorerFiles.SplitterDistance / splitExplorerFiles.Height;
            WindowManager.ActiveWindow.ShortcutsFilesSplitterRatio = (float)splitShortcutsFiles.SplitterDistance / splitShortcutsFiles.Height;
            WindowManager.SaveActiveWindow();
        }
        #endregion

        #region Explorer tab

        #region TreeView
        private void etExplorer_ExpTreeNodeSelected(string selectedPath, CShItem item)
        {
            currentExptreeItem = item;
            
            if(!expanding && !initializing)
            {
                // We don't maintain synchronization with the Shortcuts tab. 
                ResetShortcutList();
                UpdateFileList(currentExptreeItem, lvExplorer, true, false);
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
            
            mnuDeleteShortcut.Visible = false;
            bool valid = etExplorer.IsOnSelectedItem(e.Location) && !currentExptreeItem.Path.StartsWith("::");
            mnuAddToShortcuts.Visible = valid;
            mnuOpenAsReplayWatcher.Visible = valid;
            mnuLocateFolder.Visible = valid;
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
            DeleteSelectedShortcut();
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
        private void DeleteSelectedShortcut()
        {
            if(currentShortcutItem == null)
                return;
            
            foreach(ShortcutFolder sf in PreferencesManager.FileExplorerPreferences.ShortcutFolders)
            {
                if(sf.Location != currentShortcutItem.Path)
                    continue;

                PreferencesManager.FileExplorerPreferences.RemoveShortcut(sf);
                ReloadShortcuts();
                break;
            }
        }
        #endregion
        
        #region TreeView
        private void etShortcuts_ExpTreeNodeSelected(string selectedPath, CShItem item)
        {
            currentShortcutItem = item;
            if (initializing)
                return;
    
            // The operation that will trigger the thumbnail refresh MUST only be called at the end. 
            // Otherwise the other threads take precedence and the thumbnails are not 
            // shown progressively but all at once, when other operations are over.
                
            if (currentExptreeItem == null || currentExptreeItem.Path != currentShortcutItem.Path)
            {
                // Maintain synchronization with the explorer tree but don't refresh.
                UpdateFileList(currentShortcutItem, lvExplorer, false, false);
                
                expanding = true;
                etExplorer.ExpandANode(currentShortcutItem);
                etExplorer.tv1.SelectedNode?.EnsureVisible();
                expanding = false;

                currentExptreeItem = etExplorer.SelectedItem;
            }

            // Finally update the shortcuts tab, and refresh thumbs.
            UpdateFileList(currentShortcutItem, lvShortcuts, true, true);
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
            
            try
            {
                if(currentExptreeItem == null || !etShortcuts.IsOnSelectedItem(e.Location) || currentExptreeItem.Path.StartsWith("::"))
                {
                    mnuDeleteShortcut.Visible = false;	
                    mnuAddToShortcuts.Visible = false;
                    mnuOpenAsReplayWatcher.Visible = false;
                    mnuLocateFolder.Visible = false;
                    return;
                }
    
            }
            catch (Exception exp)
            {
                log.ErrorFormat(exp.Message);
            }
            
            bool known = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(currentShortcutItem.Path);
            mnuAddToShortcuts.Visible = !known;
            mnuDeleteShortcut.Visible = known;
            mnuOpenAsReplayWatcher.Visible = true;
            mnuLocateFolder.Visible = true;
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
        private void UpdateCameraList(List<CameraSummary> summaries)
        {
            cameraSummaries.Clear();
            
            // Remove lost cameras.
            List<string> lost = new List<string>();
            foreach (ListViewItem lvi in lvCameras.Items)
            {
                CameraSummary found = summaries.FirstOrDefault(s => s.Identifier == lvi.Name);
                if (found == null)
                    lost.Add(lvi.Name);
            }

            foreach (string id in lost)
            {
                cameraIcons.Images.RemoveByKey(lvCameras.Items[id].ImageKey);
                lvCameras.Items.RemoveByKey(id);
            }

            // Consolidate list.
            foreach (CameraSummary summary in summaries)
            {
                cameraSummaries.Add(summary);

                bool known = lvCameras.Items.ContainsKey(summary.Identifier);
                if (known)
                {
                    lvCameras.Items[summary.Identifier].ImageKey = summary.Identifier;
                    continue;
                }

                cameraIcons.Images.Add(summary.Identifier, summary.Icon);
                lvCameras.Items.Add(summary.Identifier, summary.Alias, summary.Identifier);
            }
        }
        
        private void UpdateCamera(CameraSummary summary)
        {
            if(!lvCameras.Items.ContainsKey(summary.Identifier))
                return;
            
            ListViewItem lvi = lvCameras.Items[summary.Identifier];
            int index = IndexOfCamera(cameraSummaries, summary.Identifier);

            cameraSummaries[index] = summary;

            cameraIcons.Images.RemoveByKey(lvi.ImageKey);
            cameraIcons.Images.Add(summary.Identifier, summary.Icon);
            
            lvi.Text = summary.Alias;
            
            // We specify the image by key, but the ListView actually uses the index to 
            // refer to the image. So when we alter the image list, everything is scrambled.
            // Assigning the key again seems to go through the piece of code that recomputes the index and fixes things.
            foreach (ListViewItem item in lvCameras.Items)
                item.ImageKey = item.ImageKey;

            lvCameras.Invalidate();
        }
        
        private int IndexOfCamera(List<CameraSummary> summaries, string id)
        {
            for (int i = 0; i < summaries.Count; i++)
            {
                if (summaries[i].Identifier == id)
                    return i;
            }

            return -1;
        }

        private void ForgetCamera(CameraSummary summary)
        {
            int index = IndexOfCamera(cameraSummaries, summary.Identifier);
            if (index >= 0)
                cameraSummaries.RemoveAt(index);
        }

        private void LvCameras_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = lvCameras.GetItemAt(e.X, e.Y);
            
            if(lvi == null || lvCameras.SelectedItems == null || lvCameras.SelectedItems.Count != 1)
                return;
            
            int index = IndexOfCamera(cameraSummaries, lvi.Name);
            
            if(index >= 0)
                CameraTypeManager.LoadCamera(cameraSummaries[index], -1);
        }
        
        private void lvCameras_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lvi = e.Item as ListViewItem;

            if(lvi == null || lvCameras.SelectedItems == null || lvCameras.SelectedItems.Count != 1)
                return;
            
            int index = IndexOfCamera(cameraSummaries, lvi.Name);
            if(index >= 0)
                DoDragDrop(cameraSummaries[index], DragDropEffects.All);
        }
        #endregion
        
        #region File list
        private void LvCaptured_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LaunchItemAt(lvCaptured, e);
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
        private void UpdateFileList(CShItem folder, ListView listView, bool doRefresh, bool isShortcuts)
        {
            if (folder == null)
                return;

            log.Debug("Updating the file list.");
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


            // Collect the list of supported file.
            ArrayList fileList = folder.GetFiles();
            List<string> filenames = new List<string>();
            foreach(object item in fileList)
            {
                CShItem shellItem = item as CShItem; 
                if(shellItem == null)
                    continue;
                
                try
                {
                    string path = shellItem.Path;
                    string extension = Path.GetExtension(path);
                    if (string.IsNullOrEmpty(extension) || !VideoTypeManager.IsSupported(extension))
                        continue;
                        
                    filenames.Add(path);
                }
                catch(Exception)
                {
                    // Known case : when we are in OS/X parallels context, the path of existing files are invalid.
                    log.ErrorFormat("An error happened while trying to add a file to the file list : {0}", shellItem.Path);
                }
            }

            log.DebugFormat("Collected list of supported files: {0} ms.", stopwatch.ElapsedMilliseconds);

            // Sort the files.
            try
            {
                FileSortAxis axis = PreferencesManager.FileExplorerPreferences.FileSortAxis;
                bool ascending = PreferencesManager.FileExplorerPreferences.FileSortAscending;
                filenames.Sort(new FileComparator(axis, ascending));
            }
            catch(Exception e)
            {
                // Sometimes when renaming a file this might throw with "FileNotFoundException.
                log.ErrorFormat("An error happened while trying to sort files : {0}", e.Message);
            }

            log.DebugFormat("Sorted files: {0} ms.", stopwatch.ElapsedMilliseconds);

            // Push them to the list view.
            foreach (string filename in filenames)
            {
                ListViewItem lvi = new ListViewItem(Path.GetFileName(filename));
                lvi.Tag = filename;
                lvi.ImageIndex = 0;
                listView.Items.Add(lvi);
            }
            
            listView.EndUpdate();

            log.DebugFormat("Updated list view: {0} ms.", stopwatch.ElapsedMilliseconds);

            UpdateFileWatcher(folder);

            if (doRefresh)
                sessionHistory.Add(folder);

            // Even if we don't want to reload the thumbnails, we must ensure that 
            // the screen manager backup list is in sync with the actual file list.
            // desync can happen in case of renaming and deleting files.
            // the screenmanager backup list is used at Unhide(), when we close a screen.
            string folderPath = folder.Path;
            if (!folder.IsFileSystem)
                folderPath = folder.DisplayName;

            log.DebugFormat("Before sending event to thumbnail viewer: {0} ms.", stopwatch.ElapsedMilliseconds);

            NotificationCenter.RaiseCurrentDirectoryChanged(folderPath, filenames, isShortcuts, doRefresh);
            NotificationCenter.RaiseUpdateStatus();
            this.Cursor = Cursors.Default;

            log.DebugFormat("Updated file list: {0} ms.", stopwatch.ElapsedMilliseconds);
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
            CShItem itemToAdd = activeTab == BrowserContentType.Files ? currentExptreeItem : currentShortcutItem; 
            if(itemToAdd == null || itemToAdd.Path.StartsWith("::"))
                return;
            
            ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(itemToAdd.Path), itemToAdd.Path);
            PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
            ReloadShortcuts();
        }
        private void mnuOpenAsReplayWatcher_Click(object sender, EventArgs e)
        {
            CShItem item = activeTab == BrowserContentType.Files ? currentExptreeItem : currentShortcutItem;
            if (item == null || item.Path.StartsWith("::"))
                return;

            string path = Path.Combine(item.Path, "*");
            NotificationCenter.RaiseLoadVideoAsked(path, -1);
        }
        private void mnuLocateFolder_Click(object sender, EventArgs e)
        {
            CShItem item = activeTab == BrowserContentType.Files ? currentExptreeItem : currentShortcutItem;
            if (item == null || string.IsNullOrEmpty(item.Path) || item.Path.StartsWith("::"))
                return;

            FilesystemHelper.LocateDirectory(item.Path);
        }
        private void mnuDeleteShortcut_Click(object sender, EventArgs e)
        {
            DeleteSelectedShortcut();
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

        private void BtnManualClick(object sender, EventArgs e)
        {
            FormCameraWizard wizard = new FormCameraWizard();
            if(wizard.ShowDialog() == DialogResult.OK)
            {
                CameraSummary summary = wizard.Result;
                if(summary != null)
                    CameraTypeManager.UpdatedCameraSummary(summary);
            }
            
            wizard.Dispose();
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

        private void UpdateFileWatcher(CShItem folder)
        {
            fileWatcher.EnableRaisingEvents = false;

            if (folder == null || folder.Path.StartsWith("::"))
                return;

            try
            {
                fileWatcher.Path = folder.Path;
                fileWatcher.EnableRaisingEvents = true;
            }
            catch
            {
                // This happens with archive files, considered directories by Windows.
                log.ErrorFormat("Error while adding path to file watcher. {0}", folder.Path);
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
        protected override bool ExecuteCommand(int cmd)
        {
            FileExplorerCommands command = (FileExplorerCommands)cmd;

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
                    return base.ExecuteCommand(cmd);
            }

            return true;
        }

        private void CommandLaunch()
        {
            ListView lv = GetActiveListView();
            LaunchSelectedVideo(lv);
        }

        private void CommandLaunchWatcher()
        {
            ListView lv = GetActiveListView();
            LaunchWatcherSelectedVideo(lv);
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

        private void LaunchWatcherSelectedVideo(ListView lv)
        {
            string path = GetSelectedVideoPath(lv);
            if (path != null)
            {
                path = Path.Combine(Path.GetDirectoryName(path), "*");
                NotificationCenter.RaiseLoadVideoAsked(path, -1);
            }
        }

        private void LaunchSelectedCamera(ListView lv)
        {
            if (lv == null || !lv.Focused)
                return;

            if (lv.SelectedItems == null || lv.SelectedItems.Count != 1)
                return;

            int index = IndexOfCamera(cameraSummaries, lv.SelectedItems[0].Name);
            if (index >= 0)
                CameraTypeManager.LoadCamera(cameraSummaries[index], -1);
        }

        private void DeleteSelectedCamera(ListView lv)
        {
            if (lv == null || !lv.Focused)
                return;

            if (lv.SelectedItems == null || lv.SelectedItems.Count != 1)
                return;

            int index = IndexOfCamera(cameraSummaries, lv.SelectedItems[0].Name);
            if (index >= 0)
            {
                CameraTypeManager.ForgetCamera(cameraSummaries[index]);
            }
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
