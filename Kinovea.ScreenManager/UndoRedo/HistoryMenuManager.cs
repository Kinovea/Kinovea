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

        private static void menuRedo_Click(object sender, EventArgs e)
        {
            if (stack == null)
                return;

            stack.StepForward();
            UpdateMenus();
        }

        private static void menuUndo_Click(object sender, EventArgs e)
        {
            if (stack == null)
                return;
            
            stack.StepBackward();
            UpdateMenus();
        }

        public static void SwitchContext(HistoryStack _stack)
        {
            if (stack != null)
                stack.HistoryChanged -= HistoryChanged;

            stack = _stack;
            stack.HistoryChanged += HistoryChanged;

            UpdateMenus();
        }
        
        public static void UpdateMenus()
        {
            ResourceManager rm = menuUndo.Tag as ResourceManager;

            menuUndo.Text = rm.GetString("mnuUndo");
            menuUndo.Enabled = stack != null && stack.CanUndo;
            if (menuUndo.Enabled)
              menuUndo.Text += " : " + stack.UndoActionName;

            menuRedo.Text = rm.GetString("mnuRedo");
            menuRedo.Enabled = stack != null && stack.CanRedo;
            if (menuRedo.Enabled)
              menuRedo.Text += " : " + stack.RedoActionName;
        }
        
        private static void HistoryChanged(object sender, EventArgs e)
        {
            UpdateMenus();
        }
    }
}
