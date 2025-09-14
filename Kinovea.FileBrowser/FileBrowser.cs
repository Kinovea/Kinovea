#region License
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
            log.Debug("Module Construction: Navigation pane");
            CameraTypeManager.CamerasDiscovered += CameraTypeManager_CamerasDiscovered;
            CameraTypeManager.CameraSummaryUpdated += CameraTypeManager_CameraSummaryUpdated;
            CameraTypeManager.CameraForgotten += CameraTypeManager_CameraForgotten;
        }

        #region IKernel Implementation
        public void BuildSubTree() {}
        public void ExtendMenu(ToolStrip menu) {}
        public void ExtendToolBar(ToolStrip toolbar) {}
        public void ExtendStatusBar(ToolStrip statusbar) {}
        public void ExtendUI() {}

        public void RefreshUICulture()
        {
            view.RefreshUICulture();
        }
        public bool CloseSubModules()
        {
            view.Closing();
            return false;
        }
        public void PreferencesUpdated()
        {
            RefreshUICulture();
        }
        #endregion

        private void CameraTypeManager_CamerasDiscovered(object sender, CamerasDiscoveredEventArgs e)
        {
            view.CamerasDiscovered(e.Summaries);
        }
        
        private void CameraTypeManager_CameraSummaryUpdated(object sender, EventArgs<CameraSummary> e)
        {
            view.CameraSummaryUpdated(e.Value);
        }

        private void CameraTypeManager_CameraForgotten(object sender, EventArgs<CameraSummary> e)
        {
            view.CameraForgotten(e.Value);
        }

    }
}
