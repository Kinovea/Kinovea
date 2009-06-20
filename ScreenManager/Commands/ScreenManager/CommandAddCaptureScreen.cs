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
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Threading;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class CommandAddCaptureScreen : IUndoableCommand
    {
        public string FriendlyName
        {
            get 
            {
                ResourceManager rm = new ResourceManager("Kinovea.ScreenManager.Languages.ScreenManagerLang", Assembly.GetExecutingAssembly());
                return rm.GetString("CommandAddCaptureScreen_FriendlyName", Thread.CurrentThread.CurrentUICulture);
            }
        }
        
        ScreenManagerKernel screenManagerKernel;

        #region constructor
        public CommandAddCaptureScreen(ScreenManagerKernel _smk, bool _bStoreState)
        {
            screenManagerKernel = _smk;
            if (_bStoreState) { screenManagerKernel.StoreCurrentState(); }
        }
        #endregion

        public void Execute()
        {
            CaptureScreen screen = new CaptureScreen();
            
            screen.SetMeAsActiveScreen += new AbstractScreen.DelegateSetMeAsActiveScreen(screenManagerKernel.Screen_SetActiveScreen);
            screen.CloseMe += new AbstractScreen.DelegateCloseMe(screenManagerKernel.Screen_CloseAsked);
            
            screen.refreshUICulture(); 
            screenManagerKernel.screenList.Add(screen);
            
            screen.Activate();
        }
        public void Unexecute()
        {
            screenManagerKernel.RecallState();
        }
    }
}
