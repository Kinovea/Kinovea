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
    /// <summary>
    /// This control contains the main splitter between the explorer panel and the general screen panel.
    /// </summary>
    public partial class SupervisorUserInterface : UserControl
    {
        #region Members
        private RootKernel rootKernel;
        private bool isOpening;
        private bool initializing = true;
        #endregion

        #region Construction Destruction
        public SupervisorUserInterface(RootKernel _RootKernel)
        {
            rootKernel = _RootKernel;
            InitializeComponent();
            
            splitWorkSpace.SplitterDistance = (int)(splitWorkSpace.Width * WindowManager.ActiveWindow.ExplorerSplitterRatio);
            splitWorkSpace.SplitterMoved += SplitWorkSpace_SplitterMoved;

            NotificationCenter.LaunchOpenDialog += NotificationCenter_LaunchOpenDialog;
            NotificationCenter.ToggleShowExplorerPanel += NotificationCenter_ToggleShowExplorerPanel;
        }
        #endregion

        #region Public methods
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
            initializing = false;
        }

        /// <summary>
        /// Force collapse or uncollapse the explorer panel.
        /// This is used during initialization, when the user is manually toggling or when going into/out of full screen mode.
        /// savePreferences should only be true if this action is originating from the user.
        /// </summary>
        public void ShowHideExplorerPanel(bool show, bool savePreferences)
        {
            splitWorkSpace.Panel1Collapsed = !show;

            if (savePreferences)
            {
                WindowManager.ActiveWindow.ExplorerVisible = show;
            }
        }
        #endregion

        private void SupervisorUserInterface_Load(object sender, EventArgs e)
        {
            bool show = LaunchSettingsManager.ExplorerVisible && WindowManager.ActiveWindow.ExplorerVisible;
            ShowHideExplorerPanel(show, false);
        }

        private void NotificationCenter_LaunchOpenDialog(object sender, EventArgs e)
        {
            if(isOpening || rootKernel.ScreenManager.ScreenCount != 0)
                return;
            
            isOpening = true;

            string title = ScreenManager.Languages.ScreenManagerLang.mnuOpenVideo;
            string filter = ScreenManager.Languages.ScreenManagerLang.FileFilter_All + "|*.*";
            string filename = FilePicker.OpenVideo(title, filter);
            if (!string.IsNullOrEmpty(filename))
                VideoTypeManager.LoadVideo(filename, -1);
                
            isOpening = false;
        }

        private void NotificationCenter_ToggleShowExplorerPanel(object sender, EventArgs e)
        {
            bool show = splitWorkSpace.Panel1Collapsed;
            ShowHideExplorerPanel(show, true);
        }

        private void buttonCloseExplo_Click(object sender, EventArgs e)
        {
            ShowHideExplorerPanel(false, true);
        }

        private void SplitWorkSpace_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (initializing)
                return;
            
            WindowManager.ActiveWindow.ExplorerSplitterRatio = (float)splitWorkSpace.SplitterDistance / splitWorkSpace.Width;
        }
    }
}
