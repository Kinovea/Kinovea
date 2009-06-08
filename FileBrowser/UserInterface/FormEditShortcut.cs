#region License
/*
Copyright © Joan Charmant 2008-2009.
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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Videa.Services;

namespace Videa.FileBrowser
{
	/// <summary>
	/// FormEditShortcut allows the user to edit the friendly name
	/// and the location fields of a given shortcut.
	/// The change is only comitted on OK.
	/// The form is also used as an initial step when adding a shortcut.
	/// </summary>
	public partial class formEditShortcut : Form
	{
		#region members
		private ShortcutFolder m_ShortcutFolder;
		#endregion
		public formEditShortcut(ShortcutFolder _shortcut)
		{
			// Keep a reference of the original shortcut so we can modify it on OK.
			m_ShortcutFolder = _shortcut;
			InitializeComponent();
			
			// Populate with current values.
			tbFriendlyName.Text = _shortcut.FriendlyName;
			tbLocation.Text = _shortcut.Location;
		}
		
		#region OK / Cancel handlers
		private void BtnCancelClick(object sender, EventArgs e)
		{
			// Nothing more to do.	
		}
		
		private void BtnOKClick(object sender, EventArgs e)
		{
			// Commit new values.
			// recheck if the folder exists.
			// if not, do not commit. (and warn user ?)
			m_ShortcutFolder.FriendlyName = tbFriendlyName.Text;
			if(Directory.Exists(tbLocation.Text))
			{
				m_ShortcutFolder.Location = tbLocation.Text;
			}
		}
		#endregion
		
		void TbFriendlyNameKeyPress(object sender, KeyPressEventArgs e)
		{
			// 
		}
		
		private void BtnBrowseClick(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
        	
            //fbd.Description = m_ResourceManager.GetString("Updater_BrowseFolderDescription", Thread.CurrentThread.CurrentUICulture);
            fbd.ShowNewFolderButton = true;
            
            // Check if the folder exists.
            //if(File.  Exists(m_ShortcutFolder.Location))
            {
            	fbd.SelectedPath = m_ShortcutFolder.Location;
            }
            /*else
            {
            	fbd.RootFolder = Environment.SpecialFolder.Desktop;
            }*/
            
            if (fbd.ShowDialog() == DialogResult.OK && fbd.SelectedPath.Length > 0)
            {
            	tbLocation.Text = fbd.SelectedPath;
            }
		}
	}
}
