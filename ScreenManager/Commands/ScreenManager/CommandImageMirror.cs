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
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
using Videa.Services;
using VideaPlayerServer;



namespace Videa.ScreenManager
{
    public class CommandImageMirror : IUndoableCommand
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandImageMirror_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        PlayerScreen        m_PlayerScreen;
        MirrorFilterParams  m_OldParams;
        ToolStripMenuItem   m_mnuMirror;

        #region constructor
        public CommandImageMirror(PlayerScreen _PlayerScreen, ToolStripMenuItem _mnu)
        {
            m_PlayerScreen = _PlayerScreen;
            m_mnuMirror = _mnu;
        }
        #endregion


        public void Execute()
        {
            m_PlayerScreen.StopPlaying();

            // Save old filter configuration for undo.
            m_OldParams = m_PlayerScreen.MirrorFilter;

            // Create new filter.
            MirrorFilterParams mfp = new MirrorFilterParams();
            mfp.bActive = true;
            mfp.bMirrored = !m_OldParams.bMirrored;

            m_PlayerScreen.MirrorFilter = mfp;
         
            // Apply filter
            m_PlayerScreen.FilterImage(PlayerScreen.ImageFilterType.Mirror);

            // Refresh
            m_PlayerScreen.RefreshImage();
            m_mnuMirror.Checked = mfp.bMirrored;
        }

        public void Unexecute()
        {
            m_PlayerScreen.StopPlaying();
            
            // Fall back to old parameters.
            m_PlayerScreen.MirrorFilter = m_OldParams;

            // Apply filter.
            m_PlayerScreen.FilterImage(PlayerScreen.ImageFilterType.Mirror);
            m_PlayerScreen.RefreshImage();
            m_mnuMirror.Checked = m_OldParams.bMirrored;
        } 
    }

}
