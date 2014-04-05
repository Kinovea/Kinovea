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
        private SplitContainer splitter;
        private ToolStripMenuItem menuItem;
        #endregion

        #region constructor
        public CommandToggleFileExplorer( SplitContainer splitter, ToolStripMenuItem menuItem)
        {
            this.splitter = splitter;
            this.menuItem = menuItem;
        }
        #endregion

        public void Execute()
        {
            splitter.Panel1Collapsed = !splitter.Panel1Collapsed;
            menuItem.Checked = !splitter.Panel1Collapsed;
        }

        public void Unexecute()
        {
            Execute();
        }        
    }

}