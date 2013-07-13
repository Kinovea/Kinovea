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

using Kinovea.ScreenManager.Languages;
using System;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandAddCaptureScreen : IUndoableCommand
    {
    	#region Properties
        public string FriendlyName
        {
        	get { return ScreenManagerLang.CommandAddCaptureScreen_FriendlyName; }
        }
        #endregion
        
        #region Members
        ScreenManagerKernel screenManagerKernel;
		#endregion
        
		#region constructor
        public CommandAddCaptureScreen(ScreenManagerKernel _smk, bool _bStoreState)
        {
            this.screenManagerKernel = _smk;
            if (_bStoreState) 
                screenManagerKernel.StoreCurrentState();
        }
        #endregion

        public void Execute()
        {
            CaptureScreen screen = new CaptureScreen();
            if(screenManagerKernel.ScreenCount > 1) 
                screen.SetShared(true);
            
            screen.RefreshUICulture();
            screenManagerKernel.AddScreen(screen);
            
        }
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}
