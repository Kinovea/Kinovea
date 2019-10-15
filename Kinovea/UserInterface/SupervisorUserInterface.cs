/*
Copyright © Joan Charmant 2008.
jcharmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using System;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.Root
{
    public partial class SupervisorUserInterface : UserControl
    {
        #region Properties
        public bool IsExplorerCollapsed
        {
            get { return explorerCollapsed; }
        }
        #endregion

        #region Members
        private int oldSplitterDistance;
        private bool explorerCollapsed;
        private RootKernel rootKernel;
        private bool isOpening;
        private bool initialized;
        #endregion

        #region Construction Destruction
        public SupervisorUserInterface(RootKernel _RootKernel)
        {
            rootKernel = _RootKernel;
            InitializeComponent();
            initialized = false;

            // Get Explorer values from settings.
            oldSplitterDistance = PreferencesManager.GeneralPreferences.ExplorerSplitterDistance;
            
            NotificationCenter.LaunchOpenDialog += NotificationCenter_LaunchOpenDialog;
        }
        private void SupervisorUserInterface_Load(object sender, EventArgs e)
        {
            if (!LaunchSettingsManager.ShowExplorer || !PreferencesManager.GeneralPreferences.ExplorerVisible)
                CollapseExplorer();
            else
                ExpandExplorer(true);

            initialized = true;
        }
        #endregion

        public void PlugUI(UserControl fileExplorer, UserControl screenManager)
        {
            SuspendLayout();

            splitWorkSpace.Panel1.Controls.Add(fileExplorer);
            splitWorkSpace.Panel2.Controls.Add(screenManager);
            
            int topMargin = 2;
            fileExplorer.Top = topMargin;
            fileExplorer.Left = 0;
            fileExplorer.Width = splitWorkSpace.Panel1.Width;
            fileExplorer.Height = splitWorkSpace.Panel1.Height - topMargin;
            fileExplorer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            ResumeLayout();
        }

        #region Event Handlers
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // Finished moving the splitter.
            
            splitWorkSpace.Panel1.Refresh();

            if (!initialized)
                return;
            
            PreferencesManager.GeneralPreferences.ExplorerSplitterDistance = splitWorkSpace.SplitterDistance;
            PreferencesManager.GeneralPreferences.ExplorerVisible = true;
            PreferencesManager.Save();
        }
        private void NotificationCenter_LaunchOpenDialog(object sender, EventArgs e)
        {
            if(isOpening || rootKernel.ScreenManager.ScreenCount != 0)
                return;
            
            isOpening = true;

            string filepath = rootKernel.LaunchOpenFileDialog();
            if (filepath.Length > 0)
                VideoTypeManager.LoadVideo(filepath, -1);
                
            isOpening = false;
        }
        private void buttonCloseExplo_Click(object sender, EventArgs e)
        {
            CollapseExplorer();
        }
        private void _splitWorkSpace_DoubleClick(object sender, EventArgs e)
        {
            if (explorerCollapsed)
                ExpandExplorer(true);
            else
                CollapseExplorer();
        }
        private void splitWorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            if (explorerCollapsed && splitWorkSpace.SplitterDistance > 30)
                ExpandExplorer(false);
        }
        private void splitWorkSpace_Panel1_Click(object sender, EventArgs e)
        {
            if (explorerCollapsed)
                ExpandExplorer(true);
        }
        #endregion

        #region Lower level methods
        public void CollapseExplorer()
        {
            splitWorkSpace.Panel2.SuspendLayout();
            splitWorkSpace.Panel1.SuspendLayout();

            oldSplitterDistance = initialized ? splitWorkSpace.SplitterDistance : PreferencesManager.GeneralPreferences.ExplorerSplitterDistance;
            
            explorerCollapsed = true;
            foreach (Control ctrl in splitWorkSpace.Panel1.Controls)
            {
                ctrl.Visible = false;
            }
            
            splitWorkSpace.SplitterDistance = 4;
            splitWorkSpace.SplitterWidth = 1;
            splitWorkSpace.BorderStyle = BorderStyle.None;
            rootKernel.mnuToggleFileExplorer.Checked = false;

            splitWorkSpace.Panel1.ResumeLayout();
            splitWorkSpace.Panel2.ResumeLayout();

            PreferencesManager.GeneralPreferences.ExplorerSplitterDistance = oldSplitterDistance;
            PreferencesManager.GeneralPreferences.ExplorerVisible = false;
            PreferencesManager.Save();
        }
        public void ExpandExplorer(bool resetSplitter)
        {
            if (oldSplitterDistance == -1)
                return;
            
            splitWorkSpace.Panel2.SuspendLayout();
            splitWorkSpace.Panel1.SuspendLayout();

            explorerCollapsed = false;
            foreach (Control ctrl in splitWorkSpace.Panel1.Controls)
            {
                ctrl.Visible = true;
            }

            if (resetSplitter) 
                splitWorkSpace.SplitterDistance = oldSplitterDistance;

            splitWorkSpace.SplitterWidth = 4;
            splitWorkSpace.BorderStyle = BorderStyle.FixedSingle;
            rootKernel.mnuToggleFileExplorer.Checked = true;

            splitWorkSpace.Panel1.ResumeLayout();
            splitWorkSpace.Panel2.ResumeLayout();

            PreferencesManager.GeneralPreferences.ExplorerSplitterDistance = splitWorkSpace.SplitterDistance;
            PreferencesManager.GeneralPreferences.ExplorerVisible = true;
            PreferencesManager.Save();
        }
        #endregion

    }
}
