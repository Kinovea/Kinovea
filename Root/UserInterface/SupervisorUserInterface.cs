/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Kinovea.Services;
using Kinovea.FileBrowser;
using Kinovea.ScreenManager;

namespace Kinovea.Root
{
    public partial class SupervisorUserInterface : UserControl
    {
        #region Properties
        public bool IsExplorerCollapsed
        {
            get { return m_bExplorerCollapsed; }
        }
        #endregion

        #region Members
        private int m_iOldSplitterDistance;
        private bool m_bExplorerCollapsed;
        private RootKernel RootKernel;
        private bool isOpening;
        private PreferencesManager m_PrefManager;
        private bool m_bInitialized;
        #endregion

        #region Construction Destruction
        public SupervisorUserInterface(RootKernel _RootKernel)
        {
            RootKernel = _RootKernel;
            InitializeComponent();
            m_bInitialized = false;

            // Get Explorer values from settings.
            m_PrefManager = PreferencesManager.Instance();
            m_iOldSplitterDistance = m_PrefManager.ExplorerSplitterDistance;
            
            // Services offered here
            DelegatesPool dp = DelegatesPool.Instance();
            dp.OpenVideoFile = DoOpenVideoFile;
        }
        private void SupervisorUserInterface_Load(object sender, EventArgs e)
        {
            if (m_PrefManager.ExplorerVisible)
            {
                ExpandExplorer(true);
            }
            else
            {
                CollapseExplorer();
            }
            m_bInitialized = true;
        }
        #endregion

        #region Event Handlers
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // Finished moving the splitter.

            //Redessiner le fileExplorer pour être sûr qu'il remplisse tout.
            splitWorkSpace.Panel1.Refresh();

            if (m_bInitialized)
            {
                m_PrefManager.ExplorerSplitterDistance = splitWorkSpace.SplitterDistance;
                m_PrefManager.ExplorerVisible = true;
                m_PrefManager.Export();
            }
        }
        public void DoOpenVideoFile()
        {
            // Open a video.
            if ((RootKernel.ScreenManager.screenList.Count == 0) && (!isOpening))
            {
                isOpening = true;

                string filePath = RootKernel.LaunchOpenFileDialog();
                if (filePath.Length > 0)
                {
                    DelegatesPool dp = DelegatesPool.Instance();
                    if (dp.LoadMovieInScreen != null)
                    {
                        dp.LoadMovieInScreen(filePath, -1, true);
                    }
                }

                isOpening = false;
            }
        }
        private void buttonCloseExplo_Click(object sender, EventArgs e)
        {
            CollapseExplorer();
        }
        private void _splitWorkSpace_DoubleClick(object sender, EventArgs e)
        {
            if (m_bExplorerCollapsed)
            {
                ExpandExplorer(true);
            }
            else
            {
                CollapseExplorer();
            }            
        }
        private void _splitWorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bExplorerCollapsed && splitWorkSpace.SplitterDistance > 30)
            {
                ExpandExplorer(false);
            }  
        }
        private void splitWorkSpace_Panel1_Click(object sender, EventArgs e)
        {
             // Clic sur l'explorer
            if (m_bExplorerCollapsed)
            {
                ExpandExplorer(true);
            }
        }
        #endregion

        #region Lower level methods
        public void CollapseExplorer()
        {
            splitWorkSpace.Panel2.SuspendLayout();
            splitWorkSpace.Panel1.SuspendLayout();

            if (m_bInitialized)
            {
                m_iOldSplitterDistance = splitWorkSpace.SplitterDistance;
            }
            else
            {
                m_iOldSplitterDistance = m_PrefManager.ExplorerSplitterDistance;
            }
            m_bExplorerCollapsed = true;
            foreach (Control ctrl in splitWorkSpace.Panel1.Controls)
            {
                ctrl.Visible = false;
            }
            splitWorkSpace.SplitterDistance = 4;
            splitWorkSpace.SplitterWidth = 1;
            splitWorkSpace.BorderStyle = BorderStyle.None;
            RootKernel.mnuToggleFileExplorer.Checked = false;

            splitWorkSpace.Panel1.ResumeLayout();
            splitWorkSpace.Panel2.ResumeLayout();

            m_PrefManager.ExplorerSplitterDistance = m_iOldSplitterDistance;
            m_PrefManager.ExplorerVisible = false;
            m_PrefManager.Export();
        }
        public void ExpandExplorer(bool resetSplitter)
        {
            if (m_iOldSplitterDistance != -1)
            {
                splitWorkSpace.Panel2.SuspendLayout();
                splitWorkSpace.Panel1.SuspendLayout();

                m_bExplorerCollapsed = false;
                foreach (Control ctrl in splitWorkSpace.Panel1.Controls)
                {
                    ctrl.Visible = true;
                }
                if (resetSplitter) { splitWorkSpace.SplitterDistance = m_iOldSplitterDistance; }
                splitWorkSpace.SplitterWidth = 4;
                splitWorkSpace.BorderStyle = BorderStyle.FixedSingle;
                RootKernel.mnuToggleFileExplorer.Checked = true;

                splitWorkSpace.Panel1.ResumeLayout();
                splitWorkSpace.Panel2.ResumeLayout();

                m_PrefManager.ExplorerSplitterDistance = splitWorkSpace.SplitterDistance;
                m_PrefManager.ExplorerVisible = true;
                m_PrefManager.Export();
            }

        }
        #endregion

        

    }
}
