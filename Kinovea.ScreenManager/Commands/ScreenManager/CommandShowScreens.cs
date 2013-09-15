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
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    /// <summary>
    /// This command is used to translate the screen list in actual screen panels.
    /// We generally land here after a command modified the screen list.
    /// We parse the list and make sure the panels are conform, by adding or removing them.
    /// </summary>
    public class CommandShowScreens : ICommand 
    {
        public string FriendlyName
        {
            get { return ScreenManagerLang.CommandShowScreen_FriendlyName; }
        }

        ScreenManagerKernel screenManagerKernel;

        public CommandShowScreens(ScreenManagerKernel screenManagerKernel)
        {
            this.screenManagerKernel = screenManagerKernel;
        }
        
        public void Execute()
        {
            if(screenManagerKernel.View == null)
                return;
            
            screenManagerKernel.OrganizeScreens();
            screenManagerKernel.UpdateStatusBar();
        }
    }
}
