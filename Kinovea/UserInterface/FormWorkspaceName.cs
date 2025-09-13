using System;
using System.Drawing;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class FormWorkspaceName : Form
    {
        public string WorkspaceName
        { 
            get { return tbAlias.Text; }
        }
        
        private WorkspaceDescriptor descriptor;
        public FormWorkspaceName(WorkspaceDescriptor descriptor)
        {
            this.descriptor = descriptor;
            InitializeComponent();
            Initialize();

            tbAlias.SelectAll();
            tbAlias.Select();
        }
        
        private void Initialize()
        {
            this.Text = "Workspace name";
            lblAlias.Text = "Name:";
            tbAlias.Text = WindowManager.GetFriendlyName(descriptor);
            btnOK.Text = ScreenManagerLang.Generic_Apply;
            btnCancel.Text = ScreenManagerLang.Generic_Cancel;
        }
        
        private void tbAlias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
