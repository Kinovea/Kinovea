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

using Kinovea.ScreenManager.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandSwapScreens : IUndoableCommand
    {
        public string FriendlyName
        {
        	get { return ScreenManagerLang.CommandSwapScreens_FriendlyName; }
        }

        ScreenManagerKernel m_ScreenManagerKernel;

        #region constructor
        public CommandSwapScreens(ScreenManagerKernel _smk)
        {
            m_ScreenManagerKernel = _smk;
        }
        #endregion

        public void Execute()
        {
            // We keep the list ordered. [0] = left.
            AbstractScreen temp = m_ScreenManagerKernel.screenList[0];
            m_ScreenManagerKernel.screenList[0] = m_ScreenManagerKernel.screenList[1];
            m_ScreenManagerKernel.screenList[1] = temp;

            // Show new disposition.
            ScreenManagerUserInterface smui = m_ScreenManagerKernel.UI as ScreenManagerUserInterface;
            if(smui != null)
            {
            	smui.splitScreens.Panel1.Controls.Clear();
            	smui.splitScreens.Panel2.Controls.Clear();
            	
            	smui.splitScreens.Panel1.Controls.Add(m_ScreenManagerKernel.screenList[0].UI);
            	smui.splitScreens.Panel2.Controls.Add(m_ScreenManagerKernel.screenList[1].UI);
            }
            
            // the following lines are placed here so they also get called at unexecute.
            m_ScreenManagerKernel.OrganizeMenus();
            m_ScreenManagerKernel.UpdateStatusBar();
            m_ScreenManagerKernel.SwapSync();
            m_ScreenManagerKernel.SetSyncPoint(true);
        }

        public void Unexecute()
        {
            Execute();
        } 
    }
}
