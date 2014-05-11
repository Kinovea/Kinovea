using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;

namespace Kinovea.ScreenManager
{
    public static class HistoryMenuManager
    {
        private static ToolStripMenuItem menuUndo;
        private static ToolStripMenuItem menuRedo;
        private static HistoryStack stack;

        public static void RegisterMenus(ToolStripMenuItem _menuUndo, ToolStripMenuItem _menuRedo)
        {
            if (_menuUndo == null || _menuRedo == null)
                throw new ArgumentNullException();

            menuUndo = _menuUndo;
            menuRedo = _menuRedo;

            menuUndo.Click += menuUndo_Click;
            menuRedo.Click += menuRedo_Click;
        }

        static void menuRedo_Click(object sender, EventArgs e)
        {
            if (stack == null)
                return;

            stack.StepForward();
            UpdateMenus();
        }

        static void menuUndo_Click(object sender, EventArgs e)
        {
            if (stack == null)
                return;
            
            stack.StepBackward();
            UpdateMenus();
        }

        public static void SwitchContext(HistoryStack _stack)
        {
            stack = _stack;
            UpdateMenus();
        }
        
        public static void UpdateMenus()
        {
            ResourceManager rm = menuUndo.Tag as ResourceManager;

            menuUndo.Enabled = stack != null && stack.CanUndo;
            menuUndo.Text = rm.GetString("mnuUndo");

            //if (stack.CanUndo)
              //  menuUndo.Text += " : " + stack.UndoAction;

            menuRedo.Enabled = stack != null && stack.CanRedo;
            menuRedo.Text = rm.GetString("mnuRedo");

            //if (stack.CanRedo)
            //  menuRedo.Text += " : " + stack.RedoAction;
        }
    }
}
