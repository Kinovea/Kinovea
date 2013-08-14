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
        FileBrowserUserInterface view;
        ShortcutFolder shortcut;
        #endregion

        #region Constructor
        public CommandDeleteShortcut(FileBrowserUserInterface view, ShortcutFolder shortcut)
        {
            this.view = view;
            this.shortcut = shortcut;
        }
        #endregion

        public void Execute()
        {
            PreferencesManager.FileExplorerPreferences.RemoveShortcut(shortcut);
            PreferencesManager.Save();
            view.ReloadShortcuts();
        }

        public void Unexecute()
        {
            // Add the shortcut back to the list (if it hasn't been added again in the meantime).
            PreferencesManager.FileExplorerPreferences.AddShortcut(shortcut);
            PreferencesManager.Save();
            view.ReloadShortcuts();
        }
    }
}