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
    /// Form to edit the properties of the active window.
    /// </summary>
    public partial class FormWindowManager : Form
    {
        private enum InstanceStatus
        {
            Myself,
            Running,
            Sleeping,
        }

        private class ListViewWindowDescriptor
        {
            public InstanceStatus InstanceStatus { get; set; }
            public string Name { get; set; }
        }

        private bool manualUpdate;
        private RootKernel rootKernel;

        public FormWindowManager(RootKernel rootKernel)
        {
            this.rootKernel = rootKernel;

            // Always get the up to date descriptors upon entering this window.


            InitializeComponent();
            this.Text = "Manage windows";

            Populate();
        }

        private void Populate()
        {
            manualUpdate = true;

            // Populate the list view.
            List<WindowDescriptor> descriptors = WindowManager.WindowDescriptors;

            // ObjectListView
            // https://objectlistview.sourceforge.net/cs/index.html
            // 23. How do I make a column that shows just an image?

            // Configure columns
            var colStatus = new OLVColumn();
            colStatus.AspectName = "IsRunning";
            colStatus.Groupable = false;
            colStatus.Sortable = false;
            colStatus.IsEditable = false;
            colStatus.MinimumWidth = 25;
            colStatus.MaximumWidth = 25;
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
                        return "myself";
                    case InstanceStatus.Running:
                        return "running";
                    case InstanceStatus.Sleeping:
                    default:
                        return "sleeping";
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
                colName,
                });

            olvWindows.Columns.AddRange(new ColumnHeader[] {
                colStatus,
                colName,
                });

            olvWindows.HeaderStyle = ColumnHeaderStyle.None;
            olvWindows.RowHeight = 22;
            olvWindows.FullRowSelect = true;
            
            List<ListViewWindowDescriptor> rows = new List<ListViewWindowDescriptor>();
            foreach (var descriptor in descriptors)
            {
                string name = descriptor.Name;
                if (string.IsNullOrEmpty(name))
                    name = WindowManager.GetIdName(descriptor);

                ListViewWindowDescriptor lvwd = new ListViewWindowDescriptor();
                lvwd.Name = name;

                if (descriptor.Id == WindowManager.ActiveWindow.Id)
                {
                    lvwd.InstanceStatus = InstanceStatus.Myself;
                }
                else
                {
                    bool isRunning = IsRunning(descriptor);
                    lvwd.InstanceStatus = isRunning ?  InstanceStatus.Running : InstanceStatus.Sleeping;
                }

                rows.Add(lvwd);
            }

            olvWindows.SetObjects(rows);
        }

        private bool IsRunning(WindowDescriptor d)
        {
            string titleName = string.IsNullOrEmpty(d.Name) ? WindowManager.GetIdName(d) : d.Name;
            string title = string.Format("Kinovea [{0}]", titleName);
            IntPtr handle = NativeMethods.FindWindow(null, title);
            return handle != IntPtr.Zero;
        }

        private void PopulateScreenList()
        {
            //if (screenList.Count == 0)
            //{
            //    // Single line for the explorer.
            //    btnScreen2.Visible = false;
            //    lblScreen2.Visible = false;
            //    PopulateScreen(null, btnScreen1, lblScreen1);
            //}
            //else if (screenList.Count == 1)
            //{
            //    btnScreen2.Visible = false;
            //    lblScreen2.Visible = false;
            //    PopulateScreen(screenList[0], btnScreen1, lblScreen1);
            //}
            //else
            //{
            //    // Dual screen.
            //    btnScreen2.Visible = true;
            //    lblScreen2.Visible = true;
            //    PopulateScreen(screenList[0], btnScreen1, lblScreen1);
            //    PopulateScreen(screenList[1], btnScreen2, lblScreen2);
            //}
        }

        private void PopulateScreen(IScreenDescriptor screen, Button btn, Label lbl)
        {
            //if (screen == null)
            //{
            //    btn.Image = Properties.Resources.home3;
            //    lbl.Text = "Explorer";
            //}
            //else if (screen.ScreenType == ScreenType.Playback)
            //{
            //    if (((ScreenDescriptionPlayback)screen).IsReplayWatcher)
            //    {
            //        btn.Image = Properties.Resources.user_detective;
            //        lbl.Text = string.Format("Replay: {0}", screen.FriendlyName);
            //    }
            //    else
            //    {
            //        btn.Image = Properties.Resources.television;
            //        lbl.Text = string.Format("Playback: {0}", screen.FriendlyName);
            //    }
            //}
            //else if (screen.ScreenType == ScreenType.Capture)
            //{
            //    btn.Image = Properties.Resources.camera_video;
            //    lbl.Text = string.Format("Capture: {0}", screen.FriendlyName);
            //}
        }

        #region Event handlers
        

        
        
        #endregion

        #region OK/Cancel/Close
        
        #endregion
    }
}
