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
        private bool manualUpdate;
        private WindowStartupMode startupMode;
        private List<IScreenDescriptor> screenList;
        private RootKernel rootKernel;

        public FormWindowProperties(RootKernel rootKernel)
        {
            this.rootKernel = rootKernel;

            // Make a local copy of the descriptor.
            WindowDescriptor descriptor = WindowManager.ActiveWindow;
            name = descriptor.Name;
            startupMode = descriptor.StartupMode;
            screenList = new List<IScreenDescriptor>();
            foreach (var screen in descriptor.ScreenList)
                screenList.Add(screen.Clone());

            InitializeComponent();
            this.Text = "Active window properties";

            Populate();
        }

        private void Populate()
        {
            manualUpdate = true;

            tbName.Text = name;
            rbOpenExplorer.Checked = (startupMode == WindowStartupMode.Explorer);
            rbContinue.Checked = (startupMode == WindowStartupMode.Continue);
            rbScreenList.Checked = (startupMode == WindowStartupMode.ScreenList);

            grpScreenList.Enabled = rbScreenList.Checked;

            manualUpdate = false;

            PopulateScreenList();
        }

        private void PopulateScreenList()
        {
            if (screenList.Count == 0)
            {
                // Single line for the explorer.
                btnScreen2.Visible = false;
                lblScreen2.Visible = false;
                PopulateScreen(null, btnScreen1, lblScreen1);
            }
            else if (screenList.Count == 1)
            {
                btnScreen2.Visible = false;
                lblScreen2.Visible = false;
                PopulateScreen(screenList[0], btnScreen1, lblScreen1);
            }
            else
            {
                // Dual screen.
                btnScreen2.Visible = true;
                lblScreen2.Visible = true;
                PopulateScreen(screenList[0], btnScreen1, lblScreen1);
                PopulateScreen(screenList[1], btnScreen2, lblScreen2);
            }
        }

        private void PopulateScreen(IScreenDescriptor screen, Button btn, Label lbl)
        {
            if (screen == null)
            {
                btn.Image = Properties.Resources.home3;
                lbl.Text = "Explorer";
            }
            else if (screen.ScreenType == ScreenType.Playback)
            {
                if (((ScreenDescriptorPlayback)screen).IsReplayWatcher)
                {
                    btn.Image = Properties.Resources.user_detective;
                    lbl.Text = string.Format("Replay: {0}", screen.FriendlyName);
                }
                else
                {
                    btn.Image = Properties.Resources.television;
                    lbl.Text = string.Format("Playback: {0}", screen.FriendlyName);
                }
            }
            else if (screen.ScreenType == ScreenType.Capture)
            {
                btn.Image = Properties.Resources.camera_video;
                lbl.Text = string.Format("Capture: {0}", screen.FriendlyName);
            }
        }

        #region Event handlers
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            name = tbName.Text.Trim();
        }

        private void rbStartupMode_CheckedChanged(object sender, EventArgs e)
        {
            if (manualUpdate)
                return;

            if (rbOpenExplorer.Checked)
                startupMode = WindowStartupMode.Explorer;
            else if (rbScreenList.Checked)
                startupMode = WindowStartupMode.ScreenList;
            else
                startupMode = WindowStartupMode.Continue;

            grpScreenList.Enabled = rbScreenList.Checked;
        }
        private void btnUseCurrent_Click(object sender, EventArgs e)
        {
            // Import the active screen list.
            var descriptors = rootKernel.ScreenManager.GetScreenDescriptors();
            screenList.Clear();
            foreach (var d in descriptors)
                screenList.Add(d.Clone());

            PopulateScreenList();
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
