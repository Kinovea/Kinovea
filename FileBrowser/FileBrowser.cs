#region License
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
#endregion

using System;
using System.Windows.Forms;
using Kinovea.Camera;
using Kinovea.Services;

// Note: this assembly is only about the side panel for explorer tree and shortcuts tree.
// The visual explorer with animated icons is part of ScreenManager assembly.

namespace Kinovea.FileBrowser
{
    public class FileBrowserKernel : IKernel
    {
        #region Properties
        public UserControl UI
        {
            get { return view; }
        }
        #endregion

        #region Members
        private FileBrowserUserInterface view = new FileBrowserUserInterface();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        
        public FileBrowserKernel()
        {
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
        }

        #region IKernel Implementation
        public void BuildSubTree()
        {
            // No sub modules.
        }
        public void ExtendMenu(ToolStrip _menu)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendToolBar(ToolStrip _toolbar)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendStatusBar(ToolStrip _statusbar)
        {
            // Nothing at this level.
            // No sub modules.
        }
        public void ExtendUI()
        {
            // No sub modules.
        }
        public void RefreshUICulture()
        {
            view.RefreshUICulture();
        }
        public bool CloseSubModules()
        {
            // Save last browsed directory
            view.Closing();
            return false;
        }
        #endregion

        private void CameraTypeManager_CamerasDiscovered(object sender, CamerasDiscoveredEventArgs e)
        {
            // Update list of cameras.
            int dbg = 42;
            
            view.CamerasDiscovered(e.Summaries);
        }
    }
}
