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

namespace Kinovea.Root
{
    /// <summary>
    /// Form to start, delete and inspect the existing windows.
    /// </summary>
    public partial class FormWindowManager : Form
    {
        #region Private types
        private enum InstanceStatus
        {
            Myself,
            Running,
            Sleeping,
        }

        private class ListViewWindowDescriptor
        {
            public InstanceStatus InstanceStatus { get; set; }
            public WindowContent WindowContent { get; set; }
            public string Name { get; set; }

            public DateTime LastSave { get; set; }

            public WindowDescriptor Tag { get; set; }
        }
        #endregion

        private bool manualUpdate;
        private RootKernel rootKernel;
        private ListViewWindowDescriptor selected;

        public FormWindowManager(RootKernel rootKernel)
        {
            this.rootKernel = rootKernel;

            InitializeComponent();
            RefreshCulture();
            PrepareListView();
            Populate();
        }

        private void RefreshCulture()
        {
            this.Text = Kinovea.Root.Languages.RootLang.dlgTitle_ManageWindows;
            btnClose.Text = Kinovea.ScreenManager.Languages.ScreenManagerLang.Generic_Close;

            toolTip1.SetToolTip(btnStartStop, Kinovea.Root.Languages.RootLang.formWindowManager_StopProcess);
            toolTip1.SetToolTip(btnDelete, Kinovea.Root.Languages.RootLang.formWindowManager_DeleteWindow);
            toolTip1.SetToolTip(btnRefresh, Kinovea.Root.Languages.RootLang.formWindowManager_RefreshWindowList);
        }

        /// <summary>
        /// Prepare the object list view control columns.
        /// </summary>
        private void PrepareListView()
        {
            // ObjectListView
            // https://objectlistview.sourceforge.net/cs/index.html
            // 23. How do I make a column that shows just an image?

            // Column level options
            var colStatus = new OLVColumn();
            colStatus.AspectName = "InstanceStatus";
            colStatus.Groupable = false;
            colStatus.Sortable = false;
            colStatus.IsEditable = false;
            colStatus.MinimumWidth = 40;
            colStatus.MaximumWidth = 40;
            colStatus.TextAlign = HorizontalAlignment.Center;
            colStatus.AspectGetter = delegate (object rowObject)
            {
                return ((ListViewWindowDescriptor)rowObject).InstanceStatus;
            };

            colStatus.AspectToStringConverter = delegate (object rowObject)
            {
                return string.Empty;
            };

            colStatus.ImageGetter = delegate (object rowObject)
            {
                ListViewWindowDescriptor lvwd = (ListViewWindowDescriptor)rowObject;
                switch (lvwd.InstanceStatus)
                {
                    case InstanceStatus.Myself:
                        return "running";
                    case InstanceStatus.Running:
                        return "running";
                    case InstanceStatus.Sleeping:
                    default:
                        return "sleeping";
                }
            };

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
                colStatus,
                colLayout,
                colName,
                });

            olvWindows.Columns.AddRange(new ColumnHeader[] {
                colStatus,
                colLayout,
                colName,
                });

            // List view level options
            olvWindows.HeaderStyle = ColumnHeaderStyle.None;
            olvWindows.RowHeight = 22;
            olvWindows.FullRowSelect = true;
        }

        /// <summary>
        /// Populate the entire window list from scratch.
        /// This function starts by re-reading the descriptors from the file system.
        /// </summary>
        private void Populate()
        {
            // This function is called not only on initialization but also 
            // after changes to the list so it should always restart from scratch.
            olvWindows.Items.Clear();
            selected = null;

            // Always get the up to date descriptors upon entering this window.
            WindowManager.ReadAllDescriptors();
            List<WindowDescriptor> descriptors = WindowManager.WindowDescriptors;

            // Populate the list view.
            List<ListViewWindowDescriptor> rows = new List<ListViewWindowDescriptor>();
            foreach (var descriptor in descriptors)
            {
                ListViewWindowDescriptor lvwd = new ListViewWindowDescriptor();
                lvwd.Name = WindowManager.GetFriendlyName(descriptor);
                lvwd.InstanceStatus = GetInstanceStatus(descriptor);
                lvwd.WindowContent = WindowManager.GetWindowContent(descriptor);
                lvwd.LastSave = descriptor.LastSave;
                lvwd.Tag = descriptor;
                rows.Add(lvwd);
            }

            // Sort strategy.
            // Primary sort by instance status (active < running < sleeping).
            // Secondary sort by last save date.
            rows.Sort((x, y) =>
            {
                int a = x.InstanceStatus.CompareTo(y.InstanceStatus);
                if (a != 0 )
                    return a;

                return y.LastSave.CompareTo(x.LastSave);
            });

            olvWindows.SetObjects(rows);

            // Select the first row if any.
            if (olvWindows.Items.Count > 0)
            {
                olvWindows.SelectedIndex = 0;
            }
            else
            {
                PopulateScreenList(null);
                UpdateButtons(null);
            }
        }

        /// <summary>
        /// Populate the screen list area for the selected instance.
        /// </summary>
        /// <param name="d"></param>
        private void PopulateScreenList(WindowDescriptor d)
        {
            bool hasData = d != null;
            grpScreenList.Text = hasData ? string.Format("[{0}]", WindowManager.GetFriendlyName(d)) : "";
            btnScreen1.Visible = hasData;
            lblScreen1.Visible = hasData;
            btnScreen2.Visible = hasData;
            lblScreen2.Visible = hasData;
            grpScreenList.Enabled = hasData;
            
            if (!hasData)
                return;
            
            // Note: this is the exact same logic as in FormWindowProperties,
            // Maybe turn the panel into a control.
            List<IScreenDescriptor> screenList = d.ScreenList;
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

        /// <summary>
        /// Populate one screen line inside the screen list area.
        /// </summary>
        private void PopulateScreen(IScreenDescriptor screen, Button btn, Label lbl)
        {
            if (screen == null)
            {
                btn.Image = Properties.Resources.file_browser;
                lbl.Text = "File browser";
            }
            else if (screen.ScreenType == ScreenType.Playback)
            {
                if (((ScreenDescriptorPlayback)screen).IsReplayWatcher)
                {
                    btn.Image = Properties.Resources.user_detective;
                    lbl.Text = string.Format(Kinovea.Root.Languages.RootLang.formWindowProperties_Replay0, screen.FriendlyName);
                }
                else
                {
                    btn.Image = Properties.Resources.television;
                    lbl.Text = string.Format(Kinovea.Root.Languages.RootLang.formWindowProperties_Playback0, screen.FriendlyName);
                }
            }
            else if (screen.ScreenType == ScreenType.Capture)
            {
                btn.Image = Properties.Resources.camera_video;
                lbl.Text = string.Format(Kinovea.Root.Languages.RootLang.formWindowProperties_Capture0, screen.FriendlyName);
            }
        }

        /// <summary>
        /// Update the side buttons based on the selected instance.
        /// </summary>
        private void UpdateButtons(ListViewWindowDescriptor lvwd)
        {
            bool hasData = lvwd != null;
            btnStartStop.Enabled = hasData;
            btnDelete.Enabled = hasData;
            if (!hasData)
                return;

            if (lvwd.InstanceStatus == InstanceStatus.Myself)
            {
                btnStartStop.Enabled = false;
                btnDelete.Enabled = false;
            }
            else if (lvwd.InstanceStatus == InstanceStatus.Running)
            {
                btnStartStop.Image = Properties.Resources.stop2_16;
                toolTip1.SetToolTip(btnStartStop, "Stop process");
                btnDelete.Enabled = false;
            }
            else
            {
                btnStartStop.Image = Properties.Resources.circled_play_green2_16;
                toolTip1.SetToolTip(btnStartStop, "Start process");
                btnDelete.Enabled = true;
            }
        }

        private InstanceStatus GetInstanceStatus(WindowDescriptor d)
        {
            InstanceStatus status = InstanceStatus.Sleeping;
            if (d.Id == WindowManager.ActiveWindow.Id)
            {
                status = InstanceStatus.Myself;
            }
            else
            {
                bool isRunning = IsRunning(d);
                status = isRunning ? InstanceStatus.Running : InstanceStatus.Sleeping;
            }

            return status;
        }

        /// <summary>
        /// Returns true if the instance is currently running.
        /// </summary>
        private bool IsRunning(WindowDescriptor d)
        {
            string titleName = WindowManager.GetFriendlyName(d);
            string title = string.Format("Kinovea [{0}]", titleName);
            IntPtr handle = NativeMethods.FindWindow(null, title);
            return handle != IntPtr.Zero;
        }

        private void olvWindows_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = olvWindows.SelectedIndex;
            var item = olvWindows.GetItem(i);
            if (item == null)
                return;

            ListViewWindowDescriptor lvwd = item.RowObject as ListViewWindowDescriptor;
            if (lvwd == null)
                return;

            selected = lvwd;

            PopulateScreenList(lvwd.Tag);
            UpdateButtons(lvwd);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (selected == null || selected.InstanceStatus == InstanceStatus.Myself)
                return;

            if (selected.InstanceStatus == InstanceStatus.Running)
            {
                // Stop running instance.
                WindowManager.StopInstance(selected.Tag);
                
                Populate();
            }
            else 
            {
                // Start sleeping instace.
                WindowManager.ReopenWindow(selected.Tag);

                // Wait a bit for the instance to start ?
                Populate();
            }

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            Populate();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int memoSelectedIndex = olvWindows.SelectedIndex;
            
            WindowManager.Delete(selected.Tag);
            Populate();

            if (memoSelectedIndex < olvWindows.Items.Count)
                olvWindows.SelectedIndex = memoSelectedIndex;
            else if (olvWindows.Items.Count > 0)
                olvWindows.SelectedIndex = olvWindows.Items.Count - 1;
        }
    }
}
