using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
using Videa.Services;


namespace Videa.Root
{
    public class CommandToggleFileExplorer : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.Supervisor.Languages.SupervisorLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandToggleFileExplorer_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
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