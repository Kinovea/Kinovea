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
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Videa.Services;


namespace Videa.ScreenManager
{
    public class CommandShowScreens : ICommand 
    {
        public string FriendlyName
        {
            get
            {
                ResourceManager rm = new ResourceManager("Videa.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandShowScreen_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }

        ScreenManagerKernel screenManagerKernel;

        #region constructor
        public CommandShowScreens(ScreenManagerKernel _smk)
        {
            screenManagerKernel = _smk;
        }
        #endregion

        /// <summary>
        /// Execution de la commande
        /// </summary>
        public void Execute()
        {
        	// - parse the current screen list and fill panels with screens.
        	// - hide unused panels if necessary.
        	
            ScreenManagerUserInterface smui = screenManagerKernel.UI as ScreenManagerUserInterface;
            if(smui != null)
            {
            	// Empty the screen panels.
            	smui.splitScreens.Panel1.Controls.Clear();
            	smui.splitScreens.Panel2.Controls.Clear();
            	smui.CloseThumbnails();            	
            	
	            if(screenManagerKernel.screenList.Count == 1)
	            {
	            	smui.pnlScreens.Visible = true;
	            	smui.AllowDrop = false;
 
	            	// no common controls.
	            	smui.splitScreensPanel.Panel2Collapsed = true;
	            	
	            	// left screen enabled.
	            	smui.splitScreens.Panel1Collapsed = false;
	            	smui.splitScreens.Panel1.AllowDrop = true;
	            	smui.splitScreens.Panel1.Controls.Add(screenManagerKernel.screenList[0].UI);
	        
	            	// right screen disabled
	            	smui.splitScreens.Panel2Collapsed = true;
	            	smui.splitScreens.Panel2.AllowDrop = false;
	            }
				else if (screenManagerKernel.screenList.Count == 2)
            	{
					smui.pnlScreens.Visible = true;
	                smui.AllowDrop = false;
	                
	                // common controls enabled.
	                smui.splitScreensPanel.Panel2Collapsed = false;

	                // left screen
	                smui.splitScreens.Panel1Collapsed = false;
	                smui.splitScreens.Panel1.AllowDrop = true;
	                smui.splitScreens.Panel1.Controls.Add(screenManagerKernel.screenList[0].UI);
	                
	                // right screen
	            	smui.splitScreens.Panel2Collapsed = false;    
	                smui.splitScreens.Panel2.AllowDrop = true;
                	smui.splitScreens.Panel2.Controls.Add(screenManagerKernel.screenList[1].UI);
				}
				else if(screenManagerKernel.screenList.Count == 0)
				{
					smui.pnlScreens.Visible = false;
                	smui.AllowDrop = true;
                	
                	// no common controls. (not really needed because we never go from 2 to 0 screens)
	            	smui.splitScreensPanel.Panel2Collapsed = true;
	            	
                	smui.splitScreens.Panel1.AllowDrop = false;
                	smui.splitScreens.Panel2.AllowDrop = false;

                	smui.BringBackThumbnails();
				}
	            
            	
            }

            /*
            // ancien code :     
            psui1.Name = "psui1";
            psui1.Dock = DockStyle.Fill;
            psui1.TabIndex = 0;
			*/

            
            // Update status bar.
            screenManagerKernel.UpdateStatusBar();
        }
    }
}
