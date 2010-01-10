#region License
/*
Copyright © Joan Charmant 2009.
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
using System.Reflection;
using System.Resources;
using System.Threading;

using Kinovea.Services;
using System.Windows.Forms;

namespace Kinovea.ScreenManager
{
    public class CommandAddCaptureScreen : IUndoableCommand
    {
    	#region Properties
        public string FriendlyName
        {
            get 
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandAddCaptureScreen_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }
        #endregion
        
        #region Members
        ScreenManagerKernel m_ScreenManagerKernel;
		#endregion
        
		#region constructor
        public CommandAddCaptureScreen(ScreenManagerKernel _smk, bool _bStoreState)
        {
            m_ScreenManagerKernel = _smk;
            if (_bStoreState) { m_ScreenManagerKernel.StoreCurrentState(); }
        }
        #endregion

        public void Execute()
        {
            CaptureScreen screen = new CaptureScreen(m_ScreenManagerKernel);
            screen.refreshUICulture(); 
            m_ScreenManagerKernel.screenList.Add(screen);
            
            // Try to connect right away.
            /*bool bAtLeastOneDevice = m_ScreenManagerKernel.Capture_TryDeviceConnection(screen);
        	
        	// if no device, alert user.
        	if(!bAtLeastOneDevice)
        	{
        		MessageBox.Show(
        			"zero capture device, pebkac",
               		"debug",
               		MessageBoxButtons.OK,
                	MessageBoxIcon.Exclamation);	
        	}*/
        }
        public void Unexecute()
        {
            m_ScreenManagerKernel.RecallState();
        }
    }
}
