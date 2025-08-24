using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager;
using Kinovea.Root.Languages;
using Kinovea.Services;

namespace Kinovea.Root
{
    /// <summary>
    /// Form to edit the properties of the active window.
    /// </summary>
    public partial class FormWindowProperties : Form
    {

        private string name;
        private WindowStartupMode startupMode;
        private List<IScreenDescription> screenList;

        public FormWindowProperties()
        {
            // Make a local copy of the descriptor.
            WindowDescriptor descriptor = WindowManager.ActiveWindow;
            name = descriptor.Name;
            startupMode = descriptor.StartupMode;
            screenList = new List<IScreenDescription>();
            foreach (var screen in descriptor.ScreenList)
                screenList.Add(screen.Clone());

            InitializeComponent();
            this.Text = "Active window properties";

            Populate();
        }

        private void Populate()
        {
            this.tbName.Text = name;
            this.rbOpenExplorer.Checked = (startupMode == WindowStartupMode.FileExplorer);
            this.rbContinue.Checked = (startupMode == WindowStartupMode.Continue);
            this.rbScreenList.Checked = (startupMode == WindowStartupMode.ScreenList);

            // TODO: populate screen list.
        }

        #region Event handlers
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            name = tbName.Text.Trim();
        }

        private void rbOpenExplorer_CheckedChanged(object sender, EventArgs e)
        {
            if (rbOpenExplorer.Checked)
                startupMode = WindowStartupMode.FileExplorer;
            else if (rbScreenList.Checked)
                startupMode = WindowStartupMode.ScreenList;
            else
                startupMode = WindowStartupMode.Continue;
        }
        #endregion

        #region OK/Cancel/Close
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Commit the local copy to the active window descriptor.
            WindowDescriptor descriptor = WindowManager.ActiveWindow;
            descriptor.Name = name;
            descriptor.StartupMode = startupMode;
            descriptor.ScreenList.Clear();
            foreach (var screen in screenList)
                descriptor.ScreenList.Add(screen.Clone());

            WindowManager.SetTitleName();
        }
        #endregion

    }
}
