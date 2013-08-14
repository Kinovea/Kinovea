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
using System.Collections.Generic;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace Kinovea.Services
{
    /// <summary>
    /// Manages Commands execution and undo/redo mechanics.
    /// Optionnellement couplé à un menu undo/redo
    /// </summary>
    /// <remarks>Design Pattern : Singleton</remarks>
    public class CommandManager
    {
        #region Members
        private List<IUndoableCommand> commandStack;
        private int playHead;
        private Boolean isEmpty;
        private ToolStripMenuItem undoMenu;
        private ToolStripMenuItem redoMenu;
        private static CommandManager instance = null;
        #endregion

        #region Instance
        public static CommandManager Instance()
        {
            if (instance == null)
                instance = new CommandManager();

            return instance;
        }

        private CommandManager()
        {
            commandStack = new List<IUndoableCommand>();
            playHead = -1;
            isEmpty = true;
        }
        #endregion

        #region Implementation
        public static void LaunchCommand(ICommand command)
        {
            if (command != null) 
                command.Execute();
        }
        public void LaunchUndoableCommand(IUndoableCommand command)
        {
            if (command == null)
                return;
            
            if ((commandStack.Count - 1) > playHead)
                commandStack.RemoveRange(playHead + 1, (commandStack.Count - 1) - playHead);

            commandStack.Add(command);
            playHead = commandStack.Count - 1;
            isEmpty = false;
                
            DoCurrentCommand();
            UpdateMenus();
            
        }
        private void DoCurrentCommand()
        {
            if (!isEmpty)
                commandStack[playHead].Execute();
        }
        public void Undo()
        {
            if (!isEmpty)
            {
                commandStack[playHead].Unexecute();
                playHead--;
            }

            UpdateMenus();
        }
        public void Redo()
        {
            if ((commandStack.Count - 1) <= playHead)
                return;
            
            playHead++;
            DoCurrentCommand();
            UpdateMenus();
        }
        public void RegisterUndoMenu(ToolStripMenuItem undoMenu)
        {
            if (undoMenu != null) 
                this.undoMenu = undoMenu;
        }
        public void RegisterRedoMenu(ToolStripMenuItem redoMenu)
        {
            if (redoMenu != null) 
                this.redoMenu = redoMenu;
        }
        public void ResetHistory()
        {
            commandStack.Clear();
            playHead = -1;
            isEmpty = true;
            UpdateMenus();
        }
        public void UnstackLastCommand()
        {
            // This happens when the command is cancelled while being performed.
            // For example, cancellation of screen closing.
            if(commandStack.Count <= 0)
                return;
            commandStack.RemoveAt(commandStack.Count - 1);
            playHead = commandStack.Count - 1;
            isEmpty = (commandStack.Count < 1);
            UpdateMenus();    
        }
        public void BlockRedo()
        {
            if ((commandStack.Count - 1) >= playHead && playHead >= 0)
                commandStack.RemoveRange(playHead, commandStack.Count - playHead);
        }
        
        public void UpdateMenus()
        {
            // Since the menus have their very own Resource Manager in the Tag field, 
            // we don't need to have a resx file here with the localization of undo redo.
            // This function is public because it accessed by the main kernel when we update preferences.
            if (undoMenu == null || redoMenu == null)
                return;

            UpdateUndoMenu();
            UpdateRedoMenu();
        }
        
        private void UpdateUndoMenu()
        {
            ResourceManager rm = undoMenu.Tag as ResourceManager;
            if (playHead < 0)
            {
                undoMenu.Enabled = false;
                if (rm != null)
                    undoMenu.Text = rm.GetString("mnuUndo");
            }
            else
            {
                undoMenu.Enabled = true;
                if (rm != null)
                    undoMenu.Text = rm.GetString("mnuUndo") + " : " + commandStack[playHead].FriendlyName;
            }
        }

        private void UpdateRedoMenu()
        {
            ResourceManager rm = redoMenu.Tag as ResourceManager;
            if (playHead == (commandStack.Count - 1))
            {
                redoMenu.Enabled = false;
                if (rm != null)
                    redoMenu.Text = rm.GetString("mnuRedo");
            }
            else
            {
                redoMenu.Enabled = true;
                if (rm != null)
                    redoMenu.Text = rm.GetString("mnuRedo") + " : " + commandStack[playHead + 1].FriendlyName;
            }
        }
        #endregion
    }
}
