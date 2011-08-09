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

[assembly: CLSCompliant(true)]
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
        private List<IUndoableCommand> m_CommandStack;
        private int m_iPlayHead;
        private Boolean bIsEmpty;
        private ToolStripMenuItem undoMenu;
        private ToolStripMenuItem redoMenu;
        private static CommandManager _instance = null;
        #endregion

        #region Instance et Ctor
        // Récup de l'instance du singleton.
        public static CommandManager Instance()
        {
            if (_instance == null)
            {
                _instance = new CommandManager();
            }
            return _instance;
        }

        //Constructeur privé.
        private CommandManager()
        {
            m_CommandStack = new List<IUndoableCommand>();
            m_iPlayHead = -1;
            bIsEmpty = true;
        }
        #endregion

        #region Implementation
        public static void LaunchCommand(ICommand command)
        {
            if (command != null) { command.Execute(); }
        }
        public void LaunchUndoableCommand(IUndoableCommand command)
        {
            if (command != null)
            {
                // Dépiler ce qui est au dessus de la tête de lecture
                if ((m_CommandStack.Count - 1) > m_iPlayHead)
                {
                    m_CommandStack.RemoveRange(m_iPlayHead + 1, (m_CommandStack.Count - 1) - m_iPlayHead);
                }

                //Empiler la commande
                m_CommandStack.Add(command);
                m_iPlayHead = m_CommandStack.Count - 1;
                bIsEmpty = false;
                
                //Executer la commande
                DoCurrentCommand();

                // Mise à jour du menu
                UpdateMenus();
            }
        }
        private void DoCurrentCommand()
        {
            if (!bIsEmpty)
            {
                m_CommandStack[m_iPlayHead].Execute();
            }
        }
        public void Undo()
        {
            if (!bIsEmpty)
            {
                //Unexecuter la commande courante.
                m_CommandStack[m_iPlayHead].Unexecute();
                
                //délpacer la tête de lecture vers le bas.
                m_iPlayHead--;
            }

            // Mettre les menus à jour.
            UpdateMenus();
        }
        public void Redo()
        {
            if ((m_CommandStack.Count - 1) > m_iPlayHead)
            {
                m_iPlayHead++;
                DoCurrentCommand();
                UpdateMenus();
            }
        }
        public void RegisterUndoMenu(ToolStripMenuItem undoMenu)
        {
            if (undoMenu != null) { this.undoMenu = undoMenu; }
        }
        public void RegisterRedoMenu(ToolStripMenuItem redoMenu)
        {
            if (redoMenu != null) { this.redoMenu = redoMenu; }
        }
        public void ResetHistory()
        {
            m_CommandStack.Clear();
            m_iPlayHead = -1;
            bIsEmpty = true;
            UpdateMenus();
        }
        public void UnstackLastCommand()
        {
            // This happens when the command is cancelled while being performed.
            // For example, cancellation of screen closing.
            if(m_CommandStack.Count > 0)
            {
                m_CommandStack.RemoveAt(m_CommandStack.Count - 1);
                m_iPlayHead = m_CommandStack.Count - 1;
                bIsEmpty = (m_CommandStack.Count < 1);
                UpdateMenus();    
            }
        }
        public void BlockRedo()
        {
            // Dépiler ce qui est au dessus de la tête de lecture
            // BlockRedo est appelé pendant le unexecute, donc la playhead n'a pas encore été déplacée.
            if ((m_CommandStack.Count - 1) >= m_iPlayHead && m_iPlayHead >= 0)
            {
                m_CommandStack.RemoveRange(m_iPlayHead, m_CommandStack.Count - m_iPlayHead);
            }
        }
        public void UpdateMenus()
        {
            // Since the menus have their very own Resource Manager in the Tag field, 
            // we don't need to have a resx file here with the localization of undo redo.
            // This function is public because it accessed by the main kernel when we update preferences.
            if (undoMenu != null)
            {
                ResourceManager rm = undoMenu.Tag as ResourceManager;
                if (m_iPlayHead < 0)
                {
                    undoMenu.Enabled = false;
                    if(rm != null) undoMenu.Text = rm.GetString("mnuUndo");
                }
                else
                {
                    undoMenu.Enabled = true;
                    if(rm != null) undoMenu.Text = rm.GetString("mnuUndo")  + " : " + m_CommandStack[m_iPlayHead].FriendlyName;
                }
            }

            if (redoMenu != null)
            {
                ResourceManager rm = undoMenu.Tag as ResourceManager;
                if (m_iPlayHead == (m_CommandStack.Count - 1))
                {
                    redoMenu.Enabled = false;
                    if(rm != null) redoMenu.Text = rm.GetString("mnuRedo");
                }
                else
                {
                    redoMenu.Enabled = true;
                    if(rm != null) redoMenu.Text = rm.GetString("mnuRedo") + " : " + m_CommandStack[m_iPlayHead + 1].FriendlyName;
                }
            }
        }
        #endregion
    }
}
