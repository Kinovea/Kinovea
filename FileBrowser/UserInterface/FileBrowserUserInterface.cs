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


using Kinovea.FileBrowser.Languages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using ExpTreeLib;
using Kinovea.Services;

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
		private CShItem m_CurrentExptreeItem;	// Current item in exptree tab.
		private CShItem m_CurrentShortcutItem;	// Current item in shortcuts tab.
		private bool m_bExpanding;			// True if the exptree is currently auto expanding. To avoid reentry.
		private bool m_bInitializing = true;
		private ResourceManager m_ResManager = new ResourceManager("Kinovea.FileBrowser.Languages.FileBrowserLang", Assembly.GetExecutingAssembly());
		private PreferencesManager m_PreferencesManager = PreferencesManager.Instance();
		
		#region Context menu
		private ContextMenuStrip  popMenu = new ContextMenuStrip();
		private ToolStripMenuItem mnuAddToShortcuts = new ToolStripMenuItem();
		private ToolStripMenuItem mnuDeleteShortcut = new ToolStripMenuItem();
		#endregion
		
		private static readonly string[] m_KnownFileTypes = { ".3gp", ".asf", ".avi", ".dv", ".flv", ".f4v", ".m1v", ".m2p", ".m2t",
			".m2ts", ".mts", ".m2v", ".m4v", ".mkv", ".mod", ".mov", ".moov", ".mpg", ".mpeg", ".tod", ".mxf",
			".mp4", ".mpv", ".ogg", ".ogm", ".ogv", ".qt", ".rm", ".swf", ".vob",
			".wmv", ".dpa", 
			".jpg", ".jpeg", ".png", ".bmp", ".gif"	};
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		#region Constructor & Initialization
		public FileBrowserUserInterface()
		{
			InitializeComponent();
			this.Dock = DockStyle.Fill;
			btnAddShortcut.Parent = lblFavFolders;
			btnDeleteShortcut.Parent = lblFavFolders;

			// Drag Drop handling.
			lvExplorer.ItemDrag += new ItemDragEventHandler(lv_ItemDrag);
			lvShortcuts.ItemDrag += new ItemDragEventHandler(lv_ItemDrag);
            etExplorer.AllowDrop = false;
			etShortcuts.AllowDrop = false;
			
			BuildContextMenu();
			
			// Associate with the system icons for files... Is this really needed ?
			//ExpTreeLib.SystemImageListManager.SetListViewImageList(lvExplorer, false, false);
			//ExpTreeLib.SystemImageListManager.SetListViewImageList(lvShortcuts, false, false);
			
			// Registers our exposed functions to the DelegatePool.
			DelegatesPool dp = DelegatesPool.Instance();
			dp.RefreshFileExplorer = DoRefreshFileList;
			
			// Take the list of shortcuts from the prefs and load them.
			ReloadShortcuts();
			
			// Reload last tab from prefs.
			// We don't reload the splitters here, because we are not at full size yet and they are anchored.
			tabControl.SelectedIndex = (int)m_PreferencesManager.ActiveTab;
			
			Application.Idle += new EventHandler(this.IdleDetector);
		}
		private void BuildContextMenu()
		{
			// Add an item to shortcuts
			mnuAddToShortcuts.Tag = new ItemResourceInfo(m_ResManager, "mnuAddToShortcuts");
			mnuAddToShortcuts.Text = ((ItemResourceInfo)mnuAddToShortcuts.Tag).resManager.GetString(((ItemResourceInfo)mnuAddToShortcuts.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuAddToShortcuts.Image = Properties.Resources.folder_add;
			mnuAddToShortcuts.Click += new EventHandler(mnuAddToShortcuts_Click);
			mnuAddToShortcuts.Visible = false;
			
			// Delete selected shortcut
			mnuDeleteShortcut.Tag = new ItemResourceInfo(m_ResManager, "mnuDeleteShortcut");
			mnuDeleteShortcut.Text = ((ItemResourceInfo)mnuDeleteShortcut.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteShortcut.Tag).strText, Thread.CurrentThread.CurrentUICulture);
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
			m_bInitializing = false;
			
			// Now that we are at full size, we can load splitters from prefs.
			splitExplorerFiles.SplitterDistance = m_PreferencesManager.ExplorerFilesSplitterDistance;
			splitShortcutsFiles.SplitterDistance = m_PreferencesManager.ShortcutsFilesSplitterDistance;
			
			// Load the initial directory.
			log.Debug("Load initial directory.");
			DoRefreshFileList(true);
		}
		#endregion

		#region Public interface
		public void DoRefreshFileList(bool _bRefreshThumbnails)
		{
			// Called when:
			// - the user changes node in exptree, either explorer or shortcuts
			// - a file modification happens in the thumbnails page. (delete/rename)
			
			log.Debug("DoRefreshFileList called");
			
			// We don't update during app start up, because we would most probably
			// end up loading the desktop, and then the saved folder.
			if(!m_bInitializing)
			{
				// Figure out which tab we are on to update the right listview.
				if(tabControl.SelectedIndex == 0)
				{
					// ExpTree tab.
					if(m_CurrentExptreeItem != null)
					{
						UpdateFileList(m_CurrentExptreeItem, lvExplorer, _bRefreshThumbnails);
					}
				}
				else if(tabControl.SelectedIndex == 1)
				{
					// Shortcuts tab.
					if(m_CurrentShortcutItem != null)
					{
						UpdateFileList(m_CurrentShortcutItem, lvShortcuts, _bRefreshThumbnails);
					}
					else if(m_CurrentExptreeItem != null)
					{
						// This is the special case where we select a folder on the exptree tab
						// and then move to the shortcuts tab.
						// -> reload the hidden list of the exptree tab.
						// We also force the thumbnail refresh, because in this case it is the only way to update the
						// filename list held in ScreenManager...
						UpdateFileList(m_CurrentExptreeItem, lvExplorer, true);
					}
				}
			}
		}
		public void RefreshUICulture()
		{
			// ExpTree tab.
			tabPageClassic.Text = FileBrowserLang.tabExplorer;
			lblFolders.Text = FileBrowserLang.lblFolders;
			lblVideoFiles.Text = FileBrowserLang.lblVideoFiles;
			
			// Shortcut tab.
			tabPageShortcuts.Text = FileBrowserLang.tabShortcuts;
			lblFavFolders.Text = lblFolders.Text;
			lblFavFiles.Text = lblVideoFiles.Text;
			etShortcuts.RootDisplayName = tabPageShortcuts.Text;
			
			// Menus
			mnuAddToShortcuts.Text = ((ItemResourceInfo)mnuAddToShortcuts.Tag).resManager.GetString(((ItemResourceInfo)mnuAddToShortcuts.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			mnuDeleteShortcut.Text = ((ItemResourceInfo)mnuDeleteShortcut.Tag).resManager.GetString(((ItemResourceInfo)mnuDeleteShortcut.Tag).strText, Thread.CurrentThread.CurrentUICulture);
			
			// ToolTips
			ttTabs.SetToolTip(btnAddShortcut, FileBrowserLang.mnuAddShortcut);
			ttTabs.SetToolTip(btnDeleteShortcut, FileBrowserLang.mnuDeleteShortcut);
		}
		public void Closing()
		{
			if(m_CurrentExptreeItem != null)
			{
				m_PreferencesManager.LastBrowsedDirectory = m_CurrentExptreeItem.Path;
			}
			
			m_PreferencesManager.ExplorerFilesSplitterDistance = splitExplorerFiles.SplitterDistance;
			m_PreferencesManager.ShortcutsFilesSplitterDistance = splitShortcutsFiles.SplitterDistance;
			
			// Flush all prefs not previoulsy flushed.
			m_PreferencesManager.Export();
		}
		#endregion
		
		#region Explorer tab
		
		#region TreeView
		private void etExplorer_ExpTreeNodeSelected(string _selPath, CShItem _item)
		{
			m_CurrentExptreeItem = _item;
			
			// Update the list view and thumb page.
			if(!m_bExpanding && !m_bInitializing)
			{
				// We don't maintain synchronization with the Shortcuts tab. 
				ResetShortcutList();
				
				UpdateFileList(m_CurrentExptreeItem, lvExplorer, true);				
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
					if(!m_CurrentExptreeItem.Path.StartsWith("::"))
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
		private void ReloadShortcuts()
		{
			// Refresh the folder tree with data stored in prefs.
			ArrayList shortcuts = new ArrayList();
			List<ShortcutFolder> scuts = m_PreferencesManager.ShortcutFolders;
			
			// Sort by last folder name.
			scuts.Sort();
			
			foreach(ShortcutFolder sf in scuts)
			{
				if(Directory.Exists(sf.Location))
					shortcuts.Add(sf.Location);
			}
			
			etShortcuts.SetShortcuts(shortcuts);
			etShortcuts.StartUpDirectory = ExpTreeLib.ExpTree.StartDir.Desktop;
		}
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
			
			//fbd.Description = m_ResourceManager.GetString("Updater_BrowseFolderDescription", Thread.CurrentThread.CurrentUICulture);
			fbd.ShowNewFolderButton = true;
			fbd.RootFolder = Environment.SpecialFolder.Desktop;

			if (fbd.ShowDialog() == DialogResult.OK && fbd.SelectedPath.Length > 0)
			{
				// Default the friendly name to the folder name.
				ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(fbd.SelectedPath), fbd.SelectedPath);
				m_PreferencesManager.ShortcutFolders.Add(sf);
				m_PreferencesManager.Export();
				ReloadShortcuts();
				SelectShortcut(sf);
			}
		}
		private void DeleteSelectedShortcut()
		{
			// TODO: Ask for confirmation, or better make an undoable command.
			
			if(m_CurrentShortcutItem != null)
			{
				// Find and delete the shortcut.
				foreach(ShortcutFolder sf in m_PreferencesManager.ShortcutFolders)
				{
					if(sf.Location == m_CurrentShortcutItem.Path)
					{
						m_PreferencesManager.ShortcutFolders.Remove(sf);
									
						// Refresh the list.
						ReloadShortcuts();
						ResetShortcutList();
						
						break;
					}
				}	
			}
		}
		private void ResetShortcutList()
		{
			lvShortcuts.Clear();
		}
		#endregion
		
		#region TreeView
		private void etShortcuts_ExpTreeNodeSelected(string _selPath, CShItem _item)
        {
        	// Update the list view and thumb page.
        	log.Debug(String.Format("Shortcut Selected : {0}.", Path.GetFileName(_selPath)));
			m_CurrentShortcutItem = _item;
			
			// Initializing happens on the explorer tab. We'll refresh later.
			if(!m_bInitializing)
			{	
				// The operation that will trigger the thumbnail refresh MUST only be called at the end. 
				// Otherwise the other threads take precedence and the thumbnails are not 
				// shown progressively but all at once, when other operations are over.
				
				// Start by updating hidden explorer tab.
				// Update list and maintain synchronization with the tree.
				UpdateFileList(m_CurrentShortcutItem, lvExplorer, false);
				
				m_bExpanding = true;
				etExplorer.ExpandANode(m_CurrentShortcutItem);
				m_bExpanding = false;
				m_CurrentExptreeItem = etExplorer.SelectedItem;
				
				// Finally update the shortcuts tab, and refresh thumbs.
				UpdateFileList(m_CurrentShortcutItem, lvShortcuts, true);
			}
			log.Debug("Shortcut Selected - Operations done.");
        }
        private void etShortcuts_MouseEnter(object sender, EventArgs e)
        {
        	// Give focus to enable mouse scroll.
			etShortcuts.Focus();	
        }
        private void etShortcuts_MouseDown(object sender, MouseEventArgs e)
        {
        	if(e.Button == MouseButtons.Right)
			{        		
				// User must first select a node to add it to shortcuts.
				// Otherwise we don't have the infos about the item.
				if(m_CurrentExptreeItem != null && etShortcuts.IsOnSelectedItem(e.Location))
				{
					if(!m_CurrentExptreeItem.Path.StartsWith("::"))
					{
						// Do we have it already ?
						bool bIsShortcutAlready = false;
						foreach(ShortcutFolder sf in m_PreferencesManager.ShortcutFolders)
						{
							if(m_CurrentShortcutItem.Path == sf.Location)
							{
								bIsShortcutAlready = true;
								break;
							}
						}
						
						if(bIsShortcutAlready)
						{
							// Cannot add, can delete.
				        	mnuAddToShortcuts.Visible = false;
							mnuDeleteShortcut.Visible = true;
						}
						else
						{
							// Can add, cannot delete.
							mnuAddToShortcuts.Visible = true;
							mnuDeleteShortcut.Visible = false;
							
						}
					}
					else
					{
						mnuDeleteShortcut.Visible = false;	
						mnuAddToShortcuts.Visible = false;
					}
				}
				else
				{
					mnuDeleteShortcut.Visible = false;	
					mnuAddToShortcuts.Visible = false;
				}
			}	
        }
        private void SelectShortcut(ShortcutFolder _shortcut)
		{
        	// Selected a particular node.
        	// Might be used at startup and after adding a shortcut.
        	// TODO
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
		
		#region Common
		private void TabControlSelectedIndexChanged(object sender, EventArgs e)
		{
			// Active tab changed.
			// We don't save to file now as this is not a critical data to loose.
			m_PreferencesManager.ActiveTab = (ActiveFileBrowserTab)tabControl.SelectedIndex;
		}
		private void _tabControl_KeyDown(object sender, KeyEventArgs e)
		{
			// Discard keyboard event as they interfere with player functions
			e.Handled = true;
		}
		private bool IsKnownFileType(string extension)
		{
			// Check if the file is of known extension.
			// All known extensions are kept in a static array.
			
			// Todo ?: load the known extensions from a file so we can add to them dynamically
			// and the user can add or remove depending on his specific config ?
			bool bIsKnown = false;

			foreach (string known in m_KnownFileTypes)
			{
				if(extension.Equals(known, StringComparison.OrdinalIgnoreCase))
				{
					bIsKnown = true;
					break;
				}
			}

			return bIsKnown;
		}
		private void UpdateFileList(CShItem _Folder, ListView _ListView, bool _bRefreshThumbnails)
		{
			// Update a file list with the given folder.
			// Triggers an update of the thumbnails pane if requested.
			
			log.Debug(String.Format("Updating file list : {0}", _ListView.Name));
			
			if(_Folder != null)
			{
				this.Cursor = Cursors.WaitCursor;
				
				_ListView.BeginUpdate();
				_ListView.Items.Clear();
				
				// Each list element will store the CShItem it's referring to in its Tag property.
				// Get all files in the folder, and add them to the list.
				ArrayList fileList = _Folder.GetFiles();
				List<String> fileNames = new List<string>();
				for (int i = 0; i < fileList.Count; i++)
				{
					CShItem shellItem = (CShItem)fileList[i];
					
					if (IsKnownFileType(Path.GetExtension(shellItem.Path)))
					{
						ListViewItem lvi = new ListViewItem(shellItem.DisplayName);
						
						lvi.Tag = shellItem;
						//lvi.ImageIndex = ExpTreeLib.SystemImageListManager.GetIconIndex(ref shellItem, false, false);
						lvi.ImageIndex = 6;
						
						_ListView.Items.Add(lvi);
						
						fileNames.Add(shellItem.Path);
					}
				}
				
				_ListView.EndUpdate();
				log.Debug("List updated");
											
				// Even if we don't want to reload the thumbnails, we must ensure that 
				// the screen manager backup list is in sync with the actual file list.
				// desync can happen in case of renaming and deleting files.
				// the screenmanager backup list is used at BringBackThumbnail,
				// (i.e. when we close a screen)
				DelegatesPool dp = DelegatesPool.Instance();
				if (dp.DisplayThumbnails != null)
				{
					log.Debug("Asking the ScreenManager to refresh the thumbnails.");
					dp.DisplayThumbnails(fileNames, _bRefreshThumbnails);
				}
				
				this.Cursor = Cursors.Default;
				
			}
		}
		private void lv_ItemDrag(object sender, ItemDragEventArgs e)
		{
			// Starting a drag drop.
			ListViewItem lvi = e.Item as ListViewItem;
			if (lvi != null)
			{
				CShItem item = lvi.Tag as CShItem;
				if(item != null)
				{
					if (item.IsFileSystem)
					{
						DoDragDrop(item.Path, DragDropEffects.All);
					}
				}
			}
		}
		private void LaunchItemAt(ListView _listView, MouseEventArgs e)
		{
			// Launch the video.
			
			ListViewItem lvi = _listView.GetItemAt(e.X, e.Y);
			
			if (lvi != null && _listView.SelectedItems != null && _listView.SelectedItems.Count == 1)
			{
				CShItem item = _listView.SelectedItems[0].Tag as CShItem;
				
				if(item != null)
				{
					if(item.IsFileSystem)
					{
						DelegatesPool dp = DelegatesPool.Instance();
						if (dp.LoadMovieInScreen != null)
						{
							dp.LoadMovieInScreen(item.Path, -1, true);
						}
					}
				}
			}
		}
		#endregion
		
		#region Menu Event Handlers
		private void mnuAddToShortcuts_Click(object sender, EventArgs e)
		{
			CShItem itemToAdd;
			
			if(tabControl.SelectedIndex == (int)ActiveFileBrowserTab.Explorer)
			{				
				itemToAdd = m_CurrentExptreeItem;
			}
			else
			{
				itemToAdd = m_CurrentShortcutItem;
			}
			
			if(itemToAdd != null)
			{
				// Don't add if root node. (Special Folder)
				if(!itemToAdd.Path.StartsWith("::"))
				{
					ShortcutFolder sf = new ShortcutFolder(Path.GetFileName(itemToAdd.Path), itemToAdd.Path);
					m_PreferencesManager.ShortcutFolders.Add(sf);
					m_PreferencesManager.Export();
					ReloadShortcuts();
				}
			}						
		}
		private void mnuDeleteShortcut_Click(object sender, EventArgs e)
		{
			DeleteSelectedShortcut();
		}
		#endregion
        
	}
}
