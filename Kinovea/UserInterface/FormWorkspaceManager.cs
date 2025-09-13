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
using BrightIdeasSoftware;
using System.Runtime.InteropServices;
using System.IO;
using IWshRuntimeLibrary;

namespace Kinovea.Root
{
    /// <summary>
    /// Form create and delete workspaces.
    /// </summary>
    public partial class FormWorkspaceManager : Form
    {
        #region Private types
        private class ListViewWindowDescriptor
        {
            public WindowContent WindowContent { get; set; }
            public string Name { get; set; }
        }
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FormWorkspaceManager()
        {
            InitializeComponent();
            RefreshCulture();
            PrepareWorkspaceListView();
            PrepareWindowListView();

            PopulateWorkspaceList();
        }

        private void RefreshCulture()
        {
            this.Text = "Manage workspaces";
            btnClose.Text = "Close";
            grpWorkspaces.Text = "Workspace";
            grpWindowList.Text = "Windows";
            btnCreateShortcut.Text = "      " + "Create a desktop shortcut";

            toolTip1.SetToolTip(btnAdd, "Create a workspace from the active windows");
            toolTip1.SetToolTip(btnRename, "Rename the selected workspace");
            toolTip1.SetToolTip(btnCreateShortcut2, "Create a desktop shortcut");
            toolTip1.SetToolTip(btnDelete, "Delete the selected workspace");
        }

        /// <summary>
        /// Prepare the object list view control columns.
        /// </summary>
        private void PrepareWorkspaceListView()
        {
            // Column level options
            var colName = new OLVColumn();
            colName.AspectName = "Name";
            colName.Groupable = false;
            colName.Sortable = false;
            colName.IsEditable = false;
            colName.MinimumWidth = 100;
            colName.FillsFreeSpace = true;
            colName.FreeSpaceProportion = 2;
            colName.TextAlign = HorizontalAlignment.Left;
            colName.AspectGetter = delegate (object rowObject)
            {
                return WindowManager.GetFriendlyName((WorkspaceDescriptor)rowObject);
            };

            olvWorkspaces.AllColumns.AddRange(new OLVColumn[] {
                colName,
                });

            olvWorkspaces.Columns.AddRange(new ColumnHeader[] {
                colName,
                });

            // List view level options
            olvWorkspaces.HeaderStyle = ColumnHeaderStyle.None;
            olvWorkspaces.RowHeight = 22;
            olvWorkspaces.FullRowSelect = true;
        }

        private void PrepareWindowListView()
        {
            // Column level options

            var colLayout = new OLVColumn();
            colLayout.AspectName = "WindowContent";
            colLayout.Groupable = false;
            colLayout.Sortable = false;
            colLayout.IsEditable = false;
            colLayout.MinimumWidth = 40;
            colLayout.MaximumWidth = 40;
            colLayout.TextAlign = HorizontalAlignment.Center;
            colLayout.AspectGetter = delegate (object rowObject)
            {
                return ((ListViewWindowDescriptor)rowObject).WindowContent;
            };

            colLayout.AspectToStringConverter = delegate (object rowObject)
            {
                return string.Empty;
            };

            colLayout.ImageGetter = delegate (object rowObject)
            {
                ListViewWindowDescriptor lvwd = (ListViewWindowDescriptor)rowObject;
                switch (lvwd.WindowContent)
                {
                    case WindowContent.Playback:
                        return "playback";
                    case WindowContent.Capture:
                        return "capture";
                    case WindowContent.DualPlayback:
                        return "dualplayback";
                    case WindowContent.DualCapture:
                        return "dualcapture";
                    case WindowContent.DualMixed:
                        return "dualmixed";
                    case WindowContent.Browser:
                    default:
                        return "explorer";
                }
            };

            var colName = new OLVColumn();
            colName.AspectName = "Name";
            colName.Groupable = false;
            colName.Sortable = false;
            colName.IsEditable = false;
            colName.MinimumWidth = 100;
            colName.FillsFreeSpace = true;
            colName.FreeSpaceProportion = 2;
            colName.TextAlign = HorizontalAlignment.Left;

            olvWindows.AllColumns.AddRange(new OLVColumn[] {
                colLayout,
                colName,
                });

            olvWindows.Columns.AddRange(new ColumnHeader[] {
                colLayout,
                colName,
                });

            // List view level options
            olvWindows.HeaderStyle = ColumnHeaderStyle.None;
            olvWindows.RowHeight = 22;
            olvWindows.FullRowSelect = true;
        }

        /// <summary>
        /// Populate the entire workspace list from scratch.
        /// This function starts by re-reading the descriptors from the file system.
        /// </summary>
        private void PopulateWorkspaceList()
        {
            // This function is called not only on initialization but also 
            // after changes to the list so it should always restart from scratch.
            olvWorkspaces.Items.Clear();

            // Always get the up to date descriptors upon entering this window.
            WindowManager.ReadAllDescriptors();
            List<WorkspaceDescriptor> descriptors = WindowManager.WorkspaceDescriptors;
            olvWorkspaces.SetObjects(descriptors);

            // Start with nothing selected.
            olvWindows.Items.Clear();
            AfterSelection();
        }

        /// <summary>
        /// Populate the window list for the selected workspace.
        /// </summary>
        private void PopulateWindowList(WorkspaceDescriptor d)
        {
            olvWindows.Items.Clear();

            bool hasData = d != null;
            
            if (!hasData)
                return;

            List<WindowDescriptor> allWindowDescriptors = WindowManager.WindowDescriptors;
            List<ListViewWindowDescriptor> rows = new List<ListViewWindowDescriptor>();

            // Keep the order as stored in the workspace.
            foreach (var id in d.WindowList)
            {
                WindowDescriptor wd = allWindowDescriptors.FirstOrDefault(desc => desc.Id == id);
                if (wd == null)
                {
                    // That's pretty bad.
                    continue;
                }

                ListViewWindowDescriptor lvwd = new ListViewWindowDescriptor();
                lvwd.Name = WindowManager.GetFriendlyName(wd);
                lvwd.WindowContent = WindowManager.GetWindowContent(wd);
                rows.Add(lvwd);
            }

            olvWindows.SetObjects(rows);
        }

        /// <summary>
        /// Select the workspace in the object list view.
        /// </summary>
        private void SelectDescriptor(WorkspaceDescriptor descriptor)
        {
            for (int i = 0; i < olvWorkspaces.Items.Count; i++)
            {
                var d = olvWorkspaces.GetItem(i).RowObject as WorkspaceDescriptor;
                if (d != null && d.Id == descriptor.Id)
                {
                    olvWorkspaces.SelectObject(d);
                    break;
                }
            }
        }

        private void LaunchNameDialog()
        {
            WorkspaceDescriptor selectedWorkspace = olvWorkspaces.SelectedObject as WorkspaceDescriptor;
            if (selectedWorkspace == null)
                return;

            // Lauch the name dialog.
            FormWorkspaceName fwn = new FormWorkspaceName(selectedWorkspace);
            fwn.StartPosition = FormStartPosition.CenterScreen;
            fwn.ShowDialog();

            if (fwn.DialogResult == DialogResult.OK)
            {
                selectedWorkspace.Name = fwn.WorkspaceName;
                WindowManager.SaveWorkspace(selectedWorkspace);
            }

            fwn.Dispose();
            PopulateWorkspaceList();
            SelectDescriptor(selectedWorkspace);
        }

        private void CreateShortcut()
        {
            WorkspaceDescriptor selectedWorkspace = olvWorkspaces.SelectedObject as WorkspaceDescriptor;
            if (selectedWorkspace == null)
                return;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            //Environment.GetFolderPath(Environment.SpecialFolder.Desktop)

            string shortcutFileName = string.Format("Kinovea - {0}.lnk", WindowManager.GetFriendlyName(selectedWorkspace));
            string link = Path.Combine(desktopPath, shortcutFileName);

            string targetPath = string.Format("{0}", Application.ExecutablePath);
            string arguments = string.Format("-id {0}", selectedWorkspace.Id.ToString());
            string workingDirectory = Application.StartupPath;
            string iconLocation = string.Format("{0}", Application.ExecutablePath);

            try
            {
                var shell = new WshShell();
                var shortcut = shell.CreateShortcut(link) as IWshShortcut;
                shortcut.TargetPath = targetPath;
                shortcut.Arguments = arguments;
                shortcut.WorkingDirectory = workingDirectory;
                shortcut.IconLocation = iconLocation;
                shortcut.Save();

                // Show altert box to confirm.
                MessageBox.Show(
                    this, 
                    string.Format("Shortcut created at:\n{0}", link), 
                    "Shortcut created",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while saving shortcut to workspace. {0}.", ex.Message);
            }
        }

        private void AfterSelection()
        {
            WorkspaceDescriptor selectedWorkspace = olvWorkspaces.SelectedObject as WorkspaceDescriptor;
            bool hasSelection = selectedWorkspace != null;

            btnRename.Enabled = hasSelection;
            btnCreateShortcut.Enabled = hasSelection;
            btnCreateShortcut2.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;

            if (!hasSelection)
                return;

            PopulateWindowList(selectedWorkspace);
        }

        #region Event handlers
        private void olvWorkspaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            AfterSelection();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Immediately create and save a workspace with the active windows,
            // then select it and launch the Name dialog.
            WorkspaceDescriptor workspaceDescriptor = new WorkspaceDescriptor();
            List<WindowDescriptor> activeWindows = WindowManager.GetActiveWindows();
            var ids = activeWindows.Select(a => a.Id).ToList();
            workspaceDescriptor.ReplaceWindows(ids);

            // If we are making a single-window workspace give it the window name.
            bool singleWindow = ids.Count == 1;
            if (singleWindow)
            {
                workspaceDescriptor.Name = WindowManager.GetFriendlyName(WindowManager.ActiveWindow);
            }

            WindowManager.SaveWorkspace(workspaceDescriptor);
            PopulateWorkspaceList();
            SelectDescriptor(workspaceDescriptor);
            
            if (!singleWindow)
            {
                LaunchNameDialog();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            WorkspaceDescriptor selectedWorkspace = olvWorkspaces.SelectedObject as WorkspaceDescriptor;
            if (selectedWorkspace == null)
                return;

            int memoSelectedIndex = olvWorkspaces.SelectedIndex;
            
            WindowManager.DeleteWorkspace(selectedWorkspace);
            PopulateWorkspaceList();

            if (memoSelectedIndex < olvWorkspaces.Items.Count)
                olvWorkspaces.SelectedIndex = memoSelectedIndex;
            else if (olvWorkspaces.Items.Count > 0)
                olvWorkspaces.SelectedIndex = olvWorkspaces.Items.Count - 1;
        }

        private void olvWorkspaces_DoubleClick(object sender, EventArgs e)
        {
            LaunchNameDialog();
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            LaunchNameDialog();
        }

        private void btnShortcut_Click(object sender, EventArgs e)
        {
            CreateShortcut();
        }
        #endregion

     
    }
}
