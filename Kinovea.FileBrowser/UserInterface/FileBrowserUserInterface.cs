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

using ExpTreeLib;
using Kinovea.Camera;
using Kinovea.FileBrowser.Languages;
using Kinovea.Services;
using Kinovea.Video;
using System.Drawing;
using System.Globalization;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Kinovea.FileBrowser
{
    /// <summary>
    /// The user interface for all explorer like stuff.
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
        private ActiveFileBrowserTab activeTab;
        private FileSystemWatcher fileWatcher = new FileSystemWatcher();

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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor & Initialization
        public FileBrowserUserInterface()
        {
            InitializeComponent();
            
            lvCameras.SmallImageList = cameraIcons;
            cameraIcons.Images.Add("historyEntryDay", Properties.Resources.calendar_view_day);
            cameraIcons.Images.Add("historyEntryMonth", Properties.Resources.calendar_view_month);
            cameraIcons.Images.Add("unknownCamera", Properties.Resources.film_small);

            btnAddShortcut.Parent = lblFavFolders;
            btnDeleteShortcut.Parent = lblFavFolders;
            
            // Drag Drop handling.
            lvExplorer.ItemDrag += lv_ItemDrag;
            lvShortcuts.ItemDrag += lv_ItemDrag;
            lvCaptured.ItemDrag += lv_ItemDrag;

            etExplorer.AllowDrop = false;
            etShortcuts.AllowDrop = false;
            
            lvCameras.ItemDrag += lvCameras_ItemDrag;
            
            BuildContextMenu();
            
            NotificationCenter.ExplorerTabChanged += NotificationCenter_ExplorerTabChangeAsked;
            NotificationCenter.RefreshFileExplorer += NotificationCenter_RefreshFileExplorer;
            NotificationCenter.FileSelected += NotificationCenter_FileSelected;
            NotificationCenter.FileOpened += NotificationCenter_FileOpened;
            NotificationCenter.FolderChangeAsked += NotificationCenter_FolderChangeAsked;
            NotificationCenter.FolderNavigationAsked += NotificationCenter_FolderNavigationAsked;

            // Reload stored persistent information.
            ReloadShortcuts();
            InitializeFileWatcher();
            
            // Reload last tab from prefs.
            // We don't reload the splitters here, because we are not at full size yet and they are anchored.
            tabControl.SelectedIndex = (int)PreferencesManager.FileExplorerPreferences.ActiveTab;
            activeTab = PreferencesManager.FileExplorerPreferences.ActiveTab;
            
            Application.Idle += new EventHandler(this.IdleDetector);
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys("FileExplorer");
        }

        private void BuildContextMenu()
        {
            // Add an item to shortcuts
            mnuAddToShortcuts.Image = Properties.Resources.folder_add;
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
            
            popMenuFolders.Items.AddRange(new ToolStripItem[] { mnuAddToShortcuts, mnuOpenAsReplayWatcher, mnuLocateFolder, mnuDeleteShortcut });
            
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
                mnuLaunchWatcher,
                new ToolStripSeparator(),
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
            
            // Now that we are at full size, we can load splitters from prefs.
            splitExplorerFiles.SplitterDistance = PreferencesManager.FileExplorerPreferences.ExplorerFilesSplitterDistance;
            splitShortcutsFiles.SplitterDistance = PreferencesManager.FileExplorerPreferences.ShortcutsFilesSplitterDistance;

            // Prune any captured file removed since last run.
            PreferencesManager.FileExplorerPreferences.ConsolidateRecentCapturedFiles();

            NotificationCenter.RaiseExplorerTabChanged(this, (ActiveFileBrowserTab)tabControl.SelectedIndex);
                
            DoRefreshFileList(true);
        }
        #endregion

        #region Public interface
        private void NotificationCenter_ExplorerTabChangeAsked(object sender, ExplorerTabEventArgs e)
        {
            if (sender == this)
                return;

            programmaticTabChange = true;
            tabControl.SelectedIndex = (int)e.Tab;
        }
        private void NotificationCenter_RefreshFileExplorer(object sender, RefreshFileExplorerEventArgs e)
        {
            DoRefreshFileList(e.RefreshThumbnails);
        }
        private void NotificationCenter_FileSelected(object sender, FileActionEventArgs e)
        {
            if (sender == this)
                return;

            if (activeTab == ActiveFileBrowserTab.Cameras)
                return;

            // Find the file and select it here.
            ListView lv = GetFileListview();
            lv.SelectedItems.Clear();

            if (string.IsNullOrEmpty(e.File))
                return;

            foreach (ListViewItem item in lv.Items)
            {
                if ((string)item.Tag != e.File)
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
                case ActiveFileBrowserTab.Shortcuts:
                    return lvShortcuts;
                case ActiveFileBrowserTab.Cameras:
                    return lvCaptured;
                case ActiveFileBrowserTab.Explorer:
                default:
                    return lvExplorer;
            }
        }

        private void NotificationCenter_FileOpened(object sender, FileActionEventArgs e)
        {
            // Create a virtual shortcut for the folder of the opened video and select it.
            LoadFolderInShortcuts(Path.GetDirectoryName(e.File));
        }
        private void NotificationCenter_FolderChangeAsked(object sender, FileActionEventArgs e)
        {
            // The thumbnail viewer is asking for a different folder to be shown.
            // The path to the new folder is stored in the File property of the event arg.
            if (activeTab == ActiveFileBrowserTab.Shortcuts)
            {
                LoadFolderInShortcuts(e.File);
            }
            else if (activeTab == ActiveFileBrowserTab.Explorer)
            {
                etExplorer.ExpandANode(e.File);
            }
        }

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

            // Trigger an update of the file list.
            // In shortcuts, update both, in explorer, update only the explorer.
            if (activeTab == ActiveFileBrowserTab.Shortcuts)
            {
                LoadFolderInShortcuts(sessionHistory.Current.Path);
            }
            else if (activeTab == ActiveFileBrowserTab.Explorer)
            {
                etExplorer.ExpandANode(sessionHistory.Current);
            }

            // End the navigating operation.
            sessionHistory.Navigating = false;
        }

        /// <summary>
        /// Add the folder as a transient shortcut and select it.
        /// This will also trigger the same folder to be selected in the explorer tab and and refreshes the thumbnails.
        /// </summary>
        private void LoadFolderInShortcuts(string path)
        {
            lastOpenedDirectory = path;

            // If the shortcuts list is already on the right folder don't do anything.
            if (activeTab == ActiveFileBrowserTab.Shortcuts && currentShortcutItem != null && currentShortcutItem.Path == lastOpenedDirectory)
                return;
            
            if (path.StartsWith("."))
                return;

            // Reload the list including the new last opened directory.
            ReloadShortcuts();

            // Select the added folder.
            if (activeTab == ActiveFileBrowserTab.Shortcuts)
            {
                // We can't currently add special directories to the the shortcuts, except the desktop.
                // If the user adds special folders to the history stack the navigation is broken.
                // We call "ExpandANode" but this will only work if the folder is already there,
                // so at the moment it only works on the Desktop.
                if (sessionHistory.Current != null && !sessionHistory.Current.IsFileSystem)
                {
                    etShortcuts.ExpandANode(sessionHistory.Current);
                }
                else
                {
                    etShortcuts.SelectNode(lastOpenedDirectory);
                }
            }
            else if (activeTab == ActiveFileBrowserTab.Explorer)
            {
                etExplorer.ExpandANode(sessionHistory.Current);
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
            if(activeTab == ActiveFileBrowserTab.Explorer)
            {
                if(currentExptreeItem != null)
                    UpdateFileList(currentExptreeItem, lvExplorer, refreshThumbnails, false);

                // TODO: synchronize shortcuts tab.

            }
            else if(activeTab == ActiveFileBrowserTab.Shortcuts)
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
            else if(activeTab == ActiveFileBrowserTab.Cameras)
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
            mnuSortBy.Text = "Sort by";
            mnuSortByName.Text = "Name";
            mnuSortByDate.Text = "Date";
            mnuSortBySize.Text = "Size";
            mnuSortAscending.Text = "Ascending";
            mnuSortDescending.Text = "Descending";
            mnuLaunch.Text = FileBrowserLang.Generic_Open;
            mnuLaunchWatcher.Text = FileBrowserLang.mnuOpenAsReplayWatcher;
            mnuLocate.Text = FileBrowserLang.mnuVideoLocate;
            mnuDelete.Text = FileBrowserLang.mnuVideoDelete;
            mnuCameraLaunch.Text = FileBrowserLang.Generic_Open;
            mnuCameraForget.Text = FileBrowserLang.ForgetCustomSettings;

            // ToolTips
            ttTabs.SetToolTip(tabPageClassic, FileBrowserLang.tabExplorer);
            ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
            ttTabs.SetToolTip(btnDeleteShortcut, FileBrowserLang.mnuDeleteShortcut);
        }
        
        /// <summary>
        /// Reload the saved shortcuts plus the current folder as a transient shortcut into the shortcut tree.
        /// </summary>
        public void ReloadShortcuts()
        {
            ArrayList shortcuts = GetShortcuts();
            etShortcuts.SetShortcuts(shortcuts);
            etShortcuts.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
        }

        /// <summary>
        /// Get a list of the saved shortcuts plus whatever the last opened directory is.
        /// </summary>
        /// <returns></returns>
        private ArrayList GetShortcuts()
        {
            ArrayList shortcuts = new ArrayList();
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
            
            PreferencesManager.FileExplorerPreferences.ExplorerFilesSplitterDistance = splitExplorerFiles.SplitterDistance;
            PreferencesManager.FileExplorerPreferences.ShortcutsFilesSplitterDistance = splitShortcutsFiles.SplitterDistance;
            PreferencesManager.Save();
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
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
            {
                ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(dialog.FileName), dialog.FileName);
                PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
                PreferencesManager.Save();
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
                PreferencesManager.Save();
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
            // We don't save to file now as this is not a critical data to loose.
            activeTab = (ActiveFileBrowserTab)tabControl.SelectedIndex;
            PreferencesManager.FileExplorerPreferences.ActiveTab = activeTab;
            
            if(programmaticTabChange)
                programmaticTabChange = false;
            else
                NotificationCenter.RaiseExplorerTabChanged(this, activeTab);
            
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

            // Push them to the list view.
            foreach (string filename in filenames)
            {
                ListViewItem lvi = new ListViewItem(Path.GetFileName(filename));
                lvi.Tag = filename;
                lvi.ImageIndex = 0;
                listView.Items.Add(lvi);
            }
            
            listView.EndUpdate();

            UpdateFileWatcher(folder);

            if (doRefresh)
                sessionHistory.Add(folder);

            // Even if we don't want to reload the thumbnails, we must ensure that 
            // the screen manager backup list is in sync with the actual file list.
            // desync can happen in case of renaming and deleting files.
            // the screenmanager backup list is used at BringBackThumbnail,
            // (i.e. when we close a screen)
            string folderPath = folder.Path;
            if (!folder.IsFileSystem)
                folderPath = folder.DisplayName;


            NotificationCenter.RaiseCurrentDirectoryChanged(this, folderPath, filenames, isShortcuts, doRefresh);
            this.Cursor = Cursors.Default;
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

        private void lv_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListViewItem lvi = e.Item as ListViewItem;
            if (lvi == null)
                return;
            
            string path = lvi.Tag as string;
            if(path == null)
                return;
            
            DoDragDrop(path, DragDropEffects.All);
        }

        private void listViews_MouseDown(object sender, MouseEventArgs e)
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
                
            VideoTypeManager.LoadVideo(path, -1);
        }
        #endregion
        
        #region Menu Event Handlers
        private void mnuAddToShortcuts_Click(object sender, EventArgs e)
        {
            CShItem itemToAdd = activeTab == ActiveFileBrowserTab.Explorer ? currentExptreeItem : currentShortcutItem; 
            if(itemToAdd == null || itemToAdd.Path.StartsWith("::"))
                return;
            
            ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(itemToAdd.Path), itemToAdd.Path);
            PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
            PreferencesManager.Save();
            ReloadShortcuts();
        }
        private void mnuOpenAsReplayWatcher_Click(object sender, EventArgs e)
        {
            CShItem item = activeTab == ActiveFileBrowserTab.Explorer ? currentExptreeItem : currentShortcutItem;
            if (item == null || item.Path.StartsWith("::"))
                return;

            string path = Path.Combine(item.Path, "*");
            VideoTypeManager.LoadVideo(path, -1);
        }
        private void mnuLocateFolder_Click(object sender, EventArgs e)
        {
            CShItem item = activeTab == ActiveFileBrowserTab.Explorer ? currentExptreeItem : currentShortcutItem;
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
            PreferencesManager.Save();
            DoRefreshFileList(true);
        }

        private void UpdateSortAscending(bool ascending)
        {
            PreferencesManager.FileExplorerPreferences.FileSortAscending = ascending;
            PreferencesManager.Save();
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

        private void listViews_SelectedIndexChanged(object sender, EventArgs e)
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
                case ActiveFileBrowserTab.Shortcuts:
                    return lvShortcuts;
                case ActiveFileBrowserTab.Cameras:
                    return lvCaptured;
                case ActiveFileBrowserTab.Explorer:
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
                VideoTypeManager.LoadVideo(path, -1);
        }

        private void LaunchWatcherSelectedVideo(ListView lv)
        {
            string path = GetSelectedVideoPath(lv);
            if (path != null)
            {
                path = Path.Combine(Path.GetDirectoryName(path), "*");
                VideoTypeManager.LoadVideo(path, -1);
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
                if (activeTab == ActiveFileBrowserTab.Cameras)
                    PreferencesManager.FileExplorerPreferences.ConsolidateRecentCapturedFiles();

                DoRefreshFileList(true);
            }
        }

        private void CommandLocate()
        {
            if (activeTab == ActiveFileBrowserTab.Explorer)
                LocateSelectedVideo(lvExplorer);
            else if (activeTab == ActiveFileBrowserTab.Shortcuts)
                LocateSelectedVideo(lvShortcuts);
            else if (activeTab == ActiveFileBrowserTab.Cameras)
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
