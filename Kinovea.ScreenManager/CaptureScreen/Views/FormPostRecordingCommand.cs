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
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace Kinovea.ScreenManager
{
    public partial class FormPostRecordingCommand : Form
    {
        #region Members
        private bool enablePRC;
        private UserCommand userCommand = null;
        private ScreenDescriptorCapture sdc;

        #region styles
        // Syntax highlighting styles.
        //private Style commentStyle = new TextStyle(new SolidBrush(Color.FromArgb(0, 128, 0)), null, FontStyle.Italic);
        //private Style commandStyle = new TextStyle(new SolidBrush(Color.FromArgb(0, 0, 255)), null, FontStyle.Bold);
        //private Style variableStyle = new TextStyle(new SolidBrush(Color.FromArgb(175, 0, 220)), null, FontStyle.Regular);
        
        //private Style commentStyle = new TextStyle(new SolidBrush(Color.FromArgb(165, 159, 160)), null, FontStyle.Italic);
        //private Style commandStyle = new TextStyle(new SolidBrush(Color.FromArgb(28, 140, 168)), null, FontStyle.Regular);
        //private Style variableStyle = new TextStyle(new SolidBrush(Color.FromArgb(204, 122, 10)), null, FontStyle.Regular);

        private Style commentStyle = new TextStyle(new SolidBrush(Color.FromArgb(165, 159, 160)), null, FontStyle.Italic);
        private Style commandStyle = new TextStyle(new SolidBrush(Color.FromArgb(225, 71, 117)), null, FontStyle.Bold);
        private Style variableStyle = new TextStyle(new SolidBrush(Color.FromArgb(28, 140, 168)), null, FontStyle.Regular);

        private Color backgroundColor = Color.FromArgb(254, 250, 249);
        private List<Style> styles;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public FormPostRecordingCommand(ScreenDescriptorCapture sdc)
        {
            this.sdc = sdc;
            this.enablePRC = sdc.EnableCommand;
            this.userCommand = sdc.UserCommand.Clone();

            styles = new List<Style>() { commentStyle, commandStyle, variableStyle };
            
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

            bool singleEmptyLine = userCommand.Instructions.Count == 1 && string.IsNullOrEmpty(userCommand.Instructions[0].Trim());

            if (userCommand.Instructions.Count > 0 && !singleEmptyLine)
            {
                fastColoredTextBox1.Text = string.Join("\n", userCommand.Instructions.ToArray());
            }
            else
            {
                // Add buit-in documentation.
                var text = new StringBuilder();
                text.AppendLine("# Add calls to external programs, one call per line.");
                text.AppendLine("# Lines starting with # are comments.");
                text.AppendLine("# Context variables like %filepath% will be replaced at run time.");
                fastColoredTextBox1.Text = text.ToString();
            }

            fastColoredTextBox1.WordWrap = true;
            fastColoredTextBox1.BackColor = backgroundColor;
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

        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Syntax highlighting.
            //clear previous highlighting
            e.ChangedRange.ClearStyle(styles.ToArray());
            
            // Comments. 
            // Start with # until the end of the line.
            e.ChangedRange.SetStyle(commentStyle, @"^\s*#.*$", RegexOptions.Multiline);

            // Command name.
            // First word of the line. Starts with a letter but may include a dot later (ex: python.exe).
            e.ChangedRange.SetStyle(commandStyle, @"^\s*\w[\w\.]+", RegexOptions.Multiline);

            // Variables.
            // Any word enclosed in %%.
            // That will include windows built-in variables like %appdata%.
            e.ChangedRange.SetStyle(variableStyle, @"%\w+%");


        }
    }
}
