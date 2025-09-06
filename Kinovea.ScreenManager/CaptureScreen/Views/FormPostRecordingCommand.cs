using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kinovea.ScreenManager.Languages;
using System.Drawing.Drawing2D;
using Kinovea.Services;
using BrightIdeasSoftware;
using Kinovea.Services.Types;
using System.Globalization;

namespace Kinovea.ScreenManager
{
    public partial class FormPostRecordingCommand : Form
    {
        #region Members
        private bool enablePRC;
        private UserCommand userCommand = null;
        private ScreenDescriptorCapture sdc;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FormPostRecordingCommand(ScreenDescriptorCapture sdc)
        {
            this.sdc = sdc;
            this.enablePRC = sdc.EnableCommand;
            this.userCommand = sdc.UserCommand.Clone();

            InitializeComponent();
            LocalizeForm();
            Populate();
        }

        private void LocalizeForm()
        {
            this.Text = "Post-recording command";
            cbEnable.Text = "Disable post-recording command";
            btnInsertVariable.Text = "Insert a variable…";
            btnSaveAndContinue.Text = "Save and continue";
            btnOK.Text = ScreenManagerLang.Generic_Save;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }

        private void Populate()
        {
            cbEnable.Checked = !this.enablePRC;
            if (userCommand.Instructions.Count > 0)
            {
                fastColoredTextBox1.Text = string.Join("\n", userCommand.Instructions.ToArray());
            }
            else
            {
                fastColoredTextBox1.Text = "";
            }

            fastColoredTextBox1.WordWrap = true;
        }

        private void cbEnable_CheckedChanged(object sender, EventArgs e)
        {
            enablePRC = !cbEnable.Checked;
            fastColoredTextBox1.Enabled = enablePRC;
        }

        private void btnSaveAndContinue_Click(object sender, EventArgs e)
        {
            Commit();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Commit();
        }

        private void UpdateUserCommand()
        {
            userCommand.Instructions.Clear();
            foreach (var l in fastColoredTextBox1.Lines)
            {
                userCommand.Instructions.Add(l);
            }
        }

        private void Commit()
        {
            UpdateUserCommand();

            // Commit to the screen state.
            sdc.EnableCommand = enablePRC;
            sdc.UserCommand = userCommand.Clone();

            // Force save to the active window.
            // But we should only save the command, not the rest of the screen,
            // if the window is in "specific screens". The user may have changed camera
            // for example, the screen should still start on the saved one.

            // Find the screen in the window.
            var screen = WindowManager.ActiveWindow.ScreenList.FirstOrDefault(s => s.Id == sdc.Id);
            if (screen == null)
            {
                // Possible if the user started on a window without a capture screen but in "specific" startup mode.
                // What should we do in this case? We are going to lose the info.
                // TODO: at least store it in the backup section.
                return;
            }

            var windowScreen = screen as ScreenDescriptorCapture;
            windowScreen.EnableCommand = sdc.EnableCommand;
            windowScreen.UserCommand = sdc.UserCommand.Clone();
            WindowManager.SaveActiveWindow();
        }
    }
}
