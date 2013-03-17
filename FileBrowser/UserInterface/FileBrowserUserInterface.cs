/*
Copyright © Joan Charmant 2008.
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

namespace Kinovea.FileBrowser
{
	/// <summary>
	/// The user interface for all explorer like stuff.
	/// We maintain the synchronization between the shortcut and exptree tab
	/// when we move between shortcuts. We don't maintain it the other way around.
	/// </summary>
	public partial class FileBrowserUserInterface : UserControl
	{
		#region Members
		private CShItem currentExptreeItem;	  // Current item in exptree tab.
		private CShItem currentShortcutItem;	  // Current item in shortcuts tab.
		private bool expanding;                // True if the exptree is currently auto expanding. To avoid reentry.
		private bool initializing = true;
		private ContextMenuStrip  popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuAddToShortcuts = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDeleteShortcut = new ToolStripMenuItem();
		private ImageList cameraIcons = new ImageList();
		private List<CameraSummary> cameraSummaries = new List<CameraSummary>();
		private bool programmaticTabChange = false;
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor & Initialization
		public FileBrowserUserInterface()
		{
			InitializeComponent();
			lvCameras.SmallImageList = cameraIcons;
			btnAddShortcut.Parent = lblFavFolders;
			btnDeleteShortcut.Parent = lblFavFolders;

			// Drag Drop handling.
			lvExplorer.ItemDrag += lv_ItemDrag;
			lvShortcuts.ItemDrag += lv_ItemDrag;
            etExplorer.AllowDrop = false;
			etShortcuts.AllowDrop = false;
			
			lvCameras.ItemDrag += lvCameras_ItemDrag;
			
			BuildContextMenu();
			
			// Registers our exposed functions to the DelegatePool.
			DelegatesPool dp = DelegatesPool.Instance();
			//dp.RefreshFileExplorer = DoRefreshFileList;
            dp.ChangeFileExplorerTab = DoChangeFileExplorerTab;
            NotificationCenter.RefreshFileExplorer += NotificationCenter_RefreshFileExplorer;
			
			
			
			// Take the list of shortcuts from the prefs and load them.
			ReloadShortcuts();
			
			// Reload last tab from prefs.
			// We don't reload the splitters here, because we are not at full size yet and they are anchored.
			tabControl.SelectedIndex = (int)PreferencesManager.FileExplorerPreferences.ActiveTab;
			
			Application.Idle += new EventHandler(this.IdleDetector);
		}

		private void BuildContextMenu()
		{
			// Add an item to shortcuts
			mnuAddToShortcuts.Image = Properties.Resources.folder_add;
			mnuAddToShortcuts.Click += new EventHandler(mnuAddToShortcuts_Click);
			mnuAddToShortcuts.Visible = false;
			
			// Delete selected shortcut
			mnuDeleteShortcut.Image = Properties.Resources.folder_delete;
			mnuDeleteShortcut.Click += new EventHandler(mnuDeleteShortcut_Click);
			mnuDeleteShortcut.Visible = false;
			
			popMenu.Items.AddRange(new ToolStripItem[] { mnuAddToShortcuts, mnuDeleteShortcut});
			
			// The context menus will be configured on a per event basis.
			etShortcuts.ContextMenuStrip = popMenu;
			etExplorer.ContextMenuStrip = popMenu;
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
			
			// Switch thumbnail area to the right tab.
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.ExplorerTabChanged != null)
			    dp.ExplorerTabChanged((ActiveFileBrowserTab)tabControl.SelectedIndex);
                
			// Load the initial directory.
			DoRefreshFileList(true);
		}
		#endregion

		#region Public interface
		private void NotificationCenter_RefreshFileExplorer(object sender, RefreshFileExplorerEventArgs e)
        {
            DoRefreshFileList(e.RefreshThumbnails);
        }
		private void DoRefreshFileList(bool refreshThumbnails)
		{
			// Called when:
			// - the user changes node in exptree, either explorer or shortcuts
			// - a file modification happens in the thumbnails page. (delete/rename)
			// - a capture is completed.
			
			// We don't update during app start up, because we would most probably
			// end up loading the desktop, and then the saved folder.
            if(initializing)
                return;
			
			// Figure out which tab we are on to update the right listview.
			if(tabControl.SelectedIndex == 0)
			{
				if(currentExptreeItem != null)
					UpdateFileList(currentExptreeItem, lvExplorer, refreshThumbnails, false);
			}
			else if(tabControl.SelectedIndex == 1)
			{
				if(currentShortcutItem != null)
				{
					UpdateFileList(currentShortcutItem, lvShortcuts, refreshThumbnails, true);
				}
				else if(currentExptreeItem != null)
				{
					// This is the special case where we select a folder on the exptree tab
					// and then move to the shortcuts tab.
					// -> reload the hidden list of the exptree tab.
					// We also force the thumbnail refresh, because in this case it is the only way to update the
					// filename list held in ScreenManager...
					UpdateFileList(currentExptreeItem, lvExplorer, true, false);
				}
			}
			else if(tabControl.SelectedIndex == 2)
			{
                // Refresh for cameras.
			}
		}
		public void RefreshUICulture()
		{
			// ExpTree tab.
			//tabPageClassic.Text = FileBrowserLang.tabExplorer;
			tabPageClassic.Text = "";
			lblFolders.Text = FileBrowserLang.lblFolders;
			lblVideoFiles.Text = FileBrowserLang.lblVideoFiles;
			
			// Shortcut tab.
			//tabPageShortcuts.Text = FileBrowserLang.tabShortcuts;
			tabPageShortcuts.Text = "";
			lblFavFolders.Text = lblFolders.Text;
			lblFavFiles.Text = lblVideoFiles.Text;
			etShortcuts.RootDisplayName = tabPageShortcuts.Text;
			
			tabPageCameras.Text = "";
			
			// Menus
			mnuAddToShortcuts.Text = FileBrowserLang.mnuAddToShortcuts;
			mnuDeleteShortcut.Text = FileBrowserLang.mnuDeleteShortcut;
			
			// ToolTips
			ttTabs.SetToolTip(tabPageClassic, FileBrowserLang.tabExplorer);
			ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
			ttTabs.SetToolTip(btnDeleteShortcut, FileBrowserLang.mnuDeleteShortcut);
		}		
		public void ReloadShortcuts()
		{
			ArrayList shortcuts = new ArrayList();
			List<ShortcutFolder> savedShortcuts = PreferencesManager.FileExplorerPreferences.ShortcutFolders;
			foreach(ShortcutFolder shortcut in savedShortcuts)
			    if(Directory.Exists(shortcut.Location))
			        shortcuts.Add(shortcut.Location);
			
			etShortcuts.SetShortcuts(shortcuts);
			etShortcuts.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
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
		public void Closing()
		{
			if(currentExptreeItem != null)
				PreferencesManager.FileExplorerPreferences.LastBrowsedDirectory = currentExptreeItem.Path;
			
			PreferencesManager.FileExplorerPreferences.ExplorerFilesSplitterDistance = splitExplorerFiles.SplitterDistance;
			PreferencesManager.FileExplorerPreferences.ShortcutsFilesSplitterDistance = splitShortcutsFiles.SplitterDistance;
			
			// Flush all prefs not previoulsy flushed.
			PreferencesManager.Save();
		}
		#endregion
		
		#region Explorer tab
		
		#region TreeView
		private void etExplorer_ExpTreeNodeSelected(string selectedPath, CShItem item)
		{
			currentExptreeItem = item;
			
			// Update the list view and thumb page.
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
			etExplorer.Focus();
		}
		private void etExplorer_MouseDown(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				mnuDeleteShortcut.Visible = false;
					
				// User must first select a node to add it to shortcuts.
				if(etExplorer.IsOnSelectedItem(e.Location))
				{
					if(!currentExptreeItem.Path.StartsWith("::"))
					{
						mnuAddToShortcuts.Visible = true;
					}
					else
					{
						// Root node selected. Cannot add.
						mnuAddToShortcuts.Visible = false;
					}
				}
				else
				{
					mnuAddToShortcuts.Visible = false;
				}
			}
		}
		#endregion
		
		#region ListView
		private void lvExplorer_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			LaunchItemAt(lvExplorer, e);
		}
		private void lvExplorer_MouseEnter(object sender, EventArgs e)
		{
			// Give focus to enable mouse scroll.
			lvExplorer.Focus();
		}
		#endregion
		
		#endregion

		#region Shortcuts tab
		
		#region Shortcuts Handling
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
			// Launch the OpenFolder common dialog.
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			
			fbd.ShowNewFolderButton = true;
			fbd.RootFolder = Environment.SpecialFolder.Desktop;

			if (fbd.ShowDialog() == DialogResult.OK && fbd.SelectedPath.Length > 0)
			{
				ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(fbd.SelectedPath), fbd.SelectedPath);
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
				
				IUndoableCommand cds = new CommandDeleteShortcut(this, sf);
		        CommandManager cm = CommandManager.Instance();
		        cm.LaunchUndoableCommand(cds);
				break;
			}	
		}
		#endregion
		
		#region TreeView
		private void etShortcuts_ExpTreeNodeSelected(string _selPath, CShItem _item)
        {
        	// Update the list view and thumb page.
			currentShortcutItem = _item;
			
			// Initializing happens on the explorer tab. We'll refresh later.
			if(!initializing)
			{	
				// The operation that will trigger the thumbnail refresh MUST only be called at the end. 
				// Otherwise the other threads take precedence and the thumbnails are not 
				// shown progressively but all at once, when other operations are over.
				
				// Start by updating hidden explorer tab.
				// Update list and maintain synchronization with the tree.
				UpdateFileList(currentShortcutItem, lvExplorer, false, false);
				
				expanding = true;
				etExplorer.ExpandANode(currentShortcutItem);
				expanding = false;
				currentExptreeItem = etExplorer.SelectedItem;
				
				// Finally update the shortcuts tab, and refresh thumbs.
				UpdateFileList(currentShortcutItem, lvShortcuts, true, true);
			}
        }
        private void etShortcuts_MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enable mouse scroll.
			etShortcuts.Focus();	
        }
        private void etShortcuts_MouseDown(object sender, MouseEventArgs e)
        {
        	if(e.Button != MouseButtons.Right)
        	    return;
        	
        	if(currentExptreeItem == null || !etShortcuts.IsOnSelectedItem(e.Location) || currentExptreeItem.Path.StartsWith("::"))
        	{
        	    mnuDeleteShortcut.Visible = false;	
				mnuAddToShortcuts.Visible = false;
				return;
        	}
        	
        	bool known = PreferencesManager.FileExplorerPreferences.IsShortcutKnown(currentShortcutItem.Path);
        	mnuAddToShortcuts.Visible = !known;
			mnuDeleteShortcut.Visible = known;
        }
        #endregion
		
		#region ListView
		private void lvShortcuts_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			LaunchItemAt(lvShortcuts, e);
		}
		private void lvShortcuts_MouseEnter(object sender, EventArgs e)
		{
			// Give focus to enable mouse scroll.
			lvShortcuts.Focus();
		}
		#endregion
		
		#endregion
		
        #region Camera tab
        private void UpdateCameraList(List<CameraSummary> summaries)
        {
            cameraSummaries.Clear();
            
            // Add new cameras.
            foreach(CameraSummary summary in summaries)
            {
                cameraSummaries.Add(summary);
                
                if(lvCameras.Items.ContainsKey(summary.Identifier))
                    continue;

                cameraIcons.Images.Add(summary.Identifier, summary.Icon);
                lvCameras.Items.Add(summary.Identifier, summary.Alias, summary.Identifier);
            }
            
            // Remove lost cameras.
            List<string> lost = new List<string>();
            foreach(ListViewItem lvi in lvCameras.Items)
            {
                if(IndexOfCamera(summaries, lvi.Name) < 0)
                    lost.Add(lvi.Name);
            }

            foreach(string id in lost)
            {
                cameraIcons.Images.RemoveByKey(lvCameras.Items[id].ImageKey);
                lvCameras.Items.RemoveByKey(id);
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
            foreach(ListViewItem item in lvCameras.Items)
                item.ImageKey = item.ImageKey;
            
            lvCameras.Invalidate();
        }
        
        private int IndexOfCamera(List<CameraSummary> summaries, string id)
        {
            for(int i = 0; i<summaries.Count; i++)
                if(summaries[i].Identifier == id)
                    return i;
            return -1;
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
		
		#region Common
		private void TabControlSelected_IndexChanged(object sender, EventArgs e)
		{
            // Active tab changed.
            // We don't save to file now as this is not a critical data to loose.
            ActiveFileBrowserTab newTab = (ActiveFileBrowserTab)tabControl.SelectedIndex;
            PreferencesManager.FileExplorerPreferences.ActiveTab = newTab;
            
            if(programmaticTabChange)
            {
                programmaticTabChange = false;
            }
            else
            {
                DelegatesPool dp = DelegatesPool.Instance();
                if (dp.ExplorerTabChanged != null)
                    dp.ExplorerTabChanged(newTab);
            }
            
            DoRefreshFileList(true);
        }
        
		private void _tabControl_KeyDown(object sender, KeyEventArgs e)
		{
			// Discard keyboard event as they interfere with player functions
			e.Handled = true;
		}
		private void UpdateFileList(CShItem folder, ListView listView, bool refreshThumbnails, bool shortcuts)
		{
			// Update a file list with the given folder.
			// Triggers an update of the thumbnails pane if requested.
			if(folder == null)
			    return;
			
			this.Cursor = Cursors.WaitCursor;
			
			listView.BeginUpdate();
			listView.Items.Clear();
			
			// Each list element will store the CShItem it's referring to in its Tag property.
			ArrayList fileList = folder.GetFiles();
			
			List<string> fileNames = new List<string>();
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
    			        
                    fileNames.Add(path);
			    }
			    catch(Exception)
                {
                    // Known case : when we are in OS/X parallels context, the path of existing files are invalid.
                    log.ErrorFormat("An error happened while trying to add a file to the file list : {0}", shellItem.Path);
                }
			}
			
			fileNames.Sort(new AlphanumComparator());
			
			foreach(string filename in fileNames)
			{
			    ListViewItem lvi = new ListViewItem(Path.GetFileName(filename));
			    lvi.Tag = filename;
			    lvi.ImageIndex = 6;
			    listView.Items.Add(lvi);
			}
			
			listView.EndUpdate();
										
			// Even if we don't want to reload the thumbnails, we must ensure that 
			// the screen manager backup list is in sync with the actual file list.
			// desync can happen in case of renaming and deleting files.
			// the screenmanager backup list is used at BringBackThumbnail,
			// (i.e. when we close a screen)
			DelegatesPool dp = DelegatesPool.Instance();
			if (dp.CurrentDirectoryChanged != null)
				dp.CurrentDirectoryChanged(shortcuts, fileNames, refreshThumbnails);
			
			this.Cursor = Cursors.Default;
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
			CShItem itemToAdd;
			
			if(tabControl.SelectedIndex == (int)ActiveFileBrowserTab.Explorer)
				itemToAdd = currentExptreeItem;
			else
				itemToAdd = currentShortcutItem;
			
			if(itemToAdd == null || itemToAdd.Path.StartsWith("::"))
			    return;
			
			ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(itemToAdd.Path), itemToAdd.Path);
			PreferencesManager.FileExplorerPreferences.AddShortcut(sf);
			PreferencesManager.Save();
			ReloadShortcuts();
		}
		private void mnuDeleteShortcut_Click(object sender, EventArgs e)
		{
			DeleteSelectedShortcut();
		}
		#endregion
        
		private void DoChangeFileExplorerTab(ActiveFileBrowserTab tab)
		{
            programmaticTabChange = true;
            tabControl.SelectedIndex = (int)tab;
		}
	}
}
