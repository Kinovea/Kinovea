using Kinovea.Root.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.Root
{
    public class CommandToggleFileExplorer : IUndoableCommand
    {
        public string FriendlyName
        {
        	get { return RootLang.CommandToggleFileExplorer_FriendlyName; }
        }

        #region Members
        private SplitContainer Splitter;
        private ToolStripMenuItem MenuItem;
        #endregion

        #region constructor
        public CommandToggleFileExplorer( SplitContainer splitter, ToolStripMenuItem menuItem)
        {
            Splitter = splitter;
            MenuItem = menuItem;
        }
        #endregion

        public void Execute()
        {
            if (Splitter.Panel1Collapsed)
            {
                Splitter.Panel1Collapsed = false;
                MenuItem.Checked = true;
            }
            else
            {
                Splitter.Panel1Collapsed = true;
                MenuItem.Checked = false;
            }
        }

        public void Unexecute()
        {
            //Annuler correspond à refaire un Toggle.
            Execute();
        }



        
    }

}