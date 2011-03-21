#region License
/*
Copyright © Joan Charmant 2011.
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
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

using Kinovea.FileBrowser.Languages;
using Kinovea.Services;

namespace Kinovea.FileBrowser
{
    public class CommandDeleteShortcut : IUndoableCommand
    {
		public string FriendlyName
        {
        	get { return FileBrowserLang.mnuDeleteShortcut; }
        }

        #region Members
        FileBrowserUserInterface m_FbUi;
        ShortcutFolder m_shortcut;
        #endregion

        #region Constructor
        public CommandDeleteShortcut(FileBrowserUserInterface _FbUi, ShortcutFolder _shortcut)
        {
        	m_FbUi = _FbUi;
        	m_shortcut = _shortcut;
        }
        #endregion

        public void Execute()
        {
        	PreferencesManager prefManager = PreferencesManager.Instance();
        	
        	// Parse the list and remove any match.
        	for(int i=prefManager.ShortcutFolders.Count-1;i>=0;i--)
			{
        		if(prefManager.ShortcutFolders[i].Location == m_shortcut.Location)
				{
        			prefManager.ShortcutFolders.RemoveAt(i);
        		}
        	}
        	
        	prefManager.Export();
        	
        	// Refresh the list.
			m_FbUi.ReloadShortcuts();
        }

        public void Unexecute()
        {
        	// Add the shortcut back to the list (if it hasn't been added again in the meantime).
        	PreferencesManager prefManager = PreferencesManager.Instance();
        	
			bool bIsShortcutAlready = false;
			foreach(ShortcutFolder sf in prefManager.ShortcutFolders)
			{
				if(sf.Location == m_shortcut.Location)
				{
					bIsShortcutAlready = true;
					break;
				}
			}
        	
			if(!bIsShortcutAlready)
			{
				prefManager.ShortcutFolders.Add(m_shortcut);
				prefManager.Export();
				
				// Refresh the list.
				m_FbUi.ReloadShortcuts();
			}
        }
    }
}